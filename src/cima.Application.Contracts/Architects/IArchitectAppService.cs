using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Architects;

/// <summary>
/// Servicio para gestionar perfiles de arquitectos
/// </summary>
public interface IArchitectAppService : IApplicationService
{
    /// <summary>
    /// Obtiene lista paginada de arquitectos
    /// </summary>
    Task<PagedResultDto<ArchitectDto>> GetListAsync(PagedAndSortedResultRequestDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene perfil de arquitecto por Id
    /// </summary>
    Task<ArchitectDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene perfil de arquitecto por UserId (IdentityUser)
    /// </summary>
    Task<ArchitectDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el perfil de arquitecto del usuario actual
    /// Retorna null si el usuario no tiene perfil de arquitecto
    /// </summary>
    Task<ArchitectDto?> GetCurrentAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Crea un nuevo arquitecto con usuario (Admin only)
    /// Incluye: crear IdentityUser + perfil Architect + contraseña temporal
    /// </summary>
    Task<CreateArchitectResultDto> CreateWithUserAsync(CreateArchitectWithUserDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Crea perfil de arquitecto para usuario actual (legacy)
    /// </summary>
    Task<ArchitectDto> CreateAsync(CreateArchitectDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza perfil de arquitecto
    /// </summary>
    Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina perfil de arquitecto (y usuario asociado)
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restablece contraseña de arquitecto (Admin only)
    /// Genera contraseña temporal y marca que debe cambiarla
    /// </summary>
    Task<string> ResetPasswordAsync(Guid id, ResetArchitectPasswordDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cambia la contraseña propia del arquitecto
    /// </summary>
    Task ChangePasswordAsync(ChangeArchitectPasswordDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario actual debe cambiar su contraseña
    /// </summary>
    Task<bool> MustChangePasswordAsync(CancellationToken cancellationToken = default);
}
