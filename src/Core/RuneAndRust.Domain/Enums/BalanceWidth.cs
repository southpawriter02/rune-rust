namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the width of a surface for balance check DC calculation.
/// </summary>
/// <remarks>
/// <para>
/// Width is the primary factor determining balance difficulty. Narrower surfaces
/// require more precise footwork and are exponentially harder to traverse.
/// The DC values use the success-counting system from v0.15.0.
/// </para>
/// <para>
/// <b>DC Mapping:</b>
/// <list type="bullet">
///   <item><description>Wide (2+ ft): DC 2 successes</description></item>
///   <item><description>Narrow (~1 ft): DC 3 successes</description></item>
///   <item><description>Cable (~6 in): DC 4 successes</description></item>
///   <item><description>RazorEdge (&lt;6 in): DC 5 successes</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance width categories.
/// </para>
/// </remarks>
public enum BalanceWidth
{
    /// <summary>
    /// Wide surface (2+ feet). Relatively easy to traverse.
    /// </summary>
    /// <remarks>
    /// DC: 2 successes required.
    /// Examples: Wide planks, thick ledges, fallen tree trunks.
    /// Most characters can manage these surfaces with basic training.
    /// </remarks>
    Wide = 0,

    /// <summary>
    /// Narrow surface (approximately 1 foot). Requires concentration.
    /// </summary>
    /// <remarks>
    /// DC: 3 successes required.
    /// Examples: Standard ledges, thin walls, narrow rafters.
    /// Requires trained acrobatics for reliable success.
    /// </remarks>
    Narrow = 1,

    /// <summary>
    /// Cable or pipe width (approximately 6 inches). Challenging traverse.
    /// </summary>
    /// <remarks>
    /// DC: 4 successes required.
    /// Examples: Power cables, water pipes, tightropes, thin chains.
    /// Requires expert-level acrobatics for consistent success.
    /// </remarks>
    Cable = 2,

    /// <summary>
    /// Razor-thin edge (less than 6 inches). Extreme difficulty.
    /// </summary>
    /// <remarks>
    /// DC: 5 successes required.
    /// Examples: Knife edges, wire cables, barely-there ledges.
    /// Master-level challenge, high risk of failure.
    /// </remarks>
    RazorEdge = 3
}
