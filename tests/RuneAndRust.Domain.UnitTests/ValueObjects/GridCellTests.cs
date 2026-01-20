using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="GridCell"/> value object (v0.5.0a).
/// </summary>
[TestFixture]
public class GridCellTests
{
    // ===== Create Tests =====

    [Test]
    public void Create_WithPosition_CreatesEmptyCell()
    {
        // Arrange
        var position = new GridPosition(3, 5);

        // Act
        var cell = GridCell.Create(position);

        // Assert
        cell.Position.Should().Be(position);
        cell.IsOccupied.Should().BeFalse();
        cell.OccupantId.Should().BeNull();
        cell.IsPlayerOccupied.Should().BeFalse();
        cell.IsPassable.Should().BeTrue();
    }

    [Test]
    public void Create_WithCoordinates_CreatesEmptyCell()
    {
        // Act
        var cell = GridCell.Create(2, 4);

        // Assert
        cell.Position.X.Should().Be(2);
        cell.Position.Y.Should().Be(4);
        cell.IsOccupied.Should().BeFalse();
    }

    // ===== PlaceEntity Tests =====

    [Test]
    public void PlaceEntity_OnEmptyCell_ReturnsTrue()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        var entityId = Guid.NewGuid();

        // Act
        var result = cell.PlaceEntity(entityId, isPlayer: false);

        // Assert
        result.Should().BeTrue();
        cell.IsOccupied.Should().BeTrue();
        cell.OccupantId.Should().Be(entityId);
        cell.IsPlayerOccupied.Should().BeFalse();
    }

    [Test]
    public void PlaceEntity_AsPlayer_SetsIsPlayerOccupied()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        var playerId = Guid.NewGuid();

        // Act
        var result = cell.PlaceEntity(playerId, isPlayer: true);

        // Assert
        result.Should().BeTrue();
        cell.IsPlayerOccupied.Should().BeTrue();
    }

    [Test]
    public void PlaceEntity_OnOccupiedCell_ReturnsFalse()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);

        // Act
        var result = cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void PlaceEntity_OnImpassableCell_ReturnsFalse()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetPassable(false);

        // Act
        var result = cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);

        // Assert
        result.Should().BeFalse();
        cell.IsOccupied.Should().BeFalse();
    }

    // ===== RemoveEntity Tests =====

    [Test]
    public void RemoveEntity_WhenOccupied_ReturnsTrue()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: true);

        // Act
        var result = cell.RemoveEntity();

        // Assert
        result.Should().BeTrue();
        cell.IsOccupied.Should().BeFalse();
        cell.OccupantId.Should().BeNull();
        cell.IsPlayerOccupied.Should().BeFalse();
    }

    [Test]
    public void RemoveEntity_WhenEmpty_ReturnsFalse()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        var result = cell.RemoveEntity();

        // Assert
        result.Should().BeFalse();
    }

    // ===== SetPassable Tests =====

    [Test]
    public void SetPassable_ToFalse_BlocksMovement()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetPassable(false);

        // Assert
        cell.IsPassable.Should().BeFalse();
    }

    // ===== GetDisplayChar Tests =====

    [Test]
    public void GetDisplayChar_EmptyCell_ReturnsDot()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        var display = cell.GetDisplayChar();

        // Assert
        display.Should().Be('.');
    }

    [Test]
    public void GetDisplayChar_PlayerOccupied_ReturnsAt()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: true);

        // Act
        var display = cell.GetDisplayChar();

        // Assert
        display.Should().Be('@');
    }

    [Test]
    public void GetDisplayChar_MonsterOccupied_ReturnsM()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);

        // Act
        var display = cell.GetDisplayChar();

        // Assert
        display.Should().Be('M');
    }

    [Test]
    public void GetDisplayChar_Impassable_ReturnsHash()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetPassable(false);

        // Act
        var display = cell.GetDisplayChar();

        // Assert
        display.Should().Be('#');
    }
}
