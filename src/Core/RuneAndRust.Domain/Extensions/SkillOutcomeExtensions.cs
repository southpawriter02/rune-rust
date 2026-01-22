using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Extensions;

/// <summary>
/// Extension methods for the <see cref="SkillOutcome"/> enum.
/// </summary>
public static class SkillOutcomeExtensions
{
    /// <summary>
    /// Determines if the outcome represents any form of success.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>True if the outcome is MarginalSuccess or better.</returns>
    /// <example>
    /// <code>
    /// var outcome = SkillOutcome.FullSuccess;
    /// if (outcome.IsSuccess())
    ///     Console.WriteLine("You succeeded!");
    /// </code>
    /// </example>
    public static bool IsSuccess(this SkillOutcome outcome)
    {
        return outcome >= SkillOutcome.MarginalSuccess;
    }

    /// <summary>
    /// Determines if the outcome represents any form of failure.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>True if the outcome is Failure or CriticalFailure.</returns>
    public static bool IsFailure(this SkillOutcome outcome)
    {
        return outcome <= SkillOutcome.Failure;
    }

    /// <summary>
    /// Determines if the outcome is a critical result (best or worst).
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>True if CriticalSuccess or CriticalFailure.</returns>
    public static bool IsCritical(this SkillOutcome outcome)
    {
        return outcome is SkillOutcome.CriticalSuccess or SkillOutcome.CriticalFailure;
    }

    /// <summary>
    /// Gets the margin range for the outcome tier.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>
    /// A tuple of (minMargin, maxMargin) for the tier.
    /// Null values indicate unbounded (e.g., any negative for Failure).
    /// CriticalFailure returns (null, null) as it's fumble-based, not margin-based.
    /// </returns>
    public static (int? Min, int? Max) GetMarginRange(this SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => (null, null),   // Fumble-based, not margin
            SkillOutcome.Failure => (null, -1),              // Any negative margin
            SkillOutcome.MarginalSuccess => (0, 0),          // Exactly 0
            SkillOutcome.FullSuccess => (1, 2),              // 1-2
            SkillOutcome.ExceptionalSuccess => (3, 4),       // 3-4
            SkillOutcome.CriticalSuccess => (5, null),       // 5+
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown SkillOutcome value")
        };
    }

    /// <summary>
    /// Converts to the legacy SuccessLevel enum for backward compatibility.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>The equivalent SuccessLevel.</returns>
    /// <remarks>
    /// Mapping:
    /// <list type="bullet">
    ///   <item><description>CriticalFailure → CriticalFailure</description></item>
    ///   <item><description>Failure → Failure</description></item>
    ///   <item><description>MarginalSuccess, FullSuccess, ExceptionalSuccess → Success</description></item>
    ///   <item><description>CriticalSuccess → CriticalSuccess</description></item>
    /// </list>
    /// </remarks>
    public static SuccessLevel ToSuccessLevel(this SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => SuccessLevel.CriticalFailure,
            SkillOutcome.Failure => SuccessLevel.Failure,
            SkillOutcome.MarginalSuccess or
            SkillOutcome.FullSuccess or
            SkillOutcome.ExceptionalSuccess => SuccessLevel.Success,
            SkillOutcome.CriticalSuccess => SuccessLevel.CriticalSuccess,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown SkillOutcome value")
        };
    }

    /// <summary>
    /// Gets a display-friendly name for the outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A human-readable outcome name.</returns>
    public static string GetDisplayName(this SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => "Critical Failure",
            SkillOutcome.Failure => "Failure",
            SkillOutcome.MarginalSuccess => "Marginal Success",
            SkillOutcome.FullSuccess => "Success",
            SkillOutcome.ExceptionalSuccess => "Exceptional Success",
            SkillOutcome.CriticalSuccess => "Critical Success",
            _ => outcome.ToString()
        };
    }

    /// <summary>
    /// Gets a short abbreviation for the outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <returns>A 2-3 character abbreviation.</returns>
    public static string GetAbbreviation(this SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => "CF",
            SkillOutcome.Failure => "F",
            SkillOutcome.MarginalSuccess => "MS",
            SkillOutcome.FullSuccess => "S",
            SkillOutcome.ExceptionalSuccess => "ES",
            SkillOutcome.CriticalSuccess => "CS",
            _ => "?"
        };
    }
}
