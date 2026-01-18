using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing environmental combat mechanics including push, knockback, and hazard interactions.
/// </summary>
/// <remarks>
/// <para>EnvironmentalCombatService coordinates all push/knockback operations and hazard damage
/// during tactical combat on the grid.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item><description>Handle push operations with opposed STR (Might) checks</description></item>
///   <item><description>Handle knockback operations (forced movement without opposition)</description></item>
///   <item><description>Detect and process environmental hazards</description></item>
///   <item><description>Apply hazard damage and status effects</description></item>
///   <item><description>Process per-turn hazard tick damage</description></item>
///   <item><description>Handle critical hit knockback triggers</description></item>
/// </list>
/// <para>Push Mechanics:</para>
/// <list type="bullet">
///   <item><description>Opposed Might check: 1d20 + pusher Might mod vs 1d20 + target Might mod</description></item>
///   <item><description>Target wins ties (resists the push)</description></item>
///   <item><description>Movement stops at walls, other entities, or hazards</description></item>
/// </list>
/// <para>Knockback Mechanics:</para>
/// <list type="bullet">
///   <item><description>Forced movement with no opposed check</description></item>
///   <item><description>Direction is away from the source position</description></item>
///   <item><description>Critical hits trigger 1-cell knockback</description></item>
/// </list>
/// </remarks>
public class EnvironmentalCombatService : IEnvironmentalCombatService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IEnvironmentalHazardProvider _hazardProvider;
    private readonly IDiceService _diceService;
    private readonly IBuffDebuffService _buffDebuffService;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<EnvironmentalCombatService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new EnvironmentalCombatService instance.
    /// </summary>
    /// <param name="hazardProvider">Provider for hazard definitions.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="buffDebuffService">Service for applying status effects.</param>
    /// <param name="eventLogger">Logger for combat events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EnvironmentalCombatService(
        IEnvironmentalHazardProvider hazardProvider,
        IDiceService diceService,
        IBuffDebuffService buffDebuffService,
        IGameEventLogger eventLogger,
        ILogger<EnvironmentalCombatService> logger)
    {
        _hazardProvider = hazardProvider ?? throw new ArgumentNullException(nameof(hazardProvider));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _buffDebuffService = buffDebuffService ?? throw new ArgumentNullException(nameof(buffDebuffService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "EnvironmentalCombatService initialized with {HazardCount} configured hazards",
            _hazardProvider.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // PUSH OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public PushResult Push(
        Combatant pusher,
        Combatant target,
        CombatGrid grid,
        MovementDirection direction,
        int distance = 1)
    {
        ArgumentNullException.ThrowIfNull(pusher);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(grid);

        _logger.LogDebug(
            "Push attempt: {Pusher} pushing {Target} {Direction} for {Distance} cell(s)",
            pusher.DisplayName,
            target.DisplayName,
            direction,
            distance);

        // Get target's current position
        var targetPosition = grid.GetEntityPosition(target.Id);
        if (!targetPosition.HasValue)
        {
            _logger.LogWarning(
                "Push failed: {Target} not found on grid",
                target.DisplayName);
            return PushResult.NoPush(new GridPosition(0, 0), "Target not on grid");
        }

        var startPosition = targetPosition.Value;

        // Perform opposed Might check
        var pusherMod = GetStrengthModifier(pusher);
        var targetMod = GetStrengthModifier(target);

        var pusherRoll = _diceService.RollTotal("1d20") + pusherMod;
        var targetRoll = _diceService.RollTotal("1d20") + targetMod;

        _logger.LogDebug(
            "Opposed Might check: Pusher {PusherRoll} (1d20+{PusherMod}) vs Target {TargetRoll} (1d20+{TargetMod})",
            pusherRoll,
            pusherMod,
            targetRoll,
            targetMod);

        // Target wins ties (resists the push)
        if (targetRoll >= pusherRoll)
        {
            _logger.LogInformation(
                "{Target} resisted push from {Pusher} (Target: {TargetRoll} vs Pusher: {PusherRoll})",
                target.DisplayName,
                pusher.DisplayName,
                targetRoll,
                pusherRoll);

            _eventLogger.LogCombat(
                "PushResisted",
                $"{target.DisplayName} resisted push from {pusher.DisplayName}",
                data: new Dictionary<string, object>
                {
                    ["pusherId"] = pusher.Id,
                    ["pusherName"] = pusher.DisplayName,
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["pusherRoll"] = pusherRoll,
                    ["targetRoll"] = targetRoll
                });

            return PushResult.Resisted(startPosition, direction, distance, pusherRoll, targetRoll);
        }

        // Push succeeded - attempt to move the target
        return ExecutePushMovement(
            target,
            grid,
            startPosition,
            direction,
            distance,
            pusherRoll,
            targetRoll,
            pusher.DisplayName);
    }

    /// <inheritdoc />
    public PushResult Knockback(
        Combatant target,
        CombatGrid grid,
        GridPosition sourcePosition,
        int distance = 1)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(grid);

        _logger.LogDebug(
            "Knockback: {Target} knocked back from source {Source} for {Distance} cell(s)",
            target.DisplayName,
            sourcePosition,
            distance);

        // Get target's current position
        var targetPosition = grid.GetEntityPosition(target.Id);
        if (!targetPosition.HasValue)
        {
            _logger.LogWarning(
                "Knockback failed: {Target} not found on grid",
                target.DisplayName);
            return PushResult.NoPush(new GridPosition(0, 0), "Target not on grid");
        }

        var startPosition = targetPosition.Value;
        var direction = GetDirectionAway(startPosition, sourcePosition);

        _logger.LogDebug(
            "Knockback direction calculated: {Direction} (away from {Source})",
            direction,
            sourcePosition);

        // Execute the knockback movement (no opposed check)
        return ExecuteKnockbackMovement(target, grid, startPosition, direction, distance);
    }

    // ═══════════════════════════════════════════════════════════════
    // HAZARD DETECTION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsHazard(CombatGrid grid, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(grid);

        var cell = grid.GetCell(position);
        if (cell == null)
        {
            return false;
        }

        var isHazard = cell.TerrainType == TerrainType.Hazardous;

        _logger.LogDebug(
            "Hazard check at {Position}: {IsHazard} (TerrainType: {TerrainType})",
            position,
            isHazard,
            cell.TerrainType);

        return isHazard;
    }

    /// <inheritdoc />
    public HazardType? GetHazardType(CombatGrid grid, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(grid);

        var cell = grid.GetCell(position);
        if (cell == null || cell.TerrainType != TerrainType.Hazardous)
        {
            return null;
        }

        // Parse hazard type from TerrainDefinitionId (format: "hazard:lava", "hazard:spikes", etc.)
        var definitionId = cell.TerrainDefinitionId;
        if (string.IsNullOrEmpty(definitionId))
        {
            _logger.LogWarning(
                "Hazardous cell at {Position} has no TerrainDefinitionId",
                position);
            return null;
        }

        // Extract hazard type from definition ID
        var hazardTypeName = definitionId.StartsWith("hazard:", StringComparison.OrdinalIgnoreCase)
            ? definitionId.Substring(7)
            : definitionId;

        if (Enum.TryParse<HazardType>(hazardTypeName, ignoreCase: true, out var hazardType))
        {
            _logger.LogDebug(
                "Hazard type at {Position}: {HazardType} (from definition '{DefinitionId}')",
                position,
                hazardType,
                definitionId);
            return hazardType;
        }

        _logger.LogWarning(
            "Unknown hazard type in definition '{DefinitionId}' at {Position}",
            definitionId,
            position);
        return null;
    }

    // ═══════════════════════════════════════════════════════════════
    // HAZARD DAMAGE
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public HazardDamageResult ApplyHazardDamage(
        Combatant target,
        CombatGrid grid,
        GridPosition position,
        bool isEntryDamage = true)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(grid);

        var hazardType = GetHazardType(grid, position);
        if (!hazardType.HasValue)
        {
            _logger.LogDebug(
                "No hazard damage applied at {Position} - not a hazard cell",
                position);
            return HazardDamageResult.NoDamage(
                HazardType.Fire,
                "None",
                position,
                target.CurrentHealth);
        }

        var hazardDef = _hazardProvider.GetHazard(hazardType.Value);
        if (hazardDef == null)
        {
            _logger.LogWarning(
                "Hazard definition not found for type {HazardType}",
                hazardType.Value);
            return HazardDamageResult.NoDamage(
                hazardType.Value,
                hazardType.Value.ToString(),
                position,
                target.CurrentHealth);
        }

        // Check if this damage type applies
        if (isEntryDamage && !hazardDef.DamageOnEnter)
        {
            _logger.LogDebug(
                "Hazard {HazardName} does not deal entry damage",
                hazardDef.Name);
            return HazardDamageResult.NoDamage(
                hazardType.Value,
                hazardDef.Name,
                position,
                target.CurrentHealth);
        }

        if (!isEntryDamage && !hazardDef.DamagePerTurn)
        {
            _logger.LogDebug(
                "Hazard {HazardName} does not deal per-turn damage",
                hazardDef.Name);
            return HazardDamageResult.NoDamage(
                hazardType.Value,
                hazardDef.Name,
                position,
                target.CurrentHealth);
        }

        // Roll damage
        var damageRoll = _diceService.RollTotal(hazardDef.DamageDice);

        _logger.LogDebug(
            "Hazard damage roll: {Dice} = {Damage} {DamageType}",
            hazardDef.DamageDice,
            damageRoll,
            hazardDef.DamageType);

        // Apply damage to the target's underlying entity (Player or Monster)
        var actualDamage = ApplyDamageToCombatant(target, damageRoll);
        var remainingHealth = target.CurrentHealth;

        // Get the effect target for potential status effect application
        var effectTarget = GetEffectTarget(target);

        _logger.LogInformation(
            "{Target} took {Damage} {DamageType} damage from {HazardName} at {Position} ({EntryType}). HP: {Remaining}",
            target.DisplayName,
            actualDamage,
            hazardDef.DamageType,
            hazardDef.Name,
            position,
            isEntryDamage ? "entry" : "tick",
            remainingHealth);

        // Log combat event
        _eventLogger.LogCombat(
            "HazardDamage",
            $"{target.DisplayName} took {actualDamage} {hazardDef.DamageType} damage from {hazardDef.Name}",
            data: new Dictionary<string, object>
            {
                ["targetId"] = target.Id,
                ["targetName"] = target.DisplayName,
                ["hazardType"] = hazardType.Value.ToString(),
                ["hazardName"] = hazardDef.Name,
                ["damage"] = actualDamage,
                ["damageType"] = hazardDef.DamageType,
                ["damageDice"] = hazardDef.DamageDice,
                ["position"] = position.ToString(),
                ["isEntryDamage"] = isEntryDamage,
                ["remainingHealth"] = remainingHealth
            });

        // Apply status effect if configured
        string? appliedStatusEffect = null;
        if (hazardDef.AppliesStatusEffect() && effectTarget != null)
        {
            appliedStatusEffect = ApplyHazardStatusEffect(
                target,
                effectTarget,
                hazardDef,
                position);
        }

        // Check for trap hazards that require climbing
        var isTrapped = hazardDef.RequiresClimbOut;
        if (isTrapped)
        {
            _logger.LogInformation(
                "{Target} is trapped in {HazardName} and must climb out",
                target.DisplayName,
                hazardDef.Name);

            _eventLogger.LogCombat(
                "CombatantTrapped",
                $"{target.DisplayName} fell into {hazardDef.Name} and is trapped",
                data: new Dictionary<string, object>
                {
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["hazardType"] = hazardType.Value.ToString(),
                    ["hazardName"] = hazardDef.Name,
                    ["position"] = position.ToString()
                });
        }

        // Check if target was killed
        if (remainingHealth <= 0)
        {
            _logger.LogInformation(
                "{Target} was killed by {HazardName}!",
                target.DisplayName,
                hazardDef.Name);

            _eventLogger.LogCombat(
                "CombatantKilledByHazard",
                $"{target.DisplayName} was killed by {hazardDef.Name}",
                data: new Dictionary<string, object>
                {
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["hazardType"] = hazardType.Value.ToString(),
                    ["hazardName"] = hazardDef.Name,
                    ["finalDamage"] = actualDamage,
                    ["position"] = position.ToString()
                });
        }

        return isEntryDamage
            ? HazardDamageResult.EntryDamage(
                hazardType.Value,
                hazardDef.Name,
                position,
                actualDamage,
                hazardDef.DamageType,
                hazardDef.DamageDice,
                remainingHealth,
                appliedStatusEffect,
                isTrapped)
            : HazardDamageResult.TickDamage(
                hazardType.Value,
                hazardDef.Name,
                position,
                actualDamage,
                hazardDef.DamageType,
                hazardDef.DamageDice,
                remainingHealth,
                hazardDef.DegradesArmor);
    }

    /// <inheritdoc />
    public IReadOnlyList<HazardDamageResult> TickHazards(
        CombatGrid grid,
        IEnumerable<Combatant> combatants)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(combatants);

        _logger.LogDebug("Processing hazard tick damage for all combatants");

        var results = new List<HazardDamageResult>();
        var totalDamage = 0;

        foreach (var combatant in combatants)
        {
            if (!combatant.IsActive)
            {
                continue;
            }

            var position = grid.GetEntityPosition(combatant.Id);
            if (!position.HasValue)
            {
                continue;
            }

            if (IsHazard(grid, position.Value))
            {
                var result = ApplyHazardDamage(combatant, grid, position.Value, isEntryDamage: false);
                if (result.DamageDealt > 0)
                {
                    results.Add(result);
                    totalDamage += result.DamageDealt;
                }
            }
        }

        _logger.LogInformation(
            "Hazard tick processed: {Count} combatants affected, {TotalDamage} total damage",
            results.Count,
            totalDamage);

        _eventLogger.LogCombat(
            "HazardTickProcessed",
            $"Hazard tick: {results.Count} combatants took {totalDamage} total damage",
            data: new Dictionary<string, object>
            {
                ["combatantsAffected"] = results.Count,
                ["totalDamage"] = totalDamage
            });

        return results.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════
    // CRITICAL KNOCKBACK
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public PushResult? ProcessCriticalKnockback(
        Combatant attacker,
        Combatant target,
        CombatGrid grid,
        bool isCritical)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(grid);

        if (!isCritical)
        {
            _logger.LogDebug(
                "No critical knockback - attack was not a critical hit");
            return null;
        }

        var attackerPosition = grid.GetEntityPosition(attacker.Id);
        if (!attackerPosition.HasValue)
        {
            _logger.LogWarning(
                "Critical knockback failed: {Attacker} not found on grid",
                attacker.DisplayName);
            return null;
        }

        _logger.LogInformation(
            "Critical hit! {Attacker} triggers 1-cell knockback on {Target}",
            attacker.DisplayName,
            target.DisplayName);

        _eventLogger.LogCombat(
            "CriticalKnockbackTriggered",
            $"{attacker.DisplayName} scored a critical hit - {target.DisplayName} is knocked back!",
            data: new Dictionary<string, object>
            {
                ["attackerId"] = attacker.Id,
                ["attackerName"] = attacker.DisplayName,
                ["targetId"] = target.Id,
                ["targetName"] = target.DisplayName
            });

        // Critical knockback is 1 cell away from the attacker
        return Knockback(target, grid, attackerPosition.Value, distance: 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // DIRECTION CALCULATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public MovementDirection GetDirectionToward(GridPosition from, GridPosition to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        // Handle equal positions
        if (dx == 0 && dy == 0)
        {
            return MovementDirection.South; // Default
        }

        // Normalize to -1, 0, or 1
        var signX = Math.Sign(dx);
        var signY = Math.Sign(dy);

        return (signX, signY) switch
        {
            (0, -1) => MovementDirection.North,
            (0, 1) => MovementDirection.South,
            (1, 0) => MovementDirection.East,
            (-1, 0) => MovementDirection.West,
            (1, -1) => MovementDirection.NorthEast,
            (-1, -1) => MovementDirection.NorthWest,
            (1, 1) => MovementDirection.SouthEast,
            (-1, 1) => MovementDirection.SouthWest,
            _ => MovementDirection.South
        };
    }

    /// <inheritdoc />
    public MovementDirection GetDirectionAway(GridPosition current, GridPosition source)
    {
        var dx = current.X - source.X;
        var dy = current.Y - source.Y;

        // Handle equal positions
        if (dx == 0 && dy == 0)
        {
            return MovementDirection.North; // Default
        }

        // Normalize to -1, 0, or 1
        var signX = Math.Sign(dx);
        var signY = Math.Sign(dy);

        return (signX, signY) switch
        {
            (0, -1) => MovementDirection.North,
            (0, 1) => MovementDirection.South,
            (1, 0) => MovementDirection.East,
            (-1, 0) => MovementDirection.West,
            (1, -1) => MovementDirection.NorthEast,
            (-1, -1) => MovementDirection.NorthWest,
            (1, 1) => MovementDirection.SouthEast,
            (-1, 1) => MovementDirection.SouthWest,
            _ => MovementDirection.North
        };
    }

    /// <inheritdoc />
    public int GetStrengthModifier(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        // For players, use Might attribute (STR equivalent)
        // For monsters, use a base modifier from their stats or fixed value
        if (combatant.IsPlayer && combatant.Player != null)
        {
            // Calculate modifier from Might: (Might - 10) / 2
            var might = combatant.Player.Attributes.Might;
            return (might - 10) / 2;
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            // Monsters use their attack stat as a proxy for strength
            // Scale down attack to a reasonable modifier range
            var attack = combatant.Monster.Stats.Attack;
            return Math.Max(-2, Math.Min(10, attack / 3));
        }

        return 0;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes push movement after a successful opposed check.
    /// </summary>
    /// <param name="target">The combatant being pushed.</param>
    /// <param name="grid">The combat grid.</param>
    /// <param name="startPosition">Starting position.</param>
    /// <param name="direction">Push direction.</param>
    /// <param name="distance">Maximum push distance.</param>
    /// <param name="pusherRoll">Pusher's opposed check roll.</param>
    /// <param name="targetRoll">Target's opposed check roll.</param>
    /// <param name="pusherName">Name of the pusher for logging.</param>
    /// <returns>Result of the push movement.</returns>
    private PushResult ExecutePushMovement(
        Combatant target,
        CombatGrid grid,
        GridPosition startPosition,
        MovementDirection direction,
        int distance,
        int pusherRoll,
        int targetRoll,
        string pusherName)
    {
        var currentPosition = startPosition;
        var cellsMoved = 0;
        string? blockedBy = null;
        HazardDamageResult? hazardDamage = null;

        // Move cell by cell
        for (int i = 0; i < distance; i++)
        {
            var nextPosition = currentPosition.Move(direction);

            // Check for hazard
            if (IsHazard(grid, nextPosition))
            {
                // Move into hazard and apply damage
                if (grid.MoveEntity(target.Id, nextPosition))
                {
                    cellsMoved++;
                    currentPosition = nextPosition;

                    hazardDamage = ApplyHazardDamage(target, grid, nextPosition, isEntryDamage: true);

                    _logger.LogInformation(
                        "{Target} was pushed into {HazardName} by {Pusher}",
                        target.DisplayName,
                        hazardDamage.HazardName,
                        pusherName);

                    _eventLogger.LogCombat(
                        "HazardEntered",
                        $"{target.DisplayName} was pushed into {hazardDamage.HazardName} by {pusherName}",
                        data: new Dictionary<string, object>
                        {
                            ["targetId"] = target.Id,
                            ["targetName"] = target.DisplayName,
                            ["hazardType"] = hazardDamage.HazardType.ToString(),
                            ["hazardName"] = hazardDamage.HazardName,
                            ["position"] = nextPosition.ToString(),
                            ["wasPushed"] = true,
                            ["pusherName"] = pusherName
                        });

                    return PushResult.PushedIntoHazard(
                        startPosition,
                        currentPosition,
                        direction,
                        cellsMoved,
                        distance,
                        hazardDamage,
                        pusherRoll,
                        targetRoll);
                }
                break;
            }

            // Check if position is valid for movement (in bounds, passable, unoccupied)
            if (!grid.IsValidPosition(nextPosition))
            {
                // Determine what blocked the movement
                var cell = grid.GetCell(nextPosition);
                if (cell == null || !grid.IsInBounds(nextPosition))
                {
                    blockedBy = "wall";
                }
                else if (cell.IsOccupied)
                {
                    blockedBy = "another combatant";
                }
                else if (!cell.IsPassable)
                {
                    blockedBy = "impassable terrain";
                }
                else
                {
                    blockedBy = "obstacle";
                }

                _logger.LogDebug(
                    "Push blocked at {Position} by {BlockedBy}",
                    nextPosition,
                    blockedBy);
                break;
            }

            // Move the entity
            if (!grid.MoveEntity(target.Id, nextPosition))
            {
                blockedBy = "movement failure";
                break;
            }

            currentPosition = nextPosition;
            cellsMoved++;
        }

        // Log the push result
        if (cellsMoved > 0)
        {
            _logger.LogInformation(
                "{Target} was pushed {Cells} cell(s) {Direction} by {Pusher}",
                target.DisplayName,
                cellsMoved,
                direction,
                pusherName);

            _eventLogger.LogCombat(
                "CombatantPushed",
                $"{target.DisplayName} was pushed {cellsMoved} cell(s) {direction}",
                data: new Dictionary<string, object>
                {
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["startPosition"] = startPosition.ToString(),
                    ["endPosition"] = currentPosition.ToString(),
                    ["direction"] = direction.ToString(),
                    ["cellsMoved"] = cellsMoved,
                    ["pusherRoll"] = pusherRoll,
                    ["targetRoll"] = targetRoll
                });

            if (blockedBy != null)
            {
                return PushResult.Blocked(
                    startPosition,
                    currentPosition,
                    direction,
                    cellsMoved,
                    distance,
                    blockedBy,
                    pusherRoll,
                    targetRoll);
            }

            return PushResult.Success(
                startPosition,
                currentPosition,
                direction,
                cellsMoved,
                pusherRoll,
                targetRoll);
        }

        // No movement occurred
        return PushResult.Blocked(
            startPosition,
            startPosition,
            direction,
            0,
            distance,
            blockedBy ?? "unknown obstacle",
            pusherRoll,
            targetRoll);
    }

    /// <summary>
    /// Executes knockback movement (forced movement without opposed check).
    /// </summary>
    /// <param name="target">The combatant being knocked back.</param>
    /// <param name="grid">The combat grid.</param>
    /// <param name="startPosition">Starting position.</param>
    /// <param name="direction">Knockback direction.</param>
    /// <param name="distance">Knockback distance.</param>
    /// <returns>Result of the knockback movement.</returns>
    private PushResult ExecuteKnockbackMovement(
        Combatant target,
        CombatGrid grid,
        GridPosition startPosition,
        MovementDirection direction,
        int distance)
    {
        var currentPosition = startPosition;
        var cellsMoved = 0;
        HazardDamageResult? hazardDamage = null;

        // Move cell by cell
        for (int i = 0; i < distance; i++)
        {
            var nextPosition = currentPosition.Move(direction);

            // Check for hazard
            if (IsHazard(grid, nextPosition))
            {
                // Move into hazard and apply damage
                if (grid.MoveEntity(target.Id, nextPosition))
                {
                    cellsMoved++;
                    currentPosition = nextPosition;

                    hazardDamage = ApplyHazardDamage(target, grid, nextPosition, isEntryDamage: true);

                    _logger.LogInformation(
                        "{Target} was knocked back into {HazardName}",
                        target.DisplayName,
                        hazardDamage.HazardName);
                }
                break;
            }

            // Check if position is valid for movement
            if (!grid.IsValidPosition(nextPosition))
            {
                _logger.LogDebug(
                    "Knockback blocked at {Position}",
                    nextPosition);
                break;
            }

            // Move the entity
            if (!grid.MoveEntity(target.Id, nextPosition))
            {
                break;
            }

            currentPosition = nextPosition;
            cellsMoved++;
        }

        if (cellsMoved > 0)
        {
            _logger.LogInformation(
                "{Target} was knocked back {Cells} cell(s) {Direction}",
                target.DisplayName,
                cellsMoved,
                direction);

            _eventLogger.LogCombat(
                "CombatantKnockedBack",
                $"{target.DisplayName} was knocked back {cellsMoved} cell(s) {direction}",
                data: new Dictionary<string, object>
                {
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["startPosition"] = startPosition.ToString(),
                    ["endPosition"] = currentPosition.ToString(),
                    ["direction"] = direction.ToString(),
                    ["cellsMoved"] = cellsMoved,
                    ["hitHazard"] = hazardDamage != null
                });
        }

        return PushResult.Knockback(
            startPosition,
            currentPosition,
            direction,
            cellsMoved,
            hazardDamage);
    }

    /// <summary>
    /// Applies a status effect from a hazard to the target.
    /// </summary>
    /// <param name="target">The combatant to apply the effect to.</param>
    /// <param name="effectTarget">The underlying effect target interface.</param>
    /// <param name="hazardDef">The hazard definition.</param>
    /// <param name="position">The position of the hazard.</param>
    /// <returns>The applied status effect ID, or null if not applied.</returns>
    private string? ApplyHazardStatusEffect(
        Combatant target,
        IEffectTarget effectTarget,
        EnvironmentalHazardDefinition hazardDef,
        GridPosition position)
    {
        if (string.IsNullOrEmpty(hazardDef.StatusEffectId))
        {
            return null;
        }

        _logger.LogDebug(
            "Applying status effect '{EffectId}' from {HazardName} to {Target}",
            hazardDef.StatusEffectId,
            hazardDef.Name,
            target.DisplayName);

        var result = _buffDebuffService.ApplyEffect(
            effectTarget,
            hazardDef.StatusEffectId,
            sourceId: null,
            sourceName: hazardDef.Name);

        if (result.WasApplied)
        {
            _logger.LogInformation(
                "{Target} gained status effect '{EffectId}' from {HazardName}",
                target.DisplayName,
                hazardDef.StatusEffectId,
                hazardDef.Name);

            _eventLogger.LogCombat(
                "HazardStatusEffectApplied",
                $"{target.DisplayName} gained {hazardDef.StatusEffectId} from {hazardDef.Name}",
                data: new Dictionary<string, object>
                {
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["hazardType"] = hazardDef.Type.ToString(),
                    ["hazardName"] = hazardDef.Name,
                    ["statusEffectId"] = hazardDef.StatusEffectId,
                    ["position"] = position.ToString()
                });

            return hazardDef.StatusEffectId;
        }

        _logger.LogDebug(
            "Status effect '{EffectId}' not applied to {Target}: {Reason}",
            hazardDef.StatusEffectId,
            target.DisplayName,
            result.Message ?? "Unknown reason");

        return null;
    }

    /// <summary>
    /// Gets the IEffectTarget interface from a combatant.
    /// </summary>
    /// <param name="combatant">The combatant.</param>
    /// <returns>The IEffectTarget, or null if not available.</returns>
    /// <remarks>
    /// Returns null if the underlying entity (Player/Monster) does not implement IEffectTarget.
    /// Currently, neither Player nor Monster implements IEffectTarget, so this will always return null
    /// until those entities are updated. Status effects will not be applied through this path.
    /// </remarks>
    private static IEffectTarget? GetEffectTarget(Combatant combatant)
    {
        if (combatant.IsPlayer)
        {
            return combatant.Player as IEffectTarget;
        }

        if (combatant.IsMonster)
        {
            return combatant.Monster as IEffectTarget;
        }

        return null;
    }

    /// <summary>
    /// Applies damage directly to a combatant's underlying entity.
    /// </summary>
    /// <param name="combatant">The combatant to damage.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>The actual damage dealt after any reductions.</returns>
    /// <remarks>
    /// This method directly calls TakeDamage on the underlying Player or Monster entity,
    /// bypassing the IEffectTarget interface requirement.
    /// </remarks>
    private static int ApplyDamageToCombatant(Combatant combatant, int damage)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.TakeDamage(damage);
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            return combatant.Monster.TakeDamage(damage);
        }

        // Fallback: return the raw damage if we can't apply it
        return damage;
    }
}
