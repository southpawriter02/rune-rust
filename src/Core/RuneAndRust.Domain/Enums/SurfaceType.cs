namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of surfaces for climbing and movement checks.
/// </summary>
/// <remarks>
/// <para>
/// Surface conditions affect climbing, balance, and stealth checks
/// through dice pool modifiers in <see cref="ValueObjects.EnvironmentModifier"/>.
/// </para>
/// <para>
/// Modifier values:
/// <list type="bullet">
///   <item><description>Stable: +1d10</description></item>
///   <item><description>Normal: +0d10</description></item>
///   <item><description>Wet: -1d10</description></item>
///   <item><description>Compromised: -2d10</description></item>
///   <item><description>Collapsing: -3d10</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SurfaceType
{
    /// <summary>
    /// Stable, well-maintained surface. +1d10 to climbing.
    /// </summary>
    Stable = 0,

    /// <summary>
    /// Normal surface with no special conditions.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Wet or slippery surface. -1d10 to climbing.
    /// </summary>
    Wet = 2,

    /// <summary>
    /// Damaged or unstable surface. -2d10 to climbing.
    /// </summary>
    Compromised = 3,

    /// <summary>
    /// Actively falling apart. -3d10 to climbing.
    /// </summary>
    Collapsing = 4
}
