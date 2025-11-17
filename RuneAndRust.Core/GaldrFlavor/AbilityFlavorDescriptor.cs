// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// AbilityFlavorDescriptor.cs
// ==============================================================================
// Purpose: Non-Galdr ability flavor (weapon arts, tactical abilities)
// Usage: Whirlwind Strike, Precision Strike, Sprint, Defensive Stance, etc.
// Integration: Warrior/Adept abilities that aren't magical
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents flavor text for non-magical abilities (weapon arts, tactics, etc.).
/// Used by Warriors, Adepts, and other non-Mystic characters.
/// </summary>
public class AbilityFlavorDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this ability flavor descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Category of the ability.
    /// Values: WeaponArt, TacticalAbility, DefensiveAbility,
    ///         PassiveAbility, ResourceAbility
    /// </summary>
    public string AbilityCategory { get; set; } = string.Empty;

    /// <summary>
    /// Specific ability name.
    /// Values: WhirlwindStrike, PrecisionStrike, Sprint, DefensiveStance,
    ///         PowerStrike, Cleave, Dodge, Parry, Rally, etc.
    /// </summary>
    public string AbilityName { get; set; } = string.Empty;

    // ==================== CONTEXT ====================

    /// <summary>
    /// Weapon type for weapon-based abilities (NULL for non-weapon abilities).
    /// Values: TwoHanded, OneHanded, Bow, Crossbow, Unarmed, NULL
    /// </summary>
    public string? WeaponType { get; set; }

    /// <summary>
    /// Specialization that uses this ability (NULL for general abilities).
    /// Values: SkarHordeAspirant, IronBane, AtgeirWielder, BoneSetter,
    ///         ScrapTinker, JotunReader, NULL
    /// </summary>
    public string? Specialization { get; set; }

    // ==================== SUCCESS LEVEL ====================

    /// <summary>
    /// Success level for outcome-based descriptors.
    /// Values: MinorSuccess, SolidSuccess, ExceptionalSuccess, Failure, NULL
    /// </summary>
    public string? SuccessLevel { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// Ability activation/resolution text with {Variable} placeholders.
    ///
    /// Examples:
    /// - WhirlwindStrike: "You spin, your {Weapon} becoming a devastating blur of steel!"
    /// - PrecisionStrike: "You wait for the perfect moment, then strike with surgical
    ///                    precision at the {Enemy}'s weak point!"
    /// - Sprint: "You dash across the battlefield with explosive speed!"
    /// - DefensiveStance: "You settle into a defensive posture, weapon ready, guard unbreakable."
    /// - Cleave: "Your {Weapon} carves through the {Enemy} and into another foe behind it!"
    /// - Parry: "You deflect the {Enemy}'s attack with a swift counter-motion!"
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

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
    /// Example: ["Dramatic", "Tactical", "Brutal", "Precise"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid ability categories.
    /// </summary>
    public static class AbilityCategories
    {
        public const string WeaponArt = "WeaponArt";
        public const string TacticalAbility = "TacticalAbility";
        public const string DefensiveAbility = "DefensiveAbility";
        public const string PassiveAbility = "PassiveAbility";
        public const string ResourceAbility = "ResourceAbility";
    }

    /// <summary>
    /// Common ability names (non-exhaustive).
    /// </summary>
    public static class AbilityNames
    {
        // Weapon Arts
        public const string WhirlwindStrike = "WhirlwindStrike";
        public const string PrecisionStrike = "PrecisionStrike";
        public const string PowerStrike = "PowerStrike";
        public const string Cleave = "Cleave";
        public const string ExecutionersStrike = "ExecutionersStrike";

        // Defensive Abilities
        public const string DefensiveStance = "DefensiveStance";
        public const string Parry = "Parry";
        public const string Dodge = "Dodge";
        public const string Brace = "Brace";

        // Tactical Abilities
        public const string Sprint = "Sprint";
        public const string Reposition = "Reposition";
        public const string Rally = "Rally";
        public const string Feint = "Feint";

        // Resource Abilities
        public const string SecondWind = "SecondWind";
        public const string BattleTrance = "BattleTrance";
        public const string FocusedBreathing = "FocusedBreathing";
    }

    /// <summary>
    /// Valid weapon types.
    /// </summary>
    public static class WeaponTypes
    {
        public const string TwoHanded = "TwoHanded";
        public const string OneHanded = "OneHanded";
        public const string Bow = "Bow";
        public const string Crossbow = "Crossbow";
        public const string Unarmed = "Unarmed";
    }

    /// <summary>
    /// Valid success levels.
    /// </summary>
    public static class SuccessLevels
    {
        public const string MinorSuccess = "MinorSuccess";
        public const string SolidSuccess = "SolidSuccess";
        public const string ExceptionalSuccess = "ExceptionalSuccess";
        public const string Failure = "Failure";
    }

    /// <summary>
    /// Warrior specializations.
    /// </summary>
    public static class Specializations
    {
        // Warriors
        public const string SkarHordeAspirant = "SkarHordeAspirant";
        public const string IronBane = "IronBane";
        public const string AtgeirWielder = "AtgeirWielder";

        // Adepts
        public const string BoneSetter = "BoneSetter";
        public const string ScrapTinker = "ScrapTinker";
        public const string JotunReader = "JotunReader";

        // Mystics (for reference, though they use Galdr)
        public const string VardWarden = "VardWarden";
        public const string RustWitch = "RustWitch";
    }
}
