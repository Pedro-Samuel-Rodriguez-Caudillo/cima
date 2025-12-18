namespace cima.Settings;

/// <summary>
/// Configuraciones para páginas legales (Privacy, Terms)
/// </summary>
public static class cimaLegalSettings
{
    private const string Prefix = "Cima.Legal";
    
    /// <summary>
    /// Contenido Markdown de la página de Privacidad
    /// </summary>
    public const string PrivacyContent = Prefix + ".PrivacyContent";
    
    /// <summary>
    /// Contenido Markdown de la página de Términos y Condiciones
    /// </summary>
    public const string TermsContent = Prefix + ".TermsContent";
    
    /// <summary>
    /// Fecha de última actualización de Privacy
    /// </summary>
    public const string PrivacyLastUpdated = Prefix + ".PrivacyLastUpdated";
    
    /// <summary>
    /// Fecha de última actualización de Terms
    /// </summary>
    public const string TermsLastUpdated = Prefix + ".TermsLastUpdated";
}
