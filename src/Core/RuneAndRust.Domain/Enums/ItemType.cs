namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes items by their function or purpose.
/// </summary>
public enum ItemType
{
    /// <summary>
    /// Items used to deal damage in combat (swords, axes, etc.).
    /// </summary>
    Weapon,

    /// <summary>
    /// Items that provide defensive bonuses (shields, helmets, etc.).
    /// </summary>
    Armor,

    /// <summary>
    /// Items that can be used once for an effect (potions, scrolls, etc.).
    /// </summary>
    Consumable,

    /// <summary>
    /// Items required for quest objectives.
    /// </summary>
    Quest,

    /// <summary>
    /// Miscellaneous items without a specific category.
    /// </summary>
    Misc,

    /// <summary>
    /// Items used to unlock locked objects.
    /// </summary>
    Key
}
