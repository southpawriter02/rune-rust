// ═══════════════════════════════════════════════════════════════════════════════
// ValidationUtilitiesTests.cs
// Unit tests for ValidationUtilities.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="ValidationUtilities"/>.
/// </summary>
[TestFixture]
public class ValidationUtilitiesTests
{
    // ═══════════════════════════════════════════════════════════════
    // VALIDATE PERCENTAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0.0, true)]
    [TestCase(0.5, true)]
    [TestCase(1.0, true)]
    [TestCase(-0.1, false)]
    [TestCase(1.1, false)]
    public void ValidatePercentage_ReturnsExpectedResult(double percentage, bool expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ValidatePercentage(percentage);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATE CURRENT MAX TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(50, 100, true)]
    [TestCase(100, 100, true)]
    [TestCase(0, 100, true)]
    [TestCase(150, 100, false)]  // current > max
    [TestCase(-1, 100, false)]   // negative current
    [TestCase(50, -1, false)]    // negative max
    public void ValidateCurrentMax_ReturnsExpectedResult(int current, int max, bool expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ValidateCurrentMax(current, max);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // IS IN RANGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(5, 0, 10, true)]
    [TestCase(0, 0, 10, true)]
    [TestCase(10, 0, 10, true)]
    [TestCase(-1, 0, 10, false)]
    [TestCase(11, 0, 10, false)]
    public void IsInRange_Int_ReturnsExpectedResult(int value, int min, int max, bool expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.IsInRange(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsInRange_Double_ValidatesCorrectly()
    {
        // Arrange & Act & Assert
        ValidationUtilities.IsInRange(0.5, 0.0, 1.0).Should().BeTrue();
        ValidationUtilities.IsInRange(1.5, 0.0, 1.0).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLAMP VALUE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(50, 0, 100, 50)]   // within range
    [TestCase(-10, 0, 100, 0)]   // below min
    [TestCase(150, 0, 100, 100)] // above max
    public void ClampValue_Int_ClampsCorrectly(int value, int min, int max, int expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ClampValue(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ClampValue_Double_ClampsCorrectly()
    {
        // Arrange & Act & Assert
        ValidationUtilities.ClampValue(0.5, 0.0, 1.0).Should().Be(0.5);
        ValidationUtilities.ClampValue(-0.1, 0.0, 1.0).Should().Be(0.0);
        ValidationUtilities.ClampValue(1.5, 0.0, 1.0).Should().Be(1.0);
    }

    [Test]
    [TestCase(0.5, 0.5)]
    [TestCase(-0.1, 0.0)]
    [TestCase(1.5, 1.0)]
    public void ClampPercentage_ClampsToZeroOne(double value, double expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ClampPercentage(value);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // NORMALIZE VALUE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(50, 0, 100, 0.5)]
    [TestCase(0, 0, 100, 0.0)]
    [TestCase(100, 0, 100, 1.0)]
    [TestCase(150, 0, 100, 1.0)]   // clamped
    [TestCase(-10, 0, 100, 0.0)]   // clamped
    public void NormalizeValue_Int_NormalizesCorrectly(
        int value, int min, int max, double expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.NormalizeValue(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void NormalizeValue_WhenMaxEqualsMin_ReturnsZero()
    {
        // Arrange & Act
        var result = ValidationUtilities.NormalizeValue(50, 100, 100);

        // Assert
        result.Should().Be(0.0);
    }

    [Test]
    public void NormalizeValue_Double_NormalizesCorrectly()
    {
        // Arrange & Act
        var result = ValidationUtilities.NormalizeValue(25.0, 0.0, 50.0);

        // Assert
        result.Should().Be(0.5);
    }

    // ═══════════════════════════════════════════════════════════════
    // DENORMALIZE VALUE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0.5, 0, 100, 50)]
    [TestCase(0.0, 0, 100, 0)]
    [TestCase(1.0, 0, 100, 100)]
    [TestCase(0.33, 50, 80, 60)]  // rounded: 50 + 0.33 * 30 = 59.9 → 60
    public void DenormalizeValue_DenormalizesCorrectly(
        double normalized, int min, int max, int expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.DenormalizeValue(normalized, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void DenormalizeValue_WithOutOfRangeNormalized_ClampsFirst()
    {
        // Arrange & Act
        var resultAbove = ValidationUtilities.DenormalizeValue(1.5, 0, 100);
        var resultBelow = ValidationUtilities.DenormalizeValue(-0.5, 0, 100);

        // Assert
        resultAbove.Should().Be(100);  // clamped to 1.0 first
        resultBelow.Should().Be(0);    // clamped to 0.0 first
    }

    // ═══════════════════════════════════════════════════════════════
    // GRID VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(8, 8, true)]
    [TestCase(1, 1, true)]
    [TestCase(0, 8, false)]
    [TestCase(8, 0, false)]
    [TestCase(-1, 8, false)]
    public void ValidateGridDimensions_ReturnsExpectedResult(
        int width, int height, bool expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ValidateGridDimensions(width, height);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(0, 0, 8, 8, true)]
    [TestCase(7, 7, 8, 8, true)]
    [TestCase(-1, 0, 8, 8, false)]
    [TestCase(8, 0, 8, 8, false)]
    public void ValidateGridPosition_ReturnsExpectedResult(
        int x, int y, int width, int height, bool expected)
    {
        // Arrange & Act
        var result = ValidationUtilities.ValidateGridPosition(x, y, width, height);

        // Assert
        result.Should().Be(expected);
    }
}
