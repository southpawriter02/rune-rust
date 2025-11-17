// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrMiscastDescriptor.cs
// ==============================================================================
// Purpose: Paradox, Blight interference, and magical failure narratives
// Lore: Runic Blight corrupts Galdr, causing paradoxical effects
// Integration: Triggered on failed casting checks or critical failures
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents a magical miscast, paradox, or Blight corruption event.
/// Describes what happens when Galdr fails or is corrupted by the Runic Blight.
/// </summary>
public class GaldrMiscastDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this miscast descriptor.
    /// </summary>
    public int MiscastId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Type of miscast event.
    /// Values: BlightCorruption, Paradox, Backlash, Fizzle,
    ///         WildMagic, AlfheimDistortion, RunicInversion
    /// </summary>
    public string MiscastType { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the miscast.
    /// Values: Minor, Moderate, Severe, Catastrophic
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    // ==================== RUNE SCHOOL ====================

    /// <summary>
    /// Which rune school was corrupted (NULL = any rune).
    /// Values: Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
    ///         Tiwaz, Berkanan, Mannaz, Laguz, NULL
    /// </summary>
    public string? RuneSchool { get; set; }

    /// <summary>
    /// Specific ability that misfired (NULL = any ability).
    /// Values: FlameBolt, FrostLance, HealingChant, etc., NULL
    /// </summary>
    public string? AbilityName { get; set; }

    // ==================== CORRUPTION SOURCE ====================

    /// <summary>
    /// Source of the magical corruption.
    /// Values: RunicBlight, AlfheimCursedChoir, AethericOverload, NULL
    /// </summary>
    public string? CorruptionSource { get; set; }

    /// <summary>
    /// Biome where miscast occurred (biome-specific effects).
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== EFFECT DESCRIPTION ====================

    /// <summary>
    /// Narrative description of the miscast event.
    ///
    /// Examples:
    /// - BlightCorruption: "Your chant falters—the Blight warps Fehu's meaning!
    ///                     Fire burns backward, cold as ice, wrong!"
    /// - Paradox: "The rune you summon isn't {Rune}—it's something else,
    ///            something that shouldn't exist. Paradoxical flame erupts!"
    /// - AlfheimDistortion: "The Cursed Choir harmonizes with your Galdr—or does
    ///                      it corrupt it? Reality rebels against your magic!"
    /// - Backlash: "The magic recoils! You stagger as {Element} lashes back
    ///             at you! [{Damage} damage]"
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    /// <summary>
    /// Mechanical effect of the miscast (JSON format).
    ///
    /// Examples:
    /// - {"damage": 5, "status": "Corrupted", "duration": 2}
    /// - {"damage": 10, "status": "Burning", "target": "Self"}
    /// - {"ap_loss": 5, "status": "Disoriented", "duration": 1}
    /// - {"random_effect": true, "severity": "moderate"}
    /// </summary>
    public string? MechanicalEffect { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["Horrifying", "Dramatic", "Minor", "Catastrophic"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid miscast types.
    /// </summary>
    public static class MiscastTypes
    {
        public const string BlightCorruption = "BlightCorruption";
        public const string Paradox = "Paradox";
        public const string Backlash = "Backlash";
        public const string Fizzle = "Fizzle";
        public const string WildMagic = "WildMagic";
        public const string AlfheimDistortion = "AlfheimDistortion";
        public const string RunicInversion = "RunicInversion";
    }

    /// <summary>
    /// Valid severity levels.
    /// </summary>
    public static class SeverityLevels
    {
        public const string Minor = "Minor";           // Cosmetic, no mechanical impact
        public const string Moderate = "Moderate";     // Minor damage/status effect
        public const string Severe = "Severe";         // Significant damage/debuff
        public const string Catastrophic = "Catastrophic"; // Critical failure, major consequences
    }

    /// <summary>
    /// Corruption sources.
    /// </summary>
    public static class CorruptionSources
    {
        public const string RunicBlight = "RunicBlight";
        public const string AlfheimCursedChoir = "AlfheimCursedChoir";
        public const string AethericOverload = "AethericOverload";
    }
}
