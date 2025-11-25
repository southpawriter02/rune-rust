using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.RegularExpressions;
using StatusEffectDefinition = RuneAndRust.Core.StatusEffectDefinition;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.21.3: Advanced Status Effect Service
/// Manages status effects with stacking, interactions, and cascade behaviors
/// </summary>
public class AdvancedStatusEffectService
{
    private static readonly ILogger _log = Log.ForContext<AdvancedStatusEffectService>();
    private readonly StatusEffectRepository _repository;
    private readonly TraumaEconomyService _traumaService;
    private readonly DiceService _diceService;
    private readonly List<StatusInteraction> _interactions;
    private readonly StatusEffectFlavorTextService? _flavorTextService;

    public AdvancedStatusEffectService(
        StatusEffectRepository repository,
        TraumaEconomyService traumaService,
        DiceService diceService,
        StatusEffectFlavorTextService? flavorTextService = null)
    {
        _repository = repository;
        _traumaService = traumaService;
        _diceService = diceService;
        _flavorTextService = flavorTextService;
        _interactions = _repository.GetAllInteractions();

        _log.Information("AdvancedStatusEffectService initialized with {InteractionCount} interactions (Flavor text: {FlavorEnabled})",
            _interactions.Count, flavorTextService != null);
    }

    #region Stacking Management

    /// <summary>
    /// Apply a status effect with stacking support
    /// </summary>
    public StatusApplicationResult ApplyEffect(
        int targetId,
        string effectType,
        int stacks = 1,
        int? duration = null,
        int appliedBy = 0)
    {
        var result = new StatusApplicationResult
        {
            EffectType = effectType,
            Success = false
        };

        // Get effect definition
        var definition = StatusEffectDefinition.GetDefinition(effectType);
        if (definition == null)
        {
            _log.Warning("Unknown status effect type: {EffectType}", effectType);
            result.Message = $"Unknown effect type: {effectType}";
            return result;
        }

        _log.Debug("Applying {EffectType} to target {TargetId} (stacks: {Stacks}, duration: {Duration})",
            effectType, targetId, stacks, duration ?? definition.DefaultDuration);

        // Check for suppression first
        if (CheckSuppression(targetId, effectType))
        {
            result.Message = $"{effectType} suppressed by existing effect";
            _log.Information("{EffectType} suppressed for target {TargetId}", effectType, targetId);
            return result;
        }

        // Check for conversion interactions (e.g., Disoriented → Stunned)
        var conversionResult = CheckConversion(targetId, effectType);
        if (conversionResult.ConversionTriggered)
        {
            result.ConversionTriggered = true;
            result.ConvertedTo = conversionResult.ConvertedTo;
            result.Success = true;
            result.Message = $"{effectType} converted to {conversionResult.ConvertedTo}!";
            return result;
        }

        // Check if effect already exists on target
        var existingEffect = _repository.GetActiveEffect(targetId, effectType);

        if (existingEffect != null && definition.CanStack)
        {
            // Stack the effect
            var newStackCount = Math.Min(existingEffect.StackCount + stacks, definition.MaxStacks);
            _repository.UpdateEffectStacks(existingEffect.EffectInstanceID, newStackCount);

            result.Success = true;
            result.CurrentStacks = newStackCount;

            // Use flavor text if available, otherwise fallback
            if (_flavorTextService != null)
            {
                result.Message = $"{definition.DisplayName} stacked! ({newStackCount} stacks)";
            }
            else
            {
                result.Message = $"{definition.DisplayName} stacked! ({newStackCount} stacks)";
            }

            _log.Information("{EffectType} stacked on target {TargetId}: {StackCount}/{MaxStacks}",
                effectType, targetId, newStackCount, definition.MaxStacks);

            // Check for critical stack stress (Trauma Economy integration)
            if (newStackCount >= definition.MaxStacks)
            {
                // Note: Stress tracking requires PlayerCharacter object, handled at higher level
                // _traumaService.AddStress(playerCharacter, 10, $"Critical {effectType} stacks reached");
                result.Message += " [CRITICAL STACKS - System Failure Imminent!]";
                _log.Warning("Critical stacks reached for {EffectType} on target {TargetId}", effectType, targetId);
            }
        }
        else if (existingEffect != null && !definition.CanStack)
        {
            // Refresh duration (non-stacking effects)
            _repository.RemoveEffect(targetId, effectType);
            var newEffect = CreateStatusEffect(targetId, definition, duration ?? definition.DefaultDuration, appliedBy);
            _repository.ApplyEffect(newEffect);

            result.Success = true;
            result.CurrentStacks = 1;
            result.Message = $"{definition.DisplayName} refreshed!";

            _log.Information("{EffectType} refreshed on target {TargetId}", effectType, targetId);
        }
        else
        {
            // Apply new effect
            var newEffect = CreateStatusEffect(targetId, definition, duration ?? definition.DefaultDuration, appliedBy, stacks);
            var effectId = _repository.ApplyEffect(newEffect);

            if (effectId > 0)
            {
                result.Success = true;
                result.CurrentStacks = stacks;

                // Use flavor text if available, otherwise fallback
                if (_flavorTextService != null)
                {
                    // Note: Flavor text requires target name, which is not available here
                    // This will be enhanced when called from CombatEngine with target context
                    result.Message = $"{definition.DisplayName} applied!";
                }
                else
                {
                    result.Message = $"{definition.DisplayName} applied!";
                }

                _log.Information("{EffectType} applied to target {TargetId} (stacks: {Stacks}, duration: {Duration})",
                    effectType, targetId, stacks, duration ?? definition.DefaultDuration);

                // Apply stress for new debuff (Trauma Economy integration)
                // Note: Stress tracking requires PlayerCharacter object, handled at higher level
                if (definition.Category == StatusEffectCategory.ControlDebuff ||
                    definition.Category == StatusEffectCategory.DamageOverTime)
                {
                    var debuffCount = GetDebuffCount(targetId);
                    if (debuffCount > 1)
                    {
                        // +2 stress per debuff beyond first (tracked externally)
                        _log.Debug("Multiple debuffs active ({DebuffCount}) for target {TargetId}", debuffCount, targetId);
                    }

                    // Special stress for control effects (tracked externally)
                    if (effectType == "Stunned")
                    {
                        _log.Information("Stunned effect applied to target {TargetId} - potential stress trigger", targetId);
                    }
                    else if (effectType == "Rooted" || effectType == "Feared")
                    {
                        _log.Information("{EffectType} effect applied to target {TargetId} - potential stress trigger", effectType, targetId);
                    }
                }
            }
        }

        // Check for amplification interactions
        result.ActiveInteractions = GetActiveAmplifications(targetId, effectType);

        return result;
    }

    private StatusEffect CreateStatusEffect(
        int targetId,
        StatusEffectDefinition definition,
        int duration,
        int appliedBy,
        int stacks = 1)
    {
        return new StatusEffect
        {
            TargetID = targetId,
            EffectType = definition.EffectType,
            StackCount = stacks,
            DurationRemaining = duration,
            AppliedBy = appliedBy,
            AppliedAt = DateTime.UtcNow,
            Category = definition.Category,
            CanStack = definition.CanStack,
            MaxStacks = definition.MaxStacks,
            IgnoresSoak = definition.IgnoresSoak,
            DamageBase = definition.DamageBase
        };
    }

    /// <summary>
    /// Get current stack count for an effect
    /// </summary>
    public int GetStackCount(int targetId, string effectType)
    {
        var effect = _repository.GetActiveEffect(targetId, effectType);
        return effect?.StackCount ?? 0;
    }

    /// <summary>
    /// Check if effect can stack
    /// </summary>
    public bool CanStack(string effectType)
    {
        var definition = StatusEffectDefinition.GetDefinition(effectType);
        return definition?.CanStack ?? false;
    }

    /// <summary>
    /// Get maximum stacks for an effect
    /// </summary>
    public int GetMaxStacks(string effectType)
    {
        var definition = StatusEffectDefinition.GetDefinition(effectType);
        return definition?.MaxStacks ?? 1;
    }

    /// <summary>
    /// Check if target has a specific effect
    /// </summary>
    public bool HasEffect(int targetId, string effectType)
    {
        var effect = _repository.GetActiveEffect(targetId, effectType);
        return effect != null && effect.DurationRemaining > 0;
    }

    #endregion

    #region Interaction Resolution

    /// <summary>
    /// Check for conversion interactions (e.g., Disoriented → Stunned)
    /// </summary>
    public StatusApplicationResult CheckConversion(int targetId, string effectType)
    {
        var result = new StatusApplicationResult
        {
            EffectType = effectType,
            ConversionTriggered = false
        };

        // Find conversion interactions for this effect
        var conversions = _interactions
            .Where(i => i.InteractionType == StatusInteractionType.Conversion &&
                       i.PrimaryEffect == effectType)
            .ToList();

        foreach (var conversion in conversions)
        {
            // Check if effect is already active (multiple applications)
            var existingEffect = _repository.GetActiveEffect(targetId, effectType);

            if (existingEffect != null)
            {
                // We're applying the effect again - check if we hit conversion threshold
                // For Disoriented: 1 existing + 1 new = 2 applications → convert to Stunned
                if (conversion.RequiredApplications == 2)
                {
                    // Remove the existing effect
                    _repository.RemoveEffect(targetId, effectType);

                    // Apply the conversion result
                    var conversionDefinition = StatusEffectDefinition.GetDefinition(conversion.ResultEffect!);
                    if (conversionDefinition != null)
                    {
                        var newEffect = CreateStatusEffect(
                            targetId,
                            conversionDefinition,
                            conversion.ResultDuration,
                            0);

                        _repository.ApplyEffect(newEffect);

                        result.ConversionTriggered = true;
                        result.ConvertedTo = conversion.ResultEffect;
                        result.Success = true;

                        _log.Warning("{EffectType} converted to {ConvertedTo} for target {TargetId}",
                            effectType, conversion.ResultEffect, targetId);

                        return result;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate amplification multiplier for an effect
    /// </summary>
    public float CalculateAmplificationMultiplier(int targetId, string effectType)
    {
        float totalMultiplier = 1.0f;

        // Find amplification interactions
        var amplifications = _interactions
            .Where(i => i.InteractionType == StatusInteractionType.Amplification &&
                       i.PrimaryEffect == effectType)
            .ToList();

        foreach (var amp in amplifications)
        {
            // Check if secondary effect is present
            if (amp.SecondaryEffect != null && HasEffect(targetId, amp.SecondaryEffect))
            {
                totalMultiplier *= amp.Multiplier;
                _log.Debug("{PrimaryEffect} amplified by {SecondaryEffect}: ×{Multiplier}",
                    effectType, amp.SecondaryEffect, amp.Multiplier);
            }
        }

        return totalMultiplier;
    }

    /// <summary>
    /// Check for suppression (effects that cancel each other)
    /// </summary>
    public bool CheckSuppression(int targetId, string newEffect)
    {
        var activeEffects = _repository.GetActiveEffects(targetId);

        // Find suppression interactions
        var suppressions = _interactions
            .Where(i => i.InteractionType == StatusInteractionType.Suppression &&
                       (i.PrimaryEffect == newEffect || i.SecondaryEffect == newEffect))
            .ToList();

        foreach (var suppression in suppressions)
        {
            string? oppositeEffect = null;

            if (suppression.PrimaryEffect == newEffect)
            {
                oppositeEffect = suppression.SecondaryEffect;
            }
            else if (suppression.SecondaryEffect == newEffect)
            {
                oppositeEffect = suppression.PrimaryEffect;
            }

            if (oppositeEffect != null && HasEffect(targetId, oppositeEffect))
            {
                // Cancel both effects
                _repository.RemoveEffect(targetId, oppositeEffect);
                _log.Information("{NewEffect} and {OppositeEffect} canceled each other for target {TargetId}",
                    newEffect, oppositeEffect, targetId);

                return true; // Suppression occurred
            }
        }

        return false; // No suppression
    }

    /// <summary>
    /// Get active amplifications for display
    /// </summary>
    private List<string> GetActiveAmplifications(int targetId, string effectType)
    {
        var amplifications = new List<string>();

        var amps = _interactions
            .Where(i => i.InteractionType == StatusInteractionType.Amplification &&
                       i.PrimaryEffect == effectType)
            .ToList();

        foreach (var amp in amps)
        {
            if (amp.SecondaryEffect != null && HasEffect(targetId, amp.SecondaryEffect))
            {
                amplifications.Add($"Amplified by [{amp.SecondaryEffect}]: +{(int)((amp.Multiplier - 1.0f) * 100)}% damage");
            }
        }

        return amplifications;
    }

    /// <summary>
    /// Get count of active debuffs (for stress calculation)
    /// </summary>
    private int GetDebuffCount(int targetId)
    {
        var effects = _repository.GetActiveEffects(targetId);
        return effects.Count(e => e.Category == StatusEffectCategory.ControlDebuff ||
                                 e.Category == StatusEffectCategory.DamageOverTime);
    }

    #endregion

    #region Turn Processing & DoT

    /// <summary>
    /// Process start of turn effects (DoT damage)
    /// </summary>
    public async Task<List<string>> ProcessStartOfTurn(int targetId, Enemy? enemy = null, PlayerCharacter? player = null)
    {
        var logMessages = new List<string>();
        var effects = _repository.GetActiveEffects(targetId);

        foreach (var effect in effects)
        {
            // Process Damage Over Time effects
            if (effect.Category == StatusEffectCategory.DamageOverTime && effect.DamageBase != null)
            {
                var damage = CalculateDotDamage(effect, targetId);

                if (damage > 0)
                {
                    string targetName = enemy?.Name ?? player?.Name ?? $"Target {targetId}";
                    bool isPlayer = player != null;

                    // Apply damage (respecting IgnoresSoak flag)
                    if (enemy != null)
                    {
                        ApplyDamageToEnemy(enemy, damage, effect.IgnoresSoak);
                    }
                    else if (player != null)
                    {
                        ApplyDamageToPlayer(player, damage, effect.IgnoresSoak);
                    }

                    // Use flavor text if available
                    string message;
                    if (_flavorTextService != null)
                    {
                        message = _flavorTextService.GenerateTickText(
                            effectType: effect.EffectType,
                            targetName: targetName,
                            damageAmount: damage,
                            stackCount: effect.StackCount,
                            biomeName: null, // TODO: Pass biome context
                            isPlayer: isPlayer);

                        // Append technical info
                        if (effect.IgnoresSoak)
                        {
                            message += " [Ignores Soak]";
                        }
                    }
                    else
                    {
                        // Fallback to simple message
                        message = $"{targetName} takes {damage} {effect.EffectType} damage!";
                        if (effect.StackCount > 1)
                        {
                            message += $" ({effect.StackCount} stacks)";
                        }
                        if (effect.IgnoresSoak)
                        {
                            message += " [Ignores Soak]";
                        }
                    }

                    logMessages.Add(message);
                    _log.Information("{Message}", message);
                }
            }
        }

        return logMessages;
    }

    /// <summary>
    /// Process end of turn effects (duration decay, Corroded damage)
    /// </summary>
    public async Task<List<string>> ProcessEndOfTurn(int targetId, Enemy? enemy = null, PlayerCharacter? player = null)
    {
        var logMessages = new List<string>();
        var effects = _repository.GetActiveEffects(targetId);

        // Process Corroded (end-of-turn damage)
        var corrodedEffect = effects.FirstOrDefault(e => e.EffectType == "Corroded");
        if (corrodedEffect != null && corrodedEffect.DamageBase != null)
        {
            var damage = CalculateDotDamage(corrodedEffect, targetId);
            if (damage > 0)
            {
                string targetName = enemy?.Name ?? player?.Name ?? $"Target {targetId}";

                if (enemy != null)
                {
                    ApplyDamageToEnemy(enemy, damage, corrodedEffect.IgnoresSoak);
                }
                else if (player != null)
                {
                    ApplyDamageToPlayer(player, damage, corrodedEffect.IgnoresSoak);
                }

                logMessages.Add($"{targetName} takes {damage} Corrosion damage! ({corrodedEffect.StackCount} stacks)");
            }
        }

        // Decrement durations
        _repository.DecrementDurations(targetId);

        // Check for expired effects
        var remainingEffects = _repository.GetActiveEffects(targetId);
        var expiredEffects = effects.Where(e => !remainingEffects.Any(r => r.EffectInstanceID == e.EffectInstanceID)).ToList();

        foreach (var expired in expiredEffects)
        {
            string targetName = enemy?.Name ?? player?.Name ?? $"Target {targetId}";
            bool isPlayer = player != null;

            // Use flavor text if available
            string message;
            if (_flavorTextService != null)
            {
                message = _flavorTextService.GenerateEndText(
                    effectType: expired.EffectType,
                    targetName: targetName,
                    wasRemoved: false, // Natural expiration
                    removalMethod: null,
                    isCatastrophic: false,
                    isPlayer: isPlayer);
            }
            else
            {
                // Fallback to simple message
                message = $"{targetName} is no longer {expired.EffectType}.";
            }

            logMessages.Add(message);
        }

        return logMessages;
    }

    /// <summary>
    /// Calculate DoT damage with amplifications
    /// </summary>
    private int CalculateDotDamage(StatusEffect effect, int targetId)
    {
        if (effect.DamageBase == null) return 0;

        // Parse damage base (e.g., "1d6")
        var match = Regex.Match(effect.DamageBase, @"(\d+)d(\d+)");
        if (!match.Success) return 0;

        int numDice = int.Parse(match.Groups[1].Value);
        int dieSize = int.Parse(match.Groups[2].Value);

        // Roll damage per stack using DiceService.Roll(numDice, dieSize)
        int totalDamage = 0;
        for (int i = 0; i < effect.StackCount; i++)
        {
            totalDamage += _diceService.Roll(numDice, dieSize);
        }

        // Apply amplification multiplier
        float multiplier = CalculateAmplificationMultiplier(targetId, effect.EffectType);
        int finalDamage = (int)(totalDamage * multiplier);

        if (multiplier > 1.0f)
        {
            _log.Debug("{EffectType} damage amplified: {BaseDamage} → {FinalDamage} (×{Multiplier})",
                effect.EffectType, totalDamage, finalDamage, multiplier);
        }

        return finalDamage;
    }

    private void ApplyDamageToEnemy(Enemy enemy, int damage, bool ignoresSoak)
    {
        if (!ignoresSoak && enemy.Soak > 0)
        {
            damage = Math.Max(0, damage - enemy.Soak);
        }

        enemy.HP = Math.Max(0, enemy.HP - damage);
    }

    private void ApplyDamageToPlayer(PlayerCharacter player, int damage, bool ignoresSoak)
    {
        // TODO: Implement Soak for player if needed
        player.HP = Math.Max(0, player.HP - damage);
    }

    #endregion

    #region Effect Management

    /// <summary>
    /// Remove a specific effect from target
    /// </summary>
    public void RemoveEffect(int targetId, string effectType)
    {
        _repository.RemoveEffect(targetId, effectType);
        _log.Information("Removed {EffectType} from target {TargetId}", effectType, targetId);
    }

    /// <summary>
    /// Remove all effects from target
    /// </summary>
    public void RemoveAllEffects(int targetId)
    {
        _repository.RemoveAllEffects(targetId);
        _log.Information("Removed all effects from target {TargetId}", targetId);
    }

    /// <summary>
    /// Get all active effects for a target
    /// </summary>
    public List<StatusEffect> GetActiveEffects(int targetId)
    {
        return _repository.GetActiveEffects(targetId);
    }

    /// <summary>
    /// Get a specific effect for a target
    /// </summary>
    public StatusEffect? GetActiveEffect(int targetId, string effectType)
    {
        return _repository.GetActiveEffect(targetId, effectType);
    }

    /// <summary>
    /// Calculate stress from multiple debuffs (Trauma Economy integration)
    /// </summary>
    public int CalculateStressFromEffects(int targetId)
    {
        var debuffCount = GetDebuffCount(targetId);
        if (debuffCount <= 1) return 0;

        // +2 stress per debuff beyond first
        return (debuffCount - 1) * 2;
    }

    #endregion
}
