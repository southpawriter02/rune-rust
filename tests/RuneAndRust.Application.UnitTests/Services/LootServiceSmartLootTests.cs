using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="LootService"/> smart loot integration (v0.16.3d).
/// </summary>
/// <remarks>
/// Tests validate the GenerateLoot(Monster, Player) method and its interaction
/// with ISmartLootService for class-appropriate loot bias.
/// </remarks>
[TestFixture]
public class LootServiceSmartLootTests
{
    #region Test Infrastructure

    private Mock<IGameConfigurationProvider> _configProviderMock = null!;
    private Mock<ILogger<LootService>> _loggerMock = null!;
    private Mock<ISmartLootService> _smartLootServiceMock = null!;

    private static readonly LootEntry TestSword = LootEntry.Create("iron-sword", "swords");
    private static readonly LootEntry TestShield = LootEntry.Create("wooden-shield", "shields");
    private static readonly LootEntry TestPotion = LootEntry.Create("health-potion", "consumables");

    [SetUp]
    public void SetUp()
    {
        _configProviderMock = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<LootService>>();
        _smartLootServiceMock = new Mock<ISmartLootService>();
    }

    private LootService CreateService(ISmartLootService? smartLootService = null)
    {
        return new LootService(
            _configProviderMock.Object,
            _loggerMock.Object,
            eventLogger: null,
            scrollProvider: null,
            recipeProvider: null,
            recipeService: null,
            smartLootService: smartLootService);
    }

    private static MonsterDefinition CreateMonsterWithLoot(params LootEntry[] entries)
    {
        var lootTable = LootTable.Create(entries.ToList());
        return MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "A small green creature",
            baseHealth: 15,
            baseAttack: 5,
            baseDefense: 2,
            experienceValue: 10,
            lootTable: lootTable);
    }

    private static Monster CreateMonster(string? monsterDefId = "goblin")
    {
        return new Monster(
            name: "Goblin Scout",
            description: "A small green scout",
            maxHealth: 15,
            stats: new Stats(15, 5, 2),
            initiativeModifier: 0,
            monsterDefinitionId: monsterDefId);
    }

    #endregion

    #region Null Input Tests

    /// <summary>
    /// Verifies that null monster returns empty loot.
    /// </summary>
    [Test]
    public void GenerateLoot_NullMonster_ReturnsEmpty()
    {
        // Arrange
        var service = CreateService(_smartLootServiceMock.Object);
        var player = new Player("TestHero");

        // Act
        var loot = service.GenerateLoot(null!, player);

        // Assert
        loot.IsEmpty.Should().BeTrue();
        _smartLootServiceMock.Verify(x => x.SelectItem(It.IsAny<SmartLootContext>()), Times.Never);
    }

    /// <summary>
    /// Verifies that null player falls back to basic generation.
    /// </summary>
    [Test]
    public void GenerateLoot_NullPlayer_FallsBackToBasicGeneration()
    {
        // Arrange
        var monster = CreateMonster();
        var definition = CreateMonsterWithLoot(TestSword);
        _configProviderMock.Setup(x => x.GetMonsterById("goblin")).Returns(definition);
        
        var service = CreateService(_smartLootServiceMock.Object);

        // Act
        var loot = service.GenerateLoot(monster, null!);

        // Assert
        // Basic generation should be used, not smart loot
        _smartLootServiceMock.Verify(x => x.SelectItem(It.IsAny<SmartLootContext>()), Times.Never);
    }

    #endregion

    #region Smart Loot Service Not Available Tests

    /// <summary>
    /// Verifies that when ISmartLootService is null, basic generation is used.
    /// </summary>
    [Test]
    public void GenerateLoot_NoSmartLootService_UsesBasicGeneration()
    {
        // Arrange
        var monster = CreateMonster();
        var definition = CreateMonsterWithLoot(TestSword, TestShield);
        _configProviderMock.Setup(x => x.GetMonsterById("goblin")).Returns(definition);
        
        var service = CreateService(smartLootService: null);
        var player = new Player("TestHero");

        // Act
        var loot = service.GenerateLoot(monster, player);

        // Assert
        // Should use basic generation path - smart loot metadata defaults
        loot.WasClassAppropriate.Should().BeFalse();
        loot.BiasRoll.Should().Be(-1);
    }

    #endregion

    #region No Monster Definition Tests

    /// <summary>
    /// Verifies that monster without definition ID returns empty.
    /// </summary>
    [Test]
    public void GenerateLoot_MonsterWithNoDefinitionId_ReturnsEmpty()
    {
        // Arrange
        var monster = new Monster(
            name: "Unknown",
            description: "An unknown creature",
            maxHealth: 10,
            stats: new Stats(10, 5, 2),
            monsterDefinitionId: null);
        var service = CreateService(_smartLootServiceMock.Object);
        var player = new Player("TestHero");

        // Act
        var loot = service.GenerateLoot(monster, player);

        // Assert
        loot.IsEmpty.Should().BeTrue();
        _smartLootServiceMock.Verify(x => x.SelectItem(It.IsAny<SmartLootContext>()), Times.Never);
    }

    /// <summary>
    /// Verifies that monster with no loot table returns empty.
    /// </summary>
    [Test]
    public void GenerateLoot_MonsterWithNoLootTable_ReturnsEmpty()
    {
        // Arrange
        var monster = CreateMonster();
        var definitionWithNoLoot = MonsterDefinition.Create(
            id: "goblin",
            name: "Goblin",
            description: "A small green creature",
            baseHealth: 15,
            baseAttack: 5,
            baseDefense: 2);
        
        _configProviderMock.Setup(x => x.GetMonsterById("goblin")).Returns(definitionWithNoLoot);
        
        var service = CreateService(_smartLootServiceMock.Object);
        var player = new Player("TestHero");

        // Act
        var loot = service.GenerateLoot(monster, player);

        // Assert
        loot.IsEmpty.Should().BeTrue();
        _smartLootServiceMock.Verify(x => x.SelectItem(It.IsAny<SmartLootContext>()), Times.Never);
    }

    #endregion
}
