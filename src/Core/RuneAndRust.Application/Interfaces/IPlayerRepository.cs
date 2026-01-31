// ═══════════════════════════════════════════════════════════════════════════════
// IPlayerRepository.cs
// Interface defining the contract for Player entity persistence operations.
// Provides methods for saving newly created characters, retrieving characters
// by ID or most recent, checking name uniqueness, updating existing characters,
// and listing all saved characters. Implementations may use in-memory storage
// (InMemoryPlayerRepository) or a database backend (future EF Core implementation).
// Version: 0.17.5g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Repository interface for <see cref="Player"/> entity persistence operations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IPlayerRepository"/> provides the persistence contract for characters
/// created through the character creation workflow (v0.17.5). It supports the full
/// lifecycle: saving newly created characters, loading them for gameplay, checking
/// name uniqueness, updating state during gameplay, and listing all saved characters
/// for a character selection screen.
/// </para>
/// <para>
/// <strong>Persistence Flow (Character Creation):</strong>
/// </para>
/// <list type="number">
///   <item><description><c>CharacterCreationController.ConfirmCharacterAsync()</c> validates the name</description></item>
///   <item><description><c>ICharacterFactory.CreateCharacterAsync()</c> assembles the Player entity</description></item>
///   <item><description><c>IPlayerRepository.SaveAsync()</c> persists the Player to storage</description></item>
///   <item><description><c>SaveResult</c> communicates success/failure back to the controller</description></item>
/// </list>
/// <para>
/// <strong>Name Uniqueness:</strong> The <see cref="ExistsWithNameAsync"/> method
/// performs a case-insensitive check to prevent duplicate character names. The
/// controller should call this before creating the character, and <see cref="SaveAsync"/>
/// also validates uniqueness as a safety net.
/// </para>
/// <para>
/// <strong>Implementations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><c>InMemoryPlayerRepository</c> — Thread-safe in-memory storage using ConcurrentDictionary (current)</description></item>
///   <item><description>EF Core <c>PlayerRepository</c> — Database persistence with SQLite/PostgreSQL (future)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="Player"/>
/// <seealso cref="SaveResult"/>
public interface IPlayerRepository
{
    /// <summary>
    /// Saves a newly created player to persistent storage.
    /// </summary>
    /// <param name="player">
    /// The <see cref="Player"/> entity to save. Must not be null. The player's
    /// <see cref="Player.Id"/> is used as the unique identifier. The player's
    /// <see cref="Player.Name"/> must be unique (case-insensitive).
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// A <see cref="SaveResult"/> indicating success (with the player's ID) or
    /// failure (with an error message describing the issue, such as a name collision).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method validates name uniqueness before persisting. If a character
    /// with the same name (case-insensitive) already exists, the operation fails
    /// with an appropriate error message.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await repository.SaveAsync(player);
    /// if (result.Success)
    ///     logger.LogInformation("Player saved: {Id}", result.EntityId);
    /// else
    ///     logger.LogWarning("Save failed: {Error}", result.ErrorMessage);
    /// </code>
    /// </example>
    Task<SaveResult> SaveAsync(Player player, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a player by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the player to retrieve.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// The <see cref="Player"/> entity if found; <c>null</c> if no player
    /// exists with the specified <paramref name="id"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var player = await repository.GetByIdAsync(playerId);
    /// if (player != null)
    ///     logger.LogInformation("Loaded: {Name}", player.Name);
    /// </code>
    /// </example>
    Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the most recently created or played character.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// The most recent <see cref="Player"/> entity if any characters exist;
    /// <c>null</c> if no characters have been saved.
    /// </returns>
    /// <remarks>
    /// Used by the "Continue" option on the main menu to quickly load the
    /// player's last active character without requiring a character selection screen.
    /// </remarks>
    Task<Player?> GetMostRecentAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if a character with the given name already exists (case-insensitive).
    /// </summary>
    /// <param name="name">The character name to check for uniqueness.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// <c>true</c> if a character with the specified name exists (case-insensitive
    /// comparison); <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// Called by the controller before character creation to provide early feedback
    /// on name availability. Also used internally by <see cref="SaveAsync"/> as
    /// a safety net to prevent race conditions.
    /// </remarks>
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing player's state in persistent storage.
    /// </summary>
    /// <param name="player">
    /// The <see cref="Player"/> entity with updated state. The player must
    /// already exist in storage (matched by <see cref="Player.Id"/>).
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// A <see cref="SaveResult"/> indicating success or failure. Fails if the
    /// player does not exist in storage.
    /// </returns>
    /// <remarks>
    /// Used during gameplay to persist progress (HP changes, inventory updates,
    /// experience gains, etc.). Does not re-validate name uniqueness since the
    /// player already exists.
    /// </remarks>
    Task<SaveResult> UpdateAsync(Player player, CancellationToken ct = default);

    /// <summary>
    /// Gets all saved characters, ordered by most recently played/created first.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// A read-only list of all saved <see cref="Player"/> entities, ordered
    /// by most recent first. Returns an empty list if no characters exist.
    /// </returns>
    /// <remarks>
    /// Used by the character selection screen to display all available characters.
    /// The ordering places the most recently active character first for convenience.
    /// </remarks>
    Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default);
}
