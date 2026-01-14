using RuneAndRust.Application.Configuration;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to game configuration data loaded from JSON files.
/// </summary>
public interface IGameConfigurationProvider
{
    /// <summary>
    /// Gets all playable race definitions.
    /// </summary>
    /// <returns>Read-only list of race definitions.</returns>
    IReadOnlyList<RaceDefinition> GetRaces();

    /// <summary>
    /// Gets a race definition by ID.
    /// </summary>
    /// <param name="raceId">The race identifier.</param>
    /// <returns>The race definition or null if not found.</returns>
    RaceDefinition? GetRaceById(string raceId);

    /// <summary>
    /// Gets all playable background definitions.
    /// </summary>
    /// <returns>Read-only list of background definitions.</returns>
    IReadOnlyList<BackgroundDefinition> GetBackgrounds();

    /// <summary>
    /// Gets a background definition by ID.
    /// </summary>
    /// <param name="backgroundId">The background identifier.</param>
    /// <returns>The background definition or null if not found.</returns>
    BackgroundDefinition? GetBackgroundById(string backgroundId);

    /// <summary>
    /// Gets all attribute definitions.
    /// </summary>
    /// <returns>Read-only list of attribute definitions.</returns>
    IReadOnlyList<AttributeDefinition> GetAttributes();

    /// <summary>
    /// Gets the point-buy configuration for character creation.
    /// </summary>
    /// <returns>Point-buy rules including starting points, min/max values.</returns>
    PointBuyRules GetPointBuyRules();

    /// <summary>
    /// Gets the lexicon configuration for synonym selection.
    /// </summary>
    /// <returns>Lexicon configuration with terms and synonyms.</returns>
    LexiconConfiguration GetLexiconConfiguration();

    /// <summary>
    /// Gets the theme configuration.
    /// </summary>
    /// <returns>Theme configuration with active theme and presets.</returns>
    ThemeConfiguration GetThemeConfiguration();

    /// <summary>
    /// Gets all descriptor pools keyed by pool path (e.g., "environmental.lighting").
    /// </summary>
    /// <returns>Dictionary of descriptor pools.</returns>
    IReadOnlyDictionary<string, DescriptorPool> GetAllDescriptorPools();

    /// <summary>
    /// Gets all archetype definitions.
    /// </summary>
    IReadOnlyList<ArchetypeDefinition> GetArchetypes();

    /// <summary>
    /// Gets an archetype definition by ID.
    /// </summary>
    ArchetypeDefinition? GetArchetypeById(string archetypeId);

    /// <summary>
    /// Gets all class definitions.
    /// </summary>
    IReadOnlyList<ClassDefinition> GetClasses();

    /// <summary>
    /// Gets a class definition by ID.
    /// </summary>
    ClassDefinition? GetClassById(string classId);

    /// <summary>
    /// Gets all classes for a specific archetype.
    /// </summary>
    IReadOnlyList<ClassDefinition> GetClassesForArchetype(string archetypeId);

    /// <summary>
    /// Gets all resource type definitions.
    /// </summary>
    IReadOnlyList<ResourceTypeDefinition> GetResourceTypes();

    /// <summary>
    /// Gets a resource type definition by ID.
    /// </summary>
    ResourceTypeDefinition? GetResourceTypeById(string resourceTypeId);

    /// <summary>
    /// Gets all ability definitions.
    /// </summary>
    IReadOnlyList<AbilityDefinition> GetAbilities();

    /// <summary>
    /// Gets an ability definition by ID.
    /// </summary>
    /// <param name="abilityId">The ability identifier.</param>
    /// <returns>The ability definition or null if not found.</returns>
    AbilityDefinition? GetAbilityById(string abilityId);

    /// <summary>
    /// Gets all abilities available to a specific class.
    /// </summary>
    /// <param name="classId">The class identifier.</param>
    /// <returns>List of ability definitions available to the class.</returns>
    IReadOnlyList<AbilityDefinition> GetAbilitiesForClass(string classId);

    /// <summary>
    /// Gets all skill definitions.
    /// </summary>
    /// <returns>Read-only list of all skills.</returns>
    IReadOnlyList<SkillDefinition> GetSkills();

    /// <summary>
    /// Gets a skill definition by ID.
    /// </summary>
    /// <param name="id">The skill ID.</param>
    /// <returns>The skill definition, or null if not found.</returns>
    SkillDefinition? GetSkillById(string id);

    /// <summary>
    /// Gets all difficulty class definitions.
    /// </summary>
    /// <returns>Read-only list of all difficulty classes.</returns>
    IReadOnlyList<DifficultyClassDefinition> GetDifficultyClasses();

    /// <summary>
    /// Gets a difficulty class definition by ID.
    /// </summary>
    /// <param name="id">The difficulty class ID.</param>
    /// <returns>The difficulty class definition, or null if not found.</returns>
    DifficultyClassDefinition? GetDifficultyClassById(string id);

    /// <summary>
    /// Gets all dice descriptors organized by category.
    /// </summary>
    /// <returns>Dictionary mapping category names to lists of descriptors.</returns>
    IReadOnlyDictionary<string, IReadOnlyList<string>> GetDiceDescriptors();

    /// <summary>
    /// Gets the progression configuration.
    /// </summary>
    /// <returns>The progression definition, or default if not configured.</returns>
    ProgressionDefinition GetProgressionConfiguration();

    /// <summary>
    /// Gets all monster definitions.
    /// </summary>
    /// <returns>Read-only list of monster definitions.</returns>
    IReadOnlyList<MonsterDefinition> GetMonsters();

    /// <summary>
    /// Gets a monster definition by ID.
    /// </summary>
    /// <param name="monsterId">The monster identifier.</param>
    /// <returns>The monster definition or null if not found.</returns>
    MonsterDefinition? GetMonsterById(string monsterId);

    /// <summary>
    /// Gets all damage type definitions.
    /// </summary>
    /// <returns>Read-only list of damage type definitions.</returns>
    IReadOnlyList<DamageTypeDefinition> GetDamageTypes();

    /// <summary>
    /// Gets a damage type definition by ID.
    /// </summary>
    /// <param name="damageTypeId">The damage type identifier.</param>
    /// <returns>The damage type definition or null if not found.</returns>
    DamageTypeDefinition? GetDamageTypeById(string damageTypeId);

    // ===== Tier & Trait Definitions (v0.0.9c) =====

    /// <summary>
    /// Gets all tier definitions.
    /// </summary>
    /// <returns>Read-only list of tier definitions.</returns>
    IReadOnlyList<TierDefinition> GetTiers();

    /// <summary>
    /// Gets a tier definition by ID.
    /// </summary>
    /// <param name="tierId">The tier identifier.</param>
    /// <returns>The tier definition or null if not found.</returns>
    TierDefinition? GetTierById(string tierId);

    /// <summary>
    /// Gets all monster trait definitions.
    /// </summary>
    /// <returns>Read-only list of monster trait definitions.</returns>
    IReadOnlyList<MonsterTrait> GetTraits();

    /// <summary>
    /// Gets a monster trait definition by ID.
    /// </summary>
    /// <param name="traitId">The trait identifier.</param>
    /// <returns>The monster trait definition or null if not found.</returns>
    MonsterTrait? GetTraitById(string traitId);

    // ===== Currency Definitions (v0.0.9d) =====

    /// <summary>
    /// Gets all currency definitions.
    /// </summary>
    /// <returns>Read-only list of currency definitions.</returns>
    IReadOnlyList<CurrencyDefinition> GetCurrencies();

    /// <summary>
    /// Gets a currency definition by ID.
    /// </summary>
    /// <param name="currencyId">The currency identifier.</param>
    /// <returns>The currency definition or null if not found.</returns>
    CurrencyDefinition? GetCurrencyById(string currencyId);

    // ===== Environment Configuration (v0.0.11a) =====

    /// <summary>
    /// Gets the environment category configuration.
    /// </summary>
    /// <returns>Environment category configuration with categories and exclusion rules.</returns>
    EnvironmentCategoryConfiguration GetEnvironmentCategories();

    /// <summary>
    /// Gets the biome configuration.
    /// </summary>
    /// <returns>Biome configuration with biome definitions.</returns>
    BiomeConfiguration GetBiomeConfiguration();

    // ===== Sensory Configuration (v0.0.11c) =====

    /// <summary>
    /// Gets the sensory configuration for environmental descriptors.
    /// </summary>
    /// <returns>Sensory configuration with light sources, weather, and time of day definitions.</returns>
    SensoryConfiguration GetSensoryConfiguration();

    // ===== Interactive Object & Ambient Event Configuration (v0.0.11d) =====

    /// <summary>
    /// Gets the object descriptor configuration for interactive objects.
    /// </summary>
    /// <returns>Object descriptor configuration with object type definitions.</returns>
    ObjectDescriptorConfiguration GetObjectDescriptorConfiguration();

    /// <summary>
    /// Gets the ambient event configuration for atmospheric events.
    /// </summary>
    /// <returns>Ambient event configuration with event pools and settings.</returns>
    AmbientEventConfiguration GetAmbientEventConfiguration();

    // ===== Combat Grid Configuration (v0.5.0a) =====

    /// <summary>
    /// Gets the combat grid configuration settings.
    /// </summary>
    /// <returns>Grid settings with default dimensions and spawn zones.</returns>
    GridSettings GetGridSettings();

    // ===== Terrain Configuration (v0.5.2a) =====

    /// <summary>
    /// Gets all terrain definitions.
    /// </summary>
    /// <returns>Read-only list of terrain definitions.</returns>
    IReadOnlyList<TerrainDefinition> GetTerrainDefinitions();

    /// <summary>
    /// Gets a terrain definition by ID.
    /// </summary>
    /// <param name="terrainId">The terrain identifier.</param>
    /// <returns>The terrain definition or null if not found.</returns>
    TerrainDefinition? GetTerrainDefinitionById(string terrainId);
}

/// <summary>
/// Configuration for the point-buy attribute system.
/// </summary>
public class PointBuyRules
{
    /// <summary>
    /// Total points available for attribute allocation.
    /// </summary>
    public int StartingPoints { get; init; } = 25;

    /// <summary>
    /// Base value for all attributes before allocation.
    /// </summary>
    public int BaseAttributeValue { get; init; } = 8;

    /// <summary>
    /// Minimum value an attribute can be set to.
    /// </summary>
    public int MinimumAttribute { get; init; } = 6;

    /// <summary>
    /// Maximum value before racial modifiers.
    /// </summary>
    public int MaximumAttribute { get; init; } = 15;

    /// <summary>
    /// Maximum value after racial modifiers.
    /// </summary>
    public int MaximumAfterRacial { get; init; } = 18;
}

/// <summary>
/// Configuration for combat grid dimensions and spawn settings.
/// </summary>
/// <remarks>
/// Grid dimensions are constrained between MinWidth/MinHeight and MaxWidth/MaxHeight.
/// Player starts at the configured start position (default: center-south).
/// Monsters spawn in the configured zone (default: north).
/// </remarks>
public record GridSettings
{
    /// <summary>
    /// Default grid width when not overridden.
    /// </summary>
    public int DefaultWidth { get; init; } = 8;

    /// <summary>
    /// Default grid height when not overridden.
    /// </summary>
    public int DefaultHeight { get; init; } = 8;

    /// <summary>
    /// Minimum allowed grid width.
    /// </summary>
    public int MinWidth { get; init; } = 3;

    /// <summary>
    /// Maximum allowed grid width.
    /// </summary>
    public int MaxWidth { get; init; } = 20;

    /// <summary>
    /// Minimum allowed grid height.
    /// </summary>
    public int MinHeight { get; init; } = 3;

    /// <summary>
    /// Maximum allowed grid height.
    /// </summary>
    public int MaxHeight { get; init; } = 20;

    /// <summary>
    /// Player starting position descriptor (e.g., "center-south").
    /// </summary>
    public string PlayerStartPosition { get; init; } = "center-south";

    /// <summary>
    /// Monster spawning zone descriptor (e.g., "north").
    /// </summary>
    public string MonsterSpawnZone { get; init; } = "north";
}

