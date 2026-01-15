using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.Converters;
using System.Globalization;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Converters;

/// <summary>
/// Unit tests for <see cref="DirectionToSymbolConverter"/>.
/// </summary>
[TestFixture]
public class DirectionToSymbolConverterTests
{
    private DirectionToSymbolConverter _converter = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new DirectionToSymbolConverter();
    }

    /// <summary>
    /// Verifies that all cardinal directions convert to correct symbols.
    /// </summary>
    [TestCase(Direction.North, "↑")]
    [TestCase(Direction.South, "↓")]
    [TestCase(Direction.East, "→")]
    [TestCase(Direction.West, "←")]
    [TestCase(Direction.Up, "⬆")]
    [TestCase(Direction.Down, "⬇")]
    public void Convert_Direction_ReturnsCorrectSymbol(Direction direction, string expected)
    {
        // Act
        var result = _converter.Convert(direction, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that non-Direction value returns question mark.
    /// </summary>
    [Test]
    public void Convert_NonDirectionValue_ReturnsQuestionMark()
    {
        // Act
        var result = _converter.Convert("not a direction", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("?");
    }

    /// <summary>
    /// Verifies that null value returns question mark.
    /// </summary>
    [Test]
    public void Convert_NullValue_ReturnsQuestionMark()
    {
        // Act
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("?");
    }

    /// <summary>
    /// Verifies that ConvertBack throws NotImplementedException.
    /// </summary>
    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        var act = () => _converter.ConvertBack("↑", typeof(Direction), null, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
