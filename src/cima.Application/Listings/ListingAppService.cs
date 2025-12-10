using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cima.Architects;
using cima.Domain.Entities;
using cima.Domain.Services.Listings;
using cima.Domain.Shared;
using cima.Images;
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
    private readonly IListingManager _listingManager;
    private readonly Images.IImageStorageService _imageStorageService;
    private const string FeaturedListingsCacheKey = "FeaturedListingsForHomepage";

    public ListingAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IDistributedCache distributedCache,
        IListingManager listingManager,
        Images.IImageStorageService imageStorageService)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _distributedCache = distributedCache;
        _listingManager = listingManager;
        _imageStorageService = imageStorageService;
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
    /// Aplica ordenamiento a la consulta.
    /// Soporta múltiples formatos de entrada para flexibilidad.
    /// </summary>
    private IQueryable<Listing> ApplySorting(IQueryable<Listing> queryable, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(p => p.CreatedAt);
        }

        // Normalizar: quitar espacios extras y convertir a minúsculas
        var normalizedSorting = sorting.Trim().ToLower().Replace(" ", "");

        return normalizedSorting switch
        {
            // Price sorting
            "price" or "priceasc" => queryable.OrderBy(p => p.Price),
            "pricedesc" => queryable.OrderByDescending(p => p.Price),
            
            // Land Area sorting - corregidos todos los casos posibles
            "landarea" or "landareaasc" => queryable.OrderBy(p => p.LandArea),
            "landareadesc" => queryable.OrderByDescending(p => p.LandArea),
            
            // Area sorting (backward compatibility)
            "area" or "areaasc" => queryable.OrderBy(p => p.LandArea),
            "areadesc" => queryable.OrderByDescending(p => p.LandArea),
            
            // CreatedAt sorting
            "createdat" or "createdatasc" => queryable.OrderBy(p => p.CreatedAt),
            "createdatdesc" => queryable.OrderByDescending(p => p.CreatedAt),
            
            // Title sorting
            "title" or "titleasc" => queryable.OrderBy(p => p.Title),
            "titledesc" => queryable.OrderByDescending(p => p.Title),
            
            // Default - más recientes primero
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
    /// Crea nueva propiedad en estado Draft usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDto> CreateAsync(CreateUpdateListingDto input)
    {
        // Usar ListingManager para crear con validaciones y eventos de dominio
        var listing = await _listingManager.CreateAsync(
            title: input.Title?.Trim() ?? string.Empty,
            description: input.Description?.Trim() ?? string.Empty,
            location: input.Location?.Trim(),
            price: input.Price,
            landArea: input.LandArea,
            constructionArea: input.ConstructionArea,
            bedrooms: input.Bedrooms,
            bathrooms: input.Bathrooms,
            category: input.Category,
            type: input.Type,
            transactionType: input.TransactionType,
            architectId: input.ArchitectId,
            createdBy: CurrentUser.Id);

        await _listingRepository.InsertAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Actualiza propiedad existente con validaciones del ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> UpdateAsync(Guid id, CreateUpdateListingDto input)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion: Solo el dueno o admin puede editar
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes editar tus propias propiedades");
        }

        // Validar que el arquitecto está activo
        if (!architect.IsActive)
        {
            throw new BusinessException(cimaDomainErrorCodes.ArchitectInactive)
                .WithData("ArchitectId", architect.Id);
        }

        // Normalizar y validar datos usando ListingManager
        var normalizedTitle = input.Title?.Trim() ?? string.Empty;
        var normalizedDescription = input.Description?.Trim() ?? string.Empty;
        var normalizedLocation = input.Location?.Trim();

        _listingManager.ValidateListingData(
            normalizedTitle,
            normalizedDescription,
            input.Price,
            input.LandArea,
            input.ConstructionArea);

        // Mapear campos editables
        listing.Title = normalizedTitle;
        listing.Description = normalizedDescription;
        listing.Location = normalizedLocation;
        listing.Price = input.Price;
        listing.LandArea = input.LandArea;
        listing.ConstructionArea = input.ConstructionArea;
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
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes eliminar tus propias propiedades");
        }

        await _listingRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Cambia estado de Draft a Published usando ListingManager
    /// Los eventos de dominio actualizan las estadásticas del arquitecto
    /// </summary>
    [Authorize(cimaPermissions.Listings.Publish)]
    public async Task<ListingDto> PublishAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad del listing
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes publicar tus propias propiedades");
        }

        // Validar que el arquitecto está activo
        if (!architect.IsActive)
        {
            throw new BusinessException(cimaDomainErrorCodes.ArchitectInactive)
                .WithData("ArchitectId", architect.Id);
        }

        // Warning si no tiene imagenes (pero permite publicar)
        if (listing.Images == null || !listing.Images.Any())
        {
            Logger.LogWarning("Publicando propiedad {ListingId} sin imágenes", id);
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.PublishAsync(listing, CurrentUser.Id);

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Cambia estado a Archived usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Archive)]
    public async Task<ListingDto> ArchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes archivar tus propias propiedades");
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.ArchiveAsync(listing, CurrentUser.Id);

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Reactiva una propiedad archivada a estado Published usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Publish)]
    public async Task<ListingDto> UnarchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes desarchivar tus propias propiedades");
        }

        // Validar que el arquitecto está activo
        if (!architect.IsActive)
        {
            throw new BusinessException(cimaDomainErrorCodes.ArchitectInactive)
                .WithData("ArchitectId", architect.Id);
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnarchiveAsync(listing, CurrentUser.Id);

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Cambia una propiedad publicada de vuelta a Draft usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> UnpublishAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes despublicar tus propias propiedades");
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnpublishAsync(listing, CurrentUser.Id);

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Cambia una propiedad a estado Portfolio usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDto> MoveToPortfolioAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes mover a portafolio tus propias propiedades");
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.MoveToPortfolioAsync(listing, CurrentUser.Id);

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDto>(listing);
    }

    /// <summary>
    /// Duplica una propiedad existente creando una copia en estado Draft
    /// No copia las imágenes, solo los datos de la propiedad
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDto> DuplicateAsync(Guid id)
    {
        var original = await _listingRepository.GetAsync(id);

        // Validacion: Solo el dueño o admin puede duplicar
        var architect = await _architectRepository.GetAsync(original.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes duplicar tus propias propiedades");
        }

        // Crear nueva propiedad usando ListingManager
        var duplicatedListing = await _listingManager.CreateAsync(
            title: $"{original.Title} (Copia)",
            description: original.Description,
            location: original.Location,
            price: original.Price,
            landArea: original.LandArea,
            constructionArea: original.ConstructionArea,
            bedrooms: original.Bedrooms,
            bathrooms: original.Bathrooms,
            category: original.Category,
            type: original.Type,
            transactionType: original.TransactionType,
            architectId: original.ArchitectId,
            createdBy: CurrentUser.Id);

        await _listingRepository.InsertAsync(duplicatedListing);
        
        Logger.LogInformation(
            "Propiedad {OriginalId} duplicada a {NewId} por usuario {UserId}",
            id, duplicatedListing.Id, CurrentUser.Id);

        return ObjectMapper.Map<Listing, ListingDto>(duplicatedListing);
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
    /// Verifica si el usuario actual es administrador (método sincrónico)
    /// </summary>
    private bool IsAdmin()
    {
        return CurrentUser.IsInRole("admin");
    }

    /// <summary>
    /// Agrega una imagen a una propiedad usando lista enlazada.
    /// La nueva imagen se agrega al final de la galería.
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingImageDto> AddImageAsync(Guid listingId, CreateListingImageDto input)
    {
        var listing = await GetListingWithImagesAsync(listingId);

        // Validación de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes editar imágenes de tus propias propiedades");
        }

        // Guardar imagen en almacenamiento si viene como data URL
        var storedImage = await StoreImageIfNeededAsync(input);

        // Crear nuevo ID para la imagen
        var newImageId = Guid.NewGuid();
        
        // Encontrar la última imagen (NextImageId == null)
        ListingImage? lastImage = null;
        if (listing.Images.Any())
        {
            lastImage = listing.Images.FirstOrDefault(img => img.NextImageId == null);
        }

        // Crear la nueva imagen
        var newImage = new ListingImage(
            imageId: newImageId,
            url: storedImage.Url,
            thumbnailUrl: storedImage.ThumbnailUrl,
            altText: input.AltText,
            fileSize: input.FileSize,
            contentType: input.ContentType,
            previousImageId: lastImage?.ImageId,
            nextImageId: null
        );

        // Si hay una última imagen, actualizarla para que apunte a la nueva
        if (lastImage != null)
        {
            var updatedLastImage = lastImage.WithNextImage(newImageId);
            
            // Reemplazar la imagen en la colección (ValueObject es inmutable)
            var imagesList = listing.Images.ToList();
            var lastImageIndex = imagesList.FindIndex(img => img.ImageId == lastImage.ImageId);
            if (lastImageIndex >= 0)
            {
                imagesList[lastImageIndex] = updatedLastImage;
            }
            imagesList.Add(newImage);
            
            listing.Images = imagesList;
        }
        else
        {
            // Es la primera imagen
            listing.Images = new List<ListingImage> { newImage };
        }

        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);

        return new ListingImageDto
        {
            ImageId = newImage.ImageId,
            Url = newImage.Url,
            ThumbnailUrl = storedImage.ThumbnailUrl,
            AltText = newImage.AltText,
            PreviousImageId = newImage.PreviousImageId,
            NextImageId = newImage.NextImageId
        };
    }

    /// <summary>
    /// Elimina una imagen de una propiedad.
    /// Actualiza los punteros de la lista enlazada para mantener consistencia.
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveImageAsync(Guid listingId, Guid imageId)
    {
        var listing = await GetListingWithImagesAsync(listingId);

        // Validación de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes editar imágenes de tus propias propiedades");
        }

        var imageToRemove = listing.Images.FirstOrDefault(img => img.ImageId == imageId);
        if (imageToRemove == null)
        {
            throw new BusinessException("Listing:ImageNotFound")
                .WithData("ImageId", imageId)
                .WithData("ListingId", listingId);
        }

        var imageUrl = imageToRemove.Url;
        var imagesList = listing.Images.ToList();

        // Actualizar la imagen anterior para que apunte a la siguiente
        if (imageToRemove.PreviousImageId.HasValue)
        {
            var previousIndex = imagesList.FindIndex(img => img.ImageId == imageToRemove.PreviousImageId.Value);
            if (previousIndex >= 0)
            {
                imagesList[previousIndex] = imagesList[previousIndex].WithNextImage(imageToRemove.NextImageId);
            }
        }

        // Actualizar la imagen siguiente para que apunte a la anterior
        if (imageToRemove.NextImageId.HasValue)
        {
            var nextIndex = imagesList.FindIndex(img => img.ImageId == imageToRemove.NextImageId.Value);
            if (nextIndex >= 0)
            {
                imagesList[nextIndex] = imagesList[nextIndex].WithPreviousImage(imageToRemove.PreviousImageId);
            }
        }

        // Eliminar la imagen
        imagesList.RemoveAll(img => img.ImageId == imageId);
        
        listing.Images = imagesList;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);

        // Intentar eliminar el archivo f?sico (ignorar errores)
        await _imageStorageService.DeleteImageAsync(imageUrl);
    }

    /// <summary>
    /// Actualiza el orden de las imágenes reconstruyendo la lista enlazada.
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task UpdateImagesOrderAsync(Guid listingId, List<UpdateImageOrderDto> input)
    {
        if (input == null || !input.Any())
        {
            return;
        }

        var listing = await GetListingWithImagesAsync(listingId);

        // Validación de propiedad
        var architect = await _architectRepository.GetAsync(listing.ArchitectId);
        if (architect.UserId != CurrentUser.Id && !IsAdmin())
        {
            throw new AbpAuthorizationException("Solo puedes editar imágenes de tus propias propiedades");
        }

        // Ordenar por DisplayOrder del input
        var orderedInput = input.OrderBy(x => x.DisplayOrder).ToList();
        
        // Crear diccionario de imágenes existentes
        var existingImages = listing.Images.ToDictionary(img => img.ImageId);
        
        // Verificar que todas las imágenes del input existen
        foreach (var item in orderedInput)
        {
            if (!existingImages.ContainsKey(item.ImageId))
            {
                throw new BusinessException("Listing:ImageNotFound")
                    .WithData("ImageId", item.ImageId)
                    .WithData("ListingId", listingId);
            }
        }

        // Reconstruir la lista enlazada según el nuevo orden
        var newImagesList = new List<ListingImage>();
        
        for (int i = 0; i < orderedInput.Count; i++)
        {
            var currentImageId = orderedInput[i].ImageId;
            var originalImage = existingImages[currentImageId];
            
            Guid? previousId = i > 0 ? orderedInput[i - 1].ImageId : null;
            Guid? nextId = i < orderedInput.Count - 1 ? orderedInput[i + 1].ImageId : null;
            
            var reorderedImage = new ListingImage(
                imageId: originalImage.ImageId,
                url: originalImage.Url,
                thumbnailUrl: originalImage.ThumbnailUrl,
                altText: originalImage.AltText,
                fileSize: originalImage.FileSize,
                contentType: originalImage.ContentType,
                previousImageId: previousId,
                nextImageId: nextId
            );
            
            newImagesList.Add(reorderedImage);
        }

        listing.Images = newImagesList;
        listing.LastModifiedAt = Clock.Now;
        listing.LastModifiedBy = CurrentUser.Id;

        await _listingRepository.UpdateAsync(listing);
    }

    /// <summary>
    /// Búsqueda avanzada de propiedades con filtros
    /// Incluye Validación automática por DataAnnotations de ABP
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

        // Rango de área (usar LandArea para backward compatibility)
        if (searchDto.MinArea.HasValue)
        {   
            queryable = queryable.Where(p => p.LandArea >= searchDto.MinArea.Value);
        }
        if (searchDto.MaxArea.HasValue)
        {
            queryable = queryable.Where(p => p.LandArea <= searchDto.MaxArea.Value);
        }

        // Aplicar ordenamiento
        queryable = searchDto.SortBy?.ToLower() switch
        {
            "price-low" => queryable.OrderBy(p => p.Price),
            "price-high" => queryable.OrderByDescending(p => p.Price),
            "area-large" => queryable.OrderByDescending(p => p.LandArea),
            "area-small" => queryable.OrderBy(p => p.LandArea),
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

        // Sanitizar input
        searchTerm = searchTerm.Trim();

        var queryable = await _listingRepository.GetQueryableAsync();

        // Solo propiedades publicadas o en portafolio, con ubicación definida
        // Proyectar SOLO el campo Location para minimizar transferencia de datos
        var suggestions = await AsyncExecuter.ToListAsync(
            queryable
                .Where(p => 
                    (p.Status == ListingStatus.Published || p.Status == ListingStatus.Portfolio) &&
                    p.Location != null &&
                    p.Location.Contains(searchTerm))
                .GroupBy(p => p.Location!)
                .Select(g => new LocationSuggestionDto
                {
                    Location = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(s => s.Count)
                .ThenBy(s => s.Location)
                .Take(10)
        );

        return suggestions;
    }

    private async Task<Listing> GetListingWithImagesAsync(Guid listingId)
    {
        var queryable = await _listingRepository.WithDetailsAsync(listing => listing.Images!);

        var listing = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(l => l.Id == listingId));

        if (listing == null)
        {
            throw new EntityNotFoundException(typeof(Listing), listingId);
        }

        return listing;
    }

    private static string? GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null
        };
    }

    private async Task<UploadImageResult> StoreImageIfNeededAsync(CreateListingImageDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Url))
        {
            throw new BusinessException("Listing:ImageUrlRequired");
        }

        // Data URL desde el cliente: data:image/png;base64,...
        if (input.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var commaIndex = input.Url.IndexOf(',');
            if (commaIndex <= 0 || commaIndex >= input.Url.Length - 1)
            {
                throw new BusinessException("Listing:InvalidImageData");
            }

            var meta = input.Url.Substring("data:".Length, commaIndex - "data:".Length);
            var base64Data = input.Url[(commaIndex + 1)..];

            var contentType = input.ContentType;
            var semiIndex = meta.IndexOf(';');
            if (semiIndex > 0)
            {
                contentType = meta[..semiIndex];
            }
            contentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new BusinessException("Listing:InvalidImageData");
            }

            if (bytes.Length == 0)
            {
                throw new BusinessException("Listing:InvalidImageData");
            }

            // Límite server-side (5 MB)
            if (bytes.Length > 5 * 1024 * 1024)
            {
                throw new BusinessException("Listing:ImageTooLarge");
            }

            var extension = GetExtensionFromContentType(contentType);
            if (extension == null)
            {
                throw new BusinessException("Listing:UnsupportedImageType");
            }
            await using var stream = new MemoryStream(bytes);

            var fileName = $"{Guid.NewGuid()}{extension}";
            input.FileSize = bytes.Length;
            input.ContentType = contentType;

            if (!_imageStorageService.ValidateImage(fileName, bytes.Length))
            {
                throw new BusinessException("Listing:InvalidImageTypeOrSize");
            }

            return await _imageStorageService.UploadImageAsync(stream, fileName, "listings");
        }

        // URL ya alojada: se reutiliza como original y thumbnail
        return new UploadImageResult
        {
            Url = input.Url,
            ThumbnailUrl = input.Url
        };
    }
}

