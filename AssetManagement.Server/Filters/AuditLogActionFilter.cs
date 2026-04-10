using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Server.Filters
{
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;
        private readonly AssetDbContext _context;

        public AuditLogActionFilter(
            IAuditLogService auditLogService,
            AssetDbContext context)
        {
            _auditLogService = auditLogService;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Console.WriteLine("AUDIT FILTER HIT");

            try
            {
                context.HttpContext.Request.EnableBuffering();
            }
            catch
            {
                // ignore if buffering not available
            }

            var executedContext = await next();

            try
            {
                if (ShouldSkipLogging(context, executedContext))
                {
                    Console.WriteLine("AUDIT SKIPPED");
                    return;
                }

                var httpContext = context.HttpContext;
                var user = httpContext.User;

                var userId = GetUserId(user);
                var claimEmail = GetUserEmail(user);
                var claimDepartment = GetUserDepartment(user);

                Console.WriteLine($"AUDIT CLAIMS: isAuthenticated={user?.Identity?.IsAuthenticated}, userId={userId}, email={claimEmail}, department={claimDepartment}");

                if ((user == null || user.Identity == null || user.Identity.IsAuthenticated != true)
                    && !userId.HasValue
                    && string.IsNullOrWhiteSpace(claimEmail))
                {
                    // Allow logging for authentication endpoints (login/logout) even when
                    // the request is unauthenticated so we can record LOGIN/LOGOUT events.
                    var ctrl = GetControllerName(context);
                    var act = GetActionName(context);

                    if (string.Equals(ctrl, "Auth", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ctrl, "Authentication", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ctrl, "AuthController", StringComparison.OrdinalIgnoreCase))
                    {
                        // proceed to build audit entry (we can still extract email from body later)
                    }
                    else
                    {
                        Console.WriteLine("AUDIT USER NOT AUTHENTICATED");
                        return;
                    }
                }

                var controllerName = GetControllerName(context);
                var actionName = GetActionName(context);

                Console.WriteLine($"AUDIT ACTION: {controllerName} - {actionName} - {httpContext.Request.Method}");

                var action = BuildBusinessAction(controllerName, actionName, httpContext.Request.Method);
                var entityType = BuildEntityType(controllerName);
                var entityId = GetEntityId(context);

                Domain.Entities.User? actor = null;

                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    actor = await _context.Users
                        .AsNoTracking()
                        .Include(x => x.Department)
                        .FirstOrDefaultAsync(x => x.Id == userId.Value);
                }

                if (actor == null && !string.IsNullOrWhiteSpace(claimEmail))
                {
                    actor = await _context.Users
                        .AsNoTracking()
                        .Include(x => x.Department)
                        .FirstOrDefaultAsync(x => x.Email == claimEmail);
                }

                var actorEmail = actor?.Email ?? claimEmail ?? "unknown";
                var actorDepartment = actor?.Department?.Name ?? claimDepartment;

                if ((!userId.HasValue || userId == Guid.Empty) && actor != null)
                {
                    userId = actor.Id;
                }

                Console.WriteLine($"AUDIT FINAL: userId={userId}, actorEmail={actorEmail}, actorDepartment={actorDepartment}");

                // If still unknown and this is an authentication endpoint, try to read the
                // request body (JSON) to extract an email field (login/logout flows).
                if ((actorEmail == null || actorEmail == "unknown") &&
                    (string.Equals(controllerName, "Auth", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(controllerName, "Authentication", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(controllerName, "AuthController", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var req = httpContext.Request;
                        req.Body.Position = 0;
                        using var reader = new StreamReader(req.Body, leaveOpen: true);
                        var bodyText = await reader.ReadToEndAsync();
                        req.Body.Position = 0;

                        if (!string.IsNullOrWhiteSpace(bodyText))
                        {
                            try
                            {
                                using var doc = System.Text.Json.JsonDocument.Parse(bodyText);
                                if (doc.RootElement.TryGetProperty("email", out var emailProp))
                                {
                                    var emailVal = emailProp.GetString();
                                    if (!string.IsNullOrWhiteSpace(emailVal))
                                    {
                                        actorEmail = emailVal;
                                    }
                                }
                            }
                            catch
                            {
                                // ignore parse errors
                            }
                        }
                    }
                    catch
                    {
                        // ignore any IO errors
                    }
                }

                var targetEmail = await ExtractTargetEmailAsync(context, entityType, entityId, actorEmail);

                Guid? safeUserId = null;

                if (userId.HasValue && userId != Guid.Empty)
                {
                    var exists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == userId.Value);

                    if (exists)
                    {
                        safeUserId = userId;
                    }
                }

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = safeUserId, // ✅ FIX
                    ActorEmail = actorEmail,
                    ActorDepartment = actorDepartment,
                    TargetEmail = targetEmail,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    CreatedAt = DateTime.UtcNow
                };

                await _auditLogService.AddAsync(auditLog);

                Console.WriteLine("AUDIT SAVED");
            }
            catch (Exception ex)
            {
                Console.WriteLine("AUDIT ERROR: " + ex);
            }
        }

        private async Task<string?> ExtractTargetEmailAsync(
            ActionExecutingContext context,
            string entityType,
            Guid? entityId,
            string? actorEmail)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                var type = argument.GetType();

                var possibleEmailProps = new[]
                {
                    "Email",
                    "UserEmail",
                    "TargetEmail",
                    "EmployeeEmail",
                    "ManagerEmail",
                    // role name or generic name fields that may be present on DTOs
                    "RoleName",
                    "Name",
                    "Role"
                };

                foreach (var propName in possibleEmailProps)
                {
                    var prop = type.GetProperty(propName);
                    if (prop == null)
                    {
                        continue;
                    }

                    var value = prop.GetValue(argument)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }

                var possibleGuidProps = new[]
                {
                    "UserId",
                    "EmployeeId",
                    "ManagerId",
                    "AssignedToUserId",
                    "TargetUserId",
                    "CurrentUserId"
                };

                // include role id/property names commonly used in requests
                possibleGuidProps = possibleGuidProps.Concat(new[] { "RoleId", "roleId" }).ToArray();

                foreach (var propName in possibleGuidProps)
                {
                    var prop = type.GetProperty(propName);
                    if (prop == null)
                    {
                        continue;
                    }

                    var rawValue = prop.GetValue(argument);
                    if (TryParseGuid(rawValue, out var guidValue))
                    {
                        var email = await GetUserEmailByIdAsync(guidValue);
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            return email;
                        }
                    }
                }
            }

            if (entityType.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                entityId.HasValue &&
                entityId.Value != Guid.Empty)
            {
                var userEmail = await GetUserEmailByIdAsync(entityId.Value);
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    return userEmail;
                }
            }

            if (entityType.Equals("Assignment", StringComparison.OrdinalIgnoreCase) &&
                entityId.HasValue &&
                entityId.Value != Guid.Empty)
            {
                var assignment = await _context.Assignments
                    .AsNoTracking()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == entityId.Value);

                if (!string.IsNullOrWhiteSpace(assignment?.User?.Email))
                {
                    return assignment.User.Email;
                }
            }

            if (entityType.Equals("Role", StringComparison.OrdinalIgnoreCase) &&
                entityId.HasValue &&
                entityId.Value != Guid.Empty)
            {
                var role = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entityId.Value);

                if (!string.IsNullOrWhiteSpace(role?.Name))
                {
                    return role.Name;
                }
            }

            if (entityType.Equals("ReturnRequest", StringComparison.OrdinalIgnoreCase) &&
                entityId.HasValue &&
                entityId.Value != Guid.Empty)
            {
                var returnRequest = await _context.ReturnRequests
                    .AsNoTracking()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Id == entityId.Value);

                if (!string.IsNullOrWhiteSpace(returnRequest?.User?.Email))
                {
                    return returnRequest.User.Email;
                }
            }

            if (entityType.Equals("BrokenReport", StringComparison.OrdinalIgnoreCase) &&
                entityId.HasValue &&
                entityId.Value != Guid.Empty)
            {
                var brokenReport = await _context.BrokenReports
                    .AsNoTracking()
                    .Include(x => x.ReportedBy)
                    .FirstOrDefaultAsync(x => x.Id == entityId.Value);

                if (!string.IsNullOrWhiteSpace(brokenReport?.ReportedBy?.Email))
                {
                    return brokenReport.ReportedBy.Email;
                }
            }

            return actorEmail;
        }

        private async Task<string?> GetUserEmailByIdAsync(Guid userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync();
        }

        private static bool TryParseGuid(object? value, out Guid guid)
        {
            if (value is Guid g && g != Guid.Empty)
            {
                guid = g;
                return true;
            }

            if (Guid.TryParse(value?.ToString(), out var parsed) && parsed != Guid.Empty)
            {
                guid = parsed;
                return true;
            }

            guid = Guid.Empty;
            return false;
        }

        private static string? GetUserEmail(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            return user.FindFirst(ClaimTypes.Email)?.Value
                   ?? user.FindFirst("email")?.Value
                   ?? user.FindFirst("Email")?.Value
                   ?? user.FindFirst(ClaimTypes.Name)?.Value
                   ?? user.FindFirst("unique_name")?.Value
                   ?? user.Identity?.Name;
        }

        private static string? GetUserDepartment(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            return user.FindFirst("department")?.Value
                   ?? user.FindFirst("Department")?.Value
                   ?? user.FindFirst("dept")?.Value
                   ?? user.FindFirst("Dept")?.Value;
        }

        private static bool ShouldSkipLogging(ActionExecutingContext context, ActionExecutedContext executedContext)
        {
            if (executedContext.Exception != null) return true;

            var httpMethod = context.HttpContext.Request.Method;
            var controllerName = GetControllerName(context);
            var actionName = GetActionName(context);

            if (string.Equals(controllerName, "AuditLogs", StringComparison.OrdinalIgnoreCase))
                return true;


            if (IsDashboardOrStatisticRequest(controllerName, actionName))
                return true;

            var noisyActions = new[] { "GetMe", "GetProfile", "GetMyPermissions", "GetNotifications", "GetCount" };
            if (HttpMethods.IsGet(httpMethod) && noisyActions.Any(a => actionName.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (controllerName.Contains("Return", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsGet(httpMethod))
            {
                if (actionName.Contains("inspect", StringComparison.OrdinalIgnoreCase) ||
                    actionName.Contains("history", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsReadOnlyGetAction(string controllerName, string actionName)
        {
            if (string.Equals(controllerName, "WeatherForecast", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var readOnlyKeywords = new[]
            {
                "get",
                "list",
                "search",
                "filter",
                "detail",
                "details",
                "view",
                "index",
                "page",
                "history",
                "log",
                "logs",
                "profile",
                "me"
            };

            return readOnlyKeywords.Any(keyword =>
                actionName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsDashboardOrStatisticRequest(string controllerName, string actionName)
        {
            if (string.Equals(controllerName, "Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return actionName.Contains("kpi", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("chart", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("trend", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("top", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("status", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("summary", StringComparison.OrdinalIgnoreCase) ||
                   actionName.Contains("statistic", StringComparison.OrdinalIgnoreCase);
        }

        private static Guid? GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) return null;

            var possibleValues = new[]
            {
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                user.FindFirst("nameid")?.Value,
                user.FindFirst("sub")?.Value,
                user.FindFirst("userId")?.Value,
                user.FindFirst("userid")?.Value,
                user.FindFirst("user_id")?.Value,
                user.FindFirst("uid")?.Value,
                user.FindFirst("id")?.Value
            };

            foreach (var value in possibleValues)
            {
                if (Guid.TryParse(value, out var guidUserId))
                {
                    return guidUserId;
                }
            }

            return null;
        }

        private static Guid? GetEntityId(ActionExecutingContext context)
        {
            if (context.RouteData.Values.TryGetValue("id", out var idValue) &&
                Guid.TryParse(idValue?.ToString(), out var routeId))
            {
                return routeId;
            }

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                var type = argument.GetType();
                var props = new[]
                {
                    "Id",
                    "UserId",
                    "AssetId",
                    "DepartmentId",
                    "RequestId",
                    "AssignmentId",
                    "ReturnRequestId",
                    "BrokenReportId",
                    "MaintenanceId"
                };

                foreach (var propName in props)
                {
                    var prop = type.GetProperty(propName);
                    if (prop == null)
                    {
                        continue;
                    }

                    var value = prop.GetValue(argument);

                    if (value is Guid g && g != Guid.Empty)
                    {
                        return g;
                    }

                    if (Guid.TryParse(value?.ToString(), out var parsed) && parsed != Guid.Empty)
                    {
                        return parsed;
                    }
                }
            }

            return null;
        }

        private static string BuildEntityType(string controllerName)
        {
            if (controllerName.Contains("User", StringComparison.OrdinalIgnoreCase))
                return "User";

            if (controllerName.Contains("Department", StringComparison.OrdinalIgnoreCase))
                return "Department";

            if (controllerName.Contains("Asset", StringComparison.OrdinalIgnoreCase))
                return "Asset";

            if (controllerName.Contains("Assignment", StringComparison.OrdinalIgnoreCase))
                return "Assignment";

            if (controllerName.Contains("Return", StringComparison.OrdinalIgnoreCase))
                return "ReturnRequest";

            if (controllerName.Contains("Broken", StringComparison.OrdinalIgnoreCase))
                return "BrokenReport";

            if (controllerName.Contains("Maintenance", StringComparison.OrdinalIgnoreCase))
                return "MaintenanceRecord";

            if (controllerName.Contains("Role", StringComparison.OrdinalIgnoreCase))
                return "Role";

            // Manager approval controller deals with asset requests
            if (controllerName.Contains("Approval", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Contains("Approvals", StringComparison.OrdinalIgnoreCase))
                return "AssetRequest";

            if (controllerName.Contains("Vendor", StringComparison.OrdinalIgnoreCase))
                return "Vendor";

            if (controllerName.Contains("Auth", StringComparison.OrdinalIgnoreCase))
                return "Authentication";

            return controllerName;
        }

        private static string BuildBusinessAction(string controllerName, string actionName, string httpMethod)
        {
            // 1. Auth
            if (controllerName.Contains("Auth", StringComparison.OrdinalIgnoreCase))
            {
                if (actionName.Contains("login", StringComparison.OrdinalIgnoreCase)) return "LOGIN";
                if (actionName.Contains("logout", StringComparison.OrdinalIgnoreCase)) return "LOGOUT";
            }

            // 2. Named CRUD actions (check name before HTTP method)
            if (actionName.Contains("create", StringComparison.OrdinalIgnoreCase) ||
                actionName.Contains("add", StringComparison.OrdinalIgnoreCase))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"CREATE_{e}";
            }
            if (actionName.Contains("update", StringComparison.OrdinalIgnoreCase) ||
                actionName.Contains("edit", StringComparison.OrdinalIgnoreCase))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"UPDATE_{e}";
            }
            if (actionName.Contains("delete", StringComparison.OrdinalIgnoreCase) ||
                actionName.Contains("remove", StringComparison.OrdinalIgnoreCase))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"DELETE_{e}";
            }

            if (actionName.Contains("assign", StringComparison.OrdinalIgnoreCase))
                return "ASSIGN_ASSET";
            if (actionName.Contains("approve", StringComparison.OrdinalIgnoreCase) && !HttpMethods.IsGet(httpMethod))
                return "APPROVE_REQUEST";
            if (actionName.Contains("reject", StringComparison.OrdinalIgnoreCase) && !HttpMethods.IsGet(httpMethod))
                return "REJECT_REQUEST";
            if (actionName.Contains("return", StringComparison.OrdinalIgnoreCase) && !HttpMethods.IsGet(httpMethod))
                return "RETURN_ASSET";
            if (actionName.Contains("report", StringComparison.OrdinalIgnoreCase) &&
                controllerName.Contains("Broken", StringComparison.OrdinalIgnoreCase))
                return "REPORT_BROKEN_ASSET";
            if (actionName.Contains("import", StringComparison.OrdinalIgnoreCase)) return "IMPORT_DATA";
            if (actionName.Contains("export", StringComparison.OrdinalIgnoreCase)) return "EXPORT_DATA";

            // 3. HTTP-method based CRUD
            if (HttpMethods.IsPost(httpMethod))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"CREATE_{e}";
            }
            if (HttpMethods.IsPut(httpMethod) || HttpMethods.IsPatch(httpMethod))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"UPDATE_{e}";
            }
            if (HttpMethods.IsDelete(httpMethod))
            {
                var e = BuildEntityType(controllerName).ToUpperInvariant();
                return $"DELETE_{e}";
            }

            
            if (HttpMethods.IsGet(httpMethod))
            {
                // ── Assignment sub-views ──────────────────────────────────────────
                if (controllerName.Contains("Assignment", StringComparison.OrdinalIgnoreCase))
                {
                    if (actionName.Contains("current", StringComparison.OrdinalIgnoreCase))
                        return "VIEW_CURRENT_ASSIGNMENT";           
                    if (actionName.Contains("history", StringComparison.OrdinalIgnoreCase))
                        return "VIEW_ASSET_HISTORY";               
                                                                    
                    return "VIEW_ASSIGNMENT";
                }

                // ── Return sub-views ─────────────────────────────────────────────
                if (controllerName.Contains("Return", StringComparison.OrdinalIgnoreCase))
                {
                    return "VIEW_RETURNREQUEST";
                }

                // ── Department detail ─────────────────────────────────────────────
                if (controllerName.Contains("Department", StringComparison.OrdinalIgnoreCase))
                {
                    return "VIEW_DEPARTMENT";
                }



                // ── Mặc định ─────────────────────────────────────────────────────
                var entity = BuildEntityType(controllerName).ToUpperInvariant();
                return $"VIEW_{entity}";
            }

            return BuildEntityType(controllerName).ToUpperInvariant();
        }

        private static string GetControllerName(FilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                return descriptor.ControllerName;
            }

            return "Unknown";
        }

        private static string GetActionName(FilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                return descriptor.ActionName;
            }

            return "Unknown";
        }
    }
}