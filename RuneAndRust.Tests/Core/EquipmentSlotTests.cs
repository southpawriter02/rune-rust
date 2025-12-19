using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the EquipmentSlot enum.
/// Validates the seven equipment slots: MainHand, OffHand, Head, Body, Hands, Feet, Accessory.
/// </summary>
public class EquipmentSlotTests
{
    [Fact]
    public void EquipmentSlot_ShouldHaveExactlySevenValues()
    {
        // Arrange
        var values = Enum.GetValues<EquipmentSlot>();

        // Assert
        values.Should().HaveCount(7, "Rune & Rust has exactly seven equipment slots");
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_MainHand()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.MainHand).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_OffHand()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.OffHand).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_Head()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.Head).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_Body()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.Body).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_Hands()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.Hands).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_Feet()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.Feet).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_ShouldContain_Accessory()
    {
        // Assert
        Enum.IsDefined(typeof(EquipmentSlot), EquipmentSlot.Accessory).Should().BeTrue();
    }

    [Fact]
    public void EquipmentSlot_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)EquipmentSlot.MainHand).Should().Be(0);
        ((int)EquipmentSlot.OffHand).Should().Be(1);
        ((int)EquipmentSlot.Head).Should().Be(2);
        ((int)EquipmentSlot.Body).Should().Be(3);
        ((int)EquipmentSlot.Hands).Should().Be(4);
        ((int)EquipmentSlot.Feet).Should().Be(5);
        ((int)EquipmentSlot.Accessory).Should().Be(6);
    }

    [Theory]
    [InlineData(EquipmentSlot.MainHand, "MainHand")]
    [InlineData(EquipmentSlot.OffHand, "OffHand")]
    [InlineData(EquipmentSlot.Head, "Head")]
    [InlineData(EquipmentSlot.Body, "Body")]
    [InlineData(EquipmentSlot.Hands, "Hands")]
    [InlineData(EquipmentSlot.Feet, "Feet")]
    [InlineData(EquipmentSlot.Accessory, "Accessory")]
    public void EquipmentSlot_ToString_ReturnsExpectedName(EquipmentSlot slot, string expectedName)
    {
        // Assert
        slot.ToString().Should().Be(expectedName);
    }
}
