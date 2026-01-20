using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="RangeService"/> extended range methods (v0.5.1b).
/// </summary>
[TestFixture]
public class RangeServiceExtendedTests
{
    private RangeService _service = null!;
    private Mock<ICombatGridService> _gridServiceMock = null!;
    private Mock<ILogger<RangeService>> _loggerMock = null!;
    private CombatGrid _grid = null!;

    private Guid _playerId;
    private Guid _monsterId;

    [SetUp]
    public void SetUp()
    {
        _gridServiceMock = new Mock<ICombatGridService>();
        _loggerMock = new Mock<ILogger<RangeService>>();
        _grid = CombatGrid.CreateDefault();

        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns(_grid);

        // GetDistance delegates to grid service, so mock it to calculate from grid
        _gridServiceMock.Setup(g => g.GetDistance(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((Guid id1, Guid id2) =>
            {
                var pos1 = _grid.GetEntityPosition(id1);
                var pos2 = _grid.GetEntityPosition(id2);
                if (!pos1.HasValue || !pos2.HasValue) return null;
                return pos1.Value.DistanceTo(pos2.Value);
            });

        _service = new RangeService(_gridServiceMock.Object, _loggerMock.Object);

        _playerId = Guid.NewGuid();
        _monsterId = Guid.NewGuid();
    }

    private void PlaceEntities(GridPosition playerPos, GridPosition monsterPos)
    {
        _grid.PlaceEntity(_playerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(_monsterId, monsterPos, isPlayer: false);
    }

    private static Item CreateBow(int range = 12, int minRange = 2, int? optimalRange = 6)
    {
        var weapon = Item.CreateSword();
        weapon.SetRange(range, RangeType.Ranged);
        weapon.SetExtendedRangeProperties(
            minRange, optimalRange, rangePenalty: 1,
            thrown: false, twoHanded: true, meleeCapable: false, requiresAmmunition: true);
        return weapon;
    }

    // ===== CheckFullRange Tests =====

    [Test]
    public void CheckFullRange_TooClose_ReturnsTooCloseFailure()
    {
        // Arrange - distance 1, minRange 2
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));
        var bow = CreateBow(minRange: 2);

        // Act
        var result = _service.CheckFullRange(_playerId, _monsterId, bow);

        // Assert
        result.InRange.Should().BeFalse();
        result.TooClose.Should().BeTrue();
        result.FailureReason.Should().Be(RangeFailureReason.TooClose);
    }

    [Test]
    public void CheckFullRange_OutOfRange_ReturnsOutOfRangeFailure()
    {
        // Arrange - distance 7 with range 5
        PlaceEntities(new GridPosition(0, 0), new GridPosition(7, 0));
        var bow = CreateBow(range: 5);

        // Act
        var result = _service.CheckFullRange(_playerId, _monsterId, bow);

        // Assert
        result.InRange.Should().BeFalse();
        result.FailureReason.Should().Be(RangeFailureReason.OutOfRange);
    }

    [Test]
    public void CheckFullRange_InOptimalRange_ReturnsNoPenalty()
    {
        // Arrange - distance 4, optimal 6
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 0)); // Dist 4
        var bow = CreateBow(range: 12, optimalRange: 6);

        // Act
        var result = _service.CheckFullRange(_playerId, _monsterId, bow);

        // Assert
        result.InRange.Should().BeTrue();
        result.IsOptimal.Should().BeTrue();
        result.Penalty.Should().Be(0);
    }

    [Test]
    public void CheckFullRange_BeyondOptimal_ReturnsPenalty()
    {
        // Arrange - distance 7, optimal 5
        PlaceEntities(new GridPosition(0, 0), new GridPosition(7, 0)); // Dist 7
        var bow = CreateBow(range: 12, optimalRange: 5);

        // Act
        var result = _service.CheckFullRange(_playerId, _monsterId, bow);

        // Assert
        result.InRange.Should().BeTrue();
        result.IsOptimal.Should().BeFalse();
        result.Penalty.Should().Be(2); // 7 - 5 = 2
    }

    [Test]
    public void CheckFullRange_EntityNotOnGrid_ReturnsFailure()
    {
        // Arrange - only place player
        _grid.PlaceEntity(_playerId, new GridPosition(4, 4), isPlayer: true);
        var bow = CreateBow();

        // Act
        var result = _service.CheckFullRange(_playerId, _monsterId, bow);

        // Assert
        result.InRange.Should().BeFalse();
    }

    // ===== GetRangePenalty Tests =====

    [Test]
    public void GetRangePenalty_InOptimal_ReturnsZero()
    {
        // Arrange
        var bow = CreateBow(optimalRange: 6);

        // Act
        var penalty = _service.GetRangePenalty(5, bow);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetRangePenalty_BeyondOptimal_ReturnsPenalty()
    {
        // Arrange - optimal 5
        var bow = CreateBow(optimalRange: 5);

        // Act - distance 8 = 3 beyond optimal
        var penalty = _service.GetRangePenalty(8, bow);

        // Assert
        penalty.Should().Be(3);
    }

    // ===== GetAbilityRangePenalty Tests =====

    [Test]
    public void GetAbilityRangePenalty_NoPenaltyConfigured_ReturnsZero()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "fireball", Name = "Fireball", Range = 8, RangePenalty = 0
        };

        // Act
        var penalty = _service.GetAbilityRangePenalty(10, ability);

        // Assert
        penalty.Should().Be(0);
    }

    // ===== IsTooClose Tests =====

    [Test]
    public void IsTooClose_NoMinRange_ReturnsFalse()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));
        var sword = Item.CreateSword(); // No min range

        // Act
        var tooClose = _service.IsTooClose(_playerId, _monsterId, sword);

        // Assert
        tooClose.Should().BeFalse();
    }

    [Test]
    public void IsTooClose_WithinMinRange_ReturnsTrue()
    {
        // Arrange - distance 1, minRange 2
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));
        var bow = CreateBow(minRange: 2);

        // Act
        var tooClose = _service.IsTooClose(_playerId, _monsterId, bow);

        // Assert
        tooClose.Should().BeTrue();
    }

    [Test]
    public void IsTooClose_BeyondMinRange_ReturnsFalse()
    {
        // Arrange - distance 3, minRange 2
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 1));
        var bow = CreateBow(minRange: 2);

        // Act
        var tooClose = _service.IsTooClose(_playerId, _monsterId, bow);

        // Assert
        tooClose.Should().BeFalse();
    }
}
