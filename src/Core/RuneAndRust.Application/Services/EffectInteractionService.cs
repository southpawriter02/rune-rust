using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Handles interactions between status effects and damage types.
/// </summary>
public class EffectInteractionService
{
    private readonly IEffectInteractionRepository _repository;
    private readonly StatusEffectService _effectService;
    private readonly ILogger<EffectInteractionService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public EffectInteractionService(
        IEffectInteractionRepository repository,
        StatusEffectService effectService,
        ILogger<EffectInteractionService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _effectService = effectService ?? throw new ArgumentNullException(nameof(effectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
    }

    /// <summary>
    /// Processes interactions when damage is dealt to a target.
    /// </summary>
    public IReadOnlyList<EffectInteractionResult> ProcessDamageInteractions(
        IEffectTarget target,
        string damageType,
        int baseDamage)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(damageType);

        var results = new List<EffectInteractionResult>();

        foreach (var effect in target.ActiveEffects.Where(e => e.IsActive))
        {
            var interaction = _repository.GetForDamageType(effect.Definition.Id, damageType);
            if (interaction.HasValue)
            {
                var result = ExecuteInteraction(interaction.Value, target, baseDamage);
                results.Add(result);

                _logger.LogInformation(
                    "Effect interaction: {Effect} + {DamageType} on {Target} - {Message}",
                    effect.Definition.Name, damageType, target.Name, interaction.Value.Message);

                _eventLogger?.LogStatusEffect("EffectInteraction", $"{effect.Definition.Name} interacted with {damageType}",
                    data: new Dictionary<string, object>
                    {
                        ["effectName"] = effect.Definition.Name,
                        ["damageType"] = damageType,
                        ["targetName"] = target.Name,
                        ["message"] = interaction.Value.Message
                    });
            }
        }

        return results.AsReadOnly();
    }

    private EffectInteractionResult ExecuteInteraction(
        EffectInteraction interaction,
        IEffectTarget target,
        int baseDamage)
    {
        var appliedEffects = new List<string>();
        var removedEffects = new List<string>();
        var bonusDamage = 0;

        // Process bonus damage
        if (interaction.HasBonusDamage)
        {
            bonusDamage = (int)(baseDamage * interaction.BonusDamagePercent / 100f);
            target.TakeDamage(bonusDamage);
            _logger.LogDebug("Interaction dealt {Damage} bonus damage to {Target}",
                bonusDamage, target.Name);
        }

        // Apply new effect
        if (interaction.AppliesEffect)
        {
            var result = _effectService.ApplyEffect(interaction.ApplyEffect!, target);
            if (result.Applied)
                appliedEffects.Add(interaction.ApplyEffect!);
        }

        // Remove existing effect
        if (interaction.RemovesEffect)
        {
            var removed = target.RemoveEffectsByDefinition(interaction.RemoveEffect!);
            if (removed > 0)
                removedEffects.Add(interaction.RemoveEffect!);
        }

        return EffectInteractionResult.Full(
            interaction.Id,
            interaction.Message,
            bonusDamage,
            appliedEffects.AsReadOnly(),
            removedEffects.AsReadOnly());
    }
}
