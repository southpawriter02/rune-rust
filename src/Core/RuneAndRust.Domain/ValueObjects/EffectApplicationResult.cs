using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of attempting to apply a status effect to a target.
/// </summary>
/// <remarks>
/// <para>Provides detailed information about what happened during effect application.</para>
/// <para>Used for combat log messages and UI feedback.</para>
/// </remarks>
/// <param name="Applied">Whether the effect was successfully applied or modified.</param>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="EffectName">The effect display name.</param>
/// <param name="Outcome">The specific outcome of the application attempt.</param>
/// <param name="NewStacks">Current stack count (if applicable).</param>
/// <param name="NewDuration">Current duration in turns (if applicable).</param>
/// <param name="Message">Human-readable result message.</param>
public readonly record struct EffectApplicationResult(
    bool Applied,
    string EffectId,
    string EffectName,
    EffectApplicationOutcome Outcome,
    int? NewStacks,
    int? NewDuration,
    string Message)
{
    /// <summary>
    /// Creates a result for a newly applied effect.
    /// </summary>
    public static EffectApplicationResult Success(
        string effectId,
        string effectName,
        int stacks,
        int? duration) =>
        new(true, effectId, effectName, EffectApplicationOutcome.Applied,
            stacks, duration, $"{effectName} applied");

    /// <summary>
    /// Creates a result for a duration refresh.
    /// </summary>
    public static EffectApplicationResult Refreshed(
        string effectId,
        string effectName,
        int newDuration) =>
        new(true, effectId, effectName, EffectApplicationOutcome.Refreshed,
            null, newDuration, $"{effectName} duration refreshed");

    /// <summary>
    /// Creates a result for increased stacks.
    /// </summary>
    public static EffectApplicationResult Stacked(
        string effectId,
        string effectName,
        int newStacks,
        int? duration) =>
        new(true, effectId, effectName, EffectApplicationOutcome.Stacked,
            newStacks, duration, $"{effectName} stacked to {newStacks}");

    /// <summary>
    /// Creates a result for a blocked application.
    /// </summary>
    public static EffectApplicationResult Blocked(string effectId, string effectName) =>
        new(false, effectId, effectName, EffectApplicationOutcome.Blocked,
            null, null, $"{effectName} resisted (already active)");

    /// <summary>
    /// Creates a result for an immune target.
    /// </summary>
    public static EffectApplicationResult Immune(string effectId, string effectName) =>
        new(false, effectId, effectName, EffectApplicationOutcome.Immune,
            null, null, $"Immune to {effectName}");

    /// <summary>
    /// Creates a result for an unknown effect.
    /// </summary>
    public static EffectApplicationResult NotFound(string effectId) =>
        new(false, effectId, effectId, EffectApplicationOutcome.NotFound,
            null, null, $"Unknown effect: {effectId}");
}
