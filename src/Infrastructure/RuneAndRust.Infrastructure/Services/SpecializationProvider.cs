// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationProvider.cs
// Provider implementation for loading and caching specialization definitions,
// ability tiers, special resources, and path type classifications from JSON
// configuration.
// Version: 0.17.4d
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
/// Provides access to specialization definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationProvider implements <see cref="ISpecializationProvider"/> to load
/// specialization definitions from config/specializations.json. Configuration is
/// loaded once on first access and cached in memory for efficient subsequent reads.
/// </para>
/// <para>
/// The provider aggregates all specialization data from v0.17.4a through v0.17.4c:
/// <list type="bullet">
///   <item><description>Specialization definitions (v0.17.4b): Display metadata, parent archetype, unlock cost</description></item>
///   <item><description>Special resources (v0.17.4b): Unique per-specialization combat resources (5 of 17)</description></item>
///   <item><description>Ability tiers (v0.17.4c): Three-tier progression with escalating PP costs</description></item>
///   <item><description>Path type classification (v0.17.4a): Coherent vs Heretical via enum extensions</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses double-checked locking to ensure
/// thread-safe initialization. Once initialized, all reads are lock-free.
/// </para>
/// <para>
/// <strong>Configuration Requirements:</strong>
/// <list type="bullet">
///   <item><description>Exactly 17 specialization definitions must be present</description></item>
///   <item><description>No duplicate specialization IDs allowed</description></item>
///   <item><description>All specialization ID strings must be valid SpecializationId enum values</description></item>
///   <item><description>All parent archetype strings must be valid Archetype enum values</description></item>
///   <item><description>All path type strings must be valid SpecializationPathType enum values</description></item>
///   <item><description>Path types must match the specialization's inherent classification from enum extensions</description></item>
///   <item><description>Parent archetypes must match the specialization's inherent archetype from enum extensions</description></item>
///   <item><description>Special resource values must have valid ranges (min &lt;= max, startsAt within range)</description></item>
///   <item><description>Ability tier numbers must be in range 1-4 with correct prerequisite flags</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Cache Structure:</strong>
/// <list type="bullet">
///   <item><description><c>_byId</c>: O(1) lookup by SpecializationId enum</description></item>
///   <item><description><c>_byArchetype</c>: O(1) lookup by Archetype, returns pre-grouped list</description></item>
///   <item><description><c>_heretical</c>: Pre-filtered list of Heretical specializations (5)</description></item>
///   <item><description><c>_coherent</c>: Pre-filtered list of Coherent specializations (12)</description></item>
///   <item><description><c>_withSpecialResource</c>: Pre-filtered list of specializations with special resources (5)</description></item>
///   <item><description><c>_all</c>: Complete list of all definitions (17)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Logging Levels:</strong>
/// <list type="bullet">
///   <item><description>Information: Provider initialization, cache stats, successful load</description></item>
///   <item><description>Debug: Individual specialization loading, component access, method calls, validation steps</description></item>
///   <item><description>Warning: Missing definitions, count mismatch, unknown archetype requested</description></item>
///   <item><description>Error: Validation failures, loading errors, configuration not found</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ISpecializationProvider"/>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationAbilityTier"/>
/// <seealso cref="SpecializationAbility"/>
/// <seealso cref="SpecialResourceDefinition"/>
public class SpecializationProvider : ISpecializationProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expected number of specializations in a valid configuration.
    /// </summary>
    /// <remarks>
    /// 17 specializations across 4 archetypes:
    /// Warrior (6), Skirmisher (4), Mystic (2), Adept (5).
    /// </remarks>
    private const int ExpectedSpecializationCount = 17;

    /// <summary>
    /// Default configuration file path relative to application root.
    /// </summary>
    private const string DefaultConfigPath = "config/specializations.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for diagnostic output during provider operations.
    /// </summary>
    private readonly ILogger<SpecializationProvider> _logger;

    /// <summary>
    /// Path to the specializations.json configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private readonly object _initLock = new();

    /// <summary>
    /// Flag indicating whether initialization has completed.
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    /// Cached specialization definitions keyed by SpecializationId enum.
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<SpecializationId, SpecializationDefinition>? _byId;

    /// <summary>
    /// Cached specialization definitions grouped by parent Archetype enum.
    /// Null until initialization completes.
    /// </summary>
    private Dictionary<Archetype, List<SpecializationDefinition>>? _byArchetype;

    /// <summary>
    /// Cached list of Heretical (Corruption-risk) specializations.
    /// Null until initialization completes.
    /// </summary>
    private List<SpecializationDefinition>? _heretical;

    /// <summary>
    /// Cached list of Coherent (no Corruption-risk) specializations.
    /// Null until initialization completes.
    /// </summary>
    private List<SpecializationDefinition>? _coherent;

    /// <summary>
    /// Cached list of specializations with special resource definitions.
    /// Null until initialization completes.
    /// </summary>
    private List<SpecializationDefinition>? _withSpecialResource;

    /// <summary>
    /// Cached list of all loaded specialization definitions.
    /// Null until initialization completes.
    /// </summary>
    private List<SpecializationDefinition>? _all;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="SpecializationProvider"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configPath">
    /// Optional configuration file path. Defaults to "config/specializations.json".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public SpecializationProvider(
        ILogger<SpecializationProvider> logger,
        string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "SpecializationProvider created with config path: {ConfigPath}",
            _configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES - ISpecializationProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            EnsureInitialized();

            var count = _all!.Count;

            _logger.LogDebug(
                "Count property accessed: {Count} specializations loaded",
                count);

            return count;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - ISpecializationProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public SpecializationDefinition? GetBySpecializationId(SpecializationId specializationId)
    {
        EnsureInitialized();

        var found = _byId!.TryGetValue(specializationId, out var definition);

        _logger.LogDebug(
            "GetBySpecializationId for {SpecializationId}: {Result}",
            specializationId,
            found ? $"Found — {definition!.DisplayName} ({definition.PathType})" : "Not found");

        return definition;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationDefinition> GetByArchetype(Archetype archetype)
    {
        EnsureInitialized();

        if (_byArchetype!.TryGetValue(archetype, out var definitions))
        {
            _logger.LogDebug(
                "GetByArchetype({Archetype}): {Count} specializations " +
                "({Heretical} Heretical, {Coherent} Coherent)",
                archetype,
                definitions.Count,
                definitions.Count(d => d.IsHeretical),
                definitions.Count(d => !d.IsHeretical));

            return definitions.AsReadOnly();
        }

        _logger.LogWarning(
            "GetByArchetype called for unknown archetype: {Archetype}",
            archetype);

        return Array.Empty<SpecializationDefinition>();
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationDefinition> GetHereticalSpecializations()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetHereticalSpecializations: returning {Count} specializations",
            _heretical!.Count);

        return _heretical.AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationDefinition> GetCoherentSpecializations()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetCoherentSpecializations: returning {Count} specializations",
            _coherent!.Count);

        return _coherent.AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationDefinition> GetAll()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetAll: returning {Count} specializations",
            _all!.Count);

        return _all.AsReadOnly();
    }

    /// <inheritdoc/>
    public bool Exists(SpecializationId specializationId)
    {
        EnsureInitialized();

        var exists = _byId!.ContainsKey(specializationId);

        _logger.LogDebug(
            "Exists for {SpecializationId}: {Result}",
            specializationId,
            exists);

        return exists;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationDefinition> GetWithSpecialResource()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetWithSpecialResource: returning {Count} specializations",
            _withSpecialResource!.Count);

        return _withSpecialResource.AsReadOnly();
    }

    /// <inheritdoc/>
    public (SpecializationDefinition Specialization, SpecializationAbility Ability)? GetAbility(
        string abilityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);

        EnsureInitialized();

        _logger.LogDebug(
            "GetAbility searching for ability: {AbilityId}",
            abilityId);

        // Normalize to lowercase for consistent comparison
        var normalizedId = abilityId.ToLowerInvariant();

        // Search all specializations for the matching ability
        foreach (var specialization in _all!)
        {
            var ability = specialization.GetAbility(normalizedId);
            if (ability.HasValue)
            {
                _logger.LogDebug(
                    "GetAbility found {AbilityId} in specialization {SpecializationId} " +
                    "(Tier {Tier}, {AbilityType})",
                    abilityId,
                    specialization.SpecializationId,
                    specialization.GetAbilityTier(normalizedId),
                    ability.Value.IsPassive ? "Passive" : "Active");

                return (specialization, ability.Value);
            }
        }

        _logger.LogDebug(
            "GetAbility did not find ability {AbilityId} in any specialization",
            abilityId);

        return null;
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
                "Initializing SpecializationProvider from configuration file: {Path}",
                _configPath);

            // Load and validate configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Build the caches
            BuildCache(config);
            _isInitialized = true;

            _logger.LogInformation(
                "SpecializationProvider initialized successfully. " +
                "Loaded {Total} specializations: " +
                "{Heretical} Heretical, {Coherent} Coherent, " +
                "{WithResource} with special resource. " +
                "Archetypes: Warrior={Warrior}, Skirmisher={Skirmisher}, " +
                "Mystic={Mystic}, Adept={Adept}",
                _all!.Count,
                _heretical!.Count,
                _coherent!.Count,
                _withSpecialResource!.Count,
                _byArchetype!.TryGetValue(Archetype.Warrior, out var w) ? w.Count : 0,
                _byArchetype!.TryGetValue(Archetype.Skirmisher, out var sk) ? sk.Count : 0,
                _byArchetype!.TryGetValue(Archetype.Mystic, out var my) ? my.Count : 0,
                _byArchetype!.TryGetValue(Archetype.Adept, out var ad) ? ad.Count : 0);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Configuration Loading
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads configuration from the JSON file.
    /// </summary>
    /// <returns>The deserialized configuration DTO.</returns>
    /// <exception cref="SpecializationConfigurationException">
    /// Thrown when file cannot be read or JSON is invalid.
    /// </exception>
    private SpecializationsConfigDto LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Specialization configuration file not found: {Path}",
                _configPath);

            throw new SpecializationConfigurationException(
                $"Specialization configuration file not found: {_configPath}");
        }

        try
        {
            var jsonContent = File.ReadAllText(_configPath);

            _logger.LogDebug(
                "Read {ByteCount} characters from specialization configuration file",
                jsonContent.Length);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<SpecializationsConfigDto>(jsonContent, options);

            if (config == null)
            {
                _logger.LogError("Specialization configuration deserialized to null");
                throw new SpecializationConfigurationException(
                    "Failed to deserialize specialization configuration: result was null");
            }

            _logger.LogDebug(
                "Configuration loaded successfully. " +
                "PathTypes count: {PathTypeCount}, Definitions count: {DefinitionCount}",
                config.PathTypes.Count,
                config.Definitions.Count);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse specialization configuration JSON: {ErrorMessage}",
                ex.Message);

            throw new SpecializationConfigurationException(
                $"Invalid JSON in specialization configuration: {ex.Message}",
                ex);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates the configuration meets all requirements.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="SpecializationConfigurationException">
    /// Thrown when validation fails.
    /// </exception>
    private void ValidateConfiguration(SpecializationsConfigDto config)
    {
        _logger.LogDebug("Validating specialization configuration");

        // Validate definition count
        if (config.Definitions.Count != ExpectedSpecializationCount)
        {
            _logger.LogError(
                "Invalid specialization definition count: expected {Expected}, found {Actual}",
                ExpectedSpecializationCount,
                config.Definitions.Count);

            throw new SpecializationConfigurationException(
                $"Expected exactly {ExpectedSpecializationCount} specialization definitions, " +
                $"found {config.Definitions.Count}");
        }

        // Validate no duplicates and all valid enum values
        var seenSpecializations = new HashSet<SpecializationId>();
        foreach (var dto in config.Definitions)
        {
            // Parse specialization ID enum value
            if (!Enum.TryParse<SpecializationId>(dto.SpecializationId, ignoreCase: true, out var specId))
            {
                _logger.LogError(
                    "Invalid specialization ID: {SpecializationId}. Valid values: {ValidValues}",
                    dto.SpecializationId,
                    string.Join(", ", Enum.GetNames<SpecializationId>()));

                throw new SpecializationConfigurationException(
                    $"Invalid specialization ID: '{dto.SpecializationId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<SpecializationId>())}");
            }

            // Check for duplicates
            if (!seenSpecializations.Add(specId))
            {
                _logger.LogError(
                    "Duplicate specialization ID in configuration: {SpecializationId}",
                    specId);

                throw new SpecializationConfigurationException(
                    $"Duplicate specialization ID in configuration: {specId}");
            }

            // Parse parent archetype enum value
            if (!Enum.TryParse<Archetype>(dto.ParentArchetype, ignoreCase: true, out _))
            {
                _logger.LogError(
                    "Invalid parent archetype '{ParentArchetype}' for specialization {SpecializationId}. " +
                    "Valid values: {ValidValues}",
                    dto.ParentArchetype,
                    dto.SpecializationId,
                    string.Join(", ", Enum.GetNames<Archetype>()));

                throw new SpecializationConfigurationException(
                    $"Invalid parent archetype '{dto.ParentArchetype}' for specialization " +
                    $"'{dto.SpecializationId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<Archetype>())}");
            }

            // Parse path type enum value
            if (!Enum.TryParse<SpecializationPathType>(dto.PathType, ignoreCase: true, out _))
            {
                _logger.LogError(
                    "Invalid path type '{PathType}' for specialization {SpecializationId}. " +
                    "Valid values: {ValidValues}",
                    dto.PathType,
                    dto.SpecializationId,
                    string.Join(", ", Enum.GetNames<SpecializationPathType>()));

                throw new SpecializationConfigurationException(
                    $"Invalid path type '{dto.PathType}' for specialization " +
                    $"'{dto.SpecializationId}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<SpecializationPathType>())}");
            }

            _logger.LogDebug(
                "Validated specialization: {SpecializationId} -> {DisplayName} " +
                "({ParentArchetype}, {PathType}), " +
                "hasResource: {HasResource}, abilityTiers: {TierCount}",
                dto.SpecializationId,
                dto.DisplayName,
                dto.ParentArchetype,
                dto.PathType,
                dto.SpecialResource != null,
                dto.AbilityTiers?.Count ?? 0);
        }

        // Verify all expected specializations are present
        var allSpecializations = Enum.GetValues<SpecializationId>();
        var missingSpecializations = allSpecializations.Except(seenSpecializations).ToList();
        if (missingSpecializations.Count > 0)
        {
            _logger.LogError(
                "Missing specializations in configuration: {MissingSpecializations}",
                string.Join(", ", missingSpecializations));

            throw new SpecializationConfigurationException(
                $"Missing specializations in configuration: " +
                $"{string.Join(", ", missingSpecializations)}");
        }

        _logger.LogDebug("Specialization configuration validation passed");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Cache Building
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds all lookup caches from validated configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    private void BuildCache(SpecializationsConfigDto config)
    {
        _logger.LogDebug("Building specialization caches");

        // Initialize cache collections
        _byId = new Dictionary<SpecializationId, SpecializationDefinition>();
        _byArchetype = new Dictionary<Archetype, List<SpecializationDefinition>>();
        _heretical = new List<SpecializationDefinition>();
        _coherent = new List<SpecializationDefinition>();
        _withSpecialResource = new List<SpecializationDefinition>();
        _all = new List<SpecializationDefinition>();

        // Pre-initialize archetype buckets for all known archetypes
        foreach (var archetype in Enum.GetValues<Archetype>())
        {
            _byArchetype[archetype] = new List<SpecializationDefinition>();
        }

        // Map each definition DTO to a domain entity and register in caches
        foreach (var dto in config.Definitions)
        {
            try
            {
                var definition = MapToDefinition(dto);
                RegisterDefinition(definition);
            }
            catch (Exception ex) when (ex is not SpecializationConfigurationException)
            {
                _logger.LogError(
                    ex,
                    "Failed to map specialization definition: {SpecializationId} — {ErrorMessage}",
                    dto.SpecializationId,
                    ex.Message);

                throw new SpecializationConfigurationException(
                    $"Failed to map specialization definition '{dto.SpecializationId}': {ex.Message}",
                    ex);
            }
        }

        _logger.LogDebug(
            "Cache built with {Count} specializations across {ArchetypeCount} archetypes",
            _all.Count,
            _byArchetype.Count(kvp => kvp.Value.Count > 0));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - DTO to Domain Mapping
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps a specialization definition DTO to a domain entity.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <returns>A new <see cref="SpecializationDefinition"/> domain entity.</returns>
    /// <remarks>
    /// <para>
    /// Mapping steps:
    /// <list type="number">
    ///   <item><description>Parse enum values (SpecializationId, Archetype, SpecializationPathType)</description></item>
    ///   <item><description>Map special resource if present (5 of 17 specializations)</description></item>
    ///   <item><description>Map ability tiers if present (currently only Berserkr has full data)</description></item>
    ///   <item><description>Call SpecializationDefinition.Create() factory with all parameters</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The SpecializationDefinition.Create() factory performs cross-validation
    /// to ensure the path type and parent archetype match the specialization's
    /// inherent values from the enum extension methods.
    /// </para>
    /// </remarks>
    private SpecializationDefinition MapToDefinition(SpecializationDefinitionDto dto)
    {
        _logger.LogDebug(
            "Mapping DTO to SpecializationDefinition for {SpecializationId}",
            dto.SpecializationId);

        // Parse enum values (already validated in ValidateConfiguration)
        var specializationId = Enum.Parse<SpecializationId>(dto.SpecializationId, ignoreCase: true);
        var parentArchetype = Enum.Parse<Archetype>(dto.ParentArchetype, ignoreCase: true);
        var pathType = Enum.Parse<SpecializationPathType>(dto.PathType, ignoreCase: true);

        // Map special resource if present
        SpecialResourceDefinition? specialResource = null;
        if (dto.SpecialResource != null)
        {
            specialResource = MapSpecialResource(dto.SpecialResource, specializationId);
        }

        // Map ability tiers if present
        var abilityTiers = MapAbilityTiers(dto.AbilityTiers, specializationId);

        // Create domain entity via factory method (includes cross-validation)
        var definition = SpecializationDefinition.Create(
            specializationId: specializationId,
            displayName: dto.DisplayName,
            tagline: dto.Tagline,
            description: dto.Description,
            selectionText: dto.SelectionText,
            parentArchetype: parentArchetype,
            pathType: pathType,
            unlockCost: dto.UnlockCost,
            specialResource: specialResource,
            abilityTiers: abilityTiers);

        _logger.LogDebug(
            "Successfully mapped SpecializationDefinition for {SpecializationId}: " +
            "{DisplayName} ({ParentArchetype}, {PathType}), " +
            "hasResource: {HasResource}, abilityTiers: {TierCount}, " +
            "totalAbilities: {AbilityCount}",
            specializationId,
            definition.DisplayName,
            definition.ParentArchetype,
            definition.PathType,
            definition.HasSpecialResource,
            definition.AbilityTiers.Count,
            definition.TotalAbilityCount);

        return definition;
    }

    /// <summary>
    /// Maps a special resource DTO to a domain value object.
    /// </summary>
    /// <param name="dto">The special resource DTO to map.</param>
    /// <param name="specializationId">The parent specialization ID for logging context.</param>
    /// <returns>A new <see cref="SpecialResourceDefinition"/> value object.</returns>
    private SpecialResourceDefinition MapSpecialResource(
        SpecialResourceDto dto,
        SpecializationId specializationId)
    {
        _logger.LogDebug(
            "Mapping special resource for {SpecializationId}: " +
            "{ResourceId} ({MinValue}-{MaxValue}, starts at {StartsAt})",
            specializationId,
            dto.ResourceId,
            dto.MinValue,
            dto.MaxValue,
            dto.StartsAt);

        var resource = SpecialResourceDefinition.Create(
            resourceId: dto.ResourceId,
            displayName: dto.DisplayName,
            minValue: dto.MinValue,
            maxValue: dto.MaxValue,
            startsAt: dto.StartsAt,
            regenPerTurn: dto.RegenPerTurn,
            decayPerTurn: dto.DecayPerTurn,
            description: dto.Description);

        _logger.LogDebug(
            "Mapped special resource for {SpecializationId}: {ResourceDisplay}",
            specializationId,
            resource);

        return resource;
    }

    /// <summary>
    /// Maps ability tier DTOs to domain value objects.
    /// </summary>
    /// <param name="tierDtos">The ability tier DTOs to map. May be null or empty.</param>
    /// <param name="specializationId">The parent specialization ID for logging context.</param>
    /// <returns>
    /// A list of <see cref="SpecializationAbilityTier"/> value objects, or an
    /// empty list if no tier data is present.
    /// </returns>
    private List<SpecializationAbilityTier> MapAbilityTiers(
        List<AbilityTierDto>? tierDtos,
        SpecializationId specializationId)
    {
        if (tierDtos == null || tierDtos.Count == 0)
        {
            _logger.LogDebug(
                "No ability tiers to map for {SpecializationId}",
                specializationId);

            return new List<SpecializationAbilityTier>();
        }

        _logger.LogDebug(
            "Mapping {TierCount} ability tiers for {SpecializationId}",
            tierDtos.Count,
            specializationId);

        var abilityTiers = new List<SpecializationAbilityTier>();

        foreach (var tierDto in tierDtos)
        {
            // Map individual abilities within the tier
            var abilities = MapAbilities(tierDto.Abilities, specializationId, tierDto.Tier);

            // Create the tier value object via factory method
            var tier = SpecializationAbilityTier.Create(
                tier: tierDto.Tier,
                displayName: tierDto.DisplayName,
                unlockCost: tierDto.UnlockCost,
                requiresPreviousTier: tierDto.RequiresPreviousTier,
                requiredRank: tierDto.RequiredRank,
                abilities: abilities);

            abilityTiers.Add(tier);

            _logger.LogDebug(
                "Mapped ability tier {Tier} for {SpecializationId}: " +
                "{DisplayName} ({AbilityCount} abilities, {UnlockCost} PP, Rank {RequiredRank})",
                tierDto.Tier,
                specializationId,
                tier.DisplayName,
                tier.AbilityCount,
                tier.UnlockCost,
                tier.RequiredRank);
        }

        return abilityTiers;
    }

    /// <summary>
    /// Maps ability DTOs to domain value objects.
    /// </summary>
    /// <param name="abilityDtos">The ability DTOs to map.</param>
    /// <param name="specializationId">The parent specialization ID for logging context.</param>
    /// <param name="tier">The tier number for logging context.</param>
    /// <returns>A list of <see cref="SpecializationAbility"/> value objects.</returns>
    private List<SpecializationAbility> MapAbilities(
        List<SpecializationAbilityDto> abilityDtos,
        SpecializationId specializationId,
        int tier)
    {
        _logger.LogDebug(
            "Mapping {AbilityCount} abilities for {SpecializationId} Tier {Tier}",
            abilityDtos.Count,
            specializationId,
            tier);

        var abilities = new List<SpecializationAbility>();

        foreach (var abilityDto in abilityDtos)
        {
            var ability = SpecializationAbility.Create(
                abilityId: abilityDto.AbilityId,
                displayName: abilityDto.DisplayName,
                description: abilityDto.Description,
                isPassive: abilityDto.IsPassive,
                resourceCost: abilityDto.ResourceCost,
                resourceType: abilityDto.ResourceType,
                cooldown: abilityDto.Cooldown,
                corruptionRisk: abilityDto.CorruptionRisk);

            abilities.Add(ability);

            _logger.LogDebug(
                "Mapped ability for {SpecializationId} Tier {Tier}: " +
                "{AbilityId} — {DisplayName} [{Type}]" +
                "{ResourceInfo}{CooldownInfo}{CorruptionInfo}",
                specializationId,
                tier,
                ability.AbilityId,
                ability.DisplayName,
                ability.IsPassive ? "PASSIVE" : "ACTIVE",
                ability.HasResourceCost ? $", cost: {ability.ResourceCost} {ability.ResourceType}" : "",
                ability.HasCooldown ? $", cd: {ability.Cooldown}" : "",
                ability.RisksCorruption ? $", corruption: {ability.CorruptionRisk}" : "");
        }

        return abilities;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Cache Registration
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registers a definition in all lookup caches.
    /// </summary>
    /// <param name="definition">The definition to register.</param>
    private void RegisterDefinition(SpecializationDefinition definition)
    {
        // Primary index: by SpecializationId
        _byId![definition.SpecializationId] = definition;

        // Archetype index: grouped by parent archetype
        _byArchetype![definition.ParentArchetype].Add(definition);

        // Complete list
        _all!.Add(definition);

        // Path type filter: Heretical vs Coherent
        if (definition.IsHeretical)
        {
            _heretical!.Add(definition);
        }
        else
        {
            _coherent!.Add(definition);
        }

        // Special resource filter
        if (definition.HasSpecialResource)
        {
            _withSpecialResource!.Add(definition);
        }

        _logger.LogDebug(
            "Registered specialization {SpecializationId}: " +
            "{DisplayName} ({ParentArchetype}, {PathType})" +
            "{ResourceInfo}{AbilityInfo}",
            definition.SpecializationId,
            definition.DisplayName,
            definition.ParentArchetype,
            definition.PathType,
            definition.HasSpecialResource
                ? $", resource: {definition.SpecialResource}"
                : "",
            definition.HasAbilityTiers
                ? $", {definition.TotalAbilityCount} abilities across {definition.AbilityTiers.Count} tiers"
                : "");
    }
}
