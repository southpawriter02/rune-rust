using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of an interaction with an object.
/// </summary>
public readonly record struct InteractionResult
{
    /// <summary>
    /// Gets whether the interaction succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the result message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets whether the object state changed.
    /// </summary>
    public bool StateChanged { get; init; }

    /// <summary>
    /// Gets the new state (if changed).
    /// </summary>
    public ObjectState? NewState { get; init; }

    /// <summary>
    /// Gets the interaction type performed.
    /// </summary>
    public InteractionType InteractionType { get; init; }

    /// <summary>
    /// Creates a successful interaction result.
    /// </summary>
    public static InteractionResult Succeeded(
        string message,
        InteractionType type,
        ObjectState? newState = null) => new()
    {
        Success = true,
        Message = message,
        StateChanged = newState.HasValue,
        NewState = newState,
        InteractionType = type
    };

    /// <summary>
    /// Creates a failed interaction result.
    /// </summary>
    public static InteractionResult Failed(string message, InteractionType type) => new()
    {
        Success = false,
        Message = message,
        StateChanged = false,
        NewState = null,
        InteractionType = type
    };

    /// <summary>
    /// Creates an examine result.
    /// </summary>
    public static InteractionResult Examined(string description) =>
        Succeeded(description, InteractionType.Examine);

    /// <summary>
    /// Creates an opened result.
    /// </summary>
    public static InteractionResult Opened(string objectName) =>
        Succeeded($"You open the {objectName}.", InteractionType.Open, ObjectState.Open);

    /// <summary>
    /// Creates a closed result.
    /// </summary>
    public static InteractionResult Closed(string objectName) =>
        Succeeded($"You close the {objectName}.", InteractionType.Close, ObjectState.Closed);
}
