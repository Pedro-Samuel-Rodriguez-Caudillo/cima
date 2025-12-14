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

        builder.Entity<Listing>(b =>
        {
            b.ToTable("Listings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(5000);
            b.Property(x => x.Location).HasMaxLength(500);
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.Property(x => x.LandArea).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.ConstructionArea).HasPrecision(10, 2).IsRequired();
            
            // �ndices para b�squeda optimizada
            b.HasIndex(x => x.Status).HasDatabaseName("IX_Listings_Status");
            b.HasIndex(x => x.Location).HasDatabaseName("IX_Listings_Location");
            b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Listings_CreatedAt");
            b.HasIndex(x => x.Price).HasDatabaseName("IX_Listings_Price");
            b.HasIndex(x => x.ArchitectId).HasDatabaseName("IX_Listings_ArchitectId");
            
            // �ndices compuestos para consultas frecuentes
            b.HasIndex(x => new { x.Status, x.ArchitectId }).HasDatabaseName("IX_Listings_Status_ArchitectId");
            b.HasIndex(x => new { x.Status, x.Location }).HasDatabaseName("IX_Listings_Status_Location");
            b.HasIndex(x => new { x.Status, x.Type, x.TransactionType }).HasDatabaseName("IX_Listings_Status_Type_Transaction");
            
            b.HasOne(x => x.Architect)
                .WithMany(a => a.Listings)
                .HasForeignKey(x => x.ArchitectId)
                .OnDelete(DeleteBehavior.Restrict);
            b.OwnsMany(x => x.Images, ib =>
            {
                ib.ToTable("ListingImages");
                ib.WithOwner().HasForeignKey("ListingId");
                ib.HasKey("ListingId", "ImageId");
                
                // Configuracion explicita de las propiedades del ValueObject
                // ValueGeneratedNever() indica que ImageId se genera en el cliente, no en la BD
                ib.Property(i => i.ImageId).IsRequired().ValueGeneratedNever();
                ib.Property(i => i.Url).IsRequired().HasMaxLength(2048);
                ib.Property(i => i.ThumbnailUrl).HasMaxLength(2048).HasDefaultValue("");
                ib.Property(i => i.AltText).HasMaxLength(500);
                ib.Property(i => i.FileSize).IsRequired();
                ib.Property(i => i.ContentType).IsRequired().HasMaxLength(100);
                ib.Property(i => i.PreviousImageId);
                ib.Property(i => i.NextImageId);
            });
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
    }
}
