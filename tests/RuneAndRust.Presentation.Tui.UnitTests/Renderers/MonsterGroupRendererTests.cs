using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for the <see cref="MonsterGroupRenderer"/> class.
/// </summary>
[TestFixture]
public class MonsterGroupRendererTests
{
    private MonsterGroupRenderer _renderer = null!;
    private MonsterGroupDisplayConfig _config = null!;
    private Mock<ILogger<MonsterGroupRenderer>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _config = MonsterGroupDisplayConfig.CreateDefault();
        _mockLogger = new Mock<ILogger<MonsterGroupRenderer>>();
        _renderer = new MonsterGroupRenderer(_config, _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // GROUP HEADER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatGroupHeader_WithGroupName_ReturnsCenteredHeader()
    {
        // Arrange
        var groupName = "Goblin Warband";
        var totalWidth = 60;

        // Act
        var result = _renderer.FormatGroupHeader(groupName, totalWidth);

        // Assert
        result.Should().Contain("GOBLIN WARBAND");
        result.Length.Should().Be(totalWidth);
    }

    [Test]
    public void FormatGroupHeader_WithShortName_CentersCorrectly()
    {
        // Arrange
        var groupName = "Pack";
        var totalWidth = 20;

        // Act
        var result = _renderer.FormatGroupHeader(groupName, totalWidth);

        // Assert
        result.Should().Contain("PACK");
        result.Length.Should().Be(totalWidth);
        // PACK is 4 chars, padding should be (20-4)/2 = 8 on each side
        result.Trim().Should().Be("PACK");
    }

    [Test]
    public void FormatGroupHeader_WithLongName_DoesNotTruncate()
    {
        // Arrange
        var groupName = "The Elite Royal Guard Squadron";
        var totalWidth = 20;

        // Act
        var result = _renderer.FormatGroupHeader(groupName, totalWidth);

        // Assert
        result.Should().Contain("THE ELITE ROYAL GUARD SQUADRON");
    }

    [Test]
    public void FormatGroupHeader_WithNullName_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _renderer.FormatGroupHeader(null!, 60);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // MEMBER CARD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatMemberCard_WithLeader_IncludesLeaderBadge()
    {
        // Arrange
        var member = new GroupMemberDisplayDto(
            MonsterId: Guid.NewGuid(),
            MemberName: "Goblin Chief",
            Role: "leader",
            CurrentHealth: 45,
            MaxHealth: 45,
            HealthPercent: 100,
            IsLeader: true,
            IsAlive: true);

        // Act
        var result = _renderer.FormatMemberCard(member, 15);

        // Assert
        result.Should().Contain("[*]");
        result.Should().Contain("LEADER");
    }

    [Test]
    public void FormatMemberCard_WithNonLeader_ShowsRoleOnly()
    {
        // Arrange
        var member = new GroupMemberDisplayDto(
            MonsterId: Guid.NewGuid(),
            MemberName: "Goblin Archer",
            Role: "ranged",
            CurrentHealth: 20,
            MaxHealth: 20,
            HealthPercent: 100,
            IsLeader: false,
            IsAlive: true);

        // Act
        var result = _renderer.FormatMemberCard(member, 15);

        // Assert
        result.Should().NotContain("[*]");
        result.Should().Contain("ranged");
        result.Should().Contain("Goblin");
        result.Should().Contain("Archer");
    }

    [Test]
    public void FormatMemberCard_IncludesHealthText()
    {
        // Arrange
        var member = new GroupMemberDisplayDto(
            MonsterId: Guid.NewGuid(),
            MemberName: "Goblin",
            Role: "melee",
            CurrentHealth: 15,
            MaxHealth: 30,
            HealthPercent: 50,
            IsLeader: false,
            IsAlive: true);

        // Act
        var result = _renderer.FormatMemberCard(member, 15);

        // Assert
        result.Should().Contain("HP: 15/30");
    }

    [Test]
    public void FormatMemberCard_IncludesCardBorders()
    {
        // Arrange
        var member = new GroupMemberDisplayDto(
            MonsterId: Guid.NewGuid(),
            MemberName: "Test",
            Role: "melee",
            CurrentHealth: 10,
            MaxHealth: 10,
            HealthPercent: 100,
            IsLeader: false,
            IsAlive: true);

        // Act
        var result = _renderer.FormatMemberCard(member, 15);

        // Assert
        result.Should().Contain("┌");
        result.Should().Contain("┐");
        result.Should().Contain("└");
        result.Should().Contain("┘");
        result.Should().Contain("│");
        result.Should().Contain("─");
    }

    // ═══════════════════════════════════════════════════════════════
    // HEALTH COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(100, ExpectedResult = ConsoleColor.Green)]
    [TestCase(76, ExpectedResult = ConsoleColor.Green)]
    [TestCase(75, ExpectedResult = ConsoleColor.Yellow)]
    [TestCase(51, ExpectedResult = ConsoleColor.Yellow)]
    [TestCase(50, ExpectedResult = ConsoleColor.DarkYellow)]
    [TestCase(26, ExpectedResult = ConsoleColor.DarkYellow)]
    [TestCase(25, ExpectedResult = ConsoleColor.Red)]
    [TestCase(1, ExpectedResult = ConsoleColor.Red)]
    [TestCase(0, ExpectedResult = ConsoleColor.DarkRed)]
    public ConsoleColor GetMemberHealthColor_WithHealthPercent_ReturnsCorrectColor(int healthPercent)
    {
        return _renderer.GetMemberHealthColor(healthPercent);
    }

    // ═══════════════════════════════════════════════════════════════
    // LEADER BADGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetLeaderBadgeSymbol_ReturnsDefaultSymbol()
    {
        // Act
        var result = _renderer.GetLeaderBadgeSymbol();

        // Assert
        result.Should().Be("[*]");
    }

    // ═══════════════════════════════════════════════════════════════
    // TACTIC FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatTacticDescription_WithRoleAssignments_ReturnsFormattedText()
    {
        // Arrange
        var tactic = new TacticDisplayDto(
            TacticType: "Flank",
            TacticName: "Flanking Assault",
            Description: "Surround target for flanking bonuses",
            RoleAssignments: new List<RoleAssignmentDto>
            {
                new("Archer", "Attack from range"),
                new("Chief", "Engage in melee")
            });

        // Act
        var result = _renderer.FormatTacticDescription(tactic);

        // Assert
        result.Should().Contain("TACTIC: Flanking Assault");
        result.Should().Contain("|-- Archer: Attack from range");
        result.Should().Contain("+-- Chief: Engage in melee");
    }

    [Test]
    public void FormatRoleAssignment_WithNonLastItem_UsesBranchPrefix()
    {
        // Act
        var result = _renderer.FormatRoleAssignment("Archer", "Attack from range", isLast: false);

        // Assert
        result.Should().Be("|-- Archer: Attack from range");
    }

    [Test]
    public void FormatRoleAssignment_WithLastItem_UsesEndPrefix()
    {
        // Act
        var result = _renderer.FormatRoleAssignment("Chief", "Engage in melee", isLast: true);

        // Assert
        result.Should().Be("+-- Chief: Engage in melee");
    }

    // ═══════════════════════════════════════════════════════════════
    // SYNERGY FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatSynergyText_WhenTriggered_ReturnsTriggeredFormat()
    {
        // Arrange
        var synergy = new SynergyDisplayDto(
            SynergyId: "pack-tactics",
            SynergyName: "Pack Tactics",
            EffectDescription: "+2 attack for next attack",
            IsTriggered: true,
            SourceRole: null);

        // Act
        var result = _renderer.FormatSynergyText(synergy);

        // Assert
        result.Should().Be("SYNERGY: Pack Tactics triggered! (+2 attack for next attack)");
    }

    [Test]
    public void FormatSynergyText_WhenActive_ReturnsActiveFormat()
    {
        // Arrange
        var synergy = new SynergyDisplayDto(
            SynergyId: "shamans-blessing",
            SynergyName: "Shaman's Blessing",
            EffectDescription: "+1 attack to all allies",
            IsTriggered: false,
            SourceRole: "Shaman");

        // Act
        var result = _renderer.FormatSynergyText(synergy);

        // Assert
        result.Should().Be("Active: Shaman's Blessing (+1 attack to all allies)");
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullConfig_UsesDefault()
    {
        // Act
        var renderer = new MonsterGroupRenderer(null, null);

        // Assert
        renderer.GetConfig().Should().NotBeNull();
        renderer.GetConfig().TotalWidth.Should().Be(72);
    }
}
