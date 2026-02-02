using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Services;

/// <summary>
/// Validates and queries realm biome adjacency relationships.
/// </summary>
/// <remarks>
/// <para>
/// BiomeAdjacencyService loads adjacency rules from configuration and provides
/// fast O(1) lookups for compatibility checks. Critical incompatibilities are
/// pre-computed for immediate rejection.
/// </para>
/// <para>
/// Configuration is loaded once on first access and cached in memory.
/// Uses double-checked locking for thread-safe initialization.
/// </para>
/// </remarks>
public sealed class BiomeAdjacencyService : IBiomeAdjacencyService
{
    private const string DefaultConfigPath = "config/adjacency-matrix.json";

    private readonly string _configPath;
    private readonly ILogger<BiomeAdjacencyService> _logger;
    private readonly object _initLock = new();

    private Dictionary<string, AdjacencyRule>? _rules;
    private HashSet<string>? _incompatiblePairs;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiomeAdjacencyService"/> class.
    /// </summary>
    /// <param name="logger">Logger for adjacency operations.</param>
    /// <param name="configPath">Optional configuration file path.</param>
    public BiomeAdjacencyService(
        ILogger<BiomeAdjacencyService> logger,
        string? configPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "BiomeAdjacencyService created with config path: {ConfigPath}",
            _configPath);
    }

    /// <summary>
    /// Initializes a new instance with explicit rules (for testing).
    /// </summary>
    internal BiomeAdjacencyService(
        IEnumerable<AdjacencyRule> rules,
        ILogger<BiomeAdjacencyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = string.Empty;

        var rulesList = rules.ToList();
        _rules = rulesList.ToDictionary(r => r.GetKey());

        _incompatiblePairs = rulesList
            .Where(r => r.Compatibility == BiomeCompatibility.Incompatible)
            .Select(r => r.GetKey())
            .ToHashSet();

        _isInitialized = true;

        _logger.LogDebug(
            "BiomeAdjacencyService initialized with test data: {RuleCount} rules",
            _rules.Count);
    }

    /// <inheritdoc/>
    public bool CanBiomesNeighbor(RealmId realmA, RealmId realmB)
    {
        EnsureInitialized();

        if (realmA == realmB)
            return false; // Same realm cannot neighbor itself

        var key = AdjacencyRule.GetKey(realmA, realmB);
        var canNeighbor = !_incompatiblePairs!.Contains(key);

        _logger.LogDebug(
            "Adjacency check: {RealmA} ↔ {RealmB} = {CanNeighbor}",
            realmA, realmB, canNeighbor);

        return canNeighbor;
    }

    /// <inheritdoc/>
    public BiomeCompatibility GetCompatibility(RealmId realmA, RealmId realmB)
    {
        EnsureInitialized();

        if (realmA == realmB)
            return BiomeCompatibility.Compatible; // Same realm is trivially compatible

        var key = AdjacencyRule.GetKey(realmA, realmB);

        if (_rules!.TryGetValue(key, out var rule))
            return rule.Compatibility;

        // Default: RequiresTransition for undefined pairs
        return BiomeCompatibility.RequiresTransition;
    }

    /// <inheritdoc/>
    public (int Min, int Max) GetTransitionRoomCount(RealmId realmA, RealmId realmB)
    {
        var compatibility = GetCompatibility(realmA, realmB);

        return compatibility switch
        {
            BiomeCompatibility.Compatible => (0, 0),
            BiomeCompatibility.RequiresTransition => GetTransitionRange(realmA, realmB),
            BiomeCompatibility.Incompatible => throw new InvalidOperationException(
                $"Cannot get transition rooms for incompatible realms: {realmA} and {realmB}"),
            _ => (1, 3)
        };
    }

    /// <inheritdoc/>
    public string? GetTransitionTheme(RealmId fromRealm, RealmId toRealm)
    {
        EnsureInitialized();
        var key = AdjacencyRule.GetKey(fromRealm, toRealm);
        return _rules!.GetValueOrDefault(key)?.TransitionTheme;
    }

    /// <inheritdoc/>
    public IReadOnlyList<RealmId> GetAdjacentRealms(RealmId realmId)
    {
        return Enum.GetValues<RealmId>()
            .Where(r => r != realmId && CanBiomesNeighbor(realmId, r))
            .ToList();
    }

    /// <inheritdoc/>
    public bool ValidateRealmConfiguration(IEnumerable<(RealmId, RealmId)> adjacentPairs)
    {
        foreach (var (a, b) in adjacentPairs)
        {
            if (!CanBiomesNeighbor(a, b))
            {
                _logger.LogWarning(
                    "Invalid realm configuration: {RealmA} cannot neighbor {RealmB}",
                    a, b);
                return false;
            }
        }
        return true;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        lock (_initLock)
        {
            if (_isInitialized) return;

            _logger.LogInformation(
                "Initializing BiomeAdjacencyService from configuration file: {Path}",
                _configPath);

            var rules = LoadConfiguration();

            // Index rules by normalized key
            _rules = rules.ToDictionary(r => r.GetKey());

            // Pre-compute incompatible pairs for fast lookup
            _incompatiblePairs = rules
                .Where(r => r.Compatibility == BiomeCompatibility.Incompatible)
                .Select(r => r.GetKey())
                .ToHashSet();

            _isInitialized = true;

            _logger.LogInformation(
                "BiomeAdjacencyService initialized: {RuleCount} rules, {IncompatibleCount} incompatible pairs",
                _rules.Count, _incompatiblePairs.Count);
        }
    }

    private List<AdjacencyRule> LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        if (!File.Exists(_configPath))
        {
            _logger.LogWarning(
                "Adjacency matrix configuration file not found: {Path}. Using default rules.",
                _configPath);

            return GetDefaultRules();
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

            var config = JsonSerializer.Deserialize<AdjacencyMatrixConfiguration>(jsonContent, options);

            if (config?.Rules == null || config.Rules.Count == 0)
            {
                _logger.LogWarning("Configuration has no rules, using defaults");
                return GetDefaultRules();
            }

            // Convert DTOs to domain objects
            var rules = config.Rules.Select(dto => new AdjacencyRule
            {
                RealmA = (RealmId)dto.RealmA,
                RealmB = (RealmId)dto.RealmB,
                Compatibility = (BiomeCompatibility)dto.Compatibility,
                MinTransitionRooms = dto.MinTransitionRooms,
                MaxTransitionRooms = dto.MaxTransitionRooms,
                TransitionTheme = dto.TransitionTheme
            }).ToList();

            _logger.LogDebug("Configuration loaded with {RuleCount} rules", rules.Count);

            return rules;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse adjacency matrix configuration JSON: {ErrorMessage}",
                ex.Message);

            return GetDefaultRules();
        }
    }

    private (int Min, int Max) GetTransitionRange(RealmId realmA, RealmId realmB)
    {
        EnsureInitialized();
        var key = AdjacencyRule.GetKey(realmA, realmB);

        if (_rules!.TryGetValue(key, out var rule) && rule.RequiresTransitionRooms)
            return (rule.MinTransitionRooms, rule.MaxTransitionRooms);

        return (1, 3); // Default transition range
    }

    private static List<AdjacencyRule> GetDefaultRules()
    {
        return
        [
            // Critical incompatibilities (Fire vs Ice/Bio)
            AdjacencyRule.Incompatible(RealmId.Muspelheim, RealmId.Niflheim),
            AdjacencyRule.Incompatible(RealmId.Muspelheim, RealmId.Vanaheim),

            // Compatible pairs (natural neighbors)
            AdjacencyRule.Compatible(
                RealmId.Midgard,
                RealmId.Vanaheim,
                "River ferries upstream... canopy shadows deepen..."),

            AdjacencyRule.Compatible(
                RealmId.Svartalfheim,
                RealmId.Muspelheim,
                "Volcanic heat fades into geothermal stability..."),

            AdjacencyRule.Compatible(
                RealmId.Svartalfheim,
                RealmId.Helheim,
                "Elevators descend... decontamination airlocks seal..."),

            // Transition required pairs
            AdjacencyRule.WithTransition(
                RealmId.Midgard,
                RealmId.Alfheim,
                minRooms: 1,
                maxRooms: 2,
                "Reality shifts... the air thickens with aetheric energy..."),

            AdjacencyRule.WithTransition(
                RealmId.Niflheim,
                RealmId.Helheim,
                minRooms: 2,
                maxRooms: 3,
                "Frost gives way to choking toxicity...")
        ];
    }
}

/// <summary>
/// Root configuration object for adjacency-matrix.json.
/// </summary>
internal sealed class AdjacencyMatrixConfiguration
{
    public string? Version { get; init; }
    public string? Description { get; init; }
    public List<AdjacencyRuleDto>? Rules { get; init; }
}

/// <summary>
/// DTO for deserializing adjacency rules from JSON.
/// </summary>
internal sealed class AdjacencyRuleDto
{
    public int RealmA { get; init; }
    public int RealmB { get; init; }
    public int Compatibility { get; init; }
    public int MinTransitionRooms { get; init; }
    public int MaxTransitionRooms { get; init; }
    public string? TransitionTheme { get; init; }
    public string? Comment { get; init; }
}
