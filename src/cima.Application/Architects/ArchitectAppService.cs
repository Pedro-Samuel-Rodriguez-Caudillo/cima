using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;
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
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ArchitectDto>> GetListAsync(
        PagedAndSortedResultRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var queryable = await _architectRepository.GetQueryableAsync();
        
        // Contar total de registros
        var totalCount = await AsyncExecuter.CountAsync(queryable, cancellationToken);

        // Aplicar ordenamiento (ABP tiene extensión ApplySorting)
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            // Usar System.Linq.Dynamic.Core para ordenamiento dinámico
            queryable = queryable.OrderBy(input.Sorting);
        }

        // Aplicar paginación
        var architects = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount),
            cancellationToken
        );

        // Mapear a DTOs
        var architectDtos = ObjectMapper.Map<List<Architect>, List<ArchitectDto>>(architects);

        // Cargar nombres de usuario de forma optimizada
        if (architectDtos.Any())
        {
            try
            {
                // Obtener todos los UserIds únicos
                var userIds = architectDtos.Select(x => x.UserId).Distinct().ToList();
                
                // Obtener los usuarios de forma batch usando GetListByIdsAsync si está disponible
                // Si no, usar GetListAsync sin filtro y filtrar en memoria (menos eficiente pero funciona)
                var users = await _userRepository.GetListAsync(cancellationToken: cancellationToken);
                
                // Filtrar por los IDs que necesitamos (en memoria)
                var relevantUsers = users.Where(u => userIds.Contains(u.Id)).ToList();
                var userDict = relevantUsers.ToDictionary(u => u.Id, u => u.UserName ?? u.Email ?? "Usuario desconocido");

                // Asignar nombres de usuario desde el diccionario
                foreach (var dto in architectDtos)
                {
                    if (userDict.TryGetValue(dto.UserId, out var userName))
                    {
                        dto.UserName = userName;
                    }
                    else
                    {
                        dto.UserName = "Usuario desconocido";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // La petición fue cancelada (navegación rápida del usuario en Blazor WASM)
                // Retornar datos parciales sin nombres de usuario
                foreach (var dto in architectDtos)
                {
                    dto.UserName = "Usuario desconocido";
                }
            }
        }

        return new PagedResultDto<ArchitectDto>(totalCount, architectDtos);
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
            // Cargar nombre de usuario
            var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
            if (user != null)
            {
                dto.UserName = user.UserName ?? user.Email ?? "Usuario desconocido";
            }
            else
            {
                dto.UserName = "Usuario desconocido";
            }
        }
        catch (OperationCanceledException)
        {
            // Si se cancela, retornar con nombre por defecto
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
            // Cargar nombre de usuario
            var user = await _userRepository.FindAsync(userId, cancellationToken: cancellationToken);
            if (user != null)
            {
                dto.UserName = user.UserName ?? user.Email ?? "Usuario desconocido";
            }
            else
            {
                dto.UserName = "Usuario desconocido";
            }
        }
        catch (OperationCanceledException)
        {
            // Si se cancela, retornar con nombre por defecto
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

        // Verificar que el usuario no tenga ya un perfil de arquitecto
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

        // Crear nuevo perfil con Name requerido
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

        // Verificar que solo el propietario o admin pueda actualizar
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

        // Actualizar campos
        // Solo admin puede cambiar IsActive
        if (input.IsActive.HasValue && isAdmin)
        {
            architect.IsActive = input.IsActive.Value;
        }

        await _architectRepository.UpdateAsync(architect, autoSave: true, cancellationToken: cancellationToken);

        var dto = ObjectMapper.Map<Architect, ArchitectDto>(architect);
        
        try
        {
            // Cargar nombre de usuario
            var user = await _userRepository.FindAsync(architect.UserId, cancellationToken: cancellationToken);
            if (user != null)
            {
                dto.UserName = user.UserName ?? user.Email ?? "Usuario desconocido";
            }
            else
            {
                dto.UserName = "Usuario desconocido";
            }
        }
        catch (OperationCanceledException)
        {
            // Si se cancela, retornar con nombre por defecto
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

        // Verificar si tiene propiedades asociadas
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