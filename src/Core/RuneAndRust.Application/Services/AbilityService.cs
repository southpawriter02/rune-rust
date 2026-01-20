using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Manages ability definitions and player ability usage.
/// </summary>
public class AbilityService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ResourceService _resourceService;
    private readonly ILogger<AbilityService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public AbilityService(
        IGameConfigurationProvider configProvider,
        ResourceService resourceService,
        ILogger<AbilityService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        _logger.LogInformation(
            "AbilityService initialized with {AbilityCount} abilities",
            _configProvider.GetAbilities().Count);
    }

    /// <summary>
    /// Gets an ability definition by ID.
    /// </summary>
    public AbilityDefinition? GetAbilityDefinition(string abilityId)
    {
        _logger.LogDebug("GetAbilityDefinition called for: {AbilityId}", abilityId);
        return _configProvider.GetAbilityById(abilityId);
    }

    /// <summary>
    /// Gets all ability definitions.
    /// </summary>
    public IReadOnlyList<AbilityDefinitionDto> GetAllAbilities()
    {
        _logger.LogDebug("GetAllAbilities called");
        return _configProvider.GetAbilities()
            .Select(ToDto)
            .ToList();
    }

    /// <summary>
    /// Gets all abilities available to a specific class.
    /// </summary>
    public IReadOnlyList<AbilityDefinitionDto> GetAbilitiesForClass(string classId)
    {
        _logger.LogDebug("GetAbilitiesForClass called for: {ClassId}", classId);
        return _configProvider.GetAbilitiesForClass(classId)
            .Select(ToDto)
            .ToList();
    }

    /// <summary>
    /// Gets all of a player's abilities as DTOs.
    /// </summary>
    public IReadOnlyList<PlayerAbilityDto> GetPlayerAbilities(Player player)
    {
        _logger.LogDebug("GetPlayerAbilities for: {PlayerName}", player.Name);

        return player.Abilities.Values
            .Select(pa =>
            {
                var definition = GetAbilityDefinition(pa.AbilityDefinitionId);
                if (definition == null) return null;

                var canAfford = CanAffordAbility(player, definition);

                return new PlayerAbilityDto(
                    pa.AbilityDefinitionId,
                    definition.Name,
                    definition.Description,
                    definition.Cost.ResourceTypeId,
                    definition.Cost.Amount,
                    definition.Cooldown,
                    pa.CurrentCooldown,
                    definition.TargetType,
                    definition.UnlockLevel,
                    pa.IsUnlocked,
                    pa.IsReady && canAfford,
                    pa.IsOnCooldown,
                    canAfford,
                    pa.TimesUsed,
                    definition.Tags.ToList());
            })
            .Where(dto => dto != null)
            .Cast<PlayerAbilityDto>()
            .ToList();
    }

    /// <summary>
    /// Validates whether a player can use a specific ability.
    /// </summary>
    public AbilityValidationResult CanUseAbility(Player player, string abilityId)
    {
        _logger.LogDebug(
            "CanUseAbility for Player: {PlayerName}, Ability: {AbilityId}",
            player.Name, abilityId);

        var definition = GetAbilityDefinition(abilityId);
        if (definition == null)
        {
            _logger.LogDebug("Ability not found: {AbilityId}", abilityId);
            return AbilityValidationResult.AbilityNotFound;
        }

        var playerAbility = player.GetAbility(abilityId);
        if (playerAbility == null)
        {
            _logger.LogDebug("Player doesn't have ability: {AbilityId}", abilityId);
            return AbilityValidationResult.AbilityNotLearned;
        }

        if (!playerAbility.IsUnlocked)
        {
            _logger.LogDebug(
                "Ability locked, requires level {RequiredLevel}: {AbilityId}",
                definition.UnlockLevel, abilityId);
            return AbilityValidationResult.AbilityLocked(definition.UnlockLevel);
        }

        if (playerAbility.IsOnCooldown)
        {
            _logger.LogDebug(
                "Ability on cooldown ({TurnsRemaining} turns): {AbilityId}",
                playerAbility.CurrentCooldown, abilityId);
            return AbilityValidationResult.OnCooldown(playerAbility.CurrentCooldown);
        }

        if (definition.HasCost)
        {
            var pool = player.GetResource(definition.Cost.ResourceTypeId);
            if (pool == null)
            {
                _logger.LogWarning(
                    "Player doesn't have resource type: {ResourceType}",
                    definition.Cost.ResourceTypeId);
                return AbilityValidationResult.InsufficientResource(
                    definition.Cost.ResourceTypeId, 0, definition.Cost.Amount);
            }

            if (pool.Current < definition.Cost.Amount)
            {
                _logger.LogDebug(
                    "Insufficient {Resource}: has {Current}, needs {Required}",
                    definition.Cost.ResourceTypeId, pool.Current, definition.Cost.Amount);
                return AbilityValidationResult.InsufficientResource(
                    definition.Cost.ResourceTypeId, pool.Current, definition.Cost.Amount);
            }
        }

        _logger.LogDebug("Ability can be used: {AbilityId}", abilityId);
        return AbilityValidationResult.Success;
    }

    /// <summary>
    /// Uses an ability, applying its effects and consuming resources.
    /// </summary>
    /// <param name="player">The player using the ability.</param>
    /// <param name="abilityId">The ability to use.</param>
    /// <param name="target">Optional target entity (for enemy-targeting abilities).</param>
    /// <returns>The result of the ability use.</returns>
    public AbilityResult UseAbility(Player player, string abilityId, Monster? target = null)
    {
        _logger.LogDebug(
            "UseAbility: Player: {PlayerName}, Ability: {AbilityId}, Target: {Target}",
            player.Name, abilityId, target?.Name ?? "none");

        var validation = CanUseAbility(player, abilityId);
        if (!validation.IsValid)
        {
            return AbilityResult.Failed(validation.FailureReason!);
        }

        var definition = GetAbilityDefinition(abilityId)!;
        var playerAbility = player.GetAbility(abilityId)!;

        // Spend resource if ability has a cost
        ResourceChange? resourceSpent = null;
        if (definition.HasCost)
        {
            var pool = player.GetResource(definition.Cost.ResourceTypeId)!;
            var previousValue = pool.Current;

            if (!_resourceService.SpendResource(player, definition.Cost.ResourceTypeId, definition.Cost.Amount))
            {
                return AbilityResult.Failed($"Failed to spend {definition.Cost.Amount} {definition.Cost.ResourceTypeId}");
            }

            resourceSpent = new ResourceChange(
                definition.Cost.ResourceTypeId,
                previousValue,
                pool.Current,
                ResourceChangeType.Spent);
        }

        // Set cooldown
        playerAbility.Use(definition.Cooldown);

        // Apply effects
        var appliedEffects = ApplyEffects(player, definition, target);

        var message = BuildResultMessage(definition, target, appliedEffects);

        _logger.LogInformation(
            "Ability used: {PlayerName} used {AbilityName}. {EffectCount} effects applied",
            player.Name, definition.Name, appliedEffects.Count);

        _eventLogger?.LogAbility("AbilityUsed", $"{player.Name} used {definition.Name}",
            data: new Dictionary<string, object>
            {
                ["playerId"] = player.Id,
                ["abilityId"] = abilityId,
                ["abilityName"] = definition.Name,
                ["targetName"] = target?.Name ?? "none",
                ["effectCount"] = appliedEffects.Count
            });

        return new AbilityResult(true, message, appliedEffects, resourceSpent);
    }

    /// <summary>
    /// Initializes a player's abilities based on their class.
    /// </summary>
    public void InitializePlayerAbilities(Player player, ClassDefinition classDef)
    {
        _logger.LogDebug(
            "InitializePlayerAbilities for Player: {PlayerName}, Class: {ClassId}",
            player.Name, classDef.Id);

        foreach (var abilityId in classDef.StartingAbilityIds)
        {
            var definition = GetAbilityDefinition(abilityId);
            if (definition == null)
            {
                _logger.LogWarning(
                    "Starting ability not found for class {ClassId}: {AbilityId}",
                    classDef.Id, abilityId);
                continue;
            }

            var unlocked = definition.UnlockLevel <= player.Level;
            var playerAbility = PlayerAbility.Create(abilityId, unlocked);
            player.AddAbility(playerAbility);

            _logger.LogDebug(
                "Added ability {AbilityName} to {PlayerName} (unlocked: {Unlocked})",
                definition.Name, player.Name, unlocked);
        }

        _logger.LogInformation(
            "Initialized {AbilityCount} abilities for {PlayerName}",
            player.Abilities.Count, player.Name);
    }

    /// <summary>
    /// Processes end-of-turn cooldown reduction for all player abilities.
    /// </summary>
    /// <param name="player">The player whose abilities to process.</param>
    /// <returns>A list of cooldown changes that occurred.</returns>
    public IReadOnlyList<CooldownChangeDto> ProcessTurnEnd(Player player)
    {
        _logger.LogDebug("ProcessTurnEnd for: {PlayerName}", player.Name);

        var cooldownChanges = new List<CooldownChangeDto>();

        foreach (var ability in player.Abilities.Values)
        {
            if (ability.IsOnCooldown)
            {
                var previousCooldown = ability.CurrentCooldown;
                ability.ReduceCooldown();

                var definition = GetAbilityDefinition(ability.AbilityDefinitionId);
                var abilityName = definition?.Name ?? ability.AbilityDefinitionId;

                var isNowReady = ability.CurrentCooldown == 0;
                cooldownChanges.Add(new CooldownChangeDto(
                    abilityName,
                    previousCooldown,
                    ability.CurrentCooldown,
                    isNowReady));

                _logger.LogDebug(
                    "Cooldown reduced for {AbilityId}: {Remaining} turns remaining",
                    ability.AbilityDefinitionId, ability.CurrentCooldown);

                if (isNowReady)
                {
                    _logger.LogInformation("{Ability} is now ready", abilityName);
                }
            }
        }

        if (cooldownChanges.Count > 0)
        {
            _logger.LogInformation(
                "Reduced cooldowns for {Count} abilities for {PlayerName}",
                cooldownChanges.Count, player.Name);
        }

        return cooldownChanges;
    }

    /// <summary>
    /// Unlocks an ability for a player.
    /// </summary>
    public bool UnlockAbility(Player player, string abilityId)
    {
        _logger.LogDebug(
            "UnlockAbility: Player: {PlayerName}, Ability: {AbilityId}",
            player.Name, abilityId);

        var playerAbility = player.GetAbility(abilityId);
        if (playerAbility == null)
        {
            _logger.LogWarning("Player doesn't have ability to unlock: {AbilityId}", abilityId);
            return false;
        }

        if (playerAbility.IsUnlocked)
        {
            _logger.LogDebug("Ability already unlocked: {AbilityId}", abilityId);
            return true;
        }

        playerAbility.Unlock();
        _logger.LogInformation(
            "Unlocked ability {AbilityId} for {PlayerName}",
            abilityId, player.Name);

        return true;
    }

    /// <summary>
    /// Gets abilities that should be unlocked at a specific level for a class.
    /// </summary>
    public IReadOnlyList<AbilityDefinition> GetUnlockedAbilitiesAtLevel(string classId, int level)
    {
        _logger.LogDebug(
            "GetUnlockedAbilitiesAtLevel: Class: {ClassId}, Level: {Level}",
            classId, level);

        return _configProvider.GetAbilitiesForClass(classId)
            .Where(a => a.UnlockLevel == level)
            .ToList();
    }

    /// <summary>
    /// Finds an ability by partial name match (for user input).
    /// </summary>
    public AbilityDefinition? FindAbilityByName(Player player, string partialName)
    {
        var normalized = partialName.ToLowerInvariant().Trim();

        // First check player's abilities for exact match
        var playerAbility = player.Abilities.Values
            .Select(pa => GetAbilityDefinition(pa.AbilityDefinitionId))
            .FirstOrDefault(def => def?.Name.Equals(partialName, StringComparison.OrdinalIgnoreCase) == true);

        if (playerAbility != null) return playerAbility;

        // Then check for partial match
        return player.Abilities.Values
            .Select(pa => GetAbilityDefinition(pa.AbilityDefinitionId))
            .FirstOrDefault(def => def?.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase) == true
                                   || def?.Id.Contains(normalized) == true);
    }

    private bool CanAffordAbility(Player player, AbilityDefinition definition)
    {
        if (!definition.HasCost) return true;

        var pool = player.GetResource(definition.Cost.ResourceTypeId);
        return pool != null && pool.Current >= definition.Cost.Amount;
    }

    private List<AppliedEffect> ApplyEffects(Player player, AbilityDefinition definition, Monster? target)
    {
        var appliedEffects = new List<AppliedEffect>();

        foreach (var effect in definition.Effects)
        {
            var applied = ApplyEffect(player, effect, target);
            if (applied != null)
            {
                appliedEffects.Add(applied);
            }
        }

        return appliedEffects;
    }

    private AppliedEffect? ApplyEffect(Player player, AbilityEffect effect, Monster? target)
    {
        // Check chance-based effects
        if (effect.HasChance && Random.Shared.NextDouble() > effect.Chance)
        {
            var resistTarget = effect.EffectType switch
            {
                AbilityEffectType.Damage or
                AbilityEffectType.DamageOverTime or
                AbilityEffectType.Debuff or
                AbilityEffectType.Stun or
                AbilityEffectType.Taunt => target?.Name ?? "Target",
                _ => player.Name
            };

            return new AppliedEffect(effect.EffectType, 0, resistTarget, WasResisted: true);
        }

        // Calculate scaled value
        var value = CalculateEffectValue(player, effect);

        return effect.EffectType switch
        {
            AbilityEffectType.Damage => ApplyDamageEffect(value, target),
            AbilityEffectType.Heal => ApplyHealEffect(player, value),
            AbilityEffectType.Shield => ApplyShieldEffect(player, effect, value),
            AbilityEffectType.DamageOverTime => ApplyDotEffect(target, effect),
            AbilityEffectType.HealOverTime => ApplyHotEffect(player, effect),
            AbilityEffectType.Buff => ApplyBuffEffect(player, effect),
            AbilityEffectType.Debuff => ApplyDebuffEffect(target, effect),
            AbilityEffectType.Stun => ApplyStunEffect(target, effect),
            AbilityEffectType.Taunt => ApplyTauntEffect(target, effect),
            AbilityEffectType.ResourceGain => ApplyResourceGainEffect(player, effect),
            _ => new AppliedEffect(effect.EffectType, value, player.Name)
        };
    }

    private int CalculateEffectValue(Player player, AbilityEffect effect)
    {
        var baseValue = effect.Value;

        if (!effect.HasScaling) return baseValue;

        var scalingStat = effect.ScalingStat!.ToLowerInvariant();
        var statValue = scalingStat switch
        {
            "attack" => player.Stats.Attack,
            "defense" => player.Stats.Defense,
            "might" => player.Attributes.Might,
            "fortitude" => player.Attributes.Fortitude,
            "will" => player.Attributes.Will,
            "wits" => player.Attributes.Wits,
            "finesse" => player.Attributes.Finesse,
            _ => 0
        };

        var scaledBonus = (int)(statValue * effect.ScalingMultiplier);
        return baseValue + scaledBonus;
    }

    private AppliedEffect? ApplyDamageEffect(int value, Monster? target)
    {
        if (target == null)
        {
            _logger.LogWarning("Damage effect applied without target");
            return null;
        }

        var actualDamage = target.TakeDamage(value);
        _logger.LogDebug("Dealt {Damage} damage to {Target}", actualDamage, target.Name);

        return new AppliedEffect(AbilityEffectType.Damage, actualDamage, target.Name);
    }

    private AppliedEffect ApplyHealEffect(Player player, int value)
    {
        var actualHeal = player.Heal(value);
        _logger.LogDebug("Healed {Player} for {Amount}", player.Name, actualHeal);

        return new AppliedEffect(AbilityEffectType.Heal, actualHeal, player.Name);
    }

    private AppliedEffect ApplyShieldEffect(Player player, AbilityEffect effect, int value)
    {
        // Note: Full shield implementation deferred to status effects system
        _logger.LogDebug("Shield effect applied to {Player} for {Value}", player.Name, value);
        return new AppliedEffect(AbilityEffectType.Shield, value, player.Name);
    }

    private AppliedEffect? ApplyDotEffect(Monster? target, AbilityEffect effect)
    {
        // Note: Full DoT implementation deferred to status effects system
        if (target == null) return null;

        _logger.LogDebug("DoT applied to {Target}: {Status}", target.Name, effect.StatusEffect);
        return new AppliedEffect(
            AbilityEffectType.DamageOverTime,
            effect.Value,
            target.Name,
            StatusApplied: effect.StatusEffect);
    }

    private AppliedEffect ApplyHotEffect(Player player, AbilityEffect effect)
    {
        // Note: Full HoT implementation deferred to status effects system
        _logger.LogDebug("HoT applied to {Player}", player.Name);
        return new AppliedEffect(AbilityEffectType.HealOverTime, effect.Value, player.Name);
    }

    private AppliedEffect ApplyBuffEffect(Player player, AbilityEffect effect)
    {
        // Note: Full buff implementation deferred to status effects system
        _logger.LogDebug("Buff applied to {Player}", player.Name);
        return new AppliedEffect(
            AbilityEffectType.Buff,
            effect.Value,
            player.Name,
            StatusApplied: effect.Description);
    }

    private AppliedEffect? ApplyDebuffEffect(Monster? target, AbilityEffect effect)
    {
        // Note: Full debuff implementation deferred to status effects system
        if (target == null) return null;

        _logger.LogDebug("Debuff applied to {Target}: {Status}", target.Name, effect.StatusEffect);
        return new AppliedEffect(
            AbilityEffectType.Debuff,
            effect.Value,
            target.Name,
            StatusApplied: effect.StatusEffect);
    }

    private AppliedEffect? ApplyStunEffect(Monster? target, AbilityEffect effect)
    {
        // Note: Full stun implementation deferred to status effects system
        if (target == null) return null;

        _logger.LogDebug("Stun applied to {Target}", target.Name);
        return new AppliedEffect(AbilityEffectType.Stun, effect.Duration, target.Name, StatusApplied: "stun");
    }

    private AppliedEffect? ApplyTauntEffect(Monster? target, AbilityEffect effect)
    {
        // Note: Full taunt implementation deferred to combat system
        if (target == null) return null;

        _logger.LogDebug("Taunt applied to {Target}", target.Name);
        return new AppliedEffect(AbilityEffectType.Taunt, effect.Duration, target.Name, StatusApplied: "taunt");
    }

    private AppliedEffect ApplyResourceGainEffect(Player player, AbilityEffect effect)
    {
        // This would need to know which resource to gain
        _logger.LogDebug("Resource gain effect for {Player}", player.Name);
        return new AppliedEffect(AbilityEffectType.ResourceGain, effect.Value, player.Name);
    }

    private string BuildResultMessage(AbilityDefinition definition, Monster? target, List<AppliedEffect> effects)
    {
        if (effects.Count == 0)
            return $"Used {definition.Name}.";

        var parts = new List<string>();

        var damage = effects.Where(e => e.EffectType == AbilityEffectType.Damage && !e.WasResisted).Sum(e => e.Value);
        if (damage > 0 && target != null)
            parts.Add($"dealt {damage} damage to {target.Name}");

        var healing = effects.Where(e => e.EffectType == AbilityEffectType.Heal && !e.WasResisted).Sum(e => e.Value);
        if (healing > 0)
            parts.Add($"healed for {healing}");

        var shield = effects.FirstOrDefault(e => e.EffectType == AbilityEffectType.Shield);
        if (shield != null)
            parts.Add($"gained {shield.Value} shield");

        var statuses = effects
            .Where(e => e.StatusApplied != null && !e.WasResisted)
            .Select(e => e.StatusApplied!)
            .Distinct();
        foreach (var status in statuses)
            parts.Add($"applied {status}");

        var resisted = effects.Count(e => e.WasResisted);
        if (resisted > 0)
            parts.Add($"{resisted} effect(s) resisted");

        return parts.Count > 0
            ? $"{definition.Name}: {string.Join(", ", parts)}."
            : $"Used {definition.Name}.";
    }

    private static AbilityDefinitionDto ToDto(AbilityDefinition def)
    {
        return new AbilityDefinitionDto(
            def.Id,
            def.Name,
            def.Description,
            def.Cost.ResourceTypeId,
            def.Cost.Amount,
            def.Cooldown,
            def.TargetType,
            def.UnlockLevel,
            def.Tags.ToList(),
            def.Effects.Select(ToEffectDto).ToList());
    }

    private static AbilityEffectDto ToEffectDto(AbilityEffect effect)
    {
        return new AbilityEffectDto(
            effect.EffectType,
            effect.Value,
            effect.Duration,
            effect.StatusEffect,
            effect.StatModifier?.ToString(),
            effect.Chance,
            effect.ScalingStat,
            effect.ScalingMultiplier,
            effect.Description);
    }
}
