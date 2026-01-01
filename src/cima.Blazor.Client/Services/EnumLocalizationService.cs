using cima.Domain.Shared;
using cima.Localization;
using Microsoft.Extensions.Localization;
using System;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Servicio helper para obtener traducciones de enums
/// </summary>
public class EnumLocalizationService
{
    private readonly IStringLocalizer<cimaResource> _localizer;

    public EnumLocalizationService(IStringLocalizer<cimaResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Localiza cualquier enum usando el nombre del tipo y el valor
    /// </summary>
    public string Localize<TEnum>(TEnum enumValue) where TEnum : struct, System.Enum
    {
        var enumType = typeof(TEnum).Name;
        var enumName = enumValue.ToString();
        var key = $"{enumType}:{enumName}";
        
        var localized = _localizer[key];
        return localized.ResourceNotFound ? enumName : localized.Value;
    }

    public string GetTransactionTypeName(TransactionType type)
    {
        return type switch
        {
            TransactionType.Sale => _localizer["TransactionType:Sale"],
            TransactionType.Rent => _localizer["TransactionType:Rent"],
            TransactionType.Lease => _localizer["TransactionType:Lease"],
            _ => type.ToString()
        };
    }

    public string GetPropertyCategoryName(PropertyCategory category)
    {
        return category switch
        {
            PropertyCategory.Residential => _localizer["PropertyCategory:Residential"],
            PropertyCategory.Commercial => _localizer["PropertyCategory:Commercial"],
            PropertyCategory.Mixed => _localizer["PropertyCategory:Mixed"],
            PropertyCategory.Land => _localizer["PropertyCategory:Land"],
            _ => category.ToString()
        };
    }

    public string GetPropertyTypeName(PropertyType type)
    {
        return type switch
        {
            // Residenciales
            PropertyType.House => _localizer["PropertyType:House"],
            PropertyType.Apartment => _localizer["PropertyType:Apartment"],
            PropertyType.Condo => _localizer["PropertyType:Condo"],
            PropertyType.Townhouse => _localizer["PropertyType:Townhouse"],
            PropertyType.Villa => _localizer["PropertyType:Villa"],
            
            // Comerciales
            PropertyType.Office => _localizer["PropertyType:Office"],
            PropertyType.Warehouse => _localizer["PropertyType:Warehouse"],
            PropertyType.RetailSpace => _localizer["PropertyType:RetailSpace"],
            PropertyType.Restaurant => _localizer["PropertyType:Restaurant"],
            PropertyType.Hotel => _localizer["PropertyType:Hotel"],
            
            // Mixtos
            PropertyType.MixedUseBuilding => _localizer["PropertyType:MixedUseBuilding"],
            PropertyType.LiveWorkSpace => _localizer["PropertyType:LiveWorkSpace"],
            
            // Terrenos
            PropertyType.ResidentialLand => _localizer["PropertyType:ResidentialLand"],
            PropertyType.CommercialLand => _localizer["PropertyType:CommercialLand"],
            PropertyType.AgriculturalLand => _localizer["PropertyType:AgriculturalLand"],
            
            _ => type.ToString()
        };
    }

    public string GetListingStatusName(ListingStatus status)
    {
        return status switch
        {
            ListingStatus.Draft => _localizer["ListingStatus:Draft"],
            ListingStatus.Published => _localizer["ListingStatus:Published"],
            ListingStatus.Archived => _localizer["ListingStatus:Archived"],
            ListingStatus.Portfolio => _localizer["ListingStatus:Portfolio"],
            _ => status.ToString()
        };
    }

    public string GetPortfolioCategoryName(PortfolioCategory category)
    {
        return category switch
        {
            PortfolioCategory.ResidentialConstruction => _localizer["PortfolioCategory:ResidentialConstruction"],
            PortfolioCategory.CommercialConstruction => _localizer["PortfolioCategory:CommercialConstruction"],
            PortfolioCategory.Renovation => _localizer["PortfolioCategory:Renovation"],
            PortfolioCategory.InteriorDesign => _localizer["PortfolioCategory:InteriorDesign"],
            PortfolioCategory.ArchitecturalDesign => _localizer["PortfolioCategory:ArchitecturalDesign"],
            PortfolioCategory.Landscaping => _localizer["PortfolioCategory:Landscaping"],
            PortfolioCategory.Other => _localizer["PortfolioCategory:Other"],
            _ => category.ToString()
        };
    }
}
