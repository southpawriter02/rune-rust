// ═══════════════════════════════════════════════════════════════════════════════
// RUNE & RUST — v0.16.3c Weighted Item Selection Tests
// ═══════════════════════════════════════════════════════════════════════════════
// File: WeightedItemPoolTests.cs
// Purpose: Unit tests for WeightedItemPool entity
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for <see cref="WeightedItemPool"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
/// <item><description>Weighted random selection distribution</description></item>
/// <item><description>Empty pool exception handling</description></item>
/// <item><description>Total weight calculation</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class WeightedItemPoolTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a simple loot entry for testing.
    /// </summary>
    private static LootEntry CreateTestEntry(string itemId) =>
        LootEntry.Create(itemId);

    /// <summary>
    /// Creates a pool with Common and Rare items for distribution testing.
    /// </summary>
    /// <remarks>
    /// Common weight: 100, Rare weight: 25
    /// Expected distribution: Common ~80%, Rare ~20%
    /// </remarks>
    private static WeightedItemPool CreateDistributionTestPool()
    {
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("common-item")));
        pool.Add(WeightedItem.CreateRare(CreateTestEntry("rare-item")));
        return pool;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISTRIBUTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectRandom_ReturnsItemsWithCorrectDistribution()
    {
        // Arrange
        // Common (100) + Rare (25) = 125 total weight
        // Expected: Common ~80%, Rare ~20%
        var pool = CreateDistributionTestPool();
        var selectionCounts = new Dictionary<string, int>
        {
            ["common-item"] = 0,
            ["rare-item"] = 0
        };

        const int iterations = 1000;

        // Act
        // Use incrementing seeds for reproducible but varied results
        for (var seed = 0; seed < iterations; seed++)
        {
            var selected = pool.SelectRandom(seed);
            selectionCounts[selected.ItemId]++;
        }

        // Assert
        // With 1000 iterations and 80/20 distribution:
        // - Common should be selected 700-900 times (80% ± 10%)
        // - Rare should be selected 100-300 times (20% ± 10%)
        selectionCounts["common-item"].Should().BeGreaterThan(700,
            "Common items (weight 100/125 = 80%) should be selected most often");
        selectionCounts["common-item"].Should().BeLessThan(900);

        selectionCounts["rare-item"].Should().BeGreaterThan(100,
            "Rare items (weight 25/125 = 20%) should be selected less often");
        selectionCounts["rare-item"].Should().BeLessThan(300);
    }

    [Test]
    public void SelectRandom_WithSameSeed_ReturnsSameItem()
    {
        // Arrange
        var pool = CreateDistributionTestPool();

        // Act
        var result1 = pool.SelectRandom(seed: 42);
        var result2 = pool.SelectRandom(seed: 42);

        // Assert
        result1.ItemId.Should().Be(result2.ItemId,
            "Same seed should produce same selection");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDGE CASE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectRandom_WithEmptyPool_ThrowsInvalidOperationException()
    {
        // Arrange
        var pool = WeightedItemPool.Create();

        // Act
        var act = () => pool.SelectRandom(new Random());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty*");
    }

    [Test]
    public void SelectRandom_WithAllZeroWeights_ThrowsInvalidOperationException()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.Create(CreateTestEntry("disabled-1"), 0));
        pool.Add(WeightedItem.Create(CreateTestEntry("disabled-2"), 0));

        // Act
        var act = () => pool.SelectRandom(new Random());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*selectable*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOTAL WEIGHT CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void TotalWeight_CalculatesCorrectly()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("common")));   // 100
        pool.Add(WeightedItem.CreateUncommon(CreateTestEntry("uncommon"))); // 50
        pool.Add(WeightedItem.CreateRare(CreateTestEntry("rare")));       // 25

        // Act
        var totalWeight = pool.TotalWeight;

        // Assert
        totalWeight.Should().Be(175, "100 + 50 + 25 = 175");
    }

    [Test]
    public void TotalWeight_ExcludesZeroWeightItems()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("common")));       // 100
        pool.Add(WeightedItem.Create(CreateTestEntry("disabled"), 0));         // 0 (excluded)
        pool.Add(WeightedItem.CreateRare(CreateTestEntry("rare")));           // 25

        // Act & Assert
        pool.TotalWeight.Should().Be(125, "Zero-weight items should not contribute");
        pool.ItemCount.Should().Be(3, "All items should be in the pool");
        pool.SelectableItemCount.Should().Be(2, "Only selectable items counted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ITEM MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Add_IncreasesTotalWeight()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("item-1"))); // 100

        // Act
        pool.Add(WeightedItem.CreateRare(CreateTestEntry("item-2"))); // +25

        // Assert
        pool.TotalWeight.Should().Be(125);
        pool.ItemCount.Should().Be(2);
    }

    [Test]
    public void Remove_DecreasesTotalWeight()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("item-1"))); // 100
        pool.Add(WeightedItem.CreateRare(CreateTestEntry("item-2")));   // 25

        // Act
        var removed = pool.Remove("item-1");

        // Assert
        removed.Should().BeTrue();
        pool.TotalWeight.Should().Be(25);
        pool.ItemCount.Should().Be(1);
    }

    [Test]
    public void Remove_NonExistentItem_ReturnsFalse()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("item-1")));

        // Act
        var removed = pool.Remove("non-existent");

        // Assert
        removed.Should().BeFalse();
        pool.ItemCount.Should().Be(1);
    }

    [Test]
    public void Contains_WithExistingItem_ReturnsTrue()
    {
        // Arrange
        var pool = WeightedItemPool.Create();
        pool.Add(WeightedItem.CreateCommon(CreateTestEntry("sword")));

        // Act & Assert
        pool.Contains("sword").Should().BeTrue();
        pool.Contains("axe").Should().BeFalse();
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        var pool = CreateDistributionTestPool();

        // Act
        pool.Clear();

        // Assert
        pool.ItemCount.Should().Be(0);
        pool.TotalWeight.Should().Be(0);
        pool.HasSelectableItems.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROBABILITY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetSelectionProbability_ReturnsCorrectValue()
    {
        // Arrange
        var pool = CreateDistributionTestPool();
        // Common (100) + Rare (25) = 125 total

        // Act
        var commonProb = pool.GetSelectionProbability("common-item");
        var rareProb = pool.GetSelectionProbability("rare-item");

        // Assert
        commonProb.Should().BeApproximately(0.80, 0.01, "Common: 100/125 = 80%");
        rareProb.Should().BeApproximately(0.20, 0.01, "Rare: 25/125 = 20%");
    }

    [Test]
    public void GetAllProbabilities_ReturnsAllItems()
    {
        // Arrange
        var pool = CreateDistributionTestPool();

        // Act
        var probabilities = pool.GetAllProbabilities();

        // Assert
        probabilities.Should().HaveCount(2);
        probabilities.Should().ContainKey("common-item");
        probabilities.Should().ContainKey("rare-item");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATEFROM TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateFrom_InitializesPoolWithItems()
    {
        // Arrange
        var items = new[]
        {
            WeightedItem.CreateCommon(CreateTestEntry("item-1")),
            WeightedItem.CreateRare(CreateTestEntry("item-2"))
        };

        // Act
        var pool = WeightedItemPool.CreateFrom(items);

        // Assert
        pool.ItemCount.Should().Be(2);
        pool.TotalWeight.Should().Be(125);
    }
}
