namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a branch (specialization path) within an ability tree.
/// </summary>
/// <remarks>
/// <para>AbilityTreeBranch groups related talents into a specialization path.
/// Each class's ability tree typically has 3 branches representing different playstyles.</para>
/// <para>Example branches for Warrior:</para>
/// <list type="bullet">
///   <item><description>Berserker: Fury and raw destructive power</description></item>
///   <item><description>Guardian: Protection, resilience, and damage mitigation</description></item>
///   <item><description>Champion: Leadership, inspiration, and battlefield command</description></item>
/// </list>
/// <para>Each branch contains multiple nodes organized by tiers, with Tier 1 nodes
/// being the entry point and higher tiers requiring prerequisites.</para>
/// </remarks>
public class AbilityTreeBranch
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the unique identifier for this branch.
    /// </summary>
    /// <remarks>
    /// The BranchId should be unique within its parent tree.
    /// Typically uses kebab-case (e.g., "berserker", "guardian", "champion").
    /// </remarks>
    public string BranchId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display name of this branch.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of this specialization path.
    /// </summary>
    /// <remarks>
    /// Should describe the playstyle and benefits of focusing on this branch.
    /// </remarks>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the nodes contained in this branch.
    /// </summary>
    /// <remarks>
    /// Nodes are typically organized by tiers, with each tier building on the previous.
    /// </remarks>
    public IReadOnlyList<AbilityTreeNode> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the path to the branch's icon asset.
    /// </summary>
    /// <remarks>
    /// Used in the UI to represent this specialization path.
    /// </remarks>
    public string? IconPath { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets nodes at a specific tier within this branch.
    /// </summary>
    /// <param name="tier">The tier to filter by (1, 2, 3, etc.).</param>
    /// <returns>An enumerable of nodes at the specified tier.</returns>
    public IEnumerable<AbilityTreeNode> GetNodesAtTier(int tier)
    {
        return Nodes.Where(n => n.Tier == tier);
    }

    /// <summary>
    /// Gets the maximum tier present in this branch.
    /// </summary>
    /// <returns>The highest tier value among nodes, or 0 if no nodes exist.</returns>
    public int GetMaxTier()
    {
        return Nodes.Count > 0 ? Nodes.Max(n => n.Tier) : 0;
    }

    /// <summary>
    /// Gets the minimum tier present in this branch.
    /// </summary>
    /// <returns>The lowest tier value among nodes, or 0 if no nodes exist.</returns>
    public int GetMinTier()
    {
        return Nodes.Count > 0 ? Nodes.Min(n => n.Tier) : 0;
    }

    /// <summary>
    /// Gets the total number of nodes in this branch.
    /// </summary>
    /// <returns>The count of nodes.</returns>
    public int GetNodeCount() => Nodes.Count;

    /// <summary>
    /// Gets the total points required to fully max all nodes in this branch.
    /// </summary>
    /// <returns>The sum of (PointCost * MaxRank) for all nodes.</returns>
    public int GetTotalPointsRequired()
    {
        return Nodes.Sum(n => n.GetTotalPointsToMax());
    }

    /// <summary>
    /// Finds a node by its ID within this branch.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>The matching node, or null if not found.</returns>
    public AbilityTreeNode? FindNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return Nodes.FirstOrDefault(n =>
            n.NodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks whether a node with the specified ID exists in this branch.
    /// </summary>
    /// <param name="nodeId">The node identifier to check.</param>
    /// <returns>True if the node exists; otherwise, false.</returns>
    public bool ContainsNode(string nodeId)
    {
        return FindNode(nodeId) is not null;
    }

    /// <summary>
    /// Gets all tier 1 (entry point) nodes in this branch.
    /// </summary>
    /// <returns>An enumerable of tier 1 nodes.</returns>
    public IEnumerable<AbilityTreeNode> GetEntryNodes()
    {
        return GetNodesAtTier(1);
    }

    /// <summary>
    /// Gets all nodes that have no prerequisites (typically tier 1 nodes).
    /// </summary>
    /// <returns>An enumerable of nodes with no prerequisites.</returns>
    public IEnumerable<AbilityTreeNode> GetNodesWithoutPrerequisites()
    {
        return Nodes.Where(n => !n.HasPrerequisites);
    }

    /// <summary>
    /// Gets the distinct tiers present in this branch.
    /// </summary>
    /// <returns>An ordered enumerable of tier numbers.</returns>
    public IEnumerable<int> GetDistinctTiers()
    {
        return Nodes.Select(n => n.Tier).Distinct().OrderBy(t => t);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({BranchId})";
}
