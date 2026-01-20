namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Shop Window for buying and selling items.
/// </summary>
public partial class ShopWindow : Window
{
    /// <summary>Creates a new shop window.</summary>
    public ShopWindow()
    {
        InitializeComponent();
        DataContext = new ShopWindowViewModel(Close);
    }

    private void OnShopItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            DataContext is ShopWindowViewModel vm &&
            border.DataContext is ShopItemViewModel item)
        {
            vm.SelectShopItemCommand.Execute(item);
        }
    }

    private void OnPlayerItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            DataContext is ShopWindowViewModel vm &&
            border.DataContext is ShopItemViewModel item)
        {
            vm.SelectPlayerItemCommand.Execute(item);
        }
    }
}
