using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for platform-specific audio playback.
/// Implementations handle the actual sound generation (Console.Beep, audio files, etc.).
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public interface IAudioProvider
{
    /// <summary>
    /// Plays a sound cue using the platform-specific audio mechanism.
    /// This method should not block the calling thread.
    /// </summary>
    /// <param name="cue">The sound cue to play.</param>
    /// <param name="masterVolume">The master volume level (0-100).</param>
    /// <returns>A task representing the asynchronous playback operation.</returns>
    Task PlayAsync(SoundCue cue, int masterVolume);

    /// <summary>
    /// Gets whether audio playback is supported on the current platform.
    /// May return false on headless systems or unsupported OS configurations.
    /// </summary>
    bool IsSupported { get; }
}
