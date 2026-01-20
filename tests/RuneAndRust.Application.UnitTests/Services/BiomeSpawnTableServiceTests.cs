using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for BiomeSpawnTableService.
/// </summary>
[TestFixture]
public class BiomeSpawnTableServiceTests
{
    private SeededRandomService _random = null!;
    private BiomeSpawnTableService _service = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new BiomeSpawnTableService(_random);
    }

    [Test]
    public void GetSpawnTable_WithExistingBiome_ReturnsTable()
    {
        // Act
        var table = _service.GetSpawnTable("stone-corridors");

        // Assert
        table.Should().NotBeNull();
        table!.BiomeId.Should().Be("stone-corridors");
    }

    [Test]
    public void GetSpawnTable_WithNonExistingBiome_ReturnsNull()
    {
        // Act
        var table = _service.GetSpawnTable("nonexistent");

        // Assert
        table.Should().BeNull();
    }

    [Test]
    public void GetValidMonsters_FiltersbyDepth()
    {
        // Act
        var depth0Monsters = _service.GetValidMonsters("stone-corridors", 0);
        var depth2Monsters = _service.GetValidMonsters("stone-corridors", 2);

        // Assert
        depth0Monsters.Should().Contain(e => e.EntityId == "giant-rat");
        depth2Monsters.Should().Contain(e => e.EntityId == "skeleton");
    }

    [Test]
    public void SelectMonster_ReturnsMonsterFromPool()
    {
        // Arrange
        var position = new Position3D(5, 5, -1);

        // Act
        var selected = _service.SelectMonster("stone-corridors", position);

        // Assert
        selected.Should().NotBeNull();
        selected.Should().BeOneOf("giant-rat", "skeleton", "zombie");
    }

    [Test]
    public void GetLootModifiers_WithExistingBiome_ReturnsModifiers()
    {
        // Act
        var modifiers = _service.GetLootModifiers("flooded-depths");

        // Assert
        modifiers.GoldMultiplier.Should().Be(1.5f);
        modifiers.RareChanceBonus.Should().Be(0.1f);
    }

    [Test]
    public void GetLootModifiers_WithNonExistingBiome_ReturnsDefault()
    {
        // Act
        var modifiers = _service.GetLootModifiers("nonexistent");

        // Assert
        modifiers.Should().Be(LootModifiers.Default);
    }

    [Test]
    public void GetExclusiveItems_ReturnsBiomeExclusives()
    {
        // Act
        var items = _service.GetExclusiveItems("fungal-caverns");

        // Assert
        items.Should().Contain("bioluminescent-orb");
    }

    [Test]
    public void GetCraftingMaterials_ReturnsBiomeMaterials()
    {
        // Act
        var materials = _service.GetCraftingMaterials("fungal-caverns");

        // Assert
        materials.Should().Contain("fungal-spore");
        materials.Should().Contain("luminescent-cap");
    }
}
