namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of checking and applying a level-up.
/// </summary>
/// <remarks>
/// Contains information about levels gained, stat increases, and any
/// abilities unlocked. Used by ProgressionService to communicate
/// level-up results to the application layer.
/// </remarks>
public readonly record struct LevelUpResult
{
    /// <summary>
    /// Gets the level before leveling up.
    /// </summary>
    public int OldLevel { get; init; }

    /// <summary>
    /// Gets the level after leveling up.
    /// </summary>
    public int NewLevel { get; init; }

    /// <summary>
    /// Gets the stat increases applied.
    /// </summary>
    public LevelStatModifiers StatIncreases { get; init; }

    /// <summary>
    /// Gets the IDs of newly unlocked abilities.
    /// </summary>
    public IReadOnlyList<string> NewAbilities { get; init; }

    /// <summary>
    /// Gets the number of levels gained.
    /// </summary>
    public int LevelsGained => NewLevel - OldLevel;

    /// <summary>
    /// Gets whether multiple levels were gained at once.
    /// </summary>
    public bool IsMultiLevel => LevelsGained > 1;

    /// <summary>
    /// Gets whether any new abilities were unlocked.
    /// </summary>
    public bool HasNewAbilities => NewAbilities.Count > 0;

    /// <summary>
    /// Gets whether a level-up occurred.
    /// </summary>
    public bool DidLevelUp => LevelsGained > 0;

    /// <summary>
    /// Creates a new level-up result.
    /// </summary>
    /// <param name="oldLevel">The level before leveling up.</param>
    /// <param name="newLevel">The level after leveling up.</param>
    /// <param name="statIncreases">The stat increases applied.</param>
    /// <param name="newAbilities">The IDs of newly unlocked abilities.</param>
    public LevelUpResult(
        int oldLevel,
        int newLevel,
        LevelStatModifiers statIncreases,
        IReadOnlyList<string>? newAbilities = null)
    {
        OldLevel = oldLevel;
        NewLevel = newLevel;
        StatIncreases = statIncreases;
        NewAbilities = newAbilities ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a result indicating no level-up occurred.
    /// </summary>
    /// <param name="currentLevel">The current level.</param>
    /// <returns>A result with no level change.</returns>
    public static LevelUpResult None(int currentLevel) =>
        new(currentLevel, currentLevel, LevelStatModifiers.Zero);

    /// <summary>
    /// Creates a result for a single level gain.
    /// </summary>
    /// <param name="oldLevel">The level before leveling up.</param>
    /// <param name="newAbilities">Optional list of unlocked ability IDs.</param>
    /// <returns>A result for gaining one level.</returns>
    public static LevelUpResult SingleLevel(int oldLevel, IReadOnlyList<string>? newAbilities = null) =>
        new(oldLevel, oldLevel + 1, LevelStatModifiers.DefaultLevelUp, newAbilities);

    /// <summary>
    /// Creates a result for multiple level gains.
    /// </summary>
    /// <param name="oldLevel">The level before leveling up.</param>
    /// <param name="levelsGained">The number of levels gained.</param>
    /// <param name="newAbilities">Optional list of unlocked ability IDs.</param>
    /// <returns>A result for gaining multiple levels.</returns>
    public static LevelUpResult MultiLevel(int oldLevel, int levelsGained, IReadOnlyList<string>? newAbilities = null) =>
        new(oldLevel, oldLevel + levelsGained, LevelStatModifiers.ForLevels(levelsGained), newAbilities);

    /// <inheritdoc />
    public override string ToString() =>
        DidLevelUp
            ? $"Level {OldLevel} -> {NewLevel} ({StatIncreases})"
            : $"Level {OldLevel} (no change)";
}
