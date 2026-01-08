using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository for loading effect interaction definitions.
/// </summary>
public interface IEffectInteractionRepository
{
    /// <summary>Gets all defined effect interactions.</summary>
    IReadOnlyList<EffectInteraction> GetAll();

    /// <summary>Gets interactions triggered by a specific existing effect.</summary>
    /// <param name="triggerEffectId">The effect ID on the target.</param>
    IReadOnlyList<EffectInteraction> GetByTriggerEffect(string triggerEffectId);

    /// <summary>Gets a specific interaction by ID.</summary>
    /// <param name="interactionId">The interaction ID.</param>
    EffectInteraction? GetById(string interactionId);

    /// <summary>Gets interaction for effect + damage type combination.</summary>
    /// <param name="effectId">The effect on target.</param>
    /// <param name="damageType">The incoming damage type.</param>
    EffectInteraction? GetForDamageType(string effectId, string damageType);
}
