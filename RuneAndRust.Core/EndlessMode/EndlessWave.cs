namespace RuneAndRust.Core.EndlessMode;

/// <summary>
/// v0.40.4: Individual wave within an endless mode run
/// Tracks wave composition and performance
/// </summary>
public class EndlessWave
{
    /// <summary>Unique wave ID</summary>
    public int WaveId { get; set; }

    /// <summary>Parent run ID</summary>
    public int RunId { get; set; }

    /// <summary>Wave number (1-indexed)</summary>
    public int WaveNumber { get; set; }

    // ═══════════════════════════════════════════════════════════
    // WAVE COMPOSITION
    // ═══════════════════════════════════════════════════════════

    /// <summary>Number of enemies in this wave</summary>
    public int EnemyCount { get; set; }

    /// <summary>Enemy level for this wave</summary>
    public int EnemyLevel { get; set; }

    /// <summary>Difficulty multiplier for this wave</summary>
    public float DifficultyMultiplier { get; set; }

    /// <summary>Is this a boss wave?</summary>
    public bool IsBossWave { get; set; } = false;

    // ═══════════════════════════════════════════════════════════
    // WAVE PERFORMANCE
    // ═══════════════════════════════════════════════════════════

    /// <summary>When this wave started</summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>When this wave ended</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Wave completion time in seconds</summary>
    public int? WaveTimeSeconds { get; set; }

    /// <summary>Enemies killed in this wave</summary>
    public int EnemiesKilled { get; set; } = 0;

    /// <summary>Damage taken during this wave</summary>
    public int DamageTaken { get; set; } = 0;

    /// <summary>Damage dealt during this wave</summary>
    public int DamageDealt { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Is this wave complete?</summary>
    public bool IsComplete => EndTime.HasValue;

    /// <summary>Wave duration</summary>
    public TimeSpan? Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : null;

    /// <summary>Wave duration in minutes</summary>
    public float? DurationMinutes => Duration?.TotalMinutes;

    /// <summary>Did player kill all enemies?</summary>
    public bool AllEnemiesKilled => EnemiesKilled >= EnemyCount;

    /// <summary>Damage per second dealt</summary>
    public float DPS => WaveTimeSeconds.HasValue && WaveTimeSeconds.Value > 0
        ? (float)DamageDealt / WaveTimeSeconds.Value
        : 0;

    /// <summary>Wave display text</summary>
    public string DisplayText => IsBossWave
        ? $"Wave {WaveNumber} (BOSS) - {EnemyCount} enemies, Level {EnemyLevel}"
        : $"Wave {WaveNumber} - {EnemyCount} enemies, Level {EnemyLevel}";
}
