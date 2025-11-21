using System.Threading.Tasks;
using cima.Domain.Shared.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Statistics;

/// <summary>
/// Servicio para obtener estadísticas y métricas del sistema
/// Solo accesible para usuarios con permisos administrativos
/// </summary>
public interface IStatisticsAppService : IApplicationService
{
    /// <summary>
    /// Obtiene estadísticas generales para el dashboard
    /// </summary>
    Task<DashboardStatsDto> GetDashboardAsync();

    /// <summary>
    /// Obtiene estadísticas detalladas de propiedades
    /// </summary>
    Task<ListingStatsDto> GetListingStatsAsync();

    /// <summary>
    /// Obtiene estadísticas de solicitudes de contacto
    /// </summary>
    Task<ContactRequestStatsDto> GetContactRequestStatsAsync();
}
