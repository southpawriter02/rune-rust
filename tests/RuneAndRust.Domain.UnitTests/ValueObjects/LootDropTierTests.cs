using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for LootDrop tier aggregation functionality (v0.16.0d).
/// </summary>
[TestFixture]
public class LootDropTierTests
{
    // ═══════════════════════════════════════════════════════════════
    // HighestTier TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithMultipleTiers_CalculatesHighestTier()
    {
        // Arrange
        var items = new[]
        {
            DroppedItem.CreateWeapon("item1", "Item 1", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("item2", "Item 2", QualityTier.ClanForged),
            DroppedItem.CreateWeapon("item3", "Item 3", QualityTier.JuryRigged)
        };

        // Act
        var loot = LootDrop.Create(items, sourceMonster: "goblin");

        // Assert
        loot.HighestTier.Should().Be(QualityTier.ClanForged);
    }

    [Test]
    public void Create_WithNoItems_HighestTierIsNull()
    {
        // Arrange & Act
        var loot = LootDrop.Create();

        // Assert
        loot.HighestTier.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // HasMythForged TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithMythForgedItem_SetsMythForgedFlag()
    {
        // Arrange
        var items = new[]
        {
            DroppedItem.CreateWeapon("common", "Common", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("legendary", "Legendary", QualityTier.MythForged)
        };

        // Act
        var loot = LootDrop.Create(items, sourceMonster: "boss");

        // Assert
        loot.HasMythForged.Should().BeTrue();
    }

    [Test]
    public void Create_WithoutMythForged_ClearsMythForgedFlag()
    {
        // Arrange
        var items = new[]
        {
            DroppedItem.CreateWeapon("common", "Common", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("uncommon", "Uncommon", QualityTier.ClanForged)
        };

        // Act
        var loot = LootDrop.Create(items, sourceMonster: "goblin");

        // Assert
        loot.HasMythForged.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // TierCounts TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithMixedTiers_CountsCorrectly()
    {
        // Arrange
        var items = new[]
        {
            DroppedItem.CreateWeapon("item1", "Item 1", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("item2", "Item 2", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("item3", "Item 3", QualityTier.ClanForged),
            DroppedItem.CreateWeapon("item4", "Item 4", QualityTier.MythForged)
        };

        // Act
        var loot = LootDrop.Create(items, sourceMonster: "miniboss");

        // Assert
        loot.TierCounts.Should().ContainKey(QualityTier.Scavenged).WhoseValue.Should().Be(2);
        loot.TierCounts.Should().ContainKey(QualityTier.ClanForged).WhoseValue.Should().Be(1);
        loot.TierCounts.Should().ContainKey(QualityTier.MythForged).WhoseValue.Should().Be(1);
        loot.TierCounts.Should().NotContainKey(QualityTier.JuryRigged);
    }

    // ═══════════════════════════════════════════════════════════════
    // TotalItems TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TotalItems_WithItems_ReturnsCorrectCount()
    {
        // Arrange
        var items = new[]
        {
            DroppedItem.CreateWeapon("item1", "Item 1", QualityTier.Scavenged),
            DroppedItem.CreateWeapon("item2", "Item 2", QualityTier.ClanForged)
        };

        // Act
        var loot = LootDrop.Create(items);

        // Assert
        loot.TotalItems.Should().Be(2);
    }

    [Test]
    public void TotalItems_WhenEmpty_ReturnsZero()
    {
        // Act
        var loot = LootDrop.Empty;

        // Assert
        loot.TotalItems.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // SourceMonster TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithSourceMonster_StoresMonsterName()
    {
        // Arrange
        var items = new[] { DroppedItem.CreateWeapon("sword", "Sword", QualityTier.Scavenged) };

        // Act
        var loot = LootDrop.Create(items, sourceMonster: "dragon-boss");

        // Assert
        loot.SourceMonster.Should().Be("dragon-boss");
    }

    [Test]
    public void Create_WithoutSourceMonster_DefaultsToEmpty()
    {
        // Act
        var loot = LootDrop.Create();

        // Assert
        loot.SourceMonster.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // CombineWith TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CombineWith_RecalculatesTierStatistics()
    {
        // Arrange
        var loot1 = LootDrop.Create(
            new[] { DroppedItem.CreateWeapon("item1", "Item 1", QualityTier.Scavenged) },
            sourceMonster: "goblin");
        
        var loot2 = LootDrop.Create(
            new[] { DroppedItem.CreateWeapon("item2", "Item 2", QualityTier.MythForged) },
            sourceMonster: "boss");

        // Act
        var combined = loot1.CombineWith(loot2);

        // Assert
        combined.TotalItems.Should().Be(2);
        combined.HighestTier.Should().Be(QualityTier.MythForged);
        combined.HasMythForged.Should().BeTrue();
        combined.SourceMonster.Should().Be("goblin"); // First source preserved
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_WhenEmpty_ReturnsEmptyMarker()
    {
        // Act
        var result = LootDrop.Empty.ToString();

        // Assert
        result.Should().Be("[LootDrop: Empty]");
    }

    [Test]
    public void ToString_WithItems_IncludesTierInfo()
    {
        // Arrange
        var loot = LootDrop.Create(
            new[] { DroppedItem.CreateWeapon("sword", "Sword", QualityTier.ClanForged) },
            sourceMonster: "goblin");

        // Act
        var result = loot.ToString();

        // Assert
        result.Should().Contain("1 item(s)");
        result.Should().Contain("highest tier=ClanForged");
        result.Should().Contain("from goblin");
    }

    [Test]
    public void ToString_WithMythForged_IndicatesLegendaryDrop()
    {
        // Arrange
        var loot = LootDrop.Create(
            new[] { DroppedItem.CreateWeapon("legendary", "Legendary", QualityTier.MythForged) });

        // Act
        var result = loot.ToString();

        // Assert
        result.Should().Contain("includes Myth-Forged");
    }
}
