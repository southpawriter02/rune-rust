// ═══════════════════════════════════════════════════════════════════════════════
// SaveResult.cs
// Represents the result of a persistence operation (save or update) on an entity.
// Contains either a successful outcome with the saved entity's ID or a failure
// with an error message describing what went wrong (name collision, database error,
// entity not found for update, etc.). Used by IPlayerRepository to communicate
// persistence outcomes to the CharacterCreationController.
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a persistence operation on an entity.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaveResult"/> is returned by repository methods such as
/// <c>IPlayerRepository.SaveAsync()</c> and <c>IPlayerRepository.UpdateAsync()</c>
/// to indicate whether the persistence operation succeeded or failed.
/// </para>
/// <para>
/// <strong>Success scenario:</strong> The <see cref="EntityId"/> property contains
/// the persisted entity's unique identifier, and <see cref="ErrorMessage"/> is null.
/// </para>
/// <para>
/// <strong>Failure scenarios:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Name collision — a character with the same name already exists</description></item>
///   <item><description>Database error — the underlying storage operation failed</description></item>
///   <item><description>Entity not found — an update was attempted on a non-existent entity</description></item>
/// </list>
/// <para>
/// Instances are created exclusively through the <see cref="Succeeded"/> and
/// <see cref="Failed"/> factory methods. Use <see cref="Success"/> to check
/// the result before proceeding.
/// </para>
/// </remarks>
/// <seealso cref="Entities.Player"/>
public readonly record struct SaveResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the persistence operation succeeded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the entity was persisted successfully; <c>false</c>
    /// if the operation failed due to a constraint violation, database error,
    /// or other issue.
    /// </value>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the saved entity's unique identifier if successful.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> of the persisted entity when <see cref="Success"/>
    /// is <c>true</c>; <c>null</c> when the operation failed.
    /// </value>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    /// <value>
    /// A user-friendly error message describing why the operation failed,
    /// or <c>null</c> when <see cref="Success"/> is <c>true</c>.
    /// </value>
    public string? ErrorMessage { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful persistence result.
    /// </summary>
    /// <param name="entityId">The unique identifier of the persisted entity.</param>
    /// <returns>
    /// A <see cref="SaveResult"/> with <see cref="Success"/> set to <c>true</c>,
    /// the provided <paramref name="entityId"/>, and a null error message.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = SaveResult.Succeeded(player.Id);
    /// // result.Success == true
    /// // result.EntityId == player.Id
    /// // result.ErrorMessage == null
    /// </code>
    /// </example>
    public static SaveResult Succeeded(Guid entityId) => new()
    {
        Success = true,
        EntityId = entityId,
        ErrorMessage = null
    };

    /// <summary>
    /// Creates a failed persistence result.
    /// </summary>
    /// <param name="errorMessage">A user-friendly description of the failure.</param>
    /// <returns>
    /// A <see cref="SaveResult"/> with <see cref="Success"/> set to <c>false</c>,
    /// a null <see cref="EntityId"/>, and the provided error message.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = SaveResult.Failed("A character named 'Bjorn' already exists.");
    /// // result.Success == false
    /// // result.EntityId == null
    /// // result.ErrorMessage == "A character named 'Bjorn' already exists."
    /// </code>
    /// </example>
    public static SaveResult Failed(string errorMessage) => new()
    {
        Success = false,
        EntityId = null,
        ErrorMessage = errorMessage
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// "Saved: {EntityId}" when successful, or "Failed: {ErrorMessage}" when failed.
    /// </returns>
    public override string ToString() =>
        Success
            ? $"Saved: {EntityId}"
            : $"Failed: {ErrorMessage}";
}
