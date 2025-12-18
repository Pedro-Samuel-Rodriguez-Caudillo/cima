namespace cima.Settings;

public static class cimaBusinessSettings
{
    private const string Prefix = "Cima.Business";
    
    /// <summary>
    /// Número máximo de imágenes por listing
    /// </summary>
    public const string MaxImagesPerListing = Prefix + ".MaxImagesPerListing";
    
    /// <summary>
    /// Días antes de archivar borradores automáticamente
    /// </summary>
    public const string DraftCleanupDays = Prefix + ".DraftCleanupDays";
    
    /// <summary>
    /// Email para notificaciones de admin
    /// </summary>
    public const string AdminNotificationEmail = Prefix + ".AdminNotificationEmail";
    
    /// <summary>
    /// Número máximo de listings destacados
    /// </summary>
    public const string MaxFeaturedListings = Prefix + ".MaxFeaturedListings";
    
    /// <summary>
    /// Días para expirar listings publicados automáticamente (0 = nunca)
    /// </summary>
    public const string ListingExpirationDays = Prefix + ".ListingExpirationDays";
}
