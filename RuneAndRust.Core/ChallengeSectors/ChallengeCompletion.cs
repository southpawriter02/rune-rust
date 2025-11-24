namespace RuneAndRust.Core.ChallengeSectors;

/// <summary>
/// v0.40.2: Record of a completed Challenge Sector
/// Tracks completion statistics and performance
/// </summary>
public class ChallengeCompletion
{
    /// <summary>Database ID for this completion record</summary>
    public int CompletionId { get; set; }

    /// <summary>Character ID who completed the challenge</summary>
    public int CharacterId { get; set; }

    /// <summary>Sector ID that was completed</summary>
    public string SectorId { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // COMPLETION METADATA
    // ═══════════════════════════════════════════════════════════

    /// <summary>When the sector was completed (UTC)</summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>How long the run took (seconds)</summary>
    public int? CompletionTimeSeconds { get; set; }

    /// <summary>Number of deaths during the run</summary>
    public int DeathsDuringRun { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // RUN STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total damage taken during the run</summary>
    public int DamageTaken { get; set; } = 0;

    /// <summary>Total damage dealt during the run</summary>
    public int DamageDealt { get; set; } = 0;

    /// <summary>Total enemies killed during the run</summary>
    public int EnemiesKilled { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // NG+ CONTEXT
    // ═══════════════════════════════════════════════════════════

    /// <summary>Which NG+ tier was active during completion</summary>
    public int NGPlusTier { get; set; } = 0;

    /// <summary>Was this the first time completing this sector?</summary>
    public bool IsFirstCompletion { get; set; } = false;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Human-readable completion time</summary>
    public TimeSpan? CompletionTime => CompletionTimeSeconds.HasValue
        ? TimeSpan.FromSeconds(CompletionTimeSeconds.Value)
        : null;

    /// <summary>Formatted completion time string (MM:SS)</summary>
    public string FormattedTime => CompletionTime.HasValue
        ? $"{CompletionTime.Value.Minutes:D2}:{CompletionTime.Value.Seconds:D2}"
        : "Unknown";

    /// <summary>Perfect run (no deaths)?</summary>
    public bool IsPerfectRun => DeathsDuringRun == 0;

    /// <summary>Performance rating (0-100)</summary>
    public int PerformanceRating
    {
        get
        {
            var rating = 100;

            // Deduct for deaths (10 points each)
            rating -= Math.Min(50, DeathsDuringRun * 10);

            // Deduct for excessive damage taken (relative to damage dealt)
            if (DamageDealt > 0)
            {
                var damageRatio = (float)DamageTaken / DamageDealt;
                if (damageRatio > 1.0f)
                {
                    rating -= Math.Min(30, (int)((damageRatio - 1.0f) * 20));
                }
            }

            return Math.Max(0, rating);
        }
    }
}
