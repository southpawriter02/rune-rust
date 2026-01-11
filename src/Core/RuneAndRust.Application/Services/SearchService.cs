using Microsoft.Extensions.Logging;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for performing search actions and discovering hidden content.
/// </summary>
public class SearchService
{
    private readonly SkillCheckService _skillCheckService;
    private readonly ILogger<SearchService> _logger;

    /// <summary>
    /// The default skill used for searching.
    /// </summary>
    public const string DefaultSearchSkill = "perception";

    /// <summary>
    /// Alternative skill that can be used for searching.
    /// </summary>
    public const string AlternativeSearchSkill = "investigation";

    /// <summary>
    /// Creates a new SearchService.
    /// </summary>
    public SearchService(
        SkillCheckService skillCheckService,
        ILogger<SearchService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs a search action in the current room.
    /// </summary>
    /// <param name="player">The player performing the search.</param>
    /// <param name="room">The room being searched.</param>
    /// <param name="skillId">The skill to use (defaults to perception).</param>
    /// <returns>The search result with any discoveries.</returns>
    public SearchResult PerformSearch(
        Player player,
        Room room,
        string skillId = DefaultSearchSkill)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);

        _logger.LogDebug(
            "Player {Player} searching room {Room} with {Skill}",
            player.Name, room.Name, skillId);

        // Get all hidden content
        var hiddenExits = room.GetHiddenExits();
        var hiddenItems = room.GetUndiscoveredHiddenItems();

        // Nothing to find?
        if (hiddenExits.Count == 0 && hiddenItems.Count == 0)
        {
            _logger.LogDebug("No hidden content in room {Room}", room.Name);
            return SearchResult.NothingHidden();
        }

        // Find the lowest DC (easiest to discover)
        var exitDCs = hiddenExits.Values.Select(e => e.DiscoveryDC);
        var itemDCs = hiddenItems.Select(i => i.DiscoveryDC);
        var lowestDC = exitDCs.Concat(itemDCs).Min();

        // Perform skill check against lowest DC
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            skillId,
            lowestDC,
            GetDifficultyName(lowestDC));

        _logger.LogInformation(
            "Search check: {Total} vs DC {DC} = {Result}",
            checkResult.TotalResult, lowestDC, checkResult.SuccessLevel);

        if (!checkResult.IsSuccess)
        {
            // Check if we should give a hint (within 3 of DC)
            if (checkResult.Margin >= -3)
            {
                LogNearMissHint(hiddenExits, hiddenItems);
            }

            return SearchResult.Failed(checkResult);
        }

        // Discover everything with DC <= roll result
        var discoveredExits = new List<Direction>();
        var discoveredItems = new List<Guid>();

        foreach (var (direction, exit) in hiddenExits)
        {
            if (exit.DiscoveryDC <= checkResult.TotalResult)
            {
                if (room.RevealExit(direction))
                {
                    discoveredExits.Add(direction);
                    _logger.LogInformation(
                        "Discovered hidden exit to {Direction} (DC {DC})",
                        direction, exit.DiscoveryDC);
                }
            }
        }

        foreach (var hiddenItem in hiddenItems)
        {
            if (hiddenItem.DiscoveryDC <= checkResult.TotalResult)
            {
                var revealed = room.RevealHiddenItem(hiddenItem.Id);
                if (revealed != null)
                {
                    discoveredItems.Add(hiddenItem.Id);
                    _logger.LogInformation(
                        "Discovered hidden item {Item} (DC {DC})",
                        revealed.Name, hiddenItem.DiscoveryDC);
                }
            }
        }

        return SearchResult.Success(checkResult, discoveredExits, discoveredItems);
    }

    /// <summary>
    /// Gets a difficulty name for a DC value.
    /// </summary>
    private static string GetDifficultyName(int dc) => dc switch
    {
        <= 5 => "Trivial",
        <= 8 => "Easy",
        <= 12 => "Moderate",
        <= 15 => "Challenging",
        <= 18 => "Hard",
        <= 22 => "Very Hard",
        <= 26 => "Extreme",
        _ => "Legendary"
    };

    /// <summary>
    /// Logs a hint for near-miss searches.
    /// </summary>
    private void LogNearMissHint(
        IReadOnlyDictionary<Direction, Exit> exits,
        IReadOnlyList<HiddenItem> items)
    {
        // Find hints to potentially show
        var exitHints = exits.Values
            .Where(e => !string.IsNullOrEmpty(e.HiddenHint))
            .Select(e => e.HiddenHint!)
            .ToList();

        var itemHints = items
            .Where(i => !string.IsNullOrEmpty(i.Hint))
            .Select(i => i.Hint!)
            .ToList();

        if (exitHints.Count > 0 || itemHints.Count > 0)
        {
            _logger.LogDebug(
                "Near miss - could show hints: {Hints}",
                string.Join(", ", exitHints.Concat(itemHints)));
        }
    }
}
