namespace RuneAndRust.Core.NewGamePlus;

/// <summary>
/// v0.40.1: Record of a completed NG+ tier
/// Tracks completion history and statistics
/// </summary>
public class NGPlusCompletion
{
    /// <summary>Database ID for this completion record</summary>
    public int CompletionId { get; set; }

    /// <summary>Character ID who completed the tier</summary>
    public int CharacterId { get; set; }

    /// <summary>Which tier was completed (1-5)</summary>
    public int CompletedTier { get; set; }

    /// <summary>When the tier was completed (UTC)</summary>
    public DateTime CompletionTimestamp { get; set; }

    /// <summary>Total playtime for this run (seconds)</summary>
    public int? TotalPlaytimeSeconds { get; set; }

    /// <summary>Number of deaths during the run</summary>
    public int DeathsDuringRun { get; set; }

    /// <summary>Number of bosses defeated during the run</summary>
    public int BossesDefeated { get; set; }

    /// <summary>Human-readable playtime</summary>
    public TimeSpan? Playtime => TotalPlaytimeSeconds.HasValue
        ? TimeSpan.FromSeconds(TotalPlaytimeSeconds.Value)
        : null;

    /// <summary>Formatted playtime string (HH:MM:SS)</summary>
    public string FormattedPlaytime => Playtime.HasValue
        ? $"{Playtime.Value.Hours:D2}:{Playtime.Value.Minutes:D2}:{Playtime.Value.Seconds:D2}"
        : "Unknown";
}
