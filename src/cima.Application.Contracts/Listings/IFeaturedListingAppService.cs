using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Listings;

/// <summary>
/// Servicio para gestionar propiedades destacadas
/// Límite máximo: 12 propiedades destacadas
/// Las propiedades se muestran en orden aleatorio
/// </summary>
public interface IFeaturedListingAppService : IApplicationService
{
    /// <summary>
    /// Obtiene todas las propiedades destacadas (para admin)
    /// </summary>
    Task<List<FeaturedListingDto>> GetAllAsync();

    /// <summary>
    /// Obtiene propiedades destacadas con paginación y orden aleatorio
    /// </summary>
    Task<PagedResultDto<FeaturedListingDto>> GetPagedAsync(GetFeaturedListingsDto input);

    /// <summary>
    /// Obtiene propiedades destacadas para mostrar en homepage
    /// Con orden aleatorio y límite de 6 por defecto
    /// </summary>
    Task<List<ListingDto>> GetForHomepageAsync(int count = 6);

    /// <summary>
    /// Agrega una propiedad a destacados
    /// Valida límite máximo de 12
    /// </summary>
    Task<FeaturedListingDto> AddAsync(CreateFeaturedListingDto input);

    /// <summary>
    /// Remueve una propiedad de destacados
    /// </summary>
    Task RemoveAsync(Guid featuredListingId);

    /// <summary>
    /// Remueve una propiedad de destacados por ListingId
    /// </summary>
    Task RemoveByListingIdAsync(Guid listingId);

    /// <summary>
    /// Verifica si una propiedad está en destacados
    /// </summary>
    Task<bool> IsListingFeaturedAsync(Guid listingId);

    /// <summary>
    /// Obtiene el conteo actual de propiedades destacadas
    /// </summary>
    Task<int> GetCountAsync();
}
