using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace cima.Listings;

/// <summary>
/// Handle que representa un lock adquirido. Debe liberarse con Dispose/DisposeAsync.
/// </summary>
public interface IListingImageLockHandle : IAsyncDisposable
{
}

/// <summary>
/// Servicio singleton que proporciona locks por listingId para operaciones de imagen.
/// Previene conflictos de concurrencia cuando multiples requests intentan modificar
/// las imagenes del mismo listing simultaneamente.
/// </summary>
public interface IListingImageLockService
{
    /// <summary>
    /// Adquiere un lock exclusivo para el listingId especificado.
    /// El lock debe liberarse llamando a DisposeAsync en el handle retornado.
    /// Uso: await using var handle = await _lockService.AcquireAsync(listingId);
    /// </summary>
    Task<IListingImageLockHandle> AcquireAsync(Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una operacion de manera serializada para un listing especifico.
    /// Solo una operacion por listingId puede ejecutarse a la vez.
    /// </summary>
    Task<T> ExecuteWithLockAsync<T>(Guid listingId, Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Version sin retorno para operaciones void.
    /// </summary>
    Task ExecuteWithLockAsync(Guid listingId, Func<Task> operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementacion singleton del servicio de locks por listing.
/// Usa ConcurrentDictionary con SemaphoreSlim para serializar operaciones por listing.
/// </summary>
public class ListingImageLockService : IListingImageLockService, ISingletonDependency
{
    private readonly ConcurrentDictionary<Guid, LockEntry> _locks = new();

    // Configuracion de timeout para evitar deadlocks
    private const int LockTimeoutSeconds = 60;

    /// <summary>
    /// Entry interna que mantiene el semaforo y un contador de referencias.
    /// El contador evita que se limpie el semaforo mientras alguien espera.
    /// </summary>
    private class LockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount;
    }

    /// <summary>
    /// Handle que libera el lock cuando se dispone.
    /// </summary>
    private class LockHandle : IListingImageLockHandle
    {
        private readonly ListingImageLockService _service;
        private readonly Guid _listingId;
        private readonly LockEntry _entry;
        private bool _disposed;

        public LockHandle(ListingImageLockService service, Guid listingId, LockEntry entry)
        {
            _service = service;
            _listingId = listingId;
            _entry = entry;
        }

        public ValueTask DisposeAsync()
        {
            if (_disposed) return ValueTask.CompletedTask;
            _disposed = true;

            _entry.Semaphore.Release();

            // Decrementar contador y limpiar si nadie mas lo usa
            if (Interlocked.Decrement(ref _entry.ReferenceCount) == 0)
            {
                _service._locks.TryRemove(_listingId, out _);
            }

            return ValueTask.CompletedTask;
        }
    }

    public async Task<IListingImageLockHandle> AcquireAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        // Obtener o crear entry, incrementando el contador atomicamente
        var entry = _locks.GetOrAdd(listingId, _ => new LockEntry());
        Interlocked.Increment(ref entry.ReferenceCount);

        try
        {
            // Intentar adquirir el lock con timeout
            var acquired = await entry.Semaphore.WaitAsync(
                TimeSpan.FromSeconds(LockTimeoutSeconds),
                cancellationToken);

            if (!acquired)
            {
                // Decrementar y limpiar si fallamos
                if (Interlocked.Decrement(ref entry.ReferenceCount) == 0)
                {
                    _locks.TryRemove(listingId, out _);
                }
                throw new TimeoutException(
                    $"No se pudo obtener el lock para el listing {listingId} despues de {LockTimeoutSeconds} segundos");
            }

            return new LockHandle(this, listingId, entry);
        }
        catch (OperationCanceledException)
        {
            // Decrementar y limpiar si se cancela
            if (Interlocked.Decrement(ref entry.ReferenceCount) == 0)
            {
                _locks.TryRemove(listingId, out _);
            }
            throw;
        }
    }

    public async Task<T> ExecuteWithLockAsync<T>(Guid listingId, Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using var handle = await AcquireAsync(listingId, cancellationToken);
        return await operation();
    }

    public async Task ExecuteWithLockAsync(Guid listingId, Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var handle = await AcquireAsync(listingId, cancellationToken);
        await operation();
    }
}
