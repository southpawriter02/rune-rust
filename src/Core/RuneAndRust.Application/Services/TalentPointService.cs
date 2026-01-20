using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing talent point allocation and tracking.
/// </summary>
/// <remarks>
/// <para>TalentPointService implements the core talent point economy:</para>
/// <list type="bullet">
///   <item><description>Awarding points to players (typically on level-up)</description></item>
///   <item><description>Validating and processing point spending on nodes</description></item>
///   <item><description>Tracking allocations and calculating totals</description></item>
///   <item><description>Publishing events for system integration</description></item>
/// </list>
/// <para>Thread safety: The service is stateless and safe for concurrent use.
/// However, the Player entity being modified is not thread-safe and should
/// be protected by the caller if concurrent access is possible.</para>
/// </remarks>
public class TalentPointService : ITalentPointService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IAbilityTreeProvider _treeProvider;
    private readonly IPrerequisiteValidator _prerequisiteValidator;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<TalentPointService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the TalentPointService class.
    /// </summary>
    /// <param name="treeProvider">Provider for ability tree definitions.</param>
    /// <param name="prerequisiteValidator">Validator for node prerequisites.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public TalentPointService(
        IAbilityTreeProvider treeProvider,
        IPrerequisiteValidator prerequisiteValidator,
        IGameEventLogger eventLogger,
        ILogger<TalentPointService> logger)
    {
        _treeProvider = treeProvider ?? throw new ArgumentNullException(nameof(treeProvider));
        _prerequisiteValidator = prerequisiteValidator ?? throw new ArgumentNullException(nameof(prerequisiteValidator));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("TalentPointService initialized with {TreeCount} trees available",
            _treeProvider.TreeCount);
    }

    // ═══════════════════════════════════════════════════════════════
    // POINT QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int GetUnspentPoints(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogTrace("GetUnspentPoints called for player {PlayerId}: {Unspent}",
            player.Id, player.UnspentTalentPoints);

        return player.UnspentTalentPoints;
    }

    /// <inheritdoc />
    public int GetTotalPointsEarned(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogTrace("GetTotalPointsEarned called for player {PlayerId}: {TotalEarned}",
            player.Id, player.TotalTalentPointsEarned);

        return player.TotalTalentPointsEarned;
    }

    /// <inheritdoc />
    public int GetTotalPointsSpent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var totalSpent = player.TalentAllocations.Sum(a => a.GetTotalPointsSpent());

        _logger.LogTrace("GetTotalPointsSpent called for player {PlayerId}: {TotalSpent} " +
            "across {NodeCount} nodes",
            player.Id, totalSpent, player.TalentAllocations.Count);

        return totalSpent;
    }

    /// <inheritdoc />
    public TalentPointSummary GetPointSummary(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var summary = new TalentPointSummary(
            Unspent: player.UnspentTalentPoints,
            TotalEarned: player.TotalTalentPointsEarned,
            TotalSpent: GetTotalPointsSpent(player),
            AllocatedNodes: player.TalentAllocations.Count);

        _logger.LogTrace("GetPointSummary for player {PlayerId}: " +
            "Unspent={Unspent}, Earned={Earned}, Spent={Spent}, Nodes={Nodes}",
            player.Id, summary.Unspent, summary.TotalEarned, summary.TotalSpent, summary.AllocatedNodes);

        return summary;
    }

    // ═══════════════════════════════════════════════════════════════
    // POINT OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void AwardPoints(Player player, int count)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));

        _logger.LogDebug("Awarding {Count} talent point(s) to player {PlayerId} ({PlayerName})",
            count, player.Id, player.Name);

        // Award the points to the player
        player.AddTalentPoints(count);

        _logger.LogInformation(
            "Player {PlayerName} earned {PointsEarned} talent point(s). " +
            "Total unspent: {UnspentPoints}, Total earned: {TotalEarned}",
            player.Name,
            count,
            player.UnspentTalentPoints,
            player.TotalTalentPointsEarned);

        // Log the event
        _eventLogger.LogCharacter(
            "TalentPointEarned",
            $"{player.Name} earned {count} talent point(s). Total unspent: {player.UnspentTalentPoints}",
            player.Id,
            new Dictionary<string, object>
            {
                ["pointsEarned"] = count,
                ["totalUnspent"] = player.UnspentTalentPoints,
                ["totalEarned"] = player.TotalTalentPointsEarned
            });
    }

    /// <inheritdoc />
    public AllocationResult SpendPoint(Player player, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId, nameof(nodeId));

        _logger.LogDebug("SpendPoint called: Player {PlayerId} ({PlayerName}) attempting " +
            "to allocate to node {NodeId}",
            player.Id, player.Name, nodeId);

        // Step 1: Find the node in the tree provider
        var node = _treeProvider.FindNode(nodeId);
        if (node is null)
        {
            _logger.LogWarning("Node not found: {NodeId}. Player {PlayerId} cannot allocate.",
                nodeId, player.Id);
            return AllocationResult.Failed($"Node not found: {nodeId}");
        }

        _logger.LogTrace("Found node {NodeId}: {NodeName}, Tier {Tier}, Cost {Cost}, MaxRank {MaxRank}",
            node.NodeId, node.Name, node.Tier, node.PointCost, node.MaxRank);

        // Step 2: Check if player has enough points
        if (player.UnspentTalentPoints < node.PointCost)
        {
            _logger.LogDebug(
                "Player {PlayerName} has insufficient points for {NodeId}: " +
                "need {Required}, have {Available}",
                player.Name, nodeId, node.PointCost, player.UnspentTalentPoints);

            return AllocationResult.InsufficientPoints(node.PointCost, player.UnspentTalentPoints);
        }

        // Step 3: Check current rank against max rank
        var currentRank = GetNodeRank(player, nodeId);
        if (currentRank >= node.MaxRank)
        {
            _logger.LogDebug(
                "Player {PlayerName} already at max rank {MaxRank} for node {NodeId}",
                player.Name, node.MaxRank, nodeId);

            return AllocationResult.AtMaxRank(nodeId, node.MaxRank);
        }

        // Step 4: Validate prerequisites
        var prereqResult = _prerequisiteValidator.ValidatePrerequisites(player, node);
        if (!prereqResult.IsValid)
        {
            _logger.LogDebug(
                "Player {PlayerName} does not meet prerequisites for {NodeId}: {Reasons}",
                player.Name, nodeId, string.Join(", ", prereqResult.FailureReasons));

            return AllocationResult.PrerequisitesNotMet(prereqResult.FailureReasons);
        }

        // Step 5: Perform the allocation
        var existingAllocation = player.GetAllocation(nodeId);
        var isFirstRank = existingAllocation is null;
        TalentAllocation allocation;

        if (isFirstRank)
        {
            // Create new allocation at rank 1
            allocation = TalentAllocation.Create(nodeId, node.PointCost);
            player.AddTalentAllocation(allocation);

            _logger.LogDebug("Created new allocation for node {NodeId} at rank 1", nodeId);
        }
        else
        {
            // Increment existing allocation
            allocation = existingAllocation!;
            allocation.IncrementRank();

            _logger.LogDebug("Incremented allocation for node {NodeId} to rank {NewRank}",
                nodeId, allocation.CurrentRank);
        }

        // Step 6: Deduct points from player
        player.SpendTalentPoints(node.PointCost);

        _logger.LogInformation(
            "Player {PlayerName} spent {PointCost} point(s) on {NodeId} ({NodeName}). " +
            "Now rank {CurrentRank}/{MaxRank}. Remaining unspent: {UnspentPoints}",
            player.Name,
            node.PointCost,
            nodeId,
            node.Name,
            allocation.CurrentRank,
            node.MaxRank,
            player.UnspentTalentPoints);

        // Step 7: Log the spend event
        _eventLogger.LogCharacter(
            "TalentPointSpent",
            $"{player.Name} spent {node.PointCost} point(s) on {node.Name} (rank {allocation.CurrentRank}/{node.MaxRank})",
            player.Id,
            new Dictionary<string, object>
            {
                ["nodeId"] = nodeId,
                ["nodeName"] = node.Name,
                ["pointsSpent"] = node.PointCost,
                ["newRank"] = allocation.CurrentRank,
                ["maxRank"] = node.MaxRank,
                ["unspentRemaining"] = player.UnspentTalentPoints
            });

        // Step 8: If first rank, log ability unlock event
        if (isFirstRank)
        {
            _logger.LogInformation(
                "Player {PlayerName} unlocked ability {AbilityId} from talent node {NodeId}",
                player.Name, node.AbilityId, nodeId);

            _eventLogger.LogAbility(
                "AbilityUnlocked",
                $"{player.Name} unlocked ability {node.AbilityId} from talent {node.Name}",
                null,
                new Dictionary<string, object>
                {
                    ["playerId"] = player.Id,
                    ["playerName"] = player.Name,
                    ["abilityId"] = node.AbilityId,
                    ["nodeId"] = nodeId,
                    ["nodeName"] = node.Name
                });
        }

        return AllocationResult.Success(nodeId, allocation.CurrentRank);
    }

    // ═══════════════════════════════════════════════════════════════
    // ALLOCATION QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public TalentAllocation? GetAllocation(Player player, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        var allocation = player.GetAllocation(nodeId);

        _logger.LogTrace("GetAllocation for player {PlayerId}, node {NodeId}: {Result}",
            player.Id, nodeId, allocation is not null ? $"rank {allocation.CurrentRank}" : "not found");

        return allocation;
    }

    /// <inheritdoc />
    public int GetNodeRank(Player player, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var allocation = GetAllocation(player, nodeId);
        var rank = allocation?.CurrentRank ?? 0;

        _logger.LogTrace("GetNodeRank for player {PlayerId}, node {NodeId}: {Rank}",
            player.Id, nodeId ?? "(null)", rank);

        return rank;
    }

    /// <inheritdoc />
    public IReadOnlyList<TalentAllocation> GetAllAllocations(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogTrace("GetAllAllocations for player {PlayerId}: {Count} allocation(s)",
            player.Id, player.TalentAllocations.Count);

        return player.TalentAllocations;
    }

    /// <inheritdoc />
    public IReadOnlyList<TalentAllocation> GetAllocationsForTree(Player player, string treeId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (string.IsNullOrWhiteSpace(treeId))
        {
            _logger.LogTrace("GetAllocationsForTree called with null/empty treeId, returning empty");
            return [];
        }

        var tree = _treeProvider.GetTree(treeId);
        if (tree is null)
        {
            _logger.LogTrace("Tree {TreeId} not found, returning empty allocations list", treeId);
            return [];
        }

        // Get all node IDs in this tree
        var treeNodeIds = tree.GetAllNodes()
            .Select(n => n.NodeId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Filter player's allocations to only those in this tree
        var allocations = player.TalentAllocations
            .Where(a => treeNodeIds.Contains(a.NodeId))
            .ToList();

        _logger.LogTrace("GetAllocationsForTree for player {PlayerId}, tree {TreeId}: " +
            "{Count} allocation(s) out of {TotalNodes} nodes",
            player.Id, treeId, allocations.Count, treeNodeIds.Count);

        return allocations;
    }

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY CHECKS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool CanSpendOn(Player player, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (string.IsNullOrWhiteSpace(nodeId))
        {
            _logger.LogTrace("CanSpendOn called with null/empty nodeId, returning false");
            return false;
        }

        // Check 1: Node must exist
        var node = _treeProvider.FindNode(nodeId);
        if (node is null)
        {
            _logger.LogTrace("CanSpendOn: Node {NodeId} not found", nodeId);
            return false;
        }

        // Check 2: Player must have enough points
        if (player.UnspentTalentPoints < node.PointCost)
        {
            _logger.LogTrace("CanSpendOn: Player {PlayerId} has {Unspent} points, " +
                "node {NodeId} requires {Cost}",
                player.Id, player.UnspentTalentPoints, nodeId, node.PointCost);
            return false;
        }

        // Check 3: Node must not be at max rank
        var currentRank = GetNodeRank(player, nodeId);
        if (currentRank >= node.MaxRank)
        {
            _logger.LogTrace("CanSpendOn: Node {NodeId} already at max rank {CurrentRank}/{MaxRank}",
                nodeId, currentRank, node.MaxRank);
            return false;
        }

        // Check 4: Prerequisites must be met
        var prereqResult = _prerequisiteValidator.ValidatePrerequisites(player, node);
        if (!prereqResult.IsValid)
        {
            _logger.LogTrace("CanSpendOn: Prerequisites not met for node {NodeId}: {Reasons}",
                nodeId, string.Join(", ", prereqResult.FailureReasons));
            return false;
        }

        _logger.LogTrace("CanSpendOn: Player {PlayerId} can spend on node {NodeId}", player.Id, nodeId);
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<AbilityTreeNode> GetAvailableNodes(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Get the player's class tree
        if (string.IsNullOrWhiteSpace(player.ClassId))
        {
            _logger.LogTrace("GetAvailableNodes: Player {PlayerId} has no class set", player.Id);
            return [];
        }

        var tree = _treeProvider.GetTreeForClass(player.ClassId);
        if (tree is null)
        {
            _logger.LogTrace("GetAvailableNodes: No tree found for class {ClassId}", player.ClassId);
            return [];
        }

        // Filter to nodes the player can spend on
        var availableNodes = tree.GetAllNodes()
            .Where(n => CanSpendOn(player, n.NodeId))
            .ToList();

        _logger.LogTrace("GetAvailableNodes for player {PlayerId} (class {ClassId}): " +
            "{AvailableCount} of {TotalCount} nodes available",
            player.Id, player.ClassId, availableNodes.Count, tree.GetAllNodes().Count());

        return availableNodes;
    }
}
