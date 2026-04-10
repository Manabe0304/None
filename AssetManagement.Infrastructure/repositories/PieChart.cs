using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using static AssetManagement.Application.dtos.ChartDto;
namespace AssetManagement.Infrastructure.repositories
{
    public class PieChart
    {
        private readonly AssetDbContext _context;

        public PieChart(AssetDbContext context)
        {
            _context = context;
        }
        public async Task<List<AssetStatusDto>> GetAssetStatusSummary()
        {
            var statuses = new List<string>
    {
        "AVAILABLE",
        "IN_USE",
        "BROKEN",
        "UNDER_MAINTENANCE",
        "BEYOND_REPAIR",
        "LIQUIDATED"
    };

            var data = await _context.Assets
                .GroupBy(a => a.Status)
                .Select(g => new AssetStatusDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = statuses.Select(status =>
                new AssetStatusDto
                {
                    Status = status,
                    Count = data.FirstOrDefault(x => x.Status == status)?.Count ?? 0
                }).ToList();

            return result;
        }


        public async Task<List<RepairSpendingDto>> GetRepairSpendingByMonth()
        {
            return await _context.MaintenanceRecords
                .Where(x => x.CompletedAt != null)
                .GroupBy(x => new { x.CompletedAt.Value.Year, x.CompletedAt.Value.Month })
                .Select(g => new RepairSpendingDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = (decimal)g.Sum(x => x.RepairCost)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<object> GetDashboardKPI()
        {
            var totalAssets = await _context.Assets.CountAsync();

            var availableAssets = await _context.Assets
                .Where(x => x.Status == "AVAILABLE" && !x.IsDeleted)
                .CountAsync();

            var maintenanceAssets = await _context.Assets
                .Where(x => x.Status == "UNDER_MAINTENANCE" && !x.IsDeleted)
                .CountAsync();

            var pendingRequests = await _context.AssetRequests
                .Where(x => x.Status == "PENDING" && !x.IsDeleted)
                .CountAsync();

            return new
            {
                totalAssets,
                availableAssets,
                maintenanceAssets,
                pendingRequests
            };
        }

        public async Task<object> GetAssetActivityTrend()
        {
            var data = await _context.Assignments
                .GroupBy(x => new { x.AssignedAt.Year, x.AssignedAt.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    count = g.Count()
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.month)
                .ToListAsync();

            return data;
        }

        public async Task<object> GetAssetsByDepartment()
        {
            var data = await _context.Assets
                .Include(x => x.Department)
                .GroupBy(x => x.Department.Name)
                .Select(g => new
                {
                    department = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return data;
        }
        public async Task<object> GetTopUsedAssets()
        {
            var data = await _context.Assignments
                .GroupBy(x => x.Asset.AssetTag)
                .Select(g => new
                {
                    assetName = g.Key,
                    usage = g.Count()
                })
                .OrderByDescending(x => x.usage)
                .Take(5)
                .ToListAsync();

            return data;
        }

        public class AssetRepository : IAssetRepository
        {
            private readonly AssetDbContext _context;

            public AssetRepository(AssetDbContext context)
            {
                _context = context;
            }

            public async Task<Application.dtos.PagedResult<AssetListDto>> GetPagedAsync(int page, string? search)
            {
                var query = _context.Assets
                    .Include(a => a.Department)
                    .Include(a => a.CurrentUser)
                    .Where(a => !a.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.AssetTag.Contains(search) ||
                        x.SerialNumber.Contains(search) ||
                        x.Category.Contains(search));
                }

                var total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * 10)
                    .Take(10)
                    .Select(x => new AssetListDto
                    {
                        Id = x.Id,
                        AssetTag = x.AssetTag,
                        Category = x.Category,
                        Model = x.Model,
                        SerialNumber = x.SerialNumber,
                        PurchaseDate = x.PurchaseDate,
                        PurchaseValue = x.PurchaseValue,
                        Status = x.Status,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.Name : null,
                        CurrentUserId = x.CurrentUserId,
                        CurrentUserName = x.CurrentUser != null ? x.CurrentUser.FullName : null,
                        AssignmentType = x.Status == "IN_USE_SHARED" ? "DEPARTMENT" : x.CurrentUserId != null ? "PERSONAL" : "UNASSIGNED"
                    })
                    .ToListAsync();

                return new Application.dtos.PagedResult<AssetListDto>
                {
                    Items = items,
                    TotalCount = total
                };
            }

            public async Task<Asset?> GetByIdAsync(Guid id)
                 => await _context.Assets.FindAsync(id);

            public async Task AddAsync(Asset asset)
            {
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateAsync(Asset asset)
            {
                _context.Assets.Update(asset);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(Asset asset)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }


            public async Task SoftDeleteAsync(Guid id)
            {
                var asset = await _context.Assets.FindAsync(id);

                if (asset == null)
                    throw new Exception("Asset not found");

                asset.IsDeleted = true;
                asset.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            public async Task RestoreAsync(Guid id)
            {
                var asset = await _context.Assets.FindAsync(id);

                if (asset == null)
                    throw new Exception("Asset not found");

                asset.IsDeleted = false;
                asset.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            public async Task<List<Department>> GetAllAsync()
            {
                return await _context.Departments
                    .AsNoTracking()
                    .ToListAsync();
            }


            public async Task<Application.dtos.PagedResult<AssetListDto>> GetDeletedAsync(int page, string? search)
            {
                var query = _context.Assets
                    .Where(x => x.IsDeleted == true);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x => x.AssetTag.Contains(search));
                }

                var total = await query.CountAsync();

                var data = await query
                    .Skip((page - 1) * 10)
                    .Take(10)
                    .Select(x => new AssetListDto
                    {
                        Id = x.Id,
                        AssetTag = x.AssetTag,
                        Category = x.Category,
                        Model = x.Model,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.Name : null,
                        AssignmentType = x.Status == "IN_USE_SHARED" ? "DEPARTMENT" : x.CurrentUserId != null ? "PERSONAL" : "UNASSIGNED"
                    })
                    .ToListAsync();

                return new Application.dtos.PagedResult<AssetListDto>
                {
                    Items = data,
                    TotalCount = total
                };
            }
        }
    }
}