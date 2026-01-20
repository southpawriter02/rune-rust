using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Player and Monster entity facing functionality.
/// </summary>
[TestFixture]
public class EntityFacingTests
{
    // ===== Player Facing Tests =====

    [Test]
    public void Player_DefaultFacing_IsSouth()
    {
        // Arrange & Act
        var player = new Player("TestHero");

        // Assert
        player.Facing.Should().Be(FacingDirection.South);
    }

    [Test]
    public void Player_SetFacing_ChangesDirection()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        player.SetFacing(FacingDirection.East);

        // Assert
        player.Facing.Should().Be(FacingDirection.East);
    }

    [Test]
    public void Player_FaceToward_UpdatesFacingCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");
        var currentPos = new GridPosition(5, 5);
        var targetPos = new GridPosition(5, 3); // North

        // Act
        player.FaceToward(targetPos, currentPos);

        // Assert
        player.Facing.Should().Be(FacingDirection.North);
    }

    [Test]
    public void Player_FaceToward_SamePosition_FacingUnchanged()
    {
        // Arrange
        var player = new Player("TestHero");
        player.SetFacing(FacingDirection.West);
        var currentPos = new GridPosition(5, 5);
        var targetPos = new GridPosition(5, 5); // Same position

        // Act
        player.FaceToward(targetPos, currentPos);

        // Assert
        player.Facing.Should().Be(FacingDirection.West);
    }

    // ===== Monster Facing Tests =====

    [Test]
    public void Monster_DefaultFacing_IsNorth()
    {
        // Arrange & Act
        var monster = Monster.CreateGoblin();

        // Assert
        monster.Facing.Should().Be(FacingDirection.North);
    }

    [Test]
    public void Monster_SetFacing_ChangesDirection()
    {
        // Arrange
        var monster = Monster.CreateGoblin();

        // Act
        monster.SetFacing(FacingDirection.SouthWest);

        // Assert
        monster.Facing.Should().Be(FacingDirection.SouthWest);
    }

    [Test]
    public void Monster_FaceToward_UpdatesFacingCorrectly()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var currentPos = new GridPosition(3, 3);
        var targetPos = new GridPosition(5, 5); // SouthEast

        // Act
        monster.FaceToward(targetPos, currentPos);

        // Assert
        monster.Facing.Should().Be(FacingDirection.SouthEast);
    }

    // ===== CombatGrid Facing Tests =====

    [Test]
    public void CombatGrid_GetEntityFacing_EntityNotTracked_ReturnsNull()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var unknownId = Guid.NewGuid();

        // Act
        var result = grid.GetEntityFacing(unknownId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CombatGrid_SetEntityFacing_StoresFacing()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var entityId = Guid.NewGuid();
        grid.PlaceEntity(entityId, new GridPosition(4, 4), isPlayer: false);

        // Act
        grid.SetEntityFacing(entityId, FacingDirection.West);
        var result = grid.GetEntityFacing(entityId);

        // Assert
        result.Should().Be(FacingDirection.West);
    }

    [Test]
    public void CombatGrid_FaceEntityToward_UpdatesFacing()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var entityId = Guid.NewGuid();
        var entityPos = new GridPosition(4, 4);
        var targetPos = new GridPosition(6, 2); // NorthEast

        grid.PlaceEntity(entityId, entityPos, isPlayer: false);

        // Act
        grid.FaceEntityToward(entityId, targetPos);
        var result = grid.GetEntityFacing(entityId);

        // Assert
        result.Should().Be(FacingDirection.NorthEast);
    }

    [Test]
    public void CombatGrid_FaceEntityToward_EntityNotOnGrid_DoesNothing()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var entityId = Guid.NewGuid();
        var targetPos = new GridPosition(6, 2);

        // Act - Should not throw
        grid.FaceEntityToward(entityId, targetPos);
        var result = grid.GetEntityFacing(entityId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CombatGrid_SetEntityFacing_OverwritesPreviousFacing()
    {
        // Arrange
        var grid = CombatGrid.Create(8, 8);
        var entityId = Guid.NewGuid();
        grid.PlaceEntity(entityId, new GridPosition(4, 4), isPlayer: false);
        grid.SetEntityFacing(entityId, FacingDirection.North);

        // Act
        grid.SetEntityFacing(entityId, FacingDirection.East);
        var result = grid.GetEntityFacing(entityId);

        // Assert
        result.Should().Be(FacingDirection.East);
    }
}
