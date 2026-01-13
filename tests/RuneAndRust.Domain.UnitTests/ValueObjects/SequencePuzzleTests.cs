using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the SequencePuzzle value object.
/// </summary>
[TestFixture]
public class SequencePuzzleTests
{
    [Test]
    public void Create_WithValidSequence_CreatesInstance()
    {
        // Act
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth", "air"]);

        // Assert
        puzzle.RequiredSequence.Should().HaveCount(4);
        puzzle.TotalSteps.Should().Be(4);
        puzzle.ResetOnWrongStep.Should().BeTrue();
    }

    [Test]
    public void Create_WithEmptySequence_ThrowsException()
    {
        // Act
        var act = () => SequencePuzzle.Create([]);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void IsCorrectNextStep_WhenCorrect_ReturnsTrue()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth"]);
        var completed = new List<string>();

        // Act & Assert
        puzzle.IsCorrectNextStep(completed, "fire").Should().BeTrue();
    }

    [Test]
    public void IsCorrectNextStep_WhenWrong_ReturnsFalse()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth"]);
        var completed = new List<string>();

        // Act & Assert
        puzzle.IsCorrectNextStep(completed, "water").Should().BeFalse();
    }

    [Test]
    public void IsCorrectNextStep_IsCaseInsensitive()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water"]);
        var completed = new List<string>();

        // Act & Assert
        puzzle.IsCorrectNextStep(completed, "FIRE").Should().BeTrue();
    }

    [Test]
    public void IsComplete_WhenAllStepsCompleted_ReturnsTrue()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water"]);
        var completed = new List<string> { "fire", "water" };

        // Act & Assert
        puzzle.IsComplete(completed).Should().BeTrue();
    }

    [Test]
    public void IsComplete_WhenPartiallyCompleted_ReturnsFalse()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth"]);
        var completed = new List<string> { "fire", "water" };

        // Act & Assert
        puzzle.IsComplete(completed).Should().BeFalse();
    }

    [Test]
    public void GetRemainingSteps_ReturnsCorrectCount()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth", "air"]);
        var completed = new List<string> { "fire" };

        // Act & Assert
        puzzle.GetRemainingSteps(completed).Should().Be(3);
    }

    [Test]
    public void GetStepDescription_WhenExists_ReturnsDescription()
    {
        // Arrange
        var descriptions = new Dictionary<string, string>
        {
            ["fire"] = "The flames flicker..."
        };
        var puzzle = SequencePuzzle.Create(["fire"], descriptions);

        // Act & Assert
        puzzle.GetStepDescription("fire").Should().Be("The flames flicker...");
    }

    [Test]
    public void GetNextExpectedStep_ReturnsCorrectStep()
    {
        // Arrange
        var puzzle = SequencePuzzle.Create(["fire", "water", "earth"]);
        var completed = new List<string> { "fire" };

        // Act & Assert
        puzzle.GetNextExpectedStep(completed).Should().Be("water");
    }
}
