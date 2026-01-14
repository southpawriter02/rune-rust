using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.TestUtilities.Mocks;

/// <summary>
/// In-memory configuration provider for tests.
/// </summary>
/// <remarks>
/// Implements IGameConfigurationProvider with in-memory lists that can be
/// populated using fluent methods. Use for service tests that require
/// configuration data without loading from JSON files.
/// </remarks>
public class MockConfigurationProvider : IGameConfigurationProvider
{
    private readonly List<RaceDefinition> _races = [];
    private readonly List<BackgroundDefinition> _backgrounds = [];
    private readonly List<AttributeDefinition> _attributes = [];
    private readonly List<ArchetypeDefinition> _archetypes = [];
    private readonly List<ClassDefinition> _classes = [];
    private readonly List<ResourceTypeDefinition> _resourceTypes = [];
    private readonly List<AbilityDefinition> _abilities = [];
    private readonly List<SkillDefinition> _skills = [];
    private readonly List<DifficultyClassDefinition> _difficultyClasses = [];
    private readonly List<MonsterDefinition> _monsters = [];
    private readonly List<DamageTypeDefinition> _damageTypes = [];
    private readonly List<TierDefinition> _tiers = [];
    private readonly List<MonsterTrait> _traits = [];
    private readonly List<CurrencyDefinition> _currencies = [];
    private readonly Dictionary<string, DescriptorPool> _descriptorPools = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyList<string>> _diceDescriptors = new(StringComparer.OrdinalIgnoreCase);

    private PointBuyRules _pointBuyRules = new();
    private LexiconConfiguration _lexiconConfiguration = new();
    private ThemeConfiguration _themeConfiguration = new();
    private ProgressionDefinition _progressionDefinition = ProgressionDefinition.Default;

    // ===== Fluent Configuration Methods =====

    /// <summary>
    /// Adds a race definition.
    /// </summary>
    public MockConfigurationProvider WithRace(RaceDefinition race)
    {
        _races.Add(race);
        return this;
    }

    /// <summary>
    /// Adds a background definition.
    /// </summary>
    public MockConfigurationProvider WithBackground(BackgroundDefinition background)
    {
        _backgrounds.Add(background);
        return this;
    }

    /// <summary>
    /// Adds an archetype definition.
    /// </summary>
    public MockConfigurationProvider WithArchetype(ArchetypeDefinition archetype)
    {
        _archetypes.Add(archetype);
        return this;
    }

    /// <summary>
    /// Adds a class definition.
    /// </summary>
    public MockConfigurationProvider WithClass(ClassDefinition classDef)
    {
        _classes.Add(classDef);
        return this;
    }

    /// <summary>
    /// Adds an ability definition.
    /// </summary>
    public MockConfigurationProvider WithAbility(AbilityDefinition ability)
    {
        _abilities.Add(ability);
        return this;
    }

    /// <summary>
    /// Adds a resource type definition.
    /// </summary>
    public MockConfigurationProvider WithResourceType(ResourceTypeDefinition resourceType)
    {
        _resourceTypes.Add(resourceType);
        return this;
    }

    /// <summary>
    /// Adds a monster definition.
    /// </summary>
    public MockConfigurationProvider WithMonster(MonsterDefinition monster)
    {
        _monsters.Add(monster);
        return this;
    }

    /// <summary>
    /// Adds a damage type definition.
    /// </summary>
    public MockConfigurationProvider WithDamageType(DamageTypeDefinition damageType)
    {
        _damageTypes.Add(damageType);
        return this;
    }

    /// <summary>
    /// Adds a tier definition.
    /// </summary>
    public MockConfigurationProvider WithTier(TierDefinition tier)
    {
        _tiers.Add(tier);
        return this;
    }

    /// <summary>
    /// Adds a monster trait definition.
    /// </summary>
    public MockConfigurationProvider WithTrait(MonsterTrait trait)
    {
        _traits.Add(trait);
        return this;
    }

    /// <summary>
    /// Adds a currency definition.
    /// </summary>
    public MockConfigurationProvider WithCurrency(CurrencyDefinition currency)
    {
        _currencies.Add(currency);
        return this;
    }

    /// <summary>
    /// Adds a skill definition.
    /// </summary>
    public MockConfigurationProvider WithSkill(SkillDefinition skill)
    {
        _skills.Add(skill);
        return this;
    }

    /// <summary>
    /// Adds a difficulty class definition.
    /// </summary>
    public MockConfigurationProvider WithDifficultyClass(DifficultyClassDefinition dc)
    {
        _difficultyClasses.Add(dc);
        return this;
    }

    /// <summary>
    /// Sets the point-buy rules.
    /// </summary>
    public MockConfigurationProvider WithPointBuyRules(PointBuyRules rules)
    {
        _pointBuyRules = rules;
        return this;
    }

    /// <summary>
    /// Sets the progression configuration.
    /// </summary>
    public MockConfigurationProvider WithProgression(ProgressionDefinition progression)
    {
        _progressionDefinition = progression;
        return this;
    }

    /// <summary>
    /// Adds default resource types (mana, rage, faith, stamina).
    /// </summary>
    public MockConfigurationProvider WithDefaultResourceTypes()
    {
        _resourceTypes.AddRange([
            ResourceTypeDefinition.Create("mana", "Mana", "MP", "Magical energy", "#0066FF", 100),
            ResourceTypeDefinition.Create("rage", "Rage", "RG", "Battle fury", "#FF0000", 100, startsAtZero: true),
            ResourceTypeDefinition.Create("faith", "Faith", "FP", "Divine power", "#FFD700", 50),
            ResourceTypeDefinition.Create("stamina", "Stamina", "SP", "Physical energy", "#00FF00", 100)
        ]);
        return this;
    }

    /// <summary>
    /// Adds default tier definitions.
    /// </summary>
    public MockConfigurationProvider WithDefaultTiers()
    {
        _tiers.AddRange([
            TierDefinition.Create("common", "Common", namePrefix: null, healthMultiplier: 1.0f, attackMultiplier: 1.0f, defenseMultiplier: 1.0f, experienceMultiplier: 1.0f, lootMultiplier: 1.0f, color: "white"),
            TierDefinition.Create("elite", "Elite", namePrefix: "Elite", healthMultiplier: 1.5f, attackMultiplier: 1.5f, defenseMultiplier: 1.5f, experienceMultiplier: 2.0f, lootMultiplier: 1.5f, color: "yellow"),
            TierDefinition.Create("boss", "Boss", namePrefix: "Boss", healthMultiplier: 2.0f, attackMultiplier: 2.0f, defenseMultiplier: 2.0f, experienceMultiplier: 3.0f, lootMultiplier: 2.0f, color: "red")
        ]);
        return this;
    }

    // ===== IGameConfigurationProvider Implementation =====

    public IReadOnlyList<RaceDefinition> GetRaces() => _races;
    public RaceDefinition? GetRaceById(string raceId) =>
        _races.FirstOrDefault(r => r.Id.Equals(raceId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<BackgroundDefinition> GetBackgrounds() => _backgrounds;
    public BackgroundDefinition? GetBackgroundById(string backgroundId) =>
        _backgrounds.FirstOrDefault(b => b.Id.Equals(backgroundId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<AttributeDefinition> GetAttributes() => _attributes;

    public PointBuyRules GetPointBuyRules() => _pointBuyRules;

    public LexiconConfiguration GetLexiconConfiguration() => _lexiconConfiguration;

    public ThemeConfiguration GetThemeConfiguration() => _themeConfiguration;

    public IReadOnlyDictionary<string, DescriptorPool> GetAllDescriptorPools() => _descriptorPools;

    public IReadOnlyList<ArchetypeDefinition> GetArchetypes() => _archetypes;
    public ArchetypeDefinition? GetArchetypeById(string archetypeId) =>
        _archetypes.FirstOrDefault(a => a.Id.Equals(archetypeId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ClassDefinition> GetClasses() => _classes;
    public ClassDefinition? GetClassById(string classId) =>
        _classes.FirstOrDefault(c => c.Id.Equals(classId, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<ClassDefinition> GetClassesForArchetype(string archetypeId) =>
        _classes.Where(c => c.ArchetypeId.Equals(archetypeId, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<ResourceTypeDefinition> GetResourceTypes() => _resourceTypes;
    public ResourceTypeDefinition? GetResourceTypeById(string resourceTypeId) =>
        _resourceTypes.FirstOrDefault(r => r.Id.Equals(resourceTypeId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<AbilityDefinition> GetAbilities() => _abilities;
    public AbilityDefinition? GetAbilityById(string abilityId) =>
        _abilities.FirstOrDefault(a => a.Id.Equals(abilityId, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<AbilityDefinition> GetAbilitiesForClass(string classId) =>
        _abilities.Where(a => a.IsAvailableToClass(classId)).ToList();

    public IReadOnlyList<SkillDefinition> GetSkills() => _skills;
    public SkillDefinition? GetSkillById(string id) =>
        _skills.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<DifficultyClassDefinition> GetDifficultyClasses() => _difficultyClasses;
    public DifficultyClassDefinition? GetDifficultyClassById(string id) =>
        _difficultyClasses.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetDiceDescriptors() => _diceDescriptors;

    public ProgressionDefinition GetProgressionConfiguration() => _progressionDefinition;

    public IReadOnlyList<MonsterDefinition> GetMonsters() => _monsters;
    public MonsterDefinition? GetMonsterById(string monsterId) =>
        _monsters.FirstOrDefault(m => m.Id.Equals(monsterId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<DamageTypeDefinition> GetDamageTypes() => _damageTypes;
    public DamageTypeDefinition? GetDamageTypeById(string damageTypeId) =>
        _damageTypes.FirstOrDefault(d => d.Id.Equals(damageTypeId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<TierDefinition> GetTiers() => _tiers;
    public TierDefinition? GetTierById(string tierId) =>
        _tiers.FirstOrDefault(t => t.Id.Equals(tierId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<MonsterTrait> GetTraits() => _traits;
    public MonsterTrait? GetTraitById(string traitId) =>
        _traits.FirstOrDefault(t => t.Id.Equals(traitId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<CurrencyDefinition> GetCurrencies() => _currencies;
    public CurrencyDefinition? GetCurrencyById(string currencyId) =>
        _currencies.FirstOrDefault(c => c.Id.Equals(currencyId, StringComparison.OrdinalIgnoreCase));

    // ===== Environment Configuration (v0.0.11a) =====

    private EnvironmentCategoryConfiguration _environmentCategories = new();
    private BiomeConfiguration _biomeConfiguration = new();

    public EnvironmentCategoryConfiguration GetEnvironmentCategories() => _environmentCategories;
    public BiomeConfiguration GetBiomeConfiguration() => _biomeConfiguration;

    /// <summary>
    /// Sets the environment category configuration.
    /// </summary>
    public MockConfigurationProvider WithEnvironmentCategories(EnvironmentCategoryConfiguration config)
    {
        _environmentCategories = config;
        return this;
    }

    /// <summary>
    /// Sets the biome configuration.
    /// </summary>
    public MockConfigurationProvider WithBiomeConfiguration(BiomeConfiguration config)
    {
        _biomeConfiguration = config;
        return this;
    }

    // ===== Sensory Configuration (v0.0.11c) =====

    private SensoryConfiguration _sensoryConfiguration = new();

    public SensoryConfiguration GetSensoryConfiguration() => _sensoryConfiguration;

    /// <summary>
    /// Sets the sensory configuration.
    /// </summary>
    public MockConfigurationProvider WithSensoryConfiguration(SensoryConfiguration config)
    {
        _sensoryConfiguration = config;
        return this;
    }

    // ===== Interactive Object & Ambient Event Configuration (v0.0.11d) =====

    private ObjectDescriptorConfiguration _objectDescriptorConfiguration = new();
    private AmbientEventConfiguration _ambientEventConfiguration = new();

    public ObjectDescriptorConfiguration GetObjectDescriptorConfiguration() => _objectDescriptorConfiguration;
    public AmbientEventConfiguration GetAmbientEventConfiguration() => _ambientEventConfiguration;

    /// <summary>
    /// Sets the object descriptor configuration.
    /// </summary>
    public MockConfigurationProvider WithObjectDescriptorConfiguration(ObjectDescriptorConfiguration config)
    {
        _objectDescriptorConfiguration = config;
        return this;
    }

    /// <summary>
    /// Sets the ambient event configuration.
    /// </summary>
    public MockConfigurationProvider WithAmbientEventConfiguration(AmbientEventConfiguration config)
    {
        _ambientEventConfiguration = config;
        return this;
    }

    // ===== Combat Grid Configuration (v0.5.0a) =====

    private GridSettings _gridSettings = new();

    public GridSettings GetGridSettings() => _gridSettings;

    /// <summary>
    /// Sets the grid settings.
    /// </summary>
    public MockConfigurationProvider WithGridSettings(GridSettings settings)
    {
        _gridSettings = settings;
        return this;
    }

    // ===== Terrain Configuration (v0.5.2a) =====

    private readonly List<TerrainDefinition> _terrainDefinitions = [];

    public IReadOnlyList<TerrainDefinition> GetTerrainDefinitions() => _terrainDefinitions;

    public TerrainDefinition? GetTerrainDefinitionById(string terrainId) =>
        _terrainDefinitions.FirstOrDefault(t => t.Id.Equals(terrainId, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Adds a terrain definition.
    /// </summary>
    public MockConfigurationProvider WithTerrainDefinition(TerrainDefinition terrain)
    {
        _terrainDefinitions.Add(terrain);
        return this;
    }

    /// <summary>
    /// Adds default terrain definitions for testing.
    /// </summary>
    public MockConfigurationProvider WithDefaultTerrainDefinitions()
    {
        _terrainDefinitions.AddRange([
            TerrainDefinition.Create("normal-floor", "Stone Floor", Domain.Enums.TerrainType.Normal),
            TerrainDefinition.Create("rubble", "Rubble", Domain.Enums.TerrainType.Difficult, movementCostMultiplier: 2.0f),
            TerrainDefinition.Create("wall", "Wall", Domain.Enums.TerrainType.Impassable, blocksLOS: true, displayChar: '#'),
            TerrainDefinition.Create("fire", "Fire", Domain.Enums.TerrainType.Hazardous, damageOnEntry: "1d6", damageType: "fire", displayChar: '▲')
        ]);
        return this;
    }
}

