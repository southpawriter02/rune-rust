// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrCasterVoiceProfile.cs
// ==============================================================================
// Purpose: Mystic personality profiles (like Enemy_Voice_Profiles)
// Usage: Defines casting style, personality, and preferred descriptors
// Integration: Player specializations (VardWarden, RustWitch) + NPC mages
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents a caster's personality, style, and preferred descriptor sets.
/// Similar to EnemyVoiceProfile but for magical characters.
/// </summary>
public class GaldrCasterVoiceProfile
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this voice profile.
    /// </summary>
    public int VoiceId { get; set; }

    /// <summary>
    /// Caster archetype identifier.
    /// Values: VardWarden, RustWitch, Völva, Seiðkona, Seiðmaðr,
    ///         CorruptedMage, BossMage, Elder_Runesmith, etc.
    /// </summary>
    public string CasterArchetype { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the voice profile.
    /// Example: "Vard-Warden's Reverent Chanting", "Rust-Witch's Heretical Whispers"
    /// </summary>
    public string VoiceName { get; set; } = string.Empty;

    // ==================== PERSONALITY ====================

    /// <summary>
    /// Personality description.
    /// Examples:
    /// - VardWarden: "Reverent and methodical, invoking divine order through sacred wards."
    /// - RustWitch: "Desperate and heretical, whispering forbidden entropy to rust and decay."
    /// - Völva: "Ancient and authoritative, channeling prophetic visions through runes."
    /// </summary>
    public string VoiceDescription { get; set; } = string.Empty;

    /// <summary>
    /// Casting style description.
    /// Examples:
    /// - VardWarden: "Formal runic incantations, slow and deliberate, with ritual gestures."
    /// - RustWitch: "Broken whispered chants, feverish and erratic, with self-inflicted corruption."
    /// - Seiðkona: "Sung Galdr in melodic verses, weaving reality with voice and will."
    /// </summary>
    public string CastingStyle { get; set; } = string.Empty;

    // ==================== LORE CONTEXT ====================

    /// <summary>
    /// Background lore for this caster archetype.
    /// Example: "Vard-Wardens are defensive mystics who specialize in Tiwaz runes,
    ///          creating barriers and sanctified ground to protect against the Blight."
    /// </summary>
    public string? SettingContext { get; set; }

    // ==================== DESCRIPTOR ASSOCIATIONS ====================

    /// <summary>
    /// JSON array of invocation descriptor IDs (Galdr_Action_Descriptors).
    /// These descriptors are preferred when this caster invokes spells.
    /// Example: "[12, 15, 18, 23, 27]"
    /// </summary>
    public string? InvocationDescriptors { get; set; }

    /// <summary>
    /// JSON array of manifestation descriptor IDs (Galdr_Manifestation_Descriptors).
    /// These descriptors describe how this caster's magic appears.
    /// Example: "[3, 7, 9, 14]"
    /// </summary>
    public string? ManifestationDescriptors { get; set; }

    /// <summary>
    /// JSON array of miscast descriptor IDs (Galdr_Miscast_Descriptors).
    /// These descriptors are used when this caster's magic fails.
    /// Example: "[2, 5, 8]"
    /// </summary>
    public string? MiscastDescriptors { get; set; }

    // ==================== PREFERRED RUNE SCHOOLS ====================

    /// <summary>
    /// JSON array of preferred rune schools for this caster.
    /// Examples:
    /// - VardWarden: '["Tiwaz", "Berkanan"]' (Protection, Healing)
    /// - RustWitch: '["Naudiz", "Isa"]' (Draining, Stasis)
    /// - Völva: '["Ansuz", "Laguz"]' (Divination, Purification)
    /// </summary>
    public string? PreferredSchools { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Whether this voice profile is active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["PlayerSpecialization", "NPC", "Boss", "Corrupted"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Known caster archetypes.
    /// </summary>
    public static class CasterArchetypes
    {
        // Player Specializations
        public const string VardWarden = "VardWarden";
        public const string RustWitch = "RustWitch";

        // NPC Archetypes
        public const string Völva = "Völva";                   // Seer/Prophet
        public const string Seiðkona = "Seiðkona";             // Female practitioner
        public const string Seiðmaðr = "Seiðmaðr";             // Male practitioner
        public const string ElderRunesmith = "Elder_Runesmith"; // Ancient craftsman-mage
        public const string CorruptedMage = "CorruptedMage";   // Blight-corrupted caster
        public const string BossMage = "BossMage";             // Boss-tier spellcaster
    }
}
