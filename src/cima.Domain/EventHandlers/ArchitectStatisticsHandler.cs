using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Events.Listings;
using cima.Domain.Shared;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace cima.Domain.EventHandlers;

/// <summary>
/// Handler de eventos de dominio para actualizar las estadísticas del arquitecto
/// cuando cambia el estado de un Listing.
/// </summary>
public class ArchitectStatisticsHandler :
    ILocalEventHandler<ListingStatusChangedEto>,
    ILocalEventHandler<ListingCreatedEto>,
    ITransientDependency
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly ILogger<ArchitectStatisticsHandler> _logger;

    public ArchitectStatisticsHandler(
        IRepository<Architect, Guid> architectRepository,
        ILogger<ArchitectStatisticsHandler> logger)
    {
        _architectRepository = architectRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(ListingStatusChangedEto eventData)
    {
        _logger.LogDebug(
            "Procesando cambio de estado de Listing {ListingId}: {OldStatus} -> {NewStatus}",
            eventData.ListingId,
            eventData.OldStatus,
            eventData.NewStatus);

        var architect = await _architectRepository.FindAsync(eventData.ArchitectId);
        if (architect == null)
        {
            _logger.LogWarning(
                "Arquitecto {ArchitectId} no encontrado al procesar evento de Listing {ListingId}",
                eventData.ArchitectId,
                eventData.ListingId);
            return;
        }

        var statsUpdated = false;

        // Actualizar TotalListingsPublished cuando se publica por primera vez
        if (eventData.WasPublished)
        {
            architect.TotalListingsPublished++;
            architect.ActiveListings++;
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: TotalListingsPublished incrementado a {Total}, ActiveListings a {Active}",
                architect.Id,
                architect.TotalListingsPublished,
                architect.ActiveListings);
        }
        // Cuando se archiva desde Published o Portfolio
        else if (eventData.WasArchived && 
                 (eventData.OldStatus == ListingStatus.Published || 
                  eventData.OldStatus == ListingStatus.Portfolio))
        {
            architect.ActiveListings = Math.Max(0, architect.ActiveListings - 1);
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: ActiveListings decrementado a {Active}",
                architect.Id,
                architect.ActiveListings);
        }
        // Cuando se desarchiva (Archived -> Published)
        else if (eventData.OldStatus == ListingStatus.Archived && 
                 eventData.NewStatus == ListingStatus.Published)
        {
            architect.ActiveListings++;
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: ActiveListings incrementado a {Active} (desarchivado)",
                architect.Id,
                architect.ActiveListings);
        }
        // Cuando se despublica (Published -> Draft)
        else if (eventData.OldStatus == ListingStatus.Published && 
                 eventData.NewStatus == ListingStatus.Draft)
        {
            architect.ActiveListings = Math.Max(0, architect.ActiveListings - 1);
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: ActiveListings decrementado a {Active} (despublicado)",
                architect.Id,
                architect.ActiveListings);
        }

        if (statsUpdated)
        {
            await _architectRepository.UpdateAsync(architect);
        }
    }

    public async Task HandleEventAsync(ListingCreatedEto eventData)
    {
        _logger.LogDebug(
            "Listing {ListingId} creado por Arquitecto {ArchitectId}: {Title}",
            eventData.ListingId,
            eventData.ArchitectId,
            eventData.Title);

        // Aquí se podrían agregar métricas o notificaciones
        await Task.CompletedTask;
    }
}
