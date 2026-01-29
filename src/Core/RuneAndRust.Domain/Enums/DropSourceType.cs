namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of sources from which unique items can drop.
/// </summary>
/// <remarks>
/// <para>
/// Drop sources categorize where Myth-Forged (unique) items can be obtained.
/// Each unique item can have multiple drop sources with different drop chances.
/// </para>
/// <para>
/// Source types affect drop probability calculations and are used by the
/// <see cref="ValueObjects.DropSource"/> value object to specify valid
/// acquisition locations for unique items.
/// </para>
/// <para>
/// Type values are explicitly assigned (0-4) to ensure stable serialization
/// and database storage. New types should be added at the end if needed.
/// </para>
/// </remarks>
public enum DropSourceType
{
    /// <summary>
    /// Standard monster drops from regular enemies.
    /// </summary>
    /// <remarks>
    /// Monster drops typically have low drop chances for unique items,
    /// usually less than 1% for non-boss enemies.
    /// </remarks>
    Monster = 0,

    /// <summary>
    /// Container loot from chests, crates, and other interactable objects.
    /// </summary>
    /// <remarks>
    /// Containers include small chests, medium chests, large chests, and
    /// hidden caches. Drop chances vary by container type and quality tier.
    /// </remarks>
    Container = 1,

    /// <summary>
    /// Boss-specific drops with significantly higher drop rates.
    /// </summary>
    /// <remarks>
    /// Bosses have the highest chance of dropping unique items, typically
    /// 2-10% depending on the boss tier and unique item rarity.
    /// </remarks>
    Boss = 2,

    /// <summary>
    /// Items purchasable from vendor NPCs.
    /// </summary>
    /// <remarks>
    /// Vendor-source unique items are available for purchase, typically
    /// with a 100% acquisition rate once requirements are met (currency, reputation).
    /// </remarks>
    Vendor = 3,

    /// <summary>
    /// Quest reward items guaranteed upon completion.
    /// </summary>
    /// <remarks>
    /// Quest rewards have a 100% drop chance when the associated quest
    /// is completed. These are typically story-significant unique items.
    /// </remarks>
    Quest = 4
}
