using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class GameSessionTests
{
    [Test]
    public void CreateNew_WithValidPlayerName_CreatesSession()
    {
        // Arrange & Act
        var session = GameSession.CreateNew("TestPlayer");

        // Assert
        session.Id.Should().NotBeEmpty();
        session.Player.Name.Should().Be("TestPlayer");
        session.State.Should().Be(GameState.Playing);
        session.CurrentRoom.Should().NotBeNull();
        session.Dungeon.Should().NotBeNull();
    }

    [Test]
    public void CreateNew_InitializesPlayerInStartingRoom()
    {
        // Arrange & Act
        var session = GameSession.CreateNew("TestPlayer");

        // Assert
        session.CurrentRoomId.Should().Be(session.Dungeon.StartingRoomId);
        session.CurrentRoom?.Name.Should().Be("Entrance Hall");
    }

    [Test]
    public void TryMovePlayer_WhenValidDirection_ReturnsTrue()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");
        var initialRoomId = session.CurrentRoomId;

        // Act - Entrance Hall has exits to North, East, West
        var result = session.TryMovePlayer(Direction.North);

        // Assert
        result.Should().BeTrue();
        session.CurrentRoomId.Should().NotBe(initialRoomId);
    }

    [Test]
    public void TryMovePlayer_WhenInvalidDirection_ReturnsFalse()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");
        var initialRoomId = session.CurrentRoomId;

        // Act - Entrance Hall has no South exit
        var result = session.TryMovePlayer(Direction.South);

        // Assert
        result.Should().BeFalse();
        session.CurrentRoomId.Should().Be(initialRoomId);
    }

    [Test]
    public void TryPickUpItem_WhenItemExists_ReturnsTrue()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");

        // Move to Armory where there's a sword
        session.TryMovePlayer(Direction.North);

        // Act
        var result = session.TryPickUpItem("Rusty Sword");

        // Assert
        result.Should().BeTrue();
        session.Player.Inventory.Items.Should().ContainSingle(i => i.Name == "Rusty Sword");
    }

    [Test]
    public void TryPickUpItem_WhenItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");

        // Act
        var result = session.TryPickUpItem("Nonexistent Item");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void SetState_UpdatesGameState()
    {
        // Arrange
        var session = GameSession.CreateNew("TestPlayer");

        // Act
        session.SetState(GameState.GameOver);

        // Assert
        session.State.Should().Be(GameState.GameOver);
    }
}
