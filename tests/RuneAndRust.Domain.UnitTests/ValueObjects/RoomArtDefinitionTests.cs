using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="RoomArtDefinition"/>.
/// </summary>
[TestFixture]
public class RoomArtDefinitionTests
{
    #region MaxWidth Tests

    [Test]
    public void MaxWidth_WithArtLines_ReturnsMaxLineLength()
    {
        // Arrange
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "12345", "1234567890", "123" },
            new Dictionary<char, string>());

        // Assert
        art.MaxWidth.Should().Be(10);
    }

    [Test]
    public void MaxWidth_WithEmptyArt_ReturnsZero()
    {
        // Arrange
        var art = RoomArtDefinition.Empty("empty-room");

        // Assert
        art.MaxWidth.Should().Be(0);
    }

    #endregion

    #region Height Tests

    [Test]
    public void Height_ReturnsLineCount()
    {
        // Arrange
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "line1", "line2", "line3" },
            new Dictionary<char, string>());

        // Assert
        art.Height.Should().Be(3);
    }

    [Test]
    public void Height_WithEmptyArt_ReturnsZero()
    {
        // Arrange
        var art = RoomArtDefinition.Empty("empty-room");

        // Assert
        art.Height.Should().Be(0);
    }

    #endregion

    #region UsedSymbols Tests

    [Test]
    public void UsedSymbols_ReturnsUniqueSymbols()
    {
        // Arrange
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "AAA", "BBB", "ABA" },
            new Dictionary<char, string>());

        // Assert
        art.UsedSymbols.Should().Contain('A');
        art.UsedSymbols.Should().Contain('B');
        art.UsedSymbols.Count.Should().Be(2);
    }

    #endregion

    #region Legend Tests

    [Test]
    public void Constructor_WithLegend_StoresLegend()
    {
        // Arrange
        var legend = new Dictionary<char, string>
        {
            ['~'] = "water",
            ['▲'] = "rock"
        };

        // Act
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "~~~▲▲▲" },
            legend);

        // Assert
        art.Legend.Should().HaveCount(2);
        art.Legend['~'].Should().Be("water");
    }

    [Test]
    public void Constructor_WithSymbolColors_StoresColors()
    {
        // Arrange
        var colors = new Dictionary<char, ConsoleColor>
        {
            ['~'] = ConsoleColor.Blue,
            ['▲'] = ConsoleColor.Gray
        };

        // Act
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "~~~▲▲▲" },
            new Dictionary<char, string>(),
            colors);

        // Assert
        art.SymbolColors.Should().NotBeNull();
        art.SymbolColors!['~'].Should().Be(ConsoleColor.Blue);
    }

    #endregion

    #region ShowLegend Tests

    [Test]
    public void ShowLegend_DefaultsToTrue()
    {
        // Arrange & Act
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "test" },
            new Dictionary<char, string>());

        // Assert
        art.ShowLegend.Should().BeTrue();
    }

    [Test]
    public void ShowLegend_CanBeSetToFalse()
    {
        // Arrange & Act
        var art = new RoomArtDefinition(
            "test-room",
            new[] { "test" },
            new Dictionary<char, string>(),
            null,
            ShowLegend: false);

        // Assert
        art.ShowLegend.Should().BeFalse();
    }

    #endregion

    #region Empty Factory Tests

    [Test]
    public void Empty_CreatesEmptyDefinition()
    {
        // Act
        var art = RoomArtDefinition.Empty("test-room");

        // Assert
        art.RoomId.Should().Be("test-room");
        art.ArtLines.Should().BeEmpty();
        art.Legend.Should().BeEmpty();
        art.SymbolColors.Should().BeNull();
    }

    #endregion
}
