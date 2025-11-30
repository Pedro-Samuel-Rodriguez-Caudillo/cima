using System;
using cima.Domain.Shared;

namespace cima.Domain.Shared.Dtos
{
    /// <summary>
    /// DTO completo de solicitud de contacto (con todos los campos)
    /// </summary>
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public required string Name { get; set; }  // ? required
        public required string Email { get; set; }  // ? required
        public string? Phone { get; set; }  // ? nullable
        public required string Message { get; set; }  // ? required
        public Guid ArchitectId { get; set; }
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReplyNotes { get; set; }  // ? nullable
        public DateTime? RepliedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear solicitud de contacto (público)
    /// </summary>
    public class CreateContactRequestDto
    {
        public required string Name { get; set; }  // ? required
        public required string Email { get; set; }  // ? required
        public string? Phone { get; set; }  // ? nullable
        public required string Message { get; set; }  // ? required
        public Guid ListingId { get; set; }
    }

    /// <summary>
    /// DTO para marcar como respondida
    /// </summary>
    public class MarkAsRepliedDto
    {
        public required string ReplyNotes { get; set; }  // ? required
    }

    /// <summary>
    /// DTO simplificado para listados
    /// </summary>
    public class ContactRequestListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }  // ? required
        public required string Email { get; set; }  // ? required
        public string? Phone { get; set; }  // ? nullable
        public required string Message { get; set; }  // ? required
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReplyNotes { get; set; }  // ? nullable
    }
}
