using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cima.Architects;

public class ArchitectDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
}

/// <summary>
/// DTO para crear un nuevo arquitecto (usuario + perfil)
/// </summary>
public class CreateArchitectWithUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña temporal (si no se proporciona, se genera automáticamente)
    /// </summary>
    [StringLength(128, MinimumLength = 6)]
    public string? TemporaryPassword { get; set; }
}

/// <summary>
/// DTO legacy - para crear perfil a usuario existente
/// </summary>
public class CreateArchitectDto
{
    // Sin campos - se crea vinculado al usuario actual
}

public class UpdateArchitectDto
{
    public bool? IsActive { get; set; }
    
    [StringLength(64)]
    public string? Name { get; set; }
    
    [StringLength(64)]
    public string? Surname { get; set; }
}

/// <summary>
/// DTO para restablecer contraseña de arquitecto
/// </summary>
public class ResetArchitectPasswordDto
{
    /// <summary>
    /// Nueva contraseña temporal (si no se proporciona, se genera automáticamente)
    /// </summary>
    [StringLength(128, MinimumLength = 6)]
    public string? NewTemporaryPassword { get; set; }
}

/// <summary>
/// DTO para cambiar contraseña propia
/// </summary>
public class ChangeArchitectPasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de crear arquitecto con contraseña temporal
/// </summary>
public class CreateArchitectResultDto
{
    public ArchitectDto Architect { get; set; } = null!;
    public string TemporaryPassword { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ArchitectDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public List<ArchitectListingDto> Listings { get; set; } = new();
}

public class ArchitectListDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public int TotalListingsPublished { get; set; }
    public int ActiveListings { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO simplificado para listings dentro de ArchitectDetailDto
/// Evita referencia circular con ListingDto
/// </summary>
public class ArchitectListingDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
