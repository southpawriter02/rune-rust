using RuneAndRust.DesktopUI.Models;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Implementation of sprite service with caching and JSON loading.
/// </summary>
public class SpriteService : ISpriteService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, PixelSprite> _sprites = new();
    private readonly ConcurrentDictionary<string, SKBitmap> _bitmapCache = new();

    public SpriteService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void LoadSpritesFromDirectory(string directoryPath)
    {
        _logger.Information("Loading sprites from directory: {Directory}", directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            _logger.Warning("Sprite directory not found: {Directory}. Creating sample sprites.", directoryPath);

            // Create directory and sample sprites for demo purposes
            Directory.CreateDirectory(directoryPath);
            CreateSampleSprites(directoryPath);

            // Now load the sample sprites we just created
            LoadSpritesFromDirectoryInternal(directoryPath);
            return;
        }

        LoadSpritesFromDirectoryInternal(directoryPath);
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
            return cachedBitmap;
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
            var bitmap = sprite.ToBitmap(scale);
            _bitmapCache[cacheKey] = bitmap;
            _logger.Debug("Rendered and cached sprite: {SpriteName} at scale {Scale}", spriteName, scale);
            return bitmap;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to render sprite: {SpriteName}", spriteName);
            return null;
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
        _logger.Information("Clearing sprite bitmap cache ({Count} cached bitmaps)", _bitmapCache.Count);

        foreach (var bitmap in _bitmapCache.Values)
        {
            bitmap.Dispose();
        }

        _bitmapCache.Clear();
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
