using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for calculating dynamic prices based on reputation, merchant type, and category (v0.9)
/// </summary>
public class PricingService
{
    private static readonly ILogger _log = Log.ForContext<PricingService>();

    /// <summary>
    /// Calculate the price player pays when buying from a merchant
    /// Base price is marked up 2x, then modified by reputation and category
    /// </summary>
    public int CalculateBuyPrice(Merchant merchant, int basePrice, string itemCategory, PlayerCharacter player)
    {
        // Start with base markup (default 2.0x)
        float finalPrice = basePrice * merchant.BaseMarkup;

        // Apply category modifier (merchant specialization)
        if (merchant.CategoryModifiers.TryGetValue(itemCategory, out float categoryMod))
        {
            finalPrice *= categoryMod;
        }

        // Apply reputation modifier (±30% based on faction reputation)
        float reputationMod = CalculateReputationModifier(merchant, player);
        finalPrice *= reputationMod;

        // Round and ensure minimum price of 1
        int result = Math.Max(1, (int)Math.Round(finalPrice));

        _log.Debug("Buy price calculated: BasePrice={BasePrice}, Category={Category}, RepMod={RepMod:F2}, FinalPrice={FinalPrice}",
            basePrice, itemCategory, reputationMod, result);

        return result;
    }

    /// <summary>
    /// Calculate the price merchant pays when player sells an item
    /// Default is 50% of base value (inverse of 2x markup), modified by reputation and category
    /// </summary>
    public int CalculateSellPrice(Merchant merchant, int basePrice, string itemCategory, PlayerCharacter player)
    {
        // Start with base sell price (50% of base value)
        float finalPrice = basePrice / merchant.BaseMarkup;

        // Apply category modifier (merchant specialization)
        if (merchant.CategoryModifiers.TryGetValue(itemCategory, out float categoryMod))
        {
            finalPrice *= categoryMod;
        }

        // Apply reputation modifier (±30% based on faction reputation)
        float reputationMod = CalculateReputationModifier(merchant, player);
        finalPrice *= reputationMod;

        // Round and ensure minimum price of 1
        int result = Math.Max(1, (int)Math.Round(finalPrice));

        _log.Debug("Sell price calculated: BasePrice={BasePrice}, Category={Category}, RepMod={RepMod:F2}, FinalPrice={FinalPrice}",
            basePrice, itemCategory, reputationMod, result);

        return result;
    }

    /// <summary>
    /// Calculate reputation-based price modifier
    /// Range: 0.7x (Hostile) to 1.3x (Friendly)
    /// Neutral reputation = 1.0x (no change)
    /// </summary>
    private float CalculateReputationModifier(Merchant merchant, PlayerCharacter player)
    {
        // Get faction reputation (-100 to +100)
        int factionRep = player.FactionReputations.GetReputation(merchant.Faction);

        // Convert to modifier range (0.7 to 1.3)
        // At -100 rep: 0.7x (30% worse prices when buying, 30% less when selling)
        // At 0 rep: 1.0x (neutral)
        // At +100 rep: 1.3x (30% better prices when buying, 30% more when selling)
        float modifier = 1.0f + (factionRep / 100f * merchant.ReputationPriceRange);

        // When buying, higher reputation = lower prices (inverse)
        // When selling, higher reputation = higher prices (direct)
        // This is handled by the caller context

        return modifier;
    }

    /// <summary>
    /// Calculate the reputation modifier for BUYING (inverse relationship)
    /// Higher reputation = better prices = lower multiplier
    /// </summary>
    public float GetBuyPriceModifier(Merchant merchant, PlayerCharacter player)
    {
        int factionRep = player.FactionReputations.GetReputation(merchant.Faction);

        // Inverse: high rep = lower prices
        // At -100 rep: 1.3x (30% markup - expensive)
        // At 0 rep: 1.0x (neutral)
        // At +100 rep: 0.7x (30% discount - cheap)
        float modifier = 1.0f - (factionRep / 100f * merchant.ReputationPriceRange);

        return modifier;
    }

    /// <summary>
    /// Calculate the reputation modifier for SELLING (direct relationship)
    /// Higher reputation = merchant pays more = higher multiplier
    /// </summary>
    public float GetSellPriceModifier(Merchant merchant, PlayerCharacter player)
    {
        int factionRep = player.FactionReputations.GetReputation(merchant.Faction);

        // Direct: high rep = higher sell prices
        // At -100 rep: 0.7x (30% less - poor offer)
        // At 0 rep: 1.0x (neutral)
        // At +100 rep: 1.3x (30% more - generous offer)
        float modifier = 1.0f + (factionRep / 100f * merchant.ReputationPriceRange);

        return modifier;
    }

    /// <summary>
    /// Get formatted price display with reputation modifier indication
    /// </summary>
    public string GetPriceDisplay(int price, Merchant merchant, PlayerCharacter player, bool isBuying)
    {
        float repMod = isBuying
            ? GetBuyPriceModifier(merchant, player)
            : GetSellPriceModifier(merchant, player);

        string modifierText = "";
        if (repMod < 0.95f)
        {
            int discount = (int)((1.0f - repMod) * 100);
            modifierText = $" [green](-{discount}%)[/]";
        }
        else if (repMod > 1.05f)
        {
            int markup = (int)((repMod - 1.0f) * 100);
            modifierText = $" [red](+{markup}%)[/]";
        }

        return $"{price} Cogs ⚙{modifierText}";
    }

    /// <summary>
    /// Get the category for an item (Equipment, Component, Consumable)
    /// </summary>
    public string GetItemCategory(object item)
    {
        return item switch
        {
            Equipment => "Equipment",
            CraftingComponent => "Component",
            _ => "Consumable"
        };
    }

    /// <summary>
    /// Calculate final buy price with all modifiers
    /// </summary>
    public int GetFinalBuyPrice(Merchant merchant, ShopItem shopItem, PlayerCharacter player)
    {
        float price = shopItem.BasePrice * merchant.BaseMarkup;

        // Apply category modifier
        if (merchant.CategoryModifiers.TryGetValue(shopItem.ItemType, out float categoryMod))
        {
            price *= categoryMod;
        }

        // Apply reputation modifier (inverse for buying)
        float repMod = GetBuyPriceModifier(merchant, player);
        price *= repMod;

        return Math.Max(1, (int)Math.Round(price));
    }

    /// <summary>
    /// Calculate final sell price with all modifiers
    /// </summary>
    public int GetFinalSellPrice(Merchant merchant, int baseValue, string itemType, PlayerCharacter player)
    {
        float price = baseValue / merchant.BaseMarkup;

        // Apply category modifier
        if (merchant.CategoryModifiers.TryGetValue(itemType, out float categoryMod))
        {
            price *= categoryMod;
        }

        // Apply reputation modifier (direct for selling)
        float repMod = GetSellPriceModifier(merchant, player);
        price *= repMod;

        return Math.Max(1, (int)Math.Round(price));
    }
}
