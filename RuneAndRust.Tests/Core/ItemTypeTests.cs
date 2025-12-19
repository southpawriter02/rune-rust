using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the ItemType enum.
/// Validates the six item types: Weapon, Armor, Consumable, Material, KeyItem, Junk.
/// </summary>
public class ItemTypeTests
{
    [Fact]
    public void ItemType_ShouldHaveExactlySixValues()
    {
        // Arrange
        var values = Enum.GetValues<ItemType>();

        // Assert
        values.Should().HaveCount(6, "Rune & Rust has exactly six item types");
    }

    [Fact]
    public void ItemType_ShouldContain_Weapon()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.Weapon).Should().BeTrue();
    }

    [Fact]
    public void ItemType_ShouldContain_Armor()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.Armor).Should().BeTrue();
    }

    [Fact]
    public void ItemType_ShouldContain_Consumable()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.Consumable).Should().BeTrue();
    }

    [Fact]
    public void ItemType_ShouldContain_Material()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.Material).Should().BeTrue();
    }

    [Fact]
    public void ItemType_ShouldContain_KeyItem()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.KeyItem).Should().BeTrue();
    }

    [Fact]
    public void ItemType_ShouldContain_Junk()
    {
        // Assert
        Enum.IsDefined(typeof(ItemType), ItemType.Junk).Should().BeTrue();
    }

    [Fact]
    public void ItemType_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)ItemType.Weapon).Should().Be(0);
        ((int)ItemType.Armor).Should().Be(1);
        ((int)ItemType.Consumable).Should().Be(2);
        ((int)ItemType.Material).Should().Be(3);
        ((int)ItemType.KeyItem).Should().Be(4);
        ((int)ItemType.Junk).Should().Be(5);
    }

    [Theory]
    [InlineData(ItemType.Weapon, "Weapon")]
    [InlineData(ItemType.Armor, "Armor")]
    [InlineData(ItemType.Consumable, "Consumable")]
    [InlineData(ItemType.Material, "Material")]
    [InlineData(ItemType.KeyItem, "KeyItem")]
    [InlineData(ItemType.Junk, "Junk")]
    public void ItemType_ToString_ReturnsExpectedName(ItemType itemType, string expectedName)
    {
        // Assert
        itemType.ToString().Should().Be(expectedName);
    }
}
