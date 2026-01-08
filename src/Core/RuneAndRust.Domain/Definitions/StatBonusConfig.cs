using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration class for stat bonuses, used in JSON deserialization.
/// </summary>
/// <remarks>
/// This is a mutable class for JSON binding. Use <see cref="ToLevelStatModifiers"/>
/// to convert to the immutable <see cref="LevelStatModifiers"/> value object.
/// </remarks>
public class StatBonusConfig
{
    /// <summary>
    /// Gets or sets the max health bonus per level.
    /// </summary>
    public int MaxHealth { get; set; } = 5;

    /// <summary>
    /// Gets or sets the attack bonus per level.
    /// </summary>
    public int Attack { get; set; } = 1;

    /// <summary>
    /// Gets or sets the defense bonus per level.
    /// </summary>
    public int Defense { get; set; } = 1;

    /// <summary>
    /// Converts this configuration to a <see cref="LevelStatModifiers"/> value object.
    /// </summary>
    /// <returns>An immutable <see cref="LevelStatModifiers"/> instance.</returns>
    public LevelStatModifiers ToLevelStatModifiers() => new(MaxHealth, Attack, Defense);
}
