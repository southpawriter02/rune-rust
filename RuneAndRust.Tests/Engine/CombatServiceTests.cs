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
/// Tests for the CombatService class.
/// Validates combat lifecycle management including start, turn advancement, and end.
/// </summary>
public class CombatServiceTests
{
    private readonly Mock<IInitiativeService> _mockInitiative;
    private readonly Mock<IAttackResolutionService> _mockAttackResolution;
    private readonly Mock<ILootService> _mockLootService;
    private readonly Mock<IStatusEffectService> _mockStatusEffects;
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _mockInitiative = new Mock<IInitiativeService>();
        _mockAttackResolution = new Mock<IAttackResolutionService>();
        _mockLootService = new Mock<ILootService>();
        _mockStatusEffects = new Mock<IStatusEffectService>();
        _mockLogger = new Mock<ILogger<CombatService>>();
        _gameState = new GameState();
        _sut = new CombatService(
            _gameState,
            _mockInitiative.Object,
            _mockAttackResolution.Object,
            _mockLootService.Object,
            _mockStatusEffects.Object,
            _mockLogger.Object);

        // Default setup for loot service
        _mockLootService.Setup(l => l.GenerateLoot(It.IsAny<LootGenerationContext>()))
            .Returns(LootResult.Empty("No loot"));

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());

        // Default setup for attack resolution service
        _mockAttackResolution.Setup(a => a.GetStaminaCost(It.IsAny<AttackType>()))
            .Returns(25);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(true);

        // Default setup for status effects: no DoT, can act, no effects
        _mockStatusEffects.Setup(s => s.ProcessTurnStart(It.IsAny<Combatant>())).Returns(0);
        _mockStatusEffects.Setup(s => s.CanAct(It.IsAny<Combatant>())).Returns(true);
    }

    #region StartCombat Tests

    [Fact]
    public void StartCombat_SetsPhaseToСombat()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        _gameState.Phase = GamePhase.Exploration;
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.Phase.Should().Be(GamePhase.Combat);
    }

    [Fact]
    public void StartCombat_CreatesCombatState()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.CombatState.Should().NotBeNull();
    }

    [Fact]
    public void StartCombat_AddsPlayerAndEnemies()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy>
        {
            CreateTestEnemy("Enemy1"),
            CreateTestEnemy("Enemy2")
        };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.CombatState!.TurnOrder.Should().HaveCount(3); // 1 player + 2 enemies
    }

    [Fact]
    public void StartCombat_SortsTurnOrder()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _mockInitiative.Verify(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()), Times.Once);
    }

    [Fact]
    public void StartCombat_RollsInitiativeForAllCombatants()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _mockInitiative.Verify(i => i.RollInitiative(It.IsAny<Combatant>()), Times.Exactly(2));
    }

    [Fact]
    public void StartCombat_WithNoCharacter_DoesNothing()
    {
        // Arrange
        _gameState.CurrentCharacter = null;
        _gameState.Phase = GamePhase.Exploration;
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.Phase.Should().Be(GamePhase.Exploration);
        _gameState.CombatState.Should().BeNull();
    }

    [Fact]
    public void StartCombat_SetsRoundToOne()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.CombatState!.RoundNumber.Should().Be(1);
    }

    [Fact]
    public void StartCombat_SetsTurnIndexToZero()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        _gameState.CombatState!.TurnIndex.Should().Be(0);
    }

    [Fact]
    public void StartCombat_PlayerCombatantIsMarkedAsPlayer()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var playerCombatant = _gameState.CombatState!.TurnOrder.FirstOrDefault(c => c.IsPlayer);
        playerCombatant.Should().NotBeNull();
        playerCombatant!.CharacterSource.Should().Be(_gameState.CurrentCharacter);
    }

    [Fact]
    public void StartCombat_EnemyCombatantIsNotMarkedAsPlayer()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var enemyCombatant = _gameState.CombatState!.TurnOrder.FirstOrDefault(c => !c.IsPlayer);
        enemyCombatant.Should().NotBeNull();
        enemyCombatant!.EnemySource.Should().NotBeNull();
    }

    #endregion

    #region NextTurn Tests

    [Fact]
    public void NextTurn_IncrementsTurnIndex()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 3);
        var initialIndex = _gameState.CombatState!.TurnIndex;

        // Act
        _sut.NextTurn();

        // Assert
        _gameState.CombatState.TurnIndex.Should().Be(initialIndex + 1);
    }

    [Fact]
    public void NextTurn_WrapsToZeroAtEndOfTurnOrder()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 2);
        _gameState.CombatState!.TurnIndex = 1; // Last combatant

        // Act
        _sut.NextTurn();

        // Assert
        _gameState.CombatState.TurnIndex.Should().Be(0);
    }

    [Fact]
    public void NextTurn_IncrementsRoundWhenWrapping()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 2);
        _gameState.CombatState!.TurnIndex = 1; // Last combatant
        _gameState.CombatState.RoundNumber = 1;

        // Act
        _sut.NextTurn();

        // Assert
        _gameState.CombatState.RoundNumber.Should().Be(2);
    }

    [Fact]
    public void NextTurn_WithNoCombat_DoesNothing()
    {
        // Arrange
        _gameState.CombatState = null;

        // Act - Should not throw
        var act = () => _sut.NextTurn();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NextTurn_MiddleOfRound_DoesNotIncrementRound()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 3);
        _gameState.CombatState!.TurnIndex = 0;
        _gameState.CombatState.RoundNumber = 1;

        // Act
        _sut.NextTurn();

        // Assert
        _gameState.CombatState.RoundNumber.Should().Be(1);
    }

    #endregion

    #region EndCombat Tests

    [Fact]
    public void EndCombat_SetsPhaseToExploration()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 2);
        _gameState.Phase = GamePhase.Combat;

        // Act
        _sut.EndCombat();

        // Assert
        _gameState.Phase.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public void EndCombat_ClearsCombatState()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 2);

        // Act
        _sut.EndCombat();

        // Assert
        _gameState.CombatState.Should().BeNull();
    }

    [Fact]
    public void EndCombat_WhenNoCombatActive_DoesNotThrow()
    {
        // Arrange
        _gameState.CombatState = null;
        _gameState.Phase = GamePhase.Exploration;

        // Act
        var act = () => _sut.EndCombat();

        // Assert
        act.Should().NotThrow();
        _gameState.Phase.Should().Be(GamePhase.Exploration);
    }

    #endregion

    #region ExecutePlayerAttack Tests

    [Fact]
    public void ExecutePlayerAttack_WhenNoCombat_ReturnsNotInCombatMessage()
    {
        // Arrange
        _gameState.CombatState = null;

        // Act
        var result = _sut.ExecutePlayerAttack("enemy", AttackType.Standard);

        // Assert
        result.Should().Contain("not in combat");
    }

    [Fact]
    public void ExecutePlayerAttack_WhenNotPlayerTurn_ReturnsNotYourTurnMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: false);

        // Act
        var result = _sut.ExecutePlayerAttack("enemy", AttackType.Standard);

        // Assert
        result.Should().Contain("not your turn");
    }

    [Fact]
    public void ExecutePlayerAttack_TargetNotFound_ReturnsTargetNotFoundMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        var result = _sut.ExecutePlayerAttack("nonexistent", AttackType.Standard);

        // Assert
        result.Should().Contain("not found");
    }

    [Fact]
    public void ExecutePlayerAttack_InsufficientStamina_ReturnsNotEnoughStaminaMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.GetStaminaCost(AttackType.Heavy))
            .Returns(40);

        // Act
        var result = _sut.ExecutePlayerAttack("Enemy", AttackType.Heavy);

        // Assert
        result.Should().Contain("Not enough stamina");
    }

    [Fact]
    public void ExecutePlayerAttack_SuccessfulHit_DeductsStamina()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        var initialStamina = player.CurrentStamina;

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 8, true));

        // Act
        _sut.ExecutePlayerAttack("Enemy", AttackType.Standard);

        // Assert
        player.CurrentStamina.Should().Be(initialStamina - 25);
    }

    [Fact]
    public void ExecutePlayerAttack_Hit_AppliesDamageToTarget()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        var initialHp = enemy.CurrentHp;

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 8, true));

        // Act
        _sut.ExecutePlayerAttack("Enemy", AttackType.Standard);

        // Assert
        enemy.CurrentHp.Should().Be(initialHp - 8);
    }

    [Fact]
    public void ExecutePlayerAttack_Miss_DoesNotApplyDamage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        var initialHp = enemy.CurrentHp;

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Miss, -1, 0, 0, false));

        // Act
        _sut.ExecutePlayerAttack("Enemy", AttackType.Standard);

        // Assert
        enemy.CurrentHp.Should().Be(initialHp);
    }

    [Fact]
    public void ExecutePlayerAttack_KillsEnemy_RemovesFromTurnOrder()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 5; // Low HP

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 10, true));

        // Act
        _sut.ExecutePlayerAttack("Enemy", AttackType.Standard);

        // Assert
        _gameState.CombatState!.TurnOrder.Should().NotContain(enemy);
    }

    [Fact]
    public void ExecutePlayerAttack_KillsLastEnemy_ReturnsVictoryMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 5;

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Critical, 6, 20, 15, true));

        // Act
        var result = _sut.ExecutePlayerAttack("Enemy", AttackType.Standard);

        // Assert
        result.Should().Contain("VICTORY");
    }

    [Fact]
    public void ExecutePlayerAttack_PartialMatch_FindsTarget()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Training Dummy");

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 8, true));

        // Act - Use partial name "dummy"
        var result = _sut.ExecutePlayerAttack("dummy", AttackType.Standard);

        // Assert
        result.Should().NotContain("not found");
        _mockAttackResolution.Verify(a => a.ResolveMeleeAttack(
            It.IsAny<Combatant>(),
            It.Is<Combatant>(c => c.Name == "Training Dummy"),
            It.IsAny<AttackType>()), Times.Once);
    }

    #endregion

    #region RemoveDefeatedCombatant Tests

    [Fact]
    public void RemoveDefeatedCombatant_RemovesCombatantFromTurnOrder()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 3);
        var combatant = _gameState.CombatState!.TurnOrder[1];

        // Act
        _sut.RemoveDefeatedCombatant(combatant);

        // Assert
        _gameState.CombatState.TurnOrder.Should().HaveCount(2);
        _gameState.CombatState.TurnOrder.Should().NotContain(combatant);
    }

    [Fact]
    public void RemoveDefeatedCombatant_AdjustsTurnIndex_WhenRemovedBeforeCurrent()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 3);
        _gameState.CombatState!.TurnIndex = 2; // Currently on last combatant
        var combatant = _gameState.CombatState.TurnOrder[0]; // Remove first

        // Act
        _sut.RemoveDefeatedCombatant(combatant);

        // Assert
        _gameState.CombatState.TurnIndex.Should().Be(1); // Adjusted down by 1
    }

    [Fact]
    public void RemoveDefeatedCombatant_DoesNotAdjustTurnIndex_WhenRemovedAfterCurrent()
    {
        // Arrange
        SetupActiveCombat(combatantCount: 3);
        _gameState.CombatState!.TurnIndex = 0; // Currently on first combatant
        var combatant = _gameState.CombatState.TurnOrder[2]; // Remove last

        // Act
        _sut.RemoveDefeatedCombatant(combatant);

        // Assert
        _gameState.CombatState.TurnIndex.Should().Be(0); // Unchanged
    }

    [Fact]
    public void RemoveDefeatedCombatant_WhenNoCombat_DoesNotThrow()
    {
        // Arrange
        _gameState.CombatState = null;
        var combatant = new Combatant { Name = "Test" };

        // Act
        var act = () => _sut.RemoveDefeatedCombatant(combatant);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region CheckVictoryCondition Tests

    [Fact]
    public void CheckVictoryCondition_WhenNoEnemiesRemain_ReturnsTrue()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        _gameState.CombatState.TurnOrder.Remove(enemy);

        // Act
        var result = _sut.CheckVictoryCondition();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckVictoryCondition_WhenEnemiesRemain_ReturnsFalse()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        var result = _sut.CheckVictoryCondition();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckVictoryCondition_WhenNoCombat_ReturnsFalse()
    {
        // Arrange
        _gameState.CombatState = null;

        // Act
        var result = _sut.CheckVictoryCondition();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCombatStatus Tests

    [Fact]
    public void GetCombatStatus_WhenNoCombat_ReturnsNoActiveCombatMessage()
    {
        // Arrange
        _gameState.CombatState = null;

        // Act
        var result = _sut.GetCombatStatus();

        // Assert
        result.Should().Contain("No active combat");
    }

    [Fact]
    public void GetCombatStatus_IncludesRoundNumber()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        _gameState.CombatState!.RoundNumber = 3;

        // Act
        var result = _sut.GetCombatStatus();

        // Assert
        result.Should().Contain("Round 3");
    }

    [Fact]
    public void GetCombatStatus_IncludesAllCombatants()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");

        // Act
        var result = _sut.GetCombatStatus();

        // Assert
        result.Should().Contain("[PLAYER]");
        result.Should().Contain("[ENEMY]");
        result.Should().Contain("Draugr");
    }

    [Fact]
    public void GetCombatStatus_ShowsHpAndStamina()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        player.CurrentHp = 75;
        player.MaxHp = 100;
        player.CurrentStamina = 40;
        player.MaxStamina = 60;

        // Act
        var result = _sut.GetCombatStatus();

        // Assert
        result.Should().Contain("HP 75/100");
        result.Should().Contain("Stamina 40/60");
    }

    #endregion

    #region Helper Methods

    private static CharacterEntity CreateTestCharacter(string name = "TestPlayer")
    {
        return new CharacterEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Finesse = 5,
            Wits = 4,
            CurrentHP = 100,
            MaxHP = 100
        };
    }

    private static Enemy CreateTestEnemy(string name = "TestEnemy")
    {
        return new Enemy
        {
            Id = Guid.NewGuid(),
            Name = name,
            MaxHp = 50,
            CurrentHp = 50
        };
    }

    private void SetupActiveCombat(int combatantCount)
    {
        var combatState = new CombatState
        {
            RoundNumber = 1,
            TurnIndex = 0
        };

        for (int i = 0; i < combatantCount; i++)
        {
            combatState.TurnOrder.Add(new Combatant
            {
                Name = $"Combatant{i}",
                Initiative = 10 - i
            });
        }

        _gameState.CombatState = combatState;
        _gameState.Phase = GamePhase.Combat;
    }

    private void SetupActiveCombatWithPlayerAndEnemy(bool playerFirst, string enemyName = "Enemy")
    {
        var combatState = new CombatState
        {
            RoundNumber = 1,
            TurnIndex = 0
        };

        var player = new Combatant
        {
            Name = "TestPlayer",
            IsPlayer = true,
            CurrentHp = 100,
            MaxHp = 100,
            CurrentStamina = 60,
            MaxStamina = 60,
            CharacterSource = CreateTestCharacter()
        };

        var enemy = new Combatant
        {
            Name = enemyName,
            IsPlayer = false,
            CurrentHp = 50,
            MaxHp = 50,
            CurrentStamina = 35,
            MaxStamina = 35,
            EnemySource = CreateTestEnemy(enemyName)
        };

        if (playerFirst)
        {
            combatState.TurnOrder.Add(player);
            combatState.TurnOrder.Add(enemy);
        }
        else
        {
            combatState.TurnOrder.Add(enemy);
            combatState.TurnOrder.Add(player);
        }

        _gameState.CombatState = combatState;
        _gameState.Phase = GamePhase.Combat;
    }

    #endregion

    #region LogCombatEvent Tests

    [Fact]
    public void LogCombatEvent_AddsMessageToLog()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        _sut.LogCombatEvent("Test message");

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain("Test message");
    }

    [Fact]
    public void LogCombatEvent_MaxTenEntries_RemovesOldest()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act - Add 12 messages
        for (int i = 1; i <= 12; i++)
        {
            _sut.LogCombatEvent($"Message {i}");
        }

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().HaveCount(10);
        viewModel.CombatLog.Should().NotContain("Message 1");
        viewModel.CombatLog.Should().NotContain("Message 2");
        viewModel.CombatLog.Should().Contain("Message 3");
        viewModel.CombatLog.Should().Contain("Message 12");
    }

    [Fact]
    public void LogCombatEvent_PreservesInsertionOrder()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        _sut.LogCombatEvent("First");
        _sut.LogCombatEvent("Second");
        _sut.LogCombatEvent("Third");

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog[0].Should().Be("First");
        viewModel.CombatLog[1].Should().Be("Second");
        viewModel.CombatLog[2].Should().Be("Third");
    }

    #endregion

    #region GetViewModel Tests

    [Fact]
    public void GetViewModel_WhenNoCombat_ReturnsNull()
    {
        // Arrange
        _gameState.CombatState = null;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetViewModel_ReturnsCorrectRoundNumber()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        _gameState.CombatState!.RoundNumber = 5;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.RoundNumber.Should().Be(5);
    }

    [Fact]
    public void GetViewModel_ReturnsActiveCombatantName()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.ActiveCombatantName.Should().Be("TestPlayer");
    }

    [Fact]
    public void GetViewModel_ReturnsAllCombatantsInTurnOrder()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.TurnOrder.Should().HaveCount(2);
        result.TurnOrder[0].Name.Should().Be("TestPlayer");
        result.TurnOrder[1].Name.Should().Be("Draugr");
    }

    [Fact]
    public void GetViewModel_MarksActiveCombatant()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.TurnOrder[0].IsActive.Should().BeTrue();
        result.TurnOrder[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetViewModel_PlayerShowsExactHpNumbers()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        player.CurrentHp = 75;
        player.MaxHp = 100;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var playerView = result!.TurnOrder.First(c => c.IsPlayer);
        playerView.HealthStatus.Should().Be("75/100");
    }

    [Fact]
    public void GetViewModel_EnemyShowsNarrativeHealth_Healthy()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 50;
        enemy.MaxHp = 50; // 100%

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var enemyView = result!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.HealthStatus.Should().Contain("Healthy");
    }

    [Fact]
    public void GetViewModel_EnemyShowsNarrativeHealth_Wounded()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 25;
        enemy.MaxHp = 50; // 50%

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var enemyView = result!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.HealthStatus.Should().Contain("Wounded");
    }

    [Fact]
    public void GetViewModel_EnemyShowsNarrativeHealth_Critical()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 10;
        enemy.MaxHp = 50; // 20%

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var enemyView = result!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.HealthStatus.Should().Contain("Critical");
    }

    [Fact]
    public void GetViewModel_EnemyShowsNarrativeHealth_Dead()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 0;
        enemy.MaxHp = 50;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var enemyView = result!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.HealthStatus.Should().Contain("Dead");
    }

    [Fact]
    public void GetViewModel_IncludesPlayerStats()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var player = _gameState.CombatState!.TurnOrder.First(c => c.IsPlayer);
        player.CurrentHp = 80;
        player.MaxHp = 100;
        player.CurrentStamina = 45;
        player.MaxStamina = 60;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.PlayerStats.CurrentHp.Should().Be(80);
        result.PlayerStats.MaxHp.Should().Be(100);
        result.PlayerStats.CurrentStamina.Should().Be(45);
        result.PlayerStats.MaxStamina.Should().Be(60);
    }

    [Fact]
    public void GetViewModel_IncludesCombatLog()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        _sut.LogCombatEvent("Test event");

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result!.CombatLog.Should().Contain("Test event");
    }

    [Fact]
    public void GetViewModel_WhenNoPlayer_ReturnsNull()
    {
        // Arrange - Create combat state with only enemies (abnormal but possible)
        _gameState.CombatState = new CombatState
        {
            RoundNumber = 1,
            TurnIndex = 0
        };
        _gameState.CombatState.TurnOrder.Add(new Combatant
        {
            Name = "Enemy",
            IsPlayer = false,
            CurrentHp = 50,
            MaxHp = 50
        });

        // Act
        var result = _sut.GetViewModel();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Narrative Health Thresholds Tests

    [Theory]
    [InlineData(100, 100, "Healthy")]    // 100%
    [InlineData(76, 100, "Healthy")]     // 76% (threshold)
    [InlineData(75, 100, "Wounded")]     // 75% (boundary)
    [InlineData(50, 100, "Wounded")]     // 50%
    [InlineData(26, 100, "Wounded")]     // 26% (threshold)
    [InlineData(25, 100, "Critical")]    // 25% (boundary)
    [InlineData(10, 100, "Critical")]    // 10%
    [InlineData(1, 100, "Critical")]     // 1%
    [InlineData(0, 100, "Dead")]         // 0%
    public void GetViewModel_NarrativeHealthThresholds_AreCorrect(int currentHp, int maxHp, string expectedNarrative)
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true);
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = currentHp;
        enemy.MaxHp = maxHp;

        // Act
        var result = _sut.GetViewModel();

        // Assert
        var enemyView = result!.TurnOrder.First(c => !c.IsPlayer);
        enemyView.HealthStatus.Should().Contain(expectedNarrative);
    }

    #endregion

    #region StartCombat Combat Log Tests

    [Fact]
    public void StartCombat_ClearsPreviousCombatLog()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy() };

        // First combat
        _sut.StartCombat(enemies);
        _sut.LogCombatEvent("Old combat message");

        // Act - Start new combat
        _sut.StartCombat(enemies);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().NotContain("Old combat message");
    }

    [Fact]
    public void StartCombat_AddsInitialCombatBeginsMessage()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy("Draugr") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain(m => m.Contains("Combat begins"));
    }

    [Fact]
    public void StartCombat_LogsEnemyNames()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter();
        var enemies = new List<Enemy> { CreateTestEnemy("Draugr") };

        // Act
        _sut.StartCombat(enemies);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain(m => m.Contains("Draugr"));
    }

    #endregion

    #region ExecutePlayerAttack Combat Log Tests

    [Fact]
    public void ExecutePlayerAttack_Hit_LogsToСombatLog()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");
        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 8, true));

        // Act
        _sut.ExecutePlayerAttack("Draugr", AttackType.Standard);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain(m => m.Contains("Draugr"));
    }

    [Fact]
    public void ExecutePlayerAttack_Miss_LogsToСombatLog()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");
        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Miss, -1, 0, 0, false));

        // Act
        _sut.ExecutePlayerAttack("Draugr", AttackType.Standard);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain(m => m.Contains("Miss") || m.Contains("evades"));
    }

    [Fact]
    public void ExecutePlayerAttack_Critical_LogsCriticalMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");
        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Critical, 6, 20, 15, true));

        // Act
        _sut.ExecutePlayerAttack("Draugr", AttackType.Standard);

        // Assert
        var viewModel = _sut.GetViewModel();
        viewModel!.CombatLog.Should().Contain(m => m.Contains("CRITICAL"));
    }

    [Fact]
    public void ExecutePlayerAttack_Victory_LogsVictoryMessage()
    {
        // Arrange
        SetupActiveCombatWithPlayerAndEnemy(playerFirst: true, enemyName: "Draugr");
        var enemy = _gameState.CombatState!.TurnOrder.First(c => !c.IsPlayer);
        enemy.CurrentHp = 5; // Low HP to ensure death

        _mockAttackResolution.Setup(a => a.ResolveMeleeAttack(
                It.IsAny<Combatant>(), It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(new AttackResult(AttackOutcome.Solid, 3, 10, 10, true));

        // Act
        _sut.ExecutePlayerAttack("Draugr", AttackType.Standard);

        // Assert
        var viewModel = _sut.GetViewModel();
        // Note: After victory, the combat might end. Check log exists before combat ended.
        viewModel?.CombatLog.Should().Contain(m => m.Contains("VICTORY"));
    }

    #endregion
}
