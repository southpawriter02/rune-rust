using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for harvestable feature definitions.
/// </summary>
/// <remarks>
/// <para>
/// JsonHarvestableFeatureProvider loads harvestable feature definitions from a JSON
/// configuration file and provides thread-safe access to the loaded definitions
/// with indexed lookups.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "harvestableFeatures": [
///     {
///       "id": "iron-ore-vein",
///       "name": "Iron Ore Vein",
///       "description": "A vein of iron ore embedded in the rock wall",
///       "resourceId": "iron-ore",
///       "minQuantity": 1,
///       "maxQuantity": 5,
///       "difficultyClass": 12,
///       "requiredTool": null,
///       "replenishTurns": 0,
///       "icon": "icons/features/ore_vein.png"
///     }
///   ]
/// }
/// </code>
/// <para>
/// The provider builds an index for efficient lookup by feature ID (case-insensitive).
/// Resource ID references are validated against the resource provider during loading.
/// </para>
/// </remarks>
public class JsonHarvestableFeatureProvider : IHarvestableFeatureProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: feature ID -> HarvestableFeatureDefinition.
    /// </summary>
    private readonly Dictionary<string, HarvestableFeatureDefinition> _features;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<JsonHarvestableFeatureProvider> _logger;

    /// <summary>
    /// Resource provider for validating resource references.
    /// </summary>
    private readonly IResourceProvider _resourceProvider;

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonHarvestableFeatureProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the harvestable-features.json configuration file.</param>
    /// <param name="resourceProvider">Resource provider for validating resource references.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    /// <remarks>
    /// <para>
    /// The constructor loads all features from the JSON file immediately.
    /// Resource ID references are validated against the resource provider,
    /// and features with invalid references are skipped with a warning.
    /// </para>
    /// </remarks>
    public JsonHarvestableFeatureProvider(
        string configPath,
        IResourceProvider resourceProvider,
        ILogger<JsonHarvestableFeatureProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _features = new Dictionary<string, HarvestableFeatureDefinition>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Initializing harvestable feature provider from configuration file: {ConfigPath}",
            configPath);

        LoadFeatures();

        _logger.LogInformation(
            "Harvestable feature provider initialized successfully with {FeatureCount} features",
            _features.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads harvestable feature definitions from the JSON configuration file.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is empty or invalid.</exception>
    private void LoadFeatures()
    {
        _logger.LogDebug("Loading harvestable feature definitions from {ConfigPath}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError("Harvestable feature configuration file not found: {Path}", _configPath);
            throw new FileNotFoundException(
                $"Harvestable feature configuration file not found: {_configPath}",
                _configPath);
        }

        try
        {
            // Read and parse JSON
            var json = File.ReadAllText(_configPath);
            _logger.LogDebug("Read {ByteCount} bytes from configuration file", json.Length);

            var config = JsonSerializer.Deserialize<HarvestableFeaturesConfigDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate configuration
            if (config?.HarvestableFeatures is null || config.HarvestableFeatures.Count == 0)
            {
                _logger.LogWarning("No harvestable features found in configuration file");
                return;
            }

            _logger.LogDebug(
                "Parsing {Count} harvestable feature entries from configuration",
                config.HarvestableFeatures.Count);

            // Parse each feature entry
            var loadedCount = 0;
            var skippedCount = 0;

            foreach (var featureDto in config.HarvestableFeatures)
            {
                if (!TryParseFeature(featureDto, out var definition))
                {
                    skippedCount++;
                    continue;
                }

                // Check for duplicates
                if (_features.ContainsKey(definition!.FeatureId))
                {
                    _logger.LogWarning(
                        "Duplicate feature ID '{FeatureId}' found - skipping duplicate entry",
                        definition.FeatureId);
                    skippedCount++;
                    continue;
                }

                // Add to index
                _features[definition.FeatureId] = definition;
                loadedCount++;

                _logger.LogDebug(
                    "Loaded feature: {FeatureId} -> {ResourceId} (DC: {DC}, Qty: {MinQty}-{MaxQty})",
                    definition.FeatureId,
                    definition.ResourceId,
                    definition.DifficultyClass,
                    definition.MinQuantity,
                    definition.MaxQuantity);
            }

            _logger.LogInformation(
                "Loaded {LoadedCount} harvestable feature definitions from configuration ({SkippedCount} skipped)",
                loadedCount,
                skippedCount);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse harvestable feature configuration file: {Path}", _configPath);
            throw new InvalidDataException(
                $"Failed to parse harvestable feature configuration file: {_configPath}",
                ex);
        }
    }

    /// <summary>
    /// Attempts to parse a feature DTO into a HarvestableFeatureDefinition.
    /// </summary>
    /// <param name="dto">The DTO to parse.</param>
    /// <param name="definition">The parsed definition, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    private bool TryParseFeature(HarvestableFeatureDto dto, out HarvestableFeatureDefinition? definition)
    {
        definition = null;

        try
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                _logger.LogWarning("Harvestable feature entry missing ID - skipping");
                return false;
            }

            // Validate resource ID exists
            if (string.IsNullOrWhiteSpace(dto.ResourceId))
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' missing resource ID - skipping",
                    dto.Id);
                return false;
            }

            // Validate resource reference exists in resource provider
            if (!_resourceProvider.Exists(dto.ResourceId))
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' references unknown resource '{ResourceId}' - skipping",
                    dto.Id,
                    dto.ResourceId);
                return false;
            }

            // Validate quantity range
            if (dto.MinQuantity < 0)
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' has negative minQuantity ({MinQty}) - skipping",
                    dto.Id,
                    dto.MinQuantity);
                return false;
            }

            if (dto.MaxQuantity < dto.MinQuantity)
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' maxQuantity ({MaxQty}) < minQuantity ({MinQty}) - skipping",
                    dto.Id,
                    dto.MaxQuantity,
                    dto.MinQuantity);
                return false;
            }

            // Validate difficulty class
            if (dto.DifficultyClass <= 0)
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' has invalid difficulty class ({DC}) - skipping",
                    dto.Id,
                    dto.DifficultyClass);
                return false;
            }

            // Validate replenish turns (allow 0 for non-replenishing)
            var replenishTurns = dto.ReplenishTurns >= 0 ? dto.ReplenishTurns : 0;
            if (dto.ReplenishTurns < 0)
            {
                _logger.LogWarning(
                    "Feature '{FeatureId}' has negative replenishTurns ({Turns}) - defaulting to 0",
                    dto.Id,
                    dto.ReplenishTurns);
            }

            // Ensure min quantity is at least 1 for valid harvest
            var minQuantity = dto.MinQuantity > 0 ? dto.MinQuantity : 1;
            if (dto.MinQuantity == 0)
            {
                _logger.LogDebug(
                    "Feature '{FeatureId}' has minQuantity of 0 - using as specified",
                    dto.Id);
                minQuantity = 0;
            }

            // Ensure max quantity is at least min quantity
            var maxQuantity = dto.MaxQuantity >= minQuantity ? dto.MaxQuantity : minQuantity;

            // Create definition
            definition = HarvestableFeatureDefinition.Create(
                dto.Id,
                dto.Name ?? dto.Id,
                dto.Description ?? string.Empty,
                dto.ResourceId,
                minQuantity,
                maxQuantity,
                dto.DifficultyClass,
                dto.RequiredTool,
                replenishTurns);

            // Apply optional icon
            if (!string.IsNullOrEmpty(dto.Icon))
            {
                definition.WithIcon(dto.Icon);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse harvestable feature '{FeatureId}'", dto.Id);
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // IHarvestableFeatureProvider IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public HarvestableFeatureDefinition? GetFeature(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            _logger.LogDebug("GetFeature called with null/empty featureId - returning null");
            return null;
        }

        var found = _features.TryGetValue(featureId, out var definition);

        _logger.LogDebug(
            "GetFeature: {FeatureId} -> {Result}",
            featureId,
            found ? definition!.Name : "not found");

        return definition;
    }

    /// <inheritdoc />
    public IReadOnlyList<HarvestableFeatureDefinition> GetAllFeatures()
    {
        var result = _features.Values.ToList();
        _logger.LogDebug("GetAllFeatures: returning {Count} features", result.Count);
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByResource(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogDebug("GetFeaturesByResource called with null/empty resourceId - returning empty list");
            return [];
        }

        var normalizedId = resourceId.ToLowerInvariant();
        var result = _features.Values
            .Where(f => f.ResourceId.Equals(normalizedId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogDebug(
            "GetFeaturesByResource({ResourceId}): returning {Count} features",
            resourceId,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByTool(string toolId)
    {
        if (string.IsNullOrWhiteSpace(toolId))
        {
            _logger.LogDebug("GetFeaturesByTool called with null/empty toolId - returning empty list");
            return [];
        }

        var normalizedId = toolId.ToLowerInvariant();
        var result = _features.Values
            .Where(f => f.RequiredToolId is not null &&
                       f.RequiredToolId.Equals(normalizedId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogDebug(
            "GetFeaturesByTool({ToolId}): returning {Count} features",
            toolId,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByDifficulty(int minDC, int maxDC)
    {
        var result = _features.Values
            .Where(f => f.DifficultyClass >= minDC && f.DifficultyClass <= maxDC)
            .ToList();

        _logger.LogDebug(
            "GetFeaturesByDifficulty({MinDC}-{MaxDC}): returning {Count} features",
            minDC,
            maxDC,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public bool Exists(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            return false;
        }

        var exists = _features.ContainsKey(featureId);
        _logger.LogDebug("Exists({FeatureId}): {Result}", featureId, exists);
        return exists;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetFeatureIds()
    {
        var result = _features.Keys.ToList();
        _logger.LogDebug("GetFeatureIds: returning {Count} IDs", result.Count);
        return result;
    }

    /// <inheritdoc />
    public int Count => _features.Count;

    /// <inheritdoc />
    public HarvestableFeature? CreateFeatureInstance(string featureId, Random? random = null)
    {
        var definition = GetFeature(featureId);
        if (definition is null)
        {
            _logger.LogDebug(
                "Cannot create instance: feature '{FeatureId}' not found",
                featureId);
            return null;
        }

        var instance = HarvestableFeature.CreateWithRandomQuantity(definition, random);

        _logger.LogDebug(
            "Created feature instance: {FeatureId} with quantity {Quantity}",
            featureId,
            instance.RemainingQuantity);

        return instance;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTOs
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for deserializing harvestable-features.json.
    /// </summary>
    private sealed class HarvestableFeaturesConfigDto
    {
        /// <summary>
        /// Gets or sets the list of harvestable feature entries.
        /// </summary>
        public List<HarvestableFeatureDto>? HarvestableFeatures { get; set; }
    }

    /// <summary>
    /// DTO for a single harvestable feature entry.
    /// </summary>
    private sealed class HarvestableFeatureDto
    {
        /// <summary>
        /// Gets or sets the unique feature identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the resource ID this feature yields.
        /// </summary>
        public string ResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the minimum quantity yield.
        /// </summary>
        public int MinQuantity { get; set; }

        /// <summary>
        /// Gets or sets the maximum quantity yield.
        /// </summary>
        public int MaxQuantity { get; set; }

        /// <summary>
        /// Gets or sets the difficulty class for gathering.
        /// </summary>
        public int DifficultyClass { get; set; }

        /// <summary>
        /// Gets or sets the optional required tool ID.
        /// </summary>
        public string? RequiredTool { get; set; }

        /// <summary>
        /// Gets or sets the replenishment turns (0 = never).
        /// </summary>
        public int ReplenishTurns { get; set; }

        /// <summary>
        /// Gets or sets the optional icon path.
        /// </summary>
        public string? Icon { get; set; }
    }
}
