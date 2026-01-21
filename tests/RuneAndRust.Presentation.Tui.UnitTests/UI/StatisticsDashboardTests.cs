// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsDashboardTests.cs
// Unit tests for the StatisticsDashboard component.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the <see cref="StatisticsDashboard"/> component.
/// </summary>
/// <remarks>
/// Tests cover dashboard construction, category filtering, and input handling.
/// </remarks>
[TestFixture]
public class StatisticsDashboardTests
{
    private Mock<ITerminalService> _mockTerminalService = null!;
    private StatisticsDashboardConfig _config = null!;
    private CategoryTabs _categoryTabs = null!;
    private StatComparisonView _comparisonView = null!;
    private SimpleChart _chart = null!;
    private PlaytimeRenderer _playtimeRenderer = null!;
    private StatisticsDashboard _dashboard = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminalService = new Mock<ITerminalService>();
        _config = new StatisticsDashboardConfig();
        _categoryTabs = new CategoryTabs(_mockTerminalService.Object, _config);
        _comparisonView = new StatComparisonView(_mockTerminalService.Object, _config);
        _chart = new SimpleChart(_mockTerminalService.Object, _config);
        _playtimeRenderer = new PlaytimeRenderer();

        _dashboard = new StatisticsDashboard(
            _categoryTabs,
            _comparisonView,
            _chart,
            _playtimeRenderer,
            _mockTerminalService.Object,
            _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullCategoryTabs_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new StatisticsDashboard(
            null!,
            _comparisonView,
            _chart,
            _playtimeRenderer,
            _mockTerminalService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("categoryTabs");
    }

    [Test]
    public void Constructor_NullComparisonView_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new StatisticsDashboard(
            _categoryTabs,
            null!,
            _chart,
            _playtimeRenderer,
            _mockTerminalService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("comparisonView");
    }

    [Test]
    public void Constructor_ValidParameters_DefaultsToCombatCategory()
    {
        // Arrange & Act - use existing dashboard from SetUp

        // Assert
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Combat);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FILTER BY CATEGORY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FilterByCategory_ChangeToExploration_UpdatesActiveCategory()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        _dashboard.FilterByCategory(StatisticCategory.Exploration);

        // Assert
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Exploration);
    }

    [Test]
    public void FilterByCategory_ChangeToDice_UpdatesActiveCategory()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        _dashboard.FilterByCategory(StatisticCategory.Dice);

        // Assert
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Dice);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HANDLE CATEGORY INPUT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HandleCategoryInput_ValidKeyNumber_ReturnsTrue()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        var result = _dashboard.HandleCategoryInput(2);

        // Assert
        result.Should().BeTrue();
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Exploration);
    }

    [Test]
    public void HandleCategoryInput_InvalidKeyNumber_ReturnsFalse()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        var result = _dashboard.HandleCategoryInput(9);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HandleCategoryInput_ConsoleKeyD3_ChangesToProgression()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        var result = _dashboard.HandleCategoryInput(ConsoleKey.D3);

        // Assert
        result.Should().BeTrue();
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Progression);
    }

    [Test]
    public void HandleCategoryInput_NumPad5_ChangesToDice()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        var result = _dashboard.HandleCategoryInput(ConsoleKey.NumPad5);

        // Assert
        result.Should().BeTrue();
        _dashboard.ActiveCategory.Should().Be(StatisticCategory.Dice);
    }

    [Test]
    public void HandleCategoryInput_EscapeKey_ReturnsFalse()
    {
        // Arrange
        var stats = new List<StatisticDisplayDto>();
        _dashboard.SetPosition(0, 0);
        _dashboard.RenderStatistics(stats, TimeSpan.Zero, TimeSpan.Zero);

        // Act
        var result = _dashboard.HandleCategoryInput(ConsoleKey.Escape);

        // Assert
        result.Should().BeFalse();
    }
}
