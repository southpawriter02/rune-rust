using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks a player's allocation of talent points into an ability tree node.
/// </summary>
/// <remarks>
/// <para>TalentAllocation represents the player's investment in a specific talent tree node.
/// Each allocation tracks:</para>
/// <list type="bullet">
///   <item><description>The node being invested in</description></item>
///   <item><description>Current rank (can be 1 to MaxRank)</description></item>
///   <item><description>Point cost per rank (for refund calculations)</description></item>
///   <item><description>Timestamps for creation and modification</description></item>
/// </list>
/// <para>Allocations are created when a player first invests in a node and are
/// updated when additional ranks are purchased.</para>
/// </remarks>
public class TalentAllocation : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this allocation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the talent tree node this allocation is for.
    /// </summary>
    /// <remarks>
    /// Node IDs are normalized to lowercase for case-insensitive lookups.
    /// </remarks>
    public string NodeId { get; private set; } = null!;

    /// <summary>
    /// Gets the current rank invested in this node.
    /// </summary>
    /// <remarks>
    /// Starts at 1 when created and can be incremented up to the node's MaxRank.
    /// </remarks>
    public int CurrentRank { get; private set; }

    /// <summary>
    /// Gets the point cost per rank for this node.
    /// </summary>
    /// <remarks>
    /// Stored at allocation time for accurate refund calculations even if
    /// the node's configuration changes later.
    /// </remarks>
    public int PointCostPerRank { get; private set; }

    /// <summary>
    /// Gets when the first rank was allocated.
    /// </summary>
    public DateTime AllocatedAt { get; private set; }

    /// <summary>
    /// Gets when the last rank modification occurred.
    /// </summary>
    /// <remarks>
    /// Updated each time IncrementRank() is called.
    /// </remarks>
    public DateTime LastModifiedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private TalentAllocation() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new talent allocation for a node.
    /// </summary>
    /// <param name="nodeId">The ID of the talent tree node.</param>
    /// <param name="pointCostPerRank">The point cost for each rank.</param>
    /// <returns>A new TalentAllocation with rank 1.</returns>
    /// <exception cref="ArgumentException">Thrown when nodeId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pointCostPerRank is not positive.</exception>
    /// <remarks>
    /// The allocation is created with CurrentRank = 1, meaning the player has
    /// already invested in the first rank. Both AllocatedAt and LastModifiedAt
    /// are set to the current UTC time.
    /// </remarks>
    public static TalentAllocation Create(string nodeId, int pointCostPerRank)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId, nameof(nodeId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pointCostPerRank, nameof(pointCostPerRank));

        var now = DateTime.UtcNow;
        return new TalentAllocation
        {
            Id = Guid.NewGuid(),
            NodeId = nodeId.ToLowerInvariant(),
            CurrentRank = 1,
            PointCostPerRank = pointCostPerRank,
            AllocatedAt = now,
            LastModifiedAt = now
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Increases the rank by one.
    /// </summary>
    /// <remarks>
    /// This method does not validate against MaxRank - that validation
    /// should be performed by the TalentPointService before calling this method.
    /// Updates LastModifiedAt to the current UTC time.
    /// </remarks>
    public void IncrementRank()
    {
        CurrentRank++;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the total points spent on this node across all ranks.
    /// </summary>
    /// <returns>CurrentRank multiplied by PointCostPerRank.</returns>
    /// <remarks>
    /// Used for refund calculations during talent respec operations.
    /// </remarks>
    public int GetTotalPointsSpent()
    {
        return CurrentRank * PointCostPerRank;
    }

    /// <summary>
    /// Returns a string representation of this allocation.
    /// </summary>
    /// <returns>A string containing the node ID and current rank.</returns>
    public override string ToString() => $"{NodeId}: Rank {CurrentRank}";
}
