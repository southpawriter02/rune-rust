// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrActionDescriptor.cs
// ==============================================================================
// Purpose: Casting sequences, invocations, and chanting patterns
// Pattern: Follows CombatActionDescriptor structure
// Integration: Used by GaldrFlavorTextService to generate casting narratives
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents a Galdr casting action descriptor for generating flavor text.
/// Describes the act of casting magic (invocation, chanting, rune manifestation).
/// </summary>
public class GaldrActionDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Primary category of the action.
    /// Values: GaldrCasting, AbilityActivation, WeaponArt, TacticalAction, PassiveEffect
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Specific type of action within the category.
    /// Values: Invocation, Chant, RuneManifestation, Discharge, Aftermath, EffectTrigger, Activation
    /// </summary>
    public string? ActionType { get; set; }

    // ==================== GALDR SPECIFICS ====================

    /// <summary>
    /// Rune school associated with this casting.
    /// Values: Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
    ///         Tiwaz, Berkanan, Mannaz, Laguz, NULL (for non-Galdr)
    /// </summary>
    public string? RuneSchool { get; set; }

    /// <summary>
    /// Specific ability name (e.g., FlameBolt, FrostLance, HealingChant).
    /// NULL for generic descriptors that apply to multiple abilities.
    /// </summary>
    public string? AbilityName { get; set; }

    // ==================== SUCCESS LEVEL ====================

    /// <summary>
    /// Success level for outcome-based descriptors.
    /// Values: MinorSuccess (1-2 successes), SolidSuccess (3-4 successes),
    ///         ExceptionalSuccess (5+ successes), Miscast, NULL
    /// </summary>
    public string? SuccessLevel { get; set; }

    // ==================== NON-GALDR ABILITIES ====================

    /// <summary>
    /// Category for non-Galdr abilities (Warrior/Adept abilities).
    /// Values: WeaponArt, TacticalAbility, DefensiveStance, NULL
    /// </summary>
    public string? AbilityCategory { get; set; }

    /// <summary>
    /// Weapon type for weapon-based abilities.
    /// Values: TwoHanded, OneHanded, Bow, Crossbow, NULL
    /// </summary>
    public string? WeaponType { get; set; }

    // ==================== CONTEXT ====================

    /// <summary>
    /// Biome-specific casting descriptors.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL (any biome)
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - {Caster}, {Target}, {Enemy}, {Ally}
    /// - {Ability}, {Rune}, {RuneSymbol}, {Element}
    /// - {Weapon}, {WeaponType}
    /// - {Damage}, {SuccessCount}, {Healing}, {Duration}
    /// - {Target_Location}, {Vital_Location}, {Biome}, {Environment_Feature}
    /// - {RunicGlyph}, {MagicColor}, {SoundEffect}, {TactileEffect}
    /// - {BlightEffect}, {ParadoxManifestation}, {CorruptionLevel}
    ///
    /// Example: "You chant the {Rune} rune, its fiery syllables harsh on your tongue.
    ///           Flames sputter weakly from your palm, barely reaching the {Target}."
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
    /// Example: ["Verbose", "Concise", "Dramatic", "Technical"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid categories for Galdr actions.
    /// </summary>
    public static class Categories
    {
        public const string GaldrCasting = "GaldrCasting";
        public const string AbilityActivation = "AbilityActivation";
        public const string WeaponArt = "WeaponArt";
        public const string TacticalAction = "TacticalAction";
        public const string PassiveEffect = "PassiveEffect";
    }

    /// <summary>
    /// Valid action types within categories.
    /// </summary>
    public static class ActionTypes
    {
        public const string Invocation = "Invocation";
        public const string Chant = "Chant";
        public const string RuneManifestation = "RuneManifestation";
        public const string Discharge = "Discharge";
        public const string Aftermath = "Aftermath";
        public const string EffectTrigger = "EffectTrigger";
        public const string Activation = "Activation";
    }

    /// <summary>
    /// Valid success levels.
    /// </summary>
    public static class SuccessLevels
    {
        public const string MinorSuccess = "MinorSuccess";         // 1-2 successes
        public const string SolidSuccess = "SolidSuccess";         // 3-4 successes
        public const string ExceptionalSuccess = "ExceptionalSuccess"; // 5+ successes
        public const string Miscast = "Miscast";                   // Failed casting
    }

    /// <summary>
    /// Rune schools (Elder Futhark organized by Ættir).
    /// </summary>
    public static class RuneSchools
    {
        // Fehu's Ætt (Material World)
        public const string Fehu = "Fehu";           // Fire
        public const string Uruz = "Uruz";           // Strength
        public const string Thurisaz = "Thurisaz";   // Ice/Thorns
        public const string Ansuz = "Ansuz";         // Wind/Lightning
        public const string Raido = "Raido";         // Movement/Speed
        public const string Kenaz = "Kenaz";         // Knowledge/Vision

        // Hagalaz's Ætt (Chaos & Transformation)
        public const string Hagalaz = "Hagalaz";     // Destructive Ice
        public const string Naudiz = "Naudiz";       // Draining/Weakening
        public const string Isa = "Isa";             // Stasis/Freezing
        public const string Jera = "Jera";           // Time/Growth

        // Tiwaz's Ætt (Divine Order)
        public const string Tiwaz = "Tiwaz";         // Protective Wards
        public const string Berkanan = "Berkanan";   // Healing
        public const string Ehwaz = "Ehwaz";         // Partnership/Movement
        public const string Mannaz = "Mannaz";       // Enhancement
        public const string Laguz = "Laguz";         // Purification/Water
    }
}
