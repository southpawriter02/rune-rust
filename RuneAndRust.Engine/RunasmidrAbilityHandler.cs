using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Handler for Rúnasmiðr (Runesmith) trap-based abilities
/// Integrates with TrapService to place and manage battlefield traps
/// </summary>
public class RunasmidrAbilityHandler
{
    private static readonly ILogger _log = Log.ForContext<RunasmidrAbilityHandler>();
    private readonly TrapService _trapService;

    public RunasmidrAbilityHandler(TrapService trapService)
    {
        _trapService = trapService;
    }

    /// <summary>
    /// Executes a Rúnasmiðr trap ability
    /// </summary>
    public AbilityExecutionResult ExecuteTrapAbility(PlayerCharacter caster, string abilityName,
        GridPosition targetPosition, CombatState combat)
    {
        if (combat.Grid == null)
        {
            return AbilityExecutionResult.Failure("Combat grid not initialized");
        }

        _log.Information("Rúnasmiðr trap ability execution: Ability={AbilityName}, Target={Position}",
            abilityName, targetPosition);

        return abilityName switch
        {
            "Hagalaz Trap" => PlaceHagalazTrap(caster, targetPosition, combat),
            "Rune of Disruption" => PlaceRuneOfDisruption(caster, targetPosition, combat),
            "Rune of Isolation" => PlaceRuneOfIsolation(caster, targetPosition, combat),
            _ => AbilityExecutionResult.Failure($"Unknown Rúnasmiðr ability: {abilityName}")
        };
    }

    /// <summary>
    /// Hagalaz Trap (Tier 1): Ice damage trap with AoE
    /// Cost: 15 Stamina
    /// Effect: Place a trap that deals 3d6 ice damage in a 1-tile radius when triggered
    /// </summary>
    private AbilityExecutionResult PlaceHagalazTrap(PlayerCharacter caster, GridPosition targetPosition, CombatState combat)
    {
        const int STAMINA_COST = 15;

        if (caster.Stamina < STAMINA_COST)
        {
            return AbilityExecutionResult.Failure($"Insufficient Stamina (need {STAMINA_COST}, have {caster.Stamina})");
        }

        var trap = new BattlefieldTrap(
            trapId: Guid.NewGuid().ToString(),
            trapName: "Hagalaz Trap",
            ownerId: "player"
        )
        {
            EffectType = TrapEffectType.AreaEffect,
            TriggerType = TrapTriggerType.OnEnter,
            TurnsRemaining = 3,
            IsVisible = false
        };

        // Set trap effect data
        trap.EffectData["DamageDice"] = 3; // 3d6 damage
        trap.EffectData["Radius"] = 1; // 1-tile radius
        trap.EffectData["DamageType"] = "Ice";

        var result = _trapService.PlaceTrap(caster.Name, "player", targetPosition, combat.Grid, trap);

        if (result.Success)
        {
            caster.Stamina -= STAMINA_COST;
            combat.AddLogEntry($"[Rúnasmiðr] {caster.Name} inscribes a Hagalaz rune at {targetPosition}...");
            combat.AddLogEntry($"  The air grows cold around the trap.");

            _log.Information("Hagalaz Trap placed: Position={Position}, StaminaCost={Cost}",
                targetPosition, STAMINA_COST);

            return AbilityExecutionResult.CreateSuccess("Hagalaz Trap placed successfully", STAMINA_COST);
        }

        return AbilityExecutionResult.Failure(result.Message);
    }

    /// <summary>
    /// Rune of Disruption (Tier 1): Disorients enemies
    /// Cost: 12 Stamina
    /// Effect: Place a trap that applies [Disoriented] (2 turns) when triggered
    /// </summary>
    private AbilityExecutionResult PlaceRuneOfDisruption(PlayerCharacter caster, GridPosition targetPosition, CombatState combat)
    {
        const int STAMINA_COST = 12;

        if (caster.Stamina < STAMINA_COST)
        {
            return AbilityExecutionResult.Failure($"Insufficient Stamina (need {STAMINA_COST}, have {caster.Stamina})");
        }

        var trap = new BattlefieldTrap(
            trapId: Guid.NewGuid().ToString(),
            trapName: "Rune of Disruption",
            ownerId: "player"
        )
        {
            EffectType = TrapEffectType.Status,
            TriggerType = TrapTriggerType.OnEnter,
            TurnsRemaining = 3,
            IsVisible = false
        };

        // Set trap effect data
        trap.EffectData["StatusEffect"] = "Disoriented";
        trap.EffectData["Duration"] = 2;

        var result = _trapService.PlaceTrap(caster.Name, "player", targetPosition, combat.Grid, trap);

        if (result.Success)
        {
            caster.Stamina -= STAMINA_COST;
            combat.AddLogEntry($"[Rúnasmiðr] {caster.Name} inscribes a Rune of Disruption at {targetPosition}...");
            combat.AddLogEntry($"  Reality flickers around the unstable rune.");

            _log.Information("Rune of Disruption placed: Position={Position}, StaminaCost={Cost}",
                targetPosition, STAMINA_COST);

            return AbilityExecutionResult.CreateSuccess("Rune of Disruption placed successfully", STAMINA_COST);
        }

        return AbilityExecutionResult.Failure(result.Message);
    }

    /// <summary>
    /// Rune of Isolation (Tier 2): Roots enemies in place
    /// Cost: 18 Stamina
    /// Effect: Place a trap that applies [Rooted] (3 turns) when triggered
    /// </summary>
    private AbilityExecutionResult PlaceRuneOfIsolation(PlayerCharacter caster, GridPosition targetPosition, CombatState combat)
    {
        const int STAMINA_COST = 18;

        if (caster.Stamina < STAMINA_COST)
        {
            return AbilityExecutionResult.Failure($"Insufficient Stamina (need {STAMINA_COST}, have {caster.Stamina})");
        }

        var trap = new BattlefieldTrap(
            trapId: Guid.NewGuid().ToString(),
            trapName: "Rune of Isolation",
            ownerId: "player"
        )
        {
            EffectType = TrapEffectType.Status,
            TriggerType = TrapTriggerType.OnEnter,
            TurnsRemaining = 3,
            IsVisible = false
        };

        // Set trap effect data
        trap.EffectData["StatusEffect"] = "Rooted";
        trap.EffectData["Duration"] = 3;

        var result = _trapService.PlaceTrap(caster.Name, "player", targetPosition, combat.Grid, trap);

        if (result.Success)
        {
            caster.Stamina -= STAMINA_COST;
            combat.AddLogEntry($"[Rúnasmiðr] {caster.Name} inscribes a Rune of Isolation at {targetPosition}...");
            combat.AddLogEntry($"  The rune pulses with binding energy.");

            _log.Information("Rune of Isolation placed: Position={Position}, StaminaCost={Cost}",
                targetPosition, STAMINA_COST);

            return AbilityExecutionResult.CreateSuccess("Rune of Isolation placed successfully", STAMINA_COST);
        }

        return AbilityExecutionResult.Failure(result.Message);
    }

    /// <summary>
    /// Checks if an ability is a trap-based Rúnasmiðr ability
    /// </summary>
    public static bool IsTrapAbility(string abilityName)
    {
        return abilityName switch
        {
            "Hagalaz Trap" => true,
            "Rune of Disruption" => true,
            "Rune of Isolation" => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the target position for a trap ability based on player position
    /// Default: Place trap in front of player (Enemy zone, same column)
    /// </summary>
    public static GridPosition GetDefaultTrapPosition(PlayerCharacter player, BattlefieldGrid grid)
    {
        if (player.Position == null)
        {
            // Default to center front of enemy zone
            return new GridPosition(Zone.Enemy, Row.Front, grid.Columns / 2);
        }

        // Place in enemy zone, front row, same column as player
        return new GridPosition(Zone.Enemy, Row.Front, player.Position.Value.Column);
    }
}

/// <summary>
/// Result of an ability execution
/// </summary>
public class AbilityExecutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int ResourceCost { get; set; }

    public AbilityExecutionResult(bool success, string message, int resourceCost = 0)
    {
        Success = success;
        Message = message;
        ResourceCost = resourceCost;
    }

    public static AbilityExecutionResult CreateSuccess(string message, int resourceCost)
    {
        return new AbilityExecutionResult(true, message, resourceCost);
    }

    public static AbilityExecutionResult Failure(string message)
    {
        return new AbilityExecutionResult(false, message, 0);
    }
}
