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
    MainHand = 0,

    /// <summary>
    /// Secondary hand. Shields, off-hand weapons, torches.
    /// </summary>
    OffHand = 1,

    /// <summary>
    /// Head protection. Helmets, hoods, masks.
    /// </summary>
    Head = 2,

    /// <summary>
    /// Torso protection. Armor, robes, jackets.
    /// </summary>
    Body = 3,

    /// <summary>
    /// Hand protection. Gauntlets, gloves, bracers.
    /// </summary>
    Hands = 4,

    /// <summary>
    /// Foot protection. Boots, greaves, sandals.
    /// </summary>
    Feet = 5,

    /// <summary>
    /// Miscellaneous equipped items. Rings, amulets, belts.
    /// </summary>
    Accessory = 6
}
