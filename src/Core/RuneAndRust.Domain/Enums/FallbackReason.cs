namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Reasons why Myth-Forged generation failed and fell back to normal loot.
/// </summary>
/// <remarks>
/// <para>
/// When a Tier 4 (Legendary) quality roll occurs, the loot system attempts to generate
/// a Myth-Forged unique item. If this attempt fails, the <see cref="FallbackReason"/>
/// indicates why, allowing the system to fall back to generating a Tier 3 (Optimized) item.
/// </para>
/// <para>
/// Reason values are explicitly assigned (0-4) to ensure stable serialization
/// and database storage. New reasons should be added at the end if needed.
/// </para>
/// <para>
/// The <see cref="None"/> value indicates successful generation - no fallback occurred.
/// All other values indicate failure conditions requiring fallback behavior.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = mythForgedService.TryGenerateMythForged(context);
/// if (!result.Success)
/// {
///     _logger.LogDebug("Myth-Forged failed: {Reason}", result.FallbackReason);
///     switch (result.FallbackReason)
///     {
///         case FallbackReason.PoolExhausted:
///             // All uniques already dropped - generate Tier 3
///             break;
///         case FallbackReason.DropChanceFailed:
///             // RNG failed the drop check - generate Tier 3
///             break;
///     }
/// }
/// </code>
/// </example>
public enum FallbackReason
{
    /// <summary>
    /// No fallback occurred - successful Myth-Forged drop.
    /// </summary>
    /// <remarks>
    /// This value is only present in successful <see cref="ValueObjects.MythForgedResult"/>
    /// instances. A failed result will always have a non-zero reason.
    /// </remarks>
    None = 0,

    /// <summary>
    /// All available unique items have already dropped this run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Entities.UniqueItemRegistry"/> tracks which unique items have
    /// already been obtained. When all eligible items for a source have dropped,
    /// the pool is exhausted and no more Myth-Forged items can generate from that source.
    /// </para>
    /// <para>
    /// This maintains the "legendary" status of unique items by preventing duplicates
    /// within a single run.
    /// </para>
    /// </remarks>
    PoolExhausted = 1,

    /// <summary>
    /// No unique items are configured for this source.
    /// </summary>
    /// <remarks>
    /// Some drop sources may not have any unique items configured to drop from them.
    /// For example, a standard container might not have any legendary items in its
    /// drop table, even if the tier roll succeeded.
    /// </remarks>
    NoMatchingSource = 2,

    /// <summary>
    /// Random drop chance roll failed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Even after a Tier 4 quality roll, unique items have an additional source-based
    /// drop chance that must be passed:
    /// </para>
    /// <list type="bullet">
    /// <item>Boss: 70% chance</item>
    /// <item>Elite: 10% chance</item>
    /// <item>Boss Chest: 50% chance</item>
    /// <item>Hidden Cache: 15% chance</item>
    /// <item>Quest Reward: 100% chance</item>
    /// </list>
    /// <para>
    /// When this roll fails, the system falls back to Tier 3 generation.
    /// </para>
    /// </remarks>
    DropChanceFailed = 3,

    /// <summary>
    /// Item tier doesn't qualify for Myth-Forged (must be tier 4).
    /// </summary>
    /// <remarks>
    /// Myth-Forged generation is only attempted when the quality tier roll produces
    /// Tier 4 (Legendary). Lower tier rolls bypass Myth-Forged entirely and generate
    /// standard tiered items.
    /// </remarks>
    TierTooLow = 4
}
