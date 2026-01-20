using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace RuneAndRust.TestUtilities.Logging;

/// <summary>
/// Logger implementation that captures log entries for testing.
/// Provides methods to query and verify logged messages in unit tests.
/// </summary>
/// <typeparam name="T">The category type.</typeparam>
public class TestLogger<T> : ILogger<T>
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    /// <summary>
    /// Gets all captured log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> Entries => _entries.ToList();

    /// <summary>
    /// Gets entries at a specific log level.
    /// </summary>
    /// <param name="level">The log level to filter by.</param>
    /// <returns>Entries matching the specified level.</returns>
    public IEnumerable<LogEntry> GetEntries(LogLevel level) =>
        _entries.Where(e => e.Level == level);

    /// <summary>
    /// Checks if any entry contains the specified message fragment.
    /// </summary>
    /// <param name="fragment">The text to search for (case-insensitive).</param>
    /// <returns>True if any entry contains the fragment.</returns>
    public bool ContainsMessage(string fragment) =>
        _entries.Any(e => e.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if any entry at the specified level contains the message fragment.
    /// </summary>
    /// <param name="level">The log level to filter by.</param>
    /// <param name="fragment">The text to search for (case-insensitive).</param>
    /// <returns>True if any matching entry contains the fragment.</returns>
    public bool ContainsMessage(LogLevel level, string fragment) =>
        _entries.Any(e => e.Level == level &&
            e.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the count of entries at the specified level.
    /// </summary>
    /// <param name="level">The log level to count.</param>
    /// <returns>The number of entries at that level.</returns>
    public int GetCount(LogLevel level) => _entries.Count(e => e.Level == level);

    /// <summary>
    /// Clears all captured entries.
    /// </summary>
    public void Clear()
    {
        while (_entries.TryDequeue(out _)) { }
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        new TestScope();

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _entries.Enqueue(new LogEntry(
            logLevel,
            formatter(state, exception),
            exception,
            DateTime.UtcNow));
    }

    private sealed class TestScope : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// Represents a captured log entry.
/// </summary>
/// <param name="Level">The log level of the entry.</param>
/// <param name="Message">The formatted message.</param>
/// <param name="Exception">The exception, if any.</param>
/// <param name="Timestamp">When the entry was logged.</param>
public record LogEntry(
    LogLevel Level,
    string Message,
    Exception? Exception,
    DateTime Timestamp);
