using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for combat stance definitions.
/// </summary>
/// <remarks>
/// <para>JsonStanceProvider loads stance definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions.</para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "stances": [
///     {
///       "id": "aggressive",
///       "name": "Aggressive Stance",
///       "description": "...",
///       "attackBonus": 2,
///       "damageBonus": "1d4",
///       "defenseBonus": -2,
///       "saveBonus": -1,
///       "isDefault": false,
///       "icon": "icons/stances/aggressive.png"
///     }
///   ]
/// }
/// </code>
/// </remarks>
public class JsonStanceProvider : IStanceProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, StanceDefinition> _stances;
    private readonly ILogger<JsonStanceProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonStanceProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the stances.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonStanceProvider(string configPath, ILogger<JsonStanceProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "Loading stance definitions from {ConfigPath}",
            configPath);

        _stances = LoadStances();

        _logger.LogInformation(
            "Loaded {Count} stance definitions from configuration",
            _stances.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public StanceDefinition? GetStance(CombatStance stance)
    {
        var stanceId = stance.ToString().ToLowerInvariant();
        return _stances.GetValueOrDefault(stanceId);
    }

    /// <inheritdoc />
    public StanceDefinition? GetStance(string stanceId)
    {
        ArgumentNullException.ThrowIfNull(stanceId);
        return _stances.GetValueOrDefault(stanceId.ToLowerInvariant());
    }

    /// <inheritdoc />
    public StanceDefinition GetDefaultStance()
    {
        // First try to find a stance marked as default
        var defaultStance = _stances.Values.FirstOrDefault(s => s.IsDefault);

        if (defaultStance is not null)
        {
            _logger.LogDebug(
                "Default stance: {StanceName}",
                defaultStance.Name);
            return defaultStance;
        }

        // Fall back to "balanced" if no default is marked
        if (_stances.TryGetValue("balanced", out var balanced))
        {
            _logger.LogDebug(
                "No default stance marked, using balanced: {StanceName}",
                balanced.Name);
            return balanced;
        }

        // No default and no balanced stance - this is a configuration error
        _logger.LogError(
            "No default stance configured and no 'balanced' stance found");

        throw new InvalidOperationException(
            "No default stance configured. Ensure at least one stance has 'isDefault: true' " +
            "or a stance with id 'balanced' exists.");
    }

    /// <inheritdoc />
    public IReadOnlyList<StanceDefinition> GetAllStances()
    {
        return _stances.Values.ToList();
    }

    /// <inheritdoc />
    public bool StanceExists(string stanceId)
    {
        ArgumentNullException.ThrowIfNull(stanceId);
        return _stances.ContainsKey(stanceId.ToLowerInvariant());
    }

    /// <inheritdoc />
    public int Count => _stances.Count;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads stance definitions from the JSON configuration file.
    /// </summary>
    /// <returns>A dictionary of stance definitions keyed by stance ID.</returns>
    private Dictionary<string, StanceDefinition> LoadStances()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Stance configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Stance configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from stance configuration",
            json.Length);

        var config = JsonSerializer.Deserialize<StancesConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Stances is null || config.Stances.Count == 0)
        {
            _logger.LogError(
                "Stance configuration is empty or invalid");

            throw new InvalidDataException(
                "Stance configuration must contain at least one stance definition.");
        }

        var stances = new Dictionary<string, StanceDefinition>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var dto in config.Stances)
        {
            try
            {
                var definition = MapToDefinition(dto);
                stances[definition.StanceId] = definition;

                _logger.LogDebug(
                    "Loaded stance: {StanceId} - ATK:{Attack:+#;-#;0} DEF:{Defense:+#;-#;0} SAVE:{Save:+#;-#;0}",
                    definition.StanceId,
                    definition.AttackBonus,
                    definition.DefenseBonus,
                    definition.SaveBonus);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load stance '{StanceId}': {Error}",
                    dto.Id,
                    ex.Message);
                throw;
            }
        }

        return stances;
    }

    /// <summary>
    /// Maps a DTO from JSON to a StanceDefinition domain entity.
    /// </summary>
    /// <param name="dto">The DTO from JSON deserialization.</param>
    /// <returns>A StanceDefinition entity.</returns>
    private StanceDefinition MapToDefinition(StanceDto dto)
    {
        return StanceDefinition.Create(
            dto.Id,
            dto.Name,
            dto.Description ?? string.Empty,
            dto.AttackBonus,
            dto.DamageBonus,
            dto.DefenseBonus,
            dto.SaveBonus,
            dto.IsDefault,
            dto.Icon);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for stances.json.
    /// </summary>
    private sealed class StancesConfigDto
    {
        public List<StanceDto>? Stances { get; set; }
    }

    /// <summary>
    /// DTO for individual stance definitions in JSON.
    /// </summary>
    private sealed class StanceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AttackBonus { get; set; }
        public string? DamageBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int SaveBonus { get; set; }
        public bool IsDefault { get; set; }
        public string? Icon { get; set; }
    }
}
