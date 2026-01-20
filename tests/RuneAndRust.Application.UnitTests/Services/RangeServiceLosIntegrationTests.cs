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
/// Unit tests for RangeService LOS integration (v0.5.1c).
/// </summary>
[TestFixture]
public class RangeServiceLosIntegrationTests
{
    private RangeService _service = null!;
    private Mock<ICombatGridService> _gridServiceMock = null!;
    private Mock<ILineOfSightService> _losServiceMock = null!;
    private Mock<ILogger<RangeService>> _loggerMock = null!;
    private CombatGrid _grid = null!;

    private Guid _playerId;
    private Guid _monsterId;

    [SetUp]
    public void SetUp()
    {
        _gridServiceMock = new Mock<ICombatGridService>();
        _losServiceMock = new Mock<ILineOfSightService>();
        _loggerMock = new Mock<ILogger<RangeService>>();
        _grid = CombatGrid.CreateDefault();

        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns(_grid);

        // Mock GetDistance to calculate from grid
        _gridServiceMock.Setup(g => g.GetDistance(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((Guid id1, Guid id2) =>
            {
                var pos1 = _grid.GetEntityPosition(id1);
                var pos2 = _grid.GetEntityPosition(id2);
                if (!pos1.HasValue || !pos2.HasValue) return null;
                return pos1.Value.DistanceTo(pos2.Value);
            });

        _service = new RangeService(
            _gridServiceMock.Object, 
            _loggerMock.Object,
            _losServiceMock.Object);

        _playerId = Guid.NewGuid();
        _monsterId = Guid.NewGuid();
    }

    private void PlaceEntities(GridPosition playerPos, GridPosition monsterPos)
    {
        _grid.PlaceEntity(_playerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(_monsterId, monsterPos, isPlayer: false);
    }

    [Test]
    public void CheckRange_RangedWithLOS_Succeeds()
    {
        // Arrange
        PlaceEntities(new GridPosition(2, 2), new GridPosition(5, 2));
        _losServiceMock.Setup(l => l.HasLineOfSight(_playerId, _monsterId))
            .Returns(new LineOfSightResult(true, default, default, null, "Clear"));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 10, RangeType.Ranged);

        // Assert
        result.InRange.Should().BeTrue();
    }

    [Test]
    public void CheckRange_RangedWithoutLOS_Fails()
    {
        // Arrange
        PlaceEntities(new GridPosition(2, 2), new GridPosition(5, 2));
        var blockingPos = new GridPosition(4, 2);
        _losServiceMock.Setup(l => l.HasLineOfSight(_playerId, _monsterId))
            .Returns(new LineOfSightResult(false, default, default, blockingPos, "Blocked"));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 10, RangeType.Ranged);

        // Assert
        result.InRange.Should().BeFalse();
        result.FailureReason.Should().Be(RangeFailureReason.NoLineOfSight);
        result.BlockedBy.Should().Be(blockingPos);
    }

    [Test]
    public void CheckRange_MeleeDoesNotCheckLOS()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeTrue();
        // LOS service should NOT have been called for melee
        _losServiceMock.Verify(l => l.HasLineOfSight(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void CheckRange_ReachDoesNotCheckLOS()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 2));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 2, RangeType.Reach);

        // Assert
        result.InRange.Should().BeTrue();
        _losServiceMock.Verify(l => l.HasLineOfSight(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
