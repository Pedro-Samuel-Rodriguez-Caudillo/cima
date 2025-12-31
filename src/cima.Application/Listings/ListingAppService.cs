using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cima.Architects;
using cima.Domain.Entities;
using cima.Domain.Listings;
using cima.Domain.Services.Listings;
using cima.Domain.Shared;
using cima.Events;
using cima.Images;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using cima.Listings.Inputs;
using cima.Listings.Outputs;


namespace cima.Listings;

/// <summary>
/// Implementacion del servicio de propiedades inmobiliarias
/// </summary>
[Authorize(cimaPermissions.Listings.Default)]
public class ListingAppService : cimaAppService, IListingAppService
{
    private readonly IListingRepository _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IDistributedCache _distributedCache;
    private readonly IRepository<FeaturedListing, Guid> _featuredListingRepository;
    private readonly IListingManager _listingManager;
    private readonly Images.IImageStorageService _imageStorageService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IListingImageLockService _listingImageLockService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILocalEventBus _localEventBus;
    private readonly IListingPriceHistoryRepository _priceHistoryRepository;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
    private const string FeaturedListingsCacheKey = "FeaturedListingsForHomepage";

    public ListingAppService(
        IListingRepository listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IRepository<FeaturedListing, Guid> featuredListingRepository,
        IDistributedCache distributedCache,
        IListingManager listingManager,
        Images.IImageStorageService imageStorageService,
        IUnitOfWorkManager unitOfWorkManager,
        IListingImageLockService listingImageLockService,
        IHostEnvironment hostEnvironment,
        ILocalEventBus localEventBus,
        IListingPriceHistoryRepository priceHistoryRepository,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _featuredListingRepository = featuredListingRepository;
        _distributedCache = distributedCache;
        _listingManager = listingManager;
        _imageStorageService = imageStorageService;
        _unitOfWorkManager = unitOfWorkManager;
        _listingImageLockService = listingImageLockService;
        _hostEnvironment = hostEnvironment;
        _localEventBus = localEventBus;
        _priceHistoryRepository = priceHistoryRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    public async Task<PagedResultDto<ListingSummaryDto>> GetListAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Architect!,  // ? null-forgiving (WithDetailsAsync garantiza carga)
            listing => listing.Images!);    // ? null-forgiving

        if (input.FeaturedOnly == true)
        {
            var featuredQueryable = await _featuredListingRepository.GetQueryableAsync();
            queryable = from listing in queryable
                join featured in featuredQueryable on listing.Id equals featured.ListingId
                select listing;
        }

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

        return new PagedResultDto<ListingSummaryDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingSummaryDto>>(listings)
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
                (p.Location != null && p.Location.Value.Contains(input.SearchTerm)) ||
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
    /// <summary>
    /// Obtiene detalle de una propiedad por Id
    /// </summary>
    [AllowAnonymous]
    public async Task<ListingDetailDto> GetAsync(Guid id)
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

        // Si no está publicada ni en portafolio, validar permisos (solo dueño o admin)
        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                throw new EntityNotFoundException(typeof(Listing), id);
            }

            var currentUserId = CurrentUser.Id!.Value;
            var architect = await _architectRepository.GetAsync(listing.ArchitectId);
            
            if (architect.UserId != currentUserId && !IsAdmin())
            {
                throw new EntityNotFoundException(typeof(Listing), id);
            }
        }

        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Crea nueva propiedad en estado Draft usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDetailDto> CreateAsync(CreateListingDto input)
    {
        var resolvedPrice = ResolvePrice(input.IsPriceOnRequest, input.Price);

        // Usar ListingManager para crear con validaciones y eventos de dominio
        var listing = await _listingManager.CreateAsync(
            title: input.Title.Trim(),
            description: input.Description.Trim(),
            location: input.Address?.Value.Trim(),
            price: resolvedPrice,
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
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Actualiza propiedad existente con validaciones del ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDetailDto> UpdateAsync(Guid id, UpdateListingDto input)
    {
        if (id != input.Id) throw new UserFriendlyException("ID mismatch");

        var listing = await _listingRepository.GetAsync(id);

        // Capturar precio anterior para detectar cambios
        var oldPrice = listing.Price;
        var resolvedPrice = ResolvePrice(input.IsPriceOnRequest, input.Price);

        // Validacion: Solo el dueno o admin puede editar
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "editar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

        // Usar ListingManager para actualizar (encapsula reglas de negocio)
        await _listingManager.UpdateAsync(
            listing,
            input.Title.Trim(),
            input.Description.Trim(),
            input.Address?.Value.Trim(),
            resolvedPrice,
            input.LandArea,
            input.ConstructionArea,
            input.Bedrooms,
            input.Bathrooms,
            input.Category,
            input.Type,
            input.TransactionType,
            GetCurrentUserIdOrThrow()
        );

        await _listingRepository.UpdateAsync(listing);

        // Si el precio cambió, registrar en historial con hash chain anti-tampering
        if (oldPrice != resolvedPrice)
        {
            await RecordPriceChangeAsync(listing.Id, oldPrice, resolvedPrice);
        }
        
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }
    
    /// <summary>
    /// Registra un cambio de precio con metadatos anti-fraude y hash chain
    /// </summary>
    private async Task RecordPriceChangeAsync(Guid listingId, decimal oldPrice, decimal newPrice)
    {
        // Obtener el último registro para encadenar hashes (blockchain-lite)
        var previousRecords = await _priceHistoryRepository.GetByListingIdAsync(listingId);
        var lastRecord = previousRecords.FirstOrDefault();
        var previousHash = lastRecord?.IntegrityHash;
        
        // Capturar metadatos anti-fraude del contexto HTTP
        var httpContext = _httpContextAccessor.HttpContext;
        var clientIp = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
        var correlationId = httpContext?.TraceIdentifier;
        var sessionId = httpContext?.Session?.Id;
        var authMethod = httpContext?.User?.Identity?.AuthenticationType;
        
        var priceHistory = new Domain.Entities.Listings.ListingPriceHistory(
            GuidGenerator.Create(),
            listingId,
            oldPrice,
            newPrice,
            CurrentUser.Id,
            CurrentUser.UserName,
            clientIp,
            userAgent,
            correlationId,
            changeReason: null, // Podría agregarse como parámetro opcional
            sessionId,
            authMethod,
            previousHash // Hash del registro anterior para cadena de integridad
        );
        
        await _priceHistoryRepository.InsertAsync(priceHistory);
        
        Logger.LogInformation(
            "Price changed for Listing {ListingId}: {OldPrice} -> {NewPrice} by User {UserId} from IP {ClientIp}",
            listingId, oldPrice, newPrice, CurrentUser.Id, clientIp);
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
    public async Task<ListingDetailDto> PublishAsync(PublishListingDto input)
    {
        var query = await _listingRepository.WithDetailsAsync(l => l.Images); // Cargar imagenes para validar regla
        var listing = await AsyncExecuter.FirstOrDefaultAsync(query.Where(l => l.Id == input.ListingId));
        
        if (listing == null) throw new EntityNotFoundException(typeof(Listing), input.ListingId);

        // Validacion de propiedad del listing
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "publicar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.PublishAsync(listing, GetCurrentUserIdOrThrow());

        await _listingRepository.UpdateAsync(listing);

        // Publicar evento de aplicacion para handlers adicionales
        await _localEventBus.PublishAsync(new ListingPublishedEto
        {
            ListingId = listing.Id,
            ArchitectId = listing.ArchitectId,
            Title = listing.Title,
            PublishedAt = Clock.Now
        });

        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Cambia estado a Archived usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Archive)]
    public async Task<ListingDetailDto> ArchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        await ValidateListingOwnershipAsync(listing.ArchitectId, "archivar");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.ArchiveAsync(listing, GetCurrentUserIdOrThrow());

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Reactiva una propiedad archivada a estado Published usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Publish)]
    public async Task<ListingDetailDto> UnarchiveAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        var architect = await ValidateListingOwnershipAsync(listing.ArchitectId, "desarchivar");

        // Validar que el arquitecto está activo
        ValidateArchitectIsActive(architect);

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnarchiveAsync(listing, GetCurrentUserIdOrThrow());

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Cambia una propiedad publicada de vuelta a Draft usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDetailDto> UnpublishAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        await ValidateListingOwnershipAsync(listing.ArchitectId, "despublicar");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.UnpublishAsync(listing, GetCurrentUserIdOrThrow());

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Cambia una propiedad a estado Portfolio usando ListingManager
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingDetailDto> MoveToPortfolioAsync(Guid id)
    {
        var listing = await _listingRepository.GetAsync(id);

        // Validacion de propiedad
        await ValidateListingOwnershipAsync(listing.ArchitectId, "mover a portafolio");

        // Usar ListingManager - dispara eventos de dominio
        await _listingManager.MoveToPortfolioAsync(listing, GetCurrentUserIdOrThrow());

        await _listingRepository.UpdateAsync(listing);
        return ObjectMapper.Map<Listing, ListingDetailDto>(listing);
    }

    /// <summary>
    /// Duplica una propiedad existente creando una copia en estado Draft
    /// No copia las imágenes, solo los datos de la propiedad
    /// </summary>
    [Authorize(cimaPermissions.Listings.Create)]
    public async Task<ListingDetailDto> DuplicateAsync(Guid id)
    {
        var original = await _listingRepository.GetAsync(id);

        // Validacion: Solo el dueño o admin puede duplicar
        await ValidateListingOwnershipAsync(original.ArchitectId, "duplicar");

        // Crear nueva propiedad usando ListingManager
        var duplicatedListing = await _listingManager.CreateAsync(
            title: $"{original.Title} (Copia)",
            description: original.Description,
            location: original.Location?.Value, // Extraer valor de VO
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

        return ObjectMapper.Map<Listing, ListingDetailDto>(duplicatedListing);
    }

    /// <summary>
    /// Obtiene solo propiedades publicadas (público - sin autenticación)
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ListingSummaryDto>> GetPublishedAsync(GetListingsInput input)
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

        return new PagedResultDto<ListingSummaryDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingSummaryDto>>(listings)
        );
    }

    /// <summary>
    /// Obtiene propiedades de un arquitecto especifico
    /// </summary>
    public async Task<PagedResultDto<ListingSummaryDto>> GetByArchitectAsync(
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

        return new PagedResultDto<ListingSummaryDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingSummaryDto>>(listings)
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
    /// <summary>
    /// Agrega una imagen a un listing usando metodo de dominio
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingImageDto> AddImageAsync(AddListingImageDto input)
    {
        // 1. Subir la imagen al storage (valida y/o sube)
        var uploadedImage = await StoreImageIfNeededAsync(input);
        
        // 2. Usar repositorio para obtener la entidad
        var query = await _listingRepository.WithDetailsAsync(x => x.Images);
        var listing = query.Where(x => x.Id == input.ListingId).FirstOrDefault(); 
        
        if (listing == null) throw new EntityNotFoundException(typeof(Listing), input.ListingId);

        // Validar ownership
        await ValidateListingOwnershipAsync(listing.ArchitectId, "editar imagenes de");

        // 3. Agregar usando metodo de dominio
        listing.AddImage(
            GuidGenerator.Create(),
            uploadedImage.Url,
            uploadedImage.ThumbnailUrl ?? uploadedImage.Url, // Fallback si thumb es null
            input.AltText ?? string.Empty,
            input.FileSize,
            input.ContentType
        );

        // 4. Guardar cambios
        await _listingRepository.UpdateAsync(listing);
        
        var newImage = listing.Images.Last(); 

        return new ListingImageDto
        {
            ImageId = newImage.ImageId,
            Url = newImage.Url,
            ThumbnailUrl = newImage.ThumbnailUrl,
            AltText = newImage.AltText,
            SortOrder = newImage.SortOrder
        };
    }



    private async Task SafeDeleteUploadedImageAsync(bool uploadedNewImage, UploadImageResult? storedImage)
    {
        if (!uploadedNewImage || storedImage == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(storedImage.Url))
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

        if (!string.IsNullOrWhiteSpace(storedImage.ThumbnailUrl) &&
            !string.Equals(storedImage.ThumbnailUrl, storedImage.Url, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _imageStorageService.DeleteImageAsync(storedImage.ThumbnailUrl);
            }
            catch (Exception cleanupEx)
            {
                Logger.LogWarning(cleanupEx, "No se pudo limpiar thumbnail huerfano {Url}", storedImage.ThumbnailUrl);
            }
        }
    }

    /// <summary>
    /// Elimina una imagen de una propiedad.
    /// Actualiza los punteros de la lista enlazada para mantener consistencia.
    /// </summary>
    /// <summary>
    /// Elimina una imagen usando metodo de dominio
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveImageAsync(Guid listingId, Guid imageId)
    {
        var listing = await _listingRepository.WithDetailsAsync(x => x.Images);
        var entity = listing.Where(x => x.Id == listingId).FirstOrDefault();
             
        if (entity == null) throw new EntityNotFoundException(typeof(Listing), listingId);

        await ValidateListingOwnershipAsync(entity.ArchitectId, "editar imagenes de");
        
        // Obtener URL antes de borrar para limpiar storage si es necesario
        var image = entity.Images.FirstOrDefault(x => x.ImageId == imageId);
        var imageUrl = image?.Url;

        entity.RemoveImage(imageId);
        
        await _listingRepository.UpdateAsync(entity);

        if (!string.IsNullOrEmpty(imageUrl))
        {
            // Opcional: borrar del storage background job o inmediato
            // await _imageStorageService.DeleteImageAsync(imageUrl);
        }
    }

    /// <summary>
    /// Actualiza el orden de las imagenes reconstruyendo la lista enlazada.
    /// </summary>
    /// <summary>
    /// Reordena imagenes usando metodo de dominio
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task UpdateImagesOrderAsync(Guid listingId, List<UpdateImageOrderDto> input)
    {
        if (input == null || !input.Any()) return;
        
        var listing = await _listingRepository.WithDetailsAsync(x => x.Images);
        var entity = listing.Where(x => x.Id == listingId).FirstOrDefault();
             
        if (entity == null) throw new EntityNotFoundException(typeof(Listing), listingId);

        await ValidateListingOwnershipAsync(entity.ArchitectId, "editar imagenes de");

        // Ordenar input por DisplayOrder y extraer IDs
        var orderedIds = input.OrderBy(x => x.DisplayOrder).Select(x => x.ImageId).ToList();
        
        entity.ReorderImages(orderedIds);
        
        await _listingRepository.UpdateAsync(entity);
    }

    public async Task<PagedResultDto<ListingSummaryDto>> SearchAsync(PropertySearchDto searchDto)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
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
            queryable = queryable.Where(p => p.Location != null && p.Location.Value.Contains(searchDto.Location));  // ? null check
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

        return new PagedResultDto<ListingSummaryDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingSummaryDto>>(listings)
        );
    }

    /// <summary>
    /// Obtiene propiedades en portafolio (proyectos completados/showcase)
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<ListingSummaryDto>> GetPortfolioAsync(GetListingsInput input)
    {
        var queryable = await _listingRepository.WithDetailsAsync(
            listing => listing.Images!);    // ? null-forgiving

        // Solo propiedades en estado Portfolio
        queryable = queryable.Where(p => p.Status == ListingStatus.Portfolio);

        // Aplicar filtros básicos
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Title.Contains(input.SearchTerm) ||
                (p.Location != null && p.Location.Value.Contains(input.SearchTerm)));  // ? null check
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

        return new PagedResultDto<ListingSummaryDto>(
            totalCount,
            ObjectMapper.Map<List<Listing>, List<ListingSummaryDto>>(listings)
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
                    p.Location.Value.Contains(searchTerm))
                .GroupBy(p => p.Location!.Value)
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

    private static decimal ResolvePrice(bool isPriceOnRequest, decimal? price)
    {
        if (isPriceOnRequest)
        {
            return -1;
        }

        if (!price.HasValue)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingInvalidPrice);
        }

        return price.Value;
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
    ///
    /// Implementa reintentos ante errores de concurrencia optimista usando unidades de trabajo
    /// independientes para evitar que un DbContext en estado inconsistente vuelva a fallar.
    /// </summary>
    private async Task<T> ExecuteImageOperationAsync<T>(Guid listingId, string operationName, Func<Listing, Task<T>> operation)
    {
        const int maxRetries = 3;
        var attempt = 0;
        Exception? lastException = null;
        // #region agent log
        var _execReqId = Guid.NewGuid().ToString("N").Substring(0, 8);
        // #endregion

        while (attempt < maxRetries)
        {
            // #region agent log
            _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1070", message = "ExecuteImageOp attempt START", data = new { listingId, operationName, attempt, _execReqId, threadId = Environment.CurrentManagedThreadId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "B,D" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
            // #endregion
            using var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);

            try
            {
                // Cargar la entidad en una unidad de trabajo dedicada para aislar el DbContext
                var listing = await GetListingWithImagesAsync(listingId);
                // #region agent log
                _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1078", message = "Listing loaded in UoW", data = new { listingId, _execReqId, concurrencyStamp = listing.ConcurrencyStamp, imageCount = listing.Images?.Count ?? 0 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "B,D" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
                // #endregion

                // Ejecutar la operación solicitada
                var result = await operation(listing);

                var _imagesInfo = listing.Images?.OrderBy(i => i.SortOrder).Select(img => new { Id = img.ImageId, UrlStart = img.Url?.Substring(0, Math.Min(50, img.Url?.Length ?? 0)), Order = img.SortOrder }).ToList();
                _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1095", message = "Before UoW.CompleteAsync", data = new { listingId, _execReqId, concurrencyStamp = listing.ConcurrencyStamp, imageCount = listing.Images?.Count ?? 0, imagesInfo = _imagesInfo, imagesCollectionType = listing.Images?.GetType().FullName }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "J,K" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
                // #endregion
                await uow.CompleteAsync();
                // #region agent log
                _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1090", message = "UoW.CompleteAsync SUCCESS", data = new { listingId, _execReqId, attempt }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "B,D" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
                // #endregion
                return result;
            }
            catch (Exception ex) when (IsConcurrencyException(ex))
            {
                lastException = ex;
                var snapshot = await BuildListingImagesDebugSnapshotAsync(listingId);
                var concurrencyDetails = BuildConcurrencyDetails(ex);
                
                // #region agent log
                _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1108", message = "CONCURRENCY EXCEPTION CAUGHT", data = new { listingId, _execReqId, attempt, exType = ex.GetType().Name, exMsg = ex.Message, snapshot, concurrencyDetails }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "B,D,E" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
                // #endregion

                Logger.LogWarning(
                    ex,
                    "Concurrencia al {Operation} del listing {ListingId} (intento {Attempt}/{Max}). Snapshot: {Snapshot}. Detalles: {Details}",
                    operationName,
                    listingId,
                    attempt + 1,
                    maxRetries,
                    snapshot,
                    concurrencyDetails);

                attempt++;

                if (attempt >= maxRetries)
                {
                    // #region agent log
                    _ = Task.Run(async () => { try { using var c = new System.Net.Http.HttpClient(); await c.PostAsync("http://127.0.0.1:7242/ingest/3732978b-ef68-4667-9b0b-c599e86f26bf", new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(new { location = "ListingAppService.cs:1125", message = "MAX RETRIES EXHAUSTED", data = new { listingId, _execReqId, maxRetries }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), sessionId = "debug-session", hypothesisId = "B,D,E" }), System.Text.Encoding.UTF8, "application/json")); } catch { } });
                    // #endregion
                    break;
                }

                // Pequeña espera exponencial para evitar contención inmediata
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt));
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

        // Re-lanzar la última excepción de concurrencia si se agotaron los reintentos
        var finalException = lastException ?? new AbpException("Fallo desconocido al operar imagenes del listing.");

        var finalSnapshot = await BuildListingImagesDebugSnapshotAsync(listingId);
        var finalDetails = BuildConcurrencyDetails(finalException);

        Logger.LogError(
            finalException,
            "Se agotaron los reintentos al {Operation} del listing {ListingId}. Snapshot final: {Snapshot}. Detalles: {Details}",
            operationName,
            listingId,
            finalSnapshot,
            finalDetails);
        /*
        if (_hostEnvironment.IsDevelopment())
        {
            throw new BusinessException("Listing:ImageConcurrencyFailure")
                .WithData("ListingId", listingId)
                .WithData("Operation", operationName)
                .WithData("Snapshot", finalSnapshot)
                .WithData("Details", finalDetails)
                .WithData("ExceptionChain", BuildExceptionChain(finalException))
                .WithData("StackTrace", finalException.StackTrace ?? "(sin stack)");
        }
        */
        throw finalException;
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
        var typeName = ex.GetType().Name;
        if (string.Equals(typeName, "DbUpdateConcurrencyException", StringComparison.Ordinal))
        {
            return true;
        }

        // Verificar el tipo por nombre (para evitar dependencia directa de EF Core en InnerExceptions)
        if (typeName == "InvalidOperationException" &&
            (ex.Message.Contains("cannot be tracked", StringComparison.OrdinalIgnoreCase) ||
             ex.Message.Contains("already being tracked", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar mensaje de excepcion directamente (fallback) buscando pattern clásico de filas afectadas
        if (ex.Message.Contains("expected to affect 1 row(s), but actually affected 0 row(s)", StringComparison.OrdinalIgnoreCase) ||
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
        var ordered = images.OrderBy(i => i.SortOrder).ToList();
        return ordered.Count == 0
            ? "(sin imagenes)"
            : string.Join(" -> ", ordered.Select(img => $"{ShortId(img.ImageId)}(o:{img.SortOrder})"));
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

    private async Task<UploadImageResult> StoreImageIfNeededAsync(AddListingImageDto input)
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
