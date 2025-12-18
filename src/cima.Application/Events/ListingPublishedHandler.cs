using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace cima.Events;

/// <summary>
/// Handler para el evento de publicación de listing.
/// Puede usarse para actualizar estadísticas, enviar notificaciones, etc.
/// </summary>
public class ListingPublishedHandler : ILocalEventHandler<ListingPublishedEto>, ITransientDependency
{
    private readonly ILogger<ListingPublishedHandler> _logger;

    public ListingPublishedHandler(ILogger<ListingPublishedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(ListingPublishedEto eventData)
    {
        _logger.LogInformation(
            "Listing publicado: {ListingId} - {Title} por arquitecto {ArchitectId} el {PublishedAt}",
            eventData.ListingId,
            eventData.Title,
            eventData.ArchitectId,
            eventData.PublishedAt);

        // Aquí puedes agregar lógica adicional:
        // - Actualizar estadísticas del arquitecto
        // - Enviar notificación push
        // - Actualizar cache
        // - Indexar en motor de búsqueda

        return Task.CompletedTask;
    }
}
