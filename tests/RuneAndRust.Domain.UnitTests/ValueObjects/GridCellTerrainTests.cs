using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="GridCell"/> terrain properties (v0.5.2a).
/// </summary>
[TestFixture]
public class GridCellTerrainTests
{
    [Test]
    public void Create_DefaultsToNormalTerrain()
    {
        // Arrange & Act
        var cell = GridCell.Create(0, 0);

        // Assert
        cell.TerrainType.Should().Be(TerrainType.Normal);
        cell.TerrainDefinitionId.Should().BeNull();
    }

    [Test]
    public void SetTerrain_SetsTerrainType()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetTerrain(TerrainType.Difficult);

        // Assert
        cell.TerrainType.Should().Be(TerrainType.Difficult);
        cell.TerrainDefinitionId.Should().BeNull();
    }

    [Test]
    public void SetTerrain_ClearsTerrainDefinitionId()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetTerrainDefinition("fire");

        // Act
        cell.SetTerrain(TerrainType.Normal);

        // Assert
        cell.TerrainDefinitionId.Should().BeNull();
    }

    [Test]
    public void SetTerrainDefinition_SetsDefinitionId()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetTerrainDefinition("fire");

        // Assert
        cell.TerrainDefinitionId.Should().Be("fire");
    }

    [Test]
    public void SetTerrainDefinition_NormalizesToLowercase()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetTerrainDefinition("FIRE-PIT");

        // Assert
        cell.TerrainDefinitionId.Should().Be("fire-pit");
    }

    [Test]
    public void IsPassable_ReturnsFalse_ForImpassableTerrain()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetTerrain(TerrainType.Impassable);

        // Assert
        cell.IsPassable.Should().BeFalse();
    }

    [Test]
    public void IsPassable_ReturnsTrue_ForNormalTerrain()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Act
        cell.SetTerrain(TerrainType.Normal);

        // Assert
        cell.IsPassable.Should().BeTrue();
    }

    [Test]
    public void IsPassable_ConsidersBothPassabilityAndTerrainType()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Cell is passable with normal terrain
        cell.IsPassable.Should().BeTrue();

        // Set passable to false
        cell.SetPassable(false);
        cell.IsPassable.Should().BeFalse();

        // Set back to passable but with impassable terrain
        cell.SetPassable(true);
        cell.SetTerrain(TerrainType.Impassable);
        cell.IsPassable.Should().BeFalse();
    }

    [Test]
    public void GetMovementCostMultiplier_ReturnsCorrectValues()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Assert Normal
        cell.SetTerrain(TerrainType.Normal);
        cell.GetMovementCostMultiplier().Should().Be(1.0f);

        // Assert Difficult
        cell.SetTerrain(TerrainType.Difficult);
        cell.GetMovementCostMultiplier().Should().Be(2.0f);

        // Assert Hazardous
        cell.SetTerrain(TerrainType.Hazardous);
        cell.GetMovementCostMultiplier().Should().Be(1.0f);

        // Assert Impassable
        cell.SetTerrain(TerrainType.Impassable);
        cell.GetMovementCostMultiplier().Should().Be(float.MaxValue);
    }

    [Test]
    public void EffectivelyBlocksLOS_ConsidersImpassableTerrain()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);

        // Normal terrain doesn't block LOS
        cell.EffectivelyBlocksLOS.Should().BeFalse();

        // Impassable terrain blocks LOS
        cell.SetTerrain(TerrainType.Impassable);
        cell.EffectivelyBlocksLOS.Should().BeTrue();
    }
}
