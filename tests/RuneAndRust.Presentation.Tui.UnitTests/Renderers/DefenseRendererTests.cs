using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="DefenseRenderer"/>.
/// </summary>
[TestFixture]
public class DefenseRendererTests
{
    private DefenseRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new DefenseRenderer(null, NullLogger<DefenseRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // ACTION KEY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("block", 'B')]
    [TestCase("dodge", 'D')]
    [TestCase("parry", 'P')]
    [TestCase("counter", 'C')]
    public void GetActionKey_KnownTypes_ReturnsCorrectKey(string defenseType, char expectedKey)
    {
        // Arrange & Act
        var result = _renderer.GetActionKey(defenseType);

        // Assert
        result.Should().Be(expectedKey);
    }

    [Test]
    public void GetActionKey_UnknownType_ReturnsFirstLetter()
    {
        // Arrange & Act
        var result = _renderer.GetActionKey("unknown");

        // Assert
        result.Should().Be('U');
    }

    // ═══════════════════════════════════════════════════════════════
    // TIMING BAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatTimingBar_FullProgress_ReturnsFullBar()
    {
        // Arrange & Act
        var result = _renderer.FormatTimingBar(1.0f);

        // Assert
        result.Should().Be("[==========]");
    }

    [Test]
    public void FormatTimingBar_HalfProgress_ReturnsHalfBar()
    {
        // Arrange & Act
        var result = _renderer.FormatTimingBar(0.5f);

        // Assert
        result.Should().Be("[=====.....]");
    }

    [Test]
    public void FormatTimingBar_ZeroProgress_ReturnsEmptyBar()
    {
        // Arrange & Act
        var result = _renderer.FormatTimingBar(0.0f);

        // Assert
        result.Should().Be("[..........]");
    }

    [Test]
    public void GetTimingBarColor_CriticalTime_ReturnsRed()
    {
        // Arrange & Act
        var result = _renderer.GetTimingBarColor(0.2f);

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetTimingBarColor_WarningTime_ReturnsYellow()
    {
        // Arrange & Act
        var result = _renderer.GetTimingBarColor(0.4f);

        // Assert
        result.Should().Be(ConsoleColor.Yellow);
    }

    [Test]
    public void GetTimingBarColor_NormalTime_ReturnsGreen()
    {
        // Arrange & Act
        var result = _renderer.GetTimingBarColor(0.75f);

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }

    // ═══════════════════════════════════════════════════════════════
    // RESULT COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetResultColor_Success_ReturnsGreen()
    {
        // Arrange & Act
        var result = _renderer.GetResultColor(true);

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetResultColor_Failure_ReturnsRed()
    {
        // Arrange & Act
        var result = _renderer.GetResultColor(false);

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    // ═══════════════════════════════════════════════════════════════
    // DAMAGE REDUCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatDamageReduction_WithReduction_FormatsCorrectly()
    {
        // Arrange & Act
        var result = _renderer.FormatDamageReduction(12, 4);

        // Assert
        result.Should().Be("Damage reduced: 12 → 4 (-8)");
    }

    [Test]
    public void FormatDamageReduction_NoReduction_ShowsZero()
    {
        // Arrange & Act
        var result = _renderer.FormatDamageReduction(12, 12);

        // Assert
        result.Should().Be("Damage reduced: 12 → 12 (-0)");
    }

    // ═══════════════════════════════════════════════════════════════
    // INCOMING ATTACK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatIncomingAttack_WithAttack_FormatsCorrectly()
    {
        // Arrange
        var attack = new IncomingAttackDto(
            Guid.NewGuid(),
            "Goblin",
            "Slash",
            12,
            "physical");

        // Act
        var result = _renderer.FormatIncomingAttack(attack);

        // Assert
        result.Should().Be("INCOMING ATTACK: Goblin Slash (12 damage)");
    }
}
