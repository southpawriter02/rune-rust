using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for playing screen transition animations between game phases (v0.3.14b).
/// Implements ASCII-based visual effects using Spectre.Console.Live rendering.
/// </summary>
/// <remarks>See: SPEC-TRANSITION-001 for Screen Transition System design.</remarks>
public interface IScreenTransitionService
{
    /// <summary>
    /// Plays a transition animation. Blocks until complete.
    /// Respects GameSettings.ReduceMotion - returns immediately if enabled.
    /// </summary>
    /// <param name="type">The transition effect to play.</param>
    /// <returns>A task that completes when the transition finishes.</returns>
    Task PlayAsync(TransitionType type);
}
