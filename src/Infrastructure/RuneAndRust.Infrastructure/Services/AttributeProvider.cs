// ═══════════════════════════════════════════════════════════════════════════════
// AttributeProvider.cs
// Provider implementation for loading and caching attribute definitions,
// recommended builds, and point-buy configuration from JSON.
// Version: 0.17.2e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to attribute definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// AttributeProvider implements <see cref="IAttributeProvider"/> to load attribute
/// descriptions, recommended builds, and point-buy configuration from
/// config/attributes.json. Configuration is loaded once on first access and
/// cached in memory for efficient subsequent reads.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses double-checked locking to ensure
/// thread-safe initialization. Once initialized, all reads are lock-free.
/// </para>
/// <para>
/// <strong>Configuration Requirements:</strong>
/// <list type="bullet">
///   <item><description>Exactly 5 attribute definitions must be present (one per CoreAttribute)</description></item>
///   <item><description>No duplicate attribute IDs allowed</description></item>
///   <item><description>All CoreAttribute enum values must be represented</description></item>
///   <item><description>Exactly 4 recommended builds (one per archetype)</description></item>
///   <item><description>Point-buy configuration with valid starting points and cost table</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Logging Levels:</strong>
/// <list type="bullet">
///   <item><description>Information: Provider initialization, cache stats, successful load</description></item>
///   <item><description>Debug: Individual item loading, method calls, cache lookups</description></item>
///   <item><description>Warning: Missing optional data, not-found lookups</description></item>
///   <item><description>Error: Validation failures, loading errors, configuration not found</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="IAttributeProvider"/>
/// <seealso cref="AttributeDescription"/>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="PointBuyConfiguration"/>
public class AttributeProvider : IAttributeProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expected number of attribute definitions in a valid configuration.
    /// </summary>
    private const int ExpectedAttributeCount = 5;

    /// <summary>
    /// Expected number of recommended builds in a valid configuration.
    /// </summary>
    private const int ExpectedRecommendedBuildCount = 4;

    /// <summary>
    /// Default configuration file path relative to application root.
    /// </summary>
    private const string DefaultConfigPath = "config/attributes.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for diagnostic output during provider operations.
    /// </summary>
    private readonly ILogger<AttributeProvider> _logger;

    /// <summary>
    /// Path to the attributes.json configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private readonly object _initLock = new();

    /// <summary>
    /// Cached attribute descriptions keyed by CoreAttribute enum.
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<CoreAttribute, AttributeDescription>? _descriptionCache;

    /// <summary>
    /// Cached recommended builds keyed by archetype ID (case-insensitive).
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<string, AttributeAllocationState>? _buildCache;

    /// <summary>
    /// Cached point-buy configuration.
    /// Null until initialization completes.
    /// </summary>
    private PointBuyConfiguration? _pointBuyConfig;

    /// <summary>
    /// Flag indicating whether initialization has completed.
    /// </summary>
    private bool _isInitialized;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="AttributeProvider"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configPath">
    /// Optional configuration file path. Defaults to "config/attributes.json".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public AttributeProvider(
        ILogger<AttributeProvider> logger,
        string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "AttributeProvider created with config path: {ConfigPath}",
            _configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - IAttributeProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public AttributeDescription GetAttributeDescription(CoreAttribute attribute)
    {
        EnsureInitialized();

        if (_descriptionCache!.TryGetValue(attribute, out var description))
        {
            _logger.LogDebug(
                "GetAttributeDescription found description for {Attribute}: {DisplayName}",
                attribute,
                description.DisplayName);

            return description;
        }

        _logger.LogWarning(
            "GetAttributeDescription requested unknown attribute: {Attribute}. " +
            "Valid attributes: {ValidAttributes}",
            attribute,
            string.Join(", ", _descriptionCache.Keys));

        throw new ArgumentException(
            $"Unknown attribute: {attribute}. " +
            $"Valid attributes: {string.Join(", ", Enum.GetNames<CoreAttribute>())}",
            nameof(attribute));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AttributeDescription> GetAllAttributeDescriptions()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetAllAttributeDescriptions returning {Count} descriptions",
            _descriptionCache!.Count);

        return _descriptionCache!.Values.ToList();
    }

    /// <inheritdoc/>
    public AttributeAllocationState GetRecommendedBuild(string archetypeId)
    {
        EnsureInitialized();

        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);

        if (_buildCache!.TryGetValue(archetypeId, out var build))
        {
            _logger.LogDebug(
                "GetRecommendedBuild found build for {Archetype}: " +
                "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness} ({TotalPoints} pts)",
                archetypeId,
                build.CurrentMight,
                build.CurrentFinesse,
                build.CurrentWits,
                build.CurrentWill,
                build.CurrentSturdiness,
                build.TotalPoints);

            return build;
        }

        _logger.LogWarning(
            "GetRecommendedBuild requested unknown archetype: {Archetype}. " +
            "Valid archetypes: {ValidArchetypes}",
            archetypeId,
            string.Join(", ", _buildCache.Keys));

        throw new ArgumentException(
            $"Unknown archetype: {archetypeId}. " +
            $"Valid archetypes: {string.Join(", ", _buildCache.Keys)}",
            nameof(archetypeId));
    }

    /// <inheritdoc/>
    public PointBuyConfiguration GetPointBuyConfiguration()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetPointBuyConfiguration returning config: " +
            "StartingPoints={StartingPoints}, AdeptPoints={AdeptPoints}, " +
            "Range=[{Min}-{Max}], CostTableEntries={CostTableEntries}",
            _pointBuyConfig!.Value.StartingPoints,
            _pointBuyConfig!.Value.AdeptStartingPoints,
            _pointBuyConfig!.Value.MinAttributeValue,
            _pointBuyConfig!.Value.MaxAttributeValue,
            _pointBuyConfig!.Value.CostTableEntryCount);

        return _pointBuyConfig!.Value;
    }

    /// <inheritdoc/>
    public int GetStartingPoints(string? archetypeId = null)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(archetypeId))
        {
            _logger.LogDebug(
                "GetStartingPoints returning default starting points: {Points}",
                _pointBuyConfig!.Value.StartingPoints);

            return _pointBuyConfig!.Value.StartingPoints;
        }

        var points = _pointBuyConfig!.Value.GetStartingPointsForArchetype(archetypeId);

        _logger.LogDebug(
            "GetStartingPoints for archetype '{Archetype}': {Points} points",
            archetypeId,
            points);

        return points;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Initialization
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures the provider is initialized with configuration data.
    /// Uses double-checked locking for thread safety.
    /// </summary>
    private void EnsureInitialized()
    {
        // Fast path - already initialized
        if (_isInitialized) return;

        lock (_initLock)
        {
            // Double-check after acquiring lock
            if (_isInitialized) return;

            _logger.LogInformation(
                "Initializing AttributeProvider from configuration file: {Path}",
                _configPath);

            // Load and validate configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Build the caches
            BuildCaches(config);
            _isInitialized = true;

            _logger.LogInformation(
                "AttributeProvider initialized successfully. " +
                "Loaded {DescriptionCount} attribute descriptions, " +
                "{BuildCount} recommended builds, " +
                "point-buy config with {CostTableEntries} cost table entries. " +
                "Attributes: {AttributeList}. Archetypes: {ArchetypeList}",
                _descriptionCache!.Count,
                _buildCache!.Count,
                _pointBuyConfig!.Value.CostTableEntryCount,
                string.Join(", ", _descriptionCache.Keys),
                string.Join(", ", _buildCache.Keys));
        }
    }

    /// <summary>
    /// Loads configuration from the JSON file.
    /// </summary>
    /// <returns>The deserialized configuration DTO.</returns>
    /// <exception cref="AttributeConfigurationException">
    /// Thrown when file cannot be read or JSON is invalid.
    /// </exception>
    private AttributeConfigurationDto LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Attribute configuration file not found: {Path}",
                _configPath);

            throw new AttributeConfigurationException(
                $"Attribute configuration file not found: {_configPath}");
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

            var config = JsonSerializer.Deserialize<AttributeConfigurationDto>(jsonContent, options);

            if (config == null)
            {
                _logger.LogError("Configuration deserialized to null");
                throw new AttributeConfigurationException(
                    "Failed to deserialize attribute configuration: result was null");
            }

            _logger.LogDebug(
                "Configuration loaded successfully. " +
                "Attributes: {AttributeCount}, RecommendedBuilds: {BuildCount}, " +
                "PointBuy: {HasPointBuy}",
                config.Attributes.Count,
                config.RecommendedBuilds.Count,
                config.PointBuy != null);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse attribute configuration JSON: {ErrorMessage}",
                ex.Message);

            throw new AttributeConfigurationException(
                $"Invalid JSON in attribute configuration: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates the configuration meets all requirements.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="AttributeConfigurationException">
    /// Thrown when validation fails.
    /// </exception>
    private void ValidateConfiguration(AttributeConfigurationDto config)
    {
        _logger.LogDebug("Validating attribute configuration");

        // ─── Validate attribute count ───────────────────────────────────────
        if (config.Attributes.Count != ExpectedAttributeCount)
        {
            _logger.LogError(
                "Invalid attribute count: expected {Expected}, found {Actual}",
                ExpectedAttributeCount,
                config.Attributes.Count);

            throw new AttributeConfigurationException(
                $"Expected exactly {ExpectedAttributeCount} attribute definitions, " +
                $"found {config.Attributes.Count}");
        }

        // ─── Validate no duplicates and all valid enum values ───────────────
        var seenAttributes = new HashSet<CoreAttribute>();
        foreach (var dto in config.Attributes)
        {
            // Parse enum value
            if (!Enum.TryParse<CoreAttribute>(dto.Attribute, out var attribute))
            {
                _logger.LogError(
                    "Invalid attribute ID: '{AttributeId}'. Valid values: {ValidValues}",
                    dto.Attribute,
                    string.Join(", ", Enum.GetNames<CoreAttribute>()));

                throw new AttributeConfigurationException(
                    $"Invalid attribute ID: '{dto.Attribute}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<CoreAttribute>())}");
            }

            // Check for duplicates
            if (!seenAttributes.Add(attribute))
            {
                _logger.LogError(
                    "Duplicate attribute ID in configuration: {AttributeId}",
                    attribute);

                throw new AttributeConfigurationException(
                    $"Duplicate attribute ID in configuration: {attribute}");
            }

            _logger.LogDebug(
                "Validated attribute: {AttributeId} -> {DisplayName}",
                dto.Attribute,
                dto.DisplayName);
        }

        // ─── Verify all expected attributes present ─────────────────────────
        var allAttributes = Enum.GetValues<CoreAttribute>();
        var missingAttributes = allAttributes.Except(seenAttributes).ToList();
        if (missingAttributes.Count > 0)
        {
            _logger.LogError(
                "Missing attributes in configuration: {MissingAttributes}",
                string.Join(", ", missingAttributes));

            throw new AttributeConfigurationException(
                $"Missing attributes in configuration: {string.Join(", ", missingAttributes)}");
        }

        // ─── Validate recommended builds ────────────────────────────────────
        if (config.RecommendedBuilds.Count != ExpectedRecommendedBuildCount)
        {
            _logger.LogError(
                "Invalid recommended build count: expected {Expected}, found {Actual}",
                ExpectedRecommendedBuildCount,
                config.RecommendedBuilds.Count);

            throw new AttributeConfigurationException(
                $"Expected exactly {ExpectedRecommendedBuildCount} recommended builds, " +
                $"found {config.RecommendedBuilds.Count}");
        }

        // Validate each recommended build has a non-empty archetype ID
        var seenArchetypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var build in config.RecommendedBuilds)
        {
            if (string.IsNullOrWhiteSpace(build.ArchetypeId))
            {
                _logger.LogError(
                    "Recommended build has empty or null archetypeId");

                throw new AttributeConfigurationException(
                    "Recommended build has empty or null archetypeId");
            }

            if (!seenArchetypes.Add(build.ArchetypeId))
            {
                _logger.LogError(
                    "Duplicate archetype ID in recommended builds: {ArchetypeId}",
                    build.ArchetypeId);

                throw new AttributeConfigurationException(
                    $"Duplicate archetype ID in recommended builds: {build.ArchetypeId}");
            }

            _logger.LogDebug(
                "Validated recommended build: {ArchetypeId} " +
                "(M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}, {TotalPoints} pts)",
                build.ArchetypeId,
                build.Might,
                build.Finesse,
                build.Wits,
                build.Will,
                build.Sturdiness,
                build.TotalPoints);
        }

        // ─── Validate point-buy configuration ───────────────────────────────
        if (config.PointBuy == null)
        {
            _logger.LogError("Point-buy configuration section is missing");

            throw new AttributeConfigurationException(
                "Point-buy configuration section is missing from attributes.json");
        }

        if (config.PointBuy.StartingPoints <= 0)
        {
            _logger.LogError(
                "Invalid starting points: {StartingPoints}. Must be greater than 0",
                config.PointBuy.StartingPoints);

            throw new AttributeConfigurationException(
                $"Invalid starting points: {config.PointBuy.StartingPoints}. " +
                "Must be greater than 0");
        }

        if (config.PointBuy.CostTable.Count == 0)
        {
            _logger.LogError("Point-buy cost table is empty");

            throw new AttributeConfigurationException(
                "Point-buy cost table is empty. " +
                "At least one cost entry is required");
        }

        _logger.LogDebug(
            "Configuration validation passed. " +
            "Attributes: {AttributeCount}, Builds: {BuildCount}, " +
            "CostTable: {CostTableCount} entries",
            config.Attributes.Count,
            config.RecommendedBuilds.Count,
            config.PointBuy.CostTable.Count);
    }

    /// <summary>
    /// Builds all three caches from validated configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    private void BuildCaches(AttributeConfigurationDto config)
    {
        _logger.LogDebug("Building attribute caches");

        BuildDescriptionCache(config);
        BuildRecommendedBuildCache(config);
        BuildPointBuyConfig(config);

        _logger.LogDebug(
            "All caches built. Descriptions: {DescCount}, Builds: {BuildCount}, " +
            "PointBuy: StartingPoints={StartingPoints}",
            _descriptionCache!.Count,
            _buildCache!.Count,
            _pointBuyConfig!.Value.StartingPoints);
    }

    /// <summary>
    /// Builds the attribute description cache from configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    private void BuildDescriptionCache(AttributeConfigurationDto config)
    {
        _logger.LogDebug("Building attribute description cache");

        _descriptionCache = new Dictionary<CoreAttribute, AttributeDescription>();

        foreach (var dto in config.Attributes)
        {
            var attribute = Enum.Parse<CoreAttribute>(dto.Attribute);

            // Create the domain value object via factory method
            var description = AttributeDescription.Create(
                attribute: attribute,
                displayName: dto.DisplayName,
                shortDescription: dto.ShortDescription,
                detailedDescription: dto.DetailedDescription,
                affectedStats: dto.AffectedStats,
                affectedSkills: dto.AffectedSkills);

            _descriptionCache[attribute] = description;

            _logger.LogDebug(
                "Cached attribute description: {Attribute} -> {DisplayName} " +
                "(stats: {StatCount}, skills: {SkillCount})",
                attribute,
                description.DisplayName,
                description.AffectedStatCount,
                description.AffectedSkillCount);
        }

        _logger.LogDebug(
            "Description cache built with {Count} attributes: {AttributeList}",
            _descriptionCache.Count,
            string.Join(", ", _descriptionCache.Keys));
    }

    /// <summary>
    /// Builds the recommended build cache from configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    private void BuildRecommendedBuildCache(AttributeConfigurationDto config)
    {
        _logger.LogDebug("Building recommended build cache");

        _buildCache = new Dictionary<string, AttributeAllocationState>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var dto in config.RecommendedBuilds)
        {
            // Create the domain value object via factory method
            var state = AttributeAllocationState.CreateFromRecommendedBuild(
                archetypeId: dto.ArchetypeId,
                might: dto.Might,
                finesse: dto.Finesse,
                wits: dto.Wits,
                will: dto.Will,
                sturdiness: dto.Sturdiness,
                totalPoints: dto.TotalPoints);

            _buildCache[dto.ArchetypeId] = state;

            _logger.LogDebug(
                "Cached recommended build: {ArchetypeId} -> " +
                "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness} " +
                "({TotalPoints} pts, complete: {IsComplete})",
                dto.ArchetypeId,
                state.CurrentMight,
                state.CurrentFinesse,
                state.CurrentWits,
                state.CurrentWill,
                state.CurrentSturdiness,
                state.TotalPoints,
                state.IsComplete);
        }

        _logger.LogDebug(
            "Build cache built with {Count} archetypes: {ArchetypeList}",
            _buildCache.Count,
            string.Join(", ", _buildCache.Keys));
    }

    /// <summary>
    /// Builds the point-buy configuration from the configuration DTO.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    private void BuildPointBuyConfig(AttributeConfigurationDto config)
    {
        _logger.LogDebug("Building point-buy configuration");

        var pbDto = config.PointBuy!;

        // Map cost table entries from DTOs to domain value objects
        var costTable = pbDto.CostTable
            .Select(entry =>
            {
                _logger.LogDebug(
                    "Mapping cost table entry: TargetValue={TargetValue}, " +
                    "IndividualCost={IndividualCost}, CumulativeCost={CumulativeCost}",
                    entry.TargetValue,
                    entry.IndividualCost,
                    entry.CumulativeCost);

                return PointBuyCost.Create(
                    entry.TargetValue,
                    entry.IndividualCost,
                    entry.CumulativeCost);
            })
            .ToList();

        _pointBuyConfig = new PointBuyConfiguration
        {
            StartingPoints = pbDto.StartingPoints,
            AdeptStartingPoints = pbDto.AdeptStartingPoints,
            MinAttributeValue = pbDto.MinAttributeValue,
            MaxAttributeValue = pbDto.MaxAttributeValue,
            CostTable = costTable
        };

        _logger.LogDebug(
            "Point-buy configuration built. " +
            "StartingPoints={StartingPoints}, AdeptStartingPoints={AdeptStartingPoints}, " +
            "Range=[{Min}-{Max}], CostTableEntries={CostTableEntries}, " +
            "MaxCumulativeCost={MaxCumulativeCost}",
            _pointBuyConfig.Value.StartingPoints,
            _pointBuyConfig.Value.AdeptStartingPoints,
            _pointBuyConfig.Value.MinAttributeValue,
            _pointBuyConfig.Value.MaxAttributeValue,
            _pointBuyConfig.Value.CostTableEntryCount,
            _pointBuyConfig.Value.MaxCumulativeCost);
    }
}
