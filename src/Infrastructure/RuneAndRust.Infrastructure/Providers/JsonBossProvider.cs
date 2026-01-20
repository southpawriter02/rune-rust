using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for boss definitions.
/// </summary>
/// <remarks>
/// <para>
/// JsonBossProvider loads boss definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with indexed lookups.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "bosses": [
///     {
///       "bossId": "skeleton-king",
///       "name": "The Skeleton King",
///       "description": "An ancient ruler risen from death",
///       "baseMonsterDefinitionId": "skeleton-elite",
///       "titleText": "Lord of the Undead Crypt",
///       "phases": [
///         {
///           "phaseNumber": 1,
///           "name": "Awakened",
///           "healthThreshold": 100,
///           "behavior": "Tactical",
///           "abilityIds": ["bone-strike", "raise-dead"]
///         }
///       ],
///       "loot": [
///         { "itemId": "gold", "amount": 500, "chance": 1.0 },
///         { "itemId": "crown-of-bones", "amount": 1, "chance": 0.25 }
///       ]
///     }
///   ]
/// }
/// </code>
/// <para>
/// The provider builds an index for efficient lookup by boss ID.
/// </para>
/// </remarks>
public class JsonBossProvider : IBossProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: boss ID -> BossDefinition.
    /// </summary>
    private readonly Dictionary<string, BossDefinition> _bosses;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<JsonBossProvider> _logger;

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonBossProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the bosses.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonBossProvider(string configPath, ILogger<JsonBossProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _bosses = new Dictionary<string, BossDefinition>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Initializing boss provider from configuration file: {ConfigPath}",
            configPath);

        LoadBosses();

        _logger.LogInformation(
            "Boss provider initialized successfully with {BossCount} bosses, total {PhaseCount} phases",
            _bosses.Count,
            _bosses.Values.Sum(b => b.PhaseCount));
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public BossDefinition? GetBoss(string bossId)
    {
        ArgumentNullException.ThrowIfNull(bossId);

        var found = _bosses.TryGetValue(bossId.ToLowerInvariant(), out var boss);

        _logger.LogDebug(
            "GetBoss: {BossId} -> {Result}",
            bossId,
            found ? boss!.Name : "not found");

        return boss;
    }

    /// <inheritdoc />
    public IReadOnlyList<BossDefinition> GetAllBosses()
    {
        var bosses = _bosses.Values.ToList();

        _logger.LogDebug(
            "GetAllBosses: returning {Count} bosses",
            bosses.Count);

        return bosses;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetBossIds()
    {
        var ids = _bosses.Keys.ToList();

        _logger.LogDebug(
            "GetBossIds: returning {Count} boss IDs",
            ids.Count);

        return ids;
    }

    /// <inheritdoc />
    public bool BossExists(string bossId)
    {
        ArgumentNullException.ThrowIfNull(bossId);

        var exists = _bosses.ContainsKey(bossId.ToLowerInvariant());

        _logger.LogDebug(
            "BossExists: {BossId} -> {Result}",
            bossId,
            exists);

        return exists;
    }

    /// <inheritdoc />
    public IReadOnlyList<BossDefinition> GetBossesByPhaseCount(int minPhases)
    {
        var bosses = _bosses.Values
            .Where(b => b.PhaseCount >= minPhases)
            .ToList();

        _logger.LogDebug(
            "GetBossesByPhaseCount: minPhases={MinPhases} -> {Count} bosses",
            minPhases,
            bosses.Count);

        return bosses;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads boss definitions from the JSON configuration file.
    /// </summary>
    private void LoadBosses()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Boss configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Boss configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from boss configuration file",
            json.Length);

        var config = JsonSerializer.Deserialize<BossesConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Bosses is null || config.Bosses.Count == 0)
        {
            _logger.LogError(
                "Boss configuration is empty or invalid");

            throw new InvalidDataException(
                "Boss configuration must contain at least one boss definition.");
        }

        _logger.LogDebug(
            "Parsing {Count} boss definitions from configuration",
            config.Bosses.Count);

        foreach (var dto in config.Bosses)
        {
            try
            {
                var boss = MapToBossDefinition(dto);

                // Validate the boss definition
                var errors = boss.GetValidationErrors();
                if (errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Boss '{BossId}' has validation warnings: {Warnings}",
                        dto.BossId,
                        string.Join("; ", errors));
                }

                _bosses[boss.BossId] = boss;

                _logger.LogDebug(
                    "Loaded boss: {BossId} ({Name}) - {PhaseCount} phases, base monster: {BaseMonster}, loot entries: {LootCount}",
                    boss.BossId,
                    boss.Name,
                    boss.PhaseCount,
                    boss.BaseMonsterDefinitionId,
                    boss.Loot.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load boss '{BossId}': {Error}",
                    dto.BossId,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Maps a DTO from JSON to a BossDefinition domain entity.
    /// </summary>
    /// <param name="dto">The DTO from JSON deserialization.</param>
    /// <returns>A BossDefinition entity.</returns>
    private BossDefinition MapToBossDefinition(BossDto dto)
    {
        _logger.LogDebug(
            "Mapping boss DTO: {BossId} with {PhaseCount} phases and {LootCount} loot entries",
            dto.BossId,
            dto.Phases?.Count ?? 0,
            dto.Loot?.Count ?? 0);

        var boss = BossDefinition.Create(
            dto.BossId,
            dto.Name,
            dto.Description ?? string.Empty,
            dto.BaseMonsterDefinitionId);

        // Set title text if provided
        if (!string.IsNullOrWhiteSpace(dto.TitleText))
        {
            boss.WithTitleText(dto.TitleText);
        }

        // Map phases
        if (dto.Phases != null)
        {
            foreach (var phaseDto in dto.Phases.OrderBy(p => p.PhaseNumber))
            {
                var phase = MapToBossPhase(phaseDto);
                boss.WithPhase(phase);

                _logger.LogDebug(
                    "  Mapped phase {PhaseNumber}: {Name} (threshold: {Threshold}%, behavior: {Behavior}, abilities: {AbilityCount})",
                    phase.PhaseNumber,
                    phase.Name,
                    phase.HealthThreshold,
                    phase.Behavior,
                    phase.AbilityIds.Count);
            }
        }

        // Map loot
        if (dto.Loot != null)
        {
            foreach (var lootDto in dto.Loot)
            {
                var loot = MapToBossLootEntry(lootDto);
                if (loot.IsValid)
                {
                    boss.WithLoot(loot);

                    _logger.LogDebug(
                        "  Mapped loot: {ItemId} x{Amount} ({Chance})",
                        loot.ItemId,
                        loot.Amount,
                        loot.ChancePercent);
                }
            }
        }

        return boss;
    }

    /// <summary>
    /// Maps a phase DTO to a BossPhase entity.
    /// </summary>
    /// <param name="dto">The phase DTO from JSON.</param>
    /// <returns>A BossPhase entity.</returns>
    private BossPhase MapToBossPhase(BossPhaseDto dto)
    {
        var behavior = ParseBehavior(dto.Behavior);

        var phase = BossPhase.Create(
            dto.PhaseNumber,
            dto.Name,
            dto.HealthThreshold,
            behavior);

        // Add abilities
        if (dto.AbilityIds != null && dto.AbilityIds.Count > 0)
        {
            phase.WithAbilities(dto.AbilityIds.ToArray());
        }

        // Add stat modifiers
        if (dto.StatModifiers != null)
        {
            foreach (var modDto in dto.StatModifiers)
            {
                var modifier = MapToStatModifier(modDto);
                phase.WithStatModifier(modifier);
            }
        }

        // Set transition text
        if (!string.IsNullOrWhiteSpace(dto.TransitionText))
        {
            phase.WithTransitionText(dto.TransitionText);
        }

        // Set transition effect
        if (!string.IsNullOrWhiteSpace(dto.TransitionEffectId))
        {
            phase.WithTransitionEffect(dto.TransitionEffectId);
        }

        // Set summon config
        if (dto.SummonConfig != null)
        {
            var summonConfig = MapToSummonConfiguration(dto.SummonConfig);
            if (summonConfig.IsValid)
            {
                phase.WithSummonConfig(summonConfig);
            }
        }

        return phase;
    }

    /// <summary>
    /// Maps a loot entry DTO to a BossLootEntry value object.
    /// </summary>
    /// <param name="dto">The loot DTO from JSON.</param>
    /// <returns>A BossLootEntry value object.</returns>
    private BossLootEntry MapToBossLootEntry(BossLootDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemId))
        {
            _logger.LogWarning("Loot entry has empty item ID, skipping");
            return BossLootEntry.Empty;
        }

        return BossLootEntry.Create(
            dto.ItemId,
            dto.Chance,
            dto.Amount > 0 ? dto.Amount : 1);
    }

    /// <summary>
    /// Maps a stat modifier DTO to a StatModifier value object.
    /// </summary>
    /// <param name="dto">The stat modifier DTO from JSON.</param>
    /// <returns>A StatModifier value object.</returns>
    private StatModifier MapToStatModifier(StatModifierDto dto)
    {
        var modifierType = ParseStatModifierType(dto.Type);

        return new StatModifier(
            dto.StatId,
            modifierType,
            dto.Value);
    }

    /// <summary>
    /// Maps a summon configuration DTO to a SummonConfiguration value object.
    /// </summary>
    /// <param name="dto">The summon config DTO from JSON.</param>
    /// <returns>A SummonConfiguration value object.</returns>
    private SummonConfiguration MapToSummonConfiguration(SummonConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MonsterDefinitionId))
        {
            _logger.LogWarning("Summon config has empty monster definition ID, skipping");
            return SummonConfiguration.Empty;
        }

        return SummonConfiguration.Create(dto.MonsterDefinitionId, dto.Count > 0 ? dto.Count : 1)
            .WithIntervalTurns(dto.IntervalTurns > 0 ? dto.IntervalTurns : 2)
            .WithMaxActive(dto.MaxActive > 0 ? dto.MaxActive : 4);
    }

    /// <summary>
    /// Parses a behavior string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, defaulting to Tactical.</returns>
    private BossBehavior ParseBehavior(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return BossBehavior.Tactical;
        }

        if (Enum.TryParse<BossBehavior>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown boss behavior '{Value}', defaulting to Tactical",
            value);

        return BossBehavior.Tactical;
    }

    /// <summary>
    /// Parses a stat modifier type string to the enum value.
    /// </summary>
    /// <param name="value">The string value from JSON.</param>
    /// <returns>The parsed enum value, defaulting to Flat.</returns>
    private StatModifierType ParseStatModifierType(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return StatModifierType.Flat;
        }

        if (Enum.TryParse<StatModifierType>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        _logger.LogWarning(
            "Unknown stat modifier type '{Value}', defaulting to Flat",
            value);

        return StatModifierType.Flat;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for bosses.json.
    /// </summary>
    private sealed class BossesConfigDto
    {
        public List<BossDto>? Bosses { get; set; }
    }

    /// <summary>
    /// DTO for individual boss definitions in JSON.
    /// </summary>
    private sealed class BossDto
    {
        public string BossId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BaseMonsterDefinitionId { get; set; } = string.Empty;
        public string? TitleText { get; set; }
        public List<BossPhaseDto>? Phases { get; set; }
        public List<BossLootDto>? Loot { get; set; }
    }

    /// <summary>
    /// DTO for boss phase definitions in JSON.
    /// </summary>
    private sealed class BossPhaseDto
    {
        public int PhaseNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HealthThreshold { get; set; }
        public string? Behavior { get; set; }
        public List<string>? AbilityIds { get; set; }
        public List<StatModifierDto>? StatModifiers { get; set; }
        public string? TransitionText { get; set; }
        public string? TransitionEffectId { get; set; }
        public SummonConfigDto? SummonConfig { get; set; }
    }

    /// <summary>
    /// DTO for boss loot entries in JSON.
    /// </summary>
    private sealed class BossLootDto
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; } = 1;
        public double Chance { get; set; } = 1.0;
    }

    /// <summary>
    /// DTO for stat modifier definitions in JSON.
    /// </summary>
    private sealed class StatModifierDto
    {
        public string StatId { get; set; } = string.Empty;
        public string? Type { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// DTO for summon configuration in JSON.
    /// </summary>
    private sealed class SummonConfigDto
    {
        public string MonsterDefinitionId { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
        public int IntervalTurns { get; set; } = 2;
        public int MaxActive { get; set; } = 4;
    }
}
