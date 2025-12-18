namespace cima.TextTemplates;

/// <summary>
/// Definiciones de templates de texto para emails y notificaciones.
/// ABP TextTemplating permite usar Razor syntax y localización.
/// </summary>
public static class CimaTextTemplateDefinitions
{
    /// <summary>
    /// Template para notificación de nueva solicitud de contacto al admin
    /// </summary>
    public const string ContactRequestNotification = "Cima.ContactRequestNotification";

    /// <summary>
    /// Template para confirmación de solicitud enviada al cliente
    /// </summary>
    public const string ContactRequestConfirmation = "Cima.ContactRequestConfirmation";

    /// <summary>
    /// Template para notificación de listing publicado
    /// </summary>
    public const string ListingPublishedNotification = "Cima.ListingPublishedNotification";

    /// <summary>
    /// Template para bienvenida de nuevo arquitecto
    /// </summary>
    public const string WelcomeArchitect = "Cima.WelcomeArchitect";
}
