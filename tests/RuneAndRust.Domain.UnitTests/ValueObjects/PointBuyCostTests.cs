// ═══════════════════════════════════════════════════════════════════════════════
// PointBuyCostTests.cs
// Unit tests for the PointBuyCost value object verifying factory methods,
// computed properties, default cost table generation, and string formatting.
// Version: 0.17.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="PointBuyCost"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that PointBuyCost correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates a default cost table with 9 entries (values 2-10)</description></item>
///   <item><description>Classifies standard tier values (2-8) and premium tier values (9-10)</description></item>
///   <item><description>Validates target value range (2-10) and rejects out-of-range values</description></item>
///   <item><description>Validates non-negative costs</description></item>
///   <item><description>Produces correct ToString output for debugging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="PointBuyCost"/>
[TestFixture]
public class PointBuyCostTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEFAULT COST TABLE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.CreateDefaultCostTable"/>
    /// returns exactly 9 entries covering target values 2 through 10.
    /// </summary>
    [Test]
    public void CreateDefaultCostTable_ReturnsNineEntries()
    {
        // Arrange & Act
        var table = PointBuyCost.CreateDefaultCostTable();

        // Assert
        table.Should().HaveCount(9);
    }

    /// <summary>
    /// Verifies that the default cost table covers all target values from 2 to 10.
    /// </summary>
    [Test]
    public void CreateDefaultCostTable_CoversAllTargetValues()
    {
        // Arrange & Act
        var table = PointBuyCost.CreateDefaultCostTable();
        var targetValues = table.Select(c => c.TargetValue).ToList();

        // Assert
        targetValues.Should().BeEquivalentTo(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Verifies that the default cost table has standard tier entries (1 point each)
    /// for values 2-8 and premium tier entries (2 points each) for values 9-10.
    /// </summary>
    [Test]
    public void CreateDefaultCostTable_HasCorrectIndividualCosts()
    {
        // Arrange & Act
        var table = PointBuyCost.CreateDefaultCostTable();

        // Assert — Standard tier (values 2-8): 1 point each
        for (int i = 0; i < 7; i++)
        {
            table[i].IndividualCost.Should().Be(1,
                $"value {table[i].TargetValue} should cost 1 point (standard tier)");
        }

        // Assert — Premium tier (values 9-10): 2 points each
        table[7].IndividualCost.Should().Be(2, "value 9 should cost 2 points (premium tier)");
        table[8].IndividualCost.Should().Be(2, "value 10 should cost 2 points (premium tier)");
    }

    /// <summary>
    /// Verifies that the default cost table has correct cumulative costs.
    /// </summary>
    [Test]
    public void CreateDefaultCostTable_HasCorrectCumulativeCosts()
    {
        // Arrange & Act
        var table = PointBuyCost.CreateDefaultCostTable();

        // Assert — Cumulative costs from base (1)
        table[0].CumulativeCost.Should().Be(1, "value 2 cumulative should be 1");
        table[4].CumulativeCost.Should().Be(5, "value 6 cumulative should be 5 (index 4 = target value 6)");
        table[6].CumulativeCost.Should().Be(7, "value 8 cumulative should be 7 (end of standard)");
        table[7].CumulativeCost.Should().Be(9, "value 9 cumulative should be 9 (7 + 2)");
        table[8].CumulativeCost.Should().Be(11, "value 10 cumulative should be 11 (9 + 2)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIER CLASSIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.IsPremiumTier"/> returns the correct
    /// classification for standard and premium tier values.
    /// </summary>
    /// <param name="targetValue">The target attribute value to classify.</param>
    /// <param name="expected">Whether the value should be classified as premium tier.</param>
    [Test]
    [TestCase(2, false)]
    [TestCase(5, false)]
    [TestCase(8, false)]
    [TestCase(9, true)]
    [TestCase(10, true)]
    public void IsPremiumTier_ReturnsCorrectClassification(int targetValue, bool expected)
    {
        // Arrange
        var cost = new PointBuyCost(targetValue, targetValue <= 8 ? 1 : 2, 0);

        // Act & Assert
        cost.IsPremiumTier.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.IsStandardTier"/> returns the correct
    /// classification for standard and premium tier values.
    /// </summary>
    /// <param name="targetValue">The target attribute value to classify.</param>
    /// <param name="expected">Whether the value should be classified as standard tier.</param>
    [Test]
    [TestCase(2, true)]
    [TestCase(5, true)]
    [TestCase(8, true)]
    [TestCase(9, false)]
    [TestCase(10, false)]
    public void IsStandardTier_ReturnsCorrectClassification(int targetValue, bool expected)
    {
        // Arrange
        var cost = new PointBuyCost(targetValue, targetValue <= 8 ? 1 : 2, 0);

        // Act & Assert
        cost.IsStandardTier.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Create
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> creates a valid cost entry
    /// with correct properties.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesPointBuyCost()
    {
        // Arrange & Act
        var cost = PointBuyCost.Create(5, 1, 4);

        // Assert
        cost.TargetValue.Should().Be(5);
        cost.IndividualCost.Should().Be(1);
        cost.CumulativeCost.Should().Be(4);
        cost.IsStandardTier.Should().BeTrue();
        cost.IsPremiumTier.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> creates a valid premium tier
    /// cost entry.
    /// </summary>
    [Test]
    public void Create_WithPremiumTierValue_CreatesPremiumCost()
    {
        // Arrange & Act
        var cost = PointBuyCost.Create(9, 2, 9);

        // Assert
        cost.TargetValue.Should().Be(9);
        cost.IndividualCost.Should().Be(2);
        cost.CumulativeCost.Should().Be(9);
        cost.IsPremiumTier.Should().BeTrue();
        cost.IsStandardTier.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the target value is less than 2.
    /// </summary>
    [Test]
    public void Create_WithTargetValueBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PointBuyCost.Create(1, 0, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the target value exceeds 10.
    /// </summary>
    [Test]
    public void Create_WithTargetValueAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PointBuyCost.Create(11, 2, 13);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the individual cost is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeIndividualCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PointBuyCost.Create(5, -1, 4);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when the cumulative cost is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeCumulativeCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PointBuyCost.Create(5, 1, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.ToString"/> returns the expected
    /// formatted string with target value, individual cost, and cumulative cost.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var cost = new PointBuyCost(9, 2, 9);

        // Act
        var result = cost.ToString();

        // Assert
        result.Should().Be("Value 9: 2 pts (cumulative: 9)");
    }

    /// <summary>
    /// Verifies that <see cref="PointBuyCost.ToString"/> works correctly for
    /// a standard tier entry.
    /// </summary>
    [Test]
    public void ToString_StandardTierEntry_ReturnsFormattedString()
    {
        // Arrange
        var cost = new PointBuyCost(5, 1, 4);

        // Act
        var result = cost.ToString();

        // Assert
        result.Should().Be("Value 5: 1 pts (cumulative: 4)");
    }
}
