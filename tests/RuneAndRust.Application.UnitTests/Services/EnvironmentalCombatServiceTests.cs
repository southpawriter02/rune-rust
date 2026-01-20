using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for EnvironmentalCombatService.
/// </summary>
/// <remarks>
/// <para>Tests cover all environmental combat operations:</para>
/// <list type="bullet">
///   <item><description>Push operations with opposed Might checks</description></item>
///   <item><description>Knockback operations (forced movement)</description></item>
///   <item><description>Hazard detection and damage application</description></item>
///   <item><description>Status effect application from hazards</description></item>
///   <item><description>Critical knockback processing</description></item>
///   <item><description>Direction calculation methods</description></item>
///   <item><description>Per-turn hazard tick damage</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class EnvironmentalCombatServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<IEnvironmentalHazardProvider> _mockHazardProvider = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IBuffDebuffService> _mockBuffDebuffService = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<EnvironmentalCombatService>> _mockLogger = null!;
    private EnvironmentalCombatService _service = null!;

    // Test hazard definitions
    private EnvironmentalHazardDefinition _lavaHazard = null!;
    private EnvironmentalHazardDefinition _spikesHazard = null!;
    private EnvironmentalHazardDefinition _pitHazard = null!;
    private EnvironmentalHazardDefinition _poisonGasHazard = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockHazardProvider = new Mock<IEnvironmentalHazardProvider>();
        _mockDiceService = new Mock<IDiceService>();
        _mockBuffDebuffService = new Mock<IBuffDebuffService>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<EnvironmentalCombatService>>();

        // Create test hazard definitions
        _lavaHazard = EnvironmentalHazardDefinition.Create(
            HazardType.Lava,
            "Lava",
            damageDice: "3d6",
            damageType: "fire",
            damageOnEnter: true,
            damagePerTurn: true,
            statusEffectId: "burning");

        _spikesHazard = EnvironmentalHazardDefinition.Create(
            HazardType.Spikes,
            "Spike Trap",
            damageDice: "2d6",
            damageType: "piercing",
            damageOnEnter: true,
            damagePerTurn: false,
            statusEffectId: "bleeding");

        _pitHazard = EnvironmentalHazardDefinition.Create(
            HazardType.Pit,
            "Pit",
            damageDice: "2d6",
            damageType: "bludgeoning",
            damageOnEnter: true,
            damagePerTurn: false,
            statusEffectId: "prone",
            requiresClimbOut: true);

        _poisonGasHazard = EnvironmentalHazardDefinition.Create(
            HazardType.PoisonGas,
            "Poison Gas",
            damageDice: "1d6",
            damageType: "poison",
            damageOnEnter: false,
            damagePerTurn: true,
            statusEffectId: "poisoned");

        // Setup default provider behavior
        SetupDefaultHazardProvider();
        SetupDefaultDiceService();
        SetupDefaultBuffDebuffService();

        _service = new EnvironmentalCombatService(
            _mockHazardProvider.Object,
            _mockDiceService.Object,
            _mockBuffDebuffService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // PUSH TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Push moves the target in the correct direction when the pusher wins the opposed check.
    /// </summary>
    [Test]
    public void Push_SuccessfulCheck_MovesTargetInDirection()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();

        // Place entities on grid
        grid.PlaceEntity(pusher.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Setup dice to make pusher win (pusher rolls 15, target rolls 5)
        var rollSequence = new Queue<int>(new[] { 15, 5 });
        _mockDiceService.Setup(d => d.RollTotal("1d20"))
            .Returns(() => rollSequence.Dequeue());

        // Act
        var result = _service.Push(pusher, target, grid, MovementDirection.East, distance: 1);

        // Assert
        result.WasPushed.Should().BeTrue();
        result.WasResisted.Should().BeFalse();
        result.CellsMoved.Should().Be(1);
        result.EndPosition.X.Should().Be(5);
        result.EndPosition.Y.Should().Be(3);
    }

    /// <summary>
    /// Verifies that Push uses opposed Might (STR) check mechanics.
    /// </summary>
    [Test]
    public void Push_UsesOpposedStrengthCheck()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(pusher.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Setup dice for verification
        _mockDiceService.Setup(d => d.RollTotal("1d20")).Returns(10);

        // Act
        _service.Push(pusher, target, grid, MovementDirection.East);

        // Assert - verify d20 was rolled twice (once for pusher, once for target)
        _mockDiceService.Verify(d => d.RollTotal("1d20"), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that the target wins ties (resists the push).
    /// </summary>
    [Test]
    public void Push_TargetWins_ReturnsResisted()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(pusher.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Setup dice so target wins the tie (both roll 10)
        _mockDiceService.Setup(d => d.RollTotal("1d20")).Returns(10);

        // Act
        var result = _service.Push(pusher, target, grid, MovementDirection.East);

        // Assert - target wins ties, so push should be resisted
        result.WasResisted.Should().BeTrue();
        result.WasPushed.Should().BeFalse();
        result.CellsMoved.Should().Be(0);
    }

    /// <summary>
    /// Verifies that push stops when blocked by a wall (out of bounds).
    /// </summary>
    [Test]
    public void Push_BlockedByWall_StopsAtWall()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();

        // Place target near edge
        grid.PlaceEntity(pusher.Id, new GridPosition(5, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(6, 3), isPlayer: false);

        // Setup dice so pusher wins with big roll
        var rollSequence = new Queue<int>(new[] { 20, 1 });
        _mockDiceService.Setup(d => d.RollTotal("1d20"))
            .Returns(() => rollSequence.Dequeue());

        // Act - try to push 3 cells (but wall at edge)
        var result = _service.Push(pusher, target, grid, MovementDirection.East, distance: 3);

        // Assert - should move to position (7,3) and stop (edge of 8x8 grid)
        result.WasPushed.Should().BeTrue();
        result.WasBlocked.Should().BeTrue();
        result.CellsMoved.Should().Be(1); // Only moved 1 cell before hitting wall
        result.EndPosition.X.Should().Be(7);
    }

    /// <summary>
    /// Verifies that push stops when blocked by another entity.
    /// </summary>
    [Test]
    public void Push_BlockedByEntity_StopsBeforeEntity()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();
        var blocker = MonsterBuilder.Skeleton().Build();
        var blockerCombatant = Combatant.ForMonster(blocker, CreateInitiativeRoll(), 2);

        grid.PlaceEntity(pusher.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);
        grid.PlaceEntity(blockerCombatant.Id, new GridPosition(6, 3), isPlayer: false);

        // Setup dice so pusher wins
        var rollSequence = new Queue<int>(new[] { 20, 1 });
        _mockDiceService.Setup(d => d.RollTotal("1d20"))
            .Returns(() => rollSequence.Dequeue());

        // Act - try to push 3 cells (but blocker at position 6,3)
        var result = _service.Push(pusher, target, grid, MovementDirection.East, distance: 3);

        // Assert - should move to position (5,3) and stop before blocker
        result.WasPushed.Should().BeTrue();
        result.WasBlocked.Should().BeTrue();
        result.CellsMoved.Should().Be(1);
        result.EndPosition.X.Should().Be(5);
    }

    /// <summary>
    /// Verifies that pushing into a hazard triggers hazard damage.
    /// </summary>
    [Test]
    public void Push_IntoHazard_TriggersHazardDamage()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var pusher = CreatePlayerCombatant("Pusher");
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(pusher.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Set up hazard at position (5, 3)
        var hazardCell = grid.GetCell(5, 3);
        hazardCell!.SetTerrain(TerrainType.Hazardous);
        hazardCell.SetTerrainDefinition("hazard:lava");

        // Setup dice - pusher wins opposed check, then hazard damage roll
        var rollSequence = new Queue<int>(new[] { 20, 1, 12 }); // push check, then 12 damage
        _mockDiceService.Setup(d => d.RollTotal(It.IsAny<string>()))
            .Returns(() => rollSequence.Dequeue());

        // Act
        var result = _service.Push(pusher, target, grid, MovementDirection.East, distance: 2);

        // Assert
        result.WasPushed.Should().BeTrue();
        result.HitHazard.Should().BeTrue();
        result.HazardDamage.Should().NotBeNull();
        result.HazardDamage!.HazardType.Should().Be(HazardType.Lava);
        result.HazardDamage.DamageDealt.Should().BeGreaterThan(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // KNOCKBACK TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that knockback calculates the direction away from the source.
    /// </summary>
    [Test]
    public void Knockback_CalculatesDirectionAwayFromSource()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(target.Id, new GridPosition(4, 4), isPlayer: false);

        // Source is at (3, 4), target at (4, 4), so knockback should be East
        var sourcePosition = new GridPosition(3, 4);

        // Act
        var result = _service.Knockback(target, grid, sourcePosition, distance: 1);

        // Assert
        result.Direction.Should().Be(MovementDirection.East);
        result.EndPosition.X.Should().Be(5);
        result.EndPosition.Y.Should().Be(4);
    }

    /// <summary>
    /// Verifies that knockback has no opposed check and always moves the target.
    /// </summary>
    [Test]
    public void Knockback_NoOpposedCheck_AlwaysMoves()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(target.Id, new GridPosition(4, 4), isPlayer: false);

        var sourcePosition = new GridPosition(3, 4);

        // Act
        var result = _service.Knockback(target, grid, sourcePosition, distance: 1);

        // Assert - no d20 rolls for opposed check
        _mockDiceService.Verify(d => d.RollTotal("1d20"), Times.Never);
        result.WasPushed.Should().BeTrue();
        result.WasKnockback.Should().BeTrue();
        result.CellsMoved.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // CRITICAL KNOCKBACK TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ProcessCriticalKnockback triggers knockback on a critical hit.
    /// </summary>
    [Test]
    public void ProcessCriticalKnockback_OnCritical_TriggersKnockback()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var attacker = CreatePlayerCombatant("Attacker");
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(attacker.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Act
        var result = _service.ProcessCriticalKnockback(attacker, target, grid, isCritical: true);

        // Assert
        result.Should().NotBeNull();
        result!.WasPushed.Should().BeTrue();
        result.WasKnockback.Should().BeTrue();
        result.CellsMoved.Should().Be(1);

        // Verify event logging
        _mockEventLogger.Verify(
            e => e.LogCombat(
                "CriticalKnockbackTriggered",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>?>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that ProcessCriticalKnockback returns null when not a critical hit.
    /// </summary>
    [Test]
    public void ProcessCriticalKnockback_NotCritical_ReturnsNull()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var attacker = CreatePlayerCombatant("Attacker");
        var target = CreateMonsterCombatant();

        grid.PlaceEntity(attacker.Id, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(target.Id, new GridPosition(4, 3), isPlayer: false);

        // Act
        var result = _service.ProcessCriticalKnockback(attacker, target, grid, isCritical: false);

        // Assert
        result.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // HAZARD DETECTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsHazard correctly detects hazardous terrain.
    /// </summary>
    [Test]
    public void IsHazard_DetectsHazardousTerrain()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);

        // Set up hazard at position (3, 3)
        var hazardCell = grid.GetCell(3, 3);
        hazardCell!.SetTerrain(TerrainType.Hazardous);

        // Normal terrain at (4, 4)
        var normalCell = grid.GetCell(4, 4);
        normalCell!.SetTerrain(TerrainType.Normal);

        // Act & Assert
        _service.IsHazard(grid, new GridPosition(3, 3)).Should().BeTrue();
        _service.IsHazard(grid, new GridPosition(4, 4)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetHazardType correctly identifies the hazard type from terrain definition.
    /// </summary>
    [Test]
    public void GetHazardType_ReturnsCorrectType()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);

        // Set up lava hazard
        var lavaCell = grid.GetCell(3, 3);
        lavaCell!.SetTerrain(TerrainType.Hazardous);
        lavaCell.SetTerrainDefinition("hazard:lava");

        // Set up spikes hazard
        var spikesCell = grid.GetCell(4, 4);
        spikesCell!.SetTerrain(TerrainType.Hazardous);
        spikesCell.SetTerrainDefinition("hazard:spikes");

        // Act & Assert
        _service.GetHazardType(grid, new GridPosition(3, 3)).Should().Be(HazardType.Lava);
        _service.GetHazardType(grid, new GridPosition(4, 4)).Should().Be(HazardType.Spikes);
    }

    // ═══════════════════════════════════════════════════════════════
    // HAZARD DAMAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ApplyHazardDamage deals damage correctly.
    /// </summary>
    /// <remarks>
    /// Damage is applied through the entity's TakeDamage method which reduces
    /// damage by the entity's defense. Goblin has 2 defense, so 12 rolled becomes 10 dealt.
    /// </remarks>
    [Test]
    public void ApplyHazardDamage_DealsDamageCorrectly()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var target = CreateMonsterCombatant(); // Goblin with defense = 2

        var hazardCell = grid.GetCell(3, 3);
        hazardCell!.SetTerrain(TerrainType.Hazardous);
        hazardCell.SetTerrainDefinition("hazard:lava");

        grid.PlaceEntity(target.Id, new GridPosition(3, 3), isPlayer: false);

        // Setup dice to roll 12 damage
        _mockDiceService.Setup(d => d.RollTotal("3d6")).Returns(12);

        // Act
        var result = _service.ApplyHazardDamage(target, grid, new GridPosition(3, 3), isEntryDamage: true);

        // Assert - 12 rolled - 2 defense = 10 actual damage
        result.DamageDealt.Should().Be(10);
        result.DamageType.Should().Be("fire");
        result.DamageDice.Should().Be("3d6");
        result.WasEntryDamage.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ApplyHazardDamage does not apply status effects when the target
    /// does not implement IEffectTarget.
    /// </summary>
    /// <remarks>
    /// Currently, neither Player nor Monster implements IEffectTarget, so status effects
    /// from hazards are not applied. This test documents this limitation.
    /// When Player/Monster are updated to implement IEffectTarget, this test should be
    /// changed to verify that status effects ARE applied.
    /// </remarks>
    [Test]
    public void ApplyHazardDamage_DoesNotApplyStatusEffect_WhenTargetNotIEffectTarget()
    {
        // Arrange - Player does not implement IEffectTarget, so status effects won't be applied
        var grid = CombatGrid.Create(8, 8);
        var target = CreatePlayerCombatant("HazardVictim");

        var hazardCell = grid.GetCell(3, 3);
        hazardCell!.SetTerrain(TerrainType.Hazardous);
        hazardCell.SetTerrainDefinition("hazard:lava");

        grid.PlaceEntity(target.Id, new GridPosition(3, 3), isPlayer: true);

        _mockDiceService.Setup(d => d.RollTotal("3d6")).Returns(10);

        // Act
        var result = _service.ApplyHazardDamage(target, grid, new GridPosition(3, 3), isEntryDamage: true);

        // Assert - Status effect is NOT applied because Player doesn't implement IEffectTarget
        result.StatusEffectApplied.Should().BeFalse("Player does not implement IEffectTarget");
        result.AppliedStatusEffectId.Should().BeNull();

        // Verify buff/debuff service was NOT called
        _mockBuffDebuffService.Verify(
            b => b.ApplyEffect(
                It.IsAny<IEffectTarget>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // HAZARD TICK TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TickHazards processes per-turn damage for combatants in hazards.
    /// </summary>
    /// <remarks>
    /// Damage is applied through the entity's TakeDamage method which reduces
    /// damage by the entity's defense. Goblin has 2 defense, so 10 rolled becomes 8 dealt.
    /// </remarks>
    [Test]
    public void TickHazards_ProcessesPerTurnDamage()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var combatant1 = CreateMonsterCombatant(); // Goblin with defense = 2
        var combatant2 = MonsterBuilder.Skeleton().Build();
        var combatant2Wrapped = Combatant.ForMonster(combatant2, CreateInitiativeRoll(), 2);

        // Place combatant1 in lava (has per-turn damage)
        var lavaCell = grid.GetCell(3, 3);
        lavaCell!.SetTerrain(TerrainType.Hazardous);
        lavaCell.SetTerrainDefinition("hazard:lava");
        grid.PlaceEntity(combatant1.Id, new GridPosition(3, 3), isPlayer: false);

        // Place combatant2 on normal terrain
        grid.PlaceEntity(combatant2Wrapped.Id, new GridPosition(5, 5), isPlayer: false);

        _mockDiceService.Setup(d => d.RollTotal("3d6")).Returns(10);

        var combatants = new[] { combatant1, combatant2Wrapped };

        // Act
        var results = _service.TickHazards(grid, combatants);

        // Assert - only combatant1 should take damage (in lava)
        // 10 rolled - 2 defense = 8 actual damage
        results.Should().HaveCount(1);
        results[0].HazardType.Should().Be(HazardType.Lava);
        results[0].DamageDealt.Should().Be(8);
    }

    /// <summary>
    /// Verifies that TickHazards skips hazards that don't deal per-turn damage.
    /// </summary>
    [Test]
    public void TickHazards_SkipsNonTickDamageHazards()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var combatant = CreateMonsterCombatant();

        // Place combatant in spikes (no per-turn damage)
        var spikesCell = grid.GetCell(3, 3);
        spikesCell!.SetTerrain(TerrainType.Hazardous);
        spikesCell.SetTerrainDefinition("hazard:spikes");
        grid.PlaceEntity(combatant.Id, new GridPosition(3, 3), isPlayer: false);

        var combatants = new[] { combatant };

        // Act
        var results = _service.TickHazards(grid, combatants);

        // Assert - spikes has damagePerTurn=false, so no damage
        results.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // DIRECTION CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDirectionToward calculates correct cardinal directions.
    /// </summary>
    [Test]
    [TestCase(0, 0, 0, -1, MovementDirection.North)]
    [TestCase(0, 0, 0, 1, MovementDirection.South)]
    [TestCase(0, 0, 1, 0, MovementDirection.East)]
    [TestCase(0, 0, -1, 0, MovementDirection.West)]
    [TestCase(0, 0, 1, -1, MovementDirection.NorthEast)]
    [TestCase(0, 0, -1, -1, MovementDirection.NorthWest)]
    [TestCase(0, 0, 1, 1, MovementDirection.SouthEast)]
    [TestCase(0, 0, -1, 1, MovementDirection.SouthWest)]
    public void GetDirectionToward_ReturnsCorrectDirection(
        int fromX, int fromY, int toX, int toY, MovementDirection expected)
    {
        // Arrange
        var from = new GridPosition(fromX, fromY);
        var to = new GridPosition(toX, toY);

        // Act
        var result = _service.GetDirectionToward(from, to);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDirectionAway calculates the opposite direction correctly.
    /// </summary>
    [Test]
    [TestCase(1, 0, 0, 0, MovementDirection.East)]  // Current is east of source, so direction away is East
    [TestCase(0, 1, 0, 0, MovementDirection.South)] // Current is south of source, so direction away is South
    [TestCase(-1, 0, 0, 0, MovementDirection.West)] // Current is west of source, so direction away is West
    [TestCase(0, -1, 0, 0, MovementDirection.North)] // Current is north of source, so direction away is North
    public void GetDirectionAway_ReturnsCorrectDirection(
        int currentX, int currentY, int sourceX, int sourceY, MovementDirection expected)
    {
        // Arrange
        var current = new GridPosition(currentX, currentY);
        var source = new GridPosition(sourceX, sourceY);

        // Act
        var result = _service.GetDirectionAway(current, source);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // STRENGTH MODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStrengthModifier calculates correctly for players.
    /// </summary>
    [Test]
    public void GetStrengthModifier_ForPlayer_CalculatesFromMight()
    {
        // Arrange - player with Might 14 should have modifier +2 ((14-10)/2)
        var player = PlayerBuilder.Create()
            .WithName("StrongPlayer")
            .WithAttributes(might: 14, fortitude: 10, will: 10, wits: 10, finesse: 10)
            .Build();
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll());

        // Act
        var result = _service.GetStrengthModifier(combatant);

        // Assert
        result.Should().Be(2); // (14 - 10) / 2 = 2
    }

    /// <summary>
    /// Verifies that GetStrengthModifier calculates correctly for monsters.
    /// </summary>
    [Test]
    public void GetStrengthModifier_ForMonster_CalculatesFromAttack()
    {
        // Arrange - monster with attack 12 should have modifier +4 (12/3)
        var monster = MonsterBuilder.Create()
            .WithName("StrongMonster")
            .WithStats(maxHealth: 50, attack: 12, defense: 5)
            .Build();
        var combatant = Combatant.ForMonster(monster, CreateInitiativeRoll(), 0);

        // Act
        var result = _service.GetStrengthModifier(combatant);

        // Assert
        result.Should().Be(4); // 12 / 3 = 4
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the default hazard provider mock behavior.
    /// </summary>
    private void SetupDefaultHazardProvider()
    {
        _mockHazardProvider.Setup(p => p.GetHazard(HazardType.Lava)).Returns(_lavaHazard);
        _mockHazardProvider.Setup(p => p.GetHazard(HazardType.Spikes)).Returns(_spikesHazard);
        _mockHazardProvider.Setup(p => p.GetHazard(HazardType.Pit)).Returns(_pitHazard);
        _mockHazardProvider.Setup(p => p.GetHazard(HazardType.PoisonGas)).Returns(_poisonGasHazard);
        _mockHazardProvider.Setup(p => p.Count).Returns(4);
    }

    /// <summary>
    /// Sets up the default dice service mock behavior.
    /// </summary>
    private void SetupDefaultDiceService()
    {
        // Default: return 10 for any dice roll
        _mockDiceService.Setup(d => d.RollTotal(It.IsAny<string>())).Returns(10);
    }

    /// <summary>
    /// Sets up the default buff/debuff service mock behavior.
    /// </summary>
    private void SetupDefaultBuffDebuffService()
    {
        // Create a status effect definition for testing
        var effectDef = Domain.Definitions.StatusEffectDefinition.Create(
            id: "burning",
            name: "Burning",
            description: "Target is on fire, taking damage over time.",
            category: EffectCategory.Debuff,
            durationType: DurationType.Turns,
            baseDuration: 3);

        var activeEffect = ActiveStatusEffect.Create(effectDef, sourceId: null, sourceName: "Test Hazard");

        _mockBuffDebuffService
            .Setup(b => b.ApplyEffect(
                It.IsAny<IEffectTarget>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>()))
            .Returns(ApplyResult.Success(activeEffect));
    }

    /// <summary>
    /// Creates a player combatant for testing.
    /// </summary>
    /// <param name="name">Optional player name.</param>
    /// <returns>A Combatant wrapping a test player.</returns>
    private static Combatant CreatePlayerCombatant(string name = "TestPlayer")
    {
        var player = PlayerBuilder.Create()
            .WithName(name)
            .WithAttributes(might: 10, fortitude: 10, will: 10, wits: 10, finesse: 10)
            .Build();
        return Combatant.ForPlayer(player, CreateInitiativeRoll());
    }

    /// <summary>
    /// Creates a monster combatant for testing.
    /// </summary>
    /// <returns>A Combatant wrapping a test monster.</returns>
    private static Combatant CreateMonsterCombatant()
    {
        var monster = MonsterBuilder.Goblin().Build();
        return Combatant.ForMonster(monster, CreateInitiativeRoll(), displayNumber: 0);
    }

    /// <summary>
    /// Creates a valid InitiativeRoll for testing.
    /// </summary>
    /// <param name="rollValue">The dice roll value.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>An InitiativeRoll instance.</returns>
    private static InitiativeRoll CreateInitiativeRoll(int rollValue = 10, int modifier = 0)
    {
        var diceResult = new DiceRollResult
        {
            Pool = DicePool.Parse("1d10"),
            Rolls = new[] { rollValue },
            ExplosionRolls = Array.Empty<int>(),
            DiceTotal = rollValue,
            Total = rollValue
        };
        return new InitiativeRoll(diceResult, modifier);
    }
}
