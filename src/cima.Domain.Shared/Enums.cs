namespace cima.Domain.Shared
{
    public enum ListingStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum ContactRequestStatus
    {
        New = 0,
        Replied = 1,
        Closed = 2
    }

    public enum PropertyType
    {
        House = 0,
        Apartment = 1,
        Commercial = 2,
        Land = 3,
        Office = 4
    }

    public enum TransactionType
    {
        Sale = 0,
        Rent = 1,
        Lease = 2
    }
}
