using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class MaintanceRecordRepository : IMaintenanceRepository
    {
        private readonly AssetDbContext _context;

        public MaintanceRecordRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MaintenanceRecordDto>, int total)> GetAllAsync(string search, int page, int pageSize)
        {
            var query = from m in _context.MaintenanceRecords
                        join a in _context.Assets on m.AssetId equals a.Id
                        join v in _context.Vendors on m.VendorId equals v.Id into vendor
                        from v in vendor.DefaultIfEmpty()
                        where !m.IsDeleted
                        select new MaintenanceRecordDto
                        {
                            Id = m.Id,
                            AssetTag = a.AssetTag,
                            VendorName = v != null ? v.Name : "",
                            Status = m.Status,
                            Outcome = m.Outcome,
                            EstimatedCost = m.EstimatedCost
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.AssetTag.Contains(search) ||
                    x.VendorName.Contains(search));
            }

            int total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<bool> CreateAsync(CreateMaintenanceRecordDto dto)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == dto.AssetId && !a.IsDeleted);

            if (asset == null)
                throw new InvalidOperationException("Asset not found.");

            if (asset.Status == AssetStatus.BEYOND_REPAIR.ToString() ||
                asset.Status == AssetStatus.LIQUIDATED.ToString())
            {
                throw new InvalidOperationException("Cannot continue maintenance for BEYOND_REPAIR or LIQUIDATED asset.");
            }

            if (asset.Status != AssetStatus.UNDER_MAINTENANCE.ToString())
            {
                throw new InvalidOperationException("Only UNDER_MAINTENANCE assets can have maintenance records.");
            }

            var entity = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AssetId = dto.AssetId,
                VendorId = dto.VendorId,
                Description = dto.Description,
                EstimatedCost = dto.EstimatedCost,
                StartedAt = dto.StartedAt,
                CompletedAt = dto.CompletedAt,
                Outcome = "PENDING",
                Status = "UNDER_MAINTENANCE",
                CreatedAt = DateTime.UtcNow
            };

            _context.MaintenanceRecords.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<DropdownDto>> GetVendorsAsync()
        {
            return await _context.Vendors
                .Select(v => new DropdownDto
                {
                    Id = v.Id,
                    Name = v.Name
                })
                .ToListAsync();
        }

        public async Task<List<DropdownDto>> GetAssetsAsync()
        {
            return await _context.Assets
                .Where(a =>
                    !a.IsDeleted &&
                    a.Status == AssetStatus.UNDER_MAINTENANCE.ToString())
                .Select(a => new DropdownDto
                {
                    Id = a.Id,
                    Name = a.AssetTag
                })
                .ToListAsync();
        }
    }
}