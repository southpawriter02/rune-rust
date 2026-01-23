namespace RuneAndRust.Infrastructure.Providers;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides master ability definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// Loads abilities from <c>config/master-abilities.json</c> at construction
/// and caches them for the lifetime of the application. The provider validates
/// abilities against the JSON schema during loading.
/// </para>
/// <para>
/// Abilities are indexed by ID and skill ID for O(1) lookup performance.
/// </para>
/// </remarks>
public sealed class MasterAbilityProvider : IMasterAbilityProvider
{
    private readonly ILogger<MasterAbilityProvider> _logger;
    private readonly Dictionary<string, MasterAbility> _abilitiesById;
    private readonly Dictionary<string, List<MasterAbility>> _abilitiesBySkill;
    private readonly List<MasterAbility> _allAbilities;

    /// <summary>
    /// Creates a new master ability provider.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public MasterAbilityProvider(ILogger<MasterAbilityProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _abilitiesById = new Dictionary<string, MasterAbility>(StringComparer.OrdinalIgnoreCase);
        _abilitiesBySkill = new Dictionary<string, List<MasterAbility>>(StringComparer.OrdinalIgnoreCase);
        _allAbilities = new List<MasterAbility>();

        LoadAbilities();
    }

    /// <inheritdoc />
    public IReadOnlyList<MasterAbility> GetAllAbilities() => _allAbilities.AsReadOnly();

    /// <inheritdoc />
    public MasterAbility? GetAbilityById(string abilityId)
    {
        if (string.IsNullOrWhiteSpace(abilityId))
        {
            _logger.LogDebug("GetAbilityById called with null or empty abilityId");
            return null;
        }

        if (_abilitiesById.TryGetValue(abilityId, out var ability))
        {
            _logger.LogDebug("Found ability {AbilityId}: {AbilityName}", abilityId, ability.Name);
            return ability;
        }

        _logger.LogDebug("Ability {AbilityId} not found", abilityId);
        return null;
    }

    /// <inheritdoc />
    public IReadOnlyList<MasterAbility> GetAbilitiesForSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            _logger.LogDebug("GetAbilitiesForSkill called with null or empty skillId");
            return Array.Empty<MasterAbility>();
        }

        if (_abilitiesBySkill.TryGetValue(skillId, out var abilities))
        {
            _logger.LogDebug(
                "Found {Count} abilities for skill {SkillId}",
                abilities.Count, skillId);
            return abilities.AsReadOnly();
        }

        _logger.LogDebug("No abilities found for skill {SkillId}", skillId);
        return Array.Empty<MasterAbility>();
    }

    /// <inheritdoc />
    public IReadOnlyList<MasterAbility> GetAbilitiesForSkillAndSubType(string skillId, string? subType)
    {
        var skillAbilities = GetAbilitiesForSkill(skillId);
        if (skillAbilities.Count == 0)
            return skillAbilities;

        var filtered = skillAbilities
            .Where(a => a.AppliesToSubType(subType))
            .ToList();

        _logger.LogDebug(
            "Filtered {OriginalCount} abilities to {FilteredCount} for skill {SkillId}, subtype {SubType}",
            skillAbilities.Count, filtered.Count, skillId, subType ?? "null");

        return filtered.AsReadOnly();
    }

    /// <inheritdoc />
    public bool HasAbilitiesForSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
            return false;

        return _abilitiesBySkill.ContainsKey(skillId);
    }

    private void LoadAbilities()
    {
        var configPaths = new[]
        {
            "config/master-abilities.json",
            "Configuration/master-abilities.json",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config/master-abilities.json")
        };

        string? configPath = null;
        foreach (var path in configPaths)
        {
            if (File.Exists(path))
            {
                configPath = path;
                break;
            }
        }

        if (configPath == null)
        {
            _logger.LogWarning(
                "Master abilities configuration not found in any of: {Paths}",
                string.Join(", ", configPaths));
            return;
        }

        try
        {
            _logger.LogDebug("Loading master abilities from {Path}", configPath);

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<MasterAbilitiesConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config?.MasterAbilities == null)
            {
                _logger.LogWarning("No master abilities found in configuration at {Path}", configPath);
                return;
            }

            foreach (var dto in config.MasterAbilities)
            {
                try
                {
                    var ability = CreateAbilityFromDto(dto);

                    _abilitiesById[ability.AbilityId] = ability;
                    _allAbilities.Add(ability);

                    if (!_abilitiesBySkill.TryGetValue(ability.SkillId, out var skillList))
                    {
                        skillList = new List<MasterAbility>();
                        _abilitiesBySkill[ability.SkillId] = skillList;
                    }
                    skillList.Add(ability);

                    _logger.LogDebug(
                        "Loaded master ability {AbilityId} ({AbilityName}) for skill {SkillId}",
                        ability.AbilityId, ability.Name, ability.SkillId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load master ability: {AbilityId}", dto.Id);
                }
            }

            _logger.LogInformation(
                "Loaded {Count} master abilities for {SkillCount} skills from {Path}",
                _allAbilities.Count,
                _abilitiesBySkill.Count,
                configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load master abilities configuration from {Path}", configPath);
        }
    }

    private static MasterAbility CreateAbilityFromDto(MasterAbilityDto dto)
    {
        var abilityType = Enum.Parse<MasterAbilityType>(dto.AbilityType, ignoreCase: true);
        var effect = CreateEffectFromDto(dto, abilityType);

        return MasterAbility.Create(
            abilityId: dto.Id,
            skillId: dto.SkillId,
            name: dto.Name,
            description: dto.Description,
            abilityType: abilityType,
            effect: effect,
            subTypes: dto.SubTypes,
            isPassive: dto.IsPassive,
            triggerMessage: dto.TriggerMessage);
    }

    private static MasterAbilityEffect CreateEffectFromDto(MasterAbilityDto dto, MasterAbilityType abilityType)
    {
        return abilityType switch
        {
            MasterAbilityType.AutoSucceed =>
                MasterAbilityEffect.ForAutoSucceed(dto.AutoSucceedDc ?? 10),

            MasterAbilityType.DiceBonus =>
                MasterAbilityEffect.ForDiceBonus(dto.DiceBonus ?? 0),

            MasterAbilityType.DamageReduction =>
                MasterAbilityEffect.ForDamageReduction(
                    dto.DamageThreshold ?? 0,
                    dto.ReductionAmount),

            MasterAbilityType.DistanceBonus =>
                MasterAbilityEffect.ForDistanceBonus(dto.DistanceBonus ?? 0),

            MasterAbilityType.RerollFailure =>
                MasterAbilityEffect.ForRerollFailure(
                    Enum.Parse<RerollPeriod>(dto.RerollPeriod ?? "Scene", ignoreCase: true)),

            MasterAbilityType.SpecialAction =>
                MasterAbilityEffect.ForSpecialAction(dto.SpecialEffect ?? string.Empty),

            _ => MasterAbilityEffect.Empty
        };
    }

    // Configuration DTOs for JSON deserialization
    private sealed class MasterAbilitiesConfig
    {
        public List<MasterAbilityDto>? MasterAbilities { get; set; }
    }

    private sealed class MasterAbilityDto
    {
        public string Id { get; set; } = string.Empty;
        public string SkillId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AbilityType { get; set; } = string.Empty;
        public List<string>? SubTypes { get; set; }
        public bool IsPassive { get; set; } = true;
        public string? TriggerMessage { get; set; }

        // Type-specific parameters
        public int? AutoSucceedDc { get; set; }
        public int? DiceBonus { get; set; }
        public int? DamageThreshold { get; set; }
        public int? ReductionAmount { get; set; }
        public int? DistanceBonus { get; set; }
        public string? RerollPeriod { get; set; }
        public string? SpecialEffect { get; set; }
    }
}
