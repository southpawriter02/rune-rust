// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryPanelTests.cs
// Unit tests for DiceHistoryPanel UI component.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Records;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the DiceHistoryPanel class.
/// </summary>
[TestFixture]
public class DiceHistoryPanelTests
{
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private DiceHistoryPanelConfig _config = null!;
    private DiceRollRenderer _rollRenderer = null!;
    private StreakIndicator _streakIndicator = null!;
    private RollDistributionChart _distributionChart = null!;
    private DiceHistoryPanel _panel = null!;

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        _config = new DiceHistoryPanelConfig();
        _rollRenderer = new DiceRollRenderer(_config);
        _streakIndicator = new StreakIndicator(_terminalServiceMock.Object, _config);
        _distributionChart = new RollDistributionChart(_terminalServiceMock.Object, _config);
        _panel = new DiceHistoryPanel(
            _terminalServiceMock.Object,
            _rollRenderer,
            _streakIndicator,
            _distributionChart,
            _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminal_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new DiceHistoryPanel(
            null!,
            _rollRenderer,
            _streakIndicator,
            _distributionChart,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminal");
    }

    [Test]
    public void Constructor_WithNullRollRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new DiceHistoryPanel(
            _terminalServiceMock.Object,
            null!,
            _streakIndicator,
            _distributionChart,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("rollRenderer");
    }

    [Test]
    public void Constructor_WithNullStreakIndicator_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new DiceHistoryPanel(
            _terminalServiceMock.Object,
            _rollRenderer,
            null!,
            _distributionChart,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("streakIndicator");
    }

    [Test]
    public void Constructor_WithNullDistributionChart_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new DiceHistoryPanel(
            _terminalServiceMock.Object,
            _rollRenderer,
            _streakIndicator,
            null!,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("distributionChart");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefaults()
    {
        // Arrange & Act
        var act = () => new DiceHistoryPanel(
            _terminalServiceMock.Object,
            _rollRenderer,
            _streakIndicator,
            _distributionChart,
            null);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SetPosition_SetsRenderPosition()
    {
        // Act & Assert
        var act = () => _panel.SetPosition(10, 5);
        act.Should().NotThrow();
    }

    [Test]
    public void SetDisplayCount_SetsRollCount()
    {
        // Act & Assert
        var act = () => _panel.SetDisplayCount(15);
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Render_ValidDto_WritesToTerminal()
    {
        // Arrange
        var dto = CreateTestDisplayDto();

        // Act
        _panel.Render(dto);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void Render_EmptyHistory_DisplaysEmptyMessage()
    {
        // Arrange
        var dto = DiceHistoryDisplayDto.Empty;

        // Act
        _panel.Render(dto);

        // Assert - Should display "No roll history available"
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.Is<string>(s => s.Contains("No roll history"))),
            Times.AtLeastOnce);
    }

    [Test]
    public void Render_NullDto_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _panel.Render(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RenderHistory_ValidRolls_DisplaysFormattedList()
    {
        // Arrange
        var rolls = new[]
        {
            DiceRollRecord.Create("1d20", 18, new[] { 18 }, "attack"),
            DiceRollRecord.Create("1d20", 20, new[] { 20 }, "attack"),
            DiceRollRecord.Create("1d20", 12, new[] { 12 }, "attack")
        };

        // Act
        var act = () => _panel.RenderHistory(rolls);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void RenderHistory_NullRolls_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _panel.RenderHistory(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ShowStatistics_ValidData_DisplaysAllStats()
    {
        // Act
        var act = () => _panel.ShowStatistics(
            totalRolls: 523,
            nat20s: 47,
            nat20Percentage: 9.0f,
            nat1s: 31,
            nat1Percentage: 5.9f,
            average: 11.2f,
            expected: 10.5f);

        // Assert
        act.Should().NotThrow();
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void ShowLuckRating_PositiveDeviation_DisplaysLucky()
    {
        // Act
        _panel.ShowLuckRating(deviation: 6.7f);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteColoredAt(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.Is<string>(s => s.Contains("LUCKY")),
                It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void ShowLuckRating_NegativeDeviation_DisplaysUnlucky()
    {
        // Act
        _panel.ShowLuckRating(deviation: -8.5f);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteColoredAt(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.Is<string>(s => s.Contains("UNLUCKY")),
                It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    private static DiceHistoryDisplayDto CreateTestDisplayDto()
    {
        var distribution = new int[20];
        for (int i = 0; i < 20; i++) distribution[i] = 25 + i;

        return new DiceHistoryDisplayDto(
            RecentRolls: new[] { 20, 15, 8, 12, 1, 18, 14 },
            DieType: "d20",
            TotalRolls: 523,
            NaturalTwenties: 47,
            NaturalOnes: 31,
            AverageRoll: 11.2f,
            ExpectedAverage: 10.5f,
            LuckDeviation: 6.7f,
            CurrentStreak: 3,
            LongestLuckyStreak: 7,
            LongestUnluckyStreak: 4,
            Distribution: distribution);
    }
}
