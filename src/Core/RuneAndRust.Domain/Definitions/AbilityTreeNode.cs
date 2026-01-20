using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a single node (talent) in an ability tree.
/// </summary>
/// <remarks>
/// <para>AbilityTreeNode defines an individual talent that players can invest points into.
/// Each node belongs to a branch within an ability tree and grants access to an ability.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description>NodeId uniquely identifies the node across all trees</description></item>
///   <item><description>AbilityId references the ability granted when points are invested</description></item>
///   <item><description>Tier determines the node's position in the talent hierarchy</description></item>
///   <item><description>PointCost is the cost per rank to invest in this node</description></item>
///   <item><description>MaxRank allows for multi-rank abilities (e.g., 3 ranks at 10% per rank)</description></item>
///   <item><description>PrerequisiteNodeIds lists nodes that must be unlocked first</description></item>
///   <item><description>StatPrerequisites lists minimum stat requirements</description></item>
/// </list>
/// <para>Example node hierarchy:</para>
/// <code>
///   Tier 1: Frenzy (no prerequisites, 3 ranks)
///      ↓
///   Tier 2: Rage (requires Frenzy, requires STR 14)
/// </code>
/// </remarks>
public class AbilityTreeNode
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the unique identifier for this node.
    /// </summary>
    /// <remarks>
    /// The NodeId should be unique across all trees and is used for
    /// prerequisite references and lookup operations.
    /// </remarks>
    public string NodeId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the ability this node grants.
    /// </summary>
    /// <remarks>
    /// References an ability definition that becomes available when
    /// the player invests at least one point in this node.
    /// </remarks>
    public string AbilityId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display name of this talent.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of what this talent does.
    /// </summary>
    /// <remarks>
    /// Should include per-rank effects if applicable (e.g., "Increases attack speed by 10% per rank").
    /// </remarks>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tier level of this node.
    /// </summary>
    /// <remarks>
    /// <para>Tiers organize nodes hierarchically:</para>
    /// <list type="bullet">
    ///   <item><description>Tier 1: Base talents, typically no prerequisites</description></item>
    ///   <item><description>Tier 2: Advanced talents, require Tier 1 nodes</description></item>
    ///   <item><description>Tier 3+: Specialized talents with multiple prerequisites</description></item>
    /// </list>
    /// </remarks>
    public int Tier { get; set; }

    /// <summary>
    /// Gets or sets the point cost per rank for this node.
    /// </summary>
    /// <remarks>
    /// Higher tier talents typically have higher point costs.
    /// Total cost to max a node = PointCost * MaxRank.
    /// </remarks>
    public int PointCost { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of ranks for this talent.
    /// </summary>
    /// <remarks>
    /// <para>Most talents have 1 rank (unlocked or not).</para>
    /// <para>Multi-rank talents (2-5 ranks) provide scaling benefits.</para>
    /// </remarks>
    public int MaxRank { get; set; } = 1;

    /// <summary>
    /// Gets or sets the list of node IDs that must be unlocked before this node.
    /// </summary>
    /// <remarks>
    /// <para>All listed nodes must have at least one point invested.</para>
    /// <para>Empty list means no node prerequisites (tier 1 nodes typically).</para>
    /// </remarks>
    public IReadOnlyList<string> PrerequisiteNodeIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the stat requirements for this node.
    /// </summary>
    /// <remarks>
    /// All listed stat prerequisites must be met before investing points.
    /// </remarks>
    public IReadOnlyList<StatPrerequisite> StatPrerequisites { get; set; } = [];

    /// <summary>
    /// Gets or sets the position coordinates for UI layout.
    /// </summary>
    /// <remarks>
    /// Used by the UI to position nodes within the tree visualization.
    /// </remarks>
    public NodePosition Position { get; set; } = NodePosition.Origin;

    /// <summary>
    /// Gets or sets the path to the node's icon asset.
    /// </summary>
    /// <remarks>
    /// May be null if the ability's default icon should be used.
    /// </remarks>
    public string? IconPath { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this node has any prerequisites (node or stat).
    /// </summary>
    /// <value>
    /// True if either PrerequisiteNodeIds or StatPrerequisites has entries;
    /// otherwise, false.
    /// </value>
    public bool HasPrerequisites => PrerequisiteNodeIds.Count > 0 || StatPrerequisites.Count > 0;

    /// <summary>
    /// Gets whether this node has node prerequisites.
    /// </summary>
    /// <value>True if PrerequisiteNodeIds is not empty; otherwise, false.</value>
    public bool HasNodePrerequisites => PrerequisiteNodeIds.Count > 0;

    /// <summary>
    /// Gets whether this node has stat prerequisites.
    /// </summary>
    /// <value>True if StatPrerequisites is not empty; otherwise, false.</value>
    public bool HasStatPrerequisites => StatPrerequisites.Count > 0;

    /// <summary>
    /// Gets whether this is a multi-rank talent.
    /// </summary>
    /// <value>True if MaxRank is greater than 1; otherwise, false.</value>
    public bool IsMultiRank => MaxRank > 1;

    /// <summary>
    /// Gets whether this is a tier 1 (base) node.
    /// </summary>
    /// <value>True if Tier equals 1; otherwise, false.</value>
    public bool IsTierOne => Tier == 1;

    // ═══════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total points required to fully max this node.
    /// </summary>
    /// <returns>The product of PointCost and MaxRank.</returns>
    public int GetTotalPointsToMax() => PointCost * MaxRank;

    /// <summary>
    /// Checks whether the specified node ID is a prerequisite for this node.
    /// </summary>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if the node ID is in the prerequisites list; otherwise, false.</returns>
    public bool RequiresNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return false;

        return PrerequisiteNodeIds.Any(id =>
            id.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks whether the specified stat meets the requirement for this node.
    /// </summary>
    /// <param name="statId">The stat identifier to check.</param>
    /// <param name="statValue">The actual stat value.</param>
    /// <returns>True if the stat meets the requirement or no requirement exists; otherwise, false.</returns>
    public bool MeetsStatRequirement(string statId, int statValue)
    {
        var requirement = StatPrerequisites.FirstOrDefault(sp =>
            sp.StatId.Equals(statId, StringComparison.OrdinalIgnoreCase));

        // If no requirement for this stat, it's considered met
        if (requirement.StatId is null)
            return true;

        return requirement.IsMet(statValue);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({NodeId}) T{Tier}";
}
