namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Manages smooth audio transitions between music tracks.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Linear volume interpolation for crossfades</description></item>
///   <item><description>Cancellable transitions</description></item>
///   <item><description>Stinger ducking with automatic restore</description></item>
///   <item><description>Fade in/out for game pause</description></item>
/// </list>
/// </para>
/// </remarks>
public class CrossfadeManager : ICrossfadeManager, IDisposable
{
    private readonly IMusicService _musicService;
    private readonly IMusicThemeConfig _themeConfig;
    private readonly ILogger<CrossfadeManager> _logger;
    private readonly TransitionSettings _settings;

    private CancellationTokenSource? _transitionCts;
    private float _originalVolume;
    private bool _isTransitioning;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsTransitioning => _isTransitioning;

    /// <summary>
    /// Creates a new crossfade manager.
    /// </summary>
    /// <param name="musicService">Music service for playback control.</param>
    /// <param name="themeConfig">Theme configuration for track lookup.</param>
    /// <param name="logger">Logger for transition operations.</param>
    public CrossfadeManager(
        IMusicService musicService,
        IMusicThemeConfig themeConfig,
        ILogger<CrossfadeManager> logger)
    {
        _musicService = musicService;
        _themeConfig = themeConfig;
        _logger = logger;
        _settings = new TransitionSettings();
        _logger.LogDebug("CrossfadeManager initialized");
    }

    /// <summary>
    /// Creates a new crossfade manager with custom settings.
    /// </summary>
    public CrossfadeManager(
        IMusicService musicService,
        IMusicThemeConfig themeConfig,
        TransitionSettings settings,
        ILogger<CrossfadeManager> logger)
    {
        _musicService = musicService;
        _themeConfig = themeConfig;
        _settings = settings;
        _logger = logger;
        _logger.LogDebug("CrossfadeManager initialized with custom settings");
    }

    /// <inheritdoc />
    public void CrossfadeTo(string newTrack, float duration, Action? onComplete = null)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot crossfade: manager disposed");
            return;
        }

        Cancel();
        _transitionCts = new CancellationTokenSource();
        _isTransitioning = true;

        _ = ExecuteCrossfadeAsync(newTrack, duration, onComplete, _transitionCts.Token);
        _logger.LogDebug("Started crossfade to {Track} over {Duration}s", newTrack, duration);
    }

    /// <inheritdoc />
    public void CrossfadeToTheme(MusicTheme theme, float duration)
    {
        var track = _themeConfig.GetTrackForTheme(theme);
        if (track is null)
        {
            _logger.LogWarning("No track for theme {Theme}", theme);
            return;
        }

        // Check for intro track
        var intro = _themeConfig.GetIntroTrack(theme);
        if (intro is not null)
        {
            // Play intro first, then crossfade to main
            _logger.LogDebug("Playing intro track before main theme {Theme}", theme);
            CrossfadeTo(intro, duration, () =>
            {
                _musicService.PlayTrack(track, loop: true);
            });
        }
        else
        {
            CrossfadeTo(track, duration);
        }
    }

    /// <inheritdoc />
    public void FadeOut(float duration)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot fade out: manager disposed");
            return;
        }

        Cancel();
        _transitionCts = new CancellationTokenSource();
        _isTransitioning = true;

        _ = ExecuteFadeAsync(fadeIn: false, duration, _transitionCts.Token);
        _logger.LogDebug("Started fade out over {Duration}s", duration);
    }

    /// <inheritdoc />
    public void FadeIn(float duration)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot fade in: manager disposed");
            return;
        }

        Cancel();
        _transitionCts = new CancellationTokenSource();
        _isTransitioning = true;

        _ = ExecuteFadeAsync(fadeIn: true, duration, _transitionCts.Token);
        _logger.LogDebug("Started fade in over {Duration}s", duration);
    }

    /// <inheritdoc />
    public void DuckAndPlay(string stingerPath, float duckLevel = 0.2f, Action? onComplete = null)
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot duck and play: manager disposed");
            onComplete?.Invoke();
            return;
        }

        _originalVolume = _musicService.GetVolume();

        // Duck background music
        var duckedVolume = _originalVolume * Math.Clamp(duckLevel, 0f, 1f);
        _musicService.SetVolume(duckedVolume);

        // Play stinger
        _musicService.PlayStinger(stingerPath, () =>
        {
            // Restore volume after delay
            _ = RestoreVolumeAfterDelayAsync(onComplete);
        });

        _logger.LogDebug("Ducked to {Level:P0} and playing stinger {Path}", duckLevel, stingerPath);
    }

    /// <inheritdoc />
    public void Cancel()
    {
        if (_transitionCts is not null)
        {
            _transitionCts.Cancel();
            _transitionCts.Dispose();
            _transitionCts = null;
            _logger.LogDebug("Cancelled active transition");
        }
        _isTransitioning = false;
    }

    private async Task ExecuteCrossfadeAsync(
        string newTrack,
        float duration,
        Action? onComplete,
        CancellationToken ct)
    {
        try
        {
            const int steps = 20;
            var stepDuration = duration / steps;
            _originalVolume = _musicService.GetVolume();

            // Fade out current track
            for (int i = steps; i >= 0 && !ct.IsCancellationRequested; i--)
            {
                var vol = _originalVolume * (i / (float)steps);
                _musicService.SetVolume(vol);
                await Task.Delay(TimeSpan.FromSeconds(stepDuration), ct);
            }

            if (ct.IsCancellationRequested)
            {
                _logger.LogDebug("Crossfade cancelled during fade out");
                return;
            }

            // Switch to new track
            _musicService.PlayTrack(newTrack, loop: true);

            // Fade in new track
            for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
            {
                var vol = _originalVolume * (i / (float)steps);
                _musicService.SetVolume(vol);
                await Task.Delay(TimeSpan.FromSeconds(stepDuration), ct);
            }

            // Ensure final volume is restored
            _musicService.SetVolume(_originalVolume);
            _logger.LogDebug("Crossfade complete: {Track}", newTrack);
            onComplete?.Invoke();
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Crossfade cancelled");
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async Task ExecuteFadeAsync(bool fadeIn, float duration, CancellationToken ct)
    {
        try
        {
            const int steps = 20;
            var stepDuration = duration / steps;

            if (fadeIn)
            {
                // Start from current (possibly low) volume
                _musicService.Resume();
            }
            else
            {
                _originalVolume = _musicService.GetVolume();
            }

            for (int i = 0; i <= steps && !ct.IsCancellationRequested; i++)
            {
                var progress = i / (float)steps;
                float vol;

                if (fadeIn)
                {
                    // Fade from 0 to original
                    vol = _originalVolume * progress;
                }
                else
                {
                    // Fade from original to 0
                    vol = _originalVolume * (1 - progress);
                }

                _musicService.SetVolume(vol);
                await Task.Delay(TimeSpan.FromSeconds(stepDuration), ct);
            }

            if (!fadeIn && !ct.IsCancellationRequested)
            {
                _musicService.Pause();
            }

            _logger.LogDebug("Fade {Direction} complete", fadeIn ? "in" : "out");
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Fade cancelled");
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async Task RestoreVolumeAfterDelayAsync(Action? onComplete)
    {
        await Task.Delay(TimeSpan.FromSeconds(_settings.StingerResumeDelay));
        _musicService.SetVolume(_originalVolume);
        _logger.LogDebug("Volume restored after stinger");
        onComplete?.Invoke();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        Cancel();
        _disposed = true;
        _logger.LogInformation("CrossfadeManager disposed");
    }
}
