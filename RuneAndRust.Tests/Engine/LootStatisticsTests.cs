using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Analysis;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the LootStatistics accumulator model.
/// </summary>
public class LootStatisticsTests
{
    #region Record Tests

    [Fact]
    public void Record_WithSuccessfulResult_IncrementsCounters()
    {
        // Arrange
        var stats = new LootStatistics();
        var items = new List<Item>
        {
            CreateItem(QualityTier.Scavenged, ItemType.Weapon, 50, 500),
            CreateItem(QualityTier.ClanForged, ItemType.Armor, 100, 1000)
        };
        var result = LootResult.Found("Test loot", items);

        // Act
        stats.Record(result);

        // Assert
        stats.TotalIterations.Should().Be(1);
        stats.TotalItemsDropped.Should().Be(2);
        stats.TotalScripValue.Should().Be(150);
        stats.TotalWeight.Should().Be(1500);
        stats.SuccessfulIterations.Should().Be(1);
    }

    [Fact]
    public void Record_WithEmptyResult_IncrementsIterationsOnly()
    {
        // Arrange
        var stats = new LootStatistics();
        var result = LootResult.Empty("Nothing found");

        // Act
        stats.Record(result);

        // Assert
        stats.TotalIterations.Should().Be(1);
        stats.TotalItemsDropped.Should().Be(0);
        stats.TotalScripValue.Should().Be(0);
        stats.SuccessfulIterations.Should().Be(0);
    }

    [Fact]
    public void Record_WithFailedResult_IncrementsIterationsOnly()
    {
        // Arrange
        var stats = new LootStatistics();
        var result = LootResult.Failure("Cannot search");

        // Act
        stats.Record(result);

        // Assert
        stats.TotalIterations.Should().Be(1);
        stats.TotalItemsDropped.Should().Be(0);
        stats.SuccessfulIterations.Should().Be(0);
    }

    [Fact]
    public void Record_AccumulatesMultipleResults()
    {
        // Arrange
        var stats = new LootStatistics();
        var result1 = LootResult.Found("First", new List<Item>
        {
            CreateItem(QualityTier.JuryRigged, ItemType.Junk, 10, 100)
        });
        var result2 = LootResult.Found("Second", new List<Item>
        {
            CreateItem(QualityTier.Scavenged, ItemType.Consumable, 20, 200),
            CreateItem(QualityTier.Scavenged, ItemType.Material, 30, 300)
        });
        var result3 = LootResult.Empty("Nothing");

        // Act
        stats.Record(result1);
        stats.Record(result2);
        stats.Record(result3);

        // Assert
        stats.TotalIterations.Should().Be(3);
        stats.TotalItemsDropped.Should().Be(3);
        stats.TotalScripValue.Should().Be(60);
        stats.TotalWeight.Should().Be(600);
        stats.SuccessfulIterations.Should().Be(2);
    }

    #endregion

    #region Quality Tier Distribution Tests

    [Fact]
    public void Record_TracksQualityTierDistribution()
    {
        // Arrange
        var stats = new LootStatistics();
        var items = new List<Item>
        {
            CreateItem(QualityTier.JuryRigged, ItemType.Weapon, 10, 100),
            CreateItem(QualityTier.JuryRigged, ItemType.Armor, 20, 200),
            CreateItem(QualityTier.Scavenged, ItemType.Consumable, 30, 300),
            CreateItem(QualityTier.ClanForged, ItemType.Material, 40, 400)
        };
        var result = LootResult.Found("Mixed loot", items);

        // Act
        stats.Record(result);

        // Assert
        stats.QualityTierCounts.Should().ContainKey(QualityTier.JuryRigged).WhoseValue.Should().Be(2);
        stats.QualityTierCounts.Should().ContainKey(QualityTier.Scavenged).WhoseValue.Should().Be(1);
        stats.QualityTierCounts.Should().ContainKey(QualityTier.ClanForged).WhoseValue.Should().Be(1);
        stats.QualityTierCounts.Should().NotContainKey(QualityTier.Optimized);
        stats.QualityTierCounts.Should().NotContainKey(QualityTier.MythForged);
    }

    [Fact]
    public void GetQualityTierPercent_CalculatesCorrectly()
    {
        // Arrange
        var stats = new LootStatistics();

        // Record 10 items: 6 Scavenged, 3 JuryRigged, 1 ClanForged
        for (int i = 0; i < 6; i++)
        {
            stats.Record(LootResult.Found("Scav", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100) }));
        }
        for (int i = 0; i < 3; i++)
        {
            stats.Record(LootResult.Found("Jury", new[] { CreateItem(QualityTier.JuryRigged, ItemType.Junk, 10, 100) }));
        }
        stats.Record(LootResult.Found("Clan", new[] { CreateItem(QualityTier.ClanForged, ItemType.Junk, 10, 100) }));

        // Act & Assert
        stats.GetQualityTierPercent(QualityTier.Scavenged).Should().BeApproximately(60.0, 0.01);
        stats.GetQualityTierPercent(QualityTier.JuryRigged).Should().BeApproximately(30.0, 0.01);
        stats.GetQualityTierPercent(QualityTier.ClanForged).Should().BeApproximately(10.0, 0.01);
        stats.GetQualityTierPercent(QualityTier.Optimized).Should().Be(0);
        stats.GetQualityTierPercent(QualityTier.MythForged).Should().Be(0);
    }

    [Fact]
    public void GetQualityTierPercent_ReturnsZeroWhenNoItems()
    {
        // Arrange
        var stats = new LootStatistics();

        // Act & Assert
        stats.GetQualityTierPercent(QualityTier.Scavenged).Should().Be(0);
    }

    #endregion

    #region Item Type Distribution Tests

    [Fact]
    public void Record_TracksItemTypeDistribution()
    {
        // Arrange
        var stats = new LootStatistics();
        var items = new List<Item>
        {
            CreateItem(QualityTier.Scavenged, ItemType.Weapon, 100, 1000),
            CreateItem(QualityTier.Scavenged, ItemType.Weapon, 100, 1000),
            CreateItem(QualityTier.Scavenged, ItemType.Armor, 80, 800),
            CreateItem(QualityTier.Scavenged, ItemType.Consumable, 20, 200),
            CreateItem(QualityTier.Scavenged, ItemType.Material, 30, 300)
        };
        var result = LootResult.Found("Type mix", items);

        // Act
        stats.Record(result);

        // Assert
        stats.ItemTypeCounts.Should().ContainKey(ItemType.Weapon).WhoseValue.Should().Be(2);
        stats.ItemTypeCounts.Should().ContainKey(ItemType.Armor).WhoseValue.Should().Be(1);
        stats.ItemTypeCounts.Should().ContainKey(ItemType.Consumable).WhoseValue.Should().Be(1);
        stats.ItemTypeCounts.Should().ContainKey(ItemType.Material).WhoseValue.Should().Be(1);
        stats.ItemTypeCounts.Should().NotContainKey(ItemType.Junk);
        stats.ItemTypeCounts.Should().NotContainKey(ItemType.KeyItem);
    }

    [Fact]
    public void GetItemTypePercent_CalculatesCorrectly()
    {
        // Arrange
        var stats = new LootStatistics();

        // Record 8 items: 4 Weapon, 2 Armor, 2 Consumable
        for (int i = 0; i < 4; i++)
        {
            stats.Record(LootResult.Found("Wpn", new[] { CreateItem(QualityTier.Scavenged, ItemType.Weapon, 100, 1000) }));
        }
        for (int i = 0; i < 2; i++)
        {
            stats.Record(LootResult.Found("Arm", new[] { CreateItem(QualityTier.Scavenged, ItemType.Armor, 80, 800) }));
        }
        for (int i = 0; i < 2; i++)
        {
            stats.Record(LootResult.Found("Con", new[] { CreateItem(QualityTier.Scavenged, ItemType.Consumable, 20, 200) }));
        }

        // Act & Assert
        stats.GetItemTypePercent(ItemType.Weapon).Should().BeApproximately(50.0, 0.01);
        stats.GetItemTypePercent(ItemType.Armor).Should().BeApproximately(25.0, 0.01);
        stats.GetItemTypePercent(ItemType.Consumable).Should().BeApproximately(25.0, 0.01);
        stats.GetItemTypePercent(ItemType.Junk).Should().Be(0);
    }

    #endregion

    #region Derived Metrics Tests

    [Fact]
    public void AverageScripPerIteration_CalculatesCorrectly()
    {
        // Arrange
        var stats = new LootStatistics();

        // 3 iterations: 100, 200, 0 scrip => 300 total / 3 = 100 avg
        stats.Record(LootResult.Found("A", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 100, 100) }));
        stats.Record(LootResult.Found("B", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 200, 200) }));
        stats.Record(LootResult.Empty("None"));

        // Act & Assert
        stats.AverageScripPerIteration.Should().BeApproximately(100.0, 0.01);
    }

    [Fact]
    public void AverageItemsPerIteration_CalculatesCorrectly()
    {
        // Arrange
        var stats = new LootStatistics();

        // 4 iterations: 2, 1, 0, 3 items => 6 total / 4 = 1.5 avg
        stats.Record(LootResult.Found("A", new[]
        {
            CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100),
            CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100)
        }));
        stats.Record(LootResult.Found("B", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100) }));
        stats.Record(LootResult.Empty("None"));
        stats.Record(LootResult.Found("C", new[]
        {
            CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100),
            CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100),
            CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100)
        }));

        // Act & Assert
        stats.AverageItemsPerIteration.Should().BeApproximately(1.5, 0.01);
    }

    [Fact]
    public void SuccessRate_CalculatesCorrectly()
    {
        // Arrange
        var stats = new LootStatistics();

        // 4 iterations: 3 successful, 1 empty => 75% success rate
        stats.Record(LootResult.Found("A", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100) }));
        stats.Record(LootResult.Found("B", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100) }));
        stats.Record(LootResult.Empty("None"));
        stats.Record(LootResult.Found("C", new[] { CreateItem(QualityTier.Scavenged, ItemType.Junk, 10, 100) }));

        // Act & Assert
        stats.SuccessRate.Should().BeApproximately(75.0, 0.01);
    }

    [Fact]
    public void DerivedMetrics_ReturnZeroWhenEmpty()
    {
        // Arrange
        var stats = new LootStatistics();

        // Act & Assert
        stats.AverageScripPerIteration.Should().Be(0);
        stats.AverageItemsPerIteration.Should().Be(0);
        stats.SuccessRate.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static Item CreateItem(QualityTier quality, ItemType type, int value, int weight)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Name = $"Test {type}",
            Quality = quality,
            ItemType = type,
            Value = value,
            Weight = weight,
            Description = "Test item for statistics"
        };
    }

    #endregion
}
