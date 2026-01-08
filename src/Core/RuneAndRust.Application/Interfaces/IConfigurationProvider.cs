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
