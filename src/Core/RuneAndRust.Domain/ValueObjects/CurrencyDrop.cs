namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a possible currency drop in a loot table.
/// </summary>
public readonly record struct CurrencyDrop
{
    /// <summary>
    /// Gets the ID of the currency to drop.
    /// </summary>
    public string CurrencyId { get; init; }

    /// <summary>
    /// Gets the minimum amount to drop.
    /// </summary>
    public int MinAmount { get; init; }

    /// <summary>
    /// Gets the maximum amount to drop.
    /// </summary>
    public int MaxAmount { get; init; }

    /// <summary>
    /// Gets the chance (0.0-1.0) that this currency drops.
    /// </summary>
    public float DropChance { get; init; }

    /// <summary>
    /// Creates a validated CurrencyDrop.
    /// </summary>
    /// <param name="currencyId">The ID of the currency to drop.</param>
    /// <param name="minAmount">The minimum amount to drop.</param>
    /// <param name="maxAmount">The maximum amount to drop.</param>
    /// <param name="dropChance">The chance (0.0-1.0) that this currency drops (default: 1.0).</param>
    /// <returns>A validated CurrencyDrop.</returns>
    /// <exception cref="ArgumentException">Thrown when currencyId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amounts are invalid.</exception>
    public static CurrencyDrop Create(
        string currencyId,
        int minAmount,
        int maxAmount,
        float dropChance = 1.0f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyId);
        ArgumentOutOfRangeException.ThrowIfNegative(minAmount);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAmount, minAmount);

        return new CurrencyDrop
        {
            CurrencyId = currencyId.ToLowerInvariant(),
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            DropChance = Math.Clamp(dropChance, 0f, 1f)
        };
    }
}
