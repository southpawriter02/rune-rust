namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StressRecoveryResult"/> value object.
/// Tests cover factory methods, threshold dropping detection, amount recovered calculation,
/// clamping behavior, arrow-expression properties, recovery source tracking, and ToString formatting.
/// </summary>
[TestFixture]
public class StressRecoveryResultTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create (Basic Value Storage)
    // -------------------------------------------------------------------------

    [Test]
    public void Create_StoresValuesCorrectly()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 75,
            newStress: 55,
            recoverySource: RestType.Long);

        // Assert
        result.PreviousStress.Should().Be(75);
        result.NewStress.Should().Be(55);
        result.AmountRecovered.Should().Be(20);
        result.RecoverySource.Should().Be(RestType.Long);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Create (Clamping)
    // -------------------------------------------------------------------------

    [Test]
    public void Create_ClampsNewStressToMin()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 10,
            newStress: -5,
            recoverySource: RestType.Sanctuary);

        // Assert
        result.NewStress.Should().Be(0, "stress should be clamped to MinStress (0)");
        result.AmountRecovered.Should().Be(10, "amount recovered should use clamped value");
    }

    [Test]
    public void Create_ZeroNewStress_NotClamped()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 0,
            recoverySource: RestType.Sanctuary);

        // Assert
        result.NewStress.Should().Be(0);
        result.AmountRecovered.Should().Be(50);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — ThresholdDropped
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(85, 35, true)]   // Breaking → Uneasy
    [TestCase(85, 82, false)]  // Breaking → Breaking (same tier)
    [TestCase(50, 0, true)]    // Anxious → Calm
    public void Create_DetectsThresholdDroppedCorrectly(
        int previous, int newStress, bool expectedDropped)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Long);

        // Assert
        result.ThresholdDropped.Should().Be(expectedDropped);
        result.AmountRecovered.Should().Be(previous - newStress);
    }

    [Test]
    [TestCase(100, 99, true)]  // Trauma → Breaking
    [TestCase(80, 79, true)]   // Breaking → Panicked
    [TestCase(60, 59, true)]   // Panicked → Anxious
    [TestCase(40, 39, true)]   // Anxious → Uneasy
    [TestCase(20, 19, true)]   // Uneasy → Calm
    public void ThresholdDropped_AllDownwardBoundaries(int previous, int newStress, bool expected)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Long);

        // Assert
        result.ThresholdDropped.Should().Be(expected);
    }

    [Test]
    [TestCase(25, 22)]   // Uneasy → Uneasy
    [TestCase(45, 42)]   // Anxious → Anxious
    [TestCase(65, 62)]   // Panicked → Panicked
    [TestCase(85, 82)]   // Breaking → Breaking
    public void ThresholdDropped_SameThreshold_False(int previous, int newStress)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Short);

        // Assert
        result.ThresholdDropped.Should().Be(false);
    }

    [Test]
    public void ThresholdDropped_MultipleThresholdsDrop()
    {
        // Arrange & Act — Breaking → Calm (multiple tier drop)
        var result = StressRecoveryResult.Create(
            previousStress: 85,
            newStress: 15,
            recoverySource: RestType.Sanctuary);

        // Assert
        result.ThresholdDropped.Should().Be(true);
        result.PreviousThreshold.Should().Be(StressThreshold.Breaking);
        result.NewThreshold.Should().Be(StressThreshold.Calm);
    }

    [Test]
    public void ThresholdDropped_NoChange_False()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 50,
            recoverySource: RestType.Short);

        // Assert
        result.ThresholdDropped.Should().Be(false);
        result.AmountRecovered.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — AmountRecovered
    // -------------------------------------------------------------------------

    [Test]
    public void AmountRecovered_CalculatedCorrectly()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 75,
            newStress: 55,
            recoverySource: RestType.Long);

        // Assert
        result.AmountRecovered.Should().Be(20);
    }

    [Test]
    public void AmountRecovered_WithClamping_ReflectsClampedValue()
    {
        // Arrange & Act — Previous 10, raw new -20, clamped to 0
        var result = StressRecoveryResult.Create(
            previousStress: 10,
            newStress: -20,
            recoverySource: RestType.Sanctuary);

        // Assert
        result.AmountRecovered.Should().Be(10, "should be 10 - 0 (clamped) = 10");
    }

    [Test]
    public void AmountRecovered_FullRecovery()
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 85,
            newStress: 0,
            recoverySource: RestType.Sanctuary);

        // Assert
        result.AmountRecovered.Should().Be(85);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — Thresholds
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, StressThreshold.Calm)]
    [TestCase(19, StressThreshold.Calm)]
    [TestCase(20, StressThreshold.Uneasy)]
    [TestCase(40, StressThreshold.Anxious)]
    [TestCase(60, StressThreshold.Panicked)]
    [TestCase(80, StressThreshold.Breaking)]
    [TestCase(100, StressThreshold.Trauma)]
    public void PreviousThreshold_DerivedFromPreviousStress(
        int previousStress, StressThreshold expectedThreshold)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress, newStress: previousStress, recoverySource: RestType.Short);

        // Assert
        result.PreviousThreshold.Should().Be(expectedThreshold);
    }

    [Test]
    [TestCase(0, StressThreshold.Calm)]
    [TestCase(19, StressThreshold.Calm)]
    [TestCase(20, StressThreshold.Uneasy)]
    [TestCase(40, StressThreshold.Anxious)]
    [TestCase(60, StressThreshold.Panicked)]
    [TestCase(80, StressThreshold.Breaking)]
    [TestCase(100, StressThreshold.Trauma)]
    public void NewThreshold_DerivedFromNewStress(
        int newStress, StressThreshold expectedThreshold)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 100, newStress, recoverySource: RestType.Sanctuary);

        // Assert
        result.NewThreshold.Should().Be(expectedThreshold);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsFullyRecovered
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(50, 0, true)]
    [TestCase(50, 1, false)]
    [TestCase(10, 0, true)]
    [TestCase(0, 0, true)]
    public void IsFullyRecovered_TrueOnlyAtZero(int previous, int newStress, bool expected)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Sanctuary);

        // Assert
        result.IsFullyRecovered.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsNowCalm
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(50, 0, true)]    // Calm threshold (stress 0)
    [TestCase(50, 19, true)]   // Calm threshold (stress 19)
    [TestCase(50, 20, false)]  // Uneasy threshold (stress 20)
    [TestCase(80, 40, false)]  // Anxious threshold (stress 40)
    public void IsNowCalm_TrueWhenNewThresholdIsCalm(int previous, int newStress, bool expected)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Long);

        // Assert
        result.IsNowCalm.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — DefensePenaltyImprovement
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(60, 20, 2)]   // floor(60/20) - floor(20/20) = 3 - 1 = 2
    [TestCase(80, 0, 4)]    // floor(80/20) - floor(0/20) = 4 - 0 = 4
    [TestCase(40, 40, 0)]   // No change
    [TestCase(100, 0, 5)]   // Maximum improvement: floor(100/20) - floor(0/20) = 5 - 0 = 5
    [TestCase(25, 22, 0)]   // Same tier — floor(25/20)=1, floor(22/20)=1
    public void DefensePenaltyImprovement_CalculatedCorrectly(
        int previous, int newStress, int expectedImprovement)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previous, newStress, RestType.Long);

        // Assert
        result.DefensePenaltyImprovement.Should().Be(expectedImprovement);
    }

    // -------------------------------------------------------------------------
    // RecoverySource — All Enum Values
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(RestType.Short)]
    [TestCase(RestType.Long)]
    [TestCase(RestType.Sanctuary)]
    [TestCase(RestType.Milestone)]
    public void RecoverySource_StoredCorrectly(RestType restType)
    {
        // Arrange & Act
        var result = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 30,
            recoverySource: restType);

        // Assert
        result.RecoverySource.Should().Be(restType);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly_NoThresholdDrop()
    {
        // Arrange
        var result = StressRecoveryResult.Create(
            previousStress: 55,
            newStress: 45,
            recoverySource: RestType.Short);

        // Act
        var display = result.ToString();

        // Assert — same threshold (Anxious → Anxious), no IMPROVED
        display.Should().Contain("55 → 45");
        display.Should().Contain("-10");
        display.Should().Contain("[Short]");
        display.Should().Contain("Anxious → Anxious");
        display.Should().NotContain("IMPROVED");
    }

    [Test]
    public void ToString_FormatsCorrectly_WithThresholdDrop()
    {
        // Arrange
        var result = StressRecoveryResult.Create(
            previousStress: 75,
            newStress: 55,
            recoverySource: RestType.Long);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("75 → 55");
        display.Should().Contain("-20");
        display.Should().Contain("[Long]");
        display.Should().Contain("Panicked → Anxious");
        display.Should().Contain("IMPROVED");
    }

    [Test]
    public void ToString_FormatsCorrectly_FullRecovery()
    {
        // Arrange
        var result = StressRecoveryResult.Create(
            previousStress: 85,
            newStress: 0,
            recoverySource: RestType.Sanctuary);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("85 → 0");
        display.Should().Contain("-85");
        display.Should().Contain("[Sanctuary]");
        display.Should().Contain("Breaking → Calm");
        display.Should().Contain("IMPROVED");
    }
}
