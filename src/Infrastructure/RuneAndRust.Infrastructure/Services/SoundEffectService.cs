namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages sound effect playback across the application.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Play effects by ID with volume control</description></item>
///   <item><description>Play random effects from category</description></item>
///   <item><description>Max simultaneous sound enforcement</description></item>
///   <item><description>Randomized file selection for variation</description></item>
/// </list>
/// </para>
/// </remarks>
public class SoundEffectService : ISoundEffectService, IDisposable
{
    private readonly IAudioService _audioService;
    private readonly ISoundEffectConfig _config;
    private readonly ILogger<SoundEffectService> _logger;
    private readonly Random _random = new();
    private readonly List<Guid> _activePlaybacks = new();
    private readonly int _maxSimultaneous;
    private bool _disposed;

    /// <summary>
    /// Creates a new sound effect service.
    /// </summary>
    /// <param name="audioService">Audio service for playback.</param>
    /// <param name="config">Sound effect configuration.</param>
    /// <param name="logger">Logger for SFX operations.</param>
    public SoundEffectService(
        IAudioService audioService,
        ISoundEffectConfig config,
        ILogger<SoundEffectService> logger)
    {
        _audioService = audioService;
        _config = config;
        _logger = logger;
        _maxSimultaneous = _config.GetSettings().MaxSimultaneous;
        _logger.LogDebug("SoundEffectService initialized with max {Max} simultaneous", _maxSimultaneous);
    }

    /// <inheritdoc />
    public void Play(string effectId, float volume = 1.0f)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot play: service disposed");
            return;
        }

        var effect = _config.GetEffect(effectId);
        if (effect is null)
        {
            _logger.LogWarning("Sound effect not found: {EffectId}", effectId);
            return;
        }

        if (effect.Files.Count == 0)
        {
            _logger.LogWarning("Sound effect has no files: {EffectId}", effectId);
            return;
        }

        // Enforce max simultaneous sounds
        CleanupFinishedPlaybacks();
        if (_activePlaybacks.Count >= _maxSimultaneous)
        {
            _logger.LogDebug("Max simultaneous sounds ({Max}) reached, skipping: {EffectId}",
                _maxSimultaneous, effectId);
            return;
        }

        // Select file (random if multiple and randomize enabled)
        var file = effect.Files.Count > 1 && effect.Randomize
            ? effect.Files[_random.Next(effect.Files.Count)]
            : effect.Files[0];

        var effectiveVolume = effect.Volume * Math.Clamp(volume, 0f, 1f);
        var playbackId = _audioService.Play(file, AudioChannel.Effects, effectiveVolume, loop: false);

        if (playbackId != Guid.Empty)
        {
            _activePlaybacks.Add(playbackId);
            _logger.LogDebug("Playing SFX: {EffectId} → {File} at {Volume:P0}", effectId, file, effectiveVolume);
        }
    }

    /// <inheritdoc />
    public void PlayRandom(string category, float volume = 1.0f)
    {
        var effects = _config.GetEffectsInCategory(category);
        if (effects.Count == 0)
        {
            _logger.LogWarning("No effects in category: {Category}", category);
            return;
        }

        var effectId = effects[_random.Next(effects.Count)];
        Play(effectId, volume);
        _logger.LogDebug("PlayRandom from {Category}: selected {EffectId}", category, effectId);
    }

    /// <inheritdoc />
    public void StopAll()
    {
        foreach (var id in _activePlaybacks)
        {
            _audioService.Stop(id);
        }

        var count = _activePlaybacks.Count;
        _activePlaybacks.Clear();
        _logger.LogDebug("Stopped all {Count} sound effects", count);
    }

    /// <inheritdoc />
    public void SetVolume(float volume)
    {
        var clamped = Math.Clamp(volume, 0f, 1f);
        _audioService.SetChannelVolume(AudioChannel.Effects, clamped);
        _logger.LogDebug("Effects volume set to {Volume:P0}", clamped);
    }

    /// <inheritdoc />
    public float GetVolume() => _audioService.GetChannelVolume(AudioChannel.Effects);

    /// <inheritdoc />
    public async Task PreloadCategoryAsync(string category, CancellationToken ct = default)
    {
        var effects = _config.GetEffectsInCategory(category);
        var allFiles = new List<string>();

        foreach (var effectId in effects)
        {
            var effect = _config.GetEffect(effectId);
            if (effect is not null)
            {
                allFiles.AddRange(effect.Files);
            }
        }

        await _audioService.PreloadAsync(allFiles, ct);
        _logger.LogDebug("Preloaded {Count} files for category: {Category}", allFiles.Count, category);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableEffects() => _config.GetAllEffectIds();

    /// <inheritdoc />
    public IReadOnlyList<string> GetCategories() => _config.GetAllCategories();

    /// <summary>
    /// Cleans up the active playbacks list when at capacity.
    /// Since IAudioService doesn't track individual playback status,
    /// we simply clear older entries when at capacity.
    /// </summary>
    private void CleanupFinishedPlaybacks()
    {
        // Simple cleanup: if at max, remove oldest entries (first half)
        if (_activePlaybacks.Count >= _maxSimultaneous)
        {
            var removeCount = _maxSimultaneous / 2;
            _activePlaybacks.RemoveRange(0, Math.Min(removeCount, _activePlaybacks.Count));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        StopAll();
        _disposed = true;
        _logger.LogInformation("SoundEffectService disposed");
    }
}
