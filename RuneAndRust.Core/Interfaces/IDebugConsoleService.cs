namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing the debug console state and log buffer (v0.3.17a).
/// Part of "The Architect" debug tools milestone.
/// </summary>
public interface IDebugConsoleService
{
    /// <summary>
    /// Gets whether the debug console is currently visible/active.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets the log history (read-only view).
    /// Limited to 50 entries (oldest removed when exceeded).
    /// </summary>
    IReadOnlyList<string> LogHistory { get; }

    /// <summary>
    /// Gets the command history for up-arrow recall.
    /// Limited to 20 commands (oldest removed when exceeded).
    /// </summary>
    IReadOnlyList<string> CommandHistory { get; }

    /// <summary>
    /// Toggles console visibility on/off.
    /// </summary>
    void Toggle();

    /// <summary>
    /// Writes a message to the log buffer.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="source">The source of the message (default: "System").</param>
    void WriteLog(string message, string source = "System");

    /// <summary>
    /// Submits a command and adds to history.
    /// Also writes the command to the log with [User] source.
    /// </summary>
    /// <param name="command">The command to submit.</param>
    void SubmitCommand(string command);

    /// <summary>
    /// Clears the log buffer.
    /// </summary>
    void ClearLog();
}
