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
/// Tests for the timeline projection system introduced in v0.3.6b.
/// Validates timeline generation showing remaining turns in current round plus next round.
/// </summary>
public class TimelineProjectionTests
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
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public TimelineProjectionTests()
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
            _mockLogger.Object);

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());

        // Default setup for ability repository (return empty list)
        _mockAbilityRepository
            .Setup(r => r.GetByArchetypeAsync(It.IsAny<ArchetypeType>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActiveAbility>());
    }

    #region GetTimelineProjection Tests

    [Fact]
    public void GetTimelineProjection_NoCombatState_ReturnsEmptyList()
    {
        // Arrange - no combat state set
        _gameState.CombatState = null;

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTimelineProjection_CurrentRoundOnly_ReturnsRemainingCombatants()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        state.TurnIndex = 0; // First combatant's turn

        // Act
        var result = _sut.GetTimelineProjection(windowSize: 3);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountLessThanOrEqualTo(3);
        result.All(e => e.RoundNumber == state.RoundNumber || e.RoundNumber == state.RoundNumber + 1).Should().BeTrue();
    }

    [Fact]
    public void GetTimelineProjection_FillsFromNextRound_WhenNeeded()
    {
        // Arrange
        // SetupCombatWithTwoCombatants creates 3 combatants (player + 2 enemies)
        SetupCombatWithTwoCombatants();
        var state = _gameState.CombatState!;
        state.TurnIndex = 2; // Third combatant's turn, only 1 remaining this round

        // Act
        var result = _sut.GetTimelineProjection(windowSize: 4);

        // Assert
        // With TurnIndex=2 and 3 combatants: 1 remaining in current round + 3 from next round = 4
        result.Should().HaveCount(4);
        result.Count(e => e.RoundNumber == state.RoundNumber).Should().Be(1);
        result.Count(e => e.RoundNumber == state.RoundNumber + 1).Should().Be(3);
    }

    [Fact]
    public void GetTimelineProjection_ExcludesDeadCombatants()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        var deadEnemy = state.TurnOrder.First(c => !c.IsPlayer);
        deadEnemy.CurrentHp = 0; // Kill one enemy

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        result.Should().NotContain(e => e.CombatantId == deadEnemy.Id);
    }

    [Fact]
    public void GetTimelineProjection_MarksActiveCorrectly()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        var activeCombatant = state.ActiveCombatant;

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        var activeEntry = result.FirstOrDefault(e => e.CombatantId == activeCombatant!.Id);
        activeEntry.Should().NotBeNull();
        activeEntry!.IsActive.Should().BeTrue();

        // Other entries from current round should not be marked active
        var otherCurrentRoundEntries = result
            .Where(e => e.RoundNumber == state.RoundNumber && e.CombatantId != activeCombatant!.Id);
        otherCurrentRoundEntries.Should().OnlyContain(e => !e.IsActive);
    }

    [Fact]
    public void GetTimelineProjection_RespectsWindowSize()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();

        // Act
        var result = _sut.GetTimelineProjection(windowSize: 2);

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(2);
    }

    [Fact]
    public void GetTimelineProjection_NextRoundEntries_NotMarkedActive()
    {
        // Arrange
        SetupCombatWithTwoCombatants();
        var state = _gameState.CombatState!;
        state.TurnIndex = 1; // Near end of round

        // Act
        var result = _sut.GetTimelineProjection(windowSize: 8);

        // Assert
        var nextRoundEntries = result.Where(e => e.RoundNumber > state.RoundNumber);
        nextRoundEntries.Should().OnlyContain(e => !e.IsActive);
    }

    #endregion

    #region Health Indicator Tests

    [Fact]
    public void GetTimelineProjection_CriticalHealth_ReturnsCorrectIndicator()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        var wounded = state.TurnOrder.First(c => !c.IsPlayer);
        wounded.CurrentHp = wounded.MaxHp / 5; // Below 25% = critical

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        var entry = result.First(e => e.CombatantId == wounded.Id);
        entry.HealthIndicator.Should().Be("critical");
    }

    [Fact]
    public void GetTimelineProjection_WoundedHealth_ReturnsCorrectIndicator()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        var wounded = state.TurnOrder.First(c => !c.IsPlayer);
        wounded.CurrentHp = wounded.MaxHp / 3; // Between 25-50% = wounded

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        var entry = result.First(e => e.CombatantId == wounded.Id);
        entry.HealthIndicator.Should().Be("wounded");
    }

    [Fact]
    public void GetTimelineProjection_HealthyHealth_ReturnsCorrectIndicator()
    {
        // Arrange
        SetupCombatWithMultipleEnemies();
        var state = _gameState.CombatState!;
        var healthy = state.TurnOrder.First(c => !c.IsPlayer);
        healthy.CurrentHp = healthy.MaxHp; // Full health

        // Act
        var result = _sut.GetTimelineProjection();

        // Assert
        var entry = result.First(e => e.CombatantId == healthy.Id);
        entry.HealthIndicator.Should().Be("healthy");
    }

    #endregion

    #region Helper Methods

    private void SetupCombatWithMultipleEnemies()
    {
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.Tank, "Tank Enemy"),
            CreateTestEnemy(EnemyArchetype.DPS, "DPS Enemy"),
            CreateTestEnemy(EnemyArchetype.Caster, "Caster Enemy")
        };

        _sut.StartCombat(enemies);
    }

    private void SetupCombatWithTwoCombatants()
    {
        var character = CreateTestCharacter(ArchetypeType.Warrior);
        _gameState.CurrentCharacter = character;

        // Create 2 enemies so total combatants = 3 (player + 2 enemies)
        // This allows test to verify filling from next round properly
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(EnemyArchetype.DPS, "Enemy A"),
            CreateTestEnemy(EnemyArchetype.Tank, "Enemy B")
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
