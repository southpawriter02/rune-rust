// ═══════════════════════════════════════════════════════════════════════════════
// AttributeConfigurationException.cs
// Exception thrown when attribute configuration is invalid or cannot be loaded.
// Version: 0.17.2e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when attribute configuration is invalid or cannot be loaded.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Interfaces.IAttributeProvider"/> implementations
/// when configuration loading or validation fails. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration file not found at expected path</description></item>
///   <item><description>Invalid JSON syntax in configuration file</description></item>
///   <item><description>Missing required attributes (expected exactly 5)</description></item>
///   <item><description>Duplicate attribute IDs in configuration</description></item>
///   <item><description>Invalid enum values that cannot be parsed to CoreAttribute</description></item>
///   <item><description>Missing required recommended builds (expected exactly 4)</description></item>
///   <item><description>Invalid or missing point-buy configuration</description></item>
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
///     var descriptions = provider.GetAllAttributeDescriptions();
/// }
/// catch (AttributeConfigurationException ex)
/// {
///     logger.LogError(ex, "Failed to load attribute configuration");
///     // Display user-friendly error or fall back to defaults
/// }
/// </code>
/// </example>
public class AttributeConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="AttributeConfigurationException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public AttributeConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AttributeConfigurationException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public AttributeConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
