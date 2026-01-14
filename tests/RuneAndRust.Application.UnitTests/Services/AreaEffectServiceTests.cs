using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="AreaEffectService"/>.
/// </summary>
[TestFixture]
public class AreaEffectServiceTests
{
    private Mock<ICombatGridService> _mockGridService = null!;
    private Mock<ILineOfSightService> _mockLosService = null!;
    private Mock<ILogger<AreaEffectService>> _mockLogger = null!;
    private AreaEffectService _service = null!;
    private CombatGrid _grid = null!;

    [SetUp]
    public void Setup()
    {
        _mockGridService = new Mock<ICombatGridService>();
        _mockLosService = new Mock<ILineOfSightService>();
        _mockLogger = new Mock<ILogger<AreaEffectService>>();

        _grid = CombatGrid.Create(8, 8);
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns(_grid);

        _service = new AreaEffectService(_mockGridService.Object, _mockLosService.Object, _mockLogger.Object);
    }

    #region GetCircleCells Tests

    [Test]
    public void GetCircleCells_Radius0_ReturnsOnlyCenter()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetCircleCells(center, 0).ToList();

        // Assert
        cells.Should().HaveCount(1);
        cells.Should().Contain(center);
    }

    [Test]
    public void GetCircleCells_Radius1_Returns9Cells()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetCircleCells(center, 1).ToList();

        // Assert - 3x3 grid = 9 cells (Chebyshev distance includes diagonals)
        cells.Should().HaveCount(9);
        cells.Should().Contain(center);
        cells.Should().Contain(new GridPosition(3, 3)); // diagonal
        cells.Should().Contain(new GridPosition(5, 5)); // diagonal
    }

    [Test]
    public void GetCircleCells_Radius2_Returns25Cells()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetCircleCells(center, 2).ToList();

        // Assert - 5x5 grid = 25 cells for Chebyshev distance
        cells.Should().HaveCount(25);
    }

    [Test]
    public void GetCircleCells_CenterNearEdge_IncludesOutOfGridCells()
    {
        // Arrange - center near origin, some cells will be negative
        var center = new GridPosition(1, 1);

        // Act
        var cells = _service.GetCircleCells(center, 2).ToList();

        // Assert - includes all 25 cells regardless of grid bounds
        cells.Should().HaveCount(25);
        cells.Should().Contain(new GridPosition(-1, -1)); // out of bounds but included
    }

    #endregion

    #region GetConeCells Tests

    [Test]
    public void GetConeCells_NorthDirection_ReturnsCellsToNorth()
    {
        // Arrange
        var origin = new GridPosition(4, 4);

        // Act
        var cells = _service.GetConeCells(origin, FacingDirection.North, 2, 90).ToList();

        // Assert - should have cells with y < 4
        cells.Should().NotBeEmpty();
        cells.All(c => c.Y < origin.Y).Should().BeTrue();
    }

    [Test]
    public void GetConeCells_EastDirection_ReturnsCellsToEast()
    {
        // Arrange
        var origin = new GridPosition(4, 4);

        // Act
        var cells = _service.GetConeCells(origin, FacingDirection.East, 2, 90).ToList();

        // Assert - should have cells with x > 4
        cells.Should().NotBeEmpty();
        cells.All(c => c.X > origin.X).Should().BeTrue();
    }

    [Test]
    public void GetConeCells_Length1_ReturnsMinimalCells()
    {
        // Arrange
        var origin = new GridPosition(4, 4);

        // Act
        var cells = _service.GetConeCells(origin, FacingDirection.North, 1, 90).ToList();

        // Assert - short cone should have few cells
        cells.Should().NotBeEmpty();
        cells.Count.Should().BeLessOrEqualTo(5);
    }

    [Test]
    public void GetConeCells_NarrowAngle_ReturnsFewerCells()
    {
        // Arrange
        var origin = new GridPosition(4, 4);

        // Act
        var narrowCone = _service.GetConeCells(origin, FacingDirection.North, 3, 30).ToList();
        var wideCone = _service.GetConeCells(origin, FacingDirection.North, 3, 120).ToList();

        // Assert
        narrowCone.Count.Should().BeLessOrEqualTo(wideCone.Count);
    }

    #endregion

    #region GetLineCells Tests

    [Test]
    public void GetLineCells_SameOriginAndTarget_ReturnsOriginCell()
    {
        // Arrange
        var origin = new GridPosition(4, 4);
        _mockLosService.Setup(l => l.GetLineCells(origin, origin))
            .Returns(new[] { origin });

        // Act
        var cells = _service.GetLineCells(origin, origin, 1).ToList();

        // Assert
        cells.Should().HaveCount(1);
        cells.Should().Contain(origin);
    }

    [Test]
    public void GetLineCells_HorizontalLine_ReturnsCorrectCells()
    {
        // Arrange
        var origin = new GridPosition(2, 4);
        var target = new GridPosition(6, 4);
        var lineCells = new[]
        {
            new GridPosition(2, 4), new GridPosition(3, 4),
            new GridPosition(4, 4), new GridPosition(5, 4), new GridPosition(6, 4)
        };
        _mockLosService.Setup(l => l.GetLineCells(origin, target)).Returns(lineCells);

        // Act
        var cells = _service.GetLineCells(origin, target, 1).ToList();

        // Assert
        cells.Should().HaveCount(5);
    }

    [Test]
    public void GetLineCells_WiderLine_ReturnsMoreCells()
    {
        // Arrange
        var origin = new GridPosition(2, 4);
        var target = new GridPosition(6, 4);
        var lineCells = new[]
        {
            new GridPosition(2, 4), new GridPosition(3, 4),
            new GridPosition(4, 4), new GridPosition(5, 4), new GridPosition(6, 4)
        };
        _mockLosService.Setup(l => l.GetLineCells(origin, target)).Returns(lineCells);

        // Act
        var narrowCells = _service.GetLineCells(origin, target, 1).ToList();
        var wideCells = _service.GetLineCells(origin, target, 3).ToList();

        // Assert
        wideCells.Count.Should().BeGreaterThan(narrowCells.Count);
    }

    [Test]
    public void GetLineCells_VerticalLine_ReturnsCorrectCells()
    {
        // Arrange
        var origin = new GridPosition(4, 2);
        var target = new GridPosition(4, 6);
        var lineCells = new[]
        {
            new GridPosition(4, 2), new GridPosition(4, 3),
            new GridPosition(4, 4), new GridPosition(4, 5), new GridPosition(4, 6)
        };
        _mockLosService.Setup(l => l.GetLineCells(origin, target)).Returns(lineCells);

        // Act
        var cells = _service.GetLineCells(origin, target, 1).ToList();

        // Assert
        cells.Should().HaveCount(5);
    }

    #endregion

    #region GetSquareCells Tests

    [Test]
    public void GetSquareCells_Size1_ReturnsSingleCell()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetSquareCells(center, 1).ToList();

        // Assert
        cells.Should().HaveCount(1);
        cells.Should().Contain(center);
    }

    [Test]
    public void GetSquareCells_Size3_Returns9Cells()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetSquareCells(center, 3).ToList();

        // Assert - 3x3 square = 9 cells
        cells.Should().HaveCount(9);
    }

    [Test]
    public void GetSquareCells_Size5_Returns25Cells()
    {
        // Arrange
        var center = new GridPosition(4, 4);

        // Act
        var cells = _service.GetSquareCells(center, 5).ToList();

        // Assert - 5x5 square = 25 cells
        cells.Should().HaveCount(25);
    }

    #endregion

    #region GetAffectedCells Tests

    [Test]
    public void GetAffectedCells_CircleShape_DelegatesToGetCircleCells()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var target = new GridPosition(4, 4);

        // Act
        var cells = _service.GetAffectedCells(effect, origin, target).ToList();

        // Assert - 5x5 circle = 25 cells, filtered by grid bounds (8x8)
        cells.Should().NotBeEmpty();
    }

    [Test]
    public void GetAffectedCells_ConeShape_DelegatesToGetConeCells()
    {
        // Arrange
        var effect = AreaEffect.Cone(3, 90);
        var origin = new GridPosition(4, 4);

        // Act
        var cells = _service.GetAffectedCells(effect, origin, direction: FacingDirection.North).ToList();

        // Assert
        cells.Should().NotBeEmpty();
        cells.All(c => c.Y < origin.Y).Should().BeTrue();
    }

    [Test]
    public void GetAffectedCells_LineShape_DelegatesToGetLineCells()
    {
        // Arrange
        var effect = AreaEffect.Line(4);
        var origin = new GridPosition(2, 4);
        var target = new GridPosition(6, 4);
        var lineCells = new[]
        {
            new GridPosition(2, 4), new GridPosition(3, 4),
            new GridPosition(4, 4), new GridPosition(5, 4), new GridPosition(6, 4)
        };
        _mockLosService.Setup(l => l.GetLineCells(origin, target)).Returns(lineCells);

        // Act
        var cells = _service.GetAffectedCells(effect, origin, target).ToList();

        // Assert
        cells.Should().NotBeEmpty();
    }

    [Test]
    public void GetAffectedCells_SquareShape_DelegatesToGetSquareCells()
    {
        // Arrange
        var effect = AreaEffect.Square(3);
        var origin = new GridPosition(4, 4);
        var target = new GridPosition(4, 4);

        // Act
        var cells = _service.GetAffectedCells(effect, origin, target).ToList();

        // Assert
        cells.Should().NotBeEmpty();
    }

    #endregion

    #region GetAffectedEntities Tests

    [Test]
    public void GetAffectedEntities_EmptyGrid_ReturnsNoEntities()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);

        // Act
        var entities = _service.GetAffectedEntities(effect, origin, origin).ToList();

        // Assert
        entities.Should().BeEmpty();
    }

    [Test]
    public void GetAffectedEntities_EntityInArea_ReturnsEntity()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var enemyId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        // Place player at origin, enemy nearby
        _grid.PlaceEntity(playerId, origin, isPlayer: true);
        _grid.PlaceEntity(enemyId, new GridPosition(5, 4), isPlayer: false);

        // Act
        var entities = _service.GetAffectedEntities(effect, origin, origin, casterId: playerId).ToList();

        // Assert
        entities.Should().Contain(enemyId);
    }

    [Test]
    public void GetAffectedEntities_CasterNotIncluded_ExcludesCaster()
    {
        // Arrange
        var effect = AreaEffect.Circle(2, includesCaster: false);
        var origin = new GridPosition(4, 4);
        var playerId = Guid.NewGuid();
        _grid.PlaceEntity(playerId, origin, isPlayer: true);

        // Act
        var entities = _service.GetAffectedEntities(effect, origin, origin, casterId: playerId).ToList();

        // Assert
        entities.Should().NotContain(playerId);
    }

    [Test]
    public void GetAffectedEntities_AlliesNotAffected_ExcludesAllies()
    {
        // Arrange - AffectsAllies = false by default
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var playerId = Guid.NewGuid();
        var allyId = Guid.NewGuid();

        _grid.PlaceEntity(playerId, origin, isPlayer: true);
        _grid.PlaceEntity(allyId, new GridPosition(5, 4), isPlayer: true); // Another player = ally

        // Act
        var entities = _service.GetAffectedEntities(effect, origin, origin, casterId: playerId).ToList();

        // Assert
        entities.Should().NotContain(allyId);
    }

    #endregion

    #region GetPreview Tests

    [Test]
    public void GetPreview_EmptyArea_ReturnsEmptyPreview()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);

        // Act
        var preview = _service.GetPreview(effect, origin, origin);

        // Assert
        preview.AffectedEnemies.Should().BeEmpty();
        preview.AffectedAllies.Should().BeEmpty();
        preview.AffectedCells.Should().NotBeEmpty();
    }

    [Test]
    public void GetPreview_WithEnemies_IncludesEnemyInfo()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var enemyId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        _grid.PlaceEntity(playerId, origin, isPlayer: true);
        _grid.PlaceEntity(enemyId, new GridPosition(5, 4), isPlayer: false);

        // Act
        var preview = _service.GetPreview(effect, origin, origin, casterId: playerId);

        // Assert
        preview.AffectedEnemies.Should().HaveCount(1);
        preview.AffectedEnemies[0].Id.Should().Be(enemyId);
    }

    [Test]
    public void GetPreview_DescriptionIncludesCellCount()
    {
        // Arrange
        var effect = AreaEffect.Circle(1);
        var origin = new GridPosition(4, 4);

        // Act
        var preview = _service.GetPreview(effect, origin, origin);

        // Assert - Circle radius 1 = 9 cells
        preview.Description.Should().Contain("9 cells");
    }

    #endregion

    #region ValidateTarget Tests

    [Test]
    public void ValidateTarget_InRange_ReturnsSuccess()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var target = new GridPosition(5, 5);
        _mockLosService.Setup(l => l.HasLineOfSight(origin, target))
            .Returns(new LineOfSightResult(true, origin, target, null, "Clear"));

        // Act
        var result = _service.ValidateTarget(effect, origin, target, 6);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void ValidateTarget_OutOfRange_ReturnsOutOfRange()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(0, 0);
        var target = new GridPosition(7, 7);

        // Act
        var result = _service.ValidateTarget(effect, origin, target, 5);

        // Assert
        result.IsValid.Should().BeFalse();
        result.InRange.Should().BeFalse();
    }

    [Test]
    public void ValidateTarget_NoLineOfSight_ReturnsNoLOS()
    {
        // Arrange
        var effect = AreaEffect.Circle(2);
        var origin = new GridPosition(4, 4);
        var target = new GridPosition(5, 5);
        _mockLosService.Setup(l => l.HasLineOfSight(origin, target))
            .Returns(new LineOfSightResult(false, origin, target, new GridPosition(4, 5), "Blocked"));

        // Act
        var result = _service.ValidateTarget(effect, origin, target, 6);

        // Assert
        result.IsValid.Should().BeFalse();
        result.HasLineOfSight.Should().BeFalse();
    }

    #endregion

    #region AreaEffect Value Object Tests

    [Test]
    public void AreaEffect_Circle_CreatesCorrectShape()
    {
        // Act
        var effect = AreaEffect.Circle(3);

        // Assert
        effect.Shape.Should().Be(AreaEffectShape.Circle);
        effect.Radius.Should().Be(3);
        effect.AffectsEnemies.Should().BeTrue();
        effect.AffectsAllies.Should().BeFalse();
    }

    [Test]
    public void AreaEffect_Cone_CreatesCorrectShape()
    {
        // Act
        var effect = AreaEffect.Cone(4, 90);

        // Assert
        effect.Shape.Should().Be(AreaEffectShape.Cone);
        effect.Length.Should().Be(4);
        effect.Width.Should().Be(90);
    }

    [Test]
    public void AreaEffect_Line_CreatesCorrectShape()
    {
        // Act
        var effect = AreaEffect.Line(6, 2);

        // Assert
        effect.Shape.Should().Be(AreaEffectShape.Line);
        effect.Length.Should().Be(6);
        effect.Width.Should().Be(2);
    }

    [Test]
    public void AreaEffect_Square_CreatesCorrectShape()
    {
        // Act
        var effect = AreaEffect.Square(5);

        // Assert
        effect.Shape.Should().Be(AreaEffectShape.Square);
        effect.Radius.Should().Be(2); // 5 / 2 = 2
    }

    #endregion
}
