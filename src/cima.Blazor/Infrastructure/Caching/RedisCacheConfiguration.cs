using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace cima.Blazor.Infrastructure.Caching;

/// <summary>
/// Configuración de Redis para caché distribuido
/// Nota: Requiere paquete Microsoft.Extensions.Caching.StackExchangeRedis
/// </summary>
public static class RedisCacheConfiguration
{
    public static IServiceCollection AddCimaRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = configuration
            .GetSection("Redis")
            .Get<RedisOptions>() ?? new RedisOptions();

        // Siempre usar memoria por ahora hasta que se instale el paquete de Redis
        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}

public class RedisOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "cima_";
    public string? Password { get; set; }
    public bool UseSsl { get; set; } = false;
    public int ConnectTimeoutMs { get; set; } = 5000;
    public int DefaultExpirationMinutes { get; set; } = 30;
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _options = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await _cache.GetStringAsync(GetKey(key), cancellationToken);
            if (cached == null)
                return default;

            return System.Text.Json.JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo caché para key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes)
            };

            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(GetKey(key), json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error guardando caché para key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(GetKey(key), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando caché para key: {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    private string GetKey(string key) => $"{_options.InstanceName}{key}";
}
