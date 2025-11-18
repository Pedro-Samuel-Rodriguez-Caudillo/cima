using System;
using System.Collections.Generic;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO para filtrar y paginar propiedades
    /// Usado en GetListAsync de PropertyAppService
    /// </summary>
    public class PropertyFiltersDto
    {
        // Busqueda de texto
        public string SearchTerm { get; set; }

        // Filtros de estado
        public PropertyStatus? Status { get; set; }

        // Filtros de precio
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Filtros de habitaciones
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }

        // Filtro por arquitecto
        public Guid? ArchitectId { get; set; }

        // Ordenamiento
        public string SortBy { get; set; }  // "price", "area", "createdat"
        public bool SortDescending { get; set; } = false;

        // Paginacion
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }

    /// <summary>
    /// DTO para filtros basicos (backward compatibility)
    /// </summary>
    public class PropertyFilterDto
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaMin { get; set; }
        public decimal? AreaMax { get; set; }
        public PropertyStatus? Status { get; set; }
        public Guid? ArchitectId { get; set; }
        public int SkipCount { get; set; } = 0;
        public int MaxResultCount { get; set; } = 10;
    }

    public class PropertyDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public PropertyStatus Status { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDetailDto Architect { get; set; }
        public List<PropertyImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }

    public class PropertyListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public PropertyImageDto MainImage { get; set; }
        public PropertyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
