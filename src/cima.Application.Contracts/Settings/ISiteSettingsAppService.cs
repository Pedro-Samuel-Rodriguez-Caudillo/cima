using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Settings;

public interface ISiteSettingsAppService : IApplicationService
{
    Task<SiteSettingsDto> GetAsync();

    Task<PublicSiteSettingsDto> GetPublicAsync();
    
    Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto input);
    
    Task UpdateWhatsAppSettingsAsync(UpdateWhatsAppSettingsDto input);
    
    Task UpdateBusinessInfoAsync(UpdateBusinessInfoDto input);
    
    Task<bool> TestEmailAsync(string email);
}
