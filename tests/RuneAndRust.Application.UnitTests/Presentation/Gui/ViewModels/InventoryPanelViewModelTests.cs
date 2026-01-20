using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="InventoryPanelViewModel"/>.
/// </summary>
[TestFixture]
public class InventoryPanelViewModelTests
{
    private InventoryPanelViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new InventoryPanelViewModel();
    }

    /// <summary>
    /// Verifies that constructor initializes with sample data.
    /// </summary>
    [Test]
    public void Constructor_InitializesWithSampleData()
    {
        // Assert
        _viewModel.Slots.Should().HaveCount(_viewModel.MaxCapacity);
        _viewModel.CurrentCapacity.Should().Be(8);
        _viewModel.GridColumns.Should().Be(5);
    }

    /// <summary>
    /// Verifies that SelectSlotCommand selects a slot.
    /// </summary>
    [Test]
    public void SelectSlotCommand_SelectsSlot()
    {
        // Arrange
        var slot = _viewModel.Slots[0];

        // Act
        _viewModel.SelectSlotCommand.Execute(slot);

        // Assert
        _viewModel.SelectedSlot.Should().Be(slot);
        slot.IsSelected.Should().BeTrue();
        _viewModel.SelectedItem.Should().Be(slot.Item);
    }

    /// <summary>
    /// Verifies that selecting a new slot deselects the previous one.
    /// </summary>
    [Test]
    public void SelectSlotCommand_DeselectsPreviousSlot()
    {
        // Arrange
        var slot1 = _viewModel.Slots[0];
        var slot2 = _viewModel.Slots[1];

        // Act
        _viewModel.SelectSlotCommand.Execute(slot1);
        _viewModel.SelectSlotCommand.Execute(slot2);

        // Assert
        slot1.IsSelected.Should().BeFalse();
        slot2.IsSelected.Should().BeTrue();
        _viewModel.SelectedSlot.Should().Be(slot2);
    }

    /// <summary>
    /// Verifies that NavigateCommand moves selection right.
    /// </summary>
    [Test]
    public void NavigateCommand_Right_MovesSelection()
    {
        // Arrange
        _viewModel.SelectSlotCommand.Execute(_viewModel.Slots[0]);

        // Act
        _viewModel.NavigateCommand.Execute("right");

        // Assert
        _viewModel.SelectedSlot.Should().Be(_viewModel.Slots[1]);
    }

    /// <summary>
    /// Verifies that NavigateCommand moves selection down.
    /// </summary>
    [Test]
    public void NavigateCommand_Down_MovesSelectionByGridColumns()
    {
        // Arrange
        _viewModel.SelectSlotCommand.Execute(_viewModel.Slots[0]);

        // Act
        _viewModel.NavigateCommand.Execute("down");

        // Assert
        _viewModel.SelectedSlot.Should().Be(_viewModel.Slots[5]); // GridColumns = 5
    }

    /// <summary>
    /// Verifies that NavigateCommand does not go out of bounds left.
    /// </summary>
    [Test]
    public void NavigateCommand_Left_StaysAtBoundary()
    {
        // Arrange
        _viewModel.SelectSlotCommand.Execute(_viewModel.Slots[0]);

        // Act
        _viewModel.NavigateCommand.Execute("left");

        // Assert
        _viewModel.SelectedSlot.Should().Be(_viewModel.Slots[0]);
    }

    /// <summary>
    /// Verifies that ClearSelectionCommand clears selection.
    /// </summary>
    [Test]
    public void ClearSelectionCommand_ClearsSelection()
    {
        // Arrange
        _viewModel.SelectSlotCommand.Execute(_viewModel.Slots[0]);

        // Act
        _viewModel.ClearSelectionCommand.Execute(null);

        // Assert
        _viewModel.SelectedSlot.Should().BeNull();
        _viewModel.SelectedItem.Should().BeNull();
    }

    /// <summary>
    /// Verifies that UpdateFromInventory updates slots.
    /// </summary>
    [Test]
    public void UpdateFromInventory_UpdatesSlots()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.TryAdd(Item.CreateHealthPotion());
        inventory.TryAdd(Item.CreateIronSword());

        // Act
        _viewModel.UpdateFromInventory(inventory);

        // Assert
        _viewModel.CurrentCapacity.Should().Be(2);
        _viewModel.MaxCapacity.Should().Be(10);
        _viewModel.Slots[0].Item.Should().NotBeNull();
        _viewModel.Slots[1].Item.Should().NotBeNull();
        _viewModel.Slots[2].Item.Should().BeNull();
    }
}
