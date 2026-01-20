using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of applying environmental hazard damage to a combatant.
/// </summary>
/// <remarks>
/// <para>HazardDamageResult encapsulates all information about damage dealt by environmental
/// hazards such as lava, spikes, pits, and acid pools during tactical combat.</para>
/// <para>Key information provided:</para>
/// <list type="bullet">
///   <item><description>Damage amount and type dealt</description></item>
///   <item><description>Status effect applied (if any)</description></item>
///   <item><description>Hazard type and position</description></item>
///   <item><description>Whether the combatant was killed by the hazard</description></item>
/// </list>
/// </remarks>
/// <seealso cref="HazardType"/>
/// <seealso cref="EnvironmentalHazardDefinition"/>
public record HazardDamageResult
{
    /// <summary>
    /// Gets the type of hazard that caused the damage.
    /// </summary>
    public HazardType HazardType { get; init; }

    /// <summary>
    /// Gets the display name of the hazard.
    /// </summary>
    /// <remarks>
    /// Human-readable name for combat log display (e.g., "Lava", "Spike Trap").
    /// </remarks>
    public string HazardName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the grid position where the hazard damage occurred.
    /// </summary>
    public GridPosition Position { get; init; }

    /// <summary>
    /// Gets the amount of damage dealt by the hazard.
    /// </summary>
    /// <remarks>
    /// Result of rolling the hazard's damage dice (e.g., 2d6 for spikes).
    /// Zero if damage was fully resisted or prevented.
    /// </remarks>
    public int DamageDealt { get; init; }

    /// <summary>
    /// Gets the type of damage dealt (e.g., "fire", "piercing", "bludgeoning").
    /// </summary>
    /// <remarks>
    /// Used for resistance/vulnerability calculations and combat log display.
    /// </remarks>
    public string DamageType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the dice notation that was rolled for damage.
    /// </summary>
    /// <remarks>
    /// For combat log display (e.g., "2d6", "3d6").
    /// </remarks>
    public string DamageDice { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether a status effect was applied.
    /// </summary>
    public bool StatusEffectApplied { get; init; }

    /// <summary>
    /// Gets the identifier of the status effect that was applied.
    /// </summary>
    /// <remarks>
    /// Null if no status effect was applied. Examples: "burning", "bleeding", "prone".
    /// </remarks>
    public string? AppliedStatusEffectId { get; init; }

    /// <summary>
    /// Gets whether the hazard damage was dealt on entry to the cell.
    /// </summary>
    /// <remarks>
    /// True for initial entry damage, false for per-turn tick damage.
    /// </remarks>
    public bool WasEntryDamage { get; init; }

    /// <summary>
    /// Gets whether the combatant was killed by this hazard damage.
    /// </summary>
    public bool WasLethal { get; init; }

    /// <summary>
    /// Gets the combatant's remaining health after the hazard damage.
    /// </summary>
    public int RemainingHealth { get; init; }

    /// <summary>
    /// Gets whether armor was degraded by the hazard (acid pools).
    /// </summary>
    public bool ArmorDegraded { get; init; }

    /// <summary>
    /// Gets whether the combatant is trapped and requires climbing to escape.
    /// </summary>
    /// <remarks>
    /// True for pits and similar hazards that require a climb action to escape.
    /// </remarks>
    public bool IsTrapped { get; init; }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a hazard damage result for entry into a hazard cell.
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <param name="hazardName">The display name of the hazard.</param>
    /// <param name="position">The grid position of the hazard.</param>
    /// <param name="damageDealt">The amount of damage dealt.</param>
    /// <param name="damageType">The type of damage.</param>
    /// <param name="damageDice">The dice notation rolled.</param>
    /// <param name="remainingHealth">The combatant's health after damage.</param>
    /// <param name="statusEffectId">Optional status effect applied.</param>
    /// <param name="isTrapped">Whether the combatant is trapped.</param>
    /// <returns>A HazardDamageResult for entry damage.</returns>
    public static HazardDamageResult EntryDamage(
        HazardType hazardType,
        string hazardName,
        GridPosition position,
        int damageDealt,
        string damageType,
        string damageDice,
        int remainingHealth,
        string? statusEffectId = null,
        bool isTrapped = false)
    {
        return new HazardDamageResult
        {
            HazardType = hazardType,
            HazardName = hazardName,
            Position = position,
            DamageDealt = damageDealt,
            DamageType = damageType,
            DamageDice = damageDice,
            WasEntryDamage = true,
            RemainingHealth = remainingHealth,
            WasLethal = remainingHealth <= 0,
            StatusEffectApplied = !string.IsNullOrEmpty(statusEffectId),
            AppliedStatusEffectId = statusEffectId,
            IsTrapped = isTrapped
        };
    }

    /// <summary>
    /// Creates a hazard damage result for per-turn tick damage.
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <param name="hazardName">The display name of the hazard.</param>
    /// <param name="position">The grid position of the hazard.</param>
    /// <param name="damageDealt">The amount of damage dealt.</param>
    /// <param name="damageType">The type of damage.</param>
    /// <param name="damageDice">The dice notation rolled.</param>
    /// <param name="remainingHealth">The combatant's health after damage.</param>
    /// <param name="armorDegraded">Whether armor was degraded (acid).</param>
    /// <returns>A HazardDamageResult for tick damage.</returns>
    public static HazardDamageResult TickDamage(
        HazardType hazardType,
        string hazardName,
        GridPosition position,
        int damageDealt,
        string damageType,
        string damageDice,
        int remainingHealth,
        bool armorDegraded = false)
    {
        return new HazardDamageResult
        {
            HazardType = hazardType,
            HazardName = hazardName,
            Position = position,
            DamageDealt = damageDealt,
            DamageType = damageType,
            DamageDice = damageDice,
            WasEntryDamage = false,
            RemainingHealth = remainingHealth,
            WasLethal = remainingHealth <= 0,
            ArmorDegraded = armorDegraded
        };
    }

    /// <summary>
    /// Creates a result indicating no damage was dealt (hazard missed or resisted).
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <param name="hazardName">The display name of the hazard.</param>
    /// <param name="position">The grid position of the hazard.</param>
    /// <param name="remainingHealth">The combatant's current health.</param>
    /// <returns>A HazardDamageResult with zero damage.</returns>
    public static HazardDamageResult NoDamage(
        HazardType hazardType,
        string hazardName,
        GridPosition position,
        int remainingHealth)
    {
        return new HazardDamageResult
        {
            HazardType = hazardType,
            HazardName = hazardName,
            Position = position,
            DamageDealt = 0,
            RemainingHealth = remainingHealth
        };
    }
}
