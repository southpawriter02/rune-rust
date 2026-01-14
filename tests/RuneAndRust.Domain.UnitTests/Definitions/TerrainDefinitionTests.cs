using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for the <see cref="TerrainDefinition"/> class (v0.5.2a).
/// </summary>
[TestFixture]
public class TerrainDefinitionTests
{
    [Test]
    public void Create_WithValidId_CreatesDefinition()
    {
        // Arrange & Act
        var definition = TerrainDefinition.Create(
            id: "test-terrain",
            name: "Test Terrain",
            type: TerrainType.Normal);

        // Assert
        definition.Id.Should().Be("test-terrain");
        definition.Name.Should().Be("Test Terrain");
        definition.Type.Should().Be(TerrainType.Normal);
        definition.MovementCostMultiplier.Should().Be(1.0f);
        definition.DamageOnEntry.Should().BeNull();
        definition.DamageType.Should().BeNull();
        definition.BlocksLOS.Should().BeFalse();
    }

    [Test]
    public void Create_WithNullOrEmptyId_ThrowsArgumentException()
    {
        // Act
        var actNull = () => TerrainDefinition.Create(null!, "Test", TerrainType.Normal);
        var actEmpty = () => TerrainDefinition.Create("", "Test", TerrainType.Normal);
        var actWhitespace = () => TerrainDefinition.Create("   ", "Test", TerrainType.Normal);

        // Assert
        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
        actWhitespace.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_NormalizesIdToLowercase()
    {
        // Act
        var definition = TerrainDefinition.Create("FIRE-PIT", "Fire Pit", TerrainType.Hazardous);

        // Assert
        definition.Id.Should().Be("fire-pit");
    }

    [Test]
    public void IsPassable_ReturnsFalse_ForImpassableTerrain()
    {
        // Arrange
        var wall = TerrainDefinition.Create("wall", "Wall", TerrainType.Impassable);
        var floor = TerrainDefinition.Create("floor", "Floor", TerrainType.Normal);
        var rubble = TerrainDefinition.Create("rubble", "Rubble", TerrainType.Difficult);
        var fire = TerrainDefinition.Create("fire", "Fire", TerrainType.Hazardous);

        // Assert
        wall.IsPassable.Should().BeFalse();
        floor.IsPassable.Should().BeTrue();
        rubble.IsPassable.Should().BeTrue();
        fire.IsPassable.Should().BeTrue();
    }

    [Test]
    public void DealsDamage_ReturnsTrue_OnlyForHazardousWithDamageExpression()
    {
        // Arrange
        var hazardWithDamage = TerrainDefinition.Create(
            "fire", "Fire", TerrainType.Hazardous,
            damageOnEntry: "1d6", damageType: "fire");

        var hazardWithoutDamage = TerrainDefinition.Create(
            "cold-zone", "Cold Zone", TerrainType.Hazardous);

        var normalTerrain = TerrainDefinition.Create(
            "floor", "Floor", TerrainType.Normal);

        // Assert
        hazardWithDamage.DealsDamage.Should().BeTrue();
        hazardWithoutDamage.DealsDamage.Should().BeFalse();
        normalTerrain.DealsDamage.Should().BeFalse();
    }

    [Test]
    public void Create_WithAllParameters_SetsAllProperties()
    {
        // Arrange & Act
        var definition = TerrainDefinition.Create(
            id: "fire",
            name: "Fire",
            type: TerrainType.Hazardous,
            movementCostMultiplier: 1.5f,
            damageOnEntry: "2d6",
            damageType: "fire",
            blocksLOS: false,
            displayChar: '▲',
            description: "Burning fire");

        // Assert
        definition.Id.Should().Be("fire");
        definition.Name.Should().Be("Fire");
        definition.Type.Should().Be(TerrainType.Hazardous);
        definition.MovementCostMultiplier.Should().Be(1.5f);
        definition.DamageOnEntry.Should().Be("2d6");
        definition.DamageType.Should().Be("fire");
        definition.BlocksLOS.Should().BeFalse();
        definition.DisplayChar.Should().Be('▲');
        definition.Description.Should().Be("Burning fire");
    }
}
