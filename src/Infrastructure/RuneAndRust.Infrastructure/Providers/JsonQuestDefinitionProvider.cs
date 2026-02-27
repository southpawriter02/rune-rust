using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for quest definitions (v0.21.0a — Narrative Infrastructure).
/// Loads quest definitions from config/quests.json and provides indexed lookups.
/// </summary>
public class JsonQuestDefinitionProvider : IQuestDefinitionProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, QuestDefinitionDto> _questsById;
    private readonly Dictionary<string, List<QuestDefinitionDto>> _questsByFaction;
    private readonly Dictionary<string, List<QuestDefinitionDto>> _questsByGiver;
    private readonly ILogger<JsonQuestDefinitionProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public JsonQuestDefinitionProvider(string configPath, ILogger<JsonQuestDefinitionProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _questsById = new Dictionary<string, QuestDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        _questsByFaction = new Dictionary<string, List<QuestDefinitionDto>>(StringComparer.OrdinalIgnoreCase);
        _questsByGiver = new Dictionary<string, List<QuestDefinitionDto>>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug("Initializing quest definition provider from: {ConfigPath}", configPath);
        LoadQuests();
        _logger.LogInformation(
            "Quest definition provider initialized: {QuestCount} quests, {FactionCount} factions, {GiverCount} givers",
            _questsById.Count,
            _questsByFaction.Count,
            _questsByGiver.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetAllDefinitions()
    {
        return _questsById.Values.ToList();
    }

    /// <inheritdoc />
    public QuestDefinitionDto? GetDefinition(string questId)
    {
        ArgumentNullException.ThrowIfNull(questId);
        _questsById.TryGetValue(questId, out var quest);

        _logger.LogDebug("GetDefinition: {QuestId} -> {Result}",
            questId, quest != null ? quest.Name : "not found");

        return quest;
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetQuestsByFaction(string faction)
    {
        ArgumentNullException.ThrowIfNull(faction);

        if (_questsByFaction.TryGetValue(faction, out var quests))
            return quests;

        return [];
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetQuestsByGiver(string npcId)
    {
        ArgumentNullException.ThrowIfNull(npcId);

        if (_questsByGiver.TryGetValue(npcId, out var quests))
            return quests;

        return [];
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetAvailableQuests(
        int legendLevel,
        IReadOnlySet<string> completedQuestIds)
    {
        ArgumentNullException.ThrowIfNull(completedQuestIds);

        var available = _questsById.Values
            .Where(q => q.MinimumLegend <= legendLevel)
            .Where(q => q.PrerequisiteQuestIds.All(prereq =>
                completedQuestIds.Contains(prereq)))
            .Where(q => !completedQuestIds.Contains(q.QuestId) || q.IsRepeatable)
            .ToList();

        _logger.LogDebug(
            "GetAvailableQuests: legendLevel={Level}, completed={CompletedCount} -> {AvailableCount} available",
            legendLevel, completedQuestIds.Count, available.Count);

        return available;
    }

    /// <inheritdoc />
    public void Reload()
    {
        _logger.LogInformation("Reloading quest definitions from {ConfigPath}", _configPath);

        _questsById.Clear();
        _questsByFaction.Clear();
        _questsByGiver.Clear();
        LoadQuests();

        _logger.LogInformation("Quest definitions reloaded: {QuestCount} quests", _questsById.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    private void LoadQuests()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "Quest configuration file not found: {Path}. No quests will be available.",
                _configPath);
            return;
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug("Read {Length} bytes from quest configuration file", json.Length);

        var config = JsonSerializer.Deserialize<QuestsConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Quests is null || config.Quests.Count == 0)
        {
            _logger.LogWarning("Quest configuration is empty or contains no quest definitions");
            return;
        }

        _logger.LogDebug("Parsing {Count} quest definitions", config.Quests.Count);

        foreach (var quest in config.Quests)
        {
            if (string.IsNullOrWhiteSpace(quest.QuestId))
            {
                _logger.LogWarning("Skipping quest with empty ID");
                continue;
            }

            _questsById[quest.QuestId] = quest;

            // Index by faction
            if (!string.IsNullOrWhiteSpace(quest.Faction))
            {
                if (!_questsByFaction.TryGetValue(quest.Faction, out var factionList))
                {
                    factionList = [];
                    _questsByFaction[quest.Faction] = factionList;
                }
                factionList.Add(quest);
            }

            // Index by giver NPC
            if (!string.IsNullOrWhiteSpace(quest.GiverNpcId))
            {
                if (!_questsByGiver.TryGetValue(quest.GiverNpcId, out var giverList))
                {
                    giverList = [];
                    _questsByGiver[quest.GiverNpcId] = giverList;
                }
                giverList.Add(quest);
            }

            _logger.LogDebug(
                "Loaded quest: {QuestId} ({Name}) - {Category}, {ObjectiveCount} objectives",
                quest.QuestId, quest.Name, quest.Category, quest.Objectives.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    private sealed class QuestsConfigDto
    {
        public List<QuestDefinitionDto>? Quests { get; set; }
    }
}
