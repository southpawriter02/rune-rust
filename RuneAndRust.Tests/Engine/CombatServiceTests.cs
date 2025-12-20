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
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _mockInitiative = new Mock<IInitiativeService>();
        _mockAttackResolution = new Mock<IAttackResolutionService>();
        _mockLogger = new Mock<ILogger<CombatService>>();
        _gameState = new GameState();
        _sut = new CombatService(
            _gameState,
            _mockInitiative.Object,
            _mockAttackResolution.Object,
            _mockLogger.Object);

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());

        // Default setup for attack resolution service
        _mockAttackResolution.Setup(a => a.GetStaminaCost(It.IsAny<AttackType>()))
            .Returns(25);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(true);
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
}
