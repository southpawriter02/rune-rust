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
    /// Gets the effect this item provides when used.
    /// </summary>
    public ItemEffect Effect { get; private set; }

    /// <summary>
    /// Gets the magnitude of the effect (healing amount, damage, buff value).
    /// </summary>
    public int EffectValue { get; private set; }

    /// <summary>
    /// Gets the duration of the effect in turns (0 for instant effects).
    /// </summary>
    public int EffectDuration { get; private set; }

    /// <summary>
    /// Gets the equipment slot this item can be equipped to, or null if not equippable.
    /// </summary>
    /// <remarks>
    /// Items without an equipment slot (consumables, quest items, misc) cannot be equipped.
    /// The slot determines which equipment position the item occupies when equipped.
    /// </remarks>
    public EquipmentSlot? EquipmentSlot { get; private set; }

    /// <summary>
    /// Gets whether this item can be equipped.
    /// </summary>
    public bool IsEquippable => EquipmentSlot.HasValue;

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
    /// <param name="effect">The effect this item provides when used (default is None).</param>
    /// <param name="effectValue">The magnitude of the effect (default is 0).</param>
    /// <param name="effectDuration">The duration of the effect in turns (default is 0).</param>
    /// <param name="equipmentSlot">The equipment slot this item can be equipped to, or null if not equippable.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or description is null.</exception>
    public Item(string name, string description, ItemType type, int value = 0,
                ItemEffect effect = ItemEffect.None, int effectValue = 0, int effectDuration = 0,
                EquipmentSlot? equipmentSlot = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Value = value;
        Effect = effect;
        EffectValue = effectValue;
        EffectDuration = effectDuration;
        EquipmentSlot = equipmentSlot;
    }

    /// <summary>
    /// Factory method to create a basic sword weapon.
    /// </summary>
    /// <returns>A new sword item.</returns>
    public static Item CreateSword() => new(
        "Rusty Sword",
        "An old sword covered in rust. Still sharp enough to cut.",
        ItemType.Weapon,
        value: 5,
        equipmentSlot: Enums.EquipmentSlot.Weapon
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
        value: 25,
        effect: ItemEffect.Heal,
        effectValue: 25
    );

    /// <summary>
    /// Factory method to create basic leather armor.
    /// </summary>
    /// <returns>A new armor item.</returns>
    public static Item CreateLeatherArmor() => new(
        "Leather Armor",
        "Simple armor made of tanned leather. Provides basic protection.",
        ItemType.Armor,
        value: 15,
        equipmentSlot: Enums.EquipmentSlot.Armor
    );

    /// <summary>
    /// Factory method to create a basic wooden shield.
    /// </summary>
    /// <returns>A new shield item.</returns>
    public static Item CreateWoodenShield() => new(
        "Wooden Shield",
        "A basic wooden shield. Better than nothing.",
        ItemType.Armor,
        value: 10,
        equipmentSlot: Enums.EquipmentSlot.Shield
    );

    /// <summary>
    /// Factory method to create a basic iron helmet.
    /// </summary>
    /// <returns>A new helmet item.</returns>
    public static Item CreateIronHelmet() => new(
        "Iron Helmet",
        "A sturdy iron helmet that protects your head.",
        ItemType.Armor,
        value: 20,
        equipmentSlot: Enums.EquipmentSlot.Helmet
    );

    /// <summary>
    /// Returns the name of this item.
    /// </summary>
    /// <returns>The item name.</returns>
    public override string ToString() => Name;
}
