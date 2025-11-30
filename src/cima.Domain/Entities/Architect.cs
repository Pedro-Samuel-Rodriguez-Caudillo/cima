using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Entidad que representa a un arquitecto/socio de CIMA.
    /// Cada arquitecto tiene su perfil y portafolio interno de proyectos.
    /// </summary>
    public class Architect : AggregateRoot<Guid>
    {
        /// <summary>
        /// ID del usuario de Identity vinculado a este arquitecto
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre completo del arquitecto
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Biografía del arquitecto (opcional)
        /// Puede llenarse gradualmente durante onboarding
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// Colección de proyectos/propiedades del arquitecto
        /// Esto funciona como su portafolio interno
        /// </summary>
        public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}