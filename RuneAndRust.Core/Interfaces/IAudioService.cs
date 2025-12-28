using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for audio playback management.
/// Handles volume filtering and delegates to platform-specific providers.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public interface IAudioService
{
    /// <summary>
    /// Plays a sound cue if audio is not muted.
    /// Respects GameSettings.MasterVolume for volume control.
    /// </summary>
    /// <param name="cue">The sound cue to play.</param>
    /// <returns>A task representing the asynchronous playback operation.</returns>
    Task PlayAsync(SoundCue cue);

    /// <summary>
    /// Plays a predefined system sound cue by its identifier.
    /// Looks up the cue from SoundCue static properties.
    /// </summary>
    /// <param name="cueId">The sound cue identifier (e.g., "ui_click", "combat_hit_light").</param>
    /// <returns>A task representing the asynchronous playback operation.</returns>
    Task PlaySystemCueAsync(string cueId);

    /// <summary>
    /// Gets whether audio is currently muted (MasterVolume = 0).
    /// </summary>
    bool IsMuted { get; }

    /// <summary>
    /// Gets the current master volume level (0-100).
    /// </summary>
    int MasterVolume { get; }
}
