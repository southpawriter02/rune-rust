namespace RuneAndRust.Core.ChallengeSectors;

/// <summary>
/// v0.40.2: Challenge Sector definition
/// Represents a handcrafted extreme difficulty challenge
/// </summary>
public class ChallengeSector
{
    /// <summary>Unique sector identifier (e.g., "iron_gauntlet", "the_silence_falls")</summary>
    public string SectorId { get; set; } = string.Empty;

    /// <summary>Display name (e.g., "Iron Gauntlet", "The Silence Falls")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the challenge</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Lore/narrative text</summary>
    public string? LoreText { get; set; }

    // ═══════════════════════════════════════════════════════════
    // MODIFIERS
    // ═══════════════════════════════════════════════════════════

    /// <summary>List of modifier IDs applied to this sector</summary>
    public List<string> ModifierIds { get; set; } = new();

    /// <summary>Total difficulty multiplier (product of all modifier multipliers)</summary>
    public float TotalDifficultyMultiplier { get; set; } = 1.0f;

    // ═══════════════════════════════════════════════════════════
    // GENERATION PARAMETERS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Biome theme for procedural generation</summary>
    public string BiomeTheme { get; set; } = "Midgard";

    /// <summary>Enemy types allowed to spawn (JSON array)</summary>
    public List<string> EnemyPool { get; set; } = new();

    /// <summary>Number of rooms in the sector</summary>
    public int RoomCount { get; set; } = 10;

    // ═══════════════════════════════════════════════════════════
    // REWARDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Unique legendary reward item ID</summary>
    public string? UniqueRewardId { get; set; }

    /// <summary>Display name of the unique reward</summary>
    public string? UniqueRewardName { get; set; }

    /// <summary>Description of the unique reward</summary>
    public string? UniqueRewardDescription { get; set; }

    // ═══════════════════════════════════════════════════════════
    // PROGRESSION
    // ═══════════════════════════════════════════════════════════

    /// <summary>Minimum NG+ tier required to attempt this sector (0-5)</summary>
    public int RequiredNGPlusTier { get; set; } = 0;

    /// <summary>Prerequisite sector IDs that must be completed first</summary>
    public List<string> PrerequisiteSectorIds { get; set; } = new();

    // ═══════════════════════════════════════════════════════════
    // COMPLETION STATE
    // ═══════════════════════════════════════════════════════════

    /// <summary>Is this sector active and available?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Has this sector been completed?</summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>Number of attempts made on this sector</summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>When this sector was first completed (UTC)</summary>
    public DateTime? FirstCompletionDate { get; set; }

    /// <summary>Sort order for display</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>Designer notes for balance/testing</summary>
    public string? DesignNotes { get; set; }

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>Difficulty tier description</summary>
    public string DifficultyTier
    {
        get
        {
            return TotalDifficultyMultiplier switch
            {
                < 2.0f => "Moderate",
                < 2.5f => "Hard",
                < 3.0f => "Extreme",
                < 3.5f => "Near-Impossible",
                _ => "Impossible"
            };
        }
    }

    /// <summary>Completion percentage string</summary>
    public string CompletionStatus => IsCompleted ? "✓ Completed" : $"Attempts: {AttemptCount}";
}
