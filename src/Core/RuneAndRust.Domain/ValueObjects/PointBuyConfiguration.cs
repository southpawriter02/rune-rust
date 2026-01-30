// ═══════════════════════════════════════════════════════════════════════════════
// PointBuyConfiguration.cs
// Value object holding all configuration for the point-buy attribute allocation
// system, including starting point pools, attribute value constraints, the
// complete cost table, and cost calculation logic.
// Version: 0.17.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

/// <summary>
/// Configuration for the point-buy attribute allocation system.
/// </summary>
/// <remarks>
/// <para>
/// PointBuyConfiguration provides all parameters needed for Advanced mode
/// attribute allocation during character creation. It encapsulates:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Starting point pools: 15 points for most archetypes, 14 for the Adept
///       archetype (balancing their broader ability access with slightly lower
///       base attributes).
///     </description>
///   </item>
///   <item>
///     <description>
///       Attribute value constraints: minimum of 1 (base) and maximum of 10
///       per attribute, defining the valid allocation range.
///     </description>
///   </item>
///   <item>
///     <description>
///       Cost table: a tiered pricing structure where standard values (2-8) cost
///       1 point each and premium values (9-10) cost 2 points each.
///     </description>
///   </item>
/// </list>
/// <para>
/// The configuration also provides cost calculation methods for both increasing
/// and decreasing attribute values, enabling bidirectional point management
/// during the allocation process.
/// </para>
/// <para>
/// PointBuyConfiguration instances are immutable value objects. Use the
/// <see cref="CreateDefault"/> factory method to create a configuration with
/// the standard game balance values, or construct directly from deserialized
/// configuration data.
/// </para>
/// </remarks>
/// <seealso cref="PointBuyCost"/>
/// <seealso cref="AttributeAllocationState"/>
public readonly record struct PointBuyConfiguration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during configuration creation and cost calculations.
    /// </summary>
    private static ILogger<PointBuyConfiguration>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the standard starting point pool.
    /// </summary>
    /// <value>
    /// The number of points available for attribute allocation for most archetypes.
    /// Default is 15.
    /// </value>
    /// <remarks>
    /// This pool applies to Warrior, Skirmisher, and Mystic archetypes.
    /// The Adept archetype uses <see cref="AdeptStartingPoints"/> instead.
    /// </remarks>
    public int StartingPoints { get; init; }

    /// <summary>
    /// Gets the Adept archetype's starting point pool.
    /// </summary>
    /// <value>
    /// The number of points available for attribute allocation for the Adept archetype.
    /// Default is 14.
    /// </value>
    /// <remarks>
    /// The Adept receives 1 fewer point than other archetypes to balance their
    /// broader utility abilities and +20% consumable effectiveness bonus.
    /// </remarks>
    public int AdeptStartingPoints { get; init; }

    /// <summary>
    /// Gets the minimum attribute value.
    /// </summary>
    /// <value>
    /// The lowest value any attribute can have. Default is 1.
    /// All attributes begin at this value in Advanced mode.
    /// </value>
    public int MinAttributeValue { get; init; }

    /// <summary>
    /// Gets the maximum attribute value.
    /// </summary>
    /// <value>
    /// The highest value any attribute can reach. Default is 10.
    /// Reaching this value requires 11 cumulative points from base.
    /// </value>
    public int MaxAttributeValue { get; init; }

    /// <summary>
    /// Gets the cost table for each target value.
    /// </summary>
    /// <value>
    /// An ordered list of <see cref="PointBuyCost"/> entries, one for each
    /// target value from 2 through <see cref="MaxAttributeValue"/>.
    /// </value>
    /// <remarks>
    /// The cost table defines the tiered pricing structure. Each entry specifies
    /// the individual cost (for a single increment) and cumulative cost (total
    /// from base) for its target value.
    /// </remarks>
    public IReadOnlyList<PointBuyCost> CostTable { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of entries in the cost table.
    /// </summary>
    /// <value>The count of <see cref="PointBuyCost"/> entries (typically 9 for values 2-10).</value>
    public int CostTableEntryCount => CostTable?.Count ?? 0;

    /// <summary>
    /// Gets whether the cost table has been populated.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="CostTable"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasCostTable => CostTable?.Count > 0;

    /// <summary>
    /// Gets the maximum cumulative cost in the cost table.
    /// </summary>
    /// <value>
    /// The cumulative cost of the last entry in the cost table (typically 11),
    /// or 0 if the cost table is empty.
    /// </value>
    public int MaxCumulativeCost => CostTable?.Count > 0 ? CostTable[^1].CumulativeCost : 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the default point-buy configuration with standard game balance values.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="PointBuyConfiguration"/> with:
    /// <list type="bullet">
    ///   <item><description>StartingPoints: 15</description></item>
    ///   <item><description>AdeptStartingPoints: 14</description></item>
    ///   <item><description>MinAttributeValue: 1</description></item>
    ///   <item><description>MaxAttributeValue: 10</description></item>
    ///   <item><description>CostTable: 9 entries (values 2-10) with tiered pricing</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// // config.StartingPoints == 15
    /// // config.AdeptStartingPoints == 14
    /// // config.CostTable.Count == 9
    /// // config.CalculateCost(1, 10) == 11
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method creates the canonical configuration used in the base game.
    /// Configuration loaded from JSON files should produce equivalent values unless
    /// balance adjustments have been made.
    /// </para>
    /// </remarks>
    public static PointBuyConfiguration CreateDefault(
        ILogger<PointBuyConfiguration>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug("Creating default PointBuyConfiguration");

        var config = new PointBuyConfiguration
        {
            StartingPoints = 15,
            AdeptStartingPoints = 14,
            MinAttributeValue = 1,
            MaxAttributeValue = 10,
            CostTable = PointBuyCost.CreateDefaultCostTable()
        };

        _logger?.LogInformation(
            "Created default PointBuyConfiguration. " +
            "StartingPoints={StartingPoints}, AdeptStartingPoints={AdeptStartingPoints}, " +
            "Range=[{Min}-{Max}], CostTableEntries={CostTableCount}, " +
            "MaxCumulativeCost={MaxCumulativeCost}",
            config.StartingPoints,
            config.AdeptStartingPoints,
            config.MinAttributeValue,
            config.MaxAttributeValue,
            config.CostTableEntryCount,
            config.MaxCumulativeCost);

        return config;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ARCHETYPE POINT POOL METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the starting points for a specific archetype.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "adept").
    /// Comparison is case-insensitive.
    /// </param>
    /// <returns>
    /// 14 for the Adept archetype, or <see cref="StartingPoints"/> (15) for all others.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// config.GetStartingPointsForArchetype("warrior");  // 15
    /// config.GetStartingPointsForArchetype("adept");    // 14
    /// config.GetStartingPointsForArchetype("ADEPT");    // 14 (case-insensitive)
    /// config.GetStartingPointsForArchetype("mystic");   // 15
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// The Adept archetype receives fewer starting points to balance their broader
    /// utility capabilities. All other archetypes (Warrior, Skirmisher, Mystic)
    /// share the standard pool of 15 points.
    /// </para>
    /// </remarks>
    public int GetStartingPointsForArchetype(string archetypeId)
    {
        var isAdept = archetypeId.Equals("adept", StringComparison.OrdinalIgnoreCase);
        var points = isAdept ? AdeptStartingPoints : StartingPoints;

        _logger?.LogDebug(
            "GetStartingPointsForArchetype. ArchetypeId='{ArchetypeId}', " +
            "IsAdept={IsAdept}, StartingPoints={StartingPoints}",
            archetypeId,
            isAdept,
            points);

        return points;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COST CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the cost to change an attribute from one value to another.
    /// </summary>
    /// <param name="fromValue">Current attribute value (1-10).</param>
    /// <param name="toValue">Target attribute value (1-10).</param>
    /// <returns>
    /// Points to spend (positive) for increases, or points to refund (negative) for decreases.
    /// Returns 0 when <paramref name="fromValue"/> equals <paramref name="toValue"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    ///
    /// // Standard tier increases
    /// config.CalculateCost(1, 5);   // 4 (4 × 1 point each)
    /// config.CalculateCost(1, 8);   // 7 (7 × 1 point each)
    ///
    /// // Premium tier increases
    /// config.CalculateCost(8, 10);  // 4 (2 × 2 points each)
    /// config.CalculateCost(1, 10);  // 11 (7 standard + 4 premium)
    ///
    /// // Decreases (refunds)
    /// config.CalculateCost(10, 8);  // -4 (refund of 2 × 2 points)
    /// config.CalculateCost(5, 1);   // -4 (refund of 4 × 1 point)
    ///
    /// // No change
    /// config.CalculateCost(5, 5);   // 0
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// Cost calculation is bidirectional: increasing an attribute costs points
    /// (positive return), while decreasing refunds points (negative return).
    /// The refund amount always equals the cost of the equivalent increase,
    /// ensuring no points are lost during reallocation.
    /// </para>
    /// <para>
    /// For decreases, the method recursively calculates the cost of the
    /// equivalent increase and negates it, guaranteeing symmetry.
    /// </para>
    /// </remarks>
    public int CalculateCost(int fromValue, int toValue)
    {
        // No change: zero cost
        if (fromValue == toValue)
        {
            _logger?.LogDebug(
                "CalculateCost: No change. FromValue={FromValue}, ToValue={ToValue}, Cost=0",
                fromValue,
                toValue);
            return 0;
        }

        // Decrease: negate the cost of the equivalent increase (refund)
        if (toValue < fromValue)
        {
            var refund = -CalculateCost(toValue, fromValue);

            _logger?.LogDebug(
                "CalculateCost: Decrease (refund). FromValue={FromValue}, ToValue={ToValue}, " +
                "Refund={Refund}",
                fromValue,
                toValue,
                refund);

            return refund;
        }

        // Increase: sum individual costs for each step
        int cost = 0;
        for (int value = fromValue + 1; value <= toValue; value++)
        {
            cost += GetIndividualCost(value);
        }

        _logger?.LogDebug(
            "CalculateCost: Increase. FromValue={FromValue}, ToValue={ToValue}, " +
            "Cost={Cost}",
            fromValue,
            toValue,
            cost);

        return cost;
    }

    /// <summary>
    /// Gets the individual cost for incrementing to a specific target value.
    /// </summary>
    /// <param name="targetValue">The target attribute value (2-10).</param>
    /// <returns>
    /// The point cost for a single increment to the target value.
    /// Returns 0 if <paramref name="targetValue"/> is at or below <see cref="MinAttributeValue"/>.
    /// Falls back to tier-based defaults (1 for standard, 2 for premium) if the
    /// value is not found in the cost table.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// config.GetIndividualCost(5);   // 1 (standard tier)
    /// config.GetIndividualCost(9);   // 2 (premium tier)
    /// config.GetIndividualCost(1);   // 0 (base value, no cost)
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This method first checks the cost table for an exact match. If no entry
    /// is found (e.g., due to partial configuration), it falls back to the
    /// tier-based default: 1 point for values 2-8, 2 points for values 9-10.
    /// </para>
    /// </remarks>
    public int GetIndividualCost(int targetValue)
    {
        if (targetValue <= MinAttributeValue)
        {
            _logger?.LogDebug(
                "GetIndividualCost: At or below minimum. " +
                "TargetValue={TargetValue}, MinValue={MinValue}, Cost=0",
                targetValue,
                MinAttributeValue);
            return 0;
        }

        var costEntry = CostTable.FirstOrDefault(c => c.TargetValue == targetValue);
        var cost = costEntry.TargetValue == targetValue
            ? costEntry.IndividualCost
            : (targetValue <= 8 ? 1 : 2);

        _logger?.LogDebug(
            "GetIndividualCost: TargetValue={TargetValue}, " +
            "IndividualCost={Cost}, Source={Source}",
            targetValue,
            cost,
            costEntry.TargetValue == targetValue ? "CostTable" : "Fallback");

        return cost;
    }

    /// <summary>
    /// Gets the cumulative cost from the base value to a specific target value.
    /// </summary>
    /// <param name="targetValue">The target attribute value (1-10).</param>
    /// <returns>
    /// The total point cost to reach <paramref name="targetValue"/> from <see cref="MinAttributeValue"/>.
    /// Returns 0 if <paramref name="targetValue"/> is at or below <see cref="MinAttributeValue"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// config.GetCumulativeCost(1);   // 0 (base value)
    /// config.GetCumulativeCost(5);   // 4
    /// config.GetCumulativeCost(8);   // 7
    /// config.GetCumulativeCost(10);  // 11
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This method first checks the cost table for an exact match. If no entry
    /// is found, it calculates the cost by summing individual costs from
    /// <see cref="MinAttributeValue"/> to the target value.
    /// </para>
    /// </remarks>
    public int GetCumulativeCost(int targetValue)
    {
        if (targetValue <= MinAttributeValue)
        {
            _logger?.LogDebug(
                "GetCumulativeCost: At or below minimum. " +
                "TargetValue={TargetValue}, CumulativeCost=0",
                targetValue);
            return 0;
        }

        var costEntry = CostTable.FirstOrDefault(c => c.TargetValue == targetValue);
        var cost = costEntry.TargetValue == targetValue
            ? costEntry.CumulativeCost
            : CalculateCost(MinAttributeValue, targetValue);

        _logger?.LogDebug(
            "GetCumulativeCost: TargetValue={TargetValue}, " +
            "CumulativeCost={Cost}, Source={Source}",
            targetValue,
            cost,
            costEntry.TargetValue == targetValue ? "CostTable" : "Calculated");

        return cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AFFORDABILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a value change can be afforded with the available points.
    /// </summary>
    /// <param name="currentValue">The current attribute value (1-10).</param>
    /// <param name="targetValue">The desired target value (1-10).</param>
    /// <param name="pointsRemaining">The number of unspent points available.</param>
    /// <returns>
    /// <c>true</c> if the target value is within valid range and the cost does
    /// not exceed <paramref name="pointsRemaining"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// config.CanAfford(1, 5, 10);    // true (costs 4, have 10)
    /// config.CanAfford(1, 10, 5);    // false (costs 11, have 5)
    /// config.CanAfford(1, 11, 15);   // false (target > max)
    /// config.CanAfford(5, 3, 0);     // true (decrease = refund, always affordable)
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This method validates both the target value range and the point cost.
    /// Decreases always return <c>true</c> (assuming valid range) because they
    /// refund points rather than spending them.
    /// </para>
    /// </remarks>
    public bool CanAfford(int currentValue, int targetValue, int pointsRemaining)
    {
        // Validate target is within attribute range
        if (targetValue < MinAttributeValue || targetValue > MaxAttributeValue)
        {
            _logger?.LogDebug(
                "CanAfford: Target out of range. CurrentValue={CurrentValue}, " +
                "TargetValue={TargetValue}, ValidRange=[{Min}-{Max}], Result=false",
                currentValue,
                targetValue,
                MinAttributeValue,
                MaxAttributeValue);
            return false;
        }

        var cost = CalculateCost(currentValue, targetValue);
        var canAfford = cost <= pointsRemaining;

        _logger?.LogDebug(
            "CanAfford: CurrentValue={CurrentValue}, TargetValue={TargetValue}, " +
            "Cost={Cost}, PointsRemaining={PointsRemaining}, CanAfford={CanAfford}",
            currentValue,
            targetValue,
            cost,
            pointsRemaining,
            canAfford);

        return canAfford;
    }

    /// <summary>
    /// Gets the maximum value reachable from the current value with available points.
    /// </summary>
    /// <param name="currentValue">The current attribute value (1-10).</param>
    /// <param name="pointsRemaining">The number of unspent points available.</param>
    /// <returns>
    /// The highest attribute value reachable without exceeding <paramref name="pointsRemaining"/>.
    /// Returns <paramref name="currentValue"/> if no increase is affordable.
    /// </returns>
    /// <example>
    /// <code>
    /// var config = PointBuyConfiguration.CreateDefault();
    /// config.GetMaxReachableValue(1, 15);  // 10 (15 pts, costs 11 to reach 10)
    /// config.GetMaxReachableValue(1, 7);   // 8 (7 pts, standard tier max)
    /// config.GetMaxReachableValue(1, 0);   // 1 (no points to spend)
    /// config.GetMaxReachableValue(8, 3);   // 9 (3 pts, can afford one premium step)
    /// config.GetMaxReachableValue(8, 2);   // 9 (2 pts, exactly one premium step)
    /// config.GetMaxReachableValue(8, 1);   // 8 (1 pt, can't afford premium)
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This method iteratively checks each value above <paramref name="currentValue"/>
    /// until the cost exceeds the available points or the maximum attribute value
    /// is reached. It stops at the first unaffordable value, returning the last
    /// affordable one.
    /// </para>
    /// </remarks>
    public int GetMaxReachableValue(int currentValue, int pointsRemaining)
    {
        int maxValue = currentValue;

        for (int v = currentValue + 1; v <= MaxAttributeValue; v++)
        {
            if (CalculateCost(currentValue, v) <= pointsRemaining)
                maxValue = v;
            else
                break;
        }

        _logger?.LogDebug(
            "GetMaxReachableValue: CurrentValue={CurrentValue}, " +
            "PointsRemaining={PointsRemaining}, MaxReachable={MaxReachable}",
            currentValue,
            pointsRemaining,
            maxValue);

        return maxValue;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this point-buy configuration.
    /// </summary>
    /// <returns>
    /// A string in the format "PointBuy: {StartingPoints} pts ({CostTableEntryCount} entries, range {Min}-{Max})"
    /// (e.g., "PointBuy: 15 pts (9 entries, range 1-10)").
    /// </returns>
    public override string ToString() =>
        $"PointBuy: {StartingPoints} pts ({CostTableEntryCount} entries, range {MinAttributeValue}-{MaxAttributeValue})";
}
