using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TierColorDefinition"/> value object.
/// </summary>
[TestFixture]
public class TierColorDefinitionTests
{
    /// <summary>
    /// Verifies that FromHex creates a valid color definition from a hex string.
    /// </summary>
    [Test]
    public void FromHex_WithValidHexColor_CreatesColorDefinition()
    {
        // Act
        var color = TierColorDefinition.FromHex("#FF5500", ConsoleColor.Red);

        // Assert
        color.HexColor.Should().Be("#FF5500");
        color.ConsoleColor.Should().Be(ConsoleColor.Red);
        color.RgbRed.Should().Be(255);
        color.RgbGreen.Should().Be(85);
        color.RgbBlue.Should().Be(0);
    }

    /// <summary>
    /// Verifies that FromHex handles hex colors without the # prefix.
    /// </summary>
    [Test]
    public void FromHex_WithoutHashPrefix_AddsHashPrefix()
    {
        // Act
        var color = TierColorDefinition.FromHex("00FF00", ConsoleColor.Green);

        // Assert
        color.HexColor.Should().Be("#00FF00");
        color.RgbRed.Should().Be(0);
        color.RgbGreen.Should().Be(255);
        color.RgbBlue.Should().Be(0);
    }

    /// <summary>
    /// Verifies that FromHex throws for invalid hex formats.
    /// </summary>
    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("#FFF")]
    [TestCase("#FFFFFFF")]
    [TestCase("#GGGGGG")]
    [TestCase("invalid")]
    public void FromHex_WithInvalidHexColor_ThrowsArgumentException(string invalidHex)
    {
        // Act
        var act = () => TierColorDefinition.FromHex(invalidHex, ConsoleColor.White);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that FromHex throws for null input.
    /// </summary>
    [Test]
    public void FromHex_WithNull_ThrowsArgumentException()
    {
        // Act
        var act = () => TierColorDefinition.FromHex(null!, ConsoleColor.White);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that FromRgb creates a valid color definition from RGB components.
    /// </summary>
    [Test]
    public void FromRgb_WithValidComponents_CreatesColorDefinition()
    {
        // Act
        var color = TierColorDefinition.FromRgb(128, 0, 128, ConsoleColor.Magenta);

        // Assert
        color.HexColor.Should().Be("#800080");
        color.ConsoleColor.Should().Be(ConsoleColor.Magenta);
        color.RgbRed.Should().Be(128);
        color.RgbGreen.Should().Be(0);
        color.RgbBlue.Should().Be(128);
    }

    /// <summary>
    /// Verifies that RgbTuple returns correct component values.
    /// </summary>
    [Test]
    public void RgbTuple_ReturnsCorrectComponents()
    {
        // Arrange
        var color = TierColorDefinition.FromHex("#FFD700", ConsoleColor.Yellow);

        // Act
        var (r, g, b) = color.RgbTuple;

        // Assert
        r.Should().Be(255);
        g.Should().Be(215);
        b.Should().Be(0);
    }

    /// <summary>
    /// Verifies that HexColorRaw returns the hex value without the # prefix.
    /// </summary>
    [Test]
    public void HexColorRaw_ReturnsHexWithoutHash()
    {
        // Arrange
        var color = TierColorDefinition.FromHex("#808080", ConsoleColor.Gray);

        // Assert
        color.HexColorRaw.Should().Be("808080");
    }

    /// <summary>
    /// Verifies that default tier colors match the specification.
    /// </summary>
    [Test]
    [TestCase("#808080", ConsoleColor.Gray, nameof(TierColorDefinition.JuryRiggedDefault))]
    [TestCase("#FFFFFF", ConsoleColor.White, nameof(TierColorDefinition.ScavengedDefault))]
    [TestCase("#00FF00", ConsoleColor.Green, nameof(TierColorDefinition.ClanForgedDefault))]
    [TestCase("#800080", ConsoleColor.Magenta, nameof(TierColorDefinition.OptimizedDefault))]
    [TestCase("#FFD700", ConsoleColor.Yellow, nameof(TierColorDefinition.MythForgedDefault))]
    public void DefaultColors_MatchSpecification(string expectedHex, ConsoleColor expectedConsoleColor, string propertyName)
    {
        // Arrange
        var defaultColor = propertyName switch
        {
            nameof(TierColorDefinition.JuryRiggedDefault) => TierColorDefinition.JuryRiggedDefault,
            nameof(TierColorDefinition.ScavengedDefault) => TierColorDefinition.ScavengedDefault,
            nameof(TierColorDefinition.ClanForgedDefault) => TierColorDefinition.ClanForgedDefault,
            nameof(TierColorDefinition.OptimizedDefault) => TierColorDefinition.OptimizedDefault,
            nameof(TierColorDefinition.MythForgedDefault) => TierColorDefinition.MythForgedDefault,
            _ => throw new ArgumentException($"Unknown property: {propertyName}")
        };

        // Assert
        defaultColor.HexColor.Should().Be(expectedHex);
        defaultColor.ConsoleColor.Should().Be(expectedConsoleColor);
    }

    /// <summary>
    /// Verifies that ToString returns a useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var color = TierColorDefinition.FromHex("#00FF00", ConsoleColor.Green);

        // Act
        var result = color.ToString();

        // Assert
        result.Should().Contain("#00FF00");
        result.Should().Contain("Green");
    }
}
