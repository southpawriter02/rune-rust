// ═══════════════════════════════════════════════════════════════════════════════
// LineageProvider.cs
// Provider implementation for loading and caching lineage definitions from JSON.
// Version: 0.17.0e
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
/// Provides access to lineage definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// LineageProvider implements <see cref="ILineageProvider"/> to load lineage
/// definitions from config/lineages.json. Configuration is loaded once on first
/// access and cached in memory for efficient subsequent reads.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class uses double-checked locking to ensure
/// thread-safe initialization. Once initialized, all reads are lock-free.
/// </para>
/// <para>
/// <strong>Configuration Requirements:</strong>
/// <list type="bullet">
///   <item><description>Exactly 4 lineages must be defined</description></item>
///   <item><description>No duplicate lineage types allowed</description></item>
///   <item><description>All lineage type strings must be valid Lineage enum values</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Logging Levels:</strong>
/// <list type="bullet">
///   <item><description>Information: Provider initialization, cache stats</description></item>
///   <item><description>Debug: Individual lineage loading, component access</description></item>
///   <item><description>Warning: Missing optional fields, fallback values used</description></item>
///   <item><description>Error: Validation failures, loading errors</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ILineageProvider"/>
/// <seealso cref="LineageDefinition"/>
public class LineageProvider : ILineageProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expected number of lineages in a valid configuration.
    /// </summary>
    private const int ExpectedLineageCount = 4;

    /// <summary>
    /// Default configuration file path relative to application root.
    /// </summary>
    private const string DefaultConfigPath = "config/lineages.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ILogger<LineageProvider> _logger;
    private readonly string _configPath;
    private readonly object _initLock = new();

    /// <summary>
    /// Cached lineage definitions keyed by Lineage enum.
    /// </summary>
    private Dictionary<Lineage, LineageDefinition>? _cache;

    /// <summary>
    /// Flag indicating whether initialization has completed.
    /// </summary>
    private bool _isInitialized;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="LineageProvider"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configPath">
    /// Optional configuration file path. Defaults to "config/lineages.json".
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public LineageProvider(
        ILogger<LineageProvider> logger,
        string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _configPath = configPath ?? DefaultConfigPath;

        _logger.LogDebug(
            "LineageProvider created with config path: {ConfigPath}",
            _configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - ILineageProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<LineageDefinition> GetAllLineages()
    {
        EnsureInitialized();

        _logger.LogDebug(
            "GetAllLineages returning {Count} lineages",
            _cache!.Count);

        return _cache!.Values.ToList();
    }

    /// <inheritdoc/>
    public LineageDefinition? GetLineage(Lineage lineage)
    {
        EnsureInitialized();

        if (_cache!.TryGetValue(lineage, out var definition))
        {
            _logger.LogDebug(
                "GetLineage found lineage {Lineage}: {DisplayName}",
                lineage,
                definition.DisplayName);

            return definition;
        }

        _logger.LogWarning(
            "GetLineage requested unknown lineage: {Lineage}",
            lineage);

        return null;
    }

    /// <inheritdoc/>
    public LineageAttributeModifiers GetAttributeModifiers(Lineage lineage)
    {
        var definition = GetLineageOrThrow(lineage);

        _logger.LogDebug(
            "GetAttributeModifiers for {Lineage}: TotalFixedModifiers={TotalFixed}, HasFlexible={HasFlexible}",
            lineage,
            definition.AttributeModifiers.TotalFixedModifiers,
            definition.AttributeModifiers.HasFlexibleBonus);

        return definition.AttributeModifiers;
    }

    /// <inheritdoc/>
    public LineagePassiveBonuses GetPassiveBonuses(Lineage lineage)
    {
        var definition = GetLineageOrThrow(lineage);

        _logger.LogDebug(
            "GetPassiveBonuses for {Lineage}: HP={Hp}, AP={Ap}, Soak={Soak}, Move={Move}, Skills={SkillCount}",
            lineage,
            definition.PassiveBonuses.MaxHpBonus,
            definition.PassiveBonuses.MaxApBonus,
            definition.PassiveBonuses.SoakBonus,
            definition.PassiveBonuses.MovementBonus,
            definition.PassiveBonuses.SkillBonuses.Count);

        return definition.PassiveBonuses;
    }

    /// <inheritdoc/>
    public LineageTrait GetUniqueTrait(Lineage lineage)
    {
        var definition = GetLineageOrThrow(lineage);

        _logger.LogDebug(
            "GetUniqueTrait for {Lineage}: {TraitName} ({EffectType})",
            lineage,
            definition.UniqueTrait.TraitName,
            definition.UniqueTrait.EffectType);

        return definition.UniqueTrait;
    }

    /// <inheritdoc/>
    public LineageTraumaBaseline GetTraumaBaseline(Lineage lineage)
    {
        var definition = GetLineageOrThrow(lineage);

        _logger.LogDebug(
            "GetTraumaBaseline for {Lineage}: StartCorr={StartCorr}, CorrResist={CorrResist}, StressResist={StressResist}",
            lineage,
            definition.TraumaBaseline.StartingCorruption,
            definition.TraumaBaseline.CorruptionResistanceModifier,
            definition.TraumaBaseline.StressResistanceModifier);

        return definition.TraumaBaseline;
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
                "Initializing LineageProvider from configuration file: {Path}",
                _configPath);

            // Load and validate configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Build the cache
            _cache = BuildCache(config);
            _isInitialized = true;

            _logger.LogInformation(
                "LineageProvider initialized successfully. Loaded {Count} lineages: {LineageList}",
                _cache.Count,
                string.Join(", ", _cache.Keys));
        }
    }

    /// <summary>
    /// Loads configuration from the JSON file.
    /// </summary>
    /// <returns>The deserialized configuration DTO.</returns>
    /// <exception cref="LineageConfigurationException">
    /// Thrown when file cannot be read or JSON is invalid.
    /// </exception>
    private LineageConfigurationDto LoadConfiguration()
    {
        _logger.LogDebug("Loading configuration from {Path}", _configPath);

        // Check file exists
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Lineage configuration file not found: {Path}",
                _configPath);

            throw new LineageConfigurationException(
                $"Lineage configuration file not found: {_configPath}");
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

            var config = JsonSerializer.Deserialize<LineageConfigurationDto>(jsonContent, options);

            if (config == null)
            {
                _logger.LogError("Configuration deserialized to null");
                throw new LineageConfigurationException(
                    "Failed to deserialize lineage configuration: result was null");
            }

            _logger.LogDebug(
                "Configuration loaded successfully. Version: {Version}, Lineage count: {Count}",
                config.Version,
                config.Lineages.Count);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to parse lineage configuration JSON: {ErrorMessage}",
                ex.Message);

            throw new LineageConfigurationException(
                $"Invalid JSON in lineage configuration: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates the configuration meets all requirements.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="LineageConfigurationException">
    /// Thrown when validation fails.
    /// </exception>
    private void ValidateConfiguration(LineageConfigurationDto config)
    {
        _logger.LogDebug("Validating lineage configuration");

        // Validate lineage count
        if (config.Lineages.Count != ExpectedLineageCount)
        {
            _logger.LogError(
                "Invalid lineage count: expected {Expected}, found {Actual}",
                ExpectedLineageCount,
                config.Lineages.Count);

            throw new LineageConfigurationException(
                $"Expected exactly {ExpectedLineageCount} lineages, found {config.Lineages.Count}");
        }

        // Validate no duplicates and all valid enum values
        var seenLineages = new HashSet<Lineage>();
        foreach (var lineageDto in config.Lineages)
        {
            // Parse enum value
            if (!Enum.TryParse<Lineage>(lineageDto.LineageType, out var lineage))
            {
                _logger.LogError(
                    "Invalid lineage type: {LineageType}. Valid values: {ValidValues}",
                    lineageDto.LineageType,
                    string.Join(", ", Enum.GetNames<Lineage>()));

                throw new LineageConfigurationException(
                    $"Invalid lineage type: '{lineageDto.LineageType}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<Lineage>())}");
            }

            // Check for duplicates
            if (!seenLineages.Add(lineage))
            {
                _logger.LogError(
                    "Duplicate lineage type in configuration: {LineageType}",
                    lineage);

                throw new LineageConfigurationException(
                    $"Duplicate lineage type in configuration: {lineage}");
            }

            _logger.LogDebug(
                "Validated lineage: {Id} -> {LineageType}",
                lineageDto.Id,
                lineage);
        }

        // Verify all expected lineages present
        var allLineages = Enum.GetValues<Lineage>();
        var missingLineages = allLineages.Except(seenLineages).ToList();
        if (missingLineages.Count > 0)
        {
            _logger.LogError(
                "Missing lineages in configuration: {MissingLineages}",
                string.Join(", ", missingLineages));

            throw new LineageConfigurationException(
                $"Missing lineages in configuration: {string.Join(", ", missingLineages)}");
        }

        _logger.LogDebug("Configuration validation passed");
    }

    /// <summary>
    /// Builds the lineage cache from validated configuration.
    /// </summary>
    /// <param name="config">The validated configuration.</param>
    /// <returns>Dictionary mapping Lineage enum to LineageDefinition.</returns>
    private Dictionary<Lineage, LineageDefinition> BuildCache(LineageConfigurationDto config)
    {
        _logger.LogDebug("Building lineage cache");

        var cache = new Dictionary<Lineage, LineageDefinition>();

        foreach (var dto in config.Lineages)
        {
            var lineage = Enum.Parse<Lineage>(dto.LineageType);
            var definition = MapDtoToDefinition(dto, lineage);
            cache[lineage] = definition;

            _logger.LogDebug(
                "Cached lineage: {Lineage} -> {DisplayName}",
                lineage,
                definition.DisplayName);
        }

        _logger.LogDebug("Cache built with {Count} lineages", cache.Count);
        return cache;
    }

    /// <summary>
    /// Maps a DTO to a domain LineageDefinition entity.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <param name="lineage">The parsed Lineage enum value.</param>
    /// <returns>A new LineageDefinition entity.</returns>
    private LineageDefinition MapDtoToDefinition(LineageDefinitionDto dto, Lineage lineage)
    {
        _logger.LogDebug(
            "Mapping DTO to LineageDefinition for {Lineage}",
            lineage);

        // Map attribute modifiers (record struct uses constructor, not factory)
        var attributeModifiers = new LineageAttributeModifiers(
            MightModifier: dto.AttributeModifiers.Might,
            FinesseModifier: dto.AttributeModifiers.Finesse,
            WitsModifier: dto.AttributeModifiers.Wits,
            WillModifier: dto.AttributeModifiers.Will,
            SturdinessModifier: dto.AttributeModifiers.Sturdiness,
            HasFlexibleBonus: dto.AttributeModifiers.HasFlexibleBonus,
            FlexibleBonusAmount: dto.AttributeModifiers.FlexibleBonusAmount);

        // Map skill bonuses
        var skillBonuses = dto.PassiveBonuses.SkillBonuses
            .Select(sb => new SkillBonus(sb.SkillId, sb.BonusAmount))
            .ToList();

        // Map passive bonuses
        var passiveBonuses = LineagePassiveBonuses.Create(
            maxHpBonus: dto.PassiveBonuses.MaxHpBonus,
            maxApBonus: dto.PassiveBonuses.MaxApBonus,
            soakBonus: dto.PassiveBonuses.SoakBonus,
            movementBonus: dto.PassiveBonuses.MovementBonus,
            skillBonuses: skillBonuses);

        // Map unique trait - handle invalid effect types by using default trait
        LineageTrait uniqueTrait;
        if (!Enum.TryParse<LineageTraitEffectType>(dto.UniqueTrait.EffectType, out var effectType))
        {
            _logger.LogWarning(
                "Unknown trait effect type '{EffectType}' for lineage {Lineage}, using default None trait",
                dto.UniqueTrait.EffectType,
                lineage);
            uniqueTrait = LineageTrait.None;
        }
        else
        {
            uniqueTrait = LineageTrait.Create(
                traitId: dto.UniqueTrait.TraitId,
                traitName: dto.UniqueTrait.TraitName,
                description: dto.UniqueTrait.Description,
                effectType: effectType,
                bonusDice: dto.UniqueTrait.BonusDice,
                percentModifier: dto.UniqueTrait.PercentModifier,
                targetCheck: dto.UniqueTrait.TargetCheck,
                targetCondition: dto.UniqueTrait.TargetCondition);
        }

        // Map trauma baseline
        var traumaBaseline = LineageTraumaBaseline.Create(
            startingCorruption: dto.TraumaBaseline.StartingCorruption,
            startingStress: dto.TraumaBaseline.StartingStress,
            corruptionResistanceModifier: dto.TraumaBaseline.CorruptionResistanceModifier,
            stressResistanceModifier: dto.TraumaBaseline.StressResistanceModifier);

        // Create the definition
        return LineageDefinition.Create(
            lineageId: lineage,
            displayName: dto.DisplayName,
            description: dto.Description,
            selectionText: dto.SelectionText,
            attributeModifiers: attributeModifiers,
            passiveBonuses: passiveBonuses,
            uniqueTrait: uniqueTrait,
            traumaBaseline: traumaBaseline,
            appearanceNotes: dto.AppearanceNotes,
            socialRole: dto.SocialRole);
    }

    /// <summary>
    /// Gets a lineage definition or throws if not found.
    /// </summary>
    /// <param name="lineage">The lineage to retrieve.</param>
    /// <returns>The lineage definition.</returns>
    /// <exception cref="InvalidOperationException">Thrown if lineage not found.</exception>
    private LineageDefinition GetLineageOrThrow(Lineage lineage)
    {
        EnsureInitialized();

        if (!_cache!.TryGetValue(lineage, out var definition))
        {
            _logger.LogError(
                "Lineage not found in cache: {Lineage}. This should not happen after successful initialization.",
                lineage);

            throw new InvalidOperationException(
                $"Lineage '{lineage}' not found in cache. This indicates a configuration error.");
        }

        return definition;
    }
}
