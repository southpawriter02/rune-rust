using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Weapons",
        "Implements of harm ranging from salvaged blades to Pre-Glitch firearms. Weapons determine a survivor's offensive capability and often dictate combat style. Each weapon type favors different attributes.")]
    Weapon = 0,

    /// <summary>
    /// Defensive equipment. Helmets, chest plates, shields.
    /// </summary>
    [GameDocument(
        "Armor",
        "Protective gear that absorbs physical punishment. Armor reduces incoming damage but often carries weight penalties. The quality of one's armor frequently determines survival in the wastes.")]
    Armor = 1,

    /// <summary>
    /// Single-use items. Potions, bandages, rations.
    /// </summary>
    [GameDocument(
        "Consumables",
        "Single-use items destroyed upon activation. Potions, bandages, and rations fall into this category. Wise survivors maintain a supply of consumables for emergencies.")]
    Consumable = 2,

    /// <summary>
    /// Crafting components. Scrap metal, rune fragments, herbs.
    /// </summary>
    [GameDocument(
        "Materials",
        "Raw components used in crafting and repair. Scrap metal, rune fragments, and medicinal herbs represent common materials. Value lies in potential rather than direct use.")]
    Material = 3,

    /// <summary>
    /// Quest-critical items. Keys, documents, relics.
    /// Cannot be dropped or sold.
    /// </summary>
    [GameDocument(
        "Key Items",
        "Quest-critical objects that cannot be discarded or sold. Keys, documents, and ancient relics fall into this protected category. The game prevents accidental loss of these crucial items.")]
    KeyItem = 4,

    /// <summary>
    /// Miscellaneous salvage. Sold for Scrip but no other use.
    /// </summary>
    [GameDocument(
        "Junk",
        "Miscellaneous salvage with no practical application beyond sale. Traders convert junk into Scrip, the common currency of the wastes. Experienced scavengers recognize junk quickly.")]
    Junk = 5
}
