using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for displaying an equipped item in a specific slot.
/// </summary>
/// <param name="Slot">The equipment slot.</param>
/// <param name="ItemName">The name of the equipped item, or null if empty.</param>
/// <param name="ItemDescription">The description of the equipped item, or null if empty.</param>
public record EquippedItemDto(
    EquipmentSlot Slot,
    string? ItemName,
    string? ItemDescription)
{
    /// <summary>
    /// Whether this slot has an item equipped.
    /// </summary>
    public bool IsOccupied => ItemName != null;

    /// <summary>
    /// Gets the display name for the slot.
    /// </summary>
    public string SlotDisplayName => Slot.ToString();
}

/// <summary>
/// DTO for displaying all equipment slots.
/// </summary>
/// <param name="Slots">The list of all equipment slots with their items.</param>
public record EquipmentSlotsDto(IReadOnlyList<EquippedItemDto> Slots)
{
    /// <summary>
    /// Creates an EquipmentSlotsDto from a player's equipment.
    /// </summary>
    /// <param name="player">The player whose equipment to display.</param>
    /// <returns>A DTO with all slots.</returns>
    public static EquipmentSlotsDto FromPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var slots = Enum.GetValues<EquipmentSlot>()
            .Select(slot =>
            {
                var item = player.GetEquippedItem(slot);
                return new EquippedItemDto(
                    slot,
                    item?.Name,
                    item?.Description);
            })
            .ToList();

        return new EquipmentSlotsDto(slots);
    }

    /// <summary>
    /// Gets the number of occupied slots.
    /// </summary>
    public int OccupiedSlotCount => Slots.Count(s => s.IsOccupied);

    /// <summary>
    /// Gets the total number of slots.
    /// </summary>
    public int TotalSlotCount => Slots.Count;
}

/// <summary>
/// DTO for displaying an equip/unequip result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">The result message.</param>
/// <param name="ReplacedItemName">Name of replaced item if swapped.</param>
public record EquipResultDto(
    bool Success,
    string Message,
    string? ReplacedItemName = null)
{
    /// <summary>
    /// Whether an item was swapped during this operation.
    /// </summary>
    public bool WasSwapped => ReplacedItemName != null;

    /// <summary>
    /// Creates a DTO from an EquipResult.
    /// </summary>
    /// <param name="result">The domain result.</param>
    /// <returns>A DTO for rendering.</returns>
    public static EquipResultDto FromResult(EquipResult result) =>
        new(result.Success, result.Message, result.UnequippedItem?.Name);
}

/// <summary>
/// DTO for displaying armor information.
/// </summary>
/// <param name="Name">The armor's display name.</param>
/// <param name="ArmorType">The type of armor (Light, Medium, Heavy).</param>
/// <param name="DefenseBonus">Defense bonus when equipped.</param>
/// <param name="InitiativePenalty">Initiative penalty when equipped.</param>
/// <param name="Description">The armor's description.</param>
/// <param name="StatBonuses">String representation of stat bonuses.</param>
/// <param name="Requirements">String representation of equip requirements.</param>
public record ArmorDto(
    string Name,
    string ArmorType,
    int DefenseBonus,
    int InitiativePenalty,
    string Description,
    string? StatBonuses = null,
    string? Requirements = null)
{
    /// <summary>
    /// Creates an ArmorDto from an Item.
    /// </summary>
    /// <param name="item">The item to convert.</param>
    /// <returns>An ArmorDto, or null if the item is not armor.</returns>
    public static ArmorDto? FromItem(Item item)
    {
        if (!item.IsArmor || item.ArmorType == null)
            return null;

        return new ArmorDto(
            item.Name,
            item.ArmorType.Value.ToString(),
            item.DefenseBonus,
            item.InitiativePenalty,
            item.Description,
            item.StatModifiers.HasModifiers ? item.StatModifiers.ToString() : null,
            item.HasRequirements ? item.Requirements.ToString() : null);
    }
}

/// <summary>
/// DTO for equipment display with stat summary.
/// </summary>
/// <param name="Slots">The equipment slots data.</param>
/// <param name="BaseDefense">Player's base defense value.</param>
/// <param name="TotalDefense">Total defense including equipment bonuses.</param>
/// <param name="InitiativePenalty">Total initiative penalty from equipment.</param>
/// <param name="AttributeBonuses">Formatted string of attribute bonuses.</param>
public record EquipmentDisplayDto(
    EquipmentSlotsDto Slots,
    int BaseDefense,
    int TotalDefense,
    int InitiativePenalty,
    string? AttributeBonuses = null)
{
    /// <summary>
    /// Creates an EquipmentDisplayDto from a player.
    /// </summary>
    /// <param name="player">The player whose equipment to display.</param>
    /// <returns>A DTO with equipment and stat summary.</returns>
    public static EquipmentDisplayDto FromPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var effectiveStats = player.GetEffectiveStats();
        var defenseBonus = player.GetTotalDefenseBonus();
        var initiativePenalty = player.GetTotalInitiativePenalty();

        return new EquipmentDisplayDto(
            Slots: EquipmentSlotsDto.FromPlayer(player),
            BaseDefense: player.Stats.Defense,
            TotalDefense: effectiveStats.Defense,
            InitiativePenalty: initiativePenalty,
            AttributeBonuses: GetAttributeBonusesDisplay(player));
    }

    private static string? GetAttributeBonusesDisplay(Player player)
    {
        var effectiveAttrs = player.GetEffectiveAttributes();
        var baseAttrs = player.Attributes;
        var bonuses = new List<string>();

        if (effectiveAttrs.Might != baseAttrs.Might)
            bonuses.Add($"Might: {baseAttrs.Might} + {effectiveAttrs.Might - baseAttrs.Might} = {effectiveAttrs.Might}");
        if (effectiveAttrs.Fortitude != baseAttrs.Fortitude)
            bonuses.Add($"Fortitude: {baseAttrs.Fortitude} + {effectiveAttrs.Fortitude - baseAttrs.Fortitude} = {effectiveAttrs.Fortitude}");
        if (effectiveAttrs.Will != baseAttrs.Will)
            bonuses.Add($"Will: {baseAttrs.Will} + {effectiveAttrs.Will - baseAttrs.Will} = {effectiveAttrs.Will}");
        if (effectiveAttrs.Wits != baseAttrs.Wits)
            bonuses.Add($"Wits: {baseAttrs.Wits} + {effectiveAttrs.Wits - baseAttrs.Wits} = {effectiveAttrs.Wits}");
        if (effectiveAttrs.Finesse != baseAttrs.Finesse)
            bonuses.Add($"Finesse: {baseAttrs.Finesse} + {effectiveAttrs.Finesse - baseAttrs.Finesse} = {effectiveAttrs.Finesse}");

        return bonuses.Count > 0 ? string.Join(", ", bonuses) : null;
    }
}
