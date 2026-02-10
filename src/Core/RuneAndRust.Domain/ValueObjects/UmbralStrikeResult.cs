// ═══════════════════════════════════════════════════════════════════════════════
// UmbralStrikeResult.cs
// Immutable value object encapsulating the outcome of an Umbral Strike
// attack, including hit/miss, damage breakdown, and corruption data.
// Version: 0.20.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates the complete outcome of an Umbral Strike execution.
/// </summary>
/// <remarks>
/// <para>
/// Umbral Strike is a shadow-infused melee attack that deals weapon damage
/// plus 2d6 shadow damage. On critical hits, an additional 1d6 shadow damage
/// is added. If a shadow clone is consumed, the strike gains advantage
/// (roll twice, take higher) and adds 1d4 bonus shadow damage.
/// </para>
/// <para>Possible outcomes:</para>
/// <list type="bullet">
///   <item><description><b>Hit:</b> Attack roll meets or exceeds target defense</description></item>
///   <item><description><b>Miss:</b> Attack roll is below target defense</description></item>
///   <item><description><b>Failure:</b> Prerequisites not met (not in shadow, no essence)</description></item>
/// </list>
/// <example>
/// <code>
/// var hit = UmbralStrikeResult.CreateHit(18, 13, breakdown, false, true, null, resource);
/// // hit.IsHit == true, hit.Damage.GetTotal() == 16
///
/// var miss = UmbralStrikeResult.CreateMiss(8, 13, resource);
/// // miss.IsHit == false
///
/// var fail = UmbralStrikeResult.CreateFailure("Not in shadow", resource);
/// // fail.IsHit == false, fail.FailureReason == "Not in shadow"
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="DamageBreakdown"/>
/// <seealso cref="CorruptionRiskResult"/>
public sealed record UmbralStrikeResult
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Whether the attack hit the target.</summary>
    public bool IsHit { get; private init; }

    /// <summary>The d20 attack roll (with modifiers).</summary>
    public int AttackRoll { get; private init; }

    /// <summary>Target's defense value.</summary>
    public int TargetDefense { get; private init; }

    /// <summary>Damage breakdown (physical + shadow + bonus). Null on miss/failure.</summary>
    public DamageBreakdown? Damage { get; private init; }

    /// <summary>Whether the attack was a critical hit (natural 20).</summary>
    public bool IsCritical { get; private init; }

    /// <summary>Whether a shadow clone was consumed for advantage.</summary>
    public bool CloneConsumed { get; private init; }

    /// <summary>Corruption risk result, if applicable.</summary>
    public CorruptionRiskResult? CorruptionResult { get; private init; }

    /// <summary>Updated Shadow Essence resource after ability execution.</summary>
    public ShadowEssenceResource UpdatedResource { get; private init; } = null!;

    /// <summary>Reason for failure, if the ability could not execute.</summary>
    public string? FailureReason { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a result for a successful hit.
    /// </summary>
    public static UmbralStrikeResult CreateHit(
        int attackRoll,
        int targetDefense,
        DamageBreakdown damage,
        bool isCritical,
        bool cloneConsumed,
        CorruptionRiskResult? corruptionResult,
        ShadowEssenceResource updatedResource)
    {
        ArgumentNullException.ThrowIfNull(damage);
        ArgumentNullException.ThrowIfNull(updatedResource);

        return new UmbralStrikeResult
        {
            IsHit = true,
            AttackRoll = attackRoll,
            TargetDefense = targetDefense,
            Damage = damage,
            IsCritical = isCritical,
            CloneConsumed = cloneConsumed,
            CorruptionResult = corruptionResult,
            UpdatedResource = updatedResource
        };
    }

    /// <summary>
    /// Creates a result for a miss (attack roll below defense).
    /// </summary>
    public static UmbralStrikeResult CreateMiss(
        int attackRoll,
        int targetDefense,
        ShadowEssenceResource updatedResource,
        CorruptionRiskResult? corruptionResult = null)
    {
        ArgumentNullException.ThrowIfNull(updatedResource);

        return new UmbralStrikeResult
        {
            IsHit = false,
            AttackRoll = attackRoll,
            TargetDefense = targetDefense,
            Damage = null,
            IsCritical = false,
            CloneConsumed = false,
            CorruptionResult = corruptionResult,
            UpdatedResource = updatedResource
        };
    }

    /// <summary>
    /// Creates a result for a prerequisite failure (e.g., not in shadow).
    /// </summary>
    public static UmbralStrikeResult CreateFailure(
        string failureReason,
        ShadowEssenceResource resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureReason);
        ArgumentNullException.ThrowIfNull(resource);

        return new UmbralStrikeResult
        {
            IsHit = false,
            AttackRoll = 0,
            TargetDefense = 0,
            Damage = null,
            IsCritical = false,
            CloneConsumed = false,
            CorruptionResult = null,
            UpdatedResource = resource,
            FailureReason = failureReason
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the total damage dealt (0 on miss/failure).
    /// </summary>
    public int GetTotalDamage() => Damage?.GetTotal() ?? 0;

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of this result.
    /// </summary>
    public override string ToString() =>
        IsHit
            ? $"UmbralStrike(HIT: {AttackRoll} vs {TargetDefense}, " +
              $"Damage={GetTotalDamage()}, Crit={IsCritical}, Clone={CloneConsumed})"
            : FailureReason != null
                ? $"UmbralStrike(FAILED: {FailureReason})"
                : $"UmbralStrike(MISS: {AttackRoll} vs {TargetDefense})";
}
