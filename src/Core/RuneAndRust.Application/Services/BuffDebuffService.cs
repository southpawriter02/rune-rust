namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service for managing buffs and debuffs on targets.
/// </summary>
/// <remarks>
/// <para>
/// BuffDebuffService provides:
/// <list type="bullet">
///   <item><description>Typed ApplyResult responses</description></item>
///   <item><description>Category-based queries (buffs/debuffs)</description></item>
///   <item><description>Stack and duration queries</description></item>
/// </list>
/// </para>
/// </remarks>
public class BuffDebuffService : IBuffDebuffService
{
    private readonly IStatusEffectRepository _repository;
    private readonly ILogger<BuffDebuffService> _logger;

    /// <summary>
    /// Creates a new buff/debuff service.
    /// </summary>
    /// <param name="repository">Repository for effect definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public BuffDebuffService(
        IStatusEffectRepository repository,
        ILogger<BuffDebuffService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public ApplyResult ApplyEffect(IEffectTarget target, string effectId, Guid? sourceId = null, string? sourceName = null)
    {
        var definition = _repository.GetById(effectId);
        if (definition is null)
        {
            _logger.LogWarning("Effect not found: {EffectId}", effectId);
            return ApplyResult.Failed($"Effect '{effectId}' not found");
        }

        // Check immunity
        if (target.IsImmuneToEffect(effectId))
        {
            _logger.LogDebug("Target {Target} is immune to {Effect}", target.Name, effectId);
            return ApplyResult.Immune(effectId);
        }

        var existing = target.GetEffect(effectId);

        if (existing is not null)
        {
            return ProcessStacking(target, existing, definition);
        }

        // Apply new effect
        var newEffect = ActiveStatusEffect.Create(definition, sourceId, sourceName);
        target.AddEffect(newEffect);

        _logger.LogInformation("Applied {Effect} to {Target}", effectId, target.Name);
        return ApplyResult.Success(newEffect);
    }

    private ApplyResult ProcessStacking(IEffectTarget target, ActiveStatusEffect existing, StatusEffectDefinition definition)
    {
        switch (definition.StackingRule)
        {
            case StackingRule.RefreshDuration:
                existing.RefreshDuration();
                _logger.LogDebug("Refreshed {Effect} on {Target}", definition.Id, target.Name);
                return ApplyResult.Refreshed(existing);

            case StackingRule.Stack:
                if (existing.Stacks < definition.MaxStacks)
                {
                    existing.AddStacks(1);
                    existing.RefreshDuration();
                    _logger.LogDebug("Stacked {Effect} on {Target} ({Stacks}/{Max})",
                        definition.Id, target.Name, existing.Stacks, definition.MaxStacks);
                    return ApplyResult.Stacked(existing);
                }
                else
                {
                    existing.RefreshDuration();
                    return ApplyResult.AtMaxStacks(existing);
                }

            case StackingRule.Block:
                _logger.LogDebug("Blocked reapplication of {Effect} on {Target}", definition.Id, target.Name);
                return ApplyResult.Blocked(definition.Id);

            default:
                existing.RefreshDuration();
                return ApplyResult.Refreshed(existing);
        }
    }

    /// <inheritdoc />
    public bool RemoveEffect(IEffectTarget target, string effectId)
    {
        var removed = target.RemoveEffectsByDefinition(effectId);
        if (removed > 0)
        {
            _logger.LogInformation("Removed {Effect} from {Target}", effectId, target.Name);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public int ClearDebuffs(IEffectTarget target)
    {
        var debuffs = target.ActiveEffects
            .Where(e => e.Definition.Category == EffectCategory.Debuff)
            .ToList();

        int removed = 0;
        foreach (var effect in debuffs)
        {
            if (target.RemoveEffect(effect.Id))
                removed++;
        }

        _logger.LogDebug("Cleared {Count} debuffs from {Target}", removed, target.Name);
        return removed;
    }

    /// <inheritdoc />
    public int ClearBuffs(IEffectTarget target)
    {
        var buffs = target.ActiveEffects
            .Where(e => e.Definition.Category == EffectCategory.Buff)
            .ToList();

        int removed = 0;
        foreach (var effect in buffs)
        {
            if (target.RemoveEffect(effect.Id))
                removed++;
        }

        _logger.LogDebug("Cleared {Count} buffs from {Target}", removed, target.Name);
        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyList<ActiveStatusEffect> GetActiveEffects(IEffectTarget target)
        => target.ActiveEffects;

    /// <inheritdoc />
    public IReadOnlyList<ActiveStatusEffect> GetActiveEffects(IEffectTarget target, EffectCategory category)
        => target.ActiveEffects.Where(e => e.Definition.Category == category).ToList();

    /// <inheritdoc />
    public bool HasEffect(IEffectTarget target, string effectId)
        => target.HasEffect(effectId);

    /// <inheritdoc />
    public int GetStackCount(IEffectTarget target, string effectId)
        => target.GetEffect(effectId)?.Stacks ?? 0;

    /// <inheritdoc />
    public int? GetRemainingDuration(IEffectTarget target, string effectId)
        => target.GetEffect(effectId)?.RemainingDuration;
}
