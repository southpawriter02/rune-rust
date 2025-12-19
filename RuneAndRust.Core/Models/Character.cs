using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents a character in Rune &amp; Rust with the five core attributes.
/// </summary>
public class Character
{
    private const int DefaultAttributeValue = 5;

    /// <summary>
    /// Gets the unique identifier for this character.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the character's name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the character's attributes and their values.
    /// </summary>
    public Dictionary<CharacterAttribute, int> Attributes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class with default attribute values.
    /// </summary>
    /// <param name="name">The character's name.</param>
    public Character(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        Attributes = new Dictionary<CharacterAttribute, int>
        {
            { CharacterAttribute.Sturdiness, DefaultAttributeValue },
            { CharacterAttribute.Might, DefaultAttributeValue },
            { CharacterAttribute.Wits, DefaultAttributeValue },
            { CharacterAttribute.Will, DefaultAttributeValue },
            { CharacterAttribute.Finesse, DefaultAttributeValue }
        };
    }

    /// <summary>
    /// Gets the value of a specific attribute.
    /// </summary>
    /// <param name="attribute">The attribute to retrieve.</param>
    /// <returns>The attribute's current value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the attribute is not found.</exception>
    public int GetAttribute(CharacterAttribute attribute)
    {
        return Attributes[attribute];
    }

    /// <summary>
    /// Sets the value of a specific attribute.
    /// </summary>
    /// <param name="attribute">The attribute to set.</param>
    /// <param name="value">The new value for the attribute.</param>
    public void SetAttribute(CharacterAttribute attribute, int value)
    {
        Attributes[attribute] = value;
    }
}
