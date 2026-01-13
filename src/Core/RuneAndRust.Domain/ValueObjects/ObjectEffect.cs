namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines an effect that occurs when an interactive object changes state.
/// </summary>
/// <remarks>
/// ObjectEffect represents a linked effect between two interactive objects.
/// When the source object enters a specific state (TriggerOnState), the effect
/// is applied to the target object. Effects can be immediate or delayed by turns.
/// 
/// <example>
/// Example: Lever unlocks door when activated:
/// <code>
/// var effect = ObjectEffect.UnlockTarget("iron-gate", ObjectState.Active, 
///     "You hear a loud clunk as the iron gate unlocks.");
/// lever.AddEffect(effect);
/// </code>
/// </example>
/// </remarks>
public readonly record struct ObjectEffect
{
    /// <summary>
    /// Gets the effect type to apply to the target.
    /// </summary>
    public EffectType Type { get; init; }

    /// <summary>
    /// Gets the target object definition ID affected by this effect.
    /// </summary>
    /// <remarks>
    /// This is matched against InteractiveObject.DefinitionId in the same room.
    /// Can be empty for Message-only effects.
    /// </remarks>
    public string TargetObjectId { get; init; }

    /// <summary>
    /// Gets the state that triggers this effect.
    /// </summary>
    /// <remarks>
    /// The effect triggers when the source object enters this state.
    /// For example, ObjectState.Active triggers when a lever is pulled.
    /// </remarks>
    public ObjectState TriggerOnState { get; init; }

    /// <summary>
    /// Gets the delay in turns before the effect triggers (0 = immediate).
    /// </summary>
    /// <remarks>
    /// Delayed effects are queued and processed on subsequent turn ticks.
    /// Most effects should be immediate (0 delay).
    /// </remarks>
    public int DelayTurns { get; init; }

    /// <summary>
    /// Gets the message displayed when this effect triggers.
    /// </summary>
    /// <remarks>
    /// If null, a default message based on the effect type is generated.
    /// Custom messages provide better feedback for puzzle elements.
    /// </remarks>
    public string? EffectMessage { get; init; }

    /// <summary>
    /// Gets whether this effect triggers immediately (no delay).
    /// </summary>
    public bool IsImmediate => DelayTurns == 0;

    /// <summary>
    /// Gets whether this effect has a target object.
    /// </summary>
    public bool HasTarget => !string.IsNullOrEmpty(TargetObjectId);

    /// <summary>
    /// Creates an immediate open effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that opens the target.</returns>
    public static ObjectEffect OpenTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.OpenTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate close effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that closes the target.</returns>
    public static ObjectEffect CloseTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.CloseTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate unlock effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that unlocks the target.</returns>
    public static ObjectEffect UnlockTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.UnlockTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate lock effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that locks the target.</returns>
    public static ObjectEffect LockTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.LockTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate toggle effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that toggles the target state.</returns>
    public static ObjectEffect ToggleTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.ToggleTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate activate effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that activates the target.</returns>
    public static ObjectEffect ActivateTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.ActivateTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate deactivate effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that deactivates the target.</returns>
    public static ObjectEffect DeactivateTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.DeactivateTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate destroy effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that destroys the target.</returns>
    public static ObjectEffect DestroyTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.DestroyTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates an immediate reveal effect.
    /// </summary>
    /// <param name="targetId">The target object definition ID.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <param name="message">Optional message to display.</param>
    /// <returns>A new ObjectEffect that reveals the target.</returns>
    public static ObjectEffect RevealTarget(string targetId, ObjectState triggerOn, string? message = null) => new()
    {
        Type = EffectType.RevealTarget,
        TargetObjectId = targetId?.ToLowerInvariant() ?? string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates a message-only effect (no state change).
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="triggerOn">The state that triggers this effect.</param>
    /// <returns>A new ObjectEffect that only displays a message.</returns>
    public static ObjectEffect MessageOnly(string message, ObjectState triggerOn) => new()
    {
        Type = EffectType.Message,
        TargetObjectId = string.Empty,
        TriggerOnState = triggerOn,
        DelayTurns = 0,
        EffectMessage = message
    };

    /// <summary>
    /// Creates a delayed version of an effect.
    /// </summary>
    /// <param name="baseEffect">The base effect to delay.</param>
    /// <param name="delayTurns">Number of turns to delay (must be positive).</param>
    /// <returns>A new ObjectEffect with the specified delay.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when delayTurns is negative.</exception>
    public static ObjectEffect Delayed(ObjectEffect baseEffect, int delayTurns)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(delayTurns);
        return baseEffect with { DelayTurns = delayTurns };
    }

    /// <summary>
    /// Returns a string representation of this effect.
    /// </summary>
    /// <returns>A string describing the effect.</returns>
    public override string ToString()
    {
        var delay = IsImmediate ? "" : $" (delayed {DelayTurns} turns)";
        return HasTarget
            ? $"{Type} -> {TargetObjectId} on {TriggerOnState}{delay}"
            : $"{Type} on {TriggerOnState}{delay}";
    }
}
