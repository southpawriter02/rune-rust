using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages status effect lifecycle: application, ticking, and removal.
/// Provides comprehensive logging for all effect operations.
/// </summary>
public class StatusEffectService : IStatusEffectService
{
    private readonly IDiceService _dice;
    private readonly ILogger<StatusEffectService> _logger;

    /// <summary>
    /// Soak bonus per stack of Fortified.
    /// </summary>
    private const int FortifiedSoakPerStack = 2;

    /// <summary>
    /// Damage multiplier when Vulnerable is active.
    /// </summary>
    private const float VulnerableDamageMultiplier = 1.5f;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusEffectService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for DoT damage rolls.</param>
    /// <param name="logger">The logger for traceability.</param>
    public StatusEffectService(IDiceService dice, ILogger<StatusEffectService> logger)
    {
        _dice = dice;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void ApplyEffect(Combatant target, StatusEffectType type, int duration, Guid sourceId)
    {
        _logger.LogTrace(
            "ApplyEffect called: {Type} on {Target}, Duration: {Duration}, Source: {Source}",
            type, target.Name, duration, sourceId);

        var existing = target.StatusEffects.FirstOrDefault(e => e.Type == type);

        if (existing != null)
        {
            // Effect already exists - stack or refresh
            if (ActiveStatusEffect.CanStack(type))
            {
                var maxStacks = ActiveStatusEffect.GetMaxStacks(type);
                var oldStacks = existing.Stacks;

                if (existing.Stacks < maxStacks)
                {
                    existing.Stacks++;
                    _logger.LogDebug(
                        "Stacked {Type} on {Target}: {OldStacks} -> {NewStacks}",
                        type, target.Name, oldStacks, existing.Stacks);
                }

                // Always refresh duration on reapplication
                existing.DurationRemaining = duration;
                _logger.LogDebug(
                    "Refreshed {Type} duration on {Target}: {Duration} turns",
                    type, target.Name, duration);
            }
            else
            {
                // Non-stackable: just refresh duration
                existing.DurationRemaining = duration;
                _logger.LogDebug(
                    "Refreshed {Type} duration on {Target}: {Duration} turns",
                    type, target.Name, duration);
            }
        }
        else
        {
            // New effect
            var effect = new ActiveStatusEffect
            {
                Type = type,
                Stacks = 1,
                DurationRemaining = duration,
                SourceId = sourceId
            };

            target.StatusEffects.Add(effect);

            _logger.LogInformation(
                "Applied {Type} to {Target} (Stacks: {Stacks}, Duration: {Duration})",
                type, target.Name, effect.Stacks, duration);
        }
    }

    /// <inheritdoc/>
    public void RemoveEffect(Combatant target, StatusEffectType type)
    {
        var removed = target.StatusEffects.RemoveAll(e => e.Type == type);

        if (removed > 0)
        {
            _logger.LogInformation(
                "Removed {Type} from {Target}",
                type, target.Name);
        }
        else
        {
            _logger.LogTrace(
                "RemoveEffect: {Type} not found on {Target}",
                type, target.Name);
        }
    }

    /// <inheritdoc/>
    public void ClearAllEffects(Combatant target)
    {
        var count = target.StatusEffects.Count;
        target.StatusEffects.Clear();

        if (count > 0)
        {
            _logger.LogDebug(
                "Cleared {Count} status effects from {Target}",
                count, target.Name);
        }
    }

    /// <inheritdoc/>
    public int ProcessTurnStart(Combatant combatant)
    {
        _logger.LogTrace(
            "ProcessTurnStart for {Name} with {Count} effects",
            combatant.Name, combatant.StatusEffects.Count);

        var totalDamage = 0;

        // Process Bleeding (ignores soak)
        var bleeding = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Bleeding);
        if (bleeding != null)
        {
            var damage = 0;
            for (var i = 0; i < bleeding.Stacks; i++)
            {
                damage += _dice.RollSingle(6, $"Bleeding Tick ({i + 1}/{bleeding.Stacks})");
            }

            totalDamage += damage;

            _logger.LogInformation(
                "{Target} takes {Damage} Bleeding damage (Stacks: {Stacks})",
                combatant.Name, damage, bleeding.Stacks);
        }

        // Process Poisoned (applies soak)
        var poisoned = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Poisoned);
        if (poisoned != null)
        {
            var rawDamage = 0;
            for (var i = 0; i < poisoned.Stacks; i++)
            {
                rawDamage += _dice.RollSingle(6, $"Poison Tick ({i + 1}/{poisoned.Stacks})");
            }

            // Apply soak (minimum 1 damage if any was dealt)
            var soak = combatant.ArmorSoak + GetSoakModifier(combatant);
            var finalDamage = rawDamage > 0 ? Math.Max(1, rawDamage - soak) : 0;

            totalDamage += finalDamage;

            _logger.LogInformation(
                "{Target} takes {Damage} Poisoned damage (Stacks: {Stacks})",
                combatant.Name, finalDamage, poisoned.Stacks);

            if (rawDamage != finalDamage)
            {
                _logger.LogDebug(
                    "Poisoned damage reduced by soak: {Raw} - {Soak} = {Final}",
                    rawDamage, soak, finalDamage);
            }
        }

        return totalDamage;
    }

    /// <inheritdoc/>
    public bool CanAct(Combatant combatant)
    {
        var stunned = HasEffect(combatant, StatusEffectType.Stunned);

        if (stunned)
        {
            _logger.LogInformation(
                "{Target} is stunned and loses their turn",
                combatant.Name);
        }

        return !stunned;
    }

    /// <inheritdoc/>
    public void ProcessTurnEnd(Combatant combatant)
    {
        _logger.LogTrace(
            "ProcessTurnEnd for {Name} with {Count} effects",
            combatant.Name, combatant.StatusEffects.Count);

        var expiredEffects = new List<StatusEffectType>();

        foreach (var effect in combatant.StatusEffects)
        {
            effect.DurationRemaining--;

            if (effect.DurationRemaining <= 0)
            {
                expiredEffects.Add(effect.Type);
            }
            else
            {
                _logger.LogTrace(
                    "{Type} on {Target}: {Duration} turns remaining",
                    effect.Type, combatant.Name, effect.DurationRemaining);
            }
        }

        // Remove expired effects
        foreach (var type in expiredEffects)
        {
            combatant.StatusEffects.RemoveAll(e => e.Type == type);

            _logger.LogInformation(
                "{Type} expired on {Target}",
                type, combatant.Name);
        }
    }

    /// <inheritdoc/>
    public int GetSoakModifier(Combatant combatant)
    {
        var fortified = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Fortified);

        if (fortified == null)
        {
            return 0;
        }

        var modifier = fortified.Stacks * FortifiedSoakPerStack;

        _logger.LogDebug(
            "Soak modifier for {Target}: +{Modifier} from Fortified (×{Stacks})",
            combatant.Name, modifier, fortified.Stacks);

        return modifier;
    }

    /// <inheritdoc/>
    public float GetDamageMultiplier(Combatant combatant)
    {
        if (HasEffect(combatant, StatusEffectType.Vulnerable))
        {
            _logger.LogDebug(
                "Damage multiplier for {Target}: ×{Multiplier} from Vulnerable",
                combatant.Name, VulnerableDamageMultiplier);

            return VulnerableDamageMultiplier;
        }

        return 1.0f;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ActiveStatusEffect> GetActiveEffects(Combatant combatant)
    {
        return combatant.StatusEffects.AsReadOnly();
    }

    /// <inheritdoc/>
    public bool HasEffect(Combatant combatant, StatusEffectType type)
    {
        return combatant.StatusEffects.Any(e => e.Type == type);
    }

    /// <inheritdoc/>
    public int GetEffectStacks(Combatant combatant, StatusEffectType type)
    {
        var effect = combatant.StatusEffects.FirstOrDefault(e => e.Type == type);
        return effect?.Stacks ?? 0;
    }
}
