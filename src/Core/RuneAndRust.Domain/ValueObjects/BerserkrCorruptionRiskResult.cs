using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a Berserkr-specific Corruption risk evaluation.
/// Returned by the Corruption service to indicate whether an action triggered
/// Corruption accumulation and the amount to apply.
/// </summary>
/// <remarks>
/// <para>Corruption risk is evaluated BEFORE resource spending to ensure the player
/// sees the risk assessment before committing to the action. Key design principle:
/// inform before commit.</para>
/// <para>Corruption amounts vary by trigger:</para>
/// <list type="bullet">
/// <item>Passive abilities: always 0 (safe)</item>
/// <item>Active abilities at 80+ Rage: typically +1</item>
/// <item>Capstone activation: +2</item>
/// <item>Coherent-aligned target kills while Enraged: +1</item>
/// </list>
/// </remarks>
public sealed record BerserkrCorruptionRiskResult
{
    /// <summary>
    /// Amount of Corruption to apply (0 if no Corruption triggered).
    /// </summary>
    public int CorruptionAmount { get; init; }

    /// <summary>
    /// The specific trigger that caused this Corruption evaluation.
    /// Null if no trigger was applicable (safe action).
    /// </summary>
    public BerserkrCorruptionTrigger? Trigger { get; init; }

    /// <summary>
    /// Human-readable reason describing why Corruption was or was not triggered.
    /// Always populated for logging and player feedback.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this evaluation resulted in Corruption accumulation.
    /// True when <see cref="CorruptionAmount"/> is greater than zero.
    /// </summary>
    public bool IsTriggered => CorruptionAmount > 0;

    /// <summary>
    /// UTC timestamp when this risk evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; init; }

    /// <summary>
    /// Creates a Corruption risk result indicating Corruption was triggered.
    /// </summary>
    /// <param name="amount">The amount of Corruption to apply. Must be positive.</param>
    /// <param name="trigger">The specific trigger that caused this Corruption.</param>
    /// <param name="reason">Human-readable reason for the Corruption trigger.</param>
    /// <returns>A triggered Corruption risk result.</returns>
    public static BerserkrCorruptionRiskResult CreateTriggered(
        int amount,
        BerserkrCorruptionTrigger trigger,
        string reason)
    {
        return new BerserkrCorruptionRiskResult
        {
            CorruptionAmount = Math.Max(amount, 0),
            Trigger = trigger,
            Reason = reason,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a Corruption risk result indicating the action was safe.
    /// </summary>
    /// <param name="reason">Human-readable reason why no Corruption was triggered.</param>
    /// <returns>A safe (non-triggered) Corruption risk result.</returns>
    public static BerserkrCorruptionRiskResult CreateSafe(string reason)
    {
        return new BerserkrCorruptionRiskResult
        {
            CorruptionAmount = 0,
            Trigger = null,
            Reason = reason,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a warning message suitable for display to the player.
    /// </summary>
    /// <returns>
    /// A warning string including Corruption amount and trigger if triggered;
    /// a "safe" message otherwise.
    /// </returns>
    public string GetWarning()
    {
        return IsTriggered
            ? $"WARNING: Corruption +{CorruptionAmount} — {Reason} (Trigger: {Trigger})"
            : $"Safe — {Reason}";
    }

    /// <summary>
    /// Gets a player-facing description of this Corruption evaluation.
    /// </summary>
    /// <returns>A formatted string for UI display or combat log.</returns>
    public string GetDescriptionForPlayer()
    {
        if (!IsTriggered)
            return "No Corruption risk detected.";

        return $"Your fury draws the attention of darker forces. " +
               $"Corruption increased by {CorruptionAmount}. ({Reason})";
    }
}
