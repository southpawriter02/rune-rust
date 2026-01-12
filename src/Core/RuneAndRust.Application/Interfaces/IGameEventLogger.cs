using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for logging game events.
/// </summary>
public interface IGameEventLogger
{
    /// <summary>
    /// Logs a game event.
    /// </summary>
    void Log(GameEvent gameEvent);

    /// <summary>
    /// Logs a combat event.
    /// </summary>
    void LogCombat(string eventType, string message, Guid? correlationId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an exploration event.
    /// </summary>
    void LogExploration(string eventType, string message, Guid? roomId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an interaction event.
    /// </summary>
    void LogInteraction(string eventType, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs a quest event.
    /// </summary>
    void LogQuest(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an ability event.
    /// </summary>
    void LogAbility(string eventType, string message, Guid? correlationId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs a character event.
    /// </summary>
    void LogCharacter(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an inventory event.
    /// </summary>
    void LogInventory(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs a status effect event.
    /// </summary>
    void LogStatusEffect(string eventType, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs a dice event.
    /// </summary>
    void LogDice(string eventType, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an AI event.
    /// </summary>
    void LogAI(string eventType, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs an environment event.
    /// </summary>
    void LogEnvironment(string eventType, string message, Dictionary<string, object>? data = null);

    /// <summary>
    /// Logs a system event.
    /// </summary>
    void LogSystem(string eventType, string message, EventSeverity severity = EventSeverity.Info, Dictionary<string, object>? data = null);

    /// <summary>
    /// Sets the current session context.
    /// </summary>
    void SetSession(Guid sessionId, Guid? playerId = null);

    /// <summary>
    /// Clears the current session context.
    /// </summary>
    void ClearSession();

    /// <summary>
    /// Gets all events for the current session.
    /// </summary>
    IReadOnlyList<GameEvent> GetSessionEvents();

    /// <summary>
    /// Gets events by category.
    /// </summary>
    IReadOnlyList<GameEvent> GetEventsByCategory(EventCategory category);
}
