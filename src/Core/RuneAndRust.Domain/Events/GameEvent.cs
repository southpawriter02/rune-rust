using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Events;

/// <summary>
/// Base record for all game events.
/// </summary>
public record GameEvent
{
    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the event category.
    /// </summary>
    public EventCategory Category { get; init; }

    /// <summary>
    /// Gets the event severity.
    /// </summary>
    public EventSeverity Severity { get; init; } = EventSeverity.Info;

    /// <summary>
    /// Gets the event type name (e.g., "PlayerMoved", "DamageDealt").
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the human-readable message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the correlation ID for tracing related events.
    /// </summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    /// Gets the session ID.
    /// </summary>
    public Guid? SessionId { get; init; }

    /// <summary>
    /// Gets the player ID (if applicable).
    /// </summary>
    public Guid? PlayerId { get; init; }

    /// <summary>
    /// Gets the room ID (if applicable).
    /// </summary>
    public Guid? RoomId { get; init; }

    /// <summary>
    /// Gets additional structured data.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Data { get; init; }

    /// <summary>
    /// Creates a system event.
    /// </summary>
    public static GameEvent System(string eventType, string message, EventSeverity severity = EventSeverity.Info) =>
        new() { Category = EventCategory.System, EventType = eventType, Message = message, Severity = severity };

    /// <summary>
    /// Creates a combat event.
    /// </summary>
    public static GameEvent Combat(string eventType, string message, Guid? correlationId = null) =>
        new() { Category = EventCategory.Combat, EventType = eventType, Message = message, CorrelationId = correlationId };

    /// <summary>
    /// Creates an exploration event.
    /// </summary>
    public static GameEvent Exploration(string eventType, string message, Guid? roomId = null) =>
        new() { Category = EventCategory.Exploration, EventType = eventType, Message = message, RoomId = roomId };

    /// <summary>
    /// Creates a quest event.
    /// </summary>
    public static GameEvent Quest(string eventType, string message, Guid? playerId = null) =>
        new() { Category = EventCategory.Quest, EventType = eventType, Message = message, PlayerId = playerId };

    /// <summary>
    /// Creates an ability event.
    /// </summary>
    public static GameEvent Ability(string eventType, string message, Guid? correlationId = null) =>
        new() { Category = EventCategory.Ability, EventType = eventType, Message = message, CorrelationId = correlationId };

    /// <summary>
    /// Creates an interaction event.
    /// </summary>
    public static GameEvent Interaction(string eventType, string message) =>
        new() { Category = EventCategory.Interaction, EventType = eventType, Message = message };

    /// <summary>
    /// Creates a character event.
    /// </summary>
    public static GameEvent Character(string eventType, string message, Guid? playerId = null) =>
        new() { Category = EventCategory.Character, EventType = eventType, Message = message, PlayerId = playerId };

    /// <summary>
    /// Creates an inventory event.
    /// </summary>
    public static GameEvent Inventory(string eventType, string message, Guid? playerId = null) =>
        new() { Category = EventCategory.Inventory, EventType = eventType, Message = message, PlayerId = playerId };
}
