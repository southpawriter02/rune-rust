namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the contents of a container, including items and currency.
/// </summary>
/// <remarks>
/// <para>
/// This value object is immutable and created by the container loot generation service.
/// Item IDs reference items in the game's item database by their string identifier
/// (e.g., "sword-iron", "potion-health").
/// </para>
/// <para>
/// Use the static <see cref="Empty"/> property for looted or empty containers.
/// Use <see cref="Create"/> for validated content creation, or the convenience
/// factories <see cref="CurrencyOnly"/> and <see cref="ItemsOnly"/> for common cases.
/// </para>
/// </remarks>
/// <param name="ItemIds">Collection of item identifiers in the container.</param>
/// <param name="CurrencyAmount">Amount of currency (gold) in the container. Must be non-negative.</param>
/// <param name="AppliedTier">The quality tier (0-4) applied to generated items.</param>
public readonly record struct ContainerContents(
    IReadOnlyList<string> ItemIds,
    int CurrencyAmount,
    int AppliedTier)
{
    /// <summary>
    /// Gets an empty contents instance representing a looted or empty container.
    /// </summary>
    /// <remarks>
    /// This is a singleton instance returned for:
    /// <list type="bullet">
    /// <item>Containers that have already been looted</item>
    /// <item>Containers that generated no contents</item>
    /// <item>Invalid loot attempts (container not open, etc.)</item>
    /// </list>
    /// </remarks>
    public static ContainerContents Empty { get; } = new(
        Array.Empty<string>(),
        0,
        0);

    /// <summary>
    /// Gets whether this container has any items.
    /// </summary>
    /// <value><c>true</c> if <see cref="ItemIds"/> contains at least one item; otherwise, <c>false</c>.</value>
    public bool HasItems => ItemIds.Count > 0;

    /// <summary>
    /// Gets whether this container has any currency.
    /// </summary>
    /// <value><c>true</c> if <see cref="CurrencyAmount"/> is greater than zero; otherwise, <c>false</c>.</value>
    public bool HasCurrency => CurrencyAmount > 0;

    /// <summary>
    /// Gets whether this container has any contents at all (items or currency).
    /// </summary>
    /// <value><c>true</c> if the container has items or currency; otherwise, <c>false</c>.</value>
    public bool HasContents => HasItems || HasCurrency;

    /// <summary>
    /// Gets the total number of items in the container.
    /// </summary>
    /// <value>The count of items in <see cref="ItemIds"/>.</value>
    public int ItemCount => ItemIds.Count;

    /// <summary>
    /// Creates a new <see cref="ContainerContents"/> instance with validation.
    /// </summary>
    /// <param name="itemIds">Collection of item identifiers. Cannot be null.</param>
    /// <param name="currencyAmount">Amount of currency. Must be non-negative.</param>
    /// <param name="appliedTier">Quality tier applied (0-4). Must be in valid range.</param>
    /// <returns>A validated <see cref="ContainerContents"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="itemIds"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="currencyAmount"/> is negative, or
    /// <paramref name="appliedTier"/> is not in range 0-4.
    /// </exception>
    /// <example>
    /// <code>
    /// var contents = ContainerContents.Create(
    ///     new List&lt;string&gt; { "sword-iron", "potion-health" },
    ///     currencyAmount: 50,
    ///     appliedTier: 2);
    /// </code>
    /// </example>
    public static ContainerContents Create(
        IReadOnlyList<string> itemIds,
        int currencyAmount,
        int appliedTier)
    {
        ArgumentNullException.ThrowIfNull(itemIds, nameof(itemIds));
        ArgumentOutOfRangeException.ThrowIfNegative(currencyAmount, nameof(currencyAmount));
        ArgumentOutOfRangeException.ThrowIfNegative(appliedTier, nameof(appliedTier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(appliedTier, 4, nameof(appliedTier));

        return new ContainerContents(itemIds, currencyAmount, appliedTier);
    }

    /// <summary>
    /// Creates contents with only currency (no items).
    /// </summary>
    /// <param name="amount">Currency amount. Must be non-negative.</param>
    /// <returns>A <see cref="ContainerContents"/> with only currency.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="amount"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var goldOnly = ContainerContents.CurrencyOnly(100);
    /// </code>
    /// </example>
    public static ContainerContents CurrencyOnly(int amount) =>
        Create(Array.Empty<string>(), amount, 0);

    /// <summary>
    /// Creates contents with only items (no currency).
    /// </summary>
    /// <param name="itemIds">Collection of item identifiers. Cannot be null.</param>
    /// <param name="tier">Quality tier applied (0-4).</param>
    /// <returns>A <see cref="ContainerContents"/> with only items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="itemIds"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tier"/> is not in range 0-4.
    /// </exception>
    /// <example>
    /// <code>
    /// var itemsOnly = ContainerContents.ItemsOnly(
    ///     new List&lt;string&gt; { "sword-iron" },
    ///     tier: 2);
    /// </code>
    /// </example>
    public static ContainerContents ItemsOnly(IReadOnlyList<string> itemIds, int tier) =>
        Create(itemIds, 0, tier);

    /// <inheritdoc />
    /// <remarks>
    /// Returns a human-readable summary of the container contents.
    /// </remarks>
    public override string ToString() =>
        $"Contents: {ItemCount} item(s), {CurrencyAmount} gold, Tier {AppliedTier}";
}
