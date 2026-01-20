using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for GridCell line of sight properties (v0.5.1c).
/// </summary>
[TestFixture]
public class GridCellBlocksLosTests
{
    [Test]
    public void BlocksLOS_Default_IsFalse()
    {
        // Arrange & Act
        var cell = GridCell.Create(0, 0);

        // Assert
        cell.BlocksLOS.Should().BeFalse();
    }

    [Test]
    public void SetBlocksLOS_True_SetsProperty()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetBlocksLOS(true);

        // Assert
        cell.BlocksLOS.Should().BeTrue();
    }

    [Test]
    public void EffectivelyBlocksLOS_WhenPassable_ReturnsFalse()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Assert
        cell.EffectivelyBlocksLOS.Should().BeFalse();
    }

    [Test]
    public void EffectivelyBlocksLOS_WhenImpassable_ReturnsTrue()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetPassable(false);

        // Assert
        cell.EffectivelyBlocksLOS.Should().BeTrue();
    }

    [Test]
    public void EffectivelyBlocksLOS_WhenExplicitlyBlocking_ReturnsTrue()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetBlocksLOS(true);

        // Assert - passable but still blocks LOS
        cell.IsPassable.Should().BeTrue();
        cell.EffectivelyBlocksLOS.Should().BeTrue();
    }

    [Test]
    public void EffectivelyBlocksLOS_WhenOccupied_ReturnsFalse()
    {
        // Arrange - occupied cells don't block LOS
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);

        // Assert
        cell.IsOccupied.Should().BeTrue();
        cell.EffectivelyBlocksLOS.Should().BeFalse();
    }
}
