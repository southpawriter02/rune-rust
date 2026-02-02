using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Services;

/// <summary>
/// Provides access to realm biome definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// RealmBiomeProvider loads biome data from realm-biomes.json and indexes it
/// for fast O(1) lookups by RealmId and EnvironmentalConditionType.
/// </para>
/// <para>
/// Configuration is loaded once on first access and cached in memory.
/// Uses double-checked locking for thread-safe initialization.
/// </para>
/// </remarks>
public sealed class RealmBiomeProvider : IRealmBiomeProvider
{
    private const string DefaultConfigPath = "config/realm-biomes.json";

    private readonly string _configPath;
    private readonly ILogger<RealmBiomeProvider> _logger;
    private readonly object _initLock = new();

    private Dictionary<RealmId, RealmBiomeDefinition>? _biomes;
    private Dictionary<EnvironmentalConditionType, EnvironmentalCondition>? _conditions;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealmBiomeProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for biome operations.</param>
    /// <param name="configPath">Optional configuration file path.</param>
    public RealmBiomeProvider(
        ILogger<RealmBiomeProvider> logger,
        string? configPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "RealmBiomeProvider created with config path: {ConfigPath}",
            _configPath);
    }

    /// <summary>
    /// Initializes a new instance with explicit data (for testing).
    /// </summary>
    internal RealmBiomeProvider(
        IEnumerable<RealmBiomeDefinition> biomes,
        IEnumerable<EnvironmentalCondition> conditions,
        ILogger<RealmBiomeProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = string.Empty;
        _biomes = biomes.ToDictionary(b => b.RealmId);
        _conditions = conditions.ToDictionary(c => c.Type);
        _isInitialized = true;

        _logger.LogDebug(
            "RealmBiomeProvider initialized with test data: {BiomeCount} biomes, {ConditionCount} conditions",
            _biomes.Count, _conditions.Count);
    }

    /// <inheritdoc/>
    public IReadOnlyList<RealmBiomeDefinition> GetAllBiomes()
    {
        EnsureInitialized();
        return _biomes!.Values.ToList();
    }

    /// <inheritdoc/>
    public RealmBiomeDefinition? GetBiome(RealmId realmId)
    {
        EnsureInitialized();

        if (_biomes!.TryGetValue(realmId, out var biome))
            return biome;

        _logger.LogWarning("Biome not found for realm: {RealmId}", realmId);
        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<RealmBiomeZone> GetZonesForRealm(RealmId realmId) =>
        GetBiome(realmId)?.Zones ?? [];

    /// <inheritdoc/>
    public RealmBiomeZone? GetZone(RealmId realmId, string zoneId) =>
        GetBiome(realmId)?.GetZone(zoneId);

    /// <inheritdoc/>
    public EnvironmentalCondition? GetEnvironmentalCondition(EnvironmentalConditionType type)
    {
        EnsureInitialized();

        if (_conditions!.TryGetValue(type, out var condition))
            return condition;

        _logger.LogWarning("Environmental condition not found: {ConditionType}", type);
        return null;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        lock (_initLock)
        {
            if (_isInitialized) return;

            _logger.LogInformation(
                "Initializing RealmBiomeProvider from configuration file: {Path}",
                _configPath);

            var config = LoadConfiguration();

            // Index biomes by RealmId
            _biomes = config.Biomes?.ToDictionary(b => b.RealmId)
                ?? new Dictionary<RealmId, RealmBiomeDefinition>();

            // Index conditions by type
            _conditions = config.EnvironmentalConditions?.ToDictionary(c => c.Type)
                ?? new Dictionary<EnvironmentalConditionType, EnvironmentalCondition>();

            _isInitialized = true;

            _logger.LogInformation(
                "RealmBiomeProvider initialized: {BiomeCount} biomes, {ConditionCount} conditions",
                _biomes.Count, _conditions.Count);
        }
    }

    private RealmBiomeConfiguration LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "Realm biome configuration file not found: {Path}. Using empty configuration.",
                _configPath);

            return new RealmBiomeConfiguration();
        }

        try
        {
            var jsonContent = File.ReadAllText(_configPath);

            _logger.LogDebug(
                "Read {ByteCount} bytes from configuration file",
                jsonContent.Length);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<RealmBiomeConfiguration>(jsonContent, options);

            if (config == null)
            {
                _logger.LogWarning("Configuration deserialized to null, using empty configuration");
                return new RealmBiomeConfiguration();
            }

            _logger.LogDebug(
                "Configuration loaded. Biomes: {BiomeCount}, Conditions: {ConditionCount}",
                config.Biomes?.Count ?? 0,
                config.EnvironmentalConditions?.Count ?? 0);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse realm biome configuration JSON: {ErrorMessage}",
                ex.Message);

            return new RealmBiomeConfiguration();
        }
    }
}

/// <summary>
/// Root configuration object for realm-biomes.json.
/// </summary>
internal sealed class RealmBiomeConfiguration
{
    /// <summary>
    /// List of realm biome definitions.
    /// </summary>
    public List<RealmBiomeDefinition>? Biomes { get; init; }

    /// <summary>
    /// List of environmental condition definitions.
    /// </summary>
    public List<EnvironmentalCondition>? EnvironmentalConditions { get; init; }
}
