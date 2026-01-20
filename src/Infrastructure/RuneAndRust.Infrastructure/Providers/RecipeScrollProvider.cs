using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Configuration;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provider for recipe scroll configurations with lazy loading and indexed lookups.
/// </summary>
/// <remarks>
/// <para>
/// RecipeScrollProvider loads recipe scroll configurations from application settings
/// (typically bound from recipe-scrolls.json) and provides thread-safe access with
/// multiple indexes for efficient lookups by level, source, and recipe ID.
/// </para>
/// <para>
/// Configurations are loaded lazily on first access, allowing the application to
/// start quickly while deferring the loading cost until scrolls are needed.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive recipe ID lookups</description></item>
///   <item><description>Filtering by dungeon level for level-appropriate drops</description></item>
///   <item><description>Filtering by loot source type for source-specific drops</description></item>
///   <item><description>Combined level and source filtering for eligible scrolls</description></item>
///   <item><description>Configurable drop chances per loot source</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // DI registration
/// services.Configure&lt;RecipeScrollSettings&gt;(
///     configuration.GetSection("RecipeScrolls"));
/// services.AddSingleton&lt;IRecipeScrollProvider, RecipeScrollProvider&gt;();
///
/// // Usage
/// var eligibleScrolls = scrollProvider.GetEligibleScrolls(5, LootSourceType.Chest);
/// var dropChance = scrollProvider.GetDropChance(LootSourceType.Chest);
/// </code>
/// </example>
public sealed class RecipeScrollProvider : IRecipeScrollProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: recipe ID -> RecipeScrollConfig.
    /// </summary>
    private readonly Dictionary<string, RecipeScrollConfig> _scrollConfigs =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Secondary index: loot source type -> list of configs.
    /// </summary>
    private readonly Dictionary<LootSourceType, List<RecipeScrollConfig>> _bySource = [];

    /// <summary>
    /// Drop chances by loot source type.
    /// </summary>
    private readonly Dictionary<LootSourceType, decimal> _dropChances = [];

    /// <summary>
    /// All loaded scroll configs as a list for enumeration.
    /// </summary>
    private readonly List<RecipeScrollConfig> _allConfigs = [];

    /// <summary>
    /// Settings loaded from configuration.
    /// </summary>
    private readonly RecipeScrollSettings _settings;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<RecipeScrollProvider> _logger;

    /// <summary>
    /// Flag indicating whether configs have been loaded.
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
    /// Creates a new RecipeScrollProvider instance with lazy loading.
    /// </summary>
    /// <param name="settings">The recipe scroll settings from configuration.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when settings or logger is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Scroll configurations are not processed until first access. This allows
    /// the application to start quickly while deferring the processing cost.
    /// </para>
    /// <para>
    /// Settings are typically bound from the "RecipeScrolls" section of configuration:
    /// <code>
    /// services.Configure&lt;RecipeScrollSettings&gt;(
    ///     configuration.GetSection("RecipeScrolls"));
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Direct construction (for testing)
    /// var provider = new RecipeScrollProvider(
    ///     Options.Create(settings),
    ///     loggerFactory.CreateLogger&lt;RecipeScrollProvider&gt;());
    ///
    /// // Scroll configs loaded on first access
    /// var config = provider.GetScrollConfig("steel-sword");
    /// </code>
    /// </example>
    public RecipeScrollProvider(
        IOptions<RecipeScrollSettings> settings,
        ILogger<RecipeScrollProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings), "Settings value cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "RecipeScrollProvider created with {ScrollCount} scroll configs, {ChanceCount} drop chance entries",
            _settings.RecipeScrolls?.Count ?? 0,
            _settings.ScrollDropChances?.Count ?? 0);
    }

    // ═══════════════════════════════════════════════════════════════
    // IRecipeScrollProvider IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public RecipeScrollConfig? GetScrollConfig(string recipeId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(recipeId))
        {
            _logger.LogDebug("GetScrollConfig called with null/empty recipeId - returning null");
            return null;
        }

        var found = _scrollConfigs.TryGetValue(recipeId, out var config);

        _logger.LogDebug(
            "GetScrollConfig: {RecipeId} -> {Result}",
            recipeId,
            found ? "found" : "not found");

        return config;
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeScrollConfig> GetAllScrollConfigs()
    {
        EnsureLoaded();

        _logger.LogDebug("GetAllScrollConfigs: returning {Count} configs", _allConfigs.Count);
        return _allConfigs.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeScrollConfig> GetScrollsForLevel(int dungeonLevel)
    {
        EnsureLoaded();

        var result = _allConfigs
            .Where(config => config.CanDropAtLevel(dungeonLevel))
            .ToList();

        _logger.LogDebug(
            "GetScrollsForLevel({Level}): returning {Count} eligible scrolls",
            dungeonLevel,
            result.Count);

        return result.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeScrollConfig> GetScrollsForSource(LootSourceType source)
    {
        EnsureLoaded();

        if (_bySource.TryGetValue(source, out var configs))
        {
            _logger.LogDebug(
                "GetScrollsForSource({Source}): returning {Count} eligible scrolls",
                source,
                configs.Count);
            return configs.AsReadOnly();
        }

        _logger.LogDebug(
            "GetScrollsForSource({Source}): no scrolls found",
            source);
        return Array.Empty<RecipeScrollConfig>();
    }

    /// <inheritdoc />
    public IReadOnlyList<RecipeScrollConfig> GetEligibleScrolls(int dungeonLevel, LootSourceType source)
    {
        EnsureLoaded();

        var result = _allConfigs
            .Where(config => config.CanDrop(dungeonLevel, source))
            .ToList();

        _logger.LogDebug(
            "GetEligibleScrolls(level={Level}, source={Source}): returning {Count} eligible scrolls",
            dungeonLevel,
            source,
            result.Count);

        return result.AsReadOnly();
    }

    /// <inheritdoc />
    public decimal GetDropChance(LootSourceType source)
    {
        EnsureLoaded();

        if (_dropChances.TryGetValue(source, out var chance))
        {
            _logger.LogDebug(
                "GetDropChance({Source}): {Chance:F4}",
                source,
                chance);
            return chance;
        }

        _logger.LogDebug(
            "GetDropChance({Source}): no chance configured, returning 0",
            source);
        return 0m;
    }

    /// <inheritdoc />
    public bool HasScrollConfig(string recipeId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        var exists = _scrollConfigs.ContainsKey(recipeId);
        _logger.LogDebug("HasScrollConfig({RecipeId}): {Result}", recipeId, exists);
        return exists;
    }

    /// <inheritdoc />
    public int GetScrollCount()
    {
        EnsureLoaded();
        return _scrollConfigs.Count;
    }

    // ═══════════════════════════════════════════════════════════════
    // LOADING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures scroll configurations are loaded from settings.
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

            LoadConfigurations();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads scroll configurations from settings.
    /// </summary>
    private void LoadConfigurations()
    {
        _logger.LogInformation("Loading recipe scroll configurations from settings");

        // Load drop chances first
        LoadDropChances();

        // Load scroll configs
        LoadScrollConfigs();

        // Log summary
        LogLoadingSummary();
    }

    /// <summary>
    /// Loads drop chances from settings.
    /// </summary>
    private void LoadDropChances()
    {
        if (_settings.ScrollDropChances == null || _settings.ScrollDropChances.Count == 0)
        {
            _logger.LogWarning("No scroll drop chances configured");
            return;
        }

        var loadedCount = 0;
        var skippedCount = 0;

        foreach (var (sourceKey, chance) in _settings.ScrollDropChances)
        {
            // Parse loot source type
            if (!Enum.TryParse<LootSourceType>(sourceKey, ignoreCase: true, out var sourceType))
            {
                _logger.LogWarning(
                    "Invalid loot source type in drop chances: '{Source}'",
                    sourceKey);
                skippedCount++;
                continue;
            }

            // Validate chance range
            if (chance < 0 || chance > 1)
            {
                _logger.LogWarning(
                    "Drop chance for {Source} out of range [0, 1]: {Chance}",
                    sourceType,
                    chance);
                skippedCount++;
                continue;
            }

            _dropChances[sourceType] = chance;
            loadedCount++;

            _logger.LogDebug(
                "Loaded drop chance: {Source} = {Chance:P2}",
                sourceType,
                chance);
        }

        _logger.LogInformation(
            "Loaded {Loaded} drop chances, skipped {Skipped}",
            loadedCount,
            skippedCount);
    }

    /// <summary>
    /// Loads scroll configurations from settings.
    /// </summary>
    private void LoadScrollConfigs()
    {
        if (_settings.RecipeScrolls == null || _settings.RecipeScrolls.Count == 0)
        {
            _logger.LogWarning("No recipe scroll configurations found");
            return;
        }

        _logger.LogDebug(
            "Processing {Count} recipe scroll entries",
            _settings.RecipeScrolls.Count);

        var loadedCount = 0;
        var skippedCount = 0;

        foreach (var dto in _settings.RecipeScrolls)
        {
            try
            {
                var config = CreateConfigFromDto(dto);

                if (config != null && ValidateConfig(config))
                {
                    AddConfigToIndexes(config);
                    loadedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load scroll config: {RecipeId}", dto?.RecipeId);
                skippedCount++;
            }
        }

        _logger.LogInformation(
            "Loaded {Loaded} scroll configs, skipped {Skipped}",
            loadedCount,
            skippedCount);
    }

    /// <summary>
    /// Creates a RecipeScrollConfig from a DTO.
    /// </summary>
    /// <param name="dto">The DTO from settings.</param>
    /// <returns>The created config, or null if creation failed.</returns>
    private RecipeScrollConfig? CreateConfigFromDto(RecipeScrollConfigDto dto)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.RecipeId))
        {
            _logger.LogWarning("Scroll config entry missing RecipeId - skipping");
            return null;
        }

        // Validate drop weight
        if (dto.DropWeight < 1)
        {
            _logger.LogWarning(
                "Scroll config {RecipeId} has invalid drop weight {Weight} - defaulting to 1",
                dto.RecipeId,
                dto.DropWeight);
            dto.DropWeight = 1;
        }

        // Validate min level
        if (dto.MinDungeonLevel < 1)
        {
            _logger.LogWarning(
                "Scroll config {RecipeId} has invalid min level {Level} - defaulting to 1",
                dto.RecipeId,
                dto.MinDungeonLevel);
            dto.MinDungeonLevel = 1;
        }

        // Validate max level
        if (dto.MaxDungeonLevel.HasValue && dto.MaxDungeonLevel.Value < dto.MinDungeonLevel)
        {
            _logger.LogWarning(
                "Scroll config {RecipeId} has max level {Max} < min level {Min} - clearing max level",
                dto.RecipeId,
                dto.MaxDungeonLevel,
                dto.MinDungeonLevel);
            dto.MaxDungeonLevel = null;
        }

        // Parse loot sources
        var lootSources = new List<LootSourceType>();
        if (dto.LootSources != null && dto.LootSources.Count > 0)
        {
            foreach (var sourceStr in dto.LootSources)
            {
                if (Enum.TryParse<LootSourceType>(sourceStr, ignoreCase: true, out var sourceType))
                {
                    lootSources.Add(sourceType);
                }
                else
                {
                    _logger.LogWarning(
                        "Scroll config {RecipeId} has invalid loot source '{Source}' - skipping source",
                        dto.RecipeId,
                        sourceStr);
                }
            }
        }

        // Validate base value
        if (dto.BaseValue < 0)
        {
            _logger.LogWarning(
                "Scroll config {RecipeId} has invalid base value {Value} - defaulting to 100",
                dto.RecipeId,
                dto.BaseValue);
            dto.BaseValue = 100;
        }

        // Create the config
        return RecipeScrollConfig.Create(
            recipeId: dto.RecipeId,
            dropWeight: dto.DropWeight,
            minDungeonLevel: dto.MinDungeonLevel,
            maxDungeonLevel: dto.MaxDungeonLevel,
            lootSources: lootSources,
            baseValue: dto.BaseValue);
    }

    /// <summary>
    /// Validates a scroll configuration.
    /// </summary>
    /// <param name="config">The config to validate.</param>
    /// <returns>True if the config is valid, false otherwise.</returns>
    private bool ValidateConfig(RecipeScrollConfig config)
    {
        // Check for duplicates
        if (_scrollConfigs.ContainsKey(config.RecipeId))
        {
            _logger.LogWarning("Duplicate scroll config for recipe: {RecipeId}", config.RecipeId);
            return false;
        }

        // Warn if no loot sources
        if (config.LootSources.Count == 0)
        {
            _logger.LogWarning(
                "Scroll config {RecipeId} has no loot sources - will not drop naturally",
                config.RecipeId);
            // Still valid, just won't drop
        }

        return true;
    }

    /// <summary>
    /// Adds a config to all lookup indexes.
    /// </summary>
    /// <param name="config">The config to index.</param>
    private void AddConfigToIndexes(RecipeScrollConfig config)
    {
        // Primary index
        _scrollConfigs[config.RecipeId] = config;

        // All configs list
        _allConfigs.Add(config);

        // Index by source
        foreach (var source in config.LootSources)
        {
            if (!_bySource.TryGetValue(source, out var sourceList))
            {
                sourceList = [];
                _bySource[source] = sourceList;
            }
            sourceList.Add(config);
        }

        _logger.LogDebug(
            "Loaded scroll config: {RecipeId} (weight={Weight}, levels={Min}-{Max}, sources={Sources})",
            config.RecipeId,
            config.DropWeight,
            config.MinDungeonLevel,
            config.MaxDungeonLevel?.ToString() ?? "∞",
            string.Join(",", config.LootSources));
    }

    /// <summary>
    /// Logs a summary of loaded configurations.
    /// </summary>
    private void LogLoadingSummary()
    {
        // Log by source
        foreach (var (source, configs) in _bySource.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "Source {Source}: {Count} eligible scrolls",
                source,
                configs.Count);
        }

        // Log drop chances
        foreach (var (source, chance) in _dropChances.OrderBy(kvp => kvp.Key))
        {
            _logger.LogDebug(
                "Drop chance {Source}: {Chance:P2}",
                source,
                chance);
        }

        _logger.LogInformation(
            "RecipeScrollProvider initialized: {Total} scroll configs, {Chances} drop chances configured",
            _scrollConfigs.Count,
            _dropChances.Count);
    }
}
