using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="CombatGrid"/> entity (v0.5.0a).
/// </summary>
[TestFixture]
public class CombatGridTests
{
    // ===== Create Tests =====

    [Test]
    public void Create_WithValidDimensions_CreatesGrid()
    {
        // Act
        var grid = CombatGrid.Create(10, 8);

        // Assert
        grid.Id.Should().NotBe(Guid.Empty);
        grid.Width.Should().Be(10);
        grid.Height.Should().Be(8);
        grid.Cells.Should().HaveCount(80); // 10 * 8
    }

    [Test]
    public void Create_WithRoomId_AssociatesRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var grid = CombatGrid.Create(8, 8, roomId);

        // Assert
        grid.RoomId.Should().Be(roomId);
    }

    [TestCase(2)]
    [TestCase(21)]
    public void Create_WithInvalidWidth_ThrowsException(int invalidWidth)
    {
        // Act
        var act = () => CombatGrid.Create(invalidWidth, 8);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(2)]
    [TestCase(21)]
    public void Create_WithInvalidHeight_ThrowsException(int invalidHeight)
    {
        // Act
        var act = () => CombatGrid.Create(8, invalidHeight);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateDefault_Creates8x8Grid()
    {
        // Act
        var grid = CombatGrid.CreateDefault();

        // Assert
        grid.Width.Should().Be(8);
        grid.Height.Should().Be(8);
        grid.Cells.Should().HaveCount(64);
    }

    // ===== GetCell Tests =====

    [Test]
    public void GetCell_ValidPosition_ReturnsCell()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var position = new GridPosition(3, 4);

        // Act
        var cell = grid.GetCell(position);

        // Assert
        cell.Should().NotBeNull();
        cell!.Position.Should().Be(position);
    }

    [Test]
    public void GetCell_ByCoordinates_ReturnsCell()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        var cell = grid.GetCell(2, 3);

        // Assert
        cell.Should().NotBeNull();
        cell!.Position.X.Should().Be(2);
        cell.Position.Y.Should().Be(3);
    }

    [Test]
    public void GetCell_OutOfBounds_ReturnsNull()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        var cell = grid.GetCell(new GridPosition(10, 10));

        // Assert
        cell.Should().BeNull();
    }

    // ===== IsInBounds Tests =====

    [Test]
    public void IsInBounds_ValidPosition_ReturnsTrue()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act & Assert
        grid.IsInBounds(new GridPosition(0, 0)).Should().BeTrue();
        grid.IsInBounds(new GridPosition(7, 7)).Should().BeTrue();
        grid.IsInBounds(new GridPosition(4, 4)).Should().BeTrue();
    }

    [Test]
    public void IsInBounds_OutsideGrid_ReturnsFalse()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act & Assert
        grid.IsInBounds(new GridPosition(-1, 0)).Should().BeFalse();
        grid.IsInBounds(new GridPosition(0, -1)).Should().BeFalse();
        grid.IsInBounds(new GridPosition(8, 0)).Should().BeFalse();
        grid.IsInBounds(new GridPosition(0, 8)).Should().BeFalse();
    }

    // ===== PlaceEntity Tests =====

    [Test]
    public void PlaceEntity_ValidPosition_ReturnsTrue()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entityId = Guid.NewGuid();
        var position = new GridPosition(3, 3);

        // Act
        var result = grid.PlaceEntity(entityId, position, isPlayer: true);

        // Assert
        result.Should().BeTrue();
        grid.EntityPositions.Should().ContainKey(entityId);
        grid.EntityPositions[entityId].Should().Be(position);
    }

    [Test]
    public void PlaceEntity_OutOfBounds_ReturnsFalse()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        var result = grid.PlaceEntity(Guid.NewGuid(), new GridPosition(10, 10), isPlayer: false);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void PlaceEntity_OccupiedCell_ReturnsFalse()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var position = new GridPosition(3, 3);
        grid.PlaceEntity(Guid.NewGuid(), position, isPlayer: false);

        // Act
        var result = grid.PlaceEntity(Guid.NewGuid(), position, isPlayer: false);

        // Assert
        result.Should().BeFalse();
    }

    // ===== RemoveEntity Tests =====

    [Test]
    public void RemoveEntity_ExistingEntity_ReturnsTrue()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entityId = Guid.NewGuid();
        grid.PlaceEntity(entityId, new GridPosition(3, 3), isPlayer: false);

        // Act
        var result = grid.RemoveEntity(entityId);

        // Assert
        result.Should().BeTrue();
        grid.EntityPositions.Should().NotContainKey(entityId);
    }

    [Test]
    public void RemoveEntity_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        var result = grid.RemoveEntity(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    // ===== MoveEntity Tests =====

    [Test]
    public void MoveEntity_ToValidPosition_UpdatesPosition()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entityId = Guid.NewGuid();
        var oldPos = new GridPosition(3, 3);
        var newPos = new GridPosition(4, 4);
        grid.PlaceEntity(entityId, oldPos, isPlayer: true);

        // Act
        var result = grid.MoveEntity(entityId, newPos);

        // Assert
        result.Should().BeTrue();
        grid.GetEntityPosition(entityId).Should().Be(newPos);
        grid.GetCell(oldPos)!.IsOccupied.Should().BeFalse();
        grid.GetCell(newPos)!.IsOccupied.Should().BeTrue();
    }

    [Test]
    public void MoveEntity_ToOccupiedCell_ReturnsFalseAndRollsBack()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var pos1 = new GridPosition(3, 3);
        var pos2 = new GridPosition(4, 4);
        grid.PlaceEntity(entity1, pos1, isPlayer: true);
        grid.PlaceEntity(entity2, pos2, isPlayer: false);

        // Act
        var result = grid.MoveEntity(entity1, pos2);

        // Assert
        result.Should().BeFalse();
        grid.GetEntityPosition(entity1).Should().Be(pos1); // Still at original position
    }

    // ===== Distance Tests =====

    [Test]
    public void GetDistanceBetween_TwoEntities_ReturnsDistance()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(0, 0), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(3, 4), isPlayer: false);

        // Act
        var distance = grid.GetDistanceBetween(entity1, entity2);

        // Assert - Chebyshev distance = max(3, 4) = 4
        distance.Should().Be(4);
    }

    [Test]
    public void GetDistanceBetween_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(0, 0), isPlayer: true);

        // Act
        var distance = grid.GetDistanceBetween(entity1, Guid.NewGuid());

        // Assert
        distance.Should().BeNull();
    }

    // ===== Adjacency Tests =====

    [Test]
    public void AreAdjacent_AdjacentEntities_ReturnsTrue()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(4, 4), isPlayer: false);

        // Act
        var areAdjacent = grid.AreAdjacent(entity1, entity2);

        // Assert
        areAdjacent.Should().BeTrue();
    }

    [Test]
    public void AreAdjacent_NonAdjacentEntities_ReturnsFalse()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(0, 0), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(5, 5), isPlayer: false);

        // Act
        var areAdjacent = grid.AreAdjacent(entity1, entity2);

        // Assert
        areAdjacent.Should().BeFalse();
    }

    // ===== GetEntitiesInRange Tests =====

    [Test]
    public void GetEntitiesInRange_ReturnsEntitiesWithinRange()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var entity3 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(4, 4), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(5, 5), isPlayer: false); // Distance 1
        grid.PlaceEntity(entity3, new GridPosition(0, 0), isPlayer: false); // Distance 4

        // Act
        var inRange = grid.GetEntitiesInRange(new GridPosition(4, 4), 2).ToList();

        // Assert
        inRange.Should().Contain(entity1);
        inRange.Should().Contain(entity2);
        inRange.Should().NotContain(entity3);
    }

    // ===== GetAdjacentCells Tests =====

    [Test]
    public void GetAdjacentCells_CenterPosition_Returns8Cells()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var center = new GridPosition(4, 4);

        // Act
        var adjacentCells = grid.GetAdjacentCells(center).ToList();

        // Assert
        adjacentCells.Should().HaveCount(8); // All 8 directions
    }

    [Test]
    public void GetAdjacentCells_CornerPosition_ReturnsLessCells()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var corner = new GridPosition(0, 0);

        // Act
        var adjacentCells = grid.GetAdjacentCells(corner).ToList();

        // Assert
        adjacentCells.Should().HaveCount(3); // Only South, East, SouthEast
    }
}
