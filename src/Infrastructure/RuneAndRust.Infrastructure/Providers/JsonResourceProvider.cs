using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for resource definitions.
/// </summary>
/// <remarks>
/// <para>
/// JsonResourceProvider loads resource definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with indexed lookups.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "resources": [
///     {
///       "id": "iron-ore",
///       "name": "Iron Ore",
///       "description": "Raw iron ore, can be smelted into iron ingots",
///       "category": "Ore",
///       "quality": "Common",
///       "baseValue": 5,
///       "stackSize": 20,
///       "icon": "icons/resources/iron_ore.png"
///     }
///   ]
/// }
/// </code>
/// <para>
/// The provider builds an index for efficient lookup by resource ID (case-insensitive).
/// </para>
/// </remarks>
public class JsonResourceProvider : IResourceProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: resource ID -> ResourceDefinition.
    /// </summary>
    private readonly Dictionary<string, ResourceDefinition> _resources;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<JsonResourceProvider> _logger;

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonResourceProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the resources.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonResourceProvider(string configPath, ILogger<JsonResourceProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _resources = new Dictionary<string, ResourceDefinition>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Initializing resource provider from configuration file: {ConfigPath}",
            configPath);

        LoadResources();

        _logger.LogInformation(
            "Resource provider initialized successfully with {ResourceCount} resources across {CategoryCount} categories",
            _resources.Count,
            _resources.Values.Select(r => r.Category).Distinct().Count());
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads resource definitions from the JSON configuration file.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is empty or invalid.</exception>
    private void LoadResources()
    {
        _logger.LogDebug("Loading resource definitions from {ConfigPath}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError("Resource configuration file not found: {Path}", _configPath);
            throw new FileNotFoundException(
                $"Resource configuration file not found: {_configPath}",
                _configPath);
        }

        try
        {
            // Read and parse JSON
            var json = File.ReadAllText(_configPath);
            _logger.LogDebug("Read {ByteCount} bytes from configuration file", json.Length);

            var config = JsonSerializer.Deserialize<ResourcesConfigDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate configuration
            if (config?.Resources is null || config.Resources.Count == 0)
            {
                _logger.LogWarning("No resources found in configuration file");
                return;
            }

            _logger.LogDebug("Parsing {Count} resource entries from configuration", config.Resources.Count);

            // Parse each resource entry
            var loadedCount = 0;
            var skippedCount = 0;

            foreach (var resourceDto in config.Resources)
            {
                if (!TryParseResource(resourceDto, out var definition))
                {
                    skippedCount++;
                    continue;
                }

                // Check for duplicates
                if (_resources.ContainsKey(definition!.ResourceId))
                {
                    _logger.LogWarning(
                        "Duplicate resource ID '{ResourceId}' found - skipping duplicate entry",
                        definition.ResourceId);
                    skippedCount++;
                    continue;
                }

                // Add to index
                _resources[definition.ResourceId] = definition;
                loadedCount++;

                _logger.LogDebug(
                    "Loaded resource: {ResourceId} ({Category}, {Quality}, BaseValue={BaseValue})",
                    definition.ResourceId,
                    definition.Category,
                    definition.Quality,
                    definition.BaseValue);
            }

            _logger.LogInformation(
                "Loaded {LoadedCount} resource definitions from configuration ({SkippedCount} skipped)",
                loadedCount,
                skippedCount);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse resource configuration file: {Path}", _configPath);
            throw new InvalidDataException(
                $"Failed to parse resource configuration file: {_configPath}",
                ex);
        }
    }

    /// <summary>
    /// Attempts to parse a resource DTO into a ResourceDefinition.
    /// </summary>
    /// <param name="dto">The DTO to parse.</param>
    /// <param name="definition">The parsed definition, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    private bool TryParseResource(ResourceDto dto, out ResourceDefinition? definition)
    {
        definition = null;

        try
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                _logger.LogWarning("Resource entry missing ID - skipping");
                return false;
            }

            // Parse category
            if (!Enum.TryParse<ResourceCategory>(dto.Category, ignoreCase: true, out var category))
            {
                _logger.LogWarning(
                    "Invalid category '{Category}' for resource '{ResourceId}' - skipping",
                    dto.Category,
                    dto.Id);
                return false;
            }

            // Parse quality
            if (!Enum.TryParse<ResourceQuality>(dto.Quality, ignoreCase: true, out var quality))
            {
                _logger.LogWarning(
                    "Invalid quality '{Quality}' for resource '{ResourceId}' - skipping",
                    dto.Quality,
                    dto.Id);
                return false;
            }

            // Validate base value
            var baseValue = dto.BaseValue > 0 ? dto.BaseValue : 1;
            if (dto.BaseValue <= 0)
            {
                _logger.LogWarning(
                    "Invalid base value '{BaseValue}' for resource '{ResourceId}' - defaulting to 1",
                    dto.BaseValue,
                    dto.Id);
            }

            // Validate stack size
            var stackSize = dto.StackSize > 0 ? dto.StackSize : 20;
            if (dto.StackSize <= 0)
            {
                _logger.LogWarning(
                    "Invalid stack size '{StackSize}' for resource '{ResourceId}' - defaulting to 20",
                    dto.StackSize,
                    dto.Id);
            }

            // Create definition
            definition = ResourceDefinition.Create(
                dto.Id,
                dto.Name ?? dto.Id,
                dto.Description ?? string.Empty,
                category,
                quality,
                baseValue,
                stackSize);

            // Apply optional icon
            if (!string.IsNullOrEmpty(dto.Icon))
            {
                definition.WithIcon(dto.Icon);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse resource '{ResourceId}'", dto.Id);
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // IResourceProvider IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ResourceDefinition? GetResource(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            _logger.LogDebug("GetResource called with null/empty resourceId - returning null");
            return null;
        }

        var found = _resources.TryGetValue(resourceId, out var definition);

        _logger.LogDebug(
            "GetResource: {ResourceId} -> {Result}",
            resourceId,
            found ? definition!.Name : "not found");

        return definition;
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceDefinition> GetAllResources()
    {
        var result = _resources.Values.ToList();
        _logger.LogDebug("GetAllResources: returning {Count} resources", result.Count);
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceDefinition> GetResourcesByCategory(ResourceCategory category)
    {
        var result = _resources.Values
            .Where(r => r.Category == category)
            .ToList();

        _logger.LogDebug(
            "GetResourcesByCategory({Category}): returning {Count} resources",
            category,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceDefinition> GetResourcesByQuality(ResourceQuality quality)
    {
        var result = _resources.Values
            .Where(r => r.Quality == quality)
            .ToList();

        _logger.LogDebug(
            "GetResourcesByQuality({Quality}): returning {Count} resources",
            quality,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceDefinition> GetResources(
        ResourceCategory category,
        ResourceQuality minimumQuality)
    {
        var result = _resources.Values
            .Where(r => r.Category == category && r.Quality >= minimumQuality)
            .ToList();

        _logger.LogDebug(
            "GetResources({Category}, {MinQuality}): returning {Count} resources",
            category,
            minimumQuality,
            result.Count);

        return result;
    }

    /// <inheritdoc />
    public bool Exists(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return false;
        }

        var exists = _resources.ContainsKey(resourceId);
        _logger.LogDebug("Exists({ResourceId}): {Result}", resourceId, exists);
        return exists;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetResourceIds()
    {
        var result = _resources.Keys.ToList();
        _logger.LogDebug("GetResourceIds: returning {Count} IDs", result.Count);
        return result;
    }

    /// <inheritdoc />
    public int Count => _resources.Count;

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTOs
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for deserializing resources.json.
    /// </summary>
    private sealed class ResourcesConfigDto
    {
        public List<ResourceDto>? Resources { get; set; }
    }

    /// <summary>
    /// DTO for a single resource entry.
    /// </summary>
    private sealed class ResourceDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Quality { get; set; } = string.Empty;
        public int BaseValue { get; set; }
        public int StackSize { get; set; }
        public string? Icon { get; set; }
    }
}
