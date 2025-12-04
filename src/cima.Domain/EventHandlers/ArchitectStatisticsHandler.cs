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
            "Procesando cambio de estado de Listing {ListingId}: {OldStatus} -> {NewStatus} (PrimeraPublicacion: {IsFirst})",
            eventData.ListingId,
            eventData.OldStatus,
            eventData.NewStatus,
            eventData.IsFirstTimePublished);

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

        // Actualizar TotalListingsPublished SOLO si es primera publicación
        if (eventData.WasPublished && eventData.IsFirstTimePublished)
        {
            architect.TotalListingsPublished++;
            architect.ActiveListings++;
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: Primera publicación - TotalListingsPublished={Total}, ActiveListings={Active}",
                architect.Id,
                architect.TotalListingsPublished,
                architect.ActiveListings);
        }
        // Republicación (Draft -> Published pero NO es primera vez)
        else if (eventData.WasPublished && !eventData.IsFirstTimePublished)
        {
            architect.ActiveListings++;
            statsUpdated = true;
            
            _logger.LogInformation(
                "Arquitecto {ArchitectId}: Republicación - ActiveListings incrementado a {Active} (TotalPublished sin cambio)",
                architect.Id,
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
                "Arquitecto {ArchitectId}: Archivado - ActiveListings decrementado a {Active}",
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
                "Arquitecto {ArchitectId}: Desarchivado - ActiveListings incrementado a {Active}",
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
                "Arquitecto {ArchitectId}: Despublicado - ActiveListings decrementado a {Active}",
                architect.Id,
                architect.ActiveListings);
        }
        // Cuando se mueve de Published a Portfolio (sigue activo, sin cambio)
        else if (eventData.OldStatus == ListingStatus.Published && 
                 eventData.NewStatus == ListingStatus.Portfolio)
        {
            _logger.LogDebug(
                "Arquitecto {ArchitectId}: Movido a Portfolio - ActiveListings sin cambio ({Active})",
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

        // Los listings se crean en Draft, no afectan estadísticas hasta publicación
        await Task.CompletedTask;
    }
}
