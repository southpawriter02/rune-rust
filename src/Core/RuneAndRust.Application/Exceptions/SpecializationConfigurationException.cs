// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationConfigurationException.cs
// Exception thrown when specialization configuration is invalid or cannot be loaded.
// Version: 0.17.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when specialization configuration is invalid or cannot be loaded.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Interfaces.ISpecializationProvider"/> implementations
/// when configuration loading or validation fails. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration file not found at expected path</description></item>
///   <item><description>Invalid JSON syntax in configuration file</description></item>
///   <item><description>Missing required specializations (expected exactly 17)</description></item>
///   <item><description>Duplicate specialization IDs in configuration</description></item>
///   <item><description>Invalid enum values that cannot be parsed (specializationId, parentArchetype, pathType)</description></item>
///   <item><description>Missing required fields (displayName, tagline, description, selectionText)</description></item>
///   <item><description>Invalid special resource values (negative numbers, invalid range)</description></item>
///   <item><description>Invalid ability tier structure (duplicate tiers, invalid tier numbers)</description></item>
///   <item><description>Mismatched path type or parent archetype (configuration disagrees with enum extension methods)</description></item>
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
///     var specializations = provider.GetAll();
/// }
/// catch (SpecializationConfigurationException ex)
/// {
///     logger.LogError(ex, "Failed to load specialization configuration");
///     // Display user-friendly error or fall back to defaults
/// }
/// </code>
/// </example>
/// <seealso cref="Interfaces.ISpecializationProvider"/>
public class SpecializationConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="SpecializationConfigurationException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public SpecializationConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SpecializationConfigurationException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public SpecializationConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
