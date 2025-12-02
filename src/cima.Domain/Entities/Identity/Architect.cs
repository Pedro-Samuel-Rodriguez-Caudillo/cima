using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Entidad que representa a un arquitecto/socio de CIMA.
    /// Bounded Context: Identity (Gestión de Usuarios)
    /// 
    /// Vinculado a IdentityUser de ABP para autenticación.
    /// Contiene solo datos específicos del dominio de arquitectos y estadísticas internas.
    /// </summary>
    public class Architect : AggregateRoot<Guid>
    {
        #region Vinculación con Identity
        /// <summary>
        /// ID del usuario de Identity vinculado a este arquitecto
        /// </summary>
        public Guid UserId { get; set; }
        #endregion

        #region Estadísticas
        /// <summary>
        /// Total de propiedades publicadas por el arquitecto
        /// Se actualiza cuando se publica una propiedad
        /// </summary>
        public int TotalListingsPublished { get; set; }

        /// <summary>
        /// Número de propiedades activas (Published o Portfolio)
        /// Se actualiza cuando cambia el estado de una propiedad
        /// </summary>
        public int ActiveListings { get; set; }
        #endregion

        #region Metadata
        /// <summary>
        /// Fecha de registro como arquitecto en el sistema
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Indica si el arquitecto está activo en el sistema
        /// Un admin puede desactivar a un arquitecto
        /// </summary>
        public bool IsActive { get; set; }
        #endregion

        #region Relaciones (Navigation Properties)
        /// <summary>
        /// Colección de proyectos/propiedades del arquitecto
        /// Esto funciona como su portafolio interno
        /// </summary>
        public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
        #endregion
    }
}