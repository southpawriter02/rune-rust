using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for NPC definitions (v0.21.0a — Narrative Infrastructure).
/// Loads NPC definitions from config/npcs.json and provides indexed lookups.
/// </summary>
public class JsonNpcDefinitionProvider : INpcDefinitionProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, NpcDefinitionDto> _npcsById;
    private readonly Dictionary<string, List<NpcDefinitionDto>> _npcsByFaction;
    private readonly Dictionary<string, List<NpcDefinitionDto>> _npcsByTag;
    private readonly ILogger<JsonNpcDefinitionProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public JsonNpcDefinitionProvider(string configPath, ILogger<JsonNpcDefinitionProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _npcsById = new Dictionary<string, NpcDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        _npcsByFaction = new Dictionary<string, List<NpcDefinitionDto>>(StringComparer.OrdinalIgnoreCase);
        _npcsByTag = new Dictionary<string, List<NpcDefinitionDto>>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug("Initializing NPC definition provider from: {ConfigPath}", configPath);
        LoadNpcs();
        _logger.LogInformation(
            "NPC definition provider initialized: {NpcCount} NPCs, {FactionCount} factions",
            _npcsById.Count,
            _npcsByFaction.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<NpcDefinitionDto> GetAllDefinitions()
    {
        return _npcsById.Values.ToList();
    }

    /// <inheritdoc />
    public NpcDefinitionDto? GetDefinition(string npcId)
    {
        ArgumentNullException.ThrowIfNull(npcId);
        _npcsById.TryGetValue(npcId, out var npc);

        _logger.LogDebug("GetDefinition: {NpcId} -> {Result}",
            npcId, npc != null ? npc.Name : "not found");

        return npc;
    }

    /// <inheritdoc />
    public IReadOnlyList<NpcDefinitionDto> GetNpcsByFaction(string faction)
    {
        ArgumentNullException.ThrowIfNull(faction);

        if (_npcsByFaction.TryGetValue(faction, out var npcs))
            return npcs;

        return [];
    }

    /// <inheritdoc />
    public IReadOnlyList<NpcDefinitionDto> GetNpcsByTag(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_npcsByTag.TryGetValue(tag, out var npcs))
            return npcs;

        return [];
    }

    /// <inheritdoc />
    public bool NpcExists(string npcId)
    {
        ArgumentNullException.ThrowIfNull(npcId);
        return _npcsById.ContainsKey(npcId);
    }

    /// <inheritdoc />
    public void Reload()
    {
        _logger.LogInformation("Reloading NPC definitions from {ConfigPath}", _configPath);

        _npcsById.Clear();
        _npcsByFaction.Clear();
        _npcsByTag.Clear();
        LoadNpcs();

        _logger.LogInformation("NPC definitions reloaded: {NpcCount} NPCs", _npcsById.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    private void LoadNpcs()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "NPC configuration file not found: {Path}. No NPCs will be available.",
                _configPath);
            return;
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug("Read {Length} bytes from NPC configuration file", json.Length);

        var config = JsonSerializer.Deserialize<NpcsConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Npcs is null || config.Npcs.Count == 0)
        {
            _logger.LogWarning("NPC configuration is empty or contains no NPC definitions");
            return;
        }

        _logger.LogDebug("Parsing {Count} NPC definitions", config.Npcs.Count);

        foreach (var npc in config.Npcs)
        {
            if (string.IsNullOrWhiteSpace(npc.NpcId))
            {
                _logger.LogWarning("Skipping NPC with empty ID");
                continue;
            }

            _npcsById[npc.NpcId] = npc;

            // Index by faction
            if (!string.IsNullOrWhiteSpace(npc.Faction))
            {
                if (!_npcsByFaction.TryGetValue(npc.Faction, out var factionList))
                {
                    factionList = [];
                    _npcsByFaction[npc.Faction] = factionList;
                }
                factionList.Add(npc);
            }

            // Index by tags
            foreach (var tag in npc.Tags)
            {
                if (!_npcsByTag.TryGetValue(tag, out var tagList))
                {
                    tagList = [];
                    _npcsByTag[tag] = tagList;
                }
                tagList.Add(npc);
            }

            _logger.LogDebug(
                "Loaded NPC: {NpcId} ({Name}) - {Archetype}, {QuestCount} quests",
                npc.NpcId, npc.Name, npc.Archetype, npc.QuestIds.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    private sealed class NpcsConfigDto
    {
        public List<NpcDefinitionDto>? Npcs { get; set; }
    }
}
