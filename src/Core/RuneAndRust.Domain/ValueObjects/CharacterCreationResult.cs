// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationResult.cs
// Represents the final result of character creation, returned by
// ICharacterCreationController.ConfirmCharacterAsync(). Contains either the
// successfully created Player character or validation errors explaining why
// creation failed (invalid name, incomplete state, factory error).
// Version: 0.17.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Represents the final result of character creation.
/// Contains either the created character or validation errors.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CharacterCreationResult"/> is returned by
/// <c>ICharacterCreationController.ConfirmCharacterAsync()</c> after the player
/// enters a name and confirms character creation at Step 6 (Summary). The result
/// indicates whether creation succeeded and provides either the created
/// <see cref="Player"/> entity or error details.
/// </para>
/// <para>
/// <strong>Success scenario:</strong> The <see cref="Character"/> property contains
/// the newly created <see cref="Player"/> entity, and <see cref="Message"/> contains
/// a flavor text confirmation (e.g., "Bjorn begins their saga.").
/// </para>
/// <para>
/// <strong>Failure scenarios:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Name validation failed — <see cref="Message"/> contains the error from <c>INameValidator</c></description></item>
///   <item><description>Incomplete state — not all steps completed before confirmation</description></item>
///   <item><description>No active session — <c>ConfirmCharacterAsync</c> called without <c>Initialize()</c></description></item>
///   <item><description>Factory unavailable — character factory not yet implemented (v0.17.5e)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="Player"/>
public readonly record struct CharacterCreationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether character creation succeeded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character was created successfully (or is ready for
    /// creation pending factory implementation); <c>false</c> if creation failed.
    /// </value>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the created character if successful. Null if creation failed.
    /// </summary>
    /// <value>
    /// The newly created <see cref="Player"/> entity, or <c>null</c> when
    /// <see cref="Success"/> is <c>false</c> or when the character factory
    /// is not yet available (v0.17.5e).
    /// </value>
    public Player? Character { get; init; }

    /// <summary>
    /// Gets the success or error message.
    /// </summary>
    /// <value>
    /// On success, a flavor text confirmation (e.g., "Bjorn begins their saga.").
    /// On failure, a description of what went wrong.
    /// </value>
    public string Message { get; init; }

    /// <summary>
    /// Gets validation errors if creation failed. Empty if successful.
    /// </summary>
    /// <value>
    /// A read-only list of user-friendly error messages. Empty (not null)
    /// when <see cref="Success"/> is <c>true</c>.
    /// </value>
    public IReadOnlyList<string> ValidationErrors { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful creation result.
    /// </summary>
    /// <param name="character">
    /// The created <see cref="Player"/> entity. May be <c>null</c> when the
    /// character factory is not yet available (pending v0.17.5e).
    /// </param>
    /// <param name="message">
    /// A confirmation message (e.g., "Bjorn begins their saga.").
    /// </param>
    /// <returns>
    /// A <see cref="CharacterCreationResult"/> with <see cref="Success"/> set
    /// to <c>true</c> and an empty validation errors list.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = CharacterCreationResult.Succeeded(
    ///     player,
    ///     "Bjorn begins their saga.");
    /// // result.Success == true
    /// // result.Character == player
    /// </code>
    /// </example>
    public static CharacterCreationResult Succeeded(Player? character, string message) => new()
    {
        Success = true,
        Character = character,
        Message = message,
        ValidationErrors = Array.Empty<string>()
    };

    /// <summary>
    /// Creates a failed creation result.
    /// </summary>
    /// <param name="message">A description of what went wrong.</param>
    /// <param name="errors">One or more validation error messages.</param>
    /// <returns>
    /// A <see cref="CharacterCreationResult"/> with <see cref="Success"/> set
    /// to <c>false</c> and <see cref="Character"/> set to <c>null</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = CharacterCreationResult.Failed(
    ///     "Name is required.",
    ///     "Name is required.");
    /// // result.Success == false
    /// // result.Character == null
    /// </code>
    /// </example>
    public static CharacterCreationResult Failed(string message, params string[] errors) => new()
    {
        Success = false,
        Character = null,
        Message = message,
        ValidationErrors = errors
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// "Created: {Message}" when successful, or "Failed: {Message}" when creation failed.
    /// </returns>
    public override string ToString() =>
        Success
            ? $"Created: {Message}"
            : $"Failed: {Message}";
}
