using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines an ability tree for a specific class.
/// </summary>
/// <remarks>
/// <para>AbilityTreeDefinition is the root entity for the ability tree system.
/// Each class has one ability tree containing multiple branches (specialization paths),
/// each with nodes (talents) organized by tiers.</para>
/// <para>Tree hierarchy:</para>
/// <code>
///   AbilityTreeDefinition (1 per class)
///   └── Branches (typically 3 per tree)
///       └── Nodes (talents organized by tier)
/// </code>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description>TreeId uniquely identifies the tree</description></item>
///   <item><description>ClassId links the tree to a specific class</description></item>
///   <item><description>PointsPerLevel determines talent point gain rate</description></item>
///   <item><description>Branches contain specialized talent paths</description></item>
/// </list>
/// <para>Example Warrior tree:</para>
/// <code>
///   warrior-tree
///   ├── Berserker (damage/fury)
///   ├── Guardian (defense/protection)
///   └── Champion (leadership/support)
/// </code>
/// </remarks>
public class AbilityTreeDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this tree definition instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this tree.
    /// </summary>
    /// <remarks>
    /// Used for configuration loading and lookup operations.
    /// Always stored in lowercase (e.g., "warrior-tree", "mage-tree").
    /// </remarks>
    public string TreeId { get; private set; } = null!;

    /// <summary>
    /// Gets the identifier of the class this tree belongs to.
    /// </summary>
    /// <remarks>
    /// References a ClassDefinition. Each class has exactly one ability tree.
    /// </remarks>
    public string ClassId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of this ability tree.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description of this ability tree.
    /// </summary>
    /// <remarks>
    /// Should describe the overall theme and capabilities of the class's talents.
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the number of talent points awarded per level.
    /// </summary>
    /// <remarks>
    /// <para>Default is 1 point per level.</para>
    /// <para>Players use these points to unlock nodes in the tree.</para>
    /// </remarks>
    public int PointsPerLevel { get; private set; } = 1;

    /// <summary>
    /// Gets the branches (specialization paths) in this tree.
    /// </summary>
    /// <remarks>
    /// Typically 3 branches per tree representing different playstyles.
    /// </remarks>
    public IReadOnlyList<AbilityTreeBranch> Branches { get; private set; } = [];

    /// <summary>
    /// Gets the path to the tree's icon asset.
    /// </summary>
    /// <remarks>
    /// Used in the UI to represent this ability tree.
    /// </remarks>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core and serialization.
    /// </summary>
    private AbilityTreeDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new ability tree definition with the specified properties.
    /// </summary>
    /// <param name="treeId">Unique string identifier for the tree (will be lowercased).</param>
    /// <param name="classId">The class this tree belongs to (will be lowercased).</param>
    /// <param name="name">Display name of the tree.</param>
    /// <param name="description">Description of the tree's theme and capabilities.</param>
    /// <param name="pointsPerLevel">Points awarded per level (default: 1).</param>
    /// <param name="iconPath">Path to the tree's icon (optional).</param>
    /// <returns>A new AbilityTreeDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when treeId, classId, or name is null or whitespace.</exception>
    public static AbilityTreeDefinition Create(
        string treeId,
        string classId,
        string name,
        string description,
        int pointsPerLevel = 1,
        string? iconPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(treeId, nameof(treeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(classId, nameof(classId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new AbilityTreeDefinition
        {
            Id = Guid.NewGuid(),
            TreeId = treeId.ToLowerInvariant(),
            ClassId = classId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            PointsPerLevel = Math.Max(1, pointsPerLevel),
            IconPath = iconPath
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the branches for this tree.
    /// </summary>
    /// <param name="branches">The branches to set.</param>
    /// <remarks>
    /// Used during tree construction from configuration.
    /// </remarks>
    public void SetBranches(IReadOnlyList<AbilityTreeBranch> branches)
    {
        Branches = branches ?? [];
    }

    /// <summary>
    /// Gets all nodes across all branches in this tree.
    /// </summary>
    /// <returns>An enumerable of all nodes in the tree.</returns>
    public IEnumerable<AbilityTreeNode> GetAllNodes()
    {
        return Branches.SelectMany(b => b.Nodes);
    }

    /// <summary>
    /// Finds a node by its ID across all branches.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>The matching node, or null if not found.</returns>
    public AbilityTreeNode? FindNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return GetAllNodes().FirstOrDefault(n =>
            n.NodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the total points needed to fully unlock all nodes in this tree.
    /// </summary>
    /// <returns>The sum of (PointCost * MaxRank) for all nodes across all branches.</returns>
    public int GetTotalPointsRequired()
    {
        return GetAllNodes().Sum(n => n.GetTotalPointsToMax());
    }

    /// <summary>
    /// Gets all nodes at a specific tier across all branches.
    /// </summary>
    /// <param name="tier">The tier to filter by (1, 2, 3, etc.).</param>
    /// <returns>An enumerable of nodes at the specified tier.</returns>
    public IEnumerable<AbilityTreeNode> GetNodesAtTier(int tier)
    {
        return GetAllNodes().Where(n => n.Tier == tier);
    }

    /// <summary>
    /// Gets the maximum tier present in this tree.
    /// </summary>
    /// <returns>The highest tier value among all nodes, or 0 if no nodes exist.</returns>
    public int GetMaxTier()
    {
        var allNodes = GetAllNodes().ToList();
        return allNodes.Count > 0 ? allNodes.Max(n => n.Tier) : 0;
    }

    /// <summary>
    /// Gets the total number of nodes in this tree.
    /// </summary>
    /// <returns>The count of all nodes across all branches.</returns>
    public int GetTotalNodeCount()
    {
        return GetAllNodes().Count();
    }

    /// <summary>
    /// Gets the number of branches in this tree.
    /// </summary>
    /// <returns>The count of branches.</returns>
    public int GetBranchCount() => Branches.Count;

    /// <summary>
    /// Finds a branch by its ID.
    /// </summary>
    /// <param name="branchId">The branch identifier to find.</param>
    /// <returns>The matching branch, or null if not found.</returns>
    public AbilityTreeBranch? FindBranch(string branchId)
    {
        if (string.IsNullOrWhiteSpace(branchId))
            return null;

        return Branches.FirstOrDefault(b =>
            b.BranchId.Equals(branchId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the branch that contains the specified node.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>The containing branch, or null if the node is not found.</returns>
    public AbilityTreeBranch? GetBranchContainingNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return Branches.FirstOrDefault(b => b.ContainsNode(nodeId));
    }

    /// <summary>
    /// Checks whether a node with the specified ID exists in this tree.
    /// </summary>
    /// <param name="nodeId">The node identifier to check.</param>
    /// <returns>True if the node exists; otherwise, false.</returns>
    public bool ContainsNode(string nodeId)
    {
        return FindNode(nodeId) is not null;
    }

    /// <summary>
    /// Gets all entry point (tier 1) nodes in this tree.
    /// </summary>
    /// <returns>An enumerable of tier 1 nodes across all branches.</returns>
    public IEnumerable<AbilityTreeNode> GetEntryNodes()
    {
        return GetNodesAtTier(1);
    }

    /// <summary>
    /// Gets the distinct tiers present in this tree.
    /// </summary>
    /// <returns>An ordered enumerable of tier numbers.</returns>
    public IEnumerable<int> GetDistinctTiers()
    {
        return GetAllNodes().Select(n => n.Tier).Distinct().OrderBy(t => t);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({TreeId}) for {ClassId}";
}
