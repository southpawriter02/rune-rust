using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for handling talent point reallocation (respec) operations.
/// </summary>
/// <remarks>
/// <para>RespecService implements the complete respec workflow:</para>
/// <list type="bullet">
///   <item><description>Validates eligibility (enabled, level, allocations, gold)</description></item>
///   <item><description>Refunds all spent talent points to the unspent pool</description></item>
///   <item><description>Clears all talent allocations</description></item>
///   <item><description>Removes abilities granted by talents</description></item>
///   <item><description>Deducts the gold cost</description></item>
///   <item><description>Logs events for system integration</description></item>
/// </list>
/// <para>Cost formula: BaseRespecCost + (PlayerLevel × LevelMultiplier)</para>
/// <para>Thread safety: The service is stateless and safe for concurrent use.
/// However, the Player entity being modified is not thread-safe and should
/// be protected by the caller if concurrent access is possible.</para>
/// </remarks>
public class RespecService : IRespecService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IAbilityTreeProvider _treeProvider;
    private readonly IRespecConfiguration _config;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<RespecService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the RespecService class.
    /// </summary>
    /// <param name="treeProvider">Provider for ability tree definitions and node lookups.</param>
    /// <param name="config">Configuration for respec costs and eligibility.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RespecService(
        IAbilityTreeProvider treeProvider,
        IRespecConfiguration config,
        IGameEventLogger eventLogger,
        ILogger<RespecService> logger)
    {
        _treeProvider = treeProvider ?? throw new ArgumentNullException(nameof(treeProvider));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "RespecService initialized. Config: BaseCost={BaseCost}, LevelMultiplier={Multiplier}, " +
            "MinLevel={MinLevel}, Enabled={Enabled}, Currency={Currency}",
            _config.BaseRespecCost,
            _config.LevelMultiplier,
            _config.MinimumLevelToRespec,
            _config.IsRespecEnabled,
            _config.CurrencyId);
    }

    // ═══════════════════════════════════════════════════════════════
    // COST QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int GetRespecCost(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Formula: BaseRespecCost + (Level × LevelMultiplier)
        var cost = _config.BaseRespecCost + (player.Level * _config.LevelMultiplier);

        _logger.LogTrace(
            "GetRespecCost for player {PlayerName} (level {Level}): " +
            "{BaseCost} + ({Level} × {Multiplier}) = {TotalCost}",
            player.Name,
            player.Level,
            _config.BaseRespecCost,
            player.Level,
            _config.LevelMultiplier,
            cost);

        return cost;
    }

    /// <inheritdoc />
    public bool CanAffordRespec(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var cost = GetRespecCost(player);
        var available = player.GetCurrency(_config.CurrencyId);
        var canAfford = available >= cost;

        _logger.LogTrace(
            "CanAffordRespec for player {PlayerName}: cost={Cost}, available={Available}, result={CanAfford}",
            player.Name,
            cost,
            available,
            canAfford);

        return canAfford;
    }

    // ═══════════════════════════════════════════════════════════════
    // ELIGIBILITY QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool HasAllocations(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var hasAllocations = player.TalentAllocations.Count > 0;

        _logger.LogTrace(
            "HasAllocations for player {PlayerName}: {Count} allocation(s), result={HasAllocations}",
            player.Name,
            player.TalentAllocations.Count,
            hasAllocations);

        return hasAllocations;
    }

    /// <inheritdoc />
    public int GetRefundAmount(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Sum up all points spent across all allocations
        var totalSpent = player.TalentAllocations.Sum(a => a.GetTotalPointsSpent());

        _logger.LogTrace(
            "GetRefundAmount for player {PlayerName}: {TotalSpent} points across {Count} allocation(s)",
            player.Name,
            totalSpent,
            player.TalentAllocations.Count);

        return totalSpent;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAbilitiesToRemove(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Build list of ability IDs from talent allocations
        var abilityIds = new List<string>();

        foreach (var allocation in player.TalentAllocations)
        {
            // Find the node to get its ability ID
            var node = _treeProvider.FindNode(allocation.NodeId);
            if (node is not null && !string.IsNullOrWhiteSpace(node.AbilityId))
            {
                abilityIds.Add(node.AbilityId);

                _logger.LogTrace(
                    "Ability {AbilityId} from node {NodeId} marked for removal",
                    node.AbilityId,
                    allocation.NodeId);
            }
            else
            {
                _logger.LogWarning(
                    "Could not find ability for node {NodeId} during respec preview",
                    allocation.NodeId);
            }
        }

        _logger.LogTrace(
            "GetAbilitiesToRemove for player {PlayerName}: {Count} abilities from {AllocationCount} allocations",
            player.Name,
            abilityIds.Count,
            player.TalentAllocations.Count);

        return abilityIds;
    }

    // ═══════════════════════════════════════════════════════════════
    // RESPEC OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public RespecResult Respec(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug(
            "Respec initiated for player {PlayerId} ({PlayerName}), level {Level}",
            player.Id,
            player.Name,
            player.Level);

        // ═══════════════════════════════════════════════════════════════
        // VALIDATION PHASE
        // ═══════════════════════════════════════════════════════════════

        // Check 1: Is respec feature enabled?
        if (!_config.IsRespecEnabled)
        {
            _logger.LogDebug(
                "Respec denied for player {PlayerName}: feature is disabled",
                player.Name);

            LogRespecFailed(player, "Disabled", "Respec feature is disabled");
            return RespecResult.Disabled();
        }

        // Check 2: Does player meet minimum level requirement?
        if (player.Level < _config.MinimumLevelToRespec)
        {
            _logger.LogDebug(
                "Respec denied for player {PlayerName}: level {CurrentLevel} < required {RequiredLevel}",
                player.Name,
                player.Level,
                _config.MinimumLevelToRespec);

            LogRespecFailed(player, "LevelTooLow",
                $"Level {player.Level} < required {_config.MinimumLevelToRespec}");
            return RespecResult.LevelTooLow(_config.MinimumLevelToRespec, player.Level);
        }

        // Check 3: Does player have any allocations to reset?
        if (!HasAllocations(player))
        {
            _logger.LogDebug(
                "Respec denied for player {PlayerName}: no talent allocations to reset",
                player.Name);

            LogRespecFailed(player, "NoAllocations", "No talent allocations to reset");
            return RespecResult.NothingToRespec();
        }

        // Check 4: Can player afford the respec cost?
        var cost = GetRespecCost(player);
        var availableGold = player.GetCurrency(_config.CurrencyId);
        if (!CanAffordRespec(player))
        {
            _logger.LogDebug(
                "Respec denied for player {PlayerName}: cannot afford cost {Cost} (has {Available} {Currency})",
                player.Name,
                cost,
                availableGold,
                _config.CurrencyId);

            LogRespecFailed(player, "CannotAfford",
                $"Cannot afford {cost} {_config.CurrencyId} (has {availableGold})");
            return RespecResult.CannotAfford(cost, availableGold);
        }

        // ═══════════════════════════════════════════════════════════════
        // PREPARATION PHASE
        // ═══════════════════════════════════════════════════════════════

        // Capture values before modification for logging and result
        var pointsToRefund = GetRefundAmount(player);
        var abilitiesToRemove = GetAbilitiesToRemove(player);
        var allocationCount = player.TalentAllocations.Count;

        _logger.LogDebug(
            "Respec preparation for player {PlayerName}: " +
            "refunding {Points} points, removing {AbilityCount} abilities, cost {Cost} {Currency}",
            player.Name,
            pointsToRefund,
            abilitiesToRemove.Count,
            cost,
            _config.CurrencyId);

        // Log the respec started event
        _eventLogger.LogCharacter(
            "RespecStarted",
            $"{player.Name} starting respec: {allocationCount} allocations, {pointsToRefund} points, cost {cost} {_config.CurrencyId}",
            player.Id,
            new Dictionary<string, object>
            {
                ["playerLevel"] = player.Level,
                ["allocationsCount"] = allocationCount,
                ["pointsToRefund"] = pointsToRefund,
                ["abilitiesToRemove"] = abilitiesToRemove.Count,
                ["cost"] = cost,
                ["currencyId"] = _config.CurrencyId
            });

        // ═══════════════════════════════════════════════════════════════
        // EXECUTION PHASE
        // ═══════════════════════════════════════════════════════════════

        // Step 1: Refund all spent talent points back to unspent pool
        player.RefundAllTalentPoints();
        _logger.LogTrace(
            "Refunded {Points} talent points to player {PlayerName}. New unspent: {Unspent}",
            pointsToRefund,
            player.Name,
            player.UnspentTalentPoints);

        // Step 2: Clear all talent allocations
        player.ClearTalentAllocations();
        _logger.LogTrace(
            "Cleared all talent allocations for player {PlayerName}",
            player.Name);

        // Step 3: Deduct gold cost from player
        var currencyRemoved = player.RemoveCurrency(_config.CurrencyId, cost);
        if (!currencyRemoved)
        {
            // This should not happen since we validated above, but log error if it does
            _logger.LogError(
                "Failed to remove {Cost} {Currency} from player {PlayerName} during respec - " +
                "validation passed but removal failed",
                cost,
                _config.CurrencyId,
                player.Name);
        }
        else
        {
            _logger.LogTrace(
                "Deducted {Cost} {Currency} from player {PlayerName}. Remaining: {Remaining}",
                cost,
                _config.CurrencyId,
                player.Name,
                player.GetCurrency(_config.CurrencyId));
        }

        // Step 4: Remove abilities granted by talents
        var abilitiesActuallyRemoved = 0;
        foreach (var abilityId in abilitiesToRemove)
        {
            var removed = player.RemoveAbility(abilityId);
            if (removed)
            {
                abilitiesActuallyRemoved++;
                _logger.LogTrace(
                    "Removed ability {AbilityId} from player {PlayerName}",
                    abilityId,
                    player.Name);
            }
            else
            {
                // Not necessarily an error - ability might not have been added yet
                _logger.LogDebug(
                    "Ability {AbilityId} was not present on player {PlayerName} during respec",
                    abilityId,
                    player.Name);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // COMPLETION PHASE
        // ═══════════════════════════════════════════════════════════════

        _logger.LogInformation(
            "Respec completed for player {PlayerName}: " +
            "refunded {Points} points, removed {AbilityCount} abilities, spent {Cost} {Currency}",
            player.Name,
            pointsToRefund,
            abilitiesActuallyRemoved,
            cost,
            _config.CurrencyId);

        // Log the respec completed event
        _eventLogger.LogCharacter(
            "RespecCompleted",
            $"{player.Name} completed respec: {pointsToRefund} points refunded, {abilitiesActuallyRemoved} abilities removed, {cost} {_config.CurrencyId} spent",
            player.Id,
            new Dictionary<string, object>
            {
                ["playerLevel"] = player.Level,
                ["pointsRefunded"] = pointsToRefund,
                ["abilitiesRemoved"] = abilitiesActuallyRemoved,
                ["goldSpent"] = cost,
                ["currencyId"] = _config.CurrencyId,
                ["unspentPointsNow"] = player.UnspentTalentPoints,
                ["goldRemaining"] = player.GetCurrency(_config.CurrencyId)
            });

        return RespecResult.Success(pointsToRefund, cost, abilitiesActuallyRemoved);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs a respec failed event.
    /// </summary>
    /// <param name="player">The player whose respec failed.</param>
    /// <param name="resultType">The type of failure.</param>
    /// <param name="reason">The reason for failure.</param>
    private void LogRespecFailed(Player player, string resultType, string reason)
    {
        _eventLogger.LogCharacter(
            "RespecFailed",
            $"{player.Name} respec failed: {reason}",
            player.Id,
            new Dictionary<string, object>
            {
                ["playerLevel"] = player.Level,
                ["resultType"] = resultType,
                ["failureReason"] = reason
            });
    }
}
