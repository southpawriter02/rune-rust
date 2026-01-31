namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StressApplicationResult"/> value object.
/// Tests cover factory methods, threshold crossing detection, trauma check triggers,
/// stress gained calculation, resistance integration, defense penalty changes,
/// source tracking, clamping behavior, and ToString formatting.
/// </summary>
[TestFixture]
public class StressApplicationResultTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create (Basic Value Storage)
    // -------------------------------------------------------------------------

    [Test]
    public void Create_StoresValuesCorrectly()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 55,
            source: StressSource.Combat);

        // Assert
        result.PreviousStress.Should().Be(30);
        result.NewStress.Should().Be(55);
        result.StressGained.Should().Be(25);
        result.Source.Should().Be(StressSource.Combat);
    }

    [Test]
    public void Create_WithResistanceResult_StoresResistanceResult()
    {
        // Arrange
        var resistance = StressCheckResult.Create(successes: 1, baseStress: 20);

        // Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 40,
            source: StressSource.Combat,
            resistanceResult: resistance);

        // Assert
        result.ResistanceResult.Should().NotBeNull();
        result.ResistanceResult!.Value.Successes.Should().Be(1);
    }

    [Test]
    public void Create_WithoutResistanceResult_ResistanceResultIsNull()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 55,
            source: StressSource.Combat);

        // Assert
        result.ResistanceResult.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Create (Clamping)
    // -------------------------------------------------------------------------

    [Test]
    public void Create_ClampsNewStressToMax()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 90,
            newStress: 150,
            source: StressSource.Combat);

        // Assert
        result.NewStress.Should().Be(100, "stress should be clamped to MaxStress (100)");
        result.StressGained.Should().Be(10, "stress gained should be based on clamped value");
    }

    [Test]
    public void Create_ClampsNewStressToMin()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 10,
            newStress: -5,
            source: StressSource.Combat);

        // Assert
        result.NewStress.Should().Be(0, "stress should be clamped to MinStress (0)");
    }

    // -------------------------------------------------------------------------
    // Computed Properties — ThresholdCrossed
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(35, 65, true)]   // Uneasy → Panicked
    [TestCase(35, 38, false)]  // Uneasy → Uneasy (same tier)
    [TestCase(0, 15, false)]   // Calm → Calm
    [TestCase(75, 100, true)]  // Panicked → Trauma
    public void Create_DetectsThresholdCrossedCorrectly(
        int previous, int newStress, bool expectedCrossed)
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previous, newStress, StressSource.Combat);

        // Assert
        result.ThresholdCrossed.Should().Be(expectedCrossed);
    }

    [Test]
    [TestCase(0, 20, true)]    // Calm → Uneasy (first boundary)
    [TestCase(19, 20, true)]   // Calm → Uneasy (exact boundary)
    [TestCase(39, 40, true)]   // Uneasy → Anxious
    [TestCase(59, 60, true)]   // Anxious → Panicked
    [TestCase(79, 80, true)]   // Panicked → Breaking
    [TestCase(99, 100, true)]  // Breaking → Trauma
    public void ThresholdCrossed_AllBoundaries(
        int previous, int newStress, bool expectedCrossed)
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previous, newStress, StressSource.Combat);

        // Assert
        result.ThresholdCrossed.Should().Be(expectedCrossed);
    }

    [Test]
    [TestCase(21, 35)]   // Uneasy → Uneasy
    [TestCase(41, 58)]   // Anxious → Anxious
    [TestCase(61, 78)]   // Panicked → Panicked
    [TestCase(81, 98)]   // Breaking → Breaking
    public void ThresholdCrossed_SameThreshold_False(int previous, int newStress)
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previous, newStress, StressSource.Combat);

        // Assert
        result.ThresholdCrossed.Should().Be(false);
    }

    [Test]
    public void ThresholdCrossed_MultipleThresholds_True()
    {
        // Arrange & Act — jumping from Calm to Breaking (multiple tiers)
        var result = StressApplicationResult.Create(
            previousStress: 5,
            newStress: 85,
            source: StressSource.Combat);

        // Assert
        result.ThresholdCrossed.Should().Be(true);
        result.PreviousThreshold.Should().Be(StressThreshold.Calm);
        result.NewThreshold.Should().Be(StressThreshold.Breaking);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — TraumaCheckTriggered
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaCheckTriggered_TrueAt100()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 75,
            newStress: 100,
            source: StressSource.Combat);

        // Assert
        result.TraumaCheckTriggered.Should().Be(true);
    }

    [Test]
    public void TraumaCheckTriggered_TrueAbove100_ClampedTo100()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 90,
            newStress: 120,
            source: StressSource.Combat);

        // Assert
        result.TraumaCheckTriggered.Should().Be(true);
        result.NewStress.Should().Be(100, "should be clamped to 100");
    }

    [Test]
    public void TraumaCheckTriggered_FalseAt99()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 75,
            newStress: 99,
            source: StressSource.Combat);

        // Assert
        result.TraumaCheckTriggered.Should().Be(false);
    }

    [Test]
    public void TraumaCheckTriggered_FalseAtZero()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 0,
            newStress: 0,
            source: StressSource.Combat);

        // Assert
        result.TraumaCheckTriggered.Should().Be(false);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — StressGained
    // -------------------------------------------------------------------------

    [Test]
    public void StressGained_CalculatedCorrectly()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 55,
            source: StressSource.Combat);

        // Assert
        result.StressGained.Should().Be(25);
    }

    [Test]
    public void StressGained_WithClamping_ReflectsClampedValue()
    {
        // Arrange & Act — Previous 90, raw new 120, clamped to 100
        var result = StressApplicationResult.Create(
            previousStress: 90,
            newStress: 120,
            source: StressSource.Combat);

        // Assert
        result.StressGained.Should().Be(10, "should be 100 (clamped) - 90 = 10");
    }

    [Test]
    public void StressGained_ZeroWhenNoChange()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 50,
            newStress: 50,
            source: StressSource.Combat);

        // Assert
        result.StressGained.Should().Be(0);
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
        var result = StressApplicationResult.Create(
            previousStress, newStress: previousStress, source: StressSource.Combat);

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
        var result = StressApplicationResult.Create(
            previousStress: 0, newStress, source: StressSource.Combat);

        // Assert
        result.NewThreshold.Should().Be(expectedThreshold);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — WasResisted
    // -------------------------------------------------------------------------

    [Test]
    public void WasResisted_TrueWhenResistanceSucceeded()
    {
        // Arrange
        var resistance = StressCheckResult.Create(successes: 1, baseStress: 20);

        // Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 40,
            source: StressSource.Combat,
            resistanceResult: resistance);

        // Assert
        result.WasResisted.Should().Be(true);
    }

    [Test]
    public void WasResisted_FalseWhenNoResistanceResult()
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 55,
            source: StressSource.Combat);

        // Assert
        result.WasResisted.Should().Be(false);
    }

    [Test]
    public void WasResisted_FalseWhenResistanceFailed()
    {
        // Arrange
        var resistance = StressCheckResult.Create(successes: 0, baseStress: 20);

        // Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 50,
            source: StressSource.Combat,
            resistanceResult: resistance);

        // Assert
        result.WasResisted.Should().Be(false);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — DefensePenaltyChange
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 20, 1)]    // floor(20/20) - floor(0/20) = 1 - 0 = 1
    [TestCase(0, 40, 2)]    // floor(40/20) - floor(0/20) = 2 - 0 = 2
    [TestCase(40, 60, 1)]   // floor(60/20) - floor(40/20) = 3 - 2 = 1
    [TestCase(0, 0, 0)]     // No change
    [TestCase(35, 38, 0)]   // Same tier — floor(38/20)=1, floor(35/20)=1
    [TestCase(0, 100, 5)]   // Calm → Trauma — maximum penalty change
    public void DefensePenaltyChange_CalculatedCorrectly(
        int previous, int newStress, int expectedChange)
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previous, newStress, StressSource.Combat);

        // Assert
        result.DefensePenaltyChange.Should().Be(expectedChange);
    }

    // -------------------------------------------------------------------------
    // Source — All Enum Values
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(StressSource.Combat)]
    [TestCase(StressSource.Exploration)]
    [TestCase(StressSource.Narrative)]
    [TestCase(StressSource.Heretical)]
    [TestCase(StressSource.Environmental)]
    [TestCase(StressSource.Corruption)]
    public void Source_StoredCorrectly(StressSource source)
    {
        // Arrange & Act
        var result = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 55,
            source: source);

        // Assert
        result.Source.Should().Be(source);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_FormatsCorrectly_NoThresholdCross()
    {
        // Arrange
        var result = StressApplicationResult.Create(
            previousStress: 25,
            newStress: 35,
            source: StressSource.Combat);

        // Act
        var display = result.ToString();

        // Assert — same threshold (Uneasy → Uneasy), no CROSSED, no TRAUMA!
        display.Should().Contain("25 → 35");
        display.Should().Contain("[Combat]");
        display.Should().Contain("Uneasy → Uneasy");
        display.Should().NotContain("CROSSED");
        display.Should().NotContain("TRAUMA!");
    }

    [Test]
    public void ToString_FormatsCorrectly_WithThresholdCross()
    {
        // Arrange
        var result = StressApplicationResult.Create(
            previousStress: 35,
            newStress: 65,
            source: StressSource.Exploration);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("35 → 65");
        display.Should().Contain("[Exploration]");
        display.Should().Contain("Uneasy → Panicked");
        display.Should().Contain("CROSSED");
        display.Should().NotContain("TRAUMA!");
    }

    [Test]
    public void ToString_FormatsCorrectly_WithTraumaTrigger()
    {
        // Arrange
        var result = StressApplicationResult.Create(
            previousStress: 85,
            newStress: 100,
            source: StressSource.Heretical);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("85 → 100");
        display.Should().Contain("[Heretical]");
        display.Should().Contain("Breaking → Trauma");
        display.Should().Contain("CROSSED");
        display.Should().Contain("TRAUMA!");
    }

    [Test]
    public void ToString_FormatsCorrectly_TraumaWithoutCrossing()
    {
        // Arrange — already at Trauma threshold (100 → 100 with no gain due to clamping)
        var result = StressApplicationResult.Create(
            previousStress: 100,
            newStress: 100,
            source: StressSource.Combat);

        // Act
        var display = result.ToString();

        // Assert — Trauma → Trauma, no CROSSED but still TRAUMA!
        display.Should().Contain("Trauma → Trauma");
        display.Should().NotContain("CROSSED");
        display.Should().Contain("TRAUMA!");
    }
}
