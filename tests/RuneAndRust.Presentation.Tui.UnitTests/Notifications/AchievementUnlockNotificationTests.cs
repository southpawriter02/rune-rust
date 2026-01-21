// ═══════════════════════════════════════════════════════════════════════════════
// AchievementUnlockNotificationTests.cs
// Unit tests for AchievementUnlockNotification.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Notifications;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.Notifications;

/// <summary>
/// Unit tests for <see cref="AchievementUnlockNotification"/>.
/// </summary>
[TestFixture]
public class AchievementUnlockNotificationTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private RarityBadge _rarityBadge = null!;
    private AchievementPanelConfig _config = null!;
    private AchievementUnlockNotification _notification = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _rarityBadge = new RarityBadge(_mockTerminal.Object);
        _config = new AchievementPanelConfig
        {
            NotificationDurationSeconds = 5,
            NotificationWidth = 55
        };

        _notification = new AchievementUnlockNotification(
            _rarityBadge,
            _mockTerminal.Object,
            _config,
            NullLogger<AchievementUnlockNotification>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // DURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DurationSeconds_ReturnsConfiguredDuration()
    {
        // Arrange & Act
        var duration = _notification.DurationSeconds;

        // Assert
        duration.Should().Be(5);
    }

    [Test]
    public void DurationSeconds_WithCustomConfig_ReturnsCustomDuration()
    {
        // Arrange
        var customConfig = new AchievementPanelConfig
        {
            NotificationDurationSeconds = 10
        };
        var notification = new AchievementUnlockNotification(
            _rarityBadge,
            _mockTerminal.Object,
            customConfig);

        // Act
        var duration = notification.DurationSeconds;

        // Assert
        duration.Should().Be(10);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW UNLOCK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowUnlock_WithValidAchievement_CallsTerminalService()
    {
        // Arrange
        var achievement = CreateAchievementDto(
            "first-blood", "First Blood", "Defeat your first monster",
            AchievementTier.Bronze);

        // Act
        _notification.ShowUnlock(achievement, centerX: 40, topY: 5);

        // Assert
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
        _mockTerminal.Verify(
            t => t.WriteColoredAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void ShowUnlock_DisplaysAchievementUnlockedTitle()
    {
        // Arrange
        var achievement = CreateAchievementDto(
            "first-blood", "First Blood", "Defeat your first monster",
            AchievementTier.Bronze);

        // Act
        _notification.ShowUnlock(achievement, centerX: 40, topY: 5);

        // Assert
        _mockTerminal.Verify(
            t => t.WriteColoredAt(It.IsAny<int>(), It.IsAny<int>(), 
                It.Is<string>(s => s.Contains("ACHIEVEMENT UNLOCKED")), 
                It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void ShowUnlock_WithNullAchievement_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _notification.ShowUnlock(null!, centerX: 40, topY: 5);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ClearsNotificationArea()
    {
        // Arrange & Act
        _notification.Clear(centerX: 40, topY: 5);

        // Assert - Should write empty lines to clear the area
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.Is<string>(s => s.Trim().Length == 0)),
            Times.AtLeast(8)); // 8 lines in notification
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRarityBadge_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementUnlockNotification(
            null!,
            _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("rarityBadge");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AchievementUnlockNotification(
            _rarityBadge,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefaults()
    {
        // Arrange & Act
        var notification = new AchievementUnlockNotification(
            _rarityBadge,
            _mockTerminal.Object,
            config: null);

        // Assert - Default duration is 5 seconds
        notification.DurationSeconds.Should().Be(5);
    }

    #region Test Helpers

    private static AchievementDisplayDto CreateAchievementDto(
        string id,
        string name,
        string description,
        AchievementTier tier)
    {
        return new AchievementDisplayDto(
            Id: id,
            Name: name,
            Description: description,
            Category: AchievementCategory.Combat,
            Tier: tier,
            TargetValue: 1,
            CurrentValue: 1,
            IsUnlocked: true,
            IsSecret: false,
            PointValue: (int)tier,
            UnlockedAt: DateTime.UtcNow);
    }

    #endregion
}
