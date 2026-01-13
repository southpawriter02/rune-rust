using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the CombinationPuzzle value object.
/// </summary>
[TestFixture]
public class CombinationPuzzleTests
{
    [Test]
    public void Create_WithValidSolution_CreatesInstance()
    {
        // Act
        var puzzle = CombinationPuzzle.Create("1234");

        // Assert
        puzzle.Solution.Should().Be("1234");
        puzzle.Length.Should().Be(4);
        puzzle.ValidCharacters.Should().Be("0123456789");
    }

    [Test]
    public void Validate_WhenCorrect_ReturnsTrue()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234");

        // Act & Assert
        puzzle.Validate("1234").Should().BeTrue();
    }

    [Test]
    public void Validate_WhenWrong_ReturnsFalse()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234");

        // Act & Assert
        puzzle.Validate("4321").Should().BeFalse();
    }

    [Test]
    public void Validate_CaseInsensitive_MatchesIgnoringCase()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("ABCD", validChars: "ABCD", caseSensitive: false);

        // Act & Assert
        puzzle.Validate("abcd").Should().BeTrue();
    }

    [Test]
    public void Validate_CaseSensitive_RequiresExactCase()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("ABCD", validChars: "ABCDabcd", caseSensitive: true);

        // Act & Assert
        puzzle.Validate("abcd").Should().BeFalse();
        puzzle.Validate("ABCD").Should().BeTrue();
    }

    [Test]
    public void Validate_WithAlternateSolution_AcceptsBoth()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234", alternateSolutions: ["4321", "1111"]);

        // Act & Assert
        puzzle.Validate("1234").Should().BeTrue();
        puzzle.Validate("4321").Should().BeTrue();
        puzzle.Validate("1111").Should().BeTrue();
        puzzle.Validate("9999").Should().BeFalse();
    }

    [Test]
    public void IsValidInput_WithValidChars_ReturnsTrue()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234");

        // Act & Assert
        puzzle.IsValidInput("5678").Should().BeTrue();
    }

    [Test]
    public void IsValidInput_WithInvalidChars_ReturnsFalse()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234");

        // Act & Assert
        puzzle.IsValidInput("12AB").Should().BeFalse();
    }

    [Test]
    public void GetFeedback_WhenPartialFeedbackEnabled_ReturnsCorrectCounts()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234", showPartialFeedback: true);

        // Act
        var feedback = puzzle.GetFeedback("1243"); // 2 correct position (1,2), 2 correct char (4,3)

        // Assert
        feedback.CorrectPositions.Should().Be(2);
        feedback.CorrectCharacters.Should().Be(2);
    }

    [Test]
    public void GetFeedback_WhenPartialFeedbackDisabled_ReturnsZeroes()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234", showPartialFeedback: false);

        // Act
        var feedback = puzzle.GetFeedback("1234");

        // Assert
        feedback.CorrectPositions.Should().Be(0);
        feedback.CorrectCharacters.Should().Be(0);
    }

    [Test]
    public void Validate_WithSeparator_NormalizesInput()
    {
        // Arrange
        var puzzle = CombinationPuzzle.Create("1234", separator: "-");

        // Act & Assert
        puzzle.Validate("1-2-3-4").Should().BeTrue();
        puzzle.Validate("1234").Should().BeTrue();
    }
}
