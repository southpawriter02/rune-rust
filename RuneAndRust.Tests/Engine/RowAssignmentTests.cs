using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the row assignment system introduced in v0.3.6a.
/// Validates archetype-based row assignment and melee targeting rules.
/// </summary>
public class RowAssignmentTests
{
    private readonly Mock<IInitiativeService> _mockInitiative;
    private readonly Mock<IAttackResolutionService> _mockAttackResolution;
    private readonly Mock<ILootService> _mockLootService;
    private readonly Mock<IStatusEffectService> _mockStatusEffects;
    private readonly Mock<IEnemyAIService> _mockAIService;
    private readonly Mock<ICreatureTraitService> _mockTraitService;
    private readonly Mock<IResourceService> _mockResourceService;
    private readonly Mock<IAbilityService> _mockAbilityService;
    private readonly Mock<IActiveAbilityRepository> _mockAbilityRepository;
    private readonly Mock<ITraumaService> _mockTraumaService;
    private readonly Mock<IHazardService> _mockHazardService;
    private readonly Mock<IConditionService> _mockConditionService;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<IVisualEffectService> _mockVisualEffectService;
    private readonly Mock<ISpatialHashGrid> _mockSpatialGrid;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ISagaService> _mockSagaService;
    private readonly Mock<IAetherService> _mockAetherService;
    private readonly Mock<IMagicService> _mockMagicService;
    private readonly Mock<ISpellRepository> _mockSpellRepository;
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public RowAssignmentTests()
    {
        _mockInitiative = new Mock<IInitiativeService>();
        _mockAttackResolution = new Mock<IAttackResolutionService>();
        _mockLootService = new Mock<ILootService>();
        _mockStatusEffects = new Mock<IStatusEffectService>();
        _mockAIService = new Mock<IEnemyAIService>();
        _mockTraitService = new Mock<ICreatureTraitService>();
        _mockResourceService = new Mock<IResourceService>();
        _mockAbilityService = new Mock<IAbilityService>();
        _mockAbilityRepository = new Mock<IActiveAbilityRepository>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockHazardService = new Mock<IHazardService>();
        _mockConditionService = new Mock<IConditionService>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockDice = new Mock<IDiceService>();
        _mockVisualEffectService = new Mock<IVisualEffectService>();
        _mockSpatialGrid = new Mock<ISpatialHashGrid>();
        _mockEventBus = new Mock<IEventBus>();
        _mockSagaService = new Mock<ISagaService>();
        _mockAetherService = new Mock<IAetherService>();
        _mockMagicService = new Mock<IMagicService>();
        _mockSpellRepository = new Mock<ISpellRepository>();
        _mockLogger = new Mock<ILogger<CombatService>>();
        _gameState = new GameState();
        _sut = new CombatService(
            _gameState,
            _mockInitiative.Object,
            _mockAttackResolution.Object,
            _mockLootService.Object,
            _mockStatusEffects.Object,
            _mockAIService.Object,
            _mockTraitService.Object,
            _mockResourceService.Object,
            _mockAbilityService.Object,
            _mockAbilityRepository.Object,
            _mockTraumaService.Object,
            _mockHazardService.Object,
            _mockConditionService.Object,
            _mockRoomRepository.Object,
            _mockDice.Object,
            _mockVisualEffectService.Object,
            _mockSpatialGrid.Object,
            _mockEventBus.Object,
            _mockSagaService.Object,
            _mockAetherService.Object,
            _mockMagicService.Object,
            _mockSpellRepository.Object,
            _mockLogger.Object);

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());

        // Default setup for ability repository (return empty list)
        _mockAbilityRepository
            .Setup(r => r.GetByArchetypeAsync(It.IsAny<ArchetypeType>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActiveAbility>());

        // Default setup for dice service (v0.3.6c): return 0 successes for intent checks
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));
    }

    #region Player Row Assignment Tests

    [Fact]
    public void GetDefaultPlayerRow_Warrior_ReturnsFront()
    {
        // Act
        var result = CombatService.GetDefaultPlayerRow(ArchetypeType.Warrior);

        // Assert
        result.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void GetDefaultPlayerRow_Skirmisher_ReturnsFront()
    {
        // Act
        var result = CombatService.GetDefaultPlayerRow(ArchetypeType.Skirmisher);

        // Assert
        result.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void GetDefaultPlayerRow_Adept_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultPlayerRow(ArchetypeType.Adept);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void GetDefaultPlayerRow_Mystic_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultPlayerRow(ArchetypeType.Mystic);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    #endregion

    #region Enemy Row Assignment Tests

    [Fact]
    public void GetDefaultEnemyRow_Tank_ReturnsFront()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.Tank);

        // Assert
        result.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void GetDefaultEnemyRow_DPS_ReturnsFront()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.DPS);

        // Assert
        result.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void GetDefaultEnemyRow_GlassCannon_ReturnsFront()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.GlassCannon);

        // Assert
        result.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void GetDefaultEnemyRow_Support_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.Support);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void GetDefaultEnemyRow_Swarm_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.Swarm);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void GetDefaultEnemyRow_Caster_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.Caster);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void GetDefaultEnemyRow_Boss_ReturnsBack()
    {
        // Act
        var result = CombatService.GetDefaultEnemyRow(EnemyArchetype.Boss);

        // Assert
        result.Should().Be(RowPosition.Back);
    }

    #endregion

    #region Melee Targeting Validation Tests

    [Fact]
    public void IsValidMeleeTarget_FrontRowTarget_ReturnsTrue()
    {
        // Arrange
        SetupCombatWithRows();
        var attacker = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        var target = _gameState.CombatState.TurnOrder.First(c => !c.IsPlayer && c.Row == RowPosition.Front);

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidMeleeTarget_BackRowTargetProtected_ReturnsFalse()
    {
        // Arrange
        SetupCombatWithRows();
        var attacker = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        var target = _gameState.CombatState.TurnOrder.First(c => !c.IsPlayer && c.Row == RowPosition.Back);

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidMeleeTarget_BackRowTargetWithReach_ReturnsTrue()
    {
        // Arrange
        SetupCombatWithRows();
        var attacker = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        var target = _gameState.CombatState.TurnOrder.First(c => !c.IsPlayer && c.Row == RowPosition.Back);

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target, hasReach: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidMeleeTarget_BackRowTargetFrontEmpty_ReturnsTrue()
    {
        // Arrange
        SetupCombatWithRows();

        // Kill all front row enemies
        var frontRowEnemies = _gameState.CombatState!.TurnOrder
            .Where(c => !c.IsPlayer && c.Row == RowPosition.Front).ToList();
        foreach (var enemy in frontRowEnemies)
        {
            enemy.CurrentHp = 0;
        }

        var attacker = _gameState.CombatState.TurnOrder.First(c => c.IsPlayer);
        var target = _gameState.CombatState.TurnOrder.First(c => !c.IsPlayer && c.Row == RowPosition.Back);

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidMeleeTarget_AllyTarget_ReturnsTrue()
    {
        // Arrange - targeting ally should always be valid (for heals/buffs)
        SetupCombatWithRows();
        var attacker = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        var target = attacker; // Self-targeting

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidMeleeTarget_NoCombatState_ReturnsFalse()
    {
        // Arrange - no combat state
        var attacker = new Combatant { IsPlayer = true };
        var target = new Combatant { IsPlayer = false, Row = RowPosition.Front };

        // Act
        var result = _sut.IsValidMeleeTarget(attacker, target);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Combat Integration Tests

    [Fact]
    public void StartCombat_AssignsCorrectPlayerRow_Warrior()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;
        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS) };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        player.Row.Should().Be(RowPosition.Front);
    }

    [Fact]
    public void StartCombat_AssignsCorrectPlayerRow_Mystic()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Mystic);
        _gameState.CurrentCharacter = character;
        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS) };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        player.Row.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void StartCombat_AssignsCorrectEnemyRows()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.Tank, "Tank Enemy"),
            CreateTestEnemy(EnemyArchetype.Caster, "Caster Enemy")
        };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var tank = _gameState.CombatState!.TurnOrder.First(c => c.Name == "Tank Enemy");
        var caster = _gameState.CombatState.TurnOrder.First(c => c.Name == "Caster Enemy");

        tank.Row.Should().Be(RowPosition.Front);
        caster.Row.Should().Be(RowPosition.Back);
    }

    [Fact]
    public void GetViewModel_GroupsCombatantsByRow()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.Tank, "Front Tank"),
            CreateTestEnemy(EnemyArchetype.Caster, "Back Caster")
        };

        _sut.StartCombat(enemies);

        // Act
        var vm = _sut.GetViewModel();

        // Assert
        vm.Should().NotBeNull();
        vm!.PlayerFrontRow.Should().HaveCount(1);
        vm.PlayerBackRow.Should().BeEmpty();
        vm.EnemyFrontRow.Should().HaveCount(1);
        vm.EnemyBackRow.Should().HaveCount(1);

        vm.EnemyFrontRow!.First().Name.Should().Be("Front Tank");
        vm.EnemyBackRow!.First().Name.Should().Be("Back Caster");
    }

    #endregion

    #region Helper Methods

    private void SetupCombatWithRows()
    {
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.Tank, "Front Tank"),
            CreateTestEnemy(EnemyArchetype.Caster, "Back Caster")
        };

        _sut.StartCombat(enemies);
    }

    private static CharacterEntity CreateTestCharacter(ArchetypeType archetype)
    {
        return new CharacterEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Hero",
            Archetype = archetype,
            Lineage = LineageType.Human,
            CurrentHP = 100,
            MaxHP = 100,
            CurrentStamina = 50,
            MaxStamina = 50,
            CurrentAp = 20,
            MaxAp = 20,
            PsychicStress = 0,
            Corruption = 0
        };
    }

    private static Enemy CreateTestEnemy(EnemyArchetype archetype, string name = "Test Enemy")
    {
        return new Enemy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Archetype = archetype,
            CurrentHp = 50,
            MaxHp = 50,
            CurrentStamina = 30,
            MaxStamina = 30,
            WeaponDamageDie = 6,
            WeaponAccuracyBonus = 0,
            ArmorSoak = 0,
            WeaponName = "Claws"
        };
    }

    #endregion
}
