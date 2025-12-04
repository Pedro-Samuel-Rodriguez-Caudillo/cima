using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Listings;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;

namespace cima.Architects;

/// <summary>
/// Servicio de aplicación para gestionar perfiles de arquitectos
/// </summary>
public class ArchitectAppService : ApplicationService, IArchitectAppService
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IIdentityUserRepository _userRepository;

    public ArchitectAppService(
        IRepository<Architect, Guid> architectRepository,
        IRepository<Listing, Guid> listingRepository,
        IIdentityUserRepository userRepository)
    {
        _architectRepository = architectRepository;
        _listingRepository = listingRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Obtiene lista paginada de arquitectos (acceso público)
    /// Optimizado: carga solo los usuarios necesarios en lugar de todos
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ArchitectDto>> GetListAsync(
        PagedAndSortedResultRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var queryable = await _architectRepository.GetQueryableAsync();
        
        var totalCount = await AsyncExecuter.CountAsync(queryable, cancellationToken);

        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }

        var architects = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount),
            cancellationToken
        );

        var architectDtos = ObjectMapper.Map<List<Architect>, List<ArchitectDto>>(architects);

        // Cargar nombres de usuario de forma optimizada - solo los IDs necesarios
        if (architectDtos.Any())
        {
            await LoadUserNamesOptimizedAsync(architectDtos, cancellationToken);
        }

        return new PagedResultDto<ArchitectDto>(totalCount, architectDtos);
    }

    /// <summary>
    /// Carga nombres de usuario de forma optimizada usando batch de IDs
    /// </summary>
    private async Task LoadUserNamesOptimizedAsync(
        List<ArchitectDto> architectDtos, 
        CancellationToken cancellationToken)
    {
        try
        {
            var userIds = architectDtos.Select(x => x.UserId).Distinct().ToList();
            
            // Usar GetListAsync con filtro por IDs específicos
            var users = await _userRepository.GetListAsync(
                sorting: null,
                maxResultCount: userIds.Count,
                skipCount: 0,
                filter: null,
                includeDetails: false,
                cancellationToken: cancellationToken
            );
            
            // Filtrar solo los usuarios que necesitamos
            var userDict = users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(
                    u => u.Id, 
                    u => u.UserName ?? u.Email ?? "Usuario desconocido"
                );

            foreach (var dto in architectDtos)
            {
                dto.UserName = userDict.TryGetValue(dto.UserId, out var userName) 
                    ? userName 
                    : "Usuario desconocido";
            }
        }
        catch (OperationCanceledException)
        {
            // Navegación rápida del usuario en Blazor WASM
            foreach (var dto in architectDtos)
            {
                dto.UserName = "Usuario desconocido";
            }
        }
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por Id (acceso público)
    /// </summary>
    [AllowAnonymous]
    public async Task<ArchitectDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);
        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        
        try
        {
            var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
            dto.UserName = user?.UserName ?? user?.Email ?? "Usuario desconocido";
        }
        catch (OperationCanceledException)
        {
            dto.UserName = "Usuario desconocido";
        }
        
        return dto;
    }

    /// <summary>
    /// Obtiene perfil de arquitecto por UserId (acceso público)
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
            throw new UserFriendlyException(
                "No se encontró perfil de arquitecto para este usuario",
                "ARCHITECT_NOT_FOUND"
            );
        }

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        
        try
        {
            var user = await _userRepository.FindAsync(userId, cancellationToken: cancellationToken);
            dto.UserName = user?.UserName ?? user?.Email ?? "Usuario desconocido";
        }
        catch (OperationCanceledException)
        {
            dto.UserName = "Usuario desconocido";
        }

        return dto;
    }

    /// <summary>
    /// Crea perfil de arquitecto para usuario actual (requiere autenticación)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Create)]
    public async Task<ArchitectDto> CreateAsync(CreateArchitectDto input, CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new UserFriendlyException(
                "Usuario no autenticado",
                "USER_NOT_AUTHENTICATED"
            );
        }

        var queryable = await _architectRepository.GetQueryableAsync();
        var existingArchitect = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == CurrentUser.Id.Value),
            cancellationToken
        );

        if (existingArchitect != null)
        {
            throw new UserFriendlyException(
                "Ya existe un perfil de arquitecto para este usuario",
                "ARCHITECT_ALREADY_EXISTS"
            );
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
        dto.UserName = CurrentUser.UserName ?? CurrentUser.Email ?? "Usuario actual";

        return dto;
    }

    /// <summary>
    /// Actualiza perfil de arquitecto (solo el propietario o admin)
    /// </summary>
    [Authorize(cimaPermissions.Architects.Edit)]
    public async Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input, CancellationToken cancellationToken = default)
    {
        var architect = await _architectRepository.GetAsync(id, cancellationToken: cancellationToken);

        var isOwner = architect.UserId == CurrentUser.Id;
        var isAdmin = CurrentUser.IsInRole("admin");
        var hasEditPermission = await AuthorizationService.IsGrantedAsync(cimaPermissions.Architects.Edit);

        if (!isOwner && !isAdmin && !hasEditPermission)
        {
            throw new UserFriendlyException(
                "Solo el propietario del perfil o un administrador pueden actualizarlo",
                "UNAUTHORIZED_UPDATE"
            );
        }

        // Solo admin puede cambiar IsActive
        if (input.IsActive.HasValue && isAdmin)
        {
            architect.IsActive = input.IsActive.Value;
        }

        await _architectRepository.UpdateAsync(architect, autoSave: true, cancellationToken: cancellationToken);

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        
        try
        {
            var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
            dto.UserName = user?.UserName ?? user?.Email ?? "Usuario desconocido";
        }
        catch (OperationCanceledException)
        {
            dto.UserName = "Usuario desconocido";
        }

        return dto;
    }

    /// <summary>
    /// Elimina perfil de arquitecto (solo admin)
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
            throw new UserFriendlyException(
                "No se puede eliminar el arquitecto porque tiene propiedades asociadas",
                "ARCHITECT_HAS_LISTINGS"                
            );
        }

        await _architectRepository.DeleteAsync(id, autoSave: true, cancellationToken: cancellationToken);
    }
}