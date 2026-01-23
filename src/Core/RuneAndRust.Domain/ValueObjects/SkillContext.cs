using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Container for all contextual modifiers affecting a skill check.
/// </summary>
/// <remarks>
/// <para>
/// Aggregates modifiers from four categories:
/// <list type="bullet">
///   <item><description>Equipment: Tools, gear, weapons</description></item>
///   <item><description>Situational: Time pressure, familiarity, assistance</description></item>
///   <item><description>Environment: Surface, lighting, corruption</description></item>
///   <item><description>Target: NPC disposition, suspicion, resistance</description></item>
/// </list>
/// </para>
/// <para>
/// The context calculates total dice pool and DC adjustments that are applied
/// by SkillCheckService when performing the check.
/// </para>
/// </remarks>
public sealed class SkillContext
{
    /// <summary>
    /// Equipment modifiers from tools, gear, and weapons.
    /// </summary>
    public IReadOnlyList<EquipmentModifier> EquipmentModifiers { get; }

    /// <summary>
    /// Situational modifiers from temporary conditions.
    /// </summary>
    public IReadOnlyList<SituationalModifier> SituationalModifiers { get; }

    /// <summary>
    /// Environment modifiers from physical conditions.
    /// </summary>
    public IReadOnlyList<EnvironmentModifier> EnvironmentModifiers { get; }

    /// <summary>
    /// Target modifiers from target-related factors.
    /// </summary>
    public IReadOnlyList<TargetModifier> TargetModifiers { get; }

    /// <summary>
    /// Total dice pool modification (sum of all dice modifiers).
    /// </summary>
    /// <remarks>
    /// Positive values add dice to the pool, negative values remove dice.
    /// The final pool size is clamped to minimum 1 die.
    /// </remarks>
    public int TotalDiceModifier { get; }

    /// <summary>
    /// Total difficulty class modification (sum of all DC modifiers).
    /// </summary>
    /// <remarks>
    /// Positive values increase difficulty, negative values decrease it.
    /// The final DC is clamped to minimum 0.
    /// </remarks>
    public int TotalDcModifier { get; }

    /// <summary>
    /// Status effects to apply based on check outcome.
    /// </summary>
    /// <remarks>
    /// These statuses may be applied on success, failure, or specific outcomes
    /// depending on the skill check type. Status effect IDs reference definitions
    /// in the status effect system.
    /// </remarks>
    public IReadOnlyList<string> AppliedStatuses { get; }

    /// <summary>
    /// Gets whether this context has any modifiers.
    /// </summary>
    public bool HasModifiers =>
        EquipmentModifiers.Count > 0 ||
        SituationalModifiers.Count > 0 ||
        EnvironmentModifiers.Count > 0 ||
        TargetModifiers.Count > 0;

    /// <summary>
    /// Gets the total count of all modifiers.
    /// </summary>
    public int ModifierCount =>
        EquipmentModifiers.Count +
        SituationalModifiers.Count +
        EnvironmentModifiers.Count +
        TargetModifiers.Count;

    /// <summary>
    /// Creates a new skill context with the specified modifiers.
    /// </summary>
    public SkillContext(
        IReadOnlyList<EquipmentModifier> equipmentModifiers,
        IReadOnlyList<SituationalModifier> situationalModifiers,
        IReadOnlyList<EnvironmentModifier> environmentModifiers,
        IReadOnlyList<TargetModifier> targetModifiers,
        IReadOnlyList<string> appliedStatuses)
    {
        EquipmentModifiers = equipmentModifiers;
        SituationalModifiers = situationalModifiers;
        EnvironmentModifiers = environmentModifiers;
        TargetModifiers = targetModifiers;
        AppliedStatuses = appliedStatuses;

        TotalDiceModifier = CalculateTotalDiceModifier();
        TotalDcModifier = CalculateTotalDcModifier();
    }

    /// <summary>
    /// Creates an empty skill context with no modifiers.
    /// </summary>
    public static SkillContext Empty { get; } = new SkillContext(
        Array.Empty<EquipmentModifier>(),
        Array.Empty<SituationalModifier>(),
        Array.Empty<EnvironmentModifier>(),
        Array.Empty<TargetModifier>(),
        Array.Empty<string>());

    /// <summary>
    /// Calculates the total dice modifier from all sources.
    /// </summary>
    private int CalculateTotalDiceModifier()
    {
        var total = 0;

        foreach (var mod in EquipmentModifiers)
            total += mod.DiceModifier;

        foreach (var mod in SituationalModifiers)
            total += mod.DiceModifier;

        foreach (var mod in EnvironmentModifiers)
            total += mod.DiceModifier;

        foreach (var mod in TargetModifiers)
            total += mod.DiceModifier;

        return total;
    }

    /// <summary>
    /// Calculates the total DC modifier from all sources.
    /// </summary>
    private int CalculateTotalDcModifier()
    {
        var total = 0;

        foreach (var mod in EquipmentModifiers)
            total += mod.DcModifier;

        foreach (var mod in SituationalModifiers)
            total += mod.DcModifier;

        foreach (var mod in EnvironmentModifiers)
            total += mod.DcModifier;

        foreach (var mod in TargetModifiers)
            total += mod.DcModifier;

        return total;
    }

    /// <summary>
    /// Gets all modifiers as a flat list for iteration.
    /// </summary>
    /// <returns>All modifiers from all categories.</returns>
    public IEnumerable<ISkillModifier> GetAllModifiers()
    {
        foreach (var mod in EquipmentModifiers)
            yield return mod;

        foreach (var mod in SituationalModifiers)
            yield return mod;

        foreach (var mod in EnvironmentModifiers)
            yield return mod;

        foreach (var mod in TargetModifiers)
            yield return mod;
    }

    /// <summary>
    /// Gets modifiers by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>Modifiers in the specified category.</returns>
    public IEnumerable<ISkillModifier> GetModifiersByCategory(ModifierCategory category)
    {
        return category switch
        {
            ModifierCategory.Equipment => EquipmentModifiers.Cast<ISkillModifier>(),
            ModifierCategory.Situational => SituationalModifiers.Cast<ISkillModifier>(),
            ModifierCategory.Environment => EnvironmentModifiers.Cast<ISkillModifier>(),
            ModifierCategory.Target => TargetModifiers.Cast<ISkillModifier>(),
            _ => Enumerable.Empty<ISkillModifier>()
        };
    }

    /// <summary>
    /// Returns a human-readable breakdown of all modifiers.
    /// </summary>
    /// <returns>Formatted modifier description string.</returns>
    /// <example>
    /// "Equipment: Tinker's Toolkit (+2d10), Magnifying Glass (+1d10)
    ///  Situational: Time Pressure (-1d10)
    ///  Environment: Stable Surface (+1d10), Dim Lighting (DC +1)
    ///  Total: +3d10, DC +1"
    /// </example>
    public string ToDescription()
    {
        if (!HasModifiers)
            return "No modifiers";

        var lines = new List<string>();

        if (EquipmentModifiers.Count > 0)
        {
            var descriptions = EquipmentModifiers.Select(m => m.ToShortDescription());
            lines.Add($"Equipment: {string.Join(", ", descriptions)}");
        }

        if (SituationalModifiers.Count > 0)
        {
            var descriptions = SituationalModifiers.Select(m => m.ToShortDescription());
            lines.Add($"Situational: {string.Join(", ", descriptions)}");
        }

        if (EnvironmentModifiers.Count > 0)
        {
            var descriptions = EnvironmentModifiers.Select(m => m.ToShortDescription());
            lines.Add($"Environment: {string.Join(", ", descriptions)}");
        }

        if (TargetModifiers.Count > 0)
        {
            var descriptions = TargetModifiers.Select(m => m.ToShortDescription());
            lines.Add($"Target: {string.Join(", ", descriptions)}");
        }

        var totalDice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        var totalDc = TotalDcModifier != 0
            ? (TotalDcModifier > 0 ? $", DC +{TotalDcModifier}" : $", DC {TotalDcModifier}")
            : "";

        lines.Add($"Total: {totalDice}{totalDc}");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Returns a compact single-line summary.
    /// </summary>
    /// <example>
    /// "+3d10, DC +1 (4 modifiers)"
    /// </example>
    public override string ToString()
    {
        if (!HasModifiers)
            return "No modifiers";

        var dice = TotalDiceModifier >= 0 ? $"+{TotalDiceModifier}d10" : $"{TotalDiceModifier}d10";
        var dc = TotalDcModifier != 0
            ? (TotalDcModifier > 0 ? $", DC +{TotalDcModifier}" : $", DC {TotalDcModifier}")
            : "";

        return $"{dice}{dc} ({ModifierCount} modifier{(ModifierCount != 1 ? "s" : "")})";
    }
}
