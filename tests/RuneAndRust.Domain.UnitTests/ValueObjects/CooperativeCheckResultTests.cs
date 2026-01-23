using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="CooperativeCheckResult"/> value object.
/// </summary>
[TestFixture]
public class CooperativeCheckResultTests
{
    [Test]
    public void IsSuccess_WhenOutcomeIsMarginalOrBetter_ReturnsTrue()
    {
        // Arrange
        var result = new CooperativeCheckResult(
            CooperationType: CooperationType.BestAttempt,
            ParticipantIds: new[] { "player-1", "player-2" },
            SkillId: "perception",
            SubType: null,
            DifficultyClass: 3,
            FinalOutcome: SkillOutcome.MarginalSuccess,
            FinalNetSuccesses: 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void IsSuccess_WhenOutcomeIsFailure_ReturnsFalse()
    {
        // Arrange
        var result = new CooperativeCheckResult(
            CooperationType: CooperationType.WeakestLink,
            ParticipantIds: new[] { "player-1" },
            SkillId: "stealth",
            SubType: null,
            DifficultyClass: 4,
            FinalOutcome: SkillOutcome.Failure,
            FinalNetSuccesses: 2);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void Margin_CalculatesCorrectValue()
    {
        // Arrange
        var result = new CooperativeCheckResult(
            CooperationType: CooperationType.Combined,
            ParticipantIds: new[] { "player-1", "player-2", "player-3" },
            SkillId: "athletics",
            SubType: null,
            DifficultyClass: 5,
            FinalOutcome: SkillOutcome.ExceptionalSuccess,
            FinalNetSuccesses: 8);

        // Assert
        result.Margin.Should().Be(3); // 8 - 5 = 3
    }

    [Test]
    public void ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var result = new CooperativeCheckResult(
            CooperationType: CooperationType.Assisted,
            ParticipantIds: new[] { "player-1", "player-2" },
            SkillId: "lockpicking",
            SubType: null,
            DifficultyClass: 4,
            FinalOutcome: SkillOutcome.FullSuccess,
            FinalNetSuccesses: 5,
            ActiveRollerId: "player-1");

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("Assisted");
        display.Should().Contain("2 participants");
        display.Should().Contain("Success");
        display.Should().Contain("DC 4");
    }
}
