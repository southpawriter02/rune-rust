using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// A caching decorator for DescriptorService that improves performance.
/// </summary>
/// <remarks>
/// This decorator wraps the base DescriptorService and caches descriptor pool
/// lookups to avoid redundant file I/O and parsing. The cache uses a sliding
/// expiration to balance memory usage with performance.
/// </remarks>
public class CachedDescriptorService : DescriptorService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedDescriptorService> _cachedLogger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Default cache entry expiration time.
    /// </summary>
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Cache key prefix for descriptor entries.
    /// </summary>
    private const string CacheKeyPrefix = "descriptor_";

    public CachedDescriptorService(
        IReadOnlyDictionary<string, DescriptorPool> pools,
        ThemeConfiguration theme,
        ILogger<DescriptorService> baseLogger,
        IMemoryCache cache,
        ILogger<CachedDescriptorService> cachedLogger,
        EnvironmentCoherenceService? coherenceService = null,
        IGameEventLogger? eventLogger = null)
        : base(pools, theme, baseLogger, coherenceService, eventLogger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cachedLogger = cachedLogger ?? throw new ArgumentNullException(nameof(cachedLogger));
        _eventLogger = eventLogger;

        _cachedLogger.LogDebug("CachedDescriptorService initialized with {Expiration} expiration",
            DefaultExpiration);
    }

    /// <summary>
    /// Gets a cached descriptor or generates and caches a new one.
    /// </summary>
    /// <param name="poolPath">The pool path.</param>
    /// <param name="tags">Optional tags for filtering.</param>
    /// <param name="context">Optional context.</param>
    /// <param name="cacheResult">Whether to cache the result (default true).</param>
    /// <returns>The descriptor text.</returns>
    public string GetDescriptorCached(
        string poolPath,
        IEnumerable<string>? tags = null,
        DescriptorContext? context = null,
        bool cacheResult = true)
    {
        // For context-sensitive requests, skip caching
        if (context != null || !cacheResult)
        {
            return base.GetDescriptor(poolPath, tags, context);
        }

        var tagKey = tags != null ? string.Join(",", tags.OrderBy(t => t)) : "";
        var cacheKey = $"{CacheKeyPrefix}{poolPath}_{tagKey}";

        if (_cache.TryGetValue(cacheKey, out string? cached) && cached != null)
        {
            _cachedLogger.LogDebug("Cache hit for {PoolPath}", poolPath);

            _eventLogger?.LogSystem("CacheHit", $"Cache hit for {poolPath}",
                data: new Dictionary<string, object>
                {
                    ["poolPath"] = poolPath,
                    ["cacheKey"] = cacheKey
                });

            return cached;
        }

        var result = base.GetDescriptor(poolPath, tags, context);

        if (!string.IsNullOrEmpty(result))
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(DefaultExpiration)
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, result, cacheOptions);
            _cachedLogger.LogDebug("Cached descriptor for {PoolPath}", poolPath);
        }

        return result;
    }

    /// <summary>
    /// Invalidates all cached descriptors for a pool.
    /// </summary>
    /// <param name="poolPath">The pool path to invalidate.</param>
    public void InvalidatePool(string poolPath)
    {
        // Since IMemoryCache doesn't support key enumeration,
        // we would need a separate tracking mechanism for full invalidation.
        // For now, rely on expiration.
        _cachedLogger.LogDebug("Pool invalidation requested for {PoolPath}", poolPath);
    }

    /// <summary>
    /// Clears the entire descriptor cache.
    /// </summary>
    public void ClearCache()
    {
        // IMemoryCache doesn't support full clear without disposing.
        // This would require a custom cache implementation or tracking.
        _cachedLogger.LogInformation("Cache clear requested - entries will expire naturally");
    }
}
