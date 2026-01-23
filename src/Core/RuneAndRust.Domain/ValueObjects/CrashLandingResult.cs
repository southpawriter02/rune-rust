namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a Crash Landing attempt to reduce fall damage.
/// </summary>
/// <remarks>
/// <para>
/// Crash Landing allows a skilled character to reduce fall damage by making
/// an Acrobatics check. Each success above the DC reduces damage by 1d10.
/// </para>
/// <para>
/// Success-Counting DC Formula:
/// <list type="bullet">
///   <item><description>DC = 2 + (Height / 10) successes needed</description></item>
///   <item><description>Each success above DC reduces damage by 1d10</description></item>
///   <item><description>Cannot reduce damage below 0d10</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="CrashDc">The DC for the Crash Landing attempt (successes needed).</param>
/// <param name="Successes">The number of successes rolled.</param>
/// <param name="Margin">The margin of success or failure (Successes - DC).</param>
/// <param name="DiceReduced">The number of damage dice reduced (equals margin if positive).</param>
/// <param name="OriginalDamageDice">The original damage dice before reduction.</param>
/// <param name="FinalDamageDice">The final damage dice after reduction.</param>
/// <param name="Outcome">The skill outcome classification.</param>
/// <param name="WasAttempted">Whether a Crash Landing was attempted.</param>
public readonly record struct CrashLandingResult(
    int CrashDc,
    int Successes,
    int Margin,
    int DiceReduced,
    int OriginalDamageDice,
    int FinalDamageDice,
    SkillOutcome Outcome,
    bool WasAttempted = true)
{
    /// <summary>
    /// Gets a value indicating whether the Crash Landing succeeded.
    /// </summary>
    public bool Succeeded => Margin >= 0 && WasAttempted;

    /// <summary>
    /// Gets a value indicating whether any damage was reduced.
    /// </summary>
    public bool ReducedDamage => DiceReduced > 0;

    /// <summary>
    /// Gets a value indicating whether all damage was negated.
    /// </summary>
    public bool NegatedAllDamage => FinalDamageDice <= 0;

    /// <summary>
    /// Gets the percentage of damage reduced.
    /// </summary>
    public double ReductionPercentage => OriginalDamageDice > 0
        ? (double)DiceReduced / OriginalDamageDice * 100
        : 0;

    /// <summary>
    /// Creates a CrashLandingResult from a skill check roll.
    /// </summary>
    /// <param name="crashDc">The DC that was tested against.</param>
    /// <param name="successes">The number of successes rolled.</param>
    /// <param name="originalDamageDice">The original damage dice.</param>
    /// <param name="outcome">The skill outcome classification.</param>
    /// <returns>A new CrashLandingResult with calculated reduction.</returns>
    /// <example>
    /// <code>
    /// // 50ft fall (5d10), DC 7, rolled 9 successes
    /// var result = CrashLandingResult.FromRoll(7, 9, 5, SkillOutcome.FullSuccess);
    /// // Margin = 2, DiceReduced = 2, FinalDice = 3
    /// </code>
    /// </example>
    public static CrashLandingResult FromRoll(
        int crashDc,
        int successes,
        int originalDamageDice,
        SkillOutcome outcome)
    {
        var margin = successes - crashDc;
        var diceReduced = Math.Max(0, margin);
        var finalDice = Math.Max(0, originalDamageDice - diceReduced);

        return new CrashLandingResult(
            CrashDc: crashDc,
            Successes: successes,
            Margin: margin,
            DiceReduced: diceReduced,
            OriginalDamageDice: originalDamageDice,
            FinalDamageDice: finalDice,
            Outcome: outcome,
            WasAttempted: true);
    }

    /// <summary>
    /// Creates a CrashLandingResult when no attempt was made.
    /// </summary>
    /// <param name="originalDamageDice">The original damage dice (unchanged).</param>
    /// <returns>A CrashLandingResult indicating no attempt.</returns>
    public static CrashLandingResult NoAttempt(int originalDamageDice)
    {
        return new CrashLandingResult(
            CrashDc: 0,
            Successes: 0,
            Margin: 0,
            DiceReduced: 0,
            OriginalDamageDice: originalDamageDice,
            FinalDamageDice: originalDamageDice,
            Outcome: SkillOutcome.Failure,
            WasAttempted: false);
    }

    /// <summary>
    /// Creates a CrashLandingResult for [Featherfall] auto-success.
    /// </summary>
    /// <param name="crashDc">The DC that was auto-succeeded.</param>
    /// <param name="originalDamageDice">The original damage dice.</param>
    /// <returns>A CrashLandingResult with marginal success (exactly meets DC).</returns>
    /// <remarks>
    /// [Featherfall] from Gantry-Runner specialization auto-succeeds
    /// Crash Landing DCs of 3 or less, treating the roll as exactly meeting DC.
    /// This provides stability without reducing damage.
    /// </remarks>
    public static CrashLandingResult Featherfall(int crashDc, int originalDamageDice)
    {
        return new CrashLandingResult(
            CrashDc: crashDc,
            Successes: crashDc, // Exactly meets DC
            Margin: 0,
            DiceReduced: 0, // No reduction, but safe landing
            OriginalDamageDice: originalDamageDice,
            FinalDamageDice: originalDamageDice, // Full damage still applies
            Outcome: SkillOutcome.MarginalSuccess,
            WasAttempted: true);
    }

    /// <summary>
    /// Gets a description of the Crash Landing result.
    /// </summary>
    /// <returns>A human-readable result description.</returns>
    public string ToDescription()
    {
        if (!WasAttempted)
        {
            return $"No Crash Landing attempted - {OriginalDamageDice}d10 damage";
        }

        var outcomeStr = Outcome switch
        {
            SkillOutcome.CriticalSuccess => "PERFECT LANDING",
            SkillOutcome.ExceptionalSuccess => "Exceptional Landing",
            SkillOutcome.FullSuccess => "Successful Landing",
            SkillOutcome.MarginalSuccess => "Marginal Landing",
            SkillOutcome.Failure => "Failed Landing",
            SkillOutcome.CriticalFailure => "DISASTROUS LANDING",
            _ => Outcome.ToString()
        };

        if (NegatedAllDamage)
        {
            return $"Crash Landing: {outcomeStr} - All damage negated! " +
                   $"({Successes} successes vs DC {CrashDc})";
        }

        if (ReducedDamage)
        {
            return $"Crash Landing: {outcomeStr} - Reduced {DiceReduced}d10 damage. " +
                   $"({Successes} successes vs DC {CrashDc}, {OriginalDamageDice}d10 → {FinalDamageDice}d10)";
        }

        return $"Crash Landing: {outcomeStr} - No damage reduction. " +
               $"({Successes} successes vs DC {CrashDc}, {FinalDamageDice}d10 damage)";
    }

    /// <summary>
    /// Gets a compact summary for log output.
    /// </summary>
    /// <returns>A compact result summary.</returns>
    public string ToSummary()
    {
        if (!WasAttempted)
        {
            return "No attempt";
        }

        if (NegatedAllDamage)
        {
            return $"Perfect ({Successes} vs DC {CrashDc})";
        }

        if (ReducedDamage)
        {
            return $"Reduced -{DiceReduced}d10 ({Successes} vs DC {CrashDc})";
        }

        return $"Failed ({Successes} vs DC {CrashDc})";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
