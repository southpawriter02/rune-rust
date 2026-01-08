using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing status effects on targets.
/// </summary>
/// <remarks>
/// <para>StatusEffectService handles applying, removing, and ticking status effects.</para>
/// <para>It enforces stacking rules and immunity checks.</para>
/// </remarks>
public class StatusEffectService
{
    private readonly IStatusEffectRepository _repository;
    private readonly ILogger<StatusEffectService> _logger;

    /// <summary>
    /// Creates a new StatusEffectService instance.
    /// </summary>
    /// <param name="repository">Repository for effect definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public StatusEffectService(
        IStatusEffectRepository repository,
        ILogger<StatusEffectService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Applies a status effect to a target.
    /// </summary>
    /// <param name="effectId">The effect definition ID.</param>
    /// <param name="target">The target to apply the effect to.</param>
    /// <param name="sourceId">Optional source entity ID.</param>
    /// <param name="sourceName">Optional source entity name.</param>
    /// <returns>The result of the application attempt.</returns>
    public EffectApplicationResult ApplyEffect(
        string effectId,
        IEffectTarget target,
        Guid? sourceId = null,
        string? sourceName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(effectId);
        ArgumentNullException.ThrowIfNull(target);

        var normalizedId = effectId.ToLowerInvariant();

        // Get definition
        var definition = _repository.GetById(normalizedId);
        if (definition == null)
        {
            _logger.LogWarning("Effect not found: {EffectId}", normalizedId);
            return EffectApplicationResult.NotFound(normalizedId);
        }

        // Check immunity
        if (target.IsImmuneToEffect(normalizedId))
        {
            _logger.LogDebug("{Target} is immune to {Effect}", target.Name, definition.Name);
            return EffectApplicationResult.Immune(normalizedId, definition.Name);
        }

        // Check for existing effect
        var existing = target.GetEffect(normalizedId);
        if (existing != null)
        {
            return HandleExistingEffect(existing, definition, target);
        }

        // Apply new effect
        var newEffect = ActiveStatusEffect.Create(definition, sourceId, sourceName);
        target.AddEffect(newEffect);

        _logger.LogInformation(
            "{Effect} applied to {Target} for {Duration} turns",
            definition.Name,
            target.Name,
            newEffect.RemainingDuration);

        return EffectApplicationResult.Success(
            normalizedId,
            definition.Name,
            newEffect.Stacks,
            newEffect.RemainingDuration);
    }

    private EffectApplicationResult HandleExistingEffect(
        ActiveStatusEffect existing,
        StatusEffectDefinition definition,
        IEffectTarget target)
    {
        switch (definition.StackingRule)
        {
            case StackingRule.RefreshDuration:
                existing.RefreshDuration();
                _logger.LogDebug("{Effect} duration refreshed on {Target}",
                    definition.Name, target.Name);
                return EffectApplicationResult.Refreshed(
                    definition.Id,
                    definition.Name,
                    existing.RemainingDuration ?? 0);

            case StackingRule.Stack:
                if (existing.AddStacks(1))
                {
                    existing.RefreshDuration();
                    _logger.LogDebug("{Effect} stacked to {Stacks} on {Target}",
                        definition.Name, existing.Stacks, target.Name);
                    return EffectApplicationResult.Stacked(
                        definition.Id,
                        definition.Name,
                        existing.Stacks,
                        existing.RemainingDuration);
                }
                // At max stacks, just refresh duration
                existing.RefreshDuration();
                return EffectApplicationResult.Refreshed(
                    definition.Id,
                    definition.Name,
                    existing.RemainingDuration ?? 0);

            case StackingRule.Block:
            default:
                _logger.LogDebug("{Effect} blocked on {Target} (already active)",
                    definition.Name, target.Name);
                return EffectApplicationResult.Blocked(definition.Id, definition.Name);
        }
    }

    /// <summary>
    /// Ticks all effects on a target at the start of their turn.
    /// </summary>
    /// <param name="target">The target whose effects to tick.</param>
    /// <returns>List of tick results for DoT/HoT and expirations.</returns>
    public IReadOnlyList<EffectTickResult> TickEffects(IEffectTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var results = new List<EffectTickResult>();
        var expiredEffects = new List<Guid>();

        foreach (var effect in target.ActiveEffects)
        {
            // Process DoT
            var damage = effect.CalculateDamagePerTurn();
            if (damage > 0)
            {
                var actualDamage = target.TakeDamage(damage);
                _logger.LogDebug("{Effect} deals {Damage} damage to {Target}",
                    effect.Definition.Name, actualDamage, target.Name);
            }

            // Process HoT
            var healing = effect.CalculateHealingPerTurn();
            if (healing > 0)
            {
                var actualHealing = target.Heal(healing);
                _logger.LogDebug("{Effect} heals {Target} for {Healing}",
                    effect.Definition.Name, target.Name, actualHealing);
            }

            // Tick duration
            var stillActive = effect.TickDuration();

            // Create result
            if (damage > 0)
            {
                results.Add(EffectTickResult.WithDamage(
                    effect.Definition.Id,
                    effect.Definition.Name,
                    damage,
                    !stillActive,
                    effect.RemainingDuration));
            }
            else if (healing > 0)
            {
                results.Add(EffectTickResult.WithHealing(
                    effect.Definition.Id,
                    effect.Definition.Name,
                    healing,
                    !stillActive,
                    effect.RemainingDuration));
            }
            else if (!stillActive)
            {
                results.Add(EffectTickResult.Ticked(
                    effect.Definition.Id,
                    effect.Definition.Name,
                    true,
                    0));
            }

            if (!stillActive)
            {
                expiredEffects.Add(effect.Id);
                _logger.LogDebug("{Effect} expired on {Target}",
                    effect.Definition.Name, target.Name);
            }
        }

        // Remove expired effects
        foreach (var effectId in expiredEffects)
        {
            target.RemoveEffect(effectId);
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Removes an effect by its definition ID.
    /// </summary>
    /// <param name="effectId">The effect definition ID.</param>
    /// <param name="target">The target to remove the effect from.</param>
    /// <returns>Number of effects removed.</returns>
    public int RemoveEffect(string effectId, IEffectTarget target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(effectId);
        ArgumentNullException.ThrowIfNull(target);

        var normalizedId = effectId.ToLowerInvariant();
        var removed = target.RemoveEffectsByDefinition(normalizedId);

        if (removed > 0)
        {
            _logger.LogDebug("Removed {Count} {Effect} effect(s) from {Target}",
                removed, normalizedId, target.Name);
        }

        return removed;
    }

    /// <summary>
    /// Removes all debuffs from a target (cleanse).
    /// </summary>
    /// <param name="target">The target to cleanse.</param>
    /// <returns>Number of effects removed.</returns>
    public int Cleanse(IEffectTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var debuffs = target.ActiveEffects
            .Where(e => e.Definition.Category == EffectCategory.Debuff)
            .Select(e => e.Id)
            .ToList();

        foreach (var id in debuffs)
        {
            target.RemoveEffect(id);
        }

        if (debuffs.Count > 0)
        {
            _logger.LogDebug("Cleansed {Count} debuff(s) from {Target}",
                debuffs.Count, target.Name);
        }

        return debuffs.Count;
    }

    /// <summary>
    /// Gets an effect definition by ID.
    /// </summary>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>The definition, or null if not found.</returns>
    public StatusEffectDefinition? GetEffectDefinition(string effectId)
    {
        return _repository.GetById(effectId.ToLowerInvariant());
    }

    /// <summary>
    /// Gets all effect definitions.
    /// </summary>
    /// <returns>All registered effect definitions.</returns>
    public IEnumerable<StatusEffectDefinition> GetAllEffects()
    {
        return _repository.GetAll();
    }
}
