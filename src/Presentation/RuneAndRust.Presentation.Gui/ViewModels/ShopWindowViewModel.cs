namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Shop Window.
/// </summary>
public partial class ShopWindowViewModel : ViewModelBase
{
    private readonly Action? _closeAction;

    /// <summary>Gets the shop name.</summary>
    [ObservableProperty] private string _shopName = "MERCHANT'S WARES";

    /// <summary>Gets the player's gold.</summary>
    [ObservableProperty] private int _playerGold = 500;

    /// <summary>Gets the selected shop item.</summary>
    [ObservableProperty] private ShopItemViewModel? _selectedShopItem;

    /// <summary>Gets the selected player item.</summary>
    [ObservableProperty] private ShopItemViewModel? _selectedPlayerItem;

    /// <summary>Gets the transaction message.</summary>
    [ObservableProperty] private string? _transactionMessage;

    /// <summary>Gets the item comparison.</summary>
    [ObservableProperty] private ItemComparisonViewModel? _comparison;

    /// <summary>Shop inventory items.</summary>
    public ObservableCollection<ShopItemViewModel> ShopItems { get; } = [];

    /// <summary>Player inventory items.</summary>
    public ObservableCollection<ShopItemViewModel> PlayerItems { get; } = [];

    /// <summary>Whether a shop item is selected.</summary>
    public bool HasSelectedShopItem => SelectedShopItem is not null;

    /// <summary>Whether a player item is selected.</summary>
    public bool HasSelectedPlayerItem => SelectedPlayerItem is not null;

    /// <summary>Whether any item is selected.</summary>
    public bool HasSelectedItem => HasSelectedShopItem || HasSelectedPlayerItem;

    /// <summary>Gets the selected item name.</summary>
    public string SelectedItemName => SelectedShopItem?.Name ?? SelectedPlayerItem?.Name ?? "";

    /// <summary>Gets the selected item stats.</summary>
    public string SelectedItemStats => SelectedShopItem?.Stats ?? SelectedPlayerItem?.Stats ?? "";

    /// <summary>Gets the selected item description.</summary>
    public string SelectedItemDescription => SelectedShopItem?.Description ?? SelectedPlayerItem?.Description ?? "";

    /// <summary>Whether the player can buy the selected item.</summary>
    public bool CanBuy => SelectedShopItem is not null && PlayerGold >= SelectedShopItem.BuyPrice;

    /// <summary>Whether the player can sell the selected item.</summary>
    public bool CanSell => SelectedPlayerItem is not null;

    /// <summary>Whether there is a comparison to show.</summary>
    public bool HasComparison => Comparison is not null;

    /// <summary>Buy button text with price.</summary>
    public string BuyButtonText => SelectedShopItem is not null
        ? $"BUY ({SelectedShopItem.BuyPrice}g)"
        : "BUY";

    /// <summary>Sell button text with price.</summary>
    public string SellButtonText => SelectedPlayerItem is not null
        ? $"SELL ({SelectedPlayerItem.SellPrice}g)"
        : "SELL";

    /// <summary>Creates a shop window ViewModel.</summary>
    public ShopWindowViewModel(Action? closeAction = null)
    {
        _closeAction = closeAction;
        LoadSampleData();
    }

    /// <summary>Selects a shop item.</summary>
    [RelayCommand]
    private void SelectShopItem(ShopItemViewModel item)
    {
        // Deselect previous
        if (SelectedShopItem is not null) SelectedShopItem.IsSelected = false;
        if (SelectedPlayerItem is not null) SelectedPlayerItem.IsSelected = false;

        SelectedShopItem = item;
        SelectedPlayerItem = null;
        item.IsSelected = true;

        TransactionMessage = null;
        UpdateComparison();
        NotifySelectionChanged();

        Log.Debug("Selected shop item: {Name}", item.Name);
    }

    /// <summary>Selects a player item.</summary>
    [RelayCommand]
    private void SelectPlayerItem(ShopItemViewModel item)
    {
        // Deselect previous
        if (SelectedShopItem is not null) SelectedShopItem.IsSelected = false;
        if (SelectedPlayerItem is not null) SelectedPlayerItem.IsSelected = false;

        SelectedPlayerItem = item;
        SelectedShopItem = null;
        item.IsSelected = true;

        TransactionMessage = null;
        Comparison = null;
        NotifySelectionChanged();

        Log.Debug("Selected player item: {Name}", item.Name);
    }

    /// <summary>Buys the selected shop item.</summary>
    [RelayCommand]
    private void Buy()
    {
        if (SelectedShopItem is null) return;

        if (PlayerGold < SelectedShopItem.BuyPrice)
        {
            TransactionMessage = "❌ Insufficient gold!";
            Log.Warning("Buy failed: insufficient gold");
            return;
        }

        var item = SelectedShopItem;
        PlayerGold -= item.BuyPrice;
        ShopItems.Remove(item);

        // Add to player inventory
        var playerItem = new ShopItemViewModel(item.ItemId, item.Icon, item.Name, item.Description,
            item.BuyPrice, item.SellPrice, item.Stats, isPlayerItem: true);
        PlayerItems.Add(playerItem);

        SelectedShopItem = null;
        TransactionMessage = $"✅ Purchased {item.Name}!";
        NotifySelectionChanged();

        Log.Information("Purchased {Item} for {Price}g", item.Name, item.BuyPrice);
    }

    /// <summary>Sells the selected player item.</summary>
    [RelayCommand]
    private void Sell()
    {
        if (SelectedPlayerItem is null) return;

        var item = SelectedPlayerItem;
        PlayerGold += item.SellPrice;
        PlayerItems.Remove(item);

        SelectedPlayerItem = null;
        TransactionMessage = $"✅ Sold {item.Name} for {item.SellPrice}g!";
        NotifySelectionChanged();

        Log.Information("Sold {Item} for {Price}g", item.Name, item.SellPrice);
    }

    /// <summary>Closes the shop window.</summary>
    [RelayCommand]
    private void Close() => _closeAction?.Invoke();

    private void UpdateComparison()
    {
        // Compare against first player item of same type (simplified)
        var equipped = PlayerItems.FirstOrDefault();
        Comparison = ItemComparisonViewModel.Create(SelectedShopItem, equipped);
        OnPropertyChanged(nameof(HasComparison));
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(HasSelectedShopItem));
        OnPropertyChanged(nameof(HasSelectedPlayerItem));
        OnPropertyChanged(nameof(HasSelectedItem));
        OnPropertyChanged(nameof(SelectedItemName));
        OnPropertyChanged(nameof(SelectedItemStats));
        OnPropertyChanged(nameof(SelectedItemDescription));
        OnPropertyChanged(nameof(CanBuy));
        OnPropertyChanged(nameof(CanSell));
        OnPropertyChanged(nameof(BuyButtonText));
        OnPropertyChanged(nameof(SellButtonText));
    }

    private void LoadSampleData()
    {
        // Shop items
        ShopItems.Add(new ShopItemViewModel(Guid.NewGuid(), "⚔", "Steel Sword",
            "A finely crafted steel blade.", 150, 75, "Damage: 1d8+2"));
        ShopItems.Add(new ShopItemViewModel(Guid.NewGuid(), "🛡", "Iron Shield",
            "A sturdy iron shield.", 100, 50, "Defense: +3"));
        ShopItems.Add(new ShopItemViewModel(Guid.NewGuid(), "🧪", "Alchemical Mending",
            "Restores 25 HP.", 25, 12, "Healing: 25 HP"));
        ShopItems.Add(new ShopItemViewModel(Guid.NewGuid(), "🧪", "Aetheric Essence",
            "Restores 20 MP.", 30, 15, "Mana: 20 MP"));
        ShopItems.Add(new ShopItemViewModel(Guid.NewGuid(), "💍", "Ring of Strength",
            "Increases strength by 2.", 200, 100, "Strength: +2"));

        // Player items
        PlayerItems.Add(new ShopItemViewModel(Guid.NewGuid(), "⚔", "Iron Sword",
            "A basic iron sword.", 80, 40, "Damage: 1d6+1", isPlayerItem: true));
        PlayerItems.Add(new ShopItemViewModel(Guid.NewGuid(), "🧪", "Healing Herb",
            "A common healing herb.", 10, 5, "Healing: 10 HP", isPlayerItem: true));

        Log.Information("Shop loaded: {ShopCount} shop items, {PlayerCount} player items",
            ShopItems.Count, PlayerItems.Count);
    }
}
