using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20.4: Service responsible for managing advanced movement abilities
/// Implements Leap, Dash, Blink, Climb, and Safe Step movements
/// </summary>
public class AdvancedMovementService
{
    private static readonly ILogger _log = Log.ForContext<AdvancedMovementService>();
    private readonly PositioningService _positioning;
    private readonly DiceService _diceService;
    private readonly KineticEnergyService _keService;
    private readonly GlitchService _glitchService;

    public AdvancedMovementService(
        PositioningService? positioning = null,
        DiceService? diceService = null,
        KineticEnergyService? keService = null,
        GlitchService? glitchService = null)
    {
        _positioning = positioning ?? new PositioningService();
        _diceService = diceService ?? new DiceService();
        _keService = keService ?? new KineticEnergyService();
        _glitchService = glitchService ?? new GlitchService();
    }

    /// <summary>
    /// Leap movement: Jump 2-3 tiles, bypass hazards
    /// Cost: 20 Stamina + FINESSE check
    /// </summary>
    public AdvancedMovementResult Leap(object combatant, GridPosition target, BattlefieldGrid grid)
    {
        var (currentPosition, stamina, finesse, combatantName) = ExtractCombatantData(combatant);

        if (currentPosition == null)
        {
            return AdvancedMovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Leap attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}",
            combatantName, currentPosition, target);

        // Validate zone
        if (currentPosition.Value.Zone != target.Zone)
        {
            return AdvancedMovementResult.Failure("Cannot leap to opposing zone");
        }

        // Validate distance
        int distance = CalculateDistance(currentPosition.Value, target);
        if (distance < 2 || distance > 3)
        {
            return AdvancedMovementResult.Failure("Leap range is 2-3 tiles (too close or too far)");
        }

        // Check Stamina
        if (stamina < 20)
        {
            return AdvancedMovementResult.Failure($"Insufficient Stamina (need 20, have {stamina})");
        }

        // Check target tile
        var targetTile = grid.GetTile(target);
        if (targetTile == null)
        {
            return AdvancedMovementResult.Failure("Invalid target position");
        }

        if (!targetTile.IsPassable())
        {
            return AdvancedMovementResult.Failure("Target tile is occupied");
        }

        // FINESSE check - DC scales with distance (12/14/16)
        int dc = 10 + (distance * 2);
        var checkResult = _diceService.Roll(finesse);

        if (!checkResult.Successes >= dc)
        {
            // Fall short - land on intervening tile
            var midpointTile = CalculateMidpoint(currentPosition.Value, target, grid);

            if (midpointTile == null || !midpointTile.IsPassable())
            {
                // Can't land on midpoint - leap fails completely
                ConsumeStamina(combatant, 20);

                _log.Warning("Leap failed completely: Combatant={CombatantName}, Roll={Successes}/{DC}",
                    combatantName, checkResult.Successes, dc);

                return AdvancedMovementResult.Failure(
                    $"You don't quite make the leap! (Rolled {checkResult.Successes} successes, needed {dc})"
                );
            }

            // Fall short to midpoint
            ConsumeStamina(combatant, 20);
            TeleportCombatant(combatant, midpointTile.Position, grid, bypassGlitches: true);

            _log.Warning("Leap fell short: Combatant={CombatantName}, LandedAt={Position}, Roll={Successes}/{DC}",
                combatantName, midpointTile.Position, checkResult.Successes, dc);

            return AdvancedMovementResult.PartialSuccess(
                $"You don't quite make the leap! You land at {midpointTile.Position} instead. (Rolled {checkResult.Successes} successes, needed {dc})",
                midpointTile.Position,
                20
            );
        }

        // Success - leap to target, bypassing hazards
        ConsumeStamina(combatant, 20);
        TeleportCombatant(combatant, target, grid, bypassGlitches: true);

        _log.Information("Leap successful: Combatant={CombatantName}, NewPosition={Position}, Distance={Distance}, Roll={Successes}/{DC}",
            combatantName, target, distance, checkResult.Successes, dc);

        return AdvancedMovementResult.CreateSuccess(
            $"You leap gracefully across the battlefield! (Rolled {checkResult.Successes} successes vs DC {dc})",
            20
        );
    }

    /// <summary>
    /// Dash movement: Rapid 3-tile straight line movement
    /// Cost: 25 KE + 10 Stamina, grants +10 KE bonus on arrival
    /// </summary>
    public AdvancedMovementResult Dash(object combatant, GridPosition target, BattlefieldGrid grid)
    {
        var (currentPosition, stamina, finesse, combatantName) = ExtractCombatantData(combatant);
        int currentKE = _keService.GetCurrentKE(combatant);

        if (currentPosition == null)
        {
            return AdvancedMovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Dash attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}, KE={KE}",
            combatantName, currentPosition, target, currentKE);

        // Validate zone
        if (currentPosition.Value.Zone != target.Zone)
        {
            return AdvancedMovementResult.Failure("Cannot dash to opposing zone");
        }

        // Validate straight line
        if (!IsValidDashTarget(currentPosition.Value, target))
        {
            return AdvancedMovementResult.Failure("Must dash in a straight line (same row or same column)");
        }

        // Validate distance
        int distance = CalculateDistance(currentPosition.Value, target);
        if (distance > 3)
        {
            return AdvancedMovementResult.Failure("Dash range limited to 3 tiles");
        }

        // Check KE and Stamina
        if (currentKE < 25)
        {
            return AdvancedMovementResult.Failure($"Insufficient Kinetic Energy (need 25, have {currentKE})");
        }

        if (stamina < 10)
        {
            return AdvancedMovementResult.Failure($"Insufficient Stamina (need 10, have {stamina})");
        }

        // Check line of sight
        if (!HasLineOfSight(currentPosition.Value, target, grid))
        {
            return AdvancedMovementResult.Failure("Path blocked by obstacles");
        }

        // Check target tile
        var targetTile = grid.GetTile(target);
        if (targetTile == null)
        {
            return AdvancedMovementResult.Failure("Invalid target position");
        }

        if (!targetTile.IsPassable())
        {
            return AdvancedMovementResult.Failure("Target tile is occupied");
        }

        // Execute dash
        _keService.TrySpendKE(combatant, 25);
        ConsumeStamina(combatant, 10);
        TeleportCombatant(combatant, target, grid, bypassGlitches: true);

        // Bonus KE from momentum
        _keService.GrantKE(combatant, 10);

        _log.Information("Dash successful: Combatant={CombatantName}, NewPosition={Position}, Distance={Distance}, RemainingKE={KE}",
            combatantName, target, distance, _keService.GetCurrentKE(combatant));

        return AdvancedMovementResult.CreateSuccess(
            "You dash across the battlefield in a blur of motion! (+10 KE from momentum)",
            10,
            25
        );
    }

    /// <summary>
    /// Blink movement: Short-range teleportation
    /// Cost: 40 AP, bypasses all hazards and interception
    /// </summary>
    public AdvancedMovementResult Blink(object combatant, GridPosition target, BattlefieldGrid grid)
    {
        var (currentPosition, stamina, finesse, combatantName) = ExtractCombatantData(combatant);

        // Get Aether Pool
        int aetherPool = combatant switch
        {
            PlayerCharacter player => player.AP,
            Enemy enemy => 0, // Enemies typically don't have AP - this would be a special ability
            _ => 0
        };

        if (currentPosition == null)
        {
            return AdvancedMovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Blink attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}, AP={AP}",
            combatantName, currentPosition, target, aetherPool);

        // Validate distance
        int distance = CalculateDistance(currentPosition.Value, target);
        if (distance > 2)
        {
            return AdvancedMovementResult.Failure("Blink range limited to 2 tiles");
        }

        // Check AP
        if (aetherPool < 40)
        {
            return AdvancedMovementResult.Failure($"Insufficient Aether Pool (need 40, have {aetherPool})");
        }

        // Check target tile
        var targetTile = grid.GetTile(target);
        if (targetTile == null)
        {
            return AdvancedMovementResult.Failure("Invalid target position");
        }

        if (!targetTile.IsPassable())
        {
            return AdvancedMovementResult.Failure("Target tile is occupied");
        }

        // Execute blink (bypasses all hazards)
        ConsumeAP(combatant, 40);
        TeleportCombatant(combatant, target, grid, bypassGlitches: true);

        _log.Information("Blink successful: Combatant={CombatantName}, NewPosition={Position}, Distance={Distance}, RemainingAP={AP}",
            combatantName, target, distance, GetCurrentAP(combatant));

        return AdvancedMovementResult.CreateSuccess(
            "You phase through reality and reappear elsewhere!",
            0,
            0,
            40
        );
    }

    /// <summary>
    /// Climb movement: Access elevated positions
    /// Cost: 15 Stamina + FINESSE check (DC 12)
    /// </summary>
    public AdvancedMovementResult Climb(object combatant, GridPosition target, BattlefieldGrid grid)
    {
        var (currentPosition, stamina, finesse, combatantName) = ExtractCombatantData(combatant);

        if (currentPosition == null)
        {
            return AdvancedMovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Climb attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}",
            combatantName, currentPosition, target);

        // Validate climb (must be adjacent and upward)
        int distance = CalculateDistance(currentPosition.Value, target);
        if (distance > 1)
        {
            return AdvancedMovementResult.Failure("Can only climb to adjacent tiles");
        }

        if (target.Elevation <= currentPosition.Value.Elevation)
        {
            return AdvancedMovementResult.Failure("Must climb upward (target elevation must be higher)");
        }

        // Check target tile
        var targetTile = grid.GetTile(target);
        if (targetTile == null)
        {
            return AdvancedMovementResult.Failure("Invalid target position");
        }

        if (targetTile.Type != TileType.HighGround)
        {
            return AdvancedMovementResult.Failure("No climbable surface (tile must be HighGround type)");
        }

        if (!targetTile.IsPassable())
        {
            return AdvancedMovementResult.Failure("Target tile is occupied");
        }

        // Check Stamina
        if (stamina < 15)
        {
            return AdvancedMovementResult.Failure($"Insufficient Stamina (need 15, have {stamina})");
        }

        // FINESSE check DC 12
        int dc = 12;
        var checkResult = _diceService.Roll(finesse);

        if (checkResult.Successes < dc)
        {
            ConsumeStamina(combatant, 15);

            _log.Warning("Climb failed: Combatant={CombatantName}, Roll={Successes}/{DC}",
                combatantName, checkResult.Successes, dc);

            return AdvancedMovementResult.Failure(
                $"You fail to scale the obstacle! (Rolled {checkResult.Successes} successes, needed {dc})"
            );
        }

        // Success - climb to high ground
        ConsumeStamina(combatant, 15);
        TeleportCombatant(combatant, target, grid, bypassGlitches: false);

        _log.Information("Climb successful: Combatant={CombatantName}, NewElevation={Elevation}, Roll={Successes}/{DC}",
            combatantName, target.Elevation, checkResult.Successes, dc);

        return AdvancedMovementResult.CreateSuccess(
            $"You climb to higher ground! (+2 Accuracy ranged, +2 Defense) (Rolled {checkResult.Successes} successes vs DC {dc})",
            15
        );
    }

    /// <summary>
    /// Safe Step movement: Careful movement ignoring glitch checks
    /// Cost: 15 Stamina + WITS check (DC 10, auto-pass if WITS ≥5)
    /// </summary>
    public AdvancedMovementResult SafeStep(object combatant, GridPosition target, BattlefieldGrid grid)
    {
        var (currentPosition, stamina, finesse, combatantName) = ExtractCombatantData(combatant);

        // Get WITS
        int wits = combatant switch
        {
            PlayerCharacter player => player.Attributes.Wits,
            Enemy enemy => enemy.Attributes.Wits,
            _ => 0
        };

        if (currentPosition == null)
        {
            return AdvancedMovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Safe Step attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}, WITS={Wits}",
            combatantName, currentPosition, target, wits);

        // Validate distance (must be adjacent)
        int distance = CalculateDistance(currentPosition.Value, target);
        if (distance > 1)
        {
            return AdvancedMovementResult.Failure("Safe Step limited to adjacent tiles (1 tile)");
        }

        // Validate zone
        if (currentPosition.Value.Zone != target.Zone)
        {
            return AdvancedMovementResult.Failure("Cannot step to opposing zone");
        }

        // Check target tile
        var targetTile = grid.GetTile(target);
        if (targetTile == null)
        {
            return AdvancedMovementResult.Failure("Invalid target position");
        }

        if (!targetTile.IsPassable())
        {
            return AdvancedMovementResult.Failure("Target tile is occupied");
        }

        // Check Stamina
        if (stamina < 15)
        {
            return AdvancedMovementResult.Failure($"Insufficient Stamina (need 15, have {stamina})");
        }

        // WITS check (auto-pass if WITS ≥ 5)
        if (wits < 5)
        {
            int dc = 10;
            var checkResult = _diceService.Roll(wits);

            if (checkResult.Successes < dc)
            {
                ConsumeStamina(combatant, 15);

                _log.Warning("Safe Step failed: Combatant={CombatantName}, WITS={Wits}, Roll={Successes}/{DC}",
                    combatantName, wits, checkResult.Successes, dc);

                return AdvancedMovementResult.Failure(
                    $"You fail to navigate carefully enough! (Rolled {checkResult.Successes} successes, needed {dc})"
                );
            }

            _log.Information("Safe Step check passed: Combatant={CombatantName}, WITS={Wits}, Roll={Successes}/{DC}",
                combatantName, wits, checkResult.Successes, dc);
        }
        else
        {
            _log.Information("Safe Step auto-passed: Combatant={CombatantName}, WITS={Wits}",
                combatantName, wits);
        }

        // Move without glitch checks
        ConsumeStamina(combatant, 15);
        TeleportCombatant(combatant, target, grid, bypassGlitches: true);

        _log.Information("Safe Step successful: Combatant={CombatantName}, NewPosition={Position}",
            combatantName, target);

        return AdvancedMovementResult.CreateSuccess(
            "You carefully navigate the hazardous terrain.",
            15
        );
    }

    // Helper methods

    private (GridPosition? position, int stamina, int finesse, string name) ExtractCombatantData(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => (player.Position, player.Stamina, player.Attributes.Finesse, player.Name),
            Enemy enemy => (enemy.Position, 100, enemy.Attributes.Finesse, enemy.Name), // Enemies have infinite stamina for movement
            _ => (null, 0, 0, "Unknown")
        };
    }

    private int CalculateDistance(GridPosition from, GridPosition to)
    {
        // Calculate Manhattan distance
        int rowDistance = from.Row == to.Row ? 0 : 1;
        int columnDistance = Math.Abs(to.Column - from.Column);
        int elevationDistance = Math.Abs(to.Elevation - from.Elevation);

        return Math.Max(rowDistance + columnDistance, elevationDistance);
    }

    private bool IsValidDashTarget(GridPosition from, GridPosition to)
    {
        // Must be straight line: same row or same column (and same elevation)
        bool sameRow = from.Row == to.Row && from.Elevation == to.Elevation;
        bool sameColumn = from.Column == to.Column && from.Elevation == to.Elevation;

        return sameRow || sameColumn;
    }

    private bool HasLineOfSight(GridPosition from, GridPosition to, BattlefieldGrid grid)
    {
        // Check if there are any occupied tiles between from and to
        var path = GetTilesInPath(from, to);

        foreach (var pos in path)
        {
            var tile = grid.GetTile(pos);
            if (tile != null && tile.IsOccupied)
            {
                return false;
            }
        }

        return true;
    }

    private List<GridPosition> GetTilesInPath(GridPosition from, GridPosition to)
    {
        var path = new List<GridPosition>();

        // Determine direction
        if (from.Row == to.Row)
        {
            // Horizontal movement
            int step = to.Column > from.Column ? 1 : -1;
            for (int col = from.Column + step; col != to.Column; col += step)
            {
                path.Add(new GridPosition(from.Zone, from.Row, col, from.Elevation));
            }
        }
        else if (from.Column == to.Column)
        {
            // Only row change (front/back) - no intervening tiles
            // No tiles to check
        }

        return path;
    }

    private BattlefieldTile? CalculateMidpoint(GridPosition from, GridPosition to, BattlefieldGrid grid)
    {
        // Calculate midpoint position
        int midColumn = (from.Column + to.Column) / 2;
        Row midRow = from.Row; // Default to starting row

        // If moving to different row, midpoint is still in the same row as start
        var midPosition = new GridPosition(from.Zone, midRow, midColumn, from.Elevation);

        return grid.GetTile(midPosition);
    }

    private void TeleportCombatant(object combatant, GridPosition target, BattlefieldGrid grid, bool bypassGlitches)
    {
        // Get current position
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return;

        // Clear old tile
        var oldTile = grid.GetTile(currentPosition.Value);
        if (oldTile != null)
        {
            oldTile.IsOccupied = false;
            oldTile.OccupantId = null;
        }

        // Update combatant position
        switch (combatant)
        {
            case PlayerCharacter player:
                player.Position = target;
                player.HasMovedThisTurn = true;
                break;

            case Enemy enemy:
                enemy.Position = target;
                enemy.HasMovedThisTurn = true;
                break;
        }

        // Occupy new tile
        var targetTile = grid.GetTile(target);
        if (targetTile != null)
        {
            targetTile.IsOccupied = true;
            targetTile.OccupantId = GetCombatantId(combatant);
        }

        // Handle glitches if not bypassing
        if (!bypassGlitches && targetTile != null && targetTile.Type == TileType.Glitched)
        {
            var glitchResult = _glitchService.ResolveGlitchedTileEntry(combatant, targetTile, grid);
            // Note: Glitch effects are applied, but we don't redirect movement for advanced abilities
        }
    }

    private string GetCombatantId(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => "player",
            Enemy enemy => enemy.Id,
            _ => "unknown"
        };
    }

    private void ConsumeStamina(object combatant, int amount)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.Stamina = Math.Max(0, player.Stamina - amount);
                break;
            // Enemies don't consume stamina
        }
    }

    private void ConsumeAP(object combatant, int amount)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.AP = Math.Max(0, player.AP - amount);
                break;
            // Enemies don't typically have AP
        }
    }

    private int GetCurrentAP(object combatant)
    {
        return combatant switch
        {
            PlayerCharacter player => player.AP,
            _ => 0
        };
    }
}

/// <summary>
/// Result of an advanced movement attempt
/// Extends MovementResult to support partial success and alternate positions
/// </summary>
public class AdvancedMovementResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GridPosition? AlternatePosition { get; set; }
    public int StaminaCost { get; set; }
    public int KECost { get; set; }
    public int APCost { get; set; }

    public static AdvancedMovementResult CreateSuccess(string message, int staminaCost = 0, int keCost = 0, int apCost = 0)
    {
        return new AdvancedMovementResult
        {
            Success = true,
            Message = message,
            StaminaCost = staminaCost,
            KECost = keCost,
            APCost = apCost
        };
    }

    public static AdvancedMovementResult Failure(string message)
    {
        return new AdvancedMovementResult
        {
            Success = false,
            Message = message
        };
    }

    public static AdvancedMovementResult PartialSuccess(string message, GridPosition position, int staminaCost = 0)
    {
        return new AdvancedMovementResult
        {
            Success = true,
            Message = message,
            AlternatePosition = position,
            StaminaCost = staminaCost
        };
    }
}
