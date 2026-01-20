using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for reproducible random number generation from seeds.
/// </summary>
/// <remarks>
/// The SeededRandomService ensures that:
/// - Same master seed produces identical results
/// - Same position + context produces identical sub-results
/// - Different positions/contexts are properly isolated
/// - Results are deterministic across sessions and platforms
/// </remarks>
public interface ISeededRandomService
{
    /// <summary>
    /// Gets the master seed for this generator.
    /// </summary>
    int MasterSeed { get; }

    /// <summary>
    /// Gets a deterministic random integer for a position.
    /// </summary>
    /// <param name="position">The position for sub-seed derivation.</param>
    /// <param name="context">Context string for isolation (default: "default").</param>
    /// <returns>A deterministic random integer.</returns>
    int NextForPosition(Position3D position, string context = "default");

    /// <summary>
    /// Gets a deterministic random integer in range for a position.
    /// </summary>
    /// <param name="position">The position for sub-seed derivation.</param>
    /// <param name="minInclusive">Minimum value (inclusive).</param>
    /// <param name="maxExclusive">Maximum value (exclusive).</param>
    /// <param name="context">Context string for isolation.</param>
    /// <returns>A deterministic random integer in the specified range.</returns>
    int NextForPosition(Position3D position, int minInclusive, int maxExclusive, string context = "default");

    /// <summary>
    /// Gets a deterministic float (0.0-1.0) for a position.
    /// </summary>
    /// <param name="position">The position for sub-seed derivation.</param>
    /// <param name="context">Context string for isolation.</param>
    /// <returns>A deterministic float between 0.0 and 1.0.</returns>
    float NextFloatForPosition(Position3D position, string context = "default");

    /// <summary>
    /// Selects a weighted random item for a position.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="position">The position for sub-seed derivation.</param>
    /// <param name="items">Items with their weights.</param>
    /// <param name="context">Context string for isolation.</param>
    /// <returns>The selected item.</returns>
    T SelectWeighted<T>(Position3D position, IEnumerable<(T item, int weight)> items, string context = "default");

    /// <summary>
    /// Clears cached sub-generators to free memory.
    /// </summary>
    void ClearSubGenerators();
}

/// <summary>
/// Static utility methods for seed string conversion.
/// </summary>
public static class SeedStringUtility
{
    /// <summary>
    /// Character set for seed strings (excludes ambiguous characters).
    /// </summary>
    private const string SeedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>
    /// Generates a human-readable seed string.
    /// </summary>
    /// <param name="seed">The integer seed to convert.</param>
    /// <returns>An 8-character alphanumeric string.</returns>
    public static string ToSeedString(int seed)
    {
        var result = new char[8];
        var value = (uint)seed;

        for (int i = 0; i < 8; i++)
        {
            result[i] = SeedChars[(int)(value % (uint)SeedChars.Length)];
            value /= (uint)SeedChars.Length;
        }

        return new string(result);
    }

    /// <summary>
    /// Parses a seed string back to integer.
    /// </summary>
    /// <param name="seedString">The seed string to parse.</param>
    /// <returns>The integer seed value.</returns>
    /// <exception cref="ArgumentException">Thrown if the seed string is invalid.</exception>
    public static int FromSeedString(string seedString)
    {
        if (string.IsNullOrWhiteSpace(seedString))
            throw new ArgumentException("Seed string cannot be empty.", nameof(seedString));

        if (seedString.Length != 8)
            throw new ArgumentException($"Seed string must be exactly 8 characters (got {seedString.Length}).", nameof(seedString));

        uint value = 0;
        uint multiplier = 1;

        foreach (char c in seedString.ToUpperInvariant())
        {
            var index = SeedChars.IndexOf(c);
            if (index < 0)
                throw new ArgumentException($"Invalid seed character: '{c}'. Valid characters: {SeedChars}", nameof(seedString));

            value += (uint)index * multiplier;
            multiplier *= (uint)SeedChars.Length;
        }

        return (int)value;
    }

    /// <summary>
    /// Validates a seed string without parsing it.
    /// </summary>
    /// <param name="seedString">The seed string to validate.</param>
    /// <returns>True if the seed string is valid; false otherwise.</returns>
    public static bool IsValidSeedString(string seedString)
    {
        if (string.IsNullOrWhiteSpace(seedString) || seedString.Length != 8)
            return false;

        return seedString.ToUpperInvariant().All(c => SeedChars.Contains(c));
    }
}
