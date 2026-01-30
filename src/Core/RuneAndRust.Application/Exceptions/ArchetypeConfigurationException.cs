// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeConfigurationException.cs
// Exception thrown when archetype configuration is invalid or cannot be loaded.
// Version: 0.17.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when archetype configuration is invalid or cannot be loaded.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Interfaces.IArchetypeProvider"/> implementations
/// when configuration loading or validation fails. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Configuration file not found at expected path</description></item>
///   <item><description>Invalid JSON syntax in configuration file</description></item>
///   <item><description>Missing required archetypes (expected exactly 4)</description></item>
///   <item><description>Duplicate archetype IDs in configuration</description></item>
///   <item><description>Invalid enum values that cannot be parsed (archetypeId, primaryResource, abilityType)</description></item>
///   <item><description>Missing required fields (displayName, description, combatRole, etc.)</description></item>
///   <item><description>Invalid resource bonus values (negative numbers)</description></item>
///   <item><description>Incorrect starting ability count (expected exactly 3 per archetype)</description></item>
///   <item><description>Empty or invalid specialization mappings</description></item>
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
///     var archetypes = provider.GetAllArchetypes();
/// }
/// catch (ArchetypeConfigurationException ex)
/// {
///     logger.LogError(ex, "Failed to load archetype configuration");
///     // Display user-friendly error or fall back to defaults
/// }
/// </code>
/// </example>
/// <seealso cref="Interfaces.IArchetypeProvider"/>
public class ArchetypeConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ArchetypeConfigurationException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public ArchetypeConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ArchetypeConfigurationException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ArchetypeConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
