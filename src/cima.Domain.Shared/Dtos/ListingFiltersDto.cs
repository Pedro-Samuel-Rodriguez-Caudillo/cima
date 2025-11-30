using System;
using System.Collections.Generic;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO para filtrar y paginar propiedades
    /// Usado en GetListAsync de ListingAppService
    /// </summary>
    public class ListingFiltersDto
    {
        // Busqueda de texto
        public string? SearchTerm { get; set; }  // ? nullable

        // Filtros de estado
        public ListingStatus? Status { get; set; }

        // Filtros de precio
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Filtros de habitaciones
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }

        // Filtro por arquitecto
        public Guid? ArchitectId { get; set; }

        // Ordenamiento
        public string? SortBy { get; set; }  // ? nullable - "price", "area", "createdat"
        public bool SortDescending { get; set; } = false;

        // Paginacion
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }

    /// <summary>
    /// DTO para filtros basicos (backward compatibility)
    /// </summary>
    public class ListingFilterDto
    {
        public string? Title { get; set; }  // ? nullable
        public string? Location { get; set; }  // ? nullable
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaMin { get; set; }
        public decimal? AreaMax { get; set; }
        public ListingStatus? Status { get; set; }
        public Guid? ArchitectId { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }

    public class ListingDetailDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }  // ? required
        public required string Description { get; set; }  // ? required
        public string? Location { get; set; }  // ? nullable
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDetailDto? Architect { get; set; }  // ? nullable
        public List<ListingImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }

    public class ListingListDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }  // ? required
        public string? Location { get; set; }  // ? nullable
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingImageDto? MainImage { get; set; }  // ? nullable
        public ListingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
