using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="LineOfSightService"/> (v0.5.1c).
/// </summary>
[TestFixture]
public class LineOfSightServiceTests
{
    private LineOfSightService _service = null!;
    private Mock<ICombatGridService> _gridServiceMock = null!;
    private Mock<ILogger<LineOfSightService>> _loggerMock = null!;
    private CombatGrid _grid = null!;

    private Guid _playerId;
    private Guid _monsterId;

    [SetUp]
    public void SetUp()
    {
        _gridServiceMock = new Mock<ICombatGridService>();
        _loggerMock = new Mock<ILogger<LineOfSightService>>();
        _grid = CombatGrid.CreateDefault();

        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns(_grid);

        _service = new LineOfSightService(_gridServiceMock.Object, _loggerMock.Object);

        _playerId = Guid.NewGuid();
        _monsterId = Guid.NewGuid();
    }

    private void PlaceEntities(GridPosition playerPos, GridPosition monsterPos)
    {
        _grid.PlaceEntity(_playerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(_monsterId, monsterPos, isPlayer: false);
    }

    // ===== HasLineOfSight Tests =====

    [Test]
    public void HasLineOfSight_WhenAdjacent_ReturnsTrue()
    {
        // Arrange - adjacent cells always have LOS
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));

        // Act
        var result = _service.HasLineOfSight(_playerId, _monsterId);

        // Assert
        result.HasLOS.Should().BeTrue();
        result.Message.Should().Contain("Adjacent");
    }

    [Test]
    public void HasLineOfSight_WhenPathClear_ReturnsTrue()
    {
        // Arrange - clear line
        PlaceEntities(new GridPosition(2, 2), new GridPosition(5, 2));

        // Act
        var result = _service.HasLineOfSight(_playerId, _monsterId);

        // Assert
        result.HasLOS.Should().BeTrue();
        result.BlockedBy.Should().BeNull();
    }

    [Test]
    public void HasLineOfSight_WhenWallBlocks_ReturnsFalse()
    {
        // Arrange
        PlaceEntities(new GridPosition(2, 2), new GridPosition(6, 2));
        // Place wall between them
        var wallCell = _grid.GetCell(new GridPosition(4, 2));
        wallCell!.SetPassable(false);

        // Act
        var result = _service.HasLineOfSight(_playerId, _monsterId);

        // Assert
        result.HasLOS.Should().BeFalse();
        result.BlockedBy.Should().Be(new GridPosition(4, 2));
    }

    [Test]
    public void HasLineOfSight_WhenEntityInPath_ReturnsTrue()
    {
        // Arrange - occupied cells don't block LOS
        PlaceEntities(new GridPosition(2, 2), new GridPosition(6, 2));
        var thirdEntityId = Guid.NewGuid();
        _grid.PlaceEntity(thirdEntityId, new GridPosition(4, 2), isPlayer: false);

        // Act
        var result = _service.HasLineOfSight(_playerId, _monsterId);

        // Assert
        result.HasLOS.Should().BeTrue();
    }

    [Test]
    public void HasLineOfSight_NoActiveGrid_ReturnsFalse()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);

        // Act
        var result = _service.HasLineOfSight(_playerId, _monsterId);

        // Assert
        result.HasLOS.Should().BeFalse();
    }

    // ===== GetLineCells Tests (Bresenham) =====

    [Test]
    public void GetLineCells_HorizontalLine_ReturnsCorrectPath()
    {
        // Arrange
        var from = new GridPosition(0, 0);
        var to = new GridPosition(4, 0);

        // Act
        var cells = _service.GetLineCells(from, to).ToList();

        // Assert
        cells.Should().HaveCount(5);
        cells.Should().ContainInOrder(
            new GridPosition(0, 0),
            new GridPosition(1, 0),
            new GridPosition(2, 0),
            new GridPosition(3, 0),
            new GridPosition(4, 0));
    }

    [Test]
    public void GetLineCells_VerticalLine_ReturnsCorrectPath()
    {
        // Arrange
        var from = new GridPosition(2, 1);
        var to = new GridPosition(2, 4);

        // Act
        var cells = _service.GetLineCells(from, to).ToList();

        // Assert
        cells.Should().HaveCount(4);
        cells.First().Should().Be(new GridPosition(2, 1));
        cells.Last().Should().Be(new GridPosition(2, 4));
    }

    [Test]
    public void GetLineCells_DiagonalLine_ReturnsCorrectPath()
    {
        // Arrange
        var from = new GridPosition(0, 0);
        var to = new GridPosition(3, 3);

        // Act
        var cells = _service.GetLineCells(from, to).ToList();

        // Assert
        cells.Should().HaveCount(4);
        cells.Should().Contain(new GridPosition(0, 0));
        cells.Should().Contain(new GridPosition(3, 3));
    }

    [Test]
    public void GetLineCells_SingleCell_ReturnsSelf()
    {
        // Arrange
        var pos = new GridPosition(5, 5);

        // Act
        var cells = _service.GetLineCells(pos, pos).ToList();

        // Assert
        cells.Should().ContainSingle().Which.Should().Be(pos);
    }

    // ===== GetFirstBlockingCell Tests =====

    [Test]
    public void GetFirstBlockingCell_NoPBlocker_ReturnsNull()
    {
        // Arrange
        var from = new GridPosition(2, 2);
        var to = new GridPosition(5, 2);

        // Act
        var result = _service.GetFirstBlockingCell(from, to);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetFirstBlockingCell_SkipsStartAndEnd_ReturnsNull()
    {
        // Arrange - start and end are not checked
        var from = new GridPosition(2, 2);
        var to = new GridPosition(4, 2);
        // Make end position blocking (shouldn't matter)
        _grid.GetCell(to)!.SetPassable(false);

        // Act - this should still return null since we skip end
        var result = _service.GetFirstBlockingCell(from, to);

        // Assert - checking only intermediate (position 3,2)
        result.Should().BeNull();
    }

    [Test]
    public void GetFirstBlockingCell_FindsFirstBlocker()
    {
        // Arrange
        _grid.GetCell(new GridPosition(3, 2))!.SetPassable(false);
        _grid.GetCell(new GridPosition(4, 2))!.SetPassable(false);

        // Act
        var result = _service.GetFirstBlockingCell(
            new GridPosition(1, 2), new GridPosition(5, 2));

        // Assert - should return first one
        result.Should().Be(new GridPosition(3, 2));
    }

    // ===== GetVisiblePositions Tests =====

    [Test]
    public void GetVisiblePositions_ReturnsVisibleCells()
    {
        // Arrange
        var from = new GridPosition(4, 4);

        // Act
        var visible = _service.GetVisiblePositions(from, maxRange: 2).ToList();

        // Assert - should include all positions within range 2 with LOS
        visible.Should().NotBeEmpty();
        visible.Should().NotContain(from); // Self excluded
    }

    [Test]
    public void GetVisiblePositions_ExcludesBlockedCells()
    {
        // Arrange
        var from = new GridPosition(4, 4);
        // Place a wall
        _grid.GetCell(new GridPosition(5, 4))!.SetPassable(false);

        // Act
        var visible = _service.GetVisiblePositions(from, maxRange: 3).ToList();

        // Assert - cells behind wall should be excluded
        visible.Should().NotContain(new GridPosition(6, 4));
    }
}
