using AssetManagement.Application.dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.interfaces
{
    public interface IMaintenanceRepository
    {
        Task<(List<MaintenanceRecordDto>, int total)> GetAllAsync(string search, int page, int pageSize);

        Task<bool> CreateAsync(CreateMaintenanceRecordDto dto);

        Task<List<DropdownDto>> GetVendorsAsync();
        Task<List<DropdownDto>> GetAssetsAsync();
    }
}
