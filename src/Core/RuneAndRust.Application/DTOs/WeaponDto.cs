using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying weapon information.
/// </summary>
/// <param name="Name">The weapon's display name.</param>
/// <param name="WeaponType">The type of weapon (Sword, Axe, etc.).</param>
/// <param name="DamageDice">The damage dice notation (e.g., "1d8").</param>
/// <param name="Description">The weapon's description.</param>
/// <param name="Bonuses">String representation of weapon bonuses.</param>
public record WeaponDto(
    string Name,
    string WeaponType,
    string DamageDice,
    string Description,
    string? Bonuses = null)
{
    /// <summary>
    /// Creates a WeaponDto from an Item.
    /// </summary>
    /// <param name="item">The item to convert.</param>
    /// <returns>A WeaponDto if the item is a weapon, null otherwise.</returns>
    public static WeaponDto? FromItem(Item item)
    {
        if (!item.IsWeapon || item.WeaponType == null)
            return null;

        return new WeaponDto(
            item.Name,
            item.WeaponType.Value.ToString(),
            item.DamageDice ?? "1d4",
            item.Description,
            item.WeaponBonuses.HasBonuses ? item.WeaponBonuses.ToString() : null);
    }
}
