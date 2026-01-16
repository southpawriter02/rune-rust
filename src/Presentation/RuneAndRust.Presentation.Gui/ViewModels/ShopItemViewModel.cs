namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for an item in the shop.
/// </summary>
public partial class ShopItemViewModel : ViewModelBase
{
    /// <summary>Gets the item ID.</summary>
    public Guid ItemId { get; }

    /// <summary>Gets the item icon.</summary>
    [ObservableProperty] private string _icon;

    /// <summary>Gets the item name.</summary>
    [ObservableProperty] private string _name;

    /// <summary>Gets the item description.</summary>
    [ObservableProperty] private string _description;

    /// <summary>Gets the buy price.</summary>
    [ObservableProperty] private int _buyPrice;

    /// <summary>Gets the sell price.</summary>
    [ObservableProperty] private int _sellPrice;

    /// <summary>Gets whether the item is selected.</summary>
    [ObservableProperty] private bool _isSelected;

    /// <summary>Gets whether this is a player item (for selling).</summary>
    public bool IsPlayerItem { get; }

    /// <summary>Gets the buy price text.</summary>
    public string BuyPriceText => $"💰 {BuyPrice}";

    /// <summary>Gets the sell price text.</summary>
    public string SellPriceText => $"💰 {SellPrice}";

    /// <summary>Gets item stats for display.</summary>
    public string Stats { get; }

    /// <summary>Creates a shop item ViewModel.</summary>
    public ShopItemViewModel(Guid id, string icon, string name, string description, int buyPrice, int sellPrice,
        string stats = "", bool isPlayerItem = false)
    {
        ItemId = id;
        _icon = icon;
        _name = name;
        _description = description;
        _buyPrice = buyPrice;
        _sellPrice = sellPrice;
        Stats = stats;
        IsPlayerItem = isPlayerItem;
    }
}
