using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.3: Service for resolving glitched tile hazards on the battlefield
/// Handles Flickering Platforms, Inverted Gravity, and Looping Corridors
/// </summary>
public class GlitchService
{
    private static readonly ILogger _log = Log.ForContext<GlitchService>();
    private readonly DiceService _diceService;
    private readonly Random _random;

    public GlitchService(DiceService? diceService = null, Random? random = null)
    {
        _diceService = diceService ?? new DiceService();
        _random = random ?? new Random();
    }

    /// <summary>
    /// Resolves a glitched tile entry for a combatant
    /// Returns result indicating success, failure, or teleportation
    /// </summary>
    public GlitchResult ResolveGlitchedTileEntry(object combatant, BattlefieldTile tile, BattlefieldGrid grid)
    {
        if (tile.GlitchType == null)
            return GlitchResult.Success();

        var (combatantName, combatantId) = combatant switch
        {
            PlayerCharacter player => (player.Name, "player"),
            Enemy enemy => (enemy.Name, enemy.Id),
            _ => ("Unknown", "unknown")
        };

        _log.Warning("Glitched tile entry: Combatant={CombatantName}, Position={Position}, GlitchType={GlitchType}, Severity={Severity}",
            combatantName, tile.Position, tile.GlitchType, tile.GlitchSeverity);

        switch (tile.GlitchType.Value)
        {
            case Core.GlitchType.Flickering:
                return ResolveFlickeringPlatform(combatant, tile);

            case Core.GlitchType.InvertedGravity:
                return ResolveInvertedGravity(combatant, tile);

            case Core.GlitchType.Looping:
                return ResolveLoopingCorridor(combatant, tile, grid);

            default:
                return GlitchResult.Success();
        }
    }

    /// <summary>
    /// Resolves Flickering Platform glitch
    /// FINESSE check DC 12/14/16, failure causes 1d6/2d6/3d6 damage and movement fails
    /// </summary>
    private GlitchResult ResolveFlickeringPlatform(object combatant, BattlefieldTile tile)
    {
        int severity = ScaleSeverityByCorruption(tile, combatant);
        int dc = CalculateDC(severity);
        int damageDice = severity;

        int finesseValue = combatant switch
        {
            PlayerCharacter player => player.Attributes.Finesse,
            Enemy enemy => enemy.Attributes.Finesse,
            _ => 0
        };

        var result = _diceService.Roll(finesseValue);

        if (result.Successes >= dc)
        {
            _log.Information("Flickering platform traversed: Combatant={CombatantId}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), result.Successes, dc);

            return GlitchResult.Success("You deftly time your step through the flickering platform.");
        }
        else
        {
            int damage = RollDamage(damageDice);
            ApplyDamage(combatant, damage);

            _log.Warning("Flickering platform failed: Combatant={CombatantId}, Damage={Damage}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), damage, result.Successes, dc);

            return GlitchResult.Failure(
                $"The platform phases out beneath you! You fall for {damage} damage and fail to cross.",
                movementFailed: true
            );
        }
    }

    /// <summary>
    /// Resolves Inverted Gravity glitch
    /// STURDINESS check DC 12/14/16, failure applies [Disoriented] for 1/2/3 turns
    /// </summary>
    private GlitchResult ResolveInvertedGravity(object combatant, BattlefieldTile tile)
    {
        int severity = ScaleSeverityByCorruption(tile, combatant);
        int dc = CalculateDC(severity);
        int duration = severity;

        int sturdinessValue = combatant switch
        {
            PlayerCharacter player => player.Attributes.Sturdiness,
            Enemy enemy => enemy.Attributes.Sturdiness,
            _ => 0
        };

        var result = _diceService.Roll(sturdinessValue);

        if (result.Successes >= dc)
        {
            _log.Information("Inverted gravity resisted: Combatant={CombatantId}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), result.Successes, dc);

            return GlitchResult.Success("You maintain your equilibrium despite the gravitational anomaly.");
        }
        else
        {
            ApplyDisoriented(combatant, duration);

            _log.Warning("Inverted gravity afflicted: Combatant={CombatantId}, Duration={Duration}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), duration, result.Successes, dc);

            return GlitchResult.Failure(
                $"The inverted gravity disorients you! [Disoriented] for {duration} turns.",
                movementFailed: false
            );
        }
    }

    /// <summary>
    /// Resolves Looping Corridor glitch
    /// WITS check DC 12/14/16, failure teleports to random tile in same zone
    /// </summary>
    private GlitchResult ResolveLoopingCorridor(object combatant, BattlefieldTile tile, BattlefieldGrid grid)
    {
        int severity = ScaleSeverityByCorruption(tile, combatant);
        int dc = CalculateDC(severity);

        int witsValue = combatant switch
        {
            PlayerCharacter player => player.Attributes.Wits,
            Enemy enemy => enemy.Attributes.Wits,
            _ => 0
        };

        var result = _diceService.Roll(witsValue);

        if (result.Successes >= dc)
        {
            _log.Information("Looping corridor navigated: Combatant={CombatantId}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), result.Successes, dc);

            return GlitchResult.Success("You recognize the spatial recursion and navigate correctly.");
        }
        else
        {
            // Teleport to random valid tile in same zone
            var randomTile = SelectRandomTile(grid, tile.Position.Zone);

            if (randomTile == null)
            {
                // Fallback if no valid tile found
                return GlitchResult.Success("The looping corridor distorts space, but you emerge at your intended destination.");
            }

            _log.Warning("Looping corridor teleported: Combatant={CombatantId}, From={FromPos}, To={ToPos}, Roll={Successes}/{DC}",
                GetCombatantId(combatant), tile.Position, randomTile.Position, result.Successes, dc);

            return GlitchResult.TeleportFailure(
                $"The looping corridor redirects you! You emerge at {randomTile.Position}.",
                randomTile.Position
            );
        }
    }

    /// <summary>
    /// Calculates DC based on glitch severity
    /// Severity 1 = DC 12, Severity 2 = DC 14, Severity 3 = DC 16
    /// </summary>
    private int CalculateDC(int severity)
    {
        return 10 + (severity * 2);
    }

    /// <summary>
    /// Scales glitch severity based on combatant's Corruption level
    /// High Corruption (75+) increases severity by 1 (max 3)
    /// </summary>
    private int ScaleSeverityByCorruption(BattlefieldTile tile, object combatant)
    {
        int baseSeverity = tile.GlitchSeverity;

        if (combatant is PlayerCharacter player && player.Corruption >= 75)
        {
            int scaledSeverity = Math.Min(3, baseSeverity + 1);

            if (scaledSeverity > baseSeverity)
            {
                _log.Warning("Glitch severity increased by Corruption: Combatant={CombatantId}, Corruption={Corruption}, Severity={OldSeverity}->{NewSeverity}",
                    player.Name, player.Corruption, baseSeverity, scaledSeverity);
            }

            return scaledSeverity;
        }

        return baseSeverity;
    }

    /// <summary>
    /// Selects a random unoccupied tile in the specified zone
    /// </summary>
    private BattlefieldTile? SelectRandomTile(BattlefieldGrid grid, Zone zone)
    {
        var validTiles = grid.Tiles.Values
            .Where(t => t.Position.Zone == zone && !t.IsOccupied)
            .ToList();

        if (validTiles.Count == 0)
            return null;

        return validTiles[_random.Next(validTiles.Count)];
    }

    /// <summary>
    /// Rolls damage dice (Xd6)
    /// </summary>
    private int RollDamage(int diceCount)
    {
        return _diceService.RollDamage(diceCount);
    }

    /// <summary>
    /// Applies damage to a combatant
    /// </summary>
    private void ApplyDamage(object combatant, int damage)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.HP = Math.Max(0, player.HP - damage);
                break;

            case Enemy enemy:
                enemy.HP = Math.Max(0, enemy.HP - damage);
                break;
        }
    }

    /// <summary>
    /// Applies [Disoriented] status effect to a combatant
    /// </summary>
    private void ApplyDisoriented(object combatant, int duration)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.DisorientedTurnsRemaining = duration;
                break;

            case Enemy enemy:
                enemy.DisorientedTurnsRemaining = duration;
                break;
        }
    }

    /// <summary>
    /// Gets combatant ID for logging
    /// </summary>
    private string GetCombatantId(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Id,
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Result of a glitched tile resolution
/// </summary>
public class GlitchResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool MovementFailed { get; set; }
    public GridPosition? TeleportTo { get; set; }

    public static GlitchResult Success(string? message = null)
    {
        return new GlitchResult
        {
            Success = true,
            Message = message ?? string.Empty
        };
    }

    public static GlitchResult Failure(string message, bool movementFailed)
    {
        return new GlitchResult
        {
            Success = false,
            Message = message,
            MovementFailed = movementFailed
        };
    }

    public static GlitchResult TeleportFailure(string message, GridPosition teleportTo)
    {
        return new GlitchResult
        {
            Success = false,
            Message = message,
            TeleportTo = teleportTo,
            MovementFailed = false
        };
    }
}
