using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements ability execution and cooldown management.
/// Delegates EffectScript parsing to EffectScriptExecutor (v0.3.3a refactor).
/// Supports telegraphed charge abilities (v0.2.4c).
/// </summary>
/// <remarks>See: SPEC-ABILITY-001 for Ability System design.</remarks>
public class AbilityService : IAbilityService
{
    private readonly IResourceService _resourceService;
    private readonly IStatusEffectService _statusEffects;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly ILogger<AbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityService"/> class.
    /// </summary>
    /// <param name="resourceService">Service for resource validation and deduction.</param>
    /// <param name="statusEffects">Service for status effect management (v0.2.4c).</param>
    /// <param name="scriptExecutor">Shared utility for effect script execution.</param>
    /// <param name="logger">Logger for traceability.</param>
    public AbilityService(
        IResourceService resourceService,
        IStatusEffectService statusEffects,
        EffectScriptExecutor scriptExecutor,
        ILogger<AbilityService> logger)
    {
        _resourceService = resourceService;
        _statusEffects = statusEffects;
        _scriptExecutor = scriptExecutor;
        _logger = logger;

        _logger.LogInformation("AbilityService initialized");
    }

    /// <inheritdoc/>
    public bool CanUse(Combatant user, ActiveAbility ability)
    {
        _logger.LogDebug(
            "[Ability] {User} checking if can use {Ability}",
            user.Name, ability.Name);

        // Check cooldown
        if (user.Cooldowns.TryGetValue(ability.Id, out var remaining) && remaining > 0)
        {
            _logger.LogDebug(
                "[Ability] {User} cannot use {Ability}: on cooldown ({Remaining} turns)",
                user.Name, ability.Name, remaining);
            return false;
        }

        // Check stamina cost
        if (ability.StaminaCost > 0 && !_resourceService.CanAfford(user, ResourceType.Stamina, ability.StaminaCost))
        {
            _logger.LogDebug(
                "[Ability] {User} cannot use {Ability}: insufficient stamina ({Cost} required, {Current} available)",
                user.Name, ability.Name, ability.StaminaCost, _resourceService.GetCurrent(user, ResourceType.Stamina));
            return false;
        }

        // Check aether cost
        if (ability.AetherCost > 0 && !_resourceService.CanAfford(user, ResourceType.Aether, ability.AetherCost))
        {
            _logger.LogDebug(
                "[Ability] {User} cannot use {Ability}: insufficient aether ({Cost} required, {Current} available)",
                user.Name, ability.Name, ability.AetherCost, _resourceService.GetCurrent(user, ResourceType.Aether));
            return false;
        }

        _logger.LogDebug(
            "[Ability] {User} can use {Ability}",
            user.Name, ability.Name);
        return true;
    }

    /// <inheritdoc/>
    public AbilityResult Execute(Combatant user, Combatant target, ActiveAbility ability)
    {
        _logger.LogInformation(
            "[Ability] {User} uses {Ability} on {Target}",
            user.Name, ability.Name, target.Name);

        // Validate usage
        if (!CanUse(user, ability))
        {
            var reason = GetCannotUseReason(user, ability);
            _logger.LogWarning(
                "[Ability] {User} failed to use {Ability}: {Reason}",
                user.Name, ability.Name, reason);
            return AbilityResult.Failure(reason);
        }

        // v0.2.4c: Check if this is a charge ability initiation
        if (ability.ChargeTurns > 0 && !_statusEffects.HasEffect(user, StatusEffectType.Chanting))
        {
            return InitiateCharge(user, ability);
        }

        // v0.2.4c: Check if this is a charge release (user is chanting and ability matches)
        if (user.ChanneledAbilityId == ability.Id &&
            _statusEffects.HasEffect(user, StatusEffectType.Chanting))
        {
            return ReleaseCharge(user, target, ability);
        }

        // Standard instant-cast logic
        DeductResources(user, ability);
        SetCooldown(user, ability);

        // Execute effect script via shared executor (v0.3.3a)
        var result = ExecuteEffectScript(user, target, ability);

        _logger.LogInformation(
            "[Ability] {User} used {Ability} on {Target}: {Message}",
            user.Name, ability.Name, target.Name, result.Message);

        return result;
    }

    #region Telegraphed Ability Methods (v0.2.4c)

    /// <summary>
    /// Initiates a charge for a telegraphed ability.
    /// Applies Chanting status, deducts resources, and stores the channeled ability.
    /// </summary>
    private AbilityResult InitiateCharge(Combatant user, ActiveAbility ability)
    {
        _logger.LogInformation(
            "[Ability] {User} begins charging {Ability}",
            user.Name, ability.Name);

        // Deduct resources immediately (committed to the action)
        DeductResources(user, ability);

        // Apply Chanting status with duration = ChargeTurns
        _statusEffects.ApplyEffect(user, StatusEffectType.Chanting, ability.ChargeTurns, user.Id);

        // Store which ability is being channeled
        user.ChanneledAbilityId = ability.Id;

        // Return telegraph message
        var message = ability.TelegraphMessage ?? $"{user.Name} begins charging a powerful attack!";
        return AbilityResult.Ok($"⚠ {message}");
    }

    /// <summary>
    /// Releases a charged ability, executing its effect script.
    /// Clears Chanting status, sets cooldown, and deals damage.
    /// </summary>
    private AbilityResult ReleaseCharge(Combatant user, Combatant target, ActiveAbility ability)
    {
        _logger.LogInformation(
            "[Ability] {User} releases {Ability}!",
            user.Name, ability.Name);

        // Clear channeling state
        user.ChanneledAbilityId = null;
        _statusEffects.RemoveEffect(user, StatusEffectType.Chanting);

        // Set cooldown now (after release)
        SetCooldown(user, ability);

        // Execute the actual effect
        var scriptResult = ExecuteEffectScript(user, target, ability);

        var message = $"{user.Name} unleashes {ability.Name}! {scriptResult.Message}";
        _logger.LogInformation(
            "[Ability] {User} released {Ability} on {Target}: {Message}",
            user.Name, ability.Name, target.Name, message);

        return AbilityResult.Ok(
            message,
            scriptResult.TotalDamage,
            scriptResult.TotalHealing,
            scriptResult.StatusesApplied);
    }

    /// <summary>
    /// Deducts stamina and aether costs from the user.
    /// </summary>
    private void DeductResources(Combatant user, ActiveAbility ability)
    {
        if (ability.StaminaCost > 0)
        {
            _resourceService.Deduct(user, ResourceType.Stamina, ability.StaminaCost);
        }

        if (ability.AetherCost > 0)
        {
            _resourceService.Deduct(user, ResourceType.Aether, ability.AetherCost);
        }
    }

    /// <summary>
    /// Sets the cooldown for an ability if applicable.
    /// </summary>
    private void SetCooldown(Combatant user, ActiveAbility ability)
    {
        if (ability.CooldownTurns > 0)
        {
            user.Cooldowns[ability.Id] = ability.CooldownTurns;
            _logger.LogDebug(
                "[Ability] {User} ability {Ability} on cooldown: {Turns} turns",
                user.Name, ability.Name, ability.CooldownTurns);
        }
    }

    #endregion

    /// <inheritdoc/>
    public void ProcessCooldowns(Combatant combatant)
    {
        if (combatant.Cooldowns.Count == 0)
        {
            return;
        }

        _logger.LogTrace(
            "[Ability] Processing cooldowns for {Combatant}: {Count} active",
            combatant.Name, combatant.Cooldowns.Count);

        var expiredAbilities = new List<Guid>();

        foreach (var kvp in combatant.Cooldowns)
        {
            var newValue = kvp.Value - 1;
            if (newValue <= 0)
            {
                expiredAbilities.Add(kvp.Key);
                _logger.LogDebug(
                    "[Ability] {Combatant} ability {AbilityId} cooldown expired",
                    combatant.Name, kvp.Key);
            }
            else
            {
                combatant.Cooldowns[kvp.Key] = newValue;
                _logger.LogTrace(
                    "[Ability] Decremented {AbilityId} cooldown to {Remaining}",
                    kvp.Key, newValue);
            }
        }

        foreach (var abilityId in expiredAbilities)
        {
            combatant.Cooldowns.Remove(abilityId);
        }
    }

    /// <inheritdoc/>
    public int GetCooldownRemaining(Combatant combatant, Guid abilityId)
    {
        return combatant.Cooldowns.TryGetValue(abilityId, out var remaining) ? remaining : 0;
    }

    #region Private Methods

    /// <summary>
    /// Gets a human-readable reason why the ability cannot be used.
    /// </summary>
    private string GetCannotUseReason(Combatant user, ActiveAbility ability)
    {
        if (user.Cooldowns.TryGetValue(ability.Id, out var remaining) && remaining > 0)
        {
            return $"On cooldown ({remaining} turns remaining)";
        }

        if (ability.StaminaCost > 0 && !_resourceService.CanAfford(user, ResourceType.Stamina, ability.StaminaCost))
        {
            return $"Insufficient stamina ({ability.StaminaCost} required)";
        }

        if (ability.AetherCost > 0 && !_resourceService.CanAfford(user, ResourceType.Aether, ability.AetherCost))
        {
            return $"Insufficient aether ({ability.AetherCost} required)";
        }

        return "Unknown reason";
    }

    /// <summary>
    /// Delegates to EffectScriptExecutor and wraps result in AbilityResult.
    /// </summary>
    private AbilityResult ExecuteEffectScript(Combatant user, Combatant target, ActiveAbility ability)
    {
        if (string.IsNullOrWhiteSpace(ability.EffectScript))
        {
            _logger.LogWarning("[Ability] {Ability} has no EffectScript", ability.Name);
            return AbilityResult.Ok($"{user.Name} uses {ability.Name}, but nothing happens.");
        }

        // Delegate to shared executor
        var scriptResult = _scriptExecutor.Execute(
            ability.EffectScript,
            target,
            user.Name,
            user.Id);

        // Build combined narrative
        var message = new StringBuilder();
        message.Append($"{user.Name} uses {ability.Name}");

        if (target != user)
        {
            message.Append($" on {target.Name}");
        }

        message.Append('!');

        if (!string.IsNullOrEmpty(scriptResult.Narrative))
        {
            message.Append(' ');
            message.Append(scriptResult.Narrative);
        }

        return AbilityResult.Ok(
            message.ToString(),
            scriptResult.TotalDamage,
            scriptResult.TotalHealing,
            scriptResult.StatusesApplied.Count > 0 ? scriptResult.StatusesApplied : null);
    }

    #endregion
}
