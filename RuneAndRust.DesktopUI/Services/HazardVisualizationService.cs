using RuneAndRust.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for visualizing environmental hazards, cover, and terrain features.
/// Integrates with v0.22 Environmental Combat System.
/// </summary>
public interface IHazardVisualizationService
{
    SKBitmap? GetHazardSprite(string damageType);
    SKColor GetHazardOverlayColor(string damageType);
    string GetHazardDescription(EnvironmentalObject hazard);
    bool ShouldAnimateHazard(string damageType);
    SKColor GetCoverColor(CoverType coverType);
    string GetCoverIcon(CoverType coverType);
    SKColor GetTileTypeColor(TileType tileType);
}

public class HazardVisualizationService : IHazardVisualizationService
{
    private readonly ISpriteService _spriteService;

    // Map damage types to sprite names
    private readonly Dictionary<string, string> _hazardSprites = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Fire", "hazard_fire" },
        { "Poison", "hazard_poison" },
        { "Ice", "hazard_ice" },
        { "Physical", "hazard_spike" },
        { "Acid", "hazard_acid" },
        { "Electrical", "hazard_lightning" },
        { "Electric", "hazard_lightning" },
        { "Lightning", "hazard_lightning" },
        { "Psychic", "hazard_darkness" },
        { "Darkness", "hazard_darkness" },
        { "Corruption", "hazard_corruption" },
        { "Toxic", "hazard_poison" }, // Toxic maps to poison sprite
    };

    public HazardVisualizationService(ISpriteService spriteService)
    {
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
    }

    public SKBitmap? GetHazardSprite(string damageType)
    {
        if (string.IsNullOrEmpty(damageType))
            return null;

        if (_hazardSprites.TryGetValue(damageType, out var spriteName))
        {
            return _spriteService.GetSpriteBitmap(spriteName, scale: 3);
        }

        // Return generic hazard sprite if specific type not found
        return _spriteService.GetSpriteBitmap("hazard_fire", scale: 3);
    }

    public SKColor GetHazardOverlayColor(string damageType)
    {
        if (string.IsNullOrEmpty(damageType))
            return new SKColor(255, 0, 0, 80);

        return damageType.ToLowerInvariant() switch
        {
            "fire" => new SKColor(255, 165, 0, 100),      // Orange
            "poison" => new SKColor(160, 32, 240, 80),    // Purple
            "toxic" => new SKColor(160, 32, 240, 80),     // Purple
            "ice" => new SKColor(0, 255, 255, 90),        // Cyan
            "physical" => new SKColor(128, 128, 128, 70), // Gray
            "spike" => new SKColor(128, 128, 128, 70),    // Gray
            "acid" => new SKColor(50, 205, 50, 85),       // Lime Green
            "electrical" => new SKColor(255, 255, 0, 120), // Yellow
            "electric" => new SKColor(255, 255, 0, 120),  // Yellow
            "lightning" => new SKColor(255, 255, 0, 120), // Yellow
            "psychic" => new SKColor(0, 0, 0, 150),       // Black
            "darkness" => new SKColor(0, 0, 0, 150),      // Black
            "corruption" => new SKColor(255, 0, 255, 100), // Magenta
            _ => new SKColor(255, 0, 0, 80)               // Red (default)
        };
    }

    public string GetHazardDescription(EnvironmentalObject hazard)
    {
        if (hazard == null || !hazard.IsHazard)
            return "Unknown hazard";

        var desc = hazard.Description;
        if (string.IsNullOrEmpty(desc))
        {
            desc = hazard.DamageType switch
            {
                "Fire" => "Deals fire damage to units in area",
                "Poison" => "Applies poison status to units",
                "Ice" => "Reduces movement and deals cold damage",
                "Physical" => "Deals damage when entering area",
                "Acid" => "Deals corrosive damage over time",
                "Electrical" or "Electric" or "Lightning" => "Random chance of shock damage",
                "Psychic" or "Darkness" => "Increases Psychic Stress",
                "Corruption" => "Corrupts units over time",
                _ => "Environmental hazard"
            };
        }

        var damageInfo = !string.IsNullOrEmpty(hazard.DamageFormula)
            ? $"\nDamage: {hazard.DamageFormula}"
            : "";

        var statusInfo = !string.IsNullOrEmpty(hazard.StatusEffect)
            ? $"\nEffect: {hazard.StatusEffect}"
            : "";

        return $"{hazard.Name}\n{desc}{damageInfo}{statusInfo}";
    }

    public bool ShouldAnimateHazard(string damageType)
    {
        if (string.IsNullOrEmpty(damageType))
            return false;

        var type = damageType.ToLowerInvariant();
        return type == "fire" || type == "lightning" ||
               type == "electrical" || type == "electric" ||
               type == "corruption";
    }

    public SKColor GetCoverColor(CoverType coverType)
    {
        return coverType switch
        {
            CoverType.Physical => new SKColor(105, 105, 105),      // Dim Gray
            CoverType.Metaphysical => new SKColor(135, 206, 250),  // Light Sky Blue
            CoverType.Both => new SKColor(148, 0, 211),            // Dark Violet
            _ => SKColors.Transparent
        };
    }

    public string GetCoverIcon(CoverType coverType)
    {
        return coverType switch
        {
            CoverType.Physical => "▓",           // Dark shade (physical barrier)
            CoverType.Metaphysical => "▒",       // Medium shade (ethereal)
            CoverType.Both => "█",               // Full block (complete cover)
            _ => ""
        };
    }

    public SKColor GetTileTypeColor(TileType tileType)
    {
        return tileType switch
        {
            TileType.HighGround => new SKColor(255, 215, 0, 40),   // Gold tint (elevated)
            TileType.Glitched => new SKColor(255, 0, 255, 60),     // Magenta tint (corrupted)
            _ => SKColors.Transparent
        };
    }
}
