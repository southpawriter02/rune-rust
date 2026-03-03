using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for faction definitions (SPEC-REPUTATION-001).
/// Loads faction definitions from config/factions.json and provides indexed lookups.
/// </summary>
/// <remarks>
/// <para>Follows the same pattern as <see cref="JsonNpcDefinitionProvider"/> and
/// <see cref="JsonQuestDefinitionProvider"/>: constructor loads from file path,
/// indexes by ID, and provides read-only access through the interface.</para>
///
/// <para>All lookups are case-insensitive on faction IDs.</para>
/// </remarks>
public class JsonFactionDefinitionProvider : IFactionDefinitionProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, FactionDefinitionDto> _factionsById;
    private readonly ILogger<JsonFactionDefinitionProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="JsonFactionDefinitionProvider"/>
    /// and loads faction definitions from the specified config file.
    /// </summary>
    /// <param name="configPath">The path to config/factions.json.</param>
    /// <param name="logger">Logger for structured faction loading events.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configPath"/> or <paramref name="logger"/> is null.
    /// </exception>
    public JsonFactionDefinitionProvider(string configPath, ILogger<JsonFactionDefinitionProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _factionsById = new Dictionary<string, FactionDefinitionDto>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug("Initializing faction definition provider from: {ConfigPath}", configPath);
        LoadFactions();
        _logger.LogInformation(
            "Faction definition provider initialized: {FactionCount} factions loaded",
            _factionsById.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<FactionDefinitionDto> GetAllFactions()
    {
        return _factionsById.Values.ToList();
    }

    /// <inheritdoc />
    public FactionDefinitionDto? GetFaction(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return null;

        _factionsById.TryGetValue(factionId, out var faction);

        _logger.LogDebug("GetFaction: {FactionId} -> {Result}",
            factionId, faction != null ? faction.Name : "not found");

        return faction;
    }

    /// <inheritdoc />
    public bool FactionExists(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return false;

        return _factionsById.ContainsKey(factionId);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAllies(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return [];

        var faction = GetFaction(factionId);
        if (faction == null)
            return [];

        return faction.Allies.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetEnemies(string factionId)
    {
        if (string.IsNullOrWhiteSpace(factionId))
            return [];

        var faction = GetFaction(factionId);
        if (faction == null)
            return [];

        return faction.Enemies.ToList();
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads faction definitions from the configured JSON file.
    /// </summary>
    /// <remarks>
    /// If the file doesn't exist or is malformed, logs a warning and initializes
    /// with an empty collection rather than throwing. This allows the game to run
    /// in a degraded state (all factions unknown, all reputation changes are no-ops).
    /// </remarks>
    private void LoadFactions()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "Faction configuration file not found: {Path}. No factions will be available.",
                _configPath);
            return;
        }

        try
        {
            var json = File.ReadAllText(_configPath);

            _logger.LogDebug("Read {Length} bytes from faction configuration file", json.Length);

            var config = JsonSerializer.Deserialize<FactionsConfigDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config?.Factions is null || config.Factions.Count == 0)
            {
                _logger.LogWarning("Faction configuration is empty or contains no faction definitions");
                return;
            }

            _logger.LogDebug("Parsing {Count} faction definitions", config.Factions.Count);

            foreach (var faction in config.Factions)
            {
                if (string.IsNullOrWhiteSpace(faction.FactionId))
                {
                    _logger.LogWarning("Skipping faction definition with empty/null FactionId");
                    continue;
                }

                if (_factionsById.ContainsKey(faction.FactionId))
                {
                    _logger.LogWarning(
                        "Duplicate faction ID '{FactionId}' — keeping first definition",
                        faction.FactionId);
                    continue;
                }

                _factionsById[faction.FactionId] = faction;

                _logger.LogDebug(
                    "Loaded faction: {FactionId} ({Name}), allies=[{Allies}], enemies=[{Enemies}]",
                    faction.FactionId,
                    faction.Name,
                    string.Join(", ", faction.Allies),
                    string.Join(", ", faction.Enemies));
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to parse faction configuration from {Path}: {Error}",
                _configPath, ex.Message);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex,
                "Failed to read faction configuration from {Path}: {Error}",
                _configPath, ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO FOR DESERIALIZATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for config/factions.json deserialization.
    /// </summary>
    private record FactionsConfigDto
    {
        /// <summary>The list of faction definitions.</summary>
        public List<FactionDefinitionDto> Factions { get; init; } = [];
    }
}
