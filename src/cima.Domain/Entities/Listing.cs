using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Entities
{
    public class Listing : AggregateRoot<Guid>
    {
        #region Propiedades de las casas
        // Formulario
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
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
        public virtual Architect Architect { get; set; }
        #endregion
    }
}