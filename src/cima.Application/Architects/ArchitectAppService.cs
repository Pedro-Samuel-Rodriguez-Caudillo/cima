using System;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace cima.Architects;

/// <summary>
/// Servicio de aplicación para gestionar perfiles de arquitectos
/// </summary>
public class ArchitectAppService : ApplicationService, IArchitectAppService
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;

    public ArchitectAppService(
        IRepository<Architect, Guid> architectRepository,
        IRepository<Listing, Guid> listingRepository)
    {
        _architectRepository = architectRepository;
        _listingRepository = listingRepository;
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por Id (acceso público)
    /// </summary>
    [AllowAnonymous]
    public async Task<ArchitectDto> GetAsync(Guid id)
    {
        var architect = await _architectRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        
        // UserId ya es Guid, no Guid?
        dto.UserId = architect.UserId;
        
        return dto;
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por UserId (acceso público)
    /// </summary>
    [AllowAnonymous]
    public async Task<ArchitectDto> GetByUserIdAsync(Guid userId)
    {
        var queryable = await _architectRepository.GetQueryableAsync();
        var architect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == userId)
        );

        if (architect == null)
        {
            throw new UserFriendlyException(
                "No se encontró perfil de arquitecto para este usuario",
                "ARCHITECT_NOT_FOUND"
            );
        }

        return ObjectMapper.Map<Architect, ArchitectDto>(architect);
    }

    /// <summary>
    /// Crea perfil de arquitecto para usuario actual (requiere autenticación)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Create)]
    public async Task<ArchitectDto> CreateAsync(CreateArchitectDto input)
    {
        // CurrentUser.Id es Guid?, necesitamos verificar que existe
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException(
                "Usuario no autenticado",
                "USER_NOT_AUTHENTICATED"
            );
        }

        // Verificar que el usuario no tenga ya un perfil de arquitecto
        var queryable = await _architectRepository.GetQueryableAsync();
        var existingArchitect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == CurrentUser.Id.Value)
        );

        if (existingArchitect != null)
        {
            throw new UserFriendlyException(
                "Ya existe un perfil de arquitecto para este usuario",
                "ARCHITECT_ALREADY_EXISTS"
            );
        }

        // Crear nuevo perfil - El Id se genera automáticamente por ABP
        var architect = new Architect
        {
            UserId = CurrentUser.Id.Value,
            Bio = input.Bio?.Trim() ?? string.Empty,
            PortfolioUrl = input.PortfolioUrl?.Trim() ?? string.Empty
        };

        await _architectRepository.InsertAsync(architect, autoSave: true);

        return ObjectMapper.Map<Architect, ArchitectDto>(architect);
    }

    /// <summary>
    /// Actualiza perfil de arquitecto (solo el propietario o admin)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Edit)]
    public async Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input)
    {
        var architect = await _architectRepository.GetAsync(id);

        // Verificar que solo el propietario o admin pueda actualizar
        if (architect.UserId != CurrentUser.Id && 
            !await AuthorizationService.IsGrantedAsync(cimaPermissions.Architects.Delete))
        {
            throw new UserFriendlyException(
                "Solo el propietario del perfil puede actualizarlo",
                "UNAUTHORIZED_UPDATE"
            );
        }

        // Actualizar campos
        architect.Bio = input.Bio?.Trim() ?? string.Empty;
        architect.PortfolioUrl = input.PortfolioUrl?.Trim() ?? string.Empty;

        await _architectRepository.UpdateAsync(architect, autoSave: true);

        return ObjectMapper.Map<Architect, ArchitectDto>(architect);
    }

    /// <summary>
    /// Elimina perfil de arquitecto (solo admin)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var architect = await _architectRepository.GetAsync(id);

        // Verificar si tiene propiedades asociadas
        var queryable = await _listingRepository.GetQueryableAsync();
        var hasListings = await AsyncExecuter.AnyAsync(
            queryable.Where(l => l.ArchitectId == id)
        );

        if (hasListings)
        {
            throw new UserFriendlyException(
                "No se puede eliminar el arquitecto porque tiene propiedades asociadas",
                "ARCHITECT_HAS_LISTINGS"                
            );
        }

        await _architectRepository.DeleteAsync(id, autoSave: true);
    }
}       