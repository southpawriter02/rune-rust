// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrRageThreshold.cs
// Value object defining specific thresholds for the Berserkr's Rage levels,
// including minimum Rage required, associated effects, and Corruption risk.
// Named BerserkrRageThreshold to avoid collision with the v0.18.4d
// RageThreshold enum (Enums/RageThreshold.cs).
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines a specific threshold for a Berserkr Rage level, including the minimum
/// Rage required, associated mechanical effects, and Corruption risk.
/// </summary>
/// <remarks>
/// <para>
/// Each threshold represents a breakpoint in the Berserkr Rage system. When a
/// Berserkr's Rage crosses a threshold, new effects activate. Thresholds
/// at <see cref="RageLevel.Enraged"/> and above carry Corruption risk.
/// </para>
/// <para>
/// This is distinct from <see cref="Domain.Enums.RageThreshold"/> (v0.18.4d),
/// which defines thresholds for the general Trauma Economy Rage system.
/// </para>
/// <example>
/// <code>
/// var thresholds = BerserkrRageThreshold.CreateStandardThresholds();
/// var enraged = thresholds.First(t => t.Level == RageLevel.Enraged);
/// // enraged.MinimumRage = 80, enraged.CorruptionRisk = 1
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="RageLevel"/>
/// <seealso cref="RageResource"/>
public sealed record BerserkrRageThreshold
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The Rage level this threshold represents.
    /// </summary>
    public RageLevel Level { get; init; }

    /// <summary>
    /// Minimum Rage points required to reach this threshold.
    /// </summary>
    public int MinimumRage { get; init; }

    /// <summary>
    /// Human-readable description of the effects active at this threshold.
    /// </summary>
    public string EffectDescription { get; init; } = string.Empty;

    /// <summary>
    /// Corruption points risked per ability usage at this threshold.
    /// Zero for non-Heretical thresholds.
    /// </summary>
    public int CorruptionRisk { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the standard set of 6 Berserkr Rage thresholds.
    /// </summary>
    /// <returns>Array of all standard thresholds from Calm to Berserk.</returns>
    public static BerserkrRageThreshold[] CreateStandardThresholds() =>
    [
        new BerserkrRageThreshold
        {
            Level = RageLevel.Calm,
            MinimumRage = 0,
            EffectDescription = "No bonuses or penalties. The Berserkr is composed.",
            CorruptionRisk = 0
        },
        new BerserkrRageThreshold
        {
            Level = RageLevel.Irritated,
            MinimumRage = 20,
            EffectDescription = "Minor combat awareness. The battle fury stirs.",
            CorruptionRisk = 0
        },
        new BerserkrRageThreshold
        {
            Level = RageLevel.Angry,
            MinimumRage = 40,
            EffectDescription = "Growing aggression. Strikes carry mounting force.",
            CorruptionRisk = 0
        },
        new BerserkrRageThreshold
        {
            Level = RageLevel.Furious,
            MinimumRage = 60,
            EffectDescription = "Significant combat bonuses. Reckless intensity.",
            CorruptionRisk = 0
        },
        new BerserkrRageThreshold
        {
            Level = RageLevel.Enraged,
            MinimumRage = 80,
            EffectDescription = "Peak power. Abilities may trigger Corruption.",
            CorruptionRisk = 1
        },
        new BerserkrRageThreshold
        {
            Level = RageLevel.Berserk,
            MinimumRage = 100,
            EffectDescription = "Maximum power and Corruption risk. Consumed by fury.",
            CorruptionRisk = 2
        }
    ];

    // ─────────────────────────────────────────────────────────────────────────
    // Query Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the given Rage value meets this threshold.
    /// </summary>
    /// <param name="currentRage">Current Rage points to check.</param>
    /// <returns><c>true</c> if Rage is at or above <see cref="MinimumRage"/>.</returns>
    public bool IsMet(int currentRage) => currentRage >= MinimumRage;

    /// <summary>
    /// Gets a formatted description of this threshold for UI display.
    /// </summary>
    /// <returns>Formatted threshold description.</returns>
    public string GetDescription() =>
        $"{Level} ({MinimumRage}+ Rage): {EffectDescription}";

    /// <summary>
    /// Gets a Corruption warning message if this threshold carries risk.
    /// </summary>
    /// <returns>Warning string, or empty if no Corruption risk.</returns>
    public string GetCorruptionWarning() => CorruptionRisk > 0
        ? $"Warning: +{CorruptionRisk} Corruption risk per ability at {Level} ({MinimumRage}+ Rage)"
        : string.Empty;

    /// <summary>
    /// Returns a diagnostic representation of this threshold.
    /// </summary>
    public override string ToString() =>
        $"BerserkrRageThreshold({Level}, Min={MinimumRage}, Corruption={CorruptionRisk})";
}
