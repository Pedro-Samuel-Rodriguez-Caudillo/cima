using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Listings;
using cima.Notifications;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using Volo.Abp.Guids;

namespace cima.Architects;

/// <summary>
/// Servicio de aplicación para gestionar perfiles de arquitectos
/// </summary>
public class ArchitectAppService : ApplicationService, IArchitectAppService
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IEmailNotificationService _emailService;
    private readonly IConfiguration _configuration;

    // Claim personalizado para marcar que debe cambiar contraseña
    private const string MustChangePasswordClaim = "MustChangePassword";

    public ArchitectAppService(
        IRepository<Architect, Guid> architectRepository,
        IRepository<Listing, Guid> listingRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IGuidGenerator guidGenerator,
        IEmailNotificationService emailService,
        IConfiguration configuration)
    {
        _architectRepository = architectRepository;
        _listingRepository = listingRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _guidGenerator = guidGenerator;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene lista paginada de arquitectos (acceso público)
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ArchitectDto>> GetListAsync(
        PagedAndSortedResultRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var queryable = await _architectRepository.GetQueryableAsync();

        queryable = string.IsNullOrWhiteSpace(input.Sorting)
            ? queryable.OrderByDescending(a => a.RegistrationDate)
            : queryable.OrderBy(input.Sorting);

        var totalCount = await AsyncExecuter.CountAsync(queryable, cancellationToken);

        var architects = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount),
            cancellationToken
        );

        var architectDtos = ObjectMapper.Map<List<Architect>, List<ArchitectDto>>(architects);

        if (architectDtos.Any())
        {
            await LoadUserDetailsAsync(architectDtos, cancellationToken);
        }

        return new PagedResultDto<ArchitectDto>(totalCount, architectDtos);
    }

    /// <summary>
    /// Carga detalles del usuario para cada arquitecto
    /// </summary>
    private async Task LoadUserDetailsAsync(List<ArchitectDto> architectDtos, CancellationToken cancellationToken)
    {
        try
        {
            var userIds = architectDtos.Select(x => x.UserId).Distinct().ToList();
            var users = await _userRepository.GetListAsync(
                sorting: null,
                maxResultCount: userIds.Count,
                skipCount: 0,
                filter: null,
                includeDetails: false,
                cancellationToken: cancellationToken
            );

            var userDict = users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id);

            foreach (var dto in architectDtos)
            {
                if (userDict.TryGetValue(dto.UserId, out var user))
                {
                    dto.UserName = user.UserName ?? user.Email;
                    dto.Email = user.Email;
                    dto.Name = user.Name;
                    dto.Surname = user.Surname;
                    
                    // Verificar si debe cambiar contraseña
                    var claims = await _userManager.GetClaimsAsync(user);
                    dto.MustChangePassword = claims.Any(c => c.Type == MustChangePasswordClaim && c.Value == "true");
                }
                else
                {
                    dto.UserName = "Usuario desconocido";
                }
            }
        }
        catch (OperationCanceledException)
        {
            foreach (var dto in architectDtos)
            {
                dto.UserName = "Usuario desconocido";
            }
        }
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por Id
    /// </summary>
    [AllowAnonymous]
    public async Task<ArchitectDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);
        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);

        try
        {
            var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
            if (user != null)
            {
                dto.UserName = user.UserName ?? user.Email;
                dto.Email = user.Email;
                dto.Name = user.Name;
                dto.Surname = user.Surname;
                
                var claims = await _userManager.GetClaimsAsync(user);
                dto.MustChangePassword = claims.Any(c => c.Type == MustChangePasswordClaim && c.Value == "true");
            }
        }
        catch (OperationCanceledException)
        {
            dto.UserName = "Usuario desconocido";
        }

        return dto;
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por UserId
    /// </summary>
    [AllowAnonymous]
    public async Task<ArchitectDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var queryable = await _architectRepository.GetQueryableAsync();
        var architect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == userId),
            cancellationToken
        );

        if (architect == null)
        {
            throw new UserFriendlyException("No se encontró perfil de arquitecto para este usuario");
        }

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);

        try
        {
            var user = await _userRepository.FindAsync(userId, cancellationToken: cancellationToken);
            if (user != null)
            {
                dto.UserName = user.UserName ?? user.Email;
                dto.Email = user.Email;
                dto.Name = user.Name;
                dto.Surname = user.Surname;
                
                var claims = await _userManager.GetClaimsAsync(user);
                dto.MustChangePassword = claims.Any(c => c.Type == MustChangePasswordClaim && c.Value == "true");
            }
        }
        catch (OperationCanceledException)
        {
            dto.UserName = "Usuario desconocido";
        }

        return dto;
    }

    /// <summary>
    /// Obtiene el perfil de arquitecto del usuario actual
    /// Retorna null si el usuario no tiene perfil de arquitecto
    /// </summary>
    [Authorize]
    public async Task<ArchitectDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return null;
        }

        var queryable = await _architectRepository.GetQueryableAsync();
        var architect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == CurrentUser.Id.Value),
            cancellationToken
        );

        // Si es admin y no tiene perfil, crearlo automáticamente para que sea tratado como arquitecto
        if (architect == null && CurrentUser.IsInRole("admin"))
        {
            architect = await EnsureArchitectProfileForUserAsync(CurrentUser.Id.Value, cancellationToken);
        }

        if (architect == null)
        {
            return null;
        }

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        dto.UserName = CurrentUser.UserName ?? CurrentUser.Email;
        dto.Email = CurrentUser.Email;
        dto.Name = CurrentUser.Name;
        dto.Surname = CurrentUser.SurName;

        try
        {
            var user = await _userRepository.FindAsync(CurrentUser.Id.Value, cancellationToken: cancellationToken);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                dto.MustChangePassword = claims.Any(c => c.Type == MustChangePasswordClaim && c.Value == "true");
            }
        }
        catch (OperationCanceledException)
        {
            // Ignorar
        }

        return dto;
    }

    /// <summary>
    /// Crea un nuevo arquitecto con usuario (Admin only)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Create)]
    public async Task<CreateArchitectResultDto> CreateWithUserAsync(
        CreateArchitectWithUserDto input, 
        CancellationToken cancellationToken = default)
    {
        // Verificar que el email no esté en uso
        var existingUser = await _userRepository.FindByNormalizedEmailAsync(input.Email.ToUpperInvariant());
        if (existingUser != null)
        {
            throw new UserFriendlyException($"Ya existe un usuario con el email {input.Email}");
        }

        // Generar contraseña temporal si no se proporcionó
        var temporaryPassword = input.TemporaryPassword ?? GenerateTemporaryPassword();

        // Crear usuario de Identity
        var userId = _guidGenerator.Create();
        var user = new IdentityUser(userId, input.Email, input.Email)
        {
            Name = input.Name,
            Surname = input.Surname
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Error al crear usuario: {errors}");
        }

        // Agregar claim para forzar cambio de contraseña
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(MustChangePasswordClaim, "true"));

        // Crear perfil de arquitecto
        var architect = new Architect
        {
            UserId = userId,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = Clock.Now,
            IsActive = true
        };

        await _architectRepository.InsertAsync(architect, autoSave: true, cancellationToken: cancellationToken);

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        dto.UserName = user.UserName;
        dto.Email = user.Email;
        dto.Name = user.Name;
        dto.Surname = user.Surname;
        dto.MustChangePassword = true;

        // Enviar email de bienvenida con credenciales (fire and forget)
        _ = SendWelcomeEmailAsync(input.Email, input.Name ?? "", input.Surname ?? "", temporaryPassword);

        return new CreateArchitectResultDto
        {
            Architect = dto,
            TemporaryPassword = temporaryPassword,
            Message = $"Arquitecto creado. Contraseña temporal: {temporaryPassword}"
        };
    }

    /// <summary>
    /// Envía email de bienvenida al nuevo arquitecto
    /// </summary>
    private async Task SendWelcomeEmailAsync(string email, string name, string surname, string temporaryPassword)
    {
        try
        {
            var baseUrl = _configuration["App:SelfUrl"] ?? "https://4cima.com";
            var fullName = string.IsNullOrEmpty(surname) ? name : $"{name} {surname}";

            await _emailService.SendArchitectWelcomeEmailAsync(new ArchitectWelcomeEmailDto
            {
                ArchitectEmail = email,
                ArchitectName = fullName,
                TemporaryPassword = temporaryPassword,
                LoginUrl = $"{baseUrl}/Account/Login"
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error al enviar email de bienvenida a arquitecto {Email}", email);
        }
    }

    /// <summary>
    /// Genera una contraseña temporal segura
    /// </summary>
    private string GenerateTemporaryPassword()
    {
        // Formato: Cima + 4 dígitos + símbolo
        var random = new Random();
        var digits = random.Next(1000, 9999);
        return $"Cima{digits}!";
    }

    /// <summary>
    /// Garantiza que el usuario tenga un perfil de arquitecto.
    /// Si no existe, lo crea con valores por defecto.
    /// </summary>
    private async Task<Architect> EnsureArchitectProfileForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var queryable = await _architectRepository.GetQueryableAsync();
        var existing = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == userId),
            cancellationToken
        );

        if (existing != null)
        {
            return existing;
        }

        var architect = new Architect
        {
            UserId = userId,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = Clock.Now,
            IsActive = true
        };

        await _architectRepository.InsertAsync(architect, autoSave: true, cancellationToken: cancellationToken);
        return architect;
    }

    /// <summary>
    /// Crea perfil de arquitecto para usuario actual (legacy)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Create)]
    public async Task<ArchitectDto> CreateAsync(CreateArchitectDto input, CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException("Usuario no autenticado");
        }

        var queryable = await _architectRepository.GetQueryableAsync();
        var existingArchitect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == CurrentUser.Id.Value),
            cancellationToken
        );

        if (existingArchitect != null)
        {
            throw new UserFriendlyException("Ya existe un perfil de arquitecto para este usuario");
        }

        var architect = new Architect
        {
            UserId = CurrentUser.Id.Value,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = Clock.Now,
            IsActive = true
        };

        await _architectRepository.InsertAsync(architect, autoSave: true, cancellationToken: cancellationToken);

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        dto.UserName = CurrentUser.UserName ?? CurrentUser.Email;

        return dto;
    }

    /// <summary>
    /// Actualiza perfil de arquitecto
    /// </summary>
    [Authorize(cimaPermissions.Architects.Edit)]
    public async Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input, CancellationToken cancellationToken = default)
    {
        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);

        var isOwner = architect.UserId == CurrentUser.Id;
        var isAdmin = CurrentUser.IsInRole("admin");

        if (!isOwner && !isAdmin)
        {
            throw new UserFriendlyException("No tienes permiso para editar este arquitecto");
        }

        // Solo admin puede cambiar IsActive
        if (input.IsActive.HasValue && isAdmin)
        {
            architect.IsActive = input.IsActive.Value;
        }

        await _architectRepository.UpdateAsync(architect, autoSave: true, cancellationToken: cancellationToken);

        // Actualizar datos del usuario si se proporcionaron
        var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
        if (user != null && (input.Name != null || input.Surname != null))
        {
            if (input.Name != null) user.Name = input.Name;
            if (input.Surname != null) user.Surname = input.Surname;
            await _userManager.UpdateAsync(user);
        }

        return await GetAsync(id, cancellationToken);
    }

    /// <summary>
    /// Elimina perfil de arquitecto y usuario asociado
    /// </summary>
    [Authorize(cimaPermissions.Architects.Delete)]
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);

        var queryable = await _listingRepository.GetQueryableAsync();
        var hasListings = await AsyncExecuter.AnyAsync(
            queryable.Where(l => l.ArchitectId == id),
            cancellationToken
        );

        if (hasListings)
        {
            throw new UserFriendlyException("No se puede eliminar el arquitecto porque tiene propiedades asociadas");
        }

        // Eliminar perfil de arquitecto
        await _architectRepository.DeleteAsync(id, autoSave: true, cancellationToken: cancellationToken);

        // Eliminar usuario asociado
        var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    /// <summary>
    /// Restablece contraseña de arquitecto (Admin only)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Edit)]
    public async Task<string> ResetPasswordAsync(Guid id, ResetArchitectPasswordDto input, CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("Solo administradores pueden restablecer contraseñas");
        }

        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);
        var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);

        if (user == null)
        {
            throw new UserFriendlyException("Usuario no encontrado");
        }

        // Generar o usar contraseña proporcionada
        var newPassword = input.NewTemporaryPassword ?? GenerateTemporaryPassword();

        // Resetear contraseña
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Error al restablecer contraseña: {errors}");
        }

        // Agregar/actualizar claim para forzar cambio
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var mustChangeClaim = existingClaims.FirstOrDefault(c => c.Type == MustChangePasswordClaim);
        
        if (mustChangeClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, mustChangeClaim);
        }
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(MustChangePasswordClaim, "true"));

        return newPassword;
    }

    /// <summary>
    /// Cambia la contraseña propia del arquitecto
    /// </summary>
    [Authorize]
    public async Task ChangePasswordAsync(ChangeArchitectPasswordDto input, CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException("Usuario no autenticado");
        }

        var user = await _userRepository.FindAsync(CurrentUser.Id.Value, cancellationToken: cancellationToken);
        if (user == null)
        {
            throw new UserFriendlyException("Usuario no encontrado");
        }

        var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Error al cambiar contraseña: {errors}");
        }

        // Remover claim de cambio obligatorio
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var mustChangeClaim = existingClaims.FirstOrDefault(c => c.Type == MustChangePasswordClaim);
        
        if (mustChangeClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, mustChangeClaim);
        }
    }

    /// <summary>
    /// Verifica si el usuario actual debe cambiar su contraseña
    /// </summary>
    [Authorize]
    public async Task<bool> MustChangePasswordAsync(CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return false;
        }

        try
        {
            // If caller already requested cancellation, return quickly
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var user = await _userRepository.FindAsync(CurrentUser.Id.Value, cancellationToken: cancellationToken);
            if (user == null)
            {
                return false;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Any(c => c.Type == MustChangePasswordClaim && c.Value == "true");
        }
        catch (OperationCanceledException)
        {
            // The operation was cancelled (client disconnected, timeout, or debugger pause).
            // Treat as non-critical and return false instead of bubbling the exception.
            return false;
        }
    }
}