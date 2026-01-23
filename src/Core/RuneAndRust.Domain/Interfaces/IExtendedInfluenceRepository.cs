// ------------------------------------------------------------------------------
// <copyright file="IExtendedInfluenceRepository.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Repository interface for managing extended influence tracking persistence.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Repository interface for managing extended influence tracking persistence.
/// </summary>
/// <remarks>
/// <para>
/// Provides CRUD operations for ExtendedInfluence entities with query methods
/// optimized for common access patterns:
/// <list type="bullet">
///   <item><description>By ID: Direct lookup of a specific influence</description></item>
///   <item><description>By Character: All influences for a player</description></item>
///   <item><description>By Target: All influences targeting a specific NPC</description></item>
///   <item><description>By Status: Filter by Active, Successful, Failed, Stalled</description></item>
///   <item><description>By Character+Target+Belief: Unique influence lookup</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IExtendedInfluenceRepository
{
    /// <summary>
    /// Adds a new extended influence to the repository.
    /// </summary>
    /// <param name="influence">The influence tracking to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if influence is null.
    /// </exception>
    void Add(ExtendedInfluence influence);

    /// <summary>
    /// Gets an extended influence by its unique ID.
    /// </summary>
    /// <param name="influenceId">The unique identifier.</param>
    /// <returns>The influence tracking, or null if not found.</returns>
    ExtendedInfluence? GetById(Guid influenceId);

    /// <summary>
    /// Gets an extended influence by character, target, and belief ID.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="beliefId">The belief ID.</param>
    /// <returns>The influence tracking, or null if not found.</returns>
    /// <remarks>
    /// This represents the natural unique key for an influence: one player
    /// can only have one active influence attempt per belief per NPC.
    /// </remarks>
    ExtendedInfluence? GetByCharacterTargetAndBelief(
        string characterId,
        string targetId,
        string beliefId);

    /// <summary>
    /// Gets all extended influences between a character and target.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <returns>List of all influences between the character and target.</returns>
    /// <remarks>
    /// An NPC may have multiple beliefs, so there could be multiple
    /// influence trackings between the same character and target.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetByCharacterAndTarget(
        string characterId,
        string targetId);

    /// <summary>
    /// Gets all extended influences for a player character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>List of all influences for the character.</returns>
    IReadOnlyList<ExtendedInfluence> GetByCharacter(string characterId);

    /// <summary>
    /// Gets all active (non-terminal) extended influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>List of active and stalled influences for the character.</returns>
    /// <remarks>
    /// Returns influences with status Active or Stalled (both are non-terminal).
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetActiveByCharacter(string characterId);

    /// <summary>
    /// Gets all extended influences targeting a specific NPC.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <returns>List of all influences targeting the NPC.</returns>
    /// <remarks>
    /// Useful for determining all players attempting to influence an NPC
    /// or for cleanup when an NPC is removed.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetByTarget(string targetId);

    /// <summary>
    /// Gets all extended influences with a specific status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <returns>List of influences with the specified status.</returns>
    IReadOnlyList<ExtendedInfluence> GetByStatus(InfluenceStatus status);

    /// <summary>
    /// Gets all stalled influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>List of stalled influences for the character.</returns>
    /// <remarks>
    /// Useful for displaying influences that require player action to resume.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetStalledByCharacter(string characterId);

    /// <summary>
    /// Gets all successful influences for a character.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <returns>List of successful influences for the character.</returns>
    /// <remarks>
    /// Useful for tracking achievements or displaying changed beliefs.
    /// </remarks>
    IReadOnlyList<ExtendedInfluence> GetSuccessfulByCharacter(string characterId);

    /// <summary>
    /// Updates an existing extended influence.
    /// </summary>
    /// <param name="influence">The influence to update.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if influence is null.
    /// </exception>
    /// <remarks>
    /// The influence must already exist in the repository.
    /// </remarks>
    void Update(ExtendedInfluence influence);

    /// <summary>
    /// Saves an extended influence (adds if new, updates if exists).
    /// </summary>
    /// <param name="influence">The influence to save.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if influence is null.
    /// </exception>
    /// <remarks>
    /// This is the preferred method for saving influences as it handles
    /// both creation and update scenarios.
    /// </remarks>
    void Save(ExtendedInfluence influence);

    /// <summary>
    /// Removes an extended influence from the repository.
    /// </summary>
    /// <param name="influenceId">The ID of the influence to remove.</param>
    /// <returns>True if the influence was removed; false if not found.</returns>
    bool Delete(Guid influenceId);

    /// <summary>
    /// Removes all influences for a specific character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The number of influences removed.</returns>
    /// <remarks>
    /// Useful for character deletion or data cleanup.
    /// </remarks>
    int DeleteByCharacter(string characterId);

    /// <summary>
    /// Removes all influences targeting a specific NPC.
    /// </summary>
    /// <param name="targetId">The target NPC ID.</param>
    /// <returns>The number of influences removed.</returns>
    /// <remarks>
    /// Useful for NPC deletion or data cleanup.
    /// </remarks>
    int DeleteByTarget(string targetId);

    /// <summary>
    /// Gets the total count of influences in the repository.
    /// </summary>
    /// <returns>The total number of influence trackings.</returns>
    int Count();

    /// <summary>
    /// Gets the count of influences with a specific status.
    /// </summary>
    /// <param name="status">The status to count.</param>
    /// <returns>The number of influences with that status.</returns>
    int CountByStatus(InfluenceStatus status);

    /// <summary>
    /// Checks if an influence exists for a character, target, and belief.
    /// </summary>
    /// <param name="characterId">The player character ID.</param>
    /// <param name="targetId">The target NPC ID.</param>
    /// <param name="beliefId">The belief ID.</param>
    /// <returns>True if an influence exists; otherwise false.</returns>
    bool Exists(string characterId, string targetId, string beliefId);
}
