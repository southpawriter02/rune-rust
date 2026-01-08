using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for removing status effects through cleansing.
/// </summary>
public class CleanseService
{
    private readonly StatusEffectService _effectService;
    private readonly ILogger<CleanseService> _logger;

    private static readonly HashSet<string> PhysicalEffects = new()
    {
        "bleeding", "poisoned", "exhausted"
    };

    private static readonly HashSet<string> MagicalEffects = new()
    {
        "cursed", "silenced", "feared", "weakened"
    };

    private static readonly HashSet<string> ElementalEffects = new()
    {
        "burning", "frozen", "wet", "chilled", "electrified", "on-fire"
    };

    public CleanseService(
        StatusEffectService effectService,
        ILogger<CleanseService> logger)
    {
        _effectService = effectService ?? throw new ArgumentNullException(nameof(effectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cleanses effects from a target using a cleanse item.
    /// </summary>
    public CleanseResult Cleanse(
        IEffectTarget target,
        CleanseItem item)
    {
        return Cleanse(target, item.CleanseType, item.SpecificEffect);
    }

    /// <summary>
    /// Cleanses effects from a target.
    /// </summary>
    public CleanseResult Cleanse(
        IEffectTarget target,
        CleanseType cleanseType,
        string? specificEffectId = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        var removedEffects = cleanseType switch
        {
            CleanseType.AllNegative => RemoveByPredicate(target,
                e => e.Definition.Category == EffectCategory.Debuff),
            CleanseType.AllPositive => RemoveByPredicate(target,
                e => e.Definition.Category == EffectCategory.Buff),
            CleanseType.Physical => RemoveByPredicate(target,
                e => PhysicalEffects.Contains(e.Definition.Id)),
            CleanseType.Magical => RemoveByPredicate(target,
                e => MagicalEffects.Contains(e.Definition.Id)),
            CleanseType.Elemental => RemoveByPredicate(target,
                e => ElementalEffects.Contains(e.Definition.Id)),
            CleanseType.Specific when specificEffectId != null =>
                RemoveSpecific(target, specificEffectId),
            _ => new List<string>()
        };

        if (removedEffects.Count > 0)
        {
            _logger.LogInformation(
                "Cleansed {Count} effects from {Target}: {Effects}",
                removedEffects.Count, target.Name, string.Join(", ", removedEffects));
        }

        return removedEffects.Count > 0
            ? CleanseResult.Succeeded(removedEffects.AsReadOnly())
            : CleanseResult.NoEffectsRemoved();
    }

    private List<string> RemoveByPredicate(
        IEffectTarget target,
        Func<ActiveStatusEffect, bool> predicate)
    {
        var toRemove = target.ActiveEffects
            .Where(e => e.IsActive && predicate(e))
            .Select(e => e.Definition.Id)
            .ToList();

        foreach (var effectId in toRemove)
        {
            target.RemoveEffectsByDefinition(effectId);
        }

        return toRemove;
    }

    private List<string> RemoveSpecific(IEffectTarget target, string effectId)
    {
        var removed = target.RemoveEffectsByDefinition(effectId.ToLowerInvariant());
        return removed > 0 ? new List<string> { effectId } : new List<string>();
    }
}
