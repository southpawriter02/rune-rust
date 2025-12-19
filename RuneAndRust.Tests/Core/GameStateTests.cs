using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the GameState class.
/// Validates state initialization, mutation, and reset behavior.
/// </summary>
public class GameStateTests
{
    #region Constructor/Default Tests

    [Fact]
    public void GameState_DefaultPhase_ShouldBeMainMenu()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.Phase.Should().Be(GamePhase.MainMenu);
    }

    [Fact]
    public void GameState_DefaultCurrentCharacter_ShouldBeNull()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.CurrentCharacter.Should().BeNull();
    }

    [Fact]
    public void GameState_DefaultTurnCount_ShouldBeZero()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.TurnCount.Should().Be(0);
    }

    [Fact]
    public void GameState_DefaultIsSessionActive_ShouldBeFalse()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.IsSessionActive.Should().BeFalse();
    }

    [Fact]
    public void GameState_DefaultPendingAction_ShouldBeNone()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.PendingAction.Should().Be(PendingGameAction.None);
    }

    [Fact]
    public void GameState_DefaultCurrentRoomId_ShouldBeNull()
    {
        // Arrange & Act
        var state = new GameState();

        // Assert
        state.CurrentRoomId.Should().BeNull();
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void GameState_Phase_CanBeSet()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.Phase = GamePhase.Exploration;

        // Assert
        state.Phase.Should().Be(GamePhase.Exploration);
    }

    [Fact]
    public void GameState_CurrentCharacter_CanBeSet()
    {
        // Arrange
        var state = new GameState();
        var character = new Character("Test Hero");

        // Act
        state.CurrentCharacter = character;

        // Assert
        state.CurrentCharacter.Should().Be(character);
        state.CurrentCharacter!.Name.Should().Be("Test Hero");
    }

    [Fact]
    public void GameState_TurnCount_CanBeSet()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.TurnCount = 42;

        // Assert
        state.TurnCount.Should().Be(42);
    }

    [Fact]
    public void GameState_IsSessionActive_CanBeSet()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsSessionActive = true;

        // Assert
        state.IsSessionActive.Should().BeTrue();
    }

    [Fact]
    public void GameState_PendingAction_CanBeSet()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.PendingAction = PendingGameAction.Save;

        // Assert
        state.PendingAction.Should().Be(PendingGameAction.Save);
    }

    [Theory]
    [InlineData(PendingGameAction.None)]
    [InlineData(PendingGameAction.Save)]
    [InlineData(PendingGameAction.Load)]
    public void GameState_PendingAction_CanBeSetToAnyValidAction(PendingGameAction action)
    {
        // Arrange
        var state = new GameState();

        // Act
        state.PendingAction = action;

        // Assert
        state.PendingAction.Should().Be(action);
    }

    [Fact]
    public void GameState_CurrentRoomId_CanBeSet()
    {
        // Arrange
        var state = new GameState();
        var roomId = Guid.NewGuid();

        // Act
        state.CurrentRoomId = roomId;

        // Assert
        state.CurrentRoomId.Should().Be(roomId);
    }

    [Fact]
    public void GameState_CurrentRoomId_CanBeSetToNull()
    {
        // Arrange
        var state = new GameState { CurrentRoomId = Guid.NewGuid() };

        // Act
        state.CurrentRoomId = null;

        // Assert
        state.CurrentRoomId.Should().BeNull();
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ShouldSetPhaseToMainMenu()
    {
        // Arrange
        var state = new GameState { Phase = GamePhase.Combat };

        // Act
        state.Reset();

        // Assert
        state.Phase.Should().Be(GamePhase.MainMenu);
    }

    [Fact]
    public void Reset_ShouldSetCurrentCharacterToNull()
    {
        // Arrange
        var state = new GameState { CurrentCharacter = new Character("Hero") };

        // Act
        state.Reset();

        // Assert
        state.CurrentCharacter.Should().BeNull();
    }

    [Fact]
    public void Reset_ShouldSetTurnCountToZero()
    {
        // Arrange
        var state = new GameState { TurnCount = 100 };

        // Act
        state.Reset();

        // Assert
        state.TurnCount.Should().Be(0);
    }

    [Fact]
    public void Reset_ShouldSetIsSessionActiveToFalse()
    {
        // Arrange
        var state = new GameState { IsSessionActive = true };

        // Act
        state.Reset();

        // Assert
        state.IsSessionActive.Should().BeFalse();
    }

    [Fact]
    public void Reset_ShouldSetPendingActionToNone()
    {
        // Arrange
        var state = new GameState { PendingAction = PendingGameAction.Save };

        // Act
        state.Reset();

        // Assert
        state.PendingAction.Should().Be(PendingGameAction.None);
    }

    [Fact]
    public void Reset_ShouldSetCurrentRoomIdToNull()
    {
        // Arrange
        var state = new GameState { CurrentRoomId = Guid.NewGuid() };

        // Act
        state.Reset();

        // Assert
        state.CurrentRoomId.Should().BeNull();
    }

    [Fact]
    public void Reset_ShouldResetAllProperties()
    {
        // Arrange
        var state = new GameState
        {
            Phase = GamePhase.Combat,
            CurrentCharacter = new Character("Hero"),
            TurnCount = 50,
            IsSessionActive = true,
            PendingAction = PendingGameAction.Save,
            CurrentRoomId = Guid.NewGuid()
        };

        // Act
        state.Reset();

        // Assert
        state.Phase.Should().Be(GamePhase.MainMenu);
        state.CurrentCharacter.Should().BeNull();
        state.TurnCount.Should().Be(0);
        state.IsSessionActive.Should().BeFalse();
        state.PendingAction.Should().Be(PendingGameAction.None);
        state.CurrentRoomId.Should().BeNull();
    }

    #endregion

    #region Phase Transition Tests

    [Theory]
    [InlineData(GamePhase.MainMenu)]
    [InlineData(GamePhase.Exploration)]
    [InlineData(GamePhase.Combat)]
    [InlineData(GamePhase.Quit)]
    public void GameState_Phase_CanBeSetToAnyValidPhase(GamePhase phase)
    {
        // Arrange
        var state = new GameState();

        // Act
        state.Phase = phase;

        // Assert
        state.Phase.Should().Be(phase);
    }

    #endregion

    #region TurnCount Tests

    [Fact]
    public void TurnCount_CanIncrement()
    {
        // Arrange
        var state = new GameState { TurnCount = 0 };

        // Act
        state.TurnCount++;
        state.TurnCount++;
        state.TurnCount++;

        // Assert
        state.TurnCount.Should().Be(3);
    }

    [Fact]
    public void TurnCount_CanBeNegative()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.TurnCount = -1;

        // Assert
        state.TurnCount.Should().Be(-1);
    }

    #endregion

    #region Character Association Tests

    [Fact]
    public void CurrentCharacter_WhenSet_ShouldRetainCharacterProperties()
    {
        // Arrange
        var state = new GameState();
        var character = new Character("Test Hero");
        character.SetAttribute(RuneAndRust.Core.Enums.Attribute.Might, 8);

        // Act
        state.CurrentCharacter = character;

        // Assert
        state.CurrentCharacter!.Name.Should().Be("Test Hero");
        state.CurrentCharacter.GetAttribute(RuneAndRust.Core.Enums.Attribute.Might).Should().Be(8);
    }

    [Fact]
    public void CurrentCharacter_CanBeReassigned()
    {
        // Arrange
        var state = new GameState();
        var character1 = new Character("Hero One");
        var character2 = new Character("Hero Two");

        // Act
        state.CurrentCharacter = character1;
        state.CurrentCharacter = character2;

        // Assert
        state.CurrentCharacter!.Name.Should().Be("Hero Two");
    }

    #endregion
}
