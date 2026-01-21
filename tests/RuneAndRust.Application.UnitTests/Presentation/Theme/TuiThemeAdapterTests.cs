using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.ValueObjects;
using RuneAndRust.Presentation.Tui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for the <see cref="TuiThemeAdapter"/> class.
/// </summary>
[TestFixture]
public class TuiThemeAdapterTests
{
    private TuiThemeAdapter _adapter = null!;

    [SetUp]
    public void SetUp()
    {
        var theme = ThemeDefinition.CreateDefault();
        _adapter = new TuiThemeAdapter(theme);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetHealthColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(1.00, "#228B22", Description = "100% = HealthFull")]
    [TestCase(0.80, "#228B22", Description = "80% = HealthFull")]
    [TestCase(0.76, "#228B22", Description = "76% = HealthFull (boundary)")]
    [TestCase(0.75, "#32CD32", Description = "75% = HealthGood")]
    [TestCase(0.60, "#32CD32", Description = "60% = HealthGood")]
    [TestCase(0.51, "#32CD32", Description = "51% = HealthGood (boundary)")]
    [TestCase(0.50, "#FFD700", Description = "50% = HealthLow")]
    [TestCase(0.40, "#FFD700", Description = "40% = HealthLow")]
    [TestCase(0.26, "#FFD700", Description = "26% = HealthLow (boundary)")]
    [TestCase(0.25, "#DC143C", Description = "25% = HealthCritical")]
    [TestCase(0.10, "#DC143C", Description = "10% = HealthCritical")]
    [TestCase(0.00, "#DC143C", Description = "0% = HealthCritical")]
    public void GetHealthColor_ReturnsCorrectColorForThreshold(double percentage, string expectedHex)
    {
        // Act
        var color = _adapter.GetHealthColor(percentage);

        // Assert
        color.Hex.Should().Be(expectedHex);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetResourceColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase("Mana", "#4169E1")]
    [TestCase("mana", "#4169E1")]
    [TestCase("MP", "#4169E1")]
    [TestCase("Rage", "#DC143C")]
    [TestCase("Energy", "#FFD700")]
    [TestCase("Focus", "#00CED1")]
    [TestCase("Stamina", "#32CD32")]
    public void GetResourceColor_ReturnsCorrectColorForType(string resourceType, string expectedHex)
    {
        // Act
        var color = _adapter.GetResourceColor(resourceType);

        // Assert
        color.Hex.Should().Be(expectedHex);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToConsoleColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToConsoleColor_LimeGreen_MapsToGreen()
    {
        // Arrange
        var limeGreen = ThemeColor.FromHex("#32CD32");

        // Act
        var consoleColor = _adapter.ToConsoleColor(limeGreen);

        // Assert
        consoleColor.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void ToConsoleColor_Crimson_MapsToRed()
    {
        // Arrange
        var crimson = ThemeColor.FromHex("#DC143C");

        // Act
        var consoleColor = _adapter.ToConsoleColor(crimson);

        // Assert - Crimson (220, 20, 60) is nearest to Red (255, 0, 0)
        consoleColor.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void ToConsoleColor_RoyalBlue_MapsToNearestColor()
    {
        // Arrange
        var royalBlue = ThemeColor.FromHex("#4169E1");

        // Act
        var consoleColor = _adapter.ToConsoleColor(royalBlue);

        // Assert - Royal Blue (65, 105, 225) is nearest to DarkCyan (0, 139, 139)
        consoleColor.Should().Be(ConsoleColor.DarkCyan);
    }

    [Test]
    public void ToConsoleColor_PureMagenta_MapsToMagenta()
    {
        // Arrange
        var magenta = new ThemeColor(255, 0, 255);

        // Act
        var consoleColor = _adapter.ToConsoleColor(magenta);

        // Assert
        consoleColor.Should().Be(ConsoleColor.Magenta);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToSpectreColorString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToSpectreColorString_ReturnsHexWithoutHash()
    {
        // Arrange
        var color = ThemeColor.FromHex("#FF5500");

        // Act
        var spectreColor = _adapter.ToSpectreColorString(color);

        // Assert
        spectreColor.Should().Be("FF5500");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetIcon Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetIcon_UseAsciiIconsFalse_ReturnsUnicode()
    {
        // Arrange
        _adapter.UseAsciiIcons = false;

        // Act
        var icon = _adapter.GetIcon(IconKey.Health);

        // Assert
        icon.Should().Be("♥");
    }

    [Test]
    public void GetIcon_UseAsciiIconsTrue_ReturnsAscii()
    {
        // Arrange
        _adapter.UseAsciiIcons = true;

        // Act
        var icon = _adapter.GetIcon(IconKey.Health);

        // Assert
        icon.Should().Be("[HP]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Color Blind Mode Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsColorBlindModeEnabled_WhenNone_ReturnsFalse()
    {
        // Arrange
        _adapter.ColorBlindMode = ColorBlindMode.None;

        // Act & Assert
        _adapter.IsColorBlindModeEnabled.Should().BeFalse();
    }

    [Test]
    public void IsColorBlindModeEnabled_WhenSet_ReturnsTrue()
    {
        // Arrange
        _adapter.ColorBlindMode = ColorBlindMode.Protanopia;

        // Act & Assert
        _adapter.IsColorBlindModeEnabled.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Animation Duration Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetAnimationDuration_Short_Returns100ms()
    {
        // Act
        var duration = _adapter.GetAnimationDuration(AnimationKey.Short);

        // Assert
        duration.TotalMilliseconds.Should().Be(100);
    }

    [Test]
    public void GetAnimationDuration_Medium_Returns250ms()
    {
        // Act
        var duration = _adapter.GetAnimationDuration(AnimationKey.Medium);

        // Assert
        duration.TotalMilliseconds.Should().Be(250);
    }

    [Test]
    public void GetAnimationDuration_HealthChange_Returns300ms()
    {
        // Act
        var duration = _adapter.GetAnimationDuration(AnimationKey.HealthChange);

        // Assert
        duration.TotalMilliseconds.Should().Be(300);
    }
}
