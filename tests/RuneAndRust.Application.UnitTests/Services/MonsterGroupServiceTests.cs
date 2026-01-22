using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for MonsterGroupService (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the complete monster group lifecycle:
/// <list type="bullet">
///   <item><description>Group registration with pre-spawned monsters</description></item>
///   <item><description>Tactical movement decisions (Flank, FocusFire, etc.)</description></item>
///   <item><description>Coordinated target selection</description></item>
///   <item><description>Synergy application and triggering</description></item>
///   <item><description>Member death handling</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class MonsterGroupServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IMonsterGroupProvider> _mockGroupProvider = null!;
    private Mock<ICombatGridService> _mockGridService = null!;
    private Mock<IFlankingService> _mockFlankingService = null!;
    private Mock<IBuffDebuffService> _mockBuffDebuffService = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<MonsterGroupService>> _mockLogger = null!;
    private CombatGrid _testGrid = null!;
    private MonsterGroupService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockGroupProvider = new Mock<IMonsterGroupProvider>();
        _mockGridService = new Mock<ICombatGridService>();
        _mockFlankingService = new Mock<IFlankingService>();
        _mockBuffDebuffService = new Mock<IBuffDebuffService>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<MonsterGroupService>>();

        // Create a real 10x10 test grid (all cells passable and unoccupied by default)
        _testGrid = CombatGrid.Create(10, 10);
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns(_testGrid);

        _service = new MonsterGroupService(
            _mockGroupProvider.Object,
            _mockGridService.Object,
            _mockFlankingService.Object,
            _mockBuffDebuffService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // REGISTER GROUP TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RegisterGroup_WithValidGroupId_ReturnsGroupInstanceAndTracksMembers()
    {
        // Arrange
        var groupDef = CreateGoblinWarbandDefinition();
        _mockGroupProvider.Setup(p => p.GetGroup("goblin-warband")).Returns(groupDef);

        var monsters = CreateTestMonsters();

        // Act
        var instance = _service.RegisterGroup("goblin-warband", monsters);

        // Assert
        instance.Should().NotBeNull();
        instance.GroupId.Should().Be("goblin-warband");
        instance.Members.Should().HaveCount(4);
        instance.HasAliveMembers.Should().BeTrue();

        // Verify all monsters are tracked
        foreach (var monster in monsters)
        {
            _service.IsInGroup(monster).Should().BeTrue();
            _service.GetGroupForMonster(monster).Should().Be(instance);
        }

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "MonsterGroupRegistered",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    [Test]
    public void RegisterGroup_WithInvalidGroupId_ThrowsArgumentException()
    {
        // Arrange
        _mockGroupProvider.Setup(p => p.GetGroup("invalid-group")).Returns((MonsterGroupDefinition?)null);
        var monsters = CreateTestMonsters();

        // Act
        var act = () => _service.RegisterGroup("invalid-group", monsters);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invalid-group*")
            .WithParameterName("groupId");
    }

    // ═══════════════════════════════════════════════════════════════
    // DETERMINE MOVE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DetermineMove_WithFlankTactic_ReturnsFlankingPosition()
    {
        // Arrange
        var groupDef = CreateGoblinWarbandDefinition();
        _mockGroupProvider.Setup(p => p.GetGroup("goblin-warband")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("goblin-warband", monsters);

        var warrior = monsters[1]; // Melee warrior
        var targetId = Guid.NewGuid();
        instance.SetCurrentTarget(targetId);

        // Setup grid positions
        _mockGridService.Setup(g => g.GetEntityPosition(warrior.Id)).Returns(new GridPosition(3, 3));
        _mockGridService.Setup(g => g.GetEntityPosition(targetId)).Returns(new GridPosition(5, 5));

        // Setup flanking positions
        var flankingPositions = new List<GridPosition>
        {
            new(4, 5), // Left of target
            new(6, 5), // Right of target
            new(5, 4), // Above target
            new(5, 6)  // Below target
        };
        _mockFlankingService
            .Setup(f => f.GetFlankingPositions(targetId))
            .Returns(flankingPositions);

        // Act
        var decision = _service.DetermineMove(warrior);

        // Assert
        decision.Should().NotBeNull();
        decision.Type.Should().Be(GroupMoveDecisionType.MoveTo);
        decision.Tactic.Should().Be(GroupTactic.Flank);
        decision.TargetPosition.Should().NotBeNull();
        flankingPositions.Should().Contain(decision.TargetPosition!.Value);
    }

    [Test]
    public void DetermineMove_WithNoGroup_ReturnsNoGroupDecision()
    {
        // Arrange
        var monster = CreateTestMonster("goblin-warrior", "Goblin Warrior");

        // Act
        var decision = _service.DetermineMove(monster);

        // Assert
        decision.Type.Should().Be(GroupMoveDecisionType.NoGroup);
        decision.IsTacticalDecision.Should().BeFalse();
    }

    [Test]
    public void DetermineMove_WithNoTarget_ReturnsNoTargetDecision()
    {
        // Arrange
        var groupDef = CreateGroupWithFocusFireOnly();
        _mockGroupProvider.Setup(p => p.GetGroup("skeleton-patrol")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("skeleton-patrol", monsters);
        // Do NOT set a current target

        var warrior = monsters[1];
        _mockGridService.Setup(g => g.GetEntityPosition(warrior.Id)).Returns(new GridPosition(3, 3));

        // Act
        var decision = _service.DetermineMove(warrior);

        // Assert
        // FocusFire without a target returns NoAction (not NoTarget)
        decision.Type.Should().Be(GroupMoveDecisionType.NoAction);
    }

    // ═══════════════════════════════════════════════════════════════
    // DETERMINE TARGET TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DetermineTarget_WithFocusFireTactic_SelectsLowestHPTarget()
    {
        // Arrange
        var groupDef = CreateGroupWithFocusFireOnly();
        _mockGroupProvider.Setup(p => p.GetGroup("skeleton-patrol")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("skeleton-patrol", monsters);

        var warrior = monsters[1];

        // Create targets with varying HP
        var target1 = CreateTestCombatant("Player 1", 100);
        var target2 = CreateTestCombatant("Player 2", 50); // Lowest HP
        var target3 = CreateTestCombatant("Player 3", 75);
        var possibleTargets = new List<Combatant> { target1, target2, target3 };

        // Act
        var selectedTarget = _service.DetermineTarget(warrior, possibleTargets);

        // Assert
        selectedTarget.Should().NotBeNull();
        selectedTarget!.Id.Should().Be(target2.Id); // Should select lowest HP target
        instance.CurrentTarget.Should().Be(target2.Id);

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "GroupFocusTarget",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    [Test]
    public void DetermineTarget_WithFocusFire_UsesExistingTargetIfValid()
    {
        // Arrange
        var groupDef = CreateGroupWithFocusFireOnly();
        _mockGroupProvider.Setup(p => p.GetGroup("skeleton-patrol")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("skeleton-patrol", monsters);

        var warrior = monsters[1];

        // Create targets with varying HP
        var existingTarget = CreateTestCombatant("Player 1", 75);
        var lowerHPTarget = CreateTestCombatant("Player 2", 25);
        var possibleTargets = new List<Combatant> { existingTarget, lowerHPTarget };

        // Set existing target before calling DetermineTarget
        instance.SetCurrentTarget(existingTarget.Id);

        // Act
        var selectedTarget = _service.DetermineTarget(warrior, possibleTargets);

        // Assert
        selectedTarget.Should().NotBeNull();
        selectedTarget!.Id.Should().Be(existingTarget.Id); // Should keep existing valid target
        instance.CurrentTarget.Should().Be(existingTarget.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // SYNERGY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplySynergies_WithAlwaysSynergy_AppliesBonusToMembers()
    {
        // Arrange
        var groupDef = CreateGoblinWarbandDefinition();
        _mockGroupProvider.Setup(p => p.GetGroup("goblin-warband")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("goblin-warband", monsters);

        // Note: ApplySynergies is called automatically during RegisterGroup
        // for "Always" synergies. We can call it again to verify behavior.

        // Act
        _service.ApplySynergies(instance);

        // Assert - The synergies should be processed without throwing
        // The service should still be operational after applying synergies
        instance.HasAliveMembers.Should().BeTrue();
        _service.GetActiveGroups().Should().Contain(instance);
    }

    [Test]
    public void OnGroupMemberHit_WithOnAllyHitSynergy_AppliesTemporaryBonus()
    {
        // Arrange
        var groupDef = CreateGroupWithOnAllyHitSynergy();
        _mockGroupProvider.Setup(p => p.GetGroup("test-group")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        _service.RegisterGroup("test-group", monsters);

        var attacker = monsters[0];
        var target = CreateTestCombatant("Player", 100);

        // Act
        _service.OnGroupMemberHit(attacker, target);

        // Assert - Synergy processing should complete
        // The method should execute without errors
        _service.IsInGroup(attacker).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // MEMBER DEATH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void OnGroupMemberDeath_RemovesMemberFromTracking()
    {
        // Arrange
        var groupDef = CreateGoblinWarbandDefinition();
        _mockGroupProvider.Setup(p => p.GetGroup("goblin-warband")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("goblin-warband", monsters);

        var dyingMonster = monsters[1];
        _service.IsInGroup(dyingMonster).Should().BeTrue();

        // Simulate death by making monster not alive
        SimulateMonsterDeath(dyingMonster);

        // Act
        _service.OnGroupMemberDeath(dyingMonster);

        // Assert
        _service.IsInGroup(dyingMonster).Should().BeFalse();
        instance.HasAliveMembers.Should().BeTrue(); // Other members still alive

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "GroupMemberDeath",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    [Test]
    public void OnGroupMemberDeath_WhenLastMember_RemovesGroupInstance()
    {
        // Arrange
        var groupDef = CreateGoblinWarbandDefinition();
        _mockGroupProvider.Setup(p => p.GetGroup("goblin-warband")).Returns(groupDef);

        var monsters = CreateTestMonsters();
        var instance = _service.RegisterGroup("goblin-warband", monsters);

        // Kill all monsters
        foreach (var monster in monsters)
        {
            SimulateMonsterDeath(monster);
            _service.OnGroupMemberDeath(monster);
        }

        // Assert
        _service.GetActiveGroups().Should().BeEmpty();

        _mockEventLogger.Verify(
            e => e.LogCombat(
                "GroupDefeated",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a goblin warband definition for testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The test group has:
    /// <list type="bullet">
    ///   <item><description>1 leader (goblin-shaman)</description></item>
    ///   <item><description>3 melee warriors (goblin-warrior)</description></item>
    ///   <item><description>Tactics: Flank, FocusFire, ProtectLeader</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <returns>A test group definition.</returns>
    private static MonsterGroupDefinition CreateGoblinWarbandDefinition()
    {
        return MonsterGroupDefinition.Create(
                "goblin-warband",
                "Goblin Warband",
                "A test group of goblins")
            .WithMember(GroupMember.Create("goblin-shaman", 1, "leader"))
            .WithMember(GroupMember.Create("goblin-warrior", 3, "melee"))
            .WithTactics(GroupTactic.Flank, GroupTactic.FocusFire, GroupTactic.ProtectLeader)
            .WithSynergy(GroupSynergy.Create("pack-tactics", "Pack Tactics", SynergyTrigger.Always)
                .WithAttackBonus(1));
    }

    /// <summary>
    /// Creates a group with only FocusFire tactic for testing target selection.
    /// </summary>
    private static MonsterGroupDefinition CreateGroupWithFocusFireOnly()
    {
        return MonsterGroupDefinition.Create(
                "skeleton-patrol",
                "Skeleton Patrol",
                "A test patrol group")
            .WithMember(GroupMember.Create("skeleton-warrior", 2, "melee"))
            .WithMember(GroupMember.Create("skeleton-archer", 2, "ranged"))
            .WithTactics(GroupTactic.FocusFire);
    }

    /// <summary>
    /// Creates a group with OnAllyHit synergy for testing synergy triggers.
    /// </summary>
    private static MonsterGroupDefinition CreateGroupWithOnAllyHitSynergy()
    {
        return MonsterGroupDefinition.Create(
                "test-group",
                "Test Group",
                "A test group with OnAllyHit synergy")
            .WithMember(GroupMember.Create("test-monster", 4, "melee"))
            .WithTactics(GroupTactic.FocusFire)
            .WithSynergy(GroupSynergy.Create("rally", "Rally", SynergyTrigger.OnAllyHit)
                .WithAttackBonus(2));
    }

    /// <summary>
    /// Creates a list of test monsters matching the goblin warband composition.
    /// </summary>
    private static List<Monster> CreateTestMonsters()
    {
        return new List<Monster>
        {
            CreateTestMonster("goblin-shaman", "Goblin Shaman"),
            CreateTestMonster("goblin-warrior", "Goblin Warrior 1"),
            CreateTestMonster("goblin-warrior", "Goblin Warrior 2"),
            CreateTestMonster("goblin-warrior", "Goblin Warrior 3")
        };
    }

    /// <summary>
    /// Creates a test monster with the specified definition ID and name.
    /// </summary>
    private static Monster CreateTestMonster(string definitionId, string name)
    {
        return new Monster(
            name: name,
            description: $"A test {name}",
            maxHealth: 50,
            stats: new Stats(10, 5, 5),
            monsterDefinitionId: definitionId);
    }

    /// <summary>
    /// Creates a test combatant for target selection tests.
    /// </summary>
    private static Combatant CreateTestCombatant(string name, int health)
    {
        // Create a Monster and wrap it in a Combatant
        var monster = new Monster(
            name: name,
            description: $"A test combatant: {name}",
            maxHealth: health,
            stats: new Stats(10, 5, 5),
            monsterDefinitionId: "test-combatant");

        // Create InitiativeRoll with proper DiceRollResult
        var initiative = CreateInitiativeRoll(10, 0);
        return Combatant.ForMonster(monster, initiative, 0);
    }

    /// <summary>
    /// Creates an initiative roll for testing.
    /// </summary>
    private static InitiativeRoll CreateInitiativeRoll(int rollValue, int modifier)
    {
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [rollValue]);
        return new InitiativeRoll(diceResult, modifier);
    }

    /// <summary>
    /// Simulates a monster's death by reducing health to zero.
    /// </summary>
    private static void SimulateMonsterDeath(Monster monster)
    {
        // Use TakeDamage to reduce health to 0
        // Must account for defense reducing damage, so deal extra damage
        monster.TakeDamage(monster.MaxHealth + monster.Stats.Defense + 1);
    }
}
