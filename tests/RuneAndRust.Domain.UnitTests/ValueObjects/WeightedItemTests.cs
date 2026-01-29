// ═══════════════════════════════════════════════════════════════════════════════
// RUNE & RUST — v0.16.3c Weighted Item Selection Tests
// ═══════════════════════════════════════════════════════════════════════════════
// File: WeightedItemTests.cs
// Purpose: Unit tests for WeightedItem value object
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="WeightedItem"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
/// <item><description>Creation with valid parameters</description></item>
/// <item><description>IsSelectable property for zero-weight items</description></item>
/// <item><description>Probability calculation accuracy</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class WeightedItemTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a simple loot entry for testing.
    /// </summary>
    private static LootEntry CreateTestEntry(string itemId = "test-item", string? categoryId = null) =>
        LootEntry.Create(itemId, categoryId: categoryId);

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidWeight_SetsPropertiesCorrectly()
    {
        // Arrange
        var entry = CreateTestEntry("iron-sword", "blades");

        // Act
        var item = WeightedItem.Create(entry, 75, "Uncommon");

        // Assert
        item.Item.Should().Be(entry);
        item.Weight.Should().Be(75);
        item.Rarity.Should().Be("Uncommon");
        item.ItemId.Should().Be("iron-sword");
        item.IsSelectable.Should().BeTrue();
    }

    [Test]
    public void CreateCommon_SetsCorrectWeightAndRarity()
    {
        // Arrange
        var entry = CreateTestEntry("wooden-shield");

        // Act
        var item = WeightedItem.CreateCommon(entry);

        // Assert
        item.Weight.Should().Be(WeightedItem.CommonWeight);
        item.Weight.Should().Be(100);
        item.Rarity.Should().Be("Common");
        item.IsSelectable.Should().BeTrue();
    }

    [Test]
    public void CreateRare_SetsCorrectWeightAndRarity()
    {
        // Arrange
        var entry = CreateTestEntry("enchanted-staff");

        // Act
        var item = WeightedItem.CreateRare(entry);

        // Assert
        item.Weight.Should().Be(WeightedItem.RareWeight);
        item.Weight.Should().Be(25);
        item.Rarity.Should().Be("Rare");
    }

    [Test]
    public void Create_WithNegativeWeight_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var entry = CreateTestEntry("test-item");

        // Act
        var act = () => WeightedItem.Create(entry, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weight");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ISSELECTABLE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsSelectable_WithZeroWeight_ReturnsFalse()
    {
        // Arrange & Act
        var item = WeightedItem.Create(CreateTestEntry("disabled-item"), 0);

        // Assert
        item.IsSelectable.Should().BeFalse();
        item.Weight.Should().Be(0);
    }

    [Test]
    public void IsSelectable_WithPositiveWeight_ReturnsTrue()
    {
        // Arrange & Act
        var item = WeightedItem.Create(CreateTestEntry("enabled-item"), 1);

        // Assert
        item.IsSelectable.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROBABILITY CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetSelectionProbability_CalculatesCorrectly()
    {
        // Arrange
        // Item with weight 25 in pool with total weight 100 = 25% probability
        var item = WeightedItem.CreateRare(CreateTestEntry("rare-item"));

        // Act
        var probability = item.GetSelectionProbability(totalWeight: 100);

        // Assert
        probability.Should().BeApproximately(0.25, 0.0001);
    }

    [Test]
    public void GetSelectionProbability_WithZeroTotalWeight_ReturnsZero()
    {
        // Arrange
        var item = WeightedItem.CreateCommon(CreateTestEntry("item"));

        // Act
        var probability = item.GetSelectionProbability(totalWeight: 0);

        // Assert
        probability.Should().Be(0);
    }

    [Test]
    public void FormatProbability_ReturnsFormattedPercentage()
    {
        // Arrange
        // Weight 50 / total 200 = 25%
        var item = WeightedItem.CreateUncommon(CreateTestEntry("item"));

        // Act
        var formatted = item.FormatProbability(totalWeight: 200);

        // Assert
        formatted.Should().Be("25.00%");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_WithRarity_IncludesRarityLabel()
    {
        // Arrange
        var item = WeightedItem.CreateRare(CreateTestEntry("rare-dagger"));

        // Act
        var result = item.ToString();

        // Assert
        result.Should().Contain("rare-dagger");
        result.Should().Contain("25");
        result.Should().Contain("Rare");
    }

    [Test]
    public void ToString_WithoutRarity_ExcludesRarityLabel()
    {
        // Arrange
        var item = WeightedItem.Create(CreateTestEntry("plain-item"), 50);

        // Act
        var result = item.ToString();

        // Assert
        result.Should().Contain("plain-item");
        result.Should().Contain("50");
        result.Should().NotContain("Rarity");
    }
}
