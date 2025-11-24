namespace RuneAndRust.Core.EndlessMode;

/// <summary>
/// v0.40.4: Endless Mode Leaderboard Entry
/// Competitive ranking for endless mode performance
/// </summary>
public class EndlessLeaderboardEntry
{
    /// <summary>Unique entry ID</summary>
    public int EntryId { get; set; }

    /// <summary>Run ID that achieved this score</summary>
    public int RunId { get; set; }

    /// <summary>Character ID</summary>
    public int CharacterId { get; set; }

    /// <summary>Player display name</summary>
    public string PlayerName { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // CATEGORY
    // ═══════════════════════════════════════════════════════════

    /// <summary>Leaderboard category</summary>
    public EndlessLeaderboardCategory Category { get; set; }

    // ═══════════════════════════════════════════════════════════
    // PERFORMANCE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Highest wave reached</summary>
    public int HighestWaveReached { get; set; }

    /// <summary>Total score achieved</summary>
    public int TotalScore { get; set; }

    /// <summary>Total run time (seconds)</summary>
    public int TotalTimeSeconds { get; set; }

    // ═══════════════════════════════════════════════════════════
    // METADATA
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character level</summary>
    public int? CharacterLevel { get; set; }

    /// <summary>Character specialization</summary>
    public string? SpecializationName { get; set; }

    /// <summary>Run seed</summary>
    public string Seed { get; set; } = string.Empty;

    /// <summary>Character build hash</summary>
    public string? CharacterBuildHash { get; set; }

    /// <summary>Is this entry verified?</summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>Community report count</summary>
    public int ReportCount { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // SEASONAL
    // ═══════════════════════════════════════════════════════════

    /// <summary>Season ID this entry belongs to</summary>
    public string? SeasonId { get; set; }

    /// <summary>When this entry was submitted</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════
    // RANKING
    // ═══════════════════════════════════════════════════════════

    /// <summary>Rank in leaderboard (1 = best)</summary>
    public int? Rank { get; set; }

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Run duration in minutes</summary>
    public float DurationMinutes => TotalTimeSeconds / 60f;

    /// <summary>Is this a top 10 entry?</summary>
    public bool IsTopTen => Rank.HasValue && Rank.Value <= 10;

    /// <summary>Is this a top 3 entry?</summary>
    public bool IsTopThree => Rank.HasValue && Rank.Value <= 3;

    /// <summary>Is this #1?</summary>
    public bool IsFirst => Rank == 1;

    /// <summary>Is this entry flagged?</summary>
    public bool IsFlagged => ReportCount > 0;

    /// <summary>Category display name</summary>
    public string CategoryDisplay => Category switch
    {
        EndlessLeaderboardCategory.HighestWave => "Highest Wave Reached",
        EndlessLeaderboardCategory.HighestScore => "Highest Total Score",
        _ => "Unknown"
    };

    /// <summary>Summary display text</summary>
    public string SummaryDisplay => Category == EndlessLeaderboardCategory.HighestWave
        ? $"#{Rank} - {PlayerName} (Wave {HighestWaveReached})"
        : $"#{Rank} - {PlayerName} ({TotalScore:N0} points)";

    /// <summary>Formatted time display</summary>
    public string TimeDisplay
    {
        get
        {
            var hours = TotalTimeSeconds / 3600;
            var minutes = (TotalTimeSeconds % 3600) / 60;
            var seconds = TotalTimeSeconds % 60;

            if (hours > 0)
                return $"{hours}h {minutes}m {seconds}s";
            else if (minutes > 0)
                return $"{minutes}m {seconds}s";
            else
                return $"{seconds}s";
        }
    }
}

/// <summary>
/// Endless mode leaderboard categories
/// </summary>
public enum EndlessLeaderboardCategory
{
    /// <summary>Highest wave reached</summary>
    HighestWave,

    /// <summary>Highest total score</summary>
    HighestScore
}
