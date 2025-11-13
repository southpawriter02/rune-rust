using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for processing buy/sell transactions between player and merchants (v0.9)
/// </summary>
public class TransactionService
{
    private static readonly ILogger _log = Log.ForContext<TransactionService>();
    private readonly CurrencyService _currencyService;
    private readonly PricingService _pricingService;

    public TransactionService(CurrencyService currencyService, PricingService pricingService)
    {
        _currencyService = currencyService;
        _pricingService = pricingService;
    }

    /// <summary>
    /// Process player buying an item from a merchant
    /// </summary>
    public TransactionResult BuyItem(Merchant merchant, ShopItem shopItem, PlayerCharacter player, int quantity = 1)
    {
        // Calculate final price
        int unitPrice = _pricingService.GetFinalBuyPrice(merchant, shopItem, player);
        int totalPrice = unitPrice * quantity;

        // Check if player can afford
        if (!_currencyService.CanAfford(player, totalPrice))
        {
            _log.Information("Purchase failed: Player cannot afford {ItemId} - Price={Price}, PlayerCurrency={PlayerCurrency}",
                shopItem.ItemId, totalPrice, player.Currency);
            return new TransactionResult
            {
                Success = false,
                Message = $"Not enough currency. Need {_currencyService.GetCurrencyDisplay(totalPrice)}, have {_currencyService.GetCurrencyDisplay(player.Currency)}"
            };
        }

        // Check if merchant has enough stock
        if (!shopItem.IsInfiniteStock && shopItem.Quantity < quantity)
        {
            _log.Information("Purchase failed: Insufficient stock for {ItemId} - Requested={Requested}, Available={Available}",
                shopItem.ItemId, quantity, shopItem.Quantity);
            return new TransactionResult
            {
                Success = false,
                Message = $"Merchant only has {shopItem.Quantity} in stock."
            };
        }

        // Process transaction
        _currencyService.SpendCurrency(player, totalPrice, $"Purchase: {shopItem.ItemId} from {merchant.Name}");

        // Update merchant inventory
        merchant.Inventory.DecreaseQuantity(shopItem.ItemId, quantity);

        _log.Information("Purchase successful: Player={PlayerId}, Merchant={MerchantId}, Item={ItemId}, Quantity={Quantity}, Price={Price}",
            player.Name, merchant.Id, shopItem.ItemId, quantity, totalPrice);

        return new TransactionResult
        {
            Success = true,
            Message = $"Purchased {shopItem.ItemId} x{quantity} for {_currencyService.GetCurrencyDisplay(totalPrice)}",
            ItemId = shopItem.ItemId,
            ItemType = shopItem.ItemType,
            Quantity = quantity,
            Price = totalPrice
        };
    }

    /// <summary>
    /// Process player selling an equipment item to a merchant
    /// </summary>
    public TransactionResult SellEquipment(Merchant merchant, Equipment equipment, PlayerCharacter player)
    {
        // Calculate sell price
        int sellPrice = _pricingService.GetFinalSellPrice(merchant, ((int)equipment.Quality * 10) /* SellValue calculated from Quality */, "Equipment", player);

        // Check if player has the item
        if (!player.Inventory.Contains(equipment))
        {
            _log.Warning("Sell failed: Player does not have equipment {ItemName}", equipment.Name);
            return new TransactionResult
            {
                Success = false,
                Message = $"You don't have {equipment.Name} in your inventory."
            };
        }

        // Remove from player inventory
        player.Inventory.Remove(equipment);

        // Give currency to player
        _currencyService.AddCurrency(player, sellPrice, $"Sold: {equipment.Name} to {merchant.Name}");

        _log.Information("Sale successful: Player={PlayerId}, Merchant={MerchantId}, Item={ItemName}, Price={Price}",
            player.Name, merchant.Id, equipment.Name, sellPrice);

        return new TransactionResult
        {
            Success = true,
            Message = $"Sold {equipment.GetDisplayName()} for {_currencyService.GetCurrencyDisplay(sellPrice)}",
            ItemId = equipment.Name,
            ItemType = "Equipment",
            Quantity = 1,
            Price = sellPrice
        };
    }

    /// <summary>
    /// Process player selling crafting components to a merchant
    /// </summary>
    public TransactionResult SellComponents(Merchant merchant, ComponentType componentType, int quantity, PlayerCharacter player)
    {
        var component = CraftingComponent.Create(componentType);

        // Check if component is tradeable
        if (!component.IsTradeable)
        {
            _log.Warning("Sell failed: Component {ComponentType} is not tradeable", componentType);
            return new TransactionResult
            {
                Success = false,
                Message = $"{component.Name} cannot be sold."
            };
        }

        // Check if player has enough components
        if (!player.CraftingComponents.ContainsKey(componentType) ||
            player.CraftingComponents[componentType] < quantity)
        {
            _log.Warning("Sell failed: Player does not have enough {ComponentType} - Requested={Requested}, Has={Has}",
                componentType, quantity, player.CraftingComponents.GetValueOrDefault(componentType, 0));
            return new TransactionResult
            {
                Success = false,
                Message = $"You don't have {quantity} {component.Name}."
            };
        }

        // Calculate sell price (per unit)
        int unitPrice = _pricingService.GetFinalSellPrice(merchant, ((int)component.Quality * 10) /* SellValue calculated from Quality */, "Component", player);
        int totalPrice = unitPrice * quantity;

        // Remove components from player
        player.CraftingComponents[componentType] -= quantity;
        if (player.CraftingComponents[componentType] <= 0)
        {
            player.CraftingComponents.Remove(componentType);
        }

        // Give currency to player
        _currencyService.AddCurrency(player, totalPrice, $"Sold: {component.Name} x{quantity} to {merchant.Name}");

        _log.Information("Component sale successful: Player={PlayerId}, Merchant={MerchantId}, Component={ComponentType}, Quantity={Quantity}, TotalPrice={Price}",
            player.Name, merchant.Id, componentType, quantity, totalPrice);

        return new TransactionResult
        {
            Success = true,
            Message = $"Sold {component.Name} x{quantity} for {_currencyService.GetCurrencyDisplay(totalPrice)}",
            ItemId = componentType.ToString(),
            ItemType = "Component",
            Quantity = quantity,
            Price = totalPrice
        };
    }

    /// <summary>
    /// Find a merchant in the current room
    /// </summary>
    public Merchant? FindMerchantInRoom(Room room)
    {
        return room.NPCs.OfType<Merchant>().FirstOrDefault();
    }

    /// <summary>
    /// Check if there's a merchant in the room
    /// </summary>
    public bool HasMerchantInRoom(Room room)
    {
        return room.NPCs.Any(npc => npc is Merchant);
    }
}

/// <summary>
/// Result of a buy/sell transaction
/// </summary>
public class TransactionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // Equipment, Component, Consumable
    public int Quantity { get; set; } = 0;
    public int Price { get; set; } = 0;
}
