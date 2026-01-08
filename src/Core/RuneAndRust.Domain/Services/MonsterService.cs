using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Service for spawning and managing monsters using configuration-driven definitions.
/// </summary>
public class MonsterService : IMonsterService
{
    private readonly Func<IReadOnlyList<MonsterDefinition>> _getMonsters;
    private readonly Func<string, MonsterDefinition?> _getMonsterById;
    private readonly ILogger<MonsterService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new monster service.
    /// </summary>
    /// <param name="getMonsters">Function to get all monster definitions.</param>
    /// <param name="getMonsterById">Function to get a monster definition by ID.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public MonsterService(
        Func<IReadOnlyList<MonsterDefinition>> getMonsters,
        Func<string, MonsterDefinition?> getMonsterById,
        ILogger<MonsterService> logger)
    {
        _getMonsters = getMonsters ?? throw new ArgumentNullException(nameof(getMonsters));
        _getMonsterById = getMonsterById ?? throw new ArgumentNullException(nameof(getMonsterById));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <inheritdoc/>
    public Monster SpawnMonster(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID cannot be null or empty.", nameof(definitionId));

        var definition = _getMonsterById(definitionId);
        if (definition == null)
        {
            _logger.LogWarning("Monster definition not found: {DefinitionId}", definitionId);
            throw new ArgumentException($"Monster definition '{definitionId}' not found.", nameof(definitionId));
        }

        var monster = definition.CreateMonster();
        _logger.LogDebug("Spawned monster: {Name} (ID: {DefinitionId})", monster.Name, definitionId);
        return monster;
    }

    /// <inheritdoc/>
    public Monster SpawnRandomMonster()
    {
        var definitions = _getMonsters();
        if (definitions.Count == 0)
        {
            _logger.LogWarning("No monster definitions available, creating fallback goblin");
            return CreateFallbackMonster();
        }

        var selected = SelectWeightedRandom(definitions);
        var monster = selected.CreateMonster();
        _logger.LogDebug("Spawned random monster: {Name}", monster.Name);
        return monster;
    }

    /// <inheritdoc/>
    public Monster SpawnRandomMonster(IEnumerable<string> requiredTags)
    {
        if (requiredTags == null)
            return SpawnRandomMonster();

        var tagList = requiredTags.ToList();
        if (tagList.Count == 0)
            return SpawnRandomMonster();

        var definitions = _getMonsters();
        var matching = definitions.Where(d => d.HasAllTags(tagList)).ToList();

        if (matching.Count == 0)
        {
            _logger.LogWarning("No monsters found with tags: {Tags}", string.Join(", ", tagList));
            throw new InvalidOperationException($"No monsters found with required tags: {string.Join(", ", tagList)}");
        }

        var selected = SelectWeightedRandom(matching);
        var monster = selected.CreateMonster();
        _logger.LogDebug("Spawned random monster with tags [{Tags}]: {Name}", string.Join(", ", tagList), monster.Name);
        return monster;
    }

    /// <inheritdoc/>
    public IReadOnlyList<MonsterDefinition> GetAllDefinitions()
    {
        return _getMonsters();
    }

    /// <inheritdoc/>
    public MonsterDefinition? GetDefinition(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _getMonsterById(id);
    }

    /// <summary>
    /// Selects a random monster definition using weighted random selection.
    /// </summary>
    private MonsterDefinition SelectWeightedRandom(IReadOnlyList<MonsterDefinition> definitions)
    {
        var totalWeight = definitions.Sum(d => d.SpawnWeight);
        var roll = _random.Next(totalWeight);

        var cumulative = 0;
        foreach (var definition in definitions)
        {
            cumulative += definition.SpawnWeight;
            if (roll < cumulative)
            {
                return definition;
            }
        }

        // Fallback to last definition (should never happen)
        return definitions[definitions.Count - 1];
    }

    /// <summary>
    /// Creates a fallback monster when no definitions are available.
    /// </summary>
    /// <remarks>
    /// Uses the deprecated factory method intentionally as a fallback when config is missing.
    /// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
    private static Monster CreateFallbackMonster()
    {
        return Monster.CreateGoblin();
    }
#pragma warning restore CS0618
}
