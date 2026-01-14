using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines an animation sequence with frames.
/// </summary>
/// <param name="Type">Type of animation.</param>
/// <param name="Frames">Ordered list of animation frames.</param>
/// <param name="DefaultDurationMs">Default frame duration if not specified.</param>
/// <param name="Verbs">Optional verbs for {verb} substitution.</param>
public record AnimationDefinition(
    AnimationType Type,
    IReadOnlyList<AnimationFrame> Frames,
    int DefaultDurationMs = 150,
    IReadOnlyList<string>? Verbs = null)
{
    /// <summary>
    /// Gets the total animation duration in milliseconds.
    /// </summary>
    public int TotalDurationMs => Frames.Sum(f => f.DurationMs);
    
    /// <summary>
    /// Gets the frame count.
    /// </summary>
    public int FrameCount => Frames.Count;
}
