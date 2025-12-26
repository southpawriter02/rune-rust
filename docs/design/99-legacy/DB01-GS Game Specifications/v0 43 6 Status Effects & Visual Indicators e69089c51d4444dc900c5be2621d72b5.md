# v0.43.6: Status Effects & Visual Indicators

Type: UI
Description: Visual representation of status effects from v0.21.3: status effect icon system, HP bar overlay on units, status effect tooltips with duration, buff/debuff visual distinction. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.4-v0.43.5, v0.21.3 (Status Effects System)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.4-v0.43.5, v0.21.3 (Status Effects System)

**Estimated Time:** 5-7 hours

**Group:** Combat UI

**Deliverable:** Status effect icons, tooltips, and visual indicators

---

## Executive Summary

v0.43.6 implements visual representation of status effects from v0.21.3, including icons, tooltips with full effect details, HP bars, and buff/debuff indicators on units.

**What This Delivers:**

- Status effect icon system (sprites for all effects)
- HP bar overlay on units
- Status effect tooltips with remaining duration
- Buff/debuff visual distinction
- Icon positioning and stacking
- Integration with v0.21.3 status system

**Success Metric:** All status effects visible and identifiable at a glance with detailed information on hover.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### StatusEffectIconService

```csharp
using RuneAndRust.Core.Combat;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using SkiaSharp;

namespace [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);

public interface IStatusEffectIconService
{
    SKBitmap? GetStatusEffectIcon(StatusEffectType type);
    string GetStatusEffectDescription(StatusEffect effect);
    Color GetStatusEffectColor(StatusEffectType type);
}

public class StatusEffectIconService : IStatusEffectIconService
{
    private readonly ISpriteService _spriteService;
    private readonly Dictionary<StatusEffectType, string> _iconMap = new()
    {
        { StatusEffectType.Bleeding, "status_bleeding" },
        { StatusEffectType.Poisoned, "status_poisoned" },
        { StatusEffectType.Stunned, "status_stunned" },
        { StatusEffectType.Blessed, "status_blessed" },
        { StatusEffectType.Shielded, "status_shielded" },
        { StatusEffectType.Burning, "status_burning" },
        { StatusEffectType.Frozen, "status_frozen" },
        { StatusEffectType.Weakened, "status_weakened" },
        { StatusEffectType.Strengthened, "status_strengthened" },
        { StatusEffectType.Hasted, "status_hasted" },
        { StatusEffectType.Slowed, "status_slowed" },
        { StatusEffectType.Corrupted, "status_corrupted" },
        { StatusEffectType.Regenerating, "status_regenerating" }
    };
    
    public StatusEffectIconService(ISpriteService spriteService)
    {
        _spriteService = spriteService;
    }
    
    public SKBitmap? GetStatusEffectIcon(StatusEffectType type)
    {
        if (_iconMap.TryGetValue(type, out var spriteName))
        {
            return _spriteService.GetSpriteBitmap(spriteName, scale: 2); // 32x32 for icons
        }
        return null;
    }
    
    public string GetStatusEffectDescription(StatusEffect effect)
    {
        var desc = effect.Type switch
        {
            StatusEffectType.Bleeding => $"Taking {effect.Magnitude} damage per turn",
            StatusEffectType.Poisoned => $"Taking {effect.Magnitude} poison damage per turn",
            StatusEffectType.Stunned => "Cannot act",
            StatusEffectType.Blessed => $"+{effect.Magnitude}% to all rolls",
            StatusEffectType.Shielded => $"Absorbing {effect.Magnitude} damage",
            StatusEffectType.Burning => $"Taking {effect.Magnitude} fire damage per turn",
            StatusEffectType.Frozen => "Movement reduced, vulnerable to attacks",
            StatusEffectType.Weakened => $"-{effect.Magnitude}% damage dealt",
            StatusEffectType.Strengthened => $"+{effect.Magnitude}% damage dealt",
            StatusEffectType.Hasted => $"+{effect.Magnitude} to Speed",
            StatusEffectType.Slowed => $"-{effect.Magnitude} to Speed",
            StatusEffectType.Corrupted => $"Psychic Stress increasing by {effect.Magnitude} per turn",
            StatusEffectType.Regenerating => $"Healing {effect.Magnitude} HP per turn",
            _ => "Unknown effect"
        };
        
        return $"{[effect.Name](http://effect.Name)}: {desc}\nRemaining: {effect.RemainingDuration} turns";
    }
    
    public Color GetStatusEffectColor(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Bleeding => Colors.DarkRed,
            StatusEffectType.Poisoned => Colors.Purple,
            StatusEffectType.Stunned => Colors.Gray,
            StatusEffectType.Blessed => [Colors.Gold](http://Colors.Gold),
            StatusEffectType.Shielded => Colors.LightBlue,
            StatusEffectType.Burning => Colors.OrangeRed,
            StatusEffectType.Frozen => Colors.Cyan,
            StatusEffectType.Weakened => Colors.Brown,
            StatusEffectType.Strengthened => [Colors.Red](http://Colors.Red),
            StatusEffectType.Hasted => Colors.Yellow,
            StatusEffectType.Slowed => Colors.SlateGray,
            StatusEffectType.Corrupted => Colors.DarkMagenta,
            StatusEffectType.Regenerating => Colors.LimeGreen,
            _ => Colors.White
        };
    }
}
```

### Enhanced CombatGridControl (Status Effects Overlay)

```csharp
// Add to CombatGridControl from v0.43.4

private readonly IStatusEffectIconService _statusEffectIconService;

private void DrawUnit(SKCanvas canvas, GridPosition pos, Combatant unit)
{
    var spriteName = GetSpriteNameForUnit(unit);
    var sprite = _spriteService.GetSpriteBitmap(spriteName, scale: 3);
    
    if (sprite == null) return;
    
    var x = pos.X * CellSize + (CellSize - SpriteSize) / 2;
    var y = pos.Y * CellSize + (CellSize - SpriteSize) / 2;
    
    canvas.DrawBitmap(sprite, new SKPoint(x, y));
    
    // Draw HP bar
    DrawHPBar(canvas, x, y, unit);
    
    // Draw status effects
    DrawStatusEffects(canvas, x, y, unit);
}

private void DrawHPBar(SKCanvas canvas, float x, float y, Combatant unit)
{
    const int barWidth = 48;
    const int barHeight = 6;
    const int barY = -8; // Above sprite
    
    // Background (red)
    using var bgPaint = new SKPaint { Color = SKColors.DarkRed };
    canvas.DrawRect(x, y + barY, barWidth, barHeight, bgPaint);
    
    // Foreground (green based on HP %)
    float hpPercent = (float)unit.CurrentHP / unit.MaxHP;
    var hpColor = hpPercent > 0.5f ? [SKColors.Green](http://SKColors.Green) :
                  hpPercent > 0.25f ? SKColors.Yellow :
                  [SKColors.Red](http://SKColors.Red);
    
    using var hpPaint = new SKPaint { Color = hpColor };
    canvas.DrawRect(x, y + barY, barWidth * hpPercent, barHeight, hpPaint);
    
    // Border
    using var borderPaint = new SKPaint
    {
        Color = [SKColors.Black](http://SKColors.Black),
        IsStroke = true,
        StrokeWidth = 1
    };
    canvas.DrawRect(x, y + barY, barWidth, barHeight, borderPaint);
}

private void DrawStatusEffects(SKCanvas canvas, float x, float y, Combatant unit)
{
    var effects = unit.StatusEffects;
    if (!effects.Any()) return;
    
    const int iconSize = 16;
    const int iconSpacing = 2;
    const int maxIconsPerRow = 3;
    
    int index = 0;
    foreach (var effect in effects.Take(6)) // Show max 6 icons
    {
        var icon = _statusEffectIconService.GetStatusEffectIcon(effect.Type);
        if (icon == null) continue;
        
        int row = index / maxIconsPerRow;
        int col = index % maxIconsPerRow;
        
        var iconX = x + col * (iconSize + iconSpacing);
        var iconY = y + SpriteSize + 2 + row * (iconSize + iconSpacing); // Below sprite
        
        // Draw icon with scaling
        var destRect = new SKRect(iconX, iconY, iconX + iconSize, iconY + iconSize);
        canvas.DrawBitmap(icon, destRect);
        
        // Draw duration text
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = [SKTextAlign.Center](http://SKTextAlign.Center)
        };
        
        canvas.DrawText(effect.RemainingDuration.ToString(),
            iconX + iconSize / 2, iconY + iconSize - 2, textPaint);
        
        index++;
    }
    
    // Draw "+" if more effects
    if (effects.Count() > 6)
    {
        using var textPaint = new SKPaint
        {
            Color = SKColors.Yellow,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        
        canvas.DrawText("+", x + SpriteSize, y + SpriteSize + 16, textPaint);
    }
}
```

### StatusEffectTooltipControl

```csharp
using Avalonia.Controls;
using RuneAndRust.Core.Combat;

namespace RuneAndRust.DesktopUI.Controls;

public class StatusEffectTooltip : UserControl
{
    private readonly IStatusEffectIconService _iconService;
    
    public StatusEffectTooltip(IStatusEffectIconService iconService)
    {
        _iconService = iconService;
    }
    
    public void ShowTooltip(StatusEffect effect, Point position)
    {
        var description = _iconService.GetStatusEffectDescription(effect);
        
        // Create tooltip content
        var tooltip = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 30, 30, 30)),
            BorderBrush = new SolidColorBrush(_iconService.GetStatusEffectColor(effect.Type)),
            BorderThickness = new Thickness(2),
            Padding = new Thickness(8),
            Child = new TextBlock
            {
                Text = description,
                Foreground = Brushes.White,
                FontSize = 12
            }
        };
        
        // Position and show tooltip
        Canvas.SetLeft(tooltip, position.X);
        Canvas.SetTop(tooltip, position.Y);
        // Add to canvas overlay
    }
}
```

---

## Sample Status Effect Sprites

### Bleeding

```json
{
  "Name": "status_bleeding",
  "PixelData": [
    "                ",
    "                ",
    "   RR      RR   ",
    "   RRR    RRR   ",
    "    RRR  RRR    ",
    "     RRRRRR     ",
    "    RRRRRRRR    ",
    "   RRRRRRRRRR   ",
    "   RRRRRRRRRR   ",
    "    RRRRRRRR    ",
    "     RRRRRR     ",
    "      RRRR      ",
    "       RR       ",
    "                ",
    "                ",
    "                "
  ],
  "Palette": {
    "R": "#DC143C"
  }
}
```

### Blessed

```json
{
  "Name": "status_blessed",
  "PixelData": [
    "                ",
    "       GG       ",
    "      GGGG      ",
    "      GGGG      ",
    "       GG       ",
    "   GGGGGGGGGG   ",
    "   GGGGGGGGGG   ",
    "       GG       ",
    "       GG       ",
    "      YYYY      ",
    "     YYYYYY     ",
    "      YYYY      ",
    "       YY       ",
    "                ",
    "                ",
    "                "
  ],
  "Palette": {
    "G": "#FFD700",
    "Y": "#FFFF00"
  }
}
```

---

## Integration Points

**With v0.21.3 (Status Effects):**

- Displays all status effect types
- Shows remaining duration
- Reflects magnitude in tooltips

**With v0.43.4 (Grid Control):**

- Overlays status icons on units
- HP bars above sprites

**With v0.43.2 (Sprite System):**

- Loads status effect icons
- Scales appropriately

---

## Functional Requirements

### FR1: Status Icon Display

**Requirement:** Show status effect icons on units.

**Test:**

```csharp
[Fact]
public void StatusEffectIconService_ReturnsIconForEffect()
{
    var spriteService = CreateMockSpriteService();
    var iconService = new StatusEffectIconService(spriteService);
    
    var icon = iconService.GetStatusEffectIcon(StatusEffectType.Bleeding);
    
    Assert.NotNull(icon);
}

[Fact]
public void StatusEffectIcons_DisplayOnUnit()
{
    var unit = new PlayerCharacter();
    unit.ApplyStatusEffect(new StatusEffect
    {
        Type = StatusEffectType.Bleeding,
        RemainingDuration = 3
    });
    
    Assert.Single(unit.StatusEffects);
}
```

### FR2: HP Bar Display

**Requirement:** Show HP bar above each unit.

**Test:**

```csharp
[Fact]
public void HPBar_ReflectsCurrentHP()
{
    var unit = new PlayerCharacter
    {
        CurrentHP = 50,
        MaxHP = 100
    };
    
    var hpPercent = (float)unit.CurrentHP / unit.MaxHP;
    Assert.Equal(0.5f, hpPercent);
}
```

### FR3: Status Effect Tooltips

**Requirement:** Show detailed tooltip on hover.

**Test:**

```csharp
[Fact]
public void StatusEffectTooltip_ShowsDescription()
{
    var iconService = CreateIconService();
    var effect = new StatusEffect
    {
        Type = StatusEffectType.Bleeding,
        Name = "Bleeding",
        Magnitude = 5,
        RemainingDuration = 3
    };
    
    var description = iconService.GetStatusEffectDescription(effect);
    
    Assert.Contains("Bleeding", description);
    Assert.Contains("5", description);
    Assert.Contains("3 turns", description);
}
```

---

## Success Criteria

**v0.43.6 is DONE when:**

### ✅ Status Icons

- [ ]  All 13 status types have icons
- [ ]  Icons display on affected units
- [ ]  Duration shown on icons
- [ ]  Max 6 icons shown, "+" for more

### ✅ HP Bars

- [ ]  HP bar above each unit
- [ ]  Color changes with HP %
- [ ]  Accurate width reflects HP

### ✅ Tooltips

- [ ]  Hover shows full description
- [ ]  Duration displayed
- [ ]  Magnitude shown
- [ ]  Color-coded by effect type

---

**Status effects visualization complete. Ready for environmental hazards in v0.43.7.**