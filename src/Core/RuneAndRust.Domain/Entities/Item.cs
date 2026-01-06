using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

public class Item : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ItemType Type { get; private set; }
    public int Value { get; private set; }

    private Item() { } // For EF Core

    public Item(string name, string description, ItemType type, int value = 0)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Value = value;
    }

    public static Item CreateSword() => new(
        "Rusty Sword",
        "An old sword covered in rust. Still sharp enough to cut.",
        ItemType.Weapon,
        5
    );

    public static Item CreateScroll() => new(
        "Ancient Scroll",
        "A scroll with mysterious runes. You can't quite make out what it says.",
        ItemType.Quest,
        0
    );

    public static Item CreateHealthPotion() => new(
        "Health Potion",
        "A vial of red liquid that restores health when consumed.",
        ItemType.Consumable,
        25
    );

    public override string ToString() => Name;
}
