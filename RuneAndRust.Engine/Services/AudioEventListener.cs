using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Data;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Subscribes to game events and triggers appropriate sound cues (v0.3.19b).
/// Acts as the bridge between the EventBus and AudioService systems.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public class AudioEventListener
{
    private readonly IEventBus _eventBus;
    private readonly IAudioService _audioService;
    private readonly ILogger<AudioEventListener> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioEventListener"/> class.
    /// </summary>
    /// <param name="eventBus">The event bus to subscribe to.</param>
    /// <param name="audioService">The audio service for playing sounds.</param>
    /// <param name="logger">Logger for traceability.</param>
    public AudioEventListener(
        IEventBus eventBus,
        IAudioService audioService,
        ILogger<AudioEventListener> logger)
    {
        _eventBus = eventBus;
        _audioService = audioService;
        _logger = logger;

        _logger.LogInformation("[AudioListener] AudioEventListener initialized");
    }

    /// <summary>
    /// Subscribes to all relevant game events.
    /// Call this method during application startup after DI registration.
    /// </summary>
    public void SubscribeAll()
    {
        _logger.LogInformation("[AudioListener] Subscribing to game events");

        _eventBus.SubscribeAsync<EntityDamagedEvent>(OnEntityDamagedAsync);
        _eventBus.SubscribeAsync<EntityDeathEvent>(OnEntityDeathAsync);
        _eventBus.SubscribeAsync<ItemLootedEvent>(OnItemLootedAsync);

        _logger.LogInformation("[AudioListener] Subscribed to 3 event types");
    }

    /// <summary>
    /// Handles entity damage events by playing appropriate combat sounds.
    /// </summary>
    /// <param name="evt">The damage event data.</param>
    private async Task OnEntityDamagedAsync(EntityDamagedEvent evt)
    {
        _logger.LogTrace(
            "[AudioListener] Received EntityDamagedEvent: {Target} took {Amount} damage (Critical: {IsCrit})",
            evt.TargetName,
            evt.Amount,
            evt.IsCritical);

        var cue = SoundRegistry.GetDamageCue(evt.IsCritical, evt.Amount);

        _logger.LogDebug(
            "[AudioListener] Playing damage cue: {CueId} for {Target}",
            cue.Id,
            evt.TargetName);

        await _audioService.PlayAsync(cue);
    }

    /// <summary>
    /// Handles entity death events by playing appropriate death sounds.
    /// </summary>
    /// <param name="evt">The death event data.</param>
    private async Task OnEntityDeathAsync(EntityDeathEvent evt)
    {
        _logger.LogTrace(
            "[AudioListener] Received EntityDeathEvent: {Deceased} killed by {Killer} (IsPlayer: {IsPlayer})",
            evt.DeceasedName,
            evt.KilledByName,
            evt.IsPlayer);

        var cue = SoundRegistry.GetDeathCue(evt.IsPlayer);

        _logger.LogDebug(
            "[AudioListener] Playing death cue: {CueId} for {Deceased}",
            cue.Id,
            evt.DeceasedName);

        await _audioService.PlayAsync(cue);
    }

    /// <summary>
    /// Handles item looted events by playing appropriate loot pickup sounds.
    /// </summary>
    /// <param name="evt">The loot event data.</param>
    private async Task OnItemLootedAsync(ItemLootedEvent evt)
    {
        _logger.LogTrace(
            "[AudioListener] Received ItemLootedEvent: {Char} looted {Qty}x {Item} (Value: {Value})",
            evt.CharacterId,
            evt.Quantity,
            evt.ItemName,
            evt.ItemValue);

        var cue = SoundRegistry.GetLootCue(evt.ItemValue);

        _logger.LogDebug(
            "[AudioListener] Playing loot cue: {CueId} for {Item}",
            cue.Id,
            evt.ItemName);

        await _audioService.PlayAsync(cue);
    }
}
