using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for prioritizing and selecting optimal abilities for enemies.
/// v0.42.2: Ability Usage & Behavior Patterns
/// </summary>
public class AbilityPrioritizationService : IAbilityPrioritizationService
{
    private readonly ILogger<AbilityPrioritizationService> _logger;
    private readonly IBehaviorPatternService _behaviorService;
    private readonly IAIConfigurationRepository _configRepo;

    // Cache for archetype configurations
    private Dictionary<AIArchetype, AIArchetypeConfiguration>? _archetypeConfigCache;

    public AbilityPrioritizationService(
        ILogger<AbilityPrioritizationService> logger,
        IBehaviorPatternService behaviorService,
        IAIConfigurationRepository configRepo)
    {
        _logger = logger;
        _behaviorService = behaviorService;
        _configRepo = configRepo;
    }

    /// <inheritdoc/>
    public async Task<object> SelectOptimalAbilityAsync(
        Enemy enemy,
        object target,
        BattlefieldState state)
    {
        var availableAbilities = await GetAvailableAbilitiesAsync(enemy);

        if (!availableAbilities.Any())
        {
            _logger.LogWarning("Enemy {EnemyId} has no available abilities, using basic attack", enemy.Id);
            // Return a basic attack action
            return new { Name = "BasicAttack", IsBasicAttack = true };
        }

        var archetype = await _behaviorService.GetArchetypeAsync(enemy);
        var scores = new Dictionary<object, AbilityScore>();

        foreach (var ability in availableAbilities)
        {
            var score = await ScoreAbilityAsync(ability, enemy, target, state);
            scores[ability] = score;
        }

        var selected = scores.OrderByDescending(kvp => kvp.Value.TotalScore).First();

        var abilityName = ExtractAbilityName(selected.Key);
        var targetName = ExtractTargetName(target);

        _logger.LogInformation(
            "Ability selected: {EnemyId} ({Archetype}) uses {AbilityName} on {Target} (score={Score:F1})",
            enemy.Id, archetype, abilityName, targetName, selected.Value.TotalScore);

        return selected.Key;
    }

    /// <inheritdoc/>
    public async Task<AbilityScore> ScoreAbilityAsync(
        object ability,
        Enemy enemy,
        object target,
        BattlefieldState state)
    {
        var archetype = await _behaviorService.GetArchetypeAsync(enemy);
        var config = await GetArchetypeConfigurationAsync(archetype);

        // Calculate component scores
        float damageScore = CalculateDamageScore(ability, target);
        float utilityScore = CalculateUtilityScore(ability, target, state);
        float efficiencyScore = CalculateEfficiencyScore(ability, enemy);
        float situationScore = CalculateSituationScore(ability, state);

        // Apply archetype modifiers
        var category = GetAbilityCategory(ability);
        float archetypeModifier = GetArchetypeModifierForCategory(config, category);

        // Total score = weighted sum of components
        float totalScore =
            (damageScore * 0.4f) +
            (utilityScore * 0.3f) +
            (efficiencyScore * 0.2f) +
            (situationScore * 0.1f);

        // Apply archetype modifier
        totalScore *= archetypeModifier;

        var abilityName = ExtractAbilityName(ability);

        return new AbilityScore
        {
            Ability = ability,
            AbilityId = abilityName,
            AbilityName = abilityName,
            TotalScore = totalScore,
            DamageScore = damageScore,
            UtilityScore = utilityScore,
            EfficiencyScore = efficiencyScore,
            SituationScore = situationScore,
            ArchetypeModifier = archetypeModifier,
            Reasoning = GenerateReasoning(damageScore, utilityScore, efficiencyScore, situationScore, archetype)
        };
    }

    /// <inheritdoc/>
    public async Task<List<object>> GetAvailableAbilitiesAsync(Enemy enemy)
    {
        var available = new List<object>();

        // TODO: v0.42.2 - Implement proper ability tracking for enemies
        // For now, return an empty list - enemies will use basic attacks
        // This will be fully implemented when enemy abilities are properly defined

        return available;
    }

    /// <inheritdoc/>
    public float CalculateDamageScore(object ability, object target)
    {
        if (ability is Ability playerAbility)
        {
            // Score based on damage dice and bonus
            float damage = playerAbility.DamageDice * 3.5f; // Average d6 = 3.5

            // Bonus for ignoring armor
            if (playerAbility.IgnoresArmor)
                damage *= 1.3f;

            return Math.Min(damage, 100f);
        }

        // For EnemyAction enum values (legacy system)
        var actionName = ability.ToString() ?? "";

        if (actionName.Contains("Heavy") || actionName.Contains("Strike") || actionName.Contains("Berserker"))
            return 60f; // High damage
        else if (actionName.Contains("Quick") || actionName.Contains("Basic"))
            return 30f; // Medium damage
        else if (actionName.Contains("Heal") || actionName.Contains("Repair"))
            return 0f; // No damage

        return 20f; // Default low damage
    }

    /// <inheritdoc/>
    public float CalculateUtilityScore(object ability, object target, BattlefieldState state)
    {
        if (ability is Ability playerAbility)
        {
            float utility = 0f;

            // Control abilities (stuns, disables)
            if (playerAbility.SkipEnemyTurn)
                utility += 30f;

            // Defensive abilities
            if (playerAbility.DefensePercent > 0)
                utility += playerAbility.DefensePercent * 0.5f;

            // Buff abilities
            if (playerAbility.NextAttackBonusDice > 0)
                utility += playerAbility.NextAttackBonusDice * 5f;

            // Dodge/negate abilities
            if (playerAbility.NegateNextAttack)
                utility += 25f;

            return Math.Min(utility, 50f);
        }

        // For EnemyAction enum values
        var actionName = ability.ToString() ?? "";

        if (actionName.Contains("Stun") || actionName.Contains("Disable"))
            return 40f; // High CC value
        else if (actionName.Contains("Buff") || actionName.Contains("Overcharge"))
            return 30f; // Buff value
        else if (actionName.Contains("Heal") || actionName.Contains("Repair"))
            return 35f; // Heal value
        else if (actionName.Contains("Defense"))
            return 25f; // Defensive value

        return 0f; // No utility
    }

    /// <inheritdoc/>
    public float CalculateEfficiencyScore(object ability, Enemy enemy)
    {
        if (ability is Ability playerAbility)
        {
            // Simple efficiency: higher stamina cost = lower efficiency
            float cost = playerAbility.StaminaCost + playerAbility.APCost;

            if (cost == 0)
                return 30f; // Free ability, max efficiency

            float efficiency = 30f - (cost * 2f);
            return Math.Max(efficiency, 0f);
        }

        // For EnemyAction enum values
        var actionName = ability.ToString() ?? "";

        if (actionName.Contains("Basic"))
            return 30f; // Basic attacks are very efficient
        else if (actionName.Contains("Emergency") || actionName.Contains("Last"))
            return 10f; // Expensive/desperate abilities

        return 20f; // Default medium efficiency
    }

    /// <inheritdoc/>
    public float CalculateSituationScore(object ability, BattlefieldState state)
    {
        // Situational context scoring
        float score = 10f; // Base score

        var actionName = ability.ToString() ?? "";

        // High value if outnumbered and ability is AOE
        var playerCount = state.PlayerCharacters.Count(p => p.IsAlive);
        var enemyCount = state.Enemies.Count(e => e.IsAlive);

        if (playerCount > enemyCount && (actionName.Contains("AOE") || actionName.Contains("Storm") || actionName.Contains("Whirlwind")))
        {
            score += 10f; // AOE more valuable when outnumbered
        }

        // Healing more valuable if allies are wounded
        if (actionName.Contains("Heal") || actionName.Contains("Repair"))
        {
            var woundedAllies = state.Enemies.Count(e => e.IsAlive && e.HP < e.MaxHP * 0.5f);
            if (woundedAllies > 0)
                score += 10f;
        }

        return Math.Min(score, 20f);
    }

    /// <inheritdoc/>
    public AbilityCategory GetAbilityCategory(object ability)
    {
        if (ability is Ability playerAbility)
        {
            return playerAbility.Type switch
            {
                AbilityType.Attack => AbilityCategory.Damage,
                AbilityType.Defense => AbilityCategory.Defensive,
                AbilityType.Control => AbilityCategory.CrowdControl,
                AbilityType.Utility => AbilityCategory.Buff,
                _ => AbilityCategory.BasicAttack
            };
        }

        var actionName = ability.ToString() ?? "";

        if (actionName.Contains("Heal") || actionName.Contains("Repair"))
            return AbilityCategory.Healing;
        else if (actionName.Contains("Stun") || actionName.Contains("Disable") || actionName.Contains("Fear"))
            return AbilityCategory.CrowdControl;
        else if (actionName.Contains("Buff") || actionName.Contains("Overcharge"))
            return AbilityCategory.Buff;
        else if (actionName.Contains("Defense") || actionName.Contains("Guard"))
            return AbilityCategory.Defensive;
        else if (actionName.Contains("AOE") || actionName.Contains("Storm") || actionName.Contains("Whirlwind"))
            return AbilityCategory.AOE;
        else if (actionName.Contains("Summon"))
            return AbilityCategory.Summoning;
        else if (actionName.Contains("Basic"))
            return AbilityCategory.BasicAttack;

        return AbilityCategory.Damage;
    }

    /// <inheritdoc/>
    public bool IsAbilityOnCooldown(object ability, Enemy enemy)
    {
        // TODO: v0.42.2 - Implement cooldown tracking
        // For now, assume abilities are always available
        return false;
    }

    /// <inheritdoc/>
    public bool HasSufficientResources(object ability, Enemy enemy)
    {
        // TODO: v0.42.2 - Implement resource tracking for enemies
        // For now, assume enemies always have resources
        return true;
    }

    // ===== HELPER METHODS =====

    private async Task<AIArchetypeConfiguration> GetArchetypeConfigurationAsync(AIArchetype archetype)
    {
        // Initialize cache if needed
        if (_archetypeConfigCache == null)
        {
            _archetypeConfigCache = await _configRepo.GetAllArchetypeConfigurationsAsync();
        }

        if (_archetypeConfigCache.TryGetValue(archetype, out var config))
        {
            return config;
        }

        _logger.LogWarning("No configuration found for archetype {Archetype}, using Tactical defaults", archetype);

        // Fallback to balanced configuration
        return new AIArchetypeConfiguration
        {
            Archetype = archetype,
            DamageAbilityModifier = 1.0m,
            UtilityAbilityModifier = 1.0m,
            DefensiveAbilityModifier = 1.0m,
            AggressionLevel = 3
        };
    }

    private float GetArchetypeModifierForCategory(AIArchetypeConfiguration config, AbilityCategory category)
    {
        return category switch
        {
            AbilityCategory.Damage => (float)config.DamageAbilityModifier,
            AbilityCategory.AOE => (float)config.DamageAbilityModifier,
            AbilityCategory.Healing => (float)config.DefensiveAbilityModifier,
            AbilityCategory.Buff => (float)config.UtilityAbilityModifier,
            AbilityCategory.Debuff => (float)config.UtilityAbilityModifier,
            AbilityCategory.CrowdControl => (float)config.UtilityAbilityModifier,
            AbilityCategory.Defensive => (float)config.DefensiveAbilityModifier,
            AbilityCategory.Summoning => (float)config.UtilityAbilityModifier,
            AbilityCategory.Ultimate => (float)config.DamageAbilityModifier * 1.2f,
            _ => 1.0f
        };
    }

    private string ExtractAbilityName(object ability)
    {
        if (ability is Ability playerAbility)
            return playerAbility.Name;

        return ability.ToString() ?? "Unknown";
    }

    private string ExtractTargetName(object target)
    {
        return target switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Name,
            _ => "Unknown"
        };
    }

    private string GenerateReasoning(float damage, float utility, float efficiency, float situation, AIArchetype archetype)
    {
        var components = new List<string>();

        if (damage > 30)
            components.Add($"High damage ({damage:F0})");
        if (utility > 20)
            components.Add($"Good utility ({utility:F0})");
        if (efficiency > 20)
            components.Add($"Efficient ({efficiency:F0})");
        if (situation > 15)
            components.Add($"Situationally strong ({situation:F0})");

        if (components.Any())
            return string.Join(", ", components);

        return $"{archetype} archetype preference";
    }
}
