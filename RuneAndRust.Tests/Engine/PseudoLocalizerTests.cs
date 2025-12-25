using FluentAssertions;
using RuneAndRust.Engine.Helpers;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for PseudoLocalizer helper (v0.3.15c - The Polyglot).
/// Tests diacritic transformation, format placeholder preservation, and expansion.
/// </summary>
public class PseudoLocalizerTests
{
    #region Transform Basic Tests

    [Fact]
    public void Transform_ReturnsNull_WhenInputIsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = PseudoLocalizer.Transform(input!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Transform_ReturnsEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Transform_AddsBrackets_ToSimpleString()
    {
        // Arrange
        var input = "Test";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().StartWith("[");
        result.Should().EndWith("]");
    }

    [Fact]
    public void Transform_AddsExpansion_ToSimpleString()
    {
        // Arrange
        var input = "Test"; // 4 chars, ~30% = 1 char expansion

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert - result should be longer than input + 2 brackets
        result.Should().Contain("_"); // Should have at least one underscore for expansion
    }

    #endregion

    #region Diacritic Transformation Tests

    [Theory]
    [InlineData("a", "á")]
    [InlineData("e", "é")]
    [InlineData("i", "í")]
    [InlineData("o", "ó")]
    [InlineData("u", "ú")]
    [InlineData("A", "Á")]
    [InlineData("E", "É")]
    [InlineData("I", "Í")]
    [InlineData("O", "Ó")]
    [InlineData("U", "Ú")]
    [InlineData("c", "ç")]
    [InlineData("C", "Ç")]
    [InlineData("n", "ñ")]
    [InlineData("N", "Ñ")]
    public void Transform_ReplacesDiacritics_ForMappedCharacters(string input, string expectedChar)
    {
        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert - result should contain the transformed character
        result.Should().Contain(expectedChar);
    }

    [Fact]
    public void Transform_PreservesUnmappedCharacters()
    {
        // Arrange
        var input = "xyz123!@#";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert - these chars should be preserved (not in diacritic map)
        result.Should().Contain("x");
        result.Should().Contain("y");
        result.Should().Contain("z");
        result.Should().Contain("1");
        result.Should().Contain("2");
        result.Should().Contain("3");
    }

    [Fact]
    public void Transform_TransformsNewGame_Correctly()
    {
        // Arrange
        var input = "New Game";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        // Should contain: [Ñéw Gámé__]  (brackets + transformed + expansion)
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain("Ñ"); // N -> Ñ
        result.Should().Contain("é"); // e -> é
        result.Should().Contain("á"); // a -> á
    }

    #endregion

    #region Format Placeholder Preservation Tests

    [Fact]
    public void Transform_PreservesFormatPlaceholder_Simple()
    {
        // Arrange
        var input = "{0} min";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().Contain("{0}");
    }

    [Fact]
    public void Transform_PreservesFormatPlaceholder_WithFormat()
    {
        // Arrange
        var input = "{0:N2} gold";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().Contain("{0:N2}");
    }

    [Fact]
    public void Transform_PreservesMultiplePlaceholders()
    {
        // Arrange
        var input = "{0} of {1}";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().Contain("{0}");
        result.Should().Contain("{1}");
    }

    [Fact]
    public void Transform_TransformsTextAroundPlaceholders()
    {
        // Arrange
        var input = "Value: {0}%";

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        result.Should().Contain("{0}"); // Placeholder preserved
        result.Should().Contain("á");   // 'a' in 'Value' -> 'á'
    }

    #endregion

    #region Expansion Tests

    [Fact]
    public void Transform_AddsAtLeastOneExpansionCharacter()
    {
        // Arrange
        var input = "Hi"; // Very short string

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert - minimum 1 underscore even for short strings
        result.Should().Contain("_");
    }

    [Fact]
    public void Transform_AddsApproximately30PercentExpansion()
    {
        // Arrange
        var input = "This is a longer test string"; // 29 chars

        // Act
        var result = PseudoLocalizer.Transform(input);

        // Assert
        // Expected: [content___...___] where underscores ~= 29 * 0.3 = ~8-9
        var underscoreCount = result.Count(c => c == '_');
        underscoreCount.Should().BeGreaterThanOrEqualTo(8);
    }

    #endregion
}
