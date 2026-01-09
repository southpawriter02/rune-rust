using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a loot table containing possible item and currency drops.
/// </summary>
/// <remarks>
/// Loot tables are embedded within MonsterDefinition and determine
/// what items and currency drop when a monster is defeated.
/// </remarks>
public class LootTable
{
    /// <summary>
    /// Gets the list of possible item drops.
    /// </summary>
    public IReadOnlyList<LootEntry> Entries { get; init; } = [];

    /// <summary>
    /// Gets the list of possible currency drops.
    /// </summary>
    public IReadOnlyList<CurrencyDrop> CurrencyDrops { get; init; } = [];

    /// <summary>
    /// Gets whether this loot table has any entries.
    /// </summary>
    public bool HasEntries => Entries.Count > 0 || CurrencyDrops.Count > 0;

    /// <summary>
    /// Gets an empty loot table.
    /// </summary>
    public static LootTable Empty => new();

    /// <summary>
    /// Private parameterless constructor for deserialization.
    /// </summary>
    private LootTable()
    {
    }

    /// <summary>
    /// Creates a loot table with the specified entries.
    /// </summary>
    /// <param name="entries">The possible item drops (optional).</param>
    /// <param name="currencyDrops">The possible currency drops (optional).</param>
    /// <returns>A new LootTable instance.</returns>
    public static LootTable Create(
        IEnumerable<LootEntry>? entries = null,
        IEnumerable<CurrencyDrop>? currencyDrops = null)
    {
        return new LootTable
        {
            Entries = entries?.ToList() ?? [],
            CurrencyDrops = currencyDrops?.ToList() ?? []
        };
    }
}
