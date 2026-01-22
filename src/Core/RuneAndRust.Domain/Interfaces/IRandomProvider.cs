namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Provides random number generation with seeding and state management capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts random number generation to enable:
/// <list type="bullet">
///   <item><description>Deterministic testing with known seeds</description></item>
///   <item><description>Reproducible gameplay sequences for debugging</description></item>
///   <item><description>Context-aware seeding (combat vs. exploration)</description></item>
///   <item><description>Save/restore state for roll replay</description></item>
/// </list>
/// </para>
/// <para>
/// Implementation note: The state save/restore mechanism saves the seed value,
/// not the internal Random state. Restoring recreates a new Random with the
/// saved seed, which produces the same sequence from the beginning.
/// </para>
/// </remarks>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a random integer within the specified range.
    /// </summary>
    /// <param name="minInclusive">The inclusive lower bound of the random number.</param>
    /// <param name="maxExclusive">The exclusive upper bound of the random number.</param>
    /// <returns>A random integer where minInclusive ≤ value &lt; maxExclusive.</returns>
    /// <remarks>
    /// Follows the standard .NET Random.Next() convention:
    /// <list type="bullet">
    ///   <item><description>minInclusive is included in the range</description></item>
    ///   <item><description>maxExclusive is excluded from the range</description></item>
    /// </list>
    /// For dice rolls: Next(1, 11) returns 1-10 inclusive.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when maxExclusive is less than or equal to minInclusive.
    /// </exception>
    /// <example>
    /// <code>
    /// var d10Roll = provider.Next(1, 11);  // Returns 1-10
    /// var d6Roll = provider.Next(1, 7);    // Returns 1-6
    /// </code>
    /// </example>
    int Next(int minInclusive, int maxExclusive);

    /// <summary>
    /// Returns multiple random integers within the specified range.
    /// </summary>
    /// <param name="count">The number of random values to generate.</param>
    /// <param name="minInclusive">The inclusive lower bound of each random number.</param>
    /// <param name="maxExclusive">The exclusive upper bound of each random number.</param>
    /// <returns>An array of random integers.</returns>
    /// <remarks>
    /// More efficient than calling Next() multiple times when generating
    /// dice pool results. The sequence is deterministic for a given seed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when count is negative or maxExclusive is less than or equal to minInclusive.
    /// </exception>
    /// <example>
    /// <code>
    /// // Roll 5d10 in one call
    /// var diceRolls = provider.NextMany(5, 1, 11);
    /// </code>
    /// </example>
    int[] NextMany(int count, int minInclusive, int maxExclusive);

    /// <summary>
    /// Sets the seed for the random number generator.
    /// </summary>
    /// <param name="seed">The seed value to use.</param>
    /// <remarks>
    /// <para>
    /// Setting a seed resets the RNG to produce a deterministic sequence.
    /// The same seed always produces the same sequence of numbers.
    /// </para>
    /// <para>
    /// Use cases:
    /// <list type="bullet">
    ///   <item><description>Testing: Set known seed for predictable results</description></item>
    ///   <item><description>Combat: Lock seed to prevent save-scumming</description></item>
    ///   <item><description>Replay: Recreate exact roll sequences</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    void SetSeed(int seed);

    /// <summary>
    /// Gets the current seed value.
    /// </summary>
    /// <returns>The seed that was used to initialize the current RNG state.</returns>
    /// <remarks>
    /// This returns the seed that was set via constructor or SetSeed(),
    /// not a representation of the current internal state.
    /// </remarks>
    int GetCurrentSeed();

    /// <summary>
    /// Saves the current RNG state for later restoration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Saves the current seed value so that RestoreState() can recreate
    /// the same sequence. Only one state can be saved at a time;
    /// calling SaveState() again overwrites the previous saved state.
    /// </para>
    /// <para>
    /// Note: This saves the seed, not the exact internal state.
    /// Restoring will replay the sequence from the beginning.
    /// </para>
    /// </remarks>
    void SaveState();

    /// <summary>
    /// Restores the RNG to the previously saved state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Recreates the RNG with the saved seed, allowing the same
    /// sequence of numbers to be generated again.
    /// </para>
    /// <para>
    /// If SaveState() was never called, this restores to the
    /// initial seed used at construction.
    /// </para>
    /// </remarks>
    void RestoreState();
}
