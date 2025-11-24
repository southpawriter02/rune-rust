using RuneAndRust.Core;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service interface for status effect icon management.
/// </summary>
public interface IStatusEffectIconService
{
    SKBitmap? GetStatusEffectIcon(string effectType);
    string GetStatusEffectDescription(StatusEffect effect);
    SKColor GetStatusEffectColor(string effectType);
    SKColor GetCategoryColor(StatusEffectCategory category);
}

/// <summary>
/// Service for managing status effect visual indicators.
/// Maps status effect types to sprites, colors, and descriptions.
/// </summary>
public class StatusEffectIconService : IStatusEffectIconService
{
    private readonly ISpriteService _spriteService;

    // Map effect types to sprite names
    private readonly Dictionary<string, string> _iconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Control Effects
        { "Stunned", "status_stunned" },
        { "Rooted", "status_rooted" },
        { "Feared", "status_feared" },
        { "Disoriented", "status_disoriented" },
        { "Slowed", "status_slowed" },

        // Damage Over Time
        { "Bleeding", "status_bleeding" },
        { "Poisoned", "status_poisoned" },
        { "Corroded", "status_corroded" },

        // Stat Modification
        { "Vulnerable", "status_vulnerable" },
        { "Analyzed", "status_analyzed" },
        { "Brittle", "status_brittle" },

        // Buffs
        { "Hasted", "status_hasted" },
        { "Inspired", "status_inspired" },

        // Legacy effects (for backwards compatibility)
        { "Blessed", "status_blessed" },
        { "Shielded", "status_shielded" },
        { "Burning", "status_burning" },
        { "Frozen", "status_frozen" },
        { "Weakened", "status_weakened" },
        { "Strengthened", "status_strengthened" },
        { "Corrupted", "status_corrupted" },
        { "Regenerating", "status_regenerating" }
    };

    public StatusEffectIconService(ISpriteService spriteService)
    {
        _spriteService = spriteService;
    }

    public SKBitmap? GetStatusEffectIcon(string effectType)
    {
        if (_iconMap.TryGetValue(effectType, out var spriteName))
        {
            // Use scale 2 for 32×32 icons (16×16 base * 2)
            return _spriteService.GetSpriteBitmap(spriteName, scale: 2);
        }

        // Return generic icon if specific not found
        return _spriteService.GetSpriteBitmap("status_unknown", scale: 2);
    }

    public string GetStatusEffectDescription(StatusEffect effect)
    {
        var definition = StatusEffectDefinition.GetDefinition(effect.EffectType);
        if (definition == null)
        {
            return $"{effect.EffectType}\\nDuration: {effect.DurationRemaining} turns";
        }

        var description = definition.Description;

        // Add stack information if applicable
        if (definition.CanStack && effect.StackCount > 1)
        {
            description = $"{description}\\nStacks: {effect.StackCount}/{definition.MaxStacks}";
        }

        // Add duration
        if (definition.DefaultDuration != -1) // -1 means permanent
        {
            description = $"{description}\\nDuration: {effect.DurationRemaining} turns";
        }
        else
        {
            description = $"{description}\\n(Permanent until cleansed)";
        }

        return $"{definition.DisplayName}\\n{description}";
    }

    public SKColor GetStatusEffectColor(string effectType)
    {
        var definition = StatusEffectDefinition.GetDefinition(effectType);
        if (definition != null)
        {
            return GetCategoryColor(definition.Category);
        }

        // Legacy color mapping for backwards compatibility
        return effectType.ToLowerInvariant() switch
        {
            "bleeding" => new SKColor(220, 20, 60),      // Crimson
            "poisoned" => new SKColor(147, 112, 219),    // Medium Purple
            "stunned" => new SKColor(128, 128, 128),     // Gray
            "blessed" => new SKColor(255, 215, 0),       // Gold
            "shielded" => new SKColor(173, 216, 230),    // Light Blue
            "burning" => new SKColor(255, 69, 0),        // Orange Red
            "frozen" => new SKColor(0, 255, 255),        // Cyan
            "weakened" => new SKColor(139, 69, 19),      // Saddle Brown
            "strengthened" => new SKColor(220, 20, 60),  // Crimson
            "hasted" => new SKColor(255, 255, 0),        // Yellow
            "slowed" => new SKColor(112, 128, 144),      // Slate Gray
            "corrupted" => new SKColor(139, 0, 139),     // Dark Magenta
            "regenerating" => new SKColor(50, 205, 50),  // Lime Green
            _ => new SKColor(255, 255, 255)              // White
        };
    }

    public SKColor GetCategoryColor(StatusEffectCategory category)
    {
        return category switch
        {
            StatusEffectCategory.ControlDebuff => new SKColor(112, 128, 144),      // Slate Gray
            StatusEffectCategory.DamageOverTime => new SKColor(220, 20, 60),       // Crimson
            StatusEffectCategory.StatModification => new SKColor(255, 165, 0),     // Orange
            StatusEffectCategory.Buff => new SKColor(50, 205, 50),                 // Lime Green
            _ => new SKColor(255, 255, 255)                                        // White
        };
    }
}
