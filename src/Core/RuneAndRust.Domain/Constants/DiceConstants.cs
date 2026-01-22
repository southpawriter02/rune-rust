namespace RuneAndRust.Domain.Constants;

/// <summary>
/// Constants for the success-counting dice system.
/// </summary>
/// <remarks>
/// <para>
/// These values define the core mechanics of the dice pool system:
/// <list type="bullet">
///   <item><description>Success threshold: dice showing 8-10 count as successes</description></item>
///   <item><description>Botch value: dice showing 1 count as botches</description></item>
///   <item><description>Critical threshold: 5+ net successes for critical success</description></item>
/// </list>
/// </para>
/// <para>
/// Probability per d10:
/// <list type="bullet">
///   <item><description>Success (8, 9, 10): 30%</description></item>
///   <item><description>Botch (1): 10%</description></item>
///   <item><description>Neutral (2-7): 60%</description></item>
/// </list>
/// </para>
/// </remarks>
public static class DiceConstants
{
    /// <summary>
    /// Minimum die value that counts as a success (8, 9, or 10).
    /// </summary>
    /// <remarks>
    /// A d10 showing 8, 9, or 10 counts as one success.
    /// This gives a 30% success rate per die.
    /// </remarks>
    public const int SuccessThreshold = 8;

    /// <summary>
    /// Die value that counts as a botch (1).
    /// </summary>
    /// <remarks>
    /// A d10 showing 1 counts as one botch.
    /// Botches subtract from the success count.
    /// This gives a 10% botch rate per die.
    /// </remarks>
    public const int BotchValue = 1;

    /// <summary>
    /// Minimum net successes required for a critical success.
    /// </summary>
    /// <remarks>
    /// When net successes (successes - botches) reaches 5 or more,
    /// the result is considered a critical success with bonus effects.
    /// </remarks>
    public const int CriticalSuccessNet = 5;

    /// <summary>
    /// Minimum dice pool size. Always roll at least 1 die.
    /// </summary>
    /// <remarks>
    /// Even with severe penalties, a character always rolls at least 1d10.
    /// This ensures some chance of success in any situation.
    /// </remarks>
    public const int MinimumPool = 1;

    /// <summary>
    /// Probability of success per die (30%).
    /// </summary>
    /// <remarks>
    /// Three values out of ten (8, 9, 10) count as successes.
    /// </remarks>
    public const float SuccessProbability = 0.30f;

    /// <summary>
    /// Probability of botch per die (10%).
    /// </summary>
    /// <remarks>
    /// One value out of ten (1) counts as a botch.
    /// </remarks>
    public const float BotchProbability = 0.10f;

    /// <summary>
    /// Probability of neutral result per die (60%).
    /// </summary>
    /// <remarks>
    /// Six values out of ten (2-7) are neutral, neither success nor botch.
    /// </remarks>
    public const float NeutralProbability = 0.60f;

    /// <summary>
    /// Maximum value on a d10.
    /// </summary>
    public const int D10MaxValue = 10;

    /// <summary>
    /// Minimum value on a d10.
    /// </summary>
    public const int D10MinValue = 1;
}
