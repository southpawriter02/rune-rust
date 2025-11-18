// ==============================================================================
// v0.38.12: Advanced Combat Mechanics Descriptors
// CombatStanceDescriptor.cs
// ==============================================================================
// Purpose: Combat stance descriptors for tactical positioning
// Pattern: Follows NPCPhysicalDescriptor structure from v0.38.11
// Integration: Used by CombatFlavorTextService to generate stance change descriptions
// ==============================================================================

namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// Represents a combat stance descriptor.
/// Describes entering, maintaining, and switching between combat stances.
/// v0.38.12: Advanced Combat Mechanics Descriptors
/// </summary>
public class CombatStanceDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Stance type.
    /// Values: Aggressive, Defensive, Balanced, Reckless, Evasive
    /// </summary>
    public string StanceType { get; set; } = string.Empty;

    /// <summary>
    /// Description moment.
    /// Values: Entering, Maintaining, Switching
    /// </summary>
    public string DescriptionMoment { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Previous stance (for stance switching) (optional).
    /// Values: Aggressive, Defensive, Balanced, Reckless, Evasive, NULL
    /// </summary>
    public string? PreviousStance { get; set; }

    /// <summary>
    /// Combat situation context (optional).
    /// Values: Winning, Losing, EvenMatch, Outnumbered, Surrounded, NULL
    /// </summary>
    public string? SituationContext { get; set; }

    /// <summary>
    /// Weapon type influencing stance (optional).
    /// Values: OneHanded, TwoHanded, DualWield, ShieldAndWeapon, Unarmed, NULL
    /// </summary>
    public string? WeaponConfiguration { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - ACTOR: {ActorName}, {WeaponName}
    /// - STANCE: {StanceName}, {PreviousStance}
    /// - MODIFIERS: {AttackBonus}, {DefenseBonus}, {RiskLevel}
    /// - CONTEXT: {EnemyCount}, {AllyCount}, {Situation}
    ///
    /// Example: "You shift into an aggressive stance, weapon raised for maximum offense."
    /// Example: "You abandon caution, focusing entirely on attack."
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
    /// Example: ["Tactical", "Defensive", "Aggressive", "Risky"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid stance types.
    /// </summary>
    public static class StanceTypes
    {
        public const string Aggressive = "Aggressive";
        public const string Defensive = "Defensive";
        public const string Balanced = "Balanced";
        public const string Reckless = "Reckless";
        public const string Evasive = "Evasive";
    }

    /// <summary>
    /// Valid description moments.
    /// </summary>
    public static class DescriptionMoments
    {
        public const string Entering = "Entering";
        public const string Maintaining = "Maintaining";
        public const string Switching = "Switching";
    }

    /// <summary>
    /// Valid situation contexts.
    /// </summary>
    public static class SituationContexts
    {
        public const string Winning = "Winning";
        public const string Losing = "Losing";
        public const string EvenMatch = "EvenMatch";
        public const string Outnumbered = "Outnumbered";
        public const string Surrounded = "Surrounded";
    }
}
