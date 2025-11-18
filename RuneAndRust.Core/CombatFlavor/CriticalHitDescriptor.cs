// ==============================================================================
// v0.38.12: Advanced Combat Mechanics Descriptors
// CriticalHitDescriptor.cs
// ==============================================================================
// Purpose: Critical hit descriptors for devastating combat outcomes
// Pattern: Follows NPCPhysicalDescriptor structure from v0.38.11
// Integration: Used by CombatFlavorTextService to generate critical hit descriptions
// ==============================================================================

namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// Represents a critical hit descriptor for combat.
/// Describes devastating attacks that inflict maximum damage and special effects.
/// v0.38.12: Advanced Combat Mechanics Descriptors
/// </summary>
public class CriticalHitDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Attack category.
    /// Values: Melee, Ranged, Magic
    /// </summary>
    public string AttackCategory { get; set; } = string.Empty;

    /// <summary>
    /// Damage type.
    /// Values: Slashing, Crushing, Piercing, Fire, Ice, Lightning, Necrotic, Radiant
    /// </summary>
    public string DamageType { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Weapon or spell type (optional).
    /// Melee: Sword, Axe, Hammer, Spear, Dagger
    /// Ranged: Bow, Crossbow
    /// Magic: Fire, Ice, Lightning, Necrotic, Radiant
    /// NULL (generic)
    /// </summary>
    public string? WeaponOrSpellType { get; set; }

    /// <summary>
    /// Target type (optional).
    /// Values: Humanoid, Beast, Construct, Forlorn, Glitch, NULL
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// Special effect applied (optional).
    /// Values: Bleeding, Stunned, Frozen, Burning, Paralyzed, Dying, InstantKill, NULL
    /// </summary>
    public string? SpecialEffect { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - ACTOR: {AttackerName}, {WeaponName}, {SpellName}
    /// - TARGET: {TargetName}, {TargetType}
    /// - DAMAGE: {DamageAmount}, {DamageType}, {Overkill}
    /// - EFFECT: {EffectApplied}, {EffectDuration}, {SpecialOutcome}
    ///
    /// Example: "Your blade finds the perfect angle—it carves through armor and flesh like butter!"
    /// Example: "The impact is catastrophic! Bones shatter under your weapon's weight!"
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
    /// Example: ["Devastating", "Lethal", "Brutal", "Spectacular"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid attack categories.
    /// </summary>
    public static class AttackCategories
    {
        public const string Melee = "Melee";
        public const string Ranged = "Ranged";
        public const string Magic = "Magic";
    }

    /// <summary>
    /// Valid damage types.
    /// </summary>
    public static class DamageTypes
    {
        public const string Slashing = "Slashing";
        public const string Crushing = "Crushing";
        public const string Piercing = "Piercing";
        public const string Fire = "Fire";
        public const string Ice = "Ice";
        public const string Lightning = "Lightning";
        public const string Necrotic = "Necrotic";
        public const string Radiant = "Radiant";
    }

    /// <summary>
    /// Valid special effects.
    /// </summary>
    public static class SpecialEffects
    {
        public const string Bleeding = "Bleeding";
        public const string Stunned = "Stunned";
        public const string Frozen = "Frozen";
        public const string Burning = "Burning";
        public const string Paralyzed = "Paralyzed";
        public const string Dying = "Dying";
        public const string InstantKill = "InstantKill";
        public const string ArmorDestroyed = "ArmorDestroyed";
        public const string Prone = "Prone";
    }
}
