using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Entities.Portfolio;
using cima.Domain.Shared;

namespace cima.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class cimaDbContext :
    AbpDbContext<cimaDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Architect> Architects { get; set; }
    public DbSet<ContactRequest> ContactRequests { get; set; }
    public DbSet<FeaturedListing> FeaturedListings { get; set; }
    public DbSet<ListingPriceHistory> ListingPriceHistories { get; set; }
    public DbSet<ListingSale> ListingSales { get; set; }
    public DbSet<PropertyCategoryEntity> PropertyCategories { get; set; }
    public DbSet<PropertyTypeEntity> PropertyTypes { get; set; }
    public DbSet<TransactionTypeEntity> TransactionTypes { get; set; }
    public DbSet<PortfolioProject> PortfolioProjects { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public cimaDbContext(DbContextOptions<cimaDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        
        /* Configure your own tables/entities inside here */

        builder.Entity<PortfolioProject>(b =>
        {
            b.ToTable("PortfolioProjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(5000); // Rich text
            b.Property(x => x.Location).HasMaxLength(200);
            b.Property(x => x.CoverImage).HasMaxLength(2048);
            b.Property(x => x.CategoryId)
                .HasDefaultValue(PropertyCatalogIds.Categories.Other);
            b.Property<int>("LegacyCategory")
                .HasColumnName("Category");
            b.Property(x => x.ListingId);
            
            b.OwnsMany(x => x.Gallery, ib =>
            {
                ib.ToTable("PortfolioImages");
                ib.WithOwner().HasForeignKey("PortfolioProjectId");
                ib.HasKey("PortfolioProjectId", "ImageId");
                
                ib.Property(i => i.ImageId).IsRequired().ValueGeneratedNever();
                ib.Property(i => i.Url).IsRequired().HasMaxLength(2048);
                ib.Property(i => i.ThumbnailUrl).HasMaxLength(2048).HasDefaultValue("");
                ib.Property(i => i.AltText).HasMaxLength(500);
                ib.Property(i => i.Caption).HasMaxLength(500);
                ib.Property(i => i.Tags).HasMaxLength(200);
                ib.Property(i => i.SortOrder).IsRequired().HasDefaultValue(0);
            });
            
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.ListingId).IsUnique();
            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Listing>()
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        var b = builder.Entity<Listing>();
        b.ToTable("Listings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(5000);
        // Location is now an Owned Type (Value Object)
        b.OwnsOne(x => x.Location, a => 
        {
            a.Property(p => p.Value).HasColumnName("Location").HasMaxLength(500);
            a.WithOwner();
        });
        
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.LandArea).HasPrecision(10, 2).IsRequired();
        b.Property(x => x.ConstructionArea).HasPrecision(10, 2).IsRequired();   
        b.Property(x => x.CategoryId)
            .HasDefaultValue(PropertyCatalogIds.Categories.Other);
        b.Property(x => x.TypeId)
            .HasDefaultValue(PropertyCatalogIds.Types.Other);
        b.Property<int>("LegacyCategory")
            .HasColumnName("Category");
        b.Property<int>("LegacyType")
            .HasColumnName("Type");
        
        // Indices para busqueda optimizada
        b.HasIndex(x => x.Status).HasDatabaseName("IX_Listings_Status");
        // Location index might need adjustment if using OwnsOne, but EF Core maps it to 'Location' column so it might work if Property name matches
        // b.HasIndex(x => x.Location) ... this refers to Navigation property now, might fail. 
        // We index the column "Location" or use property access? 
        // b.OwnsOne maps to table columns. To index, we likely need to access the property on the owned type or shadow property.
        // For now commenting out Location index to avoid complexity if it fails.
        // b.HasIndex(x => x.Location).HasDatabaseName("IX_Listings_Location"); 

        b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Listings_CreatedAt");
        b.HasIndex(x => x.Price).HasDatabaseName("IX_Listings_Price");
        b.HasIndex(x => x.ArchitectId).HasDatabaseName("IX_Listings_ArchitectId");
        
        // Indices compuestos
        b.HasIndex(x => new { x.Status, x.ArchitectId }).HasDatabaseName("IX_Listings_Status_ArchitectId");
        // b.HasIndex(x => new { x.Status, x.Location }).HasDatabaseName("IX_Listings_Status_Location");
        b.HasIndex(x => new { x.Status, x.TypeId, x.TransactionType })
            .HasDatabaseName("IX_Listings_Status_Type_Transaction");
        b.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Listings_CategoryId");
        b.HasIndex(x => x.TypeId).HasDatabaseName("IX_Listings_TypeId");
        
        b.HasOne(x => x.Architect)
            .WithMany(a => a.Listings)
            .HasForeignKey(x => x.ArchitectId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PropertyType)
            .WithMany()
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.OwnsMany(x => x.Images, ib =>
        {
            ib.ToTable("ListingImages");
            ib.WithOwner().HasForeignKey("ListingId");
            ib.HasKey("ListingId", "ImageId");
            
            ib.Property(i => i.ImageId).IsRequired().ValueGeneratedNever();
            ib.Property(i => i.Url).IsRequired().HasMaxLength(2048);
            ib.Property(i => i.ThumbnailUrl).HasMaxLength(2048).HasDefaultValue("");
            ib.Property(i => i.AltText).HasMaxLength(500);
            ib.Property(i => i.FileSize).IsRequired();
            ib.Property(i => i.ContentType).IsRequired().HasMaxLength(100);
            ib.Property(i => i.SortOrder).IsRequired().HasDefaultValue(0);
        });

        builder.Entity<Architect>(b =>
        {
            b.ToTable("Architects");
            b.HasKey(x => x.Id);
            b.Property(x => x.TotalListingsPublished).IsRequired();
            b.Property(x => x.ActiveListings).IsRequired();
            b.Property(x => x.RegistrationDate).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<ContactRequest>(b =>
        {
            b.ToTable("ContactRequests");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(20);  // Ahora nullable
            b.Property(x => x.Message).IsRequired().HasMaxLength(5000);
            b.HasIndex(x => new { x.Status, x.CreatedAt });
            b.HasIndex(x => x.ListingId);
            b.HasOne(x => x.Listing).WithMany().HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Architect).WithMany().HasForeignKey(x => x.ArchitectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FeaturedListing>(b =>
        {
            b.ToTable("FeaturedListings");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ListingId).IsUnique(); // Solo una vez por listing
            // DisplayOrder eliminado - orden aleatorio en consulta
            b.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ListingSale>(b =>
        {
            b.ToTable("ListingSales");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ListingId).IsUnique();
            b.HasIndex(x => x.ArchitectId);
            b.HasIndex(x => x.SoldAt);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
            b.Property(x => x.Notes).HasMaxLength(1000);
            b.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Architect)
                .WithMany()
                .HasForeignKey(x => x.ArchitectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ListingPriceHistory>(b =>
        {
            b.ToTable("ListingPriceHistories");
            b.HasKey(x => x.Id);
            b.Property(x => x.OldPrice).HasPrecision(18, 2);
            b.Property(x => x.NewPrice).HasPrecision(18, 2);
            b.Property(x => x.ChangedByUserName).HasMaxLength(256);
            b.Property(x => x.ClientIpAddress).HasMaxLength(45); // IPv6 max length
            b.Property(x => x.UserAgent).HasMaxLength(512);
            b.Property(x => x.CorrelationId).HasMaxLength(128);
            b.Property(x => x.ChangeReason).HasMaxLength(500);
            b.Property(x => x.SessionId).HasMaxLength(128);
            b.Property(x => x.AuthenticationMethod).HasMaxLength(50);
            
            // Anti-tampering fields
            b.Property(x => x.IntegrityHash).IsRequired().HasMaxLength(64); // SHA256 hex
            b.Property(x => x.PreviousRecordHash).HasMaxLength(64);
            
            // Índices para consultas anti-fraude
            b.HasIndex(x => x.ListingId).HasDatabaseName("IX_PriceHistory_ListingId");
            b.HasIndex(x => x.ChangedAt).HasDatabaseName("IX_PriceHistory_ChangedAt");
            b.HasIndex(x => x.ChangedByUserId).HasDatabaseName("IX_PriceHistory_UserId");
            b.HasIndex(x => x.ClientIpAddress).HasDatabaseName("IX_PriceHistory_IP");
        });

        builder.Entity<PropertyCategoryEntity>(b =>
        {
            b.ToTable("PropertyCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.Icon).HasMaxLength(50);
            b.HasIndex(x => x.Name).IsUnique().HasDatabaseName("IX_Category_Name");
            b.HasIndex(x => x.SortOrder).HasDatabaseName("IX_Category_SortOrder");
            var seedTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            b.HasData(
                new
                {
                    Id = PropertyCatalogIds.Categories.Residential,
                    Name = "Residencial",
                    Description = "Vivienda y uso residencial",
                    SortOrder = 1,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Categories.Commercial,
                    Name = "Comercial",
                    Description = "Comercio y oficinas",
                    SortOrder = 2,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Categories.Industrial,
                    Name = "Industrial",
                    Description = "Industrial y manufactura",
                    SortOrder = 3,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Categories.Land,
                    Name = "Terreno",
                    Description = "Terrenos",
                    SortOrder = 4,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Categories.Mixed,
                    Name = "Uso mixto",
                    Description = "Uso mixto",
                    SortOrder = 5,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Categories.Other,
                    Name = "Otros",
                    Description = "Otros",
                    SortOrder = 99,
                    IsActive = true,
                    Icon = (string?)null,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                });
        });

        builder.Entity<PropertyTypeEntity>(b =>
        {
            b.ToTable("PropertyTypes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasIndex(x => new { x.CategoryId, x.Name }).IsUnique().HasDatabaseName("IX_Type_Category_Name");
            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            var seedTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            b.HasData(
                new
                {
                    Id = PropertyCatalogIds.Types.House,
                    Name = "Casa",
                    CategoryId = PropertyCatalogIds.Categories.Residential,
                    Description = "Casa unifamiliar",
                    SortOrder = 1,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Apartment,
                    Name = "Departamento",
                    CategoryId = PropertyCatalogIds.Categories.Residential,
                    Description = "Departamento o apartamento",
                    SortOrder = 2,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Condo,
                    Name = "Condominio",
                    CategoryId = PropertyCatalogIds.Categories.Residential,
                    Description = "Condominio",
                    SortOrder = 3,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Townhouse,
                    Name = "Casa adosada",
                    CategoryId = PropertyCatalogIds.Categories.Residential,
                    Description = "Casa adosada",
                    SortOrder = 4,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Villa,
                    Name = "Villa",
                    CategoryId = PropertyCatalogIds.Categories.Residential,
                    Description = "Villa",
                    SortOrder = 5,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Office,
                    Name = "Oficina",
                    CategoryId = PropertyCatalogIds.Categories.Commercial,
                    Description = "Oficina",
                    SortOrder = 1,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Warehouse,
                    Name = "Bodega",
                    CategoryId = PropertyCatalogIds.Categories.Commercial,
                    Description = "Bodega o almacen",
                    SortOrder = 2,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.RetailSpace,
                    Name = "Local comercial",
                    CategoryId = PropertyCatalogIds.Categories.Commercial,
                    Description = "Local comercial",
                    SortOrder = 3,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Restaurant,
                    Name = "Restaurante",
                    CategoryId = PropertyCatalogIds.Categories.Commercial,
                    Description = "Restaurante",
                    SortOrder = 4,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Hotel,
                    Name = "Hotel",
                    CategoryId = PropertyCatalogIds.Categories.Commercial,
                    Description = "Hotel",
                    SortOrder = 5,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.MixedUseBuilding,
                    Name = "Edificio de uso mixto",
                    CategoryId = PropertyCatalogIds.Categories.Mixed,
                    Description = "Edificio de uso mixto",
                    SortOrder = 1,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.LiveWorkSpace,
                    Name = "Espacio vive-trabaja",
                    CategoryId = PropertyCatalogIds.Categories.Mixed,
                    Description = "Espacio vive-trabaja",
                    SortOrder = 2,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.ResidentialLand,
                    Name = "Terreno residencial",
                    CategoryId = PropertyCatalogIds.Categories.Land,
                    Description = "Terreno residencial",
                    SortOrder = 1,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.CommercialLand,
                    Name = "Terreno comercial",
                    CategoryId = PropertyCatalogIds.Categories.Land,
                    Description = "Terreno comercial",
                    SortOrder = 2,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.AgriculturalLand,
                    Name = "Terreno agricola",
                    CategoryId = PropertyCatalogIds.Categories.Land,
                    Description = "Terreno agricola",
                    SortOrder = 3,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                },
                new
                {
                    Id = PropertyCatalogIds.Types.Other,
                    Name = "Otro",
                    CategoryId = PropertyCatalogIds.Categories.Other,
                    Description = "Otro",
                    SortOrder = 99,
                    IsActive = true,
                    CreationTime = seedTime,
                    CreatorId = (Guid?)null,
                    LastModificationTime = (DateTime?)null,
                    LastModifierId = (Guid?)null,
                    IsDeleted = false,
                    DeleterId = (Guid?)null,
                    DeletionTime = (DateTime?)null
                });
        });

        builder.Entity<TransactionTypeEntity>(b =>
        {
            b.ToTable("TransactionTypes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_TransactionType_Code");
            b.HasIndex(x => x.SortOrder).HasDatabaseName("IX_TransactionType_SortOrder");
        });
    }
}
