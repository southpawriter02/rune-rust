using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a forced movement operation (Pull or Push)
/// </summary>
public class ForcedMovementResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Row? NewRow { get; set; }
    public int CorruptionBonus { get; set; } = 0;

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
}
