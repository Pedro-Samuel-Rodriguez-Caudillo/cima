using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;


namespace cima.Listings;

/// <summary>
/// Implementacion del servicio de propiedades inmobiliarias
/// </summary>
[Authorize(cimaPermissions.Listings.Default)]
public class ListingAppService : cimaAppService, IListingAppService
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ListingAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
    }

    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    public async Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.GetQueryableAsync();


        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                p.Location.Contains(input.SearchTerm) ||
                p.Description.Contains(input.SearchTerm));
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Status == input.Status.Value);
        }

        if (input.MinPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price >= input.MinPrice.Value);
        }

        if (input.MaxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price <= input.MaxPrice.Value);
        }

        if (input.MinBedrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bedrooms >= input.MinBedrooms.Value);
        }

        if (input.MinBathrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bathrooms >= input.MinBathrooms.Value);
        }

        if (input.ArchitectId.HasValue)
        {
            queryable = queryable.Where(p => p.ArchitectId == input.ArchitectId.Value);
        }

        if (input.PropertyType.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Type == input.PropertyType.Value);
        }

        if (input.TransactionType.HasValue)
        {
            queryable = queryable.Where(p => (int)p.TransactionType == input.TransactionType.Value);
        }

        // Aplicar ordenamiento
        queryable = ApplySorting(queryable, input.Sorting);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var listings = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<ListingDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingDto>>(listings)
        );
    }

    /// <summary>
    /// Aplica ordenamiento a la consulta
    /// </summary>
    private IQueryable<Listing> ApplySorting(IQueryable<Listing> queryable, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(p => p.CreatedAt);
        }

        return sorting.ToLower() switch
        {
            "price" => queryable.OrderBy(p => p.Price),
            "price desc" => queryable.OrderByDescending(p => p.Price),
            "priceasc" => queryable.OrderBy(p => p.Price),
            "pricedesc" => queryable.OrderByDescending(p => p.Price),
            "area" => queryable.OrderBy(p => p.Area),
            "area desc" => queryable.OrderByDescending(p => p.Area),
            "areaasc" => queryable.OrderBy(p => p.Area),
            "areadesc" => queryable.OrderByDescending(p => p.Area),
            "createdat" => queryable.OrderBy(p => p.CreatedAt),
            "createdat desc" => queryable.OrderByDescending(p => p.CreatedAt),
            "title" => queryable.OrderBy(p => p.Title),
            "title desc" => queryable.OrderByDescending(p => p.Title),
            _ => queryable.OrderByDescending(p => p.CreatedAt)
        };
    }

    /// <summary>
    /// Obtiene detalle de una propiedad por Id
    /// </summary>
    public async Task<ListingDto> GetAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Crea nueva propiedad en estado Draft
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDto> CreateAsync(CreateUpdateListingDto input)
    {
        // Validar que el arquitecto existe
        var architectExists = await _architectRepository.AnyAsync(a => a.Id == input.ArchitectId);
        if (!architectExists)
        {
            throw new BusinessException("Architect:NotFound")
                .WithData("ArchitectId", input.ArchitectId);
        }

        var listing = ObjectMapper.Map<CreateUpdateListingDto, Listing>(input);
        listing.CreatedAt = Clock.Now;
        listing.CreatedBy = CurrentUser.Id;
        listing.Status = ListingStatus.Draft;

        await _listingRepository.InsertAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Actualiza propiedad existente
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> UpdateAsync(Guid id, CreateUpdateListingDto input)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion: Solo el dueno o admin puede editar
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes editar tus propias propiedades");
        }

        // Mapear solo los campos editables (sin ArchitectId)
        listing.Title = input.Title;
        listing.Description = input.Description;
        listing.Location = input.Location;
        listing.Price = input.Price;
        listing.Area = input.Area;
        listing.Bedrooms = input.Bedrooms;
        listing.Bathrooms = input.Bathrooms;
        
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Elimina propiedad
    /// </summary>
    [Authorize(cimaPermissions.Listings.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion: Solo el dueno o admin puede eliminar
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes eliminar tus propias propiedades");
        }

        await _listingRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Cambia estado de Draft a Published
    /// Permite publicar sin imagenes (con warning en logs)
    /// </summary>
    [Authorize(cimaPermissions.Listings.Publish)]
    public async Task<ListingDto> PublishAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad del listing
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes publicar tus propias propiedades");
        }

        // Validar que está en estado Draft
        if (listing.Status == ListingStatus.Published)
        {
            throw new BusinessException("Listing:AlreadyPublished")
                .WithData("ListingId", id);
        }

        // Warning si no tiene imagenes (pero permite publicar)
        if (listing.Images == null || !listing.Images.Any())
        {
            Logger.LogWarning($"Publicando propiedad {id} sin imágenes");
        }

        listing.Status = ListingStatus.Published;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Cambia estado a Archived
    /// </summary>
    [Authorize(cimaPermissions.Listings.Archive)]
    public async Task<ListingDto> ArchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes archivar tus propias propiedades");
        }

        listing.Status = ListingStatus.Archived;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Reactiva una propiedad archivada a estado Published
    /// </summary>
    [Authorize(cimaPermissions.Listings.Publish)]
    public async Task<ListingDto> UnarchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes desarchivar tus propias propiedades");
        }

        if (listing.Status != ListingStatus.Archived)
        {
            throw new BusinessException("Listing:NotArchived")
                .WithData("ListingId", id)
                .WithData("CurrentStatus", listing.Status);
        }

        listing.Status = ListingStatus.Published;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Cambia una propiedad publicada de vuelta a Draft
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> UnpublishAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes despublicar tus propias propiedades");
        }

        if (listing.Status != ListingStatus.Published)
        {
            throw new BusinessException("Listing:NotPublished")
                .WithData("ListingId", id)
                .WithData("CurrentStatus", listing.Status);
        }

        listing.Status = ListingStatus.Draft;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Obtiene solo propiedades publicadas (público - sin autenticación)
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ListingDto>> GetPublishedAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.GetQueryableAsync();

        // Solo propiedades publicadas
        queryable = queryable.Where(p => p.Status == ListingStatus.Published);

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                p.Location.Contains(input.SearchTerm) ||
                p.Description.Contains(input.SearchTerm));
        }

        if (input.MinPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price >= input.MinPrice.Value);
        }

        if (input.MaxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price <= input.MaxPrice.Value);
        }

        if (input.MinBedrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bedrooms >= input.MinBedrooms.Value);
        }

        if (input.MinBathrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bathrooms >= input.MinBathrooms.Value);
        }

        if (input.PropertyType.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Type == input.PropertyType.Value);
        }

        if (input.TransactionType.HasValue)
        {
            queryable = queryable.Where(p => (int)p.TransactionType == input.TransactionType.Value);
        }

        // Aplicar ordenamiento
        queryable = ApplySorting(queryable, input.Sorting);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var listings = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<ListingDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingDto>>(listings)
        );
    }

    /// <summary>
    /// Obtiene propiedades de un arquitecto especifico
    /// </summary>
    public async Task<PagedResultDto<ListingDto>> GetByArchitectAsync(
        Guid architectId, int skipCount, int maxResultCount)
    {
        var queryable = await _listingRepository.GetQueryableAsync();
        queryable = queryable.Where(p => p.ArchitectId == architectId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var listings = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skipCount)
                .Take(maxResultCount)
        );

        return new PagedResultDto<ListingDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingDto>>(listings)
        );
    }

    /// <summary>
    /// Verifica si el usuario current es administrador
    /// </summary>
    private async Task<bool> IsAdminAsync()
    {
        return await Task.FromResult(CurrentUser.IsInRole("admin"));
    }
}
