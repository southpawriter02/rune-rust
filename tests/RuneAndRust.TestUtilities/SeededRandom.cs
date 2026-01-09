namespace RuneAndRust.TestUtilities;

/// <summary>
/// Provides seeded random number generation for deterministic tests.
/// </summary>
/// <remarks>
/// Use this class when tests require reproducible random sequences.
/// The same seed will always produce the same sequence of values.
/// </remarks>
public static class SeededRandom
{
    /// <summary>
    /// Default seed for reproducible tests.
    /// </summary>
    public const int DefaultSeed = 12345;

    /// <summary>
    /// Creates a seeded Random instance for deterministic tests.
    /// </summary>
    /// <param name="seed">The seed value (default: 12345).</param>
    /// <returns>A seeded Random instance.</returns>
    public static Random Create(int seed = DefaultSeed) => new(seed);

    /// <summary>
    /// Creates multiple seeded Random instances for parallel test scenarios.
    /// </summary>
    /// <param name="count">The number of Random instances to create.</param>
    /// <param name="baseSeed">The base seed value. Each instance gets baseSeed + index.</param>
    /// <returns>An enumerable of seeded Random instances.</returns>
    public static IEnumerable<Random> CreateMultiple(int count, int baseSeed = DefaultSeed)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new Random(baseSeed + i);
        }
    }
}
