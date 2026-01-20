namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Implementation of the music service managing background music playback.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Track playback with looping support</description></item>
///   <item><description>Theme-based music selection</description></item>
///   <item><description>Stinger one-shot audio with pause/resume</description></item>
///   <item><description>Volume control via AudioChannel.Music</description></item>
///   <item><description>Track and theme change events</description></item>
/// </list>
/// </para>
/// </remarks>
public class MusicService : IMusicService
{
    private readonly IAudioService _audioService;
    private readonly ILogger<MusicService> _logger;

    private Guid _currentPlaybackId;
    private MusicTheme _currentTheme = MusicTheme.None;
    private string? _currentTrack;
    private bool _isPaused;
    private bool _disposed;

    // Stinger state
    private Guid _stingerPlaybackId;
    private string? _trackBeforeStinger;
    private bool _wasPlayingBeforeStinger;
    private Action? _stingerCallback;

    /// <inheritdoc />
    public MusicTheme CurrentTheme => _currentTheme;

    /// <inheritdoc />
    public string? CurrentTrack => _currentTrack;

    /// <inheritdoc />
    public bool IsPlaying => _currentPlaybackId != Guid.Empty && !_isPaused;

    /// <inheritdoc />
    public bool IsPaused => _isPaused;

    /// <inheritdoc />
    public event Action<string>? OnTrackChanged;

    /// <inheritdoc />
    public event Action<MusicTheme>? OnThemeChanged;

    /// <summary>
    /// Creates a new music service.
    /// </summary>
    /// <param name="audioService">Audio service for playback.</param>
    /// <param name="logger">Logger for music operations.</param>
    public MusicService(IAudioService audioService, ILogger<MusicService> logger)
    {
        _audioService = audioService;
        _logger = logger;
        _logger.LogDebug("MusicService initialized");
    }

    /// <inheritdoc />
    public void PlayTrack(string trackPath, bool loop = true)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot play track: service disposed");
            return;
        }

        // Stop current music
        if (_currentPlaybackId != Guid.Empty)
        {
            _audioService.Stop(_currentPlaybackId);
        }

        // Start new track on Music channel
        _currentPlaybackId = _audioService.Play(trackPath, AudioChannel.Music, 1.0f, loop);
        _currentTrack = trackPath;
        _isPaused = false;

        _logger.LogInformation("Playing music: {Track}, Loop: {Loop}", trackPath, loop);
        OnTrackChanged?.Invoke(trackPath);
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_currentPlaybackId != Guid.Empty)
        {
            _audioService.Stop(_currentPlaybackId);
            _currentPlaybackId = Guid.Empty;
            _currentTrack = null;
            _isPaused = false;
            _logger.LogDebug("Music stopped");
        }
    }

    /// <inheritdoc />
    public void Pause()
    {
        if (_currentPlaybackId != Guid.Empty && !_isPaused)
        {
            _audioService.Pause(_currentPlaybackId);
            _isPaused = true;
            _logger.LogDebug("Music paused");
        }
    }

    /// <inheritdoc />
    public void Resume()
    {
        if (_currentPlaybackId != Guid.Empty && _isPaused)
        {
            _audioService.Resume(_currentPlaybackId);
            _isPaused = false;
            _logger.LogDebug("Music resumed");
        }
    }

    /// <inheritdoc />
    public void SetVolume(float volume)
    {
        var clamped = Math.Clamp(volume, 0f, 1f);
        _audioService.SetChannelVolume(AudioChannel.Music, clamped);
        _logger.LogDebug("Music volume set to {Volume:P0}", clamped);
    }

    /// <inheritdoc />
    public float GetVolume() => _audioService.GetChannelVolume(AudioChannel.Music);

    /// <inheritdoc />
    public void SetTheme(MusicTheme theme)
    {
        if (theme == _currentTheme)
        {
            _logger.LogDebug("Theme {Theme} already active, skipping", theme);
            return;
        }

        var previousTheme = _currentTheme;
        _currentTheme = theme;

        if (theme == MusicTheme.None)
        {
            Stop();
        }

        _logger.LogInformation("Theme changed: {From} → {To}", previousTheme, theme);
        OnThemeChanged?.Invoke(theme);
    }

    /// <inheritdoc />
    public void PlayStinger(string stingerPath, Action? onComplete = null)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot play stinger: service disposed");
            onComplete?.Invoke();
            return;
        }

        // Save current state
        _trackBeforeStinger = _currentTrack;
        _wasPlayingBeforeStinger = IsPlaying;
        _stingerCallback = onComplete;

        // Pause current music if playing
        if (IsPlaying)
        {
            Pause();
        }

        // Play stinger (no loop)
        _stingerPlaybackId = _audioService.Play(stingerPath, AudioChannel.Music, 1.0f, loop: false);
        _logger.LogDebug("Playing stinger: {Path}", stingerPath);

        // Note: In a real implementation, we would subscribe to playback completion
        // to resume music and trigger callback. For now, stinger plays over paused music.
    }

    /// <inheritdoc />
    public async Task PreloadAsync(IEnumerable<string> trackPaths, CancellationToken ct = default)
    {
        await _audioService.PreloadAsync(trackPaths, ct);
        _logger.LogDebug("Preloaded music tracks");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        Stop();

        if (_stingerPlaybackId != Guid.Empty)
        {
            _audioService.Stop(_stingerPlaybackId);
        }

        _disposed = true;
        _logger.LogInformation("MusicService disposed");
    }
}
