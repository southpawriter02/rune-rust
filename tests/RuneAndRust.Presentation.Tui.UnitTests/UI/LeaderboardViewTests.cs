// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardViewTests.cs
// Unit tests for the LeaderboardView class.
// Version: 0.13.4c
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
/// Unit tests for LeaderboardView functionality.
/// </summary>
[TestFixture]
public class LeaderboardViewTests
{
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private LeaderboardEntryRenderer _entryRenderer = null!;
    private PersonalBestHighlight _personalBestHighlight = null!;
    private LeaderboardViewConfig _config = null!;
    private LeaderboardView _view = null!;

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        _config = new LeaderboardViewConfig();
        _entryRenderer = new LeaderboardEntryRenderer();
        _personalBestHighlight = new PersonalBestHighlight(_terminalServiceMock.Object, _config);
        _view = new LeaderboardView(
            _entryRenderer,
            _personalBestHighlight,
            _terminalServiceMock.Object,
            _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new LeaderboardView(
            null!,
            _personalBestHighlight,
            _terminalServiceMock.Object,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("entryRenderer");
    }

    [Test]
    public void Constructor_WithNullPersonalBest_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new LeaderboardView(
            _entryRenderer,
            null!,
            _terminalServiceMock.Object,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("personalBestHighlight");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new LeaderboardView(
            _entryRenderer,
            _personalBestHighlight,
            null!,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_DefaultsToHighScoreCategory()
    {
        // Assert
        _view.ActiveCategory.Should().Be(LeaderboardCategory.HighScore);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CATEGORY SELECTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectCategory_ChangesActiveCategory()
    {
        // Act
        _view.SelectCategory(LeaderboardCategory.Speedrun);

        // Assert
        _view.ActiveCategory.Should().Be(LeaderboardCategory.Speedrun);
    }

    [Test]
    public void HandleCategoryInput_Key1_SelectsHighScore()
    {
        // Arrange
        _view.SelectCategory(LeaderboardCategory.Speedrun);

        // Act
        var result = _view.HandleCategoryInput(1);

        // Assert
        result.Should().BeTrue();
        _view.ActiveCategory.Should().Be(LeaderboardCategory.HighScore);
    }

    [Test]
    public void HandleCategoryInput_Key2_SelectsSpeedrun()
    {
        // Act
        var result = _view.HandleCategoryInput(2);

        // Assert
        result.Should().BeTrue();
        _view.ActiveCategory.Should().Be(LeaderboardCategory.Speedrun);
    }

    [Test]
    public void HandleCategoryInput_Key3_SelectsNoDeath()
    {
        // Act
        var result = _view.HandleCategoryInput(3);

        // Assert
        result.Should().BeTrue();
        _view.ActiveCategory.Should().Be(LeaderboardCategory.NoDeath);
    }

    [Test]
    public void HandleCategoryInput_Key4_SelectsAchievementPoints()
    {
        // Act
        var result = _view.HandleCategoryInput(4);

        // Assert
        result.Should().BeTrue();
        _view.ActiveCategory.Should().Be(LeaderboardCategory.AchievementPoints);
    }

    [Test]
    public void HandleCategoryInput_Key5_SelectsBossSlayer()
    {
        // Act
        var result = _view.HandleCategoryInput(5);

        // Assert
        result.Should().BeTrue();
        _view.ActiveCategory.Should().Be(LeaderboardCategory.BossSlayer);
    }

    [Test]
    public void HandleCategoryInput_InvalidKey_ReturnsFalse()
    {
        // Act
        var result = _view.HandleCategoryInput(9);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HandleCategoryInput_SameCategory_ReturnsFalse()
    {
        // Arrange: Already on HighScore
        // Act
        var result = _view.HandleCategoryInput(1);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderLeaderboard_EmptyEntries_DoesNotThrow()
    {
        // Arrange
        var entries = Array.Empty<LeaderboardDisplayDto>();

        // Act
        var act = () => _view.RenderLeaderboard(entries);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void RenderLeaderboard_WritesToTerminal()
    {
        // Arrange
        var entries = new[]
        {
            new LeaderboardDisplayDto
            {
                Rank = 1,
                PlayerName = "Player1",
                CharacterClass = "Warrior",
                Level = 10,
                Score = 50000,
                Date = DateTime.Today,
                Category = LeaderboardCategory.HighScore
            }
        };

        // Act
        _view.RenderLeaderboard(entries);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLAYER HIGHLIGHT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SetCurrentPlayer_SetsPlayerIdentifier()
    {
        // Act & Assert (no exception means success)
        var act = () => _view.SetCurrentPlayer("TestPlayer");
        act.Should().NotThrow();
    }

    [Test]
    public void SetPersonalBest_SetsPlayerBest()
    {
        // Arrange
        var personalBest = new LeaderboardDisplayDto
        {
            Rank = 5,
            PlayerName = "TestPlayer",
            CharacterClass = "Mage",
            Level = 8,
            Score = 32000,
            Date = DateTime.Today,
            Category = LeaderboardCategory.HighScore
        };

        // Act & Assert
        var act = () => _view.SetPersonalBest(personalBest);
        act.Should().NotThrow();
    }

    [Test]
    public void SetPosition_SetsRenderPosition()
    {
        // Act & Assert
        var act = () => _view.SetPosition(10, 5);
        act.Should().NotThrow();
    }
}
