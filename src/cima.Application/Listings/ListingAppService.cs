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
using Volo.Abp.Uow;


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
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IListingImageLockService _listingImageLockService;
    private const string FeaturedListingsCacheKey = "FeaturedListingsForHomepage";

    public ListingAppService(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IDistributedCache distributedCache,
        IListingManager listingManager,
        Images.IImageStorageService imageStorageService,
        IUnitOfWorkManager unitOfWorkManager,
        IListingImageLockService listingImageLockService)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _distributedCache = distributedCache;
        _listingManager = listingManager;
        _imageStorageService = imageStorageService;
        _unitOfWorkManager = unitOfWorkManager;
        _listingImageLockService = listingImageLockService;
    }

    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    public async Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving (WithDetailsAsync garantiza carga)
            listing => listing.Images!);    // ? null-forgiving

        // Aplicar filtro de status
        if (input.Status.HasValue)
        {
            queryable = queryable.Where(p => (int)p.Status == input.Status.Value);
        }

        // Aplicar filtro de arquitecto
        if (input.ArchitectId.HasValue)
        {
            queryable = queryable.Where(p => p.ArchitectId == input.ArchitectId.Value);
        }

        // Aplicar filtros comunes
        queryable = ApplyListingFilters(queryable, input);

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
    /// Aplica filtros comunes a las consultas de listings
    /// </summary>
    private IQueryable<Listing> ApplyListingFilters(IQueryable<Listing> queryable, GetListingsInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                (p.Location != null && p.Location.Contains(input.SearchTerm)) ||
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

        return queryable;
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
            createdBy: GetCurrentUserIdOrThrow());

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
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "editar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

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
        listing.LastModifiedBy = GetCurrentUserIdOrThrow();

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
        await ValidateListingOwnershipAsync(listing.ArchitectId, "eliminar");

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
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "publicar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

        // Warning si no tiene imagenes (pero permite publicar)
        if (listing.Images == null || !listing.Images.Any())
        {
            Logger.LogWarning("Publicando propiedad {ListingId} sin imágenes", id);
        }

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.PublishAsync(listing, GetCurrentUserIdOrThrow());

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
        await ValidateListingOwnershipAsync(listing.ArchitectId, "archivar");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.ArchiveAsync(listing, GetCurrentUserIdOrThrow());

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
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "desarchivar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnarchiveAsync(listing, GetCurrentUserIdOrThrow());

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
        await ValidateListingOwnershipAsync(listing.ArchitectId, "despublicar");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnpublishAsync(listing, GetCurrentUserIdOrThrow());

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
        await ValidateListingOwnershipAsync(listing.ArchitectId, "mover a portafolio");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.MoveToPortfolioAsync(listing, GetCurrentUserIdOrThrow());

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
        await ValidateListingOwnershipAsync(original.ArchitectId, "duplicar");

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
            createdBy: GetCurrentUserIdOrThrow());

        await _listingRepository.InsertAsync(duplicatedListing);
        
        Logger.LogInformation(
            "Propiedad {OriginalId} duplicada a {NewId} por usuario {UserId}",
            id, duplicatedListing.Id, GetCurrentUserIdOrThrow());

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

        // Aplicar filtros comunes
        queryable = ApplyListingFilters(queryable, input);

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
    /// Valida que el usuario actual sea el propietario del listing o un administrador
    /// </summary>
    private async Task<Architect> ValidateListingOwnershipAsync(Guid architectId, string operationName)
    {
        var architect = await _architectRepository.GetAsync(architectId);
        var currentUserId = GetCurrentUserIdOrThrow();
        if (architect.UserId != currentUserId && !IsAdmin())
        {
            throw new AbpAuthorizationException($"Solo puedes {operationName} tus propias propiedades");
        }
        return architect;
    }

    /// <summary>
    /// Valida que el arquitecto esté activo
    /// </summary>
    private void ValidateArchitectIsActive(Architect architect)
    {
        if (!architect.IsActive)
        {
            throw new BusinessException(cimaDomainErrorCodes.ArchitectInactive)
                .WithData("ArchitectId", architect.Id);
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual o lanza excepción si no está autenticado
    /// </summary>
    private Guid GetCurrentUserIdOrThrow()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Usuario no autenticado");
        }
        return CurrentUser.Id.Value;
    }

    

    
    /// <summary>
    /// Agrega una imagen a una propiedad usando lista enlazada.
    /// La nueva imagen se agrega al final de la galeria.
    /// 
    /// Flujo optimizado:
    /// 1. Adquirir lock exclusivo por listingId
    /// 2. Validar ownership y verificar duplicados ANTES de subir al storage
    /// 3. Solo subir al storage si pasa validaciones
    /// 4. Agregar imagen al listing
    /// 5. Si falla despues de subir, limpiar blob huerfano
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingImageDto> AddImageAsync(Guid listingId, CreateListingImageDto input)
    {
        // Adquirir lock exclusivo para este listing - serializa todas las operaciones de imagen
        await using var lockHandle = await _listingImageLockService.AcquireAsync(listingId);

        UploadImageResult? storedImage = null;

        try
        {
            return await ExecuteImageOperationAsync(
                listingId,
                "agregar imagen",
                async listing =>
                {
                    // 1. Validar ownership PRIMERO (antes de cualquier operacion costosa)
                    await ValidateListingOwnershipAsync(listing.ArchitectId, "editar imagenes de");

                    var listingImages = listing.Images ?? new List<ListingImage>();
                    if (listing.Images == null)
                    {
                        listing.Images = listingImages;
                    }

                    // 2. Verificar duplicados ANTES de subir al storage
                    // Para data URLs, no podemos verificar duplicados hasta subir,
                    // pero para URLs ya alojadas, podemos verificar directamente
                    if (!string.IsNullOrWhiteSpace(input.Url) && 
                        !input.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var existingImageWithSameUrl = listingImages.FirstOrDefault(img => img.Url == input.Url);
                        if (existingImageWithSameUrl != null)
                        {
                            return new ListingImageDto
                            {
                                ImageId = existingImageWithSameUrl.ImageId,
                                Url = existingImageWithSameUrl.Url,
                                ThumbnailUrl = existingImageWithSameUrl.ThumbnailUrl,
                                AltText = existingImageWithSameUrl.AltText,
                                PreviousImageId = existingImageWithSameUrl.PreviousImageId,
                                NextImageId = existingImageWithSameUrl.NextImageId
                            };
                        }
                    }

                    // 3. Ahora si, subir la imagen al storage (solo despues de validaciones)
                    storedImage = await StoreImageIfNeededAsync(input);

                    // 4. Verificar duplicados nuevamente con la URL final del storage
                    var existingWithStoredUrl = listingImages.FirstOrDefault(img => img.Url == storedImage.Url);
                    if (existingWithStoredUrl != null)
                    {
                        // Ya existe - eliminar la copia que acabamos de subir
                        if (input.Url?.StartsWith("data:", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Solo eliminar si fue una subida nueva (no una URL reutilizada)
                            await _imageStorageService.DeleteImageAsync(storedImage.Url);
                        }
                        return new ListingImageDto
                        {
                            ImageId = existingWithStoredUrl.ImageId,
                            Url = existingWithStoredUrl.Url,
                            ThumbnailUrl = existingWithStoredUrl.ThumbnailUrl,
                            AltText = existingWithStoredUrl.AltText,
                            PreviousImageId = existingWithStoredUrl.PreviousImageId,
                            NextImageId = existingWithStoredUrl.NextImageId
                        };
                    }

                    // 5. Agregar imagen al listing
                    var lastImage = listingImages.FirstOrDefault(img => img.NextImageId == null);
                    var newImageId = Guid.NewGuid();

                    lastImage?.UpdateNextImage(newImageId);

                    var newImage = new ListingImage(
                        imageId: newImageId,
                        url: storedImage.Url,
                        thumbnailUrl: storedImage.ThumbnailUrl,
                        altText: input.AltText ?? string.Empty,
                        fileSize: input.FileSize,
                        contentType: input.ContentType,
                        previousImageId: lastImage?.ImageId,
                        nextImageId: null
                    );

                    listingImages.Add(newImage);

                    listing.LastModifiedAt = Clock.Now;
                    listing.LastModifiedBy = GetCurrentUserIdOrThrow();

                    return new ListingImageDto
                    {
                        ImageId = newImage.ImageId,
                        Url = newImage.Url,
                        ThumbnailUrl = storedImage.ThumbnailUrl,
                        AltText = newImage.AltText,
                        PreviousImageId = newImage.PreviousImageId,
                        NextImageId = newImage.NextImageId
                    };
                });
        }
        catch
        {
            // Limpiar blob huerfano si ya se subio pero fallo despues
            if (storedImage != null && 
                !string.IsNullOrWhiteSpace(storedImage.Url) &&
                input.Url?.StartsWith("data:", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    await _imageStorageService.DeleteImageAsync(storedImage.Url);
                }
                catch (Exception cleanupEx)
                {
                    Logger.LogWarning(cleanupEx, "No se pudo limpiar imagen huerfana {Url}", storedImage.Url);
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Elimina una imagen de una propiedad.
    /// Actualiza los punteros de la lista enlazada para mantener consistencia.
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveImageAsync(Guid listingId, Guid imageId)
    {
        // Adquirir lock exclusivo para este listing - serializa todas las operaciones de imagen
        await using var lockHandle = await _listingImageLockService.AcquireAsync(listingId);

        string? imageUrl = null;

        await ExecuteImageOperationAsync(
            listingId,
            "eliminar imagen",
            async listing =>
            {
                await ValidateListingOwnershipAsync(listing.ArchitectId, "editar imagenes de");

                var listingImages = listing.Images ?? new List<ListingImage>();
                if (listing.Images == null)
                {
                    listing.Images = listingImages;
                }

                var imageToRemove = listingImages.FirstOrDefault(img => img.ImageId == imageId);
                if (imageToRemove == null)
                {
                    throw new BusinessException("Listing:ImageNotFound")
                        .WithData("ImageId", imageId)
                        .WithData("ListingId", listingId);
                }

                imageUrl = imageToRemove.Url;

                if (imageToRemove.PreviousImageId.HasValue)
                {
                    var previousImage = listingImages.FirstOrDefault(img => img.ImageId == imageToRemove.PreviousImageId.Value);
                    previousImage?.UpdateNextImage(imageToRemove.NextImageId);
                }

                if (imageToRemove.NextImageId.HasValue)
                {
                    var nextImage = listingImages.FirstOrDefault(img => img.ImageId == imageToRemove.NextImageId.Value);
                    nextImage?.UpdatePreviousImage(imageToRemove.PreviousImageId);
                }

                listing.Images = listingImages
                    .Where(img => img.ImageId != imageId)
                    .ToList();

                listing.LastModifiedAt = Clock.Now;
                listing.LastModifiedBy = GetCurrentUserIdOrThrow();

                await Task.CompletedTask;
            });

        if (!string.IsNullOrEmpty(imageUrl))
        {
            await _imageStorageService.DeleteImageAsync(imageUrl);
        }
    }

    /// <summary>
    /// Actualiza el orden de las imagenes reconstruyendo la lista enlazada.
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task UpdateImagesOrderAsync(Guid listingId, List<UpdateImageOrderDto> input)
    {
        if (input == null || !input.Any())
        {
            return;
        }

        // Adquirir lock exclusivo para este listing - serializa todas las operaciones de imagen
        await using var lockHandle = await _listingImageLockService.AcquireAsync(listingId);

        await ExecuteImageOperationAsync(
            listingId,
            "reordenar imagenes",
            async listing =>
            {
                await ValidateListingOwnershipAsync(listing.ArchitectId, "editar imagenes de");

                var orderedInput = input.OrderBy(x => x.DisplayOrder).ToList();

                var existingImages = (listing.Images ?? new List<ListingImage>()).ToDictionary(img => img.ImageId);

                foreach (var item in orderedInput)
                {
                    if (!existingImages.ContainsKey(item.ImageId))
                    {
                        throw new BusinessException("Listing:ImageNotFound")
                            .WithData("ImageId", item.ImageId)
                            .WithData("ListingId", listingId);
                    }
                }

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
                listing.LastModifiedBy = GetCurrentUserIdOrThrow();

                await Task.CompletedTask;
            });
    }

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
        // Usar WithDetailsAsync para incluir explícitamente las Images (owned entities)
        var queryable = await _listingRepository.WithDetailsAsync(l => l.Images);

        var listing = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(l => l.Id == listingId));

        if (listing == null)
        {
            throw new EntityNotFoundException(typeof(Listing), listingId);
        }

        return listing;
    }

    
        
    /// <summary>
    /// Ejecuta una operacion de imagen con reintentos en caso de conflictos de concurrencia.
    /// IMPORTANTE: El lock ya debe haber sido adquirido antes de llamar a este metodo.
    /// Los reintentos solo sirven como fallback para casos edge (ej: otras operaciones no de imagen).
    /// </summary>
    private async Task<T> ExecuteImageOperationAsync<T>(Guid listingId, string operationName, Func<Listing, Task<T>> operation)
    {
        const int maxRetries = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);

            try
            {
                var listing = await GetListingWithImagesAsync(listingId);
                var result = await operation(listing);

                await uow.CompleteAsync();
                return result;
            }
            catch (Exception ex) when (IsConcurrencyException(ex))
            {
                lastException = ex;
                Logger.LogWarning(ex, "Conflicto de concurrencia al {Operation} del listing {ListingId}. Intento {Attempt}/{MaxRetries}", operationName, listingId, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    break;
                }

                await Task.Delay(100 * attempt);
            }
        }

        Logger.LogError(lastException, "Fallo {Operation} despues de {MaxRetries} intentos por conflictos de concurrencia", operationName, maxRetries);

        throw new BusinessException("Listing:ConcurrencyConflict")
            .WithData("ListingId", listingId)
            .WithData("Message", $"No se pudo {operationName} debido a actualizaciones concurrentes. Por favor, recargue la pagina e intente nuevamente.");
    }

    private Task ExecuteImageOperationAsync(Guid listingId, string operationName, Func<Listing, Task> operation)
    {
        return ExecuteImageOperationAsync<object>(
            listingId,
            operationName,
            async listing =>
            {
                await operation(listing);
                return default!;
            });
    }

    /// <summary>
    /// Verifica si una excepcion es un error de concurrencia optimista o tracking de Entity Framework
    /// </summary>
    private static bool IsConcurrencyException(Exception ex)
    {
        // Verificar el tipo por nombre (para evitar dependencia directa de EF Core)
        var typeName = ex.GetType().Name;
        if (typeName == "DbUpdateConcurrencyException" || typeName == "InvalidOperationException")
        {
            // Verificar que sea realmente un error de tracking/concurrencia por el mensaje
            if (ex.Message.Contains("cannot be tracked", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("already being tracked", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("expected to affect 1 row(s), but actually affected 0 row(s)", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        // Verificar mensaje de excepcion directamente (fallback)
        if (ex.Message.Contains("cannot be tracked", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("already being tracked", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("expected to affect 1 row(s), but actually affected 0 row(s)", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Verificar inner exception recursivamente
        if (ex.InnerException != null)
        {
            return IsConcurrencyException(ex.InnerException);
        }
        
        return false;
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


