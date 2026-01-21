using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for the <see cref="ThemeColor"/> value object.
/// </summary>
[TestFixture]
public class ThemeColorTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FromHex Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FromHex_WithValidHexAndHash_CreatesCorrectColor()
    {
        // Arrange
        const string hex = "#FF5500";
        const string name = "Orange";

        // Act
        var color = ThemeColor.FromHex(hex, name);

        // Assert
        color.R.Should().Be(255);
        color.G.Should().Be(85);
        color.B.Should().Be(0);
        color.Name.Should().Be("Orange");
    }

    [Test]
    public void FromHex_WithValidHexWithoutHash_CreatesCorrectColor()
    {
        // Arrange
        const string hex = "228B22";
        const string name = "Forest Green";

        // Act
        var color = ThemeColor.FromHex(hex, name);

        // Assert
        color.R.Should().Be(34);
        color.G.Should().Be(139);
        color.B.Should().Be(34);
        color.Name.Should().Be("Forest Green");
    }

    [Test]
    public void FromHex_WithInvalidHexLength_ThrowsArgumentException()
    {
        // Arrange
        const string hex = "#FF55"; // Too short

        // Act
        var act = () => ThemeColor.FromHex(hex);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*6 characters*");
    }

    [Test]
    public void FromHex_WithNullOrEmpty_ThrowsArgumentException()
    {
        // Act
        var actNull = () => ThemeColor.FromHex(null!);
        var actEmpty = () => ThemeColor.FromHex("");

        // Assert
        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Hex Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Hex_ReturnsCorrectFormat()
    {
        // Arrange
        var color = new ThemeColor(255, 128, 0);

        // Act
        var hex = color.Hex;

        // Assert
        hex.Should().Be("#FF8000");
    }

    [Test]
    public void Hex_WithLowValues_PadsWithZeros()
    {
        // Arrange
        var color = new ThemeColor(0, 0, 15);

        // Act
        var hex = color.Hex;

        // Assert
        hex.Should().Be("#00000F");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_WithName_ReturnsNameAndHex()
    {
        // Arrange
        var color = new ThemeColor(255, 0, 0, "Pure Red");

        // Act
        var result = color.ToString();

        // Assert
        result.Should().Be("Pure Red (#FF0000)");
    }

    [Test]
    public void ToString_WithoutName_ReturnsHexOnly()
    {
        // Arrange
        var color = new ThemeColor(0, 255, 0);

        // Act
        var result = color.ToString();

        // Assert
        result.Should().Be("#00FF00");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Color Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void StaticColors_HaveCorrectValues()
    {
        // Assert
        ThemeColor.Black.R.Should().Be(0);
        ThemeColor.Black.G.Should().Be(0);
        ThemeColor.Black.B.Should().Be(0);

        ThemeColor.White.R.Should().Be(255);
        ThemeColor.White.G.Should().Be(255);
        ThemeColor.White.B.Should().Be(255);

        ThemeColor.Fallback.R.Should().Be(255);
        ThemeColor.Fallback.G.Should().Be(0);
        ThemeColor.Fallback.B.Should().Be(255);
    }
}
