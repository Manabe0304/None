using AssetManagement.Application.dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.interfaces
{
    public interface IVendorRepository
    {
        Task<(List<VendorViewDto> data, int total)> GetAllAsync(string search, int page, int pageSize);

        Task<bool> CreateAsync(VendorDto dto);

        Task<bool> UpdateAsync(VendorDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}
