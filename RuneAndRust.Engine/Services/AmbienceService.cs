using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides turn-based procedural ambient soundscape based on room biome (v0.3.19c).
/// Triggers atmospheric audio cues at random intervals during exploration.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public class AmbienceService : IAmbienceService
{
    private readonly IAudioService _audioService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AmbienceService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Minimum turns between ambient cues.
    /// </summary>
    private const int MinTurnInterval = 2;

    /// <summary>
    /// Maximum turns between ambient cues.
    /// </summary>
    private const int MaxTurnInterval = 5;

    /// <summary>
    /// Volume multiplier for ambient sounds (quieter than combat/UI).
    /// </summary>
    private const float AmbienceVolumeMultiplier = 0.6f;

    /// <summary>
    /// Tracks turns since last ambient cue.
    /// </summary>
    private int _turnCounter;

    /// <summary>
    /// Next threshold at which an ambient cue will trigger.
    /// </summary>
    private int _nextTriggerThreshold;

    /// <summary>
    /// Biome audio profile definitions.
    /// Each biome has a specific frequency/duration range for its atmospheric sounds.
    /// </summary>
    private static readonly Dictionary<BiomeType, BiomeAudioProfile> BiomeProfiles = new()
    {
        // Ruin: Wind whistling, stone shifting (mid-range, medium duration)
        [BiomeType.Ruin] = new BiomeAudioProfile(400, 600, 300, 600),

        // Industrial: Machine rumbles, pipe groans (low frequency, long duration)
        [BiomeType.Industrial] = new BiomeAudioProfile(100, 300, 500, 1000),

        // Organic: Skittering, rustling, dripping (high frequency, short duration)
        [BiomeType.Organic] = new BiomeAudioProfile(600, 900, 150, 400),

        // Void: Deep drones, eerie resonance (low-mid frequency, very long duration)
        [BiomeType.Void] = new BiomeAudioProfile(200, 250, 800, 1500)
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbienceService"/> class.
    /// </summary>
    /// <param name="audioService">The audio service for playing sounds.</param>
    /// <param name="scopeFactory">Factory for creating scopes to access scoped services.</param>
    /// <param name="logger">Logger for traceability.</param>
    public AmbienceService(
        IAudioService audioService,
        IServiceScopeFactory scopeFactory,
        ILogger<AmbienceService> logger)
    {
        _audioService = audioService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _random = new Random();

        Reset();

        _logger.LogInformation(
            "[Ambience] AmbienceService initialized (Enabled: {IsEnabled})",
            IsEnabled);
    }

    /// <inheritdoc/>
    public bool IsEnabled => GameSettings.AmbienceEnabled;

    /// <inheritdoc/>
    public async Task UpdateAsync(Guid roomId)
    {
        _logger.LogTrace(
            "[Ambience] UpdateAsync called (Enabled: {IsEnabled}, Turn: {Turn}/{Threshold})",
            IsEnabled, _turnCounter, _nextTriggerThreshold);

        // Skip if ambience is disabled
        if (!IsEnabled)
        {
            _logger.LogTrace("[Ambience] Skipped - ambience disabled");
            return;
        }

        // Increment turn counter
        _turnCounter++;

        // Check if we should trigger an ambient cue
        if (_turnCounter < _nextTriggerThreshold)
        {
            _logger.LogTrace(
                "[Ambience] Waiting ({Turn}/{Threshold} turns)",
                _turnCounter, _nextTriggerThreshold);
            return;
        }

        // Trigger ambient cue
        await TriggerAmbienceAsync(roomId);

        // Reset for next interval
        ResetTurnCounter();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _turnCounter = 0;
        _nextTriggerThreshold = _random.Next(MinTurnInterval, MaxTurnInterval + 1);

        _logger.LogDebug(
            "[Ambience] Reset - next trigger at {Threshold} turns",
            _nextTriggerThreshold);
    }

    /// <summary>
    /// Triggers an ambient audio cue based on the current room's biome.
    /// </summary>
    /// <param name="roomId">The room to get biome from.</param>
    private async Task TriggerAmbienceAsync(Guid roomId)
    {
        BiomeType biome;

        // Get room biome using scoped repository
        using (var scope = _scopeFactory.CreateScope())
        {
            var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
            var room = await roomRepository.GetByIdAsync(roomId);

            if (room == null)
            {
                _logger.LogWarning(
                    "[Ambience] Room not found: {RoomId}",
                    roomId);
                return;
            }

            biome = room.BiomeType;
        }

        // Get or create sound cue for this biome
        var cue = GenerateBiomeCue(biome);

        _logger.LogDebug(
            "[Ambience] Playing {Biome} ambience: {Freq}Hz/{Dur}ms",
            biome, cue.Frequency, cue.DurationMs);

        await _audioService.PlayAsync(cue);
    }

    /// <summary>
    /// Generates a randomized sound cue for the specified biome.
    /// </summary>
    /// <param name="biome">The biome type.</param>
    /// <returns>A sound cue with randomized frequency/duration within biome range.</returns>
    private SoundCue GenerateBiomeCue(BiomeType biome)
    {
        var profile = BiomeProfiles.GetValueOrDefault(biome, BiomeProfiles[BiomeType.Ruin]);

        var frequency = _random.Next(profile.MinFrequency, profile.MaxFrequency + 1);
        var duration = _random.Next(profile.MinDurationMs, profile.MaxDurationMs + 1);

        return new SoundCue(
            $"ambience_{biome.ToString().ToLowerInvariant()}",
            SoundCategory.Ambience,
            frequency,
            duration,
            AmbienceVolumeMultiplier);
    }

    /// <summary>
    /// Resets just the turn counter (not the threshold) for internal use.
    /// </summary>
    private void ResetTurnCounter()
    {
        _turnCounter = 0;
        _nextTriggerThreshold = _random.Next(MinTurnInterval, MaxTurnInterval + 1);

        _logger.LogTrace(
            "[Ambience] Counter reset - next trigger at {Threshold} turns",
            _nextTriggerThreshold);
    }

    /// <summary>
    /// Defines the audio profile for a biome type.
    /// </summary>
    /// <param name="MinFrequency">Minimum frequency in Hz.</param>
    /// <param name="MaxFrequency">Maximum frequency in Hz.</param>
    /// <param name="MinDurationMs">Minimum duration in ms.</param>
    /// <param name="MaxDurationMs">Maximum duration in ms.</param>
    private record BiomeAudioProfile(
        int MinFrequency,
        int MaxFrequency,
        int MinDurationMs,
        int MaxDurationMs);
}
