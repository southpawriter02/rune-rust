// ═══════════════════════════════════════════════════════════════════════════════
// IconUtilitiesTests.cs
// Unit tests for IconUtilities.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="IconUtilities"/>.
/// </summary>
[TestFixture]
public class IconUtilitiesTests
{
    // ═══════════════════════════════════════════════════════════════
    // DIRECTION ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(Direction.North, "↑")]
    [TestCase(Direction.East, "→")]
    [TestCase(Direction.South, "↓")]
    [TestCase(Direction.West, "←")]
    [TestCase(Direction.NorthEast, "↗")]
    [TestCase(Direction.SouthEast, "↘")]
    [TestCase(Direction.SouthWest, "↙")]
    [TestCase(Direction.NorthWest, "↖")]
    [TestCase(Direction.None, "·")]
    public void GetDirectionIcon_WithUnicode_ReturnsCorrectArrow(
        Direction direction, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDirectionIcon(direction);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(Direction.North, "^")]
    [TestCase(Direction.East, ">")]
    [TestCase(Direction.South, "v")]
    [TestCase(Direction.West, "<")]
    [TestCase(Direction.None, ".")]
    public void GetDirectionIcon_WithAscii_ReturnsCorrectChar(
        Direction direction, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDirectionIcon(direction, useUnicode: false);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // DAMAGE TYPE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase("physical", "⚔")]
    [TestCase("fire", "🔥")]
    [TestCase("ice", "❄")]
    [TestCase("lightning", "⚡")]
    [TestCase("poison", "☠")]
    [TestCase("healing", "💚")]
    [TestCase("arcane", "✨")]
    [TestCase("holy", "☀")]
    [TestCase("shadow", "🌑")]
    [TestCase("nature", "🌿")]
    public void GetDamageTypeIcon_WithUnicode_ReturnsCorrectIcon(
        string damageTypeId, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDamageTypeIcon(damageTypeId);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase("physical", "[P]")]
    [TestCase("fire", "[F]")]
    [TestCase("ice", "[I]")]
    [TestCase("lightning", "[L]")]
    [TestCase("poison", "[X]")]
    [TestCase("healing", "[H]")]
    [TestCase("arcane", "[A]")]
    [TestCase("holy", "[O]")]
    [TestCase("shadow", "[S]")]
    [TestCase("nature", "[N]")]
    public void GetDamageTypeIcon_WithAscii_ReturnsCorrectBrackets(
        string damageTypeId, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDamageTypeIcon(damageTypeId, useUnicode: false);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase("FIRE", "🔥")]
    [TestCase("Fire", "🔥")]
    [TestCase("PHYSICAL", "⚔")]
    public void GetDamageTypeIcon_CaseInsensitive_ReturnsCorrectIcon(
        string damageTypeId, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDamageTypeIcon(damageTypeId);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void GetDamageTypeIcon_WhenUnknownType_ReturnsDefaultIcon()
    {
        // Arrange & Act
        var unicodeResult = IconUtilities.GetDamageTypeIcon("unknown");
        var asciiResult = IconUtilities.GetDamageTypeIcon("unknown", useUnicode: false);

        // Assert
        unicodeResult.Should().Be("✦");
        asciiResult.Should().Be("[?]");
    }

    [Test]
    public void GetDamageTypeIcon_WhenNull_ReturnsDefaultIcon()
    {
        // Arrange & Act
        var result = IconUtilities.GetDamageTypeIcon(null!);

        // Assert
        result.Should().Be("✦");
    }

    // ═══════════════════════════════════════════════════════════════
    // DIE FACE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(1, "⚀")]
    [TestCase(2, "⚁")]
    [TestCase(3, "⚂")]
    [TestCase(4, "⚃")]
    [TestCase(5, "⚄")]
    [TestCase(6, "⚅")]
    public void GetDieFaceIcon_WithUnicode_ReturnsCorrectDieSymbol(
        int value, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDieFaceIcon(value);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void GetDieFaceIcon_WhenValueOutOfD6Range_ReturnsBracketedNumber()
    {
        // Arrange & Act
        var result = IconUtilities.GetDieFaceIcon(8);

        // Assert
        result.Should().Be("[8]");
    }

    [Test]
    public void GetDieFaceIcon_WithAscii_ReturnsBracketedNumber()
    {
        // Arrange & Act
        var result = IconUtilities.GetDieFaceIcon(3, useUnicode: false);

        // Assert
        result.Should().Be("[3]");
    }

    // ═══════════════════════════════════════════════════════════════
    // DICE TYPE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(DiceType.D20, "🎲₂₀")]
    [TestCase(DiceType.D6, "🎲₆")]
    [TestCase(DiceType.D100, "🎲₁₀₀")]
    public void GetDiceIcon_WithUnicode_ReturnsCorrectIcon(
        DiceType diceType, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDiceIcon(diceType);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(DiceType.D20, "d20")]
    [TestCase(DiceType.D6, "d6")]
    [TestCase(DiceType.D100, "d100")]
    public void GetDiceIcon_WithAscii_ReturnsCorrectFormat(
        DiceType diceType, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetDiceIcon(diceType, useUnicode: false);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // STATUS ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(StatusType.Success, "✓")]
    [TestCase(StatusType.Failure, "✗")]
    [TestCase(StatusType.Warning, "⚠")]
    [TestCase(StatusType.Info, "ℹ")]
    [TestCase(StatusType.Pending, "◌")]
    [TestCase(StatusType.InProgress, "◐")]
    public void GetStatusIcon_WithUnicode_ReturnsCorrectSymbol(
        StatusType status, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetStatusIcon(status);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(StatusType.Success, "[OK]")]
    [TestCase(StatusType.Failure, "[X]")]
    [TestCase(StatusType.Warning, "[!]")]
    [TestCase(StatusType.Info, "[i]")]
    [TestCase(StatusType.Pending, "[ ]")]
    [TestCase(StatusType.InProgress, "[.]")]
    public void GetStatusIcon_WithAscii_ReturnsCorrectBrackets(
        StatusType status, string expected)
    {
        // Arrange & Act
        var result = IconUtilities.GetStatusIcon(status, useUnicode: false);

        // Assert
        result.Should().Be(expected);
    }
}
