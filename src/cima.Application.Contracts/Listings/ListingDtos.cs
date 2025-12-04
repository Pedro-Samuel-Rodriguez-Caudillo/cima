using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using cima.Architects;
using cima.Domain.Shared;

namespace cima.Listings;

public class ListingDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public decimal LandArea { get; set; }
    public decimal ConstructionArea { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    
    public PropertyCategory Category { get; set; }
    public PropertyType Type { get; set; }
    
    public TransactionType TransactionType { get; set; }
    public Guid ArchitectId { get; set; }
    public ArchitectDto? Architect { get; set; }
    public List<ListingImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    // Propiedades de conveniencia (backward compatibility)
    public string? Address => Location;
    public DateTime CreationTime => CreatedAt;
    public decimal Area => LandArea;
}

public class CreateUpdateListingDto
{
    [Required(ErrorMessage = "El titulo es requerido")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "El titulo debe tener entre 5 y 200 caracteres")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "La descripcion es requerida")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "La descripcion debe tener entre 20 y 5000 caracteres")]
    public required string Description { get; set; }

    [Required(ErrorMessage = "La ubicacion es requerida")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "La ubicacion debe tener entre 5 y 500 caracteres")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "El area del terreno es requerida")]
    [Range(1, 100000, ErrorMessage = "El area del terreno debe estar entre 1 y 100000 m2")]
    public decimal LandArea { get; set; }

    [Required(ErrorMessage = "El area de construccion es requerida")]
    [Range(1, 100000, ErrorMessage = "El area de construccion debe estar entre 1 y 100000 m2")]
    public decimal ConstructionArea { get; set; }

    [Required(ErrorMessage = "El numero de recamaras es requerido")]
    [Range(0, 50, ErrorMessage = "El numero de recamaras debe estar entre 0 y 50")]
    public int Bedrooms { get; set; }

    [Required(ErrorMessage = "El numero de banos es requerido")]
    [Range(0, 50, ErrorMessage = "El numero de banos debe estar entre 0 y 50")]
    public int Bathrooms { get; set; }
    
    [Required(ErrorMessage = "La categoria es requerida")]
    public PropertyCategory Category { get; set; } = PropertyCategory.Residential;

    [Required(ErrorMessage = "El tipo de propiedad es requerido")]
    public PropertyType Type { get; set; } = PropertyType.House;
    
    [Required(ErrorMessage = "El tipo de transaccion es requerido")]
    public TransactionType TransactionType { get; set; } = TransactionType.Sale;

    [Required(ErrorMessage = "El arquitecto es requerido")]
    public Guid ArchitectId { get; set; }
    
    [Obsolete("Use LandArea instead")]
    public decimal? Area { get; set; }
}

public class ListingImageDto
{
    public Guid ImageId { get; set; }
    public string Url { get; set; } = string.Empty;
    public Guid? PreviousImageId { get; set; }
    public Guid? NextImageId { get; set; }
    public string AltText { get; set; } = string.Empty;
}

public class ListingListDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingImageDto? MainImage { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ListingDetailDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    public Guid ArchitectId { get; set; }
    public ArchitectDetailDto? Architect { get; set; }
    public List<ListingImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public Guid? LastModifiedBy { get; set; }
}

public class CreateListingImageDto
{
    public required string Url { get; set; }
    public int DisplayOrder { get; set; }
    public required string AltText { get; set; }
    public long FileSize { get; set; }
    public required string ContentType { get; set; }
}

public class UpdateImageOrderDto
{
    public Guid ImageId { get; set; }
    public int DisplayOrder { get; set; }
}

public class ListingFiltersDto
{
    public string? SearchTerm { get; set; }
    public ListingStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public Guid? ArchitectId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 10;
}

public class PropertySearchDto
{
    public TransactionType? TransactionType { get; set; }
    public PropertyCategory? Category { get; set; }
    public PropertyType? Type { get; set; }

    [MaxLength(200)]
    [RegularExpression(@"^[a-zA-Z0-9·ÈÌÛ˙¡…Õ”⁄Ò—¸‹\s,.\-#∞]*$", 
        ErrorMessage = "La ubicacion contiene caracteres no permitidos")]
    public string? Location { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio minimo debe ser mayor o igual a 0")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio maximo debe ser mayor o igual a 0")]
    public decimal? MaxPrice { get; set; }

    [Range(0, 100, ErrorMessage = "El numero de recamaras debe estar entre 0 y 100")]
    public int? MinBedrooms { get; set; }

    [Range(0, 100, ErrorMessage = "El numero de banos debe estar entre 0 y 100")]
    public int? MinBathrooms { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El area minima debe ser mayor o igual a 0")]
    public decimal? MinArea { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El area maxima debe ser mayor o igual a 0")]
    public decimal? MaxArea { get; set; }

    [MaxLength(50)]
    public string? SortBy { get; set; }

    [Range(0, int.MaxValue)]
    public int PageNumber { get; set; } = 0;

    [Range(1, 100, ErrorMessage = "El tamano de pagina debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 12;
}

public class LocationSuggestionDto
{
    public required string Location { get; set; }
    public int Count { get; set; }
}

public class FeaturedListingDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public DateTime FeaturedSince { get; set; }
    public Guid? CreatedBy { get; set; }
    public ListingDto? Listing { get; set; }
}

public class CreateFeaturedListingDto
{
    [Required]
    public Guid ListingId { get; set; }
}

public class GetFeaturedListingsDto
{
    [Range(0, int.MaxValue)]
    public int PageNumber { get; set; } = 0;

    [Range(1, 12)]
    public int PageSize { get; set; } = 6;
}
