using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for combo definitions.
/// </summary>
/// <remarks>
/// <para>JsonComboProvider loads combo definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with indexed lookups.</para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "combos": [
///     {
///       "comboId": "elemental-burst",
///       "name": "Elemental Burst",
///       "description": "...",
///       "windowTurns": 3,
///       "requiredClassIds": ["mage", "sorcerer"],
///       "steps": [
///         { "stepNumber": 1, "abilityId": "fire-bolt", "targetRequirement": "any" }
///       ],
///       "bonusEffects": [
///         { "effectType": "DamageMultiplier", "value": "2.0", "target": "lastTarget" }
///       ],
///       "icon": "icons/combos/elemental-burst.png"
///     }
///   ]
/// }
/// </code>
/// <para>
/// The provider builds multiple indexes for efficient lookup:
/// </para>
/// <list type="bullet">
///   <item><description>Primary index by combo ID for direct lookups</description></item>
///   <item><description>Secondary index by first ability ID for combo detection</description></item>
/// </list>
/// </remarks>
public class JsonComboProvider : IComboProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: combo ID -> ComboDefinition.
    /// </summary>
    private readonly Dictionary<string, ComboDefinition> _combos;

    /// <summary>
    /// Secondary index: first ability ID -> list of combos starting with that ability.
    /// </summary>
    private readonly Dictionary<string, List<ComboDefinition>> _byFirstAbility;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<JsonComboProvider> _logger;

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonComboProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the combos.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonComboProvider(string configPath, ILogger<JsonComboProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _combos = new Dictionary<string, ComboDefinition>(StringComparer.OrdinalIgnoreCase);
        _byFirstAbility = new Dictionary<string, List<ComboDefinition>>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Initializing combo provider from configuration file: {ConfigPath}",
            configPath);

        LoadCombos();

        _logger.LogInformation(
            "Combo provider initialized successfully with {ComboCount} combos indexed by {AbilityCount} starting abilities",
            _combos.Count,
            _byFirstAbility.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ComboDefinition? GetCombo(string comboId)
    {
        ArgumentNullException.ThrowIfNull(comboId);

        var found = _combos.TryGetValue(comboId.ToLowerInvariant(), out var combo);

        _logger.LogDebug(
            "GetCombo: {ComboId} -> {Result}",
            comboId,
            found ? combo!.Name : "not found");

        return combo;
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboDefinition> GetAllCombos()
    {
        var combos = _combos.Values.ToList();

        _logger.LogDebug(
            "GetAllCombos: returning {Count} combos",
            combos.Count);

        return combos;
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboDefinition> GetCombosForClass(string classId)
    {
        ArgumentNullException.ThrowIfNull(classId);

        var combos = _combos.Values
            .Where(c => c.IsAvailableForClass(classId))
            .ToList();

        _logger.LogDebug(
            "GetCombosForClass: {ClassId} -> {Count} combos",
            classId,
            combos.Count);

        return combos;
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboDefinition> GetCombosContaining(string abilityId)
    {
        ArgumentNullException.ThrowIfNull(abilityId);

        var combos = _combos.Values
            .Where(c => c.ContainsAbility(abilityId))
            .ToList();

        _logger.LogDebug(
            "GetCombosContaining: {AbilityId} -> {Count} combos",
            abilityId,
            combos.Count);

        return combos;
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboDefinition> GetCombosStartingWith(string abilityId)
    {
        ArgumentNullException.ThrowIfNull(abilityId);

        var lowerAbilityId = abilityId.ToLowerInvariant();

        if (_byFirstAbility.TryGetValue(lowerAbilityId, out var combos))
        {
            _logger.LogDebug(
                "GetCombosStartingWith: {AbilityId} -> {Count} combos (indexed lookup)",
                abilityId,
                combos.Count);

            return combos;
        }

        _logger.LogDebug(
            "GetCombosStartingWith: {AbilityId} -> 0 combos (no index entry)",
            abilityId);

        return [];
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads combo definitions from the JSON configuration file.
    /// </summary>
    private void LoadCombos()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Combo configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Combo configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from combo configuration file",
            json.Length);

        var config = JsonSerializer.Deserialize<CombosConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Combos is null || config.Combos.Count == 0)
        {
            _logger.LogError(
                "Combo configuration is empty or invalid");

            throw new InvalidDataException(
                "Combo configuration must contain at least one combo definition.");
        }

        _logger.LogDebug(
            "Parsing {Count} combo definitions from configuration",
            config.Combos.Count);

        foreach (var dto in config.Combos)
        {
            try
            {
                var combo = MapToComboDefinition(dto);
                _combos[combo.ComboId] = combo;

                // Build secondary index by first ability
                BuildFirstAbilityIndex(combo);

                _logger.LogDebug(
                    "Loaded combo: {ComboId} ({Name}) - {StepCount} steps, window: {Window} turns, classes: [{Classes}]",
                    combo.ComboId,
                    combo.Name,
                    combo.StepCount,
                    combo.WindowTurns,
                    combo.RequiredClassIds.Any() ? string.Join(", ", combo.RequiredClassIds) : "all");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load combo '{ComboId}': {Error}",
                    dto.ComboId,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Builds the first ability index entry for a combo.
    /// </summary>
    /// <param name="combo">The combo to index.</param>
    private void BuildFirstAbilityIndex(ComboDefinition combo)
    {
        var firstAbilityId = combo.FirstAbilityId.ToLowerInvariant();

        if (string.IsNullOrEmpty(firstAbilityId))
        {
            _logger.LogWarning(
                "Combo {ComboId} has no first ability ID, skipping index",
                combo.ComboId);
            return;
        }

        if (!_byFirstAbility.TryGetValue(firstAbilityId, out var list))
        {
            list = new List<ComboDefinition>();
            _byFirstAbility[firstAbilityId] = list;
        }

        list.Add(combo);

        _logger.LogDebug(
            "Indexed combo {ComboId} under first ability: {AbilityId}",
            combo.ComboId,
            firstAbilityId);
    }

    /// <summary>
    /// Maps a DTO from JSON to a ComboDefinition domain entity.
    /// </summary>
    /// <param name="dto">The DTO from JSON deserialization.</param>
    /// <returns>A ComboDefinition entity.</returns>
    private ComboDefinition MapToComboDefinition(ComboDto dto)
    {
        _logger.LogDebug(
            "Mapping combo DTO: {ComboId} with {StepCount} steps and {EffectCount} bonus effects",
            dto.ComboId,
            dto.Steps?.Count ?? 0,
            dto.BonusEffects?.Count ?? 0);

        // Map steps
        var steps = dto.Steps?.Select(MapToComboStep).ToList() ?? [];

        // Map bonus effects
        var bonusEffects = dto.BonusEffects?.Select(MapToBonusEffect).ToList() ?? [];

        return ComboDefinition.Create(
            dto.ComboId,
            dto.Name,
            dto.Description ?? string.Empty,
            dto.WindowTurns,
            steps,
            bonusEffects,
            dto.RequiredClassIds,
            dto.Icon);
    }

    /// <summary>
    /// Maps a step DTO to a ComboStep entity.
    /// </summary>
    /// <param name="dto">The step DTO from JSON.</param>
    /// <returns>A ComboStep entity.</returns>
    private ComboStep MapToComboStep(ComboStepDto dto)
    {
        return new ComboStep
        {
            StepNumber = dto.StepNumber,
            AbilityId = dto.AbilityId.ToLowerInvariant(),
            TargetRequirement = ParseTargetRequirement(dto.TargetRequirement),
            CustomRequirement = dto.CustomRequirement
        };
    }

    /// <summary>
    /// Maps a bonus effect DTO to a ComboBonusEffect entity.
    /// </summary>
    /// <param name="dto">The bonus effect DTO from JSON.</param>
    /// <returns>A ComboBonusEffect entity.</returns>
    private ComboBonusEffect MapToBonusEffect(BonusEffectDto dto)
    {
        return new ComboBonusEffect
        {
            EffectType = ParseBonusType(dto.EffectType),
            Value = dto.Value ?? string.Empty,
            DamageType = dto.DamageType,
            StatusEffectId = dto.StatusEffectId,
            Target = ParseBonusTarget(dto.Target)
        };
    }

    /// <summary>
    /// Parses a target requirement string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, defaulting to Any.</returns>
    private ComboTargetRequirement ParseTargetRequirement(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ComboTargetRequirement.Any;
        }

        if (Enum.TryParse<ComboTargetRequirement>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown target requirement '{Value}', defaulting to Any",
            value);

        return ComboTargetRequirement.Any;
    }

    /// <summary>
    /// Parses a bonus type string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the value cannot be parsed.</exception>
    private ComboBonusType ParseBonusType(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidDataException("Bonus effect type cannot be empty");
        }

        if (Enum.TryParse<ComboBonusType>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogError(
            "Unknown bonus type '{Value}'",
            value);

        throw new InvalidDataException($"Unknown bonus effect type: {value}");
    }

    /// <summary>
    /// Parses a bonus target string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, defaulting to LastTarget.</returns>
    private ComboBonusTarget ParseBonusTarget(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ComboBonusTarget.LastTarget;
        }

        if (Enum.TryParse<ComboBonusTarget>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown bonus target '{Value}', defaulting to LastTarget",
            value);

        return ComboBonusTarget.LastTarget;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for combos.json.
    /// </summary>
    private sealed class CombosConfigDto
    {
        public List<ComboDto>? Combos { get; set; }
    }

    /// <summary>
    /// DTO for individual combo definitions in JSON.
    /// </summary>
    private sealed class ComboDto
    {
        public string ComboId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int WindowTurns { get; set; }
        public List<string>? RequiredClassIds { get; set; }
        public List<ComboStepDto>? Steps { get; set; }
        public List<BonusEffectDto>? BonusEffects { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// DTO for combo step definitions in JSON.
    /// </summary>
    private sealed class ComboStepDto
    {
        public int StepNumber { get; set; }
        public string AbilityId { get; set; } = string.Empty;
        public string? TargetRequirement { get; set; }
        public string? CustomRequirement { get; set; }
    }

    /// <summary>
    /// DTO for bonus effect definitions in JSON.
    /// </summary>
    private sealed class BonusEffectDto
    {
        public string? EffectType { get; set; }
        public string? Value { get; set; }
        public string? DamageType { get; set; }
        public string? StatusEffectId { get; set; }
        public string? Target { get; set; }
    }
}
