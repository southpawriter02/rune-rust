using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for BiomeSpawnTable entity.
/// </summary>
[TestFixture]
public class BiomeSpawnTableTests
{
    [Test]
    public void Create_WithValidParameters_CreatesTable()
    {
        // Act
        var table = BiomeSpawnTable.Create("fungal-caverns");

        // Assert
        table.BiomeId.Should().Be("fungal-caverns");
        table.MonsterPool.Should().BeEmpty();
        table.ItemPool.Should().BeEmpty();
    }

    [Test]
    public void Create_WithPools_StoresPools()
    {
        // Arrange
        var monsters = new[] { SpawnEntry.Create("goblin") };
        var items = new[] { SpawnEntry.Create("potion") };

        // Act
        var table = BiomeSpawnTable.Create("cave", monsterPool: monsters, itemPool: items);

        // Assert
        table.MonsterPool.Should().HaveCount(1);
        table.ItemPool.Should().HaveCount(1);
    }

    [Test]
    public void GetValidMonsters_FiltersbyDepth()
    {
        // Arrange
        var monsters = new[]
        {
            SpawnEntry.Create("rat", minDepth: 0, maxDepth: 2),
            SpawnEntry.Create("goblin", minDepth: 2, maxDepth: 5),
            SpawnEntry.Create("troll", minDepth: 5)
        };
        var table = BiomeSpawnTable.Create("cave", monsterPool: monsters);

        // Act
        var depth0 = table.GetValidMonsters(0);
        var depth3 = table.GetValidMonsters(3);
        var depth6 = table.GetValidMonsters(6);

        // Assert
        depth0.Should().HaveCount(1);
        depth0[0].EntityId.Should().Be("rat");
        
        depth3.Should().HaveCount(1);
        depth3[0].EntityId.Should().Be("goblin");
        
        depth6.Should().HaveCount(1);
        depth6[0].EntityId.Should().Be("troll");
    }

    [Test]
    public void Create_WithLootModifiers_StoresModifiers()
    {
        // Arrange
        var modifiers = LootModifiers.Create(goldMultiplier: 1.5f);

        // Act
        var table = BiomeSpawnTable.Create("cave", lootModifiers: modifiers);

        // Assert
        table.LootModifiers.GoldMultiplier.Should().Be(1.5f);
    }

    [Test]
    public void Create_WithExclusiveItems_StoresItems()
    {
        // Act
        var table = BiomeSpawnTable.Create("cave", exclusiveItems: new[] { "rare-gem" });

        // Assert
        table.ExclusiveItems.Should().Contain("rare-gem");
    }
}
