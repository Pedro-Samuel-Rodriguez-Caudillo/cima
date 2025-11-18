using System;
using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Services
{
    /// <summary>
    /// Servicio para gestionar propiedades inmobiliarias
    /// </summary>
    public interface IPropertyAppService : IApplicationService
    {
        /// <summary>
        /// Obtiene lista paginada de propiedades con filtros
        /// </summary>
        Task<PagedResultDto<PropertyDto>> GetListAsync(PropertyFiltersDto filters);
        
        /// <summary>
        /// Obtiene detalle de una propiedad por Id
        /// </summary>
        Task<PropertyDto> GetAsync(Guid id);
        
        /// <summary>
        /// Crea nueva propiedad en estado Draft
        /// </summary>
        Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input);
        
        /// <summary>
        /// Actualiza propiedad existente
        /// </summary>
        Task<PropertyDto> UpdateAsync(Guid id, CreateUpdatePropertyDto input);
        
        /// <summary>
        /// Elimina propiedad
        /// </summary>
        Task DeleteAsync(Guid id);
        
        /// <summary>
        /// Cambia estado de Draft a Published
        /// Requiere al menos 1 imagen
        /// </summary>
        Task<PropertyDto> PublishAsync(Guid id);
        
        /// <summary>
        /// Cambia estado a Archived
        /// </summary>
        Task<PropertyDto> ArchiveAsync(Guid id);
        
        /// <summary>
        /// Obtiene propiedades de un arquitecto especifico
        /// </summary>
        Task<PagedResultDto<PropertyDto>> GetByArchitectAsync(
            Guid architectId, 
            int skipCount, 
            int maxResultCount);
    }
}
