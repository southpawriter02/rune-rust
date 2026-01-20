using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing threatened squares, opportunity attacks, and the reaction system.
/// </summary>
/// <remarks>
/// <para>Provides tactical combat functionality:</para>
/// <list type="bullet">
///   <item><description>Tracks which squares are threatened by melee combatants</description></item>
///   <item><description>Detects when movement triggers opportunity attacks</description></item>
///   <item><description>Manages the reaction economy (one per round)</description></item>
///   <item><description>Handles disengage action for safe movement</description></item>
/// </list>
/// </remarks>
public class ThreatService : IThreatService
{
    private readonly ICombatGridService _gridService;
    private readonly IGameConfigurationProvider _config;
    private readonly ILogger<ThreatService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    // State tracking (in-memory for current combat)
    private readonly HashSet<Guid> _usedReactions = [];
    private readonly HashSet<Guid> _disengaging = [];

    // Player tracking for faction detection
    private Guid? _playerId;

    // Configuration values
    private bool _enabled = true;
    private bool _requiresMeleeWeapon = true;
    private bool _oneReactionPerRound = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreatService"/> class.
    /// </summary>
    public ThreatService(
        ICombatGridService gridService,
        IGameConfigurationProvider config,
        ILogger<ThreatService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        LoadConfiguration();

        _logger.LogDebug("ThreatService initialized: Enabled={Enabled}", _enabled);
    }

    /// <summary>
    /// Registers the player ID for faction detection.
    /// </summary>
    /// <param name="playerId">The player's entity ID.</param>
    public void RegisterPlayer(Guid playerId)
    {
        _playerId = playerId;
        _logger.LogDebug("Registered player {PlayerId} for threat detection", playerId);
    }

    private void LoadConfiguration()
    {
        var oaConfig = _config.GetOpportunityAttackConfiguration();
        if (oaConfig != null)
        {
            _enabled = oaConfig.Enabled;
            _requiresMeleeWeapon = oaConfig.RequiresMeleeWeapon;
            _oneReactionPerRound = oaConfig.OneReactionPerRound;
        }
    }

    // ===== IThreatService: Threatened Square Queries =====

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetThreatenedSquares(Guid entityId)
    {
        if (!_enabled) yield break;

        var grid = _gridService.GetActiveGrid();
        if (grid == null) yield break;

        var position = grid.GetEntityPosition(entityId);
        if (!position.HasValue) yield break;

        // Check if entity has melee capability (for now, all entities threaten)
        // Future: integrate with weapon/equipment system
        var threatensSquares = !_requiresMeleeWeapon || EntityHasMeleeWeapon(entityId);
        if (!threatensSquares) yield break;

        // Threaten all 8 adjacent squares
        foreach (var cell in grid.GetAdjacentCells(position.Value))
        {
            yield return cell.Position;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<Guid> GetThreateningEntities(GridPosition position)
    {
        if (!_enabled) yield break;

        var grid = _gridService.GetActiveGrid();
        if (grid == null) yield break;

        // Check all adjacent cells for entities that could threaten this position
        foreach (var cell in grid.GetAdjacentCells(position))
        {
            if (cell.OccupantId.HasValue)
            {
                var occupantId = cell.OccupantId.Value;

                // Check if entity can threaten
                if (!_requiresMeleeWeapon || EntityHasMeleeWeapon(occupantId))
                {
                    yield return occupantId;
                }
            }
        }
    }

    /// <inheritdoc/>
    public bool IsPositionThreatened(GridPosition position, Guid movingEntityId)
    {
        if (!_enabled) return false;

        var grid = _gridService.GetActiveGrid();
        if (grid == null) return false;

        // Check if moving entity is a player or monster
        var movingIsPlayer = IsPlayer(movingEntityId);

        foreach (var threateningId in GetThreateningEntities(position))
        {
            // Only consider enemies as threats
            var threatenerIsPlayer = IsPlayer(threateningId);
            if (threatenerIsPlayer != movingIsPlayer)
            {
                return true;
            }
        }

        return false;
    }

    // ===== IThreatService: Opportunity Attack Methods =====

    /// <inheritdoc/>
    public OpportunityAttackCheckResult CheckOpportunityAttacks(
        Guid movingEntityId, GridPosition from, GridPosition to)
    {
        if (!_enabled)
        {
            return OpportunityAttackCheckResult.None("Opportunity attacks are disabled.");
        }

        if (IsDisengaging(movingEntityId))
        {
            _logger.LogDebug("Entity {EntityId} is disengaging, skipping OA check", movingEntityId);
            return OpportunityAttackCheckResult.Disengaging();
        }

        var grid = _gridService.GetActiveGrid();
        if (grid == null)
        {
            return OpportunityAttackCheckResult.None("No active grid.");
        }

        var attackers = new List<Guid>();
        var movingIsPlayer = IsPlayer(movingEntityId);

        // Check all entities threatening the FROM position
        foreach (var threateningId in GetThreateningEntities(from))
        {
            // Skip if same faction (ally)
            var threatenerIsPlayer = IsPlayer(threateningId);
            if (threatenerIsPlayer == movingIsPlayer) continue;

            // Skip if already used reaction this round
            if (_oneReactionPerRound && HasUsedReaction(threateningId)) continue;

            // Skip if moving entity will still be adjacent to this threatener
            var threatenerPos = grid.GetEntityPosition(threateningId);
            if (threatenerPos.HasValue && to.DistanceTo(threatenerPos.Value) <= 1)
            {
                continue; // Still adjacent, no OA
            }

            attackers.Add(threateningId);
        }

        if (attackers.Count == 0)
        {
            return OpportunityAttackCheckResult.None();
        }

        // Build warning message
        var attackerNames = attackers
            .Select(id => GetEntityName(id))
            .ToList();

        var message = $"Warning: Moving will trigger opportunity attacks from {string.Join(", ", attackerNames)}!";

        _logger.LogDebug(
            "OA check for {EntityId}: {Count} attackers would trigger",
            movingEntityId, attackers.Count);

        return new OpportunityAttackCheckResult(
            true,
            attackers.AsReadOnly(),
            false,
            message);
    }

    /// <inheritdoc/>
    public IEnumerable<OpportunityAttackResult> ExecuteOpportunityAttacks(
        Guid movingEntityId, GridPosition from, GridPosition to)
    {
        var check = CheckOpportunityAttacks(movingEntityId, from, to);
        if (!check.TriggersOpportunityAttacks)
        {
            yield break;
        }

        foreach (var attackerId in check.AttackingEntities)
        {
            // Mark reaction as used
            UseReaction(attackerId);

            var attackerName = GetEntityName(attackerId);

            // Simple damage calculation (would integrate with CombatService in full implementation)
            // For now, simulate a basic attack roll
            var attackRoll = new Random().Next(1, 21);
            var targetDefense = 10; // Simplified
            var hit = attackRoll >= targetDefense;
            var damage = hit ? new Random().Next(1, 7) + 2 : 0; // 1d6+2

            // Build result message
            var resultMessage = hit
                ? $"{attackerName} strikes as you flee! ({attackRoll} vs AC {targetDefense}) {damage} damage!"
                : $"{attackerName}'s attack misses as you slip away. ({attackRoll} vs AC {targetDefense})";

            _logger.LogInformation(
                "Opportunity attack: {Attacker} vs {Target} - Roll: {Roll}, Hit: {Hit}, Damage: {Damage}",
                attackerName, movingEntityId, attackRoll, hit, damage);

            _eventLogger?.LogCombat(
                hit ? "OpportunityAttackHit" : "OpportunityAttackMiss",
                resultMessage);

            yield return new OpportunityAttackResult(
                attackerId,
                attackerName,
                hit,
                damage,
                $"{attackRoll} vs AC {targetDefense}",
                100 - damage, // Placeholder remaining HP
                false,
                resultMessage);
        }
    }

    // ===== IThreatService: Reaction System =====

    /// <inheritdoc/>
    public bool HasUsedReaction(Guid entityId) => _usedReactions.Contains(entityId);

    /// <inheritdoc/>
    public void UseReaction(Guid entityId)
    {
        _usedReactions.Add(entityId);
        _logger.LogDebug("Entity {EntityId} used their reaction", entityId);
    }

    /// <inheritdoc/>
    public void ResetReactions()
    {
        var count = _usedReactions.Count;
        _usedReactions.Clear();
        _logger.LogInformation("Reset reactions for {Count} entities", count);
    }

    // ===== IThreatService: Disengage System =====

    /// <inheritdoc/>
    public bool IsDisengaging(Guid entityId) => _disengaging.Contains(entityId);

    /// <inheritdoc/>
    public void SetDisengaging(Guid entityId)
    {
        _disengaging.Add(entityId);
        _logger.LogDebug("Entity {EntityId} is now disengaging", entityId);

        _eventLogger?.LogCombat(
            "Disengage",
            "Entity is disengaging from combat");
    }

    /// <inheritdoc/>
    public void ClearDisengaging(Guid entityId)
    {
        _disengaging.Remove(entityId);
        _logger.LogDebug("Cleared disengage status for entity {EntityId}", entityId);
    }

    /// <inheritdoc/>
    public void ClearAllDisengaging()
    {
        var count = _disengaging.Count;
        _disengaging.Clear();
        _logger.LogDebug("Cleared disengage status for {Count} entities", count);
    }

    // ===== Private Helpers =====

    /// <summary>
    /// Checks if an entity is the player.
    /// </summary>
    private bool IsPlayer(Guid entityId) => _playerId.HasValue && _playerId.Value == entityId;

    /// <summary>
    /// Checks if an entity has a melee weapon equipped.
    /// </summary>
    /// <remarks>
    /// Current simplified implementation returns true for all entities.
    /// Full implementation would check equipped weapon via equipment service.
    /// </remarks>
    private static bool EntityHasMeleeWeapon(Guid entityId)
    {
        // TODO: Integrate with equipment system when available
        // For now, assume all entities can make melee attacks
        return true;
    }

    /// <summary>
    /// Gets the display name for an entity.
    /// </summary>
    private string GetEntityName(Guid entityId)
    {
        // Simple implementation - would integrate with actual entity lookup
        return IsPlayer(entityId) ? "Player" : "Enemy";
    }
}
