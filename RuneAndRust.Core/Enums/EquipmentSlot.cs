using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the equipment slots available for a character.
/// Each slot can hold one piece of equipment at a time.
/// </summary>
public enum EquipmentSlot
{
    /// <summary>
    /// Primary weapon hand. Melee weapons, ranged weapons, tools.
    /// </summary>
    [GameDocument(
        "Main Hand",
        "The primary weapon hand for combat implements. Melee weapons, ranged weapons, and tools occupy this slot. Two-handed weapons consume both Main Hand and Off Hand slots.")]
    MainHand = 0,

    /// <summary>
    /// Secondary hand. Shields, off-hand weapons, torches.
    /// </summary>
    [GameDocument(
        "Off Hand",
        "The secondary hand for support implements. Shields, off-hand weapons, and torches occupy this slot. Leaves the main hand free for primary weapons when equipped.")]
    OffHand = 1,

    /// <summary>
    /// Head protection. Helmets, hoods, masks.
    /// </summary>
    [GameDocument(
        "Head Slot",
        "Protective gear for the skull and face. Helmets, hoods, and masks provide armor and sometimes special abilities. Vision penalties may apply to fully enclosed headwear.")]
    Head = 2,

    /// <summary>
    /// Torso protection. Armor, robes, jackets.
    /// </summary>
    [GameDocument(
        "Body Slot",
        "Primary armor covering the torso. Armor, robes, and jackets provide the bulk of physical protection. The most important defensive slot for most survivors.")]
    Body = 3,

    /// <summary>
    /// Hand protection. Gauntlets, gloves, bracers.
    /// </summary>
    [GameDocument(
        "Hands Slot",
        "Protective gear for hands and forearms. Gauntlets, gloves, and bracers occupy this slot. Some hand gear improves weapon handling or crafting ability.")]
    Hands = 4,

    /// <summary>
    /// Foot protection. Boots, greaves, sandals.
    /// </summary>
    [GameDocument(
        "Feet Slot",
        "Protective gear for feet and lower legs. Boots, greaves, and sandals affect movement speed and terrain navigation. Heavy footwear may reduce agility.")]
    Feet = 5,

    /// <summary>
    /// Miscellaneous equipped items. Rings, amulets, belts.
    /// </summary>
    [GameDocument(
        "Accessory Slot",
        "Miscellaneous equipped items providing passive benefits. Rings, amulets, and belts occupy accessory slots. Multiple accessory slots may be available depending on character progression.")]
    Accessory = 6
}
