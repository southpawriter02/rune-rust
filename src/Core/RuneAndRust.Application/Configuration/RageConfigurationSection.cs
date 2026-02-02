// ═══════════════════════════════════════════════════════════════════════════════
// RageConfigurationSection.cs
// Configuration records for Rage resource system (Berserker specialization).
// Version: 0.18.4f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration section for the Rage resource system.
/// </summary>
/// <remarks>
/// <para>
/// Rage is a volatile combat resource for the Berserker specialization,
/// powered by violence and pain. Higher rage provides damage bonuses but
/// may force aggressive behavior at extreme thresholds.
/// </para>
/// </remarks>
public record RageConfigurationSection
{
    /// <summary>
    /// Gets the maximum rage value.
    /// </summary>
    /// <remarks>Default: 100.</remarks>
    public int MaxValue { get; init; } = 100;

    /// <summary>
    /// Gets the minimum rage value.
    /// </summary>
    /// <remarks>Default: 0.</remarks>
    public int MinValue { get; init; } = 0;

    /// <summary>
    /// Gets the rage decay per non-combat turn.
    /// </summary>
    /// <remarks>Default: 10 per turn.</remarks>
    public int DecayPerTurn { get; init; } = 10;

    /// <summary>
    /// Gets the minutes before non-combat decay begins.
    /// </summary>
    /// <remarks>Default: 1 minute after last combat action.</remarks>
    public int DecayMinutesBeforeNonCombat { get; init; } = 1;

    /// <summary>
    /// Gets the list of rage threshold configurations.
    /// </summary>
    /// <remarks>
    /// Contains 5 thresholds: Calm, Simmering, Burning, BerserkFury, FrenzyBeyondReason.
    /// </remarks>
    public IReadOnlyList<RageThresholdConfig> Thresholds { get; init; } = [];

    /// <summary>
    /// Gets the dictionary of rage source configurations.
    /// </summary>
    /// <remarks>
    /// Keys: takingDamage, dealingDamage, allyDamaged, enemyKill, rageMaintenance.
    /// </remarks>
    public IReadOnlyDictionary<string, ResourceSourceConfig> Sources { get; init; } =
        new Dictionary<string, ResourceSourceConfig>();

    /// <summary>
    /// Gets the threshold configuration for a given rage value.
    /// </summary>
    /// <param name="rageValue">The current rage value (0-100).</param>
    /// <returns>The threshold config for that value, or null if not found.</returns>
    public RageThresholdConfig? GetThresholdForValue(int rageValue)
    {
        return Thresholds.FirstOrDefault(t =>
            rageValue >= t.MinValue && rageValue <= t.MaxValue);
    }
}

/// <summary>
/// Configuration for a single rage threshold tier.
/// </summary>
/// <remarks>
/// Each threshold defines the bonuses and behavioral effects at that rage level.
/// </remarks>
public record RageThresholdConfig
{
    /// <summary>
    /// Gets the threshold name.
    /// </summary>
    /// <remarks>
    /// One of: Calm, Simmering, Burning, BerserkFury, FrenzyBeyondReason.
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum rage value for this threshold.
    /// </summary>
    public int MinValue { get; init; }

    /// <summary>
    /// Gets the maximum rage value for this threshold.
    /// </summary>
    public int MaxValue { get; init; }

    /// <summary>
    /// Gets the damage bonus at this threshold.
    /// </summary>
    /// <remarks>Added to all damage dealt by the Berserker.</remarks>
    public int DamageBonus { get; init; }

    /// <summary>
    /// Gets the soak (damage reduction) bonus at this threshold.
    /// </summary>
    /// <remarks>Reduces incoming damage by this flat amount.</remarks>
    public int SoakBonus { get; init; }

    /// <summary>
    /// Gets whether the character is immune to fear at this threshold.
    /// </summary>
    /// <remarks>Only true at FrenzyBeyondReason (81-100).</remarks>
    public bool FearImmune { get; init; }

    /// <summary>
    /// Gets whether the character must attack the nearest target.
    /// </summary>
    /// <remarks>True at BerserkFury (61+) and FrenzyBeyondReason (81+).</remarks>
    public bool MustAttackNearest { get; init; }

    /// <summary>
    /// Gets the party stress reduction bonus at this threshold.
    /// </summary>
    /// <remarks>
    /// Only available at FrenzyBeyondReason. Reduces party stress by this
    /// amount at next rest. Null if not applicable.
    /// </remarks>
    public int? PartyStressReduction { get; init; }
}

/// <summary>
/// Configuration for a resource generation source.
/// </summary>
/// <remarks>
/// Defines how a particular event generates resource points.
/// Supports flat values, formulas, and variable ranges.
/// </remarks>
public record ResourceSourceConfig
{
    /// <summary>
    /// Gets the source type.
    /// </summary>
    /// <remarks>
    /// One of: "flat" (fixed value), "formula" (calculated), "variable" (range).
    /// </remarks>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the flat value for "flat" type sources.
    /// </summary>
    public int? Value { get; init; }

    /// <summary>
    /// Gets the formula expression for "formula" type sources.
    /// </summary>
    /// <remarks>
    /// Example: "floor(damage / 5)" for rage from taking damage.
    /// </remarks>
    public string? Formula { get; init; }

    /// <summary>
    /// Gets the minimum value for "variable" type sources.
    /// </summary>
    public int? MinValue { get; init; }

    /// <summary>
    /// Gets the maximum value for "variable" type sources.
    /// </summary>
    public int? MaxValue { get; init; }

    /// <summary>
    /// Gets the description of this source.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
