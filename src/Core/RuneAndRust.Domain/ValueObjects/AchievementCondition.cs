// ═══════════════════════════════════════════════════════════════════════════════
// AchievementCondition.cs
// Represents a single condition that must be met to unlock an achievement.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a single condition that must be met to unlock an achievement.
/// </summary>
/// <remarks>
/// <para>Conditions are evaluated against player statistics:</para>
/// <list type="bullet">
///   <item><description>StatisticName maps to a property on PlayerStatistics or DiceRollHistory</description></item>
///   <item><description>Operator determines the comparison type (>=, &lt;=, ==)</description></item>
///   <item><description>Value is the threshold to compare against</description></item>
/// </list>
/// <para>Multiple conditions on an achievement require ALL to be satisfied (AND logic).</para>
/// </remarks>
/// <param name="StatisticName">The name of the statistic to check (e.g., "monstersKilled", "roomsDiscovered").</param>
/// <param name="Operator">The comparison operator to use for evaluation.</param>
/// <param name="Value">The threshold value to compare against.</param>
/// <example>
/// <code>
/// // Achievement condition: Kill at least 100 monsters
/// var condition = new AchievementCondition(
///     "monstersKilled",
///     ComparisonOperator.GreaterThanOrEqual,
///     100);
///
/// // Check if condition is met
/// bool isUnlocked = condition.Evaluate(playerStats.MonstersKilled);
///
/// // Check progress (0.0 to 1.0)
/// double progress = condition.GetProgress(50); // Returns 0.5
/// </code>
/// </example>
public record AchievementCondition(
    string StatisticName,
    ComparisonOperator Operator,
    long Value)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Evaluation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Evaluates this condition against a statistic value.
    /// </summary>
    /// <param name="actualValue">The player's current statistic value.</param>
    /// <returns>True if the condition is satisfied; otherwise, false.</returns>
    /// <remarks>
    /// <para>The evaluation depends on the operator:</para>
    /// <list type="bullet">
    ///   <item><description>GreaterThanOrEqual: actualValue >= Value</description></item>
    ///   <item><description>LessThanOrEqual: actualValue &lt;= Value</description></item>
    ///   <item><description>Equals: actualValue == Value</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
    /// bool result = condition.Evaluate(150); // Returns true (150 >= 100)
    /// </code>
    /// </example>
    public bool Evaluate(long actualValue)
    {
        return Operator switch
        {
            ComparisonOperator.GreaterThanOrEqual => actualValue >= Value,
            ComparisonOperator.LessThanOrEqual => actualValue <= Value,
            ComparisonOperator.Equals => actualValue == Value,
            _ => false
        };
    }

    /// <summary>
    /// Gets the progress towards this condition as a percentage (0.0 to 1.0).
    /// </summary>
    /// <param name="actualValue">The player's current statistic value.</param>
    /// <returns>Progress as a decimal between 0.0 and 1.0 (capped at 1.0).</returns>
    /// <remarks>
    /// <para>Progress calculation depends on the operator:</para>
    /// <list type="bullet">
    ///   <item><description>GreaterThanOrEqual: Returns actualValue/Value, capped at 1.0</description></item>
    ///   <item><description>LessThanOrEqual: Returns 1.0 if condition met, 0.0 otherwise (binary)</description></item>
    ///   <item><description>Equals: Returns 1.0 if condition met, 0.0 otherwise (binary)</description></item>
    /// </list>
    /// <para>For a Value of 0, returns 1.0 if actualValue is 0, otherwise 0.0.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
    /// double progress = condition.GetProgress(50);  // Returns 0.5 (50/100)
    /// double complete = condition.GetProgress(150); // Returns 1.0 (capped)
    /// </code>
    /// </example>
    public double GetProgress(long actualValue)
    {
        // Handle edge case where threshold is 0
        if (Value == 0)
        {
            return actualValue == 0 ? 1.0 : 0.0;
        }

        return Operator switch
        {
            // For >= conditions, calculate proportional progress
            ComparisonOperator.GreaterThanOrEqual => Math.Min(1.0, (double)actualValue / Value),

            // For <= and == conditions, progress is binary (met or not)
            ComparisonOperator.LessThanOrEqual => actualValue <= Value ? 1.0 : 0.0,
            ComparisonOperator.Equals => actualValue == Value ? 1.0 : 0.0,

            // Default case (should never occur with valid enum)
            _ => 0.0
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Display Helper Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a human-readable description of this condition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Examples: "monstersKilled &gt;= 100", "deaths &lt;= 0", "level == 10"
    /// </para>
    /// </remarks>
    public string Description => $"{StatisticName} {OperatorSymbol} {Value}";

    /// <summary>
    /// Gets the operator as a symbol string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps enum values to their symbol equivalents:
    /// GreaterThanOrEqual => ">=", LessThanOrEqual => "&lt;=", Equals => "=="
    /// </para>
    /// </remarks>
    public string OperatorSymbol => Operator switch
    {
        ComparisonOperator.GreaterThanOrEqual => ">=",
        ComparisonOperator.LessThanOrEqual => "<=",
        ComparisonOperator.Equals => "==",
        _ => "?"
    };
}
