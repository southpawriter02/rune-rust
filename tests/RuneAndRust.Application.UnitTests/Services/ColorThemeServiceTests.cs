using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ColorThemeService"/>.
/// </summary>
[TestFixture]
public class ColorThemeServiceTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private Mock<ILogger<ColorThemeService>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockLogger = new Mock<ILogger<ColorThemeService>>();
        
        // Default: terminal supports color
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_InitializesWithDarkTheme()
    {
        // Act
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        service.CurrentTheme.Should().Be("Dark Theme");
    }

    [Test]
    public void Constructor_LoadsAvailableThemes()
    {
        // Act
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        service.AvailableThemes.Should().Contain("dark");
        service.AvailableThemes.Should().Contain("light");
    }

    #endregion

    #region SetTheme Tests

    [Test]
    public void SetTheme_WithValidTheme_ChangesCurrentTheme()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        service.SetTheme("light");

        // Assert
        service.CurrentTheme.Should().Be("Light Theme");
    }

    [Test]
    public void SetTheme_CaseInsensitive_ChangesTheme()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        service.SetTheme("LIGHT");

        // Assert
        service.CurrentTheme.Should().Be("Light Theme");
    }

    [Test]
    public void SetTheme_WithInvalidTheme_ThrowsArgumentException()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        var act = () => service.SetTheme("nonexistent");

        // Assert
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("nonexistent");
    }

    #endregion

    #region GetColor Tests

    [Test]
    public void GetColor_WithDefinedType_ReturnsCorrectColor()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        var color = service.GetColor(MessageType.CombatDamage);

        // Assert - dark theme has DarkRed for CombatDamage
        color.Should().Be(ConsoleColor.DarkRed);
    }

    [Test]
    public void GetColor_Info_ReturnsCyan()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        var color = service.GetColor(MessageType.Info);

        // Assert
        color.Should().Be(ConsoleColor.Cyan);
    }

    [Test]
    public void GetColor_LootLegendary_ReturnsYellow()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        var color = service.GetColor(MessageType.LootLegendary);

        // Assert
        color.Should().Be(ConsoleColor.Yellow);
    }

    #endregion

    #region WriteColored Tests

    [Test]
    public void WriteColored_WhenColorSupported_WritesWithColor()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        service.WriteColored("Test message", MessageType.Info);

        // Assert - terminal.Write should have been called
        _mockTerminal.Verify(t => t.Write("Test message"), Times.Once());
    }

    [Test]
    public void WriteColored_WhenColorNotSupported_WritesPlainText()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsColor).Returns(false);
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        service.WriteColored("Test message", MessageType.Error);

        // Assert - should still write the text
        _mockTerminal.Verify(t => t.Write("Test message"), Times.Once());
    }

    [Test]
    public void WriteColoredAt_SetsPositionAndWrites()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);

        // Act
        service.WriteColoredAt(10, 5, "Positioned text", MessageType.Success);

        // Assert
        _mockTerminal.Verify(t => t.SetCursorPosition(10, 5), Times.Once());
        _mockTerminal.Verify(t => t.Write("Positioned text"), Times.Once());
    }

    #endregion

    #region Theme Comparison Tests

    [Test]
    public void LightTheme_HasDifferentColorsFromDark()
    {
        // Arrange
        var service = new ColorThemeService(_mockTerminal.Object, _mockLogger.Object);
        var darkErrorColor = service.GetColor(MessageType.Error);
        
        // Act
        service.SetTheme("light");
        var lightErrorColor = service.GetColor(MessageType.Error);

        // Assert - light theme should use DarkRed for errors
        darkErrorColor.Should().Be(ConsoleColor.Red);
        lightErrorColor.Should().Be(ConsoleColor.DarkRed);
    }

    #endregion
}
