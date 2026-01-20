namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a currency type that players can collect and spend.
/// </summary>
/// <remarks>
/// Currency definitions are loaded from configuration. The system supports
/// multiple currencies, though Gold is the default and primary currency.
/// </remarks>
public class CurrencyDefinition
{
    /// <summary>
    /// Gets the unique identifier for this currency.
    /// </summary>
    /// <example>"gold", "souls", "gems"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the singular display name.
    /// </summary>
    /// <example>"Gold", "Soul", "Gem"</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plural display name.
    /// </summary>
    /// <example>"Gold", "Souls", "Gems"</example>
    public string PluralName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the symbol used for compact display.
    /// </summary>
    /// <example>"G", "S", "💎"</example>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Gets the color used for displaying this currency.
    /// </summary>
    public string Color { get; init; } = "yellow";

    /// <summary>
    /// Gets the display sort order.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Private parameterless constructor for deserialization.
    /// </summary>
    private CurrencyDefinition()
    {
    }

    /// <summary>
    /// Creates a validated CurrencyDefinition.
    /// </summary>
    /// <param name="id">The unique identifier for this currency.</param>
    /// <param name="name">The singular display name.</param>
    /// <param name="pluralName">The plural display name.</param>
    /// <param name="symbol">The symbol for compact display.</param>
    /// <param name="color">The display color (default: "yellow").</param>
    /// <param name="sortOrder">The display sort order (default: 0).</param>
    /// <returns>A validated CurrencyDefinition.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or whitespace.</exception>
    public static CurrencyDefinition Create(
        string id,
        string name,
        string pluralName,
        string symbol,
        string color = "yellow",
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluralName);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        return new CurrencyDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            PluralName = pluralName,
            Symbol = symbol,
            Color = string.IsNullOrWhiteSpace(color) ? "yellow" : color,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Gets the appropriate display name based on amount.
    /// </summary>
    /// <param name="amount">The amount of currency.</param>
    /// <returns>Singular or plural name.</returns>
    public string GetDisplayName(int amount)
    {
        return amount == 1 ? Name : PluralName;
    }

    /// <summary>
    /// Formats the currency amount for display.
    /// </summary>
    /// <param name="amount">The amount to format.</param>
    /// <returns>Formatted string like "15 Gold" or "1 Gold".</returns>
    public string FormatAmount(int amount)
    {
        return $"{amount} {GetDisplayName(amount)}";
    }

    /// <summary>
    /// Gets the default Gold currency.
    /// </summary>
    public static CurrencyDefinition Gold => Create(
        "gold", "Gold", "Gold", "G", "yellow", 0);
}
