// ═══════════════════════════════════════════════════════════════════════════════
// PointBuyConfigurationTests.cs
// Unit tests for the PointBuyConfiguration value object verifying factory
// methods, cost calculations, affordability checks, and archetype-specific
// point pools.
// Version: 0.17.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="PointBuyConfiguration"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that PointBuyConfiguration correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates default configuration with standard game balance values</description></item>
///   <item><description>Calculates costs for attribute increases (standard and premium tiers)</description></item>
///   <item><description>Calculates refunds for attribute decreases (negative costs)</description></item>
///   <item><description>Returns zero cost for no-change operations</description></item>
///   <item><description>Returns correct starting points per archetype (15 standard, 14 Adept)</description></item>
///   <item><description>Checks affordability correctly for increases, decreases, and out-of-range values</description></item>
///   <item><description>Determines maximum reachable values given available points</description></item>
///   <item><description>Produces correct ToString output for debugging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="PointBuyConfiguration"/>
/// <seealso cref="PointBuyCost"/>
[TestFixture]
public class PointBuyConfigurationTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    private PointBuyConfiguration _config;

    /// <summary>
    /// Creates a fresh default configuration before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _config = PointBuyConfiguration.CreateDefault();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — CreateDefault
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.CreateDefault"/> produces
    /// a configuration with the correct standard game balance values.
    /// </summary>
    [Test]
    public void CreateDefault_HasCorrectDefaults()
    {
        // Assert
        _config.StartingPoints.Should().Be(15);
        _config.AdeptStartingPoints.Should().Be(14);
        _config.MinAttributeValue.Should().Be(1);
        _config.MaxAttributeValue.Should().Be(10);
        _config.CostTable.Should().HaveCount(9);
        _config.CostTableEntryCount.Should().Be(9);
        _config.HasCostTable.Should().BeTrue();
        _config.MaxCumulativeCost.Should().Be(11);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COST CALCULATION TESTS — CalculateCost
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the cost from base (1) to the end of the standard tier (8)
    /// is 7 points (7 increments × 1 point each).
    /// </summary>
    [Test]
    public void CalculateCost_From1To8_Returns7Points()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(1, 8);

        // Assert
        cost.Should().Be(7);
    }

    /// <summary>
    /// Verifies that the cost from the end of standard tier (8) to maximum (10)
    /// is 4 points (2 increments × 2 points each in premium tier).
    /// </summary>
    [Test]
    public void CalculateCost_From8To10_Returns4Points()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(8, 10);

        // Assert
        cost.Should().Be(4);
    }

    /// <summary>
    /// Verifies that the cost from base (1) to maximum (10) is 11 points
    /// (7 standard + 4 premium), matching the maximum cumulative cost.
    /// </summary>
    [Test]
    public void CalculateCost_From1To10_Returns11Points()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(1, 10);

        // Assert
        cost.Should().Be(11);
    }

    /// <summary>
    /// Verifies that a decrease returns a negative value (refund),
    /// equal in magnitude to the cost of the equivalent increase.
    /// </summary>
    [Test]
    public void CalculateCost_Decrease_ReturnsNegativeRefund()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(10, 8);

        // Assert
        cost.Should().Be(-4);
    }

    /// <summary>
    /// Verifies that the cost of a larger decrease correctly sums both
    /// premium and standard tier refunds.
    /// </summary>
    [Test]
    public void CalculateCost_LargeDecrease_ReturnsCorrectRefund()
    {
        // Arrange & Act — Decrease from 10 to 6: refund 2+2+1+1 = 6 points
        var cost = _config.CalculateCost(10, 6);

        // Assert
        cost.Should().Be(-6);
    }

    /// <summary>
    /// Verifies that no change (same from and to value) returns zero cost.
    /// </summary>
    [Test]
    public void CalculateCost_SameValue_ReturnsZero()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(5, 5);

        // Assert
        cost.Should().Be(0);
    }

    /// <summary>
    /// Verifies cost for a standard-only increase from 1 to 5 is 4 points.
    /// </summary>
    [Test]
    public void CalculateCost_StandardTierOnly_ReturnsCorrectCost()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(1, 5);

        // Assert
        cost.Should().Be(4);
    }

    /// <summary>
    /// Verifies cost for a single premium increment from 8 to 9 is 2 points.
    /// </summary>
    [Test]
    public void CalculateCost_SinglePremiumIncrement_Returns2Points()
    {
        // Arrange & Act
        var cost = _config.CalculateCost(8, 9);

        // Assert
        cost.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INDIVIDUAL AND CUMULATIVE COST TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetIndividualCost"/> returns
    /// the correct cost for standard and premium tier values.
    /// </summary>
    /// <param name="targetValue">The target attribute value.</param>
    /// <param name="expectedCost">The expected individual cost.</param>
    [Test]
    [TestCase(2, 1)]
    [TestCase(5, 1)]
    [TestCase(8, 1)]
    [TestCase(9, 2)]
    [TestCase(10, 2)]
    public void GetIndividualCost_ReturnsCorrectCost(int targetValue, int expectedCost)
    {
        // Arrange & Act
        var cost = _config.GetIndividualCost(targetValue);

        // Assert
        cost.Should().Be(expectedCost);
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetIndividualCost"/> returns
    /// zero for the base value (at or below minimum).
    /// </summary>
    [Test]
    public void GetIndividualCost_AtMinimum_ReturnsZero()
    {
        // Arrange & Act
        var cost = _config.GetIndividualCost(1);

        // Assert
        cost.Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetCumulativeCost"/> returns
    /// the correct total cost from base to the target value.
    /// </summary>
    /// <param name="targetValue">The target attribute value.</param>
    /// <param name="expectedCumulative">The expected cumulative cost from base.</param>
    [Test]
    [TestCase(1, 0)]
    [TestCase(2, 1)]
    [TestCase(5, 4)]
    [TestCase(8, 7)]
    [TestCase(9, 9)]
    [TestCase(10, 11)]
    public void GetCumulativeCost_ReturnsCorrectCost(int targetValue, int expectedCumulative)
    {
        // Arrange & Act
        var cost = _config.GetCumulativeCost(targetValue);

        // Assert
        cost.Should().Be(expectedCumulative);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ARCHETYPE STARTING POINTS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetStartingPointsForArchetype"/>
    /// returns 14 for the Adept archetype (case-insensitive).
    /// </summary>
    [Test]
    [TestCase("adept")]
    [TestCase("Adept")]
    [TestCase("ADEPT")]
    public void GetStartingPointsForArchetype_Adept_Returns14(string archetypeId)
    {
        // Arrange & Act
        var points = _config.GetStartingPointsForArchetype(archetypeId);

        // Assert
        points.Should().Be(14);
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetStartingPointsForArchetype"/>
    /// returns 15 for non-Adept archetypes.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier.</param>
    [Test]
    [TestCase("warrior")]
    [TestCase("skirmisher")]
    [TestCase("mystic")]
    public void GetStartingPointsForArchetype_NonAdept_Returns15(string archetypeId)
    {
        // Arrange & Act
        var points = _config.GetStartingPointsForArchetype(archetypeId);

        // Assert
        points.Should().Be(15);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AFFORDABILITY TESTS — CanAfford
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.CanAfford"/> returns true
    /// when sufficient points are available for the desired increase.
    /// </summary>
    [Test]
    public void CanAfford_WithSufficientPoints_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _config.CanAfford(1, 5, 10).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.CanAfford"/> returns false
    /// when insufficient points are available for the desired increase.
    /// </summary>
    [Test]
    public void CanAfford_WithInsufficientPoints_ReturnsFalse()
    {
        // Arrange & Act & Assert — Cost to reach 10 from 1 is 11, but only 5 available
        _config.CanAfford(1, 10, 5).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.CanAfford"/> returns false
    /// when the target value exceeds the maximum attribute value.
    /// </summary>
    [Test]
    public void CanAfford_TargetAboveMax_ReturnsFalse()
    {
        // Arrange & Act & Assert
        _config.CanAfford(1, 11, 15).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.CanAfford"/> returns false
    /// when the target value is below the minimum attribute value.
    /// </summary>
    [Test]
    public void CanAfford_TargetBelowMin_ReturnsFalse()
    {
        // Arrange & Act & Assert
        _config.CanAfford(5, 0, 10).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that decreases are always affordable (they refund points).
    /// </summary>
    [Test]
    public void CanAfford_Decrease_ReturnsTrue()
    {
        // Arrange & Act & Assert — Decreasing is always affordable (refund)
        _config.CanAfford(5, 3, 0).Should().BeTrue();
    }

    /// <summary>
    /// Verifies exact boundary: can afford exactly enough points.
    /// </summary>
    [Test]
    public void CanAfford_ExactlyEnoughPoints_ReturnsTrue()
    {
        // Arrange & Act & Assert — Cost from 8 to 9 is exactly 2
        _config.CanAfford(8, 9, 2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies boundary: one point short of affordability.
    /// </summary>
    [Test]
    public void CanAfford_OnePointShort_ReturnsFalse()
    {
        // Arrange & Act & Assert — Cost from 8 to 9 is 2, but only 1 available
        _config.CanAfford(8, 9, 1).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MAX REACHABLE VALUE TESTS — GetMaxReachableValue
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.GetMaxReachableValue"/>
    /// returns 10 with a full 15-point pool from base.
    /// </summary>
    [Test]
    public void GetMaxReachableValue_FullPool_ReturnsMaximum()
    {
        // Arrange & Act
        var maxValue = _config.GetMaxReachableValue(1, 15);

        // Assert — 15 points, costs 11 to reach 10 from 1
        maxValue.Should().Be(10);
    }

    /// <summary>
    /// Verifies that 7 points from base reaches exactly 8 (end of standard tier).
    /// </summary>
    [Test]
    public void GetMaxReachableValue_StandardTierOnly_Returns8()
    {
        // Arrange & Act
        var maxValue = _config.GetMaxReachableValue(1, 7);

        // Assert — 7 points reaches value 8 exactly
        maxValue.Should().Be(8);
    }

    /// <summary>
    /// Verifies that zero remaining points returns the current value (no increase possible).
    /// </summary>
    [Test]
    public void GetMaxReachableValue_NoPoints_ReturnsCurrentValue()
    {
        // Arrange & Act
        var maxValue = _config.GetMaxReachableValue(5, 0);

        // Assert
        maxValue.Should().Be(5);
    }

    /// <summary>
    /// Verifies that 3 points from value 8 can reach 9 (one premium step) but not 10.
    /// </summary>
    [Test]
    public void GetMaxReachableValue_LimitedPremiumBudget_ReturnsCorrectMax()
    {
        // Arrange & Act — 3 points from 8: value 9 costs 2, value 10 costs 2 more (total 4)
        var maxValue = _config.GetMaxReachableValue(8, 3);

        // Assert
        maxValue.Should().Be(9);
    }

    /// <summary>
    /// Verifies that 1 point from value 8 cannot afford any premium increment.
    /// </summary>
    [Test]
    public void GetMaxReachableValue_InsufficientForPremium_StaysAtCurrent()
    {
        // Arrange & Act — 1 point from 8: premium costs 2, cannot afford
        var maxValue = _config.GetMaxReachableValue(8, 1);

        // Assert
        maxValue.Should().Be(8);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyConfiguration.ToString"/> returns the
    /// expected formatted string with starting points, entry count, and range.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange & Act
        var result = _config.ToString();

        // Assert
        result.Should().Be("PointBuy: 15 pts (9 entries, range 1-10)");
    }
}
