using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Defines the properties and effects of an environmental combat hazard.
/// </summary>
/// <remarks>
/// <para>EnvironmentalHazardDefinition contains all configuration data for hazards
/// that can affect combatants during tactical combat, such as lava, spikes, pits, and acid pools.</para>
/// <para>Hazard definitions are loaded from JSON configuration and used by
/// <see cref="RuneAndRust.Application.Interfaces.IEnvironmentalCombatService"/> to determine
/// damage, status effects, and behavior when combatants enter or remain in hazardous cells.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item><description>Configurable damage dice and damage type</description></item>
///   <item><description>Optional status effect application</description></item>
///   <item><description>Damage on entry and/or per-turn options</description></item>
///   <item><description>Special behaviors (climb out, armor degradation)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="HazardType"/>
/// <seealso cref="RuneAndRust.Application.Interfaces.IEnvironmentalHazardProvider"/>
public record EnvironmentalHazardDefinition
{
    /// <summary>
    /// Gets the type of environmental hazard.
    /// </summary>
    /// <remarks>
    /// Maps to the <see cref="HazardType"/> enum (e.g., Lava, Spikes, Pit, AcidPool).
    /// </remarks>
    public HazardType Type { get; init; }

    /// <summary>
    /// Gets the display name of the hazard.
    /// </summary>
    /// <remarks>
    /// Human-readable name shown in combat logs and UI (e.g., "Lava", "Spike Trap", "Acid Pool").
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of the hazard for display purposes.
    /// </summary>
    /// <remarks>
    /// Optional flavor text describing the hazard's appearance or effect.
    /// </remarks>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the optional primary damage definition.
    /// </summary>
    /// <remarks>
    /// Used for backward compatibility or simple hazards.
    /// </remarks>
    public DamageDefinition? Damage { get; init; }

    /// <summary>
    /// Gets the damage dealt on entry.
    /// </summary>
    public DamageDefinition? EntryDamage { get; init; }

    /// <summary>
    /// Gets the damage dealt per turn.
    /// </summary>
    public DamageDefinition? TurnDamage { get; init; }

    /// <summary>
    /// Gets the dice notation for hazard damage.
    /// </summary>
    [Obsolete("Use EntryDamage or TurnDamage instead.")]
    public string DamageDice { get; init; } = "1d6";

    /// <summary>
    /// Gets the type of damage dealt by the hazard.
    /// </summary>
    [Obsolete("Use EntryDamage or TurnDamage instead.")]
    public string DamageType { get; init; } = "fire";

    /// <summary>
    /// Gets the identifier of the status effect applied by this hazard.
    /// </summary>
    public string? StatusEffectId { get; init; }

    /// <summary>
    /// Gets whether the hazard deals damage when a combatant enters the cell.
    /// </summary>
    public bool DamageOnEnter { get; init; } = true;

    /// <summary>
    /// Gets whether the hazard deals damage each turn the combatant remains in the cell.
    /// </summary>
    public bool DamagePerTurn { get; init; }

    /// <summary>
    /// Gets whether this hazard requires a climb action to escape.
    /// </summary>
    public bool RequiresClimbOut { get; init; }

    /// <summary>
    /// Gets whether this hazard degrades armor over time.
    /// </summary>
    public bool DegradesArmor { get; init; }

    /// <summary>
    /// Gets the optional icon path for UI display.
    /// </summary>
    public string? IconPath { get; init; }

    // ===== Factory Methods =====

    public static EnvironmentalHazardDefinition Create(
        HazardType type,
        string name,
        string damageDice,
        string damageType,
        bool damageOnEnter = true,
        bool damagePerTurn = false,
        string? statusEffectId = null,
        bool requiresClimbOut = false,
        bool degradesArmor = false)
    {
        var damageDef = new DamageDefinition { Dice = damageDice, DamageType = damageType };
#pragma warning disable CS0618 // Type or member is obsolete
        return new EnvironmentalHazardDefinition
        {
            Type = type,
            Name = name,
            DamageDice = damageDice,
            DamageType = damageType,
            EntryDamage = damageOnEnter ? damageDef : null,
            TurnDamage = damagePerTurn ? damageDef : null,
#pragma warning restore CS0618 // Type or member is obsolete
            DamageOnEnter = damageOnEnter,
            DamagePerTurn = damagePerTurn,
            StatusEffectId = statusEffectId,
            RequiresClimbOut = requiresClimbOut,
            DegradesArmor = degradesArmor
        };
    }

    public string GetModifierSource() => $"hazard:{Type.ToString().ToLowerInvariant()}";

    public bool DealsDamage() => DamageOnEnter || DamagePerTurn;

    public bool AppliesStatusEffect() => !string.IsNullOrEmpty(StatusEffectId);
}

/// <summary>
/// Defines a damage profile for a hazard.
/// </summary>
public record DamageDefinition
{
    public string Dice { get; init; } = "1d6";
    public string DamageType { get; init; } = "physical";
    public int Bonus { get; init; }
}
