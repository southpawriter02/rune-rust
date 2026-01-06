using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an item that can be found in the dungeon and collected by players.
/// </summary>
/// <remarks>
/// Items can be of various types (weapons, consumables, quest items, etc.) and may have
/// different values representing their worth or effectiveness.
/// </remarks>
public class Item : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this item.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this item.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of this item shown to players.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the type category of this item.
    /// </summary>
    public ItemType Type { get; private set; }

    /// <summary>
    /// Gets the value of this item (e.g., damage for weapons, healing for potions).
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Item()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Creates a new item with the specified properties.
    /// </summary>
    /// <param name="name">The display name of the item.</param>
    /// <param name="description">The description shown to players.</param>
    /// <param name="type">The type category of the item.</param>
    /// <param name="value">The value of the item (default is 0).</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    public Item(string name, string description, ItemType type, int value = 0)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Value = value;
    }

    /// <summary>
    /// Factory method to create a basic sword weapon.
    /// </summary>
    /// <returns>A new sword item.</returns>
    public static Item CreateSword() => new(
        "Rusty Sword",
        "An old sword covered in rust. Still sharp enough to cut.",
        ItemType.Weapon,
        5
    );

    /// <summary>
    /// Factory method to create a quest scroll item.
    /// </summary>
    /// <returns>A new scroll item.</returns>
    public static Item CreateScroll() => new(
        "Ancient Scroll",
        "A scroll with mysterious runes. You can't quite make out what it says.",
        ItemType.Quest,
        0
    );

    /// <summary>
    /// Factory method to create a health potion consumable.
    /// </summary>
    /// <returns>A new health potion item.</returns>
    public static Item CreateHealthPotion() => new(
        "Health Potion",
        "A vial of red liquid that restores health when consumed.",
        ItemType.Consumable,
        25
    );

    /// <summary>
    /// Returns the name of this item.
    /// </summary>
    /// <returns>The item name.</returns>
    public override string ToString() => Name;
}
