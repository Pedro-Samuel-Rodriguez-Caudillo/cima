namespace cima.Domain.Shared
{
    public enum ListingStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2,
        Portfolio = 3
    }

    public enum PropertyCategory
    {
        Residential = 0,
        Commercial = 1,
        Industrial = 2,
        Land = 3,
        Mixed = 4
    }

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

    public enum ContactRequestStatus
    {
        New = 0,
        Read = 1,
        Replied = 2,
        Closed = 3
    }
}
