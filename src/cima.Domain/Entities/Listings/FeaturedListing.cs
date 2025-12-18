using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Entidad para gestionar propiedades destacadas en la página principal.
    /// Las propiedades se muestran en orden aleatorio.
    /// Límite máximo: 12 propiedades destacadas.
    /// Implementa ISoftDelete para evitar advertencias de EF Core y manejar datos huerfanos.
    /// </summary>
    public class FeaturedListing : Entity<Guid>, ISoftDelete
    {
        /// <summary>
        /// ID de la propiedad destacada
        /// </summary>
        public Guid ListingId { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>
        /// Fecha desde que fue marcada como destacada
        /// </summary>
        public DateTime FeaturedSince { get; set; }

        /// <summary>
        /// Usuario que marcó la propiedad como destacada
        /// </summary>
        public Guid? CreatedBy { get; set; }

        // Navegación
        public virtual Listing? Listing { get; set; }

        public FeaturedListing()
        {
            FeaturedSince = DateTime.UtcNow;
        }

        public FeaturedListing(Guid listingId, Guid? createdBy = null)
        {
            Id = Guid.NewGuid();
            ListingId = listingId;
            FeaturedSince = DateTime.UtcNow;
            CreatedBy = createdBy;
        }
    }
}
