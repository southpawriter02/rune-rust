using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modifier from equipped tools, gear, or weapons.
/// </summary>
/// <remarks>
/// <para>
/// Equipment modifiers provide bonuses or penalties based on:
/// <list type="bullet">
///   <item><description>Tool quality (basic lockpicks vs. quality lockpicks)</description></item>
///   <item><description>Tool appropriateness (correct tool for the job)</description></item>
///   <item><description>Equipment condition (damaged equipment may give penalties)</description></item>
/// </list>
/// </para>
/// <para>
/// Some checks require specific equipment to attempt (e.g., lockpicking requires picks).
/// This is indicated by <see cref="RequiredForCheck"/>.
/// </para>
/// </remarks>
/// <param name="EquipmentId">Unique identifier of the equipment item.</param>
/// <param name="EquipmentName">Display name of the equipment.</param>
/// <param name="DiceModifier">Bonus or penalty to dice pool.</param>
/// <param name="DcModifier">Bonus or penalty to difficulty class.</param>
/// <param name="EquipmentCategory">Category of equipment (Tool, Weapon, Armor, Accessory).</param>
/// <param name="RequiredForCheck">Whether this equipment is required to attempt the check.</param>
/// <param name="Description">Optional flavor text for UI display.</param>
public readonly record struct EquipmentModifier(
    string EquipmentId,
    string EquipmentName,
    int DiceModifier,
    int DcModifier,
    EquipmentCategory EquipmentCategory,
    bool RequiredForCheck = false,
    string? Description = null) : ISkillModifier
{
    /// <summary>
    /// Gets the modifier category.
    /// </summary>
    public ModifierCategory Category => ModifierCategory.Equipment;

    /// <summary>
    /// Creates a tool modifier with dice bonus.
    /// </summary>
    /// <param name="equipmentId">Equipment identifier.</param>
    /// <param name="name">Equipment name.</param>
    /// <param name="diceBonus">Dice pool bonus.</param>
    /// <param name="required">Whether required to attempt.</param>
    /// <returns>A new equipment modifier.</returns>
    public static EquipmentModifier Tool(string equipmentId, string name, int diceBonus, bool required = false)
    {
        return new EquipmentModifier(
            equipmentId,
            name,
            diceBonus,
            DcModifier: 0,
            EquipmentCategory.Tool,
            required);
    }

    /// <summary>
    /// Creates an armor modifier (typically penalties for stealth/acrobatics).
    /// </summary>
    /// <param name="equipmentId">Equipment identifier.</param>
    /// <param name="name">Equipment name.</param>
    /// <param name="dicePenalty">Dice pool penalty (should be negative or zero).</param>
    /// <returns>A new equipment modifier.</returns>
    public static EquipmentModifier Armor(string equipmentId, string name, int dicePenalty)
    {
        return new EquipmentModifier(
            equipmentId,
            name,
            dicePenalty,
            DcModifier: 0,
            EquipmentCategory.Armor);
    }

    /// <summary>
    /// Returns a short description for UI display.
    /// </summary>
    /// <example>
    /// "Tinker's Toolkit (+2d10)"
    /// "Magnifying Glass (+1d10)"
    /// "Damaged Lockpicks (-1d10)"
    /// </example>
    public string ToShortDescription()
    {
        var parts = new List<string> { EquipmentName };

        if (DiceModifier != 0)
        {
            var diceStr = DiceModifier > 0 ? $"+{DiceModifier}d10" : $"{DiceModifier}d10";
            parts.Add($"({diceStr})");
        }

        if (DcModifier != 0)
        {
            var dcStr = DcModifier > 0 ? $"DC +{DcModifier}" : $"DC {DcModifier}";
            parts.Add($"({dcStr})");
        }

        if (RequiredForCheck)
            parts.Add("[Required]");

        return string.Join(" ", parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToShortDescription();
}
