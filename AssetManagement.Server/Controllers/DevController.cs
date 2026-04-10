using AssetManagement.Domain.Entities;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public DevController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedData()
        {
            if (!_context.Roles.Any())
            {
                // Insert Roles
                var roles = new List<Role>
                {
                    new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "ADMIN" },
                    new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "MANAGER" },
                    new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "EMPLOYEE" },
                    new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "ITAdmin" }
                };
                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }

            if (!_context.Departments.Any())
            {
                // Insert Departments
                var departments = new List<Department>
                {
                    new Department { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "IT Department" },
                    new Department { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "HR Department" },
                    new Department { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "Finance Department" }
                };
                _context.Departments.AddRange(departments);
                await _context.SaveChangesAsync();
            }

            if (!_context.Users.Any())
            {
                // Insert Users
                var users = new List<User>
                {
                    new User
                    {
                        Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                        Email = "admin@example.com",
                        PasswordHash = "admin123",
                        FullName = "Admin User",
                        RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                        Email = "manager@example.com",
                        PasswordHash = "manager123",
                        FullName = "Manager User",
                        RoleId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                        Email = "employee@example.com",
                        PasswordHash = "emp123",
                        FullName = "Employee User",
                        RoleId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
                        Email = "itadmin@example.com",
                        PasswordHash = "itadmin123",
                        FullName = "IT Admin User",
                        RoleId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            if (!_context.Vendors.Any())
            {
                // Insert Vendors
                var vendors = new List<Vendor>
                {
                    new Vendor { Id = Guid.NewGuid(), Name = "Dell Inc", ContactEmail = "support@dell.com", Phone = "+1-800-999-3355" },
                    new Vendor { Id = Guid.NewGuid(), Name = "HP Inc", ContactEmail = "support@hp.com", Phone = "+1-800-472-5548" },
                    new Vendor { Id = Guid.NewGuid(), Name = "Microsoft", ContactEmail = "support@microsoft.com", Phone = "+1-425-882-8080" }
                };
                _context.Vendors.AddRange(vendors);
                await _context.SaveChangesAsync();
            }

            if (!_context.Assets.Any())
            {
                // Insert Assets
                var assets = new List<Asset>
                {
                    new Asset
                    {
                        Id = Guid.NewGuid(),
                        AssetTag = "LAPTOP-001",
                        Category = "Laptop",
                        SerialNumber = "SN123456",
                        PurchaseDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                        PurchaseValue = 1200.00m,
                        Status = "AVAILABLE",
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Asset
                    {
                        Id = Guid.NewGuid(),
                        AssetTag = "LAPTOP-002",
                        Category = "Laptop",
                        SerialNumber = "SN123457",
                        PurchaseDate = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                        PurchaseValue = 1200.00m,
                        Status = "AVAILABLE",
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Asset
                    {
                        Id = Guid.NewGuid(),
                        AssetTag = "MONITOR-001",
                        Category = "Monitor",
                        SerialNumber = "SN234567",
                        PurchaseDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                        PurchaseValue = 350.00m,
                        Status = "AVAILABLE",
                        DepartmentId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        CreatedAt = DateTime.UtcNow
                    }
                };
                _context.Assets.AddRange(assets);
                await _context.SaveChangesAsync();
            }

            if (!_context.AssetRequests.Any())
            {
                var requests = new List<AssetRequest>
                {
                    new AssetRequest
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                        AssetType = "Laptop",
                        Reason = "Need laptop for remote work",
                        Status = "APPROVED",
                        CreatedAt = DateTime.UtcNow
                    },
                    new AssetRequest
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                        AssetType = "Monitor",
                        Reason = "Second monitor for productivity",
                        Status = "APPROVED",
                        CreatedAt = DateTime.UtcNow
                    }
                };
                _context.AssetRequests.AddRange(requests);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Seed data inserted successfully!" });
        }
    }
}
