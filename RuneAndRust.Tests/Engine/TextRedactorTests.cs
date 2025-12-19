using FluentAssertions;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the TextRedactor service.
/// Validates text masking behavior based on completion percentage.
/// </summary>
public class TextRedactorTests
{
    private readonly TextRedactor _sut;

    public TextRedactorTests()
    {
        _sut = new TextRedactor();
    }

    #region Edge Case Tests

    [Fact]
    public void RedactText_AtZeroPercent_ReturnsRedactedMessage()
    {
        // Arrange
        var fullText = "This is a test sentence with multiple words.";

        // Act
        var result = _sut.RedactText(fullText, 0);

        // Assert
        result.Should().Be("[REDACTED]");
    }

    [Fact]
    public void RedactText_AtHundredPercent_ReturnsFullText()
    {
        // Arrange
        var fullText = "This is a test sentence with multiple words.";

        // Act
        var result = _sut.RedactText(fullText, 100);

        // Assert
        result.Should().Be(fullText);
    }

    [Fact]
    public void RedactText_EmptyString_ReturnsEmpty()
    {
        // Arrange
        var fullText = "";

        // Act
        var result = _sut.RedactText(fullText, 50);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void RedactText_WhitespaceOnly_ReturnsWhitespace()
    {
        // Arrange
        var fullText = "   ";

        // Act
        var result = _sut.RedactText(fullText, 50);

        // Assert
        result.Should().Be("   ");
    }

    [Fact]
    public void RedactText_NullInput_ReturnsNull()
    {
        // Arrange
        string? fullText = null;

        // Act
        var result = _sut.RedactText(fullText!, 50);

        // Assert - null/whitespace returns itself
        result.Should().BeNull();
    }

    #endregion

    #region Partial Redaction Tests

    [Fact]
    public void RedactText_AtFiftyPercent_ReturnsMixedText()
    {
        // Arrange
        var fullText = "Word1 Word2 Word3 Word4";

        // Act
        var result = _sut.RedactText(fullText, 50);

        // Assert - Should contain both visible words and redacted blocks
        result.Should().Contain("[grey]");
        result.Should().ContainAny("Word1", "Word2", "Word3", "Word4");
    }

    [Fact]
    public void RedactText_ContainsRedactedBlocks_UsesGreyMarkup()
    {
        // Arrange
        var fullText = "Word1 Word2 Word3";

        // Act
        var result = _sut.RedactText(fullText, 30);

        // Assert - Redacted portions should use Spectre markup
        result.Should().Contain("[grey]");
        result.Should().Contain("[/]");
    }

    #endregion

    #region Stability Tests

    [Fact]
    public void RedactText_SamePercentage_ProducesStableOutput()
    {
        // Arrange
        var fullText = "This is a test sentence with multiple words.";
        var percentage = 50;

        // Act - Call multiple times with same input
        var result1 = _sut.RedactText(fullText, percentage);
        var result2 = _sut.RedactText(fullText, percentage);
        var result3 = _sut.RedactText(fullText, percentage);

        // Assert - All results should be identical
        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    [Fact]
    public void RedactText_DifferentInstances_ProduceSameOutput()
    {
        // Arrange
        var fullText = "The ancient runes glow with power.";
        var percentage = 60;
        var redactor1 = new TextRedactor();
        var redactor2 = new TextRedactor();

        // Act
        var result1 = redactor1.RedactText(fullText, percentage);
        var result2 = redactor2.RedactText(fullText, percentage);

        // Assert
        result1.Should().Be(result2);
    }

    #endregion

    #region Progressive Reveal Tests

    [Fact]
    public void RedactText_ProgressiveReveal_ShowsMoreWordsAtHigherPercentage()
    {
        // Arrange
        var fullText = "One Two Three Four Five Six Seven Eight Nine Ten";

        // Act
        var result25 = _sut.RedactText(fullText, 25);
        var result50 = _sut.RedactText(fullText, 50);
        var result75 = _sut.RedactText(fullText, 75);

        // Assert - Count visible words at each level
        var visibleWords25 = CountVisibleWords(result25);
        var visibleWords50 = CountVisibleWords(result50);
        var visibleWords75 = CountVisibleWords(result75);

        visibleWords25.Should().BeLessThan(visibleWords50);
        visibleWords50.Should().BeLessThan(visibleWords75);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(90)]
    [InlineData(99)]
    public void RedactText_AtVariousPercentages_ProducesValidOutput(int percentage)
    {
        // Arrange
        var fullText = "Sample text for testing redaction at various percentages.";

        // Act
        var result = _sut.RedactText(fullText, percentage);

        // Assert - Result should not be null or empty
        result.Should().NotBeNullOrEmpty();
        // Result should contain at least some content
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion

    #region Single Word Tests

    [Fact]
    public void RedactText_SingleWord_AtFiftyPercent_ReturnsWordOrRedacted()
    {
        // Arrange
        var fullText = "Word";

        // Act
        var result = _sut.RedactText(fullText, 50);

        // Assert - Should be either the word or a redacted block
        result.Should().BeOneOf("Word", "[grey]████[/]");
    }

    [Fact]
    public void RedactText_SingleWord_AtHundredPercent_ReturnsWord()
    {
        // Arrange
        var fullText = "Word";

        // Act
        var result = _sut.RedactText(fullText, 100);

        // Assert
        result.Should().Be("Word");
    }

    #endregion

    #region Helper Methods

    private static int CountVisibleWords(string redactedText)
    {
        // Count words that are NOT redacted (don't contain the redaction block)
        var words = redactedText.Split(' ');
        return words.Count(w => !w.Contains("[grey]"));
    }

    #endregion
}
