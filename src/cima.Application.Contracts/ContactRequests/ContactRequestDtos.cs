using System;
using System.ComponentModel.DataAnnotations;
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
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(2000)]
    public required string Message { get; set; }

    public Guid ListingId { get; set; }
}

/// <summary>
/// DTO para crear solicitud de contacto general (sin propiedad específica)
/// </summary>
public class CreateGeneralContactRequestDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public required string Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(2000)]
    public required string Message { get; set; }
}

public class MarkAsRepliedDto
{
    [Required]
    [StringLength(2000)]
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
