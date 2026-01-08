using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Interface for entities that can have status effects applied.
/// </summary>
/// <remarks>
/// <para>Implemented by Player and Monster to enable effect targeting.</para>
/// </remarks>
public interface IEffectTarget
{
    /// <summary>Unique identifier.</summary>
    Guid Id { get; }

    /// <summary>Display name.</summary>
    string Name { get; }

    /// <summary>Current health.</summary>
    int Health { get; }

    /// <summary>Maximum health.</summary>
    int MaxHealth { get; }

    /// <summary>Whether the target is alive.</summary>
    bool IsAlive { get; }

    /// <summary>Active status effects on this target.</summary>
    IReadOnlyList<ActiveStatusEffect> ActiveEffects { get; }

    /// <summary>Effect IDs this target is immune to.</summary>
    IReadOnlyList<string> EffectImmunities { get; }

    /// <summary>Applies damage to this target.</summary>
    /// <param name="amount">Amount of damage.</param>
    /// <returns>Actual damage taken.</returns>
    int TakeDamage(int amount);

    /// <summary>Restores health to this target.</summary>
    /// <param name="amount">Amount to heal.</param>
    /// <returns>Actual healing done.</returns>
    int Heal(int amount);

    /// <summary>Adds an active effect to this target.</summary>
    /// <param name="effect">The effect to add.</param>
    void AddEffect(ActiveStatusEffect effect);

    /// <summary>Removes an active effect from this target.</summary>
    /// <param name="effectId">The effect instance ID to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    bool RemoveEffect(Guid effectId);

    /// <summary>Removes all effects matching the definition ID.</summary>
    /// <param name="definitionId">The effect definition ID.</param>
    /// <returns>Number of effects removed.</returns>
    int RemoveEffectsByDefinition(string definitionId);

    /// <summary>Gets an active effect by definition ID.</summary>
    /// <param name="definitionId">The effect definition ID.</param>
    /// <returns>The active effect, or null if not found.</returns>
    ActiveStatusEffect? GetEffect(string definitionId);

    /// <summary>Checks if target has an active effect.</summary>
    /// <param name="definitionId">The effect definition ID.</param>
    /// <returns>True if the effect is active.</returns>
    bool HasEffect(string definitionId);

    /// <summary>Checks if target is immune to an effect.</summary>
    /// <param name="definitionId">The effect definition ID.</param>
    /// <returns>True if immune.</returns>
    bool IsImmuneToEffect(string definitionId);
}
