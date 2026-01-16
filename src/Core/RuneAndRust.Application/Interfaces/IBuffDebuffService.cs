namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service for managing status effects (buffs/debuffs) on combatants.
/// </summary>
/// <remarks>
/// <para>
/// IBuffDebuffService provides:
/// <list type="bullet">
///   <item><description>Effect application with stacking rules</description></item>
///   <item><description>Effect removal and cleansing</description></item>
///   <item><description>Active effect queries</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IBuffDebuffService
{
    /// <summary>
    /// Applies a status effect to a target.
    /// </summary>
    /// <param name="target">The target to apply the effect to.</param>
    /// <param name="effectId">The effect definition ID.</param>
    /// <param name="sourceId">Optional source entity ID.</param>
    /// <param name="sourceName">Optional source entity name.</param>
    /// <returns>Result of the application attempt.</returns>
    ApplyResult ApplyEffect(IEffectTarget target, string effectId, Guid? sourceId = null, string? sourceName = null);

    /// <summary>
    /// Removes a specific effect from a target.
    /// </summary>
    /// <param name="target">The target to remove the effect from.</param>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>True if the effect was removed.</returns>
    bool RemoveEffect(IEffectTarget target, string effectId);

    /// <summary>
    /// Clears all debuffs from a target (cleanse).
    /// </summary>
    /// <param name="target">The target to cleanse.</param>
    /// <returns>Number of effects removed.</returns>
    int ClearDebuffs(IEffectTarget target);

    /// <summary>
    /// Clears all buffs from a target (dispel).
    /// </summary>
    /// <param name="target">The target to dispel.</param>
    /// <returns>Number of effects removed.</returns>
    int ClearBuffs(IEffectTarget target);

    /// <summary>
    /// Gets all active effects on a target.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <returns>List of active effects.</returns>
    IReadOnlyList<ActiveStatusEffect> GetActiveEffects(IEffectTarget target);

    /// <summary>
    /// Gets active effects of a specific category.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <param name="category">The effect category.</param>
    /// <returns>Filtered list of active effects.</returns>
    IReadOnlyList<ActiveStatusEffect> GetActiveEffects(IEffectTarget target, EffectCategory category);

    /// <summary>
    /// Checks if a target has a specific effect.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>True if the effect is active.</returns>
    bool HasEffect(IEffectTarget target, string effectId);

    /// <summary>
    /// Gets the current stack count of an effect.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>Stack count, or 0 if not found.</returns>
    int GetStackCount(IEffectTarget target, string effectId);

    /// <summary>
    /// Gets the remaining duration of an effect.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>Remaining turns, or null if not found.</returns>
    int? GetRemainingDuration(IEffectTarget target, string effectId);

    // ═══════════════════════════════════════════════════════════════
    // v0.10.0c: Effect Triggers & Cleanse
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Ticks all effects on a target for a given timing.
    /// </summary>
    /// <param name="target">The target to tick effects for.</param>
    /// <param name="timing">When the tick is occurring.</param>
    /// <returns>Result of the tick processing.</returns>
    TickResult TickEffects(IEffectTarget target, TriggerTiming timing);

    /// <summary>
    /// Checks if a target is immune to an effect.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <param name="effectId">The effect to check immunity for.</param>
    /// <returns>True if target is immune.</returns>
    bool IsImmune(IEffectTarget target, string effectId);

    /// <summary>
    /// Cleanses debuffs from a target.
    /// </summary>
    /// <param name="target">The target to cleanse.</param>
    /// <param name="count">Number of debuffs to remove (null = all).</param>
    /// <returns>Number of debuffs removed.</returns>
    int Cleanse(IEffectTarget target, int? count = null);

    /// <summary>
    /// Dispels buffs from a target (for use on enemies).
    /// </summary>
    /// <param name="target">The target to dispel.</param>
    /// <param name="count">Number of buffs to remove (null = all).</param>
    /// <returns>Number of buffs removed.</returns>
    int Dispel(IEffectTarget target, int? count = null);
}
