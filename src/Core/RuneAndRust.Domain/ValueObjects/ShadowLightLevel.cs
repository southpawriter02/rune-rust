// ═══════════════════════════════════════════════════════════════════════════════
// ShadowLightLevel.cs
// Immutable value object representing environmental light conditions at a
// specific position. Used for Shadow Essence generation, ability effectiveness,
// and Corruption risk calculations.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the light conditions at a specific location, including the
/// categorized light level, source type, intensity, and affected area.
/// </summary>
/// <remarks>
/// <para>
/// ShadowLightLevel is the environmental input that drives Myrk-gengr mechanics:
/// </para>
/// <list type="bullet">
///   <item><description>Shadow Essence generation rates are determined by light level</description></item>
///   <item><description>Shadow Step can only target Darkness/DimLight positions</description></item>
///   <item><description>Cloak of Night effectiveness varies by light level</description></item>
///   <item><description>Corruption risk is triggered in BrightLight/Sunlight</description></item>
/// </list>
/// <para>
/// Named <c>ShadowLightLevel</c> to avoid collision with the existing
/// <see cref="LightLevel"/> enum in the Domain.Enums namespace.
/// </para>
/// <para>
/// This value object is immutable. Factory methods provide convenient
/// construction for common light conditions.
/// </para>
/// <example>
/// <code>
/// var darkness = ShadowLightLevel.CreateDarkness();
/// darkness.IsDarkness()   // true
/// darkness.IsShadow()     // true
/// darkness.GetShadowEssenceMultiplier() // 1.0
///
/// var bright = ShadowLightLevel.CreateBrightLight();
/// bright.IsShadow()       // false
/// bright.GetShadowEssenceMultiplier() // 0.0
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="LightLevelType"/>
/// <seealso cref="ShadowEssenceResource"/>
public sealed record ShadowLightLevel
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The categorized light level at this position.
    /// </summary>
    public LightLevelType CurrentLevel { get; init; }

    /// <summary>
    /// The source producing this light level (e.g., "Ambient", "Torch", "Sunlight", "Magical").
    /// </summary>
    public string SourceType { get; init; } = "Ambient";

    /// <summary>
    /// Light intensity as an integer (0–100). Higher values indicate brighter light.
    /// </summary>
    public int Intensity { get; init; }

    /// <summary>
    /// Radius of the affected area in game units.
    /// </summary>
    public int AffectedArea { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a Darkness light level (intensity 0, no light source).
    /// </summary>
    public static ShadowLightLevel CreateDarkness() => new()
    {
        CurrentLevel = LightLevelType.Darkness,
        SourceType = "None",
        Intensity = 0,
        AffectedArea = 0
    };

    /// <summary>
    /// Creates a DimLight level (intensity 25, ambient source).
    /// </summary>
    public static ShadowLightLevel CreateDimLight() => new()
    {
        CurrentLevel = LightLevelType.DimLight,
        SourceType = "Ambient",
        Intensity = 25,
        AffectedArea = 30
    };

    /// <summary>
    /// Creates a NormalLight level (intensity 50, ambient source).
    /// </summary>
    public static ShadowLightLevel CreateNormalLight() => new()
    {
        CurrentLevel = LightLevelType.NormalLight,
        SourceType = "Ambient",
        Intensity = 50,
        AffectedArea = 50
    };

    /// <summary>
    /// Creates a BrightLight level (intensity 80, direct source).
    /// </summary>
    public static ShadowLightLevel CreateBrightLight() => new()
    {
        CurrentLevel = LightLevelType.BrightLight,
        SourceType = "Direct",
        Intensity = 80,
        AffectedArea = 60
    };

    /// <summary>
    /// Creates a Sunlight level (intensity 100, solar source).
    /// </summary>
    public static ShadowLightLevel CreateSunlight() => new()
    {
        CurrentLevel = LightLevelType.Sunlight,
        SourceType = "Sunlight",
        Intensity = 100,
        AffectedArea = 100
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Query Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Whether the position is in complete Darkness.
    /// </summary>
    public bool IsDarkness() => CurrentLevel == LightLevelType.Darkness;

    /// <summary>
    /// Whether the position has dim illumination.
    /// </summary>
    public bool IsDimLight() => CurrentLevel == LightLevelType.DimLight;

    /// <summary>
    /// Whether the position has bright illumination (BrightLight or Sunlight).
    /// </summary>
    public bool IsBrightLight() => CurrentLevel >= LightLevelType.BrightLight;

    /// <summary>
    /// Whether the position qualifies as "shadow" (Darkness or DimLight).
    /// Shadow positions are valid targets for Shadow Step and enable
    /// full Cloak of Night effectiveness.
    /// </summary>
    public bool IsShadow() => CurrentLevel <= LightLevelType.DimLight;

    /// <summary>
    /// Gets the Shadow Essence generation multiplier for this light level.
    /// </summary>
    /// <returns>
    /// 1.0 for Darkness, 0.6 for DimLight, 0.0 for all other conditions.
    /// </returns>
    public double GetShadowEssenceMultiplier() => CurrentLevel switch
    {
        LightLevelType.Darkness => 1.0,
        LightLevelType.DimLight => 0.6,
        _ => 0.0
    };

    /// <summary>
    /// Returns a human-readable representation of the light conditions.
    /// </summary>
    public override string ToString() =>
        $"ShadowLightLevel({CurrentLevel}, Source={SourceType}, Intensity={Intensity})";
}
