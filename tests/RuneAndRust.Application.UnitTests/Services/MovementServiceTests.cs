using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="MovementService"/> (v0.5.0b).
/// </summary>
[TestFixture]
public class MovementServiceTests
{
    private MovementService _service = null!;
    private Mock<ICombatGridService> _gridServiceMock = null!;
    private Mock<ILogger<MovementService>> _loggerMock = null!;
    private CombatGrid _grid = null!;

    [SetUp]
    public void SetUp()
    {
        _gridServiceMock = new Mock<ICombatGridService>();
        _loggerMock = new Mock<ILogger<MovementService>>();

        _grid = CombatGrid.CreateDefault();
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns(_grid);

        _service = new MovementService(_gridServiceMock.Object, _loggerMock.Object);
    }

    private Player CreateAndRegisterPlayer(GridPosition position)
    {
        var player = new Player("TestPlayer");
        player.ResetMovementPoints();
        player.SetCombatGridPosition(position);
        _grid.PlaceEntity(player.Id, position, isPlayer: true);
        _service.RegisterPlayer(player);
        return player;
    }

    private Monster CreateAndRegisterMonster(GridPosition position)
    {
        var monster = new Monster("Goblin", "A test goblin", 30, Stats.Default);
        monster.ResetMovementPoints();
        monster.SetCombatGridPosition(position);
        _grid.PlaceEntity(monster.Id, position, isPlayer: false);
        _service.RegisterMonster(monster);
        return monster;
    }

    // ===== Move Success Tests =====

    [Test]
    public void Move_CardinalDirection_SucceedsAndDeductsPoints()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        var initialPoints = player.MovementPointsRemaining;

        // Act
        var result = _service.Move(player.Id, MovementDirection.North);

        // Assert
        result.Success.Should().BeTrue();
        result.OldPosition.Should().Be(new GridPosition(4, 4));
        result.NewPosition.Should().Be(new GridPosition(4, 3));
        result.MovementPointsUsed.Should().Be(2);
        result.MovementPointsRemaining.Should().Be(initialPoints - 2);
        player.CombatGridPosition.Should().Be(new GridPosition(4, 3));
    }

    [Test]
    public void Move_DiagonalDirection_SucceedsAndDeducts3Points()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        var initialPoints = player.MovementPointsRemaining;

        // Act
        var result = _service.Move(player.Id, MovementDirection.NorthEast);

        // Assert
        result.Success.Should().BeTrue();
        result.NewPosition.Should().Be(new GridPosition(5, 3));
        result.MovementPointsUsed.Should().Be(3);
        result.MovementPointsRemaining.Should().Be(initialPoints - 3);
    }

    // ===== Move Failure Tests =====

    [Test]
    public void Move_NoActiveGrid_FailsWithCorrectReason()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);
        var player = new Player("TestPlayer");
        _service.RegisterPlayer(player);

        // Act
        var result = _service.Move(player.Id, MovementDirection.North);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(MovementFailureReason.NoActiveGrid);
    }

    [Test]
    public void Move_EntityNotOnGrid_FailsWithCorrectReason()
    {
        // Arrange
        var player = new Player("TestPlayer");
        _service.RegisterPlayer(player);
        // Player not placed on grid

        // Act
        var result = _service.Move(player.Id, MovementDirection.North);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(MovementFailureReason.EntityNotOnGrid);
    }

    [Test]
    public void Move_OutOfBounds_FailsWithCorrectReason()
    {
        // Arrange - place player at edge
        var player = CreateAndRegisterPlayer(new GridPosition(0, 0));

        // Act - try to move out of bounds
        var result = _service.Move(player.Id, MovementDirection.NorthWest);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(MovementFailureReason.OutOfBounds);
    }

    [Test]
    public void Move_CellOccupied_FailsWithCorrectReason()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        _ = CreateAndRegisterMonster(new GridPosition(4, 3)); // Place monster north of player

        // Act
        var result = _service.Move(player.Id, MovementDirection.North);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(MovementFailureReason.CellOccupied);
        result.Message.Should().Contain("Goblin");
    }

    [Test]
    public void Move_InsufficientPoints_FailsWithCorrectReason()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        // Use up almost all points
        player.UseMovementPoints(7); // Only 1 point left

        // Act
        var result = _service.Move(player.Id, MovementDirection.North); // Needs 2

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(MovementFailureReason.InsufficientMovementPoints);
    }

    // ===== GetRemainingMovement Tests =====

    [Test]
    public void GetRemainingMovement_ReturnsCorrectPoints()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        player.UseMovementPoints(3);

        // Act
        var remaining = _service.GetRemainingMovement(player.Id);

        // Assert
        remaining.Should().Be(5);
    }

    // ===== ResetMovement Tests =====

    [Test]
    public void ResetMovement_ResetsEntityPoints()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        player.UseMovementPoints(6);

        // Act
        _service.ResetMovement(player.Id);

        // Assert
        player.MovementPointsRemaining.Should().Be(8);
    }

    [Test]
    public void ResetAllMovement_ResetsAllEntities()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        var monster = CreateAndRegisterMonster(new GridPosition(4, 1));
        player.UseMovementPoints(4);
        monster.UseMovementPoints(3);

        // Act
        _service.ResetAllMovement();

        // Assert
        player.MovementPointsRemaining.Should().Be(8);
        monster.MovementPointsRemaining.Should().Be(6);
    }

    // ===== CanMove Tests =====

    [Test]
    public void CanMove_ValidMove_ReturnsTrue()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));

        // Act
        var canMove = _service.CanMove(player.Id, MovementDirection.North);

        // Assert
        canMove.Should().BeTrue();
    }

    [Test]
    public void CanMove_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var player = CreateAndRegisterPlayer(new GridPosition(4, 4));
        player.UseMovementPoints(7);

        // Act
        var canMove = _service.CanMove(player.Id, MovementDirection.North);

        // Assert
        canMove.Should().BeFalse();
    }

    // ===== GetValidDirections Tests =====

    [Test]
    public void GetValidDirections_ReturnsOnlyValidMoves()
    {
        // Arrange - place at corner with limited movement
        var player = CreateAndRegisterPlayer(new GridPosition(0, 0));

        // Act
        var validDirections = _service.GetValidDirections(player.Id).ToList();

        // Assert - only south, east, and SE should be valid
        validDirections.Should().Contain(MovementDirection.South);
        validDirections.Should().Contain(MovementDirection.East);
        validDirections.Should().Contain(MovementDirection.SouthEast);
        validDirections.Should().NotContain(MovementDirection.North);
        validDirections.Should().NotContain(MovementDirection.West);
    }

    // ===== GetMovementCost Tests =====

    [Test]
    public void GetMovementCost_ReturnsCorrectCost()
    {
        // Act & Assert
        _service.GetMovementCost(MovementDirection.North).Should().Be(2);
        _service.GetMovementCost(MovementDirection.NorthEast).Should().Be(3);
    }
}
