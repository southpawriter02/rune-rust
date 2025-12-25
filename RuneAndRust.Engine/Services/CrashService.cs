using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Handles crash logging and diagnostics (v0.3.16a).
/// Writes timestamped crash reports to logs/crashes/ directory.
/// Part of "The Safety Net" crash recovery system.
/// </summary>
/// <remarks>See: SPEC-CRASH-001 for Crash Handling System design.</remarks>
public class CrashService : ICrashService
{
    private readonly ILogger<CrashService> _logger;
    private const string CrashDirectory = "logs/crashes";
    private const string GameVersion = "v0.3.16a";

    /// <summary>
    /// Initializes a new instance of the <see cref="CrashService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public CrashService(ILogger<CrashService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GenerateReportPath(DateTime timestamp)
        => Path.Combine(CrashDirectory, $"crash_{timestamp:yyyyMMdd_HHmmss}.txt");

    /// <inheritdoc/>
    public string LogCrash(Exception ex)
    {
        _logger.LogTrace("[CRASH] LogCrash invoked for {ExType}", ex.GetType().Name);

        // Ensure crash directory exists
        Directory.CreateDirectory(CrashDirectory);

        // Build crash report
        var report = new CrashReport
        {
            Timestamp = DateTime.Now,
            ExceptionType = ex.GetType().FullName ?? ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace ?? "No stack trace available.",
            InnerException = ex.InnerException?.ToString(),
            GameVersion = GameVersion,
            OperatingSystem = Environment.OSVersion.ToString(),
            RuntimeVersion = Environment.Version.ToString()
        };

        // Generate file path and write report
        var path = GenerateReportPath(report.Timestamp);
        var content = FormatReport(report);

        File.WriteAllText(path, content);

        _logger.LogInformation("[CRASH] Crash report written to {Path}", path);

        return path;
    }

    /// <summary>
    /// Formats the crash report as a human-readable text file.
    /// </summary>
    /// <param name="report">The crash report data to format.</param>
    /// <returns>Formatted crash report string.</returns>
    private static string FormatReport(CrashReport report)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        sb.AppendLine("                         RUNE & RUST CRASH REPORT");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        sb.AppendLine();

        // System Information
        sb.AppendLine($"Timestamp:      {report.Timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Game Version:   {report.GameVersion}");
        sb.AppendLine($"OS:             {report.OperatingSystem}");
        sb.AppendLine($".NET Runtime:   {report.RuntimeVersion}");
        sb.AppendLine();

        // Exception Details
        sb.AppendLine("───────────────────────────────────────────────────────────────────────────────");
        sb.AppendLine("EXCEPTION DETAILS");
        sb.AppendLine("───────────────────────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"Type:    {report.ExceptionType}");
        sb.AppendLine($"Message: {report.Message}");
        sb.AppendLine();

        // Stack Trace
        sb.AppendLine("STACK TRACE:");
        sb.AppendLine("─────────────");
        sb.AppendLine(report.StackTrace);

        // Inner Exception (if present)
        if (!string.IsNullOrEmpty(report.InnerException))
        {
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────────────────────────────────────────────");
            sb.AppendLine("INNER EXCEPTION");
            sb.AppendLine("───────────────────────────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine(report.InnerException);
        }

        // Footer
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
        sb.AppendLine("Please report this issue at:");
        sb.AppendLine("https://github.com/southpawriter02/rune-rust/issues");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════════");

        return sb.ToString();
    }
}
