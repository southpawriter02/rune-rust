using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to ability tree definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>IAbilityTreeProvider is responsible for loading and caching ability tree definitions
/// from JSON configuration files. It provides fast lookup by tree ID, class ID, and node ID.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item><description>Load ability trees from configuration on startup</description></item>
///   <item><description>Index trees by TreeId and ClassId for fast lookup</description></item>
///   <item><description>Index all nodes globally for cross-tree node searches</description></item>
///   <item><description>Provide branch lookups for UI navigation</description></item>
/// </list>
/// <para>Implementations should ensure thread-safe access and validate configuration on load.</para>
/// </remarks>
public interface IAbilityTreeProvider
{
    /// <summary>
    /// Gets the ability tree for a specific class.
    /// </summary>
    /// <param name="classId">The class identifier (e.g., "warrior", "mage").</param>
    /// <returns>
    /// The <see cref="AbilityTreeDefinition"/> for the class, or null if no tree is defined.
    /// </returns>
    /// <remarks>
    /// Lookup is case-insensitive. Each class has at most one ability tree.
    /// </remarks>
    AbilityTreeDefinition? GetTreeForClass(string classId);

    /// <summary>
    /// Gets an ability tree by its unique tree ID.
    /// </summary>
    /// <param name="treeId">The tree identifier (e.g., "warrior-tree").</param>
    /// <returns>
    /// The matching <see cref="AbilityTreeDefinition"/>, or null if not found.
    /// </returns>
    /// <remarks>
    /// Lookup is case-insensitive.
    /// </remarks>
    AbilityTreeDefinition? GetTree(string treeId);

    /// <summary>
    /// Gets all available ability trees.
    /// </summary>
    /// <returns>
    /// A read-only list of all configured <see cref="AbilityTreeDefinition"/> instances.
    /// </returns>
    /// <remarks>
    /// Returns an empty list if no trees are configured.
    /// </remarks>
    IReadOnlyList<AbilityTreeDefinition> GetAllTrees();

    /// <summary>
    /// Finds a node by its ID across all trees.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>
    /// The matching <see cref="AbilityTreeNode"/>, or null if not found in any tree.
    /// </returns>
    /// <remarks>
    /// Uses an internal index for O(1) lookup regardless of tree count.
    /// Lookup is case-insensitive.
    /// </remarks>
    AbilityTreeNode? FindNode(string nodeId);

    /// <summary>
    /// Gets the tree that contains a specific node.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>
    /// The <see cref="AbilityTreeDefinition"/> containing the node, or null if not found.
    /// </returns>
    /// <remarks>
    /// Useful for determining which class a talent belongs to.
    /// </remarks>
    AbilityTreeDefinition? GetTreeContainingNode(string nodeId);

    /// <summary>
    /// Gets the branch that contains a specific node.
    /// </summary>
    /// <param name="nodeId">The node identifier to find.</param>
    /// <returns>
    /// The <see cref="AbilityTreeBranch"/> containing the node, or null if not found.
    /// </returns>
    /// <remarks>
    /// Useful for UI navigation and determining specialization paths.
    /// </remarks>
    AbilityTreeBranch? GetBranchContainingNode(string nodeId);

    /// <summary>
    /// Gets all class IDs that have ability trees defined.
    /// </summary>
    /// <returns>
    /// A read-only list of class identifiers that have associated ability trees.
    /// </returns>
    /// <remarks>
    /// Useful for enumerating available classes with talent systems.
    /// </remarks>
    IReadOnlyList<string> GetClassesWithTrees();

    /// <summary>
    /// Checks whether an ability tree exists for the specified class.
    /// </summary>
    /// <param name="classId">The class identifier to check.</param>
    /// <returns>True if a tree exists for the class; otherwise, false.</returns>
    bool HasTreeForClass(string classId);

    /// <summary>
    /// Checks whether a node with the specified ID exists in any tree.
    /// </summary>
    /// <param name="nodeId">The node identifier to check.</param>
    /// <returns>True if the node exists; otherwise, false.</returns>
    bool NodeExists(string nodeId);

    /// <summary>
    /// Gets the total number of ability trees.
    /// </summary>
    int TreeCount { get; }

    /// <summary>
    /// Gets the total number of nodes across all trees.
    /// </summary>
    int TotalNodeCount { get; }
}
