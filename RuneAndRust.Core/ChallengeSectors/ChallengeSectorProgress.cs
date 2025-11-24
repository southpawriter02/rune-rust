namespace RuneAndRust.Core.ChallengeSectors;

/// <summary>
/// v0.40.2: Overall progress tracking for Challenge Sectors
/// Tracks account-wide completion statistics
/// </summary>
public class ChallengeSectorProgress
{
    /// <summary>Character ID this progress belongs to</summary>
    public int CharacterId { get; set; }

    // ═══════════════════════════════════════════════════════════
    // OVERALL PROGRESS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Number of sectors completed</summary>
    public int TotalSectorsCompleted { get; set; } = 0;

    /// <summary>Total number of sectors available</summary>
    public int TotalSectorsAvailable { get; set; } = 0;

    /// <summary>Total number of attempts across all sectors</summary>
    public int TotalAttempts { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // FASTEST COMPLETIONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Sector ID of the fastest completion</summary>
    public string? FastestSectorId { get; set; }

    /// <summary>Fastest completion time (seconds)</summary>
    public int? FastestCompletionTime { get; set; }

    // ═══════════════════════════════════════════════════════════
    // STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total time spent in challenges (seconds)</summary>
    public int TotalChallengeTimeSeconds { get; set; } = 0;

    /// <summary>Total deaths across all challenge attempts</summary>
    public int TotalDeathsInChallenges { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // ACHIEVEMENTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Have all sectors been completed?</summary>
    public bool AllSectorsCompleted { get; set; } = false;

    /// <summary>Number of perfect runs (no deaths)</summary>
    public int PerfectRunCount { get; set; } = 0;

    /// <summary>Last updated timestamp</summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Completion percentage (0.0 to 1.0)</summary>
    public float CompletionPercentage => TotalSectorsAvailable > 0
        ? (float)TotalSectorsCompleted / TotalSectorsAvailable
        : 0.0f;

    /// <summary>Completion percentage as formatted string</summary>
    public string CompletionPercentageFormatted => $"{(CompletionPercentage * 100):F1}%";

    /// <summary>Average completion time (seconds)</summary>
    public int? AverageCompletionTime => TotalSectorsCompleted > 0
        ? TotalChallengeTimeSeconds / TotalSectorsCompleted
        : null;

    /// <summary>Success rate (completions / attempts)</summary>
    public float SuccessRate => TotalAttempts > 0
        ? (float)TotalSectorsCompleted / TotalAttempts
        : 0.0f;

    /// <summary>Success rate as formatted string</summary>
    public string SuccessRateFormatted => $"{(SuccessRate * 100):F1}%";
}
