using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Permissions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace cima.Export;

[Authorize(cimaPermissions.Listings.Default)]
public class ExportAppService : ApplicationService, IExportAppService
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ExportAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
    }

    public async Task<byte[]> ExportPropertiesToExcelAsync(CancellationToken cancellationToken = default)
    {
        var listings = await _listingRepository.GetListAsync(cancellationToken: cancellationToken);
        var architects = await _architectRepository.GetListAsync(cancellationToken: cancellationToken);

        using var workbook = new XLWorkbook();
        
        // Sheet 1: Properties List
        var propertiesSheet = workbook.Worksheets.Add("Propiedades");
        CreatePropertiesSheet(propertiesSheet, listings, architects);

        // Sheet 2: Summary by Status
        var summarySheet = workbook.Worksheets.Add("Resumen");
        CreateSummarySheet(summarySheet, listings);

        // Sheet 3: By Architect
        var architectsSheet = workbook.Worksheets.Add("Por Arquitecto");
        CreateArchitectsSummarySheet(architectsSheet, listings, architects);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreatePropertiesSheet(IXLWorksheet sheet, System.Collections.Generic.List<Listing> listings, System.Collections.Generic.List<Architect> architects)
    {
        // Headers
        var headers = new[] { "ID", "Título", "Tipo", "Categoría", "Transacción", "Estado", "Precio", "Área (m²)", "Recámaras", "Baños", "Ubicación", "Arquitecto", "Fecha Creación" };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Data
        int row = 2;
        foreach (var listing in listings.OrderByDescending(l => l.CreatedAt))
        {
            var architect = architects.FirstOrDefault(a => a.Id == listing.ArchitectId);
            
            sheet.Cell(row, 1).Value = listing.Id.ToString();
            sheet.Cell(row, 2).Value = listing.Title;
            sheet.Cell(row, 3).Value = GetPropertyTypeName(listing.Type);
            sheet.Cell(row, 4).Value = GetPropertyCategoryName(listing.Category);
            sheet.Cell(row, 5).Value = GetTransactionTypeName(listing.TransactionType);
            sheet.Cell(row, 6).Value = GetListingStatusName(listing.Status);
            sheet.Cell(row, 7).Value = listing.Price;
            sheet.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
            sheet.Cell(row, 8).Value = listing.LandArea;
            sheet.Cell(row, 9).Value = listing.Bedrooms;
            sheet.Cell(row, 10).Value = listing.Bathrooms;
            sheet.Cell(row, 11).Value = listing.Location?.ToString() ?? "";
            sheet.Cell(row, 12).Value = architect != null ? $"ID:{architect.UserId}" : "";
            sheet.Cell(row, 13).Value = listing.CreatedAt.ToLocalTime();
            sheet.Cell(row, 13).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            
            row++;
        }

        sheet.Columns().AdjustToContents();
    }

    private void CreateSummarySheet(IXLWorksheet sheet, System.Collections.Generic.List<Listing> listings)
    {
        sheet.Cell(1, 1).Value = "Resumen de Propiedades";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        sheet.Cell(3, 1).Value = "Estado";
        sheet.Cell(3, 2).Value = "Cantidad";
        sheet.Cell(3, 3).Value = "Valor Total";
        sheet.Range("A3:C3").Style.Font.Bold = true;
        sheet.Range("A3:C3").Style.Fill.BackgroundColor = XLColor.LightBlue;

        var statuses = Enum.GetValues<ListingStatus>();
        int row = 4;
        foreach (var status in statuses)
        {
            var filtered = listings.Where(l => l.Status == status).ToList();
            sheet.Cell(row, 1).Value = GetListingStatusName(status);
            sheet.Cell(row, 2).Value = filtered.Count;
            sheet.Cell(row, 3).Value = filtered.Sum(l => l.Price);
            sheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        // Total row
        row++;
        sheet.Cell(row, 1).Value = "TOTAL";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        sheet.Cell(row, 2).Value = listings.Count;
        sheet.Cell(row, 2).Style.Font.Bold = true;
        sheet.Cell(row, 3).Value = listings.Sum(l => l.Price);
        sheet.Cell(row, 3).Style.Font.Bold = true;
        sheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";

        sheet.Columns().AdjustToContents();
    }

    private void CreateArchitectsSummarySheet(IXLWorksheet sheet, System.Collections.Generic.List<Listing> listings, System.Collections.Generic.List<Architect> architects)
    {
        sheet.Cell(1, 1).Value = "Propiedades por Arquitecto";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        sheet.Cell(3, 1).Value = "Arquitecto ID";
        sheet.Cell(3, 2).Value = "Estado";
        sheet.Cell(3, 3).Value = "Propiedades";
        sheet.Cell(3, 4).Value = "Valor Total";
        sheet.Range("A3:D3").Style.Font.Bold = true;
        sheet.Range("A3:D3").Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 4;
        foreach (var architect in architects.Where(a => a.IsActive))
        {
            var architectListings = listings.Where(l => l.ArchitectId == architect.Id).ToList();
            if (!architectListings.Any()) continue;

            sheet.Cell(row, 1).Value = architect.UserId.ToString();
            sheet.Cell(row, 2).Value = architect.IsActive ? "Activo" : "Inactivo";
            sheet.Cell(row, 3).Value = architectListings.Count;
            sheet.Cell(row, 4).Value = architectListings.Sum(l => l.Price);
            sheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        // Unassigned properties (no matching architect in the list)
        var unassigned = listings.Where(l => !architects.Any(a => a.Id == l.ArchitectId)).ToList();
        if (unassigned.Any())
        {
            sheet.Cell(row, 1).Value = "(Sin asignar)";
            sheet.Cell(row, 2).Value = "-";
            sheet.Cell(row, 3).Value = unassigned.Count;
            sheet.Cell(row, 4).Value = unassigned.Sum(l => l.Price);
            sheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
        }

        sheet.Columns().AdjustToContents();
    }

    private static string GetPropertyTypeName(PropertyType type) => type switch
    {
        PropertyType.House => "Casa",
        PropertyType.Apartment => "Departamento",
        PropertyType.Condo => "Condominio",
        PropertyType.Townhouse => "Casa Adosada",
        PropertyType.Villa => "Villa",
        PropertyType.Office => "Oficina",
        PropertyType.Warehouse => "Bodega",
        PropertyType.RetailSpace => "Local Comercial",
        PropertyType.Restaurant => "Restaurante",
        PropertyType.Hotel => "Hotel",
        PropertyType.MixedUseBuilding => "Edificio Mixto",
        PropertyType.LiveWorkSpace => "Live/Work",
        PropertyType.ResidentialLand => "Terreno Residencial",
        PropertyType.CommercialLand => "Terreno Comercial",
        PropertyType.AgriculturalLand => "Terreno Agrícola",
        _ => type.ToString()
    };

    private static string GetPropertyCategoryName(PropertyCategory category) => category switch
    {
        PropertyCategory.Residential => "Residencial",
        PropertyCategory.Commercial => "Comercial",
        PropertyCategory.Industrial => "Industrial",
        _ => category.ToString()
    };

    private static string GetTransactionTypeName(TransactionType type) => type switch
    {
        TransactionType.Sale => "Venta",
        TransactionType.Rent => "Renta",
        _ => type.ToString()
    };

    private static string GetListingStatusName(ListingStatus status) => status switch
    {
        ListingStatus.Draft => "Borrador",
        ListingStatus.Published => "Publicada",
        ListingStatus.Archived => "Archivada",
        ListingStatus.Portfolio => "Portafolio",
        _ => status.ToString()
    };
}
