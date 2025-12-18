using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;

namespace cima.Settings;

[Authorize(Roles = "admin")]
public class BusinessSettingsAppService : ApplicationService, IBusinessSettingsAppService
{
    private readonly ISettingManager _settingManager;

    public BusinessSettingsAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<BusinessSettingsDto> GetAsync()
    {
        return new BusinessSettingsDto
        {
            MaxImagesPerListing = int.Parse(
                await _settingManager.GetOrNullGlobalAsync(cimaBusinessSettings.MaxImagesPerListing) ?? "12"),
            DraftCleanupDays = int.Parse(
                await _settingManager.GetOrNullGlobalAsync(cimaBusinessSettings.DraftCleanupDays) ?? "30"),
            AdminNotificationEmail = 
                await _settingManager.GetOrNullGlobalAsync(cimaBusinessSettings.AdminNotificationEmail),
            MaxFeaturedListings = int.Parse(
                await _settingManager.GetOrNullGlobalAsync(cimaBusinessSettings.MaxFeaturedListings) ?? "6"),
            ListingExpirationDays = int.Parse(
                await _settingManager.GetOrNullGlobalAsync(cimaBusinessSettings.ListingExpirationDays) ?? "0")
        };
    }

    public async Task UpdateAsync(UpdateBusinessSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(
            cimaBusinessSettings.MaxImagesPerListing, 
            input.MaxImagesPerListing.ToString());
        
        await _settingManager.SetGlobalAsync(
            cimaBusinessSettings.DraftCleanupDays, 
            input.DraftCleanupDays.ToString());
        
        await _settingManager.SetGlobalAsync(
            cimaBusinessSettings.AdminNotificationEmail, 
            input.AdminNotificationEmail ?? "");
        
        await _settingManager.SetGlobalAsync(
            cimaBusinessSettings.MaxFeaturedListings, 
            input.MaxFeaturedListings.ToString());
        
        await _settingManager.SetGlobalAsync(
            cimaBusinessSettings.ListingExpirationDays, 
            input.ListingExpirationDays.ToString());
    }
}
