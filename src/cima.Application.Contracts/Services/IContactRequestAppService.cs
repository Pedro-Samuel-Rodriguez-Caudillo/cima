using System;
using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Services
{
    /// <summary>
    /// Servicio para gestionar solicitudes de contacto
    /// </summary>
    public interface IContactRequestAppService : IApplicationService
    {
        /// <summary>
        /// Crea nueva solicitud de contacto (publico, sin auth)
        /// </summary>
        Task<ContactRequestDto> CreateAsync(CreateContactRequestDto input);
        
        /// <summary>
        /// Obtiene lista paginada de todas las solicitudes (admin)
        /// </summary>
        Task<PagedResultDto<ContactRequestDto>> GetListAsync(
            int skipCount, 
            int maxResultCount);
        
        /// <summary>
        /// Obtiene solicitudes de un arquitecto especifico
        /// </summary>
        Task<PagedResultDto<ContactRequestDto>> GetByArchitectAsync(
            Guid architectId, 
            int skipCount, 
            int maxResultCount);
        
        /// <summary>
        /// Marca solicitud como respondida con notas
        /// </summary>
        Task<ContactRequestDto> MarkAsRepliedAsync(Guid id, string replyNotes);
        
        /// <summary>
        /// Cierra solicitud (estado final)
        /// </summary>
        Task<ContactRequestDto> CloseAsync(Guid id);
        
        /// <summary>
        /// Obtiene detalle de solicitud
        /// </summary>
        Task<ContactRequestDto> GetAsync(Guid id);
    }
}
