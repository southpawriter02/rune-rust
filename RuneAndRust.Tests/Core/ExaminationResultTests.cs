using FluentAssertions;
using RuneAndRust.Core.Models;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the ExaminationResult record.
/// Validates WITS-based examination results and computed properties.
/// </summary>
public class ExaminationResultTests
{
    #region Basic Record Behavior

    [Fact]
    public void ExaminationResult_CanBeCreated_WithRequiredParameters()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 2,
            TierRevealed: 1,
            Description: "Test description",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 3, 10 }
        );

        // Assert
        result.Success.Should().BeTrue();
        result.NetSuccesses.Should().Be(2);
        result.TierRevealed.Should().Be(1);
        result.Description.Should().Be("Test description");
        result.NewInfoRevealed.Should().BeTrue();
        result.Rolls.Should().HaveCount(3);
    }

    [Fact]
    public void ExaminationResult_Equality_WorksAsExpected()
    {
        // Arrange
        var rolls = new[] { 8, 3, 10 };
        var result1 = new ExaminationResult(true, 2, 1, "Test", true, rolls);
        var result2 = new ExaminationResult(true, 2, 1, "Test", true, rolls);

        // Assert
        result1.Should().Be(result2);
    }

    #endregion

    #region IsFumble Property

    [Fact]
    public void ExaminationResult_IsFumble_ReturnsTrueForNegativeNetSuccesses()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: false,
            NetSuccesses: -1,
            TierRevealed: 0,
            Description: "You fumbled",
            NewInfoRevealed: false,
            Rolls: new[] { 1, 1, 5 }
        );

        // Assert
        result.IsFumble.Should().BeTrue();
    }

    [Fact]
    public void ExaminationResult_IsFumble_ReturnsFalseForZeroNetSuccesses()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: false,
            NetSuccesses: 0,
            TierRevealed: 0,
            Description: "Base only",
            NewInfoRevealed: false,
            Rolls: new[] { 3, 5, 7 }
        );

        // Assert
        result.IsFumble.Should().BeFalse();
    }

    [Fact]
    public void ExaminationResult_IsFumble_ReturnsFalseForPositiveNetSuccesses()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 2,
            TierRevealed: 1,
            Description: "Detailed",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 9, 3 }
        );

        // Assert
        result.IsFumble.Should().BeFalse();
    }

    #endregion

    #region RevealedExpert Property

    [Fact]
    public void ExaminationResult_RevealedExpert_ReturnsTrueForTier2()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 4,
            TierRevealed: 2,
            Description: "Expert info",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 9, 10, 8 }
        );

        // Assert
        result.RevealedExpert.Should().BeTrue();
    }

    [Fact]
    public void ExaminationResult_RevealedExpert_ReturnsFalseForTier1()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 2,
            TierRevealed: 1,
            Description: "Detailed info",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 9, 3 }
        );

        // Assert
        result.RevealedExpert.Should().BeFalse();
    }

    [Fact]
    public void ExaminationResult_RevealedExpert_ReturnsFalseForTier0()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: false,
            NetSuccesses: 0,
            TierRevealed: 0,
            Description: "Base only",
            NewInfoRevealed: false,
            Rolls: new[] { 3, 5, 7 }
        );

        // Assert
        result.RevealedExpert.Should().BeFalse();
    }

    #endregion

    #region RevealedDetailed Property

    [Fact]
    public void ExaminationResult_RevealedDetailed_ReturnsTrueForTier1()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 1,
            TierRevealed: 1,
            Description: "Detailed info",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 3, 5 }
        );

        // Assert
        result.RevealedDetailed.Should().BeTrue();
    }

    [Fact]
    public void ExaminationResult_RevealedDetailed_ReturnsTrueForTier2()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: true,
            NetSuccesses: 4,
            TierRevealed: 2,
            Description: "Expert info",
            NewInfoRevealed: true,
            Rolls: new[] { 8, 9, 10, 8 }
        );

        // Assert
        result.RevealedDetailed.Should().BeTrue();
    }

    [Fact]
    public void ExaminationResult_RevealedDetailed_ReturnsFalseForTier0()
    {
        // Arrange & Act
        var result = new ExaminationResult(
            Success: false,
            NetSuccesses: 0,
            TierRevealed: 0,
            Description: "Base only",
            NewInfoRevealed: false,
            Rolls: new[] { 3, 5, 7 }
        );

        // Assert
        result.RevealedDetailed.Should().BeFalse();
    }

    #endregion

    #region NotFound Factory Method

    [Fact]
    public void ExaminationResult_NotFound_ReturnsFailedResult()
    {
        // Arrange & Act
        var result = ExaminationResult.NotFound("chest");

        // Assert
        result.Success.Should().BeFalse();
        result.NetSuccesses.Should().Be(0);
        result.TierRevealed.Should().Be(0);
        result.NewInfoRevealed.Should().BeFalse();
    }

    [Fact]
    public void ExaminationResult_NotFound_ContainsTargetNameInDescription()
    {
        // Arrange & Act
        var result = ExaminationResult.NotFound("rusted chest");

        // Assert
        result.Description.Should().Contain("rusted chest");
    }

    [Fact]
    public void ExaminationResult_NotFound_HasEmptyRolls()
    {
        // Arrange & Act
        var result = ExaminationResult.NotFound("something");

        // Assert
        result.Rolls.Should().BeEmpty();
    }

    [Fact]
    public void ExaminationResult_NotFound_IsNotFumble()
    {
        // Arrange & Act
        var result = ExaminationResult.NotFound("anything");

        // Assert
        result.IsFumble.Should().BeFalse();
    }

    #endregion

    #region Tier Thresholds (Documentation Tests)

    [Theory]
    [InlineData(0, 0, "Base description only - no net successes")]
    [InlineData(1, 1, "Detailed tier - 1+ net successes")]
    [InlineData(2, 1, "Detailed tier - 2 net successes still tier 1")]
    [InlineData(3, 2, "Expert tier - 3+ net successes")]
    [InlineData(5, 2, "Expert tier - 5 net successes still tier 2")]
    public void ExaminationResult_TierRevealed_MatchesExpectedValue(int netSuccesses, int expectedTier, string reason)
    {
        // This test documents the tier thresholds
        // The actual tier calculation happens in InteractionService
        // Here we just verify the tier values are correctly stored

        // Arrange & Act
        var result = new ExaminationResult(
            Success: netSuccesses > 0,
            NetSuccesses: netSuccesses,
            TierRevealed: expectedTier,
            Description: "Test",
            NewInfoRevealed: true,
            Rolls: Array.Empty<int>()
        );

        // Assert
        result.TierRevealed.Should().Be(expectedTier, reason);
    }

    #endregion
}
