using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents the result of a data capture generation operation.
/// Contains the generated capture and metadata about the discovery.
/// </summary>
/// <param name="Success">Whether a capture was successfully generated.</param>
/// <param name="Message">Descriptive message about the capture discovery.</param>
/// <param name="Capture">The generated data capture, if any.</param>
/// <param name="WasAutoAssigned">Whether the capture was automatically assigned to a Codex entry.</param>
public record CaptureResult(
    bool Success,
    string Message,
    DataCapture? Capture,
    bool WasAutoAssigned)
{
    /// <summary>
    /// Creates a successful capture result with the generated capture.
    /// </summary>
    /// <param name="message">The capture discovery message.</param>
    /// <param name="capture">The generated data capture.</param>
    /// <param name="autoAssigned">Whether the capture was auto-assigned to a Codex entry.</param>
    /// <returns>A successful capture result.</returns>
    public static CaptureResult Generated(string message, DataCapture capture, bool autoAssigned)
    {
        return new CaptureResult(true, message, capture, autoAssigned);
    }

    /// <summary>
    /// Creates a result indicating no capture was generated.
    /// </summary>
    /// <param name="message">The message explaining why no capture was found.</param>
    /// <returns>A no-capture result.</returns>
    public static CaptureResult NoCapture(string message)
    {
        return new CaptureResult(false, message, null, false);
    }
}
