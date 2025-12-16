using System.Threading.Tasks;
using cima.Settings;
using Shouldly;
using Xunit;

namespace cima.ApplicationServices;

/// <summary>
/// Tests for SiteSettingsAppService - Email, WhatsApp, and Business Info settings
/// </summary>
public sealed class SiteSettingsAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly ISiteSettingsAppService _siteSettingsAppService;

    public SiteSettingsAppServiceTests()
    {
        _siteSettingsAppService = GetRequiredService<ISiteSettingsAppService>();
    }

    [Fact]
    public async Task GetAsync_Should_Return_Default_Settings()
    {
        // Act
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.ShouldNotBeNull();
        result.EmailProvider.ShouldNotBeNull();
        result.BusinessName.ShouldBe("4cima"); // Default value
    }

    [Fact]
    public async Task GetPublicAsync_Should_Return_Public_Settings_Without_Sensitive_Data()
    {
        // Act
        var result = await _siteSettingsAppService.GetPublicAsync();

        // Assert
        result.ShouldNotBeNull();
        result.BusinessName.ShouldBe("4cima");
    }

    [Fact]
    public async Task UpdateEmailSettingsAsync_Should_Persist_Admin_Email()
    {
        // Arrange
        var input = new UpdateEmailSettingsDto
        {
            AdminNotificationEmail = "test-admin@4cima.com",
            EmailProvider = EmailProviderNames.AzureCommunicationServices
        };

        // Act
        await _siteSettingsAppService.UpdateEmailSettingsAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.AdminNotificationEmail.ShouldBe("test-admin@4cima.com");
    }

    [Fact]
    public async Task UpdateEmailSettingsAsync_Should_Persist_Brevo_Settings()
    {
        // Arrange
        var input = new UpdateEmailSettingsDto
        {
            AdminNotificationEmail = "admin@4cima.com",
            EmailProvider = EmailProviderNames.Brevo,
            BrevoApiKey = "xkeysib-test-api-key",
            BrevoSenderEmail = "noreply@4cima.com",
            BrevoSenderName = "4cima Test"
        };

        // Act
        await _siteSettingsAppService.UpdateEmailSettingsAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.EmailProvider.ShouldBe(EmailProviderNames.Brevo);
        result.BrevoApiKey.ShouldBe("xkeysib-test-api-key");
        result.BrevoSenderEmail.ShouldBe("noreply@4cima.com");
        result.BrevoSenderName.ShouldBe("4cima Test");
    }

    [Fact]
    public async Task UpdateEmailSettingsAsync_Should_Persist_Smtp_Settings()
    {
        // Arrange
        var input = new UpdateEmailSettingsDto
        {
            AdminNotificationEmail = "admin@4cima.com",
            EmailProvider = EmailProviderNames.Smtp,
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUserName = "user@test.com",
            SmtpPassword = "password123",
            SmtpFromAddress = "noreply@test.com",
            SmtpFromName = "Test Sender",
            SmtpEnableSsl = true
        };

        // Act
        await _siteSettingsAppService.UpdateEmailSettingsAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.EmailProvider.ShouldBe(EmailProviderNames.Smtp);
        result.SmtpHost.ShouldBe("smtp.test.com");
        result.SmtpPort.ShouldBe(587);
        result.SmtpUserName.ShouldBe("user@test.com");
        result.SmtpEnableSsl.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateWhatsAppSettingsAsync_Should_Persist_Settings()
    {
        // Arrange
        var input = new UpdateWhatsAppSettingsDto
        {
            Enabled = true,
            PhoneNumber = "5215512345678",
            DefaultMessage = "Hola, me interesa mas informacion"
        };

        // Act
        await _siteSettingsAppService.UpdateWhatsAppSettingsAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.WhatsAppEnabled.ShouldBeTrue();
        result.WhatsAppNumber.ShouldBe("5215512345678");
        result.WhatsAppDefaultMessage.ShouldBe("Hola, me interesa mas informacion");
        result.IsWhatsAppConfigured.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_Should_Persist_Business_Settings()
    {
        // Arrange
        var input = new UpdateBusinessInfoDto
        {
            BusinessName = "Cima Inmobiliaria",
            ContactEmail = "contacto@cima.com",
            ContactPhone = "+52 55 1234 5678",
            Address = "Col. Polanco, CDMX",
            FacebookUrl = "https://facebook.com/cima",
            InstagramUrl = "https://instagram.com/cima",
            LinkedInUrl = "https://linkedin.com/company/cima",
            BusinessHoursWeekday = "9:00 - 18:00",
            BusinessHoursSaturday = "10:00 - 14:00",
            BusinessHoursSunday = "Cerrado"
        };

        // Act
        await _siteSettingsAppService.UpdateBusinessInfoAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.BusinessName.ShouldBe("Cima Inmobiliaria");
        result.ContactEmail.ShouldBe("contacto@cima.com");
        result.ContactPhone.ShouldBe("+52 55 1234 5678");
        result.Address.ShouldBe("Col. Polanco, CDMX");
        result.FacebookUrl.ShouldBe("https://facebook.com/cima");
        result.InstagramUrl.ShouldBe("https://instagram.com/cima");
        result.LinkedInUrl.ShouldBe("https://linkedin.com/company/cima");
    }

    [Fact]
    public async Task IsEmailConfigured_Should_Be_True_When_Azure_Configured()
    {
        // Arrange
        var input = new UpdateEmailSettingsDto
        {
            AdminNotificationEmail = "admin@4cima.com",
            EmailProvider = EmailProviderNames.AzureCommunicationServices,
            AzureEmailConnectionString = "endpoint=https://test.communication.azure.com/;accesskey=xxxx",
            AzureEmailSenderAddress = "noreply@test.azurecomm.net"
        };

        // Act
        await _siteSettingsAppService.UpdateEmailSettingsAsync(input);
        var result = await _siteSettingsAppService.GetAsync();

        // Assert
        result.IsEmailConfigured.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPublicAsync_Should_Return_WhatsApp_Url_When_Enabled()
    {
        // Arrange
        await _siteSettingsAppService.UpdateWhatsAppSettingsAsync(new UpdateWhatsAppSettingsDto
        {
            Enabled = true,
            PhoneNumber = "5215512345678",
            DefaultMessage = "Hola"
        });

        // Act
        var result = await _siteSettingsAppService.GetPublicAsync();

        // Assert
        result.WhatsAppEnabled.ShouldBeTrue();
        result.WhatsAppUrl.ShouldNotBeNull();
        result.WhatsAppUrl.ShouldContain("wa.me/5215512345678");
    }
}
