using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tier 2 thematic modifier providing biome-specific variations.
/// Immutable value object with mechanical effects that alter gameplay.
/// </summary>
/// <remarks>
/// Modifiers define:
/// - Visual/narrative theming (name, adjective, detail fragment)
/// - Mechanical effects (HP multiplier, damage auras, slippery surfaces)
/// - Ambient characteristics (color palette, sounds)
/// </remarks>
public sealed record ThematicModifier
{
    /// <summary>The modifier name (e.g., "Rusted", "Scorched").</summary>
    public required string Name { get; init; }

    /// <summary>The primary biome this modifier is associated with.</summary>
    public required Biome PrimaryBiome { get; init; }

    /// <summary>Adjective form used in descriptions (e.g., "corroded", "scorched").</summary>
    public required string Adjective { get; init; }

    /// <summary>Detail fragment describing the modifier's effect (e.g., "shows centuries of oxidation").</summary>
    public required string DetailFragment { get; init; }

    /// <summary>Color palette descriptor for visual theming.</summary>
    public string ColorPalette { get; init; } = "grey-brown-faded";

    /// <summary>Ambient sounds associated with this modifier.</summary>
    public IReadOnlyList<string> AmbientSounds { get; init; } = Array.Empty<string>();

    // Mechanical Effects

    /// <summary>HP multiplier for objects/enemies in rooms with this modifier (default 1.0).</summary>
    public double HpMultiplier { get; init; } = 1.0;

    /// <summary>Whether objects are brittle and break more easily.</summary>
    public bool IsBrittle { get; init; }

    /// <summary>Damage dealt per turn to entities in the room (null = no damage).</summary>
    public int? DamagePerTurn { get; init; }

    /// <summary>Type of damage dealt (e.g., "fire", "cold").</summary>
    public string? DamageType { get; init; }

    /// <summary>Whether surfaces are slippery (affects movement).</summary>
    public bool IsSlippery { get; init; }

    /// <summary>Whether this modifier provides a light source.</summary>
    public bool IsLightSource { get; init; }

    /// <summary>Whether the light can cause dazzle effects.</summary>
    public bool CanDazzle { get; init; }

    /// <summary>Scale multiplier for room size perception (default 1.0).</summary>
    public double ScaleMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Gets the tags that should be applied to rooms with this modifier.
    /// </summary>
    public IEnumerable<string> GetEffectTags()
    {
        if (IsBrittle)
            yield return "Brittle";
        if (IsSlippery)
            yield return "Slippery";
        if (IsLightSource)
            yield return "LightSource";
        if (CanDazzle)
            yield return "Dazzle";
        if (DamagePerTurn > 0 && !string.IsNullOrEmpty(DamageType))
            yield return $"DamageAura:{DamageType}:{DamagePerTurn}";
        if (Math.Abs(ScaleMultiplier - 1.0) > 0.01)
            yield return $"Scale:{ScaleMultiplier:F1}";
        if (Math.Abs(HpMultiplier - 1.0) > 0.01)
            yield return $"HpMod:{HpMultiplier:F1}";
    }

    /// <summary>
    /// Checks if this modifier has any mechanical effects.
    /// </summary>
    public bool HasMechanicalEffects =>
        IsBrittle ||
        IsSlippery ||
        IsLightSource ||
        CanDazzle ||
        DamagePerTurn > 0 ||
        Math.Abs(ScaleMultiplier - 1.0) > 0.01 ||
        Math.Abs(HpMultiplier - 1.0) > 0.01;

    public override string ToString() => $"{Name} ({PrimaryBiome})";
}
