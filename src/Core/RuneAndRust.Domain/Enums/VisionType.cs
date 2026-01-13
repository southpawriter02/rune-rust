namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of vision that affect light level perception.
/// </summary>
/// <remarks>
/// <para>
/// Vision types determine how creatures perceive light levels
/// and whether they receive penalties from darkness.
/// </para>
/// <list type="bullet">
///   <item><description>Normal: No mitigation of light penalties</description></item>
///   <item><description>DarkVision: Dark treated as Dim (reduced penalties)</description></item>
///   <item><description>TrueSight: All light levels treated as Bright (no penalties)</description></item>
/// </list>
/// </remarks>
public enum VisionType
{
    /// <summary>
    /// Normal vision - affected by all light levels.
    /// </summary>
    /// <remarks>
    /// No mitigation of light penalties.
    /// </remarks>
    Normal = 0,

    /// <summary>
    /// Can see in darkness as if it were dim light.
    /// </summary>
    /// <remarks>
    /// Dark → treated as Dim (reduced penalties).
    /// Does not help in magical darkness.
    /// </remarks>
    DarkVision = 1,

    /// <summary>
    /// Can see normally in all light conditions.
    /// </summary>
    /// <remarks>
    /// All light levels → treated as Bright.
    /// Penetrates magical darkness.
    /// </remarks>
    TrueSight = 2
}
