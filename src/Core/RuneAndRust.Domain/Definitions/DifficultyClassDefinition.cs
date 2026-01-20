namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a difficulty class (DC) threshold for skill checks.
/// Difficulty classes are configured via JSON and provide standard DC targets.
/// </summary>
/// <remarks>
/// Standard difficulties range from Trivial (DC 5) to Nearly Impossible (DC 30).
/// </remarks>
public class DifficultyClassDefinition
{
    /// <summary>
    /// Gets the unique identifier for this difficulty class.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name shown to players.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description explaining the difficulty level.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target number that must be met or exceeded for success.
    /// </summary>
    public int TargetNumber { get; init; }

    /// <summary>
    /// Gets the display color for this difficulty level (hex code).
    /// </summary>
    public string Color { get; init; } = "#FFFFFF";

    /// <summary>
    /// Gets the sort order for display purposes.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Private constructor for JSON deserialization.
    /// </summary>
    private DifficultyClassDefinition() { }

    /// <summary>
    /// Creates a new difficulty class definition with validation.
    /// </summary>
    public static DifficultyClassDefinition Create(
        string id,
        string name,
        string description,
        int targetNumber,
        string color = "#FFFFFF",
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (targetNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetNumber),
                targetNumber,
                "Target number must be at least 1");
        }

        if (!string.IsNullOrEmpty(color) && !color.StartsWith('#'))
        {
            color = "#" + color;
        }

        return new DifficultyClassDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            TargetNumber = targetNumber,
            Color = color ?? "#FFFFFF",
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Checks if a result meets this difficulty class.
    /// </summary>
    public bool IsMet(int result) => result >= TargetNumber;

    /// <summary>
    /// Calculates the margin of success or failure.
    /// </summary>
    public int GetMargin(int result) => result - TargetNumber;

    /// <inheritdoc />
    public override string ToString() => $"{Name} (DC {TargetNumber})";
}
