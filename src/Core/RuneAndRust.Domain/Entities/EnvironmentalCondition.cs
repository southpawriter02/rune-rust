using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Defines an environmental condition/hazard that affects characters in a realm.
/// </summary>
/// <remarks>
/// <para>
/// EnvironmentalCondition captures the mechanical effects of realm hazards.
/// Each condition specifies the resistance check, damage, and mitigations.
/// </para>
/// <para>
/// Standard mechanics:
/// <list type="bullet">
/// <item>Base DC: 12 (modified by zones from -6 to +8)</item>
/// <item>Check Attribute: Usually STURDINESS or WILL</item>
/// <item>Damage: Applied on failed check</item>
/// <item>Frequency: Per Turn (combat) or Per Hour (exploration)</item>
/// </list>
/// </para>
/// </remarks>
public sealed class EnvironmentalCondition : IEntity
{
    /// <summary>
    /// Gets the unique database identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the condition type.
    /// </summary>
    public EnvironmentalConditionType Type { get; private set; }

    /// <summary>
    /// Gets the display name for UI.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the attribute used for resistance checks.
    /// </summary>
    /// <remarks>
    /// Common values: "STURDINESS", "WILL", "AGILITY".
    /// </remarks>
    public string CheckAttribute { get; private set; }

    /// <summary>
    /// Gets the base DC for resistance checks.
    /// </summary>
    /// <remarks>
    /// Standard: 12, modified by zone DC modifiers.
    /// </remarks>
    public int BaseDc { get; private set; }

    /// <summary>
    /// Gets the damage dice on failed check.
    /// </summary>
    /// <remarks>
    /// Format: "2d6", "1d8", etc. Use "0" or empty for no damage.
    /// </remarks>
    public string DamageDice { get; private set; }

    /// <summary>
    /// Gets the damage type.
    /// </summary>
    /// <remarks>
    /// Common values: "Fire", "Cold", "Poison", "Psychic".
    /// </remarks>
    public string DamageType { get; private set; }

    /// <summary>
    /// Gets the check frequency.
    /// </summary>
    /// <remarks>
    /// Combat: "Per Turn", "Per Round".
    /// Exploration: "Per Hour", "Per Watch".
    /// </remarks>
    public string Frequency { get; private set; }

    /// <summary>
    /// Gets the narrative description of this condition.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the list of mitigation methods.
    /// </summary>
    /// <remarks>
    /// Examples: "Fire Resistance", "Cooling equipment", "Hearth shelter".
    /// </remarks>
    public IReadOnlyList<string> Mitigations { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EnvironmentalCondition()
    {
        DisplayName = null!;
        CheckAttribute = null!;
        DamageDice = null!;
        DamageType = null!;
        Frequency = null!;
        Description = null!;
        Mitigations = [];
    }

    /// <summary>
    /// Creates a new environmental condition.
    /// </summary>
    public static EnvironmentalCondition Create(
        EnvironmentalConditionType type,
        string displayName,
        string checkAttribute,
        int baseDc,
        string damageDice,
        string damageType,
        string frequency,
        string description,
        IReadOnlyList<string>? mitigations = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkAttribute);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDc);

        return new EnvironmentalCondition
        {
            Id = Guid.NewGuid(),
            Type = type,
            DisplayName = displayName,
            CheckAttribute = checkAttribute,
            BaseDc = baseDc,
            DamageDice = damageDice ?? string.Empty,
            DamageType = damageType ?? string.Empty,
            Frequency = frequency ?? "Per Turn",
            Description = description ?? string.Empty,
            Mitigations = mitigations ?? []
        };
    }

    /// <summary>
    /// Gets whether this condition deals damage.
    /// </summary>
    public bool DealsDamage =>
        !string.IsNullOrEmpty(DamageDice) && DamageDice != "0";

    /// <summary>
    /// Gets whether this condition has mitigations available.
    /// </summary>
    public bool HasMitigations => Mitigations.Count > 0;

    /// <summary>
    /// Gets whether checks are required every turn (combat frequency).
    /// </summary>
    public bool IsPerTurn =>
        Frequency.Contains("Turn", StringComparison.OrdinalIgnoreCase) ||
        Frequency.Contains("Round", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the display string for the check requirements.
    /// </summary>
    public string CheckDisplay => $"{CheckAttribute} DC {BaseDc}";

    /// <summary>
    /// Gets a summary string for logging and debugging.
    /// </summary>
    public override string ToString() =>
        DealsDamage
            ? $"Condition[{Type}]: {DisplayName} — {CheckDisplay}, {DamageDice} {DamageType} ({Frequency})"
            : $"Condition[{Type}]: {DisplayName} — {CheckDisplay} ({Frequency})";
}
