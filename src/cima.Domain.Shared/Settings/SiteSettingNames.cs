namespace cima.Settings;

/// <summary>
/// Nombres de settings para el sitio (usados con ISettingProvider de ABP)
/// </summary>
public static class SiteSettingNames
{
    private const string Prefix = "cima.Site";
    
    // Email
    public const string AdminNotificationEmail = Prefix + ".AdminNotificationEmail";
    public const string AzureEmailConnectionString = Prefix + ".AzureEmailConnectionString";
    public const string AzureEmailSenderAddress = Prefix + ".AzureEmailSenderAddress";
    
    // WhatsApp
    public const string WhatsAppEnabled = Prefix + ".WhatsAppEnabled";
    public const string WhatsAppNumber = Prefix + ".WhatsAppNumber";
    public const string WhatsAppDefaultMessage = Prefix + ".WhatsAppDefaultMessage";
    
    // Business Info
    public const string BusinessName = Prefix + ".BusinessName";
    public const string ContactPhone = Prefix + ".ContactPhone";
    public const string ContactEmail = Prefix + ".ContactEmail";
    public const string Address = Prefix + ".Address";
    
    // Social Media
    public const string FacebookUrl = Prefix + ".FacebookUrl";
    public const string InstagramUrl = Prefix + ".InstagramUrl";
    public const string LinkedInUrl = Prefix + ".LinkedInUrl";
    
    // Business Hours
    public const string BusinessHoursWeekday = Prefix + ".BusinessHoursWeekday";
    public const string BusinessHoursSaturday = Prefix + ".BusinessHoursSaturday";
    public const string BusinessHoursSunday = Prefix + ".BusinessHoursSunday";
}
