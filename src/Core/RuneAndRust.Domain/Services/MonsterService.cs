using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Services;

/// <summary>
/// Service for spawning and managing monsters using configuration-driven definitions.
/// </summary>
public class MonsterService : IMonsterService
{
    private readonly Func<IReadOnlyList<MonsterDefinition>> _getMonsters;
    private readonly Func<string, MonsterDefinition?> _getMonsterById;
    private readonly Func<IReadOnlyList<string>, TierDefinition> _selectRandomTier;
    private readonly Func<string, TierDefinition?> _getTierById;
    private readonly Func<TierDefinition> _getDefaultTier;
    private readonly Func<IReadOnlyList<string>, TierDefinition, IReadOnlyList<string>> _selectRandomTraits;
    private readonly Func<IEnumerable<string>, IReadOnlyList<MonsterTrait>> _getTraits;
    private readonly ILogger<MonsterService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new monster service (backwards compatible constructor).
    /// </summary>
    /// <param name="getMonsters">Function to get all monster definitions.</param>
    /// <param name="getMonsterById">Function to get a monster definition by ID.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <remarks>
    /// This constructor is for backwards compatibility. Tier/trait functionality will use defaults.
    /// Use the full constructor for tier/trait support.
    /// </remarks>
    public MonsterService(
        Func<IReadOnlyList<MonsterDefinition>> getMonsters,
        Func<string, MonsterDefinition?> getMonsterById,
        ILogger<MonsterService> logger)
        : this(
            getMonsters,
            getMonsterById,
            _ => TierDefinition.Common,
            _ => null,
            () => TierDefinition.Common,
            (_, _) => [],
            _ => [],
            logger)
    {
    }

    /// <summary>
    /// Creates a new monster service with full tier/trait support.
    /// </summary>
    /// <param name="getMonsters">Function to get all monster definitions.</param>
    /// <param name="getMonsterById">Function to get a monster definition by ID.</param>
    /// <param name="selectRandomTier">Function to select a random tier from possible tiers.</param>
    /// <param name="getTierById">Function to get a tier by ID.</param>
    /// <param name="getDefaultTier">Function to get the default tier.</param>
    /// <param name="selectRandomTraits">Function to select random traits based on tier.</param>
    /// <param name="getTraits">Function to get trait definitions by IDs.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public MonsterService(
        Func<IReadOnlyList<MonsterDefinition>> getMonsters,
        Func<string, MonsterDefinition?> getMonsterById,
        Func<IReadOnlyList<string>, TierDefinition> selectRandomTier,
        Func<string, TierDefinition?> getTierById,
        Func<TierDefinition> getDefaultTier,
        Func<IReadOnlyList<string>, TierDefinition, IReadOnlyList<string>> selectRandomTraits,
        Func<IEnumerable<string>, IReadOnlyList<MonsterTrait>> getTraits,
        ILogger<MonsterService> logger)
    {
        _getMonsters = getMonsters ?? throw new ArgumentNullException(nameof(getMonsters));
        _getMonsterById = getMonsterById ?? throw new ArgumentNullException(nameof(getMonsterById));
        _selectRandomTier = selectRandomTier ?? throw new ArgumentNullException(nameof(selectRandomTier));
        _getTierById = getTierById ?? throw new ArgumentNullException(nameof(getTierById));
        _getDefaultTier = getDefaultTier ?? throw new ArgumentNullException(nameof(getDefaultTier));
        _selectRandomTraits = selectRandomTraits ?? throw new ArgumentNullException(nameof(selectRandomTraits));
        _getTraits = getTraits ?? throw new ArgumentNullException(nameof(getTraits));
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
    public Monster SpawnMonster(string definitionId, string? tierId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID cannot be null or empty.", nameof(definitionId));

        var definition = _getMonsterById(definitionId);
        if (definition == null)
        {
            _logger.LogWarning("Monster definition not found: {DefinitionId}", definitionId);
            throw new ArgumentException($"Monster definition '{definitionId}' not found.", nameof(definitionId));
        }

        // Select or find the tier
        TierDefinition tier;
        if (string.IsNullOrWhiteSpace(tierId))
        {
            tier = _selectRandomTier(definition.PossibleTiers);
        }
        else
        {
            tier = _getTierById(tierId) ?? _getDefaultTier();
        }

        // Select traits based on tier
        var traitIds = _selectRandomTraits(definition.PossibleTraits, tier);
        var traits = _getTraits(traitIds);

        // Create monster with tier-modified stats
        var monster = CreateMonsterWithTier(definition, tier, traitIds, traits);

        _logger.LogDebug(
            "Spawned monster: {DisplayName} (ID: {DefinitionId}, Tier: {TierId}, Traits: [{TraitIds}])",
            monster.DisplayName, definitionId, tier.Id, string.Join(", ", traitIds));

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
    /// Creates a monster with tier-modified stats and traits applied.
    /// </summary>
    private Monster CreateMonsterWithTier(
        MonsterDefinition definition,
        TierDefinition tier,
        IReadOnlyList<string> traitIds,
        IReadOnlyList<MonsterTrait> traits)
    {
        // Apply tier multipliers to base stats from definition
        var modifiedMaxHealth = (int)Math.Round(definition.BaseHealth * tier.HealthMultiplier);
        var modifiedAttack = (int)Math.Round(definition.BaseAttack * tier.AttackMultiplier);
        var modifiedDefense = (int)Math.Round(definition.BaseDefense * tier.DefenseMultiplier);
        var modifiedXp = (int)Math.Round(definition.ExperienceValue * tier.ExperienceMultiplier);

        // Apply Armored trait defense bonus
        var armoredTrait = traits.FirstOrDefault(t => t.Effect == TraitEffect.Armored);
        if (armoredTrait != null)
        {
            modifiedDefense += armoredTrait.EffectValue;
        }

        var modifiedStats = new Stats(modifiedMaxHealth, modifiedAttack, modifiedDefense);

        // Generate display name based on tier
        var displayName = GenerateDisplayName(definition, tier);

        // Create the monster
        var monster = new Monster(
            definition.Name,
            definition.Description,
            modifiedMaxHealth,
            modifiedStats,
            definition.InitiativeModifier,
            definition.Id,
            modifiedXp,
            definition.BaseResistances);

        // Apply AI behavior from definition
        monster.SetBehavior(definition.Behavior);

        // Apply healing if defined
        if (definition.CanHeal && definition.HealAmount.HasValue)
        {
            monster.EnableHealing(definition.HealAmount.Value);
        }

        // Apply tier info
        monster.SetTierInfo(
            tier.Id,
            displayName,
            tier.Color,
            tier.LootMultiplier,
            tier.GeneratesUniqueName);

        // Apply traits
        monster.SetTraits(traitIds);

        return monster;
    }

    /// <summary>
    /// Generates the display name based on tier type.
    /// </summary>
    private string GenerateDisplayName(MonsterDefinition definition, TierDefinition tier)
    {
        // Named tier gets a generated unique name
        if (tier.GeneratesUniqueName)
        {
            var nameGenerator = definition.NameGenerator ?? NameGeneratorConfig.Default;
            return nameGenerator.GenerateName(definition.Name, _random);
        }

        // Other tiers may have a prefix (e.g., "Elite Goblin")
        if (!string.IsNullOrWhiteSpace(tier.NamePrefix))
        {
            return $"{tier.NamePrefix} {definition.Name}";
        }

        // Common tier just uses the base name
        return definition.Name;
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
