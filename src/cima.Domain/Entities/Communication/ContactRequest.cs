using System;
using Volo.Abp.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Agregado raíz que representa una solicitud de contacto.
    /// Bounded Context: Customer Interaction (Interacción con clientes)
    /// 
    /// Nota: Todas las peticiones se envían a un número y correo administrativo
    /// configurado en los ajustes del sistema. No se contacta directamente al arquitecto.
    /// </summary>
    public class ContactRequest : AggregateRoot<Guid>
    {
        #region Identificación de la Solicitud
        /// <summary>
        /// ID de la propiedad por la que pregunta (null para contacto general)
        /// </summary>
        public Guid? ListingId { get; set; }
        
        /// <summary>
        /// ID del arquitecto que publicó (solo para tracking interno, null para contacto general)
        /// </summary>
        public Guid? ArchitectId { get; set; }
        #endregion

        #region Datos del Solicitante
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public required string Message { get; set; }
        #endregion

        #region Estado y Seguimiento
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string? ReplyNotes { get; set; }
        #endregion

        #region Relaciones (Navigation Properties)
        public virtual Listing? Listing { get; set; }
        public virtual Architect? Architect { get; set; }
        #endregion
    }
}
