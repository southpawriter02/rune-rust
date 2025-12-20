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

    /// <summary>
    /// Gets the attribute value for the specified attribute type.
    /// </summary>
    /// <param name="attr">The attribute to retrieve.</param>
    /// <returns>The attribute value, or 0 if not found.</returns>
    public int GetAttribute(CharacterAttribute attr) =>
        Attributes.GetValueOrDefault(attr, 0);
}
