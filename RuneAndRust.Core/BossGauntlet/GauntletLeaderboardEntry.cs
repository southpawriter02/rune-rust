namespace RuneAndRust.Core.BossGauntlet;

/// <summary>
/// v0.40.3: Boss Gauntlet Leaderboard Entry
/// Tracks best performances across different competitive categories
/// </summary>
public class GauntletLeaderboardEntry
{
    /// <summary>Unique entry ID</summary>
    public int EntryId { get; set; }

    /// <summary>Gauntlet sequence ID</summary>
    public string SequenceId { get; set; } = string.Empty;

    /// <summary>Leaderboard category</summary>
    public GauntletLeaderboardCategory Category { get; set; }

    // ═══════════════════════════════════════════════════════════
    // RUN REFERENCE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Run ID that achieved this score</summary>
    public int RunId { get; set; }

    /// <summary>Character ID</summary>
    public int CharacterId { get; set; }

    /// <summary>Character name for display</summary>
    public string CharacterName { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // PERFORMANCE METRICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total completion time (seconds)</summary>
    public int TotalTimeSeconds { get; set; }

    /// <summary>Total deaths</summary>
    public int TotalDeaths { get; set; }

    /// <summary>Heals used</summary>
    public int HealsUsed { get; set; }

    /// <summary>NG+ tier context</summary>
    public int NgPlusTier { get; set; }

    // ═══════════════════════════════════════════════════════════
    // RANKING
    // ═══════════════════════════════════════════════════════════

    /// <summary>Rank in this category (1 = best)</summary>
    public int? Rank { get; set; }

    // ═══════════════════════════════════════════════════════════
    // TIMESTAMP
    // ═══════════════════════════════════════════════════════════

    /// <summary>When this achievement was recorded</summary>
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Completion time in minutes</summary>
    public float CompletionMinutes => TotalTimeSeconds / 60.0f;

    /// <summary>Is this a top 10 entry?</summary>
    public bool IsTopTen => Rank.HasValue && Rank.Value <= 10;

    /// <summary>Is this a top 3 entry?</summary>
    public bool IsTopThree => Rank.HasValue && Rank.Value <= 3;

    /// <summary>Is this #1?</summary>
    public bool IsFirst => Rank == 1;

    /// <summary>Category display name</summary>
    public string CategoryDisplay => Category switch
    {
        GauntletLeaderboardCategory.Fastest => "Fastest Time",
        GauntletLeaderboardCategory.Flawless => "Flawless (No Deaths)",
        GauntletLeaderboardCategory.NoHeal => "No Heal Challenge",
        GauntletLeaderboardCategory.NGPlus => "NG+ Elite",
        _ => "Unknown"
    };

    /// <summary>Formatted time display</summary>
    public string TimeDisplay
    {
        get
        {
            var minutes = TotalTimeSeconds / 60;
            var seconds = TotalTimeSeconds % 60;
            return $"{minutes}:{seconds:D2}";
        }
    }

    /// <summary>Summary display text</summary>
    public string SummaryDisplay => $"#{Rank} - {CharacterName} ({TimeDisplay})";
}

/// <summary>
/// Leaderboard categories
/// </summary>
public enum GauntletLeaderboardCategory
{
    /// <summary>Fastest completion time</summary>
    Fastest,

    /// <summary>Flawless runs (no deaths)</summary>
    Flawless,

    /// <summary>No heal challenge (minimal resource use)</summary>
    NoHeal,

    /// <summary>NG+ tier leaderboard (highest NG+ completions)</summary>
    NGPlus
}
