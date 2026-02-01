// ═══════════════════════════════════════════════════════════════════════════════
// ITraumaRepository.cs
// Repository interface for CharacterTrauma entity persistence operations.
// Version: 0.18.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Repository interface for <see cref="CharacterTrauma"/> entity persistence operations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ITraumaRepository"/> provides the persistence contract for character
/// traumas acquired through the trauma system (v0.18.3). It supports trauma tracking,
/// stacking, and querying for retirement condition evaluation.
/// </para>
/// <para>
/// <strong>Persistence Flow (Trauma Acquisition):</strong>
/// </para>
/// <list type="number">
///   <item><description><c>TraumaService.AcquireTraumaAsync()</c> validates the trauma</description></item>
///   <item><description>Check existing trauma via <c>GetByCharacterAndTraumaIdAsync()</c></description></item>
///   <item><description>Create new or stack existing via <c>AddAsync()</c> or <c>UpdateAsync()</c></description></item>
///   <item><description><c>SaveChangesAsync()</c> persists changes to storage</description></item>
/// </list>
/// <para>
/// <strong>Implementations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><c>InMemoryTraumaRepository</c> — Thread-safe in-memory storage (testing)</description></item>
///   <item><description>EF Core <c>TraumaRepository</c> — Database persistence (production)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterTrauma"/>
public interface ITraumaRepository
{
    /// <summary>
    /// Gets all traumas for a specific character.
    /// </summary>
    /// <param name="characterId">The character to query traumas for.</param>
    /// <returns>
    /// A read-only list of all <see cref="CharacterTrauma"/> entities for the character.
    /// Returns empty list if character has no traumas.
    /// </returns>
    /// <remarks>
    /// Results are not ordered; caller should order as needed (typically by AcquiredAt).
    /// </remarks>
    /// <example>
    /// <code>
    /// var traumas = await repository.GetByCharacterIdAsync(characterId);
    /// var activeTraumas = traumas.Where(t => t.IsActive).ToList();
    /// </code>
    /// </example>
    Task<IReadOnlyList<CharacterTrauma>> GetByCharacterIdAsync(Guid characterId);

    /// <summary>
    /// Gets a specific trauma instance for a character.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <param name="traumaId">The trauma definition ID (e.g., "survivors-guilt").</param>
    /// <returns>
    /// The <see cref="CharacterTrauma"/> if found; <c>null</c> if character does not
    /// have this trauma.
    /// </returns>
    /// <remarks>
    /// <para>
    /// TraumaId is normalized to lowercase for case-insensitive comparison.
    /// </para>
    /// <para>
    /// Used to check if a character already has a trauma before acquisition,
    /// and to get stack count for stacking evaluation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = await repository.GetByCharacterAndTraumaIdAsync(charId, "reality-doubt");
    /// if (existing != null)
    ///     existing.IncrementStackCount();
    /// </code>
    /// </example>
    Task<CharacterTrauma?> GetByCharacterAndTraumaIdAsync(Guid characterId, string traumaId);

    /// <summary>
    /// Adds a new trauma instance to the repository.
    /// </summary>
    /// <param name="trauma">The <see cref="CharacterTrauma"/> entity to add.</param>
    /// <remarks>
    /// <para>
    /// Call <see cref="SaveChangesAsync"/> after adding to persist to storage.
    /// </para>
    /// <para>
    /// The trauma's CharacterId + TraumaDefinitionId combination must be unique.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var trauma = CharacterTrauma.Create(charId, "survivors-guilt", "AllyDeath", DateTime.UtcNow);
    /// await repository.AddAsync(trauma);
    /// await repository.SaveChangesAsync();
    /// </code>
    /// </example>
    Task AddAsync(CharacterTrauma trauma);

    /// <summary>
    /// Updates an existing trauma instance in the repository.
    /// </summary>
    /// <param name="trauma">The <see cref="CharacterTrauma"/> entity to update.</param>
    /// <remarks>
    /// <para>
    /// Call <see cref="SaveChangesAsync"/> after updating to persist to storage.
    /// </para>
    /// <para>
    /// Typically used for incrementing StackCount or toggling IsActive.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// existing.IncrementStackCount();
    /// await repository.UpdateAsync(existing);
    /// await repository.SaveChangesAsync();
    /// </code>
    /// </example>
    Task UpdateAsync(CharacterTrauma trauma);

    /// <summary>
    /// Persists all pending changes to the underlying storage.
    /// </summary>
    /// <returns>The number of entities written to storage.</returns>
    /// <remarks>
    /// Must be called after <see cref="AddAsync"/> or <see cref="UpdateAsync"/>
    /// to commit changes. Supports transactional semantics where available.
    /// </remarks>
    Task<int> SaveChangesAsync();
}
