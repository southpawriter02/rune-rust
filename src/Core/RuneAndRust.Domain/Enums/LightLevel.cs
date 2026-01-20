namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the illumination level of an area.
/// </summary>
/// <remarks>
/// <para>
/// Light levels affect combat accuracy and perception checks.
/// Bright has no penalties, while MagicalDarkness has maximum penalties.
/// </para>
/// <list type="bullet">
///   <item><description>Bright: Full visibility, no penalties</description></item>
///   <item><description>Dim: Partial visibility, minor penalties</description></item>
///   <item><description>Dark: No visibility, significant penalties</description></item>
///   <item><description>MagicalDarkness: Supernatural, maximum penalties</description></item>
/// </list>
/// <para>
/// Vision types (v0.4.3b) can mitigate some light penalties:
/// DarkVision negates Dark penalties, TrueSight negates all penalties.
/// </para>
/// </remarks>
public enum LightLevel
{
    /// <summary>
    /// Fully illuminated - no penalties.
    /// </summary>
    /// <remarks>
    /// Normal daylight, magical illumination, or well-lit rooms.
    /// Combat and perception operate at full effectiveness.
    /// </remarks>
    Bright = 0,

    /// <summary>
    /// Partially illuminated - minor penalties to perception.
    /// </summary>
    /// <remarks>
    /// Torch-lit corridors, twilight, or weak ambient light.
    /// Accuracy: -1, Perception DC: +2
    /// </remarks>
    Dim = 1,

    /// <summary>
    /// No illumination - significant penalties without dark vision.
    /// </summary>
    /// <remarks>
    /// Complete darkness in underground areas or moonless nights.
    /// Accuracy: -3, Perception DC: +5
    /// Dark Vision can mitigate these penalties.
    /// </remarks>
    Dark = 2,

    /// <summary>
    /// Supernatural darkness - even dark vision cannot penetrate.
    /// </summary>
    /// <remarks>
    /// Magical effect that suppresses all light and vision.
    /// Accuracy: -5, Perception DC: +10
    /// Only True Sight can see through this.
    /// </remarks>
    MagicalDarkness = 3
}
