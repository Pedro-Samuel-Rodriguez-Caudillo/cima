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
        public string Location { get; set; }

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
        public string SortBy { get; set; } = "newest";

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
    /// DTO simplificado para búsqueda rápida en el hero
    /// </summary>
    public class QuickSearchDto
    {
        public TransactionType TransactionType { get; set; }
        
        public PropertyCategory? Category { get; set; }

        [MaxLength(200)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúñÁÉÍÓÚÑüÜ\s,.\-#°]*$")]
        public string Location { get; set; }
    }

    /// <summary>
    /// DTO para sugerencias de autocompletado de ubicaciones
    /// </summary>
    public class LocationSuggestionDto
    {
        public string Location { get; set; }
        public int Count { get; set; } // Cantidad de propiedades en esa ubicación
    }
}
