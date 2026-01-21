using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Interfaces;

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides performance optimization for animations based on frame rate tracking
/// and accessibility settings.
/// </summary>
/// <remarks>
/// <para>Designed to ensure smooth user experience across varying hardware capabilities
/// by dynamically adjusting animation behavior based on measured performance.</para>
/// <para><b>Key Features:</b></para>
/// <list type="bullet">
/// <item><description>Respects reduced motion accessibility preference</description></item>
/// <item><description>Tracks frame rate using exponential moving average</description></item>
/// <item><description>Scales animation durations based on current performance</description></item>
/// <item><description>Provides frame skip recommendations for performance-critical areas</description></item>
/// </list>
/// <para><b>Logging:</b> Performance metrics are logged at Debug level.</para>
/// </remarks>
/// <example>
/// <code>
/// var optimizer = new AnimationOptimizer(accessibilityService, logger);
/// 
/// // In render loop
/// optimizer.TrackFrame();
/// 
/// // Check if animations should play
/// if (optimizer.ShouldAnimate())
/// {
///     var duration = optimizer.GetOptimizedDuration(TimeSpan.FromMilliseconds(300));
///     PlayAnimation(duration);
/// }
/// </code>
/// </example>
public class AnimationOptimizer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Target frame time in milliseconds for 60 FPS.
    /// </summary>
    private const double TargetFrameTimeMs = 16.67;

    /// <summary>
    /// Frame time threshold below which animations are disabled (30 FPS).
    /// </summary>
    private const double LowPowerThresholdMs = 33.33;

    /// <summary>
    /// Smoothing factor for exponential moving average.
    /// Higher values make the average more responsive to recent changes.
    /// </summary>
    private const double SmoothingAlpha = 0.1;

    /// <summary>
    /// Minimum animation duration in milliseconds to avoid zero-length animations.
    /// </summary>
    private const double MinAnimationDurationMs = 50.0;

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reference to accessibility service for reduced motion preference.
    /// </summary>
    private readonly IAccessibilityService? _accessibility;

    /// <summary>
    /// Optional logger for debug output.
    /// </summary>
    private readonly ILogger? _logger;

    /// <summary>
    /// Timestamp of last frame for calculating frame times.
    /// </summary>
    private DateTime _lastFrameTime;

    /// <summary>
    /// Exponential moving average of frame times.
    /// </summary>
    private double _averageFrameTime;

    /// <summary>
    /// Counter for periodic logging to avoid log spam.
    /// </summary>
    private int _logCounter;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationOptimizer"/> class.
    /// </summary>
    /// <param name="accessibility">
    /// Optional accessibility service to check reduced motion preference.
    /// </param>
    /// <param name="logger">Optional logger for debug output.</param>
    /// <remarks>
    /// If <paramref name="accessibility"/> is null, animations are always allowed
    /// unless performance degrades below the threshold.
    /// </remarks>
    public AnimationOptimizer(
        IAccessibilityService? accessibility = null,
        ILogger? logger = null)
    {
        _accessibility = accessibility;
        _logger = logger;
        _lastFrameTime = DateTime.UtcNow;
        _averageFrameTime = TargetFrameTimeMs; // Start with target FPS assumption
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the system is in low-power/performance mode.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> when frame rate drops below 30 FPS,
    /// indicating animations should be minimized or skipped.
    /// </remarks>
    public bool IsLowPowerMode => _averageFrameTime > LowPowerThresholdMs;

    /// <summary>
    /// Gets the current estimated frames per second.
    /// </summary>
    /// <remarks>
    /// Based on the exponential moving average of frame times.
    /// </remarks>
    public double CurrentFps => _averageFrameTime > 0 ? 1000.0 / _averageFrameTime : 60.0;

    /// <summary>
    /// Gets the current average frame time in milliseconds.
    /// </summary>
    public double AverageFrameTimeMs => _averageFrameTime;

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether animations should be played.
    /// </summary>
    /// <returns>
    /// <c>false</c> if reduced motion is enabled or performance is too low;
    /// <c>true</c> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>Animations are disabled when:</para>
    /// <list type="bullet">
    /// <item><description>Reduced motion accessibility setting is enabled</description></item>
    /// <item><description>Frame rate has dropped below 30 FPS</description></item>
    /// </list>
    /// </remarks>
    public bool ShouldAnimate()
    {
        // Respect reduced motion accessibility preference
        if (_accessibility?.IsReducedMotionEnabled == true)
        {
            return false;
        }

        // Skip animations if running at low frame rate
        if (IsLowPowerMode)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets an optimized animation duration based on current performance.
    /// </summary>
    /// <param name="baseDuration">The desired animation duration at target FPS.</param>
    /// <returns>
    /// An adjusted duration: zero if animations should be skipped,
    /// scaled down if performance is degraded, or the original if at target FPS.
    /// </returns>
    /// <remarks>
    /// <para>Duration scaling helps maintain smoothness when frame rate drops.</para>
    /// <para>For reduced motion or very low performance, returns <see cref="TimeSpan.Zero"/>
    /// to indicate the animation should be skipped entirely.</para>
    /// </remarks>
    public TimeSpan GetOptimizedDuration(TimeSpan baseDuration)
    {
        // Skip animation entirely for reduced motion preference
        if (_accessibility?.IsReducedMotionEnabled == true)
        {
            return TimeSpan.Zero;
        }

        // At or above target frame rate - use base duration
        if (_averageFrameTime <= TargetFrameTimeMs)
        {
            return baseDuration;
        }

        // Below low power threshold - skip animation
        if (IsLowPowerMode)
        {
            return TimeSpan.Zero;
        }

        // Scale duration proportionally to frame rate drop
        // e.g., at 30 FPS (33ms frame time), scale to ~50% of base duration
        var scale = TargetFrameTimeMs / _averageFrameTime;
        var scaledMs = baseDuration.TotalMilliseconds * scale;

        // Ensure minimum duration for visibility
        scaledMs = Math.Max(scaledMs, MinAnimationDurationMs);

        return TimeSpan.FromMilliseconds(scaledMs);
    }

    /// <summary>
    /// Determines how many frames to skip for better performance.
    /// </summary>
    /// <returns>
    /// Zero if at target frame rate; higher values suggest skipping
    /// more intermediate animation frames.
    /// </returns>
    /// <remarks>
    /// <para>Can be used to reduce animation complexity:</para>
    /// <list type="bullet">
    /// <item><description>0: Render every frame</description></item>
    /// <item><description>1: Skip every other frame</description></item>
    /// <item><description>2+: More aggressive frame skipping</description></item>
    /// </list>
    /// </remarks>
    public int GetFrameSkip()
    {
        if (_averageFrameTime <= TargetFrameTimeMs)
        {
            return 0;
        }

        // Calculate how many times slower than target we're running
        // e.g., at 30 FPS (33ms), skip = (33/16.67) - 1 ≈ 1
        return (int)((_averageFrameTime / TargetFrameTimeMs) - 1);
    }

    /// <summary>
    /// Updates frame time tracking. Call this once per frame.
    /// </summary>
    /// <remarks>
    /// <para>Uses exponential moving average to smooth out frame time measurements
    /// and avoid reacting to momentary spikes.</para>
    /// <para>Should be called consistently at the start or end of each frame.</para>
    /// </remarks>
    public void TrackFrame()
    {
        var now = DateTime.UtcNow;
        var frameTime = (now - _lastFrameTime).TotalMilliseconds;
        _lastFrameTime = now;

        // Ignore extremely long frame times (e.g., during debugging or app pause)
        if (frameTime > 1000.0)
        {
            return;
        }

        // Update exponential moving average
        // EMA = α × current + (1-α) × previous
        _averageFrameTime = _averageFrameTime == 0
            ? frameTime
            : (SmoothingAlpha * frameTime) + ((1 - SmoothingAlpha) * _averageFrameTime);

        // Periodic logging (every ~60 frames ≈ 1 second at 60FPS)
        _logCounter++;
        if (_logCounter >= 60)
        {
            _logCounter = 0;
            _logger?.LogDebug(
                "Animation optimizer: {FPS:F1} FPS, low power = {LowPower}",
                CurrentFps,
                IsLowPowerMode);
        }
    }

    /// <summary>
    /// Resets the frame tracking to initial state.
    /// </summary>
    /// <remarks>
    /// Useful after application resume or significant pause.
    /// </remarks>
    public void Reset()
    {
        _lastFrameTime = DateTime.UtcNow;
        _averageFrameTime = TargetFrameTimeMs;
        _logCounter = 0;
    }
}
