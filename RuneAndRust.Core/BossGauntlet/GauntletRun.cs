namespace RuneAndRust.Core.BossGauntlet;

/// <summary>
/// v0.40.3: Boss Gauntlet Run
/// Tracks an individual gauntlet attempt with resource management
/// </summary>
public class GauntletRun
{
    /// <summary>Unique run ID</summary>
    public int RunId { get; set; }

    /// <summary>Character ID</summary>
    public int CharacterId { get; set; }

    /// <summary>Gauntlet sequence ID</summary>
    public string SequenceId { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // RUN STATE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Current status: in_progress, victory, defeat</summary>
    public GauntletRunStatus Status { get; set; } = GauntletRunStatus.InProgress;

    /// <summary>Current boss index (0-based)</summary>
    public int CurrentBossIndex { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // RESOURCES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Full heals remaining</summary>
    public int FullHealsRemaining { get; set; }

    /// <summary>Revives remaining</summary>
    public int RevivesRemaining { get; set; }

    // ═══════════════════════════════════════════════════════════
    // STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Total run time in seconds</summary>
    public int? TotalTimeSeconds { get; set; }

    /// <summary>Total damage taken across all bosses</summary>
    public int TotalDamageTaken { get; set; } = 0;

    /// <summary>Total damage dealt to bosses</summary>
    public int TotalDamageDealt { get; set; } = 0;

    /// <summary>Total deaths (revives used)</summary>
    public int TotalDeaths { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // CHARACTER STATE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character state snapshot at gauntlet start (JSON)</summary>
    public string StartingCharacterState { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // NG+ CONTEXT
    // ═══════════════════════════════════════════════════════════

    /// <summary>NG+ tier when run started</summary>
    public int NgPlusTier { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // TIMESTAMPS
    // ═══════════════════════════════════════════════════════════

    /// <summary>When the run started</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the run completed (victory or defeat)</summary>
    public DateTime? CompletedAt { get; set; }

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Is the run still active?</summary>
    public bool IsActive => Status == GauntletRunStatus.InProgress;

    /// <summary>Was the run completed successfully?</summary>
    public bool IsVictory => Status == GauntletRunStatus.Victory;

    /// <summary>Did the run end in defeat?</summary>
    public bool IsDefeat => Status == GauntletRunStatus.Defeat;

    /// <summary>Is this a flawless run (no deaths)?</summary>
    public bool IsFlawless => TotalDeaths == 0 && IsVictory;

    /// <summary>Did player use no heals?</summary>
    public bool IsNoHeal => FullHealsRemaining > 0 && IsVictory;

    /// <summary>Run duration in minutes</summary>
    public float? DurationMinutes => TotalTimeSeconds.HasValue
        ? TotalTimeSeconds.Value / 60.0f
        : null;

    /// <summary>Status as display string</summary>
    public string StatusDisplay => Status switch
    {
        GauntletRunStatus.InProgress => "In Progress",
        GauntletRunStatus.Victory => "Victory",
        GauntletRunStatus.Defeat => "Defeat",
        _ => "Unknown"
    };
}

/// <summary>
/// Gauntlet run status
/// </summary>
public enum GauntletRunStatus
{
    InProgress,
    Victory,
    Defeat
}
