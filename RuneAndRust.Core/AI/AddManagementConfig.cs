using System.Collections.Generic;

namespace RuneAndRust.Core.AI;

/// <summary>
/// Configuration for boss add management.
/// Defines when and how a boss summons adds.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class AddManagementConfig
{
    /// <summary>
    /// Database ID.
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// Boss type ID.
    /// </summary>
    public int BossTypeId { get; set; }

    /// <summary>
    /// Phase this configuration applies to.
    /// </summary>
    public BossPhase Phase { get; set; }

    /// <summary>
    /// Types and counts of adds to summon.
    /// </summary>
    public List<AddType> AddTypes { get; set; } = new();

    /// <summary>
    /// Maximum number of adds that can exist simultaneously.
    /// </summary>
    public int MaxSimultaneousAdds { get; set; }

    /// <summary>
    /// Summon adds every N turns (time-based trigger).
    /// Null = no time-based summoning.
    /// </summary>
    public int? SummonEveryNTurns { get; set; }

    /// <summary>
    /// HP thresholds that trigger add summoning.
    /// </summary>
    public List<decimal> SummonAtHPThresholds { get; set; } = new();

    /// <summary>
    /// Whether to resummon adds if all are killed.
    /// </summary>
    public bool ResummonIfAllAddsDie { get; set; }

    /// <summary>
    /// Dialogue displayed when summoning adds.
    /// </summary>
    public string? SummonDialogue { get; set; }

    /// <summary>
    /// HP multiplier for summoned adds (0.5 = 50% HP).
    /// </summary>
    public decimal AddHPMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Damage multiplier for summoned adds (0.8 = 80% damage).
    /// </summary>
    public decimal AddDamageMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines a type of add to summon.
/// </summary>
public class AddType
{
    /// <summary>
    /// Enemy type ID to summon.
    /// </summary>
    public int EnemyTypeId { get; set; }

    /// <summary>
    /// Enemy type name.
    /// </summary>
    public string EnemyTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Number of this enemy type to summon.
    /// </summary>
    public int Count { get; set; }
}
