using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Validates whether a player meets the prerequisites for a talent node.
/// </summary>
/// <remarks>
/// <para>PrerequisiteValidator checks both types of prerequisites:</para>
/// <list type="bullet">
///   <item><description>Node prerequisites: Other talents that must be unlocked (rank >= 1)</description></item>
///   <item><description>Stat prerequisites: Minimum player attribute values</description></item>
/// </list>
/// <para>The validator uses the player's effective attributes (base + modifiers)
/// for stat checks, ensuring buffs and equipment are considered.</para>
/// <para>Stat IDs supported: might, fortitude, will, wits, finesse (with short aliases).</para>
/// </remarks>
public class PrerequisiteValidator : IPrerequisiteValidator
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IAbilityTreeProvider _treeProvider;
    private readonly ILogger<PrerequisiteValidator> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the PrerequisiteValidator class.
    /// </summary>
    /// <param name="treeProvider">Provider for ability tree definitions and node lookups.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public PrerequisiteValidator(
        IAbilityTreeProvider treeProvider,
        ILogger<PrerequisiteValidator> logger)
    {
        _treeProvider = treeProvider ?? throw new ArgumentNullException(nameof(treeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("PrerequisiteValidator initialized with tree provider");
    }

    // ═══════════════════════════════════════════════════════════════
    // IPrerequisiteValidator IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// <para>Validates all prerequisites and returns a detailed result:</para>
    /// <list type="bullet">
    ///   <item><description>Checks all node prerequisites first</description></item>
    ///   <item><description>Then checks all stat prerequisites</description></item>
    ///   <item><description>Collects all failure reasons for UI feedback</description></item>
    /// </list>
    /// </remarks>
    public PrerequisiteResult ValidatePrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogDebug(
            "Validating prerequisites for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        // Collect all failure reasons for comprehensive feedback
        var failureReasons = new List<string>();

        // ═══════════════════════════════════════════════════════════════
        // NODE PREREQUISITE VALIDATION
        // ═══════════════════════════════════════════════════════════════

        // Check each required node - must have at least rank 1 (unlocked)
        foreach (var prereqNodeId in node.PrerequisiteNodeIds)
        {
            _logger.LogTrace(
                "Checking node prerequisite {PrereqNodeId} for node {NodeId}",
                prereqNodeId,
                node.NodeId);

            var allocation = player.GetAllocation(prereqNodeId);

            // Prerequisite not met if: no allocation exists OR rank is 0
            if (allocation is null || allocation.CurrentRank < 1)
            {
                // Try to get the prerequisite node's name for user-friendly message
                var prereqNode = _treeProvider.FindNode(prereqNodeId);
                var displayName = prereqNode?.Name ?? prereqNodeId;

                var reason = $"Requires '{displayName}' to be unlocked";
                failureReasons.Add(reason);

                _logger.LogDebug(
                    "Node prerequisite not met: {PrereqNodeId} (player has rank {Rank})",
                    prereqNodeId,
                    allocation?.CurrentRank ?? 0);
            }
            else
            {
                _logger.LogTrace(
                    "Node prerequisite {PrereqNodeId} met (rank {Rank})",
                    prereqNodeId,
                    allocation.CurrentRank);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // STAT PREREQUISITE VALIDATION
        // ═══════════════════════════════════════════════════════════════

        // Get effective attributes (includes base stats + modifiers from buffs/equipment)
        var attributes = player.GetEffectiveAttributes();

        // Check each stat prerequisite
        foreach (var statPrereq in node.StatPrerequisites)
        {
            _logger.LogTrace(
                "Checking stat prerequisite {StatId} >= {MinValue} for node {NodeId}",
                statPrereq.StatId,
                statPrereq.MinValue,
                node.NodeId);

            // GetByName handles case-insensitive matching and aliases
            var actualValue = attributes.GetByName(statPrereq.StatId);

            // Check if the stat value meets the minimum requirement
            if (actualValue < statPrereq.MinValue)
            {
                var statDisplayName = FormatStatName(statPrereq.StatId);
                var reason = $"Requires {statDisplayName} >= {statPrereq.MinValue} (have {actualValue})";
                failureReasons.Add(reason);

                _logger.LogDebug(
                    "Stat prerequisite not met: {StatId} >= {Required} (player has {Actual})",
                    statPrereq.StatId,
                    statPrereq.MinValue,
                    actualValue);
            }
            else
            {
                _logger.LogTrace(
                    "Stat prerequisite {StatId} met ({Actual} >= {Required})",
                    statPrereq.StatId,
                    actualValue,
                    statPrereq.MinValue);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BUILD RESULT
        // ═══════════════════════════════════════════════════════════════

        if (failureReasons.Count > 0)
        {
            _logger.LogDebug(
                "Prerequisites not met for player {PlayerName} on node {NodeId}: {Count} failures",
                player.Name,
                node.NodeId,
                failureReasons.Count);

            return PrerequisiteResult.Invalid(failureReasons);
        }

        _logger.LogDebug(
            "All prerequisites met for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        return PrerequisiteResult.Valid();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Efficiently checks only node prerequisites without full validation.
    /// Returns true if all required nodes have at least rank 1.
    /// </remarks>
    public bool MeetsNodePrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogTrace(
            "Checking node prerequisites for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        // If no node prerequisites, automatically passes
        if (node.PrerequisiteNodeIds.Count == 0)
        {
            _logger.LogTrace(
                "Node {NodeId} has no node prerequisites - passes",
                node.NodeId);
            return true;
        }

        // All prerequisite nodes must have at least rank 1
        foreach (var prereqNodeId in node.PrerequisiteNodeIds)
        {
            var allocation = player.GetAllocation(prereqNodeId);

            if (allocation is null || allocation.CurrentRank < 1)
            {
                _logger.LogTrace(
                    "Node prerequisite {PrereqNodeId} not met for player {PlayerName}",
                    prereqNodeId,
                    player.Name);
                return false;
            }
        }

        _logger.LogTrace(
            "All node prerequisites met for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Efficiently checks only stat prerequisites without full validation.
    /// Uses player's effective attributes (including modifiers).
    /// </remarks>
    public bool MeetsStatPrerequisites(Player player, AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        _logger.LogTrace(
            "Checking stat prerequisites for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        // If no stat prerequisites, automatically passes
        if (node.StatPrerequisites.Count == 0)
        {
            _logger.LogTrace(
                "Node {NodeId} has no stat prerequisites - passes",
                node.NodeId);
            return true;
        }

        // Get effective attributes once for efficiency
        var attributes = player.GetEffectiveAttributes();

        // All stat prerequisites must be met
        foreach (var statPrereq in node.StatPrerequisites)
        {
            var actualValue = attributes.GetByName(statPrereq.StatId);

            if (actualValue < statPrereq.MinValue)
            {
                _logger.LogTrace(
                    "Stat prerequisite {StatId} >= {Required} not met for player {PlayerName} (has {Actual})",
                    statPrereq.StatId,
                    statPrereq.MinValue,
                    player.Name,
                    actualValue);
                return false;
            }
        }

        _logger.LogTrace(
            "All stat prerequisites met for player {PlayerName} on node {NodeId}",
            player.Name,
            node.NodeId);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a stat ID into a user-friendly display name.
    /// </summary>
    /// <param name="statId">The stat identifier to format.</param>
    /// <returns>A properly capitalized stat name.</returns>
    /// <remarks>
    /// Handles both full names (might) and abbreviations (mig).
    /// Unknown stat IDs are returned with title case.
    /// </remarks>
    private static string FormatStatName(string statId)
    {
        // Normalize and map to display name
        return statId.ToLowerInvariant() switch
        {
            "might" or "mig" => "Might",
            "fortitude" or "for" => "Fortitude",
            "will" or "wil" => "Will",
            "wits" or "wit" => "Wits",
            "finesse" or "fin" => "Finesse",
            // Fallback: capitalize first letter for unknown stats
            _ => char.ToUpperInvariant(statId[0]) + statId[1..].ToLowerInvariant()
        };
    }
}
