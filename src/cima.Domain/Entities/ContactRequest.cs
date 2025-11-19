using System;
using Volo.Abp.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Solicitud de contacto realizada por un usuario interesado en una propiedad.
    /// Su alcance es por correos y mensajes de WhatsApp.
    /// WhatsApp no se incluira si mi jefe no me lo pide.
    /// 
    /// Nota: esta implementacion no tiene Id de usuario debido a que no se soportara chat interno
    /// </summary>
    public class ContactRequest : AggregateRoot<Guid>
    {
        public Guid ListingId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } // Para responder por correo (medio principal)
        public string Phone { get; set; } // Se tendra por si se quiere llamar directamente
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public string ReplyNotes { get; set; }

        public virtual Listing Listing { get; set; }
        public virtual Architect Architect { get; set; }
    }
}
