// ═══════════════════════════════════════════════════════════════════════════════
// AchievementPanelTests.cs
// Unit tests for AchievementPanel.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="AchievementPanel"/>.
/// </summary>
[TestFixture]
public class AchievementPanelTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private AchievementCategoryView _categoryView = null!;
    private AchievementCard _achievementCard = null!;
    private AchievementPanelConfig _config = null!;
    private AchievementPanel _panel = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _config = new AchievementPanelConfig();

        _categoryView = new AchievementCategoryView(
            _mockTerminal.Object,
            NullLogger<AchievementCategoryView>.Instance);

        var rarityBadge = new RarityBadge(_mockTerminal.Object);
        var progressRenderer = new ProgressBarRenderer();

        _achievementCard = new AchievementCard(
            rarityBadge,
            progressRenderer,
            _mockTerminal.Object,
            _config,
            NullLogger<AchievementCard>.Instance);

        _panel = new AchievementPanel(
            _categoryView,
            _achievementCard,
            _mockTerminal.Object,
            _config,
            NullLogger<AchievementPanel>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // FILTER BY CATEGORY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FilterByCategory_WithCombat_SetsActiveFilter()
    {
        // Arrange & Act
        _panel.FilterByCategory(AchievementCategory.Combat);

        // Assert
        _panel.ActiveFilter.Should().Be(AchievementCategory.Combat);
    }

    [Test]
    public void FilterByCategory_WithNull_ClearsFilter()
    {
        // Arrange - Set a filter first
        _panel.FilterByCategory(AchievementCategory.Combat);

        // Act
        _panel.FilterByCategory(null);

        // Assert
        _panel.ActiveFilter.Should().BeNull();
    }

    [Test]
    [TestCase(AchievementCategory.Combat)]
    [TestCase(AchievementCategory.Exploration)]
    [TestCase(AchievementCategory.Progression)]
    [TestCase(AchievementCategory.Collection)]
    [TestCase(AchievementCategory.Challenge)]
    public void FilterByCategory_WithEachCategory_SetsCorrectFilter(AchievementCategory category)
    {
        // Arrange & Act
        _panel.FilterByCategory(category);

        // Assert
        _panel.ActiveFilter.Should().Be(category);
    }

    // ═══════════════════════════════════════════════════════════════
    // HANDLE FILTER INPUT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ConsoleKey.D1, null)]
    [TestCase(ConsoleKey.D2, AchievementCategory.Combat)]
    [TestCase(ConsoleKey.D3, AchievementCategory.Exploration)]
    [TestCase(ConsoleKey.D4, AchievementCategory.Progression)]
    [TestCase(ConsoleKey.D5, AchievementCategory.Collection)]
    [TestCase(ConsoleKey.D6, AchievementCategory.Challenge)]
    public void HandleFilterInput_WithKey_SetsCorrectFilter(
        ConsoleKey key, AchievementCategory? expectedFilter)
    {
        // Arrange & Act
        var handled = _panel.HandleFilterInput(key);

        // Assert
        handled.Should().BeTrue();
        _panel.ActiveFilter.Should().Be(expectedFilter);
    }

    [Test]
    public void HandleFilterInput_WithUnrecognizedKey_ReturnsFalse()
    {
        // Arrange & Act
        var handled = _panel.HandleFilterInput(ConsoleKey.A);

        // Assert
        handled.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER ACHIEVEMENTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderAchievements_WithValidData_CallsTerminalService()
    {
        // Arrange
        var achievements = new List<AchievementDisplayDto>
        {
            CreateAchievementDto("first-blood", "First Blood", AchievementCategory.Combat),
            CreateAchievementDto("explorer", "Explorer", AchievementCategory.Exploration)
        };
        _panel.SetPosition(0, 0);

        // Act
        _panel.RenderAchievements(achievements, totalPoints: 20, unlockedCount: 2, totalCount: 10);

        // Assert
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void RenderAchievements_WithFilter_FiltersAchievements()
    {
        // Arrange
        var achievements = new List<AchievementDisplayDto>
        {
            CreateAchievementDto("first-blood", "First Blood", AchievementCategory.Combat),
            CreateAchievementDto("explorer", "Explorer", AchievementCategory.Exploration),
            CreateAchievementDto("warrior", "Warrior", AchievementCategory.Combat)
        };
        _panel.SetPosition(0, 0);
        _panel.FilterByCategory(AchievementCategory.Combat);

        // Act
        _panel.RenderAchievements(achievements, totalPoints: 30, unlockedCount: 3, totalCount: 10);

        // Assert - Filter should still be Combat
        _panel.ActiveFilter.Should().Be(AchievementCategory.Combat);
    }

    [Test]
    public void RenderAchievements_WithEmptyList_RendersHeader()
    {
        // Arrange
        var achievements = new List<AchievementDisplayDto>();
        _panel.SetPosition(0, 0);

        // Act
        _panel.RenderAchievements(achievements, totalPoints: 0, unlockedCount: 0, totalCount: 0);

        // Assert - Should still render header
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.Is<string>(s => s.Contains("ACHIEVEMENTS"))),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullCategoryView_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementPanel(
            null!,
            _achievementCard,
            _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("categoryView");
    }

    [Test]
    public void Constructor_WithNullAchievementCard_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementPanel(
            _categoryView,
            null!,
            _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("achievementCard");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementPanel(
            _categoryView,
            _achievementCard,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    #region Test Helpers

    private static AchievementDisplayDto CreateAchievementDto(
        string id,
        string name,
        AchievementCategory category,
        bool isUnlocked = true)
    {
        return new AchievementDisplayDto(
            Id: id,
            Name: name,
            Description: $"Test description for {name}",
            Category: category,
            Tier: AchievementTier.Bronze,
            TargetValue: 1,
            CurrentValue: isUnlocked ? 1 : 0,
            IsUnlocked: isUnlocked,
            IsSecret: false,
            PointValue: 10,
            UnlockedAt: isUnlocked ? DateTime.UtcNow : null);
    }

    #endregion
}
