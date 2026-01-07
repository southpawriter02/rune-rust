using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Configuration;

/// <summary>
/// Provides game configuration by loading and caching JSON files.
/// </summary>
public class JsonConfigurationProvider : IGameConfigurationProvider
{
    private readonly string _configPath;
    private readonly ILogger<JsonConfigurationProvider> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private IReadOnlyList<RaceDefinition>? _races;
    private IReadOnlyList<BackgroundDefinition>? _backgrounds;
    private IReadOnlyList<AttributeDefinition>? _attributes;
    private PointBuyRules? _pointBuyRules;
    private LexiconConfiguration? _lexiconConfig;
    private ThemeConfiguration? _themeConfig;
    private IReadOnlyDictionary<string, DescriptorPool>? _descriptorPools;
    private IReadOnlyList<ArchetypeDefinition>? _archetypes;
    private IReadOnlyList<ClassDefinition>? _classes;
    private IReadOnlyList<ResourceTypeDefinition>? _resourceTypes;
    private IReadOnlyList<AbilityDefinition>? _abilities;

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Creates a new JSON configuration provider.
    /// </summary>
    /// <param name="configPath">Path to the config directory.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public JsonConfigurationProvider(string configPath, ILogger<JsonConfigurationProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = DefaultJsonOptions;

        _logger.LogInformation("JsonConfigurationProvider initialized with path: {ConfigPath}", configPath);
    }

    /// <inheritdoc/>
    public IReadOnlyList<RaceDefinition> GetRaces()
    {
        if (_races != null) return _races;

        var filePath = Path.Combine(_configPath, "races.json");
        var config = LoadJsonFile<RacesConfig>(filePath);
        _races = config?.Races ?? GetDefaultRaces();

        _logger.LogDebug("Loaded {Count} races", _races.Count);
        return _races;
    }

    /// <inheritdoc/>
    public RaceDefinition? GetRaceById(string raceId)
    {
        return GetRaces().FirstOrDefault(r =>
            r.Id.Equals(raceId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<BackgroundDefinition> GetBackgrounds()
    {
        if (_backgrounds != null) return _backgrounds;

        var filePath = Path.Combine(_configPath, "backgrounds.json");
        var config = LoadJsonFile<BackgroundsConfig>(filePath);
        _backgrounds = config?.Backgrounds ?? GetDefaultBackgrounds();

        _logger.LogDebug("Loaded {Count} backgrounds", _backgrounds.Count);
        return _backgrounds;
    }

    /// <inheritdoc/>
    public BackgroundDefinition? GetBackgroundById(string backgroundId)
    {
        return GetBackgrounds().FirstOrDefault(b =>
            b.Id.Equals(backgroundId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AttributeDefinition> GetAttributes()
    {
        if (_attributes != null) return _attributes;

        var filePath = Path.Combine(_configPath, "attributes.json");
        var config = LoadJsonFile<AttributesConfig>(filePath);
        _attributes = config?.Attributes ?? GetDefaultAttributes();

        _logger.LogDebug("Loaded {Count} attributes", _attributes.Count);
        return _attributes;
    }

    /// <inheritdoc/>
    public PointBuyRules GetPointBuyRules()
    {
        if (_pointBuyRules != null) return _pointBuyRules;

        var filePath = Path.Combine(_configPath, "attributes.json");
        var config = LoadJsonFile<AttributesConfig>(filePath);
        _pointBuyRules = config?.PointBuyRules ?? new PointBuyRules();

        _logger.LogDebug("Loaded point-buy rules: {StartingPoints} points",
            _pointBuyRules.StartingPoints);
        return _pointBuyRules;
    }

    /// <inheritdoc/>
    public LexiconConfiguration GetLexiconConfiguration()
    {
        if (_lexiconConfig != null) return _lexiconConfig;

        var filePath = Path.Combine(_configPath, "lexicon.json");
        var config = LoadJsonFile<LexiconJsonConfig>(filePath);
        _lexiconConfig = config?.ToConfiguration() ?? new LexiconConfiguration();

        _logger.LogDebug("Loaded lexicon with {Count} terms", _lexiconConfig.Terms.Count);
        return _lexiconConfig;
    }

    /// <inheritdoc/>
    public ThemeConfiguration GetThemeConfiguration()
    {
        if (_themeConfig != null) return _themeConfig;

        var filePath = Path.Combine(_configPath, "themes.json");
        var config = LoadJsonFile<ThemeJsonConfig>(filePath);
        _themeConfig = config?.ToConfiguration() ?? new ThemeConfiguration();

        _logger.LogDebug("Loaded theme config with active theme: {Theme}", _themeConfig.ActiveTheme);
        return _themeConfig;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, DescriptorPool> GetAllDescriptorPools()
    {
        if (_descriptorPools != null) return _descriptorPools;

        var pools = new Dictionary<string, DescriptorPool>();
        var descriptorPath = Path.Combine(_configPath, "descriptors");

        if (!Directory.Exists(descriptorPath))
        {
            _logger.LogWarning("Descriptors directory not found: {Path}", descriptorPath);
            _descriptorPools = pools;
            return _descriptorPools;
        }

        foreach (var file in Directory.GetFiles(descriptorPath, "*.json"))
        {
            var config = LoadJsonFile<DescriptorConfiguration>(file);
            if (config == null) continue;

            foreach (var (poolId, pool) in config.Pools)
            {
                var fullPath = $"{config.Category}.{poolId}";
                pools[fullPath] = pool;
                _logger.LogDebug("Loaded descriptor pool: {PoolPath}", fullPath);
            }
        }

        _descriptorPools = pools;
        _logger.LogDebug("Loaded {Count} descriptor pools total", pools.Count);
        return _descriptorPools;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ArchetypeDefinition> GetArchetypes()
    {
        if (_archetypes != null) return _archetypes;

        var filePath = Path.Combine(_configPath, "archetypes.json");
        var config = LoadJsonFile<ArchetypesJsonConfig>(filePath);
        _archetypes = config?.Archetypes.Select(ToArchetypeDefinition).ToList()
            ?? GetDefaultArchetypes();

        _logger.LogDebug("Loaded {Count} archetypes", _archetypes.Count);
        return _archetypes;
    }

    /// <inheritdoc/>
    public ArchetypeDefinition? GetArchetypeById(string archetypeId)
    {
        return GetArchetypes().FirstOrDefault(a =>
            a.Id.Equals(archetypeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<ClassDefinition> GetClasses()
    {
        if (_classes != null) return _classes;

        var filePath = Path.Combine(_configPath, "classes.json");
        var config = LoadJsonFile<ClassesJsonConfig>(filePath);
        _classes = config?.Classes.Select(ToClassDefinition).ToList()
            ?? GetDefaultClasses();

        _logger.LogDebug("Loaded {Count} classes", _classes.Count);
        return _classes;
    }

    /// <inheritdoc/>
    public ClassDefinition? GetClassById(string classId)
    {
        return GetClasses().FirstOrDefault(c =>
            c.Id.Equals(classId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<ClassDefinition> GetClassesForArchetype(string archetypeId)
    {
        return GetClasses()
            .Where(c => c.ArchetypeId.Equals(archetypeId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ResourceTypeDefinition> GetResourceTypes()
    {
        if (_resourceTypes != null) return _resourceTypes;

        var filePath = Path.Combine(_configPath, "resources.json");
        var config = LoadJsonFile<ResourceTypesJsonConfig>(filePath);
        _resourceTypes = config?.ResourceTypes.Select(ToResourceTypeDefinition).ToList()
            ?? GetDefaultResourceTypes();

        _logger.LogDebug("Loaded {Count} resource types", _resourceTypes.Count);
        return _resourceTypes;
    }

    /// <inheritdoc/>
    public ResourceTypeDefinition? GetResourceTypeById(string resourceTypeId)
    {
        return GetResourceTypes().FirstOrDefault(r =>
            r.Id.Equals(resourceTypeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AbilityDefinition> GetAbilities()
    {
        if (_abilities != null) return _abilities;

        var filePath = Path.Combine(_configPath, "abilities.json");
        var config = LoadJsonFile<AbilitiesJsonConfig>(filePath);
        _abilities = config?.Abilities?.Select(ToAbilityDefinition).ToList()
            ?? GetDefaultAbilities();

        _logger.LogDebug("Loaded {Count} abilities", _abilities.Count);
        return _abilities;
    }

    /// <inheritdoc/>
    public AbilityDefinition? GetAbilityById(string abilityId)
    {
        return GetAbilities().FirstOrDefault(a =>
            a.Id.Equals(abilityId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AbilityDefinition> GetAbilitiesForClass(string classId)
    {
        return GetAbilities()
            .Where(a => a.IsAvailableToClass(classId))
            .ToList();
    }

    private static ResourceTypeDefinition ToResourceTypeDefinition(ResourceTypeJsonConfig config)
    {
        return ResourceTypeDefinition.Create(
            config.Id,
            config.DisplayName,
            config.Abbreviation,
            config.Description,
            config.Color,
            config.DefaultMax,
            config.RegenPerTurn,
            config.DecayPerTurn,
            config.DecayOnlyOutOfCombat,
            config.BuildOnDamageDealt,
            config.BuildOnDamageTaken,
            config.BuildOnHeal,
            config.IsUniversal,
            config.StartsAtZero,
            config.SortOrder);
    }

    private static List<ResourceTypeDefinition> GetDefaultResourceTypes() =>
    [
        ResourceTypeDefinition.Create("health", "Vitality", "HP", "Life force", "#FF0000", 100, isUniversal: true)
    ];

    private static ArchetypeDefinition ToArchetypeDefinition(ArchetypeJsonConfig config)
    {
        var tendency = Enum.TryParse<StatTendency>(config.StatTendency, true, out var t)
            ? t : StatTendency.Balanced;

        return ArchetypeDefinition.Create(
            config.Id,
            config.Name,
            config.Description,
            config.PlaystyleSummary,
            tendency,
            config.SortOrder);
    }

    private static ClassDefinition ToClassDefinition(ClassJsonConfig config)
    {
        var modifiers = config.StatModifiers ?? new StatModifiersJsonConfig();
        var growth = config.GrowthRates ?? new StatModifiersJsonConfig();

        ClassRequirements? requirements = null;
        if (config.Requirements != null)
        {
            requirements = new ClassRequirements
            {
                AllowedRaceIds = config.Requirements.AllowedRaceIds,
                MinimumAttributes = config.Requirements.MinimumAttributes
            };
        }

        return ClassDefinition.Create(
            config.Id,
            config.Name,
            config.Description,
            config.ArchetypeId,
            new StatModifiers
            {
                MaxHealth = modifiers.MaxHealth,
                Attack = modifiers.Attack,
                Defense = modifiers.Defense,
                Might = modifiers.Might,
                Fortitude = modifiers.Fortitude,
                Will = modifiers.Will,
                Wits = modifiers.Wits,
                Finesse = modifiers.Finesse
            },
            new StatModifiers
            {
                MaxHealth = growth.MaxHealth,
                Attack = growth.Attack,
                Defense = growth.Defense,
                Might = growth.Might,
                Fortitude = growth.Fortitude,
                Will = growth.Will,
                Wits = growth.Wits,
                Finesse = growth.Finesse
            },
            config.PrimaryResourceId,
            config.StartingAbilityIds,
            requirements,
            config.SortOrder);
    }

    private static List<ArchetypeDefinition> GetDefaultArchetypes() =>
    [
        ArchetypeDefinition.Create("warrior", "Warrior", "Martial combatant.", "Frontline fighter", StatTendency.Defensive)
    ];

    private static List<ClassDefinition> GetDefaultClasses() =>
    [
        ClassDefinition.Create("shieldmaiden", "Shieldmaiden", "Stalwart defender.", "warrior", StatModifiers.None, StatModifiers.None, "rage")
    ];


    private T? LoadJsonFile<T>(string filePath) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Configuration file not found: {FilePath}", filePath);
                return null;
            }

            var json = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            _logger.LogDebug("Loaded configuration from: {FilePath}", filePath);
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse configuration file: {FilePath}", filePath);
            return null;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to read configuration file: {FilePath}", filePath);
            return null;
        }
    }

    private static List<RaceDefinition> GetDefaultRaces() =>
    [
        new RaceDefinition
        {
            Id = "human",
            Name = "Human",
            Description = "Versatile and adaptable.",
            AttributeModifiers = new Dictionary<string, int>
            {
                ["might"] = 1, ["fortitude"] = 1, ["will"] = 1, ["wits"] = 1, ["finesse"] = 1
            },
            TraitName = "Adaptable",
            IsPlayable = true,
            SortOrder = 1
        }
    ];

    private static List<BackgroundDefinition> GetDefaultBackgrounds() =>
    [
        new BackgroundDefinition
        {
            Id = "soldier",
            Name = "Soldier",
            Category = "Profession",
            Description = "A veteran of combat.",
            AttributeBonuses = new Dictionary<string, int> { ["might"] = 2 },
            IsPlayable = true,
            SortOrder = 1
        }
    ];

    private static List<AttributeDefinition> GetDefaultAttributes() =>
    [
        new AttributeDefinition { Id = "might", Name = "Might", Abbreviation = "MIG", SortOrder = 1 },
        new AttributeDefinition { Id = "fortitude", Name = "Fortitude", Abbreviation = "FOR", SortOrder = 2 },
        new AttributeDefinition { Id = "will", Name = "Will", Abbreviation = "WIL", SortOrder = 3 },
        new AttributeDefinition { Id = "wits", Name = "Wits", Abbreviation = "WIT", SortOrder = 4 },
        new AttributeDefinition { Id = "finesse", Name = "Finesse", Abbreviation = "FIN", SortOrder = 5 }
    ];

    // JSON config file structure classes
    private class RacesConfig
    {
        public List<RaceDefinition> Races { get; set; } = [];
    }

    private class BackgroundsConfig
    {
        public List<BackgroundDefinition> Backgrounds { get; set; } = [];
    }

    private class AttributesConfig
    {
        public PointBuyRules? PointBuyRules { get; set; }
        public List<AttributeDefinition> Attributes { get; set; } = [];
    }

    // Lexicon JSON structure
    private class LexiconJsonConfig
    {
        public List<LexiconContext> Contexts { get; set; } = [];
        public Dictionary<string, TermJsonConfig> Terms { get; set; } = [];

        public LexiconConfiguration ToConfiguration() => new()
        {
            Contexts = Contexts,
            Terms = Terms.ToDictionary(
                kvp => kvp.Key,
                kvp => new TermDefinition
                {
                    Default = kvp.Value.Default ?? kvp.Key,
                    Synonyms = kvp.Value.Synonyms ?? [],
                    Contextual = kvp.Value.Contextual?.ToDictionary(
                        c => c.Key,
                        c => (IReadOnlyList<string>)c.Value) ?? new Dictionary<string, IReadOnlyList<string>>(),
                    Weights = kvp.Value.Weights ?? new Dictionary<string, int>(),
                    Severity = kvp.Value.Severity?.ToDictionary(
                        s => s.Key,
                        s => (IReadOnlyList<string>)s.Value) ?? new Dictionary<string, IReadOnlyList<string>>()
                })
        };
    }

    private class TermJsonConfig
    {
        public string? Default { get; set; }
        public List<string>? Synonyms { get; set; }
        public Dictionary<string, List<string>>? Contextual { get; set; }
        public Dictionary<string, int>? Weights { get; set; }
        public Dictionary<string, List<string>>? Severity { get; set; }
    }

    // Theme JSON structure
    private class ThemeJsonConfig
    {
        public string ActiveTheme { get; set; } = "dark_fantasy";
        public Dictionary<string, ThemePresetJson> Themes { get; set; } = [];

        public ThemeConfiguration ToConfiguration() => new()
        {
            ActiveTheme = ActiveTheme,
            Themes = Themes.ToDictionary(
                kvp => kvp.Key,
                kvp => new ThemePreset
                {
                    Id = kvp.Value.Id ?? kvp.Key,
                    Name = kvp.Value.Name ?? kvp.Key,
                    Description = kvp.Value.Description ?? "",
                    DescriptorOverrides = kvp.Value.DescriptorOverrides ?? new Dictionary<string, string>(),
                    ExcludedTerms = kvp.Value.ExcludedTerms ?? [],
                    EmphasizedTerms = kvp.Value.EmphasizedTerms ?? []
                })
        };
    }

    private class ThemePresetJson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, string>? DescriptorOverrides { get; set; }
        public List<string>? ExcludedTerms { get; set; }
        public List<string>? EmphasizedTerms { get; set; }
    }

    // Archetype JSON structure
    private class ArchetypesJsonConfig
    {
        public List<ArchetypeJsonConfig> Archetypes { get; set; } = [];
    }

    private class ArchetypeJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PlaystyleSummary { get; set; } = string.Empty;
        public string StatTendency { get; set; } = "balanced";
        public int SortOrder { get; set; } = 0;
    }

    // Class JSON structure
    private class ClassesJsonConfig
    {
        public List<ClassJsonConfig> Classes { get; set; } = [];
    }

    private class ClassJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ArchetypeId { get; set; } = string.Empty;
        public StatModifiersJsonConfig? StatModifiers { get; set; }
        public StatModifiersJsonConfig? GrowthRates { get; set; }
        public string PrimaryResourceId { get; set; } = string.Empty;
        public List<string>? StartingAbilityIds { get; set; }
        public ClassRequirementsJsonConfig? Requirements { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    private class StatModifiersJsonConfig
    {
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Might { get; set; }
        public int Fortitude { get; set; }
        public int Will { get; set; }
        public int Wits { get; set; }
        public int Finesse { get; set; }
    }

    private class ClassRequirementsJsonConfig
    {
        public List<string>? AllowedRaceIds { get; set; }
        public Dictionary<string, int>? MinimumAttributes { get; set; }
    }

    // Resource Type JSON structure
    private class ResourceTypesJsonConfig
    {
        public List<ResourceTypeJsonConfig> ResourceTypes { get; set; } = [];
    }

    private class ResourceTypeJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF";
        public int DefaultMax { get; set; } = 100;
        public int RegenPerTurn { get; set; } = 0;
        public int DecayPerTurn { get; set; } = 0;
        public bool DecayOnlyOutOfCombat { get; set; } = true;
        public int BuildOnDamageDealt { get; set; } = 0;
        public int BuildOnDamageTaken { get; set; } = 0;
        public int BuildOnHeal { get; set; } = 0;
        public bool IsUniversal { get; set; } = false;
        public bool StartsAtZero { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    // Ability mapping function
    private static AbilityDefinition ToAbilityDefinition(AbilityJsonConfig config)
    {
        var cost = string.IsNullOrEmpty(config.CostResource)
            ? AbilityCost.None
            : AbilityCost.Create(config.CostResource, config.CostAmount);

        var effects = config.Effects?.Select(ToAbilityEffect).ToList()
            ?? [];

        var targetType = Enum.TryParse<AbilityTargetType>(config.TargetType, true, out var tt)
            ? tt : AbilityTargetType.SingleEnemy;

        return AbilityDefinition.Create(
            config.Id,
            config.Name,
            config.Description,
            config.ClassIds ?? [],
            cost,
            config.Cooldown,
            effects,
            targetType,
            config.UnlockLevel,
            config.Tags);
    }

    private static AbilityEffect ToAbilityEffect(AbilityEffectJsonConfig config)
    {
        var effectType = Enum.TryParse<AbilityEffectType>(config.Type, true, out var et)
            ? et : AbilityEffectType.Damage;

        return new AbilityEffect
        {
            EffectType = effectType,
            Value = config.Value,
            Duration = config.Duration,
            StatusEffect = config.StatusEffect,
            Chance = config.Chance,
            ScalingStat = config.ScalingStat?.ToLowerInvariant(),
            ScalingMultiplier = config.ScalingMultiplier,
            Description = config.Description
        };
    }

    private static List<AbilityDefinition> GetDefaultAbilities() =>
    [
        AbilityDefinition.Create(
            "basic-attack",
            "Basic Attack",
            "A basic melee attack.",
            ["shieldmaiden", "shadow-walker", "galdr-caster", "blood-priest", "scrap-tinker"],
            AbilityCost.None,
            cooldown: 0,
            [AbilityEffect.Damage(10, "attack", 0.5f)],
            AbilityTargetType.SingleEnemy)
    ];

    // Ability JSON structure
    private class AbilitiesJsonConfig
    {
        public List<AbilityJsonConfig>? Abilities { get; set; }
    }

    private class AbilityJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string>? ClassIds { get; set; }
        public string CostResource { get; set; } = string.Empty;
        public int CostAmount { get; set; } = 0;
        public int Cooldown { get; set; } = 0;
        public string TargetType { get; set; } = "SingleEnemy";
        public int UnlockLevel { get; set; } = 1;
        public List<string>? Tags { get; set; }
        public List<AbilityEffectJsonConfig>? Effects { get; set; }
    }

    private class AbilityEffectJsonConfig
    {
        public string Type { get; set; } = string.Empty;
        public int Value { get; set; } = 0;
        public int Duration { get; set; } = 0;
        public string? StatusEffect { get; set; }
        public float Chance { get; set; } = 1.0f;
        public string? ScalingStat { get; set; }
        public float ScalingMultiplier { get; set; } = 0f;
        public string? Description { get; set; }
    }
}
