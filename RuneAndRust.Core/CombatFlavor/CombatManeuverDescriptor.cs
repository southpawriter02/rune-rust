// ==============================================================================
// v0.38.12: Advanced Combat Mechanics Descriptors
// CombatManeuverDescriptor.cs
// ==============================================================================
// Purpose: Special combat maneuver descriptors (riposte, disarm, trip, grapple)
// Pattern: Follows NPCPhysicalDescriptor structure from v0.38.11
// Integration: Used by CombatFlavorTextService to generate special maneuver descriptions
// ==============================================================================

namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// Represents a special combat maneuver descriptor.
/// Describes tactical combat maneuvers like riposte, disarm, trip, and grapple.
/// v0.38.12: Advanced Combat Mechanics Descriptors
/// </summary>
public class CombatManeuverDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Maneuver type.
    /// Values: Riposte, Disarm, Trip, Grapple, Shove, Feint
    /// </summary>
    public string ManeuverType { get; set; } = string.Empty;

    /// <summary>
    /// Maneuver outcome.
    /// Values: Success, Failure, CriticalSuccess
    /// </summary>
    public string OutcomeType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Weapon or technique used (optional).
    /// Values: Sword, Dagger, Unarmed, Shield, Polearm, NULL
    /// </summary>
    public string? WeaponType { get; set; }

    /// <summary>
    /// Target type (optional).
    /// Values: Humanoid, Beast, Large, Small, NULL
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// Environmental context (optional).
    /// Values: OpenGround, TightQuarters, Slippery, Elevated, NULL
    /// </summary>
    public string? EnvironmentContext { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - ACTOR: {AttackerName}, {WeaponName}
    /// - TARGET: {TargetName}, {TargetWeapon}
    /// - EFFECT: {EffectApplied}, {StatusInflicted}, {AdvantageGained}
    /// - FOLLOWUP: {CounterOpportunity}, {BonusAction}
    ///
    /// Example: "You parry and immediately counter-strike! Your blade finds its mark!"
    /// Example: "You seize them in a hold! They struggle but can't break free!"
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
    /// Example: ["Tactical", "Skillful", "Opportunistic", "Defensive"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid maneuver types.
    /// </summary>
    public static class ManeuverTypes
    {
        public const string Riposte = "Riposte";
        public const string Disarm = "Disarm";
        public const string Trip = "Trip";
        public const string Grapple = "Grapple";
        public const string Shove = "Shove";
        public const string Feint = "Feint";
    }

    /// <summary>
    /// Valid outcome types.
    /// </summary>
    public static class OutcomeTypes
    {
        public const string Success = "Success";
        public const string Failure = "Failure";
        public const string CriticalSuccess = "CriticalSuccess";
    }

    /// <summary>
    /// Valid target types.
    /// </summary>
    public static class TargetTypes
    {
        public const string Humanoid = "Humanoid";
        public const string Beast = "Beast";
        public const string Large = "Large";
        public const string Small = "Small";
        public const string Construct = "Construct";
    }
}
