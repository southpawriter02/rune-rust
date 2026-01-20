using System.Text.Json.Serialization;

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration container for currency definitions loaded from JSON.
/// </summary>
public class CurrencyConfiguration
{
    /// <summary>
    /// Gets or sets the list of currency definitions.
    /// </summary>
    [JsonPropertyName("currencies")]
    public List<CurrencyDefinitionDto> Currencies { get; set; } = [];
}

/// <summary>
/// Data transfer object for currency definition JSON deserialization.
/// </summary>
public class CurrencyDefinitionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this currency.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the singular display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plural display name.
    /// </summary>
    [JsonPropertyName("pluralName")]
    public string PluralName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol for compact display.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display color.
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "yellow";

    /// <summary>
    /// Gets or sets the sort order for display.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; } = 0;
}
