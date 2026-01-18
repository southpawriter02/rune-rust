namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Summary of a player's talent point status.
/// </summary>
/// <remarks>
/// <para>Provides a snapshot of the player's talent point economy including:</para>
/// <list type="bullet">
///   <item><description>Unspent: Points available to allocate</description></item>
///   <item><description>TotalEarned: Lifetime points earned</description></item>
///   <item><description>TotalSpent: Points invested in nodes</description></item>
///   <item><description>AllocatedNodes: Number of unique nodes with points</description></item>
/// </list>
/// <para>Invariant: Unspent + TotalSpent should equal TotalEarned.</para>
/// </remarks>
/// <param name="Unspent">Talent points available to spend.</param>
/// <param name="TotalEarned">Total talent points ever earned.</param>
/// <param name="TotalSpent">Total talent points currently invested.</param>
/// <param name="AllocatedNodes">Number of unique nodes with at least one rank.</param>
public record TalentPointSummary(
    int Unspent,
    int TotalEarned,
    int TotalSpent,
    int AllocatedNodes)
{
    /// <summary>
    /// Gets whether the player has any unspent points.
    /// </summary>
    public bool HasUnspentPoints => Unspent > 0;

    /// <summary>
    /// Gets whether the player has made any allocations.
    /// </summary>
    public bool HasAllocations => AllocatedNodes > 0;

    /// <summary>
    /// Creates an empty summary (no points earned or spent).
    /// </summary>
    /// <returns>A TalentPointSummary with all values at zero.</returns>
    public static TalentPointSummary Empty => new(0, 0, 0, 0);
}
