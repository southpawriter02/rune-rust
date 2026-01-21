// ═══════════════════════════════════════════════════════════════════════════════
// NodeStateDtos.cs
// Data transfer objects for ability tree node state visualization.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for node state display information.
/// </summary>
/// <remarks>
/// <para>
/// Encapsulates all necessary data for rendering a node's current state,
/// including its unlock status, rank progress, and tree position.
/// </para>
/// <para>
/// Used by <see cref="UI.TreeNodeRenderer"/> to render individual nodes
/// with state-specific styling.
/// </para>
/// </remarks>
/// <param name="NodeId">The unique node identifier (e.g., "power-strike").</param>
/// <param name="NodeName">The display name of the node (e.g., "Power Strike").</param>
/// <param name="State">The current unlock state of the node.</param>
/// <param name="CurrentRank">The current rank invested in this node (0 if not unlocked).</param>
/// <param name="MaxRank">The maximum rank available for this node.</param>
/// <param name="PointCost">The talent point cost per rank.</param>
/// <param name="Tier">The tier level of the node (1-based).</param>
/// <param name="BranchId">The branch this node belongs to (e.g., "berserker").</param>
/// <example>
/// <code>
/// var dto = new NodeStateDisplayDto(
///     NodeId: "power-strike",
///     NodeName: "Power Strike",
///     State: NodeState.Available,
///     CurrentRank: 0,
///     MaxRank: 3,
///     PointCost: 1,
///     Tier: 1,
///     BranchId: "berserker");
/// </code>
/// </example>
public record NodeStateDisplayDto(
    string NodeId,
    string NodeName,
    NodeState State,
    int CurrentRank,
    int MaxRank,
    int PointCost,
    int Tier,
    string BranchId);

/// <summary>
/// Data transfer object for node rank progress display.
/// </summary>
/// <remarks>
/// <para>
/// Used to display rank progress for multi-rank nodes (e.g., "2/3").
/// Single-rank nodes display an empty progress string.
/// </para>
/// <para>
/// The <see cref="ProgressString"/> property formats the rank progress
/// for display in the node content area.
/// </para>
/// </remarks>
/// <param name="CurrentRank">The current rank invested (0 if not unlocked).</param>
/// <param name="MaxRank">The maximum rank available.</param>
/// <example>
/// <code>
/// // Multi-rank node with 2 of 3 ranks invested
/// var rankDto = new NodeRankDisplayDto(2, 3);
/// Console.WriteLine(rankDto.ProgressString); // "2/3"
/// Console.WriteLine(rankDto.IsMaxRank);       // false
/// 
/// // Single-rank node
/// var singleRank = new NodeRankDisplayDto(1, 1);
/// Console.WriteLine(singleRank.ProgressString); // ""
/// Console.WriteLine(singleRank.IsMaxRank);      // true
/// </code>
/// </example>
public record NodeRankDisplayDto(int CurrentRank, int MaxRank)
{
    /// <summary>
    /// Gets a value indicating whether the node is at maximum rank.
    /// </summary>
    /// <remarks>
    /// True when <see cref="CurrentRank"/> equals <see cref="MaxRank"/>.
    /// </remarks>
    public bool IsMaxRank => CurrentRank >= MaxRank;

    /// <summary>
    /// Gets the formatted progress string for multi-rank nodes.
    /// </summary>
    /// <remarks>
    /// <para>Returns "CurrentRank/MaxRank" format for multi-rank nodes.</para>
    /// <para>Returns an empty string for single-rank nodes (MaxRank == 1).</para>
    /// </remarks>
    public string ProgressString => MaxRank > 1 ? $"{CurrentRank}/{MaxRank}" : string.Empty;
}
