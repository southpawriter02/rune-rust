namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains the result of a smart loot selection operation.
/// </summary>
/// <remarks>
/// <para>
/// SmartLootResult provides detailed information about how an item was selected,
/// including whether it was class-appropriate, the bias roll value, and pool sizes.
/// This metadata enables debugging, statistics tracking, and player feedback.
/// </para>
/// <para>
/// Selection reasons explain the algorithm path taken:
/// - "Class-appropriate bias selection" - Item from filtered pool (60% path succeeded)
/// - "Random selection" - Item from full pool (40% path or no archetype)
/// - "Fallback to random" - Bias succeeded but no class-appropriate items available
/// - "No items available" - Empty result when pool is empty
/// </para>
/// </remarks>
/// <param name="SelectedItem">The selected loot entry, or null if no selection was made.</param>
/// <param name="WasClassAppropriate">True if item was selected from the class-appropriate pool.</param>
/// <param name="BiasRoll">The random roll (0-99) used for bias determination. -1 if not applicable.</param>
/// <param name="FilteredPoolSize">Number of class-appropriate items available.</param>
/// <param name="TotalPoolSize">Total number of items in the original pool.</param>
/// <param name="SelectionReason">Human-readable explanation of selection path.</param>
public readonly record struct SmartLootResult(
    LootEntry? SelectedItem,
    bool WasClassAppropriate,
    int BiasRoll,
    int FilteredPoolSize,
    int TotalPoolSize,
    string SelectionReason)
{
    /// <summary>
    /// Gets an empty result indicating no items were available for selection.
    /// </summary>
    /// <remarks>
    /// Used when the available items pool is empty or all items fail drop chance checks.
    /// </remarks>
    public static SmartLootResult Empty => new(
        SelectedItem: null,
        WasClassAppropriate: false,
        BiasRoll: -1,
        FilteredPoolSize: 0,
        TotalPoolSize: 0,
        SelectionReason: "No items available");

    /// <summary>
    /// Gets a value indicating whether an item was successfully selected.
    /// </summary>
    public bool HasSelection => SelectedItem != null;

    /// <summary>
    /// Gets a value indicating whether the bias roll favored class-appropriate selection.
    /// </summary>
    /// <remarks>
    /// True when roll was in the class-appropriate range (0 to BiasPercentage-1).
    /// This indicates intent to select class-appropriate, even if fallback occurred.
    /// </remarks>
    public bool BiasRollFavoredClass => BiasRoll >= 0 && BiasRoll < 60;

    /// <summary>
    /// Gets the percentage of total items that were class-appropriate.
    /// </summary>
    /// <remarks>
    /// Useful for statistics and understanding how much variety was available.
    /// Returns 0 if TotalPoolSize is 0.
    /// </remarks>
    public double FilteredPoolPercentage =>
        TotalPoolSize > 0 ? (double)FilteredPoolSize / TotalPoolSize * 100 : 0;

    /// <summary>
    /// Creates a result for successful class-appropriate selection.
    /// </summary>
    /// <remarks>
    /// Used when the bias roll succeeded (less than BiasPercentage) and a class-appropriate
    /// item was available and selected.
    /// </remarks>
    /// <param name="item">The selected class-appropriate item.</param>
    /// <param name="biasRoll">The random roll that determined bias selection (0-99).</param>
    /// <param name="filteredPoolSize">Number of class-appropriate items available.</param>
    /// <param name="totalPoolSize">Total number of items in original pool.</param>
    /// <returns>A SmartLootResult indicating class-appropriate selection.</returns>
    public static SmartLootResult CreateClassAppropriate(
        LootEntry item,
        int biasRoll,
        int filteredPoolSize,
        int totalPoolSize) =>
        new(
            SelectedItem: item,
            WasClassAppropriate: true,
            BiasRoll: biasRoll,
            FilteredPoolSize: filteredPoolSize,
            TotalPoolSize: totalPoolSize,
            SelectionReason: "Class-appropriate bias selection");

    /// <summary>
    /// Creates a result for random selection (40% path).
    /// </summary>
    /// <remarks>
    /// Used when the bias roll indicated random selection (greater than or equal to BiasPercentage)
    /// or when no archetype was provided for filtering.
    /// </remarks>
    /// <param name="item">The randomly selected item.</param>
    /// <param name="biasRoll">The random roll that determined random selection (0-99).</param>
    /// <param name="filteredPoolSize">Number of class-appropriate items (may be 0).</param>
    /// <param name="totalPoolSize">Total number of items in original pool.</param>
    /// <returns>A SmartLootResult indicating random selection.</returns>
    public static SmartLootResult CreateRandom(
        LootEntry item,
        int biasRoll,
        int filteredPoolSize,
        int totalPoolSize) =>
        new(
            SelectedItem: item,
            WasClassAppropriate: false,
            BiasRoll: biasRoll,
            FilteredPoolSize: filteredPoolSize,
            TotalPoolSize: totalPoolSize,
            SelectionReason: "Random selection");

    /// <summary>
    /// Creates a result for fallback selection when bias succeeded but no class items available.
    /// </summary>
    /// <remarks>
    /// Used when the bias roll favored class-appropriate selection but the
    /// filtered pool was empty, requiring fallback to random selection.
    /// </remarks>
    /// <param name="item">The randomly selected fallback item.</param>
    /// <param name="biasRoll">The random roll that favored class selection (0-59).</param>
    /// <param name="totalPoolSize">Total number of items in original pool.</param>
    /// <returns>A SmartLootResult indicating fallback selection.</returns>
    public static SmartLootResult CreateFallback(
        LootEntry item,
        int biasRoll,
        int totalPoolSize) =>
        new(
            SelectedItem: item,
            WasClassAppropriate: false,
            BiasRoll: biasRoll,
            FilteredPoolSize: 0,
            TotalPoolSize: totalPoolSize,
            SelectionReason: "Fallback to random (no class-appropriate items)");

    /// <summary>
    /// Creates a result for random-only contexts (no archetype specified).
    /// </summary>
    /// <remarks>
    /// Used when the context has no player archetype, making all selections
    /// inherently random without any class-appropriate filtering.
    /// </remarks>
    /// <param name="item">The randomly selected item.</param>
    /// <param name="totalPoolSize">Total number of items in original pool.</param>
    /// <returns>A SmartLootResult indicating random-only selection.</returns>
    public static SmartLootResult CreateRandomOnly(
        LootEntry item,
        int totalPoolSize) =>
        new(
            SelectedItem: item,
            WasClassAppropriate: false,
            BiasRoll: -1,
            FilteredPoolSize: 0,
            TotalPoolSize: totalPoolSize,
            SelectionReason: "Random-only selection (no archetype)");

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing result details.</returns>
    public override string ToString() =>
        HasSelection
            ? $"SmartLootResult[{SelectedItem!.Value.ItemId}: {SelectionReason}, " +
              $"Roll={BiasRoll}, Filtered={FilteredPoolSize}/{TotalPoolSize}]"
            : $"SmartLootResult[Empty: {SelectionReason}]";
}
