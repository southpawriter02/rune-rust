namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Gui.Services;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Panel control displaying the player's inventory.
/// </summary>
/// <remarks>
/// Displays items in a grid layout with:
/// - Click-to-select items
/// - Right-click context menus
/// - Keyboard navigation (arrow keys)
/// - Selected item details display
/// - Capacity and weight indicators
/// </remarks>
public partial class InventoryPanel : UserControl
{
    private readonly IContextMenuService? _contextMenuService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryPanel"/> class.
    /// </summary>
    public InventoryPanel()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    /// <summary>
    /// Initializes a new instance with context menu support.
    /// </summary>
    /// <param name="contextMenuService">The context menu service.</param>
    public InventoryPanel(IContextMenuService contextMenuService) : this()
    {
        _contextMenuService = contextMenuService;
    }

    /// <summary>
    /// Handles pointer pressed events on inventory slots.
    /// </summary>
    /// <param name="sender">The source control.</param>
    /// <param name="e">The pointer event args.</param>
    public void OnSlotPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control ||
            DataContext is not InventoryPanelViewModel vm)
            return;

        var point = e.GetCurrentPoint(control);

        // Get the slot ViewModel from the control's data context
        var slotVm = GetSlotViewModel(control);
        if (slotVm is null)
            return;

        if (point.Properties.IsLeftButtonPressed)
        {
            // Left-click: select
            vm.SelectSlotCommand.Execute(slotVm);
        }
        else if (point.Properties.IsRightButtonPressed && slotVm.Item is not null)
        {
            // Right-click: context menu
            ShowItemContextMenu(slotVm.Item, control, e.GetPosition(control));
            e.Handled = true;
        }
    }

    private static ItemSlotViewModel? GetSlotViewModel(Control control)
    {
        // The control itself may have the DataContext, or we need to look at parent
        if (control.DataContext is ItemSlotViewModel slotVm)
            return slotVm;
        
        // Try finding parent ItemSlotControl
        var parent = control.Parent;
        while (parent is not null)
        {
            if (parent.DataContext is ItemSlotViewModel parentSlotVm)
                return parentSlotVm;
            parent = parent.Parent;
        }

        return null;
    }

    private void ShowItemContextMenu(Item item, Control target, Point position)
    {
        if (_contextMenuService is null)
            return;

        var isEquipped = DataContext is InventoryPanelViewModel vm && vm.IsItemEquipped(item);

        var menu = _contextMenuService.CreateItemMenu(item, isEquipped);
        _contextMenuService.ShowMenu(menu, target, position);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not InventoryPanelViewModel vm)
            return;

        switch (e.Key)
        {
            case Key.Left:
                vm.NavigateCommand.Execute("left");
                e.Handled = true;
                break;
            case Key.Right:
                vm.NavigateCommand.Execute("right");
                e.Handled = true;
                break;
            case Key.Up:
                vm.NavigateCommand.Execute("up");
                e.Handled = true;
                break;
            case Key.Down:
                vm.NavigateCommand.Execute("down");
                e.Handled = true;
                break;
            case Key.Escape:
                vm.ClearSelectionCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}

