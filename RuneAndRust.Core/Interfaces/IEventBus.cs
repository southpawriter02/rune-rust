namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for a simple publish/subscribe event bus (v0.3.19b).
/// Enables decoupled communication between game systems (Combat, Inventory, Audio).
/// </summary>
/// <remarks>See: SPEC-EVENTBUS-001 for Event System design.</remarks>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a synchronous handler to receive events of the specified type.
    /// Handlers are invoked when <see cref="Publish{TEvent}"/> is called.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The action to invoke when an event is published.</param>
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// Subscribes an asynchronous handler to receive events of the specified type.
    /// Handlers are invoked when <see cref="PublishAsync{TEvent}"/> is called.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The async function to invoke when an event is published.</param>
    void SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class;

    /// <summary>
    /// Publishes an event to all subscribed synchronous handlers.
    /// Handlers are invoked sequentially on the calling thread.
    /// Exceptions in handlers are logged but do not prevent other handlers from executing.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventData">The event data to send to subscribers.</param>
    void Publish<TEvent>(TEvent eventData) where TEvent : class;

    /// <summary>
    /// Publishes an event to all subscribed asynchronous handlers.
    /// Handlers are invoked concurrently using Task.WhenAll.
    /// Exceptions in handlers are logged but do not prevent other handlers from executing.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventData">The event data to send to subscribers.</param>
    /// <returns>A task representing the completion of all handlers.</returns>
    Task PublishAsync<TEvent>(TEvent eventData) where TEvent : class;

    /// <summary>
    /// Unsubscribes a synchronous handler from receiving events.
    /// </summary>
    /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
    /// <param name="handler">The handler to remove.</param>
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// Unsubscribes an asynchronous handler from receiving events.
    /// </summary>
    /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
    /// <param name="handler">The async handler to remove.</param>
    void UnsubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}
