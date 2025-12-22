using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a player character in Rune &amp; Rust.
/// Contains identity, lineage, archetype, attributes, and derived stats.
/// </summary>
public class Character
{
    /// <summary>
    /// Unique identifier for the character.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The character's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The character's heritage/race.
    /// </summary>
    public LineageType Lineage { get; set; } = LineageType.Human;

    /// <summary>
    /// The character's combat specialization.
    /// </summary>
    public ArchetypeType Archetype { get; set; } = ArchetypeType.Warrior;

    /// <summary>
    /// The character's narrative background (v0.3.4c).
    /// Shapes the prologue text and story context.
    /// </summary>
    public BackgroundType Background { get; set; } = BackgroundType.Scavenger;

    /// <summary>
    /// Physical resilience, health, and endurance. Range: 1-10.
    /// </summary>
    public int Sturdiness { get; set; } = 5;

    /// <summary>
    /// Raw physical power and strength. Range: 1-10.
    /// </summary>
    public int Might { get; set; } = 5;

    /// <summary>
    /// Mental acuity, perception, and intelligence. Range: 1-10.
    /// </summary>
    public int Wits { get; set; } = 5;

    /// <summary>
    /// Mental fortitude and willpower. Range: 1-10.
    /// </summary>
    public int Will { get; set; } = 5;

    /// <summary>
    /// Agility, dexterity, and precision. Range: 1-10.
    /// </summary>
    public int Finesse { get; set; } = 5;

    /// <summary>
    /// Maximum health points. Derived: 50 + (Sturdiness * 10).
    /// </summary>
    public int MaxHP { get; set; } = 100;

    /// <summary>
    /// Current health points. Cannot exceed MaxHP.
    /// </summary>
    public int CurrentHP { get; set; } = 100;

    /// <summary>
    /// Maximum stamina points. Derived: 20 + (Finesse * 5) + (Sturdiness * 3).
    /// </summary>
    public int MaxStamina { get; set; } = 60;

    /// <summary>
    /// Current stamina points. Cannot exceed MaxStamina.
    /// </summary>
    public int CurrentStamina { get; set; } = 60;

    /// <summary>
    /// Action points available per turn. Derived: 2 + (Wits / 4).
    /// </summary>
    public int ActionPoints { get; set; } = 3;

    /// <summary>
    /// Maximum Aether Points (magical energy pool for Mystic archetype).
    /// Derived: 10 + (Will * 5) for Mystics, 0 for non-Mystics.
    /// </summary>
    public int MaxAp { get; set; } = 0;

    /// <summary>
    /// Current Aether Points. Does NOT regenerate during combat.
    /// Mystics can Overcast (spend HP at 2:1 ratio) when depleted.
    /// </summary>
    public int CurrentAp { get; set; } = 0;

    /// <summary>
    /// Current psychic stress accumulated by the character. Range: 0-100.
    /// Increases from traumatic combat events, decreases from rest or abilities.
    /// Affects defense score: Defense = 10 + FINESSE - (Stress / 20).
    /// </summary>
    public int PsychicStress { get; set; } = 0;

    /// <summary>
    /// Maximum psychic stress threshold. Fixed at 100 for all characters.
    /// Reaching 100 triggers a Breaking Point event.
    /// </summary>
    public int MaxPsychicStress => 100;

    /// <summary>
    /// Current Runic Blight corruption accumulated by the character. Range: 0-100.
    /// Unlike Stress, Corruption is permanent and not mitigated by WILL.
    /// Reaching 100 triggers Terminal Error (character becomes a Forlorn).
    /// </summary>
    public int Corruption { get; set; } = 0;

    /// <summary>
    /// Maximum corruption threshold. Fixed at 100 for all characters.
    /// </summary>
    public int MaxCorruption => 100;

    /// <summary>
    /// Experience points accumulated by the character.
    /// </summary>
    public int ExperiencePoints { get; set; } = 0;

    /// <summary>
    /// Character's current level. Starts at 1.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Timestamp when the character was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last time the character was modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    #region Inventory

    /// <summary>
    /// Gets or sets the collection of inventory items owned by this character.
    /// Navigation property for EF Core.
    /// </summary>
    public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();

    /// <summary>
    /// Gets or sets the equipment bonuses currently applied from equipped items.
    /// Key is the attribute type, value is the total bonus.
    /// </summary>
    public Dictionary<CharacterAttribute, int> EquipmentBonuses { get; set; } = new();

    #endregion

    #region Trauma System (v0.3.0c)

    /// <summary>
    /// Gets or sets the collection of permanent psychological traumas.
    /// Traumas are acquired when failing a Breaking Point resolve check.
    /// </summary>
    public List<Trauma> ActiveTraumas { get; set; } = new();

    #endregion

    #region Status Effects (v0.3.2a)

    /// <summary>
    /// Persistent status effects active outside of combat.
    /// Combat status effects are tracked separately on Combatant.
    /// Stored as JSONB in PostgreSQL.
    /// </summary>
    public List<StatusEffectType> ActiveStatusEffects { get; set; } = new();

    /// <summary>
    /// Checks if the character has a specific status effect active.
    /// </summary>
    /// <param name="effect">The status effect to check for.</param>
    /// <returns>True if the effect is active; otherwise false.</returns>
    public bool HasStatusEffect(StatusEffectType effect) => ActiveStatusEffects.Contains(effect);

    /// <summary>
    /// Adds a status effect if not already present.
    /// </summary>
    /// <param name="effect">The status effect to add.</param>
    public void AddStatusEffect(StatusEffectType effect)
    {
        if (!ActiveStatusEffects.Contains(effect))
            ActiveStatusEffects.Add(effect);
    }

    /// <summary>
    /// Removes a status effect if present.
    /// </summary>
    /// <param name="effect">The status effect to remove.</param>
    public void RemoveStatusEffect(StatusEffectType effect) => ActiveStatusEffects.Remove(effect);

    #endregion

    #region Attribute Methods

    /// <summary>
    /// Gets the attribute value for the specified attribute type.
    /// </summary>
    /// <param name="attribute">The attribute to retrieve.</param>
    /// <returns>The attribute value.</returns>
    public int GetAttribute(CharacterAttribute attribute)
    {
        return attribute switch
        {
            CharacterAttribute.Sturdiness => Sturdiness,
            CharacterAttribute.Might => Might,
            CharacterAttribute.Wits => Wits,
            CharacterAttribute.Will => Will,
            CharacterAttribute.Finesse => Finesse,
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown attribute type")
        };
    }

    /// <summary>
    /// Sets the attribute value for the specified attribute type.
    /// </summary>
    /// <param name="attribute">The attribute to set.</param>
    /// <param name="value">The value to assign.</param>
    public void SetAttribute(CharacterAttribute attribute, int value)
    {
        switch (attribute)
        {
            case CharacterAttribute.Sturdiness:
                Sturdiness = value;
                break;
            case CharacterAttribute.Might:
                Might = value;
                break;
            case CharacterAttribute.Wits:
                Wits = value;
                break;
            case CharacterAttribute.Will:
                Will = value;
                break;
            case CharacterAttribute.Finesse:
                Finesse = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown attribute type");
        }
    }

    /// <summary>
    /// Gets the effective attribute value including equipment bonuses.
    /// </summary>
    /// <param name="attribute">The attribute to retrieve.</param>
    /// <returns>The base attribute value plus any equipment bonuses.</returns>
    public int GetEffectiveAttribute(CharacterAttribute attribute)
    {
        var baseValue = GetAttribute(attribute);
        var bonus = EquipmentBonuses.TryGetValue(attribute, out var equipBonus) ? equipBonus : 0;
        return baseValue + bonus;
    }

    #endregion
}
