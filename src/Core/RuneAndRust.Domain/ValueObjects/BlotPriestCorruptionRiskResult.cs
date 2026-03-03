using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a Blót-Priest-specific Corruption evaluation.
/// The Blót-Priest uses deterministic self-Corruption like the Rust-Witch and Berserkr,
/// but generates FAR more Corruption due to the sacrificial casting model.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest is the most Corruption-intensive specialization in the system.
/// Nearly every action generates self-Corruption, and several abilities also transfer
/// Corruption to allies (Blight Transference).</para>
///
/// <para>Self-Corruption amounts by ability:</para>
/// <list type="table">
///   <listheader><term>Ability</term><description>Corruption</description></listheader>
///   <item><term>Sacrificial Cast (HP→AP)</term><description>+1 per cast</description></item>
///   <item><term>Blood Siphon</term><description>+1 per cast</description></item>
///   <item><term>Gift of Vitae</term><description>+1 self, +1-2 transferred to ally</description></item>
///   <item><term>Blood Ward</term><description>+1 per cast</description></item>
///   <item><term>Exsanguinate</term><description>+1 per tick (3 total)</description></item>
///   <item><term>Hemorrhaging Curse</term><description>+2 per cast (fixed)</description></item>
///   <item><term>Heartstopper: Crimson Deluge</term><description>+10 self, +5 to each ally</description></item>
///   <item><term>Heartstopper: Final Anathema</term><description>+15 self</description></item>
/// </list>
///
/// <para>Corruption is evaluated BEFORE resource spending, consistent with the
/// system-wide pattern established by the Berserkr (v0.20.5a).</para>
/// </remarks>
public sealed record BlotPriestCorruptionRiskResult
{
    /// <summary>
    /// Amount of self-Corruption inflicted on the Blót-Priest.
    /// Always positive for active abilities. Zero for passive abilities.
    /// </summary>
    public int CorruptionAmount { get; init; }

    /// <summary>
    /// Amount of Corruption transferred to an ally (Blight Transference).
    /// Only applies to Gift of Vitae and Heartstopper: Crimson Deluge.
    /// Zero for offensive abilities and passives.
    /// </summary>
    public int CorruptionTransferred { get; init; }

    /// <summary>
    /// The specific trigger that caused this Corruption.
    /// Null if no Corruption was applied (passive ability, etc.).
    /// </summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>
    /// Human-readable reason describing the Corruption outcome.
    /// Always populated for logging and player feedback.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// The rank of the ability at time of cast (1, 2, or 3).
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
    /// <param name="amount">Self-Corruption amount for the Blót-Priest.</param>
    /// <param name="transferred">Corruption transferred to an ally (0 for offensive abilities).</param>
    /// <param name="trigger">The trigger identifying the Corruption source.</param>
    /// <param name="reason">Human-readable description.</param>
    /// <param name="rank">The ability rank at time of cast (1, 2, or 3).</param>
    /// <returns>A triggered Corruption result.</returns>
    public static BlotPriestCorruptionRiskResult CreateTriggered(
        int amount,
        int transferred,
        BlotPriestCorruptionTrigger trigger,
        string reason,
        int rank)
    {
        return new BlotPriestCorruptionRiskResult
        {
            CorruptionAmount = Math.Max(amount, 0),
            CorruptionTransferred = Math.Max(transferred, 0),
            Trigger = trigger,
            Reason = reason,
            AbilityRank = rank,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a safe (no Corruption) result for passive abilities.
    /// </summary>
    /// <param name="reason">Human-readable reason why no Corruption was applied.</param>
    /// <returns>A non-triggered Corruption result.</returns>
    public static BlotPriestCorruptionRiskResult CreateSafe(string reason)
    {
        return new BlotPriestCorruptionRiskResult
        {
            CorruptionAmount = 0,
            CorruptionTransferred = 0,
            Trigger = null,
            Reason = reason,
            AbilityRank = 0,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a warning message suitable for display to the player.
    /// </summary>
    public string GetWarning()
    {
        if (IsTriggered)
        {
            var transferNote = CorruptionTransferred > 0
                ? $", Transferred: +{CorruptionTransferred} to ally"
                : string.Empty;
            return $"WARNING: Self-Corruption +{CorruptionAmount}{transferNote} — {Reason} " +
                   $"(Rank {AbilityRank}, Trigger: {Trigger})";
        }

        return $"Safe — {Reason}";
    }

    /// <summary>
    /// Gets a player-facing description of this Corruption evaluation.
    /// </summary>
    public string GetDescriptionForPlayer()
    {
        if (!IsTriggered)
        {
            return "No self-Corruption incurred.";
        }

        var transferNote = CorruptionTransferred > 0
            ? $" Your ally absorbs {CorruptionTransferred} Corruption through the Blight Transfer."
            : string.Empty;

        return $"The sacrificial energies corrode your essence. " +
               $"Self-Corruption increased by {CorruptionAmount}.{transferNote} " +
               $"(Deterministic cost for {Trigger} at Rank {AbilityRank})";
    }
}
