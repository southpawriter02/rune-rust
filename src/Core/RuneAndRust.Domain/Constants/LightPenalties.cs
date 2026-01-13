using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Constants;

/// <summary>
/// Constants and helper methods for light-based penalties.
/// </summary>
/// <remarks>
/// <para>
/// Provides centralized penalty values for combat accuracy and perception
/// checks based on light levels. Helper methods ensure consistent application
/// across all game systems.
/// </para>
/// <para>
/// Penalty values:
/// <list type="bullet">
///   <item><description>Bright: No penalties</description></item>
///   <item><description>Dim: Accuracy -1, Perception +2 DC</description></item>
///   <item><description>Dark: Accuracy -3, Perception +5 DC</description></item>
///   <item><description>MagicalDarkness: Accuracy -5, Perception +10 DC</description></item>
/// </list>
/// </para>
/// </remarks>
public static class LightPenalties
{
    #region Accuracy Penalties

    /// <summary>
    /// Accuracy penalty in dim light.
    /// </summary>
    public const int DimLightAccuracyPenalty = -1;

    /// <summary>
    /// Accuracy penalty in darkness.
    /// </summary>
    public const int DarkAccuracyPenalty = -3;

    /// <summary>
    /// Accuracy penalty in magical darkness.
    /// </summary>
    public const int MagicalDarknessAccuracyPenalty = -5;

    #endregion

    #region Perception DC Modifiers

    /// <summary>
    /// Perception DC modifier in dim light.
    /// </summary>
    public const int DimLightPerceptionDC = 2;

    /// <summary>
    /// Perception DC modifier in darkness.
    /// </summary>
    public const int DarkPerceptionDC = 5;

    /// <summary>
    /// Perception DC modifier in magical darkness.
    /// </summary>
    public const int MagicalDarknessPerceptionDC = 10;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the accuracy penalty for a light level.
    /// </summary>
    /// <param name="level">The current light level.</param>
    /// <returns>The accuracy penalty (0 or negative).</returns>
    public static int GetAccuracyPenalty(LightLevel level) => level switch
    {
        LightLevel.Bright => 0,
        LightLevel.Dim => DimLightAccuracyPenalty,
        LightLevel.Dark => DarkAccuracyPenalty,
        LightLevel.MagicalDarkness => MagicalDarknessAccuracyPenalty,
        _ => 0
    };

    /// <summary>
    /// Gets the perception DC modifier for a light level.
    /// </summary>
    /// <param name="level">The current light level.</param>
    /// <returns>The DC modifier to add (0 or positive).</returns>
    public static int GetPerceptionDCModifier(LightLevel level) => level switch
    {
        LightLevel.Bright => 0,
        LightLevel.Dim => DimLightPerceptionDC,
        LightLevel.Dark => DarkPerceptionDC,
        LightLevel.MagicalDarkness => MagicalDarknessPerceptionDC,
        _ => 0
    };

    /// <summary>
    /// Gets a descriptive string for the light level.
    /// </summary>
    /// <param name="level">The current light level.</param>
    /// <returns>A human-readable description.</returns>
    public static string GetDescription(LightLevel level) => level switch
    {
        LightLevel.Bright => "brightly lit",
        LightLevel.Dim => "dimly lit",
        LightLevel.Dark => "shrouded in darkness",
        LightLevel.MagicalDarkness => "consumed by supernatural darkness",
        _ => "unknown lighting"
    };

    /// <summary>
    /// Gets whether the light level imposes any penalties.
    /// </summary>
    /// <param name="level">The current light level.</param>
    /// <returns>True if any penalties apply.</returns>
    public static bool HasPenalties(LightLevel level) =>
        level != LightLevel.Bright;

    /// <summary>
    /// Gets a short label for the light level.
    /// </summary>
    /// <param name="level">The current light level.</param>
    /// <returns>A short display label.</returns>
    public static string GetLabel(LightLevel level) => level switch
    {
        LightLevel.Bright => "Bright",
        LightLevel.Dim => "Dim",
        LightLevel.Dark => "Dark",
        LightLevel.MagicalDarkness => "Magical Darkness",
        _ => "Unknown"
    };

    #endregion
}
