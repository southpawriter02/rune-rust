namespace RuneAndRust.Core.BossGauntlet;

/// <summary>
/// v0.40.3: Boss Gauntlet Sequence Definition
/// Defines the configuration for a gauntlet run (boss order, difficulty, rewards)
/// </summary>
public class GauntletSequence
{
    /// <summary>Unique sequence identifier</summary>
    public string SequenceId { get; set; } = string.Empty;

    /// <summary>Display name for the gauntlet</summary>
    public string SequenceName { get; set; } = string.Empty;

    /// <summary>Lore description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Difficulty tier: Moderate, Hard, Extreme, Nightmare</summary>
    public string DifficultyTier { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // BOSS CONFIGURATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>Number of bosses in sequence</summary>
    public int BossCount { get; set; }

    /// <summary>Ordered list of boss IDs from v0.23 boss system</summary>
    public List<string> BossIds { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // RESOURCE LIMITS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Maximum full heals allowed (default: 3)</summary>
    public int MaxFullHeals { get; set; } = 3;

    /// <summary>Maximum revives allowed (default: 1)</summary>
    public int MaxRevives { get; set; } = 1;

    // ═══════════════════════════════════════════════════════════
    // REQUIREMENTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Minimum NG+ tier required</summary>
    public int RequiredNGPlusTier { get; set; } = 0;

    /// <summary>Prerequisite gauntlet IDs that must be completed</summary>
    public List<string> PrerequisiteRuns { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // REWARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Legendary item reward ID</summary>
    public string? CompletionRewardId { get; set; }

    /// <summary>Prestige title granted</summary>
    public string? TitleReward { get; set; }

    // ═══════════════════════════════════════════════════════════
    // METADATA
    // ═══════════════════════════════════════════════════════════

    /// <summary>Is this gauntlet currently available?</summary>
    public bool Active { get; set; } = true;

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Is this an extreme difficulty gauntlet?</summary>
    public bool IsExtreme => DifficultyTier == "Extreme" || DifficultyTier == "Nightmare";

    /// <summary>Formatted display text</summary>
    public string DisplayText => $"{SequenceName} ({BossCount} Bosses • {DifficultyTier})";
}
