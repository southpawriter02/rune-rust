using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Handles combat animation playback.
/// </summary>
public interface ICombatAnimator
{
    /// <summary>
    /// Queues an animation for playback.
    /// </summary>
    /// <param name="type">Type of animation to play.</param>
    /// <param name="context">Context for template variable substitution.</param>
    void QueueAnimation(AnimationType type, AnimationContext context);
    
    /// <summary>
    /// Plays all queued animations sequentially.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when all animations finish.</returns>
    Task PlayQueuedAnimationsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Skips the currently playing animation.
    /// </summary>
    void SkipCurrent();
    
    /// <summary>
    /// Clears all queued animations.
    /// </summary>
    void ClearQueue();
    
    /// <summary>
    /// Gets or sets whether animations are enabled.
    /// </summary>
    bool AnimationsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the animation speed multiplier.
    /// </summary>
    /// <remarks>
    /// 1.0 = normal speed, 2.0 = double speed, 0.5 = half speed.
    /// </remarks>
    float SpeedMultiplier { get; set; }
    
    /// <summary>
    /// Gets whether an animation is currently playing.
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Gets the number of animations in the queue.
    /// </summary>
    int QueueCount { get; }
}
