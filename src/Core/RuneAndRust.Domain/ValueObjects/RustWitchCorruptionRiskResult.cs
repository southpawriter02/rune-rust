using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a Rust-Witch-specific Corruption evaluation.
/// Unlike the Seiðkona's probability-based system, the Rust-Witch uses deterministic
/// self-Corruption: every active ability always inflicts a fixed amount.
/// </summary>
/// <remarks>
/// <para>The Rust-Witch's Corruption model is deterministic (like the Berserkr), not
/// probability-based (like the Seiðkona). There is no d100 roll. Every active ability
/// has a guaranteed self-Corruption cost that decreases slightly at Rank 3 for Tier 1-2
/// abilities but remains fixed for Tier 3 and Capstone.</para>
///
/// <para>Self-Corruption amounts by ability and rank:</para>
/// <list type="table">
///   <listheader><term>Ability</term><description>R1/R2 → R3</description></listheader>
///   <item><term>Corrosive Curse</term><description>+2 → +1</description></item>
///   <item><term>System Shock</term><description>+3 → +2</description></item>
///   <item><term>Flash Rust</term><description>+4 → +3</description></item>
///   <item><term>Unmaking Word</term><description>+4 (all ranks)</description></item>
///   <item><term>Entropic Cascade</term><description>+6 (all ranks)</description></item>
/// </list>
///
/// <para>Corruption is evaluated BEFORE resource spending, consistent with the
/// system-wide pattern established by the Berserkr (v0.20.5a). However, since the
/// Rust-Witch's Corruption is deterministic, the "evaluation" is effectively just
/// looking up the fixed cost for the ability and rank.</para>
/// </remarks>
public sealed record RustWitchCorruptionRiskResult
{
    /// <summary>
    /// Amount of self-Corruption inflicted by this ability cast.
    /// Always positive for active abilities. Zero for passive abilities or failed casts.
    /// </summary>
    public int CorruptionAmount { get; init; }

    /// <summary>
    /// The specific trigger that caused this Corruption.
    /// Null if no Corruption was applied (passive ability, failed cast, etc.).
    /// </summary>
    public RustWitchCorruptionTrigger? Trigger { get; init; }

    /// <summary>
    /// Human-readable reason describing the Corruption outcome.
    /// Always populated for logging and player feedback.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// The rank of the ability at time of cast (1, 2, or 3).
    /// Affects the Corruption amount for Tier 1-2 abilities (R3 reduces by 1).
    /// </summary>
    public int AbilityRank { get; init; }

    /// <summary>
    /// Gets whether this evaluation resulted in Corruption accumulation.
    /// Always true for active ability casts (deterministic model).
    /// </summary>
    public bool IsTriggered => CorruptionAmount > 0;

    /// <summary>
    /// UTC timestamp when this evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; init; }

    /// <summary>
    /// Creates a Corruption result for a successful active ability cast.
    /// </summary>
    /// <param name="amount">The fixed Corruption amount for this ability at this rank.</param>
    /// <param name="trigger">The specific trigger identifying which ability caused the Corruption.</param>
    /// <param name="reason">Human-readable description of the Corruption source.</param>
    /// <param name="rank">The ability rank at time of cast (1, 2, or 3).</param>
    /// <returns>A triggered Corruption result with deterministic amount.</returns>
    public static RustWitchCorruptionRiskResult CreateTriggered(
        int amount,
        RustWitchCorruptionTrigger trigger,
        string reason,
        int rank)
    {
        return new RustWitchCorruptionRiskResult
        {
            CorruptionAmount = Math.Max(amount, 0),
            Trigger = trigger,
            Reason = reason,
            AbilityRank = rank,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a safe (no Corruption) result for passive abilities or failed casts.
    /// </summary>
    /// <param name="reason">Human-readable reason why no Corruption was applied.</param>
    /// <returns>A non-triggered Corruption result.</returns>
    public static RustWitchCorruptionRiskResult CreateSafe(string reason)
    {
        return new RustWitchCorruptionRiskResult
        {
            CorruptionAmount = 0,
            Trigger = null,
            Reason = reason,
            AbilityRank = 0,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a warning message suitable for display to the player.
    /// </summary>
    /// <returns>
    /// A warning string describing the deterministic Corruption cost,
    /// or a safe message if no Corruption was applied.
    /// </returns>
    public string GetWarning()
    {
        if (IsTriggered)
        {
            return $"WARNING: Self-Corruption +{CorruptionAmount} — {Reason} " +
                   $"(Rank {AbilityRank}, Trigger: {Trigger})";
        }

        return $"Safe — {Reason}";
    }

    /// <summary>
    /// Gets a player-facing description of this Corruption evaluation.
    /// </summary>
    /// <returns>A formatted string for UI display or combat log.</returns>
    public string GetDescriptionForPlayer()
    {
        if (!IsTriggered)
        {
            return "No self-Corruption incurred.";
        }

        return $"The entropic energies corrode your essence. " +
               $"Self-Corruption increased by {CorruptionAmount}. " +
               $"(Deterministic cost for {Trigger} at Rank {AbilityRank})";
    }
}
