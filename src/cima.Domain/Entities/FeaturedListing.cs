using System;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Entidad para gestionar propiedades destacadas en la página principal.
    /// Límite máximo: 12 propiedades destacadas con orden aleatorio.
    /// </summary>
    public class FeaturedListing : Entity<Guid>
    {
        /// <summary>
        /// ID de la propiedad destacada
        /// </summary>
        public Guid ListingId { get; set; }

        /// <summary>
        /// Fecha desde que fue marcada como destacada
        /// </summary>
        public DateTime FeaturedSince { get; set; }

        /// <summary>
        /// Orden manual (opcional, si el admin quiere ordenar)
        /// Valores bajos = mayor prioridad
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Usuario que marcó la propiedad como destacada
        /// </summary>
        public Guid? CreatedBy { get; set; }

        // Navegación
        public virtual Listing Listing { get; set; }

        public FeaturedListing()
        {
            FeaturedSince = DateTime.UtcNow;
            DisplayOrder = 999; // Por defecto al final
        }

        public FeaturedListing(Guid listingId, int? displayOrder = null, Guid? createdBy = null)
        {
            Id = Guid.NewGuid();
            ListingId = listingId;
            FeaturedSince = DateTime.UtcNow;
            DisplayOrder = displayOrder ?? 999;
            CreatedBy = createdBy;
        }
    }
}
