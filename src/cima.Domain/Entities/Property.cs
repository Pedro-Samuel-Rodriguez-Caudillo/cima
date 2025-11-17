using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Property : AggregateRoot<Guid>
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
        public PropertyStatus Status { get; set; }
        public Guid ArchitectId { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }

        // Relaciones
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public virtual Architect Architect { get; set; }
        #endregion
    }

    public enum PropertyStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}