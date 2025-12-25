using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages the debug console state and log buffer (v0.3.17a).
/// Thread-safe implementation for concurrent log access.
/// </summary>
public class DebugConsoleService : IDebugConsoleService
{
    private const int MaxLogHistory = 50;
    private const int MaxCommandHistory = 20;

    private readonly List<string> _logHistory = new();
    private readonly List<string> _commandHistory = new();
    private readonly object _lock = new();
    private readonly ILogger<DebugConsoleService> _logger;

    /// <summary>
    /// Gets whether the debug console is currently visible/active.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets the log history (read-only view).
    /// </summary>
    public IReadOnlyList<string> LogHistory
    {
        get
        {
            lock (_lock)
            {
                return _logHistory.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the command history for up-arrow recall.
    /// </summary>
    public IReadOnlyList<string> CommandHistory
    {
        get
        {
            lock (_lock)
            {
                return _commandHistory.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugConsoleService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DebugConsoleService(ILogger<DebugConsoleService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Toggles console visibility on/off.
    /// </summary>
    public void Toggle()
    {
        IsVisible = !IsVisible;
        _logger.LogTrace("[DEBUG] Console visibility set to {State}", IsVisible);
    }

    /// <summary>
    /// Writes a message to the log buffer.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="source">The source of the message (default: "System").</param>
    public void WriteLog(string message, string source = "System")
    {
        lock (_lock)
        {
            var entry = $"[{DateTime.Now:HH:mm:ss}] [{source}] {message}";
            _logHistory.Add(entry);

            if (_logHistory.Count > MaxLogHistory)
            {
                _logHistory.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Submits a command and adds to history.
    /// Also writes the command to the log with [User] source.
    /// </summary>
    /// <param name="command">The command to submit.</param>
    public void SubmitCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        _logger.LogDebug("[DEBUG] User submitted: {Command}", command);

        WriteLog(command, "User");

        lock (_lock)
        {
            _commandHistory.Add(command);

            if (_commandHistory.Count > MaxCommandHistory)
            {
                _commandHistory.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Clears the log buffer.
    /// </summary>
    public void ClearLog()
    {
        lock (_lock)
        {
            _logHistory.Clear();
        }

        _logger.LogTrace("[DEBUG] Log buffer cleared");
    }
}
