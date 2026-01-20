namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Quality tiers for crafted items, determined by the crafting roll result.
/// </summary>
/// <remarks>
/// <para>
/// Quality is determined during crafting based on the dice roll and margin above DC:
/// <list type="bullet">
///   <item><description><see cref="Standard"/> - Roll meets DC but margin is less than 5</description></item>
///   <item><description><see cref="Fine"/> - Roll exceeds DC by 5 or more (margin >= 5)</description></item>
///   <item><description><see cref="Masterwork"/> - Roll exceeds DC by 10 or more (margin >= 10)</description></item>
///   <item><description><see cref="Legendary"/> - Natural 20 on the crafting roll</description></item>
/// </list>
/// </para>
/// <para>
/// Higher quality items may have improved stats, special properties, or increased value.
/// The exact bonuses are determined by the item type and game balance rules.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Determine quality based on crafting roll
/// var quality = roll switch
/// {
///     20 => CraftedItemQuality.Legendary,  // Natural 20
///     _ when margin >= 10 => CraftedItemQuality.Masterwork,
///     _ when margin >= 5 => CraftedItemQuality.Fine,
///     _ => CraftedItemQuality.Standard
/// };
/// </code>
/// </example>
public enum CraftedItemQuality
{
    /// <summary>
    /// Standard quality - the baseline for successfully crafted items.
    /// </summary>
    /// <remarks>
    /// Achieved when the crafting roll meets or exceeds the DC but the margin is less than 5.
    /// Standard items have no special bonuses or modifiers.
    /// </remarks>
    Standard = 0,

    /// <summary>
    /// Fine quality - a well-crafted item above standard.
    /// </summary>
    /// <remarks>
    /// Achieved when the crafting roll exceeds the DC by 5 or more (margin >= 5).
    /// Fine items may have minor stat improvements or cosmetic enhancements.
    /// </remarks>
    Fine = 1,

    /// <summary>
    /// Masterwork quality - an exceptional piece of craftsmanship.
    /// </summary>
    /// <remarks>
    /// Achieved when the crafting roll exceeds the DC by 10 or more (margin >= 10).
    /// Masterwork items have significant stat improvements and are highly valued.
    /// </remarks>
    Masterwork = 2,

    /// <summary>
    /// Legendary quality - a once-in-a-lifetime creation.
    /// </summary>
    /// <remarks>
    /// Achieved only with a natural 20 on the crafting roll.
    /// Legendary items may have unique properties, magical enhancements,
    /// or exceptional stats. These items are extremely rare and valuable.
    /// </remarks>
    Legendary = 3
}
