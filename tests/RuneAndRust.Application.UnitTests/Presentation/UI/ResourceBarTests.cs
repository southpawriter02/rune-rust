using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="ResourceBar"/>.
/// </summary>
[TestFixture]
public class ResourceBarTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private HealthBar _healthBar = null!;
    private ResourceBar _resourceBar = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        _healthBar = new HealthBar(_mockTerminal.Object);
        _resourceBar = new ResourceBar(_healthBar);
    }

    #region GetColor Tests

    [Test]
    public void GetColor_ForMana_ReturnsBlue()
    {
        // Act
        var color = _resourceBar.GetColor("Mana", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Blue);
    }

    [Test]
    public void GetColor_ForMp_ReturnsBlue()
    {
        // Act - lowercase mp
        var color = _resourceBar.GetColor("mp", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Blue);
    }

    [Test]
    public void GetColor_ForExperience_ReturnsMagenta()
    {
        // Act
        var color = _resourceBar.GetColor("Experience", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Magenta);
    }

    [Test]
    public void GetColor_ForXp_ReturnsMagenta()
    {
        // Act
        var color = _resourceBar.GetColor("XP", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Magenta);
    }

    [Test]
    public void GetColor_ForRage_ReturnsDarkRed()
    {
        // Act - custom resource
        var color = _resourceBar.GetColor("Rage", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.DarkRed);
    }

    [Test]
    public void GetColor_ForFocus_ReturnsCyan()
    {
        // Act - custom resource
        var color = _resourceBar.GetColor("Focus", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Cyan);
    }

    [Test]
    public void GetColor_ForUnknownResource_ReturnsGray()
    {
        // Act
        var color = _resourceBar.GetColor("UnknownResource", 50, 100);

        // Assert
        color.Should().Be(ConsoleColor.Gray);
    }

    #endregion

    #region Render Tests

    [Test]
    public void Render_ReturnsFormattedBar()
    {
        // Act
        var bar = _resourceBar.Render("Mana", 50, 100, 10, BarStyle.Standard);

        // Assert
        bar.Should().HaveLength(10);
        bar.Should().Contain("█");
    }

    [Test]
    public void RenderLabeled_ReturnsLabeledBar()
    {
        // Act
        var bar = _resourceBar.RenderLabeled("MP", "Mana", 60, 100, 30);

        // Assert
        bar.Should().StartWith("MP: ");
        bar.Should().EndWith(" 60/100");
    }

    #endregion

    #region GetBarTypeForResource Tests

    [Test]
    [TestCase("health", BarType.Health)]
    [TestCase("HP", BarType.Health)]
    [TestCase("mana", BarType.Mana)]
    [TestCase("MP", BarType.Mana)]
    [TestCase("experience", BarType.Experience)]
    [TestCase("XP", BarType.Experience)]
    [TestCase("stamina", BarType.Stamina)]
    [TestCase("energy", BarType.Stamina)]
    [TestCase("rage", BarType.Custom)]
    [TestCase("focus", BarType.Custom)]
    public void GetBarTypeForResource_ReturnsCorrectType(string resourceName, BarType expectedType)
    {
        // Act
        var barType = ResourceBar.GetBarTypeForResource(resourceName);

        // Assert
        barType.Should().Be(expectedType);
    }

    [Test]
    public void GetBarTypeForResource_UnknownResource_ReturnsCustom()
    {
        // Act
        var barType = ResourceBar.GetBarTypeForResource("UnknownStuff");

        // Assert
        barType.Should().Be(BarType.Custom);
    }

    #endregion
}
