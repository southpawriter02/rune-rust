namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Enums;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Tests for <see cref="GridCellViewModel"/>.
/// </summary>
[TestFixture]
public class GridCellViewModelTests
{
    [Test]
    public void Constructor_WithPositionAndCellType_SetsCellType()
    {
        // Arrange
        var position = new GridPosition(3, 2);

        // Act
        var vm = new GridCellViewModel(position, GridCellType.Wall);

        // Assert
        vm.Position.Should().Be(position);
        vm.CellType.Should().Be(GridCellType.Wall);
        vm.IsWalkable.Should().BeFalse();
    }

    [Test]
    public void Constructor_WithFloorCellType_IsWalkable()
    {
        // Arrange
        var position = new GridPosition(0, 0);

        // Act
        var vm = new GridCellViewModel(position, GridCellType.Floor);

        // Assert
        vm.CellType.Should().Be(GridCellType.Floor);
        vm.IsWalkable.Should().BeTrue();
    }

    [Test]
    public void CoordinateLabel_FormatsCorrectly()
    {
        // Arrange - Position (3, 2) is column 4 (1-indexed), row C
        var vm = new GridCellViewModel(new GridPosition(3, 2), GridCellType.Floor);

        // Act & Assert
        vm.CoordinateLabel.Should().Be("C4");
    }

    [Test]
    public void Tooltip_IncludesCoordinateAndType()
    {
        // Arrange
        var vm = new GridCellViewModel(new GridPosition(0, 0), GridCellType.Water);

        // Act & Assert
        vm.Tooltip.Should().Contain("A1").And.Contain("Water");
    }

    [Test]
    public void IsHighlighted_DefaultsFalse()
    {
        // Arrange & Act
        var vm = new GridCellViewModel(new GridPosition(0, 0), GridCellType.Floor);

        // Assert
        vm.IsHighlighted.Should().BeFalse();
        vm.HighlightType.Should().Be(RuneAndRust.Domain.Enums.HighlightType.Movement);
    }

    [Test]
    public void HasEntity_DefaultsFalse()
    {
        // Arrange & Act
        var vm = new GridCellViewModel(new GridPosition(0, 0), GridCellType.Floor);

        // Assert
        vm.HasEntity.Should().BeFalse();
    }
}
