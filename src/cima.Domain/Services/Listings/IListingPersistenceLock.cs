using System;
using System.Threading;
using System.Threading.Tasks;

namespace cima.Domain.Services.Listings;

/// <summary>
/// Lock de persistencia para serializar operaciones de un listing (cross-process).
/// Implementado en la capa de infraestructura (por ejemplo, advisory locks de la base de datos).
/// </summary>
public interface IListingPersistenceLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid listingId, CancellationToken cancellationToken = default);
}

