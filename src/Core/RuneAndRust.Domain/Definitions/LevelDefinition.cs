namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines custom configuration for a specific level.
/// </summary>
/// <remarks>
/// Used in <see cref="ProgressionDefinition.LevelOverrides"/> to customize
/// individual levels with different XP requirements, stat bonuses, or rewards.
/// </remarks>
public class LevelDefinition
{
    /// <summary>
    /// Gets the level number this definition applies to.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Gets the cumulative XP required to reach this level.
    /// </summary>
    /// <remarks>
    /// If null, the XP is calculated from the progression curve.
    /// </remarks>
    public int? XpRequired { get; init; }

    /// <summary>
    /// Gets custom stat bonuses for this level.
    /// </summary>
    /// <remarks>
    /// If null, uses default stat bonuses or class growth rates.
    /// </remarks>
    public StatBonusConfig? StatBonuses { get; init; }

    /// <summary>
    /// Gets custom rewards granted at this level.
    /// </summary>
    /// <remarks>
    /// Descriptive text shown to the player. Actual reward logic
    /// would be implemented in future versions.
    /// </remarks>
    public IReadOnlyList<string> CustomRewards { get; init; } = [];

    /// <summary>
    /// Gets a milestone title granted at this level.
    /// </summary>
    /// <example>"Novice", "Apprentice", "Master"</example>
    public string? Title { get; init; }
}
