// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundConfigurationException.cs
// Exception thrown when background configuration is invalid or cannot be loaded.
// Version: 0.17.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when background configuration is invalid or cannot be loaded.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Interfaces.IBackgroundProvider"/> implementations
/// when configuration loading or validation fails. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration file not found at expected path</description></item>
///   <item><description>Invalid JSON syntax in configuration file</description></item>
///   <item><description>Missing required backgrounds (expected exactly 6)</description></item>
///   <item><description>Duplicate background IDs in configuration</description></item>
///   <item><description>Invalid enum values that cannot be parsed</description></item>
///   <item><description>Missing required fields (displayName, description, etc.)</description></item>
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
///     var backgrounds = provider.GetAllBackgrounds();
/// }
/// catch (BackgroundConfigurationException ex)
/// {
///     logger.LogError(ex, "Failed to load background configuration");
///     // Display user-friendly error or fall back to defaults
/// }
/// </code>
/// </example>
public class BackgroundConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="BackgroundConfigurationException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public BackgroundConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BackgroundConfigurationException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public BackgroundConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
