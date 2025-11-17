using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    /// <summary>
    /// Solicitud de contacto realizada por un usuario interesado en una propiedad.
    /// Su alcance es por correos y mensajes de WhatsApp.
    /// WhatsApp no se incluirá si mi jefe no me lo pide.
    /// 
    /// 
    /// Nota: esta implementación no tiene Id de usuario debido a que no se soportará chat interno
    /// </summary>
    public class ContactRequest : AggregateRoot<Guid>
    {
        public Guid PropertyId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } // Para responder por correo (medio principal)
        public string Phone { get; set; } // Se tendrá por si se quire llamar directamente
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public string ReplyNotes { get; set; }

        public virtual Property Property { get; set; }
        public virtual Architect Architect { get; set; }
    }

    // Estado de la solicitud de contacto (Nueva, Respondida, Cerrada)
    // Le pongo demás para evitar tener que refactorizar (es mejor prevenir).
    public enum ContactRequestStatus
    {
        New = 0,
        Replied = 1,
        Closed = 2
    }
}
