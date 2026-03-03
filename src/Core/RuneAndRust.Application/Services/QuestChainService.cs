using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing faction quest chain progression and reputation-gated availability.
/// </summary>
/// <remarks>
/// <para>Wraps <see cref="IQuestDefinitionProvider"/> with reputation-aware filtering
/// and provides chain-level views per SPEC-FACTION-QUESTS-001.</para>
///
/// <para><b>Availability Logic:</b> A quest is available when ALL of:</para>
/// <list type="number">
///   <item><description>Player's legend level ≥ quest's MinimumLegend</description></item>
///   <item><description>All PrerequisiteQuestIds are in the completed set</description></item>
///   <item><description>If Faction is set: player's reputation with that faction ≥ MinimumReputation</description></item>
/// </list>
/// </remarks>
public class QuestChainService : IQuestChainService
{
    private readonly IQuestDefinitionProvider _questProvider;
    private readonly IReputationService _reputationService;
    private readonly ILogger<QuestChainService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="QuestChainService"/>.
    /// </summary>
    /// <param name="questProvider">Provider for quest definitions.</param>
    /// <param name="reputationService">Service for reputation lookups.</param>
    /// <param name="logger">Logger for structured events.</param>
    public QuestChainService(
        IQuestDefinitionProvider questProvider,
        IReputationService reputationService,
        ILogger<QuestChainService> logger)
    {
        _questProvider = questProvider ?? throw new ArgumentNullException(nameof(questProvider));
        _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetAvailableQuestsForPlayer(
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds)
    {
        ArgumentNullException.ThrowIfNull(player);
        completedQuestIds ??= new HashSet<string>();

        // Get base available quests (legend + prerequisites filtering)
        var baseAvailable = _questProvider.GetAvailableQuests(legendLevel, completedQuestIds);

        // Apply reputation filtering on top
        var results = new List<QuestDefinitionDto>();
        foreach (var quest in baseAvailable)
        {
            if (MeetsReputationRequirement(quest, player))
            {
                results.Add(quest);
            }
            else
            {
                _logger.LogDebug(
                    "Quest {QuestId}: reputation check failed — player rep {PlayerRep} < required {MinRep} for faction {Faction}",
                    quest.QuestId,
                    quest.Faction != null ? player.GetFactionReputation(quest.Faction).Value : 0,
                    quest.MinimumReputation,
                    quest.Faction ?? "none");
            }
        }

        return results;
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestChainSummaryDto> GetAllChainSummaries(
        Player player,
        IReadOnlySet<string> completedQuestIds)
    {
        ArgumentNullException.ThrowIfNull(player);
        completedQuestIds ??= new HashSet<string>();

        // Find all distinct chain IDs
        var allQuests = _questProvider.GetAllDefinitions();
        var chainIds = allQuests
            .Where(q => !string.IsNullOrWhiteSpace(q.ChainId))
            .Select(q => q.ChainId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var summaries = new List<QuestChainSummaryDto>();
        foreach (var chainId in chainIds)
        {
            var summary = GetChainSummary(chainId, player, completedQuestIds);
            if (summary != null)
            {
                summaries.Add(summary);
            }
        }

        return summaries;
    }

    /// <inheritdoc />
    public QuestChainSummaryDto? GetChainSummary(
        string chainId,
        Player player,
        IReadOnlySet<string> completedQuestIds)
    {
        if (string.IsNullOrWhiteSpace(chainId))
            return null;

        ArgumentNullException.ThrowIfNull(player);
        completedQuestIds ??= new HashSet<string>();

        var chainQuests = GetQuestsInChain(chainId);
        if (chainQuests.Count == 0)
            return null;

        // Determine faction from first quest in chain (all quests in a chain share a faction)
        var factionId = chainQuests[0].Faction ?? string.Empty;
        var factionName = _reputationService.GetFactionName(factionId);
        var playerRep = !string.IsNullOrWhiteSpace(factionId)
            ? player.GetFactionReputation(factionId).Value
            : 0;

        // Count completed quests
        var completedCount = chainQuests.Count(q =>
            completedQuestIds.Contains(q.QuestId));

        // Find next quest (first uncompleted in order)
        var nextQuest = chainQuests.FirstOrDefault(q =>
            !completedQuestIds.Contains(q.QuestId));

        // Determine if next quest is reputation-locked
        var isRepLocked = false;
        int? nextRepRequired = null;
        if (nextQuest != null && !string.IsNullOrWhiteSpace(nextQuest.Faction) && nextQuest.MinimumReputation > 0)
        {
            isRepLocked = playerRep < nextQuest.MinimumReputation;
            nextRepRequired = nextQuest.MinimumReputation;
        }

        _logger.LogDebug(
            "Chain {ChainId}: {Completed}/{Total} quests complete, next={NextQuest}, repLocked={RepLocked}",
            chainId, completedCount, chainQuests.Count,
            nextQuest?.QuestId ?? "none", isRepLocked);

        return new QuestChainSummaryDto
        {
            ChainId = chainId,
            FactionId = factionId,
            FactionName = factionName,
            TotalQuests = chainQuests.Count,
            CompletedQuests = completedCount,
            NextQuestId = nextQuest?.QuestId,
            NextQuestName = nextQuest?.Name,
            IsNextQuestReputationLocked = isRepLocked,
            NextQuestReputationRequired = nextRepRequired,
            PlayerReputation = playerRep
        };
    }

    /// <inheritdoc />
    public QuestDefinitionDto? GetNextQuestInChain(
        string chainId,
        IReadOnlySet<string> completedQuestIds)
    {
        if (string.IsNullOrWhiteSpace(chainId))
            return null;

        completedQuestIds ??= new HashSet<string>();

        var chainQuests = GetQuestsInChain(chainId);

        // Return first uncompleted quest in chain order
        return chainQuests.FirstOrDefault(q =>
            !completedQuestIds.Contains(q.QuestId));
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetQuestsInChain(string chainId)
    {
        if (string.IsNullOrWhiteSpace(chainId))
            return [];

        var allQuests = _questProvider.GetAllDefinitions();

        return allQuests
            .Where(q => string.Equals(q.ChainId, chainId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(q => q.ChainOrder ?? int.MaxValue)
            .ThenBy(q => q.QuestId, StringComparer.OrdinalIgnoreCase) // Tiebreaker for same/null ChainOrder
            .ToList();
    }

    /// <inheritdoc />
    public bool IsQuestAvailable(
        QuestDefinitionDto quest,
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds)
    {
        ArgumentNullException.ThrowIfNull(quest);
        ArgumentNullException.ThrowIfNull(player);
        completedQuestIds ??= new HashSet<string>();

        // Check 1: Legend level
        if (quest.MinimumLegend > 0 && legendLevel < quest.MinimumLegend)
        {
            _logger.LogDebug(
                "Quest {QuestId}: legend check failed — player level {Level} < required {MinLevel}",
                quest.QuestId, legendLevel, quest.MinimumLegend);
            return false;
        }

        // Check 2: Prerequisites
        foreach (var prereqId in quest.PrerequisiteQuestIds)
        {
            if (!completedQuestIds.Contains(prereqId))
            {
                _logger.LogDebug(
                    "Quest {QuestId}: prerequisite {PrereqId} not completed",
                    quest.QuestId, prereqId);
                return false;
            }
        }

        // Check 3: Faction reputation
        if (!MeetsReputationRequirement(quest, player))
        {
            _logger.LogDebug(
                "Quest {QuestId}: rep check {PlayerRep} < {MinRep} for {Faction}",
                quest.QuestId,
                quest.Faction != null ? player.GetFactionReputation(quest.Faction).Value : 0,
                quest.MinimumReputation,
                quest.Faction ?? "none");
            return false;
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a player meets the reputation requirement for a quest.
    /// </summary>
    /// <param name="quest">The quest to check.</param>
    /// <param name="player">The player to check.</param>
    /// <returns>
    /// <c>true</c> if the quest has no reputation requirement, has no faction,
    /// or the player's reputation meets the minimum. <c>false</c> otherwise.
    /// </returns>
    private bool MeetsReputationRequirement(QuestDefinitionDto quest, Player player)
    {
        // No faction = no reputation check
        if (string.IsNullOrWhiteSpace(quest.Faction))
            return true;

        // No minimum reputation = always available
        if (quest.MinimumReputation <= 0)
            return true;

        // Check player's reputation with the quest's faction
        var playerReputation = player.GetFactionReputation(quest.Faction);
        return playerReputation.Value >= quest.MinimumReputation;
    }
}
