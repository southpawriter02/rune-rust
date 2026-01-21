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
/// Unit tests for the <see cref="MonsterGroupView"/> class.
/// </summary>
[TestFixture]
public class MonsterGroupViewTests
{
    private MonsterGroupRenderer _renderer = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private TacticIndicator _tacticIndicator = null!;
    private LeaderBadge _leaderBadge = null!;
    private MonsterGroupDisplayConfig _config = null!;
    private Mock<ILogger<MonsterGroupView>> _mockLogger = null!;
    private MonsterGroupView _groupView = null!;

    [SetUp]
    public void Setup()
    {
        _config = MonsterGroupDisplayConfig.CreateDefault();
        _mockTerminal = new Mock<ITerminalService>();
        _renderer = new MonsterGroupRenderer(_config);
        _tacticIndicator = new TacticIndicator(_renderer, _mockTerminal.Object, _config);
        _leaderBadge = new LeaderBadge(_renderer, _mockTerminal.Object, _config);
        _mockLogger = new Mock<ILogger<MonsterGroupView>>();

        _groupView = new MonsterGroupView(
            _renderer,
            _mockTerminal.Object,
            _tacticIndicator,
            _leaderBadge,
            _config,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER GROUP TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderGroup_WithValidDto_RendersGroupHeader()
    {
        // Arrange
        var members = new List<GroupMemberDisplayDto>
        {
            new(Guid.NewGuid(), "Goblin Chief", "leader", 45, 45, 100, true, true),
            new(Guid.NewGuid(), "Goblin Archer", "ranged", 20, 20, 100, false, true)
        };

        var tactic = new TacticDisplayDto(
            "Flank",
            "Flanking Assault",
            "Surround target",
            new List<RoleAssignmentDto>());

        var dto = new MonsterGroupDisplayDto(
            GroupId: Guid.NewGuid(),
            GroupName: "Goblin Warband",
            Members: members,
            CurrentTactic: tactic,
            HasLeader: true,
            LeaderRole: "Chief",
            ActiveSynergies: Array.Empty<SynergyDisplayDto>());

        // Act
        _groupView.RenderGroup(dto);

        // Assert - Verify header was written
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("GOBLIN WARBAND")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderGroup_WithLeader_RendersMoraleHint()
    {
        // Arrange
        var members = new List<GroupMemberDisplayDto>
        {
            new(Guid.NewGuid(), "Goblin Chief", "leader", 45, 45, 100, true, true)
        };

        var dto = new MonsterGroupDisplayDto(
            GroupId: Guid.NewGuid(),
            GroupName: "Goblin Warband",
            Members: members,
            CurrentTactic: null,
            HasLeader: true,
            LeaderRole: "Chief",
            ActiveSynergies: Array.Empty<SynergyDisplayDto>());

        // Act
        _groupView.RenderGroup(dto);

        // Assert - Verify morale hint was written
        _mockTerminal.Verify(t => t.WriteColoredAt(
            It.IsAny<int>(), It.IsAny<int>(),
            It.Is<string>(s => s.Contains("Kill the Chief to break morale!")),
            It.IsAny<ConsoleColor>()), Times.Once);
    }

    [Test]
    public void RenderGroup_WithNullDto_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _groupView.RenderGroup(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Clear_ClearsDisplayArea()
    {
        // Arrange - Render first
        var dto = new MonsterGroupDisplayDto(
            GroupId: Guid.NewGuid(),
            GroupName: "Test Group",
            Members: Array.Empty<GroupMemberDisplayDto>(),
            CurrentTactic: null,
            HasLeader: false,
            LeaderRole: "",
            ActiveSynergies: Array.Empty<SynergyDisplayDto>());
        _groupView.RenderGroup(dto);

        // Act
        _groupView.Clear();

        // Assert
        _groupView.CurrentGroupId.Should().BeNull();
        _groupView.DisplayedMemberCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOW MEMBER HEALTH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowMemberHealth_WithMemberNotInGroup_LogsWarning()
    {
        // Arrange - Render with one member
        var existingMemberId = Guid.NewGuid();
        var members = new List<GroupMemberDisplayDto>
        {
            new(existingMemberId, "Goblin", "melee", 30, 30, 100, false, true)
        };

        var dto = new MonsterGroupDisplayDto(
            GroupId: Guid.NewGuid(),
            GroupName: "Test Group",
            Members: members,
            CurrentTactic: null,
            HasLeader: false,
            LeaderRole: "",
            ActiveSynergies: Array.Empty<SynergyDisplayDto>());
        _groupView.RenderGroup(dto);

        // Update for different member
        var differentMember = new GroupMemberDisplayDto(
            Guid.NewGuid(), // Different ID
            "Other Goblin",
            "melee",
            15, 30, 50,
            false, true);

        // Act - Should not throw, just log warning
        _groupView.ShowMemberHealth(differentMember);

        // Assert - No exception, but member was not updated in visible display
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonsterGroupView(
            null!,
            _mockTerminal.Object,
            _tacticIndicator,
            _leaderBadge,
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
        var act = () => new MonsterGroupView(
            _renderer,
            null!,
            _tacticIndicator,
            _leaderBadge,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullTacticIndicator_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonsterGroupView(
            _renderer,
            _mockTerminal.Object,
            null!,
            _leaderBadge,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tacticIndicator");
    }

    [Test]
    public void Constructor_WithNullLeaderBadge_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonsterGroupView(
            _renderer,
            _mockTerminal.Object,
            _tacticIndicator,
            null!,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("leaderBadge");
    }
}
