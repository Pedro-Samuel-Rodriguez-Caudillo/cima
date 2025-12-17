using System;

namespace cima.Listings.Outputs;

public class AdminListingDetailDto : ListingDetailDto
{
    public string? InternalNotes { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
}
