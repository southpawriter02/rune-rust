using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents requirements that must be met to equip an item.
/// </summary>
/// <remarks>
/// <para>
/// All specified requirements must be met. Null values indicate no requirement
/// for that attribute. Class requirements use class IDs from configuration.
/// </para>
/// </remarks>
public readonly record struct EquipmentRequirements
{
    /// <summary>Minimum Might attribute required.</summary>
    public int? MinMight { get; init; }

    /// <summary>Minimum Fortitude attribute required.</summary>
    public int? MinFortitude { get; init; }

    /// <summary>Minimum Will attribute required.</summary>
    public int? MinWill { get; init; }

    /// <summary>Minimum Wits attribute required.</summary>
    public int? MinWits { get; init; }

    /// <summary>Minimum Finesse attribute required.</summary>
    public int? MinFinesse { get; init; }

    /// <summary>Class IDs that can equip this item. Null or empty means any class.</summary>
    public IReadOnlyList<string>? RequiredClassIds { get; init; }

    /// <summary>
    /// Returns true if this item has any requirements.
    /// </summary>
    public bool HasRequirements => MinMight.HasValue || MinFortitude.HasValue ||
                                   MinWill.HasValue || MinWits.HasValue ||
                                   MinFinesse.HasValue || RequiredClassIds?.Count > 0;

    /// <summary>
    /// Creates a requirements instance with no requirements.
    /// </summary>
    public static EquipmentRequirements None => new();

    /// <summary>
    /// Creates requirements with Fortitude requirement only.
    /// </summary>
    /// <param name="min">Minimum Fortitude required.</param>
    public static EquipmentRequirements ForFortitude(int min) => new() { MinFortitude = min };

    /// <summary>
    /// Creates requirements with Might requirement only.
    /// </summary>
    /// <param name="min">Minimum Might required.</param>
    public static EquipmentRequirements ForMight(int min) => new() { MinMight = min };

    /// <summary>
    /// Creates requirements with class restriction.
    /// </summary>
    /// <param name="classIds">The class IDs that can equip this item.</param>
    public static EquipmentRequirements ForClasses(params string[] classIds) =>
        new() { RequiredClassIds = classIds.ToList().AsReadOnly() };

    /// <summary>
    /// Checks if a player meets these requirements.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if all requirements are met.</returns>
    public bool MeetsRequirements(Player player)
    {
        if (player == null) return false;

        if (MinMight.HasValue && player.Attributes.Might < MinMight.Value)
            return false;
        if (MinFortitude.HasValue && player.Attributes.Fortitude < MinFortitude.Value)
            return false;
        if (MinWill.HasValue && player.Attributes.Will < MinWill.Value)
            return false;
        if (MinWits.HasValue && player.Attributes.Wits < MinWits.Value)
            return false;
        if (MinFinesse.HasValue && player.Attributes.Finesse < MinFinesse.Value)
            return false;

        if (RequiredClassIds?.Count > 0)
        {
            if (string.IsNullOrEmpty(player.ClassId))
                return false;
            if (!RequiredClassIds.Contains(player.ClassId, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a list of unmet requirements for a player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>List of requirement descriptions that are not met.</returns>
    public IReadOnlyList<string> GetUnmetRequirements(Player player)
    {
        var unmet = new List<string>();

        if (player == null)
        {
            unmet.Add("No player specified");
            return unmet;
        }

        if (MinMight.HasValue && player.Attributes.Might < MinMight.Value)
            unmet.Add($"Might {MinMight.Value} required (you have {player.Attributes.Might})");
        if (MinFortitude.HasValue && player.Attributes.Fortitude < MinFortitude.Value)
            unmet.Add($"Fortitude {MinFortitude.Value} required (you have {player.Attributes.Fortitude})");
        if (MinWill.HasValue && player.Attributes.Will < MinWill.Value)
            unmet.Add($"Will {MinWill.Value} required (you have {player.Attributes.Will})");
        if (MinWits.HasValue && player.Attributes.Wits < MinWits.Value)
            unmet.Add($"Wits {MinWits.Value} required (you have {player.Attributes.Wits})");
        if (MinFinesse.HasValue && player.Attributes.Finesse < MinFinesse.Value)
            unmet.Add($"Finesse {MinFinesse.Value} required (you have {player.Attributes.Finesse})");

        if (RequiredClassIds?.Count > 0)
        {
            if (string.IsNullOrEmpty(player.ClassId) ||
                !RequiredClassIds.Contains(player.ClassId, StringComparer.OrdinalIgnoreCase))
            {
                var classes = string.Join(", ", RequiredClassIds);
                unmet.Add($"Class must be one of: {classes}");
            }
        }

        return unmet;
    }

    /// <summary>
    /// Returns a display string of all requirements.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();
        if (MinMight.HasValue) parts.Add($"Might {MinMight.Value}");
        if (MinFortitude.HasValue) parts.Add($"Fortitude {MinFortitude.Value}");
        if (MinWill.HasValue) parts.Add($"Will {MinWill.Value}");
        if (MinWits.HasValue) parts.Add($"Wits {MinWits.Value}");
        if (MinFinesse.HasValue) parts.Add($"Finesse {MinFinesse.Value}");
        if (RequiredClassIds?.Count > 0) parts.Add($"Class: {string.Join("/", RequiredClassIds)}");
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }
}
