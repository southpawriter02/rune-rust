using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modifier from environmental or physical conditions.
/// </summary>
/// <remarks>
/// <para>
/// Environment modifiers include:
/// <list type="bullet">
///   <item><description>Surface type (stable, wet, compromised, collapsing)</description></item>
///   <item><description>Lighting level (bright, normal, dim, dark)</description></item>
///   <item><description>Corruption tier (normal, glitched, blighted, resonance)</description></item>
///   <item><description>Weather conditions (for outdoor areas)</description></item>
/// </list>
/// </para>
/// <para>
/// Environment modifiers are typically determined by the current room or area
/// and apply to relevant skill checks automatically.
/// </para>
/// </remarks>
/// <param name="ModifierId">Unique identifier for this modifier.</param>
/// <param name="Name">Display name for the modifier.</param>
/// <param name="DiceModifier">Bonus or penalty to dice pool.</param>
/// <param name="DcModifier">Bonus or penalty to difficulty class.</param>
/// <param name="SurfaceType">Surface condition (for climbing, stealth).</param>
/// <param name="LightingLevel">Lighting condition (for visual tasks).</param>
/// <param name="CorruptionTier">Corruption level of the area.</param>
/// <param name="Description">Optional flavor text.</param>
public readonly record struct EnvironmentModifier(
    string ModifierId,
    string Name,
    int DiceModifier,
    int DcModifier,
    SurfaceType? SurfaceType = null,
    LightingLevel? LightingLevel = null,
    CorruptionTier? CorruptionTier = null,
    string? Description = null) : ISkillModifier
{
    /// <summary>
    /// Gets the modifier category.
    /// </summary>
    public ModifierCategory Category => ModifierCategory.Environment;

    /// <summary>
    /// Creates a surface modifier for climbing and movement checks.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>A new environment modifier with appropriate dice adjustment.</returns>
    public static EnvironmentModifier FromSurface(SurfaceType surface)
    {
        var (dice, name) = surface switch
        {
            Enums.SurfaceType.Stable => (1, "Stable Surface"),
            Enums.SurfaceType.Wet => (-1, "Wet Surface"),
            Enums.SurfaceType.Compromised => (-2, "Compromised Surface"),
            Enums.SurfaceType.Collapsing => (-3, "Collapsing Surface"),
            _ => (0, "Normal Surface")
        };

        return new EnvironmentModifier(
            $"surface-{surface.ToString().ToLowerInvariant()}",
            name,
            DiceModifier: dice,
            DcModifier: 0,
            SurfaceType: surface);
    }

    /// <summary>
    /// Creates a lighting modifier for visual tasks.
    /// </summary>
    /// <param name="lighting">The lighting level.</param>
    /// <returns>A new environment modifier with appropriate DC adjustment.</returns>
    public static EnvironmentModifier FromLighting(LightingLevel lighting)
    {
        var (dc, name) = lighting switch
        {
            Enums.LightingLevel.Bright => (-1, "Bright Lighting"),
            Enums.LightingLevel.Normal => (0, "Normal Lighting"),
            Enums.LightingLevel.Dim => (1, "Dim Lighting"),
            Enums.LightingLevel.Dark => (2, "Dark"),
            _ => (0, "Normal Lighting")
        };

        return new EnvironmentModifier(
            $"lighting-{lighting.ToString().ToLowerInvariant()}",
            name,
            DiceModifier: 0,
            DcModifier: dc,
            LightingLevel: lighting);
    }

    /// <summary>
    /// Creates a corruption modifier for checks in corrupted areas.
    /// </summary>
    /// <param name="tier">The corruption tier.</param>
    /// <returns>A new environment modifier with appropriate DC adjustment.</returns>
    public static EnvironmentModifier FromCorruption(CorruptionTier tier)
    {
        var (dc, name) = tier switch
        {
            Enums.CorruptionTier.Normal => (0, "Normal Area"),
            Enums.CorruptionTier.Glitched => (2, "Glitched Area"),
            Enums.CorruptionTier.Blighted => (4, "Blighted Area"),
            Enums.CorruptionTier.Resonance => (6, "Resonance Zone"),
            _ => (0, "Normal Area")
        };

        // Skip if no effect
        if (dc == 0)
            return new EnvironmentModifier("corruption-normal", name, 0, 0, CorruptionTier: tier);

        return new EnvironmentModifier(
            $"corruption-{tier.ToString().ToLowerInvariant()}",
            name,
            DiceModifier: 0,
            DcModifier: dc,
            CorruptionTier: tier,
            Description: "Corruption interferes with actions");
    }

    /// <summary>
    /// Returns a short description for UI display.
    /// </summary>
    public string ToShortDescription()
    {
        var parts = new List<string> { Name };

        if (DiceModifier != 0)
        {
            var diceStr = DiceModifier > 0 ? $"+{DiceModifier}d10" : $"{DiceModifier}d10";
            parts.Add($"({diceStr})");
        }

        if (DcModifier != 0)
        {
            var dcStr = DcModifier > 0 ? $"DC +{DcModifier}" : $"DC {DcModifier}";
            parts.Add($"({dcStr})");
        }

        return string.Join(" ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToShortDescription();
}
