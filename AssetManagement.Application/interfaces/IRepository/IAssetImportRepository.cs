using AssetManagement.Application.dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.interfaces.IRepository
{
    public interface IAssetImportRepository
    {
        Task<int> ImportExcelAsync(IFormFile file);
        Task<List<AssetExportDto>> GetAllForExportAsync();

        Task<byte[]> ExportExcelAsync();
    }
}
