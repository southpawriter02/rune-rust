using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Central service for logging game events with dual output to Serilog and event store.
/// </summary>
public class GameEventLogger : IGameEventLogger
{
    private readonly ILogger<GameEventLogger> _logger;
    private readonly List<GameEvent> _events = [];
    private readonly object _lock = new();
    private Guid? _sessionId;
    private Guid? _playerId;

    public GameEventLogger(ILogger<GameEventLogger>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GameEventLogger>.Instance;
    }

    /// <inheritdoc/>
    public void Log(GameEvent gameEvent)
    {
        var enriched = gameEvent with
        {
            SessionId = gameEvent.SessionId ?? _sessionId,
            PlayerId = gameEvent.PlayerId ?? _playerId
        };

        lock (_lock)
        {
            _events.Add(enriched);
        }

        LogToSerilog(enriched);
    }

    /// <inheritdoc/>
    public void LogCombat(string eventType, string message, Guid? correlationId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Combat,
            EventType = eventType,
            Message = message,
            CorrelationId = correlationId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogExploration(string eventType, string message, Guid? roomId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Exploration,
            EventType = eventType,
            Message = message,
            RoomId = roomId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogInteraction(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Interaction,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogQuest(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Quest,
            EventType = eventType,
            Message = message,
            PlayerId = playerId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogAbility(string eventType, string message, Guid? correlationId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Ability,
            EventType = eventType,
            Message = message,
            CorrelationId = correlationId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogCharacter(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Character,
            EventType = eventType,
            Message = message,
            PlayerId = playerId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogInventory(string eventType, string message, Guid? playerId = null, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Inventory,
            EventType = eventType,
            Message = message,
            PlayerId = playerId,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogStatusEffect(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.StatusEffect,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogDice(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Dice,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogAI(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.AI,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogEnvironment(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Environment,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogSession(string eventType, string message, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.Session,
            EventType = eventType,
            Message = message,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void LogSystem(string eventType, string message, EventSeverity severity = EventSeverity.Info, Dictionary<string, object>? data = null)
    {
        Log(new GameEvent
        {
            Category = EventCategory.System,
            EventType = eventType,
            Message = message,
            Severity = severity,
            Data = data
        });
    }

    /// <inheritdoc/>
    public void SetSession(Guid sessionId, Guid? playerId = null)
    {
        _sessionId = sessionId;
        _playerId = playerId;
        LogSystem("SessionStarted", $"Session {sessionId} started");
    }

    /// <inheritdoc/>
    public void ClearSession()
    {
        if (_sessionId.HasValue)
            LogSystem("SessionEnded", $"Session {_sessionId} ended");

        _sessionId = null;
        _playerId = null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<GameEvent> GetSessionEvents()
    {
        lock (_lock)
        {
            return _sessionId.HasValue
                ? _events.Where(e => e.SessionId == _sessionId).ToList()
                : [];
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<GameEvent> GetEventsByCategory(EventCategory category)
    {
        lock (_lock)
        {
            return _events.Where(e => e.Category == category).ToList();
        }
    }

    private void LogToSerilog(GameEvent gameEvent)
    {
        var logLevel = gameEvent.Severity switch
        {
            EventSeverity.Trace => LogLevel.Trace,
            EventSeverity.Debug => LogLevel.Debug,
            EventSeverity.Info => LogLevel.Information,
            EventSeverity.Warning => LogLevel.Warning,
            EventSeverity.Error => LogLevel.Error,
            EventSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, "[{Category}:{EventType}] {Message}",
            gameEvent.Category, gameEvent.EventType, gameEvent.Message);
    }
}
