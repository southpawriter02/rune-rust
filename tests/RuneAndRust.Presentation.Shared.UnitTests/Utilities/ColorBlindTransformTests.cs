// ═══════════════════════════════════════════════════════════════════════════════
// ColorBlindTransformTests.cs
// Unit tests for ColorBlindTransform utility.
// Version: 0.13.5f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.Utilities;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="ColorBlindTransform"/>.
/// </summary>
[TestFixture]
public class ColorBlindTransformTests
{
    private Mock<ILogger> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
    }

    // ═══════════════════════════════════════════════════════════════
    // NONE MODE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_WhenModeIsNone_ReturnsOriginalColor()
    {
        // Arrange
        var originalColor = new ThemeColor(255, 128, 64);

        // Act
        var result = ColorBlindTransform.Transform(originalColor, ColorBlindMode.None);

        // Assert
        result.Should().Be(originalColor);
    }

    [Test]
    public void Transform_WhenModeIsNone_DoesNotApplyMatrix()
    {
        // Arrange
        var redColor = new ThemeColor(255, 0, 0);

        // Act
        var result = ColorBlindTransform.Transform(redColor, ColorBlindMode.None, _mockLogger.Object);

        // Assert
        result.R.Should().Be(255);
        result.G.Should().Be(0);
        result.B.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // PROTANOPIA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_Protanopia_TransformsRedColor()
    {
        // Arrange
        var redColor = new ThemeColor(255, 0, 0);

        // Act
        var result = ColorBlindTransform.Transform(redColor, ColorBlindMode.Protanopia);

        // Assert
        // Protanopia dramatically reduces red perception
        result.R.Should().BeLessThan(255);
        result.G.Should().BeGreaterThan(0);
    }

    [Test]
    public void Transform_Protanopia_PreservesGrayscale()
    {
        // Arrange
        var grayColor = new ThemeColor(128, 128, 128);

        // Act
        var result = ColorBlindTransform.Transform(grayColor, ColorBlindMode.Protanopia);

        // Assert
        // Grayscale colors should remain relatively unchanged
        Math.Abs(result.R - result.G).Should().BeLessThan(20);
        Math.Abs(result.G - result.B).Should().BeLessThan(20);
    }

    // ═══════════════════════════════════════════════════════════════
    // DEUTERANOPIA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_Deuteranopia_TransformsGreenColor()
    {
        // Arrange
        var greenColor = new ThemeColor(0, 255, 0);

        // Act
        var result = ColorBlindTransform.Transform(greenColor, ColorBlindMode.Deuteranopia);

        // Assert
        // Deuteranopia affects green perception
        result.G.Should().BeLessThan(255);
    }

    [Test]
    public void Transform_Deuteranopia_PreservesWhite()
    {
        // Arrange
        var whiteColor = new ThemeColor(255, 255, 255);

        // Act
        var result = ColorBlindTransform.Transform(whiteColor, ColorBlindMode.Deuteranopia);

        // Assert
        // White should remain close to white
        result.R.Should().BeGreaterThan(240);
        result.G.Should().BeGreaterThan(240);
        result.B.Should().BeGreaterThan(240);
    }

    // ═══════════════════════════════════════════════════════════════
    // TRITANOPIA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_Tritanopia_TransformsBlueColor()
    {
        // Arrange
        var blueColor = new ThemeColor(0, 0, 255);

        // Act
        var result = ColorBlindTransform.Transform(blueColor, ColorBlindMode.Tritanopia);

        // Assert
        // Tritanopia affects blue/yellow perception
        result.B.Should().BeLessThan(255);
    }

    [Test]
    public void Transform_Tritanopia_PreservesBlack()
    {
        // Arrange
        var blackColor = new ThemeColor(0, 0, 0);

        // Act
        var result = ColorBlindTransform.Transform(blackColor, ColorBlindMode.Tritanopia);

        // Assert
        // Black should remain black
        result.R.Should().Be(0);
        result.G.Should().Be(0);
        result.B.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // ACHROMATOPSIA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_Achromatopsia_ConvertsToGrayscale()
    {
        // Arrange
        var coloredInput = new ThemeColor(255, 0, 0);

        // Act
        var result = ColorBlindTransform.Transform(coloredInput, ColorBlindMode.Achromatopsia);

        // Assert
        // Achromatopsia produces grayscale (R ≈ G ≈ B)
        Math.Abs(result.R - result.G).Should().BeLessThanOrEqualTo(5);
        Math.Abs(result.G - result.B).Should().BeLessThanOrEqualTo(5);
    }

    [Test]
    public void Transform_Achromatopsia_ProducesCorrectLuminance()
    {
        // Arrange
        var blueColor = new ThemeColor(0, 0, 255);

        // Act
        var result = ColorBlindTransform.Transform(blueColor, ColorBlindMode.Achromatopsia);

        // Assert
        // Blue has low luminance, so grayscale should be dark
        result.R.Should().BeLessThan(100);
    }

    // ═══════════════════════════════════════════════════════════════
    // EDGE CASE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Transform_ClampsValuesToValidRange()
    {
        // Arrange - use a color that might produce overflow
        var brightColor = new ThemeColor(255, 255, 255);

        // Act & Assert - should not throw and values should be in valid range
        foreach (ColorBlindMode mode in Enum.GetValues<ColorBlindMode>())
        {
            var result = ColorBlindTransform.Transform(brightColor, mode);
            result.R.Should().BeInRange((byte)0, (byte)255);
            result.G.Should().BeInRange((byte)0, (byte)255);
            result.B.Should().BeInRange((byte)0, (byte)255);
        }
    }

    [Test]
    public void Transform_WithLogger_DoesNotThrow()
    {
        // Arrange
        var color = new ThemeColor(100, 150, 200);

        // Act
        var act = () => ColorBlindTransform.Transform(color, ColorBlindMode.Protanopia, _mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void Transform_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        var color = new ThemeColor(100, 150, 200);

        // Act
        var act = () => ColorBlindTransform.Transform(color, ColorBlindMode.Protanopia, null);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    [TestCase(ColorBlindMode.None)]
    [TestCase(ColorBlindMode.Protanopia)]
    [TestCase(ColorBlindMode.Deuteranopia)]
    [TestCase(ColorBlindMode.Tritanopia)]
    [TestCase(ColorBlindMode.Achromatopsia)]
    public void Transform_AllModes_ProduceValidOutput(ColorBlindMode mode)
    {
        // Arrange
        var color = new ThemeColor(128, 64, 192);

        // Act
        var result = ColorBlindTransform.Transform(color, mode);

        // Assert
        result.R.Should().BeInRange((byte)0, (byte)255);
        result.G.Should().BeInRange((byte)0, (byte)255);
        result.B.Should().BeInRange((byte)0, (byte)255);
    }

    [Test]
    public void Transform_PreservesColorName_WhenModeIsNone()
    {
        // Arrange
        var namedColor = new ThemeColor(255, 0, 0, "Test Red");

        // Act
        var result = ColorBlindTransform.Transform(namedColor, ColorBlindMode.None);

        // Assert
        result.Name.Should().Be("Test Red");
    }
}
