namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a magical property that can be applied to an item through Runeforging.
/// Properties provide stat modifiers and special effects to the enchanted item.
/// </summary>
public class ItemProperty
{
    /// <summary>
    /// Gets or sets the unique identifier for this property instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name of the property (e.g., "Rune of Fortitude").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the property's effects.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stat modifiers provided by this property.
    /// Key is the stat name (e.g., "Might", "Defense"), value is the modifier amount.
    /// </summary>
    public Dictionary<string, int> StatModifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets when this property was applied to the item.
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
