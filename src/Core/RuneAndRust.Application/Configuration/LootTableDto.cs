using System.Text.Json.Serialization;

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Data transfer object for loot table JSON deserialization.
/// </summary>
public class LootTableDto
{
    /// <summary>
    /// Gets or sets the item entries in this loot table.
    /// </summary>
    [JsonPropertyName("entries")]
    public List<LootEntryDto>? Entries { get; set; }

    /// <summary>
    /// Gets or sets the currency drops in this loot table.
    /// </summary>
    [JsonPropertyName("currencyDrops")]
    public List<CurrencyDropDto>? CurrencyDrops { get; set; }
}

/// <summary>
/// Data transfer object for loot entry JSON deserialization.
/// </summary>
public class LootEntryDto
{
    /// <summary>
    /// Gets or sets the ID of the item that can drop.
    /// </summary>
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight for weighted random selection.
    /// </summary>
    [JsonPropertyName("weight")]
    public int Weight { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum quantity to drop.
    /// </summary>
    [JsonPropertyName("minQuantity")]
    public int MinQuantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum quantity to drop.
    /// </summary>
    [JsonPropertyName("maxQuantity")]
    public int MaxQuantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the probability of this item dropping (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("dropChance")]
    public float DropChance { get; set; } = 1.0f;
}

/// <summary>
/// Data transfer object for currency drop JSON deserialization.
/// </summary>
public class CurrencyDropDto
{
    /// <summary>
    /// Gets or sets the currency ID.
    /// </summary>
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = "gold";

    /// <summary>
    /// Gets or sets the minimum amount to drop.
    /// </summary>
    [JsonPropertyName("minAmount")]
    public int MinAmount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum amount to drop.
    /// </summary>
    [JsonPropertyName("maxAmount")]
    public int MaxAmount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the probability of this currency dropping (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("dropChance")]
    public float DropChance { get; set; } = 1.0f;
}
