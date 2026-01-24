using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for foraging operations in the wasteland.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to scavenge resources from the wasteland
/// using the Wasteland Survival skill. Foraging allows characters to gather salvage,
/// supplies, and valuable components with yields based on skill check success.
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Search duration adds bonus dice: Quick (+0), Thorough (+2), Complete (+4)</description></item>
///   <item><description>Success level determines yields from yield table</description></item>
///   <item><description>Rolling any 10 triggers hidden cache discovery</description></item>
///   <item><description>Critical success (5+ net) grants biome-specific items</description></item>
///   <item><description>Area exhaustion reduces effectiveness on repeated searches</description></item>
/// </list>
/// </para>
/// <para>
/// Yield table by success:
/// <list type="bullet">
///   <item><description>0-1: Nothing found</description></item>
///   <item><description>2-3: 2d10 scrap</description></item>
///   <item><description>4-5: 3d10 scrap + 1d6 rations</description></item>
///   <item><description>6-7: 4d10 scrap + 1d6 rations + 1 component</description></item>
///   <item><description>8+: 5d10 scrap + 2d6 rations + 1d4 components</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IForagingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY FORAGING OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to forage for resources in the current area.
    /// </summary>
    /// <param name="player">The character attempting to forage.</param>
    /// <param name="context">The foraging context with search parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// A <see cref="ForagingResult"/> containing all yields, cache discoveries,
    /// and metadata about the foraging attempt.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> or <paramref name="context"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a Wasteland Survival skill check with duration bonus dice,
    /// then calculates yields based on net successes. Checks for cache
    /// discovery if any die shows 10.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var context = ForagingContext.CreateThorough("player-1", "industrial-ruins");
    /// var result = await foragingService.AttemptForagingAsync(player, context);
    /// if (result.FoundAnything)
    /// {
    ///     Console.WriteLine($"Found: {result.ScrapYield} scrap");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    Task<ForagingResult> AttemptForagingAsync(
        Player player,
        ForagingContext context,
        CancellationToken cancellationToken = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // YIELD CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates yields based on the number of successes.
    /// </summary>
    /// <param name="successes">Net successes from the skill check.</param>
    /// <param name="biomeId">The biome being searched (affects potential items).</param>
    /// <returns>Tuple of (scrap, rations, components) yields.</returns>
    /// <remarks>
    /// <para>
    /// Yield table:
    /// <list type="bullet">
    ///   <item><description>0-1: (0, 0, 0)</description></item>
    ///   <item><description>2-3: (2d10, 0, 0)</description></item>
    ///   <item><description>4-5: (3d10, 1d6, 0)</description></item>
    ///   <item><description>6-7: (4d10, 1d6, 1)</description></item>
    ///   <item><description>8+: (5d10, 2d6, 1d4)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    (int Scrap, int Rations, int Components) CalculateYields(int successes, string biomeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHE DISCOVERY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if any dice rolled a 10, triggering cache discovery.
    /// </summary>
    /// <param name="diceRolls">The actual values rolled on each die.</param>
    /// <returns>True if any die shows a 10.</returns>
    /// <remarks>
    /// Cache discovery is independent of success/failure. Even a failed
    /// foraging check can discover a cache if lucky enough to roll a 10.
    /// </remarks>
    bool CheckForCache(IEnumerable<int> diceRolls);

    /// <summary>
    /// Generates cache contents when a cache is discovered.
    /// </summary>
    /// <param name="biomeId">The biome where the cache was found.</param>
    /// <returns>Tuple of (marks, item name or null).</returns>
    /// <remarks>
    /// <para>
    /// Cache contents:
    /// <list type="bullet">
    ///   <item><description>Marks: 1d100 (always present)</description></item>
    ///   <item><description>Item: 50% chance, drawn from biome loot table</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    (int Marks, string? Item) GenerateCacheContents(string biomeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // DURATION AND BONUS INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the bonus dice for a given search duration.
    /// </summary>
    /// <param name="duration">The search duration.</param>
    /// <returns>Number of bonus dice (+0, +2, or +4).</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Quick: +0 bonus dice</description></item>
    ///   <item><description>Thorough: +2 bonus dice</description></item>
    ///   <item><description>Complete: +4 bonus dice</description></item>
    /// </list>
    /// </remarks>
    int GetDurationBonus(SearchDuration duration);

    /// <summary>
    /// Gets the time in minutes for a given search duration.
    /// </summary>
    /// <param name="duration">The search duration.</param>
    /// <returns>Time in minutes (10, 60, or 240).</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Quick: 10 minutes</description></item>
    ///   <item><description>Thorough: 60 minutes (1 hour)</description></item>
    ///   <item><description>Complete: 240 minutes (4 hours)</description></item>
    /// </list>
    /// </remarks>
    int GetDurationTimeMinutes(SearchDuration duration);

    /// <summary>
    /// Gets a human-readable description of a search duration.
    /// </summary>
    /// <param name="duration">The search duration.</param>
    /// <returns>A display name and description for the duration.</returns>
    (string Name, string Description) GetDurationDescription(SearchDuration duration);

    // ═══════════════════════════════════════════════════════════════════════════
    // TARGET INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base DC for a forage target.
    /// </summary>
    /// <param name="target">The forage target.</param>
    /// <returns>The base DC (10, 14, 18, or 22).</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>CommonSalvage: DC 10</description></item>
    ///   <item><description>UsefulSupplies: DC 14</description></item>
    ///   <item><description>ValuableComponents: DC 18</description></item>
    ///   <item><description>HiddenCache: DC 22</description></item>
    /// </list>
    /// </remarks>
    int GetTargetBaseDc(ForageTarget target);

    /// <summary>
    /// Gets a human-readable description of a forage target.
    /// </summary>
    /// <param name="target">The forage target.</param>
    /// <returns>A display name and description for the target.</returns>
    (string Name, string Description) GetTargetDescription(ForageTarget target);

    // ═══════════════════════════════════════════════════════════════════════════
    // BIOME LOOT TABLES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the available biome-specific items for a biome.
    /// </summary>
    /// <param name="biomeId">The biome identifier.</param>
    /// <returns>List of possible special items for this biome.</returns>
    /// <remarks>
    /// Biome items can be found:
    /// <list type="bullet">
    ///   <item><description>On critical success (5+ net successes)</description></item>
    ///   <item><description>In discovered caches (50% chance)</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<string> GetBiomeLootTable(string biomeId);

    /// <summary>
    /// Gets a random item from the biome loot table.
    /// </summary>
    /// <param name="biomeId">The biome identifier.</param>
    /// <returns>A random item name, or null if the biome has no loot table.</returns>
    string? GetRandomBiomeItem(string biomeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // ECONOMY INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the estimated value of resources.
    /// </summary>
    /// <param name="scrap">Amount of scrap.</param>
    /// <param name="rations">Amount of rations.</param>
    /// <param name="components">Number of components.</param>
    /// <returns>Estimated value in Marks.</returns>
    /// <remarks>
    /// Value estimates:
    /// <list type="bullet">
    ///   <item><description>Scrap: 1 Mark each</description></item>
    ///   <item><description>Rations: 5 Marks each</description></item>
    ///   <item><description>Components: 20 Marks each</description></item>
    /// </list>
    /// </remarks>
    int CalculateEstimatedValue(int scrap, int rations, int components);

    /// <summary>
    /// Gets the expected average value per hour for a search duration.
    /// </summary>
    /// <param name="duration">The search duration.</param>
    /// <returns>Average expected value per hour.</returns>
    /// <remarks>
    /// Economy balance estimates:
    /// <list type="bullet">
    ///   <item><description>Quick: ~48 value/hour (efficient but low rare finds)</description></item>
    ///   <item><description>Thorough: ~20 value/hour (balanced)</description></item>
    ///   <item><description>Complete: ~15 value/hour (highest total yields)</description></item>
    /// </list>
    /// </remarks>
    double GetExpectedValuePerHour(SearchDuration duration);
}
