namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for generating Myth-Forged (unique) items during loot generation.
/// Coordinates with the unique item registry for duplicate prevention.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IMythForgedService"/> is the primary interface for attempting to generate
/// legendary unique items when a Tier 4 quality roll occurs. The service encapsulates
/// the complete Myth-Forged generation logic:
/// </para>
/// <list type="number">
/// <item>Checking if the tier qualifies for Myth-Forged generation (must be Tier 4)</item>
/// <item>Validating the source-based drop chance</item>
/// <item>Retrieving available unique items (excluding already-dropped)</item>
/// <item>Applying class affinity filtering (60% bias)</item>
/// <item>Registering successful drops in the <see cref="IUniqueItemRegistry"/></item>
/// </list>
/// <para>
/// When generation fails for any reason (pool exhausted, drop chance failed, etc.),
/// the result indicates the specific <see cref="FallbackReason"/>, allowing the
/// loot system to fall back to generating a Tier 3 (Optimized) item.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ContainerLootGenerator
/// {
///     private readonly IMythForgedService _mythForgedService;
///     private readonly ISmartLootService _smartLootService;
///
///     public LootDrop GenerateLoot(Container container, Player player)
///     {
///         var tier = RollQualityTier();
///
///         if (_mythForgedService.ShouldAttemptMythForged(container.SourceType, tier))
///         {
///             var context = MythForgedContext.Create(
///                 container.SourceType,
///                 container.Id,
///                 player.ClassId,
///                 player.Level);
///
///             var result = _mythForgedService.TryGenerateMythForged(context);
///
///             if (result.Success)
///             {
///                 return LootDrop.FromUniqueItem(result.Item!);
///             }
///
///             // Fallback to Tier 3
///             tier = 3;
///         }
///
///         return _smartLootService.GenerateTieredItem(tier);
///     }
/// }
/// </code>
/// </example>
public interface IMythForgedService
{
    /// <summary>
    /// Attempts to generate a Myth-Forged item for the given context.
    /// </summary>
    /// <param name="context">Generation context with source and player info.</param>
    /// <returns>
    /// A <see cref="MythForgedResult"/> containing either the generated
    /// <see cref="UniqueItem"/> or the <see cref="FallbackReason"/> for failure.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the complete Myth-Forged generation flow:
    /// </para>
    /// <list type="number">
    /// <item>Roll against the source-based drop chance</item>
    /// <item>Retrieve available uniques from the registry</item>
    /// <item>Apply class affinity filter (60% chance when class is specified)</item>
    /// <item>Select a random item from the filtered pool</item>
    /// <item>Register the drop in the <see cref="IUniqueItemRegistry"/></item>
    /// </list>
    /// <para>
    /// If generation fails at any step, the result indicates the specific
    /// <see cref="FallbackReason"/> to guide fallback behavior.
    /// </para>
    /// <para>
    /// When the context includes a <see cref="MythForgedContext.RandomSeed"/>,
    /// a seeded random number generator is used for deterministic testing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = MythForgedContext.Create(
    ///     DropSourceType.Boss,
    ///     "shadow-lord",
    ///     "warrior",
    ///     10);
    ///
    /// var result = mythForgedService.TryGenerateMythForged(context);
    ///
    /// if (result.Success)
    /// {
    ///     _logger.LogInformation(
    ///         "Generated Myth-Forged item {ItemId} from {SourceType}:{SourceId}",
    ///         result.Item!.ItemId,
    ///         context.SourceType,
    ///         context.SourceId);
    ///
    ///     return result.Item;
    /// }
    ///
    /// _logger.LogDebug("Myth-Forged failed: {Reason}", result.FallbackReason);
    /// return GenerateTier3Fallback();
    /// </code>
    /// </example>
    MythForgedResult TryGenerateMythForged(MythForgedContext context);

    /// <summary>
    /// Gets the drop chance for a given source type.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <returns>
    /// Drop chance as a decimal from 0.0 to 1.0, where 1.0 represents 100%
    /// guaranteed drop and 0.0 represents no chance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Drop chances are loaded from configuration and vary by source type:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Source Type</term>
    /// <description>Drop Chance</description>
    /// </listheader>
    /// <item><term>Boss</term><description>70% (0.70)</description></item>
    /// <item><term>Elite</term><description>10% (0.10)</description></item>
    /// <item><term>Boss Chest</term><description>50% (0.50)</description></item>
    /// <item><term>Hidden Cache</term><description>15% (0.15)</description></item>
    /// <item><term>Quest Reward</term><description>100% (1.00)</description></item>
    /// <item><term>Monster</term><description>1% (0.01)</description></item>
    /// <item><term>Container</term><description>2% (0.02)</description></item>
    /// <item><term>Vendor</term><description>0% (0.00)</description></item>
    /// </list>
    /// <para>
    /// A source type not found in configuration returns 0.0.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var chance = mythForgedService.GetDropChance(DropSourceType.Boss);
    /// _logger.LogDebug("Boss Myth-Forged drop chance: {Chance:P0}", chance);
    /// // Output: "Boss Myth-Forged drop chance: 70%"
    /// </code>
    /// </example>
    decimal GetDropChance(DropSourceType sourceType);

    /// <summary>
    /// Determines if a Myth-Forged attempt should be made.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="tier">The rolled quality tier.</param>
    /// <returns>
    /// <c>true</c> if both conditions are met: tier is 4 (Legendary) and
    /// the source type has a non-zero drop chance; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Call this method before attempting Myth-Forged generation to avoid
    /// unnecessary processing. The method checks:
    /// </para>
    /// <list type="number">
    /// <item>Is the tier at least the configured minimum (default: 4)?</item>
    /// <item>Does the source type have a configured drop chance greater than 0?</item>
    /// </list>
    /// <para>
    /// If this returns <c>false</c>, proceed directly with normal tiered
    /// item generation without calling <see cref="TryGenerateMythForged"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier = rollQualityTier();
    ///
    /// if (mythForgedService.ShouldAttemptMythForged(DropSourceType.Boss, tier))
    /// {
    ///     // Attempt Myth-Forged generation
    ///     var result = mythForgedService.TryGenerateMythForged(context);
    ///     // ... handle result
    /// }
    /// else
    /// {
    ///     // Generate normal tiered item
    ///     return generateTieredItem(tier);
    /// }
    /// </code>
    /// </example>
    bool ShouldAttemptMythForged(DropSourceType sourceType, int tier);

    /// <summary>
    /// Gets available unique items for a source (excludes already-dropped).
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">Specific source identifier.</param>
    /// <param name="classId">Optional class for affinity info (not filtered, just context).</param>
    /// <returns>
    /// A read-only list of <see cref="UniqueItem"/> instances that are eligible
    /// to drop from this source and have not yet dropped this run.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides visibility into the available unique pool without
    /// performing generation. It applies the following filters:
    /// </para>
    /// <list type="number">
    /// <item>Items must have a <see cref="DropSource"/> matching the source type and ID</item>
    /// <item>Items must not have already dropped this run (per the registry)</item>
    /// <item>Class affinity is NOT applied - this shows the full eligible pool</item>
    /// </list>
    /// <para>
    /// Useful for debugging, analytics, or displaying "possible drops" to players.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var available = mythForgedService.GetAvailableForSource(
    ///     DropSourceType.Boss,
    ///     "shadow-lord",
    ///     "warrior");
    ///
    /// _logger.LogDebug(
    ///     "Boss {BossId} has {Count} possible unique drops",
    ///     "shadow-lord",
    ///     available.Count);
    /// </code>
    /// </example>
    IReadOnlyList<UniqueItem> GetAvailableForSource(
        DropSourceType sourceType,
        string sourceId,
        string? classId = null);

    /// <summary>
    /// Gets the configured affinity bias percentage.
    /// </summary>
    /// <value>
    /// The percentage chance (0-100) that class affinity filtering will be
    /// applied during Myth-Forged generation when a player class is specified.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="TryGenerateMythForged"/> is called with a
    /// <see cref="MythForgedContext.PlayerClassId"/>, there is a configurable
    /// chance (default 60%) that the item pool will be filtered to only
    /// include items with affinity for that class.
    /// </para>
    /// <para>
    /// This biases drops toward class-appropriate gear while still allowing
    /// universal items to drop. If the filtered pool would be empty, the
    /// full pool is used instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// _logger.LogDebug(
    ///     "Class affinity bias: {Bias}%",
    ///     mythForgedService.AffinityBiasPercent);
    /// // Output: "Class affinity bias: 60%"
    /// </code>
    /// </example>
    decimal AffinityBiasPercent { get; }
}
