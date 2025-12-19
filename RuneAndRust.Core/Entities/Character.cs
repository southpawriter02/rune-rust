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
}
