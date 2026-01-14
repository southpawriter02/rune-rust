using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for CombatGrid cover methods.
/// </summary>
[TestFixture]
public class CombatGridCoverTests
{
    private CombatGrid _grid = null!;
    private CoverDefinition _crateDef = null!;
    private CoverDefinition _pillarDef = null!;

    [SetUp]
    public void Setup()
    {
        _grid = CombatGrid.Create(8, 8);
        _crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, isDestructible: true, maxHitPoints: 10);
        _pillarDef = CoverDefinition.Create("pillar", "Pillar", CoverType.Full, blocksLOS: true);
    }

    [Test]
    public void AddCover_WithValidPosition_ReturnsTrue()
    {
        // Arrange
        var cover = CoverObject.Create(_crateDef, new GridPosition(3, 3));

        // Act
        var result = _grid.AddCover(cover);

        // Assert
        result.Should().BeTrue();
        _grid.CoverObjects.Should().ContainKey(new GridPosition(3, 3));
    }

    [Test]
    public void AddCover_WithOutOfBoundsPosition_ReturnsFalse()
    {
        // Arrange
        var cover = CoverObject.Create(_crateDef, new GridPosition(20, 20));

        // Act
        var result = _grid.AddCover(cover);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void AddCover_WhenPositionOccupied_ReturnsFalse()
    {
        // Arrange
        var pos = new GridPosition(4, 4);
        var cover1 = CoverObject.Create(_crateDef, pos);
        var cover2 = CoverObject.Create(_pillarDef, pos);
        _grid.AddCover(cover1);

        // Act
        var result = _grid.AddCover(cover2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RemoveCover_WhenCoverExists_ReturnsTrue()
    {
        // Arrange
        var pos = new GridPosition(3, 3);
        _grid.AddCover(CoverObject.Create(_crateDef, pos));

        // Act
        var result = _grid.RemoveCover(pos);

        // Assert
        result.Should().BeTrue();
        _grid.HasCover(pos).Should().BeFalse();
    }

    [Test]
    public void RemoveCover_WhenNoCover_ReturnsFalse()
    {
        // Arrange & Act
        var result = _grid.RemoveCover(new GridPosition(5, 5));

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetCover_WhenCoverExists_ReturnsCover()
    {
        // Arrange
        var pos = new GridPosition(3, 3);
        var cover = CoverObject.Create(_crateDef, pos);
        _grid.AddCover(cover);

        // Act
        var result = _grid.GetCover(pos);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Crate");
    }

    [Test]
    public void GetCover_WhenNoCover_ReturnsNull()
    {
        // Arrange & Act
        var result = _grid.GetCover(new GridPosition(5, 5));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void HasCover_WhenCoverExists_ReturnsTrue()
    {
        // Arrange
        var pos = new GridPosition(3, 3);
        _grid.AddCover(CoverObject.Create(_crateDef, pos));

        // Assert
        _grid.HasCover(pos).Should().BeTrue();
    }

    [Test]
    public void HasCover_WhenNoCover_ReturnsFalse()
    {
        // Assert
        _grid.HasCover(new GridPosition(5, 5)).Should().BeFalse();
    }

    [Test]
    public void ClearCover_RemovesAllCover()
    {
        // Arrange
        _grid.AddCover(CoverObject.Create(_crateDef, new GridPosition(1, 1)));
        _grid.AddCover(CoverObject.Create(_pillarDef, new GridPosition(2, 2)));
        _grid.AddCover(CoverObject.Create(_crateDef, new GridPosition(3, 3)));

        // Act
        _grid.ClearCover();

        // Assert
        _grid.CoverObjects.Should().BeEmpty();
    }

    [Test]
    public void CoverObjects_ReturnsReadOnlyDictionary()
    {
        // Arrange
        _grid.AddCover(CoverObject.Create(_crateDef, new GridPosition(1, 1)));
        _grid.AddCover(CoverObject.Create(_pillarDef, new GridPosition(2, 2)));

        // Assert
        _grid.CoverObjects.Should().HaveCount(2);
        _grid.CoverObjects.Should().ContainKey(new GridPosition(1, 1));
        _grid.CoverObjects.Should().ContainKey(new GridPosition(2, 2));
    }
}
