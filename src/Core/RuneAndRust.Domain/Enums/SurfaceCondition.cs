namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Describes the surface condition affecting traction for balance checks.
/// </summary>
/// <remarks>
/// <para>
/// Surface conditions modify balance DC by affecting how well
/// a character can maintain grip and footing.
/// </para>
/// <para>
/// <b>DC Modifiers:</b>
/// <list type="bullet">
///   <item><description>Dry: +0 DC</description></item>
///   <item><description>Wet: +1 DC</description></item>
///   <item><description>Icy: +2 DC</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of surface conditions.
/// </para>
/// </remarks>
public enum SurfaceCondition
{
    /// <summary>
    /// Normal dry surface. No DC modifier.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +0.
    /// Standard conditions with normal traction.
    /// </remarks>
    Dry = 0,

    /// <summary>
    /// Wet surface (rain, water). DC +1.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +1.
    /// Examples: Rain-slicked surfaces, recently washed areas.
    /// Reduces traction and requires more careful footing.
    /// </remarks>
    Wet = 1,

    /// <summary>
    /// Icy or frozen surface. DC +2.
    /// </summary>
    /// <remarks>
    /// DC Modifier: +2.
    /// Examples: Frost-covered ledges, frozen pipes, ice-coated rails.
    /// Significantly reduces traction; extreme care required.
    /// Note: Full slippery surface mechanics are deferred to a future version.
    /// </remarks>
    Icy = 2
}
