namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of equipment that can provide skill modifiers.
/// </summary>
/// <remarks>
/// Used by <see cref="ValueObjects.EquipmentModifier"/> to classify the source
/// of equipment-based bonuses or penalties.
/// </remarks>
public enum EquipmentCategory
{
    /// <summary>
    /// Tools and utility items (lockpicks, toolkit, compass).
    /// </summary>
    Tool = 0,

    /// <summary>
    /// Weapons that may affect certain checks.
    /// </summary>
    Weapon = 1,

    /// <summary>
    /// Armor that may affect certain checks (stealth, acrobatics).
    /// </summary>
    Armor = 2,

    /// <summary>
    /// Accessories and miscellaneous gear.
    /// </summary>
    Accessory = 3
}
