namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of triggering an object effect.
/// </summary>
/// <remarks>
/// EffectTriggerResult captures the outcome when a linked effect is resolved,
/// including whether it succeeded, the message to display, and any state changes.
/// </remarks>
public readonly record struct EffectTriggerResult
{
    /// <summary>
    /// Gets whether the effect was successfully applied.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the effect that was triggered.
    /// </summary>
    public ObjectEffect Effect { get; init; }

    /// <summary>
    /// Gets the ID of the target object.
    /// </summary>
    public string TargetObjectId { get; init; }

    /// <summary>
    /// Gets the message to display to the player.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the new state of the target object (if changed).
    /// </summary>
    public ObjectState? NewTargetState { get; init; }

    /// <summary>
    /// Creates a successful effect result.
    /// </summary>
    /// <param name="effect">The effect that was triggered.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="newState">The new state of the target (optional).</param>
    /// <returns>A successful EffectTriggerResult.</returns>
    public static EffectTriggerResult Succeeded(
        ObjectEffect effect,
        string message,
        ObjectState? newState = null) => new()
    {
        Success = true,
        Effect = effect,
        TargetObjectId = effect.TargetObjectId,
        Message = message,
        NewTargetState = newState
    };

    /// <summary>
    /// Creates a failed effect result.
    /// </summary>
    /// <param name="effect">The effect that failed.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed EffectTriggerResult.</returns>
    public static EffectTriggerResult Failed(
        ObjectEffect effect,
        string message) => new()
    {
        Success = false,
        Effect = effect,
        TargetObjectId = effect.TargetObjectId,
        Message = message,
        NewTargetState = null
    };

    /// <summary>
    /// Creates a result for when the target object wasn't found.
    /// </summary>
    /// <param name="effect">The effect whose target was not found.</param>
    /// <returns>A failed EffectTriggerResult with appropriate message.</returns>
    public static EffectTriggerResult TargetNotFound(ObjectEffect effect) =>
        Failed(effect, $"Effect target '{effect.TargetObjectId}' not found.");

    /// <summary>
    /// Creates a result for a message-only effect.
    /// </summary>
    /// <param name="effect">The message effect.</param>
    /// <returns>A successful EffectTriggerResult with the effect message.</returns>
    public static EffectTriggerResult MessageDisplayed(ObjectEffect effect) => new()
    {
        Success = true,
        Effect = effect,
        TargetObjectId = string.Empty,
        Message = effect.EffectMessage ?? string.Empty,
        NewTargetState = null
    };

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    /// <returns>A string describing the result.</returns>
    public override string ToString() =>
        Success
            ? $"Effect {Effect.Type} on {TargetObjectId}: {Message}"
            : $"Effect {Effect.Type} failed: {Message}";
}
