using System.ComponentModel.DataAnnotations;

namespace cima.Listings.Inputs;

public class AddressDto
{
    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;
    
    // Future expansion:
    // public string? Street { get; set; }
    // public string? City { get; set; }
}
