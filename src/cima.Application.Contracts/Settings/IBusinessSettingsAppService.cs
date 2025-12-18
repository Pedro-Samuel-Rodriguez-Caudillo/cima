using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Settings;

public interface IBusinessSettingsAppService : IApplicationService
{
    Task<BusinessSettingsDto> GetAsync();
    Task UpdateAsync(UpdateBusinessSettingsDto input);
}

public class BusinessSettingsDto
{
    public int MaxImagesPerListing { get; set; }
    public int DraftCleanupDays { get; set; }
    public string? AdminNotificationEmail { get; set; }
    public int MaxFeaturedListings { get; set; }
    public int ListingExpirationDays { get; set; }
}

public class UpdateBusinessSettingsDto
{
    public int MaxImagesPerListing { get; set; }
    public int DraftCleanupDays { get; set; }
    public string? AdminNotificationEmail { get; set; }
    public int MaxFeaturedListings { get; set; }
    public int ListingExpirationDays { get; set; }
}
