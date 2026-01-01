using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace cima.Application.Data;

public class PropertyCatalogDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PropertyCategoryEntity, Guid> _categoryRepository;
    private readonly IRepository<PropertyTypeEntity, Guid> _typeRepository;
    private readonly ILogger<PropertyCatalogDataSeeder> _logger;

    public PropertyCatalogDataSeeder(
        IRepository<PropertyCategoryEntity, Guid> categoryRepository,
        IRepository<PropertyTypeEntity, Guid> typeRepository,
        ILogger<PropertyCatalogDataSeeder> logger)
    {
        _categoryRepository = categoryRepository;
        _typeRepository = typeRepository;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedCategoriesAsync();
        await SeedTypesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        var categories = new List<PropertyCategoryEntity>
        {
            new(PropertyCatalogIds.Categories.Residential, "Residencial", "Vivienda y uso residencial", 1, true),
            new(PropertyCatalogIds.Categories.Commercial, "Comercial", "Comercio y oficinas", 2, true),
            new(PropertyCatalogIds.Categories.Industrial, "Industrial", "Industrial y manufactura", 3, true),
            new(PropertyCatalogIds.Categories.Land, "Terreno", "Terrenos", 4, true),
            new(PropertyCatalogIds.Categories.Mixed, "Uso mixto", "Uso mixto", 5, true),
            new(PropertyCatalogIds.Categories.Other, "Otros", "Otros", 99, true)
        };

        foreach (var category in categories)
        {
            var existing = await _categoryRepository.FindAsync(category.Id);
            if (existing != null)
            {
                continue;
            }

            await _categoryRepository.InsertAsync(category, autoSave: true);
            _logger.LogInformation("Seeded property category {Name}", category.Name);
        }
    }

    private async Task SeedTypesAsync()
    {
        var types = new List<PropertyTypeEntity>
        {
            new(PropertyCatalogIds.Types.House, "Casa", PropertyCatalogIds.Categories.Residential, "Casa unifamiliar", 1, true),
            new(PropertyCatalogIds.Types.Apartment, "Departamento", PropertyCatalogIds.Categories.Residential, "Departamento o apartamento", 2, true),
            new(PropertyCatalogIds.Types.Condo, "Condominio", PropertyCatalogIds.Categories.Residential, "Condominio", 3, true),
            new(PropertyCatalogIds.Types.Townhouse, "Casa adosada", PropertyCatalogIds.Categories.Residential, "Casa adosada", 4, true),
            new(PropertyCatalogIds.Types.Villa, "Villa", PropertyCatalogIds.Categories.Residential, "Villa", 5, true),
            new(PropertyCatalogIds.Types.Office, "Oficina", PropertyCatalogIds.Categories.Commercial, "Oficina", 1, true),
            new(PropertyCatalogIds.Types.Warehouse, "Bodega", PropertyCatalogIds.Categories.Commercial, "Bodega o almacen", 2, true),
            new(PropertyCatalogIds.Types.RetailSpace, "Local comercial", PropertyCatalogIds.Categories.Commercial, "Local comercial", 3, true),
            new(PropertyCatalogIds.Types.Restaurant, "Restaurante", PropertyCatalogIds.Categories.Commercial, "Restaurante", 4, true),
            new(PropertyCatalogIds.Types.Hotel, "Hotel", PropertyCatalogIds.Categories.Commercial, "Hotel", 5, true),
            new(PropertyCatalogIds.Types.MixedUseBuilding, "Edificio de uso mixto", PropertyCatalogIds.Categories.Mixed, "Edificio de uso mixto", 1, true),
            new(PropertyCatalogIds.Types.LiveWorkSpace, "Espacio vive-trabaja", PropertyCatalogIds.Categories.Mixed, "Espacio vive-trabaja", 2, true),
            new(PropertyCatalogIds.Types.ResidentialLand, "Terreno residencial", PropertyCatalogIds.Categories.Land, "Terreno residencial", 1, true),
            new(PropertyCatalogIds.Types.CommercialLand, "Terreno comercial", PropertyCatalogIds.Categories.Land, "Terreno comercial", 2, true),
            new(PropertyCatalogIds.Types.AgriculturalLand, "Terreno agricola", PropertyCatalogIds.Categories.Land, "Terreno agricola", 3, true),
            new(PropertyCatalogIds.Types.Other, "Otro", PropertyCatalogIds.Categories.Other, "Otro", 99, true)
        };

        foreach (var type in types)
        {
            var existing = await _typeRepository.FindAsync(type.Id);
            if (existing != null)
            {
                continue;
            }

            await _typeRepository.InsertAsync(type, autoSave: true);
            _logger.LogInformation("Seeded property type {Name}", type.Name);
        }
    }
}
