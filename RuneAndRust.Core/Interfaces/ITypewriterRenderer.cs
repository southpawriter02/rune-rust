namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Renders text with typewriter animation effect (v0.3.4c).
/// Supports dynamic pacing and skip functionality for user convenience.
/// </summary>
public interface ITypewriterRenderer
{
    /// <summary>
    /// Plays a text sequence with typewriter effect.
    /// User can skip the animation by pressing any key.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="delayMs">Delay between characters in milliseconds. Default: 30ms.</param>
    /// <returns>A task that completes when the sequence finishes or is skipped.</returns>
    Task PlaySequenceAsync(string text, int delayMs = 30);
}
