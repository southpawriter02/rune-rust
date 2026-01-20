using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="ItemSlotViewModel"/>.
/// </summary>
[TestFixture]
public class ItemSlotViewModelTests
{
    /// <summary>
    /// Verifies that default slot is empty.
    /// </summary>
    [Test]
    public void Constructor_DefaultSlot_IsEmpty()
    {
        // Arrange & Act
        var slot = new ItemSlotViewModel();

        // Assert
        slot.Item.Should().BeNull();
        slot.IsEmpty.Should().BeTrue();
        slot.Icon.Should().Be(" ");
        slot.ShortName.Should().Be("");
        slot.Tooltip.Should().Be("Empty Slot");
    }

    /// <summary>
    /// Verifies that slot with item is not empty.
    /// </summary>
    [Test]
    public void Item_WhenSet_UpdatesProperties()
    {
        // Arrange
        var slot = new ItemSlotViewModel();
        var item = Item.CreateIronSword();

        // Act
        slot.Item = item;

        // Assert
        slot.IsEmpty.Should().BeFalse();
        slot.Icon.Should().Be("⚔");
        slot.ShortName.Should().Be("Iro");
        slot.Tooltip.Should().Be("Iron Sword");
    }

    /// <summary>
    /// Verifies that GetCategoryIcon returns correct icons.
    /// </summary>
    [Test]
    [TestCase(ItemType.Weapon, "⚔")]
    [TestCase(ItemType.Armor, "🛡")]
    [TestCase(ItemType.Consumable, "🧪")]
    [TestCase(ItemType.Key, "🔑")]
    [TestCase(ItemType.Quest, "📜")]
    [TestCase(ItemType.Misc, "💎")]
    public void GetCategoryIcon_ReturnsCorrectIcon(ItemType type, string expected)
    {
        // Act
        var icon = ItemSlotViewModel.GetCategoryIcon(type);

        // Assert
        icon.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that IsSelected property can be toggled.
    /// </summary>
    [Test]
    public void IsSelected_WhenToggled_UpdatesCorrectly()
    {
        // Arrange
        var slot = new ItemSlotViewModel();

        // Act & Assert
        slot.IsSelected.Should().BeFalse();
        slot.IsSelected = true;
        slot.IsSelected.Should().BeTrue();
    }
}
