namespace RuneAndRust.Infrastructure.Services.Audio;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Implementation of the core audio service.
/// </summary>
/// <remarks>
/// <para>
/// Provides audio playback with:
/// <list type="bullet">
///   <item><description>Channel-based volume control (Master, Music, Effects, UI, Voice)</description></item>
///   <item><description>LRU audio caching (50 files default)</description></item>
///   <item><description>Individual playback management by ID</description></item>
///   <item><description>Global enable/disable control</description></item>
/// </list>
/// </para>
/// </remarks>
public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private readonly AudioCache _cache;
    private readonly Dictionary<AudioChannel, float> _channelVolumes;
    private readonly Dictionary<AudioChannel, bool> _channelMuted;
    private readonly Dictionary<Guid, AudioPlayback> _activePlaybacks;
    private bool _isEnabled = true;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Creates a new audio service.
    /// </summary>
    /// <param name="logger">Logger for audio operations.</param>
    /// <param name="cacheSize">Maximum cache size (default 50).</param>
    public AudioService(ILogger<AudioService> logger, int cacheSize = 50)
    {
        _logger = logger;
        _cache = new AudioCache(cacheSize);
        _channelVolumes = InitializeDefaultVolumes();
        _channelMuted = InitializeDefaultMutes();
        _activePlaybacks = new Dictionary<Guid, AudioPlayback>();

        _logger.LogInformation("AudioService initialized with cache size {Size}", cacheSize);
    }

    /// <inheritdoc />
    public Guid Play(string audioPath, AudioChannel channel, float volume = 1.0f, bool loop = false)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot play audio: service disposed");
            return Guid.Empty;
        }

        if (!_isEnabled)
        {
            _logger.LogDebug("Audio disabled, skipping: {Path}", audioPath);
            return Guid.Empty;
        }

        if (_channelMuted[AudioChannel.Master] || _channelMuted[channel])
        {
            _logger.LogDebug("Channel muted, skipping: {Path} on {Channel}", audioPath, channel);
            return Guid.Empty;
        }

        var audioData = _cache.GetOrLoad(audioPath);
        if (audioData is null)
        {
            _logger.LogWarning("Failed to load audio: {Path}", audioPath);
            return Guid.Empty;
        }

        var effectiveVolume = CalculateEffectiveVolume(channel, volume);
        var playbackId = Guid.NewGuid();
        var playback = new AudioPlayback(playbackId, channel, audioData, effectiveVolume, loop);

        _activePlaybacks[playbackId] = playback;
        playback.Start();

        _logger.LogDebug(
            "Playing: {Path} on {Channel} at volume {Volume:P0} (effective: {Effective:P0})",
            audioPath,
            channel,
            volume,
            effectiveVolume);

        return playbackId;
    }

    /// <inheritdoc />
    public void Stop(Guid playbackId)
    {
        if (_activePlaybacks.TryGetValue(playbackId, out var playback))
        {
            playback.Stop();
            _activePlaybacks.Remove(playbackId);
            _logger.LogDebug("Stopped playback: {Id}", playbackId);
        }
    }

    /// <inheritdoc />
    public void StopChannel(AudioChannel channel)
    {
        var toStop = _activePlaybacks
            .Where(p => p.Value.Channel == channel)
            .ToList();

        foreach (var (id, playback) in toStop)
        {
            playback.Stop();
            _activePlaybacks.Remove(id);
        }

        _logger.LogDebug("Stopped {Count} playbacks on {Channel}", toStop.Count, channel);
    }

    /// <inheritdoc />
    public void StopAll()
    {
        foreach (var playback in _activePlaybacks.Values)
        {
            playback.Stop();
        }

        var count = _activePlaybacks.Count;
        _activePlaybacks.Clear();
        _logger.LogDebug("Stopped all playbacks (count={Count})", count);
    }

    /// <inheritdoc />
    public void Pause(Guid playbackId)
    {
        if (_activePlaybacks.TryGetValue(playbackId, out var playback))
        {
            playback.Pause();
            _logger.LogDebug("Paused playback: {Id}", playbackId);
        }
    }

    /// <inheritdoc />
    public void PauseChannel(AudioChannel channel)
    {
        var count = 0;
        foreach (var playback in _activePlaybacks.Values.Where(p => p.Channel == channel))
        {
            playback.Pause();
            count++;
        }

        _logger.LogDebug("Paused {Count} playbacks on {Channel}", count, channel);
    }

    /// <inheritdoc />
    public void Resume(Guid playbackId)
    {
        if (_activePlaybacks.TryGetValue(playbackId, out var playback))
        {
            playback.Resume();
            _logger.LogDebug("Resumed playback: {Id}", playbackId);
        }
    }

    /// <inheritdoc />
    public void ResumeChannel(AudioChannel channel)
    {
        var count = 0;
        foreach (var playback in _activePlaybacks.Values.Where(p => p.Channel == channel))
        {
            playback.Resume();
            count++;
        }

        _logger.LogDebug("Resumed {Count} playbacks on {Channel}", count, channel);
    }

    /// <inheritdoc />
    public void SetChannelVolume(AudioChannel channel, float volume)
    {
        var clampedVolume = Math.Clamp(volume, 0f, 1f);
        _channelVolumes[channel] = clampedVolume;

        UpdateActivePlaybackVolumes(channel);
        _logger.LogDebug("Set {Channel} volume to {Volume:P0}", channel, clampedVolume);
    }

    /// <inheritdoc />
    public float GetChannelVolume(AudioChannel channel) =>
        _channelVolumes.GetValueOrDefault(channel, 1.0f);

    /// <inheritdoc />
    public void SetChannelMuted(AudioChannel channel, bool muted)
    {
        _channelMuted[channel] = muted;

        if (channel == AudioChannel.Master)
        {
            if (muted)
            {
                PauseAllInternal();
            }
            else
            {
                ResumeAllInternal();
            }
        }

        _logger.LogDebug("Set {Channel} muted = {Muted}", channel, muted);
    }

    /// <inheritdoc />
    public bool IsChannelMuted(AudioChannel channel) =>
        _channelMuted.GetValueOrDefault(channel, false);

    /// <inheritdoc />
    public async Task PreloadAsync(IEnumerable<string> audioPaths, CancellationToken ct = default)
    {
        var count = 0;
        foreach (var path in audioPaths)
        {
            ct.ThrowIfCancellationRequested();
            _cache.GetOrLoad(path);
            count++;
        }

        _logger.LogDebug("Preloaded {Count} audio files", count);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        _cache.Clear();
        _logger.LogDebug("Audio cache cleared");
    }

    /// <inheritdoc />
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;

        if (!enabled)
        {
            StopAll();
        }

        _logger.LogInformation("Audio enabled = {Enabled}", enabled);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        StopAll();
        _cache.Clear();
        _disposed = true;

        _logger.LogInformation("AudioService disposed");
    }

    /// <summary>
    /// Calculates effective volume from master, channel, and base volume.
    /// </summary>
    private float CalculateEffectiveVolume(AudioChannel channel, float baseVolume) =>
        _channelVolumes[AudioChannel.Master] * _channelVolumes[channel] * baseVolume;

    /// <summary>
    /// Updates volumes for all active playbacks on a channel (or all if Master).
    /// </summary>
    private void UpdateActivePlaybackVolumes(AudioChannel channel)
    {
        var affected = channel == AudioChannel.Master
            ? _activePlaybacks.Values
            : _activePlaybacks.Values.Where(p => p.Channel == channel);

        foreach (var playback in affected)
        {
            var newVolume = CalculateEffectiveVolume(playback.Channel, playback.BaseVolume);
            playback.SetVolume(newVolume);
        }
    }

    private void PauseAllInternal()
    {
        foreach (var playback in _activePlaybacks.Values)
        {
            playback.Pause();
        }
    }

    private void ResumeAllInternal()
    {
        foreach (var playback in _activePlaybacks.Values)
        {
            playback.Resume();
        }
    }

    private static Dictionary<AudioChannel, float> InitializeDefaultVolumes() => new()
    {
        [AudioChannel.Master] = 0.8f,
        [AudioChannel.Music] = 0.6f,
        [AudioChannel.Effects] = 0.8f,
        [AudioChannel.UI] = 0.7f,
        [AudioChannel.Voice] = 0.9f
    };

    private static Dictionary<AudioChannel, bool> InitializeDefaultMutes() => new()
    {
        [AudioChannel.Master] = false,
        [AudioChannel.Music] = false,
        [AudioChannel.Effects] = false,
        [AudioChannel.UI] = false,
        [AudioChannel.Voice] = false
    };
}
