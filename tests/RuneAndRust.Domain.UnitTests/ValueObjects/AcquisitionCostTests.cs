// ═══════════════════════════════════════════════════════════════════════════════
// AcquisitionCostTests.cs
// Unit tests for the AcquisitionCost value object.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="AcquisitionCost"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify acquisition cost functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>Static factory methods (None, FromPP, FromTraining)</description></item>
///   <item><description>Derived properties (IsFree, SpentPP, SpentPS, SpentTime)</description></item>
///   <item><description>ToString formatting</description></item>
///   <item><description>Validation and error handling</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AcquisitionCostTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // None Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that None returns a zero-cost instance.
    /// </summary>
    [Test]
    public void None_ReturnsZeroCost()
    {
        // Arrange & Act
        var cost = AcquisitionCost.None;

        // Assert
        cost.ProgressionPoints.Should().Be(0);
        cost.PiecesSilver.Should().Be(0);
        cost.TrainingWeeks.Should().Be(0);
    }

    /// <summary>
    /// Verifies that None is free.
    /// </summary>
    [Test]
    public void None_IsFree_ReturnsTrue()
    {
        // Arrange & Act
        var cost = AcquisitionCost.None;

        // Assert
        cost.IsFree.Should().BeTrue();
        cost.SpentPP.Should().BeFalse();
        cost.SpentPS.Should().BeFalse();
        cost.SpentTime.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FromPP Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FromPP creates correct cost.
    /// </summary>
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(5)]
    public void FromPP_WithValidValue_CreatesCost(int pp)
    {
        // Arrange & Act
        var cost = AcquisitionCost.FromPP(pp);

        // Assert
        cost.ProgressionPoints.Should().Be(pp);
        cost.PiecesSilver.Should().Be(0);
        cost.TrainingWeeks.Should().Be(0);
    }

    /// <summary>
    /// Verifies that FromPP sets SpentPP to true.
    /// </summary>
    [Test]
    public void FromPP_SpentPP_ReturnsTrue()
    {
        // Arrange & Act
        var cost = AcquisitionCost.FromPP(2);

        // Assert
        cost.IsFree.Should().BeFalse();
        cost.SpentPP.Should().BeTrue();
        cost.SpentPS.Should().BeFalse();
        cost.SpentTime.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that FromPP throws on zero.
    /// </summary>
    [Test]
    public void FromPP_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => AcquisitionCost.FromPP(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pp");
    }

    /// <summary>
    /// Verifies that FromPP throws on negative.
    /// </summary>
    [Test]
    public void FromPP_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => AcquisitionCost.FromPP(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pp");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FromTraining Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FromTraining creates correct cost.
    /// </summary>
    [Test]
    [TestCase(50, 2)]
    [TestCase(150, 4)]
    [TestCase(400, 8)]
    public void FromTraining_WithValidValues_CreatesCost(int ps, int weeks)
    {
        // Arrange & Act
        var cost = AcquisitionCost.FromTraining(ps, weeks);

        // Assert
        cost.ProgressionPoints.Should().Be(0);
        cost.PiecesSilver.Should().Be(ps);
        cost.TrainingWeeks.Should().Be(weeks);
    }

    /// <summary>
    /// Verifies that FromTraining sets SpentPS and SpentTime to true.
    /// </summary>
    [Test]
    public void FromTraining_SpentPSAndTime_ReturnsTrue()
    {
        // Arrange & Act
        var cost = AcquisitionCost.FromTraining(50, 2);

        // Assert
        cost.IsFree.Should().BeFalse();
        cost.SpentPP.Should().BeFalse();
        cost.SpentPS.Should().BeTrue();
        cost.SpentTime.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that FromTraining throws on zero piecesSilver.
    /// </summary>
    [Test]
    public void FromTraining_WithZeroPiecesSilver_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => AcquisitionCost.FromTraining(0, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("piecesSilver");
    }

    /// <summary>
    /// Verifies that FromTraining throws on zero weeks.
    /// </summary>
    [Test]
    public void FromTraining_WithZeroWeeks_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => AcquisitionCost.FromTraining(50, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weeks");
    }

    /// <summary>
    /// Verifies that FromTraining throws on negative piecesSilver.
    /// </summary>
    [Test]
    public void FromTraining_WithNegativePiecesSilver_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => AcquisitionCost.FromTraining(-50, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("piecesSilver");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString for free cost returns "Free".
    /// </summary>
    [Test]
    public void ToString_ForFree_ReturnsFree()
    {
        // Arrange
        var cost = AcquisitionCost.None;

        // Act & Assert
        cost.ToString().Should().Be("Free");
    }

    /// <summary>
    /// Verifies that ToString for PP cost returns correct format.
    /// </summary>
    [Test]
    public void ToString_ForPP_ReturnsCorrectFormat()
    {
        // Arrange
        var cost = AcquisitionCost.FromPP(2);

        // Act & Assert
        cost.ToString().Should().Be("2 PP");
    }

    /// <summary>
    /// Verifies that ToString for training cost returns correct format.
    /// </summary>
    [Test]
    public void ToString_ForTraining_ReturnsCorrectFormat()
    {
        // Arrange
        var cost = AcquisitionCost.FromTraining(150, 4);

        // Act & Assert
        cost.ToString().Should().Be("150 PS, 4 weeks");
    }

    /// <summary>
    /// Verifies that ToString for single week uses singular.
    /// </summary>
    [Test]
    public void ToString_ForSingleWeek_UsesSingular()
    {
        // Arrange
        var cost = AcquisitionCost.FromTraining(50, 1);

        // Act & Assert
        cost.ToString().Should().Be("50 PS, 1 week");
    }
}
