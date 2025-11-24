using Avalonia.Controls;
using RuneAndRust.DesktopUI.ViewModels;
using System;

namespace RuneAndRust.DesktopUI.Views;

public partial class CombatView : UserControl
{
    public CombatView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Wire up grid control events to ViewModel commands
        if (DataContext is CombatViewModel viewModel)
        {
            var gridControl = this.FindControl<Controls.CombatGridControl>("CombatGridControl");
            if (gridControl != null)
            {
                gridControl.CellClicked += (s, pos) => viewModel.CellClickedCommand.Execute(pos).Subscribe();
                gridControl.CellHovered += (s, pos) => viewModel.CellHoveredCommand.Execute(pos).Subscribe();
            }
        }
    }
}
