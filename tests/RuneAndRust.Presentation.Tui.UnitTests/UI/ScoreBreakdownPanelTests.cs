// ═══════════════════════════════════════════════════════════════════════════════
// ScoreBreakdownPanelTests.cs
// Unit tests for the ScoreBreakdownPanel class.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for ScoreBreakdownPanel functionality.
/// </summary>
[TestFixture]
public class ScoreBreakdownPanelTests
{
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private ScoreBreakdownPanel _panel = null!;
    private LeaderboardViewConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        _config = new LeaderboardViewConfig();
        _panel = new ScoreBreakdownPanel(_terminalServiceMock.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ScoreBreakdownPanel(null!, _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefaults()
    {
        // Arrange & Act
        var panel = new ScoreBreakdownPanel(_terminalServiceMock.Object, null);

        // Assert
        panel.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatComponent_WithCountAndPoints_ReturnsCountTimesEqualsFormat()
    {
        // Arrange
        var component = new ScoreComponentDto
        {
            Name = "Monsters Killed",
            Count = 127,
            PointsEach = 10,
            TotalPoints = 1270
        };

        // Act
        var result = _panel.FormatComponent(component);

        // Assert
        result.Should().Be("Monsters Killed: 127 x 10 = 1,270");
    }

    [Test]
    public void FormatComponent_FlatValue_ReturnsTotalOnly()
    {
        // Arrange
        var component = new ScoreComponentDto
        {
            Name = "Gold Earned",
            Count = 0,
            PointsEach = 0,
            TotalPoints = 15340
        };

        // Act
        var result = _panel.FormatComponent(component);

        // Assert
        result.Should().Be("Gold Earned: 15,340");
    }

    [Test]
    public void FormatMultiplier_WithMultiplierName_ReturnsMultiplierFormat()
    {
        // Arrange
        var multiplier = new ScoreComponentDto
        {
            Name = "Level Multiplier",
            TotalPoints = 180,  // 1.8x
            IsMultiplier = true
        };

        // Act
        var result = _panel.FormatMultiplier(multiplier);

        // Assert
        result.Should().Be("Level Multiplier: x1.8");
    }

    [Test]
    public void FormatMultiplier_FlatBonus_ReturnsFlatValue()
    {
        // Arrange
        var bonus = new ScoreComponentDto
        {
            Name = "Achievement Bonus",
            TotalPoints = 4850,
            IsMultiplier = true
        };

        // Act
        var result = _panel.FormatMultiplier(bonus);

        // Assert
        result.Should().Be("Achievement Bonus: 4,850");
    }

    [Test]
    public void FormatPenalty_NegativeValue_ReturnsWithNegativePrefix()
    {
        // Arrange
        var penalty = new ScoreComponentDto
        {
            Name = "Death Penalty",
            TotalPoints = -300,
            IsPenalty = true
        };

        // Act
        var result = _panel.FormatPenalty(penalty);

        // Assert
        result.Should().Be("Death Penalty: -300");
    }

    [Test]
    public void FormatPenalty_PositiveValueStored_StillShowsNegative()
    {
        // Arrange: Sometimes penalties might be stored as positive
        var penalty = new ScoreComponentDto
        {
            Name = "Death Penalty",
            TotalPoints = 300,  // Stored as positive
            IsPenalty = true
        };

        // Act
        var result = _panel.FormatPenalty(penalty);

        // Assert
        result.Should().Be("Death Penalty: -300");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderBreakdown_WritesHeader()
    {
        // Arrange
        var components = new List<ScoreComponentDto>
        {
            new ScoreComponentDto { Name = "Gold", TotalPoints = 100 }
        };

        // Act
        _panel.RenderBreakdown(components, 100, 0, 0);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(0, 0, "SCORE BREAKDOWN:"),
            Times.Once);
    }

    [Test]
    public void RenderBreakdown_WritesSeparatorLine()
    {
        // Arrange
        var components = new List<ScoreComponentDto>
        {
            new ScoreComponentDto { Name = "Gold", TotalPoints = 100 }
        };

        // Act
        _panel.RenderBreakdown(components, 100, 0, 0);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(0, 1, It.Is<string>(s => s.StartsWith("--"))),
            Times.Once);
    }

    [Test]
    public void RenderBreakdown_WritesTotal()
    {
        // Arrange
        var components = new List<ScoreComponentDto>
        {
            new ScoreComponentDto { Name = "Gold", TotalPoints = 52340 }
        };

        // Act
        _panel.RenderBreakdown(components, 52340, 0, 0);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(0, It.IsAny<int>(), "TOTAL: 52,340"),
            Times.Once);
    }

    [Test]
    public void RenderBreakdown_ReturnsYAfterLastLine()
    {
        // Arrange
        var components = new List<ScoreComponentDto>
        {
            new ScoreComponentDto { Name = "Gold", TotalPoints = 100 }
        };

        // Act
        var result = _panel.RenderBreakdown(components, 100, 0, 5);

        // Assert
        result.Should().BeGreaterThan(5);
    }
}
