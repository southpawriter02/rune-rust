namespace RuneAndRust.Core.AI;

/// <summary>
/// Configuration for a boss encounter defining phases, adds, and AI behavior.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class BossConfiguration
{
    /// <summary>
    /// Boss type ID.
    /// </summary>
    public int BossTypeId { get; set; }

    /// <summary>
    /// Boss name.
    /// </summary>
    public string BossName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this boss uses phase transitions.
    /// </summary>
    public bool HasPhases { get; set; } = true;

    /// <summary>
    /// Number of phases (typically 3).
    /// </summary>
    public int PhaseCount { get; set; } = 3;

    /// <summary>
    /// Whether this boss summons adds.
    /// </summary>
    public bool UsesAdds { get; set; } = true;

    /// <summary>
    /// Whether this boss uses adaptive difficulty.
    /// </summary>
    public bool UsesAdaptiveDifficulty { get; set; } = true;

    /// <summary>
    /// Base aggression level (1-5).
    /// </summary>
    public int BaseAggressionLevel { get; set; } = 4;

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
