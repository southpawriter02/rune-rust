using RuneAndRust.DesktopUI.Models;
using RuneAndRust.DesktopUI.Styles;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.21: Implementation of sprite service with caching, memory optimization, and JSON loading.
/// Includes cache size limits with LRU eviction for performance optimization.
/// </summary>
public class SpriteService : ISpriteService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, PixelSprite> _sprites = new();
    private readonly ConcurrentDictionary<string, CachedBitmap> _bitmapCache = new();
    private readonly object _cacheLock = new();
    private long _currentCacheSize;

    /// <summary>
    /// Maximum cache size in bytes (100 MB default from UIConstants).
    /// </summary>
    public long MaxCacheSize { get; set; } = UIConstants.MaxCacheSizeBytes;

    /// <summary>
    /// Current cache size in bytes.
    /// </summary>
    public long CurrentCacheSize => _currentCacheSize;

    /// <summary>
    /// Number of cached bitmaps.
    /// </summary>
    public int CachedBitmapCount => _bitmapCache.Count;

    /// <summary>
    /// Internal wrapper for cached bitmaps with metadata.
    /// </summary>
    private class CachedBitmap
    {
        public SKBitmap Bitmap { get; }
        public long ByteCount { get; }
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }

        public CachedBitmap(SKBitmap bitmap)
        {
            Bitmap = bitmap;
            ByteCount = bitmap.ByteCount;
            LastAccessed = DateTime.UtcNow;
            AccessCount = 1;
        }

        public void Touch()
        {
            LastAccessed = DateTime.UtcNow;
            AccessCount++;
        }
    }

    public SpriteService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void LoadSpritesFromDirectory(string directoryPath)
    {
        var sw = Stopwatch.StartNew();
        _logger.Information("Loading sprites from directory: {Directory}", directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            _logger.Warning("Sprite directory not found: {Directory}. Creating sample sprites.", directoryPath);

            // Create directory and sample sprites for demo purposes
            Directory.CreateDirectory(directoryPath);
            CreateSampleSprites(directoryPath);

            // Now load the sample sprites we just created
            LoadSpritesFromDirectoryInternal(directoryPath);
            sw.Stop();
            _logger.Information("Sprite loading completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
            return;
        }

        LoadSpritesFromDirectoryInternal(directoryPath);
        sw.Stop();
        _logger.Information("Sprite loading completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }

    private void LoadSpritesFromDirectoryInternal(string directoryPath)
    {
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);

        if (jsonFiles.Length == 0)
        {
            _logger.Warning("No sprite JSON files found in {Directory}", directoryPath);
            return;
        }

        int loadedCount = 0;
        int errorCount = 0;

        foreach (var file in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var sprite = JsonSerializer.Deserialize<PixelSprite>(json);

                if (sprite != null)
                {
                    sprite.Validate();
                    RegisterSprite(sprite.Name, sprite);
                    loadedCount++;
                    _logger.Debug("Loaded sprite: {SpriteName} from {File}", sprite.Name, Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.Error(ex, "Failed to load sprite from {File}", file);
            }
        }

        _logger.Information("Sprite loading complete: {LoadedCount} loaded, {ErrorCount} errors",
            loadedCount, errorCount);
    }

    /// <inheritdoc/>
    public void RegisterSprite(string name, PixelSprite sprite)
    {
        if (sprite == null)
            throw new ArgumentNullException(nameof(sprite));

        sprite.Validate();
        _sprites[name] = sprite;
        _logger.Debug("Registered sprite: {SpriteName}", name);
    }

    /// <inheritdoc/>
    public SKBitmap? GetSpriteBitmap(string spriteName, int scale = 3)
    {
        if (string.IsNullOrEmpty(spriteName))
            throw new ArgumentException("Sprite name cannot be null or empty", nameof(spriteName));

        if (scale < 1)
            throw new ArgumentException("Scale must be at least 1", nameof(scale));

        // Check cache first
        var cacheKey = $"{spriteName}_{scale}";
        if (_bitmapCache.TryGetValue(cacheKey, out var cachedBitmap))
        {
            cachedBitmap.Touch();
            return cachedBitmap.Bitmap;
        }

        // Sprite not in cache, need to render
        if (!_sprites.TryGetValue(spriteName, out var sprite))
        {
            _logger.Warning("Sprite not found: {SpriteName}", spriteName);
            return null;
        }

        // Render and cache
        try
        {
            var sw = Stopwatch.StartNew();
            var bitmap = sprite.ToBitmap(scale);
            sw.Stop();

            // Log performance warning if render took too long
            if (sw.ElapsedMilliseconds > UIConstants.TargetFrameTimeMs)
            {
                _logger.Warning("Sprite render took {ElapsedMs}ms (target: {TargetMs}ms) for {SpriteName}",
                    sw.ElapsedMilliseconds, UIConstants.TargetFrameTimeMs, spriteName);
            }

            // Add to cache with eviction if needed
            AddToCache(cacheKey, bitmap);

            _logger.Debug("Rendered and cached sprite: {SpriteName} at scale {Scale} in {ElapsedMs}ms",
                spriteName, scale, sw.ElapsedMilliseconds);
            return bitmap;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to render sprite: {SpriteName}", spriteName);
            return null;
        }
    }

    /// <summary>
    /// Adds a bitmap to the cache, evicting old entries if necessary.
    /// </summary>
    private void AddToCache(string cacheKey, SKBitmap bitmap)
    {
        var cached = new CachedBitmap(bitmap);

        lock (_cacheLock)
        {
            // Evict old entries if cache is full
            while (_currentCacheSize + cached.ByteCount > MaxCacheSize && _bitmapCache.Count > 0)
            {
                EvictOldestCacheEntry();
            }

            // Add new entry
            if (_bitmapCache.TryAdd(cacheKey, cached))
            {
                _currentCacheSize += cached.ByteCount;
            }
        }
    }

    /// <summary>
    /// Evicts the oldest (LRU) cache entry.
    /// </summary>
    private void EvictOldestCacheEntry()
    {
        // Find the oldest entry (LRU)
        string? oldestKey = null;
        DateTime oldestTime = DateTime.MaxValue;

        foreach (var kvp in _bitmapCache)
        {
            if (kvp.Value.LastAccessed < oldestTime)
            {
                oldestTime = kvp.Value.LastAccessed;
                oldestKey = kvp.Key;
            }
        }

        if (oldestKey != null && _bitmapCache.TryRemove(oldestKey, out var evicted))
        {
            _currentCacheSize -= evicted.ByteCount;
            evicted.Bitmap.Dispose();
            _logger.Debug("Evicted cache entry: {CacheKey} (last accessed: {LastAccessed})",
                oldestKey, oldestTime);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAvailableSprites()
    {
        return _sprites.Keys.OrderBy(k => k);
    }

    /// <inheritdoc/>
    public bool SpriteExists(string name)
    {
        return _sprites.ContainsKey(name);
    }

    /// <inheritdoc/>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _logger.Information("Clearing sprite bitmap cache ({Count} cached bitmaps, {SizeMB:F2} MB)",
                _bitmapCache.Count, _currentCacheSize / (1024.0 * 1024.0));

            foreach (var cached in _bitmapCache.Values)
            {
                cached.Bitmap.Dispose();
            }

            _bitmapCache.Clear();
            _currentCacheSize = 0;
        }
    }

    /// <summary>
    /// v0.43.21: Gets cache statistics for performance monitoring.
    /// </summary>
    public CacheStatistics GetCacheStatistics()
    {
        lock (_cacheLock)
        {
            var totalAccesses = _bitmapCache.Values.Sum(c => c.AccessCount);
            var avgAccesses = _bitmapCache.Count > 0
                ? _bitmapCache.Values.Average(c => c.AccessCount)
                : 0;

            return new CacheStatistics
            {
                CachedItemCount = _bitmapCache.Count,
                CurrentSizeBytes = _currentCacheSize,
                MaxSizeBytes = MaxCacheSize,
                TotalAccesses = totalAccesses,
                AverageAccessesPerItem = avgAccesses,
                UtilizationPercent = MaxCacheSize > 0 ? (double)_currentCacheSize / MaxCacheSize * 100 : 0
            };
        }
    }

    /// <summary>
    /// Creates sample sprite JSON files for demonstration.
    /// </summary>
    private void CreateSampleSprites(string basePath)
    {
        _logger.Information("Creating sample sprite files in {Path}", basePath);

        // Create subdirectories
        Directory.CreateDirectory(Path.Combine(basePath, "Players"));
        Directory.CreateDirectory(Path.Combine(basePath, "Enemies"));
        Directory.CreateDirectory(Path.Combine(basePath, "Items"));
        Directory.CreateDirectory(Path.Combine(basePath, "Effects"));

        // Create sample sprites
        CreateSpriteFile(basePath, "Players", "warrior", new PixelSprite
        {
            Name = "player_warrior",
            PixelData = new[]
            {
                "                ",
                "       HH       ",
                "      HHHH      ",
                "      H##H      ",
                "     SSSSSS     ",
                "    SFFFFFFS    ",
                "    SF####FS    ",
                "     FFFFFF     ",
                "     AAAAAA     ",
                "     AAAAAA     ",
                "      AAAA      ",
                "      LLLL      ",
                "     LL  LL     ",
                "    LL    LL    ",
                "   BB      BB   ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'H', "#8B4513" },  // Brown hair
                { '#', "#000000" },  // Black outline
                { 'S', "#C0C0C0" },  // Silver armor
                { 'F', "#FFD1A3" },  // Skin
                { 'A', "#4A90E2" },  // Blue tunic
                { 'L', "#6B6B6B" },  // Gray pants
                { 'B', "#4A3C2A" }   // Brown boots
            }
        });

        CreateSpriteFile(basePath, "Enemies", "goblin", new PixelSprite
        {
            Name = "enemy_goblin",
            PixelData = new[]
            {
                "                ",
                "     E    E     ",
                "     EE  EE     ",
                "      EEEE      ",
                "     GGGGGG     ",
                "    GG####GG    ",
                "    G##YY##G    ",
                "     GGGGGG     ",
                "      GGGG      ",
                "     BBBBBB     ",
                "      BBBB      ",
                "      LLLL      ",
                "     LL  LL     ",
                "    LL    LL    ",
                "   ##      ##   ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'E', "#2E5C2E" },  // Dark green ears
                { 'G', "#6B8E23" },  // Olive green skin
                { '#', "#000000" },  // Black outline
                { 'Y', "#FFFF00" },  // Yellow eyes
                { 'B', "#4A3C2A" },  // Brown rags
                { 'L', "#6B6B6B" }   // Gray legs
            }
        });

        CreateSpriteFile(basePath, "Items", "sword", new PixelSprite
        {
            Name = "item_sword",
            PixelData = new[]
            {
                "                ",
                "        S       ",
                "       SSS      ",
                "      SSSSS     ",
                "     SSSSSSS    ",
                "      SSSSS     ",
                "       SSS      ",
                "        S       ",
                "        H       ",
                "        H       ",
                "       HHH      ",
                "       GGG      ",
                "       GGG      ",
                "        G       ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'S', "#C0C0C0" },  // Silver blade
                { 'H', "#8B4513" },  // Brown handle
                { 'G', "#FFD700" }   // Gold pommel
            }
        });

        CreateSpriteFile(basePath, "Items", "potion", new PixelSprite
        {
            Name = "item_potion",
            PixelData = new[]
            {
                "                ",
                "                ",
                "      ####      ",
                "      ####      ",
                "     ######     ",
                "     RRRRRR     ",
                "    RRRRRRRR    ",
                "    RRRRRRRR    ",
                "    RRRRRRRR    ",
                "    RRRRRRRR    ",
                "     RRRRRR     ",
                "     RRRRRR     ",
                "      RRRR      ",
                "                ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { '#', "#6B6B6B" },  // Gray glass/cork
                { 'R', "#DC143C" }   // Red liquid
            }
        });

        CreateSpriteFile(basePath, "Effects", "status_blessed", new PixelSprite
        {
            Name = "effect_blessed",
            PixelData = new[]
            {
                "                ",
                "       Y        ",
                "       Y        ",
                "   Y   Y   Y    ",
                "    Y YYY Y     ",
                "     YYYYY      ",
                "      YYY       ",
                "   Y  YYY  Y    ",
                "    YYYYYYY     ",
                "     YYYYY      ",
                "      YYY       ",
                "       Y        ",
                "       Y        ",
                "                ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'Y', "#FFD700" }   // Golden blessed effect
            }
        });

        _logger.Information("Created {Count} sample sprite files", 5);
    }

    private void CreateSpriteFile(string basePath, string subfolder, string filename, PixelSprite sprite)
    {
        var filePath = Path.Combine(basePath, subfolder, $"{filename}.json");
        var json = JsonSerializer.Serialize(sprite, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }
}

/// <summary>
/// v0.43.21: Cache statistics for performance monitoring.
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Number of items in cache.
    /// </summary>
    public int CachedItemCount { get; set; }

    /// <summary>
    /// Current cache size in bytes.
    /// </summary>
    public long CurrentSizeBytes { get; set; }

    /// <summary>
    /// Maximum cache size in bytes.
    /// </summary>
    public long MaxSizeBytes { get; set; }

    /// <summary>
    /// Total number of cache accesses.
    /// </summary>
    public int TotalAccesses { get; set; }

    /// <summary>
    /// Average number of accesses per item.
    /// </summary>
    public double AverageAccessesPerItem { get; set; }

    /// <summary>
    /// Cache utilization percentage.
    /// </summary>
    public double UtilizationPercent { get; set; }

    /// <summary>
    /// Current cache size in MB.
    /// </summary>
    public double CurrentSizeMB => CurrentSizeBytes / (1024.0 * 1024.0);

    /// <summary>
    /// Maximum cache size in MB.
    /// </summary>
    public double MaxSizeMB => MaxSizeBytes / (1024.0 * 1024.0);
}
