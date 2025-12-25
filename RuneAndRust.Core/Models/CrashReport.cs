namespace RuneAndRust.Core.Models;

/// <summary>
/// Data Transfer Object for crash report data (v0.3.16a).
/// Captures diagnostic information when a critical exception occurs.
/// Used by CrashService to generate human-readable crash log files.
/// </summary>
public record CrashReport
{
    /// <summary>
    /// Timestamp when the crash occurred.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The fully qualified type name of the exception (e.g., "System.NullReferenceException").
    /// </summary>
    public string ExceptionType { get; init; } = string.Empty;

    /// <summary>
    /// The exception message describing what went wrong.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The full stack trace showing the call hierarchy at the time of the crash.
    /// </summary>
    public string StackTrace { get; init; } = string.Empty;

    /// <summary>
    /// Inner exception details as a formatted string, if any.
    /// Null if the exception has no inner exception.
    /// </summary>
    public string? InnerException { get; init; }

    /// <summary>
    /// Application version at time of crash (e.g., "v0.3.16a").
    /// </summary>
    public string GameVersion { get; init; } = string.Empty;

    /// <summary>
    /// Operating system description from Environment.OSVersion.
    /// </summary>
    public string OperatingSystem { get; init; } = string.Empty;

    /// <summary>
    /// .NET runtime version from Environment.Version.
    /// </summary>
    public string RuntimeVersion { get; init; } = string.Empty;
}
