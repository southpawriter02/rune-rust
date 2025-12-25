using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the VictoryScreenRenderer class.
/// Validates quality color mapping and logging behavior.
/// Updated with IThemeService mock in v0.3.14a.
/// </summary>
public class VictoryScreenRendererTests
{
    private readonly Mock<ILogger<VictoryScreenRenderer>> _mockLogger;
    private readonly Mock<IThemeService> _mockTheme;
    private readonly VictoryScreenRenderer _sut;

    public VictoryScreenRendererTests()
    {
        _mockLogger = new Mock<ILogger<VictoryScreenRenderer>>();
        _mockTheme = new Mock<IThemeService>();
        _mockTheme.Setup(t => t.GetColor(It.IsAny<string>())).Returns("grey");
        _sut = new VictoryScreenRenderer(_mockLogger.Object, _mockTheme.Object);
    }

    #region GetQualityColor Tests

    [Fact]
    public void GetQualityColor_JuryRigged_ReturnsGrey()
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(QualityTier.JuryRigged);

        // Assert
        color.Should().Be("grey");
    }

    [Fact]
    public void GetQualityColor_Scavenged_ReturnsWhite()
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(QualityTier.Scavenged);

        // Assert
        color.Should().Be("white");
    }

    [Fact]
    public void GetQualityColor_ClanForged_ReturnsGreen()
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(QualityTier.ClanForged);

        // Assert
        color.Should().Be("green");
    }

    [Fact]
    public void GetQualityColor_Optimized_ReturnsBlue()
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(QualityTier.Optimized);

        // Assert
        color.Should().Be("blue");
    }

    [Fact]
    public void GetQualityColor_MythForged_ReturnsMagenta()
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(QualityTier.MythForged);

        // Assert
        color.Should().Be("magenta");
    }

    [Fact]
    public void GetQualityColor_UndefinedValue_ReturnsWhite()
    {
        // Arrange - Cast an undefined enum value
        var undefinedQuality = (QualityTier)999;

        // Act
        var color = VictoryScreenRenderer.GetQualityColor(undefinedQuality);

        // Assert
        color.Should().Be("white");
    }

    #endregion

    #region Quality Tier Coverage Tests

    [Theory]
    [InlineData(QualityTier.JuryRigged, "grey")]
    [InlineData(QualityTier.Scavenged, "white")]
    [InlineData(QualityTier.ClanForged, "green")]
    [InlineData(QualityTier.Optimized, "blue")]
    [InlineData(QualityTier.MythForged, "magenta")]
    public void GetQualityColor_AllTiers_ReturnExpectedColors(QualityTier tier, string expectedColor)
    {
        // Act
        var color = VictoryScreenRenderer.GetQualityColor(tier);

        // Assert
        color.Should().Be(expectedColor);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Arrange
        var logger = new Mock<ILogger<VictoryScreenRenderer>>();
        var theme = new Mock<IThemeService>();
        theme.Setup(t => t.GetColor(It.IsAny<string>())).Returns("grey");

        // Act
        var renderer = new VictoryScreenRenderer(logger.Object, theme.Object);

        // Assert
        renderer.Should().NotBeNull();
    }

    #endregion

    #region CombatResult Validation Tests

    [Fact]
    public void CombatResult_WithEmptyLoot_IsValid()
    {
        // Arrange
        var result = new CombatResult(
            Victory: true,
            XpEarned: 50,
            LootFound: new List<Item>(),
            Summary: "Victory!"
        );

        // Assert
        result.LootFound.Should().BeEmpty();
        result.Victory.Should().BeTrue();
        result.XpEarned.Should().Be(50);
    }

    [Fact]
    public void CombatResult_WithLoot_ContainsItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Rusty Sword", Quality = QualityTier.JuryRigged, Value = 10 },
            new Item { Name = "Iron Shield", Quality = QualityTier.ClanForged, Value = 50 }
        };

        var result = new CombatResult(
            Victory: true,
            XpEarned: 100,
            LootFound: items,
            Summary: "Victory with loot!"
        );

        // Assert
        result.LootFound.Should().HaveCount(2);
        result.LootFound[0].Name.Should().Be("Rusty Sword");
        result.LootFound[1].Quality.Should().Be(QualityTier.ClanForged);
    }

    #endregion

    #region Color Consistency Tests

    [Fact]
    public void GetQualityColor_SameTier_ReturnsSameColor()
    {
        // Act
        var color1 = VictoryScreenRenderer.GetQualityColor(QualityTier.MythForged);
        var color2 = VictoryScreenRenderer.GetQualityColor(QualityTier.MythForged);

        // Assert
        color1.Should().Be(color2);
    }

    [Fact]
    public void GetQualityColor_DifferentTiers_ReturnDifferentColors()
    {
        // Arrange
        var tiers = Enum.GetValues<QualityTier>();
        var colors = new HashSet<string>();

        // Act
        foreach (var tier in tiers)
        {
            colors.Add(VictoryScreenRenderer.GetQualityColor(tier));
        }

        // Assert - All 5 tiers should have unique colors
        colors.Should().HaveCount(5);
    }

    #endregion
}
