// ==============================================================================
// v0.38.12: Advanced Combat Mechanics Descriptors
// FumbleDescriptor.cs
// ==============================================================================
// Purpose: Fumble and critical failure descriptors for catastrophic combat outcomes
// Pattern: Follows NPCPhysicalDescriptor structure from v0.38.11
// Integration: Used by CombatFlavorTextService to generate fumble descriptions
// ==============================================================================

namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// Represents a fumble descriptor for combat.
/// Describes catastrophic failures in attacks, magic, and defense.
/// v0.38.12: Advanced Combat Mechanics Descriptors
/// </summary>
public class FumbleDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Fumble category.
    /// Values: AttackFumble, MagicFumble, DefensiveFumble
    /// </summary>
    public string FumbleCategory { get; set; } = string.Empty;

    /// <summary>
    /// Fumble type within category.
    /// AttackFumble: Miss, WeaponDrop, SelfInjury, WeaponBreak
    /// MagicFumble: Miscast, Backfire, WildSurge, Burnout
    /// DefensiveFumble: ShieldDrop, Disarmed, Tripped, Exposed
    /// </summary>
    public string FumbleType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Weapon or equipment type involved (optional).
    /// Values: Sword, Axe, Hammer, Bow, Shield, Staff, NULL
    /// </summary>
    public string? EquipmentType { get; set; }

    /// <summary>
    /// Severity of fumble (optional).
    /// Values: Minor, Moderate, Severe, Catastrophic, NULL
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Environmental factor (optional).
    /// Values: Slippery, Unstable, Hazardous, Cramped, NULL
    /// </summary>
    public string? EnvironmentFactor { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - ACTOR: {ActorName}, {WeaponName}, {SpellName}
    /// - CONSEQUENCE: {DamageTaken}, {ItemLost}, {EffectApplied}
    /// - ENVIRONMENT: {EnvironmentFactor}, {HazardTriggered}
    /// - DURATION: {TurnsLost}, {EffectDuration}
    ///
    /// Example: "Your swing goes wide—you overextend and stumble!"
    /// Example: "Your Galdr falters—the Blight warps the spell! Paradoxical energy erupts!"
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// Higher values increase selection chance.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization and filtering.
    /// Example: ["Embarrassing", "Dangerous", "Costly", "Recoverable"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid fumble categories.
    /// </summary>
    public static class FumbleCategories
    {
        public const string AttackFumble = "AttackFumble";
        public const string MagicFumble = "MagicFumble";
        public const string DefensiveFumble = "DefensiveFumble";
    }

    /// <summary>
    /// Valid attack fumble types.
    /// </summary>
    public static class AttackFumbleTypes
    {
        public const string Miss = "Miss";
        public const string WeaponDrop = "WeaponDrop";
        public const string SelfInjury = "SelfInjury";
        public const string WeaponBreak = "WeaponBreak";
        public const string Overextension = "Overextension";
    }

    /// <summary>
    /// Valid magic fumble types.
    /// </summary>
    public static class MagicFumbleTypes
    {
        public const string Miscast = "Miscast";
        public const string Backfire = "Backfire";
        public const string WildSurge = "WildSurge";
        public const string Burnout = "Burnout";
        public const string CorruptionSurge = "CorruptionSurge";
    }

    /// <summary>
    /// Valid defensive fumble types.
    /// </summary>
    public static class DefensiveFumbleTypes
    {
        public const string ShieldDrop = "ShieldDrop";
        public const string Disarmed = "Disarmed";
        public const string Tripped = "Tripped";
        public const string Exposed = "Exposed";
        public const string Stumbled = "Stumbled";
    }

    /// <summary>
    /// Valid severity levels.
    /// </summary>
    public static class Severities
    {
        public const string Minor = "Minor";
        public const string Moderate = "Moderate";
        public const string Severe = "Severe";
        public const string Catastrophic = "Catastrophic";
    }
}
