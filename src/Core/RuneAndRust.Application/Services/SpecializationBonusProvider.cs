namespace RuneAndRust.Application.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides specialization skill bonuses from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loads specialization bonus definitions from <c>specialization-bonuses.json</c>
/// and evaluates conditional bonuses based on skill context.
/// </para>
/// <para>
/// Supports 10+ specializations including:
/// <list type="bullet">
///   <item>Gantry-Runner: Climbing and leaping bonuses</item>
///   <item>Myr-Stalker: Navigation bonuses in swamp biomes</item>
///   <item>Thul: Persuasion with special ability</item>
///   <item>Scrap-Tinker: Lockpicking and trap bonuses</item>
/// </list>
/// </para>
/// </remarks>
public class SpecializationBonusProvider : ISpecializationBonusProvider
{
    private readonly ILogger<SpecializationBonusProvider> _logger;
    private readonly Dictionary<string, SpecializationConfig> _specializations = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationBonusProvider"/> class.
    /// </summary>
    /// <param name="configurationPath">Path to the specialization-bonuses.json file.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public SpecializationBonusProvider(
        string configurationPath,
        ILogger<SpecializationBonusProvider> logger)
    {
        _logger = logger;
        LoadConfiguration(configurationPath);
    }

    /// <inheritdoc/>
    public SpecializationSkillBonus GetSkillBonus(
        string specializationId,
        string skillId,
        SkillContext context)
    {
        var normalizedSpecId = specializationId.ToLowerInvariant();
        var normalizedSkillId = skillId.ToLowerInvariant();

        if (!_specializations.TryGetValue(normalizedSpecId, out var specConfig))
        {
            _logger.LogTrace("No bonuses configured for specialization: {SpecializationId}", specializationId);
            return SpecializationSkillBonus.None(specializationId, skillId);
        }

        if (!specConfig.Skills.TryGetValue(normalizedSkillId, out var skillConfig))
        {
            _logger.LogTrace(
                "No bonus for {SpecializationId} → {SkillId}",
                specializationId,
                skillId);
            return SpecializationSkillBonus.None(specializationId, skillId);
        }

        var bonus = new SpecializationSkillBonus(
            SpecializationId: specializationId,
            SkillId: skillId,
            DiceBonus: skillConfig.DiceBonus,
            DcModifier: skillConfig.DcModifier,
            Description: skillConfig.Description ?? specConfig.Name,
            IsConditional: skillConfig.Condition != null,
            ConditionMet: true,
            SpecialAbility: skillConfig.SpecialAbility);

        // Evaluate conditions if present
        if (bonus.IsConditional)
        {
            var conditionMet = EvaluateConditionInternal(skillConfig.Condition!, context);
            bonus = bonus.WithConditionResult(conditionMet);

            _logger.LogDebug(
                "Conditional bonus {SpecializationId} → {SkillId}: condition {ConditionResult}",
                specializationId,
                skillId,
                conditionMet ? "met" : "not met");
        }

        if (bonus.ShouldApply)
        {
            _logger.LogDebug(
                "Specialization bonus applied: {Description}",
                bonus.ToDisplayString());
        }

        return bonus;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpecializationSkillBonus> GetAllBonuses(string specializationId)
    {
        var normalizedSpecId = specializationId.ToLowerInvariant();

        if (!_specializations.TryGetValue(normalizedSpecId, out var specConfig))
        {
            _logger.LogTrace("No bonuses found for specialization: {SpecializationId}", specializationId);
            return Array.Empty<SpecializationSkillBonus>();
        }

        var bonuses = specConfig.Skills
            .Select(kvp => new SpecializationSkillBonus(
                SpecializationId: specializationId,
                SkillId: kvp.Key,
                DiceBonus: kvp.Value.DiceBonus,
                DcModifier: kvp.Value.DcModifier,
                Description: kvp.Value.Description ?? specConfig.Name,
                IsConditional: kvp.Value.Condition != null,
                ConditionMet: true,
                SpecialAbility: kvp.Value.SpecialAbility))
            .ToList();

        _logger.LogTrace(
            "Retrieved {Count} bonuses for specialization: {SpecializationId}",
            bonuses.Count,
            specializationId);

        return bonuses;
    }

    /// <inheritdoc/>
    public bool HasBonusForSkill(string specializationId, string skillId)
    {
        var normalizedSpecId = specializationId.ToLowerInvariant();
        var normalizedSkillId = skillId.ToLowerInvariant();

        return _specializations.TryGetValue(normalizedSpecId, out var specConfig) &&
               specConfig.Skills.ContainsKey(normalizedSkillId);
    }

    /// <inheritdoc/>
    public bool EvaluateCondition(SpecializationSkillBonus bonus, SkillContext context)
    {
        if (!bonus.IsConditional)
            return true;

        var normalizedSpecId = bonus.SpecializationId.ToLowerInvariant();
        var normalizedSkillId = bonus.SkillId.ToLowerInvariant();

        if (!_specializations.TryGetValue(normalizedSpecId, out var specConfig))
            return false;

        if (!specConfig.Skills.TryGetValue(normalizedSkillId, out var skillConfig))
            return false;

        if (skillConfig.Condition == null)
            return true;

        return EvaluateConditionInternal(skillConfig.Condition, context);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetSpecializationsWithBonuses()
    {
        var specs = _specializations.Keys.ToList();
        _logger.LogTrace("Returning {Count} specializations with bonuses", specs.Count);
        return specs;
    }

    /// <summary>
    /// Evaluates a condition configuration against the skill context.
    /// </summary>
    private bool EvaluateConditionInternal(ConditionConfig condition, SkillContext context)
    {
        // Evaluate biome condition
        if (condition.Biomes != null && condition.Biomes.Count > 0)
        {
            var hasBiomeMatch = context.EnvironmentModifiers
                .Any(m => condition.Biomes.Contains(
                    m.ModifierId.ToLowerInvariant()));

            if (!hasBiomeMatch)
            {
                _logger.LogTrace("Biome condition not met");
                return false;
            }
        }

        // Evaluate surface condition
        if (condition.SurfaceTypes != null && condition.SurfaceTypes.Count > 0)
        {
            var hasSurfaceMatch = context.EnvironmentModifiers
                .Any(m => m.SurfaceType.HasValue &&
                         condition.SurfaceTypes.Contains(m.SurfaceType.Value.ToString().ToLowerInvariant()));

            if (!hasSurfaceMatch)
            {
                _logger.LogTrace("Surface condition not met");
                return false;
            }
        }

        // Evaluate equipment condition
        if (condition.RequiredEquipment != null && condition.RequiredEquipment.Count > 0)
        {
            var hasEquipment = context.EquipmentModifiers
                .Any(m => condition.RequiredEquipment.Contains(m.EquipmentId.ToLowerInvariant()));

            if (!hasEquipment)
            {
                _logger.LogTrace("Equipment condition not met");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Loads specialization configuration from JSON file.
    /// </summary>
    private void LoadConfiguration(string configurationPath)
    {
        try
        {
            if (!File.Exists(configurationPath))
            {
                _logger.LogWarning(
                    "Specialization bonuses configuration not found at {Path}",
                    configurationPath);
                return;
            }

            var json = File.ReadAllText(configurationPath);
            var config = JsonSerializer.Deserialize<SpecializationBonusesConfig>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config?.Specializations != null)
            {
                foreach (var (specId, specConfig) in config.Specializations)
                {
                    _specializations[specId.ToLowerInvariant()] = specConfig;
                }

                _logger.LogInformation(
                    "Loaded specialization bonuses for {Count} specializations",
                    _specializations.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load specialization bonuses configuration");
        }
    }

    #region Configuration DTOs

    private class SpecializationBonusesConfig
    {
        public Dictionary<string, SpecializationConfig> Specializations { get; set; } = new();
    }

    private class SpecializationConfig
    {
        public string Name { get; set; } = "";
        public Dictionary<string, SkillBonusConfig> Skills { get; set; } = new();
    }

    private class SkillBonusConfig
    {
        public int DiceBonus { get; set; }
        public int DcModifier { get; set; }
        public string? Description { get; set; }
        public string? SpecialAbility { get; set; }
        public ConditionConfig? Condition { get; set; }
    }

    private class ConditionConfig
    {
        public List<string>? Biomes { get; set; }
        public List<string>? SurfaceTypes { get; set; }
        public List<string>? RequiredEquipment { get; set; }
    }

    #endregion
}
