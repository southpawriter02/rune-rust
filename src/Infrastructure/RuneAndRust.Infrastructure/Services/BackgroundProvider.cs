// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundProvider.cs
// Provider implementation for loading and caching background definitions from JSON.
// Version: 0.17.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to background definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// BackgroundProvider implements <see cref="IBackgroundProvider"/> to load background
/// definitions from config/backgrounds.json. Configuration is loaded once on first
/// access and cached in memory for efficient subsequent reads.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses double-checked locking to ensure
/// thread-safe initialization. Once initialized, all reads are lock-free.
/// </para>
/// <para>
/// <strong>Configuration Requirements:</strong>
/// <list type="bullet">
///   <item><description>Exactly 6 backgrounds must be defined</description></item>
///   <item><description>No duplicate background IDs allowed</description></item>
///   <item><description>All background ID strings must be valid Background enum values</description></item>
///   <item><description>Each background must have required fields (displayName, description, etc.)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Logging Levels:</strong>
/// <list type="bullet">
///   <item><description>Information: Provider initialization, cache stats, successful load</description></item>
///   <item><description>Debug: Individual background loading, component access, method calls</description></item>
///   <item><description>Warning: Missing optional fields, fallback values used, background not found</description></item>
///   <item><description>Error: Validation failures, loading errors, configuration not found</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="IBackgroundProvider"/>
/// <seealso cref="BackgroundDefinition"/>
public class BackgroundProvider : IBackgroundProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expected number of backgrounds in a valid configuration.
    /// </summary>
    private const int ExpectedBackgroundCount = 6;

    /// <summary>
    /// Default configuration file path relative to application root.
    /// </summary>
    private const string DefaultConfigPath = "config/backgrounds.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for diagnostic output during provider operations.
    /// </summary>
    private readonly ILogger<BackgroundProvider> _logger;

    /// <summary>
    /// Path to the backgrounds.json configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private readonly object _initLock = new();

    /// <summary>
    /// Cached background definitions keyed by Background enum.
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<Background, BackgroundDefinition>? _cache;

    /// <summary>
    /// Flag indicating whether initialization has completed.
    /// </summary>
    private bool _isInitialized;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="BackgroundProvider"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configPath">
    /// Optional configuration file path. Defaults to "config/backgrounds.json".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public BackgroundProvider(
        ILogger<BackgroundProvider> logger,
        string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "BackgroundProvider created with config path: {ConfigPath}",
            _configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - IBackgroundProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<BackgroundDefinition> GetAllBackgrounds()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetAllBackgrounds returning {Count} backgrounds",
            _cache!.Count);

        return _cache!.Values.ToList();
    }

    /// <inheritdoc/>
    public BackgroundDefinition? GetBackground(Background backgroundId)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(backgroundId, out var definition))
        {
            _logger.LogDebug(
                "GetBackground found background {BackgroundId}: {DisplayName}",
                backgroundId,
                definition.DisplayName);

            return definition;
        }

        _logger.LogWarning(
            "GetBackground requested unknown background: {BackgroundId}",
            backgroundId);

        return null;
    }

    /// <inheritdoc/>
    public string GetSelectionText(Background backgroundId)
    {
        EnsureInitialized();

        var definition = GetBackground(backgroundId);

        if (definition != null)
        {
            _logger.LogDebug(
                "GetSelectionText for {BackgroundId}: returning {TextLength} characters",
                backgroundId,
                definition.SelectionText.Length);

            return definition.SelectionText;
        }

        _logger.LogWarning(
            "GetSelectionText called for unknown background {BackgroundId}, returning default message",
            backgroundId);

        return $"Unknown background: {backgroundId}";
    }

    /// <inheritdoc/>
    public IReadOnlyList<BackgroundSkillGrant> GetSkillGrants(Background backgroundId)
    {
        EnsureInitialized();

        var definition = GetBackground(backgroundId);

        if (definition != null)
        {
            _logger.LogDebug(
                "GetSkillGrants for {BackgroundId}: returning {Count} grants (total bonus: {TotalBonus})",
                backgroundId,
                definition.SkillGrants.Count,
                definition.GetTotalSkillBonus());

            return definition.SkillGrants;
        }

        _logger.LogWarning(
            "GetSkillGrants called for unknown background {BackgroundId}, returning empty list",
            backgroundId);

        return Array.Empty<BackgroundSkillGrant>();
    }

    /// <inheritdoc/>
    public IReadOnlyList<BackgroundEquipmentGrant> GetEquipmentGrants(Background backgroundId)
    {
        EnsureInitialized();

        var definition = GetBackground(backgroundId);

        if (definition != null)
        {
            _logger.LogDebug(
                "GetEquipmentGrants for {BackgroundId}: returning {Count} grants " +
                "(equipped: {EquippedCount}, inventory: {InventoryCount}, total items: {TotalItems})",
                backgroundId,
                definition.EquipmentGrants.Count,
                definition.GetEquippedItems().Count(),
                definition.GetInventoryItems().Count(),
                definition.GetTotalItemCount());

            return definition.EquipmentGrants;
        }

        _logger.LogWarning(
            "GetEquipmentGrants called for unknown background {BackgroundId}, returning empty list",
            backgroundId);

        return Array.Empty<BackgroundEquipmentGrant>();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetNarrativeHooks(Background backgroundId)
    {
        EnsureInitialized();

        var definition = GetBackground(backgroundId);

        if (definition != null)
        {
            _logger.LogDebug(
                "GetNarrativeHooks for {BackgroundId}: returning {Count} hooks",
                backgroundId,
                definition.NarrativeHooks.Count);

            return definition.NarrativeHooks;
        }

        _logger.LogWarning(
            "GetNarrativeHooks called for unknown background {BackgroundId}, returning empty list",
            backgroundId);

        return Array.Empty<string>();
    }

    /// <inheritdoc/>
    public bool HasBackground(Background backgroundId)
    {
        EnsureInitialized();

        var exists = _cache!.ContainsKey(backgroundId);

        _logger.LogDebug(
            "HasBackground check for {BackgroundId}: {Exists}",
            backgroundId,
            exists);

        return exists;
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
                "Initializing BackgroundProvider from configuration file: {Path}",
                _configPath);

            // Load and validate configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Build the cache
            _cache = BuildCache(config);
            _isInitialized = true;

            _logger.LogInformation(
                "BackgroundProvider initialized successfully. Loaded {Count} backgrounds: {BackgroundList}",
                _cache.Count,
                string.Join(", ", _cache.Keys));
        }
    }

    /// <summary>
    /// Loads configuration from the JSON file.
    /// </summary>
    /// <returns>The deserialized configuration DTO.</returns>
    /// <exception cref="BackgroundConfigurationException">
    /// Thrown when file cannot be read or JSON is invalid.
    /// </exception>
    private BackgroundConfigurationDto LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Background configuration file not found: {Path}",
                _configPath);

            throw new BackgroundConfigurationException(
                $"Background configuration file not found: {_configPath}");
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

            var config = JsonSerializer.Deserialize<BackgroundConfigurationDto>(jsonContent, options);

            if (config == null)
            {
                _logger.LogError("Configuration deserialized to null");
                throw new BackgroundConfigurationException(
                    "Failed to deserialize background configuration: result was null");
            }

            _logger.LogDebug(
                "Configuration loaded successfully. Background count: {Count}",
                config.Backgrounds.Count);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse background configuration JSON: {ErrorMessage}",
                ex.Message);

            throw new BackgroundConfigurationException(
                $"Invalid JSON in background configuration: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates the configuration meets all requirements.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="BackgroundConfigurationException">
    /// Thrown when validation fails.
    /// </exception>
    private void ValidateConfiguration(BackgroundConfigurationDto config)
    {
        _logger.LogDebug("Validating background configuration");

        // Validate background count
        if (config.Backgrounds.Count != ExpectedBackgroundCount)
        {
            _logger.LogError(
                "Invalid background count: expected {Expected}, found {Actual}",
                ExpectedBackgroundCount,
                config.Backgrounds.Count);

            throw new BackgroundConfigurationException(
                $"Expected exactly {ExpectedBackgroundCount} backgrounds, found {config.Backgrounds.Count}");
        }

        // Validate no duplicates and all valid enum values
        var seenBackgrounds = new HashSet<Background>();
        foreach (var dto in config.Backgrounds)
        {
            // Parse enum value
            if (!Enum.TryParse<Background>(dto.BackgroundId, out var background))
            {
                _logger.LogError(
                    "Invalid background ID: {BackgroundId}. Valid values: {ValidValues}",
                    dto.BackgroundId,
                    string.Join(", ", Enum.GetNames<Background>()));

                throw new BackgroundConfigurationException(
                    $"Invalid background ID: '{dto.BackgroundId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<Background>())}");
            }

            // Check for duplicates
            if (!seenBackgrounds.Add(background))
            {
                _logger.LogError(
                    "Duplicate background ID in configuration: {BackgroundId}",
                    background);

                throw new BackgroundConfigurationException(
                    $"Duplicate background ID in configuration: {background}");
            }

            _logger.LogDebug(
                "Validated background: {BackgroundId} -> {DisplayName}",
                dto.BackgroundId,
                dto.DisplayName);
        }

        // Verify all expected backgrounds present
        var allBackgrounds = Enum.GetValues<Background>();
        var missingBackgrounds = allBackgrounds.Except(seenBackgrounds).ToList();
        if (missingBackgrounds.Count > 0)
        {
            _logger.LogError(
                "Missing backgrounds in configuration: {MissingBackgrounds}",
                string.Join(", ", missingBackgrounds));

            throw new BackgroundConfigurationException(
                $"Missing backgrounds in configuration: {string.Join(", ", missingBackgrounds)}");
        }

        _logger.LogDebug("Configuration validation passed");
    }

    /// <summary>
    /// Builds the background cache from validated configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    /// <returns>Dictionary mapping Background enum to BackgroundDefinition.</returns>
    private Dictionary<Background, BackgroundDefinition> BuildCache(BackgroundConfigurationDto config)
    {
        _logger.LogDebug("Building background cache");

        var cache = new Dictionary<Background, BackgroundDefinition>();

        foreach (var dto in config.Backgrounds)
        {
            var background = Enum.Parse<Background>(dto.BackgroundId);
            var definition = MapDtoToDefinition(dto, background);
            cache[background] = definition;

            _logger.LogDebug(
                "Cached background: {BackgroundId} -> {DisplayName} " +
                "(skills: {SkillCount}, equipment: {EquipmentCount}, hooks: {HookCount})",
                background,
                definition.DisplayName,
                definition.SkillGrants.Count,
                definition.EquipmentGrants.Count,
                definition.NarrativeHooks.Count);
        }

        _logger.LogDebug("Cache built with {Count} backgrounds", cache.Count);
        return cache;
    }

    /// <summary>
    /// Maps a DTO to a domain BackgroundDefinition entity.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <param name="background">The parsed Background enum value.</param>
    /// <returns>A new BackgroundDefinition entity.</returns>
    private BackgroundDefinition MapDtoToDefinition(BackgroundDefinitionDto dto, Background background)
    {
        _logger.LogDebug(
            "Mapping DTO to BackgroundDefinition for {BackgroundId}",
            background);

        // Map skill grants from DTOs to domain value objects
        var skillGrants = dto.SkillGrants?
            .Select(s =>
            {
                if (!Enum.TryParse<SkillGrantType>(s.GrantType, out var grantType))
                {
                    _logger.LogWarning(
                        "Unknown skill grant type '{GrantType}' for background {BackgroundId}, " +
                        "skill '{SkillId}'. Defaulting to Permanent",
                        s.GrantType,
                        background,
                        s.SkillId);
                    grantType = SkillGrantType.Permanent;
                }

                _logger.LogDebug(
                    "Mapping skill grant for {BackgroundId}: {SkillId} +{BonusAmount} ({GrantType})",
                    background,
                    s.SkillId,
                    s.BonusAmount,
                    grantType);

                return BackgroundSkillGrant.Create(s.SkillId, s.BonusAmount, grantType);
            })
            .ToList() ?? new List<BackgroundSkillGrant>();

        // Map equipment grants from DTOs to domain value objects
        var equipmentGrants = dto.EquipmentGrants?
            .Select(e =>
            {
                EquipmentSlot? slot = null;
                if (e.Slot != null)
                {
                    if (!Enum.TryParse<EquipmentSlot>(e.Slot, out var parsedSlot))
                    {
                        _logger.LogWarning(
                            "Unknown equipment slot '{Slot}' for background {BackgroundId}, " +
                            "item '{ItemId}'. Setting slot to null",
                            e.Slot,
                            background,
                            e.ItemId);
                    }
                    else
                    {
                        slot = parsedSlot;
                    }
                }

                _logger.LogDebug(
                    "Mapping equipment grant for {BackgroundId}: {ItemId} x{Quantity} " +
                    "(equipped: {IsEquipped}, slot: {Slot})",
                    background,
                    e.ItemId,
                    e.Quantity,
                    e.IsEquipped,
                    slot?.ToString() ?? "none");

                return BackgroundEquipmentGrant.Create(e.ItemId, e.Quantity, e.IsEquipped, slot);
            })
            .ToList() ?? new List<BackgroundEquipmentGrant>();

        // Map narrative hooks (simple string list, no transformation needed)
        var narrativeHooks = dto.NarrativeHooks ?? new List<string>();

        if (narrativeHooks.Count == 0)
        {
            _logger.LogWarning(
                "Background {BackgroundId} has no narrative hooks defined",
                background);
        }

        // Create the domain entity via factory method
        var definition = BackgroundDefinition.Create(
            backgroundId: background,
            displayName: dto.DisplayName,
            description: dto.Description,
            selectionText: dto.SelectionText,
            professionBefore: dto.ProfessionBefore,
            socialStanding: dto.SocialStanding,
            narrativeHooks: narrativeHooks,
            skillGrants: skillGrants,
            equipmentGrants: equipmentGrants);

        _logger.LogDebug(
            "Successfully mapped BackgroundDefinition for {BackgroundId}: " +
            "{DisplayName} (profession: '{Profession}', standing: '{Standing}')",
            background,
            definition.DisplayName,
            definition.ProfessionBefore,
            definition.SocialStanding);

        return definition;
    }
}
