using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using cima.Listings.Inputs;
using cima.Listings.Outputs;

namespace cima.Listings;

/// <summary>
/// Servicio para gestionar propiedades inmobiliarias
/// </summary>
public interface IListingAppService : IApplicationService
{
    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    Task<PagedResultDto<ListingSummaryDto>> GetListAsync(GetListingsInput input);
    
    /// <summary>
    /// Obtiene detalle de una propiedad por Id
    /// </summary>
    Task<ListingDetailDto> GetAsync(Guid id);
    
    /// <summary>
    /// Crea nueva propiedad en estado Draft
    /// </summary>
    Task<ListingDetailDto> CreateAsync(CreateListingDto input);
    
    /// <summary>
    /// Actualiza propiedad existente
    /// </summary>
    Task<ListingDetailDto> UpdateAsync(Guid id, UpdateListingDto input);
    
    /// <summary>
    /// Elimina propiedad (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Cambia estado de Draft a Published
    /// Requiere al menos 1 imagen
    /// </summary>
    Task<ListingDetailDto> PublishAsync(PublishListingDto input);
    
    /// <summary>
    /// Cambia estado a Archived
    /// </summary>
    Task<ListingDetailDto> ArchiveAsync(Guid id);
    
    /// <summary>
    /// Reactiva una propiedad archivada a estado Published
    /// </summary>
    Task<ListingDetailDto> UnarchiveAsync(Guid id);
    
    /// <summary>
    /// Cambia una propiedad publicada de vuelta a Draft
    /// </summary>
    Task<ListingDetailDto> UnpublishAsync(Guid id);
    
    /// <summary>
    /// Cambia una propiedad a estado Portfolio (showcase)
    /// </summary>
    Task<ListingDetailDto> MoveToPortfolioAsync(Guid id);
    
    /// <summary>
    /// Duplica una propiedad existente (sin imágenes, estado Draft)
    /// </summary>
    Task<ListingDetailDto> DuplicateAsync(Guid id);
    
    /// <summary>
    /// Obtiene propiedades de un arquitecto especifico
    /// </summary>
    Task<PagedResultDto<ListingSummaryDto>> GetByArchitectAsync(
        Guid architectId, 
        int skipCount, 
        int maxResultCount);
    
    /// <summary>
    /// Obtiene solo propiedades publicadas (público)
    /// </summary>
    Task<PagedResultDto<ListingSummaryDto>> GetPublishedAsync(GetListingsInput input);
    
    /// <summary>
    /// Búsqueda avanzada de propiedades con filtros
    /// Incluye validación contra inyecciones SQL/XSS
    /// </summary>
    Task<PagedResultDto<ListingSummaryDto>> SearchAsync(PropertySearchDto searchDto);
    
    /// <summary>
    /// Obtiene propiedades en portafolio (proyectos completados)
    /// </summary>
    Task<PagedResultDto<ListingSummaryDto>> GetPortfolioAsync(GetListingsInput input);
    
    /// <summary>
    /// Obtiene sugerencias de ubicaciones para autocompletado
    /// Basado en ubicaciones existentes en propiedades publicadas
    /// </summary>
    Task<List<LocationSuggestionDto>> GetLocationSuggestionsAsync(string searchTerm);
    
    /// <summary>
    /// Agrega una imagen a una propiedad.
    /// Retorna un Dto simple de imagen, podria ser ListingImageOutput si quisieramos ser estrictos.
    /// ListingImageDto se considera de salida en este contexto.
    /// </summary>
    Task<ListingImageDto> AddImageAsync(AddListingImageDto input);
    
    /// <summary>
    /// Elimina una imagen de una propiedad
    /// </summary>
    Task RemoveImageAsync(Guid listingId, Guid imageId);
    
    /// <summary>
    /// Actualiza el orden de las imágenes
    /// </summary>
    Task UpdateImagesOrderAsync(Guid listingId, List<UpdateImageOrderDto> input);
}

/// <summary>
/// DTO para obtener lista de listings (compatible con query string)
/// </summary>
public class GetListingsInput : PagedAndSortedResultRequestDto
{
    public string? SearchTerm { get; set; }
    public int? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public Guid? ArchitectId { get; set; }
    public int? PropertyType { get; set; }
    public int? PropertyCategory { get; set; }
    public int? TransactionType { get; set; }
    public bool? FeaturedOnly { get; set; }
}
