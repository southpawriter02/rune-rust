using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Configuration;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provider for crafting station definitions with lazy loading and indexed lookups.
/// </summary>
/// <remarks>
/// <para>
/// CraftingStationProvider loads crafting station definitions from application settings
/// (typically bound from appsettings.json or separate config) and provides thread-safe
/// access with multiple indexes for efficient lookups by category and skill.
/// </para>
/// <para>
/// Definitions are loaded lazily on first access, allowing the application to
/// start quickly while deferring the loading cost until stations are needed.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive station ID lookups</description></item>
///   <item><description>Filtering by recipe category for crafting validation</description></item>
///   <item><description>Filtering by crafting skill for skill-based queries</description></item>
///   <item><description>Thread-safe lazy loading with double-checked locking</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // DI registration
/// services.Configure&lt;CraftingStationSettings&gt;(
///     configuration.GetSection("CraftingStations"));
/// services.AddSingleton&lt;ICraftingStationProvider, CraftingStationProvider&gt;();
///
/// // Usage
/// var anvil = stationProvider.GetStation("anvil");
/// var weaponStations = stationProvider.GetStationsForCategory(RecipeCategory.Weapon);
/// </code>
/// </example>
public sealed class CraftingStationProvider : ICraftingStationProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: station ID -> CraftingStationDefinition.
    /// </summary>
    private readonly Dictionary<string, CraftingStationDefinition> _stationDefinitions =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Secondary index: recipe category -> list of stations supporting that category.
    /// </summary>
    private readonly Dictionary<RecipeCategory, List<CraftingStationDefinition>> _byCategory = [];

    /// <summary>
    /// Tertiary index: crafting skill ID -> list of stations using that skill.
    /// </summary>
    private readonly Dictionary<string, List<CraftingStationDefinition>> _bySkill =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// All loaded station definitions as a list for enumeration.
    /// </summary>
    private readonly List<CraftingStationDefinition> _allDefinitions = [];

    /// <summary>
    /// Settings loaded from configuration.
    /// </summary>
    private readonly CraftingStationSettings _settings;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<CraftingStationProvider> _logger;

    /// <summary>
    /// Flag indicating whether definitions have been loaded.
    /// </summary>
    private bool _isLoaded;

    /// <summary>
    /// Lock object for thread-safe lazy loading.
    /// </summary>
    private readonly object _loadLock = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new CraftingStationProvider instance with lazy loading.
    /// </summary>
    /// <param name="settings">The crafting station settings from configuration.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when settings or logger is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Station definitions are not processed until first access. This allows
    /// the application to start quickly while deferring the processing cost.
    /// </para>
    /// <para>
    /// Settings are typically bound from the "CraftingStations" section of configuration:
    /// <code>
    /// services.Configure&lt;CraftingStationSettings&gt;(
    ///     configuration.GetSection("CraftingStations"));
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Direct construction (for testing)
    /// var provider = new CraftingStationProvider(
    ///     Options.Create(settings),
    ///     loggerFactory.CreateLogger&lt;CraftingStationProvider&gt;());
    ///
    /// // Station definitions loaded on first access
    /// var anvil = provider.GetStation("anvil");
    /// </code>
    /// </example>
    public CraftingStationProvider(
        IOptions<CraftingStationSettings> settings,
        ILogger<CraftingStationProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings), "Settings value cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "CraftingStationProvider created with {StationCount} station configurations",
            _settings.Stations?.Count ?? 0);
    }

    // ═══════════════════════════════════════════════════════════════
    // ICraftingStationProvider IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CraftingStationDefinition? GetStation(string stationId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(stationId))
        {
            _logger.LogDebug("GetStation called with null/empty stationId - returning null");
            return null;
        }

        var found = _stationDefinitions.TryGetValue(stationId, out var definition);

        _logger.LogDebug(
            "GetStation: {StationId} -> {Result}",
            stationId,
            found ? "found" : "not found");

        return definition;
    }

    /// <inheritdoc />
    public IReadOnlyList<CraftingStationDefinition> GetAllStations()
    {
        EnsureLoaded();

        _logger.LogDebug("GetAllStations: returning {Count} stations", _allDefinitions.Count);
        return _allDefinitions.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<CraftingStationDefinition> GetStationsForCategory(RecipeCategory category)
    {
        EnsureLoaded();

        if (_byCategory.TryGetValue(category, out var stations))
        {
            _logger.LogDebug(
                "GetStationsForCategory({Category}): returning {Count} stations",
                category,
                stations.Count);
            return stations.AsReadOnly();
        }

        _logger.LogDebug(
            "GetStationsForCategory({Category}): no stations found",
            category);
        return Array.Empty<CraftingStationDefinition>();
    }

    /// <inheritdoc />
    public string? GetCraftingSkill(string stationId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(stationId))
        {
            _logger.LogDebug("GetCraftingSkill called with null/empty stationId - returning null");
            return null;
        }

        if (_stationDefinitions.TryGetValue(stationId, out var definition))
        {
            _logger.LogDebug(
                "GetCraftingSkill({StationId}): {Skill}",
                stationId,
                definition.CraftingSkillId);
            return definition.CraftingSkillId;
        }

        _logger.LogDebug(
            "GetCraftingSkill({StationId}): station not found",
            stationId);
        return null;
    }

    /// <inheritdoc />
    public bool Exists(string stationId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(stationId))
        {
            return false;
        }

        var exists = _stationDefinitions.ContainsKey(stationId);
        _logger.LogDebug("Exists({StationId}): {Result}", stationId, exists);
        return exists;
    }

    /// <inheritdoc />
    public int GetStationCount()
    {
        EnsureLoaded();
        return _stationDefinitions.Count;
    }

    /// <inheritdoc />
    public IReadOnlyList<CraftingStationDefinition> GetStationsBySkill(string skillId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(skillId))
        {
            _logger.LogDebug("GetStationsBySkill called with null/empty skillId - returning empty list");
            return Array.Empty<CraftingStationDefinition>();
        }

        if (_bySkill.TryGetValue(skillId, out var stations))
        {
            _logger.LogDebug(
                "GetStationsBySkill({SkillId}): returning {Count} stations",
                skillId,
                stations.Count);
            return stations.AsReadOnly();
        }

        _logger.LogDebug(
            "GetStationsBySkill({SkillId}): no stations found",
            skillId);
        return Array.Empty<CraftingStationDefinition>();
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures station definitions are loaded from settings.
    /// </summary>
    /// <remarks>
    /// Thread-safe lazy loading using double-checked locking pattern.
    /// </remarks>
    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        lock (_loadLock)
        {
            if (_isLoaded)
            {
                return;
            }

            LoadDefinitions();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads station definitions from settings.
    /// </summary>
    private void LoadDefinitions()
    {
        _logger.LogInformation("Loading crafting station definitions from settings");

        if (_settings.Stations == null || _settings.Stations.Count == 0)
        {
            _logger.LogWarning("No crafting station configurations found in settings");
            return;
        }

        _logger.LogDebug(
            "Processing {Count} crafting station entries",
            _settings.Stations.Count);

        var loadedCount = 0;
        var skippedCount = 0;

        foreach (var dto in _settings.Stations)
        {
            try
            {
                var definition = CreateDefinitionFromDto(dto);

                if (definition != null && ValidateDefinition(definition))
                {
                    AddDefinitionToIndexes(definition);
                    loadedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load station definition: {StationId}", dto?.Id);
                skippedCount++;
            }
        }

        LogLoadingSummary(loadedCount, skippedCount);
    }

    /// <summary>
    /// Creates a CraftingStationDefinition from a DTO.
    /// </summary>
    /// <param name="dto">The DTO from settings.</param>
    /// <returns>The created definition, or null if creation failed.</returns>
    private CraftingStationDefinition? CreateDefinitionFromDto(CraftingStationConfigDto dto)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            _logger.LogWarning("Station config entry missing Id - skipping");
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            _logger.LogWarning("Station config {StationId} missing Name - skipping", dto.Id);
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            _logger.LogWarning("Station config {StationId} missing Description - skipping", dto.Id);
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.CraftingSkill))
        {
            _logger.LogWarning("Station config {StationId} missing CraftingSkill - skipping", dto.Id);
            return null;
        }

        // Parse recipe categories
        var categories = new List<RecipeCategory>();
        if (dto.SupportedCategories == null || dto.SupportedCategories.Count == 0)
        {
            _logger.LogWarning("Station config {StationId} has no supported categories - skipping", dto.Id);
            return null;
        }

        foreach (var categoryStr in dto.SupportedCategories)
        {
            if (Enum.TryParse<RecipeCategory>(categoryStr, ignoreCase: true, out var category))
            {
                categories.Add(category);
                _logger.LogDebug(
                    "Station {StationId}: parsed category '{CategoryStr}' -> {Category}",
                    dto.Id,
                    categoryStr,
                    category);
            }
            else
            {
                _logger.LogWarning(
                    "Station config {StationId} has invalid category '{Category}' - skipping category",
                    dto.Id,
                    categoryStr);
            }
        }

        if (categories.Count == 0)
        {
            _logger.LogWarning(
                "Station config {StationId} has no valid categories after parsing - skipping",
                dto.Id);
            return null;
        }

        // Create the definition
        try
        {
            return CraftingStationDefinition.Create(
                stationId: dto.Id,
                name: dto.Name,
                description: dto.Description,
                supportedCategories: categories,
                craftingSkillId: dto.CraftingSkill,
                iconPath: dto.Icon);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to create CraftingStationDefinition for {StationId}",
                dto.Id);
            return null;
        }
    }

    /// <summary>
    /// Validates a station definition.
    /// </summary>
    /// <param name="definition">The definition to validate.</param>
    /// <returns>True if the definition is valid, false otherwise.</returns>
    private bool ValidateDefinition(CraftingStationDefinition definition)
    {
        // Check for duplicates
        if (_stationDefinitions.ContainsKey(definition.StationId))
        {
            _logger.LogWarning(
                "Duplicate station definition for ID: {StationId} - skipping duplicate",
                definition.StationId);
            return false;
        }

        _logger.LogDebug(
            "Validated station definition: {StationId} ({Name})",
            definition.StationId,
            definition.Name);

        return true;
    }

    /// <summary>
    /// Adds a definition to all lookup indexes.
    /// </summary>
    /// <param name="definition">The definition to index.</param>
    private void AddDefinitionToIndexes(CraftingStationDefinition definition)
    {
        // Primary index
        _stationDefinitions[definition.StationId] = definition;

        // All definitions list
        _allDefinitions.Add(definition);

        // Index by category
        foreach (var category in definition.SupportedCategories)
        {
            if (!_byCategory.TryGetValue(category, out var categoryList))
            {
                categoryList = [];
                _byCategory[category] = categoryList;
            }
            categoryList.Add(definition);
        }

        // Index by skill
        if (!_bySkill.TryGetValue(definition.CraftingSkillId, out var skillList))
        {
            skillList = [];
            _bySkill[definition.CraftingSkillId] = skillList;
        }
        skillList.Add(definition);

        _logger.LogDebug(
            "Loaded station: {StationId} ({Name}) - categories=[{Categories}], skill={Skill}",
            definition.StationId,
            definition.Name,
            definition.GetCategoriesDisplay(),
            definition.CraftingSkillId);
    }

    /// <summary>
    /// Logs a summary of loaded definitions.
    /// </summary>
    /// <param name="loadedCount">Number of successfully loaded definitions.</param>
    /// <param name="skippedCount">Number of skipped/failed definitions.</param>
    private void LogLoadingSummary(int loadedCount, int skippedCount)
    {
        // Log by category
        _logger.LogDebug("Station counts by category:");
        foreach (var (category, stations) in _byCategory.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "  {Category}: {Count} stations ({StationNames})",
                category,
                stations.Count,
                string.Join(", ", stations.Select(s => s.StationId)));
        }

        // Log by skill
        _logger.LogDebug("Station counts by skill:");
        foreach (var (skill, stations) in _bySkill.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "  {Skill}: {Count} stations ({StationNames})",
                skill,
                stations.Count,
                string.Join(", ", stations.Select(s => s.StationId)));
        }

        _logger.LogInformation(
            "CraftingStationProvider initialized: {Loaded} stations loaded, {Skipped} skipped, " +
            "{Categories} categories indexed, {Skills} skills indexed",
            loadedCount,
            skippedCount,
            _byCategory.Count,
            _bySkill.Count);
    }
}
