using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the InventoryViewHelper static class (v0.3.7a).
/// Validates color mappings and formatting utilities for the inventory UI.
/// </summary>
public class InventoryViewHelperTests
{
    #region GetQualityColor Tests

    [Fact]
    public void GetQualityColor_JuryRigged_ReturnsGrey()
    {
        // Act
        var result = InventoryViewHelper.GetQualityColor(QualityTier.JuryRigged);

        // Assert
        result.Should().Be("grey");
    }

    [Fact]
    public void GetQualityColor_Scavenged_ReturnsWhite()
    {
        // Act
        var result = InventoryViewHelper.GetQualityColor(QualityTier.Scavenged);

        // Assert
        result.Should().Be("white");
    }

    [Fact]
    public void GetQualityColor_ClanForged_ReturnsGreen()
    {
        // Act
        var result = InventoryViewHelper.GetQualityColor(QualityTier.ClanForged);

        // Assert
        result.Should().Be("green");
    }

    [Fact]
    public void GetQualityColor_Optimized_ReturnsBlue()
    {
        // Act
        var result = InventoryViewHelper.GetQualityColor(QualityTier.Optimized);

        // Assert
        result.Should().Be("blue");
    }

    [Fact]
    public void GetQualityColor_MythForged_ReturnsGold()
    {
        // Act
        var result = InventoryViewHelper.GetQualityColor(QualityTier.MythForged);

        // Assert
        result.Should().Be("gold1");
    }

    #endregion

    #region GetBurdenColor Tests

    [Fact]
    public void GetBurdenColor_Light_ReturnsGreen()
    {
        // Act
        var result = InventoryViewHelper.GetBurdenColor(BurdenState.Light);

        // Assert
        result.Should().Be("green");
    }

    [Fact]
    public void GetBurdenColor_Heavy_ReturnsYellow()
    {
        // Act
        var result = InventoryViewHelper.GetBurdenColor(BurdenState.Heavy);

        // Assert
        result.Should().Be("yellow");
    }

    [Fact]
    public void GetBurdenColor_Overburdened_ReturnsRed()
    {
        // Act
        var result = InventoryViewHelper.GetBurdenColor(BurdenState.Overburdened);

        // Assert
        result.Should().Be("red");
    }

    #endregion

    #region GetSlotDisplayName Tests

    [Theory]
    [InlineData(EquipmentSlot.MainHand, "Main Hand")]
    [InlineData(EquipmentSlot.OffHand, "Off Hand")]
    [InlineData(EquipmentSlot.Head, "Head")]
    [InlineData(EquipmentSlot.Body, "Body")]
    [InlineData(EquipmentSlot.Hands, "Hands")]
    [InlineData(EquipmentSlot.Feet, "Feet")]
    [InlineData(EquipmentSlot.Accessory, "Accessory")]
    public void GetSlotDisplayName_ReturnsCorrectName(EquipmentSlot slot, string expectedName)
    {
        // Act
        var result = InventoryViewHelper.GetSlotDisplayName(slot);

        // Assert
        result.Should().Be(expectedName);
    }

    #endregion

    #region FormatWeight Tests

    [Fact]
    public void FormatWeight_Under1000_ReturnsGrams()
    {
        // Act
        var result = InventoryViewHelper.FormatWeight(500);

        // Assert
        result.Should().Be("500g");
    }

    [Fact]
    public void FormatWeight_ExactlyOneKg_ReturnsKilograms()
    {
        // Act
        var result = InventoryViewHelper.FormatWeight(1000);

        // Assert
        result.Should().Be("1.0kg");
    }

    [Fact]
    public void FormatWeight_Over1000_ReturnsKilograms()
    {
        // Act
        var result = InventoryViewHelper.FormatWeight(4200);

        // Assert
        result.Should().Be("4.2kg");
    }

    [Fact]
    public void FormatWeight_Zero_ReturnsZeroGrams()
    {
        // Act
        var result = InventoryViewHelper.FormatWeight(0);

        // Assert
        result.Should().Be("0g");
    }

    #endregion

    #region RenderBurdenBar Tests

    [Fact]
    public void RenderBurdenBar_ZeroPercent_ReturnsAllEmpty()
    {
        // Act
        var result = InventoryViewHelper.RenderBurdenBar(0, 10);

        // Assert
        result.Should().HaveLength(10);
        result.Should().NotContain("\u2588"); // No filled blocks
    }

    [Fact]
    public void RenderBurdenBar_FiftyPercent_ReturnsHalfFilled()
    {
        // Act
        var result = InventoryViewHelper.RenderBurdenBar(50, 10);

        // Assert
        result.Should().HaveLength(10);
        result.Substring(0, 5).Should().Be("\u2588\u2588\u2588\u2588\u2588"); // 5 filled
    }

    [Fact]
    public void RenderBurdenBar_HundredPercent_ReturnsAllFilled()
    {
        // Act
        var result = InventoryViewHelper.RenderBurdenBar(100, 10);

        // Assert
        result.Should().HaveLength(10);
        result.Should().NotContain("\u2591"); // No empty blocks
    }

    [Fact]
    public void RenderBurdenBar_ClampsOverHundred()
    {
        // Act
        var result = InventoryViewHelper.RenderBurdenBar(150, 10);

        // Assert - Should clamp to 100%
        result.Should().HaveLength(10);
        result.Should().NotContain("\u2591"); // All filled
    }

    #endregion

    #region FormatItemWithQuantity Tests

    [Fact]
    public void FormatItemWithQuantity_SingleItem_ReturnsNameOnly()
    {
        // Act
        var result = InventoryViewHelper.FormatItemWithQuantity("Health Potion", 1);

        // Assert
        result.Should().Be("Health Potion");
    }

    [Fact]
    public void FormatItemWithQuantity_MultipleItems_ReturnsNameWithQuantity()
    {
        // Act
        var result = InventoryViewHelper.FormatItemWithQuantity("Health Potion", 5);

        // Assert
        result.Should().Be("Health Potion (x5)");
    }

    #endregion

    #region GetItemTypeIcon Tests

    [Fact]
    public void GetItemTypeIcon_Weapon_ReturnsRedSword()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.Weapon);

        // Assert
        result.Should().Contain("\u2694"); // Crossed swords
        result.Should().Contain("red");
    }

    [Fact]
    public void GetItemTypeIcon_Armor_ReturnsCyanShield()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.Armor);

        // Assert
        result.Should().Contain("\u26E8"); // Shield
        result.Should().Contain("cyan");
    }

    [Fact]
    public void GetItemTypeIcon_Consumable_ReturnsGreenCup()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.Consumable);

        // Assert
        result.Should().Contain("\u2615"); // Cup
        result.Should().Contain("green");
    }

    [Fact]
    public void GetItemTypeIcon_Material_ReturnsYellowDiamond()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.Material);

        // Assert
        result.Should().Contain("\u25C6"); // Diamond
        result.Should().Contain("yellow");
    }

    [Fact]
    public void GetItemTypeIcon_KeyItem_ReturnsGoldStar()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.KeyItem);

        // Assert
        result.Should().Contain("\u2605"); // Star
        result.Should().Contain("gold1");
    }

    [Fact]
    public void GetItemTypeIcon_Junk_ReturnsGreySquare()
    {
        // Act
        var result = InventoryViewHelper.GetItemTypeIcon(ItemType.Junk);

        // Assert
        result.Should().Contain("\u25A0"); // Square
        result.Should().Contain("grey");
    }

    #endregion
}
