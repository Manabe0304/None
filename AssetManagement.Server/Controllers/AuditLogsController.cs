using AssetManagement.Application.DTOs.Response.AuditLog;
using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly AssetDbContext _context;

        public AuditLogsController(IAuditLogService auditLogService, AssetDbContext context)
        {
            _auditLogService = auditLogService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllAsync();

            var actorUserIds = logs
                .Where(x => x.UserId != null && x.UserId != Guid.Empty)
                .Select(x => x.UserId!.Value)
                .Distinct()
                .ToList();

            var targetUserIds = logs
                .Where(x => IsEntityType(x.EntityType, "USER", "USERS") && x.EntityId != null && x.EntityId != Guid.Empty)
                .Select(x => x.EntityId!.Value)
                .Distinct()
                .ToList();

            var targetDepartmentIds = logs
                .Where(x => IsEntityType(x.EntityType, "DEPARTMENT", "DEPARTMENTS") && x.EntityId != null && x.EntityId != Guid.Empty)
                .Select(x => x.EntityId!.Value)
                .Distinct()
                .ToList();

            var assetIds = logs
                .Where(x => IsEntityType(x.EntityType, "ASSET", "ASSETS") && x.EntityId != null && x.EntityId != Guid.Empty)
                .Select(x => x.EntityId!.Value)
                .Distinct()
                .ToList();

            var actorUsers = await _context.Users
                .Include(x => x.Department)
                .AsNoTracking()
                .Where(x => actorUserIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var targetUsers = await _context.Users
                .AsNoTracking()
                .Where(x => targetUserIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var departments = await _context.Departments
                .AsNoTracking()
                .Where(x => targetDepartmentIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var assets = await _context.Assets
                .AsNoTracking()
                .Where(x => assetIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var result = logs.Select(log => new AuditLogItemResponse
            {
                Id = log.Id,
                // createdAt stored in UTC in DB; map to DateTimeOffset to keep timezone info
                CreatedAt = DateTime.SpecifyKind(log.CreatedAt, DateTimeKind.Utc),
                Username = GetUsernameFast(log, actorUsers),
                Department = GetDepartmentFast(log, actorUsers),
                Action = FormatAction(log.Action, log.EntityType),
                TargetName = BuildTargetNameFast(log, targetUsers, departments, assets)
            }).ToList();

            return Ok(result);
        }

        private static string GetUsernameFast(
            AuditLog log,
            Dictionary<Guid, User> actorUsers)
        {
            if (!string.IsNullOrWhiteSpace(log.ActorEmail))
            {
                return log.ActorEmail;
            }

            if (log.UserId != null &&
                log.UserId != Guid.Empty &&
                actorUsers.TryGetValue(log.UserId.Value, out var actorUser))
            {
                if (!string.IsNullOrWhiteSpace(actorUser.Email))
                {
                    return actorUser.Email;
                }

                if (!string.IsNullOrWhiteSpace(actorUser.FullName))
                {
                    return actorUser.FullName;
                }
            }

            return "Unknown";
        }

        private static string GetDepartmentFast(
            AuditLog log,
            Dictionary<Guid, User> actorUsers)
        {
            if (!string.IsNullOrWhiteSpace(log.ActorDepartment))
            {
                return log.ActorDepartment;
            }

            if (log.UserId != null &&
                log.UserId != Guid.Empty &&
                actorUsers.TryGetValue(log.UserId.Value, out var actorUser))
            {
                if (actorUser.Department != null &&
                    !string.IsNullOrWhiteSpace(actorUser.Department.Name))
                {
                    return actorUser.Department.Name;
                }
            }

            return "Unknown";
        }

        private static string FormatAction(string? action, string? entityType)
        {
            var normalizedEntity = NormalizeEntity(entityType);

            if (string.IsNullOrWhiteSpace(action))
                return normalizedEntity == "item" ? "unknown" : $"update {normalizedEntity}";

            var normalizedAction = action.Trim().ToUpperInvariant();

            return normalizedAction switch
            {
                // ── Auth ──────────────────────────────────────────────────────────
                "LOGIN" => "login",
                "LOGOUT" => "logout",

                // ── User ─────────────────────────────────────────────────────────
                "CREATE_USER" => "create user",
                "UPDATE_USER" => "update user",
                "DELETE_USER" => "delete user",
                "CREATE_USERS" => "create users",
                "UPDATE_USERS" => "update users",
                "DELETE_USERS" => "delete users",
                "VIEW_USER" => "view user",

                // ── Asset ─────────────────────────────────────────────────────────
                "CREATE_ASSET" => "create asset",
                "UPDATE_ASSET" => "update asset",
                "DELETE_ASSET" => "delete asset",
                "ASSIGN_ASSET" => "assign asset",
                "RETURN_ASSET" => "return asset",
                "REPORT_BROKEN_ASSET" => "report broken asset",
                "VIEW_ASSET" => "view asset",

                // ── Department ────────────────────────────────────────────────────
                "CREATE_DEPARTMENT" => "create department",
                "UPDATE_DEPARTMENT" => "update department",
                "DELETE_DEPARTMENT" => "delete department",
                "CREATE_DEPARTMENTS" => "create departments",
                "UPDATE_DEPARTMENTS" => "update departments",
                "DELETE_DEPARTMENTS" => "delete departments",
                "VIEW_DEPARTMENT" => "view department",         

                // ── Assignment (phân biệt 3 loại) ────────────────────────────────
                "VIEW_ASSIGNMENT" => "view assignment",
                "VIEW_CURRENT_ASSIGNMENT" => "view current assignment",   
                "VIEW_ASSET_HISTORY" => "view asset history",        

                // ── Return (phân biệt 3 tab) ──────────────────────────────────────
                "VIEW_PENDING_RETURNS" => "view pending returns",       
                "VIEW_INSPECTED_RETURNS" => "view inspected returns",     
                "VIEW_PROCESSED_RETURNS" => "view processed history",     

                // ── Request ───────────────────────────────────────────────────────
                "APPROVE_REQUEST" => "approve request",
                "REJECT_REQUEST" => "reject request",

                // ── Misc ─────────────────────────────────────────────────────────
                "IMPORT_DATA" => "import data",
                "EXPORT_DATA" => "export data",

                _ => FallbackFormatAction(normalizedAction, normalizedEntity)
            };
        }

        private static string FallbackFormatAction(string action, string entity)
        {
            if (action.StartsWith("POST_", StringComparison.OrdinalIgnoreCase))
            {
                return $"create {entity}";
            }
            if (action.StartsWith("PUT_", StringComparison.OrdinalIgnoreCase) ||
                action.StartsWith("PATCH_", StringComparison.OrdinalIgnoreCase))
            {
                return $"update {entity}";
            }
            if (action.StartsWith("DELETE_", StringComparison.OrdinalIgnoreCase))
            {
                return $"delete {entity}";
            }
            if (action.StartsWith("VIEW_", StringComparison.OrdinalIgnoreCase))   // ← CHỈ SỬA ĐOẠN NÀY
            {
                return $"view {entity}";
            }
            var parts = action
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLowerInvariant())
                .ToArray();
            if (parts.Length == 0)
            {
                return "unknown";
            }
            if (parts.Length == 1)
            {
                return parts[0];
            }
            return string.Join(" ", parts);
        }

        private static string NormalizeEntity(string? entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return "item";
            }

            var value = entityType.Trim().ToUpperInvariant();

            return value switch
            {
                "USER" => "user",
                "USERS" => "users",
                "ROLE" => "role",
                "ROLES" => "roles",
                "DEPARTMENT" => "department",
                "DEPARTMENTS" => "departments",
                "ASSET" => "asset",
                "ASSETS" => "assets",
                "ASSIGNMENT" => "assignment",
                "ASSIGNMENTS" => "assignments",
                "RETURNREQUEST" => "return request",
                "RETURNREQUESTS" => "return requests",
                "BROKENREPORT" => "broken report",
                "BROKENREPORTS" => "broken reports",
                "MAINTENANCERECORD" => "maintenance record",
                "MAINTENANCERECORDS" => "maintenance records",
                "AUTHENTICATION" => "authentication",
                _ => entityType.Trim().ToLowerInvariant()
            };
        }

        private static bool IsEntityType(string? entityType, params string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return false;
            }

            var value = entityType.Trim().ToUpperInvariant();
            return candidates.Any(x => x == value);
        }

        private static string BuildTargetNameFast(
            AuditLog log,
            Dictionary<Guid, User> targetUsers,
            Dictionary<Guid, Department> departments,
            Dictionary<Guid, Asset> assets)
        {
            if (!string.IsNullOrWhiteSpace(log.TargetEmail))
            {
                return log.TargetEmail;
            }

            var entityType = log.EntityType;
            var entityId = log.EntityId;

            if (string.IsNullOrWhiteSpace(entityType))
            {
                return "Unknown";
            }

            if (entityId == null || entityId == Guid.Empty)
            {
                return GetDefaultTargetLabel(entityType);
            }

            var type = entityType.Trim().ToUpperInvariant();

            if ((type == "USER" || type == "USERS") &&
                targetUsers.TryGetValue(entityId.Value, out var user))
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    return user.Email;
                }

                if (!string.IsNullOrWhiteSpace(user.FullName))
                {
                    return user.FullName;
                }

                return $"User - {entityId}";
            }

            if ((type == "DEPARTMENT" || type == "DEPARTMENTS") &&
                departments.TryGetValue(entityId.Value, out var dept))
            {
                return !string.IsNullOrWhiteSpace(dept.Name)
                    ? dept.Name
                    : $"Department - {entityId}";
            }

            if ((type == "ASSET" || type == "ASSETS") &&
                assets.TryGetValue(entityId.Value, out var asset))
            {
                if (!string.IsNullOrWhiteSpace(asset.AssetTag))
                {
                    return asset.AssetTag;
                }

                if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
                {
                    return asset.SerialNumber;
                }

                return $"Asset - {entityId}";
            }

            return type switch
            {
                "ROLE" => "Role",
                "ROLES" => "Roles",
                "ASSIGNMENT" => $"Assignment - {entityId}",
                "ASSIGNMENTS" => $"Assignments - {entityId}",
                "RETURNREQUEST" => $"Return request - {entityId}",
                "RETURNREQUESTS" => $"Return requests - {entityId}",
                "BROKENREPORT" => $"Broken report - {entityId}",
                "BROKENREPORTS" => $"Broken reports - {entityId}",
                "MAINTENANCERECORD" => $"Maintenance record - {entityId}",
                "MAINTENANCERECORDS" => $"Maintenance records - {entityId}",
                "AUTHENTICATION" => "Authentication",
                _ => $"{entityType} - {entityId}"
            };
        }

        private static string GetDefaultTargetLabel(string entityType)
        {
            return entityType.Trim().ToUpperInvariant() switch
            {
                "USER" => "User",
                "USERS" => "Users",
                "ROLE" => "Role",
                "ROLES" => "Roles",
                "DEPARTMENT" => "Department",
                "DEPARTMENTS" => "Departments",
                "ASSET" => "Asset",
                "ASSETS" => "Assets",
                "ASSIGNMENT" => "Assignment",
                "ASSIGNMENTS" => "Assignments",
                "RETURNREQUEST" => "Return request",
                "RETURNREQUESTS" => "Return requests",
                "BROKENREPORT" => "Broken report",
                "BROKENREPORTS" => "Broken reports",
                "MAINTENANCERECORD" => "Maintenance record",
                "MAINTENANCERECORDS" => "Maintenance records",
                "AUTHENTICATION" => "Authentication",
                _ => entityType
            };
        }
    }
}