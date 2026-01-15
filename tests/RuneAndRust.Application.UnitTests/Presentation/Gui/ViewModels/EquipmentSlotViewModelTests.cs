using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="EquipmentSlotViewModel"/>.
/// </summary>
[TestFixture]
public class EquipmentSlotViewModelTests
{
    /// <summary>
    /// Verifies that empty slot has correct default values.
    /// </summary>
    [Test]
    public void EmptySlot_HasNoneAsItemName()
    {
        // Arrange & Act
        var slot = new EquipmentSlotViewModel("Weapon", "⚔");

        // Assert
        slot.SlotName.Should().Be("Weapon");
        slot.SlotIcon.Should().Be("⚔");
        slot.ItemName.Should().Be("None");
        slot.IsEmpty.Should().BeTrue();
        slot.HasBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that slot with item has correct values.
    /// </summary>
    [Test]
    public void SlotWithItem_HasCorrectProperties()
    {
        // Arrange & Act
        var slot = new EquipmentSlotViewModel("Armor", "🛡", "Plate Mail", "+5 def");

        // Assert
        slot.SlotName.Should().Be("Armor");
        slot.ItemName.Should().Be("Plate Mail");
        slot.ItemBonus.Should().Be("+5 def");
        slot.IsEmpty.Should().BeFalse();
        slot.HasBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that null item name results in empty slot.
    /// </summary>
    [Test]
    public void SlotWithNullItem_IsEmpty()
    {
        // Arrange & Act
        var slot = new EquipmentSlotViewModel("Ring", "💍", null, null);

        // Assert
        slot.ItemName.Should().Be("None");
        slot.IsEmpty.Should().BeTrue();
        slot.HasBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that item without bonus has HasBonus false.
    /// </summary>
    [Test]
    public void SlotWithItemNoBonus_HasBonusFalse()
    {
        // Arrange & Act
        var slot = new EquipmentSlotViewModel("Amulet", "📿", "Charm", null);

        // Assert
        slot.ItemName.Should().Be("Charm");
        slot.IsEmpty.Should().BeFalse();
        slot.HasBonus.Should().BeFalse();
    }
}
