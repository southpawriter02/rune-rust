// ═══════════════════════════════════════════════════════════════════════════════
// PointBuyCost.cs
// Value object representing the point cost to reach a specific attribute value
// in the point-buy allocation system. Each entry captures the individual cost
// for a single increment and the cumulative cost from the base value of 1.
// Version: 0.17.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents the point cost to reach a specific attribute value in the point-buy system.
/// </summary>
/// <remarks>
/// <para>
/// PointBuyCost is a value object that defines the cost structure for point-buy
/// attribute allocation during character creation. Each entry specifies the cost
/// to increment TO the target value from the previous value, plus the total
/// cumulative cost to reach that value from the base of 1.
/// </para>
/// <para>
/// The cost table uses a tiered pricing structure that creates meaningful trade-offs:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Standard tier (values 2-8): Each increment costs 1 point, making moderate
///       attribute values accessible to all builds.
///     </description>
///   </item>
///   <item>
///     <description>
///       Premium tier (values 9-10): Each increment costs 2 points, making
///       exceptional values a significant investment that limits other attributes.
///     </description>
///   </item>
/// </list>
/// <para>
/// The default cost table contains 9 entries (values 2 through 10), since the
/// base value of 1 has no cost. The cumulative cost to reach the maximum value
/// of 10 from base is 11 points (7 standard + 4 premium).
/// </para>
/// <para>
/// PointBuyCost instances are immutable value objects. Use the <see cref="Create"/>
/// factory method for validated construction, or the primary constructor for
/// direct initialization (e.g., when loading from configuration).
/// </para>
/// </remarks>
/// <param name="TargetValue">The attribute value this cost applies to (2-10).</param>
/// <param name="IndividualCost">Points required to increment from the previous value to this value (1 or 2).</param>
/// <param name="CumulativeCost">Total points required to reach this value from the base value of 1.</param>
/// <seealso cref="PointBuyConfiguration"/>
/// <seealso cref="AttributeAllocationState"/>
public readonly record struct PointBuyCost(
    int TargetValue,
    int IndividualCost,
    int CumulativeCost)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during cost creation and access.
    /// </summary>
    private static ILogger<PointBuyCost>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a premium tier value (9 or 10).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="TargetValue"/> is 9 or 10;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Premium tier values cost 2 points per increment instead of the standard 1 point.
    /// Investing in premium tier values is a significant trade-off, as 4 points
    /// (the cost of going from 8 to 10) could alternatively raise two other
    /// attributes by 4 levels each in the standard tier.
    /// </remarks>
    public bool IsPremiumTier => TargetValue >= 9;

    /// <summary>
    /// Gets whether this is a standard tier value (2-8).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="TargetValue"/> is between 2 and 8 (inclusive);
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Standard tier values cost 1 point per increment, making them the most
    /// efficient use of the point budget. Most balanced builds keep attributes
    /// within the standard tier.
    /// </remarks>
    public bool IsStandardTier => TargetValue >= 2 && TargetValue <= 8;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="PointBuyCost"/> with validation.
    /// </summary>
    /// <param name="targetValue">
    /// The attribute value this cost applies to. Must be between 2 and 10 (inclusive),
    /// since the base value of 1 has no cost.
    /// </param>
    /// <param name="individualCost">
    /// Points required to increment from the previous value to this value.
    /// Must be non-negative (typically 1 for standard tier, 2 for premium tier).
    /// </param>
    /// <param name="cumulativeCost">
    /// Total points required to reach this value from the base value of 1.
    /// Must be non-negative.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="PointBuyCost"/> instance with validated data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="targetValue"/> is less than 2 or greater than 10,
    /// or when <paramref name="individualCost"/> or <paramref name="cumulativeCost"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a standard tier cost entry
    /// var standardCost = PointBuyCost.Create(5, 1, 4);
    /// // standardCost.IsPremiumTier == false, standardCost.IsStandardTier == true
    ///
    /// // Create a premium tier cost entry
    /// var premiumCost = PointBuyCost.Create(9, 2, 9);
    /// // premiumCost.IsPremiumTier == true, premiumCost.IsStandardTier == false
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method validates all parameters before construction. Use this
    /// method when creating cost entries programmatically to ensure data integrity.
    /// The primary constructor can be used for deserialization or trusted data sources.
    /// </para>
    /// </remarks>
    public static PointBuyCost Create(
        int targetValue,
        int individualCost,
        int cumulativeCost,
        ILogger<PointBuyCost>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating PointBuyCost. TargetValue={TargetValue}, " +
            "IndividualCost={IndividualCost}, CumulativeCost={CumulativeCost}",
            targetValue,
            individualCost,
            cumulativeCost);

        // Validate target value is within the valid attribute range (2-10)
        // Base value of 1 has no cost entry, so minimum target is 2
        ArgumentOutOfRangeException.ThrowIfLessThan(targetValue, 2);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(targetValue, 10);

        // Validate costs are non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(individualCost);
        ArgumentOutOfRangeException.ThrowIfNegative(cumulativeCost);

        _logger?.LogDebug(
            "Validation passed for PointBuyCost. TargetValue={TargetValue}, " +
            "IndividualCost={IndividualCost}, CumulativeCost={CumulativeCost}, " +
            "IsPremium={IsPremium}",
            targetValue,
            individualCost,
            cumulativeCost,
            targetValue >= 9);

        var cost = new PointBuyCost(targetValue, individualCost, cumulativeCost);

        _logger?.LogInformation(
            "Created PointBuyCost for value {TargetValue}: " +
            "{IndividualCost} pts individual, {CumulativeCost} pts cumulative, " +
            "Tier={Tier}",
            targetValue,
            individualCost,
            cumulativeCost,
            cost.IsPremiumTier ? "Premium" : "Standard");

        return cost;
    }

    /// <summary>
    /// Creates the default cost table for the point-buy system.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{PointBuyCost}"/> containing 9 entries (values 2 through 10)
    /// with the standard tiered cost structure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The default cost table implements the following structure:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Target Value</term>
    ///     <description>Individual Cost / Cumulative Cost</description>
    ///   </listheader>
    ///   <item><term>2-8</term><description>1 point each / 1-7 cumulative</description></item>
    ///   <item><term>9</term><description>2 points / 9 cumulative</description></item>
    ///   <item><term>10</term><description>2 points / 11 cumulative</description></item>
    /// </list>
    /// <para>
    /// This table is used as the default when no configuration file is provided,
    /// and serves as the canonical reference for the point-buy cost structure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var table = PointBuyCost.CreateDefaultCostTable();
    /// // table.Count == 9 (values 2 through 10)
    /// // table[0] == PointBuyCost(2, 1, 1)   — first standard tier entry
    /// // table[6] == PointBuyCost(8, 1, 7)   — last standard tier entry
    /// // table[7] == PointBuyCost(9, 2, 9)   — first premium tier entry
    /// // table[8] == PointBuyCost(10, 2, 11) — maximum value entry
    /// </code>
    /// </example>
    public static IReadOnlyList<PointBuyCost> CreateDefaultCostTable(
        ILogger<PointBuyCost>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug("Creating default point-buy cost table with 9 entries (values 2-10)");

        var table = new List<PointBuyCost>
        {
            new(2, 1, 1),
            new(3, 1, 2),
            new(4, 1, 3),
            new(5, 1, 4),
            new(6, 1, 5),
            new(7, 1, 6),
            new(8, 1, 7),
            new(9, 2, 9),
            new(10, 2, 11)
        };

        _logger?.LogInformation(
            "Created default point-buy cost table. " +
            "EntryCount={EntryCount}, StandardTierEntries={StandardCount}, " +
            "PremiumTierEntries={PremiumCount}, MaxCumulativeCost={MaxCumulative}",
            table.Count,
            table.Count(c => c.IsStandardTier),
            table.Count(c => c.IsPremiumTier),
            table[^1].CumulativeCost);

        return table;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this point-buy cost entry.
    /// </summary>
    /// <returns>
    /// A string in the format "Value {TargetValue}: {IndividualCost} pts (cumulative: {CumulativeCost})"
    /// (e.g., "Value 9: 2 pts (cumulative: 9)").
    /// </returns>
    public override string ToString() =>
        $"Value {TargetValue}: {IndividualCost} pts (cumulative: {CumulativeCost})";
}
