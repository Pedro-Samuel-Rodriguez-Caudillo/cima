using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO para propiedades destacadas
    /// </summary>
    public class FeaturedListingDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public DateTime FeaturedSince { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? CreatedBy { get; set; }

        // Información de la propiedad
        public ListingDto Listing { get; set; }
    }

    /// <summary>
    /// DTO para crear una propiedad destacada
    /// </summary>
    public class CreateFeaturedListingDto
    {
        [Required]
        public Guid ListingId { get; set; }

        /// <summary>
        /// Orden de visualización (opcional, por defecto 999)
        /// </summary>
        [Range(0, 9999)]
        public int? DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO para actualizar el orden de propiedades destacadas
    /// </summary>
    public class UpdateFeaturedOrderDto
    {
        [Required]
        public Guid FeaturedListingId { get; set; }

        [Required]
        [Range(0, 9999)]
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO para obtener propiedades destacadas con paginación aleatoria
    /// </summary>
    public class GetFeaturedListingsDto
    {
        /// <summary>
        /// Número de página (para paginación de 12 items)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PageNumber { get; set; } = 0;

        /// <summary>
        /// Tamaño de página (máximo 12 por diseño)
        /// </summary>
        [Range(1, 12)]
        public int PageSize { get; set; } = 6;

        /// <summary>
        /// Si es true, ordena aleatoriamente. Si es false, usa DisplayOrder
        /// </summary>
        public bool RandomOrder { get; set; } = true;
    }
}
