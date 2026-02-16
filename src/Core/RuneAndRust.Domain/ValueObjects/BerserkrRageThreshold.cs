using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a Rage threshold breakpoint for the Berserkr specialization.
/// Each threshold defines the minimum Rage required, its gameplay effects,
/// and associated Corruption risk.
/// </summary>
/// <remarks>
/// <para>Named <c>BerserkrRageThreshold</c> to avoid collision with the existing
/// <c>RageThreshold</c> enum from v0.18.4d.</para>
/// <para>Standard thresholds provide escalating Attack bonuses and
/// Corruption risk at higher Rage levels. The Enraged (80+) threshold
/// is the critical point where most abilities trigger Corruption evaluation.</para>
/// </remarks>
public sealed record BerserkrRageThreshold
{
    /// <summary>
    /// The Rage level classification for this threshold.
    /// </summary>
    public RageLevel Level { get; init; }

    /// <summary>
    /// The minimum Rage value required to reach this threshold.
    /// </summary>
    public int MinimumRage { get; init; }

    /// <summary>
    /// Human-readable description of the gameplay effects at this threshold.
    /// </summary>
    public string EffectDescription { get; init; } = string.Empty;

    /// <summary>
    /// The Corruption risk level associated with this threshold.
    /// Higher values indicate greater risk of Corruption accumulation.
    /// </summary>
    /// <remarks>
    /// Risk levels: 0 = None, 1 = Low, 2 = Medium, 3 = High.
    /// </remarks>
    public int CorruptionRisk { get; init; }

    /// <summary>
    /// The Attack bonus granted at this threshold level.
    /// </summary>
    public int AttackBonus { get; init; }

    /// <summary>
    /// Creates the standard set of Berserkr Rage thresholds.
    /// </summary>
    /// <returns>An array of all 6 standard threshold breakpoints, ordered by Rage level.</returns>
    public static BerserkrRageThreshold[] CreateStandardThresholds()
    {
        return
        [
            new BerserkrRageThreshold
            {
                Level = RageLevel.Calm,
                MinimumRage = 0,
                AttackBonus = 0,
                CorruptionRisk = 0,
                EffectDescription = "Baseline state — no Rage-based bonuses"
            },
            new BerserkrRageThreshold
            {
                Level = RageLevel.Irritated,
                MinimumRage = 20,
                AttackBonus = 1,
                CorruptionRisk = 0,
                EffectDescription = "Minor heightened awareness — +1 Attack"
            },
            new BerserkrRageThreshold
            {
                Level = RageLevel.Angry,
                MinimumRage = 40,
                AttackBonus = 2,
                CorruptionRisk = 0,
                EffectDescription = "Growing fury — +2 Attack"
            },
            new BerserkrRageThreshold
            {
                Level = RageLevel.Furious,
                MinimumRage = 60,
                AttackBonus = 3,
                CorruptionRisk = 1,
                EffectDescription = "Dangerous intensity — +3 Attack, minor Corruption risk"
            },
            new BerserkrRageThreshold
            {
                Level = RageLevel.Enraged,
                MinimumRage = 80,
                AttackBonus = 4,
                CorruptionRisk = 2,
                EffectDescription = "Overwhelming fury — +4 Attack, significant Corruption risk on ability use"
            },
            new BerserkrRageThreshold
            {
                Level = RageLevel.Berserk,
                MinimumRage = 100,
                AttackBonus = 5,
                CorruptionRisk = 3,
                EffectDescription = "Maximum Rage — +5 Attack, highest Corruption risk on all actions"
            }
        ];
    }

    /// <summary>
    /// Checks if a given Rage value meets this threshold.
    /// </summary>
    /// <param name="currentRage">The current Rage value to evaluate.</param>
    /// <returns>True if <paramref name="currentRage"/> is at or above <see cref="MinimumRage"/>.</returns>
    public bool IsMet(int currentRage)
    {
        return currentRage >= MinimumRage;
    }

    /// <summary>
    /// Gets a formatted description of this threshold for display purposes.
    /// </summary>
    /// <returns>A string describing the threshold level, Rage range, and effects.</returns>
    public string GetDescription()
    {
        return $"[{Level}] Rage {MinimumRage}+: {EffectDescription} (Attack +{AttackBonus})";
    }

    /// <summary>
    /// Gets a Corruption risk warning message for this threshold.
    /// </summary>
    /// <returns>
    /// A warning string if Corruption risk is non-zero;
    /// "No Corruption risk" if risk is zero.
    /// </returns>
    public string GetCorruptionWarning()
    {
        return CorruptionRisk switch
        {
            0 => "No Corruption risk",
            1 => "Low Corruption risk — minor chance on sustained actions",
            2 => "Medium Corruption risk — ability use may trigger Corruption",
            3 => "High Corruption risk — most actions will trigger Corruption",
            _ => $"Corruption risk level: {CorruptionRisk}"
        };
    }
}
