using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
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
    private readonly IDistributedCache _distributedCache;
    private const string FeaturedListingsCacheKey = "FeaturedListingsForHomepage";

    public ListingAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IDistributedCache distributedCache)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _distributedCache = distributedCache;
    }

    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    public async Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving (WithDetailsAsync garantiza carga)
            listing => listing.Images!);    // ? null-forgiving

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                (p.Location != null && p.Location.Contains(input.SearchTerm)) ||  // ? null check
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
        var listingQueryable = await _listingRepository.WithDetailsAsync(
            l => l.Architect!,  // ? null-forgiving
            l => l.Images!);    // ? null-forgiving

        var listing = await AsyncExecuter.FirstOrDefaultAsync(
            listingQueryable.Where(l => l.Id == id));

        if (listing == null)
        {
            throw new EntityNotFoundException(typeof(Listing), id);
        }

        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Crea nueva propiedad en estado Draft
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDto> CreateAsync(CreateUpdateListingDto input)
    {
        // Normalizar datos de entrada
        var normalizedTitle = input.Title?.Trim();
        var normalizedDescription = input.Description?.Trim();
        var normalizedLocation = input.Location?.Trim();

        // Validaciones adicionales
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new BusinessException("Listing:TitleRequired")
                .WithData("Field", "Title");
        }

        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            throw new BusinessException("Listing:DescriptionRequired")
                .WithData("Field", "Description");
        }

        if (string.IsNullOrWhiteSpace(normalizedLocation))
        {
            throw new BusinessException("Listing:LocationRequired")
                .WithData("Field", "Location");
        }

        if (input.Price <= 0)
        {
            throw new BusinessException("Listing:InvalidPrice")
                .WithData("Price", input.Price)
                .WithData("MinPrice", 0.01);
        }

        if (input.Area <= 0)
        {
            throw new BusinessException("Listing:InvalidArea")
                .WithData("Area", input.Area)
                .WithData("MinArea", 1);
        }

        // Validar que el arquitecto existe
        var architectExists = await _architectRepository.AnyAsync(a => a.Id == input.ArchitectId);
        if (!architectExists)
        {
            throw new BusinessException("Architect:NotFound")
                .WithData("ArchitectId", input.ArchitectId);
        }

        var listing = new Listing
        {
            Title = normalizedTitle,
            Description = normalizedDescription,
            Location = normalizedLocation,
            Price = input.Price,
            Area = input.Area,
            Bedrooms = input.Bedrooms,
            Bathrooms = input.Bathrooms,
            Category = input.Category,
            Type = input.Type,
            TransactionType = input.TransactionType,
            ArchitectId = input.ArchitectId,
            CreatedAt = Clock.Now,
            CreatedBy = CurrentUser.Id,
            Status = ListingStatus.Draft
        };

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

        // Normalizar datos de entrada
        var normalizedTitle = input.Title?.Trim();
        var normalizedDescription = input.Description?.Trim();
        var normalizedLocation = input.Location?.Trim();

        // Validaciones adicionales
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new BusinessException("Listing:TitleRequired")
                .WithData("Field", "Title");
        }

        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            throw new BusinessException("Listing:DescriptionRequired")
                .WithData("Field", "Description");
        }

        if (string.IsNullOrWhiteSpace(normalizedLocation))
        {
            throw new BusinessException("Listing:LocationRequired")
                .WithData("Field", "Location");
        }

        if (input.Price <= 0)
        {
            throw new BusinessException("Listing:InvalidPrice")
                .WithData("Price", input.Price)
                .WithData("MinPrice", 0.01);
        }

        if (input.Area <= 0)
        {
            throw new BusinessException("Listing:InvalidArea")
                .WithData("Area", input.Area)
                .WithData("MinArea", 1);
        }

        // Mapear TODOS los campos editables with datos normalizados
        listing.Title = normalizedTitle;
        listing.Description = normalizedDescription;
        listing.Location = normalizedLocation;
        listing.Price = input.Price;
        listing.Area = input.Area;
        listing.Bedrooms = input.Bedrooms;
        listing.Bathrooms = input.Bathrooms;
        listing.Category = input.Category;
        listing.Type = input.Type;
        listing.TransactionType = input.TransactionType;
        
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
            throw new AbpAuthorizationException("Solo puedes publicar tus propias propiedades");  // ? Usar clase correcta del using
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
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving
            listing => listing.Images!);    // ? null-forgiving

        // Solo propiedades publicadas
        queryable = queryable.Where(p => p.Status == ListingStatus.Published);

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                (p.Location != null && p.Location.Contains(input.SearchTerm)) ||  // ? null check
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
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving
            listing => listing.Images!);    // ? null-forgiving
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

    /// <summary>
    /// Agrega una imagen a una propiedad
    /// TODO: Refactorizar para usar lista enlazada en lugar de DisplayOrder
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingImageDto> AddImageAsync(Guid listingId, CreateListingImageDto input)
    {
        throw new NotImplementedException("AddImageAsync necesita ser refactorizado para usar lista enlazada");
        
        // TODO: Implementar con lista enlazada
        // 1. Obtener última imagen de la lista (NextImageId == null)
        // 2. Crear nueva imagen con PreviousImageId = última imagen
        // 3. Actualizar última imagen para que NextImageId apunte a la nueva
    }

    /// <summary>
    /// Elimina una imagen de una propiedad
    /// TODO: Refactorizar para usar lista enlazada
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveImageAsync(Guid listingId, Guid imageId)
    {
        throw new NotImplementedException("RemoveImageAsync necesita ser refactorizado para usar lista enlazada");
        
        // TODO: Implementar con lista enlazada
        // 1. Encontrar la imagen a eliminar
        // 2. Actualizar imagen anterior para que NextImageId apunte a la siguiente
        // 3. Actualizar imagen siguiente para que PreviousImageId apunte a la anterior
        // 4. Eliminar la imagen
    }

    /// <summary>
    /// Actualiza el orden de las imágenes
    /// TODO: Refactorizar para usar lista enlazada
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task UpdateImagesOrderAsync(Guid listingId, List<UpdateImageOrderDto> input)
    {
        throw new NotImplementedException("UpdateImagesOrderAsync necesita ser refactorizado para usar lista enlazada");
        
        // TODO: Implementar con lista enlazada
        // 1. Reconstruir toda la lista enlazada según el nuevo orden
        // 2. Actualizar PreviousImageId y NextImageId de todas las imágenes
    }

    /// <summary>
    /// Cambia una propiedad a estado Portfolio (showcase de proyectos completados)
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> MoveToPortfolioAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !await IsAdminAsync())
        {
            throw new AbpAuthorizationException("Solo puedes mover a portafolio tus propias propiedades");
        }

        listing.Status = ListingStatus.Portfolio;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Búsqueda avanzada de propiedades con filtros
    /// Incluye validación automática por DataAnnotations de ABP
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ListingDto>> SearchAsync(PropertySearchDto searchDto)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving
            listing => listing.Images!);    // ? null-forgiving

        // Solo propiedades publicadas para búsqueda pública
        queryable = queryable.Where(p => p.Status == ListingStatus.Published);

        // Filtro por tipo de transacción
        if (searchDto.TransactionType.HasValue)
        {
            queryable = queryable.Where(p => p.TransactionType == searchDto.TransactionType.Value);
        }

        // Filtro por categoría
        if (searchDto.Category.HasValue)
        {
            queryable = queryable.Where(p => p.Category == searchDto.Category.Value);
        }

        // Filtro por tipo de propiedad
        if (searchDto.Type.HasValue)
        {
            queryable = queryable.Where(p => p.Type == searchDto.Type.Value);
        }

        // Filtro por ubicación (ya validado por RegEx en DTO)
        if (!string.IsNullOrWhiteSpace(searchDto.Location))
        {
            queryable = queryable.Where(p => p.Location != null && p.Location.Contains(searchDto.Location));  // ? null check
        }

        // Rango de precio
        if (searchDto.MinPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price >= searchDto.MinPrice.Value);
        }
        if (searchDto.MaxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price <= searchDto.MaxPrice.Value);
        }

        // Filtros de recámaras y baños
        if (searchDto.MinBedrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bedrooms >= searchDto.MinBedrooms.Value);
        }
        if (searchDto.MinBathrooms.HasValue)
        {
            queryable = queryable.Where(p => p.Bathrooms >= searchDto.MinBathrooms.Value);
        }

        // Rango de área
        if (searchDto.MinArea.HasValue)
        {
            queryable = queryable.Where(p => p.Area >= searchDto.MinArea.Value);
        }
        if (searchDto.MaxArea.HasValue)
        {
            queryable = queryable.Where(p => p.Area <= searchDto.MaxArea.Value);
        }

        // Aplicar ordenamiento
        queryable = searchDto.SortBy?.ToLower() switch
        {
            "price-low" => queryable.OrderBy(p => p.Price),
            "price-high" => queryable.OrderByDescending(p => p.Price),
            "area-large" => queryable.OrderByDescending(p => p.Area),
            "area-small" => queryable.OrderBy(p => p.Area),
            "newest" => queryable.OrderByDescending(p => p.CreatedAt),
            "oldest" => queryable.OrderBy(p => p.CreatedAt),
            _ => queryable.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var skipCount = searchDto.PageNumber * searchDto.PageSize;
        var listings = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(skipCount)
                .Take(searchDto.PageSize)
        );

        return new PagedResultDto<ListingDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingDto>>(listings)
        );
    }

    /// <summary>
    /// Obtiene propiedades en portafolio (proyectos completados/showcase)
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ListingDto>> GetPortfolioAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving
            listing => listing.Images!);    // ? null-forgiving

        // Solo propiedades en estado Portfolio
        queryable = queryable.Where(p => p.Status == ListingStatus.Portfolio);

        // Aplicar filtros básicos
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                (p.Location != null && p.Location.Contains(input.SearchTerm)));  // ? null check
        }

        if (input.PropertyType.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Type == input.PropertyType.Value);
        }

        if (input.PropertyCategory.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Category == input.PropertyCategory.Value);
        }

        // Ordenamiento
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
    /// Obtiene sugerencias de ubicaciones para autocompletado
    /// Basado en ubicaciones existentes en propiedades publicadas
    /// </summary>
    [AllowAnonymous]
    public async Task<List<LocationSuggestionDto>> GetLocationSuggestionsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return new List<LocationSuggestionDto>();
        }

        // Sanitizar input (ABP ya valida, pero doble seguridad)
        searchTerm = searchTerm.Trim();

        var queryable = await _listingRepository.GetQueryableAsync();

        // Solo propiedades publicadas o en portafolio
        queryable = queryable.Where(p => 
            p.Status == ListingStatus.Published || 
            p.Status == ListingStatus.Portfolio);

        // Filtrar por ubicaciones que contengan el término de búsqueda
        queryable = queryable.Where(p => p.Location != null && p.Location.Contains(searchTerm));  // ? null check

        // Agrupar por ubicación y contar
        var locations = await AsyncExecuter.ToListAsync(queryable);

        var suggestions = locations
            .Where(p => p.Location != null)  // ? filtrar nulls antes de agrupar
            .GroupBy(p => p.Location!)  // ? null-forgiving operator (ya verificado arriba)
            .Select(g => new LocationSuggestionDto
            {
                Location = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(s => s.Count)
            .ThenBy(s => s.Location)
            .Take(10)
            .ToList();

        return suggestions;
    }
}
