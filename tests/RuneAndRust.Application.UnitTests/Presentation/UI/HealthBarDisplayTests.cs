using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Configuration;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="HealthBarDisplay"/>.
/// </summary>
[TestFixture]
public class HealthBarDisplayTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private HealthBarDisplay _healthBarDisplayDisplay = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        _healthBarDisplayDisplay = new HealthBarDisplayDisplay(_mockTerminal.Object);
    }

    #region Render Tests

    [Test]
    public void Render_AtFullHealth_ReturnsAllFilledCharacters()
    {
        // Act
        var bar = _healthBarDisplay.Render(100, 100, 10, BarStyle.Standard);

        // Assert - all filled characters
        bar.Should().Be("██████████");
        bar.Length.Should().Be(10);
    }

    [Test]
    public void Render_AtHalfHealth_ReturnsHalfFilledHalfEmpty()
    {
        // Act
        var bar = _healthBarDisplay.Render(50, 100, 10, BarStyle.Standard);

        // Assert - half filled, half empty
        bar.Should().Be("█████░░░░░");
    }

    [Test]
    public void Render_AtZeroHealth_ReturnsAllEmptyCharacters()
    {
        // Act
        var bar = _healthBarDisplay.Render(0, 100, 10, BarStyle.Standard);

        // Assert - all empty
        bar.Should().Be("░░░░░░░░░░");
    }

    [Test]
    public void Render_DetailedStyle_IncludesBracketsAndValue()
    {
        // Act
        var bar = _healthBarDisplay.Render(80, 100, 10, BarStyle.Detailed);

        // Assert
        bar.Should().StartWith("[");
        bar.Should().Contain("] 80/100");
    }

    [Test]
    public void Render_CompactStyle_AppendsValue()
    {
        // Act
        var bar = _healthBarDisplay.Render(50, 100, 10, BarStyle.Compact);

        // Assert
        bar.Should().EndWith("50");
    }

    [Test]
    public void Render_NumericStyle_ReturnsValueOnly()
    {
        // Act
        var bar = _healthBarDisplay.Render(75, 100, 10, BarStyle.Numeric);

        // Assert
        bar.Should().Be("75/100");
    }

    [Test]
    public void Render_WithAsciiFallback_UsesAsciiCharacters()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(false);
        var healthBar = new HealthBarDisplay(_mockTerminal.Object);

        // Act
        var bar = healthBar.Render(50, 100, 10, BarStyle.Standard);

        // Assert - uses # and - instead of Unicode
        bar.Should().Contain("#");
        bar.Should().Contain("-");
    }

    #endregion

    #region RenderLabeled Tests

    [Test]
    public void RenderLabeled_WithLabel_IncludesLabelAndValue()
    {
        // Act
        var bar = _healthBarDisplay.RenderLabeled("HP", 80, 100, 30);

        // Assert
        bar.Should().StartWith("HP: ");
        bar.Should().EndWith(" 80/100");
    }

    [Test]
    public void RenderLabeled_NarrowWidth_ReturnsNumericOnly()
    {
        // Act - very narrow width
        var bar = _healthBarDisplay.RenderLabeled("HP", 80, 100, 15);

        // Assert - falls back to numeric only
        bar.Should().Be("HP: 80/100");
    }

    #endregion

    #region GetThresholdColor Tests

    [Test]
    public void GetThresholdColor_AtHighHealth_ReturnsGreen()
    {
        // Act
        var color = _healthBarDisplay.GetThresholdColor(100, 100, BarType.Health);

        // Assert
        color.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetThresholdColor_AtMediumHealth_ReturnsYellow()
    {
        // Act - 50% health
        var color = _healthBarDisplay.GetThresholdColor(50, 100, BarType.Health);

        // Assert
        color.Should().Be(ConsoleColor.Yellow);
    }

    [Test]
    public void GetThresholdColor_AtLowHealth_ReturnsRed()
    {
        // Act - 25% health
        var color = _healthBarDisplay.GetThresholdColor(25, 100, BarType.Health);

        // Assert
        color.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetThresholdColor_AtCriticalHealth_ReturnsDarkRed()
    {
        // Act - 10% health
        var color = _healthBarDisplay.GetThresholdColor(5, 100, BarType.Health);

        // Assert
        color.Should().Be(ConsoleColor.DarkRed);
    }

    [Test]
    public void GetThresholdColor_ForMana_ReturnsBlue()
    {
        // Act
        var color = _healthBarDisplay.GetThresholdColor(50, 100, BarType.Mana);

        // Assert
        color.Should().Be(ConsoleColor.Blue);
    }

    [Test]
    public void GetThresholdColor_ForExperience_ReturnsMagenta()
    {
        // Act
        var color = _healthBarDisplay.GetThresholdColor(50, 100, BarType.Experience);

        // Assert
        color.Should().Be(ConsoleColor.Magenta);
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Render_NegativeCurrent_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => _healthBarDisplay.Render(-1, 100, 10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Render_ZeroMax_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => _healthBarDisplay.Render(50, 0, 10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Render_WidthTooSmall_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => _healthBarDisplay.Render(50, 100, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
