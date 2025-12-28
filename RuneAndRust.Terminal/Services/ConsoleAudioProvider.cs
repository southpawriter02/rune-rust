using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Console-based audio provider using system beeps for TUI feedback (v0.3.19a).
/// Uses Console.Beep on Windows, falls back to ANSI bell on Unix/macOS.
/// </summary>
/// <remarks>See: SPEC-AUDIO-001 for Audio System design.</remarks>
public class ConsoleAudioProvider : IAudioProvider
{
    private readonly ILogger<ConsoleAudioProvider> _logger;
    private readonly bool _isWindows;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleAudioProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for traceability.</param>
    public ConsoleAudioProvider(ILogger<ConsoleAudioProvider> logger)
    {
        _logger = logger;
        _isWindows = OperatingSystem.IsWindows();

        _logger.LogInformation(
            "[Audio] ConsoleAudioProvider initialized (Windows: {IsWindows})",
            _isWindows);
    }

    /// <inheritdoc/>
    public bool IsSupported => true; // Console bell works on all platforms

    /// <inheritdoc/>
    public async Task PlayAsync(SoundCue cue, int masterVolume)
    {
        // Console.Beep doesn't support volume amplitude,
        // so we treat masterVolume as a simple on/off gate here.
        // Volume filtering is done by AudioService before calling this.

        if (_isWindows)
        {
            // Windows supports frequency/duration via Console.Beep
            // Run in background task to prevent blocking the UI thread
            await Task.Run(() =>
            {
                try
                {
                    // Console.Beep frequency range: 37-32767 Hz
                    var frequency = Math.Clamp(cue.Frequency, 37, 32767);
                    var duration = Math.Max(1, cue.DurationMs);

                    Console.Beep(frequency, duration);

                    _logger.LogTrace(
                        "[Audio] Played beep: {Freq}Hz for {Dur}ms",
                        frequency, duration);
                }
                catch (Exception ex)
                {
                    // Swallow hardware errors (no speakers, headless system, etc.)
                    _logger.LogTrace(
                        ex,
                        "[Audio] Console.Beep failed: {Message}",
                        ex.Message);
                }
            });
        }
        else
        {
            // Unix/macOS: Use ANSI bell character (basic beep, no frequency control)
            try
            {
                Console.Write('\a');

                _logger.LogTrace(
                    "[Audio] Sent ANSI bell for {CueId}",
                    cue.Id);
            }
            catch (Exception ex)
            {
                _logger.LogTrace(
                    ex,
                    "[Audio] ANSI bell failed: {Message}",
                    ex.Message);
            }

            // Small delay to simulate duration on non-Windows platforms
            // This prevents rapid-fire bell spam
            await Task.Delay(Math.Min(cue.DurationMs, 100));
        }
    }
}
