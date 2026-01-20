using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a pool of dice to be rolled together.
/// </summary>
/// <remarks>
/// Immutable value object supporting standard dice notation (e.g., "3d6+5").
/// Use static factory methods or Parse() for convenient construction.
/// </remarks>
/// <example>
/// <code>
/// var pool = DicePool.Parse("3d6+5");
/// var pool = DicePool.D10();
/// var pool = new DicePool(2, DiceType.D8, modifier: 3);
/// </code>
/// </example>
public readonly record struct DicePool
{
    /// <summary>Number of dice in the pool (minimum 1).</summary>
    public int Count { get; init; }

    /// <summary>Type of dice (d4, d6, d8, d10).</summary>
    public DiceType DiceType { get; init; }

    /// <summary>Flat modifier added to the total roll.</summary>
    public int Modifier { get; init; }

    /// <summary>Whether dice explode on maximum value.</summary>
    public bool Exploding { get; init; }

    /// <summary>Maximum number of explosions allowed (prevents infinite loops).</summary>
    public int MaxExplosions { get; init; }

    /// <summary>Number of sides on each die.</summary>
    public int Faces => (int)DiceType;

    /// <summary>Minimum possible result (Count × 1 + Modifier).</summary>
    public int MinimumResult => Count + Modifier;

    /// <summary>Maximum possible result without explosions (Count × Faces + Modifier).</summary>
    public int MaximumResult => Count * Faces + Modifier;

    /// <summary>Average expected result (Count × ((Faces + 1) / 2) + Modifier).</summary>
    public float AverageResult => Count * ((Faces + 1) / 2f) + Modifier;

    /// <summary>
    /// Creates a new dice pool with validation.
    /// </summary>
    /// <param name="count">Number of dice to roll (must be at least 1).</param>
    /// <param name="diceType">Type of dice to roll.</param>
    /// <param name="modifier">Flat modifier to add to the result.</param>
    /// <param name="exploding">Whether dice explode on maximum value.</param>
    /// <param name="maxExplosions">Maximum explosions allowed (default 10).</param>
    /// <exception cref="ArgumentOutOfRangeException">If count is less than 1 or maxExplosions is negative.</exception>
    public DicePool(int count, DiceType diceType, int modifier = 0, bool exploding = false, int maxExplosions = 10)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Dice count must be at least 1");
        if (maxExplosions < 0)
            throw new ArgumentOutOfRangeException(nameof(maxExplosions), "Max explosions cannot be negative");

        Count = count;
        DiceType = diceType;
        Modifier = modifier;
        Exploding = exploding;
        MaxExplosions = maxExplosions;
    }

    /// <summary>
    /// Returns dice notation string (e.g., "3d6+5" or "1d6!").
    /// </summary>
    public override string ToString()
    {
        var notation = $"{Count}d{Faces}";
        if (Modifier > 0) notation += $"+{Modifier}";
        else if (Modifier < 0) notation += Modifier.ToString();
        if (Exploding) notation += "!";
        return notation;
    }

    /// <summary>Creates a d4 dice pool.</summary>
    public static DicePool D4(int count = 1, int modifier = 0) => new(count, DiceType.D4, modifier);

    /// <summary>Creates a d6 dice pool.</summary>
    public static DicePool D6(int count = 1, int modifier = 0) => new(count, DiceType.D6, modifier);

    /// <summary>Creates a d8 dice pool.</summary>
    public static DicePool D8(int count = 1, int modifier = 0) => new(count, DiceType.D8, modifier);

    /// <summary>Creates a d10 dice pool.</summary>
    public static DicePool D10(int count = 1, int modifier = 0) => new(count, DiceType.D10, modifier);

    /// <summary>Creates an exploding version of this dice pool.</summary>
    /// <param name="maxExplosions">Maximum number of explosions (default 10).</param>
    public DicePool WithExploding(int maxExplosions = 10) => this with { Exploding = true, MaxExplosions = maxExplosions };

    /// <summary>Creates a copy with a different modifier.</summary>
    public DicePool WithModifier(int modifier) => this with { Modifier = modifier };

    /// <summary>
    /// Parses dice notation string (e.g., "3d6+5", "1d10", "2d8-1", "1d6!").
    /// </summary>
    /// <param name="notation">The dice notation to parse.</param>
    /// <returns>A DicePool matching the notation.</returns>
    /// <exception cref="FormatException">If notation is invalid.</exception>
    public static DicePool Parse(string notation)
    {
        if (string.IsNullOrWhiteSpace(notation))
            throw new FormatException("Dice notation cannot be empty");

        var input = notation.Trim();

        // Handle exploding notation
        var exploding = input.EndsWith('!');
        if (exploding)
            input = input[..^1];

        // Find 'd' separator
        var dIndex = input.IndexOf('d', StringComparison.OrdinalIgnoreCase);
        if (dIndex < 0)
            throw new FormatException($"Invalid dice notation: {notation}");

        // Parse count (default to 1 if not specified)
        var countStr = dIndex > 0 ? input[..dIndex] : "1";
        if (!int.TryParse(countStr, out var count))
            throw new FormatException($"Invalid dice count: {countStr}");
        if (count < 1)
            throw new FormatException("Dice count must be at least 1");

        // Parse faces and modifier
        var rest = input[(dIndex + 1)..];
        var modifier = 0;
        var facesStr = rest;

        var plusIndex = rest.IndexOf('+');
        var minusIndex = rest.IndexOf('-');
        var modIndex = plusIndex >= 0 ? plusIndex : minusIndex;

        if (modIndex >= 0)
        {
            facesStr = rest[..modIndex];
            var modStr = rest[modIndex..];
            if (!int.TryParse(modStr, out modifier))
                throw new FormatException($"Invalid modifier: {modStr}");
        }

        if (string.IsNullOrEmpty(facesStr) || !int.TryParse(facesStr, out var faces))
            throw new FormatException($"Invalid dice faces: {facesStr}");

        var diceType = faces switch
        {
            4 => DiceType.D4,
            6 => DiceType.D6,
            8 => DiceType.D8,
            10 => DiceType.D10,
            _ => throw new FormatException($"Unsupported dice type: d{faces}")
        };

        return new DicePool(count, diceType, modifier, exploding);
    }

    /// <summary>
    /// Tries to parse dice notation, returning false if invalid.
    /// </summary>
    /// <param name="notation">The dice notation to parse.</param>
    /// <param name="result">The parsed DicePool, or default if parsing fails.</param>
    /// <returns>True if parsing succeeded; false otherwise.</returns>
    public static bool TryParse(string notation, out DicePool result)
    {
        try
        {
            result = Parse(notation);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
