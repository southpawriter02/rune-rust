using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Mocks;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the CoverService.
/// </summary>
[TestFixture]
public class CoverServiceTests
{
    private Mock<ICombatGridService> _mockGridService = null!;
    private Mock<ILineOfSightService> _mockLosService = null!;
    private MockConfigurationProvider _mockConfig = null!;
    private CoverService _service = null!;
    private CombatGrid _grid = null!;

    [SetUp]
    public void Setup()
    {
        _mockGridService = new Mock<ICombatGridService>();
        _mockLosService = new Mock<ILineOfSightService>();
        _mockConfig = new MockConfigurationProvider().WithDefaultCoverDefinitions();

        _grid = CombatGrid.Create(10, 10);
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns(_grid);

        _service = new CoverService(
            _mockGridService.Object,
            _mockLosService.Object,
            _mockConfig,
            NullLogger<CoverService>.Instance);
    }

    // ===== GetCoverBetween Tests =====

    [Test]
    public void GetCoverBetween_WhenNoCover_ReturnsNone()
    {
        // Arrange
        var attacker = new GridPosition(0, 0);
        var target = new GridPosition(5, 0);
        _mockLosService.Setup(l => l.GetLineCells(attacker, target))
            .Returns(new[] { attacker, new GridPosition(1, 0), new GridPosition(2, 0), target });

        // Act
        var result = _service.GetCoverBetween(attacker, target);

        // Assert
        result.CoverType.Should().Be(CoverType.None);
        result.DefenseBonus.Should().Be(0);
        result.IsBlocked.Should().BeFalse();
    }

    [Test]
    public void GetCoverBetween_WithPartialCover_ReturnsPartial()
    {
        // Arrange
        var attacker = new GridPosition(0, 0);
        var coverPos = new GridPosition(3, 0);
        var target = new GridPosition(5, 0);

        var crateDef = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        _grid.AddCover(CoverObject.Create(crateDef, coverPos));

        _mockLosService.Setup(l => l.GetLineCells(attacker, target))
            .Returns(new[] { attacker, new GridPosition(1, 0), new GridPosition(2, 0), coverPos, new GridPosition(4, 0), target });

        // Act
        var result = _service.GetCoverBetween(attacker, target);

        // Assert
        result.CoverType.Should().Be(CoverType.Partial);
        result.DefenseBonus.Should().Be(2);
        result.IsBlocked.Should().BeFalse();
        result.CoverObject.Should().NotBeNull();
    }

    [Test]
    public void GetCoverBetween_WithFullCover_ReturnsFullBlocked()
    {
        // Arrange
        var attacker = new GridPosition(0, 0);
        var coverPos = new GridPosition(3, 0);
        var target = new GridPosition(5, 0);

        var pillarDef = _mockConfig.GetCoverDefinitionById("stone-pillar")!;
        _grid.AddCover(CoverObject.Create(pillarDef, coverPos));

        _mockLosService.Setup(l => l.GetLineCells(attacker, target))
            .Returns(new[] { attacker, new GridPosition(1, 0), new GridPosition(2, 0), coverPos, new GridPosition(4, 0), target });

        // Act
        var result = _service.GetCoverBetween(attacker, target);

        // Assert
        result.CoverType.Should().Be(CoverType.Full);
        result.IsBlocked.Should().BeTrue();
        result.CoverObject.Should().NotBeNull();
    }

    [Test]
    public void GetCoverBetween_WhenNoActiveGrid_ReturnsNone()
    {
        // Arrange
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);

        // Act
        var result = _service.GetCoverBetween(new GridPosition(0, 0), new GridPosition(5, 0));

        // Assert
        result.CoverType.Should().Be(CoverType.None);
    }

    // ===== GetDefenseBonus Tests =====

    [Test]
    public void GetDefenseBonus_WhenNoCover_ReturnsZero()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _grid.PlaceEntity(attackerId, new GridPosition(0, 0), false);
        _grid.PlaceEntity(targetId, new GridPosition(5, 0), false);

        _mockLosService.Setup(l => l.GetLineCells(It.IsAny<GridPosition>(), It.IsAny<GridPosition>()))
            .Returns(Array.Empty<GridPosition>());

        // Act
        var result = _service.GetDefenseBonus(attackerId, targetId);

        // Assert
        result.Should().Be(0);
    }

    // ===== HasFullCover Tests =====

    [Test]
    public void HasFullCover_WhenFullCoverBetween_ReturnsTrue()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _grid.PlaceEntity(attackerId, new GridPosition(0, 0), false);
        _grid.PlaceEntity(targetId, new GridPosition(5, 0), false);

        var pillarDef = _mockConfig.GetCoverDefinitionById("stone-pillar")!;
        _grid.AddCover(CoverObject.Create(pillarDef, new GridPosition(3, 0)));

        _mockLosService.Setup(l => l.GetLineCells(It.IsAny<GridPosition>(), It.IsAny<GridPosition>()))
            .Returns(new[] { new GridPosition(0, 0), new GridPosition(3, 0), new GridPosition(5, 0) });

        // Act
        var result = _service.HasFullCover(attackerId, targetId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasFullCover_WhenNoCover_ReturnsFalse()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _grid.PlaceEntity(attackerId, new GridPosition(0, 0), false);
        _grid.PlaceEntity(targetId, new GridPosition(5, 0), false);

        _mockLosService.Setup(l => l.GetLineCells(It.IsAny<GridPosition>(), It.IsAny<GridPosition>()))
            .Returns(Array.Empty<GridPosition>());

        // Act
        var result = _service.HasFullCover(attackerId, targetId);

        // Assert
        result.Should().BeFalse();
    }

    // ===== AddCover Tests =====

    [Test]
    public void AddCover_WithValidDefinition_AddsCoverToGrid()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        var pos = new GridPosition(3, 3);

        // Act
        var result = _service.AddCover(def, pos);

        // Assert
        result.Should().BeTrue();
        _grid.HasCover(pos).Should().BeTrue();
    }

    [Test]
    public void AddCover_WhenNoGrid_ReturnsFalse()
    {
        // Arrange
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns((CombatGrid?)null);
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;

        // Act
        var result = _service.AddCover(def, new GridPosition(3, 3));

        // Assert
        result.Should().BeFalse();
    }

    // ===== RemoveCover Tests =====

    [Test]
    public void RemoveCover_WhenCoverExists_ReturnsTrue()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        var pos = new GridPosition(3, 3);
        _service.AddCover(def, pos);

        // Act
        var result = _service.RemoveCover(pos);

        // Assert
        result.Should().BeTrue();
        _grid.HasCover(pos).Should().BeFalse();
    }

    // ===== DamageCover Tests =====

    [Test]
    public void DamageCover_WhenDestructible_DealsDamage()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        var pos = new GridPosition(3, 3);
        _service.AddCover(def, pos);

        // Act
        var result = _service.DamageCover(pos, 4);

        // Assert
        result.DamageDealt.Should().BeTrue();
        result.DamageAmount.Should().Be(4);
        result.RemainingHp.Should().Be(6);
        result.Destroyed.Should().BeFalse();
    }

    [Test]
    public void DamageCover_WhenKillingBlow_DestroysAndRemoves()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        var pos = new GridPosition(3, 3);
        _service.AddCover(def, pos);

        // Act
        var result = _service.DamageCover(pos, 15);

        // Assert
        result.Destroyed.Should().BeTrue();
        result.RemainingHp.Should().Be(0);
        _grid.HasCover(pos).Should().BeFalse();
    }

    [Test]
    public void DamageCover_WhenNotDestructible_ReturnsFalse()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("stone-pillar")!;
        var pos = new GridPosition(3, 3);
        _service.AddCover(def, pos);

        // Act
        var result = _service.DamageCover(pos, 100);

        // Assert
        result.DamageDealt.Should().BeFalse();
    }

    [Test]
    public void DamageCover_WhenNoCover_ReturnsNone()
    {
        // Arrange & Act
        var result = _service.DamageCover(new GridPosition(5, 5), 10);

        // Assert
        result.DamageDealt.Should().BeFalse();
    }

    // ===== GetCover Tests =====

    [Test]
    public void GetCover_WhenCoverExists_ReturnsCover()
    {
        // Arrange
        var def = _mockConfig.GetCoverDefinitionById("wooden-crate")!;
        var pos = new GridPosition(3, 3);
        _service.AddCover(def, pos);

        // Act
        var result = _service.GetCover(pos);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Wooden Crate");
    }

    // ===== GetAllCoverDefinitions Tests =====

    [Test]
    public void GetAllCoverDefinitions_ReturnsLoadedDefinitions()
    {
        // Act
        var result = _service.GetAllCoverDefinitions().ToList();

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(d => d.Id == "wooden-crate");
        result.Should().Contain(d => d.Id == "stone-pillar");
    }
}
