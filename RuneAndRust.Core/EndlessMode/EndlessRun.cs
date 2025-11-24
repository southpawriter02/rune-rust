namespace RuneAndRust.Core.EndlessMode;

/// <summary>
/// v0.40.4: Endless Mode Run
/// Tracks an individual endless mode attempt with wave progression and scoring
/// </summary>
public class EndlessRun
{
    /// <summary>Unique run ID</summary>
    public int RunId { get; set; }

    /// <summary>Character ID</summary>
    public int CharacterId { get; set; }

    /// <summary>Run seed for reproducibility</summary>
    public string Seed { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // RUN STATE
    // ═══════════════════════════════════════════════════════════

    /// <summary>When the run started</summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>When the run ended</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Is this run currently active?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Highest wave reached in this run</summary>
    public int HighestWaveReached { get; set; } = 1;

    /// <summary>Current active wave number</summary>
    public int CurrentWave { get; set; } = 1;

    // ═══════════════════════════════════════════════════════════
    // COMBAT METRICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total enemies killed</summary>
    public int TotalEnemiesKilled { get; set; } = 0;

    /// <summary>Total bosses killed</summary>
    public int TotalBossesKilled { get; set; } = 0;

    /// <summary>Total damage taken</summary>
    public int TotalDamageTaken { get; set; } = 0;

    /// <summary>Total damage dealt</summary>
    public int TotalDamageDealt { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // PERFORMANCE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total run time in seconds</summary>
    public int TotalTimeSeconds { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // SCORING (calculated on run end)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Wave score: WavesCompleted × 1000</summary>
    public int WaveScore { get; set; } = 0;

    /// <summary>Kill score: EnemiesKilled × 50</summary>
    public int KillScore { get; set; } = 0;

    /// <summary>Boss score: BossesKilled × 500</summary>
    public int BossScore { get; set; } = 0;

    /// <summary>Time bonus: Max(0, 10000 - TotalTime)</summary>
    public int TimeBonus { get; set; } = 0;

    /// <summary>Survival bonus: Max(0, 5000 - DamageTaken)</summary>
    public int SurvivalBonus { get; set; } = 0;

    /// <summary>Total score: Sum of all score components</summary>
    public int TotalScore { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // VERIFICATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character build hash for anti-cheat</summary>
    public string? CharacterBuildHash { get; set; }

    /// <summary>Has this run been verified?</summary>
    public bool IsVerified { get; set; } = false;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Run duration</summary>
    public TimeSpan? Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : null;

    /// <summary>Run duration in minutes</summary>
    public float? DurationMinutes => Duration?.TotalMinutes;

    /// <summary>Is this run complete?</summary>
    public bool IsComplete => !IsActive && EndTime.HasValue;

    /// <summary>Average time per wave (seconds)</summary>
    public float AverageWaveTime => HighestWaveReached > 0
        ? (float)TotalTimeSeconds / HighestWaveReached
        : 0;

    /// <summary>Kill/death ratio</summary>
    public float KillsPerWave => HighestWaveReached > 0
        ? (float)TotalEnemiesKilled / HighestWaveReached
        : 0;

    /// <summary>Summary display text</summary>
    public string SummaryDisplay => $"Wave {HighestWaveReached} • Score: {TotalScore:N0} • {TotalEnemiesKilled} Kills";
}
