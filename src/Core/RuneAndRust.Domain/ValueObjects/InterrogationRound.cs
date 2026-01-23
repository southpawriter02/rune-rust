// ------------------------------------------------------------------------------
// <copyright file="InterrogationRound.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Records the outcome of a single interrogation round including
// the method used, check result, and all changes applied.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Records the outcome of a single interrogation round including
/// the method used, check result, and all changes applied.
/// </summary>
/// <remarks>
/// <para>
/// Each interrogation round involves the interrogator selecting a method,
/// making a skill check (based on the method), and seeing the resulting
/// effect on the subject's resistance.
/// </para>
/// <para>
/// Resistance change rules:
/// <list type="bullet">
///   <item><description>Success: -1 resistance (subject's will weakens)</description></item>
///   <item><description>Critical Success: -2 resistance (major breakthrough)</description></item>
///   <item><description>Failure: No change (subject holds firm)</description></item>
///   <item><description>Fumble: Method-specific consequences (see FumbleType)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record InterrogationRound
{
    /// <summary>
    /// Gets the round number (1-based).
    /// </summary>
    public required int RoundNumber { get; init; }

    /// <summary>
    /// Gets the interrogation method used this round.
    /// </summary>
    public required InterrogationMethod MethodUsed { get; init; }

    /// <summary>
    /// Gets the outcome of the skill check.
    /// </summary>
    public required SkillOutcome CheckResult { get; init; }

    /// <summary>
    /// Gets the number of dice rolled.
    /// </summary>
    public required int DiceRolled { get; init; }

    /// <summary>
    /// Gets the number of successes achieved on the roll.
    /// </summary>
    public required int SuccessesAchieved { get; init; }

    /// <summary>
    /// Gets the number of successes required (based on DC).
    /// </summary>
    public required int SuccessesRequired { get; init; }

    /// <summary>
    /// Gets the change to resistance (negative on success, 0 on failure).
    /// </summary>
    /// <remarks>
    /// Resistance changes are always 0 or negative. Positive values would
    /// indicate the subject's will strengthening, which does not occur in
    /// standard interrogation mechanics.
    /// </remarks>
    public required int ResistanceChange { get; init; }

    /// <summary>
    /// Gets the resistance remaining after this round.
    /// </summary>
    /// <remarks>
    /// When this reaches 0, the subject's status changes to SubjectBroken.
    /// </remarks>
    public required int ResistanceAfter { get; init; }

    /// <summary>
    /// Gets the disposition change from this round's method.
    /// </summary>
    /// <remarks>
    /// Disposition changes affect the subject's relationship with the
    /// interrogator. Negative values indicate relationship damage that
    /// may affect future interactions.
    /// </remarks>
    public required int DispositionChange { get; init; }

    /// <summary>
    /// Gets the resource cost (for Bribery method).
    /// </summary>
    /// <remarks>
    /// Gold spent during this round if Bribery was used. Zero for other methods.
    /// </remarks>
    public required int ResourceCost { get; init; }

    /// <summary>
    /// Gets the time elapsed for this round in minutes.
    /// </summary>
    /// <remarks>
    /// Duration varies by method:
    /// <list type="bullet">
    ///   <item><description>Good Cop: 30 minutes</description></item>
    ///   <item><description>Bad Cop: 15 minutes</description></item>
    ///   <item><description>Deception: 20 minutes</description></item>
    ///   <item><description>Bribery: 10 minutes</description></item>
    ///   <item><description>Torture: 60 minutes</description></item>
    /// </list>
    /// </remarks>
    public required int TimeElapsedMinutes { get; init; }

    /// <summary>
    /// Gets a value indicating whether a fumble occurred this round.
    /// </summary>
    public required bool IsFumble { get; init; }

    /// <summary>
    /// Gets the fumble type if a fumble occurred, otherwise null.
    /// </summary>
    public FumbleType? FumbleType { get; init; }

    /// <summary>
    /// Gets a narrative description of what happened this round.
    /// </summary>
    /// <remarks>
    /// Provides flavor text suitable for display to the player,
    /// describing the events of the round in narrative terms.
    /// </remarks>
    public required string NarrativeDescription { get; init; }

    /// <summary>
    /// Gets a value indicating whether this round was successful.
    /// </summary>
    /// <remarks>
    /// Success means the subject's resistance was reduced. Includes
    /// MarginalSuccess and higher outcomes.
    /// </remarks>
    public bool IsSuccess => CheckResult >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Gets a value indicating whether this round was a failure.
    /// </summary>
    /// <remarks>
    /// Failure means the subject's resistance was not reduced (excluding fumbles).
    /// </remarks>
    public bool IsFailure => CheckResult == SkillOutcome.Failure;

    /// <summary>
    /// Gets a value indicating whether resistance was reduced this round.
    /// </summary>
    public bool ReducedResistance => ResistanceChange < 0;

    /// <summary>
    /// Gets a value indicating whether this round incurred resource cost.
    /// </summary>
    public bool HasResourceCost => ResourceCost > 0;

    /// <summary>
    /// Gets a value indicating whether this round damaged disposition.
    /// </summary>
    public bool DamagedDisposition => DispositionChange < 0;

    /// <summary>
    /// Gets a formatted summary of this round for display.
    /// </summary>
    /// <returns>A compact summary string describing the round outcome.</returns>
    public string GetSummary()
    {
        var outcomeText = IsFumble
            ? $"FUMBLE - {FumbleType?.GetDisplayName() ?? "Unknown"}"
            : CheckResult.ToString();

        return $"Round {RoundNumber}: {MethodUsed.GetDisplayName()} - {outcomeText}. " +
               $"Resistance: {ResistanceAfter}";
    }

    /// <summary>
    /// Gets a detailed summary including all costs and changes.
    /// </summary>
    /// <returns>A detailed multi-line summary of the round.</returns>
    public string GetDetailedSummary()
    {
        var lines = new List<string>
        {
            $"Round {RoundNumber}: {MethodUsed.GetDisplayName()}",
            $"  Outcome: {CheckResult}",
            $"  Roll: {SuccessesAchieved}/{SuccessesRequired} successes ({DiceRolled}d10)",
            $"  Resistance: {(ResistanceChange != 0 ? ResistanceChange.ToString("+0;-0") : "unchanged")} → {ResistanceAfter}"
        };

        if (HasResourceCost)
        {
            lines.Add($"  Gold spent: {ResourceCost}");
        }

        if (DamagedDisposition)
        {
            lines.Add($"  Disposition: {DispositionChange:+0;-0}");
        }

        lines.Add($"  Time: {TimeElapsedMinutes} minutes");

        if (IsFumble && FumbleType.HasValue)
        {
            lines.Add($"  ⚠️ FUMBLE: {FumbleType.Value.GetDisplayName()}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>A very compact representation of the round.</returns>
    public string ToShortDisplay()
    {
        var indicator = IsSuccess ? "✓" : (IsFumble ? "✗✗" : "✗");
        return $"R{RoundNumber}: {MethodUsed.GetDisplayName()[0..3]} {indicator} Res:{ResistanceAfter}";
    }

    /// <summary>
    /// Creates a round record for a completed check.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    /// <param name="method">The method used.</param>
    /// <param name="outcome">The check outcome.</param>
    /// <param name="diceRolled">Number of dice rolled.</param>
    /// <param name="successesAchieved">Successes achieved.</param>
    /// <param name="successesRequired">Successes required.</param>
    /// <param name="resistanceChange">Change to resistance.</param>
    /// <param name="resistanceAfter">Resistance after this round.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <returns>A new <see cref="InterrogationRound"/> recording the outcome.</returns>
    public static InterrogationRound ForCheck(
        int roundNumber,
        InterrogationMethod method,
        SkillOutcome outcome,
        int diceRolled,
        int successesAchieved,
        int successesRequired,
        int resistanceChange,
        int resistanceAfter,
        string narrative)
    {
        return new InterrogationRound
        {
            RoundNumber = roundNumber,
            MethodUsed = method,
            CheckResult = outcome,
            DiceRolled = diceRolled,
            SuccessesAchieved = successesAchieved,
            SuccessesRequired = successesRequired,
            ResistanceChange = resistanceChange,
            ResistanceAfter = resistanceAfter,
            DispositionChange = method.GetDispositionChangePerRound(),
            ResourceCost = method.RequiresResources() ? method.GetBaseDc() : 0, // Placeholder, real cost calculated in service
            TimeElapsedMinutes = method.GetRoundDurationMinutes(),
            IsFumble = outcome == SkillOutcome.CriticalFailure,
            FumbleType = outcome == SkillOutcome.CriticalFailure ? method.GetFumbleType() : null,
            NarrativeDescription = narrative
        };
    }

    /// <summary>
    /// Creates a round record for a fumble.
    /// </summary>
    /// <param name="roundNumber">The round number (1-based).</param>
    /// <param name="method">The method used.</param>
    /// <param name="diceRolled">Number of dice rolled.</param>
    /// <param name="resistanceAfter">Resistance after this round.</param>
    /// <param name="fumbleType">The type of fumble that occurred.</param>
    /// <param name="narrative">Narrative description.</param>
    /// <returns>A new <see cref="InterrogationRound"/> recording the fumble.</returns>
    public static InterrogationRound ForFumble(
        int roundNumber,
        InterrogationMethod method,
        int diceRolled,
        int resistanceAfter,
        FumbleType fumbleType,
        string narrative)
    {
        return new InterrogationRound
        {
            RoundNumber = roundNumber,
            MethodUsed = method,
            CheckResult = SkillOutcome.CriticalFailure,
            DiceRolled = diceRolled,
            SuccessesAchieved = 0,
            SuccessesRequired = 0,
            ResistanceChange = 0, // Fumbles don't reduce resistance
            ResistanceAfter = resistanceAfter,
            DispositionChange = method.GetDispositionChangePerRound() * 2, // Double disposition damage on fumble
            ResourceCost = 0,
            TimeElapsedMinutes = method.GetRoundDurationMinutes(),
            IsFumble = true,
            FumbleType = fumbleType,
            NarrativeDescription = narrative
        };
    }
}
