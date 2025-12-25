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
/// Tests for the intent system introduced in v0.3.6c.
/// Validates enemy action planning, WITS-based visibility checks, and intent icons.
/// </summary>
public class IntentSystemTests
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
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public IntentSystemTests()
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
            _mockLogger.Object);

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());

        // Default setup for ability repository (return empty list)
        _mockAbilityRepository
            .Setup(r => r.GetByArchetypeAsync(It.IsAny<ArchetypeType>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActiveAbility>());

        // Default dice setup - no successes (intent hidden)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));

        // Default status effect setup - no DoT damage, combatant can act
        _mockStatusEffects.Setup(s => s.ProcessTurnStart(It.IsAny<Combatant>())).Returns(0);
        _mockStatusEffects.Setup(s => s.CanAct(It.IsAny<Combatant>())).Returns(true);
    }

    #region PlanEnemyActions Tests

    [Fact]
    public void StartCombat_PlansActionsForAllEnemies()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        var attackAction = new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard);
        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(attackAction);

        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.DPS, "Enemy A"),
            CreateTestEnemy(EnemyArchetype.Tank, "Enemy B")
        };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var state = _gameState.CombatState!;
        var enemyCombatants = state.TurnOrder.Where(c => !c.IsPlayer).ToList();
        enemyCombatants.Should().HaveCount(2);
        enemyCombatants.Should().OnlyContain(e => e.PlannedAction != null);
    }

    [Fact]
    public void StartCombat_SkipsDeadEnemies_WhenPlanningActions()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.DPS, "Enemy A"),
            CreateTestEnemy(EnemyArchetype.Tank, "Enemy B")
        };

        // Act
        _sut.StartCombat(enemies);
        var state = _gameState.CombatState!;

        // Kill one enemy
        var deadEnemy = state.TurnOrder.First(c => !c.IsPlayer);
        deadEnemy.CurrentHp = 0;
        deadEnemy.PlannedAction = null;

        // Trigger replanning by advancing to next round
        state.TurnIndex = state.TurnOrder.Count - 1;
        _sut.NextTurn();

        // Assert
        // Dead enemy should not have a planned action
        deadEnemy.PlannedAction.Should().BeNull();
    }

    [Fact]
    public void StartCombat_PlayerCombatant_HasNullPlannedAction()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var state = _gameState.CombatState!;
        var player = state.TurnOrder.First(c => c.IsPlayer);
        player.PlannedAction.Should().BeNull();
    }

    #endregion

    #region CalculateIntentVisibility Tests

    [Fact]
    public void StartCombat_AnalyzedStatus_AlwaysRevealsIntent()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        // Set up status effect check to return true for Analyzed
        _mockStatusEffects.Setup(s => s.HasEffect(It.IsAny<Combatant>(), StatusEffectType.Analyzed))
            .Returns(true);

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var state = _gameState.CombatState!;
        var enemy = state.TurnOrder.First(c => !c.IsPlayer);
        enemy.IsIntentRevealed.Should().BeTrue();
    }

    [Fact]
    public void StartCombat_HighWits_WithSuccessfulRoll_RevealsIntent()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior, wits: 5);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        // Dice roll with 1 success
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new List<int> { 8 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var state = _gameState.CombatState!;
        var enemy = state.TurnOrder.First(c => !c.IsPlayer);
        enemy.IsIntentRevealed.Should().BeTrue();
    }

    [Fact]
    public void StartCombat_LowWits_WithFailedRoll_HidesIntent()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior, wits: 1);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        // Dice roll with 0 successes
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var state = _gameState.CombatState!;
        var enemy = state.TurnOrder.First(c => !c.IsPlayer);
        enemy.IsIntentRevealed.Should().BeFalse();
    }

    [Fact]
    public void StartCombat_AdeptArchetype_GetsBonusToWitsPool()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Adept, wits: 3);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        int? capturedPoolSize = null;
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Callback<int, string>((size, _) => capturedPoolSize = size)
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert - Adept should get WITS (3) + 2 bonus = 5 pool size
        capturedPoolSize.Should().Be(5);
    }

    [Fact]
    public void StartCombat_NonAdeptArchetype_NoBonus()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior, wits: 3);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        int? capturedPoolSize = null;
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Callback<int, string>((size, _) => capturedPoolSize = size)
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };

        // Act
        _sut.StartCombat(enemies);

        // Assert - Warrior should get WITS (3) only, no bonus
        capturedPoolSize.Should().Be(3);
    }

    #endregion

    #region ViewModel Intent Mapping Tests

    [Fact]
    public void GetViewModel_Enemy_WithRevealedIntent_ShowsIntentIcon()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        // Set dice to succeed for intent reveal
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new List<int> { 8 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };
        _sut.StartCombat(enemies);

        // Act
        var viewModel = _sut.GetViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        var enemyView = viewModel!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.IntentIcon.Should().Be("⚔️"); // Attack icon
    }

    [Fact]
    public void GetViewModel_Enemy_WithHiddenIntent_ShowsQuestionMark()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Attack, Guid.NewGuid(), Guid.NewGuid(), AttackType.Standard));

        // Set dice to fail for intent reveal
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(0, 0, new List<int> { 3 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };
        _sut.StartCombat(enemies);

        // Act
        var viewModel = _sut.GetViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        var enemyView = viewModel!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.IntentIcon.Should().Be("?");
    }

    [Fact]
    public void GetViewModel_Player_HasNullIntentIcon()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.DPS, "Test Enemy") };
        _sut.StartCombat(enemies);

        // Act
        var viewModel = _sut.GetViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        var playerView = viewModel!.TurnOrder.First(c => c.IsPlayer);
        playerView.IntentIcon.Should().BeNull();
    }

    [Fact]
    public void GetViewModel_DefendAction_ShowsShieldIcon()
    {
        // Arrange
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        _mockAIService.Setup(ai => ai.DetermineAction(It.IsAny<Combatant>(), It.IsAny<CombatState>()))
            .Returns(new CombatAction(ActionType.Defend, Guid.NewGuid(), null));

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new List<int> { 8 }));

        var enemies = new List<Enemy> { CreateTestEnemy(EnemyArchetype.Tank, "Test Tank") };
        _sut.StartCombat(enemies);

        // Act
        var viewModel = _sut.GetViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        var enemyView = viewModel!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.IntentIcon.Should().Be("🛡️");
    }

    #endregion

    #region Helper Methods

    private static CharacterEntity CreateTestCharacter(ArchetypeType archetype, int wits = 3)
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
            Corruption = 0,
            Wits = wits
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
