namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the complete outcome of a balance check attempt.
/// </summary>
/// <remarks>
/// <para>
/// Contains all information needed to narrate the result and process
/// any consequences (such as falling and taking damage).
/// </para>
/// <para>
/// <b>Outcome Categories:</b>
/// <list type="bullet">
///   <item><description>Critical Success: Cross with grace, no stamina cost</description></item>
///   <item><description>Success: Cross safely</description></item>
///   <item><description>Marginal Success: Cross slowly, double stamina</description></item>
///   <item><description>Failure: Fall from surface</description></item>
///   <item><description>Fumble: Fall + disorientation</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance check results.
/// </para>
/// </remarks>
/// <param name="Context">The context used for the check.</param>
/// <param name="NetSuccesses">The number of net successes rolled.</param>
/// <param name="Outcome">The skill outcome classification.</param>
/// <param name="Crossed">Whether the character successfully crossed.</param>
/// <param name="FallTriggered">Whether a fall occurred.</param>
/// <param name="FallHeightFeet">The fall height if FallTriggered is true.</param>
/// <param name="StaminaCost">Movement stamina cost (doubled on marginal success).</param>
/// <param name="NarrativeDescription">Descriptive text for the outcome.</param>
public readonly record struct BalanceCheckResult(
    BalanceContext Context,
    int NetSuccesses,
    SkillOutcome Outcome,
    bool Crossed,
    bool FallTriggered,
    int FallHeightFeet,
    int StaminaCost,
    string NarrativeDescription)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base stamina cost for crossing a surface.
    /// </summary>
    public const int BaseStaminaCost = 1;

    /// <summary>
    /// Stamina multiplier for marginal success.
    /// </summary>
    public const int MarginalSuccessStaminaMultiplier = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the margin of success or failure.
    /// </summary>
    public int Margin => NetSuccesses - Context.FinalDc;

    /// <summary>
    /// Indicates whether this was a critical success.
    /// </summary>
    public bool IsCritical => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Indicates whether this was a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Indicates whether this was any form of success.
    /// </summary>
    public bool IsSuccess => Outcome >= SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Indicates whether this was a marginal success (barely made it).
    /// </summary>
    public bool IsMarginalSuccess => Outcome == SkillOutcome.MarginalSuccess;

    /// <summary>
    /// Indicates whether the traverse continues (for long traverses).
    /// </summary>
    public bool TraverseContinues => Crossed && Context.IsLongTraverse &&
        Context.CheckNumber < Context.TotalChecksRequired;

    /// <summary>
    /// Indicates whether the traverse is complete (for long traverses).
    /// </summary>
    public bool TraverseComplete => Crossed && (!Context.IsLongTraverse ||
        Context.CheckNumber >= Context.TotalChecksRequired);

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a short summary of the result.
    /// </summary>
    /// <returns>Short result summary.</returns>
    public string ToShortString()
    {
        if (Crossed)
        {
            return Outcome switch
            {
                SkillOutcome.CriticalSuccess => "Crossed with grace!",
                SkillOutcome.ExceptionalSuccess => "Crossed confidently",
                SkillOutcome.FullSuccess => "Crossed safely",
                SkillOutcome.MarginalSuccess => "Crossed (barely)",
                _ => "Crossed"
            };
        }

        return IsFumble ? "Fell catastrophically!" : "Lost balance and fell";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful balance result.
    /// </summary>
    /// <param name="context">The balance context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A successful balance result.</returns>
    public static BalanceCheckResult Success(
        BalanceContext context,
        int netSuccesses,
        SkillOutcome outcome)
    {
        var staminaCost = CalculateStaminaCost(outcome);

        var narrative = outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                "You cross with perfect grace, barely seeming to touch the surface.",
            SkillOutcome.ExceptionalSuccess =>
                "You traverse the surface with confident, measured steps.",
            SkillOutcome.FullSuccess =>
                "You carefully make your way across the surface.",
            SkillOutcome.MarginalSuccess =>
                "You wobble precariously but manage to keep your balance.",
            _ => "You cross the surface."
        };

        return new BalanceCheckResult(
            context,
            netSuccesses,
            outcome,
            Crossed: true,
            FallTriggered: false,
            FallHeightFeet: 0,
            staminaCost,
            narrative);
    }

    /// <summary>
    /// Creates a failed balance result with fall.
    /// </summary>
    /// <param name="context">The balance context.</param>
    /// <param name="netSuccesses">Net successes rolled.</param>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A failed balance result.</returns>
    public static BalanceCheckResult Failure(
        BalanceContext context,
        int netSuccesses,
        SkillOutcome outcome)
    {
        var narrative = outcome switch
        {
            SkillOutcome.CriticalFailure =>
                "Your foot slips catastrophically and you tumble into the void!",
            _ => "You lose your balance and fall from the surface."
        };

        return new BalanceCheckResult(
            context,
            netSuccesses,
            outcome,
            Crossed: false,
            FallTriggered: true,
            FallHeightFeet: context.FallHeight,
            StaminaCost: 0,
            narrative);
    }

    /// <summary>
    /// Calculates stamina cost based on outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>The stamina cost.</returns>
    private static int CalculateStaminaCost(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess => 0, // Effortless
            SkillOutcome.ExceptionalSuccess => BaseStaminaCost,
            SkillOutcome.FullSuccess => BaseStaminaCost,
            SkillOutcome.MarginalSuccess => BaseStaminaCost * MarginalSuccessStaminaMultiplier,
            _ => 0
        };
    }
}
