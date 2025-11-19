using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Architect : AggregateRoot<Guid>
    {
        // Propiedades de los socios (Arquitectos)
        public Guid UserId { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; } // Enlace al portafolio

        public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>(); // Enlace a sus casas
    }
}