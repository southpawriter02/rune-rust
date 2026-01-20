namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a stat requirement for unlocking an ability tree node.
/// </summary>
/// <remarks>
/// <para>
/// StatPrerequisite defines a minimum stat value that must be met before
/// a player can invest points into a particular talent node.
/// </para>
/// <para>
/// Common usage examples:
/// <list type="bullet">
///   <item><description>"strength >= 14" for melee-focused talents</description></item>
///   <item><description>"intelligence >= 12" for magic talents</description></item>
///   <item><description>"charisma >= 10" for social abilities</description></item>
/// </list>
/// </para>
/// <para>
/// The StatId should match the stat identifiers used in the character system
/// (e.g., "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma").
/// </para>
/// </remarks>
public readonly record struct StatPrerequisite
{
    /// <summary>
    /// Gets the identifier of the required stat.
    /// </summary>
    /// <value>The stat identifier (e.g., "strength", "dexterity").</value>
    public string StatId { get; }

    /// <summary>
    /// Gets the minimum value required for this stat.
    /// </summary>
    /// <value>The minimum stat value (inclusive).</value>
    public int MinValue { get; }

    /// <summary>
    /// Creates a new StatPrerequisite with the specified stat and minimum value.
    /// </summary>
    /// <param name="statId">The identifier of the required stat.</param>
    /// <param name="minValue">The minimum value required.</param>
    /// <exception cref="ArgumentException">Thrown when statId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minValue is less than 1.</exception>
    public StatPrerequisite(string statId, int minValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(statId, nameof(statId));

        if (minValue < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minValue),
                minValue,
                "Minimum stat value must be at least 1.");
        }

        StatId = statId.ToLowerInvariant();
        MinValue = minValue;
    }

    /// <summary>
    /// Creates a new StatPrerequisite for the specified stat and value.
    /// </summary>
    /// <param name="statId">The identifier of the required stat.</param>
    /// <param name="minValue">The minimum value required.</param>
    /// <returns>A new StatPrerequisite instance.</returns>
    public static StatPrerequisite Require(string statId, int minValue) =>
        new(statId, minValue);

    /// <summary>
    /// Checks whether the given stat value meets this prerequisite.
    /// </summary>
    /// <param name="actualValue">The actual stat value to check.</param>
    /// <returns>True if the value meets or exceeds the minimum; otherwise, false.</returns>
    public bool IsMet(int actualValue) => actualValue >= MinValue;

    /// <summary>
    /// Returns a string representation of this prerequisite in "stat >= value" format.
    /// </summary>
    /// <returns>A string in the format "statId >= minValue".</returns>
    public override string ToString() => $"{StatId} >= {MinValue}";
}
