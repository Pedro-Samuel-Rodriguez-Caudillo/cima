using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Listings.Inputs;

public class PublishListingDto
{
    [Required]
    public Guid ListingId { get; set; }
    
    // Future extension:
    // public DateTime? ScheduledPublishTime { get; set; }
}
