using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the CorruptionState record.
/// Validates tier classification, penalty calculations, and terminal detection.
/// </summary>
public class CorruptionStateTests
{
    #region Tier Classification Tests

    [Theory]
    [InlineData(0, CorruptionTier.Pristine)]
    [InlineData(10, CorruptionTier.Pristine)]
    [InlineData(20, CorruptionTier.Pristine)]
    [InlineData(21, CorruptionTier.Tainted)]
    [InlineData(30, CorruptionTier.Tainted)]
    [InlineData(40, CorruptionTier.Tainted)]
    [InlineData(41, CorruptionTier.Corrupted)]
    [InlineData(50, CorruptionTier.Corrupted)]
    [InlineData(60, CorruptionTier.Corrupted)]
    [InlineData(61, CorruptionTier.Blighted)]
    [InlineData(70, CorruptionTier.Blighted)]
    [InlineData(80, CorruptionTier.Blighted)]
    [InlineData(81, CorruptionTier.Fractured)]
    [InlineData(90, CorruptionTier.Fractured)]
    [InlineData(99, CorruptionTier.Fractured)]
    [InlineData(100, CorruptionTier.Terminal)]
    public void Tier_ReturnsCorrectClassification(int corruptionValue, CorruptionTier expectedTier)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.Tier.Should().Be(expectedTier);
    }

    #endregion

    #region MaxApMultiplier Tests

    [Theory]
    [InlineData(0, 1.0)]      // Pristine: no penalty
    [InlineData(20, 1.0)]     // Pristine: no penalty
    [InlineData(21, 1.0)]     // Tainted: no penalty
    [InlineData(40, 1.0)]     // Tainted: no penalty
    [InlineData(41, 0.90)]    // Corrupted: -10% Max AP
    [InlineData(60, 0.90)]    // Corrupted: -10% Max AP
    [InlineData(61, 0.80)]    // Blighted: -20% Max AP
    [InlineData(80, 0.80)]    // Blighted: -20% Max AP
    [InlineData(81, 0.60)]    // Fractured: -40% Max AP
    [InlineData(99, 0.60)]    // Fractured: -40% Max AP
    [InlineData(100, 0.0)]    // Terminal: 0% (no AP)
    public void MaxApMultiplier_ReturnsCorrectValue(int corruptionValue, double expectedMultiplier)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.MaxApMultiplier.Should().Be(expectedMultiplier);
    }

    [Fact]
    public void MaxApPenaltyPercent_ReturnsCorrectPercentage()
    {
        // Pristine/Tainted: 0%
        new CorruptionState(0).MaxApPenaltyPercent.Should().Be(0);
        new CorruptionState(40).MaxApPenaltyPercent.Should().Be(0);

        // Corrupted: 10%
        new CorruptionState(41).MaxApPenaltyPercent.Should().Be(10);
        new CorruptionState(60).MaxApPenaltyPercent.Should().Be(10);

        // Blighted: 20%
        new CorruptionState(61).MaxApPenaltyPercent.Should().Be(20);
        new CorruptionState(80).MaxApPenaltyPercent.Should().Be(20);

        // Fractured: 40%
        new CorruptionState(81).MaxApPenaltyPercent.Should().Be(40);
        new CorruptionState(99).MaxApPenaltyPercent.Should().Be(40);

        // Terminal: 100%
        new CorruptionState(100).MaxApPenaltyPercent.Should().Be(100);
    }

    #endregion

    #region Attribute Penalty Tests

    [Theory]
    [InlineData(0, 0)]        // Pristine: no WILL penalty
    [InlineData(40, 0)]       // Tainted: no WILL penalty
    [InlineData(60, 0)]       // Corrupted: no WILL penalty
    [InlineData(61, 1)]       // Blighted: -1 WILL
    [InlineData(80, 1)]       // Blighted: -1 WILL
    [InlineData(81, 2)]       // Fractured: -2 WILL
    [InlineData(99, 2)]       // Fractured: -2 WILL
    [InlineData(100, 2)]      // Terminal: -2 WILL (same as Fractured)
    public void WillPenalty_ReturnsCorrectValue(int corruptionValue, int expectedPenalty)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.WillPenalty.Should().Be(expectedPenalty);
    }

    [Theory]
    [InlineData(0, 0)]        // Pristine: no WITS penalty
    [InlineData(40, 0)]       // Tainted: no WITS penalty
    [InlineData(60, 0)]       // Corrupted: no WITS penalty
    [InlineData(61, 0)]       // Blighted: no WITS penalty
    [InlineData(80, 0)]       // Blighted: no WITS penalty
    [InlineData(81, 1)]       // Fractured: -1 WITS
    [InlineData(99, 1)]       // Fractured: -1 WITS
    [InlineData(100, 1)]      // Terminal: -1 WITS (same as Fractured)
    public void WitsPenalty_ReturnsCorrectValue(int corruptionValue, int expectedPenalty)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.WitsPenalty.Should().Be(expectedPenalty);
    }

    #endregion

    #region Terminal Detection Tests

    [Theory]
    [InlineData(0, false)]
    [InlineData(50, false)]
    [InlineData(99, false)]
    [InlineData(100, true)]
    public void IsTerminal_DetectsTerminalState(int corruptionValue, bool expected)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.IsTerminal.Should().Be(expected);
    }

    #endregion

    #region Display Label Tests

    [Theory]
    [InlineData(0, "Pristine")]
    [InlineData(20, "Pristine")]
    [InlineData(21, "Tainted")]
    [InlineData(40, "Tainted")]
    [InlineData(41, "Corrupted")]
    [InlineData(60, "Corrupted")]
    [InlineData(61, "Blighted")]
    [InlineData(80, "Blighted")]
    [InlineData(81, "Fractured")]
    [InlineData(99, "Fractured")]
    [InlineData(100, "TERMINAL")]
    public void TierDisplayName_ReturnsCorrectLabel(int corruptionValue, string expectedLabel)
    {
        // Arrange
        var state = new CorruptionState(corruptionValue);

        // Assert
        state.TierDisplayName.Should().Be(expectedLabel);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CorruptionState_HandlesNegativeValue()
    {
        // Arrange - negative values should be treated as Pristine
        var state = new CorruptionState(-10);

        // Assert
        state.Tier.Should().Be(CorruptionTier.Pristine);
        state.MaxApMultiplier.Should().Be(1.0);
        state.WillPenalty.Should().Be(0);
        state.WitsPenalty.Should().Be(0);
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void CorruptionState_HandlesValueOver100()
    {
        // Arrange - values over 100 should be treated as Terminal
        var state = new CorruptionState(150);

        // Assert
        state.Tier.Should().Be(CorruptionTier.Terminal);
        state.MaxApMultiplier.Should().Be(0.0);
        state.IsTerminal.Should().BeTrue();
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void CorruptionState_BoundaryAt20()
    {
        // 20 is Pristine, 21 is Tainted
        new CorruptionState(20).Tier.Should().Be(CorruptionTier.Pristine);
        new CorruptionState(21).Tier.Should().Be(CorruptionTier.Tainted);
    }

    [Fact]
    public void CorruptionState_BoundaryAt40()
    {
        // 40 is Tainted, 41 is Corrupted
        new CorruptionState(40).Tier.Should().Be(CorruptionTier.Tainted);
        new CorruptionState(41).Tier.Should().Be(CorruptionTier.Corrupted);
    }

    [Fact]
    public void CorruptionState_BoundaryAt60()
    {
        // 60 is Corrupted, 61 is Blighted
        new CorruptionState(60).Tier.Should().Be(CorruptionTier.Corrupted);
        new CorruptionState(61).Tier.Should().Be(CorruptionTier.Blighted);
    }

    [Fact]
    public void CorruptionState_BoundaryAt80()
    {
        // 80 is Blighted, 81 is Fractured
        new CorruptionState(80).Tier.Should().Be(CorruptionTier.Blighted);
        new CorruptionState(81).Tier.Should().Be(CorruptionTier.Fractured);
    }

    [Fact]
    public void CorruptionState_BoundaryAt99()
    {
        // 99 is Fractured, 100 is Terminal
        new CorruptionState(99).Tier.Should().Be(CorruptionTier.Fractured);
        new CorruptionState(100).Tier.Should().Be(CorruptionTier.Terminal);
    }

    #endregion

    #region Combined Penalty Tests

    [Fact]
    public void CorruptionState_PristineHasNoPenalties()
    {
        // Arrange
        var state = new CorruptionState(15);

        // Assert
        state.MaxApMultiplier.Should().Be(1.0);
        state.WillPenalty.Should().Be(0);
        state.WitsPenalty.Should().Be(0);
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void CorruptionState_BlightedHasCorrectPenalties()
    {
        // Arrange - Blighted: -20% MaxAP, -1 WILL, no WITS
        var state = new CorruptionState(70);

        // Assert
        state.Tier.Should().Be(CorruptionTier.Blighted);
        state.MaxApMultiplier.Should().Be(0.80);
        state.WillPenalty.Should().Be(1);
        state.WitsPenalty.Should().Be(0);
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void CorruptionState_FracturedHasAllPenalties()
    {
        // Arrange - Fractured: -40% MaxAP, -2 WILL, -1 WITS
        var state = new CorruptionState(90);

        // Assert
        state.Tier.Should().Be(CorruptionTier.Fractured);
        state.MaxApMultiplier.Should().Be(0.60);
        state.WillPenalty.Should().Be(2);
        state.WitsPenalty.Should().Be(1);
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void CorruptionState_TerminalIsFullyDebilitating()
    {
        // Arrange - Terminal: 0 MaxAP, -2 WILL, -1 WITS, character lost
        var state = new CorruptionState(100);

        // Assert
        state.Tier.Should().Be(CorruptionTier.Terminal);
        state.MaxApMultiplier.Should().Be(0.0);
        state.WillPenalty.Should().Be(2);
        state.WitsPenalty.Should().Be(1);
        state.IsTerminal.Should().BeTrue();
        state.TierDisplayName.Should().Be("TERMINAL");
    }

    #endregion
}
