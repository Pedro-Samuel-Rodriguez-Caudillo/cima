using System;
using cima.Domain.Shared;

namespace cima.ContactRequests;

public class ContactRequestDto
{
    public Guid Id { get; set; }
    public Guid? ListingId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Message { get; set; }
    public Guid? ArchitectId { get; set; }
    public ContactRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReplyNotes { get; set; }
    public DateTime? RepliedAt { get; set; }
}

public class CreateContactRequestDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Message { get; set; }
    public Guid ListingId { get; set; }
}

/// <summary>
/// DTO para crear solicitud de contacto general (sin propiedad específica)
/// </summary>
public class CreateGeneralContactRequestDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Message { get; set; }
}

public class MarkAsRepliedDto
{
    public required string ReplyNotes { get; set; }
}

public class ContactRequestListDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Message { get; set; }
    public ContactRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReplyNotes { get; set; }
}
