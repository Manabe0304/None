using AssetManagement.Application.dtos;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.interfaces
{
    public interface IAssetRepository
    {
        Task<dtos.PagedResult<AssetListDto>> GetPagedAsync(int page, string? search);
        Task<Asset?> GetByIdAsync(Guid id);
        Task AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task DeleteAsync(Asset asset);

        Task SoftDeleteAsync(Guid id);

        Task RestoreAsync(Guid id);

        Task<dtos.PagedResult<AssetListDto>> GetDeletedAsync(int page, string? search);
        Task<List<Department>> GetAllAsync();
    }
}
