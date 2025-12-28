using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Thread-safe publish/subscribe event bus implementation (v0.3.19b).
/// Enables decoupled communication between game systems (Combat, Inventory, Audio).
/// </summary>
/// <remarks>See: SPEC-EVENTBUS-001 for Event System design.</remarks>
public class EventBus : IEventBus
{
    private readonly ILogger<EventBus> _logger;

    /// <summary>
    /// Stores synchronous handlers keyed by event type.
    /// </summary>
    private readonly ConcurrentDictionary<Type, List<Delegate>> _syncHandlers = new();

    /// <summary>
    /// Stores asynchronous handlers keyed by event type.
    /// </summary>
    private readonly ConcurrentDictionary<Type, List<Delegate>> _asyncHandlers = new();

    /// <summary>
    /// Lock object for thread-safe handler list modifications.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class.
    /// </summary>
    /// <param name="logger">Logger for traceability.</param>
    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger;

        _logger.LogInformation("[EventBus] EventBus initialized");
    }

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);

        lock (_lock)
        {
            var handlers = _syncHandlers.GetOrAdd(eventType, _ => new List<Delegate>());
            handlers.Add(handler);
        }

        _logger.LogDebug(
            "[EventBus] Subscribed sync handler to {EventType} (Total: {Count})",
            eventType.Name,
            _syncHandlers[eventType].Count);
    }

    /// <inheritdoc/>
    public void SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);

        lock (_lock)
        {
            var handlers = _asyncHandlers.GetOrAdd(eventType, _ => new List<Delegate>());
            handlers.Add(handler);
        }

        _logger.LogDebug(
            "[EventBus] Subscribed async handler to {EventType} (Total: {Count})",
            eventType.Name,
            _asyncHandlers[eventType].Count);
    }

    /// <inheritdoc/>
    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        var eventType = typeof(TEvent);

        _logger.LogDebug(
            "[EventBus] Publishing sync event: {EventType}",
            eventType.Name);

        if (!_syncHandlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogTrace(
                "[EventBus] No sync handlers for {EventType}",
                eventType.Name);
            return;
        }

        // Create snapshot to prevent modification during iteration
        List<Delegate> handlerSnapshot;
        lock (_lock)
        {
            handlerSnapshot = handlers.ToList();
        }

        _logger.LogTrace(
            "[EventBus] Invoking {Count} sync handler(s) for {EventType}",
            handlerSnapshot.Count,
            eventType.Name);

        foreach (var handler in handlerSnapshot)
        {
            try
            {
                ((Action<TEvent>)handler)(eventData);
            }
            catch (Exception ex)
            {
                // Log but don't rethrow - one handler failure shouldn't block others
                _logger.LogError(
                    ex,
                    "[EventBus] Sync handler failed for {EventType}: {Message}",
                    eventType.Name,
                    ex.Message);
            }
        }
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent eventData) where TEvent : class
    {
        var eventType = typeof(TEvent);

        _logger.LogDebug(
            "[EventBus] Publishing async event: {EventType}",
            eventType.Name);

        if (!_asyncHandlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogTrace(
                "[EventBus] No async handlers for {EventType}",
                eventType.Name);
            return;
        }

        // Create snapshot to prevent modification during iteration
        List<Delegate> handlerSnapshot;
        lock (_lock)
        {
            handlerSnapshot = handlers.ToList();
        }

        _logger.LogTrace(
            "[EventBus] Invoking {Count} async handler(s) for {EventType}",
            handlerSnapshot.Count,
            eventType.Name);

        // Execute all handlers concurrently
        var tasks = handlerSnapshot.Select(async handler =>
        {
            try
            {
                await ((Func<TEvent, Task>)handler)(eventData);
            }
            catch (Exception ex)
            {
                // Log but don't rethrow - one handler failure shouldn't block others
                _logger.LogError(
                    ex,
                    "[EventBus] Async handler failed for {EventType}: {Message}",
                    eventType.Name,
                    ex.Message);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (!_syncHandlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogTrace(
                "[EventBus] No sync handlers to unsubscribe for {EventType}",
                eventType.Name);
            return;
        }

        lock (_lock)
        {
            var removed = handlers.Remove(handler);

            _logger.LogDebug(
                "[EventBus] Unsubscribed sync handler from {EventType} (Removed: {Removed}, Remaining: {Count})",
                eventType.Name,
                removed,
                handlers.Count);
        }
    }

    /// <inheritdoc/>
    public void UnsubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (!_asyncHandlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogTrace(
                "[EventBus] No async handlers to unsubscribe for {EventType}",
                eventType.Name);
            return;
        }

        lock (_lock)
        {
            var removed = handlers.Remove(handler);

            _logger.LogDebug(
                "[EventBus] Unsubscribed async handler from {EventType} (Removed: {Removed}, Remaining: {Count})",
                eventType.Name,
                removed,
                handlers.Count);
        }
    }
}
