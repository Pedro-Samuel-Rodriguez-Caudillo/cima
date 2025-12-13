using System;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Services.Listings;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cima.EntityFrameworkCore.EntityFrameworkCore.Listings;

/// <summary>
/// Usa pg_advisory_xact_lock para serializar operaciones por ListingId en Postgres.
/// No realiza ninguna acción si el proveedor no es Npgsql.
/// </summary>
public class PostgresListingPersistenceLock : IListingPersistenceLock, ITransientDependency
{
    private readonly IDbContextProvider<cimaDbContext> _dbContextProvider;
    private readonly ILogger<PostgresListingPersistenceLock> _logger;

    public PostgresListingPersistenceLock(
        IDbContextProvider<cimaDbContext> dbContextProvider,
        ILogger<PostgresListingPersistenceLock> logger)
    {
        _dbContextProvider = dbContextProvider;
        _logger = logger;
    }

    public async Task<IAsyncDisposable> AcquireAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        if (!dbContext.Database.IsNpgsql())
        {
            _logger.LogDebug("Saltando advisory lock para listing {ListingId} por provider no Npgsql", listingId);
            return NoopLock.Instance;
        }

        var key = ConvertGuidToInt64(listingId);
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0});", new object[] { key }, cancellationToken);
            _logger.LogDebug("Adquirido advisory lock para listing {ListingId} (key {Key})", listingId, key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al adquirir advisory lock para listing {ListingId} (key {Key})", listingId, key);
            throw;
        }

        return NoopLock.Instance;
    }

    private static long ConvertGuidToInt64(Guid id)
    {
        var bytes = id.ToByteArray();
        return BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
    }

    private sealed class NoopLock : IAsyncDisposable
    {
        public static readonly NoopLock Instance = new();

        private NoopLock()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
