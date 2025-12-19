using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Item entity.
/// Validates item creation, properties, and defaults.
/// </summary>
public class ItemTests
{
    #region Identity Tests

    [Fact]
    public void Item_NewItem_HasUniqueId()
    {
        // Arrange & Act
        var item1 = new Item();
        var item2 = new Item();

        // Assert
        item1.Id.Should().NotBeEmpty();
        item2.Id.Should().NotBeEmpty();
        item1.Id.Should().NotBe(item2.Id, "each item should have a unique Id");
    }

    [Fact]
    public void Item_Name_DefaultsToEmptyString()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Name.Should().BeEmpty();
    }

    [Fact]
    public void Item_Name_CanBeSet()
    {
        // Arrange
        var item = new Item();

        // Act
        item.Name = "Iron Sword";

        // Assert
        item.Name.Should().Be("Iron Sword");
    }

    [Fact]
    public void Item_ItemType_DefaultsToJunk()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.ItemType.Should().Be(ItemType.Junk);
    }

    #endregion

    #region Description Tests

    [Fact]
    public void Item_Description_DefaultsToEmptyString()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Description.Should().BeEmpty();
    }

    [Fact]
    public void Item_DetailedDescription_DefaultsToNull()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.DetailedDescription.Should().BeNull();
    }

    [Fact]
    public void Item_DetailedDescription_CanBeSet()
    {
        // Arrange
        var item = new Item();

        // Act
        item.DetailedDescription = "A finely crafted blade.";

        // Assert
        item.DetailedDescription.Should().Be("A finely crafted blade.");
    }

    #endregion

    #region Physical Properties Tests

    [Fact]
    public void Item_Weight_DefaultsToZero()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Weight.Should().Be(0);
    }

    [Fact]
    public void Item_Weight_CanBeSet()
    {
        // Arrange
        var item = new Item();

        // Act
        item.Weight = 1500; // 1.5 kg in grams

        // Assert
        item.Weight.Should().Be(1500);
    }

    [Fact]
    public void Item_Value_DefaultsToZero()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Value.Should().Be(0);
    }

    [Fact]
    public void Item_Value_CanBeSet()
    {
        // Arrange
        var item = new Item();

        // Act
        item.Value = 250;

        // Assert
        item.Value.Should().Be(250);
    }

    #endregion

    #region Quality Tests

    [Fact]
    public void Item_Quality_DefaultsToScavenged()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Quality.Should().Be(QualityTier.Scavenged);
    }

    [Theory]
    [InlineData(QualityTier.JuryRigged)]
    [InlineData(QualityTier.Scavenged)]
    [InlineData(QualityTier.ClanForged)]
    [InlineData(QualityTier.Optimized)]
    [InlineData(QualityTier.MythForged)]
    public void Item_Quality_CanBeSetToAllTiers(QualityTier tier)
    {
        // Arrange
        var item = new Item();

        // Act
        item.Quality = tier;

        // Assert
        item.Quality.Should().Be(tier);
    }

    #endregion

    #region Stacking Tests

    [Fact]
    public void Item_IsStackable_DefaultsToFalse()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.IsStackable.Should().BeFalse();
    }

    [Fact]
    public void Item_MaxStackSize_DefaultsToOne()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.MaxStackSize.Should().Be(1);
    }

    [Fact]
    public void Item_StackableItem_CanHaveHighMaxStackSize()
    {
        // Arrange
        var item = new Item
        {
            IsStackable = true,
            MaxStackSize = 99
        };

        // Assert
        item.IsStackable.Should().BeTrue();
        item.MaxStackSize.Should().Be(99);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Item_CreatedAt_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var item = new Item();

        // Assert
        var after = DateTime.UtcNow;
        item.CreatedAt.Should().BeOnOrAfter(before);
        item.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Item_LastModified_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var item = new Item();

        // Assert
        var after = DateTime.UtcNow;
        item.LastModified.Should().BeOnOrAfter(before);
        item.LastModified.Should().BeOnOrBefore(after);
    }

    #endregion
}
