// ═══════════════════════════════════════════════════════════════════════════════
// AchievementCardTests.cs
// Unit tests for AchievementCard.
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
/// Unit tests for <see cref="AchievementCard"/>.
/// </summary>
[TestFixture]
public class AchievementCardTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private RarityBadge _rarityBadge = null!;
    private ProgressBarRenderer _progressRenderer = null!;
    private AchievementPanelConfig _config = null!;
    private AchievementCard _card = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _rarityBadge = new RarityBadge(_mockTerminal.Object);
        _progressRenderer = new ProgressBarRenderer();
        _config = new AchievementPanelConfig();

        _card = new AchievementCard(
            _rarityBadge,
            _progressRenderer,
            _mockTerminal.Object,
            _config,
            NullLogger<AchievementCard>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET STATUS INDICATOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetStatusIndicator_WhenUnlocked_ReturnsCheckMark()
    {
        // Arrange & Act
        var result = _card.GetStatusIndicator(isUnlocked: true, hasProgress: false);

        // Assert
        result.Should().Be("[x]");
    }

    [Test]
    public void GetStatusIndicator_WhenInProgress_ReturnsTilde()
    {
        // Arrange & Act
        var result = _card.GetStatusIndicator(isUnlocked: false, hasProgress: true);

        // Assert
        result.Should().Be("[~]");
    }

    [Test]
    public void GetStatusIndicator_WhenNotStarted_ReturnsEmptyParens()
    {
        // Arrange & Act
        var result = _card.GetStatusIndicator(isUnlocked: false, hasProgress: false);

        // Assert
        result.Should().Be("( )");
    }

    [Test]
    public void GetStatusIndicator_WhenUnlockedWithProgress_ReturnsCheckMark()
    {
        // Arrange & Act
        // If unlocked, hasProgress is irrelevant
        var result = _card.GetStatusIndicator(isUnlocked: true, hasProgress: true);

        // Assert
        result.Should().Be("[x]");
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER ACHIEVEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderAchievement_WithValidAchievement_CallsTerminalService()
    {
        // Arrange
        var achievement = CreateAchievementDto(
            "first-blood", "First Blood", "Defeat your first monster",
            AchievementCategory.Combat, AchievementTier.Bronze,
            isUnlocked: true);

        // Act
        var nextY = _card.RenderAchievement(achievement, 0, 0);

        // Assert
        nextY.Should().BeGreaterThan(0);
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void RenderAchievement_WithSecretLocked_ShowsPlaceholder()
    {
        // Arrange
        var achievement = CreateAchievementDto(
            "hidden-secret", "Secret Achievement", "This is secret",
            AchievementCategory.Secret, AchievementTier.Platinum,
            isUnlocked: false, isSecret: true);

        // Act
        var nextY = _card.RenderAchievement(achievement, 0, 0);

        // Assert
        nextY.Should().BeGreaterThan(0);
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.Is<string>(s => s.Contains("[L]"))),
            Times.AtLeastOnce);
    }

    [Test]
    public void RenderAchievement_WithSecretUnlocked_ShowsActualContent()
    {
        // Arrange
        var achievement = CreateAchievementDto(
            "hidden-secret", "Secret Achievement", "This is secret",
            AchievementCategory.Secret, AchievementTier.Platinum,
            isUnlocked: true, isSecret: true);

        // Act
        var nextY = _card.RenderAchievement(achievement, 0, 0);

        // Assert
        nextY.Should().BeGreaterThan(0);
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.Is<string>(s => s.Contains("[x]"))),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRarityBadge_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementCard(
            null!,
            _progressRenderer,
            _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("rarityBadge");
    }

    [Test]
    public void Constructor_WithNullProgressRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementCard(
            _rarityBadge,
            null!,
            _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("progressRenderer");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementCard(
            _rarityBadge,
            _progressRenderer,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    #region Test Helpers

    private static AchievementDisplayDto CreateAchievementDto(
        string id,
        string name,
        string description,
        AchievementCategory category,
        AchievementTier tier,
        bool isUnlocked = false,
        bool isSecret = false,
        int current = 0,
        int target = 1)
    {
        return new AchievementDisplayDto(
            Id: id,
            Name: name,
            Description: description,
            Category: category,
            Tier: tier,
            TargetValue: target,
            CurrentValue: isUnlocked ? target : current,
            IsUnlocked: isUnlocked,
            IsSecret: isSecret,
            PointValue: (int)tier,
            UnlockedAt: isUnlocked ? DateTime.UtcNow : null);
    }

    #endregion
}
