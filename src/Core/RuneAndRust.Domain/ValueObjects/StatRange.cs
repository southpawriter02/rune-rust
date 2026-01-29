namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an expected range for a stat value at a specific quality tier.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StatRange"/> defines the minimum and maximum allowed values for a stat
/// at a specific quality tier. It also includes a dice expression for display purposes
/// (e.g., "2d6+4" for damage).
/// </para>
/// <para>
/// The dice expression is informational; the min/max values are derived from the dice
/// formula but stored explicitly for validation efficiency.
/// </para>
/// <para>
/// <strong>Example Usage:</strong>
/// <code>
/// // Create from dice expression (2d6+4 = min 6, max 16)
/// var damageRange = StatRange.CreateFromDice(diceCount: 2, diceSides: 6, bonus: 4);
/// 
/// // Check if a value is valid
/// bool isValid = damageRange.IsInRange(12); // true (6-16)
/// bool isInvalid = damageRange.IsInRange(20); // false (exceeds max)
/// 
/// // Display format
/// Console.WriteLine(damageRange.FormatRange()); // "6-16 (2d6+4)"
/// </code>
/// </para>
/// </remarks>
/// <param name="MinValue">Minimum allowed stat value (inclusive).</param>
/// <param name="MaxValue">Maximum allowed stat value (inclusive).</param>
/// <param name="DiceExpression">Display format (e.g., "2d6+4", "3-5", "+4").</param>
public readonly record struct StatRange(
    int MinValue,
    int MaxValue,
    string DiceExpression)
{
    #region Static Factories

    /// <summary>
    /// Represents no range (invalid or N/A).
    /// </summary>
    /// <remarks>
    /// Used when a stat type is not applicable for an item (e.g., damage for armor).
    /// </remarks>
    public static StatRange None => new(0, 0, "N/A");

    /// <summary>
    /// Creates a <see cref="StatRange"/> with validation.
    /// </summary>
    /// <param name="minValue">Minimum value (must be non-negative).</param>
    /// <param name="maxValue">Maximum value (must be >= minValue).</param>
    /// <param name="diceExpression">Display format for the range.</param>
    /// <returns>A new validated <see cref="StatRange"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When minValue is negative or maxValue &lt; minValue.</exception>
    /// <exception cref="ArgumentException">When diceExpression is null or whitespace.</exception>
    public static StatRange Create(int minValue, int maxValue, string diceExpression)
    {
        // Validate minimum value is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(minValue);

        // Validate max >= min
        ArgumentOutOfRangeException.ThrowIfLessThan(maxValue, minValue);

        // Validate dice expression is provided
        ArgumentException.ThrowIfNullOrWhiteSpace(diceExpression);

        return new StatRange(minValue, maxValue, diceExpression);
    }

    /// <summary>
    /// Creates a <see cref="StatRange"/> from a dice expression.
    /// </summary>
    /// <param name="diceCount">Number of dice to roll (e.g., 2 for 2d6).</param>
    /// <param name="diceSides">Number of sides per die (e.g., 6 for d6).</param>
    /// <param name="bonus">Flat bonus added to the roll (default 0).</param>
    /// <returns>A new <see cref="StatRange"/> with calculated min/max.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When diceCount or diceSides is less than 1, or bonus is negative.</exception>
    /// <remarks>
    /// <para>
    /// Parses dice expressions in standard NdX+Y format where:
    /// <list type="bullet">
    ///   <item><description>N = number of dice</description></item>
    ///   <item><description>X = sides per die</description></item>
    ///   <item><description>Y = flat bonus</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Min/Max Calculation:</strong>
    /// <list type="bullet">
    ///   <item><description>Minimum = N + Y (all dice roll 1)</description></item>
    ///   <item><description>Maximum = (N × X) + Y (all dice roll max)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Examples:</strong>
    /// <code>
    /// StatRange.CreateFromDice(1, 6, 0)  // 1d6   → min 1, max 6
    /// StatRange.CreateFromDice(2, 6, 4)  // 2d6+4 → min 6, max 16
    /// StatRange.CreateFromDice(2, 8, 6)  // 2d8+6 → min 8, max 22
    /// </code>
    /// </para>
    /// </remarks>
    public static StatRange CreateFromDice(int diceCount, int diceSides, int bonus = 0)
    {
        // Validate dice count (at least 1 die)
        ArgumentOutOfRangeException.ThrowIfLessThan(diceCount, 1);

        // Validate dice sides (at least 1 side, though 2+ is typical)
        ArgumentOutOfRangeException.ThrowIfLessThan(diceSides, 1);

        // Validate bonus is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(bonus);

        // Calculate min: all dice roll 1 + bonus
        var min = diceCount + bonus;

        // Calculate max: all dice roll max + bonus
        var max = (diceCount * diceSides) + bonus;

        // Format dice expression (e.g., "2d6+4" or "1d6" if no bonus)
        var expression = bonus > 0
            ? $"{diceCount}d{diceSides}+{bonus}"
            : $"{diceCount}d{diceSides}";

        return new StatRange(min, max, expression);
    }

    /// <summary>
    /// Creates a <see cref="StatRange"/> for a flat value range (no dice).
    /// </summary>
    /// <param name="minValue">Minimum value.</param>
    /// <param name="maxValue">Maximum value.</param>
    /// <returns>A new <see cref="StatRange"/> with "min-max" expression format.</returns>
    /// <remarks>
    /// Used for defense ranges and other non-dice stats.
    /// <code>
    /// StatRange.CreateFlat(7, 10)  // "7-10" for Tier 4 defense
    /// </code>
    /// </remarks>
    public static StatRange CreateFlat(int minValue, int maxValue) =>
        new(minValue, maxValue, $"{minValue}-{maxValue}");

    /// <summary>
    /// Creates a <see cref="StatRange"/> for a single fixed value.
    /// </summary>
    /// <param name="value">The fixed value (both min and max).</param>
    /// <returns>A new <see cref="StatRange"/> with "+value" expression format.</returns>
    /// <remarks>
    /// Used for attribute bonuses that have a fixed value per tier.
    /// <code>
    /// StatRange.CreateFixed(4)  // "+4" for Tier 4 attribute bonus
    /// </code>
    /// </remarks>
    public static StatRange CreateFixed(int value) =>
        new(value, value, $"+{value}");

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets whether this range is valid (max >= min and has expression).
    /// </summary>
    public bool IsValid => MaxValue >= MinValue && !string.IsNullOrEmpty(DiceExpression);

    /// <summary>
    /// Gets the range span (max - min).
    /// </summary>
    /// <remarks>
    /// A span of 0 indicates a fixed value (min equals max).
    /// </remarks>
    public int Span => MaxValue - MinValue;

    #endregion

    #region Methods

    /// <summary>
    /// Checks if a value falls within this range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if value is within [MinValue, MaxValue]; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <code>
    /// var range = StatRange.Create(6, 16, "2d6+4");
    /// range.IsInRange(6);   // true (at minimum)
    /// range.IsInRange(12);  // true (within range)
    /// range.IsInRange(16);  // true (at maximum)
    /// range.IsInRange(5);   // false (below minimum)
    /// range.IsInRange(17);  // false (above maximum)
    /// </code>
    /// </remarks>
    public bool IsInRange(int value) =>
        value >= MinValue && value <= MaxValue;

    /// <summary>
    /// Calculates the average value of this range.
    /// </summary>
    /// <returns>The midpoint of the range as a double.</returns>
    /// <remarks>
    /// Useful for comparing expected DPS or average stats.
    /// <code>
    /// var range = StatRange.CreateFromDice(2, 6, 4);  // 2d6+4
    /// double avg = range.GetAverageValue();  // 11.0
    /// </code>
    /// </remarks>
    public double GetAverageValue() =>
        (MinValue + MaxValue) / 2.0;

    /// <summary>
    /// Formats the range for display.
    /// </summary>
    /// <returns>A string like "1-6 (1d6)" or "6-16 (2d6+4)".</returns>
    public string FormatRange() =>
        $"{MinValue}-{MaxValue} ({DiceExpression})";

    /// <summary>
    /// Returns a display string for debug/logging.
    /// </summary>
    /// <returns>Same as <see cref="FormatRange"/>.</returns>
    public override string ToString() => FormatRange();

    #endregion
}
