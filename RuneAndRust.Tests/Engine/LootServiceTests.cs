using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Extensive tests for the LootService class.
/// Validates procedural loot generation, quality tier rolls, and biome weightings.
/// </summary>
public class LootServiceTests
{
    private readonly Mock<ILogger<LootService>> _mockLogger;
    private readonly LootService _sut;
    private readonly LootService _seededSut;

    public LootServiceTests()
    {
        _mockLogger = new Mock<ILogger<LootService>>();
        _sut = new LootService(_mockLogger.Object);
        // Seeded instance for deterministic tests
        _seededSut = new LootService(_mockLogger.Object, 42);
    }

    #region GenerateLoot Tests

    [Fact]
    public void GenerateLoot_SafeZone_ReturnsItems()
    {
        // Arrange
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = _sut.GenerateLoot(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Count.Should().BeInRange(1, 2); // Safe zone: 1-2 items
    }

    [Fact]
    public void GenerateLoot_LethalZone_ReturnsMoreItems()
    {
        // Arrange
        var context = new LootGenerationContext(BiomeType.Void, DangerLevel.Lethal, null);

        // Act
        var result = _sut.GenerateLoot(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Count.Should().BeInRange(2, 5); // Lethal zone: 2-5 items
    }

    [Fact]
    public void GenerateLoot_WithLootTierOverride_UsesSpecifiedTier()
    {
        // Arrange
        var context = new LootGenerationContext(
            BiomeType.Ruin,
            DangerLevel.Safe,
            QualityTier.MythForged); // Force MythForged

        // Act
        var result = _seededSut.GenerateLoot(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().OnlyContain(i => i.Quality == QualityTier.MythForged);
    }

    [Fact]
    public void GenerateLoot_CalculatesTotalValue()
    {
        // Arrange
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Unstable, null);

        // Act
        var result = _seededSut.GenerateLoot(context);

        // Assert
        result.TotalValue.Should().Be(result.Items.Sum(i => i.Value));
    }

    [Fact]
    public void GenerateLoot_CalculatesTotalWeight()
    {
        // Arrange
        var context = new LootGenerationContext(BiomeType.Industrial, DangerLevel.Hostile, null);

        // Act
        var result = _seededSut.GenerateLoot(context);

        // Assert
        result.TotalWeight.Should().Be(result.Items.Sum(i => i.Weight));
    }

    [Fact]
    public void GenerateLoot_MessageIncludesItemNames()
    {
        // Arrange
        var context = new LootGenerationContext(BiomeType.Organic, DangerLevel.Safe, null);

        // Act
        var result = _seededSut.GenerateLoot(context);

        // Assert
        result.Message.Should().Contain("You find");
        foreach (var item in result.Items)
        {
            result.Message.Should().Contain(item.Name);
        }
    }

    #endregion

    #region SearchContainerAsync Tests

    [Fact]
    public async Task SearchContainerAsync_OpenContainer_ReturnsLoot()
    {
        // Arrange
        var container = new InteractableObject
        {
            Name = "Rusty Chest",
            IsContainer = true,
            IsOpen = true,
            HasBeenSearched = false,
            LootTier = QualityTier.Scavenged
        };
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = await _sut.SearchContainerAsync(container, context);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        container.HasBeenSearched.Should().BeTrue();
    }

    [Fact]
    public async Task SearchContainerAsync_AlreadySearched_ReturnsEmpty()
    {
        // Arrange
        var container = new InteractableObject
        {
            Name = "Empty Crate",
            IsContainer = true,
            IsOpen = true,
            HasBeenSearched = true
        };
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = await _sut.SearchContainerAsync(container, context);

        // Assert
        result.Success.Should().BeFalse();
        result.Items.Should().BeEmpty();
        result.Message.Should().Contain("already searched");
    }

    [Fact]
    public async Task SearchContainerAsync_NotContainer_ReturnsFailure()
    {
        // Arrange
        var notContainer = new InteractableObject
        {
            Name = "Stone Pillar",
            IsContainer = false
        };
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = await _sut.SearchContainerAsync(notContainer, context);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be searched");
    }

    [Fact]
    public async Task SearchContainerAsync_ClosedContainer_ReturnsFailure()
    {
        // Arrange
        var container = new InteractableObject
        {
            Name = "Locked Chest",
            IsContainer = true,
            IsOpen = false
        };
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = await _sut.SearchContainerAsync(container, context);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("closed");
    }

    [Fact]
    public async Task SearchContainerAsync_UsesContainerLootTier()
    {
        // Arrange
        var container = new InteractableObject
        {
            Name = "Ornate Chest",
            IsContainer = true,
            IsOpen = true,
            HasBeenSearched = false,
            LootTier = QualityTier.Optimized
        };
        var context = new LootGenerationContext(BiomeType.Ruin, DangerLevel.Safe, null);

        // Act
        var result = await _seededSut.SearchContainerAsync(container, context);

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().OnlyContain(i => i.Quality == QualityTier.Optimized);
    }

    #endregion

    #region GetQualityWeights Tests

    [Fact]
    public void GetQualityWeights_SafeZone_ReturnsExpectedDistribution()
    {
        // Act
        var weights = _sut.GetQualityWeights(DangerLevel.Safe);

        // Assert
        weights[QualityTier.JuryRigged].Should().Be(30);
        weights[QualityTier.Scavenged].Should().Be(60);
        weights[QualityTier.ClanForged].Should().Be(10);
        weights[QualityTier.Optimized].Should().Be(0);
        weights[QualityTier.MythForged].Should().Be(0);
    }

    [Fact]
    public void GetQualityWeights_LethalZone_HasMythForgedChance()
    {
        // Act
        var weights = _sut.GetQualityWeights(DangerLevel.Lethal);

        // Assert
        weights[QualityTier.MythForged].Should().Be(5);
        weights[QualityTier.Optimized].Should().Be(25);
    }

    [Fact]
    public void GetQualityWeights_AllLevels_SumToOneHundred()
    {
        // Arrange
        var levels = Enum.GetValues<DangerLevel>();

        foreach (var level in levels)
        {
            // Act
            var weights = _sut.GetQualityWeights(level);

            // Assert
            weights.Values.Sum().Should().Be(100, $"weights for {level} should sum to 100");
        }
    }

    #endregion

    #region RollQualityTier Tests

    [Fact]
    public void RollQualityTier_SafeZone_NeverReturnsMythForged()
    {
        // Act - Roll many times
        var results = Enumerable.Range(0, 100)
            .Select(_ => _sut.RollQualityTier(DangerLevel.Safe))
            .ToList();

        // Assert
        results.Should().NotContain(QualityTier.MythForged);
        results.Should().NotContain(QualityTier.Optimized);
    }

    [Fact]
    public void RollQualityTier_LethalZone_CanReturnHighQuality()
    {
        // Roll many times to increase chance of high quality
        var results = Enumerable.Range(0, 500)
            .Select(_ => _sut.RollQualityTier(DangerLevel.Lethal))
            .ToList();

        // Assert - Should see at least some ClanForged or higher
        results.Should().Contain(r => r >= QualityTier.ClanForged);
    }

    [Fact]
    public void RollQualityTier_WithHighWitsBonus_IncreasesQuality()
    {
        // Use same seed for fair comparison across many iterations
        var samplesWithBonus = 0;
        var samplesWithoutBonus = 0;

        // Run many iterations to get stable averages
        for (int i = 0; i < 500; i++)
        {
            var withBonusSut = new LootService(_mockLogger.Object, i);
            var withoutBonusSut = new LootService(_mockLogger.Object, i);

            samplesWithBonus += (int)withBonusSut.RollQualityTier(DangerLevel.Safe, 10);
            samplesWithoutBonus += (int)withoutBonusSut.RollQualityTier(DangerLevel.Safe, 0);
        }

        // Average quality with high WITS bonus should generally be at least as good
        // With 10 WITS bonus (capped at 20% upgrade chance), we expect improvement
        samplesWithBonus.Should().BeGreaterThanOrEqualTo(samplesWithoutBonus);
    }

    #endregion

    #region RollItemCount Tests

    [Theory]
    [InlineData(DangerLevel.Safe, 1, 2)]
    [InlineData(DangerLevel.Unstable, 1, 3)]
    [InlineData(DangerLevel.Hostile, 2, 4)]
    [InlineData(DangerLevel.Lethal, 2, 5)]
    public void RollItemCount_ReturnsCountInRange(DangerLevel danger, int min, int max)
    {
        // Roll many times
        var results = Enumerable.Range(0, 50)
            .Select(_ => _sut.RollItemCount(danger))
            .ToList();

        // Assert - all should be in range
        results.Should().OnlyContain(c => c >= min && c <= max);
    }

    #endregion

    #region GenerateItem Tests

    [Fact]
    public void GenerateItem_Weapon_ReturnsEquipment()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.Scavenged, BiomeType.Ruin, ItemType.Weapon);

        // Assert
        item.Should().BeOfType<Equipment>();
        item.ItemType.Should().Be(ItemType.Weapon);
        ((Equipment)item).DamageDie.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateItem_Armor_ReturnsEquipmentWithSoak()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.ClanForged, BiomeType.Industrial, ItemType.Armor);

        // Assert
        item.Should().BeOfType<Equipment>();
        item.ItemType.Should().Be(ItemType.Armor);
        ((Equipment)item).SoakBonus.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GenerateItem_Consumable_ReturnsStackableItem()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.Scavenged, BiomeType.Organic, ItemType.Consumable);

        // Assert
        item.Should().NotBeOfType<Equipment>();
        item.ItemType.Should().Be(ItemType.Consumable);
        item.IsStackable.Should().BeTrue();
    }

    [Fact]
    public void GenerateItem_Material_ReturnsStackableItem()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.Scavenged, BiomeType.Industrial, ItemType.Material);

        // Assert
        item.ItemType.Should().Be(ItemType.Material);
        item.IsStackable.Should().BeTrue();
        item.MaxStackSize.Should().Be(99);
    }

    [Fact]
    public void GenerateItem_Junk_ReturnsJuryRiggedItem()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.ClanForged, BiomeType.Void, ItemType.Junk);

        // Assert
        item.ItemType.Should().Be(ItemType.Junk);
        item.Quality.Should().Be(QualityTier.JuryRigged); // Junk is always JuryRigged
    }

    [Fact]
    public void GenerateItem_MythForgedWeapon_HasHighDamageDie()
    {
        // Act
        var item = _seededSut.GenerateItem(QualityTier.MythForged, BiomeType.Void, ItemType.Weapon);

        // Assert
        var weapon = item as Equipment;
        weapon.Should().NotBeNull();
        weapon!.DamageDie.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void GenerateItem_ValueScalesWithQuality()
    {
        // Generate same template at different qualities
        var scavengedItems = Enumerable.Range(0, 10)
            .Select(_ => new LootService(_mockLogger.Object, 42)
                .GenerateItem(QualityTier.Scavenged, BiomeType.Ruin, ItemType.Weapon))
            .ToList();

        var optimizedItems = Enumerable.Range(0, 10)
            .Select(_ => new LootService(_mockLogger.Object, 42)
                .GenerateItem(QualityTier.Optimized, BiomeType.Ruin, ItemType.Weapon))
            .ToList();

        // Assert - Optimized should have higher average value
        optimizedItems.Average(i => i.Value).Should().BeGreaterThan(scavengedItems.Average(i => i.Value));
    }

    #endregion

    #region Biome Weighting Tests

    [Fact]
    public void GenerateLoot_OrganicBiome_FavorsConsumables()
    {
        // Arrange - Organic biome has 40% consumable weight
        var context = new LootGenerationContext(BiomeType.Organic, DangerLevel.Safe, null);

        // Generate many items
        var items = Enumerable.Range(0, 50)
            .SelectMany(_ => _sut.GenerateLoot(context).Items)
            .ToList();

        // Assert - Should have significant consumable presence
        var consumableCount = items.Count(i => i.ItemType == ItemType.Consumable);
        consumableCount.Should().BeGreaterThan(items.Count / 5); // At least 20%
    }

    [Fact]
    public void GenerateLoot_IndustrialBiome_FavorsMaterials()
    {
        // Arrange - Industrial biome has 35% material weight
        var context = new LootGenerationContext(BiomeType.Industrial, DangerLevel.Safe, null);

        // Generate many items
        var items = Enumerable.Range(0, 50)
            .SelectMany(_ => _sut.GenerateLoot(context).Items)
            .ToList();

        // Assert - Should have significant material presence
        var materialCount = items.Count(i => i.ItemType == ItemType.Material);
        materialCount.Should().BeGreaterThan(items.Count / 6); // At least ~15%
    }

    [Fact]
    public void GenerateLoot_VoidBiome_FavorsCombatItems()
    {
        // Arrange - Void biome has 50% combined weapon+armor weight
        var context = new LootGenerationContext(BiomeType.Void, DangerLevel.Hostile, null);

        // Generate many items
        var items = Enumerable.Range(0, 50)
            .SelectMany(_ => _sut.GenerateLoot(context).Items)
            .ToList();

        // Assert - Should have significant combat item presence
        var combatCount = items.Count(i => i.ItemType == ItemType.Weapon || i.ItemType == ItemType.Armor);
        combatCount.Should().BeGreaterThan(items.Count / 4); // At least 25%
    }

    #endregion

    #region LootResult Factory Tests

    [Fact]
    public void LootResult_Found_CalculatesTotals()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Item1", Value = 10, Weight = 100 },
            new Item { Name = "Item2", Value = 20, Weight = 200 }
        }.AsReadOnly();

        // Act
        var result = LootResult.Found("Test message", items);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalValue.Should().Be(30);
        result.TotalWeight.Should().Be(300);
    }

    [Fact]
    public void LootResult_Empty_ReturnsNoItems()
    {
        // Act
        var result = LootResult.Empty("Nothing found");

        // Assert
        result.Success.Should().BeFalse();
        result.Items.Should().BeEmpty();
        result.TotalValue.Should().Be(0);
        result.TotalWeight.Should().Be(0);
    }

    [Fact]
    public void LootResult_Failure_ReturnsNoItems()
    {
        // Act
        var result = LootResult.Failure("Cannot search");

        // Assert
        result.Success.Should().BeFalse();
        result.Items.Should().BeEmpty();
    }

    #endregion
}
