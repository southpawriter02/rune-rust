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
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _mockInitiative = new Mock<IInitiativeService>();
        _mockLogger = new Mock<ILogger<CombatService>>();
        _gameState = new GameState();
        _sut = new CombatService(_gameState, _mockInitiative.Object, _mockLogger.Object);

        // Default setup for initiative service
        _mockInitiative.Setup(i => i.SortTurnOrder(It.IsAny<IEnumerable<Combatant>>()))
            .Returns<IEnumerable<Combatant>>(c => c.ToList());
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

    #endregion
}
