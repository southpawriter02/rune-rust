// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeProvider.cs
// Provider implementation for loading and caching archetype definitions,
// resource bonuses, starting abilities, specialization mappings, and
// recommended builds from JSON configuration.
// Version: 0.17.3e
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
/// Provides access to archetype definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeProvider implements <see cref="IArchetypeProvider"/> to load archetype
/// definitions from config/archetypes.json. Configuration is loaded once on first
/// access and cached in memory for efficient subsequent reads.
/// </para>
/// <para>
/// The provider aggregates all archetype data from v0.17.3a through v0.17.3d:
/// <list type="bullet">
///   <item><description>Archetype definitions (v0.17.3a): Display metadata, combat role, resource type</description></item>
///   <item><description>Resource bonuses (v0.17.3b): HP, Stamina, Aether Pool, Movement, Special</description></item>
///   <item><description>Starting abilities (v0.17.3c): 3 abilities per archetype (Active/Passive/Stance)</description></item>
///   <item><description>Specialization mappings (v0.17.3d): Available specializations and recommended first</description></item>
///   <item><description>Recommended builds (v0.17.3e): Optional attribute allocations for Simple mode</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses double-checked locking to ensure
/// thread-safe initialization. Once initialized, all reads are lock-free.
/// </para>
/// <para>
/// <strong>Configuration Requirements:</strong>
/// <list type="bullet">
///   <item><description>Exactly 4 archetypes must be defined (Warrior, Skirmisher, Mystic, Adept)</description></item>
///   <item><description>No duplicate archetype IDs allowed</description></item>
///   <item><description>All archetype ID strings must be valid Archetype enum values</description></item>
///   <item><description>Each archetype must have required fields and exactly 3 starting abilities</description></item>
///   <item><description>Resource bonus values must be non-negative</description></item>
///   <item><description>Specialization mappings must contain at least 1 specialization</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Logging Levels:</strong>
/// <list type="bullet">
///   <item><description>Information: Provider initialization, cache stats, successful load</description></item>
///   <item><description>Debug: Individual archetype loading, component access, method calls, validation steps</description></item>
///   <item><description>Warning: Missing optional fields, fallback values used, archetype not found</description></item>
///   <item><description>Error: Validation failures, loading errors, configuration not found</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="IArchetypeProvider"/>
/// <seealso cref="ArchetypeDefinition"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="ArchetypeSpecializationMapping"/>
/// <seealso cref="RecommendedBuild"/>
public class ArchetypeProvider : IArchetypeProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expected number of archetypes in a valid configuration.
    /// </summary>
    private const int ExpectedArchetypeCount = 4;

    /// <summary>
    /// Expected number of starting abilities per archetype.
    /// </summary>
    private const int ExpectedAbilityCount = 3;

    /// <summary>
    /// Default configuration file path relative to application root.
    /// </summary>
    private const string DefaultConfigPath = "config/archetypes.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for diagnostic output during provider operations.
    /// </summary>
    private readonly ILogger<ArchetypeProvider> _logger;

    /// <summary>
    /// Path to the archetypes.json configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private readonly object _initLock = new();

    /// <summary>
    /// Cached archetype data keyed by Archetype enum.
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<Archetype, ArchetypeData>? _cache;

    /// <summary>
    /// Flag indicating whether initialization has completed.
    /// </summary>
    private bool _isInitialized;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ArchetypeProvider"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configPath">
    /// Optional configuration file path. Defaults to "config/archetypes.json".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public ArchetypeProvider(
        ILogger<ArchetypeProvider> logger,
        string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "ArchetypeProvider created with config path: {ConfigPath}",
            _configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - IArchetypeProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public ArchetypeDefinition? GetArchetype(Archetype archetype)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(archetype, out var data))
        {
            _logger.LogDebug(
                "GetArchetype found archetype {ArchetypeId}: {DisplayName} ({CombatRole})",
                archetype,
                data.Definition.DisplayName,
                data.Definition.CombatRole);

            return data.Definition;
        }

        _logger.LogWarning(
            "GetArchetype requested unknown archetype: {ArchetypeId}",
            archetype);

        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ArchetypeDefinition> GetAllArchetypes()
    {
        EnsureInitialized();

        var definitions = _cache!.Values
            .Select(a => a.Definition)
            .OrderBy(d => (int)d.ArchetypeId)
            .ToList();

        _logger.LogDebug(
            "GetAllArchetypes returning {Count} archetypes: {ArchetypeList}",
            definitions.Count,
            string.Join(", ", definitions.Select(d => d.DisplayName)));

        return definitions;
    }

    /// <inheritdoc/>
    public ArchetypeResourceBonuses GetResourceBonuses(Archetype archetype)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(archetype, out var data))
        {
            _logger.LogDebug(
                "GetResourceBonuses for {ArchetypeId}: {Summary}",
                archetype,
                data.ResourceBonuses.GetDisplaySummary());

            return data.ResourceBonuses;
        }

        _logger.LogWarning(
            "GetResourceBonuses called for unknown archetype {ArchetypeId}, returning None",
            archetype);

        return ArchetypeResourceBonuses.None;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ArchetypeAbilityGrant> GetStartingAbilities(Archetype archetype)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(archetype, out var data))
        {
            _logger.LogDebug(
                "GetStartingAbilities for {ArchetypeId}: returning {Count} abilities " +
                "({AbilityList})",
                archetype,
                data.StartingAbilities.Count,
                string.Join(", ", data.StartingAbilities.Select(a => a.AbilityName)));

            return data.StartingAbilities;
        }

        _logger.LogWarning(
            "GetStartingAbilities called for unknown archetype {ArchetypeId}, returning empty list",
            archetype);

        return Array.Empty<ArchetypeAbilityGrant>();
    }

    /// <inheritdoc/>
    public ArchetypeSpecializationMapping GetSpecializationMapping(Archetype archetype)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(archetype, out var data))
        {
            _logger.LogDebug(
                "GetSpecializationMapping for {ArchetypeId}: {Count} available " +
                "(recommended: {Recommended})",
                archetype,
                data.SpecializationMapping.Count,
                data.SpecializationMapping.RecommendedFirst);

            return data.SpecializationMapping;
        }

        _logger.LogWarning(
            "GetSpecializationMapping called for unknown archetype {ArchetypeId}, " +
            "falling back to static mapping",
            archetype);

        return ArchetypeSpecializationMapping.GetForArchetype(archetype);
    }

    /// <inheritdoc/>
    public string GetSelectionText(Archetype archetype)
    {
        EnsureInitialized();

        var definition = GetArchetype(archetype);

        if (definition != null)
        {
            _logger.LogDebug(
                "GetSelectionText for {ArchetypeId}: returning {TextLength} characters",
                archetype,
                definition.SelectionText.Length);

            return definition.SelectionText;
        }

        _logger.LogWarning(
            "GetSelectionText called for unknown archetype {ArchetypeId}, returning empty string",
            archetype);

        return string.Empty;
    }

    /// <inheritdoc/>
    public RecommendedBuild? GetRecommendedBuild(Archetype archetype, Lineage? lineage = null)
    {
        EnsureInitialized();

        if (!_cache!.TryGetValue(archetype, out var data))
        {
            _logger.LogWarning(
                "GetRecommendedBuild called for unknown archetype {ArchetypeId}, returning null",
                archetype);
            return null;
        }

        if (data.RecommendedBuilds.Count == 0)
        {
            _logger.LogDebug(
                "GetRecommendedBuild for {ArchetypeId}: no recommended builds configured",
                archetype);
            return null;
        }

        // Try lineage-specific build first
        RecommendedBuild? build = null;
        if (lineage.HasValue)
        {
            build = data.RecommendedBuilds
                .Cast<RecommendedBuild?>()
                .FirstOrDefault(b => b!.Value.OptimalLineage == lineage);

            if (build != null)
            {
                _logger.LogDebug(
                    "GetRecommendedBuild for {ArchetypeId}/{Lineage}: found lineage-specific build '{BuildName}'",
                    archetype, lineage, build.Value.Name);
                return build;
            }
        }

        // Fall back to default build (no lineage)
        build = data.RecommendedBuilds
            .Cast<RecommendedBuild?>()
            .FirstOrDefault(b => !b!.Value.HasOptimalLineage);

        _logger.LogDebug(
            "GetRecommendedBuild for {ArchetypeId}/{Lineage}: {Result}",
            archetype,
            lineage?.ToString() ?? "None",
            build?.Name ?? "No build found");

        return build;
    }

    /// <inheritdoc/>
    public bool IsSpecializationAvailable(Archetype archetype, string specializationId)
    {
        var mapping = GetSpecializationMapping(archetype);
        var available = mapping.IsSpecializationAvailable(specializationId);

        _logger.LogDebug(
            "IsSpecializationAvailable for {ArchetypeId}/{SpecializationId}: {Result}",
            archetype,
            specializationId,
            available);

        return available;
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
                "Initializing ArchetypeProvider from configuration file: {Path}",
                _configPath);

            // Load and validate configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Build the cache
            _cache = BuildCache(config);
            _isInitialized = true;

            _logger.LogInformation(
                "ArchetypeProvider initialized successfully. Loaded {Count} archetypes: {ArchetypeList}",
                _cache.Count,
                string.Join(", ", _cache.Keys));
        }
    }

    /// <summary>
    /// Loads configuration from the JSON file.
    /// </summary>
    /// <returns>The deserialized configuration DTO.</returns>
    /// <exception cref="ArchetypeConfigurationException">
    /// Thrown when file cannot be read or JSON is invalid.
    /// </exception>
    private ArchetypeConfigurationDto LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Archetype configuration file not found: {Path}",
                _configPath);

            throw new ArchetypeConfigurationException(
                $"Archetype configuration file not found: {_configPath}");
        }

        try
        {
            var jsonContent = File.ReadAllText(_configPath);

            _logger.LogDebug(
                "Read {ByteCount} bytes from archetype configuration file",
                jsonContent.Length);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<ArchetypeConfigurationDto>(jsonContent, options);

            if (config == null)
            {
                _logger.LogError("Archetype configuration deserialized to null");
                throw new ArchetypeConfigurationException(
                    "Failed to deserialize archetype configuration: result was null");
            }

            _logger.LogDebug(
                "Configuration loaded successfully. Archetype count: {Count}",
                config.Archetypes.Count);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse archetype configuration JSON: {ErrorMessage}",
                ex.Message);

            throw new ArchetypeConfigurationException(
                $"Invalid JSON in archetype configuration: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates the configuration meets all requirements.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="ArchetypeConfigurationException">
    /// Thrown when validation fails.
    /// </exception>
    private void ValidateConfiguration(ArchetypeConfigurationDto config)
    {
        _logger.LogDebug("Validating archetype configuration");

        // Validate archetype count
        if (config.Archetypes.Count != ExpectedArchetypeCount)
        {
            _logger.LogError(
                "Invalid archetype count: expected {Expected}, found {Actual}",
                ExpectedArchetypeCount,
                config.Archetypes.Count);

            throw new ArchetypeConfigurationException(
                $"Expected exactly {ExpectedArchetypeCount} archetypes, found {config.Archetypes.Count}");
        }

        // Validate no duplicates and all valid enum values
        var seenArchetypes = new HashSet<Archetype>();
        foreach (var dto in config.Archetypes)
        {
            // Parse enum value
            if (!Enum.TryParse<Archetype>(dto.ArchetypeId, ignoreCase: true, out var archetype))
            {
                _logger.LogError(
                    "Invalid archetype ID: {ArchetypeId}. Valid values: {ValidValues}",
                    dto.ArchetypeId,
                    string.Join(", ", Enum.GetNames<Archetype>()));

                throw new ArchetypeConfigurationException(
                    $"Invalid archetype ID: '{dto.ArchetypeId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<Archetype>())}");
            }

            // Check for duplicates
            if (!seenArchetypes.Add(archetype))
            {
                _logger.LogError(
                    "Duplicate archetype ID in configuration: {ArchetypeId}",
                    archetype);

                throw new ArchetypeConfigurationException(
                    $"Duplicate archetype ID in configuration: {archetype}");
            }

            // Validate primary resource
            if (!Enum.TryParse<ResourceType>(dto.PrimaryResource, ignoreCase: true, out _))
            {
                _logger.LogError(
                    "Invalid primary resource '{PrimaryResource}' for archetype {ArchetypeId}. " +
                    "Valid values: {ValidValues}",
                    dto.PrimaryResource,
                    dto.ArchetypeId,
                    string.Join(", ", Enum.GetNames<ResourceType>()));

                throw new ArchetypeConfigurationException(
                    $"Invalid primary resource '{dto.PrimaryResource}' for archetype '{dto.ArchetypeId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<ResourceType>())}");
            }

            // Validate starting abilities count
            if (dto.StartingAbilities.Count != ExpectedAbilityCount)
            {
                _logger.LogError(
                    "Invalid starting ability count for {ArchetypeId}: expected {Expected}, found {Actual}",
                    dto.ArchetypeId,
                    ExpectedAbilityCount,
                    dto.StartingAbilities.Count);

                throw new ArchetypeConfigurationException(
                    $"Expected exactly {ExpectedAbilityCount} starting abilities for '{dto.ArchetypeId}', " +
                    $"found {dto.StartingAbilities.Count}");
            }

            // Validate starting ability types
            foreach (var ability in dto.StartingAbilities)
            {
                if (!Enum.TryParse<AbilityType>(ability.AbilityType, ignoreCase: true, out _))
                {
                    _logger.LogError(
                        "Invalid ability type '{AbilityType}' for ability '{AbilityId}' " +
                        "in archetype {ArchetypeId}. Valid values: {ValidValues}",
                        ability.AbilityType,
                        ability.AbilityId,
                        dto.ArchetypeId,
                        string.Join(", ", Enum.GetNames<AbilityType>()));

                    throw new ArchetypeConfigurationException(
                        $"Invalid ability type '{ability.AbilityType}' for ability '{ability.AbilityId}' " +
                        $"in archetype '{dto.ArchetypeId}'. " +
                        $"Valid values: {string.Join(", ", Enum.GetNames<AbilityType>())}");
                }
            }

            // Validate specializations
            if (dto.AvailableSpecializations.Specializations.Count == 0)
            {
                _logger.LogError(
                    "Archetype {ArchetypeId} has no available specializations defined",
                    dto.ArchetypeId);

                throw new ArchetypeConfigurationException(
                    $"Archetype '{dto.ArchetypeId}' must have at least 1 available specialization");
            }

            _logger.LogDebug(
                "Validated archetype: {ArchetypeId} -> {DisplayName} ({CombatRole}), " +
                "{AbilityCount} abilities, {SpecCount} specializations",
                dto.ArchetypeId,
                dto.DisplayName,
                dto.CombatRole,
                dto.StartingAbilities.Count,
                dto.AvailableSpecializations.Specializations.Count);
        }

        // Verify all expected archetypes present
        var allArchetypes = Enum.GetValues<Archetype>();
        var missingArchetypes = allArchetypes.Except(seenArchetypes).ToList();
        if (missingArchetypes.Count > 0)
        {
            _logger.LogError(
                "Missing archetypes in configuration: {MissingArchetypes}",
                string.Join(", ", missingArchetypes));

            throw new ArchetypeConfigurationException(
                $"Missing archetypes in configuration: {string.Join(", ", missingArchetypes)}");
        }

        _logger.LogDebug("Archetype configuration validation passed");
    }

    /// <summary>
    /// Builds the archetype cache from validated configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    /// <returns>Dictionary mapping Archetype enum to ArchetypeData.</returns>
    private Dictionary<Archetype, ArchetypeData> BuildCache(ArchetypeConfigurationDto config)
    {
        _logger.LogDebug("Building archetype cache");

        var cache = new Dictionary<Archetype, ArchetypeData>();

        foreach (var dto in config.Archetypes)
        {
            var archetype = Enum.Parse<Archetype>(dto.ArchetypeId, ignoreCase: true);
            var data = MapDtoToArchetypeData(dto, archetype);
            cache[archetype] = data;

            _logger.LogDebug(
                "Cached archetype: {ArchetypeId} -> {DisplayName} " +
                "(role: {CombatRole}, resource: {PrimaryResource}, " +
                "abilities: {AbilityCount}, specializations: {SpecCount}, " +
                "builds: {BuildCount})",
                archetype,
                data.Definition.DisplayName,
                data.Definition.CombatRole,
                data.Definition.PrimaryResource,
                data.StartingAbilities.Count,
                data.SpecializationMapping.Count,
                data.RecommendedBuilds.Count);
        }

        _logger.LogDebug("Cache built with {Count} archetypes", cache.Count);
        return cache;
    }

    /// <summary>
    /// Maps a DTO to a composite ArchetypeData record.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <param name="archetype">The parsed Archetype enum value.</param>
    /// <returns>A new ArchetypeData record containing all archetype components.</returns>
    private ArchetypeData MapDtoToArchetypeData(ArchetypeDefinitionDto dto, Archetype archetype)
    {
        _logger.LogDebug(
            "Mapping DTO to ArchetypeData for {ArchetypeId}",
            archetype);

        // Map core definition (v0.17.3a)
        var definition = MapDefinition(dto, archetype);

        // Map resource bonuses (v0.17.3b)
        var resourceBonuses = MapResourceBonuses(dto, archetype);

        // Map starting abilities (v0.17.3c)
        var startingAbilities = MapStartingAbilities(dto, archetype);

        // Map specialization mapping (v0.17.3d)
        var specializationMapping = MapSpecializationMapping(dto, archetype);

        // Map recommended builds (v0.17.3e)
        var recommendedBuilds = MapRecommendedBuilds(dto, archetype);

        _logger.LogDebug(
            "Successfully mapped ArchetypeData for {ArchetypeId}: " +
            "{DisplayName} (HP+{HpBonus}, {AbilityCount} abilities, " +
            "{SpecCount} specs, {BuildCount} builds)",
            archetype,
            definition.DisplayName,
            resourceBonuses.MaxHpBonus,
            startingAbilities.Count,
            specializationMapping.Count,
            recommendedBuilds.Count);

        return new ArchetypeData(
            definition,
            resourceBonuses,
            startingAbilities,
            specializationMapping,
            recommendedBuilds);
    }

    /// <summary>
    /// Maps the core definition fields from a DTO to an ArchetypeDefinition entity.
    /// </summary>
    /// <param name="dto">The DTO containing definition fields.</param>
    /// <param name="archetype">The parsed Archetype enum value.</param>
    /// <returns>A new ArchetypeDefinition entity.</returns>
    private ArchetypeDefinition MapDefinition(ArchetypeDefinitionDto dto, Archetype archetype)
    {
        _logger.LogDebug(
            "Mapping definition for {ArchetypeId}: {DisplayName} ({CombatRole})",
            archetype,
            dto.DisplayName,
            dto.CombatRole);

        var primaryResource = Enum.Parse<ResourceType>(dto.PrimaryResource, ignoreCase: true);

        return ArchetypeDefinition.Create(
            archetypeId: archetype,
            displayName: dto.DisplayName,
            tagline: dto.Tagline,
            description: dto.Description,
            selectionText: dto.SelectionText,
            combatRole: dto.CombatRole,
            primaryResource: primaryResource,
            playstyleDescription: dto.PlaystyleDescription);
    }

    /// <summary>
    /// Maps resource bonus fields from a DTO to an ArchetypeResourceBonuses value object.
    /// </summary>
    /// <param name="dto">The DTO containing resource bonus fields.</param>
    /// <param name="archetype">The archetype for logging context.</param>
    /// <returns>An ArchetypeResourceBonuses value object.</returns>
    private ArchetypeResourceBonuses MapResourceBonuses(ArchetypeDefinitionDto dto, Archetype archetype)
    {
        var rb = dto.ResourceBonuses;

        _logger.LogDebug(
            "Mapping resource bonuses for {ArchetypeId}: " +
            "HP+{HpBonus}, Stamina+{StaminaBonus}, AP+{ApBonus}, Movement+{MoveBonus}, " +
            "HasSpecial={HasSpecial}",
            archetype,
            rb.MaxHpBonus,
            rb.MaxStaminaBonus,
            rb.MaxAetherPoolBonus,
            rb.MovementBonus,
            rb.SpecialBonus != null);

        ArchetypeSpecialBonus? specialBonus = null;
        if (rb.SpecialBonus != null)
        {
            specialBonus = ArchetypeSpecialBonus.Create(
                rb.SpecialBonus.BonusType,
                rb.SpecialBonus.BonusValue,
                rb.SpecialBonus.Description);

            _logger.LogDebug(
                "Mapped special bonus for {ArchetypeId}: {BonusType} +{BonusValue:P0}",
                archetype,
                rb.SpecialBonus.BonusType,
                rb.SpecialBonus.BonusValue);
        }

        return ArchetypeResourceBonuses.Create(
            rb.MaxHpBonus,
            rb.MaxStaminaBonus,
            rb.MaxAetherPoolBonus,
            rb.MovementBonus,
            specialBonus);
    }

    /// <summary>
    /// Maps starting ability fields from a DTO to a list of ArchetypeAbilityGrant value objects.
    /// </summary>
    /// <param name="dto">The DTO containing starting ability fields.</param>
    /// <param name="archetype">The archetype for logging context.</param>
    /// <returns>List of ArchetypeAbilityGrant value objects.</returns>
    private List<ArchetypeAbilityGrant> MapStartingAbilities(ArchetypeDefinitionDto dto, Archetype archetype)
    {
        _logger.LogDebug(
            "Mapping {Count} starting abilities for {ArchetypeId}",
            dto.StartingAbilities.Count,
            archetype);

        var abilities = new List<ArchetypeAbilityGrant>();

        foreach (var abilityDto in dto.StartingAbilities)
        {
            var abilityType = Enum.Parse<AbilityType>(abilityDto.AbilityType, ignoreCase: true);

            var grant = ArchetypeAbilityGrant.Create(
                abilityDto.AbilityId,
                abilityDto.AbilityName,
                abilityType,
                abilityDto.Description);

            abilities.Add(grant);

            _logger.LogDebug(
                "Mapped ability for {ArchetypeId}: {AbilityId} -> {AbilityName} ({AbilityType})",
                archetype,
                abilityDto.AbilityId,
                abilityDto.AbilityName,
                abilityType);
        }

        return abilities;
    }

    /// <summary>
    /// Maps specialization mapping fields from a DTO to an ArchetypeSpecializationMapping value object.
    /// </summary>
    /// <param name="dto">The DTO containing specialization fields.</param>
    /// <param name="archetype">The parsed Archetype enum value.</param>
    /// <returns>An ArchetypeSpecializationMapping value object.</returns>
    private ArchetypeSpecializationMapping MapSpecializationMapping(
        ArchetypeDefinitionDto dto,
        Archetype archetype)
    {
        var specDto = dto.AvailableSpecializations;

        _logger.LogDebug(
            "Mapping specialization mapping for {ArchetypeId}: " +
            "{Count} specializations, recommended: {Recommended}",
            archetype,
            specDto.Specializations.Count,
            specDto.RecommendedFirst);

        return ArchetypeSpecializationMapping.Create(
            archetype,
            specDto.Specializations,
            specDto.RecommendedFirst);
    }

    /// <summary>
    /// Maps optional recommended build fields from a DTO to a list of RecommendedBuild value objects.
    /// </summary>
    /// <param name="dto">The DTO containing optional recommended build fields.</param>
    /// <param name="archetype">The archetype for logging context.</param>
    /// <returns>List of RecommendedBuild value objects (may be empty).</returns>
    private List<RecommendedBuild> MapRecommendedBuilds(ArchetypeDefinitionDto dto, Archetype archetype)
    {
        if (dto.RecommendedBuilds == null || dto.RecommendedBuilds.Count == 0)
        {
            _logger.LogDebug(
                "No recommended builds configured for {ArchetypeId}",
                archetype);

            return new List<RecommendedBuild>();
        }

        _logger.LogDebug(
            "Mapping {Count} recommended builds for {ArchetypeId}",
            dto.RecommendedBuilds.Count,
            archetype);

        var builds = new List<RecommendedBuild>();

        foreach (var buildDto in dto.RecommendedBuilds)
        {
            Lineage? optimalLineage = null;
            if (buildDto.OptimalLineage != null)
            {
                if (Enum.TryParse<Lineage>(buildDto.OptimalLineage, ignoreCase: true, out var parsedLineage))
                {
                    optimalLineage = parsedLineage;
                }
                else
                {
                    _logger.LogWarning(
                        "Unknown optimal lineage '{OptimalLineage}' in recommended build " +
                        "'{BuildName}' for archetype {ArchetypeId}. Treating as default build.",
                        buildDto.OptimalLineage,
                        buildDto.Name,
                        archetype);
                }
            }

            var build = RecommendedBuild.Create(
                buildDto.Name,
                buildDto.Might,
                buildDto.Finesse,
                buildDto.Wits,
                buildDto.Will,
                buildDto.Sturdiness,
                optimalLineage);

            builds.Add(build);

            _logger.LogDebug(
                "Mapped recommended build for {ArchetypeId}: '{BuildName}' " +
                "[M{Might} F{Finesse} Wi{Wits} Wl{Will} S{Sturdiness}] " +
                "OptimalLineage={OptimalLineage}",
                archetype,
                build.Name,
                build.Might,
                build.Finesse,
                build.Wits,
                build.Will,
                build.Sturdiness,
                build.OptimalLineage?.ToString() ?? "None");
        }

        return builds;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE TYPES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal record for cached archetype data, aggregating all components.
    /// </summary>
    /// <param name="Definition">The core archetype definition (v0.17.3a).</param>
    /// <param name="ResourceBonuses">Resource pool bonuses (v0.17.3b).</param>
    /// <param name="StartingAbilities">Starting ability grants (v0.17.3c).</param>
    /// <param name="SpecializationMapping">Available specializations (v0.17.3d).</param>
    /// <param name="RecommendedBuilds">Optional recommended attribute builds (v0.17.3e).</param>
    private sealed record ArchetypeData(
        ArchetypeDefinition Definition,
        ArchetypeResourceBonuses ResourceBonuses,
        IReadOnlyList<ArchetypeAbilityGrant> StartingAbilities,
        ArchetypeSpecializationMapping SpecializationMapping,
        IReadOnlyList<RecommendedBuild> RecommendedBuilds);
}
