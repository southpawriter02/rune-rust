namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents stat modifications applied when leveling up.
/// </summary>
/// <remarks>
/// This is separate from <see cref="StatModifiers"/> (used for equipment) to maintain
/// clear separation of concerns. Level-up stat increases only affect core combat stats.
/// </remarks>
public readonly record struct LevelStatModifiers(int MaxHealth, int Attack, int Defense)
{
    /// <summary>
    /// Gets a modifier set with all zeros (no modifications).
    /// </summary>
    public static readonly LevelStatModifiers Zero = new(0, 0, 0);

    /// <summary>
    /// Gets the default stat increases per level (+5 HP, +1 ATK, +1 DEF).
    /// </summary>
    public static readonly LevelStatModifiers DefaultLevelUp = new(5, 1, 1);

    /// <summary>
    /// Gets whether this modifier set has any non-zero values.
    /// </summary>
    public bool HasModifications => MaxHealth != 0 || Attack != 0 || Defense != 0;

    /// <summary>
    /// Adds two modifier sets together.
    /// </summary>
    /// <param name="other">The other modifier set to add.</param>
    /// <returns>A new modifier set with combined values.</returns>
    public LevelStatModifiers Add(LevelStatModifiers other) =>
        new(MaxHealth + other.MaxHealth, Attack + other.Attack, Defense + other.Defense);

    /// <summary>
    /// Multiplies all stat modifications by a factor (for multi-level gains).
    /// </summary>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>A new modifier set with scaled values.</returns>
    public LevelStatModifiers Multiply(int factor) =>
        new(MaxHealth * factor, Attack * factor, Defense * factor);

    /// <summary>
    /// Gets stat increases for a given number of levels gained.
    /// </summary>
    /// <param name="levelsGained">The number of levels gained.</param>
    /// <returns>Total stat increases for the levels.</returns>
    public static LevelStatModifiers ForLevels(int levelsGained) =>
        levelsGained <= 0 ? Zero : DefaultLevelUp.Multiply(levelsGained);

    /// <inheritdoc />
    public override string ToString() =>
        $"HP: +{MaxHealth}, ATK: +{Attack}, DEF: +{Defense}";
}
