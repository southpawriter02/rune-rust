using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents equipable items that provide stat bonuses.
/// Extends Item using Table-per-Hierarchy (TPH) pattern.
/// </summary>
public class Equipment : Item
{
    #region Equipment Properties

    /// <summary>
    /// Gets or sets the equipment slot this item occupies.
    /// </summary>
    public EquipmentSlot Slot { get; set; } = EquipmentSlot.MainHand;

    /// <summary>
    /// Gets or sets the attribute bonuses provided when equipped.
    /// Key is the attribute type, value is the bonus amount.
    /// </summary>
    public Dictionary<CharacterAttribute, int> AttributeBonuses { get; set; } = new();

    #endregion

    #region Durability Properties

    /// <summary>
    /// Gets or sets the maximum durability of this equipment.
    /// Equipment cannot be repaired beyond this value.
    /// </summary>
    public int MaxDurability { get; set; } = 100;

    /// <summary>
    /// Gets or sets the current durability of this equipment.
    /// Equipment becomes broken when this reaches 0.
    /// </summary>
    public int CurrentDurability { get; set; } = 100;

    /// <summary>
    /// Gets whether this equipment is broken and unusable.
    /// Broken equipment provides no bonuses and cannot be used in combat.
    /// </summary>
    public bool IsBroken => CurrentDurability <= 0;

    #endregion

    #region Combat Properties

    /// <summary>
    /// Gets or sets the bonus to soak (damage reduction) when equipped.
    /// Typically provided by armor pieces.
    /// </summary>
    public int SoakBonus { get; set; } = 0;

    /// <summary>
    /// Gets or sets the damage die size for weapons.
    /// 0 = not a weapon. 6 = d6, 8 = d8, etc.
    /// </summary>
    public int DamageDie { get; set; } = 0;

    #endregion

    #region Requirements

    /// <summary>
    /// Gets or sets the minimum attribute requirements to equip this item.
    /// Key is the attribute type, value is the minimum required.
    /// </summary>
    public Dictionary<CharacterAttribute, int> Requirements { get; set; } = new();

    #endregion

    /// <summary>
    /// Gets the total attribute bonus for a specific attribute.
    /// Returns 0 if no bonus exists for that attribute.
    /// </summary>
    /// <param name="attribute">The attribute to check.</param>
    /// <returns>The bonus value, or 0 if none.</returns>
    public int GetAttributeBonus(CharacterAttribute attribute)
    {
        return AttributeBonuses.TryGetValue(attribute, out var bonus) ? bonus : 0;
    }

    /// <summary>
    /// Checks if a character meets the requirements to equip this item.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>True if all requirements are met, false otherwise.</returns>
    public bool MeetsRequirements(Character character)
    {
        foreach (var requirement in Requirements)
        {
            if (character.GetAttribute(requirement.Key) < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }
}
