namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing fumble consequences.
/// </summary>
public interface IFumbleConsequenceService
{
    /// <summary>
    /// Creates a new fumble consequence based on the skill check context.
    /// </summary>
    /// <param name="characterId">The character who fumbled.</param>
    /// <param name="skillId">The skill that was fumbled.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <param name="context">The skill context for additional information.</param>
    /// <returns>The created fumble consequence.</returns>
    FumbleConsequence CreateConsequence(
        string characterId,
        string skillId,
        string? targetId,
        SkillContext? context);

    /// <summary>
    /// Creates a new fumble consequence with a specific type and description.
    /// </summary>
    /// <param name="characterId">The character who fumbled.</param>
    /// <param name="skillId">The skill that was fumbled.</param>
    /// <param name="fumbleType">The specific type of fumble consequence.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <param name="description">Custom description for the consequence.</param>
    /// <returns>The created fumble consequence.</returns>
    /// <remarks>
    /// <para>
    /// Use this overload when the fumble type is predetermined (e.g., lockpicking
    /// always results in [Mechanism Jammed]) rather than derived from skill ID.
    /// </para>
    /// </remarks>
    FumbleConsequence CreateConsequence(
        string characterId,
        string skillId,
        FumbleType fumbleType,
        string? targetId,
        string? description = null);

    /// <summary>
    /// Gets all active consequences for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of active fumble consequences.</returns>
    IReadOnlyList<FumbleConsequence> GetActiveConsequences(string characterId);

    /// <summary>
    /// Gets active consequences that affect a specific skill check.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="skillId">The skill being used.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <returns>List of consequences affecting the check.</returns>
    IReadOnlyList<FumbleConsequence> GetConsequencesAffectingCheck(
        string characterId,
        string skillId,
        string? targetId);

    /// <summary>
    /// Determines if any active consequence blocks a skill check.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="skillId">The skill being used.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <returns>True if the check is blocked; otherwise, false.</returns>
    bool IsCheckBlocked(string characterId, string skillId, string? targetId);

    /// <summary>
    /// Attempts to recover from a consequence by checking completed conditions.
    /// </summary>
    /// <param name="consequenceId">The consequence to recover from.</param>
    /// <param name="completedConditions">List of completed condition identifiers.</param>
    /// <returns>True if recovery was successful; otherwise, false.</returns>
    bool TryRecover(string consequenceId, IEnumerable<string> completedConditions);

    /// <summary>
    /// Deactivates a consequence manually (e.g., by GM or special ability).
    /// </summary>
    /// <param name="consequenceId">The consequence to deactivate.</param>
    /// <param name="reason">The reason for deactivation.</param>
    void DeactivateConsequence(string consequenceId, string reason);

    /// <summary>
    /// Processes time-based expiration of consequences.
    /// </summary>
    /// <param name="currentTime">The current game time.</param>
    /// <returns>List of consequences that were expired.</returns>
    IReadOnlyList<FumbleConsequence> ProcessExpirations(DateTime currentTime);

    /// <summary>
    /// Gets the total dice penalty from all active consequences for a skill check.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="skillId">The skill being used.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <returns>The total dice penalty (negative value).</returns>
    int GetTotalDicePenalty(string characterId, string skillId, string? targetId);

    /// <summary>
    /// Gets the total DC modifier from all active consequences for a skill check.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="skillId">The skill being used.</param>
    /// <param name="targetId">The optional target of the skill check.</param>
    /// <returns>The total DC modifier (positive = harder).</returns>
    int GetTotalDcModifier(string characterId, string skillId, string? targetId);
}
