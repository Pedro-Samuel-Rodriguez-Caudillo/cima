using System;
using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Listings;

/// <summary>
/// Servicio para gestionar propiedades inmobiliarias
/// </summary>
public interface IListingAppService : IApplicationService
{
    /// <summary>
    /// Obtiene lista paginada de propiedades con filtros
    /// </summary>
    Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input);
    
    /// <summary>
    /// Obtiene detalle de una propiedad por Id
    /// </summary>
    Task<ListingDto> GetAsync(Guid id);
    
    /// <summary>
    /// Crea nueva propiedad en estado Draft
    /// </summary>
    Task<ListingDto> CreateAsync(CreateUpdateListingDto input);
    
    /// <summary>
    /// Actualiza propiedad existente
    /// </summary>
    Task<ListingDto> UpdateAsync(Guid id, CreateUpdateListingDto input);
    
    /// <summary>
    /// Elimina propiedad
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Cambia estado de Draft a Published
    /// Requiere al menos 1 imagen
    /// </summary>
    Task<ListingDto> PublishAsync(Guid id);
    
    /// <summary>
    /// Cambia estado a Archived
    /// </summary>
    Task<ListingDto> ArchiveAsync(Guid id);
    
    /// <summary>
    /// Obtiene propiedades de un arquitecto especifico
    /// </summary>
    Task<PagedResultDto<ListingDto>> GetByArchitectAsync(
        Guid architectId, 
        int skipCount, 
        int maxResultCount);
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
}
