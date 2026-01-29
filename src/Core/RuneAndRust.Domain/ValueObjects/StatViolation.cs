namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a single stat violation detected during item verification.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StatViolation"/> captures the details of a single stat that failed
/// validation against tier expectations. It includes the stat type, expected range,
/// actual value, and a human-readable error message.
/// </para>
/// <para>
/// <strong>Example Usage:</strong>
/// <code>
/// // Create a damage violation
/// var expected = StatRange.Create(1, 6, "1d6");
/// var violation = StatViolation.Damage(expected, actualValue: 15);
/// 
/// Console.WriteLine(violation.Message);
/// // "Damage value 15 outside expected range 1-6 (1d6)"
/// </code>
/// </para>
/// </remarks>
/// <param name="StatType">The category of stat that failed validation.</param>
/// <param name="StatName">Human-readable name of the stat (e.g., "Damage", "Might").</param>
/// <param name="ExpectedRange">The expected stat range for the tier.</param>
/// <param name="ActualValue">The actual value found on the item.</param>
/// <seealso cref="StatViolationType"/>
/// <seealso cref="StatRange"/>
/// <seealso cref="StatVerificationResult"/>
public readonly record struct StatViolation(
    StatViolationType StatType,
    string StatName,
    StatRange ExpectedRange,
    int ActualValue)
{
    #region Computed Properties

    /// <summary>
    /// Gets a human-readable violation message.
    /// </summary>
    /// <remarks>
    /// Message format: "{StatName} value {ActualValue} outside expected range {Range}"
    /// </remarks>
    /// <example>
    /// "Damage value 15 outside expected range 1-6 (1d6)"
    /// "Might value 7 outside expected range 4-4 (+4)"
    /// </example>
    public string Message =>
        $"{StatName} value {ActualValue} outside expected range {ExpectedRange.FormatRange()}";

    /// <summary>
    /// Gets how far the actual value deviates from the expected range.
    /// </summary>
    /// <remarks>
    /// Returns:
    /// <list type="bullet">
    ///   <item><description>0 if value is within range</description></item>
    ///   <item><description>Negative if below minimum</description></item>
    ///   <item><description>Positive if above maximum</description></item>
    /// </list>
    /// </remarks>
    public int Deviation
    {
        get
        {
            if (ActualValue < ExpectedRange.MinValue)
                return ActualValue - ExpectedRange.MinValue; // Negative (below)
            if (ActualValue > ExpectedRange.MaxValue)
                return ActualValue - ExpectedRange.MaxValue; // Positive (above)
            return 0; // Within range
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a damage violation.
    /// </summary>
    /// <param name="expected">The expected damage range for the tier.</param>
    /// <param name="actual">The actual damage value found.</param>
    /// <returns>A new <see cref="StatViolation"/> for damage.</returns>
    /// <remarks>
    /// <code>
    /// // Tier 0 expects 1d6 (1-6), but item has 15 damage
    /// var violation = StatViolation.Damage(
    ///     StatRange.Create(1, 6, "1d6"), 
    ///     actual: 15);
    /// </code>
    /// </remarks>
    public static StatViolation Damage(StatRange expected, int actual) =>
        new(StatViolationType.Damage, "Damage", expected, actual);

    /// <summary>
    /// Creates a defense violation.
    /// </summary>
    /// <param name="expected">The expected defense range for the tier.</param>
    /// <param name="actual">The actual defense value found.</param>
    /// <returns>A new <see cref="StatViolation"/> for defense.</returns>
    /// <remarks>
    /// <code>
    /// // Tier 4 expects 7-10 defense, but item has 3
    /// var violation = StatViolation.Defense(
    ///     StatRange.CreateFlat(7, 10), 
    ///     actual: 3);
    /// </code>
    /// </remarks>
    public static StatViolation Defense(StatRange expected, int actual) =>
        new(StatViolationType.Defense, "Defense", expected, actual);

    /// <summary>
    /// Creates an attribute bonus violation.
    /// </summary>
    /// <param name="attributeName">Name of the attribute (e.g., "Might", "Finesse").</param>
    /// <param name="expected">The expected attribute bonus range for the tier.</param>
    /// <param name="actual">The actual attribute bonus found.</param>
    /// <returns>A new <see cref="StatViolation"/> for the attribute.</returns>
    /// <remarks>
    /// <code>
    /// // Tier 4 expects +4 Might, but item has +7
    /// var violation = StatViolation.Attribute(
    ///     "Might",
    ///     StatRange.CreateFixed(4), 
    ///     actual: 7);
    /// </code>
    /// </remarks>
    public static StatViolation Attribute(string attributeName, StatRange expected, int actual) =>
        new(StatViolationType.Attribute, attributeName, expected, actual);

    #endregion

    /// <summary>
    /// Returns the violation message for logging/debugging.
    /// </summary>
    public override string ToString() => Message;
}
