namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for handling critical application failures (v0.3.16a).
/// Provides crash logging and report generation when unhandled exceptions occur.
/// </summary>
/// <remarks>See: SPEC-CRASH-001 for Crash Handling System design.</remarks>
public interface ICrashService
{
    /// <summary>
    /// Logs a crash to the crash log directory.
    /// Creates a timestamped crash report file with exception details,
    /// stack trace, and system information.
    /// </summary>
    /// <param name="ex">The exception that caused the crash.</param>
    /// <returns>The path to the generated crash log file.</returns>
    string LogCrash(Exception ex);

    /// <summary>
    /// Generates the file path for a crash report based on timestamp.
    /// </summary>
    /// <param name="timestamp">The time of the crash.</param>
    /// <returns>The full path for the crash log file (e.g., logs/crashes/crash_20251225_143045.txt).</returns>
    string GenerateReportPath(DateTime timestamp);
}
