namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for ambient soundscape generation (v0.3.19c).
/// Provides turn-based procedural audio cues based on room biome.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public interface IAmbienceService
{
    /// <summary>
    /// Gets whether ambient soundscape is currently enabled.
    /// Reflects GameSettings.AmbienceEnabled value.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Updates the ambience system for the current turn.
    /// May trigger an ambient sound cue based on turn interval and room biome.
    /// </summary>
    /// <param name="roomId">The current room's unique identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Guid roomId);

    /// <summary>
    /// Resets the turn counter and randomizes the next trigger threshold.
    /// Call when entering a new room or starting a new game.
    /// </summary>
    void Reset();
}
