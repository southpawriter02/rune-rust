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
/// Unit tests for <see cref="RangeService"/> (v0.5.1a).
/// </summary>
[TestFixture]
public class RangeServiceTests
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

        _service = new RangeService(_gridServiceMock.Object, _loggerMock.Object);

        // Setup default entities
        _playerId = Guid.NewGuid();
        _monsterId = Guid.NewGuid();
    }

    private void PlaceEntities(GridPosition playerPos, GridPosition monsterPos)
    {
        _grid.PlaceEntity(_playerId, playerPos, isPlayer: true);
        _grid.PlaceEntity(_monsterId, monsterPos, isPlayer: false);
    }

    // ===== CheckRange Tests =====

    [Test]
    public void CheckRange_NoActiveGrid_ReturnsNoActiveGridFailure()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeFalse();
        result.FailureReason.Should().Be(RangeFailureReason.NoActiveGrid);
    }

    [Test]
    public void CheckRange_AttackerNotOnGrid_ReturnsAttackerNotOnGridFailure()
    {
        // Arrange - only place monster
        _grid.PlaceEntity(_monsterId, new GridPosition(4, 3), isPlayer: false);

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeFalse();
        result.FailureReason.Should().Be(RangeFailureReason.AttackerNotOnGrid);
    }

    [Test]
    public void CheckRange_TargetNotOnGrid_ReturnsTargetNotOnGridFailure()
    {
        // Arrange - only place player
        _grid.PlaceEntity(_playerId, new GridPosition(4, 4), isPlayer: true);

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeFalse();
        result.FailureReason.Should().Be(RangeFailureReason.TargetNotOnGrid);
    }

    [Test]
    public void CheckRange_MeleeAdjacent_ReturnsInRange()
    {
        // Arrange - adjacent (distance 1)
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeTrue();
        result.Distance.Should().Be(1);
    }

    [Test]
    public void CheckRange_MeleeNotAdjacent_ReturnsNotAdjacent()
    {
        // Arrange - distance 2
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 2));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 1, RangeType.Melee);

        // Assert
        result.InRange.Should().BeFalse();
        result.Distance.Should().Be(2);
        result.FailureReason.Should().Be(RangeFailureReason.NotAdjacent);
    }

    [Test]
    public void CheckRange_ReachDistance2_ReturnsInRange()
    {
        // Arrange - distance 2
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 2));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 2, RangeType.Reach);

        // Assert
        result.InRange.Should().BeTrue();
        result.Distance.Should().Be(2);
    }

    [Test]
    public void CheckRange_ReachDistance3_ReturnsOutOfRange()
    {
        // Arrange - distance 3
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 1));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 2, RangeType.Reach);

        // Assert
        result.InRange.Should().BeFalse();
        result.Distance.Should().Be(3);
        result.FailureReason.Should().Be(RangeFailureReason.OutOfRange);
    }

    [Test]
    public void CheckRange_RangedInRange_ReturnsInRange()
    {
        // Arrange - distance 5 with range 12
        PlaceEntities(new GridPosition(2, 2), new GridPosition(7, 2));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 12, RangeType.Ranged);

        // Assert
        result.InRange.Should().BeTrue();
        result.Distance.Should().Be(5);
    }

    [Test]
    public void CheckRange_RangedOutOfRange_ReturnsOutOfRange()
    {
        // Arrange - distance 7 with range 5
        PlaceEntities(new GridPosition(0, 0), new GridPosition(7, 0));

        // Act
        var result = _service.CheckRange(_playerId, _monsterId, 5, RangeType.Ranged);

        // Assert
        result.InRange.Should().BeFalse();
        result.Distance.Should().Be(7);
        result.FailureReason.Should().Be(RangeFailureReason.OutOfRange);
    }

    // ===== CheckAbilityRange Tests =====

    [Test]
    public void CheckAbilityRange_MeleeAbilityAdjacent_ReturnsInRange()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));
        var ability = new AbilityDefinition
        {
            Id = "slash",
            Name = "Slash",
            Range = 1,
            RangeType = RangeType.Melee
        };

        // Act
        var result = _service.CheckAbilityRange(_playerId, _monsterId, ability);

        // Assert
        result.InRange.Should().BeTrue();
    }

    // ===== GetValidTargets Tests =====

    [Test]
    public void GetValidTargets_NoActiveGrid_ReturnsEmpty()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);

        // Act
        var targets = _service.GetValidTargets(_playerId, 1, RangeType.Melee).ToList();

        // Assert
        targets.Should().BeEmpty();
    }

    [Test]
    public void GetValidTargets_MeleeRange_ReturnsOnlyAdjacent()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3));
        var monster2Id = Guid.NewGuid();
        _grid.PlaceEntity(monster2Id, new GridPosition(4, 1), isPlayer: false); // Distance 3

        // Act
        var targets = _service.GetValidTargets(_playerId, 1, RangeType.Melee).ToList();

        // Assert
        targets.Should().ContainSingle().Which.Should().Be(_monsterId);
    }

    [Test]
    public void GetValidTargets_ReachRange_ReturnsDistance1And2()
    {
        // Arrange
        PlaceEntities(new GridPosition(4, 4), new GridPosition(4, 3)); // Distance 1
        var monster2Id = Guid.NewGuid();
        _grid.PlaceEntity(monster2Id, new GridPosition(4, 2), isPlayer: false); // Distance 2

        // Act
        var targets = _service.GetValidTargets(_playerId, 2, RangeType.Reach).ToList();

        // Assert
        targets.Should().HaveCount(2);
        targets.Should().Contain(_monsterId);
        targets.Should().Contain(monster2Id);
    }

    // ===== GetDistance Tests =====

    [Test]
    public void GetDistance_DelegatesToGridService()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.GetDistance(_playerId, _monsterId)).Returns(5);

        // Act
        var distance = _service.GetDistance(_playerId, _monsterId);

        // Assert
        distance.Should().Be(5);
    }

    // ===== AreAdjacent Tests =====

    [Test]
    public void AreAdjacent_DelegatesToGridService()
    {
        // Arrange
        _gridServiceMock.Setup(g => g.AreAdjacent(_playerId, _monsterId)).Returns(true);

        // Act
        var areAdjacent = _service.AreAdjacent(_playerId, _monsterId);

        // Assert
        areAdjacent.Should().BeTrue();
    }

    // ===== GetEffectiveRange Tests =====

    [Test]
    public void GetEffectiveRange_Weapon_ReturnsWeaponEffectiveRange()
    {
        // Arrange
        var weapon = Item.CreateSword();
        weapon.SetRange(10, RangeType.Ranged);

        // Act
        var range = _service.GetEffectiveRange(weapon);

        // Assert
        range.Should().Be(10);
    }

    [Test]
    public void GetEffectiveRange_Ability_ReturnsAbilityEffectiveRange()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "fireball",
            Name = "Fireball",
            Range = 6,
            RangeType = RangeType.Ranged
        };

        // Act
        var range = _service.GetEffectiveRange(ability);

        // Assert
        range.Should().Be(6);
    }
}
