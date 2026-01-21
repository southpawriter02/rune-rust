// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTreeDtos.cs
// Data transfer objects for ability tree visualization.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for complete ability tree display.
/// </summary>
/// <remarks>
/// <para>
/// TreeDisplayDto is the root DTO for rendering an ability tree.
/// It contains all tier and branch information needed for visualization.
/// </para>
/// <para>
/// Created from <c>AbilityTreeDefinition</c> in the domain layer.
/// </para>
/// </remarks>
/// <param name="TreeId">The unique tree identifier (e.g., "warrior-tree").</param>
/// <param name="TreeName">The display name of the tree (e.g., "WARRIOR ABILITY TREE").</param>
/// <param name="ClassId">The associated character class (e.g., "warrior").</param>
/// <param name="Tiers">The tier display data collection, ordered by tier number.</param>
/// <param name="Branches">The branch display data collection.</param>
/// <param name="MaxTier">The maximum tier number in the tree.</param>
public record TreeDisplayDto(
    string TreeId,
    string TreeName,
    string ClassId,
    IReadOnlyList<TierDisplayDto> Tiers,
    IReadOnlyList<BranchDisplayDto> Branches,
    int MaxTier);

/// <summary>
/// Data transfer object for a single tier column display.
/// </summary>
/// <remarks>
/// <para>
/// Each tier is displayed as a vertical column with a label and nodes.
/// Nodes are arranged vertically within the column based on their definition Y position.
/// </para>
/// </remarks>
/// <param name="TierNumber">The tier number (1-based).</param>
/// <param name="TierLabel">The display label (e.g., "TIER 1").</param>
/// <param name="Nodes">The nodes in this tier, ordered by definition Y position.</param>
public record TierDisplayDto(
    int TierNumber,
    string TierLabel,
    IReadOnlyList<NodeLayoutDto> Nodes);

/// <summary>
/// Data transfer object for node layout information.
/// </summary>
/// <remarks>
/// <para>
/// Contains positional information from the tree definition plus display data.
/// Used by <c>TreeLayoutRenderer</c> to calculate screen positions.
/// </para>
/// </remarks>
/// <param name="NodeId">The unique node identifier (e.g., "power-strike").</param>
/// <param name="NodeName">The display name of the node (e.g., "Power Strike").</param>
/// <param name="Tier">The tier number (1-based).</param>
/// <param name="DefinitionX">The X position from the tree definition.</param>
/// <param name="DefinitionY">The Y position from the tree definition (used for vertical ordering).</param>
/// <param name="BranchId">The branch this node belongs to (e.g., "berserker").</param>
public record NodeLayoutDto(
    string NodeId,
    string NodeName,
    int Tier,
    int DefinitionX,
    int DefinitionY,
    string BranchId);

/// <summary>
/// Data transfer object for branch display information.
/// </summary>
/// <remarks>
/// <para>
/// Branches represent specialization paths within a tree (e.g., "Berserker", "Guardian").
/// Branch headers are displayed when multiple branches exist.
/// </para>
/// </remarks>
/// <param name="BranchId">The unique branch identifier (e.g., "berserker").</param>
/// <param name="BranchName">The display name of the branch (e.g., "Berserker").</param>
/// <param name="Description">A brief description of the branch theme.</param>
/// <param name="NodeCount">The total number of nodes in this branch.</param>
public record BranchDisplayDto(
    string BranchId,
    string BranchName,
    string Description,
    int NodeCount);

/// <summary>
/// Represents a calculated screen position for a node.
/// </summary>
/// <remarks>
/// <para>
/// Screen positions are calculated by <c>TreeLayoutRenderer</c> based on
/// configuration settings and node tier/index within the tree.
/// </para>
/// </remarks>
/// <param name="X">The X coordinate on screen (column position).</param>
/// <param name="Y">The Y coordinate on screen (row position).</param>
public record NodeScreenPosition(int X, int Y);
