using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for environmental hazard definitions.
/// </summary>
/// <remarks>
/// <para>JsonEnvironmentalHazardProvider loads hazard definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions.</para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "hazards": [
///     {
///       "type": "Lava",
///       "name": "Lava",
///       "description": "Molten rock that burns intensely",
///       "damageDice": "3d6",
///       "damageType": "fire",
///       "statusEffectId": "burning",
///       "damageOnEnter": true,
///       "damagePerTurn": true
///     }
///   ]
/// }
/// </code>
/// </remarks>
public class JsonEnvironmentalHazardProvider : IEnvironmentalHazardProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<HazardType, EnvironmentalHazardDefinition> _hazardsByType;
    private readonly Dictionary<string, EnvironmentalHazardDefinition> _hazardsById;
    private readonly List<EnvironmentalHazardDefinition> _allHazards;
    private readonly List<EnvironmentalHazardDefinition> _damagingHazards;
    private readonly List<EnvironmentalHazardDefinition> _tickDamageHazards;
    private readonly ILogger<JsonEnvironmentalHazardProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonEnvironmentalHazardProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the environmental-hazards.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonEnvironmentalHazardProvider(string configPath, ILogger<JsonEnvironmentalHazardProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "Loading environmental hazard definitions from {ConfigPath}",
            configPath);

        _hazardsByType = new Dictionary<HazardType, EnvironmentalHazardDefinition>();
        _hazardsById = new Dictionary<string, EnvironmentalHazardDefinition>(StringComparer.OrdinalIgnoreCase);
        _allHazards = new List<EnvironmentalHazardDefinition>();
        _damagingHazards = new List<EnvironmentalHazardDefinition>();
        _tickDamageHazards = new List<EnvironmentalHazardDefinition>();

        LoadHazards();

        _logger.LogInformation(
            "Loaded {Count} environmental hazard definitions ({Damaging} damaging, {Tick} with tick damage)",
            _allHazards.Count,
            _damagingHazards.Count,
            _tickDamageHazards.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public EnvironmentalHazardDefinition? GetHazard(HazardType type)
    {
        if (_hazardsByType.TryGetValue(type, out var hazard))
        {
            _logger.LogDebug(
                "Retrieved hazard definition for type {HazardType}: {HazardName}",
                type,
                hazard.Name);
            return hazard;
        }

        _logger.LogDebug(
            "No hazard definition found for type {HazardType}",
            type);
        return null;
    }

    /// <inheritdoc />
    public EnvironmentalHazardDefinition? GetHazard(string hazardId)
    {
        ArgumentNullException.ThrowIfNull(hazardId);

        if (_hazardsById.TryGetValue(hazardId.ToLowerInvariant(), out var hazard))
        {
            _logger.LogDebug(
                "Retrieved hazard definition for ID '{HazardId}': {HazardName}",
                hazardId,
                hazard.Name);
            return hazard;
        }

        _logger.LogDebug(
            "No hazard definition found for ID '{HazardId}'",
            hazardId);
        return null;
    }

    /// <inheritdoc />
    public IReadOnlyList<EnvironmentalHazardDefinition> GetAllHazards()
    {
        return _allHazards.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<EnvironmentalHazardDefinition> GetDamagingHazards()
    {
        return _damagingHazards.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<EnvironmentalHazardDefinition> GetTickDamageHazards()
    {
        return _tickDamageHazards.AsReadOnly();
    }

    /// <inheritdoc />
    public bool HazardExists(HazardType type)
    {
        return _hazardsByType.ContainsKey(type);
    }

    /// <inheritdoc />
    public bool HazardExists(string hazardId)
    {
        ArgumentNullException.ThrowIfNull(hazardId);
        return _hazardsById.ContainsKey(hazardId.ToLowerInvariant());
    }

    /// <inheritdoc />
    public int Count => _allHazards.Count;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads hazard definitions from the JSON configuration file.
    /// </summary>
    private void LoadHazards()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Environmental hazard configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Environmental hazard configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from environmental hazard configuration",
            json.Length);

        var config = JsonSerializer.Deserialize<HazardsConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Hazards is null || config.Hazards.Count == 0)
        {
            _logger.LogWarning(
                "Environmental hazard configuration is empty - no hazards loaded");
            return;
        }

        foreach (var dto in config.Hazards)
        {
            try
            {
                var definition = MapToDefinition(dto);

                // Store in all collections
                _hazardsByType[definition.Type] = definition;
                _hazardsById[definition.Type.ToString().ToLowerInvariant()] = definition;
                _allHazards.Add(definition);

                // Categorize by damage type
                if (definition.DealsDamage())
                {
                    _damagingHazards.Add(definition);
                }

                if (definition.DamagePerTurn)
                {
                    _tickDamageHazards.Add(definition);
                }

                _logger.LogDebug(
                    "Loaded hazard: {HazardType} ({HazardName}) - Damage: {Dice} {DamageType}, " +
                    "OnEnter: {OnEnter}, PerTurn: {PerTurn}, StatusEffect: {StatusEffect}",
                    definition.Type,
                    definition.Name,
                    definition.EntryDamage?.Dice ?? definition.TurnDamage?.Dice ?? "None",
                    definition.EntryDamage?.DamageType ?? definition.TurnDamage?.DamageType ?? "None",
                    definition.DamageOnEnter,
                    definition.DamagePerTurn,
                    definition.StatusEffectId ?? "none");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load hazard '{HazardType}': {Error}",
                    dto.Type,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Maps a DTO from JSON to an EnvironmentalHazardDefinition.
    /// </summary>
    /// <param name="dto">The DTO from JSON deserialization.</param>
    /// <returns>An EnvironmentalHazardDefinition.</returns>
    /// <exception cref="InvalidDataException">Thrown when the hazard type cannot be parsed.</exception>
    private EnvironmentalHazardDefinition MapToDefinition(HazardDto dto)
    {
        if (!Enum.TryParse<HazardType>(dto.Type, ignoreCase: true, out var hazardType))
        {
            _logger.LogError(
                "Invalid hazard type: {Type}. Valid types are: {ValidTypes}",
                dto.Type,
                string.Join(", ", Enum.GetNames<HazardType>()));

            throw new InvalidDataException(
                $"Invalid hazard type: {dto.Type}. Valid types are: {string.Join(", ", Enum.GetNames<HazardType>())}");
        }

        // Map damage DTOs to Definitions
        DamageDefinition? entryDamage = null;
        if (dto.EntryDamage != null)
        {
            entryDamage = new DamageDefinition
            {
                Dice = dto.EntryDamage.Dice,
                DamageType = dto.EntryDamage.DamageType,
                Bonus = dto.EntryDamage.Bonus
            };
        }
        else if (dto.DamageOnEnter && !string.IsNullOrEmpty(dto.DamageDice)) // Legacy fallback
        {
            entryDamage = new DamageDefinition
            {
                Dice = dto.DamageDice,
                DamageType = dto.DamageType ?? "physical"
            };
        }

        DamageDefinition? turnDamage = null;
        if (dto.DamagePerTurn != null)
        {
            turnDamage = new DamageDefinition
            {
                Dice = dto.DamagePerTurn.Dice,
                DamageType = dto.DamagePerTurn.DamageType,
                Bonus = dto.DamagePerTurn.Bonus
            };
        }
        // No legacy fallback for TurnDamage as boolean doesn't carry dice info (unless we reuse main dice)

#pragma warning disable CS0618 // Type or member is obsolete
        return new EnvironmentalHazardDefinition
        {
            Type = hazardType,
            Name = dto.Name ?? hazardType.ToString(),
            Description = dto.Description,
            DamageDice = dto.DamageDice ?? "1d6", // Legacy/Default
            DamageType = dto.DamageType ?? "fire", // Legacy/Default
            EntryDamage = entryDamage,
            TurnDamage = turnDamage,
            StatusEffectId = dto.StatusEffectId,
            DamageOnEnter = entryDamage != null,
            DamagePerTurn = turnDamage != null,
            RequiresClimbOut = dto.RequiresClimbOut,
            DegradesArmor = dto.DegradesArmor,
            IconPath = dto.IconPath
        };
#pragma warning restore CS0618 // Type or member is obsolete
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for environmental-hazards.json.
    /// </summary>
    private sealed class HazardsConfigDto
    {
        public List<HazardDto>? Hazards { get; set; }
    }

    /// <summary>
    /// DTO for individual hazard definitions in JSON.
    /// </summary>
    private sealed class HazardDto
    {
        [JsonPropertyName("hazardType")]
        public string Type { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        // Legacy fields
        public string? DamageDice { get; set; }
        public string? DamageType { get; set; }
        
        public string? StatusEffectId { get; set; }
        
        // New structured damage fields
        public HazardDamageDto? EntryDamage { get; set; }
        public HazardDamageDto? DamagePerTurn { get; set; } // Can be object in new format
        
        // These might still be used as flags or overrides, but primarily derived from existence of damage objects
        public bool DamageOnEnter { get; set; } = true;
        
        public bool RequiresClimbOut { get; set; }
        public bool DegradesArmor { get; set; }
        public string? IconPath { get; set; }
    }

    private sealed class HazardDamageDto
    {
        public string Dice { get; set; } = "1d6";
        public string DamageType { get; set; } = "physical";
        public int Bonus { get; set; }
    }
}
