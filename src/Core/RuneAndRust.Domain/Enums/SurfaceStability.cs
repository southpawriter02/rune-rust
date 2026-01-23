namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Describes the stability of a surface being balanced upon.
/// </summary>
/// <remarks>
/// <para>
/// Stability modifies the base DC from width. An unstable or swaying surface
/// adds additional difficulty as the character must compensate for movement
/// while maintaining their own balance.
/// </para>
/// <para>
/// <b>DC Modifiers:</b>
/// <list type="bullet">
///   <item><description>Stable: +0 DC</description></item>
///   <item><description>Unstable: +1 DC</description></item>
///   <item><description>Swaying: +2 DC</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of surface stability tracking.
/// </para>
/// </remarks>
public enum SurfaceStability
{
    /// <summary>
    /// The surface is fixed and solid. No DC modifier.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +0.
    /// Examples: Stone ledges, metal beams, sturdy wooden planks.
    /// The surface does not move or shift under the character's weight.
    /// </remarks>
    Stable = 0,

    /// <summary>
    /// The surface shifts or wobbles under weight. Moderate DC increase.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +1.
    /// Examples: Loose boards, crumbling ledges, stacked debris.
    /// The surface moves unexpectedly when weight is applied.
    /// </remarks>
    Unstable = 1,

    /// <summary>
    /// The surface moves continuously. Significant DC increase.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +2.
    /// Examples: Rope bridges, hanging cables, suspended platforms.
    /// The character must time their movements with the surface's rhythm.
    /// </remarks>
    Swaying = 2
}
