using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Gui.Controls;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Controls;

/// <summary>
/// Unit tests for <see cref="ItemSlotControl"/>.
/// </summary>
[TestFixture]
public class ItemSlotControlTests
{
    /// <summary>
    /// Verifies that default ItemSlotControl is empty.
    /// </summary>
    [Test]
    public void Constructor_DefaultState_IsEmpty()
    {
        // Arrange & Act
        var control = new ItemSlotControl();

        // Assert
        control.Item.Should().BeNull();
        control.IsEmpty.Should().BeTrue();
        control.IsSelected.Should().BeFalse();
        control.Tooltip.Should().Be("Empty Slot");
    }

    /// <summary>
    /// Verifies that setting Item updates computed properties.
    /// </summary>
    [Test]
    public void Item_WhenSet_UpdatesIconAndShortName()
    {
        // Arrange
        var control = new ItemSlotControl();
        var item = Item.CreateHealthPotion();

        // Act
        control.Item = item;

        // Assert
        control.IsEmpty.Should().BeFalse();
        control.Icon.Should().Be("🧪");
        control.ShortName.Should().Be("Hea");
        control.Tooltip.Should().Be("Health Potion");
    }

    /// <summary>
    /// Verifies that IsSelected property works.
    /// </summary>
    [Test]
    public void IsSelected_WhenSet_UpdatesProperty()
    {
        // Arrange
        var control = new ItemSlotControl();

        // Act
        control.IsSelected = true;

        // Assert
        control.IsSelected.Should().BeTrue();
    }
}
