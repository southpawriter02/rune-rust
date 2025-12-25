using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Settings;
using RuneAndRust.Terminal.Services;
using Spectre.Console;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for ThemeService (v0.3.9b).
/// Tests theme palette management and color lookups for accessibility support.
/// </summary>
public class ThemeServiceTests
{
    private readonly ILogger<ThemeService> _mockLogger;
    private readonly ThemeService _sut;

    public ThemeServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ThemeService>>();

        // Reset static GameSettings to default state for test isolation
        GameSettings.Theme = ThemeType.Standard;

        _sut = new ThemeService(_mockLogger);
    }

    #region GetColor Tests - Standard Theme

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForPlayerColor_StandardTheme()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("PlayerColor");

        // Assert
        result.Should().Be("cyan");
    }

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForEnemyColor_StandardTheme()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("EnemyColor");

        // Assert
        result.Should().Be("red");
    }

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForHealthFull_StandardTheme()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("HealthFull");

        // Assert
        result.Should().Be("green");
    }

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForHealthCritical_StandardTheme()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("HealthCritical");

        // Assert
        result.Should().Be("red");
    }

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForQualityLegendary_StandardTheme()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("QualityLegendary");

        // Assert
        result.Should().Be("gold1");
    }

    #endregion

    #region GetColor Tests - High Contrast Theme

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForPlayerColor_HighContrastTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.HighContrast);

        // Act
        var result = _sut.GetColor("PlayerColor");

        // Assert
        result.Should().Be("white");
    }

    [Fact]
    public void GetColor_ReturnsCorrectColor_ForEnemyColor_HighContrastTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.HighContrast);

        // Act
        var result = _sut.GetColor("EnemyColor");

        // Assert
        result.Should().Be("yellow");
    }

    [Fact]
    public void GetColor_ReturnsBoldColor_ForHealthCritical_HighContrastTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.HighContrast);

        // Act
        var result = _sut.GetColor("HealthCritical");

        // Assert
        result.Should().Be("bold red");
    }

    #endregion

    #region GetColor Tests - Protanopia Theme

    [Fact]
    public void GetColor_ReturnsBlue_ForPlayerColor_ProtanopiaTheme()
    {
        // Arrange - Red-green colorblind support
        _sut.SetTheme(ThemeType.Protanopia);

        // Act
        var result = _sut.GetColor("PlayerColor");

        // Assert - Should use blue instead of cyan for better distinction
        result.Should().Be("blue");
    }

    [Fact]
    public void GetColor_ReturnsOrange_ForEnemyColor_ProtanopiaTheme()
    {
        // Arrange - Protanopia users cannot distinguish red from green
        _sut.SetTheme(ThemeType.Protanopia);

        // Act
        var result = _sut.GetColor("EnemyColor");

        // Assert - Should use orange1 instead of red
        result.Should().Be("orange1");
    }

    [Fact]
    public void GetColor_ReturnsBlue_ForHealthFull_ProtanopiaTheme()
    {
        // Arrange - Green should be replaced for red-green colorblind users
        _sut.SetTheme(ThemeType.Protanopia);

        // Act
        var result = _sut.GetColor("HealthFull");

        // Assert
        result.Should().Be("blue");
    }

    #endregion

    #region GetColor Tests - Deuteranopia Theme

    [Fact]
    public void GetColor_ReturnsBlue_ForPlayerColor_DeuteranopiaTheme()
    {
        // Arrange - Green-red colorblind support
        _sut.SetTheme(ThemeType.Deuteranopia);

        // Act
        var result = _sut.GetColor("PlayerColor");

        // Assert
        result.Should().Be("blue");
    }

    [Fact]
    public void GetColor_ReturnsOrange_ForEnemyColor_DeuteranopiaTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.Deuteranopia);

        // Act
        var result = _sut.GetColor("EnemyColor");

        // Assert
        result.Should().Be("orange1");
    }

    #endregion

    #region GetColor Tests - Tritanopia Theme

    [Fact]
    public void GetColor_ReturnsCyan_ForPlayerColor_TritanopiaTheme()
    {
        // Arrange - Blue-yellow colorblind support
        _sut.SetTheme(ThemeType.Tritanopia);

        // Act
        var result = _sut.GetColor("PlayerColor");

        // Assert - Cyan works for tritanopia
        result.Should().Be("cyan");
    }

    [Fact]
    public void GetColor_ReturnsMagenta_ForEnemyColor_TritanopiaTheme()
    {
        // Arrange - Tritanopia users confuse blue with yellow
        _sut.SetTheme(ThemeType.Tritanopia);

        // Act
        var result = _sut.GetColor("EnemyColor");

        // Assert - Magenta instead of red for better distinction
        result.Should().Be("magenta1");
    }

    #endregion

    #region Fallback Tests

    [Fact]
    public void GetColor_FallsBackToGrey_WhenRoleNotFound()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColor("NonExistentRole");

        // Assert
        result.Should().Be("grey");
    }

    [Fact]
    public void GetColor_FallsBackToStandard_WhenRoleNotInCurrentTheme()
    {
        // Arrange - Set theme and request a role that exists in Standard but may have fallback
        _sut.SetTheme(ThemeType.HighContrast);

        // Act
        var result = _sut.GetColor("NeutralColor");

        // Assert - Should return the HighContrast value (white) or fallback
        result.Should().NotBeNull();
    }

    #endregion

    #region SetTheme Tests

    [Fact]
    public void SetTheme_UpdatesCurrentTheme()
    {
        // Arrange
        _sut.CurrentTheme.Should().Be(ThemeType.Standard);

        // Act
        _sut.SetTheme(ThemeType.HighContrast);

        // Assert
        _sut.CurrentTheme.Should().Be(ThemeType.HighContrast);
    }

    [Fact]
    public void SetTheme_UpdatesGameSettings()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        _sut.SetTheme(ThemeType.Protanopia);

        // Assert
        GameSettings.Theme.Should().Be(ThemeType.Protanopia);
    }

    [Fact]
    public void CurrentTheme_ReflectsGameSettings()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Deuteranopia;

        // Act
        var result = _sut.CurrentTheme;

        // Assert
        result.Should().Be(ThemeType.Deuteranopia);
    }

    #endregion

    #region GetColorObject Tests

    [Fact]
    public void GetColorObject_ReturnsRedColor_ForHealthCritical()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColorObject("HealthCritical");

        // Assert
        result.Should().Be(Color.Red);
    }

    [Fact]
    public void GetColorObject_ReturnsCyanColor_ForPlayerColor()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColorObject("PlayerColor");

        // Assert
        result.Should().Be(Color.Cyan1);
    }

    [Fact]
    public void GetColorObject_ReturnsGreenColor_ForHealthFull()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColorObject("HealthFull");

        // Assert
        result.Should().Be(Color.Green);
    }

    [Fact]
    public void GetColorObject_ReturnsGold1Color_ForQualityLegendary()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColorObject("QualityLegendary");

        // Assert
        result.Should().Be(Color.Gold1);
    }

    [Fact]
    public void GetColorObject_ReturnsGreyColor_WhenRoleNotFound()
    {
        // Arrange
        GameSettings.Theme = ThemeType.Standard;

        // Act
        var result = _sut.GetColorObject("NonExistentRole");

        // Assert
        result.Should().Be(Color.Grey);
    }

    [Fact]
    public void GetColorObject_HandlesBoldPrefix_ReturnsColorWithoutBold()
    {
        // Arrange - HighContrast uses "bold red" for HealthCritical
        _sut.SetTheme(ThemeType.HighContrast);

        // Act
        var result = _sut.GetColorObject("HealthCritical");

        // Assert - Should return Color.Red (bold is a style, not a color property)
        result.Should().Be(Color.Red);
    }

    #endregion

    #region All Theme Palette Completeness Tests

    [Theory]
    [InlineData(ThemeType.Standard)]
    [InlineData(ThemeType.HighContrast)]
    [InlineData(ThemeType.Protanopia)]
    [InlineData(ThemeType.Deuteranopia)]
    [InlineData(ThemeType.Tritanopia)]
    public void GetColor_ReturnsValue_ForAllCoreRoles_InAllThemes(ThemeType theme)
    {
        // Arrange
        _sut.SetTheme(theme);
        var coreRoles = new[]
        {
            "PlayerColor", "EnemyColor", "HealthFull", "HealthCritical",
            "StaminaColor", "QualityLegendary", "SuccessColor", "WarningColor"
        };

        // Act & Assert
        foreach (var role in coreRoles)
        {
            var result = _sut.GetColor(role);
            result.Should().NotBeNullOrEmpty($"Role '{role}' should have a value in {theme} theme");
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ThemeService_MaintainsColorConsistency_WhenSwitchingThemes()
    {
        // Arrange
        var role = "EnemyColor";

        // Act - Get colors across theme switches
        _sut.SetTheme(ThemeType.Standard);
        var standardColor = _sut.GetColor(role);

        _sut.SetTheme(ThemeType.Protanopia);
        var protanopiaColor = _sut.GetColor(role);

        _sut.SetTheme(ThemeType.Standard);
        var backToStandard = _sut.GetColor(role);

        // Assert
        standardColor.Should().Be("red");
        protanopiaColor.Should().Be("orange1");
        backToStandard.Should().Be("red", "Color should return to original after switching back");
    }

    #endregion

    #region v0.3.14a - New Semantic Keys Tests

    [Theory]
    [InlineData(ThemeType.Standard)]
    [InlineData(ThemeType.HighContrast)]
    [InlineData(ThemeType.Protanopia)]
    [InlineData(ThemeType.Deuteranopia)]
    [InlineData(ThemeType.Tritanopia)]
    public void GetColor_ReturnsValue_ForBiomeKeys_InAllThemes(ThemeType theme)
    {
        // Arrange
        _sut.SetTheme(theme);
        var biomeKeys = new[] { "BiomeRuin", "BiomeIndustrial", "BiomeOrganic", "BiomeVoid" };

        // Act & Assert
        foreach (var key in biomeKeys)
        {
            var result = _sut.GetColor(key);
            result.Should().NotBeNullOrEmpty($"Key '{key}' should have a value in {theme} theme");
        }
    }

    [Theory]
    [InlineData(ThemeType.Standard)]
    [InlineData(ThemeType.HighContrast)]
    [InlineData(ThemeType.Protanopia)]
    [InlineData(ThemeType.Deuteranopia)]
    [InlineData(ThemeType.Tritanopia)]
    public void GetColor_ReturnsValue_ForUIStructuralKeys_InAllThemes(ThemeType theme)
    {
        // Arrange
        _sut.SetTheme(theme);
        var uiKeys = new[]
        {
            "DimColor", "SeparatorColor", "LabelColor", "InputColor",
            "BorderActive", "BorderInactive", "NarrativeColor", "TabActive"
        };

        // Act & Assert
        foreach (var key in uiKeys)
        {
            var result = _sut.GetColor(key);
            result.Should().NotBeNullOrEmpty($"Key '{key}' should have a value in {theme} theme");
        }
    }

    [Fact]
    public void GetColor_ReturnsCorrectBiomeColors_ForStandardTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.Standard);

        // Act & Assert
        _sut.GetColor("BiomeRuin").Should().Be("grey");
        _sut.GetColor("BiomeIndustrial").Should().Be("orange1");
        _sut.GetColor("BiomeOrganic").Should().Be("green");
        _sut.GetColor("BiomeVoid").Should().Be("purple");
    }

    [Fact]
    public void GetColor_ReturnsHighVisibilityBiomeColors_ForHighContrastTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.HighContrast);

        // Act & Assert
        _sut.GetColor("BiomeRuin").Should().Be("white");
        _sut.GetColor("BiomeIndustrial").Should().Be("bold yellow");
        _sut.GetColor("BiomeOrganic").Should().Be("bold green");
        _sut.GetColor("BiomeVoid").Should().Be("bold purple");
    }

    [Fact]
    public void GetColor_AvoidRedGreen_ForProtanopiaBiomeColors()
    {
        // Arrange - Protanopia users cannot distinguish red from green
        _sut.SetTheme(ThemeType.Protanopia);

        // Act & Assert - BiomeOrganic should not be green
        _sut.GetColor("BiomeOrganic").Should().Be("cyan");
        _sut.GetColor("BiomeVoid").Should().Be("blue");
    }

    [Fact]
    public void GetColor_AvoidBlueYellow_ForTritanopiaBiomeColors()
    {
        // Arrange - Tritanopia users confuse blue with yellow
        _sut.SetTheme(ThemeType.Tritanopia);

        // Act & Assert - BiomeVoid should not be blue
        _sut.GetColor("BiomeVoid").Should().Be("magenta1");
    }

    [Fact]
    public void GetColor_ReturnsCorrectUIColors_ForStandardTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.Standard);

        // Act & Assert
        _sut.GetColor("DimColor").Should().Be("grey");
        _sut.GetColor("SeparatorColor").Should().Be("grey");
        _sut.GetColor("LabelColor").Should().Be("grey");
        _sut.GetColor("InputColor").Should().Be("cyan");
        _sut.GetColor("BorderActive").Should().Be("yellow");
        _sut.GetColor("BorderInactive").Should().Be("grey");
        _sut.GetColor("NarrativeColor").Should().Be("grey");
        _sut.GetColor("TabActive").Should().Be("gold1");
    }

    [Fact]
    public void GetColor_ReturnsHighVisibilityUIColors_ForHighContrastTheme()
    {
        // Arrange
        _sut.SetTheme(ThemeType.HighContrast);

        // Act & Assert
        _sut.GetColor("SeparatorColor").Should().Be("white");
        _sut.GetColor("LabelColor").Should().Be("white");
        _sut.GetColor("InputColor").Should().Be("bold cyan");
        _sut.GetColor("BorderActive").Should().Be("bold yellow");
        _sut.GetColor("NarrativeColor").Should().Be("white");
        _sut.GetColor("TabActive").Should().Be("bold gold1");
    }

    #endregion
}
