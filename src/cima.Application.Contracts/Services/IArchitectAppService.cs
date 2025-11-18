using System;
using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Services
{
    /// <summary>
    /// Servicio para gestionar perfiles de arquitectos
    /// </summary>
    public interface IArchitectAppService : IApplicationService
    {
        /// <summary>
        /// Obtiene perfil de arquitecto por Id
        /// </summary>
        Task<ArchitectDto> GetAsync(Guid id);
        
        /// <summary>
        /// Obtiene perfil de arquitecto por UserId (IdentityUser)
        /// </summary>
        Task<ArchitectDto> GetByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Crea perfil de arquitecto para usuario actual
        /// </summary>
        Task<ArchitectDto> CreateAsync(CreateArchitectDto input);
        
        /// <summary>
        /// Actualiza perfil de arquitecto
        /// </summary>
        Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input);
        
        /// <summary>
        /// Elimina perfil de arquitecto
        /// </summary>
        Task DeleteAsync(Guid id);
    }
}
