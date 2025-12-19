namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the functional category of an item.
/// Determines how the item can be used and where it appears in UI.
/// </summary>
public enum ItemType
{
    /// <summary>
    /// Offensive equipment. Swords, axes, bows, firearms.
    /// </summary>
    Weapon = 0,

    /// <summary>
    /// Defensive equipment. Helmets, chest plates, shields.
    /// </summary>
    Armor = 1,

    /// <summary>
    /// Single-use items. Potions, bandages, rations.
    /// </summary>
    Consumable = 2,

    /// <summary>
    /// Crafting components. Scrap metal, rune fragments, herbs.
    /// </summary>
    Material = 3,

    /// <summary>
    /// Quest-critical items. Keys, documents, relics.
    /// Cannot be dropped or sold.
    /// </summary>
    KeyItem = 4,

    /// <summary>
    /// Miscellaneous salvage. Sold for Scrip but no other use.
    /// </summary>
    Junk = 5
}
