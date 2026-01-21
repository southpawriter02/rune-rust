using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.ValueObjects;

/// <summary>
/// Contains animation timing definitions for consistent UI animations.
/// </summary>
/// <remarks>
/// <para>AnimationTimings provides standardized durations for all animations
/// to ensure a consistent feel across the application.</para>
/// <para>Standard duration tiers:</para>
/// <list type="bullet">
///   <item><description>Short (100ms) - Quick feedback</description></item>
///   <item><description>Medium (250ms) - Standard transitions</description></item>
///   <item><description>Long (500ms) - Emphasis animations</description></item>
///   <item><description>Extended (1000ms) - Dramatic effects</description></item>
/// </list>
/// </remarks>
public class AnimationTimings
{
    private readonly Dictionary<AnimationKey, TimeSpan> _durations;

    /// <summary>
    /// Initializes a new AnimationTimings with the provided durations.
    /// </summary>
    /// <param name="durations">Dictionary of animation key to duration pairs.</param>
    private AnimationTimings(Dictionary<AnimationKey, TimeSpan> durations)
    {
        _durations = durations;
    }

    /// <summary>
    /// Creates the default animation timings.
    /// </summary>
    /// <returns>A new AnimationTimings with standard durations.</returns>
    public static AnimationTimings CreateDefault()
    {
        var durations = new Dictionary<AnimationKey, TimeSpan>
        {
            // Standard durations
            [AnimationKey.Short] = TimeSpan.FromMilliseconds(100),
            [AnimationKey.Medium] = TimeSpan.FromMilliseconds(250),
            [AnimationKey.Long] = TimeSpan.FromMilliseconds(500),
            [AnimationKey.Extended] = TimeSpan.FromMilliseconds(1000),

            // Specific animations
            [AnimationKey.HealthChange] = TimeSpan.FromMilliseconds(300),
            [AnimationKey.DamagePopup] = TimeSpan.FromMilliseconds(800),
            [AnimationKey.StatusEffect] = TimeSpan.FromMilliseconds(400),
            [AnimationKey.Notification] = TimeSpan.FromMilliseconds(3000),
            [AnimationKey.PanelSlide] = TimeSpan.FromMilliseconds(200),
            [AnimationKey.AchievementUnlock] = TimeSpan.FromMilliseconds(1500)
        };

        return new AnimationTimings(durations);
    }

    /// <summary>
    /// Gets the animation duration for the specified key.
    /// </summary>
    /// <param name="key">The animation key to retrieve.</param>
    /// <returns>The duration, or Medium (250ms) as fallback.</returns>
    public TimeSpan GetDuration(AnimationKey key) =>
        _durations.TryGetValue(key, out var duration)
            ? duration
            : TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Gets the duration in milliseconds for the specified key.
    /// </summary>
    /// <param name="key">The animation key to retrieve.</param>
    /// <returns>The duration in milliseconds.</returns>
    public int GetDurationMs(AnimationKey key) =>
        (int)GetDuration(key).TotalMilliseconds;

    /// <summary>
    /// Checks if the timings contain a duration for the specified key.
    /// </summary>
    /// <param name="key">The animation key to check.</param>
    /// <returns>True if the duration is defined; otherwise false.</returns>
    public bool ContainsDuration(AnimationKey key) => _durations.ContainsKey(key);
}
