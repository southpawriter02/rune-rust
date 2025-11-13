using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Service responsible for managing battlefield traps
/// Handles placement, triggering, and duration tracking
/// </summary>
public class TrapService
{
    private static readonly ILogger _log = Log.ForContext<TrapService>();
    private readonly DiceService _diceService;

    public TrapService(DiceService diceService)
    {
        _diceService = diceService;
    }

    /// <summary>
    /// Places a trap on a battlefield tile
    /// </summary>
    public TrapPlacementResult PlaceTrap(string casterName, string casterId, GridPosition targetPosition,
        BattlefieldGrid grid, BattlefieldTrap trap)
    {
        _log.Information("Trap placement attempt: Caster={CasterName}, Trap={TrapName}, Position={Position}",
            casterName, trap.TrapName, targetPosition);

        // Validate tile exists
        var targetTile = grid.GetTile(targetPosition);
        if (targetTile == null)
        {
            return TrapPlacementResult.Failure("Invalid tile position");
        }

        // Validate tile is not occupied by a combatant
        if (targetTile.IsOccupied)
        {
            return TrapPlacementResult.Failure("Cannot place trap on occupied tile");
        }

        // Set trap properties
        trap.Position = targetPosition;
        trap.TurnsRemaining = 3; // Default duration
        trap.IsVisible = false;   // Invisible to enemies

        // Add trap to tile
        targetTile.Traps.Add(trap);

        _log.Information("Trap placed successfully: TrapId={TrapId}, Position={Position}, Duration={Duration}",
            trap.TrapId, targetPosition, trap.TurnsRemaining);

        return TrapPlacementResult.Success($"{trap.TrapName} placed at {targetPosition}");
    }

    /// <summary>
    /// Triggers all OnEnter traps when a combatant enters a tile
    /// </summary>
    public List<TrapTriggerResult> TriggerTrapsOnTileEntry(object target, BattlefieldTile tile, CombatState combat)
    {
        var results = new List<TrapTriggerResult>();

        var onEnterTraps = tile.Traps.Where(t => t.TriggerType == TrapTriggerType.OnEnter).ToList();

        if (onEnterTraps.Count == 0)
            return results;

        var (targetName, targetId, isPlayer) = target switch
        {
            PlayerCharacter player => (player.Name, "player", true),
            Enemy enemy => (enemy.Name, enemy.Id, false),
            _ => ("Unknown", "unknown", false)
        };

        _log.Information("Trap triggers: Target={TargetName}, Tile={TilePos}, TrapCount={TrapCount}",
            targetName, tile.Position, onEnterTraps.Count);

        foreach (var trap in onEnterTraps)
        {
            var result = TriggerTrap(trap, target, combat);
            results.Add(result);

            // Remove trap after triggering
            tile.Traps.Remove(trap);

            // Runic Synergy: Restore resources to trap owner if they have the passive
            ApplyRunicSynergy(trap.OwnerId, combat);
        }

        return results;
    }

    /// <summary>
    /// Triggers a specific trap
    /// </summary>
    private TrapTriggerResult TriggerTrap(BattlefieldTrap trap, object target, CombatState combat)
    {
        var (targetName, targetId, isPlayer) = target switch
        {
            PlayerCharacter player => (player.Name, "player", true),
            Enemy enemy => (enemy.Name, enemy.Id, false),
            _ => ("Unknown", "unknown", false)
        };

        _log.Information("Trap triggered: Trap={TrapName}, Target={TargetName}, EffectType={EffectType}",
            trap.TrapName, targetName, trap.EffectType);

        var result = new TrapTriggerResult
        {
            TrapName = trap.TrapName,
            TargetName = targetName,
            Success = true,
            Message = $"{trap.TrapName} triggers!"
        };

        // Apply trap effect based on type
        switch (trap.EffectType)
        {
            case TrapEffectType.Damage:
                ApplyTrapDamage(trap, target, result);
                break;

            case TrapEffectType.Status:
                ApplyTrapStatus(trap, target, result);
                break;

            case TrapEffectType.Debuff:
                ApplyTrapDebuff(trap, target, result);
                break;

            case TrapEffectType.AreaEffect:
                ApplyTrapAreaEffect(trap, target, combat, result);
                break;
        }

        combat.AddLogEntry($"[TRAP] {result.Message}");

        return result;
    }

    /// <summary>
    /// Applies damage from a trap
    /// </summary>
    private void ApplyTrapDamage(BattlefieldTrap trap, object target, TrapTriggerResult result)
    {
        if (!trap.EffectData.TryGetValue("DamageDice", out var diceObj) || diceObj is not int damageDice)
        {
            damageDice = 2; // Default 2d6
        }

        var damageRoll = _diceService.Roll(damageDice);
        int totalDamage = damageRoll.Total;

        // Apply damage to target
        switch (target)
        {
            case PlayerCharacter player:
                player.HP -= totalDamage;
                result.DamageDealt = totalDamage;
                result.Message = $"{trap.TrapName} deals {totalDamage} damage to {player.Name}!";
                break;

            case Enemy enemy:
                enemy.HP -= totalDamage;
                result.DamageDealt = totalDamage;
                result.Message = $"{trap.TrapName} deals {totalDamage} damage to {enemy.Name}!";
                break;
        }

        _log.Information("Trap damage applied: Trap={TrapName}, Damage={Damage}",
            trap.TrapName, totalDamage);
    }

    /// <summary>
    /// Applies a status effect from a trap
    /// </summary>
    private void ApplyTrapStatus(BattlefieldTrap trap, object target, TrapTriggerResult result)
    {
        if (!trap.EffectData.TryGetValue("StatusEffect", out var statusObj) || statusObj is not string statusEffect)
        {
            statusEffect = "Rooted";
        }

        if (!trap.EffectData.TryGetValue("Duration", out var durationObj) || durationObj is not int duration)
        {
            duration = 2;
        }

        // Apply status based on effect name
        switch (target)
        {
            case PlayerCharacter player:
                ApplyStatusToPlayer(player, statusEffect, duration);
                result.StatusApplied = statusEffect;
                result.Message = $"{trap.TrapName} applies [{statusEffect}] to {player.Name} for {duration} turns!";
                break;

            case Enemy enemy:
                ApplyStatusToEnemy(enemy, statusEffect, duration);
                result.StatusApplied = statusEffect;
                result.Message = $"{trap.TrapName} applies [{statusEffect}] to {enemy.Name} for {duration} turns!";
                break;
        }

        _log.Information("Trap status applied: Trap={TrapName}, Status={Status}, Duration={Duration}",
            trap.TrapName, statusEffect, duration);
    }

    private void ApplyStatusToPlayer(PlayerCharacter player, string statusEffect, int duration)
    {
        switch (statusEffect)
        {
            case "Rooted":
                player.RootedTurnsRemaining = Math.Max(player.RootedTurnsRemaining, duration);
                break;
            case "Disoriented":
                // TODO: Add Disoriented status to PlayerCharacter
                break;
            case "Vulnerable":
                player.VulnerableTurnsRemaining = Math.Max(player.VulnerableTurnsRemaining, duration);
                break;
        }
    }

    private void ApplyStatusToEnemy(Enemy enemy, string statusEffect, int duration)
    {
        switch (statusEffect)
        {
            case "Rooted":
                // TODO: Add Rooted status to Enemy
                break;
            case "Disoriented":
                // TODO: Add Disoriented status to Enemy
                break;
            case "Vulnerable":
                enemy.VulnerableTurnsRemaining = Math.Max(enemy.VulnerableTurnsRemaining, duration);
                break;
        }
    }

    /// <summary>
    /// Applies a debuff from a trap
    /// </summary>
    private void ApplyTrapDebuff(BattlefieldTrap trap, object target, TrapTriggerResult result)
    {
        // TODO: Implement debuff application (attribute penalties, etc.)
        result.Message = $"{trap.TrapName} applies a debuff!";
    }

    /// <summary>
    /// Applies an area effect from a trap
    /// </summary>
    private void ApplyTrapAreaEffect(BattlefieldTrap trap, object target, CombatState combat, TrapTriggerResult result)
    {
        if (!trap.EffectData.TryGetValue("Radius", out var radiusObj) || radiusObj is not int radius)
        {
            radius = 1; // Default 1-tile radius
        }

        // Get all combatants within radius of trap
        var affectedCombatants = GetCombatantsInRadius(trap.Position, radius, combat);

        int totalDamage = 0;
        foreach (var combatant in affectedCombatants)
        {
            // Apply damage to each combatant in radius
            var damageRoll = _diceService.Roll(2); // 2d6 area damage
            totalDamage += damageRoll.Total;

            switch (combatant)
            {
                case PlayerCharacter player:
                    player.HP -= damageRoll.Total;
                    break;
                case Enemy enemy:
                    enemy.HP -= damageRoll.Total;
                    break;
            }
        }

        result.DamageDealt = totalDamage;
        result.Message = $"{trap.TrapName} explodes, dealing damage to {affectedCombatants.Count} targets!";

        _log.Information("Trap area effect applied: Trap={TrapName}, Targets={TargetCount}, TotalDamage={TotalDamage}",
            trap.TrapName, affectedCombatants.Count, totalDamage);
    }

    /// <summary>
    /// Gets all combatants within a radius of a position
    /// </summary>
    private List<object> GetCombatantsInRadius(GridPosition center, int radius, CombatState combat)
    {
        var combatants = new List<object>();

        if (combat.Grid == null)
            return combatants;

        // Check player
        if (combat.Player.Position != null && IsWithinRadius(center, combat.Player.Position.Value, radius))
        {
            combatants.Add(combat.Player);
        }

        // Check enemies
        foreach (var enemy in combat.Enemies)
        {
            if (enemy.Position != null && IsWithinRadius(center, enemy.Position.Value, radius))
            {
                combatants.Add(enemy);
            }
        }

        return combatants;
    }

    private bool IsWithinRadius(GridPosition center, GridPosition target, int radius)
    {
        // Same zone only
        if (center.Zone != target.Zone)
            return false;

        // Calculate Manhattan distance
        int columnDist = Math.Abs(target.Column - center.Column);
        int rowDist = (center.Row == target.Row) ? 0 : 1;

        return (columnDist + rowDist) <= radius;
    }

    /// <summary>
    /// Decrements trap durations at end of turn
    /// </summary>
    public void DecrementTrapDurations(BattlefieldGrid grid, CombatState combat)
    {
        var expiredTraps = new List<(BattlefieldTile tile, BattlefieldTrap trap)>();

        foreach (var tile in grid.Tiles.Values)
        {
            foreach (var trap in tile.Traps)
            {
                trap.TurnsRemaining--;

                if (trap.TurnsRemaining <= 0)
                {
                    expiredTraps.Add((tile, trap));
                    _log.Information("Trap expired: Trap={TrapName}, Position={Position}",
                        trap.TrapName, trap.Position);
                }
            }
        }

        // Remove expired traps
        foreach (var (tile, trap) in expiredTraps)
        {
            tile.Traps.Remove(trap);
            combat.AddLogEntry($"[TRAP] {trap.TrapName} fades away...");
        }
    }

    /// <summary>
    /// Applies Runic Synergy passive: Restore resources when trap triggers
    /// </summary>
    private void ApplyRunicSynergy(string ownerId, CombatState combat)
    {
        // Check if owner is player and has Runic Synergy
        if (ownerId == "player" && combat.Player.Abilities.Any(a => a.Name == "Runic Synergy"))
        {
            // Restore 10 Stamina
            combat.Player.Stamina = Math.Min(combat.Player.Stamina + 10, combat.Player.MaxStamina);
            combat.AddLogEntry($"[Runic Synergy] {combat.Player.Name} recovers 10 Stamina!");

            _log.Information("Runic Synergy triggered: Player={PlayerName}, StaminaRestored=10",
                combat.Player.Name);
        }
    }
}

/// <summary>
/// Result of a trap placement attempt
/// </summary>
public class TrapPlacementResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public TrapPlacementResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static TrapPlacementResult Success(string message)
    {
        return new TrapPlacementResult(true, message);
    }

    public static TrapPlacementResult Failure(string message)
    {
        return new TrapPlacementResult(false, message);
    }
}

/// <summary>
/// Result of a trap triggering
/// </summary>
public class TrapTriggerResult
{
    public string TrapName { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public string? StatusApplied { get; set; }
}
