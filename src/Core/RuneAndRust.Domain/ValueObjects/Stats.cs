namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the combat statistics for a character or monster.
/// </summary>
/// <remarks>
/// Stats is an immutable value object that encapsulates the core combat attributes:
/// maximum health, attack power, and defense. These values determine combat effectiveness.
/// </remarks>
public readonly record struct Stats
{
    /// <summary>
    /// Gets the maximum health points for the character.
    /// </summary>
    public int MaxHealth { get; init; }

    /// <summary>
    /// Gets the attack power, used to calculate damage dealt in combat.
    /// </summary>
    public int Attack { get; init; }

    /// <summary>
    /// Gets the defense value, used to reduce incoming damage.
    /// </summary>
    public int Defense { get; init; }

    /// <summary>
    /// Creates a new Stats value with the specified attributes.
    /// </summary>
    /// <param name="maxHealth">The maximum health points (must be at least 1).</param>
    /// <param name="attack">The attack power (cannot be negative).</param>
    /// <param name="defense">The defense value (cannot be negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when maxHealth is less than 1, or attack/defense is negative.
    /// </exception>
    public Stats(int maxHealth, int attack, int defense)
    {
        if (maxHealth < 1)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be at least 1");
        if (attack < 0)
            throw new ArgumentOutOfRangeException(nameof(attack), "Attack cannot be negative");
        if (defense < 0)
            throw new ArgumentOutOfRangeException(nameof(defense), "Defense cannot be negative");

        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
    }

    /// <summary>
    /// Gets the default stats for a new player character (100 HP, 10 ATK, 5 DEF).
    /// </summary>
    public static Stats Default => new(100, 10, 5);

    /// <summary>
    /// Returns a string representation of these stats.
    /// </summary>
    /// <returns>A formatted string showing HP, ATK, and DEF values.</returns>
    public override string ToString() => $"HP: {MaxHealth}, ATK: {Attack}, DEF: {Defense}";
}
