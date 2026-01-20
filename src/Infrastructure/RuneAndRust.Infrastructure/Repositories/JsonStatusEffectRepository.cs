using System.Text.Json;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// JSON-based repository for status effect definitions.
/// </summary>
public class JsonStatusEffectRepository : IStatusEffectRepository
{
    private readonly Dictionary<string, StatusEffectDefinition> _effects = new();
    private readonly string _configPath;
    private bool _loaded;

    /// <summary>
    /// Creates a new JsonStatusEffectRepository.
    /// </summary>
    /// <param name="configPath">Path to the status-effects.json file.</param>
    public JsonStatusEffectRepository(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    /// <inheritdoc />
    public StatusEffectDefinition? GetById(string effectId)
    {
        EnsureLoaded();
        return _effects.GetValueOrDefault(effectId.ToLowerInvariant());
    }

    /// <inheritdoc />
    public IEnumerable<StatusEffectDefinition> GetAll()
    {
        EnsureLoaded();
        return _effects.Values;
    }

    /// <inheritdoc />
    public IEnumerable<StatusEffectDefinition> GetByCategory(EffectCategory category)
    {
        EnsureLoaded();
        return _effects.Values.Where(e => e.Category == category);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_configPath))
        {
            throw new FileNotFoundException(
                $"Status effects configuration not found: {_configPath}");
        }

        var json = File.ReadAllText(_configPath);
        var config = JsonSerializer.Deserialize<StatusEffectsConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Effects == null)
        {
            throw new InvalidDataException("Invalid status effects configuration");
        }

        foreach (var dto in config.Effects)
        {
            var definition = MapToDefinition(dto);
            _effects[definition.Id] = definition;
        }

        _loaded = true;
    }

    private static StatusEffectDefinition MapToDefinition(StatusEffectDto dto)
    {
        var category = Enum.Parse<EffectCategory>(dto.Category, ignoreCase: true);
        var durationType = Enum.Parse<DurationType>(dto.DurationType, ignoreCase: true);
        var stackingRule = Enum.Parse<StackingRule>(dto.StackingRule ?? "RefreshDuration", ignoreCase: true);

        var definition = StatusEffectDefinition.Create(
            dto.Id,
            dto.Name,
            dto.Description ?? string.Empty,
            category,
            durationType,
            dto.BaseDuration ?? 0,
            stackingRule,
            dto.MaxStacks ?? 1,
            dto.IconId);

        // Add stat modifiers
        if (dto.StatModifiers != null)
        {
            foreach (var modDto in dto.StatModifiers)
            {
                var modType = Enum.Parse<StatModifierType>(modDto.ModifierType, ignoreCase: true);
                var modifier = new StatModifier(modDto.StatId, modType, modDto.Value);
                definition.WithStatModifier(modifier);
            }
        }

        // Add DoT
        if (dto.DamagePerTurn.HasValue && !string.IsNullOrEmpty(dto.DamageType))
        {
            definition.WithDamageOverTime(dto.DamagePerTurn.Value, dto.DamageType);
        }

        // Add HoT
        if (dto.HealingPerTurn.HasValue)
        {
            definition.WithHealingOverTime(dto.HealingPerTurn.Value);
        }

        // Add action prevention
        if (dto.PreventsActions == true || dto.PreventsMovement == true ||
            dto.PreventsAbilities == true || dto.PreventsAttacking == true)
        {
            definition.WithActionPrevention(
                dto.PreventsActions ?? false,
                dto.PreventsMovement ?? false,
                dto.PreventsAbilities ?? false,
                dto.PreventsAttacking ?? false);
        }

        // Add removal trigger
        if (!string.IsNullOrEmpty(dto.RemovalTrigger))
        {
            definition.WithRemovalTrigger(dto.RemovalTrigger);
        }

        // Add resource pool
        if (dto.ResourcePool.HasValue)
        {
            definition.WithResourcePool(dto.ResourcePool.Value);
        }

        // Add damage resistances/vulnerabilities
        if (dto.ResistsDamageTypes != null)
        {
            definition.WithDamageResistance(dto.ResistsDamageTypes);
        }

        if (dto.VulnerableToDamageTypes != null)
        {
            definition.WithDamageVulnerability(dto.VulnerableToDamageTypes);
        }

        return definition;
    }

    private class StatusEffectsConfig
    {
        public List<StatusEffectDto>? Effects { get; set; }
    }

    private class StatusEffectDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "Debuff";
        public string DurationType { get; set; } = "Turns";
        public int? BaseDuration { get; set; }
        public string? StackingRule { get; set; }
        public int? MaxStacks { get; set; }
        public string? IconId { get; set; }
        public List<StatModifierDto>? StatModifiers { get; set; }
        public int? DamagePerTurn { get; set; }
        public string? DamageType { get; set; }
        public int? HealingPerTurn { get; set; }
        public bool? PreventsActions { get; set; }
        public bool? PreventsMovement { get; set; }
        public bool? PreventsAbilities { get; set; }
        public bool? PreventsAttacking { get; set; }
        public string? RemovalTrigger { get; set; }
        public int? ResourcePool { get; set; }
        public string[]? ResistsDamageTypes { get; set; }
        public string[]? VulnerableToDamageTypes { get; set; }
    }

    private class StatModifierDto
    {
        public string StatId { get; set; } = string.Empty;
        public string ModifierType { get; set; } = "Flat";
        public float Value { get; set; }
    }
}
