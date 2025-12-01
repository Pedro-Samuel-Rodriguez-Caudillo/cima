using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Agregado raíz que representa una propiedad inmobiliaria.
    /// Bounded Context: Listings (Gestión de Propiedades)
    /// </summary>
    public class Listing : AggregateRoot<Guid>
    {
        #region Propiedades de las casas
        // Formulario
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? Location { get; set; }  // Nullable - puede estar sin definir
        public decimal Price { get; set; }
        
        /// <summary>
        /// Área total del terreno en m²
        /// </summary>
        public decimal LandArea { get; set; }
        
        /// <summary>
        /// Área construida en m²
        /// </summary>
        public decimal ConstructionArea { get; set; }
        
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
        
        // Normalización de tipos de propiedad
        public PropertyCategory Category { get; set; }
        public PropertyType Type { get; set; }
        
        public TransactionType TransactionType { get; set; }
        public Guid ArchitectId { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }

        // Relaciones
        public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
        public virtual Architect? Architect { get; set; }
        #endregion
    }
}