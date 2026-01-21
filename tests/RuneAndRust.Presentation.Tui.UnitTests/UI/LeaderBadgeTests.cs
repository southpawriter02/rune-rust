using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the <see cref="LeaderBadge"/> class.
/// </summary>
[TestFixture]
public class LeaderBadgeTests
{
    private MonsterGroupRenderer _renderer = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private MonsterGroupDisplayConfig _config = null!;
    private Mock<ILogger<LeaderBadge>> _mockLogger = null!;
    private LeaderBadge _leaderBadge = null!;

    [SetUp]
    public void Setup()
    {
        _config = MonsterGroupDisplayConfig.CreateDefault();
        _mockTerminal = new Mock<ITerminalService>();
        _renderer = new MonsterGroupRenderer(_config);
        _mockLogger = new Mock<ILogger<LeaderBadge>>();

        _leaderBadge = new LeaderBadge(
            _renderer,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER BADGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderBadge_WithLivingLeader_SetsCurrentStatus()
    {
        // Arrange
        var leaderStatus = new LeaderStatusDto(
            LeaderName: "Goblin Chief",
            LeaderRole: "leader",
            IsAlive: true,
            IsDefeated: false,
            MoraleEffects: Array.Empty<string>());

        // Act
        _leaderBadge.RenderBadge(leaderStatus);

        // Assert
        _leaderBadge.CurrentLeaderName.Should().Be("Goblin Chief");
        _leaderBadge.IsLeaderAlive.Should().BeTrue();
    }

    [Test]
    public void RenderBadge_WithDeadLeader_HidesBadge()
    {
        // Arrange
        var leaderStatus = new LeaderStatusDto(
            LeaderName: "Goblin Chief",
            LeaderRole: "leader",
            IsAlive: false,
            IsDefeated: true,
            MoraleEffects: Array.Empty<string>());

        // Act - Should not throw
        _leaderBadge.RenderBadge(leaderStatus);

        // Assert
        _leaderBadge.IsLeaderAlive.Should().BeFalse();
    }

    [Test]
    public void RenderBadge_WithNullStatus_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _leaderBadge.RenderBadge(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW LEADER DEFEATED TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowLeaderDefeated_WithDefeatedStatus_RendersEffectBox()
    {
        // Arrange
        var leaderStatus = new LeaderStatusDto(
            LeaderName: "Goblin Chief",
            LeaderRole: "leader",
            IsAlive: false,
            IsDefeated: true,
            MoraleEffects: new List<string>
            {
                "-25% attack for all remaining goblins",
                "50% chance to flee on each turn"
            });

        // Act
        _leaderBadge.ShowLeaderDefeated(leaderStatus);

        // Assert - Verify effect box title was written
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("LEADER DEFEATED")),
            _config.Colors.EffectBoxTitleColor), Times.Once);
    }

    [Test]
    public void ShowLeaderDefeated_WithMoraleEffects_RendersEffects()
    {
        // Arrange
        var leaderStatus = new LeaderStatusDto(
            LeaderName: "Goblin Chief",
            LeaderRole: "leader",
            IsAlive: false,
            IsDefeated: true,
            MoraleEffects: new List<string>
            {
                "-25% attack"
            });

        // Act
        _leaderBadge.ShowLeaderDefeated(leaderStatus);

        // Assert - Verify morale effect was written
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("-25% attack")),
            _config.Colors.NegativeEffectColor), Times.Once);
    }

    [Test]
    public void ShowLeaderDefeated_WhenNotDefeated_DoesNotRenderBox()
    {
        // Arrange - Leader alive, not defeated
        var leaderStatus = new LeaderStatusDto(
            LeaderName: "Goblin Chief",
            LeaderRole: "leader",
            IsAlive: true,
            IsDefeated: false,
            MoraleEffects: Array.Empty<string>());

        // Act
        _leaderBadge.ShowLeaderDefeated(leaderStatus);

        // Assert - Effect box should NOT be rendered
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("LEADER DEFEATED")),
            It.IsAny<ConsoleColor>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW LEADER BONUS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowLeaderBonus_WithBonuses_DisplaysBonuses()
    {
        // Arrange
        var bonuses = new List<string>
        {
            "+1 attack to all allies",
            "+2 damage on flanking hits"
        };

        // Act
        _leaderBadge.ShowLeaderBonus(bonuses);

        // Assert
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("Leader provides:")),
            _config.Colors.LeaderColor), Times.Once);
    }

    [Test]
    public void ShowLeaderBonus_WithEmptyList_DoesNotDisplay()
    {
        // Arrange
        var bonuses = new List<string>();

        // Act
        _leaderBadge.ShowLeaderBonus(bonuses);

        // Assert - No output
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<ConsoleColor>()), Times.Never);
    }

    [Test]
    public void ShowLeaderBonus_WithNull_DoesNotThrow()
    {
        // Act
        var act = () => _leaderBadge.ShowLeaderBonus(null);

        // Assert
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ResetsState()
    {
        // Arrange
        var leaderStatus = new LeaderStatusDto(
            "Goblin Chief", "leader", true, false, Array.Empty<string>());
        _leaderBadge.RenderBadge(leaderStatus);

        // Act
        _leaderBadge.Clear();

        // Assert
        _leaderBadge.CurrentLeaderName.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LeaderBadge(
            null!,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("renderer");
    }

    [Test]
    public void Constructor_WithNullTerminal_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LeaderBadge(
            _renderer,
            null!,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }
}
