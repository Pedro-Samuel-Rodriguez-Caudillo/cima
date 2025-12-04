namespace cima;

/// <summary>
/// Códigos de error del dominio para excepciones de negocio.
/// Siguiendo la convención de ABP: "Namespace:ErrorCode"
/// </summary>
public static class cimaDomainErrorCodes
{
    /* Listing Errors */
    public const string ListingTitleRequired = "Cima:Listing:TitleRequired";
    public const string ListingTitleTooLong = "Cima:Listing:TitleTooLong";
    public const string ListingDescriptionRequired = "Cima:Listing:DescriptionRequired";
    public const string ListingInvalidPrice = "Cima:Listing:InvalidPrice";
    public const string ListingInvalidLandArea = "Cima:Listing:InvalidLandArea";
    public const string ListingInvalidConstructionArea = "Cima:Listing:InvalidConstructionArea";
    public const string ConstructionAreaExceedsLandArea = "Cima:Listing:ConstructionAreaExceedsLandArea";
    public const string InvalidStatusTransition = "Cima:Listing:InvalidStatusTransition";
    public const string ListingNotArchived = "Cima:Listing:NotArchived";
    public const string ListingNotPublished = "Cima:Listing:NotPublished";
    public const string ListingAlreadyPublished = "Cima:Listing:AlreadyPublished";

    /* Architect Errors */
    public const string ArchitectInactive = "Cima:Architect:Inactive";
    public const string ArchitectNotFound = "Cima:Architect:NotFound";

    /* FeaturedListing Errors */
    public const string FeaturedListingMaxLimitReached = "Cima:FeaturedListing:MaxLimitReached";
    public const string FeaturedListingAlreadyFeatured = "Cima:FeaturedListing:AlreadyFeatured";
    public const string FeaturedListingOnlyPublished = "Cima:FeaturedListing:OnlyPublishedCanBeFeatured";

    /* ContactRequest Errors */
    public const string ContactRequestListingNotPublished = "Cima:ContactRequest:ListingNotPublished";
    public const string ContactRequestAlreadyReplied = "Cima:ContactRequest:AlreadyReplied";
}
