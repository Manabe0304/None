using AssetManagement.Application.dtos;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.repositories
{
    public class VendorRepository : Application.interfaces.IVendorRepository 
    {
        private readonly AssetDbContext _context;

        public VendorRepository(AssetDbContext context)
        {
            _context = context;
        }
        public async Task<(List<VendorViewDto>, int)> GetAllAsync(string search, int page, int pageSize)
        {
            var query = _context.Vendors
                .Where(x => !x.IsDeleted)
                .Select(v => new VendorViewDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ContactEmail = v.ContactEmail,
                    Phone = v.Phone
                });

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.ContactEmail.Contains(search));
            }

            int total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task<bool> CreateAsync(VendorDto dto)
        {
            var entity = new Vendor
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ContactEmail = dto.ContactEmail,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(VendorDto dto)
        {
            var v = await _context.Vendors.FindAsync(dto.Id);
            if (v == null) return false;

            v.Name = dto.Name;
            v.ContactEmail = dto.ContactEmail;
            v.Phone = dto.Phone;
            v.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var v = await _context.Vendors.FindAsync(id);
            if (v == null) return false;

            v.IsDeleted = true;
            v.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
