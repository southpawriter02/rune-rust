using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a Seiðkona-specific Corruption risk evaluation.
/// Extends the Berserkr pattern with probability-based d100 roll tracking.
/// </summary>
/// <remarks>
/// <para>Unlike the Berserkr's deterministic Corruption system (always triggers at 80+ Rage),
/// the Seiðkona uses probability-based d100 checks against a percentage threshold that
/// scales with Aether Resonance level. This result captures both the probability context
/// and the actual roll outcome.</para>
/// <para>Corruption risk is evaluated BEFORE resource spending, consistent with the
/// Berserkr pattern (v0.20.5a). The evaluation uses the current Resonance level
/// (before any gain from the cast).</para>
/// <para>Key differences from <see cref="BerserkrCorruptionRiskResult"/>:</para>
/// <list type="bullet">
/// <item><see cref="RollResult"/>: The actual d100 roll value (1–100)</item>
/// <item><see cref="RiskPercent"/>: The threshold percentage at time of check (0, 5, 15, or 25)</item>
/// <item>Safe results may include roll data via <see cref="CreateSafeWithRoll"/> when a check
///   was performed but the roll exceeded the threshold</item>
/// </list>
/// </remarks>
public sealed record SeidkonaCorruptionRiskResult
{
    /// <summary>
    /// Amount of Corruption to apply (0 if no Corruption triggered).
    /// Standard Seiðkona Corruption is +1 per trigger; capstone is +2.
    /// </summary>
    public int CorruptionAmount { get; init; }

    /// <summary>
    /// The specific trigger that caused this Corruption evaluation.
    /// Null if no trigger was applicable (safe action or below Corruption threshold).
    /// </summary>
    public SeidkonaCorruptionTrigger? Trigger { get; init; }

    /// <summary>
    /// Human-readable reason describing why Corruption was or was not triggered.
    /// Always populated for logging and player feedback.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// The actual d100 roll result (1–100). Zero if no roll was performed
    /// (e.g., Resonance below 5 means no check needed, or ability is inherently safe).
    /// </summary>
    public int RollResult { get; init; }

    /// <summary>
    /// The Corruption risk percentage threshold at the time of evaluation.
    /// Zero if the ability doesn't trigger Corruption checks or Resonance is below 5.
    /// Values: 0 (safe), 5 (Risky), 15 (Dangerous), 25 (Critical).
    /// </summary>
    public int RiskPercent { get; init; }

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
    /// Used when the d100 roll falls within the risk threshold.
    /// </summary>
    /// <param name="amount">The amount of Corruption to apply. Must be positive.</param>
    /// <param name="trigger">The specific trigger that caused this Corruption.</param>
    /// <param name="reason">Human-readable reason for the Corruption trigger.</param>
    /// <param name="rollResult">The actual d100 roll result (1–100).</param>
    /// <param name="riskPercent">The risk percentage threshold at time of check.</param>
    /// <returns>A triggered Corruption risk result with full roll context.</returns>
    public static SeidkonaCorruptionRiskResult CreateTriggered(
        int amount,
        SeidkonaCorruptionTrigger trigger,
        string reason,
        int rollResult,
        int riskPercent)
    {
        return new SeidkonaCorruptionRiskResult
        {
            CorruptionAmount = Math.Max(amount, 0),
            Trigger = trigger,
            Reason = reason,
            RollResult = rollResult,
            RiskPercent = riskPercent,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a Corruption risk result indicating the action was safe.
    /// Used when no Corruption check is needed (Resonance below 5, passive ability, etc.).
    /// </summary>
    /// <param name="reason">Human-readable reason why no Corruption was triggered.</param>
    /// <returns>A safe (non-triggered) Corruption risk result with no roll data.</returns>
    public static SeidkonaCorruptionRiskResult CreateSafe(string reason)
    {
        return new SeidkonaCorruptionRiskResult
        {
            CorruptionAmount = 0,
            Trigger = null,
            Reason = reason,
            RollResult = 0,
            RiskPercent = 0,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a safe Corruption risk result that includes roll data.
    /// Used when a d100 check was performed but the roll exceeded the threshold —
    /// the Seiðkona dodged Corruption this time.
    /// </summary>
    /// <param name="reason">Human-readable reason describing the successful check.</param>
    /// <param name="rollResult">The actual d100 roll result (1–100) that exceeded the threshold.</param>
    /// <param name="riskPercent">The risk percentage threshold that was checked against.</param>
    /// <returns>A safe result with roll context for display and logging.</returns>
    public static SeidkonaCorruptionRiskResult CreateSafeWithRoll(
        string reason,
        int rollResult,
        int riskPercent)
    {
        return new SeidkonaCorruptionRiskResult
        {
            CorruptionAmount = 0,
            Trigger = null,
            Reason = reason,
            RollResult = rollResult,
            RiskPercent = riskPercent,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a warning message suitable for display to the player.
    /// Includes d100 roll context when available.
    /// </summary>
    /// <returns>
    /// A warning string including Corruption amount, trigger, and roll context if triggered;
    /// a "safe" message with optional roll context otherwise.
    /// </returns>
    public string GetWarning()
    {
        if (IsTriggered)
        {
            return $"WARNING: Corruption +{CorruptionAmount} — {Reason} " +
                   $"(Roll: {RollResult} vs {RiskPercent}% threshold, Trigger: {Trigger})";
        }

        return RollResult > 0
            ? $"Safe — {Reason} (Roll: {RollResult} vs {RiskPercent}% threshold)"
            : $"Safe — {Reason}";
    }

    /// <summary>
    /// Gets a player-facing description of this Corruption evaluation.
    /// </summary>
    /// <returns>A formatted string for UI display or combat log.</returns>
    public string GetDescriptionForPlayer()
    {
        if (!IsTriggered)
        {
            return RollResult > 0
                ? $"The Aether surges but your will holds. " +
                  $"(d100: {RollResult}, needed ≤{RiskPercent} to corrupt)"
                : "No Corruption risk detected.";
        }

        return $"The Aether tears at your soul as dark forces take notice. " +
               $"Corruption increased by {CorruptionAmount}. " +
               $"(d100: {RollResult} ≤ {RiskPercent}% threshold — {Reason})";
    }
}
