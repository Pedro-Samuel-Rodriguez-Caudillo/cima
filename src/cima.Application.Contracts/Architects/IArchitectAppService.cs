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
    /// Crea perfil de arquitecto para usuario actual
    /// </summary>
    Task<ArchitectDto> CreateAsync(CreateArchitectDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza perfil de arquitecto
    /// </summary>
    Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina perfil de arquitecto
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
