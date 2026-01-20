using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Converters;
using RuneAndRust.Presentation.Gui.Models;
using System.Globalization;
using Avalonia.Media;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Converters;

/// <summary>
/// Unit tests for <see cref="MessageTypeToColorConverter"/>.
/// </summary>
[TestFixture]
public class MessageTypeToColorConverterTests
{
    private MessageTypeToColorConverter _converter = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new MessageTypeToColorConverter();
    }

    /// <summary>
    /// Verifies that Default type returns White.
    /// </summary>
    [Test]
    public void Convert_DefaultType_ReturnsWhite()
    {
        // Act
        var result = _converter.Convert(MessageType.Default, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.White);
    }

    /// <summary>
    /// Verifies that CombatCritical type returns Magenta.
    /// </summary>
    [Test]
    public void Convert_CombatCritical_ReturnsMagenta()
    {
        // Act
        var result = _converter.Convert(MessageType.CombatCritical, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Magenta);
    }

    /// <summary>
    /// Verifies that LootLegendary type returns Gold.
    /// </summary>
    [Test]
    public void Convert_LootLegendary_ReturnsGold()
    {
        // Act
        var result = _converter.Convert(MessageType.LootLegendary, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Gold);
    }

    /// <summary>
    /// Verifies that Error type returns Red.
    /// </summary>
    [Test]
    public void Convert_Error_ReturnsRed()
    {
        // Act
        var result = _converter.Convert(MessageType.Error, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Red);
    }

    /// <summary>
    /// Verifies that null value returns White.
    /// </summary>
    [Test]
    public void Convert_NullValue_ReturnsWhite()
    {
        // Act
        var result = _converter.Convert(null, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.White);
    }

    /// <summary>
    /// Verifies that ConvertBack throws NotImplementedException.
    /// </summary>
    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        var act = () => _converter.ConvertBack(Brushes.Red, typeof(MessageType), null, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
