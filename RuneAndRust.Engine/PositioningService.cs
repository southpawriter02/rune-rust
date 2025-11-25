using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.20: Service responsible for managing combatant movement and positioning on the tactical grid
/// </summary>
public class PositioningService
{
    private static readonly ILogger _log = Log.ForContext<PositioningService>();
    private readonly GlitchService _glitchService; // v0.20.3

    public PositioningService(GlitchService? glitchService = null)
    {
        _glitchService = glitchService ?? new GlitchService();
    }

    /// <summary>
    /// Attempts to move a combatant to a new position on the grid
    /// v0.29.5: Updated to check environmental objects for blocking hazards
    /// </summary>
    public MovementResult MoveCombatant(object combatant, GridPosition targetPosition, BattlefieldGrid grid, List<EnvironmentalObject>? environmentalObjects = null)
    {
        // Handle both PlayerCharacter and Enemy types
        var (currentPosition, stamina, isPlayer, combatantId, combatantName) = combatant switch
        {
            PlayerCharacter player => (player.Position, player.Stamina, true, "player", player.Name),
            Enemy enemy => (enemy.Position, 0, false, enemy.Id, enemy.Name), // Enemies don't use stamina for movement
            _ => throw new ArgumentException("Invalid combatant type")
        };

        if (currentPosition == null)
        {
            return MovementResult.Failure("Combatant has no current position");
        }

        _log.Information("Movement attempt: Combatant={CombatantName}, From={FromPos}, To={ToPos}",
            combatantName, currentPosition, targetPosition);

        // Validate movement (v0.29.5: now passes environmental objects)
        if (!CanMove(currentPosition.Value, targetPosition, grid, environmentalObjects, out string reason))
        {
            _log.Warning("Movement blocked: Combatant={CombatantName}, Reason={Reason}",
                combatantName, reason);
            return MovementResult.Failure(reason);
        }

        // Calculate Stamina cost (only for players)
        int staminaCost = CalculateMovementCost(currentPosition.Value, targetPosition);

        if (isPlayer && stamina < staminaCost)
        {
            return MovementResult.Failure($"Insufficient Stamina (need {staminaCost}, have {stamina})");
        }

        // Get target tile
        var targetTile = grid.GetTile(targetPosition);
        if (targetTile == null)
        {
            return MovementResult.Failure("Invalid target position");
        }

        // v0.20.3: Check for glitched tile BEFORE moving
        if (targetTile.Type == TileType.Glitched)
        {
            var glitchResult = _glitchService.ResolveGlitchedTileEntry(combatant, targetTile, grid);

            if (!glitchResult.Success)
            {
                // Movement failed or redirected
                if (glitchResult.MovementFailed)
                {
                    // Actor doesn't move (Flickering Platform failure)
                    _log.Warning("Movement failed due to glitch: Combatant={CombatantName}, Position={Position}",
                        combatantName, targetPosition);
                    return MovementResult.Failure(glitchResult.Message);
                }
                else if (glitchResult.TeleportTo.HasValue)
                {
                    // Actor teleported to different location (Looping Corridor)
                    targetPosition = glitchResult.TeleportTo.Value;
                    targetTile = grid.GetTile(targetPosition);

                    if (targetTile == null)
                    {
                        return MovementResult.Failure("Teleport destination invalid");
                    }

                    _log.Warning("Movement teleported by glitch: Combatant={CombatantName}, NewPosition={Position}",
                        combatantName, targetPosition);
                }
                // Else: Movement succeeds with status effect (Inverted Gravity)
                // Status effect already applied by GlitchService
            }
        }

        // Execute movement
        var oldTile = grid.GetTile(currentPosition.Value);
        if (oldTile != null)
        {
            oldTile.IsOccupied = false;
            oldTile.OccupantId = null;
        }

        // Update combatant position and stamina
        switch (combatant)
        {
            case PlayerCharacter player:
                player.Position = targetPosition;
                player.Stamina -= staminaCost;
                player.TilesMovedThisTurn++;
                player.HasMovedThisTurn = true;
                BuildKineticEnergy(player, staminaCost);
                break;

            case Enemy enemy:
                enemy.Position = targetPosition;
                enemy.TilesMovedThisTurn++;
                enemy.HasMovedThisTurn = true;
                BuildKineticEnergy(enemy, staminaCost);
                break;
        }

        targetTile.IsOccupied = true;
        targetTile.OccupantId = combatantId;

        _log.Information("Movement successful: Combatant={CombatantName}, NewPos={NewPos}, StaminaCost={Cost}",
            combatantName, targetPosition, staminaCost);

        return MovementResult.Success($"Moved to {targetPosition}", staminaCost);
    }

    /// <summary>
    /// Checks if a movement from one position to another is valid
    /// v0.29.5: Updated to check environmental objects for blocking hazards
    /// </summary>
    private bool CanMove(GridPosition from, GridPosition to, BattlefieldGrid grid, List<EnvironmentalObject>? environmentalObjects, out string reason)
    {
        // Cannot move across zones (Player <-> Enemy)
        if (from.Zone != to.Zone)
        {
            reason = "Cannot move to opposing zone";
            return false;
        }

        // Check if target position is valid
        if (!grid.IsValidPosition(to))
        {
            reason = "Invalid target position";
            return false;
        }

        // Check if target tile exists
        var targetTile = grid.GetTile(to);
        if (targetTile == null)
        {
            reason = "Target tile does not exist";
            return false;
        }

        // v0.29.5: Get environmental objects for this tile
        var tileEnvironmentalObjects = environmentalObjects?.Where(obj =>
            obj.GridPosition == to.ToString()).ToList();

        // v0.29.5: Check if target tile is passable (considering environmental objects)
        if (!targetTile.IsPassable(tileEnvironmentalObjects))
        {
            // Get blocking feature for better feedback
            var blockingFeature = targetTile.GetBlockingFeature(tileEnvironmentalObjects);

            if (blockingFeature != null)
            {
                reason = $"Path blocked by {blockingFeature}";
                _log.Information("Movement blocked by environmental hazard: Feature={Feature}, Position={Position}",
                    blockingFeature, to);
            }
            else if (targetTile.IsOccupied)
            {
                reason = "Target tile is occupied";
            }
            else
            {
                reason = "Target tile is not passable";
            }

            return false;
        }

        // Check movement distance (limit to adjacent tiles or row changes)
        // Allow: same row adjacent column, or column change with row change
        bool isAdjacentColumn = Math.Abs(to.Column - from.Column) <= 1;
        bool isSameColumn = to.Column == from.Column;
        bool isRowChange = to.Row != from.Row;

        if (!isAdjacentColumn && !isSameColumn)
        {
            reason = "Can only move to adjacent tiles or same column";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    /// <summary>
    /// Calculates the Stamina cost for a movement
    /// Row change: 10 Stamina
    /// Column change: 5 Stamina per column
    /// Elevation change: +15 Stamina
    /// </summary>
    private int CalculateMovementCost(GridPosition from, GridPosition to)
    {
        int cost = 0;

        // Row change cost
        if (from.Row != to.Row)
        {
            cost += 10;
        }

        // Column change cost
        int columnDelta = Math.Abs(to.Column - from.Column);
        cost += columnDelta * 5;

        // Elevation change cost
        if (to.Elevation > from.Elevation)
        {
            cost += 15;
        }

        return cost;
    }

    /// <summary>
    /// Builds Kinetic Energy based on Stamina spent on movement
    /// KE gain = Stamina spent, capped at MaxKE
    /// </summary>
    private void BuildKineticEnergy(PlayerCharacter player, int staminaCost)
    {
        int keGain = staminaCost;
        int newKE = Math.Min(player.KineticEnergy + keGain, player.MaxKineticEnergy);
        player.KineticEnergy = newKE;

        _log.Debug("Kinetic Energy gained: Player={PlayerName}, KEGain={KEGain}, TotalKE={TotalKE}",
            player.Name, keGain, newKE);
    }

    private void BuildKineticEnergy(Enemy enemy, int movementCost)
    {
        // Enemies build KE differently - fixed amount per movement
        int keGain = 10;
        int newKE = Math.Min(enemy.KineticEnergy + keGain, enemy.MaxKineticEnergy);
        enemy.KineticEnergy = newKE;

        _log.Debug("Kinetic Energy gained: Enemy={EnemyName}, KEGain={KEGain}, TotalKE={TotalKE}",
            enemy.Name, keGain, newKE);
    }

    /// <summary>
    /// Gets all valid movement positions for a combatant
    /// v0.29.5: Updated to consider environmental objects
    /// </summary>
    public List<GridPosition> GetValidMovementPositions(object combatant, BattlefieldGrid grid, List<EnvironmentalObject>? environmentalObjects = null)
    {
        var validPositions = new List<GridPosition>();

        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return validPositions;

        // Check all possible positions in the same zone
        foreach (var tile in grid.Tiles.Values)
        {
            if (tile.Position.Zone == currentPosition.Value.Zone &&
                CanMove(currentPosition.Value, tile.Position, grid, environmentalObjects, out _))
            {
                validPositions.Add(tile.Position);
            }
        }

        return validPositions;
    }

    /// <summary>
    /// Moves a combatant to the front row of their zone
    /// </summary>
    public MovementResult MoveToFront(object combatant, BattlefieldGrid grid)
    {
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return MovementResult.Failure("No current position");

        var targetPosition = new GridPosition(
            currentPosition.Value.Zone,
            Row.Front,
            currentPosition.Value.Column,
            currentPosition.Value.Elevation
        );

        return MoveCombatant(combatant, targetPosition, grid);
    }

    /// <summary>
    /// Moves a combatant to the back row of their zone
    /// </summary>
    public MovementResult MoveToBack(object combatant, BattlefieldGrid grid)
    {
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return MovementResult.Failure("No current position");

        var targetPosition = new GridPosition(
            currentPosition.Value.Zone,
            Row.Back,
            currentPosition.Value.Column,
            currentPosition.Value.Elevation
        );

        return MoveCombatant(combatant, targetPosition, grid);
    }

    /// <summary>
    /// Moves a combatant one column to the left
    /// </summary>
    public MovementResult MoveLeft(object combatant, BattlefieldGrid grid)
    {
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return MovementResult.Failure("No current position");

        if (currentPosition.Value.Column <= 0)
            return MovementResult.Failure("Already at leftmost column");

        var targetPosition = new GridPosition(
            currentPosition.Value.Zone,
            currentPosition.Value.Row,
            currentPosition.Value.Column - 1,
            currentPosition.Value.Elevation
        );

        return MoveCombatant(combatant, targetPosition, grid);
    }

    /// <summary>
    /// Moves a combatant one column to the right
    /// </summary>
    public MovementResult MoveRight(object combatant, BattlefieldGrid grid)
    {
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition == null)
            return MovementResult.Failure("No current position");

        if (currentPosition.Value.Column >= grid.Columns - 1)
            return MovementResult.Failure("Already at rightmost column");

        var targetPosition = new GridPosition(
            currentPosition.Value.Zone,
            currentPosition.Value.Row,
            currentPosition.Value.Column + 1,
            currentPosition.Value.Elevation
        );

        return MoveCombatant(combatant, targetPosition, grid);
    }

    /// <summary>
    /// Resets movement tracking at the start of a combatant's turn
    /// </summary>
    public void OnTurnStart(object combatant)
    {
        switch (combatant)
        {
            case PlayerCharacter player:
                player.HasMovedThisTurn = false;
                player.TilesMovedThisTurn = 0;
                break;

            case Enemy enemy:
                enemy.HasMovedThisTurn = false;
                enemy.TilesMovedThisTurn = 0;
                break;
        }
    }
}

/// <summary>
/// Result of a movement attempt
/// </summary>
public class MovementResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int StaminaCost { get; set; }

    public MovementResult(bool success, string message, int staminaCost = 0)
    {
        Success = success;
        Message = message;
        StaminaCost = staminaCost;
    }

    public static MovementResult Successful(string message, int staminaCost)
    {
        return new MovementResult(true, message, staminaCost);
    }

    public static MovementResult Failure(string message)
    {
        return new MovementResult(false, message, 0);
    }
}
