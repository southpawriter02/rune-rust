using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements audio playback management with volume filtering (v0.3.19a).
/// Delegates actual playback to platform-specific IAudioProvider implementations.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public class AudioService : IAudioService
{
    private readonly IAudioProvider _provider;
    private readonly ILogger<AudioService> _logger;

    /// <summary>
    /// System sound cue registry for lookup by ID.
    /// </summary>
    private static readonly Dictionary<string, SoundCue> _systemCues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ui_click"] = SoundCue.UiClick,
        ["ui_select"] = SoundCue.UiSelect,
        ["ui_error"] = SoundCue.UiError,
        ["combat_hit_light"] = SoundCue.CombatHitLight,
        ["combat_hit_heavy"] = SoundCue.CombatHitHeavy,
        ["combat_critical"] = SoundCue.CombatCritical
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioService"/> class.
    /// </summary>
    /// <param name="provider">Platform-specific audio provider.</param>
    /// <param name="logger">Logger for traceability.</param>
    public AudioService(IAudioProvider provider, ILogger<AudioService> logger)
    {
        _provider = provider;
        _logger = logger;

        _logger.LogInformation(
            "[Audio] AudioService initialized (Provider supported: {IsSupported})",
            _provider.IsSupported);
    }

    /// <inheritdoc/>
    public bool IsMuted => GameSettings.MasterVolume <= 0;

    /// <inheritdoc/>
    public int MasterVolume => GameSettings.MasterVolume;

    /// <inheritdoc/>
    public async Task PlayAsync(SoundCue cue)
    {
        _logger.LogTrace(
            "[Audio] Requesting cue: {CueId} ({Freq}Hz/{Dur}ms)",
            cue.Id, cue.Frequency, cue.DurationMs);

        // Check mute
        if (IsMuted)
        {
            _logger.LogTrace(
                "[Audio] Skipped {CueId} (MasterVolume: 0)",
                cue.Id);
            return;
        }

        // Check provider support
        if (!_provider.IsSupported)
        {
            _logger.LogTrace(
                "[Audio] Skipped {CueId} (Provider not supported)",
                cue.Id);
            return;
        }

        _logger.LogDebug(
            "[Audio] Provider executing {CueId}",
            cue.Id);

        try
        {
            await _provider.PlayAsync(cue, MasterVolume);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[Audio] Failed to play {CueId}: {Message}",
                cue.Id, ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task PlaySystemCueAsync(string cueId)
    {
        if (_systemCues.TryGetValue(cueId, out var cue))
        {
            await PlayAsync(cue);
        }
        else
        {
            _logger.LogWarning(
                "[Audio] Unknown system cue ID: {CueId}",
                cueId);
        }
    }
}
