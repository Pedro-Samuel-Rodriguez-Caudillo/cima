using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace cima.Listings;

/// <summary>
/// Implementación del servicio de propiedades destacadas.
/// Usa caché inteligente para respuestas rápidas manteniendo variedad visual.
/// </summary>
public class FeaturedListingAppService : cimaAppService, IFeaturedListingAppService
{
    private readonly IRepository<FeaturedListing, Guid> _featuredListingRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IDistributedCache<List<ListingDto>> _cache;
    private const int MAX_FEATURED_LISTINGS = 12;
    private const string CACHE_KEY = "FeaturedListingsForHomepage";
    /// <summary>
    /// Duración del caché reducida a 5 minutos para mayor frescura
    /// mientras mantiene buena performance
    /// </summary>
    private const int CACHE_DURATION_MINUTES = 5;

    public FeaturedListingAppService(
        IRepository<FeaturedListing, Guid> featuredListingRepository,
        IRepository<Listing, Guid> listingRepository,
        IDistributedCache<List<ListingDto>> cache)
    {
        _featuredListingRepository = featuredListingRepository;
        _listingRepository = listingRepository;
        _cache = cache;
    }

    /// <summary>
    /// Obtiene todas las propiedades destacadas (para admin)
    /// En orden aleatorio
    /// </summary>
    [Authorize(cimaPermissions.Listings.Default)]
    public async Task<List<FeaturedListingDto>> GetAllAsync()
    {
        var queryable = await _featuredListingRepository.WithDetailsAsync(fl => fl.Listing!);
        var featuredListings = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(fl => Guid.NewGuid()));

        return ObjectMapper.Map<List<FeaturedListing>, List<FeaturedListingDto>>(featuredListings);
    }

    /// <summary>
    /// Obtiene propiedades destacadas con paginación
    /// Siempre en orden aleatorio
    /// </summary>
    [AllowAnonymous]
    public async Task<PagedResultDto<FeaturedListingDto>> GetPagedAsync(GetFeaturedListingsDto input)
    {
        var queryable = await _featuredListingRepository.WithDetailsAsync(fl => fl.Listing!);

        // Filtrar solo listings que estén publicados o en portafolio
        queryable = queryable.Where(fl => 
            fl.Listing != null &&
            (fl.Listing.Status == ListingStatus.Published || 
             fl.Listing.Status == ListingStatus.Portfolio));

        var allFeatured = await AsyncExecuter.ToListAsync(queryable);

        // Orden aleatorio usando Random para mejor distribución
        var random = new Random();
        var orderedFeatured = allFeatured.OrderBy(x => random.Next());

        var totalCount = allFeatured.Count;
        var skipCount = input.PageNumber * input.PageSize;

        var pagedFeatured = orderedFeatured
            .Skip(skipCount)
            .Take(input.PageSize)
            .ToList();

        return new PagedResultDto<FeaturedListingDto>(
            totalCount,
            ObjectMapper.Map<List<FeaturedListing>, List<FeaturedListingDto>>(pagedFeatured)
        );
    }

    /// <summary>
    /// Obtiene propiedades destacadas para mostrar en homepage.
    /// 
    /// Estrategia de caché optimizada para responsividad:
    /// - Caché de 5 minutos para respuestas rápidas (mejora percepción de velocidad)
    /// - El orden se aleatoriza en cada petición desde el caché
    /// - La página se siente más responsiva al cargar instantáneamente
    /// </summary>
    [AllowAnonymous]
    public async Task<List<ListingDto>> GetForHomepageAsync(int count = 6)
    {
        if (count > 12)
        {
            count = 12;
        }

        // Intentar obtener del caché
        var cachedListings = await _cache.GetAsync(CACHE_KEY);
        if (cachedListings != null && cachedListings.Count > 0)
        {
            // Aleatorizar el orden en cada petición para variedad visual
            // El caché guarda los datos, pero el orden cambia cada vez
            var random = new Random();
            var randomized = cachedListings
                .OrderBy(_ => random.Next())
                .Take(count)
                .ToList();
            
            return randomized;
        }

        // Si no hay caché, obtener de BD
        var queryable = await _featuredListingRepository.WithDetailsAsync(
            fl => fl.Listing,
            fl => fl.Listing!.Architect,
            fl => fl.Listing!.Images);

        // Filtrar solo listings activos
        queryable = queryable.Where(fl => 
            fl.Listing != null &&
            (fl.Listing.Status == ListingStatus.Published || 
             fl.Listing.Status == ListingStatus.Portfolio));

        var allFeatured = await AsyncExecuter.ToListAsync(queryable);

        // Extraer listings (sin aleatorizar para el caché)
        var allListings = allFeatured
            .Where(fl => fl.Listing != null)
            .Select(fl => fl.Listing!)
            .ToList();

        var listings = ObjectMapper.Map<List<Listing>, List<ListingDto>>(allListings);

        // Guardar TODOS en caché (sin orden específico)
        // El orden se aleatoriza al recuperar
        if (listings.Count > 0)
        {
            await _cache.SetAsync(
                CACHE_KEY,
                listings,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES)
                }
            );
        }

        // Retornar aleatorizado
        var random2 = new Random();
        return listings
            .OrderBy(_ => random2.Next())
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Agrega una propiedad a destacados
    /// Valida límite máximo de 12 e invalida caché
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<FeaturedListingDto> AddAsync(CreateFeaturedListingDto input)
    {
        // Validar que la propiedad existe
        var listing = await _listingRepository.GetAsync(input.ListingId);

        // Validar que la propiedad esté publicada o en portafolio
        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            throw new BusinessException("FeaturedListing:ListingNotPublished")
                .WithData("ListingId", input.ListingId)
                .WithData("Status", listing.Status);
        }

        // Validar que no esté ya destacada
        var exists = await _featuredListingRepository.AnyAsync(fl => fl.ListingId == input.ListingId);
        if (exists)
        {
            throw new BusinessException("FeaturedListing:AlreadyFeatured")
                .WithData("ListingId", input.ListingId);
        }

        // Validar límite máximo
        var currentCount = await _featuredListingRepository.CountAsync();
        if (currentCount >= MAX_FEATURED_LISTINGS)
        {
            throw new BusinessException("FeaturedListing:MaxLimitReached")
                .WithData("MaxLimit", MAX_FEATURED_LISTINGS)
                .WithData("CurrentCount", currentCount);
        }

        var featuredListing = new FeaturedListing(
            input.ListingId,
            CurrentUser.Id);

        await _featuredListingRepository.InsertAsync(featuredListing);

        // Invalidar caché para que se recarguen las destacadas
        await _cache.RemoveAsync(CACHE_KEY);

        // Cargar con detalles para retornar
        var queryable = await _featuredListingRepository.WithDetailsAsync(fl => fl.Listing!);
        var result = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(fl => fl.Id == featuredListing.Id));

        if (result == null)
        {
            throw new BusinessException("FeaturedListing:NotFoundAfterCreation")
                .WithData("Id", featuredListing.Id);
        }

        return ObjectMapper.Map<FeaturedListing, FeaturedListingDto>(result);
    }

    /// <summary>
    /// Remueve una propiedad de destacados e invalida caché
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveAsync(Guid featuredListingId)
    {
        await _featuredListingRepository.DeleteAsync(featuredListingId);
        await _cache.RemoveAsync(CACHE_KEY);
    }

    /// <summary>
    /// Remueve una propiedad de destacados por ListingId e invalida caché
    /// </summary>
    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task RemoveByListingIdAsync(Guid listingId)
    {
        var featuredListing = await _featuredListingRepository.FirstOrDefaultAsync(
            fl => fl.ListingId == listingId);

        if (featuredListing != null)
        {
            await _featuredListingRepository.DeleteAsync(featuredListing);
            await _cache.RemoveAsync(CACHE_KEY);
        }
    }

    /// <summary>
    /// Verifica si una propiedad está en destacados
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> IsListingFeaturedAsync(Guid listingId)
    {
        return await _featuredListingRepository.AnyAsync(fl => fl.ListingId == listingId);
    }

    /// <summary>
    /// Obtiene el conteo actual de propiedades destacadas
    /// </summary>
    [AllowAnonymous]
    public async Task<int> GetCountAsync()
    {
        return await _featuredListingRepository.CountAsync();
    }
}
