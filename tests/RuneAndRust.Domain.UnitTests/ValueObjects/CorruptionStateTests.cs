namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CorruptionState"/> value object.
/// Verifies factory methods, computed properties, arrow-expression properties,
/// static DetermineStage method, clamping behavior, and ToString formatting.
/// </summary>
[TestFixture]
public class CorruptionStateTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create
    // -------------------------------------------------------------------------

    [Test]
    public void Create_StoresValidValue()
    {
        // Act
        var state = CorruptionState.Create(45);

        // Assert
        state.CurrentCorruption.Should().Be(45);
    }

    [Test]
    public void Create_ClampsNegativeToZero()
    {
        // Act
        var state = CorruptionState.Create(-10);

        // Assert
        state.CurrentCorruption.Should().Be(0);
    }

    [Test]
    public void Create_ClampsAbove100()
    {
        // Act
        var state = CorruptionState.Create(150);

        // Assert
        state.CurrentCorruption.Should().Be(100);
    }

    [Test]
    [TestCase(0, CorruptionStage.Uncorrupted)]
    [TestCase(19, CorruptionStage.Uncorrupted)]
    [TestCase(20, CorruptionStage.Tainted)]
    [TestCase(39, CorruptionStage.Tainted)]
    [TestCase(40, CorruptionStage.Infected)]
    [TestCase(59, CorruptionStage.Infected)]
    [TestCase(60, CorruptionStage.Blighted)]
    [TestCase(79, CorruptionStage.Blighted)]
    [TestCase(80, CorruptionStage.Corrupted)]
    [TestCase(99, CorruptionStage.Corrupted)]
    [TestCase(100, CorruptionStage.Consumed)]
    public void Create_WithValidCorruption_ReturnsCorrectStage(
        int corruption,
        CorruptionStage expectedStage)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.CurrentCorruption.Should().Be(corruption);
        state.Stage.Should().Be(expectedStage);
    }

    [Test]
    [TestCase(-10, 0)]
    [TestCase(-1, 0)]
    [TestCase(101, 100)]
    [TestCase(150, 100)]
    public void Create_WithOutOfRangeCorruption_ClampsToValidRange(
        int inputCorruption,
        int expectedCorruption)
    {
        // Act
        var state = CorruptionState.Create(inputCorruption);

        // Assert
        state.CurrentCorruption.Should().Be(expectedCorruption);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Uncorrupted
    // -------------------------------------------------------------------------

    [Test]
    public void Uncorrupted_ReturnsZeroCorruptionState()
    {
        // Act
        var state = CorruptionState.Uncorrupted;

        // Assert
        state.CurrentCorruption.Should().Be(0);
        state.Stage.Should().Be(CorruptionStage.Uncorrupted);
        state.HasMutationRisk.Should().BeFalse();
        state.RequiresMutationCheck.Should().BeFalse();
        state.IsUncorrupted.Should().BeTrue();
        state.IsConsumed.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Computed Properties — PercentageToConsumption
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 0.0)]
    [TestCase(50, 0.5)]
    [TestCase(100, 1.0)]
    public void Create_CalculatesPercentageToConsumption(
        int corruption,
        double expectedPercentage)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.PercentageToConsumption.Should().BeApproximately(expectedPercentage, 0.001);
    }

    [Test]
    [TestCase(20, 0.2)]
    [TestCase(40, 0.4)]
    [TestCase(60, 0.6)]
    [TestCase(80, 0.8)]
    public void PercentageToConsumption_AtThresholdBoundaries(
        int corruption,
        double expectedPercentage)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.PercentageToConsumption.Should().BeApproximately(expectedPercentage, 0.001);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — HasMutationRisk
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(79, false)]
    [TestCase(80, true)]
    [TestCase(99, true)]
    [TestCase(100, true)]
    public void Create_CalculatesHasMutationRisk(
        int corruption,
        bool expectedMutationRisk)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.HasMutationRisk.Should().Be(expectedMutationRisk);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — RequiresMutationCheck
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(99, false)]
    [TestCase(100, true)]
    [TestCase(150, true)]  // Clamped to 100
    public void Create_CalculatesRequiresMutationCheck(
        int corruption,
        bool expectedRequiresMutationCheck)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.RequiresMutationCheck.Should().Be(expectedRequiresMutationCheck);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsUncorrupted
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, true)]
    [TestCase(19, true)]
    [TestCase(20, false)]
    [TestCase(50, false)]
    public void IsUncorrupted_TrueOnlyForUncorruptedStage(int corruption, bool expected)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.IsUncorrupted.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsConsumed
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, false)]
    [TestCase(50, false)]
    [TestCase(99, false)]
    [TestCase(100, true)]
    public void IsConsumed_TrueOnlyForConsumedStage(int corruption, bool expected)
    {
        // Act
        var state = CorruptionState.Create(corruption);

        // Assert
        state.IsConsumed.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // DetermineStage — Static Method
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, CorruptionStage.Uncorrupted)]
    [TestCase(19, CorruptionStage.Uncorrupted)]
    [TestCase(20, CorruptionStage.Tainted)]
    [TestCase(39, CorruptionStage.Tainted)]
    [TestCase(40, CorruptionStage.Infected)]
    [TestCase(59, CorruptionStage.Infected)]
    [TestCase(60, CorruptionStage.Blighted)]
    [TestCase(79, CorruptionStage.Blighted)]
    [TestCase(80, CorruptionStage.Corrupted)]
    [TestCase(99, CorruptionStage.Corrupted)]
    [TestCase(100, CorruptionStage.Consumed)]
    public void DetermineStage_ReturnsCorrectStage(
        int corruption,
        CorruptionStage expectedStage)
    {
        // Act
        var result = CorruptionState.DetermineStage(corruption);

        // Assert
        result.Should().Be(expectedStage);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_WithNormalCorruption_ReturnsFormattedString()
    {
        // Arrange
        var state = CorruptionState.Create(45);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("45/100");
        result.Should().Contain("Infected");
        result.Should().NotContain("MUTATION CHECK REQUIRED");
        result.Should().NotContain("Mutation Risk");
    }

    [Test]
    public void ToString_WithConsumed_IndicatesMutationCheckRequired()
    {
        // Arrange
        var state = CorruptionState.Create(100);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("MUTATION CHECK REQUIRED");
        result.Should().Contain("100/100");
        result.Should().Contain("Consumed");
    }

    [Test]
    public void ToString_WithMutationRisk_IndicatesRisk()
    {
        // Arrange
        var state = CorruptionState.Create(85);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("Mutation Risk");
        result.Should().NotContain("MUTATION CHECK REQUIRED");
        result.Should().Contain("Corrupted");
    }

    [Test]
    public void ToString_ZeroCorruption_FormatsCorrectly()
    {
        // Arrange
        var state = CorruptionState.Uncorrupted;

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("Corruption: 0/100 [Uncorrupted]");
    }
}
