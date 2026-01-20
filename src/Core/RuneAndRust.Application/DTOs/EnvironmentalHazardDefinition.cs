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
    /// Gets the dice notation for hazard damage.
    /// </summary>
    /// <remarks>
    /// Standard dice notation (e.g., "2d6", "3d6", "2d4").
    /// Rolled when damage is applied on entry or per turn.
    /// </remarks>
    public string DamageDice { get; init; } = "1d6";

    /// <summary>
    /// Gets the type of damage dealt by the hazard.
    /// </summary>
    /// <remarks>
    /// Standard damage type string (e.g., "fire", "piercing", "bludgeoning", "acid").
    /// Used for resistance/vulnerability calculations.
    /// </remarks>
    public string DamageType { get; init; } = "fire";

    /// <summary>
    /// Gets the identifier of the status effect applied by this hazard.
    /// </summary>
    /// <remarks>
    /// <para>Optional. References a status effect definition (e.g., "burning", "bleeding", "prone").</para>
    /// <para>When set, the status effect is applied when damage is dealt.</para>
    /// <para>Null if the hazard does not apply a status effect.</para>
    /// </remarks>
    public string? StatusEffectId { get; init; }

    /// <summary>
    /// Gets whether the hazard deals damage when a combatant enters the cell.
    /// </summary>
    /// <remarks>
    /// <para>When true, damage is applied immediately when a combatant is pushed/knocked into the hazard
    /// or moves into it voluntarily.</para>
    /// <para>Most hazards have this set to true.</para>
    /// </remarks>
    public bool DamageOnEnter { get; init; } = true;

    /// <summary>
    /// Gets whether the hazard deals damage each turn the combatant remains in the cell.
    /// </summary>
    /// <remarks>
    /// <para>When true, damage is applied at the start of each turn while the combatant remains in the hazard.</para>
    /// <para>Examples: Lava and acid pools deal per-turn damage; spikes and pits typically do not.</para>
    /// </remarks>
    public bool DamagePerTurn { get; init; }

    /// <summary>
    /// Gets whether this hazard requires a climb action to escape.
    /// </summary>
    /// <remarks>
    /// <para>When true, combatants who fall into this hazard cannot simply walk out
    /// and must use a climb action or receive help to escape.</para>
    /// <para>Primarily used for pits and chasms.</para>
    /// </remarks>
    public bool RequiresClimbOut { get; init; }

    /// <summary>
    /// Gets whether this hazard degrades armor over time.
    /// </summary>
    /// <remarks>
    /// <para>When true, each turn in the hazard may reduce the effectiveness of equipped armor.</para>
    /// <para>Primarily used for acid pools.</para>
    /// </remarks>
    public bool DegradesArmor { get; init; }

    /// <summary>
    /// Gets the optional icon path for UI display.
    /// </summary>
    /// <remarks>
    /// Relative path to the hazard icon image, if available.
    /// </remarks>
    public string? IconPath { get; init; }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a new environmental hazard definition with the specified properties.
    /// </summary>
    /// <param name="type">The hazard type.</param>
    /// <param name="name">The display name.</param>
    /// <param name="damageDice">The damage dice notation.</param>
    /// <param name="damageType">The damage type.</param>
    /// <param name="damageOnEnter">Whether damage is dealt on entry.</param>
    /// <param name="damagePerTurn">Whether damage is dealt per turn.</param>
    /// <param name="statusEffectId">Optional status effect ID.</param>
    /// <param name="requiresClimbOut">Whether climbing is required to escape.</param>
    /// <param name="degradesArmor">Whether the hazard degrades armor.</param>
    /// <returns>A new EnvironmentalHazardDefinition.</returns>
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
        return new EnvironmentalHazardDefinition
        {
            Type = type,
            Name = name,
            DamageDice = damageDice,
            DamageType = damageType,
            DamageOnEnter = damageOnEnter,
            DamagePerTurn = damagePerTurn,
            StatusEffectId = statusEffectId,
            RequiresClimbOut = requiresClimbOut,
            DegradesArmor = degradesArmor
        };
    }

    /// <summary>
    /// Gets the source tag for modifier tracking (e.g., "hazard:lava").
    /// </summary>
    /// <returns>A source identifier string for this hazard.</returns>
    /// <remarks>
    /// Used when applying status effects or modifiers to track their source.
    /// </remarks>
    public string GetModifierSource() => $"hazard:{Type.ToString().ToLowerInvariant()}";

    /// <summary>
    /// Checks whether this hazard has any damage dealing capability.
    /// </summary>
    /// <returns>True if the hazard can deal damage on entry or per turn.</returns>
    public bool DealsDamage() => DamageOnEnter || DamagePerTurn;

    /// <summary>
    /// Checks whether this hazard applies a status effect.
    /// </summary>
    /// <returns>True if the hazard has an associated status effect.</returns>
    public bool AppliesStatusEffect() => !string.IsNullOrEmpty(StatusEffectId);
}
