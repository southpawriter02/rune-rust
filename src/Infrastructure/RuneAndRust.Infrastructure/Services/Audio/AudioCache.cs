namespace RuneAndRust.Infrastructure.Services.Audio;

using Microsoft.Extensions.Logging;

/// <summary>
/// Caches loaded audio files with LRU (Least Recently Used) eviction.
/// </summary>
/// <remarks>
/// <para>
/// The cache uses a GetOrLoad pattern:
/// <list type="bullet">
///   <item><description>Returns cached audio if available</description></item>
///   <item><description>Loads and caches if not present</description></item>
///   <item><description>Evicts oldest entries when full (default 50 files)</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioCache
{
    private readonly int _maxSize;
    private readonly Dictionary<string, CachedAudio> _cache;
    private readonly LinkedList<string> _accessOrder;
    private readonly ILogger<AudioCache>? _logger;

    /// <summary>
    /// Gets the current number of cached items.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Gets the maximum cache size.
    /// </summary>
    public int MaxSize => _maxSize;

    /// <summary>
    /// Creates a new audio cache.
    /// </summary>
    /// <param name="maxSize">Maximum number of cached files.</param>
    /// <param name="logger">Optional logger.</param>
    public AudioCache(int maxSize = 50, ILogger<AudioCache>? logger = null)
    {
        _maxSize = maxSize;
        _cache = new Dictionary<string, CachedAudio>();
        _accessOrder = new LinkedList<string>();
        _logger = logger;
    }

    /// <summary>
    /// Gets a cached audio file, loading if necessary.
    /// </summary>
    /// <param name="path">Path to the audio file.</param>
    /// <returns>Cached audio, or null if loading failed.</returns>
    public CachedAudio? GetOrLoad(string path)
    {
        // Check if already cached
        if (_cache.TryGetValue(path, out var cached))
        {
            // Move to front of LRU list
            _accessOrder.Remove(path);
            _accessOrder.AddFirst(path);
            _logger?.LogDebug("Cache hit: {Path}", path);
            return cached;
        }

        // Load the audio file
        var audio = LoadAudioFile(path);
        if (audio is null)
        {
            _logger?.LogWarning("Failed to load audio: {Path}", path);
            return null;
        }

        // Evict oldest entries if cache is full
        while (_cache.Count >= _maxSize && _accessOrder.Last is not null)
        {
            var oldest = _accessOrder.Last.Value;
            _accessOrder.RemoveLast();

            if (_cache.TryGetValue(oldest, out var evicted))
            {
                evicted.Dispose();
                _cache.Remove(oldest);
                _logger?.LogDebug("Evicted from cache: {Path}", oldest);
            }
        }

        // Add to cache
        _cache[path] = audio;
        _accessOrder.AddFirst(path);
        _logger?.LogDebug("Cached audio: {Path} (count={Count})", path, _cache.Count);

        return audio;
    }

    /// <summary>
    /// Checks if a path is cached.
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns>True if cached.</returns>
    public bool Contains(string path) => _cache.ContainsKey(path);

    /// <summary>
    /// Clears the cache, disposing all cached audio.
    /// </summary>
    public void Clear()
    {
        foreach (var audio in _cache.Values)
        {
            audio.Dispose();
        }

        _cache.Clear();
        _accessOrder.Clear();
        _logger?.LogDebug("Cache cleared");
    }

    /// <summary>
    /// Loads an audio file from disk.
    /// </summary>
    /// <param name="path">Path to the audio file.</param>
    /// <returns>Cached audio, or null if file doesn't exist.</returns>
    private static CachedAudio? LoadAudioFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return new CachedAudio(path);
    }
}
