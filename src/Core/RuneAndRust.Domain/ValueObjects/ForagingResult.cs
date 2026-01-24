using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a foraging attempt, containing all yields,
/// cache discoveries, and metadata about the search.
/// </summary>
/// <remarks>
/// <para>
/// Captures the complete outcome of a foraging attempt, including:
/// <list type="bullet">
///   <item><description>Resource yields (scrap, rations, components)</description></item>
///   <item><description>Hidden cache discoveries and their contents</description></item>
///   <item><description>Biome-specific special items from critical successes</description></item>
///   <item><description>Time spent and roll details for logging</description></item>
/// </list>
/// </para>
/// <para>
/// Success tier progression:
/// <list type="bullet">
///   <item><description>0-1 successes: Nothing found</description></item>
///   <item><description>2-3 successes: Meager haul (scrap only)</description></item>
///   <item><description>4-5 successes: Decent finds (scrap + rations)</description></item>
///   <item><description>6-7 successes: Good haul (scrap + rations + 1 component)</description></item>
///   <item><description>8+ successes: Excellent haul (all resources + biome item)</description></item>
/// </list>
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// var result = foragingService.AttemptForaging(characterId, context);
/// if (result.FoundAnything)
/// {
///     Console.WriteLine($"Found: {result.ScrapYield} scrap, {result.RationsYield} rations");
///     if (result.CacheFound)
///         Console.WriteLine($"Cache: {result.CacheMarks} Marks + {result.CacheItem}");
/// }
/// </code>
/// </para>
/// </remarks>
/// <param name="SuccessLevel">The net successes from the skill check.</param>
/// <param name="ScrapYield">Amount of scrap collected (basic salvage material).</param>
/// <param name="RationsYield">Amount of rations found (consumable supplies).</param>
/// <param name="ComponentsYield">Number of valuable components found (rare materials).</param>
/// <param name="CacheFound">Whether a hidden cache was discovered.</param>
/// <param name="CacheMarks">Marks found in the cache (0 if no cache).</param>
/// <param name="CacheItem">Item found in the cache (null if no cache or no item).</param>
/// <param name="BiomeSpecificItems">Special items unique to this biome (from critical success).</param>
/// <param name="TimeSpent">Actual time spent foraging.</param>
/// <param name="RollDetails">Details about the dice roll for logging and display.</param>
public readonly record struct ForagingResult(
    int SuccessLevel,
    int ScrapYield,
    int RationsYield,
    int ComponentsYield,
    bool CacheFound,
    int CacheMarks,
    string? CacheItem,
    IReadOnlyList<string> BiomeSpecificItems,
    TimeSpan TimeSpent,
    string RollDetails)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES - SUCCESS ANALYSIS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any resources were found.
    /// </summary>
    /// <remarks>
    /// True if any of the following are non-zero: scrap, rations, components,
    /// or if a cache was discovered.
    /// </remarks>
    public bool FoundAnything => ScrapYield > 0 || RationsYield > 0 ||
                                  ComponentsYield > 0 || CacheFound;

    /// <summary>
    /// Gets the success tier description.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Success tiers:
    /// <list type="bullet">
    ///   <item><description>0-1 successes: "Nothing Found"</description></item>
    ///   <item><description>2-3 successes: "Meager Haul"</description></item>
    ///   <item><description>4-5 successes: "Decent Finds"</description></item>
    ///   <item><description>6-7 successes: "Good Haul"</description></item>
    ///   <item><description>8+ successes: "Excellent Haul"</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string SuccessTier => SuccessLevel switch
    {
        <= 1 => "Nothing Found",
        <= 3 => "Meager Haul",
        <= 5 => "Decent Finds",
        <= 7 => "Good Haul",
        _ => "Excellent Haul"
    };

    /// <summary>
    /// Gets whether this was a critical success (5+ net successes).
    /// </summary>
    /// <remarks>
    /// Critical successes may yield biome-specific special items
    /// in addition to normal yields.
    /// </remarks>
    public bool IsCriticalSuccess => SuccessLevel >= 5;

    /// <summary>
    /// Gets whether this was a failure (0-1 net successes).
    /// </summary>
    /// <remarks>
    /// Failures yield nothing, but may still discover a cache
    /// if any 10s were rolled.
    /// </remarks>
    public bool IsFailure => SuccessLevel <= 1;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES - VALUE CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates an estimated total value of the foraging result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Value estimates based on economy balance:
    /// <list type="bullet">
    ///   <item><description>Scrap: 1 Mark each</description></item>
    ///   <item><description>Rations: 5 Marks each</description></item>
    ///   <item><description>Components: 20 Marks each</description></item>
    ///   <item><description>Cache Marks: Face value</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Biome-specific items and cache items are not included in
    /// this estimate as their values vary.
    /// </para>
    /// </remarks>
    public int EstimatedValue => ScrapYield + (RationsYield * 5) +
                                  (ComponentsYield * 20) + CacheMarks;

    /// <summary>
    /// Gets the total number of resources found (excluding cache).
    /// </summary>
    /// <remarks>
    /// Sum of scrap, rations, and components. Useful for quick
    /// comparison of foraging outcomes.
    /// </remarks>
    public int TotalResourceCount => ScrapYield + RationsYield + ComponentsYield;

    /// <summary>
    /// Gets whether any biome-specific items were found.
    /// </summary>
    /// <remarks>
    /// Biome items are found on critical success (5+ net successes).
    /// </remarks>
    public bool HasBiomeItems => BiomeSpecificItems.Count > 0;

    /// <summary>
    /// Gets whether the cache contained a bonus item.
    /// </summary>
    /// <remarks>
    /// Caches have a 50% chance to contain a bonus item in addition to Marks.
    /// </remarks>
    public bool CacheHasItem => CacheItem != null;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES - TIME ANALYSIS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the time spent as minutes.
    /// </summary>
    public int TimeSpentMinutes => (int)TimeSpent.TotalMinutes;

    /// <summary>
    /// Gets the value per hour for this foraging attempt.
    /// </summary>
    /// <remarks>
    /// Calculated as EstimatedValue / (TimeSpent in hours).
    /// Returns 0 if time spent is zero to avoid division by zero.
    /// </remarks>
    public double ValuePerHour => TimeSpent.TotalHours > 0
        ? EstimatedValue / TimeSpent.TotalHours
        : 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string summarizing the foraging result.
    /// </summary>
    /// <returns>A formatted string showing all yields and discoveries.</returns>
    /// <example>
    /// "Decent Finds: 15 scrap, 3 rations (60 min)"
    /// "Good Haul: 22 scrap, 4 rations, 1 component(s), CACHE: 67 Marks + Corroded Medkit (60 min)"
    /// </example>
    public string ToDisplayString()
    {
        var parts = new List<string>();

        if (ScrapYield > 0)
            parts.Add($"{ScrapYield} scrap");
        if (RationsYield > 0)
            parts.Add($"{RationsYield} rations");
        if (ComponentsYield > 0)
            parts.Add($"{ComponentsYield} component(s)");
        if (CacheFound)
            parts.Add($"CACHE: {CacheMarks} Marks" + (CacheItem != null ? $" + {CacheItem}" : ""));
        if (BiomeSpecificItems.Count > 0)
            parts.Add($"Special: {string.Join(", ", BiomeSpecificItems)}");

        if (parts.Count == 0)
            return $"Nothing found ({TimeSpentMinutes} min)";

        return $"{SuccessTier}: {string.Join(", ", parts)} ({TimeSpentMinutes} min)";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var cacheInfo = CacheFound
            ? $"Yes - {CacheMarks} Marks" + (CacheItem != null ? $" + {CacheItem}" : " (no item)")
            : "No";

        var biomeItemsInfo = BiomeSpecificItems.Count > 0
            ? string.Join(", ", BiomeSpecificItems)
            : "None";

        return $"ForagingResult\n" +
               $"  Success Level: {SuccessLevel} ({SuccessTier})\n" +
               $"  Scrap: {ScrapYield}\n" +
               $"  Rations: {RationsYield}\n" +
               $"  Components: {ComponentsYield}\n" +
               $"  Cache Found: {cacheInfo}\n" +
               $"  Biome Items: {biomeItemsInfo}\n" +
               $"  Time Spent: {TimeSpentMinutes} minutes\n" +
               $"  Estimated Value: {EstimatedValue} Marks\n" +
               $"  Value/Hour: {ValuePerHour:F1} Marks/hr\n" +
               $"  Roll: {RollDetails}";
    }

    /// <summary>
    /// Returns a human-readable description of the foraging result.
    /// </summary>
    /// <returns>A formatted string describing the outcome.</returns>
    public override string ToString()
    {
        return ToDisplayString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for a failed foraging attempt (nothing found).
    /// </summary>
    /// <param name="timeSpent">The time spent searching.</param>
    /// <param name="rollDetails">Details about the dice roll.</param>
    /// <returns>An empty ForagingResult with all yields at zero.</returns>
    /// <remarks>
    /// Used when the foraging check fails (0-1 net successes) and no
    /// cache was discovered (no 10s rolled).
    /// </remarks>
    public static ForagingResult Empty(TimeSpan timeSpent, string rollDetails)
    {
        return new ForagingResult(
            SuccessLevel: 0,
            ScrapYield: 0,
            RationsYield: 0,
            ComponentsYield: 0,
            CacheFound: false,
            CacheMarks: 0,
            CacheItem: null,
            BiomeSpecificItems: Array.Empty<string>(),
            TimeSpent: timeSpent,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a result with only a cache discovery (failed check but rolled 10).
    /// </summary>
    /// <param name="cacheMarks">Marks found in the cache.</param>
    /// <param name="cacheItem">Item found in the cache (may be null).</param>
    /// <param name="timeSpent">The time spent searching.</param>
    /// <param name="rollDetails">Details about the dice roll.</param>
    /// <returns>A ForagingResult with only cache yields.</returns>
    /// <remarks>
    /// Even a failed foraging check can discover a cache if any die shows 10.
    /// This represents stumbling upon a hidden stash even when searching ineffectively.
    /// </remarks>
    public static ForagingResult CacheOnly(
        int cacheMarks,
        string? cacheItem,
        TimeSpan timeSpent,
        string rollDetails)
    {
        return new ForagingResult(
            SuccessLevel: 0,
            ScrapYield: 0,
            RationsYield: 0,
            ComponentsYield: 0,
            CacheFound: true,
            CacheMarks: cacheMarks,
            CacheItem: cacheItem,
            BiomeSpecificItems: Array.Empty<string>(),
            TimeSpent: timeSpent,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a basic result with only scrap yield.
    /// </summary>
    /// <param name="successLevel">Net successes from the check.</param>
    /// <param name="scrapYield">Amount of scrap found.</param>
    /// <param name="timeSpent">The time spent searching.</param>
    /// <param name="rollDetails">Details about the dice roll.</param>
    /// <returns>A ForagingResult with scrap only.</returns>
    /// <remarks>
    /// Used for low-success outcomes (2-3 net successes) where only
    /// common salvage is found.
    /// </remarks>
    public static ForagingResult ScrapOnly(
        int successLevel,
        int scrapYield,
        TimeSpan timeSpent,
        string rollDetails)
    {
        return new ForagingResult(
            SuccessLevel: successLevel,
            ScrapYield: scrapYield,
            RationsYield: 0,
            ComponentsYield: 0,
            CacheFound: false,
            CacheMarks: 0,
            CacheItem: null,
            BiomeSpecificItems: Array.Empty<string>(),
            TimeSpent: timeSpent,
            RollDetails: rollDetails);
    }
}
