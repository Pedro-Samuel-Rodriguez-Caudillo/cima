using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO para propiedades destacadas (sin DisplayOrder - orden aleatorio)
    /// </summary>
    public class FeaturedListingDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public DateTime FeaturedSince { get; set; }
        public Guid? CreatedBy { get; set; }

        // Información de la propiedad
        public ListingDto? Listing { get; set; }
    }

    /// <summary>
    /// DTO para crear una propiedad destacada
    /// </summary>
    public class CreateFeaturedListingDto
    {
        [Required]
        public Guid ListingId { get; set; }
    }

    /// <summary>
    /// DTO para obtener propiedades destacadas (siempre aleatorio)
    /// </summary>
    public class GetFeaturedListingsDto
    {
        /// <summary>
        /// Número de página (para paginación)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PageNumber { get; set; } = 0;

        /// <summary>
        /// Tamaño de página (máximo 12 por diseño)
        /// </summary>
        [Range(1, 12)]
        public int PageSize { get; set; } = 6;
    }
}
