using System;
using System.Threading.Tasks;
using cima.Domain.Services.Listings;
using cima.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace cima.BackgroundWorkers;

/// <summary>
/// Background worker que limpia borradores antiguos (más de 30 días).
/// Se ejecuta una vez al día.
/// </summary>
public class DraftCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public DraftCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Ejecutar cada 24 horas
        Timer.Period = 24 * 60 * 60 * 1000; // 24 horas en milisegundos
    }

    [UnitOfWork]
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<DraftCleanupWorker>>();
        
        logger.LogInformation("Iniciando limpieza de borradores antiguos...");

        var listingRepository = workerContext.ServiceProvider
            .GetRequiredService<IRepository<cima.Domain.Entities.Listing, Guid>>();
        var listingManager = workerContext.ServiceProvider
            .GetRequiredService<IListingManager>();

        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        // Buscar drafts más viejos de 30 días
        var oldDrafts = await listingRepository.GetListAsync(
            l => l.Status == ListingStatus.Draft && l.CreatedAt < cutoffDate);

        if (oldDrafts.Count == 0)
        {
            logger.LogInformation("No hay borradores antiguos para limpiar.");
            return;
        }

        foreach (var draft in oldDrafts)
        {
            // Archivar usando el dominio service (respeta encapsulación)
            await listingManager.ArchiveAsync(draft, Guid.Empty); // System user
            await listingRepository.UpdateAsync(draft);
            
            logger.LogInformation(
                "Borrador {ListingId} archivado automáticamente por inactividad.",
                draft.Id);
        }

        logger.LogInformation(
            "Limpieza completada: {Count} borradores archivados.",
            oldDrafts.Count);
    }
}
