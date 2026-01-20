namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Comparison operators for achievement condition evaluation.
/// </summary>
/// <remarks>
/// <para>
/// Used to compare player statistics against achievement thresholds:
/// <list type="bullet">
///   <item><description>GreaterThanOrEqual — Player value must be >= threshold</description></item>
///   <item><description>LessThanOrEqual — Player value must be &lt;= threshold</description></item>
///   <item><description>Equals — Player value must exactly match threshold</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ComparisonOperator
{
    /// <summary>Player statistic must be greater than or equal to the threshold. Most common for cumulative achievements (e.g., "Kill 100 monsters").</summary>
    GreaterThanOrEqual,

    /// <summary>Player statistic must be less than or equal to the threshold. Used for achievements like "Complete with 0 deaths".</summary>
    LessThanOrEqual,

    /// <summary>Player statistic must exactly equal the threshold. Used for specific achievements (e.g., "Reach exactly level 10").</summary>
    Equals
}
