using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements ability execution and cooldown management.
/// Parses EffectScript strings and delegates to appropriate services.
/// </summary>
public partial class AbilityService : IAbilityService
{
    private readonly IResourceService _resourceService;
    private readonly IStatusEffectService _statusEffectService;
    private readonly IDiceService _diceService;
    private readonly ILogger<AbilityService> _logger;

    /// <summary>
    /// Regex pattern for parsing dice notation (e.g., "2d6", "1d8").
    /// </summary>
    [GeneratedRegex(@"^(\d+)d(\d+)$", RegexOptions.Compiled)]
    private static partial Regex DiceNotationRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityService"/> class.
    /// </summary>
    /// <param name="resourceService">Service for resource validation and deduction.</param>
    /// <param name="statusEffectService">Service for applying status effects.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="logger">Logger for traceability.</param>
    public AbilityService(
        IResourceService resourceService,
        IStatusEffectService statusEffectService,
        IDiceService diceService,
        ILogger<AbilityService> logger)
    {
        _resourceService = resourceService;
        _statusEffectService = statusEffectService;
        _diceService = diceService;
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

        // Deduct resources
        if (ability.StaminaCost > 0)
        {
            _resourceService.Deduct(user, ResourceType.Stamina, ability.StaminaCost);
        }

        if (ability.AetherCost > 0)
        {
            _resourceService.Deduct(user, ResourceType.Aether, ability.AetherCost);
        }

        // Set cooldown
        if (ability.CooldownTurns > 0)
        {
            user.Cooldowns[ability.Id] = ability.CooldownTurns;
            _logger.LogDebug(
                "[Ability] {User} ability {Ability} on cooldown: {Turns} turns",
                user.Name, ability.Name, ability.CooldownTurns);
        }

        // Parse and execute effect script
        var result = ExecuteEffectScript(user, target, ability);

        _logger.LogInformation(
            "[Ability] {User} used {Ability} on {Target}: {Message}",
            user.Name, ability.Name, target.Name, result.Message);

        return result;
    }

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
    /// Parses and executes the EffectScript, applying all effects.
    /// </summary>
    private AbilityResult ExecuteEffectScript(Combatant user, Combatant target, ActiveAbility ability)
    {
        if (string.IsNullOrWhiteSpace(ability.EffectScript))
        {
            _logger.LogWarning(
                "[Ability] {Ability} has no EffectScript",
                ability.Name);
            return AbilityResult.Ok($"{user.Name} uses {ability.Name}, but nothing happens.");
        }

        _logger.LogDebug(
            "[Ability] Parsing EffectScript: {Script}",
            ability.EffectScript);

        var commands = ability.EffectScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var totalDamage = 0;
        var totalHealing = 0;
        var statusesApplied = new List<string>();
        var narratives = new List<string>();

        foreach (var command in commands)
        {
            var parts = command.Trim().Split(':');
            if (parts.Length == 0) continue;

            var commandType = parts[0].ToUpperInvariant();
            _logger.LogDebug(
                "[Ability] Executing command: {Command}",
                command);

            switch (commandType)
            {
                case "DAMAGE":
                    var damageResult = ExecuteDamageCommand(user, target, parts);
                    totalDamage += damageResult.damage;
                    narratives.Add(damageResult.narrative);
                    break;

                case "HEAL":
                    var healResult = ExecuteHealCommand(target, parts);
                    totalHealing += healResult.healing;
                    narratives.Add(healResult.narrative);
                    break;

                case "STATUS":
                    var statusResult = ExecuteStatusCommand(user, target, parts);
                    if (statusResult.applied)
                    {
                        statusesApplied.Add(statusResult.statusName);
                    }
                    narratives.Add(statusResult.narrative);
                    break;

                default:
                    _logger.LogWarning(
                        "[Ability] Unknown command type: {CommandType}",
                        commandType);
                    break;
            }
        }

        // Build combined narrative
        var message = new StringBuilder();
        message.Append($"{user.Name} uses {ability.Name}");

        if (target != user)
        {
            message.Append($" on {target.Name}");
        }

        message.Append('!');

        if (narratives.Count > 0)
        {
            message.Append(' ');
            message.Append(string.Join(" ", narratives));
        }

        return AbilityResult.Ok(
            message.ToString(),
            totalDamage,
            totalHealing,
            statusesApplied.Count > 0 ? statusesApplied : null);
    }

    /// <summary>
    /// Executes a DAMAGE command: DAMAGE:Type:Dice (e.g., "DAMAGE:Physical:2d6")
    /// </summary>
    private (int damage, string narrative) ExecuteDamageCommand(Combatant user, Combatant target, string[] parts)
    {
        if (parts.Length < 3)
        {
            _logger.LogWarning(
                "[Ability] DAMAGE command missing parameters: {Parts}",
                string.Join(":", parts));
            return (0, "");
        }

        var damageType = parts[1];
        var diceNotation = parts[2];

        // Parse dice notation (e.g., "2d6" -> count=2, sides=6)
        var match = DiceNotationRegex().Match(diceNotation);
        if (!match.Success)
        {
            _logger.LogWarning(
                "[Ability] Invalid dice notation: {Notation}",
                diceNotation);
            return (0, "");
        }

        var diceCount = int.Parse(match.Groups[1].Value);
        var diceSides = int.Parse(match.Groups[2].Value);

        // Roll damage
        var totalRoll = 0;
        for (var i = 0; i < diceCount; i++)
        {
            totalRoll += _diceService.RollSingle(diceSides, $"Ability damage ({diceNotation})");
        }

        // Apply vulnerability multiplier
        var multiplier = _statusEffectService.GetDamageMultiplier(target);
        var finalDamage = (int)(totalRoll * multiplier);

        // Apply armor soak (Physical damage is soaked)
        if (damageType.Equals("Physical", StringComparison.OrdinalIgnoreCase))
        {
            var soak = target.ArmorSoak + _statusEffectService.GetSoakModifier(target);
            finalDamage = Math.Max(0, finalDamage - soak);
        }

        // Apply damage to target
        target.CurrentHp -= finalDamage;

        _logger.LogInformation(
            "[Ability] DAMAGE: {User} deals {Damage} {Type} damage to {Target} (rolled {Roll}, soak applied)",
            user.Name, finalDamage, damageType, target.Name, totalRoll);

        var narrative = $"Deals {finalDamage} {damageType.ToLower()} damage.";
        return (finalDamage, narrative);
    }

    /// <summary>
    /// Executes a HEAL command: HEAL:Amount (e.g., "HEAL:15")
    /// </summary>
    private (int healing, string narrative) ExecuteHealCommand(Combatant target, string[] parts)
    {
        if (parts.Length < 2)
        {
            _logger.LogWarning(
                "[Ability] HEAL command missing amount parameter");
            return (0, "");
        }

        if (!int.TryParse(parts[1], out var amount))
        {
            _logger.LogWarning(
                "[Ability] HEAL command has invalid amount: {Amount}",
                parts[1]);
            return (0, "");
        }

        // Apply healing, clamped to max HP
        var actualHealing = Math.Min(amount, target.MaxHp - target.CurrentHp);
        target.CurrentHp += actualHealing;

        _logger.LogInformation(
            "[Ability] HEAL: {Target} healed for {Amount} HP (actual: {Actual})",
            target.Name, amount, actualHealing);

        var narrative = actualHealing > 0
            ? $"Restores {actualHealing} HP."
            : "HP is already full.";
        return (actualHealing, narrative);
    }

    /// <summary>
    /// Executes a STATUS command: STATUS:Type:Duration:Stacks (e.g., "STATUS:Bleeding:3:2")
    /// </summary>
    private (bool applied, string statusName, string narrative) ExecuteStatusCommand(
        Combatant user, Combatant target, string[] parts)
    {
        if (parts.Length < 3)
        {
            _logger.LogWarning(
                "[Ability] STATUS command missing parameters: {Parts}",
                string.Join(":", parts));
            return (false, "", "");
        }

        var statusTypeName = parts[1];
        if (!Enum.TryParse<StatusEffectType>(statusTypeName, true, out var statusType))
        {
            _logger.LogWarning(
                "[Ability] Unknown status effect type: {Type}",
                statusTypeName);
            return (false, statusTypeName, "");
        }

        if (!int.TryParse(parts[2], out var duration))
        {
            _logger.LogWarning(
                "[Ability] STATUS command has invalid duration: {Duration}",
                parts[2]);
            return (false, statusTypeName, "");
        }

        // Optional stacks parameter (default to 1)
        var stacks = 1;
        if (parts.Length >= 4 && int.TryParse(parts[3], out var parsedStacks))
        {
            stacks = parsedStacks;
        }

        // Apply the effect (stacks times if stackable)
        for (var i = 0; i < stacks; i++)
        {
            _statusEffectService.ApplyEffect(target, statusType, duration, user.Id);
        }

        _logger.LogInformation(
            "[Ability] STATUS: Applied {Type} to {Target} (Duration: {Duration}, Stacks: {Stacks})",
            statusType, target.Name, duration, stacks);

        var narrative = $"Applies {statusTypeName}.";
        return (true, statusTypeName, narrative);
    }

    #endregion
}
