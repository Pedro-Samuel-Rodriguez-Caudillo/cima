namespace cima.Domain.Shared
{
    public enum ListingStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2,      // Propiedades ya no públicas (vendidas/rentadas finalizadas)
        Portfolio = 3      // Propiedades en portafolio (públicas pero ya vendidas/rentadas como showcase)
    }

    public enum ContactRequestStatus
    {
        New = 0,
        Replied = 1,
        Closed = 2
    }

    /// <summary>
    /// Categoría principal de la propiedad según uso de suelo
    /// </summary>
    public enum PropertyCategory
    {
        Residential = 0,   // Uso residencial
        Commercial = 1,    // Uso comercial
        Mixed = 2,         // Uso mixto (residencial + comercial)
        Land = 3          // Terreno sin construcción
    }

    /// <summary>
    /// Tipo específico de propiedad dentro de su categoría
    /// </summary>
    public enum PropertyType
    {
        // Residenciales
        House = 0,
        Apartment = 1,
        Condo = 2,
        Townhouse = 3,
        Villa = 4,
        
        // Comerciales
        Office = 10,
        Warehouse = 11,
        RetailSpace = 12,
        Restaurant = 13,
        Hotel = 14,
        
        // Mixtos
        MixedUseBuilding = 20,
        LiveWorkSpace = 21,
        
        // Terrenos
        ResidentialLand = 30,
        CommercialLand = 31,
        AgriculturalLand = 32
    }

    public enum TransactionType
    {
        Sale = 0,
        Rent = 1,
        Lease = 2
    }
}
