// ==============================================================================
// v0.38.12: Advanced Combat Mechanics Descriptors
// DefensiveActionDescriptor.cs
// ==============================================================================
// Purpose: Defensive action descriptors for block, parry, dodge, and counter
// Pattern: Follows NPCPhysicalDescriptor structure from v0.38.11
// Integration: Used by CombatFlavorTextService to generate defensive action descriptions
// ==============================================================================

namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// Represents a defensive action descriptor for combat.
/// Describes blocking, parrying, dodging, and counter-attack actions with context-aware variations.
/// v0.38.12: Advanced Combat Mechanics Descriptors
/// </summary>
public class DefensiveActionDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Defensive action type.
    /// Values: Block, Parry, Dodge, Counter
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Defensive action outcome.
    /// Values: Success, Failure, CriticalSuccess, PartialSuccess
    /// </summary>
    public string OutcomeType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Weapon or equipment type used for defense (optional).
    /// Shield: LightShield, HeavyShield, TowerShield
    /// Weapon: OneHanded, TwoHanded, Dagger, Staff
    /// Body: Unarmed, NULL (any)
    /// </summary>
    public string? WeaponType { get; set; }

    /// <summary>
    /// Attack intensity being defended against (optional).
    /// Values: Light, Heavy, Overwhelming, NULL
    /// </summary>
    public string? AttackIntensity { get; set; }

    /// <summary>
    /// Environmental context (optional).
    /// Values: OpenGround, TightQuarters, Hazardous, Elevated, NULL
    /// </summary>
    public string? EnvironmentContext { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - ACTOR: {ActorName}, {WeaponName}, {ShieldName}
    /// - DAMAGE: {DamageBlocked}, {DamageReduced}, {DamageTaken}
    /// - EFFECT: {EffectApplied}, {DurationTurns}
    /// - OUTCOME: {CounterOpportunity}, {StaggerEffect}
    ///
    /// Example: "You raise your shield. The blow cracks against it harmlessly."
    /// Example: "Perfect parry! You not only deflect the attack but throw your opponent off-balance!"
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
    /// Example: ["Dramatic", "Skillful", "Desperate", "Opportunistic"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid defensive action types.
    /// </summary>
    public static class ActionTypes
    {
        public const string Block = "Block";
        public const string Parry = "Parry";
        public const string Dodge = "Dodge";
        public const string Counter = "Counter";
    }

    /// <summary>
    /// Valid outcome types for defensive actions.
    /// </summary>
    public static class OutcomeTypes
    {
        public const string Success = "Success";
        public const string Failure = "Failure";
        public const string CriticalSuccess = "CriticalSuccess";
        public const string PartialSuccess = "PartialSuccess";
        public const string WeaponDamaged = "WeaponDamaged";
        public const string ShieldBroken = "ShieldBroken";
    }

    /// <summary>
    /// Valid attack intensities.
    /// </summary>
    public static class AttackIntensities
    {
        public const string Light = "Light";
        public const string Heavy = "Heavy";
        public const string Overwhelming = "Overwhelming";
    }
}
