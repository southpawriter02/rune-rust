namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Result of a Myth-Forged generation attempt.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MythForgedResult"/> captures the outcome of attempting to generate a
/// Myth-Forged (unique/legendary) item. The result indicates either success (with
/// the generated item) or failure (with the reason for fallback to normal loot).
/// </para>
/// <para>
/// This value object follows the Result Pattern, providing clear success/failure
/// semantics with associated data. Factory methods enforce invariants - successful
/// results must have an item, and failed results must have a valid fallback reason.
/// </para>
/// <para>
/// The <see cref="AffinityApplied"/> property indicates whether class affinity
/// filtering was applied during selection, which can be useful for logging and
/// analytics to understand drop distribution patterns.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = mythForgedService.TryGenerateMythForged(context);
///
/// if (result.Success)
/// {
///     _logger.LogInformation(
///         "Generated Myth-Forged item {ItemId}, Affinity: {AffinityApplied}, Pool: {PoolSize}",
///         result.Item!.ItemId,
///         result.AffinityApplied,
///         result.PoolSize);
///
///     return result.Item;
/// }
/// else
/// {
///     _logger.LogDebug("Myth-Forged failed: {Reason}", result.FallbackReason);
///     return GenerateTier3Item(context);
/// }
/// </code>
/// </example>
public readonly record struct MythForgedResult
{
    /// <summary>
    /// Gets whether a unique item was successfully generated.
    /// </summary>
    /// <value>
    /// <c>true</c> if a Myth-Forged item was generated and <see cref="Item"/>
    /// contains the result; <c>false</c> if generation failed and fallback is required.
    /// </value>
    public bool Success { get; }

    /// <summary>
    /// Gets the generated unique item, if successful.
    /// </summary>
    /// <value>
    /// The generated <see cref="UniqueItem"/> when <see cref="Success"/> is <c>true</c>;
    /// otherwise, <c>null</c>.
    /// </value>
    /// <remarks>
    /// Always check <see cref="Success"/> before accessing this property,
    /// or use null-conditional operators for safe access.
    /// </remarks>
    public UniqueItem? Item { get; }

    /// <summary>
    /// Gets the reason for fallback, if unsuccessful.
    /// </summary>
    /// <value>
    /// The <see cref="FallbackReason"/> indicating why generation failed when
    /// <see cref="Success"/> is <c>false</c>; <see cref="FallbackReason.None"/>
    /// when successful.
    /// </value>
    public FallbackReason FallbackReason { get; }

    /// <summary>
    /// Gets whether class affinity filtering was applied.
    /// </summary>
    /// <value>
    /// <c>true</c> if the item pool was filtered by player class affinity;
    /// <c>false</c> if the full pool was used.
    /// </value>
    /// <remarks>
    /// <para>
    /// Class affinity filtering has a 60% chance to be applied when a player
    /// class is specified in the context. If applied and matching items exist,
    /// the selection is biased toward class-appropriate gear.
    /// </para>
    /// <para>
    /// This property is always <c>false</c> for failed results.
    /// </para>
    /// </remarks>
    public bool AffinityApplied { get; }

    /// <summary>
    /// Gets the number of items available in the pool before selection.
    /// </summary>
    /// <value>
    /// The count of unique items that were eligible for selection after
    /// filtering. Zero indicates pool exhaustion.
    /// </value>
    /// <remarks>
    /// This value represents the pool size after all filtering (source matching,
    /// already-dropped exclusion, optional affinity filtering) but before
    /// random selection.
    /// </remarks>
    public int PoolSize { get; }

    /// <summary>
    /// Private constructor to enforce factory method usage.
    /// </summary>
    private MythForgedResult(
        bool success,
        UniqueItem? item,
        FallbackReason fallbackReason,
        bool affinityApplied,
        int poolSize)
    {
        Success = success;
        Item = item;
        FallbackReason = fallbackReason;
        AffinityApplied = affinityApplied;
        PoolSize = poolSize;
    }

    /// <summary>
    /// Creates a successful result with a unique item.
    /// </summary>
    /// <param name="item">The generated unique item (cannot be null).</param>
    /// <param name="affinityApplied">Whether class affinity filtering was applied.</param>
    /// <param name="poolSize">The number of items available before selection.</param>
    /// <returns>A successful <see cref="MythForgedResult"/> containing the item.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var selectedItem = availableItems[random.Next(availableItems.Count)];
    /// var result = MythForgedResult.Succeeded(
    ///     selectedItem,
    ///     affinityApplied: true,
    ///     poolSize: availableItems.Count);
    /// </code>
    /// </example>
    public static MythForgedResult Succeeded(
        UniqueItem item,
        bool affinityApplied,
        int poolSize)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        return new MythForgedResult(
            success: true,
            item: item,
            fallbackReason: FallbackReason.None,
            affinityApplied: affinityApplied,
            poolSize: poolSize);
    }

    /// <summary>
    /// Creates a failed result with a fallback reason.
    /// </summary>
    /// <param name="reason">The reason for failure (cannot be <see cref="FallbackReason.None"/>).</param>
    /// <param name="poolSize">The number of items available when failure occurred (default 0).</param>
    /// <returns>A failed <see cref="MythForgedResult"/> with the specified reason.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="reason"/> is <see cref="FallbackReason.None"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Pool exhausted - all uniques already dropped
    /// var exhaustedResult = MythForgedResult.Failed(FallbackReason.PoolExhausted);
    ///
    /// // Drop chance failed with remaining pool items
    /// var failedRollResult = MythForgedResult.Failed(
    ///     FallbackReason.DropChanceFailed,
    ///     poolSize: 5);
    /// </code>
    /// </example>
    public static MythForgedResult Failed(FallbackReason reason, int poolSize = 0)
    {
        if (reason == FallbackReason.None)
        {
            throw new ArgumentException(
                "Failed result must have a fallback reason.", nameof(reason));
        }

        return new MythForgedResult(
            success: false,
            item: null,
            fallbackReason: reason,
            affinityApplied: false,
            poolSize: poolSize);
    }

    /// <summary>
    /// Returns a string representation of the result for debugging.
    /// </summary>
    /// <returns>
    /// A string describing the result, including item ID for success or
    /// fallback reason for failure.
    /// </returns>
    /// <example>
    /// <code>
    /// // Success case
    /// var success = MythForgedResult.Succeeded(item, true, 5);
    /// Console.WriteLine(success.ToString());
    /// // Output: MythForgedResult[Success: shadowfang-blade, Affinity:True, Pool:5]
    ///
    /// // Failure case
    /// var failed = MythForgedResult.Failed(FallbackReason.PoolExhausted);
    /// Console.WriteLine(failed.ToString());
    /// // Output: MythForgedResult[Failed: PoolExhausted, Pool:0]
    /// </code>
    /// </example>
    public override string ToString() =>
        Success
            ? $"MythForgedResult[Success: {Item!.ItemId}, Affinity:{AffinityApplied}, Pool:{PoolSize}]"
            : $"MythForgedResult[Failed: {FallbackReason}, Pool:{PoolSize}]";
}
