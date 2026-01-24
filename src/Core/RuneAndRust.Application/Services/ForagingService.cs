using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for foraging operations in the wasteland.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Foraging System mechanics for the Wasteland Survival skill.
/// Allows characters to scavenge resources from the wasteland with yields based
/// on skill check success and search duration.
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
public class ForagingService : IForagingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - SKILL IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in foraging checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - DURATION BONUSES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Bonus dice for a quick search (10 minutes).
    /// </summary>
    private const int QuickSearchBonus = 0;

    /// <summary>
    /// Bonus dice for a thorough search (1 hour).
    /// </summary>
    private const int ThoroughSearchBonus = 2;

    /// <summary>
    /// Bonus dice for a complete search (4 hours).
    /// </summary>
    private const int CompleteSearchBonus = 4;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - TIME IN MINUTES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Time in minutes for a quick search.
    /// </summary>
    private const int QuickSearchTimeMinutes = 10;

    /// <summary>
    /// Time in minutes for a thorough search.
    /// </summary>
    private const int ThoroughSearchTimeMinutes = 60;

    /// <summary>
    /// Time in minutes for a complete search.
    /// </summary>
    private const int CompleteSearchTimeMinutes = 240;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - FORAGE TARGET DCS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base DC for common salvage.
    /// </summary>
    private const int CommonSalvageDc = 10;

    /// <summary>
    /// Base DC for useful supplies.
    /// </summary>
    private const int UsefulSuppliesDc = 14;

    /// <summary>
    /// Base DC for valuable components.
    /// </summary>
    private const int ValuableComponentsDc = 18;

    /// <summary>
    /// Base DC for hidden cache.
    /// </summary>
    private const int HiddenCacheDc = 22;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - ECONOMY VALUES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Value per unit of scrap in Marks.
    /// </summary>
    private const int ScrapValuePerUnit = 1;

    /// <summary>
    /// Value per unit of rations in Marks.
    /// </summary>
    private const int RationsValuePerUnit = 5;

    /// <summary>
    /// Value per unit of components in Marks.
    /// </summary>
    private const int ComponentsValuePerUnit = 20;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - CACHE DISCOVERY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Die value that triggers cache discovery.
    /// </summary>
    private const int CacheTriggerValue = 10;

    /// <summary>
    /// Probability of finding a bonus item in a cache (50%).
    /// </summary>
    private const double CacheItemProbability = 0.5;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - CRITICAL SUCCESS THRESHOLD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Net successes required for critical success (biome item bonus).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // YIELD TABLE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Yield table: successes → (scrap dice count, ration dice count, component mode).
    /// Component mode: 0 = none, positive = fixed count, -1 = roll 1d4.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yield breakdown by success tier:
    /// <list type="bullet">
    ///   <item><description>0-1 successes: Nothing found</description></item>
    ///   <item><description>2-3 successes: 2d10 scrap</description></item>
    ///   <item><description>4-5 successes: 3d10 scrap, 1d6 rations</description></item>
    ///   <item><description>6-7 successes: 4d10 scrap, 1d6 rations, 1 component</description></item>
    ///   <item><description>8+ successes: 5d10 scrap, 2d6 rations, 1d4 components</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static readonly Dictionary<int, (int ScrapDice, int RationDice, int ComponentMode)> YieldTable = new()
    {
        { 0, (0, 0, 0) },   // Nothing found
        { 1, (0, 0, 0) },   // Nothing found
        { 2, (2, 0, 0) },   // 2d10 scrap
        { 3, (2, 0, 0) },   // 2d10 scrap
        { 4, (3, 1, 0) },   // 3d10 scrap, 1d6 rations
        { 5, (3, 1, 0) },   // 3d10 scrap, 1d6 rations
        { 6, (4, 1, 1) },   // 4d10 scrap, 1d6 rations, 1 component
        { 7, (4, 1, 1) },   // 4d10 scrap, 1d6 rations, 1 component
        { 8, (5, 2, -1) },  // 5d10 scrap, 2d6 rations, 1d4 components
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // BIOME LOOT TABLES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Biome-specific loot tables for special items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Items are available through:
    /// <list type="bullet">
    ///   <item><description>Critical success (5+ net successes)</description></item>
    ///   <item><description>Cache discovery (50% chance of item)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static readonly Dictionary<string, List<string>> BiomeLootTables = new()
    {
        {
            "industrial-ruins", new List<string>
            {
                "Corroded Medkit",
                "Rusty Toolkit",
                "Fuel Canister",
                "Wire Spool"
            }
        },
        {
            "residential-ruins", new List<string>
            {
                "Canned Food",
                "Tattered Map",
                "Old Currency",
                "Faded Photograph"
            }
        },
        {
            "wasteland", new List<string>
            {
                "Mutant Bone",
                "Glowing Crystal",
                "Petrified Wood",
                "Strange Ore"
            }
        },
        {
            "swamp", new List<string>
            {
                "Medicinal Moss",
                "Poison Gland",
                "Bog Iron",
                "Reed Bundle"
            }
        },
        {
            "forest", new List<string>
            {
                "Edible Mushrooms",
                "Medicinal Herbs",
                "Animal Hide",
                "Sturdy Branch"
            }
        },
        {
            "default", new List<string>
            {
                "Mysterious Object",
                "Salvageable Scrap",
                "Strange Device"
            }
        }
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly DiceService _diceService;
    private readonly ILogger<ForagingService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the ForagingService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public ForagingService(
        SkillCheckService skillCheckService,
        DiceService diceService,
        ILogger<ForagingService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("ForagingService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY FORAGING OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public Task<ForagingResult> AttemptForagingAsync(
        Player player,
        ForagingContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogInformation(
            "Attempting foraging for CharacterId={CharacterId}, Duration={Duration}, Target={Target}, Biome={Biome}",
            context.CharacterId, context.SearchDuration, context.ForageTarget, context.BiomeId);

        _logger.LogDebug(
            "Foraging context details: EquipmentBonus={EquipmentBonus}, PreviousSearches={PreviousSearches}, TotalModifier={TotalModifier}",
            context.EquipmentBonus, context.PreviousSearches, context.TotalDiceModifier);

        // Build skill context and perform check
        var skillContext = context.ToSkillContext();
        var baseDc = context.BaseDc;

        _logger.LogDebug(
            "Performing Wasteland Survival check: BaseDC={BaseDC}, DiceModifier={DiceModifier}",
            baseDc, context.TotalDiceModifier);

        // Perform the foraging check
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            baseDc,
            $"Foraging ({context.SearchDuration}) for {context.ForageTarget}",
            skillContext);

        var netSuccesses = checkResult.NetSuccesses;
        var diceRolls = checkResult.DiceResult.Rolls;

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}, Rolls=[{Rolls}]",
            netSuccesses, checkResult.Outcome, string.Join(", ", diceRolls));

        // Check for cache discovery (any 10 in the roll)
        var cacheFound = CheckForCache(diceRolls);
        var cacheMarks = 0;
        string? cacheItem = null;

        if (cacheFound)
        {
            (cacheMarks, cacheItem) = GenerateCacheContents(context.BiomeId);

            _logger.LogInformation(
                "Hidden cache discovered! CharacterId={CharacterId}, Marks={Marks}, Item={Item}",
                context.CharacterId, cacheMarks, cacheItem ?? "none");
        }

        // Calculate yields based on successes
        var (scrap, rations, components) = CalculateYields(netSuccesses, context.BiomeId);

        _logger.LogDebug(
            "Calculated yields: Scrap={Scrap}, Rations={Rations}, Components={Components}",
            scrap, rations, components);

        // Check for biome-specific items on critical success
        var biomeItems = new List<string>();
        if (netSuccesses >= CriticalSuccessThreshold)
        {
            var biomeItem = GetRandomBiomeItem(context.BiomeId);
            if (biomeItem != null)
            {
                biomeItems.Add(biomeItem);
                _logger.LogInformation(
                    "Critical success! Found biome-specific item: {Item} in {Biome}",
                    biomeItem, context.BiomeId);
            }
        }

        // Build roll details string
        var rollDetails = BuildRollDetails(checkResult, context);

        // Create and return result
        var result = new ForagingResult(
            SuccessLevel: netSuccesses,
            ScrapYield: scrap,
            RationsYield: rations,
            ComponentsYield: components,
            CacheFound: cacheFound,
            CacheMarks: cacheMarks,
            CacheItem: cacheItem,
            BiomeSpecificItems: biomeItems.AsReadOnly(),
            TimeSpent: context.TimeSpan,
            RollDetails: rollDetails);

        _logger.LogInformation(
            "Foraging complete for CharacterId={CharacterId}: {ResultDisplay}",
            context.CharacterId, result.ToDisplayString());

        return Task.FromResult(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // YIELD CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public (int Scrap, int Rations, int Components) CalculateYields(int successes, string biomeId)
    {
        // Clamp successes to yield table range (0-8, with 8 being the maximum tier)
        var tableKey = Math.Min(Math.Max(successes, 0), 8);

        // Get yield parameters from table
        var (scrapDice, rationDice, componentMode) = YieldTable[tableKey];

        _logger.LogDebug(
            "Yield table lookup: Successes={Successes} (clamped to {TableKey}), ScrapDice={ScrapDice}, RationDice={RationDice}, ComponentMode={ComponentMode}",
            successes, tableKey, scrapDice, rationDice, componentMode);

        // Roll for scrap (d10s)
        var scrap = 0;
        if (scrapDice > 0)
        {
            scrap = _diceService.RollTotal($"{scrapDice}d10");
            _logger.LogDebug("Scrap roll: {Dice}d10 = {Result}", scrapDice, scrap);
        }

        // Roll for rations (d6s)
        var rations = 0;
        if (rationDice > 0)
        {
            rations = _diceService.RollTotal($"{rationDice}d6");
            _logger.LogDebug("Rations roll: {Dice}d6 = {Result}", rationDice, rations);
        }

        // Determine components (fixed count or roll 1d4)
        var components = componentMode switch
        {
            -1 => _diceService.RollTotal("1d4"),  // Roll 1d4 for top tier
            > 0 => componentMode,                  // Fixed count
            _ => 0                                 // No components
        };

        if (componentMode == -1)
        {
            _logger.LogDebug("Components roll: 1d4 = {Result}", components);
        }

        return (scrap, rations, components);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHE DISCOVERY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CheckForCache(IEnumerable<int> diceRolls)
    {
        var hasTen = diceRolls.Any(roll => roll == CacheTriggerValue);

        _logger.LogDebug(
            "Cache check: Rolls=[{Rolls}], TriggerValue={TriggerValue}, CacheFound={Found}",
            string.Join(", ", diceRolls), CacheTriggerValue, hasTen);

        return hasTen;
    }

    /// <inheritdoc/>
    public (int Marks, string? Item) GenerateCacheContents(string biomeId)
    {
        // Roll 1d100 for Marks (always present in cache)
        // Uses percentile dice (2d10) since DicePool doesn't support d100
        var marks = RollPercentile();

        _logger.LogDebug("Cache marks roll: percentile = {Marks}", marks);

        // 50% chance for a bonus item
        var itemRoll = RollPercentile();
        var hasItem = itemRoll <= (CacheItemProbability * 100);

        _logger.LogDebug(
            "Cache item check: Roll={Roll}, Threshold={Threshold}, HasItem={HasItem}",
            itemRoll, CacheItemProbability * 100, hasItem);

        string? item = null;
        if (hasItem)
        {
            item = GetRandomBiomeItem(biomeId);

            if (item != null)
            {
                _logger.LogDebug("Cache contains bonus item: {Item}", item);
            }
        }

        return (marks, item);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DURATION AND BONUS INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetDurationBonus(SearchDuration duration)
    {
        var bonus = duration switch
        {
            SearchDuration.Quick => QuickSearchBonus,
            SearchDuration.Thorough => ThoroughSearchBonus,
            SearchDuration.Complete => CompleteSearchBonus,
            _ => QuickSearchBonus
        };

        _logger.LogDebug("Duration bonus for {Duration}: +{Bonus} dice", duration, bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetDurationTimeMinutes(SearchDuration duration)
    {
        return duration switch
        {
            SearchDuration.Quick => QuickSearchTimeMinutes,
            SearchDuration.Thorough => ThoroughSearchTimeMinutes,
            SearchDuration.Complete => CompleteSearchTimeMinutes,
            _ => QuickSearchTimeMinutes
        };
    }

    /// <inheritdoc/>
    public (string Name, string Description) GetDurationDescription(SearchDuration duration)
    {
        return duration switch
        {
            SearchDuration.Quick => (
                "Quick Search",
                "A fast 10-minute sweep of the immediate area. No bonus dice, but time-efficient."),
            SearchDuration.Thorough => (
                "Thorough Search",
                "A careful 1-hour search of the surrounding area. +2 bonus dice."),
            SearchDuration.Complete => (
                "Complete Search",
                "An exhaustive 4-hour search of all accessible areas. +4 bonus dice."),
            _ => ("Unknown", "Unknown search duration.")
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TARGET INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetTargetBaseDc(ForageTarget target)
    {
        return target switch
        {
            ForageTarget.CommonSalvage => CommonSalvageDc,
            ForageTarget.UsefulSupplies => UsefulSuppliesDc,
            ForageTarget.ValuableComponents => ValuableComponentsDc,
            ForageTarget.HiddenCache => HiddenCacheDc,
            _ => CommonSalvageDc
        };
    }

    /// <inheritdoc/>
    public (string Name, string Description) GetTargetDescription(ForageTarget target)
    {
        return target switch
        {
            ForageTarget.CommonSalvage => (
                "Common Salvage",
                "Search for scrap metal, wiring, and debris. DC 10, yields scrap."),
            ForageTarget.UsefulSupplies => (
                "Useful Supplies",
                "Search for rations, water, and basic supplies. DC 14, yields rations."),
            ForageTarget.ValuableComponents => (
                "Valuable Components",
                "Search for tech parts and rare materials. DC 18, yields components."),
            ForageTarget.HiddenCache => (
                "Hidden Cache",
                "Search specifically for hidden caches. DC 22, yields Marks and items."),
            _ => ("Unknown", "Unknown forage target.")
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BIOME LOOT TABLES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<string> GetBiomeLootTable(string biomeId)
    {
        var normalizedBiomeId = biomeId.ToLowerInvariant();

        if (BiomeLootTables.TryGetValue(normalizedBiomeId, out var table))
        {
            _logger.LogDebug(
                "Retrieved loot table for biome '{Biome}': {Count} items",
                biomeId, table.Count);
            return table.AsReadOnly();
        }

        _logger.LogWarning(
            "Unknown biome '{Biome}', using default loot table",
            biomeId);

        return BiomeLootTables["default"].AsReadOnly();
    }

    /// <inheritdoc/>
    public string? GetRandomBiomeItem(string biomeId)
    {
        var lootTable = GetBiomeLootTable(biomeId);

        if (lootTable.Count == 0)
        {
            _logger.LogDebug("Empty loot table for biome '{Biome}'", biomeId);
            return null;
        }

        // Roll to select random item from table
        var itemIndex = _diceService.RollTotal($"1d{lootTable.Count}") - 1;
        var selectedItem = lootTable[itemIndex];

        _logger.LogDebug(
            "Selected random item from biome '{Biome}': {Item} (index {Index} of {Count})",
            biomeId, selectedItem, itemIndex, lootTable.Count);

        return selectedItem;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ECONOMY INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int CalculateEstimatedValue(int scrap, int rations, int components)
    {
        var value = (scrap * ScrapValuePerUnit) +
                    (rations * RationsValuePerUnit) +
                    (components * ComponentsValuePerUnit);

        _logger.LogDebug(
            "Estimated value calculation: {Scrap} scrap × {ScrapValue} + {Rations} rations × {RationsValue} + {Components} components × {ComponentsValue} = {Total} Marks",
            scrap, ScrapValuePerUnit, rations, RationsValuePerUnit, components, ComponentsValuePerUnit, value);

        return value;
    }

    /// <inheritdoc/>
    public double GetExpectedValuePerHour(SearchDuration duration)
    {
        // Economy balance estimates based on average success rates and yields
        // These are approximations for player guidance
        return duration switch
        {
            SearchDuration.Quick => 48.0,     // ~8 value per 10 min = 48/hour (efficient but low rare finds)
            SearchDuration.Thorough => 20.0,  // ~20 value per hour (balanced)
            SearchDuration.Complete => 15.0,  // ~60 value per 4 hours = 15/hour (highest total yields)
            _ => 20.0
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Rolls a d100 (percentile dice) using two d10 rolls.
    /// </summary>
    /// <returns>A value from 1 to 100.</returns>
    /// <remarks>
    /// Uses two d10 rolls to simulate a d100:
    /// - First d10 (minus 1, times 10) gives 0, 10, 20... 90
    /// - Second d10 gives 1-10 (where 10 becomes 0 for the ones place)
    /// - Special case: 0 + 0 = 100
    /// </remarks>
    private int RollPercentile()
    {
        // Roll two d10s
        var tensResult = _diceService.Roll("1d10");
        var onesResult = _diceService.Roll("1d10");

        var tensValue = tensResult.Total;
        var onesValue = onesResult.Total;

        // Convert to percentile (tens die: 1-10 becomes 0-90, ones die: 1-10 stays as 1-10)
        var tens = (tensValue == 10 ? 0 : tensValue) * 10;
        var ones = onesValue == 10 ? 0 : onesValue;

        // 00 + 0 = 100
        var result = tens + ones;
        if (result == 0) result = 100;

        _logger.LogDebug(
            "Percentile roll: tens d10={TensRaw}→{TensValue}, ones d10={OnesRaw}→{OnesValue}, result={Result}",
            tensValue, tens, onesValue, ones, result);

        return result;
    }

    /// <summary>
    /// Builds a human-readable roll details string for the foraging result.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="context">The foraging context.</param>
    /// <returns>A formatted string describing the roll details.</returns>
    private static string BuildRollDetails(SkillCheckResult checkResult, ForagingContext context)
    {
        var durationBonus = context.DurationBonusDice > 0 ? $"+{context.DurationBonusDice}" : "0";
        var equipmentBonus = context.EquipmentBonus > 0 ? $"+{context.EquipmentBonus}" : "0";
        var exhaustionPenalty = context.ExhaustionPenalty > 0 ? $"-{context.ExhaustionPenalty}" : "0";

        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"Duration: {durationBonus}, Equipment: {equipmentBonus}, Exhaustion: {exhaustionPenalty} | " +
               $"DC: {context.BaseDc} ({context.ForageTarget})";
    }
}
