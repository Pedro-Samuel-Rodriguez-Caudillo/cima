using System;
using System.ComponentModel.DataAnnotations;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO para búsqueda de propiedades con filtros avanzados
    /// Incluye validación contra inyecciones SQL y XSS
    /// </summary>
    public class PropertySearchDto
    {
        /// <summary>
        /// Tipo de transacción: Venta, Renta, Arrendamiento
        /// </summary>
        public TransactionType? TransactionType { get; set; }

        /// <summary>
        /// Categoría de la propiedad: Residencial, Comercial, Mixto, Terreno
        /// </summary>
        public PropertyCategory? Category { get; set; }

        /// <summary>
        /// Tipo específico de propiedad
        /// </summary>
        public PropertyType? Type { get; set; }

        /// <summary>
        /// Ubicación (filtrado por autocompletado)
        /// Máximo 200 caracteres, solo alfanuméricos y caracteres especiales seguros
        /// </summary>
        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúñÁÉÍÓÚÑüÜ\s,.\-#°]*$", 
            ErrorMessage = "La ubicación contiene caracteres no permitidos")]
        public string? Location { get; set; }  // ? nullable

        /// <summary>
        /// Precio mínimo
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo debe ser mayor o igual a 0")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Precio máximo
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo debe ser mayor o igual a 0")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Número mínimo de recámaras
        /// </summary>
        [Range(0, 100, ErrorMessage = "El número de recámaras debe estar entre 0 y 100")]
        public int? MinBedrooms { get; set; }

        /// <summary>
        /// Número mínimo de baños
        /// </summary>
        [Range(0, 100, ErrorMessage = "El número de baños debe estar entre 0 y 100")]
        public int? MinBathrooms { get; set; }

        /// <summary>
        /// Área mínima en m²
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El área mínima debe ser mayor o igual a 0")]
        public decimal? MinArea { get; set; }

        /// <summary>
        /// Área máxima en m²
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El área máxima debe ser mayor o igual a 0")]
        public decimal? MaxArea { get; set; }

        /// <summary>
        /// Ordenamiento: newest, price-low, price-high, area-large, area-small
        /// </summary>
        [MaxLength(50)]
        public string? SortBy { get; set; }  // ? nullable

        /// <summary>
        /// Página actual (para paginación)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PageNumber { get; set; } = 0;

        /// <summary>
        /// Cantidad de resultados por página
        /// </summary>
        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
        public int PageSize { get; set; } = 12;
    }

    /// <summary>
    /// DTO para sugerencias de ubicación (autocompletado)
    /// </summary>
    public class LocationSuggestionDto
    {
        public required string Location { get; set; }  // ? required
        public int Count { get; set; }
    }

    // ? GetListingsInput eliminado - usar cima.Listings.GetListingsInput en su lugar
    // Está definido en IListingAppService.cs y hereda de PagedAndSortedResultRequestDto
}
