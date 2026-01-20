using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Converters;
using System.Globalization;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Converters;

/// <summary>
/// Unit tests for <see cref="ModifierToStringConverter"/>.
/// </summary>
[TestFixture]
public class ModifierToStringConverterTests
{
    private ModifierToStringConverter _converter = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new ModifierToStringConverter();
    }

    /// <summary>
    /// Verifies that positive modifiers format as (+X).
    /// </summary>
    [TestCase(0, "(+0)")]
    [TestCase(1, "(+1)")]
    [TestCase(3, "(+3)")]
    [TestCase(10, "(+10)")]
    public void Convert_PositiveModifier_ReturnsCorrectFormat(int modifier, string expected)
    {
        // Act
        var result = _converter.Convert(modifier, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that negative modifiers format as (X) without extra minus.
    /// </summary>
    [TestCase(-1, "(-1)")]
    [TestCase(-3, "(-3)")]
    [TestCase(-5, "(-5)")]
    public void Convert_NegativeModifier_ReturnsCorrectFormat(int modifier, string expected)
    {
        // Act
        var result = _converter.Convert(modifier, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that non-integer value returns default (+0).
    /// </summary>
    [Test]
    public void Convert_NonIntegerValue_ReturnsDefaultFormat()
    {
        // Act
        var result = _converter.Convert("not a number", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("(+0)");
    }

    /// <summary>
    /// Verifies that null value returns default (+0).
    /// </summary>
    [Test]
    public void Convert_NullValue_ReturnsDefaultFormat()
    {
        // Act
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("(+0)");
    }

    /// <summary>
    /// Verifies that ConvertBack throws NotImplementedException.
    /// </summary>
    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        var act = () => _converter.ConvertBack("(+3)", typeof(int), null, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
