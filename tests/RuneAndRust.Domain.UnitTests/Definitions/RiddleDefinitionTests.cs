using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Tests for the RiddleDefinition class.
/// </summary>
[TestFixture]
public class RiddleDefinitionTests
{
    [Test]
    public void Create_WithValidParameters_CreatesInstance()
    {
        // Act
        var riddle = RiddleDefinition.Create(
            "sphinx-riddle",
            "What has four legs in the morning?",
            ["man", "human", "person"]);

        // Assert
        riddle.Id.Should().Be("sphinx-riddle");
        riddle.Question.Should().Contain("four legs");
        riddle.AcceptedAnswers.Should().HaveCount(3);
    }

    [Test]
    public void Create_WithHints_StoresHintsInOrder()
    {
        // Arrange
        var hints = new[]
        {
            PuzzleHint.Free("Think about life stages", 2),
            PuzzleHint.Free("Crawling...", 1)
        };

        // Act
        var riddle = RiddleDefinition.Create("test", "Question?", ["answer"], hints);

        // Assert
        riddle.Hints.Should().HaveCount(2);
        riddle.Hints[0].Order.Should().Be(1);
        riddle.HasHints.Should().BeTrue();
    }

    [Test]
    public void ValidateAnswer_WhenCorrect_ReturnsTrue()
    {
        // Arrange
        var riddle = RiddleDefinition.Create("test", "Q?", ["answer"]);

        // Act & Assert
        riddle.ValidateAnswer("answer").Should().BeTrue();
    }

    [Test]
    public void ValidateAnswer_IsCaseInsensitive()
    {
        // Arrange
        var riddle = RiddleDefinition.Create("test", "Q?", ["Answer"]);

        // Act & Assert
        riddle.ValidateAnswer("ANSWER").Should().BeTrue();
        riddle.ValidateAnswer("answer").Should().BeTrue();
    }

    [Test]
    public void ValidateAnswer_WhenWrong_ReturnsFalse()
    {
        // Arrange
        var riddle = RiddleDefinition.Create("test", "Q?", ["answer"]);

        // Act & Assert
        riddle.ValidateAnswer("wrong").Should().BeFalse();
    }
}
