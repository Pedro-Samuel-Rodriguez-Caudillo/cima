using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Servicio de cache en cliente usando localStorage/sessionStorage
/// </summary>
public interface IClientCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class ClientCacheService : IClientCacheService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string CachePrefix = "cima_cache_";

    public ClientCacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", $"{CachePrefix}{key}");
            if (string.IsNullOrEmpty(json))
                return default;

            var wrapper = JsonSerializer.Deserialize<CacheWrapper<T>>(json, _jsonOptions);
            if (wrapper == null)
                return default;

            // Verificar expiración
            if (wrapper.ExpiresAt.HasValue && wrapper.ExpiresAt.Value < DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key);
                return default;
            }

            return wrapper.Value;
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var wrapper = new CacheWrapper<T>
            {
                Value = value,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiration.HasValue 
                    ? DateTimeOffset.UtcNow.Add(expiration.Value) 
                    : null
            };

            var json = JsonSerializer.Serialize(wrapper, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{CachePrefix}{key}", json);
        }
        catch
        {
            // Ignorar errores de storage (ej: quota exceeded)
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"{CachePrefix}{key}");
        }
        catch
        {
            // Ignorar errores
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            // Limpiar solo items con prefijo cima_cache_
            var keysToRemove = await _jsRuntime.InvokeAsync<string[]>("cimaCache.getKeys", CachePrefix);
            foreach (var key in keysToRemove)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
        }
        catch
        {
            // Ignorar errores
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }

    private class CacheWrapper<T>
    {
        public T? Value { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}

/// <summary>
/// Extensiones para registro del servicio
/// </summary>
public static class ClientCacheServiceExtensions
{
    public static IServiceCollection AddCimaClientCache(this IServiceCollection services)
    {
        services.AddScoped<IClientCacheService, ClientCacheService>();
        return services;
    }
}
