using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a performance attempt
/// </summary>
public class PerformanceResult
{
    public bool Success { get; set; }
    public string PerformanceName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Manages Skald performance/channeling system
/// </summary>
public class PerformanceService
{
    /// <summary>
    /// Start a performance (channeled ability)
    /// </summary>
    public PerformanceResult StartPerformance(PlayerCharacter player, string performanceName, int skillCheckSuccesses)
    {
        // Check if already performing
        if (player.IsPerforming)
        {
            return new PerformanceResult
            {
                Success = false,
                Message = $"Already performing {player.CurrentPerformance}! End current performance first."
            };
        }

        // Check if silenced
        if (player.SilencedTurnsRemaining > 0)
        {
            return new PerformanceResult
            {
                Success = false,
                Message = "Cannot perform while [Silenced]!"
            };
        }

        // Calculate duration based on WILL score
        int baseDuration = player.Attributes.Will;

        // Enduring Performance: +2 rounds to all performances
        var enduringPerformance = player.Abilities.FirstOrDefault(a => a.Name == "Enduring Performance");
        if (enduringPerformance != null)
        {
            baseDuration += 2;
        }

        // Start the performance
        player.IsPerforming = true;
        player.CurrentPerformance = performanceName;
        player.PerformingTurnsRemaining = baseDuration;

        return new PerformanceResult
        {
            Success = true,
            PerformanceName = performanceName,
            Duration = baseDuration,
            Message = $"You begin performing: {performanceName}\nDuration: {baseDuration} rounds"
        };
    }

    /// <summary>
    /// End a performance (voluntary or forced)
    /// </summary>
    public string EndPerformance(PlayerCharacter player, bool forced = false)
    {
        if (!player.IsPerforming)
        {
            return "No active performance to end.";
        }

        string performanceName = player.CurrentPerformance ?? "Unknown";
        player.IsPerforming = false;
        player.CurrentPerformance = null;
        player.PerformingTurnsRemaining = 0;

        // Clean up performance-specific status effects
        CleanupPerformanceEffects(player, performanceName);

        if (forced)
        {
            return $"Performance interrupted: {performanceName}";
        }
        else
        {
            return $"Performance ended: {performanceName}";
        }
    }

    /// <summary>
    /// Tick down performance duration at end of turn
    /// </summary>
    public string TickPerformance(PlayerCharacter player)
    {
        if (!player.IsPerforming)
        {
            return string.Empty;
        }

        player.PerformingTurnsRemaining--;

        if (player.PerformingTurnsRemaining <= 0)
        {
            // Performance ended naturally
            string message = EndPerformance(player, forced: false);

            // Handle special end-of-performance effects
            if (player.CurrentPerformance == "Saga of the Einherjar")
            {
                // Saga of the Einherjar: 10 Stress on end
                player.PsychicStress = Math.Min(100, player.PsychicStress + 10);
                message += "\nYou suffer 10 Psychic Stress from the exertion!";
            }

            return message;
        }

        return $"{player.CurrentPerformance}: {player.PerformingTurnsRemaining} rounds remaining";
    }

    /// <summary>
    /// Check if performance should be interrupted
    /// </summary>
    public bool ShouldInterruptPerformance(PlayerCharacter player)
    {
        if (!player.IsPerforming)
            return false;

        // Interrupted by [Silenced]
        if (player.SilencedTurnsRemaining > 0)
            return true;

        // Future: Add [Stunned] check when that status is implemented
        // if (player.StunnedTurnsRemaining > 0)
        //     return true;

        return false;
    }

    /// <summary>
    /// Handle performance interruption
    /// </summary>
    public string HandleInterruption(PlayerCharacter player)
    {
        if (ShouldInterruptPerformance(player))
        {
            return EndPerformance(player, forced: true);
        }
        return string.Empty;
    }

    /// <summary>
    /// Check if player can take actions while performing
    /// </summary>
    public bool CanTakeAction(PlayerCharacter player)
    {
        // While performing, player is locked into the performance
        // They can only:
        // 1. Maintain the performance (automatic)
        // 2. End the performance voluntarily
        // Cannot: Attack, use abilities, use items, defend, etc.
        return !player.IsPerforming;
    }

    /// <summary>
    /// Get display string for current performance
    /// </summary>
    public string GetPerformanceStatusDisplay(PlayerCharacter player)
    {
        if (!player.IsPerforming)
            return string.Empty;

        return $"[PERFORMING] {player.CurrentPerformance} ({player.PerformingTurnsRemaining} rounds)";
    }

    /// <summary>
    /// Clean up performance-specific effects when ended
    /// </summary>
    private void CleanupPerformanceEffects(PlayerCharacter player, string performanceName)
    {
        switch (performanceName)
        {
            case "Saga of Courage":
                // Bonus to ally morale - no cleanup needed (passive buff while active)
                break;

            case "Dirge of Defeat":
                // Enemy debuff - no cleanup needed (debuff on enemies, not player)
                break;

            case "Lay of the Iron Wall":
                // +2 Soak to party - no cleanup needed (applied to party, not player)
                break;

            case "Saga of the Einherjar":
                // [Inspired] status is separate from performance
                // Temp HP granted remains until damaged
                // Stress cost applied on end (handled in TickPerformance)
                break;
        }
    }

    /// <summary>
    /// Get list of all performance ability names
    /// </summary>
    public static List<string> GetPerformanceAbilities()
    {
        return new List<string>
        {
            "Saga of Courage",
            "Dirge of Defeat",
            "Lay of the Iron Wall",
            "Saga of the Einherjar"
        };
    }

    /// <summary>
    /// Check if an ability is a performance
    /// </summary>
    public static bool IsPerformanceAbility(string abilityName)
    {
        return GetPerformanceAbilities().Contains(abilityName);
    }
}
