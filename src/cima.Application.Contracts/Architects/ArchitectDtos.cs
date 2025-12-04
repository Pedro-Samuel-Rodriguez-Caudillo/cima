using System;
using System.Collections.Generic;

namespace cima.Architects;

public class ArchitectDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateArchitectDto
{
    // Sin campos - se crea vinculado al usuario actual
}

public class UpdateArchitectDto
{
    public bool? IsActive { get; set; }
}

public class ArchitectDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public List<ArchitectListingDto> Listings { get; set; } = new();
}

public class ArchitectListDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO simplificado para listings dentro de ArchitectDetailDto
/// Evita referencia circular con ListingDto
/// </summary>
public class ArchitectListingDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
