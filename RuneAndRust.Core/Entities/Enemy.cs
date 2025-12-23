using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents an enemy combatant in Rune &amp; Rust.
/// Stub entity for v0.2.0a - will be expanded with behaviors and AI in v0.2.1.
/// </summary>
public class Enemy
{
    /// <summary>
    /// Unique identifier for the enemy instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The enemy's display name.
    /// </summary>
    public string Name { get; set; } = "Training Dummy";

    /// <summary>
    /// The enemy's attributes. Keys are attribute types, values are attribute scores.
    /// </summary>
    public Dictionary<CharacterAttribute, int> Attributes { get; set; } = new()
    {
        { CharacterAttribute.Sturdiness, 5 },
        { CharacterAttribute.Might, 5 },
        { CharacterAttribute.Wits, 3 },
        { CharacterAttribute.Will, 3 },
        { CharacterAttribute.Finesse, 3 }
    };

    /// <summary>
    /// Maximum health points for this enemy.
    /// </summary>
    public int MaxHp { get; set; } = 50;

    /// <summary>
    /// Current health points. Cannot exceed MaxHp.
    /// </summary>
    public int CurrentHp { get; set; } = 50;

    /// <summary>
    /// Maximum stamina points for this enemy.
    /// Calculated as: 20 + (Finesse * 3) + (Sturdiness * 2)
    /// </summary>
    public int MaxStamina { get; set; } = 35;

    /// <summary>
    /// Current stamina points. Cannot exceed MaxStamina.
    /// </summary>
    public int CurrentStamina { get; set; } = 35;

    #region Equipment Stats (Snapshot for Combat)

    /// <summary>
    /// The damage die size for this enemy's weapon (e.g., 6 for d6).
    /// Used by AttackResolutionService during combat.
    /// </summary>
    public int WeaponDamageDie { get; set; } = 4;

    /// <summary>
    /// Accuracy bonus from the enemy's weapon.
    /// </summary>
    public int WeaponAccuracyBonus { get; set; } = 0;

    /// <summary>
    /// Total armor soak value for damage reduction.
    /// </summary>
    public int ArmorSoak { get; set; } = 0;

    /// <summary>
    /// Display name for the enemy's weapon.
    /// </summary>
    public string WeaponName { get; set; } = "Claws";

    #endregion

    #region Template and AI Properties

    /// <summary>
    /// The template ID this enemy was created from.
    /// Null for legacy enemies or those created without a template.
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Combat archetype for AI behavior selection in v0.2.2b.
    /// Determines attack priority, target selection, and ability usage.
    /// </summary>
    public EnemyArchetype Archetype { get; set; } = EnemyArchetype.DPS;

    /// <summary>
    /// Tags for status effect interactions and special handling.
    /// Examples: "Mechanical" = Bleed immune, "Undying" = Poison immune.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Active creature traits for Elite/Champion enemies.
    /// Populated by CreatureTraitService during enemy creation.
    /// </summary>
    public List<CreatureTraitType> ActiveTraits { get; set; } = new();

    /// <summary>
    /// Whether this enemy has any active traits (Elite or higher).
    /// </summary>
    public bool IsElite => ActiveTraits.Count > 0;

    /// <summary>
    /// The active abilities available to this enemy (v0.2.4a).
    /// Populated during factory hydration from template AbilityNames.
    /// </summary>
    public List<ActiveAbility> Abilities { get; set; } = new();

    #endregion

    /// <summary>
    /// Gets the attribute value for the specified attribute type.
    /// </summary>
    /// <param name="attr">The attribute to retrieve.</param>
    /// <returns>The attribute value, or 0 if not found.</returns>
    public int GetAttribute(CharacterAttribute attr) =>
        Attributes.GetValueOrDefault(attr, 0);
}
