namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages coordinated monster group behaviors including tactical movement,
/// target selection, and synergy effects (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// MonsterGroupService is the runtime manager for monster group encounters, handling:
/// </para>
/// <list type="bullet">
///   <item><description>Group registration with pre-spawned monsters and role assignment</description></item>
///   <item><description>Tactical movement decisions based on group tactics (Flank, Swarm, etc.)</description></item>
///   <item><description>Coordinated target selection for FocusFire behavior</description></item>
///   <item><description>Synergy effect application and triggering</description></item>
///   <item><description>Member death handling and group cleanup</description></item>
/// </list>
/// <para>
/// The service maintains dictionaries to track active group instances and map
/// monsters to their groups, separating runtime state from static definitions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register a group with pre-spawned monsters
/// var monsters = new[] { shaman, warrior1, warrior2, archer };
/// var instance = groupService.RegisterGroup("goblin-warband", monsters);
///
/// // Get tactical movement
/// var decision = groupService.DetermineMove(warrior1);
/// if (decision.ShouldMove)
///     MoveMonster(warrior1, decision.TargetPosition!.Value);
///
/// // Get coordinated target
/// var target = groupService.DetermineTarget(warrior1, players);
/// </code>
/// </example>
public class MonsterGroupService : IMonsterGroupService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IMonsterGroupProvider _groupProvider;
    private readonly ICombatGridService _gridService;
    private readonly IFlankingService _flankingService;
    private readonly IBuffDebuffService _buffDebuffService;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<MonsterGroupService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Tracks active group instances keyed by group instance ID.
    /// </summary>
    private readonly Dictionary<Guid, MonsterGroupInstance> _activeGroups = new();

    /// <summary>
    /// Maps monster instance IDs to their group instance IDs for fast lookup.
    /// </summary>
    private readonly Dictionary<Guid, Guid> _monsterToGroup = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new MonsterGroupService.
    /// </summary>
    /// <param name="groupProvider">Provider for group definitions.</param>
    /// <param name="gridService">Service for combat grid operations.</param>
    /// <param name="flankingService">Service for flanking calculations.</param>
    /// <param name="buffDebuffService">Service for applying synergy effects.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MonsterGroupService(
        IMonsterGroupProvider groupProvider,
        ICombatGridService gridService,
        IFlankingService flankingService,
        IBuffDebuffService buffDebuffService,
        IGameEventLogger eventLogger,
        ILogger<MonsterGroupService> logger)
    {
        _groupProvider = groupProvider ?? throw new ArgumentNullException(nameof(groupProvider));
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _flankingService = flankingService ?? throw new ArgumentNullException(nameof(flankingService));
        _buffDebuffService = buffDebuffService ?? throw new ArgumentNullException(nameof(buffDebuffService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("MonsterGroupService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // GROUP REGISTRATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public MonsterGroupInstance RegisterGroup(string groupId, IEnumerable<Monster> monsters)
    {
        _logger.LogDebug("RegisterGroup called for groupId={GroupId}", groupId);

        ArgumentNullException.ThrowIfNull(monsters);

        var groupDef = _groupProvider.GetGroup(groupId);
        if (groupDef is null)
        {
            _logger.LogError("Group definition not found: {GroupId}", groupId);
            throw new ArgumentException($"Group definition not found: {groupId}", nameof(groupId));
        }

        var monsterList = monsters.ToList();
        if (monsterList.Count == 0)
        {
            _logger.LogError("No monsters provided for group {GroupId}", groupId);
            throw new ArgumentException("At least one monster must be provided.", nameof(monsters));
        }

        _logger.LogInformation(
            "Registering group {GroupName} ({GroupId}) with {Count} monsters",
            groupDef.Name, groupId, monsterList.Count);

        // Create the group instance
        var instance = new MonsterGroupInstance(groupDef, monsterList);

        // Assign roles based on monster definition IDs matching group member definitions
        AssignRolesFromDefinition(instance, groupDef);

        // Track the group and monster mappings
        _activeGroups[instance.Id] = instance;
        foreach (var monster in monsterList)
        {
            _monsterToGroup[monster.Id] = instance.Id;
            _logger.LogDebug(
                "Mapped monster {MonsterId} ({MonsterName}) to group {GroupInstanceId}",
                monster.Id, monster.Name, instance.Id);
        }

        // Apply Always synergies
        ApplySynergies(instance);

        // Log the registration event
        _eventLogger.LogCombat(
            "MonsterGroupRegistered",
            $"A {groupDef.Name} has appeared!",
            correlationId: instance.Id,
            data: new Dictionary<string, object>
            {
                ["GroupId"] = groupId,
                ["GroupName"] = groupDef.Name,
                ["MemberCount"] = monsterList.Count,
                ["Tactics"] = groupDef.Tactics.Select(t => t.ToString()).ToList()
            });

        _logger.LogInformation(
            "Group {GroupName} registered with instance ID {InstanceId}, {MemberCount} members",
            groupDef.Name, instance.Id, instance.TotalCount);

        return instance;
    }

    /// <summary>
    /// Assigns tactical roles to group members based on their monster definition IDs.
    /// </summary>
    /// <param name="instance">The group instance to assign roles to.</param>
    /// <param name="definition">The group definition with member role mappings.</param>
    private void AssignRolesFromDefinition(MonsterGroupInstance instance, MonsterGroupDefinition definition)
    {
        _logger.LogDebug("Assigning roles for group {GroupId}", definition.GroupId);

        // Build a lookup of monster definition ID -> role from the group definition
        var rolesByDefId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var memberDef in definition.Members)
        {
            if (!string.IsNullOrEmpty(memberDef.Role))
            {
                rolesByDefId[memberDef.MonsterDefinitionId] = memberDef.Role;
            }
        }

        // Assign roles to monsters based on their MonsterDefinitionId
        foreach (var monster in instance.Members)
        {
            if (!string.IsNullOrEmpty(monster.MonsterDefinitionId) &&
                rolesByDefId.TryGetValue(monster.MonsterDefinitionId, out var role))
            {
                instance.SetMemberRole(monster, role);
                _logger.LogDebug(
                    "Assigned role '{Role}' to {MonsterName} (DefId: {DefId})",
                    role, monster.Name, monster.MonsterDefinitionId);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TACTICAL MOVEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public GroupMoveDecision DetermineMove(Monster monster)
    {
        _logger.LogDebug("DetermineMove called for monster {MonsterId}", monster.Id);

        // Check if monster is in a group
        var instance = GetGroupForMonster(monster);
        if (instance is null)
        {
            _logger.LogDebug("Monster {MonsterId} is not in any group", monster.Id);
            return GroupMoveDecision.NoGroup();
        }

        var definition = instance.Definition;
        _logger.LogDebug(
            "Evaluating {TacticCount} tactics for group {GroupId}",
            definition.Tactics.Count, definition.GroupId);

        // Evaluate tactics in priority order
        foreach (var tactic in definition.Tactics)
        {
            var decision = EvaluateTactic(monster, instance, tactic);
            if (decision.IsTacticalDecision)
            {
                _logger.LogDebug(
                    "Tactic {Tactic} produced decision {DecisionType} for monster {MonsterId}",
                    tactic, decision.Type, monster.Id);
                return decision;
            }
        }

        _logger.LogDebug("No tactic produced a move for monster {MonsterId}", monster.Id);
        return GroupMoveDecision.NoAction();
    }

    /// <summary>
    /// Evaluates a specific tactic for a monster.
    /// </summary>
    /// <param name="monster">The monster to evaluate for.</param>
    /// <param name="instance">The monster's group instance.</param>
    /// <param name="tactic">The tactic to evaluate.</param>
    /// <returns>A move decision, or NoAction if tactic doesn't apply.</returns>
    private GroupMoveDecision EvaluateTactic(Monster monster, MonsterGroupInstance instance, GroupTactic tactic)
    {
        _logger.LogDebug(
            "Evaluating tactic {Tactic} for monster {MonsterId}",
            tactic, monster.Id);

        return tactic switch
        {
            GroupTactic.Flank => EvaluateFlankTactic(monster, instance),
            GroupTactic.FocusFire => EvaluateFocusFireTactic(monster, instance),
            GroupTactic.ProtectLeader => EvaluateProtectLeaderTactic(monster, instance),
            GroupTactic.ProtectCaster => EvaluateProtectCasterTactic(monster, instance),
            GroupTactic.Swarm => EvaluateSwarmTactic(monster, instance),
            GroupTactic.Retreat => EvaluateRetreatTactic(monster, instance),
            GroupTactic.Ambush => EvaluateAmbushTactic(monster, instance),
            GroupTactic.HitAndRun => EvaluateHitAndRunTactic(monster, instance),
            _ => GroupMoveDecision.NoAction()
        };
    }

    /// <summary>
    /// Evaluates the Flank tactic to position for flanking bonuses.
    /// </summary>
    private GroupMoveDecision EvaluateFlankTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Need a current target to flank
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("Flank tactic: No current target");
            return GroupMoveDecision.NoAction();
        }

        var targetId = instance.CurrentTarget!.Value;
        var flankingPositions = _flankingService.GetFlankingPositions(targetId).ToList();

        if (flankingPositions.Count == 0)
        {
            _logger.LogDebug("Flank tactic: No flanking positions available");
            return GroupMoveDecision.NoAction();
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (!monsterPosition.HasValue)
        {
            _logger.LogDebug("Flank tactic: Monster {MonsterId} not on grid", monster.Id);
            return GroupMoveDecision.NoAction();
        }

        // Check if already in a flanking position
        if (flankingPositions.Contains(monsterPosition.Value))
        {
            _logger.LogDebug("Flank tactic: Monster already in flanking position");
            return GroupMoveDecision.NoAction();
        }

        // Find the closest available flanking position
        var grid = _gridService.GetActiveGrid();
        if (grid is null)
        {
            return GroupMoveDecision.NoAction();
        }

        var availablePositions = flankingPositions
            .Where(pos => grid.IsInBounds(pos) && grid.GetCell(pos)?.IsPassable == true && grid.GetCell(pos)?.IsOccupied != true)
            .OrderBy(pos => pos.DistanceTo(monsterPosition.Value))
            .ToList();

        if (availablePositions.Count == 0)
        {
            _logger.LogDebug("Flank tactic: No available flanking positions");
            return GroupMoveDecision.NoAction();
        }

        var targetPosition = availablePositions[0];
        _logger.LogDebug(
            "Flank tactic: Moving monster {MonsterId} to flanking position {Position}",
            monster.Id, targetPosition);

        return GroupMoveDecision.MoveTo(targetPosition, GroupTactic.Flank);
    }

    /// <summary>
    /// Evaluates the FocusFire tactic to move toward shared target.
    /// </summary>
    private GroupMoveDecision EvaluateFocusFireTactic(Monster monster, MonsterGroupInstance instance)
    {
        // FocusFire is primarily about target selection, not movement
        // Movement toward target is handled separately
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("FocusFire tactic: No current target");
            return GroupMoveDecision.NoAction();
        }

        var targetPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);
        if (!targetPosition.HasValue)
        {
            _logger.LogDebug("FocusFire tactic: Target not on grid");
            return GroupMoveDecision.NoAction();
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (!monsterPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // If not adjacent to target, move toward it
        var distance = monsterPosition.Value.DistanceTo(targetPosition.Value);
        if (distance <= 1)
        {
            _logger.LogDebug("FocusFire tactic: Already adjacent to target");
            return GroupMoveDecision.NoAction();
        }

        // Find adjacent position closest to target
        var grid = _gridService.GetActiveGrid();
        if (grid is null)
        {
            return GroupMoveDecision.NoAction();
        }

        var adjacentToTarget = GetAdjacentPositions(targetPosition.Value, grid)
            .Where(pos => grid.GetCell(pos)?.IsOccupied != true)
            .OrderBy(pos => pos.DistanceTo(monsterPosition.Value))
            .FirstOrDefault();

        if (adjacentToTarget == default)
        {
            _logger.LogDebug("FocusFire tactic: No available positions adjacent to target");
            return GroupMoveDecision.NoAction();
        }

        _logger.LogDebug(
            "FocusFire tactic: Moving monster {MonsterId} toward target at {Position}",
            monster.Id, adjacentToTarget);

        return GroupMoveDecision.MoveTo(adjacentToTarget, GroupTactic.FocusFire);
    }

    /// <summary>
    /// Evaluates the ProtectLeader tactic to position between leader and threats.
    /// </summary>
    private GroupMoveDecision EvaluateProtectLeaderTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Skip if this monster is the leader
        if (instance.HasRole(monster, "leader"))
        {
            _logger.LogDebug("ProtectLeader tactic: Monster is the leader, skipping");
            return GroupMoveDecision.NoAction();
        }

        var leader = instance.GetLeader();
        if (leader is null)
        {
            _logger.LogDebug("ProtectLeader tactic: No alive leader");
            return GroupMoveDecision.NoAction();
        }

        var leaderPosition = _gridService.GetEntityPosition(leader.Id);
        if (!leaderPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // Find the nearest threat (enemy) to the leader
        // For now, use current target as the threat if available
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("ProtectLeader tactic: No current target/threat");
            return GroupMoveDecision.NoAction();
        }

        var threatPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);
        if (!threatPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // Calculate position between leader and threat
        var midX = (leaderPosition.Value.X + threatPosition.Value.X) / 2;
        var midY = (leaderPosition.Value.Y + threatPosition.Value.Y) / 2;
        var protectPosition = new GridPosition(midX, midY);

        var grid = _gridService.GetActiveGrid();
        if (grid is null || !grid.IsInBounds(protectPosition) || grid.GetCell(protectPosition)?.IsPassable != true || grid.GetCell(protectPosition)?.IsOccupied == true)
        {
            // Try adjacent positions
            var alternatives = GetAdjacentPositions(protectPosition, grid!)
                .Where(pos => grid!.IsInBounds(pos) && grid.GetCell(pos)?.IsPassable == true && grid.GetCell(pos)?.IsOccupied != true)
                .OrderBy(pos => pos.DistanceTo(leaderPosition.Value))
                .ToList();

            if (alternatives.Count == 0)
            {
                _logger.LogDebug("ProtectLeader tactic: No available protection positions");
                return GroupMoveDecision.NoAction();
            }

            protectPosition = alternatives[0];
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (monsterPosition.HasValue && monsterPosition.Value == protectPosition)
        {
            _logger.LogDebug("ProtectLeader tactic: Already in protection position");
            return GroupMoveDecision.NoAction();
        }

        _logger.LogDebug(
            "ProtectLeader tactic: Moving monster {MonsterId} to protect leader at {Position}",
            monster.Id, protectPosition);

        return GroupMoveDecision.MoveTo(protectPosition, GroupTactic.ProtectLeader);
    }

    /// <summary>
    /// Evaluates the ProtectCaster tactic to position between casters and threats.
    /// </summary>
    private GroupMoveDecision EvaluateProtectCasterTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Skip if this monster is a caster
        if (instance.HasRole(monster, "caster"))
        {
            _logger.LogDebug("ProtectCaster tactic: Monster is a caster, skipping");
            return GroupMoveDecision.NoAction();
        }

        var casters = instance.GetAliveMembersByRole("caster");
        if (casters.Count == 0)
        {
            _logger.LogDebug("ProtectCaster tactic: No alive casters");
            return GroupMoveDecision.NoAction();
        }

        // Protect the first caster found
        var caster = casters[0];
        var casterPosition = _gridService.GetEntityPosition(caster.Id);
        if (!casterPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // Similar logic to ProtectLeader
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("ProtectCaster tactic: No current target/threat");
            return GroupMoveDecision.NoAction();
        }

        var threatPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);
        if (!threatPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        var midX = (casterPosition.Value.X + threatPosition.Value.X) / 2;
        var midY = (casterPosition.Value.Y + threatPosition.Value.Y) / 2;
        var protectPosition = new GridPosition(midX, midY);

        var grid = _gridService.GetActiveGrid();
        if (grid is null || !grid.IsInBounds(protectPosition) || grid.GetCell(protectPosition)?.IsPassable != true || grid.GetCell(protectPosition)?.IsOccupied == true)
        {
            var alternatives = GetAdjacentPositions(protectPosition, grid!)
                .Where(pos => grid!.IsInBounds(pos) && grid.GetCell(pos)?.IsPassable == true && grid.GetCell(pos)?.IsOccupied != true)
                .OrderBy(pos => pos.DistanceTo(casterPosition.Value))
                .ToList();

            if (alternatives.Count == 0)
            {
                _logger.LogDebug("ProtectCaster tactic: No available protection positions");
                return GroupMoveDecision.NoAction();
            }

            protectPosition = alternatives[0];
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (monsterPosition.HasValue && monsterPosition.Value == protectPosition)
        {
            _logger.LogDebug("ProtectCaster tactic: Already in protection position");
            return GroupMoveDecision.NoAction();
        }

        _logger.LogDebug(
            "ProtectCaster tactic: Moving monster {MonsterId} to protect caster at {Position}",
            monster.Id, protectPosition);

        return GroupMoveDecision.MoveTo(protectPosition, GroupTactic.ProtectCaster);
    }

    /// <summary>
    /// Evaluates the Swarm tactic to cluster around target.
    /// </summary>
    private GroupMoveDecision EvaluateSwarmTactic(Monster monster, MonsterGroupInstance instance)
    {
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("Swarm tactic: No current target");
            return GroupMoveDecision.NoAction();
        }

        var targetPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);
        if (!targetPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (!monsterPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // Already adjacent?
        if (monsterPosition.Value.DistanceTo(targetPosition.Value) <= 1)
        {
            _logger.LogDebug("Swarm tactic: Already adjacent to target");
            return GroupMoveDecision.NoAction();
        }

        var grid = _gridService.GetActiveGrid();
        if (grid is null)
        {
            return GroupMoveDecision.NoAction();
        }

        // Find any adjacent position to the target
        var adjacentPositions = GetAdjacentPositions(targetPosition.Value, grid)
            .Where(pos => grid.GetCell(pos)?.IsOccupied != true)
            .OrderBy(pos => pos.DistanceTo(monsterPosition.Value))
            .ToList();

        if (adjacentPositions.Count == 0)
        {
            _logger.LogDebug("Swarm tactic: No available adjacent positions");
            return GroupMoveDecision.NoAction();
        }

        var swarmPosition = adjacentPositions[0];
        _logger.LogDebug(
            "Swarm tactic: Moving monster {MonsterId} to swarm position {Position}",
            monster.Id, swarmPosition);

        return GroupMoveDecision.MoveTo(swarmPosition, GroupTactic.Swarm);
    }

    /// <summary>
    /// Evaluates the Retreat tactic for low health monsters.
    /// </summary>
    private GroupMoveDecision EvaluateRetreatTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Retreat when health is low (below 30%)
        var healthPercent = (monster.Health * 100) / monster.MaxHealth;
        if (healthPercent >= 30)
        {
            _logger.LogDebug("Retreat tactic: Monster health {Percent}% is not low enough", healthPercent);
            return GroupMoveDecision.NoAction();
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (!monsterPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        if (!instance.HasCurrentTarget)
        {
            return GroupMoveDecision.NoAction();
        }

        var threatPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);
        if (!threatPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        // Move away from threat
        var awayX = monsterPosition.Value.X + (monsterPosition.Value.X - threatPosition.Value.X);
        var awayY = monsterPosition.Value.Y + (monsterPosition.Value.Y - threatPosition.Value.Y);

        var grid = _gridService.GetActiveGrid();
        if (grid is null)
        {
            return GroupMoveDecision.NoAction();
        }

        var retreatPosition = new GridPosition(
            Math.Clamp(awayX, 0, grid.Width - 1),
            Math.Clamp(awayY, 0, grid.Height - 1));

        if (!grid.IsInBounds(retreatPosition) || grid.GetCell(retreatPosition)?.IsPassable != true || grid.GetCell(retreatPosition)?.IsOccupied == true)
        {
            var alternatives = GetAdjacentPositions(retreatPosition, grid)
                .Where(pos => grid.IsInBounds(pos) && grid.GetCell(pos)?.IsPassable == true && grid.GetCell(pos)?.IsOccupied != true)
                .OrderByDescending(pos => pos.DistanceTo(threatPosition.Value))
                .ToList();

            if (alternatives.Count == 0)
            {
                _logger.LogDebug("Retreat tactic: No available retreat positions");
                return GroupMoveDecision.NoAction();
            }

            retreatPosition = alternatives[0];
        }

        _logger.LogDebug(
            "Retreat tactic: Monster {MonsterId} retreating to {Position}",
            monster.Id, retreatPosition);

        return GroupMoveDecision.MoveTo(retreatPosition, GroupTactic.Retreat);
    }

    /// <summary>
    /// Evaluates the Ambush tactic to hold position until target in range.
    /// </summary>
    private GroupMoveDecision EvaluateAmbushTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Ambush means hold position until targets enter trigger range
        if (!instance.HasCurrentTarget)
        {
            _logger.LogDebug("Ambush tactic: Holding position, no target yet");
            return GroupMoveDecision.HoldPosition(GroupTactic.Ambush);
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        var targetPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);

        if (!monsterPosition.HasValue || !targetPosition.HasValue)
        {
            return GroupMoveDecision.HoldPosition(GroupTactic.Ambush);
        }

        // Trigger ambush when target is within 3 cells
        var distance = monsterPosition.Value.DistanceTo(targetPosition.Value);
        if (distance > 3)
        {
            _logger.LogDebug("Ambush tactic: Holding position, target at distance {Distance}", distance);
            return GroupMoveDecision.HoldPosition(GroupTactic.Ambush);
        }

        // Target in range, ambush triggered - move to attack
        _logger.LogDebug("Ambush tactic: Target in range, springing ambush!");
        return GroupMoveDecision.NoAction(); // Let other tactics handle attack movement
    }

    /// <summary>
    /// Evaluates the HitAndRun tactic for attack and disengage.
    /// </summary>
    private GroupMoveDecision EvaluateHitAndRunTactic(Monster monster, MonsterGroupInstance instance)
    {
        // Hit and run: if adjacent to target, move away after attacking
        // This would need to know if monster already attacked this turn
        // For now, implement basic disengage behavior

        if (!instance.HasCurrentTarget)
        {
            return GroupMoveDecision.NoAction();
        }

        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        var targetPosition = _gridService.GetEntityPosition(instance.CurrentTarget!.Value);

        if (!monsterPosition.HasValue || !targetPosition.HasValue)
        {
            return GroupMoveDecision.NoAction();
        }

        var distance = monsterPosition.Value.DistanceTo(targetPosition.Value);

        // If already adjacent (just attacked), disengage
        if (distance <= 1)
        {
            var grid = _gridService.GetActiveGrid();
            if (grid is null)
            {
                return GroupMoveDecision.NoAction();
            }

            // Find a position 2-3 cells away
            var disengagePositions = Enumerable.Range(0, grid.Width)
                .SelectMany(x => Enumerable.Range(0, grid.Height).Select(y => new GridPosition(x, y)))
                .Where(pos => grid.IsInBounds(pos) && grid.GetCell(pos)?.IsPassable == true && grid.GetCell(pos)?.IsOccupied != true)
                .Where(pos => pos.DistanceTo(targetPosition.Value) >= 2 && pos.DistanceTo(targetPosition.Value) <= 3)
                .OrderBy(pos => pos.DistanceTo(monsterPosition.Value))
                .ToList();

            if (disengagePositions.Count == 0)
            {
                _logger.LogDebug("HitAndRun tactic: No disengage positions available");
                return GroupMoveDecision.NoAction();
            }

            var disengagePos = disengagePositions[0];
            _logger.LogDebug(
                "HitAndRun tactic: Monster {MonsterId} disengaging to {Position}",
                monster.Id, disengagePos);

            return GroupMoveDecision.MoveTo(disengagePos, GroupTactic.HitAndRun);
        }

        // Not adjacent, move toward target to attack
        return GroupMoveDecision.NoAction(); // Let other tactics handle approach
    }

    /// <summary>
    /// Gets adjacent positions around a center point.
    /// </summary>
    private static IEnumerable<GridPosition> GetAdjacentPositions(GridPosition center, CombatGrid grid)
    {
        var offsets = new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };

        foreach (var (dx, dy) in offsets)
        {
            var pos = new GridPosition(center.X + dx, center.Y + dy);
            if (pos.X >= 0 && pos.X < grid.Width && pos.Y >= 0 && pos.Y < grid.Height)
            {
                yield return pos;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TARGET SELECTION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Combatant? DetermineTarget(Monster monster, IEnumerable<Combatant> possibleTargets)
    {
        _logger.LogDebug("DetermineTarget called for monster {MonsterId}", monster.Id);

        var instance = GetGroupForMonster(monster);
        if (instance is null)
        {
            _logger.LogDebug("Monster {MonsterId} is not in any group", monster.Id);
            return null;
        }

        var targets = possibleTargets.ToList();
        if (targets.Count == 0)
        {
            _logger.LogDebug("No possible targets available");
            return null;
        }

        // Check if group uses FocusFire tactic
        if (!instance.Definition.HasTactic(GroupTactic.FocusFire))
        {
            _logger.LogDebug("Group does not have FocusFire tactic");
            return null;
        }

        // If we have a valid current target, use it
        if (instance.HasCurrentTarget)
        {
            var currentTarget = targets.FirstOrDefault(t => t.Id == instance.CurrentTarget!.Value);
            if (currentTarget != null && currentTarget.CurrentHealth > 0)
            {
                _logger.LogDebug(
                    "Using existing FocusFire target: {TargetId}",
                    instance.CurrentTarget);
                return currentTarget;
            }
            else
            {
                _logger.LogDebug("Current target invalid or dead, selecting new target");
                instance.ClearCurrentTarget();
            }
        }

        // Select new target: lowest HP
        var newTarget = targets
            .Where(t => t.CurrentHealth > 0)
            .OrderBy(t => t.CurrentHealth)
            .FirstOrDefault();

        if (newTarget != null)
        {
            instance.SetCurrentTarget(newTarget.Id);
            _logger.LogInformation(
                "FocusFire: Group {GroupId} targeting {TargetName} (HP: {HP})",
                instance.GroupId, newTarget.DisplayName, newTarget.CurrentHealth);

            _eventLogger.LogCombat(
                "GroupFocusTarget",
                $"{instance.Definition.Name} focuses their attacks on {newTarget.DisplayName}!",
                correlationId: instance.Id,
                data: new Dictionary<string, object>
                {
                    ["GroupId"] = instance.GroupId,
                    ["TargetId"] = newTarget.Id,
                    ["TargetName"] = newTarget.DisplayName
                });
        }

        return newTarget;
    }

    // ═══════════════════════════════════════════════════════════════
    // SYNERGY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void ApplySynergies(MonsterGroupInstance group)
    {
        _logger.LogDebug("ApplySynergies called for group {GroupId}", group.GroupId);

        var alwaysSynergies = group.Definition.GetSynergiesByTrigger(SynergyTrigger.Always).ToList();
        _logger.LogDebug("Found {Count} Always synergies", alwaysSynergies.Count);

        foreach (var synergy in alwaysSynergies)
        {
            ApplySynergyToGroup(group, synergy);
        }
    }

    /// <summary>
    /// Applies a synergy effect to group members.
    /// </summary>
    private void ApplySynergyToGroup(MonsterGroupInstance group, GroupSynergy synergy)
    {
        _logger.LogDebug(
            "Applying synergy {SynergyId} to group {GroupId}",
            synergy.SynergyId, group.GroupId);

        // Check source role requirement
        if (synergy.RequiresSourceRole)
        {
            var sourceMembers = group.GetAliveMembersByRole(synergy.SourceRole!);
            if (sourceMembers.Count == 0)
            {
                _logger.LogDebug(
                    "Synergy {SynergyId} requires source role '{Role}' but no alive members have it",
                    synergy.SynergyId, synergy.SourceRole);
                return;
            }
        }

        // Apply status effect if specified
        if (synergy.HasStatusEffect)
        {
            var targets = synergy.AppliesToAllMembers
                ? group.AliveMembers
                : group.AliveMembers.Where(m =>
                    !synergy.RequiresSourceRole ||
                    !group.HasRole(m, synergy.SourceRole!)).ToList();

            foreach (var member in targets)
            {
                // Check if monster implements IEffectTarget
                if (member is IEffectTarget effectTarget)
                {
                    var result = _buffDebuffService.ApplyEffect(
                        effectTarget,
                        synergy.StatusEffectId!,
                        sourceId: null,
                        sourceName: synergy.Name);

                    _logger.LogDebug(
                        "Applied status effect {EffectId} to {MonsterName}: {Result}",
                        synergy.StatusEffectId, member.Name, result.WasApplied);
                }
            }
        }

        // Log the synergy application
        if (synergy.HasCombatBonuses)
        {
            var bonusText = new List<string>();
            if (synergy.HasAttackBonus) bonusText.Add($"+{synergy.AttackBonus} attack");
            if (synergy.HasDamageBonus) bonusText.Add($"+{synergy.DamageBonus} damage");

            _logger.LogInformation(
                "Synergy {SynergyName} active for group {GroupId}: {Bonuses}",
                synergy.Name, group.GroupId, string.Join(", ", bonusText));
        }
    }

    /// <inheritdoc />
    public void OnGroupMemberHit(Monster attacker, Combatant target)
    {
        _logger.LogDebug(
            "OnGroupMemberHit: {AttackerId} hit {TargetId}",
            attacker.Id, target.Id);

        var instance = GetGroupForMonster(attacker);
        if (instance is null)
        {
            return;
        }

        var onHitSynergies = instance.Definition.GetSynergiesByTrigger(SynergyTrigger.OnAllyHit);
        foreach (var synergy in onHitSynergies)
        {
            _logger.LogDebug(
                "Triggering OnAllyHit synergy {SynergyId}",
                synergy.SynergyId);
            ApplySynergyToGroup(instance, synergy);
        }
    }

    /// <inheritdoc />
    public void OnGroupMemberDamaged(Monster target, int damage)
    {
        _logger.LogDebug(
            "OnGroupMemberDamaged: {TargetId} took {Damage} damage",
            target.Id, damage);

        var instance = GetGroupForMonster(target);
        if (instance is null)
        {
            return;
        }

        var onDamagedSynergies = instance.Definition.GetSynergiesByTrigger(SynergyTrigger.OnAllyDamaged);
        foreach (var synergy in onDamagedSynergies)
        {
            _logger.LogDebug(
                "Triggering OnAllyDamaged synergy {SynergyId}",
                synergy.SynergyId);
            ApplySynergyToGroup(instance, synergy);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // MEMBER DEATH HANDLING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void OnGroupMemberDeath(Monster monster)
    {
        _logger.LogDebug("OnGroupMemberDeath called for monster {MonsterId}", monster.Id);

        if (!_monsterToGroup.TryGetValue(monster.Id, out var groupInstanceId))
        {
            _logger.LogDebug("Monster {MonsterId} was not in any group", monster.Id);
            return;
        }

        if (!_activeGroups.TryGetValue(groupInstanceId, out var instance))
        {
            _logger.LogWarning(
                "Group instance {InstanceId} not found for monster {MonsterId}",
                groupInstanceId, monster.Id);
            return;
        }

        _logger.LogInformation(
            "Group member {MonsterName} died in group {GroupName}",
            monster.Name, instance.Definition.Name);

        // Update the instance
        instance.OnMemberDeath(monster);

        // Remove monster from tracking
        _monsterToGroup.Remove(monster.Id);

        // Log the death event
        _eventLogger.LogCombat(
            "GroupMemberDeath",
            $"{monster.Name} has fallen! ({instance.AliveCount} remaining in {instance.Definition.Name})",
            correlationId: instance.Id,
            data: new Dictionary<string, object>
            {
                ["GroupId"] = instance.GroupId,
                ["MonsterId"] = monster.Id,
                ["MonsterName"] = monster.Name,
                ["AliveCount"] = instance.AliveCount
            });

        // Check if group is empty
        if (!instance.HasAliveMembers)
        {
            _logger.LogInformation(
                "Group {GroupName} has been defeated!",
                instance.Definition.Name);

            _eventLogger.LogCombat(
                "GroupDefeated",
                $"The {instance.Definition.Name} has been defeated!",
                correlationId: instance.Id,
                data: new Dictionary<string, object>
                {
                    ["GroupId"] = instance.GroupId,
                    ["GroupName"] = instance.Definition.Name
                });

            // Remove the group from tracking
            _activeGroups.Remove(groupInstanceId);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITIONING HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<GridPosition> GetFlankingPositions(Combatant target, int groupSize)
    {
        _logger.LogDebug(
            "GetFlankingPositions for target {TargetId}, groupSize={GroupSize}",
            target.Id, groupSize);

        var positions = _flankingService.GetFlankingPositions(target.Id).ToList();

        _logger.LogDebug("Found {Count} flanking positions", positions.Count);
        return positions.Take(groupSize).ToList();
    }

    /// <inheritdoc />
    public int GetAdjacentAllyBonus(Monster monster, string bonusType)
    {
        _logger.LogDebug(
            "GetAdjacentAllyBonus for monster {MonsterId}, bonusType={BonusType}",
            monster.Id, bonusType);

        var instance = GetGroupForMonster(monster);
        if (instance is null)
        {
            return 0;
        }

        // Check for PerAdjacentAlly synergies
        var adjacencySynergies = instance.Definition.GetSynergiesByTrigger(SynergyTrigger.PerAdjacentAlly).ToList();
        if (adjacencySynergies.Count == 0)
        {
            return 0;
        }

        // Count adjacent allies
        var adjacentCount = 0;
        var monsterPosition = _gridService.GetEntityPosition(monster.Id);
        if (!monsterPosition.HasValue)
        {
            return 0;
        }

        foreach (var ally in instance.AliveMembers)
        {
            if (ally.Id == monster.Id) continue;

            var allyPosition = _gridService.GetEntityPosition(ally.Id);
            if (allyPosition.HasValue && monsterPosition.Value.DistanceTo(allyPosition.Value) <= 1)
            {
                adjacentCount++;
            }
        }

        if (adjacentCount == 0)
        {
            return 0;
        }

        // Sum bonuses from all adjacency synergies
        var totalBonus = 0;
        foreach (var synergy in adjacencySynergies)
        {
            var bonus = bonusType.Equals("attack", StringComparison.OrdinalIgnoreCase)
                ? synergy.AttackBonus
                : synergy.DamageBonus;
            totalBonus += bonus * adjacentCount;
        }

        _logger.LogDebug(
            "Adjacent ally bonus: {Count} allies, {Bonus} total {BonusType} bonus",
            adjacentCount, totalBonus, bonusType);

        return totalBonus;
    }

    // ═══════════════════════════════════════════════════════════════
    // GROUP QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsInGroup(Monster monster)
    {
        var isInGroup = _monsterToGroup.ContainsKey(monster.Id);
        _logger.LogDebug("IsInGroup: Monster {MonsterId} inGroup={InGroup}", monster.Id, isInGroup);
        return isInGroup;
    }

    /// <inheritdoc />
    public MonsterGroupInstance? GetGroupForMonster(Monster monster)
    {
        if (!_monsterToGroup.TryGetValue(monster.Id, out var groupInstanceId))
        {
            _logger.LogDebug("Monster {MonsterId} is not in any group", monster.Id);
            return null;
        }

        var instance = _activeGroups.GetValueOrDefault(groupInstanceId);
        if (instance is null)
        {
            _logger.LogWarning(
                "Group instance {InstanceId} not found for monster {MonsterId}",
                groupInstanceId, monster.Id);
        }

        return instance;
    }

    /// <inheritdoc />
    public IReadOnlyList<MonsterGroupInstance> GetActiveGroups()
    {
        var groups = _activeGroups.Values
            .Where(g => g.HasAliveMembers)
            .ToList();

        _logger.LogDebug("GetActiveGroups: {Count} active groups", groups.Count);
        return groups;
    }
}
