namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Repository interface for examination state persistence.
/// </summary>
/// <remarks>
/// <para>
/// Manages the persistence of character-object examination state, tracking
/// the highest layer each character has unlocked for each object.
/// </para>
/// <para>
/// Key operations:
/// </para>
/// <list type="bullet">
///   <item><description>GetHighestLayer - Retrieve cached knowledge level</description></item>
///   <item><description>SetHighestLayer - Persist new knowledge level</description></item>
///   <item><description>GetAllForCharacter - List all objects examined by a character</description></item>
/// </list>
/// </remarks>
public interface IExaminationStateRepository
{
    /// <summary>
    /// Gets the highest examination layer a character has achieved for an object.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="objectId">The object identifier.</param>
    /// <returns>
    /// The highest layer unlocked (1, 2, or 3), or 0 if the character
    /// has never examined this object.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when IDs are null or whitespace.</exception>
    int GetHighestLayer(string characterId, string objectId);

    /// <summary>
    /// Sets or updates the highest examination layer for a character-object pair.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="objectId">The object identifier.</param>
    /// <param name="layer">The new highest layer (1, 2, or 3).</param>
    /// <remarks>
    /// If the character has already examined this object, only updates if
    /// the new layer is higher than the existing layer (layers never regress).
    /// Creates a new record if this is the first examination.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when IDs are null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when layer is out of range.</exception>
    void SetHighestLayer(string characterId, string objectId, int layer);

    /// <summary>
    /// Gets all examination states for a character.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <returns>
    /// A read-only list of all examination states for the character,
    /// or an empty list if the character has never examined anything.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when characterId is null or whitespace.</exception>
    IReadOnlyList<ExaminationState> GetAllForCharacter(string characterId);

    /// <summary>
    /// Gets the full examination state entity for a character-object pair.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="objectId">The object identifier.</param>
    /// <returns>
    /// The examination state if found, or null if the character has never
    /// examined this object.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when IDs are null or whitespace.</exception>
    ExaminationState? GetState(string characterId, string objectId);

    /// <summary>
    /// Clears all examination state for a character.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <remarks>
    /// Used primarily for testing or game reset scenarios.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when characterId is null or whitespace.</exception>
    void ClearForCharacter(string characterId);
}
