// ═══════════════════════════════════════════════════════════════════════════════
// MomentumConfigurationSection.cs
// Configuration records for Momentum resource system (Storm Blade specialization).
// Version: 0.18.4f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration section for the Momentum resource system.
/// </summary>
/// <remarks>
/// <para>
/// Momentum is a flow-state resource for the Storm Blade specialization
/// that rewards sustained aggression but penalizes interruption.
/// </para>
/// </remarks>
public record MomentumConfigurationSection
{
    /// <summary>
    /// Gets the maximum momentum value.
    /// </summary>
    /// <remarks>Default: 100.</remarks>
    public int MaxValue { get; init; } = 100;

    /// <summary>
    /// Gets the minimum momentum value.
    /// </summary>
    /// <remarks>Default: 0.</remarks>
    public int MinValue { get; init; } = 0;

    /// <summary>
    /// Gets the momentum decay on a missed attack.
    /// </summary>
    /// <remarks>Default: 25 flat decay. Also breaks the hit chain.</remarks>
    public int DecayOnMiss { get; init; } = 25;

    /// <summary>
    /// Gets the momentum decay when stunned.
    /// </summary>
    /// <remarks>Default: 100 (full reset). Complete momentum loss.</remarks>
    public int DecayOnStun { get; init; } = 100;

    /// <summary>
    /// Gets the momentum decay per idle turn (no attack action).
    /// </summary>
    /// <remarks>Default: 15 per turn. Encourages aggressive play.</remarks>
    public int DecayOnIdleTurn { get; init; } = 15;

    /// <summary>
    /// Gets the list of momentum threshold configurations.
    /// </summary>
    /// <remarks>
    /// Contains 5 thresholds: Stationary, Moving, Flowing, Surging, Unstoppable.
    /// </remarks>
    public IReadOnlyList<MomentumThresholdConfig> Thresholds { get; init; } = [];

    /// <summary>
    /// Gets the dictionary of momentum source configurations.
    /// </summary>
    /// <remarks>
    /// Keys: successfulAttack, killingBlow, chainAttack, movement.
    /// </remarks>
    public IReadOnlyDictionary<string, ResourceSourceConfig> Sources { get; init; } =
        new Dictionary<string, ResourceSourceConfig>();

    /// <summary>
    /// Gets the threshold configuration for a given momentum value.
    /// </summary>
    /// <param name="momentumValue">The current momentum value (0-100).</param>
    /// <returns>The threshold config for that value, or null if not found.</returns>
    public MomentumThresholdConfig? GetThresholdForValue(int momentumValue)
    {
        return Thresholds.FirstOrDefault(t =>
            momentumValue >= t.MinValue && momentumValue <= t.MaxValue);
    }
}

/// <summary>
/// Configuration for a single momentum threshold tier.
/// </summary>
/// <remarks>
/// Each threshold defines the combat bonuses at that momentum level.
/// </remarks>
public record MomentumThresholdConfig
{
    /// <summary>
    /// Gets the threshold name.
    /// </summary>
    /// <remarks>
    /// One of: Stationary, Moving, Flowing, Surging, Unstoppable.
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum momentum value for this threshold.
    /// </summary>
    public int MinValue { get; init; }

    /// <summary>
    /// Gets the maximum momentum value for this threshold.
    /// </summary>
    public int MaxValue { get; init; }

    /// <summary>
    /// Gets the number of bonus attacks at this threshold.
    /// </summary>
    /// <remarks>
    /// Scale: 0 (Stationary/Moving), 1 (Flowing/Surging), 2 (Unstoppable).
    /// </remarks>
    public int BonusAttacks { get; init; }

    /// <summary>
    /// Gets the attack bonus at this threshold.
    /// </summary>
    public int AttackBonus { get; init; }

    /// <summary>
    /// Gets the defense bonus at this threshold.
    /// </summary>
    public int DefenseBonus { get; init; }

    /// <summary>
    /// Gets the critical hit chance bonus at this threshold.
    /// </summary>
    /// <remarks>Only available at Unstoppable (81+). Null otherwise.</remarks>
    public int? CriticalChance { get; init; }

    /// <summary>
    /// Gets the movement bonus per 20 momentum.
    /// </summary>
    /// <remarks>Default: 1 extra movement per 20 momentum.</remarks>
    public int MovementBonusPerTwenty { get; init; } = 1;

    /// <summary>
    /// Gets whether the character heals on kill at this threshold.
    /// </summary>
    /// <remarks>Only true at Unstoppable (81-100).</remarks>
    public bool HealOnKill { get; init; }
}
