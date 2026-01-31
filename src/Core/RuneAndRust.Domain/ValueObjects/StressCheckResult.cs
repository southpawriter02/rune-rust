// ═══════════════════════════════════════════════════════════════════════════════
// StressCheckResult.cs
// Immutable value object representing the outcome of a WILL-based stress
// resistance check. Maps the number of successes rolled to a reduction
// percentage and calculates the final stress amount after resistance.
// Part of the Result Object Pattern for the Trauma Economy system.
// Version: 0.18.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a stress resistance check.
/// </summary>
/// <remarks>
/// <para>
/// When a character encounters a stress source, they may roll a WILL-based
/// resistance check. The number of successes determines stress reduction:
/// </para>
/// <list type="table">
/// <listheader><term>Successes</term><description>Reduction</description></listheader>
/// <item><term>0</term><description>0% (full stress)</description></item>
/// <item><term>1</term><description>50% (half stress)</description></item>
/// <item><term>2-3</term><description>75% (quarter stress)</description></item>
/// <item><term>4+</term><description>100% (no stress)</description></item>
/// </list>
/// <para>
/// <strong>Immutability:</strong> This is a <c>readonly record struct</c> with a private
/// constructor. All properties are computed at creation time and cannot be modified.
/// Use the <see cref="Create"/> or <see cref="NoResistance"/> factory methods to
/// construct instances.
/// </para>
/// <para>
/// <strong>FinalStress Calculation:</strong> The final stress is calculated as
/// <c>BaseStress × (1 - ReductionPercent)</c>, truncated to an integer (not rounded).
/// For example, 15 base stress with 50% reduction yields 7 (not 8).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Character rolls 2 successes against 20 base stress
/// var result = StressCheckResult.Create(successes: 2, baseStress: 20);
/// Console.WriteLine(result.ReductionPercent); // 0.75m (75%)
/// Console.WriteLine(result.FinalStress);      // 5
/// Console.WriteLine(result.Succeeded);        // true
/// Console.WriteLine(result.WasFullyResisted); // false
///
/// // No resistance check performed (unavoidable stress)
/// var noResist = StressCheckResult.NoResistance(baseStress: 15);
/// Console.WriteLine(noResist.Succeeded);      // false
/// Console.WriteLine(noResist.FinalStress);    // 15
/// </code>
/// </example>
/// <seealso cref="StressApplicationResult"/>
/// <seealso cref="StressState"/>
public readonly record struct StressCheckResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored (set in constructor)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the resistance check had any successes.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Successes"/> &gt; 0, meaning some stress was reduced;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the number of successes rolled on the resistance check.
    /// </summary>
    /// <value>
    /// A non-negative integer, typically 0-4+, based on the WILL dice pool vs DC.
    /// Higher values yield greater stress reduction.
    /// </value>
    public int Successes { get; }

    /// <summary>
    /// Gets the original stress amount before resistance reduction.
    /// </summary>
    /// <value>
    /// A non-negative integer representing the raw stress from the stress source.
    /// </value>
    public int BaseStress { get; }

    /// <summary>
    /// Gets the final stress amount after resistance reduction.
    /// </summary>
    /// <value>
    /// Calculated as: <c>BaseStress × (1 - ReductionPercent)</c>, truncated to an integer.
    /// Always in the range [0, BaseStress].
    /// </value>
    /// <remarks>
    /// <para>
    /// Truncation (not rounding) is used for the integer conversion. For example:
    /// BaseStress=15, Successes=1 → 15 × 0.5 = 7.5 → FinalStress=7.
    /// </para>
    /// </remarks>
    public int FinalStress { get; }

    /// <summary>
    /// Gets the reduction percentage based on successes.
    /// </summary>
    /// <value>
    /// A decimal value representing the fraction of stress reduced:
    /// <list type="bullet">
    /// <item><description>0 successes: 0.00m (0%)</description></item>
    /// <item><description>1 success: 0.50m (50%)</description></item>
    /// <item><description>2-3 successes: 0.75m (75%)</description></item>
    /// <item><description>4+ successes: 1.00m (100%)</description></item>
    /// </list>
    /// </value>
    public decimal ReductionPercent { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the stress was fully resisted (no stress gained).
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="FinalStress"/> equals 0 (either 4+ successes
    /// or zero base stress); otherwise, <c>false</c>.
    /// </value>
    public bool WasFullyResisted => FinalStress == 0;

    /// <summary>
    /// Gets whether partial resistance occurred (some but not all stress reduced).
    /// </summary>
    /// <value>
    /// <c>true</c> when the check <see cref="Succeeded"/> but <see cref="FinalStress"/>
    /// is still greater than 0; otherwise, <c>false</c>.
    /// </value>
    public bool WasPartiallyResisted => Succeeded && FinalStress > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StressCheckResult"/> struct.
    /// </summary>
    /// <param name="successes">
    /// The number of successes on the resistance check. Must be non-negative.
    /// </param>
    /// <param name="baseStress">
    /// The original stress amount before reduction. Must be non-negative.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="successes"/> or <paramref name="baseStress"/> is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The constructor computes all derived properties from the input values:
    /// <list type="number">
    /// <item><description>Maps successes to <see cref="ReductionPercent"/> via the reduction table.</description></item>
    /// <item><description>Calculates <see cref="FinalStress"/> = BaseStress × (1 - ReductionPercent), truncated.</description></item>
    /// <item><description>Sets <see cref="Succeeded"/> = successes &gt; 0.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private StressCheckResult(int successes, int baseStress)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(successes);
        ArgumentOutOfRangeException.ThrowIfNegative(baseStress);

        Successes = successes;
        BaseStress = baseStress;
        Succeeded = successes > 0;
        ReductionPercent = CalculateReductionPercent(successes);
        FinalStress = (int)(baseStress * (1m - ReductionPercent));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a stress check result from the number of successes and base stress.
    /// </summary>
    /// <param name="successes">Number of successes on the resistance check. Must be non-negative.</param>
    /// <param name="baseStress">The original stress amount before reduction. Must be non-negative.</param>
    /// <returns>A new <see cref="StressCheckResult"/> instance with computed reduction.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="successes"/> or <paramref name="baseStress"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = StressCheckResult.Create(successes: 1, baseStress: 20);
    /// // result.ReductionPercent == 0.50m
    /// // result.FinalStress == 10
    /// // result.Succeeded == true
    /// </code>
    /// </example>
    public static StressCheckResult Create(int successes, int baseStress) =>
        new(successes, baseStress);

    /// <summary>
    /// Creates a result representing no resistance check (full stress applied).
    /// </summary>
    /// <param name="baseStress">The stress amount applied without resistance. Must be non-negative.</param>
    /// <returns>A result with 0 successes, 0% reduction, and full stress applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="baseStress"/> is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Used when a stress source is unavoidable (e.g., Narrative or Corruption sources)
    /// or when no resistance DC is specified.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = StressCheckResult.NoResistance(baseStress: 15);
    /// // result.Successes == 0
    /// // result.FinalStress == 15
    /// // result.Succeeded == false
    /// </code>
    /// </example>
    public static StressCheckResult NoResistance(int baseStress) =>
        new(0, baseStress);

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the reduction percentage from the success count using the
    /// stress resistance reduction table.
    /// </summary>
    /// <param name="successes">The number of successes rolled.</param>
    /// <returns>
    /// A decimal representing the reduction fraction:
    /// 0 → 0.00m, 1 → 0.50m, 2-3 → 0.75m, 4+ → 1.00m.
    /// </returns>
    private static decimal CalculateReductionPercent(int successes) => successes switch
    {
        0 => 0.00m,
        1 => 0.50m,
        2 or 3 => 0.75m,
        _ => 1.00m // 4+ successes = full resistance
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the stress check result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string in the format:
    /// <c>"Resistance: {Successes} successes, {ReductionPercent:P0} reduction ({BaseStress} → {FinalStress})"</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = StressCheckResult.Create(2, 20);
    /// var display = result.ToString();
    /// // Returns "Resistance: 2 successes, 75% reduction (20 → 5)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Resistance: {Successes} successes, {ReductionPercent:P0} reduction ({BaseStress} → {FinalStress})";
}
