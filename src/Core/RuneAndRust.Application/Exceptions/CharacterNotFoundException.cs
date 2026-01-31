// ═══════════════════════════════════════════════════════════════════════════════
// CharacterNotFoundException.cs
// Exception thrown when a character cannot be found by the specified identifier.
// Version: 0.18.0c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Exceptions;

/// <summary>
/// Exception thrown when a character cannot be found by the specified identifier.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by service implementations when an operation targets
/// a character that does not exist in the repository. Common causes include:
/// </para>
/// <list type="bullet">
///   <item><description>Character ID does not match any persisted character</description></item>
///   <item><description>Character was deleted or retired before the operation</description></item>
///   <item><description>Incorrect character ID passed from a stale UI reference</description></item>
///   <item><description>Character creation was not completed before querying</description></item>
/// </list>
/// <para>
/// The exception message provides specific details about which character ID
/// was not found and, optionally, the operation that was being attempted.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     var stressState = stressService.GetStressState(characterId);
/// }
/// catch (CharacterNotFoundException ex)
/// {
///     logger.LogWarning(ex, "Character not found during stress query");
///     // Display user-friendly error or redirect to character selection
/// }
/// </code>
/// </example>
/// <seealso cref="Interfaces.IStressService"/>
public class CharacterNotFoundException : Exception
{
    /// <summary>
    /// Gets the character identifier that was not found.
    /// </summary>
    /// <remarks>
    /// Contains the <see cref="Guid"/> that was passed to the service method
    /// which triggered this exception. May be <see cref="Guid.Empty"/> if the
    /// exception was created without specifying an ID.
    /// </remarks>
    public Guid CharacterId { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CharacterNotFoundException"/> with a message.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public CharacterNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CharacterNotFoundException"/> with a message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public CharacterNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CharacterNotFoundException"/> with the
    /// character identifier that was not found.
    /// </summary>
    /// <param name="characterId">The character identifier that could not be located.</param>
    public CharacterNotFoundException(Guid characterId)
        : base($"Character with ID '{characterId}' was not found.")
    {
        CharacterId = characterId;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CharacterNotFoundException"/> with the
    /// character identifier and a custom message.
    /// </summary>
    /// <param name="characterId">The character identifier that could not be located.</param>
    /// <param name="message">The error message describing what went wrong.</param>
    public CharacterNotFoundException(Guid characterId, string message)
        : base(message)
    {
        CharacterId = characterId;
    }
}
