using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete result of a contested skill check between two parties.
/// </summary>
/// <remarks>
/// <para>
/// Immutable value object containing both parties' rolls, the outcome,
/// and the margin of victory.
/// </para>
/// <para>
/// The contested check resolution follows this priority:
/// <list type="bullet">
///   <item><description>Fumble handling: fumbles auto-lose (or both fumble = mutual failure)</description></item>
///   <item><description>Net success comparison: higher net wins</description></item>
///   <item><description>Tie handling: initiator gets minor advantage</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly record struct ContestedCheckResult
{
    /// <summary>
    /// Gets the unique identifier of the initiator (active party).
    /// </summary>
    /// <remarks>
    /// The initiator is the party who triggered the contested check.
    /// In ties, the initiator receives minor advantage.
    /// </remarks>
    public string InitiatorId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the defender (passive party).
    /// </summary>
    public string DefenderId { get; init; }

    /// <summary>
    /// Gets the skill ID used by the initiator.
    /// </summary>
    public string InitiatorSkillId { get; init; }

    /// <summary>
    /// Gets the skill ID used by the defender.
    /// </summary>
    public string DefenderSkillId { get; init; }

    /// <summary>
    /// Gets the complete dice roll result for the initiator.
    /// </summary>
    /// <remarks>
    /// Contains the individual dice, net successes, and fumble/critical status.
    /// </remarks>
    public DiceRollResult InitiatorRoll { get; init; }

    /// <summary>
    /// Gets the complete dice roll result for the defender.
    /// </summary>
    public DiceRollResult DefenderRoll { get; init; }

    /// <summary>
    /// Gets the outcome of the contested check.
    /// </summary>
    /// <remarks>
    /// One of: InitiatorWins, DefenderWins, Tie, BothFumble, InitiatorFumble, DefenderFumble
    /// </remarks>
    public ContestedOutcome Outcome { get; init; }

    /// <summary>
    /// Gets the margin of victory (winner's net - loser's net).
    /// </summary>
    /// <remarks>
    /// <para>
    /// For fumble outcomes, the margin is the non-fumbling party's net successes.
    /// For BothFumble and Tie, the margin is 0.
    /// </para>
    /// <para>
    /// Higher margin indicates a more decisive victory.
    /// </para>
    /// </remarks>
    public int Margin { get; init; }

    /// <summary>
    /// Gets the ID of the winner, or null for ties and mutual fumbles.
    /// </summary>
    public string? WinnerId => Outcome switch
    {
        ContestedOutcome.InitiatorWins => InitiatorId,
        ContestedOutcome.DefenderWins => DefenderId,
        ContestedOutcome.InitiatorFumble => DefenderId,
        ContestedOutcome.DefenderFumble => InitiatorId,
        _ => null
    };

    /// <summary>
    /// Gets whether there is a clear winner.
    /// </summary>
    public bool HasWinner => WinnerId != null;

    /// <summary>
    /// Gets whether any party fumbled.
    /// </summary>
    public bool HadFumble => Outcome is ContestedOutcome.BothFumble
        or ContestedOutcome.InitiatorFumble
        or ContestedOutcome.DefenderFumble;

    /// <summary>
    /// Gets whether both parties fumbled.
    /// </summary>
    public bool IsMutualFumble => Outcome == ContestedOutcome.BothFumble;

    /// <summary>
    /// Gets whether this was a tie (no clear winner, initiator gets minor advantage).
    /// </summary>
    public bool IsTie => Outcome == ContestedOutcome.Tie;

    /// <summary>
    /// Gets the initiator's net successes for convenience.
    /// </summary>
    public int InitiatorNetSuccesses => InitiatorRoll.NetSuccesses;

    /// <summary>
    /// Gets the defender's net successes for convenience.
    /// </summary>
    public int DefenderNetSuccesses => DefenderRoll.NetSuccesses;

    /// <summary>
    /// Returns a formatted string describing the contested check result.
    /// </summary>
    /// <example>
    /// "Contested (Stealth vs Perception): Initiator 3 vs Defender 1 → InitiatorWins (margin: 2)"
    /// "Contested (Grapple vs Grapple): Initiator FUMBLE vs Defender 2 → DefenderWins (auto)"
    /// </example>
    public override string ToString()
    {
        var initiatorDisplay = InitiatorRoll.IsFumble
            ? "FUMBLE"
            : InitiatorRoll.NetSuccesses.ToString();
        var defenderDisplay = DefenderRoll.IsFumble
            ? "FUMBLE"
            : DefenderRoll.NetSuccesses.ToString();

        var marginDisplay = Outcome switch
        {
            ContestedOutcome.InitiatorFumble or ContestedOutcome.DefenderFumble => "auto",
            ContestedOutcome.BothFumble => "mutual",
            _ => $"margin: {Margin}"
        };

        return $"Contested ({InitiatorSkillId} vs {DefenderSkillId}): " +
               $"Initiator {initiatorDisplay} vs Defender {defenderDisplay} → " +
               $"{Outcome} ({marginDisplay})";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the contested outcome based on roll results.
    /// </summary>
    /// <param name="initiatorRoll">The initiator's dice roll result.</param>
    /// <param name="defenderRoll">The defender's dice roll result.</param>
    /// <returns>A tuple containing the outcome and margin.</returns>
    /// <remarks>
    /// Resolution priority:
    /// <list type="bullet">
    ///   <item><description>Both fumble → BothFumble, margin 0</description></item>
    ///   <item><description>Initiator fumbles → InitiatorFumble, margin = defender's net</description></item>
    ///   <item><description>Defender fumbles → DefenderFumble, margin = initiator's net</description></item>
    ///   <item><description>Compare net successes</description></item>
    /// </list>
    /// </remarks>
    public static (ContestedOutcome outcome, int margin) DetermineOutcome(
        DiceRollResult initiatorRoll,
        DiceRollResult defenderRoll)
    {
        // Priority 1: Both fumble
        if (initiatorRoll.IsFumble && defenderRoll.IsFumble)
        {
            return (ContestedOutcome.BothFumble, 0);
        }

        // Priority 2: Initiator fumble (defender auto-wins)
        if (initiatorRoll.IsFumble)
        {
            return (ContestedOutcome.InitiatorFumble, defenderRoll.NetSuccesses);
        }

        // Priority 3: Defender fumble (initiator auto-wins)
        if (defenderRoll.IsFumble)
        {
            return (ContestedOutcome.DefenderFumble, initiatorRoll.NetSuccesses);
        }

        // Priority 4: Compare net successes
        int difference = initiatorRoll.NetSuccesses - defenderRoll.NetSuccesses;

        if (difference > 0)
        {
            return (ContestedOutcome.InitiatorWins, difference);
        }

        if (difference < 0)
        {
            return (ContestedOutcome.DefenderWins, -difference);
        }

        return (ContestedOutcome.Tie, 0);
    }
}
