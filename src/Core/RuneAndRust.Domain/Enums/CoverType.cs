namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of cover providing defense bonuses in tactical combat.
/// </summary>
/// <remarks>
/// <para>Cover affects ranged combat by providing defense bonuses or blocking attacks entirely.</para>
/// <list type="bullet">
///   <item><description><see cref="None"/>: No cover effect - attacks proceed normally.</description></item>
///   <item><description><see cref="Partial"/>: Target gains defense bonus (typically +2).</description></item>
///   <item><description><see cref="Full"/>: Target cannot be attacked from that angle.</description></item>
/// </list>
/// </remarks>
public enum CoverType
{
    /// <summary>
    /// No cover - the target has no defensive bonus from cover.
    /// </summary>
    None = 0,

    /// <summary>
    /// Partial cover - provides a defense bonus (typically +2) but target can still be attacked.
    /// Examples: crates, barrels, low walls.
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Full cover - the target cannot be targeted from this angle.
    /// The cover completely blocks the line of attack.
    /// Examples: stone pillars, solid walls.
    /// </summary>
    Full = 2
}
