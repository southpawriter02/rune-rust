// ==============================================================================
// v0.38.9: Perception & Examination Descriptors
// ExaminationLoreFragment.cs
// ==============================================================================
// Purpose: Lore nuggets revealed through expert examination
// Usage: Connects examination to world history, rewards thorough players
// ==============================================================================

namespace RuneAndRust.Core.ExaminationFlavor;

/// <summary>
/// Represents a lore fragment revealed through examination.
/// Expert-level examinations unlock deeper world history and context.
/// </summary>
public class ExaminationLoreFragment
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this lore fragment.
    /// </summary>
    public int LoreId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Category of lore.
    /// Values: HistoricalEvent, TechnicalKnowledge, CulturalArtifact,
    ///         BlightOrigin, PreBlightSociety, EvacuationRecord, ReligiousText
    /// </summary>
    public string LoreCategory { get; set; } = string.Empty;

    /// <summary>
    /// What object type reveals this lore.
    /// Example: "WallInscription", "AncientConsole", "Skeleton"
    /// NULL for general lore.
    /// </summary>
    public string? RelatedObjectType { get; set; }

    /// <summary>
    /// Required detail level to reveal this lore.
    /// Values: Cursory, Detailed, Expert
    /// </summary>
    public string? RequiredDetailLevel { get; set; }

    // ==================== BIOME ====================

    /// <summary>
    /// Where this lore is found.
    /// NULL for multiple locations.
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== LORE CONTENT ====================

    /// <summary>
    /// Short title for the lore entry.
    /// Example: "The Final Evacuation", "Jötun Engineering Protocols"
    /// </summary>
    public string LoreTitle { get; set; } = string.Empty;

    /// <summary>
    /// The actual lore content.
    /// Can be multi-paragraph for expert-level discoveries.
    /// </summary>
    public string LoreText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Whether this lore fragment is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["PreBlight", "Dvergr", "Evacuation"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid lore categories.
    /// </summary>
    public static class Categories
    {
        public const string HistoricalEvent = "HistoricalEvent";
        public const string TechnicalKnowledge = "TechnicalKnowledge";
        public const string CulturalArtifact = "CulturalArtifact";
        public const string BlightOrigin = "BlightOrigin";
        public const string PreBlightSociety = "PreBlightSociety";
        public const string EvacuationRecord = "EvacuationRecord";
        public const string ReligiousText = "ReligiousText";
    }
}
