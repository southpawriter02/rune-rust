namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CorruptionAddResult"/> value object.
/// Tests cover factory creation, computed properties (AmountGained, StageCrossed,
/// IsTerminalError, NowFactionLocked), and ToString formatting.
/// </summary>
[TestFixture]
public class CorruptionAddResultTests
{
    // -------------------------------------------------------------------------
    // Factory — Create (Basic Properties)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that Create stores all provided values correctly.
    /// </summary>
    [Test]
    public void Create_StoresAllPropertiesCorrectly()
    {
        // Arrange & Act
        var result = CorruptionAddResult.Create(
            previousCorruption: 20,
            newCorruption: 45,
            source: CorruptionSource.HereticalAbility,
            thresholdCrossed: 25,
            previousStage: CorruptionStage.Tainted,
            newStage: CorruptionStage.Infected);

        // Assert
        result.PreviousCorruption.Should().Be(20);
        result.NewCorruption.Should().Be(45);
        result.Source.Should().Be(CorruptionSource.HereticalAbility);
        result.ThresholdCrossed.Should().Be(25);
        result.PreviousStage.Should().Be(CorruptionStage.Tainted);
        result.NewStage.Should().Be(CorruptionStage.Infected);
    }

    // -------------------------------------------------------------------------
    // Computed — AmountGained
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that AmountGained is computed as NewCorruption - PreviousCorruption.
    /// </summary>
    [Test]
    [TestCase(0, 25, 25)]      // Gain 25
    [TestCase(20, 45, 25)]     // Gain 25 from 20
    [TestCase(95, 100, 5)]     // Clamped gain (only 5 effective)
    [TestCase(30, 20, -10)]    // Negative (corruption reduced)
    [TestCase(50, 50, 0)]      // No change
    public void AmountGained_ComputedCorrectly(
        int previousCorruption,
        int newCorruption,
        int expectedGained)
    {
        // Act
        var result = CorruptionAddResult.Create(
            previousCorruption, newCorruption,
            CorruptionSource.MysticMagic,
            null,
            CorruptionState.DetermineStage(previousCorruption),
            CorruptionState.DetermineStage(newCorruption));

        // Assert
        result.AmountGained.Should().Be(expectedGained);
    }

    // -------------------------------------------------------------------------
    // Computed — StageCrossed
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that StageCrossed detects stage transitions correctly via CorruptionTracker.
    /// </summary>
    [Test]
    [TestCase(35, 65, true)]        // Tainted -> Blighted
    [TestCase(35, 3, false)]        // Tainted -> Tainted (same stage, 35+3=38)
    [TestCase(0, 15, false)]        // Uncorrupted -> Uncorrupted
    [TestCase(95, 100, true)]       // Corrupted -> Consumed (Terminal Error)
    [TestCase(19, 20, true)]        // Uncorrupted -> Tainted (exact boundary)
    [TestCase(59, 60, true)]        // Infected -> Blighted (exact boundary)
    public void AddCorruption_DetectsStageCrossingCorrectly(
        int previousCorruption,
        int addAmount,
        bool expectedStageCrossed)
    {
        // Arrange
        var tracker = CorruptionTracker.Create(Guid.NewGuid());
        tracker.SetCorruption(previousCorruption);
        tracker.SetThresholdTriggers(true, true, true); // Don't interfere with threshold tests

        // Act
        var result = tracker.AddCorruption(addAmount, CorruptionSource.MysticMagic);

        // Assert
        result.StageCrossed.Should().Be(expectedStageCrossed);
    }

    // -------------------------------------------------------------------------
    // Computed — IsTerminalError
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that IsTerminalError is set when NewCorruption reaches 100.
    /// </summary>
    [Test]
    [TestCase(50, 80, false)]   // Not terminal
    [TestCase(50, 99, false)]   // Just under terminal
    [TestCase(50, 100, true)]   // Exactly terminal
    public void IsTerminalError_CorrectAtBoundary(
        int previousCorruption,
        int newCorruption,
        bool expectedTerminal)
    {
        // Act
        var result = CorruptionAddResult.Create(
            previousCorruption, newCorruption,
            CorruptionSource.ForlornContact,
            null,
            CorruptionState.DetermineStage(previousCorruption),
            CorruptionState.DetermineStage(newCorruption));

        // Assert
        result.IsTerminalError.Should().Be(expectedTerminal);
    }

    // -------------------------------------------------------------------------
    // Arrow — NowFactionLocked
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that NowFactionLocked detects the transition across 50 corruption.
    /// </summary>
    [Test]
    [TestCase(45, 55, true)]    // Crossed from below 50 to above 50
    [TestCase(49, 50, true)]    // Crossed from exactly below to exactly at
    [TestCase(50, 60, false)]   // Already above 50 — not "now" locked
    [TestCase(30, 45, false)]   // Still below 50
    [TestCase(0, 100, true)]    // Jumped from 0 to 100 — crosses 50
    public void NowFactionLocked_DetectsTransitionCorrectly(
        int previousCorruption,
        int newCorruption,
        bool expectedNowLocked)
    {
        // Act
        var result = CorruptionAddResult.Create(
            previousCorruption, newCorruption,
            CorruptionSource.MysticMagic,
            null,
            CorruptionState.DetermineStage(previousCorruption),
            CorruptionState.DetermineStage(newCorruption));

        // Assert
        result.NowFactionLocked.Should().Be(expectedNowLocked);
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies ToString includes corruption transition and source.
    /// </summary>
    [Test]
    public void ToString_BasicResult_IncludesTransitionAndSource()
    {
        // Arrange
        var result = CorruptionAddResult.Create(
            previousCorruption: 20,
            newCorruption: 35,
            source: CorruptionSource.MysticMagic,
            thresholdCrossed: null,
            previousStage: CorruptionStage.Tainted,
            newStage: CorruptionStage.Tainted);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("20 -> 35");
        display.Should().Contain("[MysticMagic]");
        display.Should().NotContain("Crossed");
        display.Should().NotContain("Stage:");
        display.Should().NotContain("TERMINAL ERROR");
    }

    /// <summary>
    /// Verifies ToString includes threshold crossing info when present.
    /// </summary>
    [Test]
    public void ToString_WithThresholdCrossing_IncludesCrossingInfo()
    {
        // Arrange
        var result = CorruptionAddResult.Create(
            previousCorruption: 20,
            newCorruption: 45,
            source: CorruptionSource.HereticalAbility,
            thresholdCrossed: 25,
            previousStage: CorruptionStage.Tainted,
            newStage: CorruptionStage.Infected);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("(Crossed 25%)");
        display.Should().Contain("Stage: Tainted -> Infected");
    }

    /// <summary>
    /// Verifies ToString includes Terminal Error indicator at corruption 100.
    /// </summary>
    [Test]
    public void ToString_AtTerminalError_IncludesTerminalFlag()
    {
        // Arrange
        var result = CorruptionAddResult.Create(
            previousCorruption: 95,
            newCorruption: 100,
            source: CorruptionSource.ForlornContact,
            thresholdCrossed: null,
            previousStage: CorruptionStage.Corrupted,
            newStage: CorruptionStage.Consumed);

        // Act
        var display = result.ToString();

        // Assert
        display.Should().Contain("[TERMINAL ERROR!]");
    }
}
