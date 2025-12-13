using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Los eventos de dominio actualizan las estadística del arquitecto
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
    /// Verifica si el usuario currente es administrador (método sincrónico)
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
        var uploadedNewImage = false;

        try
        {
            // Validar ownership y duplicados antes de subir al storage para evitar copias huerfanas
            var listingSnapshot = await GetListingWithImagesNoTrackingAsync(listingId);
            await ValidateListingOwnershipAsync(listingSnapshot.ArchitectId, "editar imagenes de");

            var snapshotImages = listingSnapshot.Images ?? new List<ListingImage>();

            // 1. Verificar duplicados para URLs ya alojadas (sin subir nada todavia)
            if (!string.IsNullOrWhiteSpace(input.Url) &&
                !input.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var existingImageWithSameUrl = snapshotImages.FirstOrDefault(img => img.Url == input.Url);
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

            // 2. Subir la imagen SOLO una vez (para evitar duplicados en storage en caso de reintentos)
            storedImage = await StoreImageIfNeededAsync(input);
            uploadedNewImage = input.Url?.StartsWith("data:", StringComparison.OrdinalIgnoreCase) == true;

            // 3. Ejecutar la inserción en una unidad de trabajo aislada con reintentos
            return await ExecuteImageOperationAsync(
                listingId,
                "agregar imagen",
                async listing =>
                {
                    await ValidateListingOwnershipAsync(listing.ArchitectId, "editar imagenes de");

                    var listingImages = listing.Images ?? new List<ListingImage>();
                    if (listing.Images == null)
                    {
                        listing.Images = listingImages;
                    }

                    // Doble verificación con la URL final para evitar insertar duplicados
                    var existingWithStoredUrl = listingImages.FirstOrDefault(img => img.Url == storedImage!.Url);
                    if (existingWithStoredUrl != null)
                    {
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

                    // Agregar imagen al listing
                    var lastImage = GetTailImage(listingImages);
                    if (listingImages.Count(img => img.NextImageId == null) > 1)
                    {
                        Logger.LogWarning(
                            "Se detectaron multiples nodos finales en la cadena de imagenes del listing {ListingId}. Cadena actual: {Chain}",
                            listingId,
                            BuildImageChainDebugInfo(listingImages));
                    }
                    var newImageId = Guid.NewGuid();

                    lastImage?.UpdateNextImage(newImageId);

                    var newImage = new ListingImage(
                        imageId: newImageId,
                        url: storedImage!.Url,
                        thumbnailUrl: storedImage.ThumbnailUrl,
                        altText: input.AltText ?? string.Empty,
                        fileSize: input.FileSize,
                        contentType: input.ContentType,
                        previousImageId: lastImage?.ImageId,
                        nextImageId: null
                    );

                    listingImages.Add(newImage);

                    // listing.LastModifiedAt = Clock.Now;
                    // listing.LastModifiedBy = GetCurrentUserIdOrThrow();

                    Logger.LogDebug(
                        "Cadena de imagenes luego de agregar {ImageId} en listing {ListingId}: {Chain}",
                        newImageId,
                        listingId,
                        BuildImageChainDebugInfo(listingImages));

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
                uploadedNewImage)
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

                listingImages.Remove(imageToRemove);

                Logger.LogDebug(
                    "Cadena de imagenes luego de eliminar {ImageId} en listing {ListingId}: {Chain}",
                    imageId,
                    listingId,
                    BuildImageChainDebugInfo(listingImages));

                // listing.LastModifiedAt = Clock.Now;
                // listing.LastModifiedBy = GetCurrentUserIdOrThrow();

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

                var listingImages = listing.Images ?? new List<ListingImage>();
                if (listing.Images == null)
                {
                    listing.Images = listingImages;
                }

                var existingImages = listingImages.ToDictionary(img => img.ImageId);

                foreach (var item in orderedInput)
                {
                    if (!existingImages.ContainsKey(item.ImageId))
                    {
                        throw new BusinessException("Listing:ImageNotFound")
                            .WithData("ImageId", item.ImageId)
                            .WithData("ListingId", listingId);
                    }
                }

                for (int i = 0; i < orderedInput.Count; i++)
                {
                    var currentImageId = orderedInput[i].ImageId;
                    var imageToUpdate = existingImages[currentImageId];

                    Guid? previousId = i > 0 ? orderedInput[i - 1].ImageId : null;
                    Guid? nextId = i < orderedInput.Count - 1 ? orderedInput[i + 1].ImageId : null;

                    imageToUpdate.UpdateLinks(previousId, nextId);
                }

                listing.Images = orderedInput
                    .Select(item => existingImages[item.ImageId])
                    .ToList();
                // listing.LastModifiedAt = Clock.Now;
                // listing.LastModifiedBy = GetCurrentUserIdOrThrow();

                Logger.LogDebug(
                    "Cadena de imagenes luego de reordenar listing {ListingId}: {Chain}",
                    listingId,
                    BuildImageChainDebugInfo(listing.Images));

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
        var queryable = await _listingRepository.WithDetailsAsync(l => l.Images);
        var listing = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(l => l.Id == listingId));

        if (listing == null)
            throw new EntityNotFoundException(typeof(Listing), listingId);

        return listing;
    }

    private async Task<Listing> GetListingWithImagesNoTrackingAsync(Guid listingId)
    {
        using var uow = _unitOfWorkManager.Begin(isTransactional: false);

        var queryable = await _listingRepository.WithDetailsAsync(l => l.Images);

        var listing = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(l => l.Id == listingId));

        await uow.CompleteAsync();

        if (listing == null)
        {
            throw new EntityNotFoundException(typeof(Listing), listingId);
        }

        return listing;
    }

    /// <summary>
    /// Ejecuta una operacion de imagen dentro del contexto del lock de aplicación existente.
    /// El lock ya debe haber sido adquirido antes de llamar a este metodo.
    /// Se ejecuta directamente en el UnitOfWork ambient sin crear contextos adicionales.
    /// </summary>
    private async Task<T> ExecuteImageOperationAsync<T>(Guid listingId, string operationName, Func<Listing, Task<T>> operation)
    {
        try
        {
            // Cargar la entidad directamente en el UnitOfWork ambient
            var listing = await GetListingWithImagesAsync(listingId);

            // Ejecutar la operación
            var result = await operation(listing);

            // El cambio se guarda automáticamente al salir del UnitOfWork ambient
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error al {Operation} del listing {ListingId}. Cadena de errores: {ExceptionChain}",
                operationName,
                listingId,
                BuildExceptionChain(ex));

            throw;
        }
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

    private static string BuildExceptionChain(Exception ex)
    {
        var builder = new StringBuilder();
        var current = ex;
        var level = 0;

        while (current != null)
        {
            if (builder.Length > 0)
            {
                builder.Append(" --> ");
            }

            builder.Append('[')
                .Append(level)
                .Append(']')
                .Append(' ')
                .Append(current.GetType().FullName)
                .Append(':')
                .Append(' ')
                .Append(current.Message);

            current = current.InnerException;
            level++;
        }

        return builder.ToString();
    }

    private static string BuildConcurrencyDetails(Exception ex)
    {
        var dbConcurrencyException = FindDbConcurrencyException(ex);
        if (dbConcurrencyException == null)
        {
            return "No se encontro DbUpdateConcurrencyException en la cadena de errores.";
        }

        var builder = new StringBuilder();

        var entries = GetPropertyValue<IEnumerable?>(dbConcurrencyException, "Entries");
        if (entries == null)
        {
            return "DbUpdateConcurrencyException sin entradas en seguimiento.";
        }

        foreach (var entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(" | ");
            }

            var metadata = GetPropertyValue<object?>(entry, "Metadata");
            var entityName = GetPropertyValue<string?>(metadata, "Name") ?? "(desconocido)";
            var state = GetPropertyValue<object?>(entry, "State")?.ToString() ?? "(desconocido)";

            builder.Append("Entity=")
                .Append(entityName)
                .Append(", State=")
                .Append(state);

            var primaryKey = InvokeMethod(metadata, "FindPrimaryKey", null);
            var properties = GetPropertyValue<IEnumerable?>(primaryKey, "Properties");
            if (properties != null)
            {
                var keys = new List<string>();
                foreach (var prop in properties)
                {
                    if (prop == null)
                    {
                        continue;
                    }

                    var propName = GetPropertyValue<string?>(prop, "Name") ?? "(sin nombre)";
                    var propertyEntry = InvokeMethod(entry, "Property", new object?[] { propName });
                    var currentValue = GetPropertyValue<object?>(propertyEntry, "CurrentValue") ?? "null";
                    keys.Add($"{propName}={currentValue}");
                }

                if (keys.Count > 0)
                {
                    builder.Append(", Keys=[")
                        .Append(string.Join(", ", keys))
                        .Append(']');
                }
            }

            var metadataProperties = InvokeMethod(metadata, "GetProperties", null) as IEnumerable;
            if (metadataProperties != null)
            {
                var concurrencyTokens = new List<string>();
                foreach (var metaProp in metadataProperties)
                {
                    if (metaProp == null)
                    {
                        continue;
                    }

                    var isConcurrency = GetPropertyValue<bool?>(metaProp, "IsConcurrencyToken") == true;
                    if (!isConcurrency)
                    {
                        continue;
                    }

                    var propName = GetPropertyValue<string?>(metaProp, "Name") ?? "(sin nombre)";
                    var propertyEntry = InvokeMethod(entry, "Property", new object?[] { propName });
                    var originalValue = GetPropertyValue<object?>(propertyEntry, "OriginalValue") ?? "null";
                    var currentValue = GetPropertyValue<object?>(propertyEntry, "CurrentValue") ?? "null";
                    concurrencyTokens.Add($"{propName}: Original={originalValue}, Current={currentValue}");
                }

                if (concurrencyTokens.Count > 0)
                {
                    builder.Append(", Concurrency=[")
                        .Append(string.Join(", ", concurrencyTokens))
                        .Append(']');
                }
            }
        }

        return builder.Length == 0
            ? "DbUpdateConcurrencyException sin entradas en seguimiento."
            : builder.ToString();
    }

    private static Exception? FindDbConcurrencyException(Exception ex)
    {
        var current = ex;
        while (current != null)
        {
            if (string.Equals(current.GetType().Name, "DbUpdateConcurrencyException", StringComparison.Ordinal))
            {
                return current;
            }

            current = current.InnerException;
        }

        return null;
    }

    private static object? InvokeMethod(object? instance, string methodName, object?[]? parameters)
    {
        if (instance == null)
        {
            return null;
        }

        var method = instance.GetType().GetMethod(methodName);
        return method?.Invoke(instance, parameters);
    }

    private static T? GetPropertyValue<T>(object? instance, string propertyName)
    {
        if (instance == null)
        {
            return default;
        }

        var property = instance.GetType().GetProperty(propertyName);
        if (property == null)
        {
            return default;
        }

        var value = property.GetValue(instance);
        if (value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    private static ListingImage? GetTailImage(IEnumerable<ListingImage> images)
    {
        var imageList = images as IList<ListingImage> ?? images.ToList();

        if (imageList.Count == 0)
        {
            return null;
        }

        var tails = imageList.Where(img => img.NextImageId == null).ToList();
        if (tails.Count == 0)
        {
            return null;
        }

        return tails.Count == 1
            ? tails[0]
            : tails.OrderBy(t => t.ImageId).First();
    }

    private static IReadOnlyList<ListingImage> OrderImages(IEnumerable<ListingImage> images)
    {
        var imageDict = images.ToDictionary(img => img.ImageId, img => img);
        var ordered = new List<ListingImage>();

        var head = imageDict.Values.FirstOrDefault(img => !img.PreviousImageId.HasValue) ??
                   imageDict.Values.FirstOrDefault();

        var visited = new HashSet<Guid>();
        var current = head;

        while (current != null && visited.Add(current.ImageId))
        {
            ordered.Add(current);

            if (current.NextImageId.HasValue &&
                imageDict.TryGetValue(current.NextImageId.Value, out var next))
            {
                current = next;
            }
            else
            {
                current = null;
            }
        }

        foreach (var orphan in imageDict.Values.Where(img => !visited.Contains(img.ImageId)))
        {
            ordered.Add(orphan);
        }

        return ordered;
    }

    private async Task<string> BuildListingImagesDebugSnapshotAsync(Guid listingId)
    {
        try
        {
            using var uow = _unitOfWorkManager.Begin(isTransactional: false);

            var queryable = await _listingRepository.WithDetailsAsync(l => l.Images);
            var listing = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(l => l.Id == listingId));

            await uow.CompleteAsync();

            if (listing == null)
            {
                return "listing no encontrado";
            }

            var images = listing.Images?.ToList() ?? new List<ListingImage>();
            var duplicates = images
                .GroupBy(i => i.ImageId)
                .Where(g => g.Count() > 1)
                .Select(g => $"{ShortId(g.Key)}x{g.Count()}");

            return $"imgs:{images.Count}, dup:[{string.Join(",", duplicates)}], cadena:{BuildImageChainDebugInfo(images)}";
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "No se pudo construir snapshot de imagenes para listing {ListingId}", listingId);
            return "snapshot no disponible";
        }
    }

    private static string BuildImageChainDebugInfo(IEnumerable<ListingImage> images)
    {
        var ordered = OrderImages(images);
        return ordered.Count == 0
            ? "(sin imagenes)"
            : string.Join(" -> ", ordered.Select(img =>
                $"{ShortId(img.ImageId)}(p:{ShortId(img.PreviousImageId)},n:{ShortId(img.NextImageId)})"));
    }

    private static string ShortId(Guid? id)
    {
        return id.HasValue ? id.Value.ToString("N").Substring(0, 8) : "null";
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

            var validationResult = _imageStorageService.ValidateImage(fileName, bytes.Length);
            if (!validationResult.IsValid)
            {
                throw new BusinessException("Listing:InvalidImageTypeOrSize")
                    .WithData("Reason", validationResult.Message ?? "Validación de imagen fallida")
                    .WithData("Severity", validationResult.Severity.ToString())
                    .WithData("ErrorCode", validationResult.ErrorCode ?? "UnknownValidationError");
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


