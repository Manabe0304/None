using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;

public class AssetImportRepository : IAssetImportRepository
{
    private readonly AssetDbContext _context;

    public AssetImportRepository(AssetDbContext context)
    {
        _context = context;
    }

    public async Task<int> ImportExcelAsync(IFormFile file)
    {
        ExcelPackage.License.SetNonCommercialPersonal("Hiep Ngo");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var package = new ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets[0];

        int rowCount = sheet.Dimension.Rows;
        int success = 0;

        for (int row = 2; row <= rowCount; row++)
        {
            var assetTag = sheet.Cells[row, 1].Text?.Trim();
            var category = sheet.Cells[row, 2].Text?.Trim();
            var model = sheet.Cells[row, 3].Text?.Trim();
            var serial = sheet.Cells[row, 4].Text?.Trim();
            var purchaseDateText = sheet.Cells[row, 5].Text?.Trim();
            var purchaseValueText = sheet.Cells[row, 6].Text?.Trim();
            var status = sheet.Cells[row, 7].Text?.Trim();

            if (string.IsNullOrWhiteSpace(assetTag))
                continue;

            // ❗ check duplicate
            bool exists = await _context.Assets
                .AnyAsync(x => x.AssetTag == assetTag);

            if (exists) continue;

            // 📅 parse date
            DateTime? purchaseDate = null;
            if (DateTime.TryParse(purchaseDateText, out var parsedDate))
            {
                purchaseDate = parsedDate;
            }

            // 💰 parse number
            decimal? purchaseValue = null;
            if (decimal.TryParse(purchaseValueText, out var parsedValue))
            {
                purchaseValue = parsedValue;
            }

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                AssetTag = assetTag,
                Category = category,
                Model = model,
                SerialNumber = serial,
                PurchaseDate = purchaseDate,
                PurchaseValue = purchaseValue,
                Status = string.IsNullOrEmpty(status) ? "AVAILABLE" : status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            success++;
        }

        await _context.SaveChangesAsync();
        return success;
    }

    public async Task<List<AssetExportDto>> GetAllForExportAsync()
    {
        var data = await (
            from a in _context.Assets
            join d in _context.Departments
                on a.DepartmentId equals d.Id into dept
            from d in dept.DefaultIfEmpty()

            join u in _context.Users
                on a.CurrentUserId equals u.Id into usr
            from u in usr.DefaultIfEmpty()

            where !a.IsDeleted

            select new AssetExportDto
            {
                AssetTag = a.AssetTag,
                Category = a.Category,
                Model = a.Model,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseValue = a.PurchaseValue,
                Status = a.Status,

                DepartmentName = d != null ? d.Name : "",
                UserEmail = u != null ? u.Email : ""
            }
        ).ToListAsync();

        return data;
    }

    public async Task<byte[]> ExportExcelAsync()
    {
        var data = await GetAllForExportAsync();

        ExcelPackage.License.SetNonCommercialPersonal("Hiep Ngo");

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Assets");

        // HEADER
        sheet.Cells[1, 1].Value = "Asset Tag";
        sheet.Cells[1, 2].Value = "Category";
        sheet.Cells[1, 3].Value = "Model";
        sheet.Cells[1, 4].Value = "Serial Number";
        sheet.Cells[1, 5].Value = "Purchase Date";
        sheet.Cells[1, 6].Value = "Purchase Value";
        sheet.Cells[1, 7].Value = "Status";
        sheet.Cells[1, 8].Value = "Department";
        sheet.Cells[1, 9].Value = "User Email";

        int row = 2;

        foreach (var item in data)
        {
            sheet.Cells[row, 1].Value = item.AssetTag;
            sheet.Cells[row, 2].Value = item.Category;
            sheet.Cells[row, 3].Value = item.Model;
            sheet.Cells[row, 4].Value = item.SerialNumber;
            sheet.Cells[row, 5].Value = item.PurchaseDate?.ToString("yyyy-MM-dd");
            sheet.Cells[row, 6].Value = item.PurchaseValue;
            sheet.Cells[row, 7].Value = item.Status;
            sheet.Cells[row, 8].Value = item.DepartmentName;
            sheet.Cells[row, 9].Value = item.UserEmail;

            row++;
        }

        // AUTO FIT
        sheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}