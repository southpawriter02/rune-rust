using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a forced movement operation (Pull or Push)
/// v0.29.5: Extended to support tile-based movement and environmental kills
/// </summary>
public class ForcedMovementResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Row? NewRow { get; set; }
    public int CorruptionBonus { get; set; } = 0;

    // v0.29.5: Tile-based movement properties
    public GridPosition? FinalPosition { get; set; }
    public int TilesTraversed { get; set; } = 0;
    public bool WasLethal { get; set; } = false;
    public string? KillingHazard { get; set; }

    public static ForcedMovementResult Successful(Row newRow, int corruptionBonus = 0)
    {
        return new ForcedMovementResult
        {
            Success = true,
            Message = $"Target moved to {newRow} row",
            NewRow = newRow,
            CorruptionBonus = corruptionBonus
        };
    }

    public static ForcedMovementResult Failure(string reason)
    {
        return new ForcedMovementResult
        {
            Success = false,
            Message = reason
        };
    }

    /// <summary>
    /// v0.29.5: Creates a result for tile-based forced movement
    /// </summary>
    public static ForcedMovementResult TileBasedSuccess(GridPosition finalPosition, int tilesTraversed, bool wasLethal = false, string? killingHazard = null)
    {
        return new ForcedMovementResult
        {
            Success = true,
            Message = wasLethal ? $"Target pushed into {killingHazard}! Environmental kill." : $"Target pushed {tilesTraversed} tiles",
            FinalPosition = finalPosition,
            TilesTraversed = tilesTraversed,
            WasLethal = wasLethal,
            KillingHazard = killingHazard
        };
    }
}

/// <summary>
/// v0.25.2: Service responsible for forced movement mechanics (Pull/Push)
/// Used by Hlekkr-master specialization abilities to manipulate enemy positioning
/// </summary>
public class ForcedMovementService
{
    private static readonly ILogger _log = Log.ForContext<ForcedMovementService>();

    public enum MovementDirection
    {
        Pull, // Back Row → Front Row
        Push  // Front Row → Back Row
    }

    /// <summary>
    /// Attempts to forcibly move an enemy between rows (Pull or Push)
    /// </summary>
    public ForcedMovementResult AttemptForcedMovement(
        Enemy target,
        MovementDirection direction,
        string abilityName)
    {
        using (_log.BeginTimedOperation("Attempt Forced Movement"))
        {
            _log.Information(
                "Forced Movement Attempt: Target={TargetName}, Direction={Direction}, Ability={Ability}, Corruption={Corruption}",
                target.Name, direction, abilityName, target.Corruption);

            // TODO v0.26+: Check size restrictions (Large/Huge enemies immune)
            // For now, all enemies can be moved

            // Check current position
            if (target.Position == null)
            {
                _log.Warning("Target has no position - cannot perform forced movement");
                return ForcedMovementResult.Failure("Target has no position");
            }

            var currentRow = target.Position.Value.Row;

            // Validate movement direction
            if (direction == MovementDirection.Pull && currentRow != Row.Back)
            {
                _log.Information("Pull failed: Target not in Back Row (current: {Row})", currentRow);
                return ForcedMovementResult.Failure("Target must be in Back Row to be pulled");
            }

            if (direction == MovementDirection.Push && currentRow != Row.Front)
            {
                _log.Information("Push failed: Target not in Front Row (current: {Row})", currentRow);
                return ForcedMovementResult.Failure("Target must be in Front Row to be pushed");
            }

            // Calculate corruption bonus for success rate
            int corruptionBonus = CalculatePullSuccessBonus(target.Corruption);

            // Execute movement
            var newRow = direction == MovementDirection.Pull ? Row.Front : Row.Back;
            var newPosition = new GridPosition(
                target.Position.Value.Zone,
                newRow,
                target.Position.Value.Column,
                target.Position.Value.Elevation);

            target.Position = newPosition;

            _log.Information(
                "Forced Movement Success: Target={TargetName}, OldRow={OldRow}, NewRow={NewRow}, CorruptionBonus={Bonus}",
                target.Name, currentRow, newRow, corruptionBonus);

            return ForcedMovementResult.Successful(newRow, corruptionBonus);
        }
    }

    /// <summary>
    /// Calculates success rate bonus based on target's Corruption level
    /// Implements Snag the Glitch mechanic: corrupted enemies are easier to control
    /// </summary>
    public int CalculatePullSuccessBonus(int corruption)
    {
        if (corruption >= 90) return 60;  // Extreme Corruption: +60%
        if (corruption >= 60) return 40;  // High Corruption: +40%
        if (corruption >= 30) return 20;  // Medium Corruption: +20%
        if (corruption >= 1) return 10;   // Low Corruption: +10%
        return 0;                          // No Corruption: +0%
    }

    /// <summary>
    /// Gets the current row of a combatant
    /// </summary>
    public Row? GetCombatantRow(Enemy enemy)
    {
        return enemy.Position?.Row;
    }

    /// <summary>
    /// Gets the current row of a player character
    /// </summary>
    public Row? GetCombatantRow(PlayerCharacter character)
    {
        return character.Position?.Row;
    }

    /// <summary>
    /// Checks if an enemy can be targeted for Pull (must be in Back Row)
    /// </summary>
    public bool CanPullTarget(Enemy target)
    {
        if (target.Position == null) return false;
        return target.Position.Value.Row == Row.Back;
    }

    /// <summary>
    /// Checks if an enemy can be targeted for Push (must be in Front Row)
    /// </summary>
    public bool CanPushTarget(Enemy target)
    {
        if (target.Position == null) return false;
        return target.Position.Value.Row == Row.Front;
    }

    /// <summary>
    /// v0.29.5: Attempts to push a target in a specific direction for a given distance.
    /// Checks each tile along the path for blocking hazards and lethality.
    /// Used for abilities like Shield Bash, Telekinetic Shove, etc.
    /// </summary>
    /// <param name="source">The combatant doing the pushing</param>
    /// <param name="target">The combatant being pushed</param>
    /// <param name="direction">Direction to push (column delta, row delta)</param>
    /// <param name="distance">Number of tiles to push</param>
    /// <param name="grid">The battlefield grid</param>
    /// <param name="environmentalObjects">List of environmental objects for hazard checking</param>
    /// <returns>Result including success, final position, and whether it was lethal</returns>
    public ForcedMovementResult TryPushTileBased(
        object source,
        object target,
        (int columnDelta, int rowDelta) direction,
        int distance,
        BattlefieldGrid grid,
        List<EnvironmentalObject>? environmentalObjects = null)
    {
        var (sourceName, sourcePosition) = source switch
        {
            PlayerCharacter player => (player.Name, player.Position),
            Enemy enemy => (enemy.Name, enemy.Position),
            _ => throw new ArgumentException("Invalid source combatant type")
        };

        var (targetName, targetPosition) = target switch
        {
            PlayerCharacter player => (player.Name, player.Position),
            Enemy enemy => (enemy.Name, enemy.Position),
            _ => throw new ArgumentException("Invalid target combatant type")
        };

        if (targetPosition == null)
        {
            return ForcedMovementResult.Failure("Target has no position");
        }

        _log.Information(
            "Tile-based forced movement: Source={Source}, Target={Target}, Direction=({ColDelta},{RowDelta}), Distance={Distance}",
            sourceName, targetName, direction.columnDelta, direction.rowDelta, distance);

        var currentPosition = targetPosition.Value;
        int tilesTraversed = 0;

        // Traverse the push path
        for (int i = 1; i <= distance; i++)
        {
            // Calculate next position
            var nextColumn = currentPosition.Column + direction.columnDelta;
            var nextRow = direction.rowDelta == 0 ? currentPosition.Row : (direction.rowDelta > 0 ? Row.Back : Row.Front);
            var nextPosition = new GridPosition(currentPosition.Zone, nextRow, nextColumn, currentPosition.Elevation);

            // Check if next tile exists
            var nextTile = grid.GetTile(nextPosition);
            if (nextTile == null)
            {
                _log.Debug("Forced movement stopped: Edge of battlefield at {Position}", nextPosition);
                break; // Stop at edge
            }

            // Get environmental objects for this tile
            var tileEnvironmentalObjects = environmentalObjects?.Where(obj =>
                obj.GridPosition == nextPosition.ToString()).ToList();

            // Check if tile is passable
            if (!nextTile.IsPassable(tileEnvironmentalObjects))
            {
                var blockingFeature = nextTile.GetBlockingFeature(tileEnvironmentalObjects);

                if (blockingFeature != null)
                {
                    // Check if blocking feature is lethal
                    if (IsLethalHazard(blockingFeature))
                    {
                        return HandleEnvironmentalKill(source, target, nextTile, blockingFeature, tilesTraversed, grid);
                    }
                    else
                    {
                        // Non-lethal blocking (e.g., wall, occupied tile)
                        _log.Information(
                            "Forced movement stopped: {Target} impacts {Feature}",
                            targetName, blockingFeature);
                        break;
                    }
                }
                else
                {
                    // Occupied or otherwise not passable
                    _log.Information("Forced movement stopped: Tile not passable at {Position}", nextPosition);
                    break;
                }
            }

            // Move to next tile
            currentPosition = nextPosition;
            tilesTraversed++;
        }

        // Update target position
        UpdateCombatantPosition(target, currentPosition, grid);

        _log.Information(
            "Forced movement complete: {Target} pushed {Tiles} tiles to {Position}",
            targetName, tilesTraversed, currentPosition);

        return ForcedMovementResult.TileBasedSuccess(currentPosition, tilesTraversed);
    }

    /// <summary>
    /// v0.29.5: Checks if a hazard causes instant death on forced entry
    /// </summary>
    private bool IsLethalHazard(string featureName)
    {
        return featureName switch
        {
            "[Chasm/Lava River]" => true,
            "[Chasm]" => true,
            "[Lava River]" => true,
            "[Molten Slag]" => true,
            _ => false
        };
    }

    /// <summary>
    /// v0.29.5: Handles instant death from forced movement into lethal hazard
    /// </summary>
    private ForcedMovementResult HandleEnvironmentalKill(
        object source,
        object target,
        BattlefieldTile lethalTile,
        string hazardName,
        int tilesTraversed,
        BattlefieldGrid grid)
    {
        var (sourceName, _) = source switch
        {
            PlayerCharacter player => (player.Name, player.Position),
            Enemy enemy => (enemy.Name, enemy.Position),
            _ => ("Unknown", (GridPosition?)null)
        };

        var (targetName, _) = target switch
        {
            PlayerCharacter player => (player.Name, player.Position),
            Enemy enemy => (enemy.Name, enemy.Position),
            _ => ("Unknown", (GridPosition?)null)
        };

        _log.Warning(
            "Environmental kill: {Target} pushed into {Hazard} by {Source}",
            targetName, hazardName, sourceName);

        // Apply instant death
        switch (target)
        {
            case PlayerCharacter player:
                player.CurrentHP = 0;
                player.Position = lethalTile.Position;
                // Note: Biome status tracking should be done by the calling service
                break;

            case Enemy enemy:
                enemy.CurrentHP = 0;
                enemy.Position = lethalTile.Position;
                break;
        }

        // Update tile occupancy
        UpdateCombatantPosition(target, lethalTile.Position, grid);

        _log.Warning("{Target} has been defeated by environmental hazard", targetName);

        return ForcedMovementResult.TileBasedSuccess(
            lethalTile.Position,
            tilesTraversed,
            wasLethal: true,
            killingHazard: hazardName);
    }

    /// <summary>
    /// v0.29.5: Helper to update combatant position and grid tiles
    /// </summary>
    private void UpdateCombatantPosition(object combatant, GridPosition newPosition, BattlefieldGrid grid)
    {
        // Get current position and clear old tile
        var currentPosition = combatant switch
        {
            PlayerCharacter player => player.Position,
            Enemy enemy => enemy.Position,
            _ => null
        };

        if (currentPosition != null)
        {
            var oldTile = grid.GetTile(currentPosition.Value);
            if (oldTile != null)
            {
                oldTile.IsOccupied = false;
                oldTile.OccupantId = null;
            }
        }

        // Update combatant position
        switch (combatant)
        {
            case PlayerCharacter player:
                player.Position = newPosition;
                break;

            case Enemy enemy:
                enemy.Position = newPosition;
                break;
        }

        // Occupy new tile
        var newTile = grid.GetTile(newPosition);
        if (newTile != null)
        {
            newTile.IsOccupied = true;
            newTile.OccupantId = combatant switch
            {
                PlayerCharacter _ => "player",
                Enemy enemy => enemy.Id,
                _ => "unknown"
            };
        }
    }
}
