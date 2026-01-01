using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using cima.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace cima.EntityFrameworkCore.Data;

public class PropertyCatalogBackfillSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IDbContextProvider<cimaDbContext> _dbContextProvider;
    private readonly ILogger<PropertyCatalogBackfillSeeder> _logger;

    public PropertyCatalogBackfillSeeder(
        IDbContextProvider<cimaDbContext> dbContextProvider,
        ILogger<PropertyCatalogBackfillSeeder> logger)
    {
        _dbContextProvider = dbContextProvider;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var connection = dbContext.Database.GetDbConnection();
        var opened = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            opened = true;
        }

        try
        {
            var listingsHasLegacyCategory = await ColumnExistsAsync(connection, "Listings", "Category");
            var listingsHasLegacyType = await ColumnExistsAsync(connection, "Listings", "Type");
            var listingsHasCategoryId = await ColumnExistsAsync(connection, "Listings", "CategoryId");
            var listingsHasTypeId = await ColumnExistsAsync(connection, "Listings", "TypeId");

            if (listingsHasLegacyCategory && listingsHasCategoryId)
            {
                await BackfillListingCategoriesAsync(dbContext);
            }

            if (listingsHasLegacyType && listingsHasTypeId)
            {
                await BackfillListingTypesAsync(dbContext);
            }

            var portfolioHasLegacyCategory = await ColumnExistsAsync(connection, "PortfolioProjects", "Category");
            var portfolioHasCategoryId = await ColumnExistsAsync(connection, "PortfolioProjects", "CategoryId");

            if (portfolioHasLegacyCategory && portfolioHasCategoryId)
            {
                await BackfillPortfolioCategoriesAsync(dbContext);
            }
        }
        finally
        {
            if (opened)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 1
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = @tableName
  AND column_name = @columnName
LIMIT 1;";

        var tableParam = command.CreateParameter();
        tableParam.ParameterName = "@tableName";
        tableParam.Value = tableName;
        command.Parameters.Add(tableParam);

        var columnParam = command.CreateParameter();
        columnParam.ParameterName = "@columnName";
        columnParam.Value = columnName;
        command.Parameters.Add(columnParam);

        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    private async Task BackfillListingCategoriesAsync(cimaDbContext dbContext)
    {
        var otherCategoryId = PropertyCatalogIds.Categories.Other.ToString();
        var sql = $@"
UPDATE ""Listings""
SET ""CategoryId"" = CASE ""Category""
    WHEN 0 THEN '{PropertyCatalogIds.Categories.Residential}'::uuid
    WHEN 1 THEN '{PropertyCatalogIds.Categories.Commercial}'::uuid
    WHEN 2 THEN '{PropertyCatalogIds.Categories.Industrial}'::uuid
    WHEN 3 THEN '{PropertyCatalogIds.Categories.Land}'::uuid
    WHEN 4 THEN '{PropertyCatalogIds.Categories.Mixed}'::uuid
    ELSE '{PropertyCatalogIds.Categories.Other}'::uuid
END
WHERE ""CategoryId"" IN ('00000000-0000-0000-0000-000000000000'::uuid, '{otherCategoryId}'::uuid);";

        var updated = await dbContext.Database.ExecuteSqlRawAsync(sql);
        if (updated > 0)
        {
            _logger.LogInformation("Backfilled Listing.CategoryId for {Count} rows", updated);
        }
    }

    private async Task BackfillListingTypesAsync(cimaDbContext dbContext)
    {
        var otherTypeId = PropertyCatalogIds.Types.Other.ToString();
        var sql = $@"
UPDATE ""Listings""
SET ""TypeId"" = CASE ""Type""
    WHEN 0 THEN '{PropertyCatalogIds.Types.House}'::uuid
    WHEN 1 THEN '{PropertyCatalogIds.Types.Apartment}'::uuid
    WHEN 2 THEN '{PropertyCatalogIds.Types.Condo}'::uuid
    WHEN 3 THEN '{PropertyCatalogIds.Types.Townhouse}'::uuid
    WHEN 4 THEN '{PropertyCatalogIds.Types.Villa}'::uuid
    WHEN 10 THEN '{PropertyCatalogIds.Types.Office}'::uuid
    WHEN 11 THEN '{PropertyCatalogIds.Types.Warehouse}'::uuid
    WHEN 12 THEN '{PropertyCatalogIds.Types.RetailSpace}'::uuid
    WHEN 13 THEN '{PropertyCatalogIds.Types.Restaurant}'::uuid
    WHEN 14 THEN '{PropertyCatalogIds.Types.Hotel}'::uuid
    WHEN 20 THEN '{PropertyCatalogIds.Types.MixedUseBuilding}'::uuid
    WHEN 21 THEN '{PropertyCatalogIds.Types.LiveWorkSpace}'::uuid
    WHEN 30 THEN '{PropertyCatalogIds.Types.ResidentialLand}'::uuid
    WHEN 31 THEN '{PropertyCatalogIds.Types.CommercialLand}'::uuid
    WHEN 32 THEN '{PropertyCatalogIds.Types.AgriculturalLand}'::uuid
    ELSE '{PropertyCatalogIds.Types.Other}'::uuid
END
WHERE ""TypeId"" IN ('00000000-0000-0000-0000-000000000000'::uuid, '{otherTypeId}'::uuid);";

        var updated = await dbContext.Database.ExecuteSqlRawAsync(sql);
        if (updated > 0)
        {
            _logger.LogInformation("Backfilled Listing.TypeId for {Count} rows", updated);
        }
    }

    private async Task BackfillPortfolioCategoriesAsync(cimaDbContext dbContext)
    {
        var otherCategoryId = PropertyCatalogIds.Categories.Other.ToString();
        var sql = $@"
UPDATE ""PortfolioProjects""
SET ""CategoryId"" = CASE ""Category""
    WHEN 0 THEN '{PropertyCatalogIds.Categories.Residential}'::uuid
    WHEN 1 THEN '{PropertyCatalogIds.Categories.Commercial}'::uuid
    WHEN 2 THEN '{PropertyCatalogIds.Categories.Mixed}'::uuid
    WHEN 3 THEN '{PropertyCatalogIds.Categories.Mixed}'::uuid
    WHEN 4 THEN '{PropertyCatalogIds.Categories.Mixed}'::uuid
    WHEN 5 THEN '{PropertyCatalogIds.Categories.Land}'::uuid
    WHEN 99 THEN '{PropertyCatalogIds.Categories.Other}'::uuid
    ELSE '{PropertyCatalogIds.Categories.Other}'::uuid
END
WHERE ""CategoryId"" IN ('00000000-0000-0000-0000-000000000000'::uuid, '{otherCategoryId}'::uuid);";

        var updated = await dbContext.Database.ExecuteSqlRawAsync(sql);
        if (updated > 0)
        {
            _logger.LogInformation("Backfilled Portfolio.CategoryId for {Count} rows", updated);
        }
    }
}
