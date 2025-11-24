namespace RuneAndRust.Core.BossGauntlet;

/// <summary>
/// v0.40.3: Individual boss encounter within a gauntlet run
/// Tracks performance, resource usage, and outcome for a single boss fight
/// </summary>
public class GauntletBossEncounter
{
    /// <summary>Unique encounter ID</summary>
    public int EncounterId { get; set; }

    /// <summary>Parent gauntlet run ID</summary>
    public int RunId { get; set; }

    /// <summary>Boss number in sequence (0-based)</summary>
    public int BossIndex { get; set; }

    /// <summary>Boss identifier from v0.23 system</summary>
    public string BossId { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // OUTCOME
    // ═══════════════════════════════════════════════════════════

    /// <summary>Fight result: victory or defeat</summary>
    public GauntletEncounterResult Result { get; set; }

    // ═══════════════════════════════════════════════════════════
    // STATISTICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Time to complete this boss (seconds)</summary>
    public int? CompletionTimeSeconds { get; set; }

    /// <summary>Damage taken during this fight</summary>
    public int DamageTaken { get; set; } = 0;

    /// <summary>Damage dealt to this boss</summary>
    public int DamageDealt { get; set; } = 0;

    /// <summary>Deaths during this fight</summary>
    public int Deaths { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // RESOURCE USAGE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Full heals used in this fight</summary>
    public int HealsUsed { get; set; } = 0;

    /// <summary>Did we revive in this fight?</summary>
    public bool ReviveUsed { get; set; } = false;

    // ═══════════════════════════════════════════════════════════
    // CHARACTER STATE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Character state after fight (JSON: HP, corruption, etc.)</summary>
    public string? EndingCharacterState { get; set; }

    // ═══════════════════════════════════════════════════════════
    // TIMESTAMP
    // ═══════════════════════════════════════════════════════════

    /// <summary>When this fight was completed</summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Was this fight won?</summary>
    public bool IsVictory => Result == GauntletEncounterResult.Victory;

    /// <summary>Was this fight lost?</summary>
    public bool IsDefeat => Result == GauntletEncounterResult.Defeat;

    /// <summary>Was this a perfect fight (no deaths)?</summary>
    public bool IsPerfect => Deaths == 0 && IsVictory;

    /// <summary>Did player use resources in this fight?</summary>
    public bool UsedResources => HealsUsed > 0 || ReviveUsed;

    /// <summary>Completion time in minutes</summary>
    public float? CompletionMinutes => CompletionTimeSeconds.HasValue
        ? CompletionTimeSeconds.Value / 60.0f
        : null;

    /// <summary>Result as display string</summary>
    public string ResultDisplay => Result switch
    {
        GauntletEncounterResult.Victory => "Victory",
        GauntletEncounterResult.Defeat => "Defeat",
        _ => "Unknown"
    };

    /// <summary>Boss number display (1-based for users)</summary>
    public int BossNumber => BossIndex + 1;
}

/// <summary>
/// Boss encounter result
/// </summary>
public enum GauntletEncounterResult
{
    Victory,
    Defeat
}
