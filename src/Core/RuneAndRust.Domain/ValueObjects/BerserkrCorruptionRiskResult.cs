// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrCorruptionRiskResult.cs
// Value object encapsulating the outcome of a Corruption risk evaluation
// for Berserkr (Heretical path) abilities. Distinct from CorruptionRiskResult
// which is Myrk-gengr-specific (light-level-based).
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the outcome of a Corruption risk evaluation for a
/// Berserkr ability activation.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the Myrk-gengr's <see cref="CorruptionRiskResult"/> (which is triggered by
/// light conditions), Berserkr Corruption is triggered by Rage levels. Corruption
/// risk escalates when the Berserkr uses abilities while at 80+ Rage (Enraged/Berserk).
/// </para>
/// <para>
/// Common triggers for the Berserkr specialization:
/// </para>
/// <list type="bullet">
///   <item><description>Using Fury Strike while Enraged: +1 Corruption</description></item>
///   <item><description>Entering combat while Enraged: +1 Corruption</description></item>
///   <item><description>Passive abilities (Blood Scent, Pain is Fuel): No Corruption risk</description></item>
/// </list>
/// <example>
/// <code>
/// var risk = BerserkrCorruptionRiskResult.CreateTriggered(
///     corruptionAmount: 1,
///     trigger: BerserkrCorruptionTrigger.FuryStrikeWhileEnraged,
///     reason: "Fury Strike used at 85 Rage");
///
/// if (risk.IsTriggered)
///     Console.WriteLine(risk.GetWarning());
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="BerserkrCorruptionTrigger"/>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="RageLevel"/>
public sealed record BerserkrCorruptionRiskResult
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Amount of Corruption gained (0 if not triggered).
    /// </summary>
    public int CorruptionAmount { get; private init; }

    /// <summary>
    /// The specific trigger type, if Corruption was triggered.
    /// </summary>
    public BerserkrCorruptionTrigger? Trigger { get; private init; }

    /// <summary>
    /// Human-readable reason for the corruption evaluation outcome.
    /// </summary>
    public string Reason { get; private init; } = string.Empty;

    /// <summary>
    /// Whether this result indicates that Corruption was triggered.
    /// </summary>
    public bool IsTriggered => CorruptionAmount > 0;

    /// <summary>
    /// When this evaluation occurred.
    /// </summary>
    public DateTime EvaluatedAt { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a result indicating Corruption was triggered.
    /// </summary>
    /// <param name="corruptionAmount">Amount of Corruption gained. Must be positive.</param>
    /// <param name="trigger">The specific Corruption trigger type.</param>
    /// <param name="reason">Description of why Corruption was triggered.</param>
    /// <returns>A new triggered corruption result.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="corruptionAmount"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reason"/> is null or whitespace.</exception>
    public static BerserkrCorruptionRiskResult CreateTriggered(
        int corruptionAmount,
        BerserkrCorruptionTrigger trigger,
        string reason)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(corruptionAmount, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new BerserkrCorruptionRiskResult
        {
            CorruptionAmount = corruptionAmount,
            Trigger = trigger,
            Reason = reason,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a result indicating no Corruption was triggered (safe usage).
    /// </summary>
    /// <param name="reason">Description of why this usage was safe.</param>
    /// <returns>A new safe (no corruption) result.</returns>
    public static BerserkrCorruptionRiskResult CreateSafe(string reason = "No corruption risk in current conditions")
    {
        return new BerserkrCorruptionRiskResult
        {
            CorruptionAmount = 0,
            Trigger = null,
            Reason = reason,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a formatted warning message for this corruption risk.
    /// </summary>
    /// <returns>Warning string for game UI, or empty if no risk.</returns>
    public string GetWarning() => IsTriggered
        ? $"Corruption Warning: +{CorruptionAmount} ({Reason})"
        : string.Empty;

    /// <summary>
    /// Formats a player-facing description of the corruption outcome.
    /// </summary>
    /// <returns>A formatted string suitable for game UI display.</returns>
    public string GetDescriptionForPlayer()
    {
        if (!IsTriggered)
            return "Your fury remains within your control.";

        return CorruptionAmount switch
        {
            1 => "The fury burns through your restraint, tainting your spirit.",
            2 => "Rage floods your being unchecked, corruption seeping deep.",
            >= 3 => "Your fury consumes all reason — darkness rushes in to fill the void.",
            _ => "The rage stirs uneasily within."
        };
    }

    /// <summary>
    /// Returns a diagnostic representation of this result.
    /// </summary>
    public override string ToString() =>
        $"BerserkrCorruption(Triggered={IsTriggered}, Amount={CorruptionAmount}, " +
        $"Trigger={Trigger}, Reason={Reason})";
}
