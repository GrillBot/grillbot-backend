using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrillBot.Core.Redis.Extensions;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan? expiration, CancellationToken cancellationToken = default)
    {
        var cacheOptions = new DistributedCacheEntryOptions();

        if (expiration is not null)
            cacheOptions.SetAbsoluteExpiration(expiration.Value);

        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        return cache.SetAsync(key, bytes, cacheOptions, cancellationToken);
    }

    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes is null)
            return default;

        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
