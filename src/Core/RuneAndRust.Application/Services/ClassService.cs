using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides operations for archetype and class management.
/// </summary>
public class ClassService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly AbilityService _abilityService;
    private readonly ILogger<ClassService> _logger;

    public ClassService(
        IGameConfigurationProvider configProvider,
        AbilityService abilityService,
        ILogger<ClassService> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation(
            "ClassService initialized with {ArchetypeCount} archetypes and {ClassCount} classes",
            _configProvider.GetArchetypes().Count,
            _configProvider.GetClasses().Count);
    }

    /// <summary>
    /// Gets all available archetypes.
    /// </summary>
    public IReadOnlyList<ArchetypeDto> GetAllArchetypes()
    {
        _logger.LogDebug("GetAllArchetypes called");
        var archetypes = _configProvider.GetArchetypes();
        var classes = _configProvider.GetClasses();

        return archetypes
            .OrderBy(a => a.SortOrder)
            .Select(a => ToDto(a, classes))
            .ToList();
    }

    /// <summary>
    /// Gets a specific archetype by ID.
    /// </summary>
    public ArchetypeDto? GetArchetype(string id)
    {
        _logger.LogDebug("GetArchetype called for ID: {ArchetypeId}", id);
        var archetype = _configProvider.GetArchetypeById(id);
        if (archetype == null) return null;

        var classes = _configProvider.GetClasses();
        return ToDto(archetype, classes);
    }

    /// <summary>
    /// Gets all classes for a specific archetype.
    /// </summary>
    public IReadOnlyList<ClassDto> GetClassesForArchetype(string archetypeId)
    {
        _logger.LogDebug("GetClassesForArchetype called for: {ArchetypeId}", archetypeId);
        return _configProvider.GetClasses()
            .Where(c => c.ArchetypeId.Equals(archetypeId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.SortOrder)
            .Select(ToDto)
            .ToList();
    }

    /// <summary>
    /// Gets all available classes.
    /// </summary>
    public IReadOnlyList<ClassDto> GetAllClasses()
    {
        _logger.LogDebug("GetAllClasses called");
        return _configProvider.GetClasses()
            .OrderBy(c => c.ArchetypeId)
            .ThenBy(c => c.SortOrder)
            .Select(ToDto)
            .ToList();
    }

    /// <summary>
    /// Gets a specific class by ID.
    /// </summary>
    public ClassDto? GetClass(string id)
    {
        _logger.LogDebug("GetClass called for ID: {ClassId}", id);
        var classDef = _configProvider.GetClassById(id);
        return classDef != null ? ToDto(classDef) : null;
    }

    /// <summary>
    /// Validates whether a player meets the requirements for a class.
    /// </summary>
    public ClassRequirementValidation ValidateClassRequirements(
        string classId,
        string raceId,
        IReadOnlyDictionary<string, int> attributes)
    {
        _logger.LogDebug(
            "ValidateClassRequirements for Class: {ClassId}, Race: {RaceId}",
            classId, raceId);

        var classDef = _configProvider.GetClassById(classId);

        if (classDef == null)
        {
            _logger.LogWarning("Class not found: {ClassId}", classId);
            return new ClassRequirementValidation(false, ["Class not found"]);
        }

        if (classDef.Requirements == null || !classDef.Requirements.Value.HasRequirements)
        {
            _logger.LogDebug("Class {ClassId} has no requirements", classId);
            return new ClassRequirementValidation(true, []);
        }

        var result = classDef.Requirements.Value.Validate(raceId, attributes);

        if (!result.IsValid)
        {
            _logger.LogInformation(
                "Class requirements not met for {ClassId}: {Reasons}",
                classId, string.Join(", ", result.FailureReasons));
        }

        return result;
    }

    /// <summary>
    /// Applies a class to a player, setting archetype/class IDs, applying stat modifiers, and initializing abilities.
    /// </summary>
    public void ApplyClassToPlayer(string classId, Player player)
    {
        _logger.LogDebug(
            "ApplyClassToPlayer for Class: {ClassId}, Player: {PlayerName}",
            classId, player.Name);

        var classDef = _configProvider.GetClassById(classId)
            ?? throw new ArgumentException($"Class not found: {classId}", nameof(classId));

        player.SetClass(classDef.ArchetypeId, classDef.Id);

        var modifiedStats = classDef.StatModifiers.ApplyTo(player.Stats);
        player.SetStats(modifiedStats);

        // Initialize abilities for this class
        _abilityService.InitializePlayerAbilities(player, classDef);

        _logger.LogInformation(
            "Applied class {ClassName} to {PlayerName}. " +
            "Stats: HP {OldHP}->{NewHP}, ATK {OldATK}->{NewATK}, DEF {OldDEF}->{NewDEF}, Abilities: {AbilityCount}",
            classDef.Name, player.Name,
            player.Stats.MaxHealth - classDef.StatModifiers.MaxHealth, player.Stats.MaxHealth,
            player.Stats.Attack - classDef.StatModifiers.Attack, player.Stats.Attack,
            player.Stats.Defense - classDef.StatModifiers.Defense, player.Stats.Defense,
            player.Abilities.Count);
    }

    /// <summary>
    /// Calculates what stats would be after applying a class (for preview).
    /// </summary>
    public Stats CalculateModifiedStats(Stats baseStats, string classId)
    {
        var classDef = _configProvider.GetClassById(classId);
        if (classDef == null) return baseStats;

        return classDef.StatModifiers.ApplyTo(baseStats);
    }

    private static ArchetypeDto ToDto(ArchetypeDefinition archetype, IReadOnlyList<ClassDefinition> allClasses)
    {
        var classNames = allClasses
            .Where(c => c.ArchetypeId.Equals(archetype.Id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Name)
            .ToList();

        return new ArchetypeDto(
            archetype.Id,
            archetype.Name,
            archetype.Description,
            archetype.PlaystyleSummary,
            archetype.StatTendency,
            archetype.SortOrder,
            classNames);
    }

    private static ClassDto ToDto(ClassDefinition classDef)
    {
        return new ClassDto(
            classDef.Id,
            classDef.Name,
            classDef.Description,
            classDef.ArchetypeId,
            classDef.StatModifiers,
            classDef.GrowthRates,
            classDef.PrimaryResourceId,
            classDef.StartingAbilityIds,
            classDef.Requirements?.HasRequirements ?? false,
            classDef.SortOrder);
    }
}
