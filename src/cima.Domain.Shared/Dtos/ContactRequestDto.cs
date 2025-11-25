using System;
using System.ComponentModel.DataAnnotations;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string ReplyNotes { get; set; }
    }

    public class CreateContactRequestDto
    {
        [Required(ErrorMessage = "El ID de la propiedad es requerido")]
        public Guid ListingId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 5000 caracteres")]
        public string Message { get; set; }
    }

    public class UpdateContactRequestDto
    {
        public ContactRequestStatus Status { get; set; }
        public string ReplyNotes { get; set; }
    }

    public class MarkAsRepliedDto
    {
        [StringLength(2000, ErrorMessage = "Las notas de respuesta no pueden exceder 2000 caracteres")]
        public string ReplyNotes { get; set; }
    }
}
