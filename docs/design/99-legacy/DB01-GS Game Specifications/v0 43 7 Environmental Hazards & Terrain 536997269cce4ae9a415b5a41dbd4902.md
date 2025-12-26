# v0.43.7: Environmental Hazards & Terrain

Type: UI
Description: Visualization of environmental elements from v0.22: cover indicators (physical/metaphysical), hazard visualization with animated effects, elevation display, terrain type indicators. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.4-v0.43.6, v0.22 (Environmental Combat)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.4-v0.43.6, v0.22 (Environmental Combat)

**Estimated Time:** 5-7 hours

**Group:** Combat UI

**Deliverable:** Visual representation of hazards, cover, and terrain

---

## Executive Summary

v0.43.7 implements visualization of environmental elements from v0.22, including cover indicators, hazards (fire, poison, etc.), elevation differences, and terrain types.

**What This Delivers:**

- Cover indicators (physical ▓, metaphysical ▒)
- Hazard visualization with animated effects
- Elevation display
- Terrain type indicators
- Integration with v0.22 environmental system
- Hazard tooltips with damage info

**Success Metric:** All environmental elements clearly visible and distinguishable on combat grid.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### HazardVisualizationService

```csharp
using RuneAndRust.Core.Combat;
using SkiaSharp;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface IHazardVisualizationService
{
    SKBitmap? GetHazardSprite(HazardType type);
    SKColor GetHazardOverlayColor(HazardType type);
    string GetHazardDescription(EnvironmentalHazard hazard);
    bool ShouldAnimateHazard(HazardType type);
}

public class HazardVisualizationService : IHazardVisualizationService
{
    private readonly ISpriteService _spriteService;
    private readonly Dictionary<HazardType, string> _hazardSprites = new()
    {
        { [HazardType.Fire](http://HazardType.Fire), "hazard_fire" },
        { HazardType.Poison, "hazard_poison" },
        { [HazardType.Ice](http://HazardType.Ice), "hazard_ice" },
        { HazardType.Spike, "hazard_spike" },
        { HazardType.Acid, "hazard_acid" },
        { HazardType.Lightning, "hazard_lightning" },
        { HazardType.Darkness, "hazard_darkness" },
        { HazardType.Corruption, "hazard_corruption" }
    };
    
    public HazardVisualizationService(ISpriteService spriteService)
    {
        _spriteService = spriteService;
    }
    
    public SKBitmap? GetHazardSprite(HazardType type)
    {
        if (_hazardSprites.TryGetValue(type, out var spriteName))
        {
            return _spriteService.GetSpriteBitmap(spriteName, scale: 3);
        }
        return null;
    }
    
    public SKColor GetHazardOverlayColor(HazardType type)
    {
        return type switch
        {
            [HazardType.Fire](http://HazardType.Fire) => [SKColors.Orange](http://SKColors.Orange).WithAlpha(100),
            HazardType.Poison => SKColors.Purple.WithAlpha(80),
            [HazardType.Ice](http://HazardType.Ice) => SKColors.Cyan.WithAlpha(90),
            HazardType.Spike => SKColors.Gray.WithAlpha(70),
            HazardType.Acid => SKColors.LimeGreen.WithAlpha(85),
            HazardType.Lightning => SKColors.Yellow.WithAlpha(120),
            HazardType.Darkness => [SKColors.Black](http://SKColors.Black).WithAlpha(150),
            HazardType.Corruption => SKColors.Magenta.WithAlpha(100),
            _ => [SKColors.Red](http://SKColors.Red).WithAlpha(80)
        };
    }
    
    public string GetHazardDescription(EnvironmentalHazard hazard)
    {
        var desc = hazard.Type switch
        {
            [HazardType.Fire](http://HazardType.Fire) => "Deals fire damage to units entering or ending turn in area",
            HazardType.Poison => "Applies poison status to units in area",
            [HazardType.Ice](http://HazardType.Ice) => "Reduces movement and increases fall risk",
            HazardType.Spike => "Deals damage when entering area",
            HazardType.Acid => "Deals corrosive damage over time",
            HazardType.Lightning => "Random chance of shock damage",
            HazardType.Darkness => "Reduces accuracy and vision",
            HazardType.Corruption => "Increases Psychic Stress over time",
            _ => "Unknown hazard"
        };
        
        return $"{[hazard.Name](http://hazard.Name)}\n{desc}\nDamage: {hazard.DamagePerTurn} per turn";
    }
    
    public bool ShouldAnimateHazard(HazardType type)
    {
        return type is [HazardType.Fire](http://HazardType.Fire) or HazardType.Lightning or HazardType.Corruption;
    }
}
```

### Enhanced CombatGridControl (Hazards & Cover)

```csharp
// Add to CombatGridControl from v0.43.4

private readonly IHazardVisualizationService _hazardService;
private float _animationTime = 0f;

public override void Render(DrawingContext context)
{
    base.Render(context);
    
    if (Grid == null) return;
    
    using var skContext = context as ISkiaDrawingContextImpl;
    if (skContext == null) return;
    
    var canvas = skContext.SkCanvas;
    
    DrawGridBackground(canvas);
    DrawTerrainTypes(canvas);
    DrawElevationIndicators(canvas);
    DrawHazards(canvas);
    DrawCover(canvas);
    DrawHighlightedCells(canvas);
    DrawGridLines(canvas);
    DrawUnits(canvas);
    
    // Increment animation time
    _animationTime += 0.05f;
    if (_animationTime > Math.PI * 2) _animationTime = 0f;
}

private void DrawCover(SKCanvas canvas)
{
    foreach (var tile in Grid!.Tiles.Values)
    {
        if (tile.CoverType == CoverType.None) continue;
        
        var x = tile.Position.X * CellSize;
        var y = tile.Position.Y * CellSize;
        
        // Draw cover icon in corner
        var coverIcon = tile.CoverType == CoverType.Physical ? "▓" : "▒";
        var coverColor = tile.CoverType == CoverType.Physical ? 
            SKColors.DarkGray : SKColors.LightBlue;
        
        using var paint = new SKPaint
        {
            Color = coverColor,
            TextSize = 24,
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        
        canvas.DrawText(coverIcon, x + 5, y + 25, paint);
        
        // Draw cover percentage
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 10,
            IsAntialias = true
        };
        
        var coverPercent = tile.CoverAmount;
        canvas.DrawText($"{coverPercent}%", x + 5, y + CellSize - 5, textPaint);
    }
}

private void DrawHazards(SKCanvas canvas)
{
    foreach (var tile in Grid!.Tiles.Values)
    {
        if (tile.Hazard == null) continue;
        
        var x = tile.Position.X * CellSize;
        var y = tile.Position.Y * CellSize;
        
        // Draw hazard overlay
        var overlayColor = _hazardService.GetHazardOverlayColor(tile.Hazard.Type);
        
        // Animate if appropriate
        if (_hazardService.ShouldAnimateHazard(tile.Hazard.Type))
        {
            var pulse = (float)Math.Sin(_animationTime) * 0.3f + 0.7f;
            overlayColor = overlayColor.WithAlpha((byte)(overlayColor.Alpha * pulse));
        }
        
        using var overlayPaint = new SKPaint { Color = overlayColor };
        canvas.DrawRect(x, y, CellSize, CellSize, overlayPaint);
        
        // Draw hazard sprite
        var hazardSprite = _hazardService.GetHazardSprite(tile.Hazard.Type);
        if (hazardSprite != null)
        {
            var spriteX = x + (CellSize - 48) / 2;
            var spriteY = y + (CellSize - 48) / 2;
            canvas.DrawBitmap(hazardSprite, new SKPoint(spriteX, spriteY));
        }
    }
}

private void DrawElevationIndicators(SKCanvas canvas)
{
    foreach (var tile in Grid!.Tiles.Values)
    {
        if (tile.Elevation == 0) continue;
        
        var x = tile.Position.X * CellSize;
        var y = tile.Position.Y * CellSize;
        
        // Draw elevation in top-right corner
        using var paint = new SKPaint
        {
            Color = tile.Elevation > 0 ? SKColors.Yellow : [SKColors.Blue](http://SKColors.Blue),
            TextSize = 14,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        
        var elevText = tile.Elevation > 0 ? $"+{tile.Elevation}" : tile.Elevation.ToString();
        canvas.DrawText(elevText, x + CellSize - 5, y + 20, paint);
    }
}

private void DrawTerrainTypes(SKCanvas canvas)
{
    foreach (var tile in Grid!.Tiles.Values)
    {
        if (tile.TerrainType == TerrainType.Normal) continue;
        
        var x = tile.Position.X * CellSize;
        var y = tile.Position.Y * CellSize;
        
        // Tint cell based on terrain type
        var tintColor = tile.TerrainType switch
        {
            TerrainType.Difficult => SKColors.Brown.WithAlpha(40),
            TerrainType.Water => [SKColors.Blue](http://SKColors.Blue).WithAlpha(60),
            TerrainType.Mud => SKColors.SaddleBrown.WithAlpha(50),
            TerrainType.Stone => SKColors.Gray.WithAlpha(30),
            _ => SKColors.Transparent
        };
        
        if (tintColor != SKColors.Transparent)
        {
            using var paint = new SKPaint { Color = tintColor };
            canvas.DrawRect(x, y, CellSize, CellSize, paint);
        }
    }
}
```

---

## Sample Hazard Sprites

### Fire Hazard

```json
{
  "Name": "hazard_fire",
  "PixelData": [
    "                ",
    "                ",
    "      YY        ",
    "     YYYY    Y  ",
    "    YYRRYY  YY  ",
    "   YYRRRRYY YY  ",
    "   YYRRRRRRYY   ",
    "  YYRRRRRRRRRY  ",
    "  YRRRRRRRRRRR  ",
    "  YRRRRRRRRRR   ",
    "   YRRRRRRRR    ",
    "    YRRRRRR     ",
    "     RRRRR      ",
    "      RRR       ",
    "                ",
    "                "
  ],
  "Palette": {
    "Y": "#FFFF00",
    "R": "#FF4500"
  }
}
```

### Poison Cloud

```json
{
  "Name": "hazard_poison",
  "PixelData": [
    "                ",
    "   PP      PP   ",
    "  PPPP    PPPP  ",
    "  PPGPP  PPGPP  ",
    " PPGGPPPPPGGPP  ",
    " PGGGGPPGGGGP   ",
    "  PGGGGGGGGPP   ",
    "  PPGGGGGGPP    ",
    "   PPGGGGPP     ",
    "    PPGGPP      ",
    "     PPPP       ",
    "      PP        ",
    "                ",
    "                ",
    "                ",
    "                "
  ],
  "Palette": {
    "P": "#9400D3",
    "G": "#00FF00"
  }
}
```

---

## Integration Points

**With v0.22 (Environmental Combat):**

- Displays all hazard types
- Shows cover percentages
- Elevation indicators
- Terrain type tints

**With v0.43.4 (Grid Control):**

- Renders hazards on grid cells
- Overlays on battlefield

---

## Success Criteria

**v0.43.7 is DONE when:**

### ✅ Cover Display

- [ ]  Physical cover (▓) visible
- [ ]  Metaphysical cover (▒) visible
- [ ]  Cover percentage shown
- [ ]  Positioned in cell corner

### ✅ Hazard Display

- [ ]  All 8 hazard types render
- [ ]  Animated hazards pulse
- [ ]  Overlay tints cells
- [ ]  Sprites centered in cells

### ✅ Terrain & Elevation

- [ ]  Elevation shown (+/-)
- [ ]  Terrain types tinted
- [ ]  No visual clutter

---

**Environmental elements complete. Ready for combat animations in v0.43.8.**