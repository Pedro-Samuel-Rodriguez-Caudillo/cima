using System;

namespace cima.Blazor.Client.Navigation;

/// <summary>
/// Rutas públicas del sitio web
/// </summary>
public static class PublicRoutes
{
    public const string Home = "/";
    
    // Propiedades
    public const string Properties = "/propiedades";
    public const string PropertyDetail = "/propiedades/{id}";
    
    // Contacto
    public const string Contact = "/contacto";
    
    // Ejemplos (desarrollo)
    public const string Examples = "/examples";
    public const string ListingCardPreview = "/examples/listing-card";
    public const string TailwindExample = "/examples/tailwind";
    
    /// <summary>
    /// Genera la ruta del detalle de una propiedad
    /// </summary>
    public static string GetPropertyDetailRoute(Guid id) => $"/propiedades/{id}";
}

/// <summary>
/// Rutas del área de administración
/// </summary>
public static class AdminRoutes
{
    public const string Dashboard = "/admin";
    
    // Listados
    public const string Listings = "/admin/listings";
    public const string ListingsCreate = "/admin/listings/create";
    public const string ListingsEdit = "/admin/listings/edit/{id}";
    
    // Arquitectos
    public const string Architects = "/admin/architects";
    
    // Solicitudes de contacto
    public const string ContactRequests = "/admin/contact-requests";
    
    /// <summary>
    /// Genera la ruta de edición de un listado
    /// </summary>
    public static string GetListingEditRoute(Guid id) => $"/admin/listings/edit/{id}";
}
