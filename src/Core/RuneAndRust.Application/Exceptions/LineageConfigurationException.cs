// ═══════════════════════════════════════════════════════════════════════════════
// LineageConfigurationException.cs
// Exception thrown when lineage configuration is invalid or cannot be loaded.
// Version: 0.17.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when lineage configuration is invalid or cannot be loaded.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Interfaces.ILineageProvider"/> implementations
/// when configuration loading or validation fails. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration file not found at expected path</description></item>
///   <item><description>Invalid JSON syntax in configuration file</description></item>
///   <item><description>Missing required lineages (expected exactly 4)</description></item>
///   <item><description>Duplicate lineage IDs in configuration</description></item>
///   <item><description>Invalid enum values that cannot be parsed</description></item>
/// </list>
/// <para>
/// The exception message provides specific details about what validation failed
/// to help game administrators quickly identify and fix configuration issues.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     var lineages = provider.GetAllLineages();
/// }
/// catch (LineageConfigurationException ex)
/// {
///     logger.LogError(ex, "Failed to load lineage configuration");
///     // Display user-friendly error or fall back to defaults
/// }
/// </code>
/// </example>
public class LineageConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="LineageConfigurationException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public LineageConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LineageConfigurationException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public LineageConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
