// ═══════════════════════════════════════════════════════════════════════════════
// FormatUtilitiesTests.cs
// Unit tests for FormatUtilities.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="FormatUtilities"/>.
/// </summary>
[TestFixture]
public class FormatUtilitiesTests
{
    // ═══════════════════════════════════════════════════════════════
    // FORMAT PERCENTAGE (INT, INT) TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(50, 100, "50%")]
    [TestCase(0, 100, "0%")]
    [TestCase(100, 100, "100%")]
    [TestCase(75, 100, "75%")]
    public void FormatPercentage_WithCurrentMax_ReturnsExpectedPercentage(
        int current, int max, string expected)
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(current, max);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FormatPercentage_WhenMaxIsZero_ReturnsZeroPercent()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(50, 0);

        // Assert
        result.Should().Be("0%");
    }

    [Test]
    public void FormatPercentage_WhenCurrentExceedsMax_ClampsTo100Percent()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(150, 100);

        // Assert
        result.Should().Be("100%");
    }

    [Test]
    public void FormatPercentage_WhenCurrentIsNegative_ClampsToZeroPercent()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(-10, 100);

        // Assert
        result.Should().Be("0%");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT PERCENTAGE (DOUBLE) TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0.5, "50%")]
    [TestCase(0.0, "0%")]
    [TestCase(1.0, "100%")]
    [TestCase(0.75, "75%")]
    public void FormatPercentage_WithDouble_ReturnsExpectedPercentage(
        double percentage, string expected)
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(percentage);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FormatPercentage_WithDoubleExceeding1_ClampsTo100Percent()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatPercentage(1.5);

        // Assert
        result.Should().Be("100%");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT DURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatDuration_WhenLessThanOneSecond_ReturnsLessThanOneSecond()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(500);

        // Act
        var result = FormatUtilities.FormatDuration(duration);

        // Assert
        result.Should().Be("<1s");
    }

    [Test]
    public void FormatDuration_WhenSecondsOnly_ReturnsSecondsFormat()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(45);

        // Act
        var result = FormatUtilities.FormatDuration(duration);

        // Assert
        result.Should().Be("45s");
    }

    [Test]
    public void FormatDuration_WhenMinutesAndSeconds_ReturnsMinutesSecondsFormat()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(150); // 2m 30s

        // Act
        var result = FormatUtilities.FormatDuration(duration);

        // Assert
        result.Should().Be("2m 30s");
    }

    [Test]
    public void FormatDuration_WhenHoursAndMinutes_ReturnsHoursMinutesFormat()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(90); // 1h 30m

        // Act
        var result = FormatUtilities.FormatDuration(duration);

        // Assert
        result.Should().Be("1h 30m");
    }

    [Test]
    public void FormatDuration_WhenCompactFalse_ReturnsVerboseFormat()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(45);

        // Act
        var result = FormatUtilities.FormatDuration(duration, compact: false);

        // Assert
        result.Should().Be("45 seconds");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT NUMBER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatNumber_WithSmallNumber_ReturnsUnformattedNumber()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatNumber(42);

        // Assert
        result.Should().Be("42");
    }

    [Test]
    public void FormatNumber_WithLargeNumber_ReturnsFormattedWithSeparators()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatNumber(1234567);

        // Assert
        result.Should().Be("1,234,567");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT COMPACT NUMBER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(123L, "123")]
    [TestCase(1234L, "1.2K")]
    [TestCase(1234567L, "1.2M")]
    [TestCase(1234567890L, "1.2B")]
    public void FormatCompactNumber_ReturnsExpectedFormat(long value, string expected)
    {
        // Arrange & Act
        var result = FormatUtilities.FormatCompactNumber(value);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT DELTA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatDelta_WhenPositive_ReturnsWithPlusSign()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatDelta(10);

        // Assert
        result.Should().Be("+10");
    }

    [Test]
    public void FormatDelta_WhenNegative_ReturnsWithMinusSign()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatDelta(-10);

        // Assert
        result.Should().Be("-10");
    }

    [Test]
    public void FormatDelta_WhenZero_ReturnsWithPlusSign()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatDelta(0);

        // Assert
        result.Should().Be("+0");
    }

    [Test]
    public void FormatDelta_WithOldAndNewValues_CalculatesDelta()
    {
        // Arrange & Act
        var result = FormatUtilities.FormatDelta(50, 60);

        // Assert
        result.Should().Be("+10");
    }

    // ═══════════════════════════════════════════════════════════════
    // TRUNCATE TEXT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TruncateText_WhenTextFits_ReturnsUnchanged()
    {
        // Arrange & Act
        var result = FormatUtilities.TruncateText("Hello", 10);

        // Assert
        result.Should().Be("Hello");
    }

    [Test]
    public void TruncateText_WhenTextTooLong_TruncatesWithEllipsis()
    {
        // Arrange & Act
        var result = FormatUtilities.TruncateText("Hello World", 8);

        // Assert
        result.Should().Be("Hello...");
        result.Should().HaveLength(8);
    }

    [Test]
    public void TruncateText_WhenNullOrEmpty_ReturnsEmpty()
    {
        // Arrange & Act
        var resultNull = FormatUtilities.TruncateText(null!, 10);
        var resultEmpty = FormatUtilities.TruncateText("", 10);

        // Assert
        resultNull.Should().BeEmpty();
        resultEmpty.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // CENTER TEXT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CenterText_CentersTextWithPadding()
    {
        // Arrange & Act
        var result = FormatUtilities.CenterText("Hi", 10);

        // Assert
        result.Should().Be("    Hi    ");
        result.Should().HaveLength(10);
    }

    [Test]
    public void CenterText_WhenTextFillsWidth_ReturnsUnchanged()
    {
        // Arrange & Act
        var result = FormatUtilities.CenterText("Hello World", 5);

        // Assert
        result.Should().Be("Hello World");
    }
}
