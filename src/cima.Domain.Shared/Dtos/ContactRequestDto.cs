using System;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReplyNotes { get; set; }
    }

    public class CreateContactRequestDto
    {
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }

    public class UpdateContactRequestDto
    {
        public ContactRequestStatus Status { get; set; }
        public string ReplyNotes { get; set; }
    }
}
