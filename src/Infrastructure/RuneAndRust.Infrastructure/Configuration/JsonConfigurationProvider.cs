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
    private IReadOnlyList<SkillDefinition>? _skills;
    private IReadOnlyList<DifficultyClassDefinition>? _difficultyClasses;
    private IReadOnlyDictionary<string, IReadOnlyList<string>>? _diceDescriptors;
    private ProgressionDefinition? _progression;
    private IReadOnlyList<MonsterDefinition>? _monsters;
    private IReadOnlyList<DamageTypeDefinition>? _damageTypes;
    private IReadOnlyList<TierDefinition>? _tiers;
    private IReadOnlyList<MonsterTrait>? _traits;
    private IReadOnlyList<CurrencyDefinition>? _currencies;
    private EnvironmentCategoryConfiguration? _environmentCategories;
    private BiomeConfiguration? _biomeConfig;
    private SensoryConfiguration? _sensoryConfig;
    private ObjectDescriptorConfiguration? _objectDescriptorConfig;
    private AmbientEventConfiguration? _ambientEventConfig;

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

    /// <inheritdoc/>
    public IReadOnlyList<SkillDefinition> GetSkills()
    {
        if (_skills != null) return _skills;

        var filePath = Path.Combine(_configPath, "skills.json");
        var config = LoadJsonFile<SkillsJsonConfig>(filePath);
        
        if (config?.Skills == null || config.Skills.Count == 0)
        {
            _logger.LogWarning("No skills found in configuration, using defaults");
            _skills = GetDefaultSkills();
            return _skills;
        }

        _skills = config.Skills
            .Select(ToSkillDefinition)
            .Where(s => s != null)
            .Cast<SkillDefinition>()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToList();

        _logger.LogDebug("Loaded {Count} skills", _skills.Count);
        return _skills;
    }

    /// <inheritdoc/>
    public SkillDefinition? GetSkillById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return GetSkills().FirstOrDefault(s =>
            s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<DifficultyClassDefinition> GetDifficultyClasses()
    {
        if (_difficultyClasses != null) return _difficultyClasses;

        var filePath = Path.Combine(_configPath, "difficulty.json");
        var config = LoadJsonFile<DifficultyJsonConfig>(filePath);
        
        if (config?.DifficultyClasses == null || config.DifficultyClasses.Count == 0)
        {
            _logger.LogWarning("No difficulty classes found in configuration, using defaults");
            _difficultyClasses = GetDefaultDifficultyClasses();
            return _difficultyClasses;
        }

        _difficultyClasses = config.DifficultyClasses
            .Select(ToDifficultyClassDefinition)
            .Where(dc => dc != null)
            .Cast<DifficultyClassDefinition>()
            .OrderBy(dc => dc.SortOrder)
            .ToList();

        _logger.LogDebug("Loaded {Count} difficulty classes", _difficultyClasses.Count);
        return _difficultyClasses;
    }

    /// <inheritdoc/>
    public DifficultyClassDefinition? GetDifficultyClassById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return GetDifficultyClasses().FirstOrDefault(dc =>
            dc.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetDiceDescriptors()
    {
        if (_diceDescriptors != null) return _diceDescriptors;

        try
        {
            var filePath = Path.Combine(_configPath, "dice-descriptors.json");

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Dice descriptors configuration not found at {Path}", filePath);
                _diceDescriptors = new Dictionary<string, IReadOnlyList<string>>();
                return _diceDescriptors;
            }

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<DiceDescriptorsJsonConfig>(json, _jsonOptions);

            if (config?.Descriptors == null)
            {
                _diceDescriptors = new Dictionary<string, IReadOnlyList<string>>();
                return _diceDescriptors;
            }

            _diceDescriptors = config.Descriptors
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly());

            _logger.LogInformation("Loaded {Count} dice descriptor categories", _diceDescriptors.Count);
            return _diceDescriptors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dice descriptors configuration");
            _diceDescriptors = new Dictionary<string, IReadOnlyList<string>>();
            return _diceDescriptors;
        }
    }

    /// <inheritdoc/>
    public ProgressionDefinition GetProgressionConfiguration()
    {
        if (_progression != null) return _progression;

        var filePath = Path.Combine(_configPath, "progression.json");
        var config = LoadJsonFile<ProgressionJsonConfig>(filePath);

        if (config == null)
        {
            _logger.LogInformation("No progression.json found, using default progression configuration");
            _progression = ProgressionDefinition.Default;
            return _progression;
        }

        _progression = ToProgressionDefinition(config);
        _logger.LogDebug("Loaded progression configuration: MaxLevel={MaxLevel}, Curve={Curve}",
            _progression.MaxLevel, _progression.CurveType);
        return _progression;
    }

    /// <inheritdoc/>
    public IReadOnlyList<MonsterDefinition> GetMonsters()
    {
        if (_monsters != null) return _monsters;

        var filePath = Path.Combine(_configPath, "monsters.json");
        var config = LoadJsonFile<MonstersJsonConfig>(filePath);

        if (config?.Monsters == null || config.Monsters.Count == 0)
        {
            _logger.LogWarning("No monsters found in configuration, using defaults");
            _monsters = GetDefaultMonsters();
            return _monsters;
        }

        _monsters = config.Monsters
            .Select(ToMonsterDefinition)
            .Where(m => m != null)
            .Cast<MonsterDefinition>()
            .ToList();

        _logger.LogDebug("Loaded {Count} monsters", _monsters.Count);
        return _monsters;
    }

    /// <inheritdoc/>
    public MonsterDefinition? GetMonsterById(string monsterId)
    {
        if (string.IsNullOrWhiteSpace(monsterId)) return null;
        return GetMonsters().FirstOrDefault(m =>
            m.Id.Equals(monsterId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<DamageTypeDefinition> GetDamageTypes()
    {
        if (_damageTypes != null) return _damageTypes;

        var filePath = Path.Combine(_configPath, "damage-types.json");
        var config = LoadJsonFile<DamageTypesJsonConfig>(filePath);

        if (config?.DamageTypes == null || config.DamageTypes.Count == 0)
        {
            _logger.LogWarning("No damage types found in configuration, using defaults");
            _damageTypes = GetDefaultDamageTypes();
            return _damageTypes;
        }

        _damageTypes = config.DamageTypes
            .Select(ToDamageTypeDefinition)
            .Where(d => d != null)
            .Cast<DamageTypeDefinition>()
            .OrderBy(d => d.SortOrder)
            .ToList();

        _logger.LogDebug("Loaded {Count} damage types", _damageTypes.Count);
        return _damageTypes;
    }

    /// <inheritdoc/>
    public DamageTypeDefinition? GetDamageTypeById(string damageTypeId)
    {
        if (string.IsNullOrWhiteSpace(damageTypeId)) return null;
        return GetDamageTypes().FirstOrDefault(d =>
            d.Id.Equals(damageTypeId, StringComparison.OrdinalIgnoreCase));
    }

    // ===== Tier & Trait Methods (v0.0.9c) =====

    /// <inheritdoc/>
    public IReadOnlyList<TierDefinition> GetTiers()
    {
        if (_tiers != null) return _tiers;

        var filePath = Path.Combine(_configPath, "tiers.json");
        var config = LoadJsonFile<TiersJsonConfig>(filePath);

        if (config?.Tiers == null || config.Tiers.Count == 0)
        {
            _logger.LogWarning("No tiers found in configuration, using defaults");
            _tiers = GetDefaultTiers();
            return _tiers;
        }

        _tiers = config.Tiers
            .Select(ToTierDefinition)
            .Where(t => t != null)
            .Cast<TierDefinition>()
            .OrderBy(t => t.SortOrder)
            .ToList();

        _logger.LogDebug("Loaded {Count} tiers", _tiers.Count);
        return _tiers;
    }

    /// <inheritdoc/>
    public TierDefinition? GetTierById(string tierId)
    {
        if (string.IsNullOrWhiteSpace(tierId)) return null;
        return GetTiers().FirstOrDefault(t =>
            t.Id.Equals(tierId, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IReadOnlyList<MonsterTrait> GetTraits()
    {
        if (_traits != null) return _traits;

        var filePath = Path.Combine(_configPath, "traits.json");
        var config = LoadJsonFile<TraitsJsonConfig>(filePath);

        if (config?.Traits == null || config.Traits.Count == 0)
        {
            _logger.LogWarning("No traits found in configuration, using defaults");
            _traits = GetDefaultTraits();
            return _traits;
        }

        _traits = config.Traits
            .Select(ToMonsterTrait)
            .Where(t => t != null)
            .Cast<MonsterTrait>()
            .OrderBy(t => t.SortOrder)
            .ToList();

        _logger.LogDebug("Loaded {Count} traits", _traits.Count);
        return _traits;
    }

    /// <inheritdoc/>
    public MonsterTrait? GetTraitById(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId)) return null;
        return GetTraits().FirstOrDefault(t =>
            t.Id.Equals(traitId, StringComparison.OrdinalIgnoreCase));
    }

    // ===== Currency Methods (v0.0.9d) =====

    /// <inheritdoc/>
    public IReadOnlyList<CurrencyDefinition> GetCurrencies()
    {
        if (_currencies != null) return _currencies;

        var filePath = Path.Combine(_configPath, "currency.json");
        var config = LoadJsonFile<CurrencyJsonConfig>(filePath);

        if (config?.Currencies == null || config.Currencies.Count == 0)
        {
            _logger.LogWarning("No currencies found in configuration, using defaults");
            _currencies = GetDefaultCurrencies();
            return _currencies;
        }

        _currencies = config.Currencies
            .Select(ToCurrencyDefinition)
            .Where(c => c != null)
            .Cast<CurrencyDefinition>()
            .OrderBy(c => c.SortOrder)
            .ToList();

        _logger.LogDebug("Loaded {Count} currencies", _currencies.Count);
        return _currencies;
    }

    /// <inheritdoc/>
    public CurrencyDefinition? GetCurrencyById(string currencyId)
    {
        if (string.IsNullOrWhiteSpace(currencyId)) return null;
        return GetCurrencies().FirstOrDefault(c =>
            c.Id.Equals(currencyId, StringComparison.OrdinalIgnoreCase));
    }

    private CurrencyDefinition? ToCurrencyDefinition(CurrencyConfigJsonEntry config)
    {
        try
        {
            return CurrencyDefinition.Create(
                id: config.Id,
                name: config.Name,
                pluralName: config.PluralName ?? config.Name,
                symbol: config.Symbol ?? config.Id[..1].ToUpper(),
                color: config.Color ?? "yellow",
                sortOrder: config.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create currency definition for {Id}", config.Id);
            return null;
        }
    }

    private static List<CurrencyDefinition> GetDefaultCurrencies() =>
    [
        CurrencyDefinition.Gold
    ];

    // ===== Environment Configuration Methods (v0.0.11a) =====

    /// <inheritdoc/>
    public EnvironmentCategoryConfiguration GetEnvironmentCategories()
    {
        if (_environmentCategories != null) return _environmentCategories;

        var filePath = Path.Combine(_configPath, "environment-categories.json");
        var config = LoadJsonFile<EnvironmentCategoryJsonConfig>(filePath);

        if (config == null)
        {
            _logger.LogWarning("No environment categories found, using defaults");
            _environmentCategories = GetDefaultEnvironmentCategories();
            return _environmentCategories;
        }

        _environmentCategories = ToEnvironmentCategoryConfiguration(config);
        _logger.LogDebug("Loaded {CategoryCount} environment categories with {RuleCount} exclusion rules",
            _environmentCategories.Categories.Count,
            _environmentCategories.ExclusionRules.Count);
        return _environmentCategories;
    }

    /// <inheritdoc/>
    public BiomeConfiguration GetBiomeConfiguration()
    {
        if (_biomeConfig != null) return _biomeConfig;

        var filePath = Path.Combine(_configPath, "biomes.json");
        var config = LoadJsonFile<BiomeJsonConfig>(filePath);

        if (config == null)
        {
            _logger.LogWarning("No biomes found, using defaults");
            _biomeConfig = GetDefaultBiomeConfiguration();
            return _biomeConfig;
        }

        _biomeConfig = ToBiomeConfiguration(config);
        _logger.LogDebug("Loaded {BiomeCount} biomes", _biomeConfig.Biomes.Count);
        return _biomeConfig;
    }

    // ===== Sensory Configuration Methods (v0.0.11c) =====

    /// <inheritdoc/>
    public SensoryConfiguration GetSensoryConfiguration()
    {
        if (_sensoryConfig != null) return _sensoryConfig;

        // Build sensory configuration from default definitions
        // These define how the SensoryDescriptorService maps light sources, weather, etc.
        // to descriptor pools
        _sensoryConfig = GetDefaultSensoryConfiguration();
        _logger.LogDebug("Loaded sensory configuration with {LightSources} light sources, {Weather} weather conditions",
            _sensoryConfig.LightSources.Count,
            _sensoryConfig.WeatherConditions.Count);
        return _sensoryConfig;
    }

    /// <inheritdoc/>
    public ObjectDescriptorConfiguration GetObjectDescriptorConfiguration()
    {
        if (_objectDescriptorConfig != null) return _objectDescriptorConfig;

        // Build object descriptor configuration from default definitions
        _objectDescriptorConfig = GetDefaultObjectDescriptorConfiguration();
        _logger.LogDebug("Loaded object descriptor configuration with {ObjectTypes} object types",
            _objectDescriptorConfig.ObjectTypes.Count);
        return _objectDescriptorConfig;
    }

    /// <inheritdoc/>
    public AmbientEventConfiguration GetAmbientEventConfiguration()
    {
        if (_ambientEventConfig != null) return _ambientEventConfig;

        // Build ambient event configuration from default definitions
        _ambientEventConfig = GetDefaultAmbientEventConfiguration();
        _logger.LogDebug("Loaded ambient event configuration with {EventPools} event pools",
            _ambientEventConfig.EventPools.Count);
        return _ambientEventConfig;
    }

    private static SensoryConfiguration GetDefaultSensoryConfiguration()
    {
        return new SensoryConfiguration
        {
            Version = "1.0",
            LightSources = new Dictionary<string, LightSourceDefinition>
            {
                ["torch"] = new LightSourceDefinition
                {
                    Id = "torch",
                    Name = "Torch",
                    LightQuality = "warm orange",
                    DescriptorPool = "lighting.lighting_torch",
                    Atmosphere = "flickering",
                    CommonBiomes = ["dungeon", "cave", "ruins"],
                    IsFlickering = true
                },
                ["crystal"] = new LightSourceDefinition
                {
                    Id = "crystal",
                    Name = "Crystal",
                    LightQuality = "cold blue",
                    DescriptorPool = "lighting.lighting_crystal",
                    Atmosphere = "ethereal",
                    CommonBiomes = ["cave", "frozen"],
                    IsFlickering = false
                },
                ["bioluminescence"] = new LightSourceDefinition
                {
                    Id = "bioluminescence",
                    Name = "Bioluminescence",
                    LightQuality = "eerie green",
                    DescriptorPool = "lighting.lighting_bioluminescence",
                    Atmosphere = "organic",
                    CommonBiomes = ["cave", "swamp"],
                    IsFlickering = false
                },
                ["magical"] = new LightSourceDefinition
                {
                    Id = "magical",
                    Name = "Magical",
                    LightQuality = "arcane",
                    DescriptorPool = "lighting.lighting_magical",
                    Atmosphere = "mystical",
                    CommonBiomes = ["ruins", "dungeon"],
                    IsFlickering = false
                },
                ["sunlight"] = new LightSourceDefinition
                {
                    Id = "sunlight",
                    Name = "Sunlight",
                    LightQuality = "warm golden",
                    DescriptorPool = "lighting.lighting_sunlight",
                    Atmosphere = "natural",
                    CommonBiomes = ["forest"],
                    IsFlickering = false
                },
                ["moonlight"] = new LightSourceDefinition
                {
                    Id = "moonlight",
                    Name = "Moonlight",
                    LightQuality = "pale silver",
                    DescriptorPool = "lighting.lighting_moonlight",
                    Atmosphere = "nocturnal",
                    CommonBiomes = ["forest"],
                    IsFlickering = false
                },
                ["lava_glow"] = new LightSourceDefinition
                {
                    Id = "lava_glow",
                    Name = "Lava Glow",
                    LightQuality = "hellish orange",
                    DescriptorPool = "lighting.lighting_lava_glow",
                    Atmosphere = "infernal",
                    CommonBiomes = ["volcanic"],
                    IsFlickering = true
                }
            },
            DarknessLevels = new Dictionary<string, DarknessLevelDefinition>
            {
                ["pitch_black"] = new DarknessLevelDefinition
                {
                    Id = "pitch_black",
                    Name = "Pitch Black",
                    VisibilityLevel = 0,
                    DescriptorPool = "lighting.lighting_pitch_black",
                    ImpliedTags = ["dark", "blind"]
                },
                ["dim"] = new DarknessLevelDefinition
                {
                    Id = "dim",
                    Name = "Dim",
                    VisibilityLevel = 30,
                    DescriptorPool = "lighting.lighting_dim",
                    ImpliedTags = ["dim"]
                },
                ["bright"] = new DarknessLevelDefinition
                {
                    Id = "bright",
                    Name = "Bright",
                    VisibilityLevel = 100,
                    DescriptorPool = "lighting.lighting_bright",
                    ImpliedTags = ["bright"]
                }
            },
            WeatherConditions = new Dictionary<string, WeatherDefinition>
            {
                ["rain"] = new WeatherDefinition
                {
                    Id = "rain",
                    Name = "Rain",
                    IndoorPool = "weather.weather_rain_indoor",
                    OutdoorPool = "weather.weather_rain_outdoor",
                    ValidClimates = ["temperate", "tropical"],
                    AffectsVisibility = true,
                    AffectsSound = true
                },
                ["storm"] = new WeatherDefinition
                {
                    Id = "storm",
                    Name = "Storm",
                    IndoorPool = "weather.weather_storm_indoor",
                    OutdoorPool = "weather.weather_storm_outdoor",
                    ValidClimates = ["temperate", "tropical"],
                    AffectsVisibility = true,
                    AffectsSound = true
                },
                ["snow"] = new WeatherDefinition
                {
                    Id = "snow",
                    Name = "Snow",
                    IndoorPool = "weather.weather_snow_indoor",
                    OutdoorPool = "weather.weather_snow_outdoor",
                    ValidClimates = ["cold", "frozen"],
                    AffectsVisibility = true,
                    AffectsSound = true
                },
                ["fog"] = new WeatherDefinition
                {
                    Id = "fog",
                    Name = "Fog",
                    IndoorPool = "weather.weather_fog_indoor",
                    OutdoorPool = "weather.weather_fog_outdoor",
                    ValidClimates = ["temperate", "swamp"],
                    AffectsVisibility = true,
                    AffectsSound = false
                },
                ["wind"] = new WeatherDefinition
                {
                    Id = "wind",
                    Name = "Wind",
                    IndoorPool = "weather.weather_wind_indoor",
                    OutdoorPool = "weather.weather_wind_outdoor",
                    ValidClimates = [],
                    AffectsVisibility = false,
                    AffectsSound = true
                }
            },
            TimesOfDay = new Dictionary<string, TimeOfDayDefinition>
            {
                ["dawn"] = new TimeOfDayDefinition
                {
                    Id = "dawn",
                    Name = "Dawn",
                    OutdoorPool = "environmental.time_dawn",
                    LightQuality = "pale pink",
                    DefaultDarknessLevel = "dim",
                    ImpliedTags = ["dawn", "morning"]
                },
                ["day"] = new TimeOfDayDefinition
                {
                    Id = "day",
                    Name = "Day",
                    OutdoorPool = "environmental.time_day",
                    LightQuality = "bright",
                    DefaultDarknessLevel = "bright",
                    ImpliedTags = ["day", "daytime"]
                },
                ["dusk"] = new TimeOfDayDefinition
                {
                    Id = "dusk",
                    Name = "Dusk",
                    OutdoorPool = "environmental.time_dusk",
                    LightQuality = "orange",
                    DefaultDarknessLevel = "dim",
                    ImpliedTags = ["dusk", "evening"]
                },
                ["night"] = new TimeOfDayDefinition
                {
                    Id = "night",
                    Name = "Night",
                    OutdoorPool = "environmental.time_night",
                    LightQuality = "dark",
                    DefaultDarknessLevel = "pitch_black",
                    ImpliedTags = ["night", "nighttime"]
                }
            }
        };
    }

    private static EnvironmentCategoryConfiguration ToEnvironmentCategoryConfiguration(EnvironmentCategoryJsonConfig config)
    {
        var categories = config.Categories?.ToDictionary(
            kvp => kvp.Key,
            kvp => new EnvironmentCategory
            {
                Id = kvp.Value.Id ?? kvp.Key,
                Name = kvp.Value.Name ?? kvp.Key,
                Description = kvp.Value.Description ?? string.Empty,
                IsRequired = kvp.Value.IsRequired,
                DefaultValue = kvp.Value.DefaultValue,
                Values = kvp.Value.Values?.Select(v => new CategoryValue
                {
                    Id = v.Id ?? string.Empty,
                    Name = v.Name ?? string.Empty,
                    Description = v.Description ?? string.Empty,
                    ImpliedTags = v.ImpliedTags ?? []
                }).ToList() ?? []
            }) ?? new Dictionary<string, EnvironmentCategory>();

        var exclusionRules = config.ExclusionRules?.Select(r => new CategoryExclusionRule
        {
            Id = r.Id ?? string.Empty,
            Reason = r.Reason ?? string.Empty,
            Category1 = r.Category1 ?? string.Empty,
            Values1 = r.Values1 ?? [],
            Category2 = r.Category2 ?? string.Empty,
            Values2 = r.Values2 ?? [],
            IsHardRule = r.IsHardRule
        }).ToList() ?? [];

        return new EnvironmentCategoryConfiguration
        {
            Version = config.Version ?? "1.0",
            Categories = categories,
            ExclusionRules = exclusionRules
        };
    }

    private static BiomeConfiguration ToBiomeConfiguration(BiomeJsonConfig config)
    {
        var biomes = config.Biomes?.ToDictionary(
            kvp => kvp.Key,
            kvp => new BiomeConfigurationDto
            {
                Id = kvp.Value.Id ?? kvp.Key,
                Name = kvp.Value.Name ?? kvp.Key,
                Description = kvp.Value.Description ?? string.Empty,
                DefaultCategoryValues = kvp.Value.DefaultCategoryValues ?? new Dictionary<string, string>(),
                ImpliedTags = kvp.Value.ImpliedTags ?? [],
                DescriptorPoolOverrides = kvp.Value.DescriptorPoolOverrides ?? new Dictionary<string, string>(),
                EmphasizedTerms = kvp.Value.EmphasizedTerms ?? [],
                ExcludedTerms = kvp.Value.ExcludedTerms ?? []
            }) ?? new Dictionary<string, BiomeConfigurationDto>();

        return new BiomeConfiguration
        {
            Version = config.Version ?? "1.0",
            Biomes = biomes
        };
    }

    private static EnvironmentCategoryConfiguration GetDefaultEnvironmentCategories()
    {
        return new EnvironmentCategoryConfiguration
        {
            Version = "1.0",
            Categories = new Dictionary<string, EnvironmentCategory>
            {
                ["biome"] = new EnvironmentCategory
                {
                    Id = "biome",
                    Name = "Biome",
                    Description = "The fundamental environment type",
                    IsRequired = true,
                    DefaultValue = "dungeon",
                    Values =
                    [
                        new CategoryValue { Id = "dungeon", Name = "Dungeon", ImpliedTags = ["underground", "dungeon"] }
                    ]
                },
                ["lighting"] = new EnvironmentCategory
                {
                    Id = "lighting",
                    Name = "Lighting",
                    Description = "Illumination level",
                    IsRequired = true,
                    DefaultValue = "dim",
                    Values =
                    [
                        new CategoryValue { Id = "dim", Name = "Dim", ImpliedTags = ["dim"] }
                    ]
                }
            },
            ExclusionRules = []
        };
    }

    private static BiomeConfiguration GetDefaultBiomeConfiguration()
    {
        return new BiomeConfiguration
        {
            Version = "1.0",
            Biomes = new Dictionary<string, BiomeConfigurationDto>
            {
                ["dungeon"] = new BiomeConfigurationDto
                {
                    Id = "dungeon",
                    Name = "Dungeon",
                    Description = "Constructed underground passages",
                    DefaultCategoryValues = new Dictionary<string, string>
                    {
                        ["climate"] = "cold",
                        ["lighting"] = "dim"
                    },
                    ImpliedTags = ["underground", "dungeon"]
                }
            }
        };
    }

    private static LootTable? ToLootTable(LootTableJsonConfig? config)
    {
        if (config == null)
            return null;

        var entries = config.Entries?
            .Select(e => LootEntry.Create(
                e.ItemId,
                e.Weight,
                e.MinQuantity,
                e.MaxQuantity,
                e.DropChance))
            .ToList() ?? [];

        var currencyDrops = config.CurrencyDrops?
            .Select(c => CurrencyDrop.Create(
                c.CurrencyId,
                c.MinAmount,
                c.MaxAmount,
                c.DropChance))
            .ToList() ?? [];

        return LootTable.Create(entries, currencyDrops);
    }

    private TierDefinition? ToTierDefinition(TierJsonConfig config)
    {
        try
        {
            return TierDefinition.Create(
                id: config.Id,
                name: config.Name,
                namePrefix: config.NamePrefix,
                healthMultiplier: config.HealthMultiplier,
                attackMultiplier: config.AttackMultiplier,
                defenseMultiplier: config.DefenseMultiplier,
                experienceMultiplier: config.ExperienceMultiplier,
                lootMultiplier: config.LootMultiplier,
                color: config.Color ?? "white",
                spawnWeight: config.SpawnWeight,
                generatesUniqueName: config.GeneratesUniqueName,
                sortOrder: config.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create tier definition for {Id}", config.Id);
            return null;
        }
    }

    private MonsterTrait? ToMonsterTrait(TraitJsonConfig config)
    {
        try
        {
            var effect = Enum.TryParse<TraitEffect>(config.Effect, true, out var e)
                ? e : TraitEffect.None;

            return MonsterTrait.Create(
                id: config.Id,
                name: config.Name,
                description: config.Description ?? string.Empty,
                effect: effect,
                effectValue: config.EffectValue,
                triggerThreshold: config.TriggerThreshold,
                tags: config.Tags ?? [],
                color: config.Color ?? "white",
                sortOrder: config.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create monster trait for {Id}", config.Id);
            return null;
        }
    }

    private static List<TierDefinition> GetDefaultTiers() =>
    [
        TierDefinition.Create("common", "Common", null, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, "white", 70, false, 0),
        TierDefinition.Create("named", "Named", null, 1.5f, 1.3f, 1.2f, 2.0f, 2.0f, "yellow", 20, true, 1),
        TierDefinition.Create("elite", "Elite", "Elite", 2.0f, 1.5f, 1.5f, 3.0f, 3.0f, "orange3", 8, false, 2),
        TierDefinition.Create("boss", "Boss", "Boss", 5.0f, 2.0f, 2.0f, 10.0f, 5.0f, "red", 2, false, 3)
    ];

    private static List<MonsterTrait> GetDefaultTraits() =>
    [
        MonsterTrait.Create("regenerating", "Regenerating", "Heals each turn.", TraitEffect.Regeneration, 5, null, ["healing"], "green", 0),
        MonsterTrait.Create("flying", "Flying", "Harder to hit with melee.", TraitEffect.Flying, 3, null, ["mobility"], "cyan", 1),
        MonsterTrait.Create("venomous", "Venomous", "Attacks may poison.", TraitEffect.Venomous, 3, null, ["poison"], "green", 2),
        MonsterTrait.Create("armored", "Armored", "Extra defense.", TraitEffect.Armored, 3, null, ["defensive"], "grey", 3),
        MonsterTrait.Create("berserker", "Berserker", "More damage when low HP.", TraitEffect.Berserker, 50, 30, ["offensive"], "red", 4)
    ];

    private DamageTypeDefinition? ToDamageTypeDefinition(DamageTypeJsonConfig config)
    {
        try
        {
            return DamageTypeDefinition.Create(
                id: config.Id,
                name: config.Name,
                description: config.Description ?? string.Empty,
                color: config.Color ?? "white",
                icon: config.Icon,
                sortOrder: config.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create damage type definition for {Id}", config.Id);
            return null;
        }
    }

    private static List<DamageTypeDefinition> GetDefaultDamageTypes() =>
    [
        DamageTypeDefinition.Create("physical", "Physical", "Standard physical damage from weapons and natural attacks.", "white", null, 0),
        DamageTypeDefinition.Create("fire", "Fire", "Burning damage from flames and heat.", "red", null, 1),
        DamageTypeDefinition.Create("ice", "Ice", "Freezing damage from cold and frost.", "cyan", null, 2),
        DamageTypeDefinition.Create("lightning", "Lightning", "Electrical damage from storms and magic.", "yellow", null, 3),
        DamageTypeDefinition.Create("poison", "Poison", "Toxic damage from venoms and chemicals.", "green", null, 4),
        DamageTypeDefinition.Create("holy", "Holy", "Radiant damage from divine sources.", "gold1", null, 5),
        DamageTypeDefinition.Create("dark", "Dark", "Necrotic damage from shadow and death magic.", "purple", null, 6)
    ];

    private MonsterDefinition? ToMonsterDefinition(MonsterDefinitionJsonConfig config)
    {
        try
        {
            var behavior = Enum.TryParse<AIBehavior>(config.Behavior, true, out var b)
                ? b : AIBehavior.Aggressive;

            // Parse resistances from dictionary
            DamageResistances? resistances = null;
            if (config.BaseResistances != null && config.BaseResistances.Count > 0)
            {
                resistances = new DamageResistances(config.BaseResistances);
            }

            // Parse name generator config (v0.0.9c)
            NameGeneratorConfig? nameGenerator = null;
            if (config.NameGenerator != null)
            {
                nameGenerator = new NameGeneratorConfig
                {
                    Prefixes = config.NameGenerator.Prefixes ?? [],
                    Suffixes = config.NameGenerator.Suffixes ?? [],
                    TitleFormat = config.NameGenerator.TitleFormat ?? "{0} the {1}"
                };
            }

            // Parse loot table (v0.0.9d)
            LootTable? lootTable = null;
            if (config.LootTable != null)
            {
                lootTable = ToLootTable(config.LootTable);
            }

            return MonsterDefinition.Create(
                id: config.Id,
                name: config.Name,
                description: config.Description,
                baseHealth: config.BaseHealth,
                baseAttack: config.BaseAttack,
                baseDefense: config.BaseDefense,
                experienceValue: config.ExperienceValue,
                behavior: behavior,
                tags: config.Tags ?? [],
                canHeal: config.CanHeal,
                healAmount: config.HealAmount,
                spawnWeight: config.SpawnWeight,
                initiativeModifier: config.InitiativeModifier,
                baseResistances: resistances,
                possibleTiers: config.PossibleTiers ?? ["common"],
                possibleTraits: config.PossibleTraits ?? [],
                nameGenerator: nameGenerator,
                lootTable: lootTable);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create monster definition for {Id}", config.Id);
            return null;
        }
    }

    private static List<MonsterDefinition> GetDefaultMonsters() =>
    [
        MonsterDefinition.Create("goblin", "Goblin", "A small, green creature with sharp teeth and beady eyes. It looks hostile.", 30, 8, 2, 25, AIBehavior.Cowardly, ["humanoid"], spawnWeight: 100, initiativeModifier: 1),
        MonsterDefinition.Create("skeleton", "Skeleton", "An animated pile of bones held together by dark magic.", 25, 6, 3, 20, AIBehavior.Aggressive, ["undead"], spawnWeight: 80),
        MonsterDefinition.Create("orc", "Orc", "A large, brutish creature with green skin and tusks. It wields a crude axe.", 45, 12, 4, 40, AIBehavior.Aggressive, ["humanoid"], spawnWeight: 50, initiativeModifier: -1),
        MonsterDefinition.Create("goblin_shaman", "Goblin Shaman", "A goblin adorned with crude fetishes and glowing runes.", 25, 6, 1, 30, AIBehavior.Support, ["humanoid", "magic"], true, 10, 30, 2),
        MonsterDefinition.Create("slime", "Slime", "A gelatinous blob that oozes across the floor.", 40, 5, 5, 15, AIBehavior.Chaotic, ["ooze"], spawnWeight: 90, initiativeModifier: -2)
    ];

    private static ProgressionDefinition ToProgressionDefinition(ProgressionJsonConfig config)
    {
        var curveType = Enum.TryParse<ProgressionCurve>(config.CurveType, true, out var ct)
            ? ct : ProgressionCurve.Exponential;

        var levelOverrides = config.Levels?
            .Where(l => l.Level > 0)
            .ToDictionary(
                l => l.Level,
                l => new LevelDefinition
                {
                    Level = l.Level,
                    XpRequired = l.XpRequired,
                    StatBonuses = l.StatBonuses,
                    CustomRewards = l.CustomRewards ?? [],
                    Title = l.Title
                })
            ?? new Dictionary<int, LevelDefinition>();

        return new ProgressionDefinition
        {
            ExperienceTerminology = config.ExperienceTerminology ?? "XP",
            LevelTerminology = config.LevelTerminology ?? "Level",
            MaxLevel = config.MaxLevel ?? 20,
            CurveType = curveType,
            BaseXpRequirement = config.BaseXpRequirement ?? 100,
            XpMultiplier = config.XpMultiplier ?? 1.5f,
            DefaultStatBonuses = config.DefaultStatBonuses ?? new StatBonusConfig(),
            LevelOverrides = levelOverrides,
            HealOnLevelUp = config.HealOnLevelUp ?? true
        };
    }

    private SkillDefinition? ToSkillDefinition(SkillJsonEntry entry)
    {
        try
        {
            return SkillDefinition.Create(
                id: entry.Id,
                name: entry.Name,
                description: entry.Description,
                primaryAttribute: entry.PrimaryAttribute,
                secondaryAttribute: entry.SecondaryAttribute,
                baseDicePool: entry.BaseDicePool,
                allowUntrained: entry.AllowUntrained,
                untrainedPenalty: entry.UntrainedPenalty,
                category: entry.Category,
                tags: entry.Tags,
                sortOrder: entry.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create skill definition for {Id}", entry.Id);
            return null;
        }
    }

    private DifficultyClassDefinition? ToDifficultyClassDefinition(DifficultyClassJsonEntry entry)
    {
        try
        {
            return DifficultyClassDefinition.Create(
                id: entry.Id,
                name: entry.Name,
                description: entry.Description,
                targetNumber: entry.TargetNumber,
                color: entry.Color,
                sortOrder: entry.SortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create difficulty class for {Id}", entry.Id);
            return null;
        }
    }

    private static List<SkillDefinition> GetDefaultSkills() =>
    [
        SkillDefinition.Create("athletics", "Athletics", "Physical feats of strength.", "might", "fortitude", "1d10", true, 0, "Physical", ["physical", "strength"], 1),
        SkillDefinition.Create("perception", "Perception", "Noticing hidden things.", "wits", null, "1d10", true, 0, "Mental", ["mental", "awareness"], 2),
        SkillDefinition.Create("stealth", "Stealth", "Moving silently.", "finesse", "wits", "1d10", true, 0, "Physical", ["physical", "sneaking"], 3),
    ];

    private static List<DifficultyClassDefinition> GetDefaultDifficultyClasses() =>
    [
        DifficultyClassDefinition.Create("trivial", "Trivial", "Almost anyone can do this.", 5, "#88FF88", 1),
        DifficultyClassDefinition.Create("easy", "Easy", "A simple task.", 8, "#44DD44", 2),
        DifficultyClassDefinition.Create("moderate", "Moderate", "Requires some skill.", 12, "#FFFF44", 3),
        DifficultyClassDefinition.Create("hard", "Hard", "Difficult for most.", 18, "#FF6644", 4),
    ];

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

    // Skill JSON structure
    private class SkillsJsonConfig
    {
        public List<SkillJsonEntry> Skills { get; set; } = [];
    }

    private class SkillJsonEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryAttribute { get; set; } = string.Empty;
        public string? SecondaryAttribute { get; set; }
        public string BaseDicePool { get; set; } = "1d10";
        public bool AllowUntrained { get; set; } = true;
        public int UntrainedPenalty { get; set; } = 0;
        public string Category { get; set; } = "General";
        public List<string> Tags { get; set; } = [];
        public int SortOrder { get; set; } = 0;
    }

    // Difficulty JSON structure
    private class DifficultyJsonConfig
    {
        public List<DifficultyClassJsonEntry> DifficultyClasses { get; set; } = [];
    }

    private class DifficultyClassJsonEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetNumber { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public int SortOrder { get; set; } = 0;
    }

    // Dice descriptors JSON structure
    private class DiceDescriptorsJsonConfig
    {
        public Dictionary<string, List<string>> Descriptors { get; set; } = new();
    }

    // Progression JSON structure
    private class ProgressionJsonConfig
    {
        public string? ExperienceTerminology { get; set; }
        public string? LevelTerminology { get; set; }
        public int? MaxLevel { get; set; }
        public string? CurveType { get; set; }
        public int? BaseXpRequirement { get; set; }
        public float? XpMultiplier { get; set; }
        public bool? HealOnLevelUp { get; set; }
        public StatBonusConfig? DefaultStatBonuses { get; set; }
        public List<LevelDefinitionJsonConfig>? Levels { get; set; }
    }

    private class LevelDefinitionJsonConfig
    {
        public int Level { get; set; }
        public int? XpRequired { get; set; }
        public StatBonusConfig? StatBonuses { get; set; }
        public List<string>? CustomRewards { get; set; }
        public string? Title { get; set; }
    }

    // Monster JSON structure
    private class MonstersJsonConfig
    {
        public List<MonsterDefinitionJsonConfig> Monsters { get; set; } = [];
    }

    private class MonsterDefinitionJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BaseHealth { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int ExperienceValue { get; set; }
        public string Behavior { get; set; } = "Aggressive";
        public List<string>? Tags { get; set; }
        public bool CanHeal { get; set; }
        public int? HealAmount { get; set; }
        public int SpawnWeight { get; set; } = 100;
        public int InitiativeModifier { get; set; }
        public Dictionary<string, int>? BaseResistances { get; set; }
        // v0.0.9c additions
        public List<string>? PossibleTiers { get; set; }
        public List<string>? PossibleTraits { get; set; }
        public NameGeneratorJsonConfig? NameGenerator { get; set; }
        // v0.0.9d additions
        public LootTableJsonConfig? LootTable { get; set; }
    }

    private class NameGeneratorJsonConfig
    {
        public List<string>? Prefixes { get; set; }
        public List<string>? Suffixes { get; set; }
        public string? TitleFormat { get; set; }
    }

    // Damage Type JSON structure
    private class DamageTypesJsonConfig
    {
        public List<DamageTypeJsonConfig> DamageTypes { get; set; } = [];
    }

    private class DamageTypeJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    // Tier JSON structure (v0.0.9c)
    private class TiersJsonConfig
    {
        public List<TierJsonConfig> Tiers { get; set; } = [];
    }

    private class TierJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NamePrefix { get; set; }
        public float HealthMultiplier { get; set; } = 1.0f;
        public float AttackMultiplier { get; set; } = 1.0f;
        public float DefenseMultiplier { get; set; } = 1.0f;
        public float ExperienceMultiplier { get; set; } = 1.0f;
        public float LootMultiplier { get; set; } = 1.0f;
        public string? Color { get; set; }
        public int SpawnWeight { get; set; } = 100;
        public bool GeneratesUniqueName { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    // Trait JSON structure (v0.0.9c)
    private class TraitsJsonConfig
    {
        public List<TraitJsonConfig> Traits { get; set; } = [];
    }

    private class TraitJsonConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Effect { get; set; } = "None";
        public int EffectValue { get; set; } = 0;
        public int? TriggerThreshold { get; set; }
        public List<string>? Tags { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    // Currency JSON structure (v0.0.9d)
    private class CurrencyJsonConfig
    {
        public List<CurrencyConfigJsonEntry> Currencies { get; set; } = [];
    }

    private class CurrencyConfigJsonEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PluralName { get; set; }
        public string? Symbol { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    // Loot Table JSON structure (v0.0.9d)
    private class LootTableJsonConfig
    {
        public List<LootEntryJsonConfig>? Entries { get; set; }
        public List<CurrencyDropJsonConfig>? CurrencyDrops { get; set; }
    }

    private class LootEntryJsonConfig
    {
        public string ItemId { get; set; } = string.Empty;
        public int Weight { get; set; } = 100;
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
        public float DropChance { get; set; } = 1.0f;
    }

    private class CurrencyDropJsonConfig
    {
        public string CurrencyId { get; set; } = "gold";
        public int MinAmount { get; set; } = 0;
        public int MaxAmount { get; set; } = 0;
        public float DropChance { get; set; } = 1.0f;
    }

    // Environment Category JSON structure (v0.0.11a)
    private class EnvironmentCategoryJsonConfig
    {
        public string? Version { get; set; }
        public Dictionary<string, EnvironmentCategoryEntryJson>? Categories { get; set; }
        public List<ExclusionRuleJson>? ExclusionRules { get; set; }
    }

    private class EnvironmentCategoryEntryJson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public List<CategoryValueJson>? Values { get; set; }
    }

    private class CategoryValueJson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? ImpliedTags { get; set; }
    }

    private class ExclusionRuleJson
    {
        public string? Id { get; set; }
        public string? Reason { get; set; }
        public string? Category1 { get; set; }
        public List<string>? Values1 { get; set; }
        public string? Category2 { get; set; }
        public List<string>? Values2 { get; set; }
        public bool IsHardRule { get; set; } = true;
    }

    // Biome JSON structure (v0.0.11a)
    private class BiomeJsonConfig
    {
        public string? Version { get; set; }
        public Dictionary<string, BiomeDefinitionJson>? Biomes { get; set; }
    }

    private class BiomeDefinitionJson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, string>? DefaultCategoryValues { get; set; }
        public List<string>? ImpliedTags { get; set; }
        public Dictionary<string, string>? DescriptorPoolOverrides { get; set; }
        public List<string>? EmphasizedTerms { get; set; }
        public List<string>? ExcludedTerms { get; set; }
    }

    // ===== Default Configuration Methods (v0.0.11d) =====

    private static ObjectDescriptorConfiguration GetDefaultObjectDescriptorConfiguration()
    {
        return new ObjectDescriptorConfiguration
        {
            ObjectTypes = new Dictionary<string, ObjectTypeDefinition>
            {
                ["door"] = new ObjectTypeDefinition
                {
                    Id = "door",
                    Name = "Door",
                    ValidStates = ["normal", "closed", "open", "locked", "barred", "broken", "destroyed"],
                    DefaultState = "closed",
                    IsInteractable = true
                },
                ["chest"] = new ObjectTypeDefinition
                {
                    Id = "chest",
                    Name = "Chest",
                    ValidStates = ["normal", "closed", "open", "locked", "empty", "trapped", "broken"],
                    DefaultState = "closed",
                    IsInteractable = true
                },
                ["lever"] = new ObjectTypeDefinition
                {
                    Id = "lever",
                    Name = "Lever",
                    ValidStates = ["normal", "up", "down", "stuck", "broken"],
                    DefaultState = "up",
                    IsInteractable = true
                },
                ["statue"] = new ObjectTypeDefinition
                {
                    Id = "statue",
                    Name = "Statue",
                    ValidStates = ["normal", "damaged", "destroyed", "active", "inactive", "defaced"],
                    DefaultState = "normal",
                    IsInteractable = false
                },
                ["altar"] = new ObjectTypeDefinition
                {
                    Id = "altar",
                    Name = "Altar",
                    ValidStates = ["normal", "active", "inactive", "desecrated", "blessed", "destroyed"],
                    DefaultState = "inactive",
                    IsInteractable = true
                },
                ["lightsource"] = new ObjectTypeDefinition
                {
                    Id = "lightsource",
                    Name = "Light Source",
                    ValidStates = ["normal", "lit", "unlit", "flickering", "broken"],
                    DefaultState = "lit",
                    IsInteractable = true
                },
                ["inscription"] = new ObjectTypeDefinition
                {
                    Id = "inscription",
                    Name = "Inscription",
                    ValidStates = ["normal", "damaged", "destroyed"],
                    DefaultState = "normal",
                    IsInteractable = false
                },
                ["waterfeature"] = new ObjectTypeDefinition
                {
                    Id = "waterfeature",
                    Name = "Water Feature",
                    ValidStates = ["normal", "active", "inactive", "broken"],
                    DefaultState = "active",
                    IsInteractable = false
                }
            }
        };
    }

    private static AmbientEventConfiguration GetDefaultAmbientEventConfiguration()
    {
        return new AmbientEventConfiguration
        {
            BaseProbability = 0.15f,
            CooldownSeconds = 30,
            EventPools = new Dictionary<string, AmbientEventPool>
            {
                ["sound"] = new AmbientEventPool
                {
                    Id = "sound",
                    Name = "Sound Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic", "room_entry"]
                },
                ["sound_cave"] = new AmbientEventPool
                {
                    Id = "sound_cave",
                    Name = "Cave Sound Events",
                    ValidBiomes = ["cave"],
                    ValidTriggers = ["exploration", "periodic", "room_entry"]
                },
                ["sound_dungeon"] = new AmbientEventPool
                {
                    Id = "sound_dungeon",
                    Name = "Dungeon Sound Events",
                    ValidBiomes = ["dungeon"],
                    ValidTriggers = ["exploration", "periodic", "room_entry"]
                },
                ["visual"] = new AmbientEventPool
                {
                    Id = "visual",
                    Name = "Visual Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic"]
                },
                ["visual_volcanic"] = new AmbientEventPool
                {
                    Id = "visual_volcanic",
                    Name = "Volcanic Visual Events",
                    ValidBiomes = ["volcanic"],
                    ValidTriggers = ["exploration", "periodic"]
                },
                ["creature"] = new AmbientEventPool
                {
                    Id = "creature",
                    Name = "Creature Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "room_entry"]
                },
                ["creature_swamp"] = new AmbientEventPool
                {
                    Id = "creature_swamp",
                    Name = "Swamp Creature Events",
                    ValidBiomes = ["swamp"],
                    ValidTriggers = ["exploration", "room_entry"]
                },
                ["environmental"] = new AmbientEventPool
                {
                    Id = "environmental",
                    Name = "Environmental Events",
                    ValidBiomes = [],
                    ValidTriggers = ["exploration", "periodic", "player_action"]
                },
                ["environmental_frozen"] = new AmbientEventPool
                {
                    Id = "environmental_frozen",
                    Name = "Frozen Environmental Events",
                    ValidBiomes = ["frozen"],
                    ValidTriggers = ["exploration", "periodic", "player_action"]
                }
            }
        };
    }
}
