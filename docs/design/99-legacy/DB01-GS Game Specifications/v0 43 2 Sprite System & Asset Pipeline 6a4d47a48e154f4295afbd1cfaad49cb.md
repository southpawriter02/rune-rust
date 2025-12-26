# v0.43.2: Sprite System & Asset Pipeline

Type: Technical
Description: Implements sprite rendering system: 16×16 PixelSprite data structure, JSON sprite definition format, SpriteService with caching, SkiaSharp bitmap rendering, and sprite scaling support (1x, 3x, 5x). 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1

**Estimated Time:** 6-8 hours

**Group:** Foundation

**Deliverable:** Working sprite rendering system with caching

---

## Executive Summary

v0.43.2 implements the sprite rendering system for Rune & Rust, defining the 16×16 pixel art format, creating the SpriteService for loading and caching sprites, and integrating with SkiaSharp for high-performance rendering.

**What This Delivers:**

- `PixelSprite` data structure (16×16 with color palette)
- JSON sprite definition format
- `SpriteService` with caching
- SkiaSharp bitmap rendering
- Sprite scaling support (1x, 3x, 5x)
- Demo view showing all sprites

**Success Metric:** Can load and display all sprite types at multiple scales without performance issues.

---

## Database Schema Changes

*Optional:* If storing sprites in database instead of JSON files:

```sql
CREATE TABLE SpriteDefinitions (
    SpriteId INT PRIMARY KEY IDENTITY,
    SpriteName VARCHAR(100) NOT NULL UNIQUE,
    SpriteType VARCHAR(50) NOT NULL,  -- Player, Enemy, Item, Terrain, Effect
    PixelData NVARCHAR(MAX) NOT NULL,  -- JSON array of 16 strings
    PaletteData NVARCHAR(MAX) NOT NULL,  -- JSON object of char -> color
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX IX_Sprite_Type (SpriteType)
);
```

**Recommendation:** Use JSON files for initial implementation (easier artist workflow).

---

## Service Implementation

### PixelSprite Data Structure

```csharp
using SkiaSharp;
using System.Text.Json;

namespace RuneAndRust.DesktopUI.Models;

public class PixelSprite
{
    public string Name { get; set; } = string.Empty;
    public string[] PixelData { get; set; } = Array.Empty<string>();  // 16 rows of 16 chars
    public Dictionary<char, string> Palette { get; set; } = new();
    
    public void Validate()
    {
        if (PixelData.Length != 16)
            throw new InvalidOperationException($"Sprite {Name} must have exactly 16 rows");
        
        foreach (var row in PixelData)
        {
            if (row.Length != 16)
                throw new InvalidOperationException($"Sprite {Name} row must have exactly 16 pixels");
        }
        
        // Validate all characters in pixel data are in palette
        var usedChars = PixelData.SelectMany(row => row).Distinct();
        foreach (var ch in usedChars)
        {
            if (ch != ' ' && !Palette.ContainsKey(ch))
                throw new InvalidOperationException($"Sprite {Name} uses character '{ch}' not in palette");
        }
    }
    
    public SKBitmap ToBitmap(int scale = 3)
    {
        var bitmap = new SKBitmap(16 * scale, 16 * scale);
        
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                char pixel = PixelData[y][x];
                if (pixel == ' ') continue;  // Transparent
                
                if (Palette.TryGetValue(pixel, out var colorHex))
                {
                    var color = SKColor.Parse(colorHex);
                    using var paint = new SKPaint { Color = color };
                    canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                }
            }
        }
        
        return bitmap;
    }
}
```

### SpriteService

```csharp
using SkiaSharp;
using Serilog;
using System.Collections.Concurrent;
using System.Text.Json;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface ISpriteService
{
    SKBitmap? GetSpriteBitmap(string spriteName, int scale = 3);
    void RegisterSprite(string name, PixelSprite sprite);
    IEnumerable<string> GetAvailableSprites();
    bool SpriteExists(string name);
}

public class SpriteService : ISpriteService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, PixelSprite> _sprites = new();
    private readonly ConcurrentDictionary<string, SKBitmap> _bitmapCache = new();
    
    public SpriteService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void LoadSpritesFromDirectory(string directoryPath)
    {
        _logger.Information("Loading sprites from {Directory}", directoryPath);
        
        if (!Directory.Exists(directoryPath))
        {
            _logger.Warning("Sprite directory not found: {Directory}", directoryPath);
            return;
        }
        
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
        
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var sprite = JsonSerializer.Deserialize<PixelSprite>(json);
                
                if (sprite != null)
                {
                    sprite.Validate();
                    RegisterSprite([sprite.Name](http://sprite.Name), sprite);
                    _logger.Debug("Loaded sprite: {SpriteName}", [sprite.Name](http://sprite.Name));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load sprite from {File}", file);
            }
        }
        
        _logger.Information("Loaded {Count} sprites", _sprites.Count);
    }
    
    public void RegisterSprite(string name, PixelSprite sprite)
    {
        sprite.Validate();
        _sprites[name] = sprite;
    }
    
    public SKBitmap? GetSpriteBitmap(string spriteName, int scale = 3)
    {
        var cacheKey = $"{spriteName}_{scale}";
        
        if (_bitmapCache.TryGetValue(cacheKey, out var cachedBitmap))
            return cachedBitmap;
        
        if (!_sprites.TryGetValue(spriteName, out var sprite))
        {
            _logger.Warning("Sprite not found: {SpriteName}", spriteName);
            return null;
        }
        
        var bitmap = sprite.ToBitmap(scale);
        _bitmapCache[cacheKey] = bitmap;
        
        return bitmap;
    }
    
    public IEnumerable<string> GetAvailableSprites()
    {
        return _sprites.Keys;
    }
    
    public bool SpriteExists(string name)
    {
        return _sprites.ContainsKey(name);
    }
    
    public void ClearCache()
    {
        foreach (var bitmap in _bitmapCache.Values)
        {
            bitmap.Dispose();
        }
        _bitmapCache.Clear();
    }
}
```

### Sprite Demo ViewModel

```csharp
using ReactiveUI;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using System.Collections.ObjectModel;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SpriteDemoViewModel : ViewModelBase
{
    private readonly ISpriteService _spriteService;
    private int _scale = 3;
    
    public ObservableCollection<string> AvailableSprites { get; } = new();
    
    public int Scale
    {
        get => _scale;
        set => this.RaiseAndSetIfChanged(ref _scale, value);
    }
    
    public SpriteDemoViewModel(ISpriteService spriteService)
    {
        _spriteService = spriteService;
        LoadSprites();
    }
    
    private void LoadSprites()
    {
        AvailableSprites.Clear();
        foreach (var sprite in _spriteService.GetAvailableSprites())
        {
            AvailableSprites.Add(sprite);
        }
    }
}
```

---

## Sprite Format Specification

### JSON Format

```json
{
  "Name": "player_warrior",
  "PixelData": [
    "                ",
    "       ##       ",
    "      ####      ",
    "      ####      ",
    "     HHHHHH     ",
    "    HFFFFFFH    ",
    "    HFSSSSSFH   ",
    "    HFFFFFFFFH  ",
    "     AAAAAAAA   ",
    "     AAAAAAAA   ",
    "      AAAA      ",
    "      LLLL      ",
    "     LL  LL     ",
    "    LL    LL    ",
    "   BB      BB   ",
    "                "
  ],
  "Palette": {
    "#": "#3A3A3A",
    "H": "#8B7355",
    "F": "#FFD1A3",
    "S": "#2C2C2C",
    "A": "#4A90E2",
    "L": "#6B6B6B",
    "B": "#4A3C2A"
  }
}
```

### Character Conventions

- **Space ( ``):** Transparent pixel
- **Palette Keys:** Single ASCII character (A-Z, a-z, 0-9, symbols)
- **Colors:** Hex format `#RRGGBB` or `#RRGGBBAA`

### Recommended Palette

```json
{
  "#": "#000000",  // Black (outlines)
  "F": "#FFD1A3",  // Skin tone
  "H": "#8B4513",  // Brown (hair, leather)
  "A": "#4A90E2",  // Blue (cloth)
  "S": "#C0C0C0",  // Silver (metal)
  "G": "#FFD700",  // Gold
  "R": "#DC143C",  // Red (blood, danger)
  "E": "#228B22",  // Green (nature)
  "P": "#800080",  // Purple (magic)
  "Y": "#FFFF00"   // Yellow (light)
}
```

---

## Integration Points

**With v0.43.1:**

- Register `ISpriteService` in DI container
- Load sprites on application startup

**With v0.43.4+ (Combat UI):**

- Provide sprites for player characters
- Provide sprites for enemies
- Provide sprites for status effects
- Provide sprites for hazards

**With v0.43.10 (Inventory):**

- Provide sprites for items
- Provide sprites for equipment

---

## Functional Requirements

### FR1: Sprite Loading

**Requirement:** Load sprites from JSON files on startup.

**Test:**

```csharp
[Fact]
public void SpriteService_LoadsFromDirectory()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new SpriteService(logger);
    
    service.LoadSpritesFromDirectory("Assets/Sprites");
    
    Assert.True(service.SpriteExists("player_warrior"));
    Assert.Contains("player_warrior", service.GetAvailableSprites());
}
```

### FR2: Sprite Rendering

**Requirement:** Render sprites as SKBitmap at specified scale.

**Test:**

```csharp
[Fact]
public void PixelSprite_RendersToBitmap()
{
    var sprite = new PixelSprite
    {
        Name = "test",
        PixelData = new string[16],
        Palette = new Dictionary<char, string> { { '#', "#000000" } }
    };
    
    for (int i = 0; i < 16; i++)
        sprite.PixelData[i] = "################";
    
    var bitmap = sprite.ToBitmap(scale: 3);
    
    Assert.NotNull(bitmap);
    Assert.Equal(48, bitmap.Width);  // 16 * 3
    Assert.Equal(48, bitmap.Height);
}

[Fact]
public void PixelSprite_RendersTransparentPixels()
{
    var sprite = new PixelSprite
    {
        Name = "test",
        PixelData = new string[16],
        Palette = new Dictionary<char, string> { { '#', "#FF0000" } }
    };
    
    for (int i = 0; i < 16; i++)
        sprite.PixelData[i] = "####        ####";
    
    var bitmap = sprite.ToBitmap(scale: 1);
    
    // Center pixels should be transparent
    var centerPixel = bitmap.GetPixel(8, 8);
    Assert.Equal(SKColors.Transparent, centerPixel);
}
```

### FR3: Sprite Caching

**Requirement:** Cache rendered bitmaps to avoid redundant rendering.

**Test:**

```csharp
[Fact]
public void SpriteService_CachesBitmaps()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new SpriteService(logger);
    
    var sprite = CreateTestSprite();
    service.RegisterSprite("test", sprite);
    
    var bitmap1 = service.GetSpriteBitmap("test", scale: 3);
    var bitmap2 = service.GetSpriteBitmap("test", scale: 3);
    
    // Should return same cached instance
    Assert.Same(bitmap1, bitmap2);
}

[Fact]
public void SpriteService_CachesDifferentScales()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new SpriteService(logger);
    
    var sprite = CreateTestSprite();
    service.RegisterSprite("test", sprite);
    
    var bitmap3x = service.GetSpriteBitmap("test", scale: 3);
    var bitmap5x = service.GetSpriteBitmap("test", scale: 5);
    
    Assert.NotSame(bitmap3x, bitmap5x);
    Assert.Equal(48, bitmap3x.Width);
    Assert.Equal(80, bitmap5x.Width);
}
```

### FR4: Sprite Validation

**Requirement:** Validate sprite format on load.

**Test:**

```csharp
[Fact]
public void PixelSprite_ValidatesRowCount()
{
    var sprite = new PixelSprite
    {
        Name = "invalid",
        PixelData = new string[15],  // Should be 16
        Palette = new()
    };
    
    Assert.Throws<InvalidOperationException>(() => sprite.Validate());
}

[Fact]
public void PixelSprite_ValidatesRowLength()
{
    var sprite = new PixelSprite
    {
        Name = "invalid",
        PixelData = Enumerable.Repeat("###", 16).ToArray(),  // Too short
        Palette = new Dictionary<char, string> { { '#', "#000000" } }
    };
    
    Assert.Throws<InvalidOperationException>(() => sprite.Validate());
}

[Fact]
public void PixelSprite_ValidatesPalette()
{
    var sprite = new PixelSprite
    {
        Name = "invalid",
        PixelData = Enumerable.Repeat("################", 16).ToArray(),
        Palette = new()  // Missing '#' character
    };
    
    Assert.Throws<InvalidOperationException>(() => sprite.Validate());
}
```

---

## Sample Sprites

### Player: Warrior

```json
{
  "Name": "player_warrior",
  "PixelData": [
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
  ],
  "Palette": {
    "H": "#8B4513",
    "#": "#000000",
    "S": "#C0C0C0",
    "F": "#FFD1A3",
    "A": "#4A90E2",
    "L": "#6B6B6B",
    "B": "#4A3C2A"
  }
}
```

### Enemy: Goblin

```json
{
  "Name": "enemy_goblin",
  "PixelData": [
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
  ],
  "Palette": {
    "E": "#2E5C2E",
    "G": "#6B8E23",
    "#": "#000000",
    "Y": "#FFFF00",
    "B": "#4A3C2A",
    "L": "#6B6B6B"
  }
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**PixelSprite Tests:**

- Bitmap rendering at various scales
- Transparent pixel handling
- Validation (row count, length, palette)
- Color parsing and rendering

**SpriteService Tests:**

- Loading from directory
- Sprite registration
- Bitmap caching
- Scale handling
- Error handling for missing sprites

### Performance Tests

**Rendering Performance:**

```csharp
[Fact]
public void SpriteRendering_PerformanceTest()
{
    var logger = new LoggerConfiguration().CreateLogger();
    var service = new SpriteService(logger);
    
    // Register 100 sprites
    for (int i = 0; i < 100; i++)
    {
        service.RegisterSprite($"sprite_{i}", CreateTestSprite());
    }
    
    var sw = Stopwatch.StartNew();
    
    // Render each sprite at 3 scales
    for (int i = 0; i < 100; i++)
    {
        service.GetSpriteBitmap($"sprite_{i}", scale: 1);
        service.GetSpriteBitmap($"sprite_{i}", scale: 3);
        service.GetSpriteBitmap($"sprite_{i}", scale: 5);
    }
    
    sw.Stop();
    
    // Should complete in < 1 second
    Assert.True(sw.ElapsedMilliseconds < 1000);
}
```

---

## Success Criteria

**v0.43.2 is DONE when:**

### ✅ Sprite System

- [ ]  PixelSprite class implemented
- [ ]  SpriteService implemented
- [ ]  JSON loading works
- [ ]  Bitmap rendering works
- [ ]  Caching works

### ✅ Asset Pipeline

- [ ]  Sprite directory structure defined
- [ ]  Sample sprites created (player, enemies, items)
- [ ]  Sprite validation implemented
- [ ]  Error handling for malformed sprites

### ✅ Performance

- [ ]  100 sprites load in < 1 second
- [ ]  Cached sprites return instantly
- [ ]  No memory leaks in rendering
- [ ]  Cache clearing works

### ✅ Testing

- [ ]  Unit tests written (80%+ coverage)
- [ ]  All tests pass
- [ ]  Demo view shows all sprites
- [ ]  Sprites render at multiple scales

---

## Implementation Notes

**NuGet Packages Required:**

```xml
<PackageReference Include="SkiaSharp" Version="2.88.3" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

**Asset Organization:**

```
Assets/Sprites/
├── Players/
│   ├── warrior.json
│   ├── mage.json
│   └── rogue.json
├── Enemies/
│   ├── goblin.json
│   ├── draugr.json
│   └── boss_witch_king.json
├── Items/
│   ├── sword.json
│   ├── potion.json
│   └── scroll.json
├── Terrain/
│   ├── cover_physical.json
│   └── hazard_fire.json
└── Effects/
    ├── status_bleeding.json
    └── status_blessed.json
```

---

**Sprite system complete. Ready for navigation in v0.43.3.**