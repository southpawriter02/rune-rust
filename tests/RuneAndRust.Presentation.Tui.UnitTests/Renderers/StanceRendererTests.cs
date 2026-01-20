using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="StanceRenderer"/>.
/// </summary>
[TestFixture]
public class StanceRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private StanceRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);

        _renderer = new StanceRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<StanceRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("aggressive", "⚔")]
    [TestCase("defensive", "🛡")]
    [TestCase("balanced", "⚖")]
    [TestCase("tactical", "🎯")]
    [TestCase("special", "✦")]
    public void GetStanceIcon_UnicodeSupported_ReturnsUnicodeIcon(string category, string expectedIcon)
    {
        // Arrange & Act
        var result = _renderer.GetStanceIcon(category);

        // Assert
        result.Should().Be(expectedIcon);
    }

    [Test]
    public void GetStanceIcon_UnicodeNotSupported_ReturnsAscii()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(false);
        var asciiRenderer = new StanceRenderer(
            _mockTerminal.Object,
            null,
            NullLogger<StanceRenderer>.Instance);

        // Act
        var result = asciiRenderer.GetStanceIcon("aggressive");

        // Assert
        result.Should().Be("[A]");
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("aggressive", ConsoleColor.Red)]
    [TestCase("defensive", ConsoleColor.Blue)]
    [TestCase("balanced", ConsoleColor.Yellow)]
    [TestCase("tactical", ConsoleColor.Cyan)]
    [TestCase("special", ConsoleColor.Magenta)]
    public void GetStanceColor_KnownCategories_ReturnsCorrectColor(string category, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var result = _renderer.GetStanceColor(category);

        // Assert
        result.Should().Be(expectedColor);
    }

    // ═══════════════════════════════════════════════════════════════
    // MODIFIER FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatModifier_PositiveValue_IncludesPlusSign()
    {
        // Arrange & Act
        var result = _renderer.FormatModifier("Attack", 2);

        // Assert
        result.Should().Be("+2 Attack");
    }

    [Test]
    public void FormatModifier_NegativeValue_IncludesMinusSign()
    {
        // Arrange & Act
        var result = _renderer.FormatModifier("Defense", -1);

        // Assert
        result.Should().Be("-1 Defense");
    }

    [Test]
    public void FormatModifier_Percentage_IncludesPercentSign()
    {
        // Arrange & Act
        var result = _renderer.FormatModifier("Critical", 10, isPercentage: true);

        // Assert
        result.Should().Be("+10% Critical");
    }

    // ═══════════════════════════════════════════════════════════════
    // MODIFIER COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetModifierColor_Positive_ReturnsGreen()
    {
        // Arrange & Act
        var result = _renderer.GetModifierColor(true);

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetModifierColor_Negative_ReturnsRed()
    {
        // Arrange & Act
        var result = _renderer.GetModifierColor(false);

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    // ═══════════════════════════════════════════════════════════════
    // SWITCH COMPARISON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatSwitchComparison_WithChanges_FormatsCorrectly()
    {
        // Arrange
        var fromModifiers = new List<ModifierDisplayDto>
        {
            new("Attack", 2, true, "+2 Attack"),
            new("Defense", -1, false, "-1 Defense")
        };

        var toModifiers = new List<ModifierDisplayDto>
        {
            new("Attack", -2, false, "-2 Attack"),
            new("Defense", 3, true, "+3 Defense"),
            new("BlockChance", 20, true, "+20% BlockChance")
        };

        var switchDto = new StanceSwitchDto(
            "Aggressive",
            "Defensive",
            fromModifiers,
            toModifiers);

        // Act
        var result = _renderer.FormatSwitchComparison(switchDto);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(l => l.Contains("-2 Attack") && l.Contains("was +2"));
        result.Should().Contain(l => l.Contains("+3 Defense") && l.Contains("was -1"));
        result.Should().Contain(l => l.Contains("+20% BlockChance") && l.Contains("(new)"));
    }
}
