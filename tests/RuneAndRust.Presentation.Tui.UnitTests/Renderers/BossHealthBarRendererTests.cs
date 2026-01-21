using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="BossHealthBarRenderer"/>.
/// </summary>
[TestFixture]
public class BossHealthBarRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private BossHealthBarRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);

        _renderer = new BossHealthBarRenderer(
            null,
            _mockTerminal.Object,
            NullLogger<BossHealthBarRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT HEALTH TEXT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatHealthText_WithValidValues_ReturnsFormattedString()
    {
        // Arrange
        var current = 2847;
        var max = 5000;

        // Act
        var result = _renderer.FormatHealthText(current, max);

        // Assert
        result.Should().Be("HP: 2,847 / 5,000");
    }

    [Test]
    public void FormatHealthText_WithSmallValues_FormatsCorrectly()
    {
        // Arrange
        var current = 100;
        var max = 100;

        // Act
        var result = _renderer.FormatHealthText(current, max);

        // Assert
        result.Should().Be("HP: 100 / 100");
    }

    [Test]
    public void FormatHealthText_WithLargeValues_HasThousandsSeparators()
    {
        // Arrange
        var current = 12500;
        var max = 25000;

        // Act
        var result = _renderer.FormatHealthText(current, max);

        // Assert
        result.Should().Be("HP: 12,500 / 25,000");
    }

    [Test]
    public void FormatHealthText_WithZeroCurrent_FormatsCorrectly()
    {
        // Arrange
        var current = 0;
        var max = 5000;

        // Act
        var result = _renderer.FormatHealthText(current, max);

        // Assert
        result.Should().Be("HP: 0 / 5,000");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET HEALTH COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(100, ConsoleColor.Green)]
    [TestCase(76, ConsoleColor.Green)]
    [TestCase(75, ConsoleColor.Yellow)]
    [TestCase(51, ConsoleColor.Yellow)]
    [TestCase(50, ConsoleColor.DarkYellow)]
    [TestCase(26, ConsoleColor.DarkYellow)]
    [TestCase(25, ConsoleColor.Red)]
    [TestCase(1, ConsoleColor.Red)]
    [TestCase(0, ConsoleColor.DarkRed)]
    public void GetHealthColor_WithHealthPercent_ReturnsCorrectColor(int healthPercent, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var result = _renderer.GetHealthColor(healthPercent);

        // Assert
        result.Should().Be(expectedColor);
    }

    [Test]
    public void GetHealthColor_AtBoundary76Percent_ReturnsGreen()
    {
        // Arrange & Act
        var result = _renderer.GetHealthColor(76);

        // Assert - Just above 75% threshold
        result.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetHealthColor_AtBoundary75Percent_ReturnsYellow()
    {
        // Arrange & Act
        var result = _renderer.GetHealthColor(75);

        // Assert - At 75% threshold, should be yellow (not above green threshold)
        result.Should().Be(ConsoleColor.Yellow);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT BOSS NAME HEADER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatBossNameHeader_WithName_ReturnsCenteredDecoratedHeader()
    {
        // Arrange
        var bossName = "Skeleton King";
        var totalWidth = 60;

        // Act
        var result = _renderer.FormatBossNameHeader(bossName, totalWidth);

        // Assert
        result.Should().Contain("<<  SKELETON KING  >>");
        result.Length.Should().Be(totalWidth);
    }

    [Test]
    public void FormatBossNameHeader_ConvertsToUppercase()
    {
        // Arrange
        var bossName = "dragon lord";
        var totalWidth = 60;

        // Act
        var result = _renderer.FormatBossNameHeader(bossName, totalWidth);

        // Assert
        result.Should().Contain("DRAGON LORD");
        result.Should().NotContain("dragon lord");
    }

    [Test]
    public void FormatBossNameHeader_CentersContent()
    {
        // Arrange
        var bossName = "Boss";
        var totalWidth = 40;

        // Act
        var result = _renderer.FormatBossNameHeader(bossName, totalWidth);

        // Assert
        var trimmedResult = result.Trim();
        var leftPadding = result.IndexOf('<');
        var rightPadding = result.Length - result.LastIndexOf('>') - 1;
        
        // Padding should be roughly equal (within 1 character for odd widths)
        Math.Abs(leftPadding - rightPadding).Should().BeLessOrEqualTo(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT BAR BORDER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatBarBorder_WithWidth_ReturnsCorrectFormat()
    {
        // Arrange
        var barWidth = 10;

        // Act
        var result = _renderer.FormatBarBorder(barWidth);

        // Assert
        result.Should().Be("+==========+");
        result.Length.Should().Be(barWidth + 2); // +2 for corner characters
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT DAMAGE DELTA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatDamageDelta_PositiveDamage_ReturnsNegativeFormat()
    {
        // Arrange
        var damage = 150;

        // Act
        var result = _renderer.FormatDamageDelta(damage);

        // Assert
        result.Should().Be("[-150]");
    }

    [Test]
    public void FormatDamageDelta_NegativeDamage_ReturnsPositiveFormat()
    {
        // Arrange - negative damage = healing
        var healing = -50;

        // Act
        var result = _renderer.FormatDamageDelta(healing);

        // Assert
        result.Should().Be("[+50]");
    }

    [Test]
    public void FormatDamageDelta_Zero_ReturnsEmpty()
    {
        // Arrange
        var noDamage = 0;

        // Act
        var result = _renderer.FormatDamageDelta(noDamage);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void FormatDamageDelta_LargeDamage_HasThousandsSeparators()
    {
        // Arrange
        var damage = 12500;

        // Act
        var result = _renderer.FormatDamageDelta(damage);

        // Assert
        result.Should().Be("[-12,500]");
    }

    // ═══════════════════════════════════════════════════════════════
    // CALCULATE FILL WIDTH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(100, 60, 60)]
    [TestCase(75, 60, 45)]
    [TestCase(50, 60, 30)]
    [TestCase(25, 60, 15)]
    [TestCase(0, 60, 0)]
    public void CalculateFillWidth_WithPercentage_ReturnsCorrectWidth(
        int healthPercent, int barWidth, int expectedFillWidth)
    {
        // Arrange & Act
        var result = _renderer.CalculateFillWidth(healthPercent, barWidth);

        // Assert
        result.Should().Be(expectedFillWidth);
    }

    [Test]
    public void CalculateFillWidth_ClampsBelowZero()
    {
        // Arrange
        var negativepercent = -10;
        var barWidth = 60;

        // Act
        var result = _renderer.CalculateFillWidth(negativepercent, barWidth);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void CalculateFillWidth_ClampsAbove100()
    {
        // Arrange
        var overPercent = 150;
        var barWidth = 60;

        // Act
        var result = _renderer.CalculateFillWidth(overPercent, barWidth);

        // Assert
        result.Should().Be(60);
    }

    // ═══════════════════════════════════════════════════════════════
    // CALCULATE MARKER POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(75, 0, 60, 45)]
    [TestCase(50, 0, 60, 30)]
    [TestCase(25, 0, 60, 15)]
    [TestCase(66, 0, 60, 39)]
    [TestCase(33, 0, 60, 19)]
    public void CalculateMarkerPosition_WithThreshold_ReturnsCorrectPosition(
        int thresholdPercent, int barStartX, int barWidth, int expectedPosition)
    {
        // Arrange & Act
        var result = _renderer.CalculateMarkerPosition(thresholdPercent, barStartX, barWidth);

        // Assert
        result.Should().Be(expectedPosition);
    }

    [Test]
    public void CalculateMarkerPosition_WithOffset_AddsBarStartX()
    {
        // Arrange
        var threshold = 50;
        var barStartX = 10;
        var barWidth = 60;

        // Act
        var result = _renderer.CalculateMarkerPosition(threshold, barStartX, barWidth);

        // Assert
        result.Should().Be(40); // 10 + (60 * 0.5) = 40
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT PHASE LABEL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatPhaseLabel_ReturnsMultilineFormat()
    {
        // Arrange
        var phaseNumber = 2;
        var threshold = 75;

        // Act
        var result = _renderer.FormatPhaseLabel(phaseNumber, threshold);

        // Assert
        result.Should().Contain("Phase 2");
        result.Should().Contain("(75%)");
        result.Should().Contain("\n");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET FLASH COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetDamageFlashColor_ReturnsConfiguredColor()
    {
        // Arrange & Act
        var result = _renderer.GetDamageFlashColor();

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetHealingFlashColor_ReturnsConfiguredColor()
    {
        // Arrange & Act
        var result = _renderer.GetHealingFlashColor();

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }
}
