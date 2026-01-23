using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for handling cooperative and chained skill checks.
/// </summary>
/// <remarks>
/// <para>
/// This service extends the basic skill check functionality with two advanced mechanics:
/// </para>
/// <list type="bullet">
///   <item><description>Cooperative checks: Multiple participants working together</description></item>
///   <item><description>Chained checks: Sequential multi-step procedures</description></item>
/// </list>
/// <para>
/// Both mechanics integrate with master abilities from v0.15.1c and skill context
/// modifiers from v0.15.1a.
/// </para>
/// </remarks>
public interface IExtendedSkillCheckService
{
    #region Cooperative Checks

    /// <summary>
    /// Resolves a cooperative skill check with multiple participants.
    /// </summary>
    /// <param name="participants">The participating players.</param>
    /// <param name="skillId">The skill used for the check.</param>
    /// <param name="difficultyClass">The DC to meet or exceed.</param>
    /// <param name="cooperationType">How participants combine their efforts.</param>
    /// <param name="subType">Optional skill subtype for master ability filtering.</param>
    /// <param name="context">Optional shared context for all participants.</param>
    /// <returns>The cooperative check result.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when participants is empty.
    /// </exception>
    CooperativeCheckResult ResolveCooperativeCheck(
        IReadOnlyList<Player> participants,
        string skillId,
        int difficultyClass,
        CooperationType cooperationType,
        string? subType = null,
        SkillContext? context = null);

    /// <summary>
    /// Resolves a cooperative check with per-participant contexts.
    /// </summary>
    /// <param name="participants">Participants with their individual contexts.</param>
    /// <param name="skillId">The skill used for the check.</param>
    /// <param name="difficultyClass">The DC to meet or exceed.</param>
    /// <param name="cooperationType">How participants combine their efforts.</param>
    /// <param name="subType">Optional skill subtype.</param>
    /// <returns>The cooperative check result.</returns>
    CooperativeCheckResult ResolveCooperativeCheckWithContexts(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        CooperationType cooperationType,
        string? subType = null);

    #endregion

    #region Chained Checks

    /// <summary>
    /// Starts a new chained skill check.
    /// </summary>
    /// <param name="player">The player performing the chain.</param>
    /// <param name="chainName">Display name for the chain.</param>
    /// <param name="steps">The steps in sequence.</param>
    /// <param name="targetId">Optional target identifier.</param>
    /// <returns>The initial chain state, ready for first step.</returns>
    ChainedCheckState StartChainedCheck(
        Player player,
        string chainName,
        IReadOnlyList<ChainedCheckStep> steps,
        string? targetId = null);

    /// <summary>
    /// Processes the current step of a chained check.
    /// </summary>
    /// <param name="player">The player performing the check.</param>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <param name="stepContext">Optional context overriding the step's default.</param>
    /// <returns>Result containing updated state and step outcome.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the chain is not in a processable state.
    /// </exception>
    ChainedCheckProcessResult ProcessChainStep(
        Player player,
        string checkId,
        SkillContext? stepContext = null);

    /// <summary>
    /// Retries the current failed step of a chained check.
    /// </summary>
    /// <param name="player">The player performing the retry.</param>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <param name="stepContext">Optional context for the retry.</param>
    /// <returns>Result containing updated state and step outcome.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no retry is available.
    /// </exception>
    ChainedCheckProcessResult RetryChainStep(
        Player player,
        string checkId,
        SkillContext? stepContext = null);

    /// <summary>
    /// Abandons a chained check in progress.
    /// </summary>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <returns>The final abandoned state.</returns>
    ChainedCheckState AbandonChain(string checkId);

    /// <summary>
    /// Gets the current state of a chained check.
    /// </summary>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <returns>The chain state, or null if not found.</returns>
    ChainedCheckState? GetChainState(string checkId);

    /// <summary>
    /// Gets all active chains for a character.
    /// </summary>
    /// <param name="characterId">The character's ID.</param>
    /// <returns>Collection of active chain states.</returns>
    IReadOnlyList<ChainedCheckState> GetActiveChainsForCharacter(string characterId);

    #endregion
}

/// <summary>
/// Result of processing a step in a chained check.
/// </summary>
/// <param name="State">The updated chain state.</param>
/// <param name="StepResult">The skill check result for this step.</param>
/// <param name="IsChainComplete">Whether the chain has reached a terminal state.</param>
/// <param name="Message">Display message for the result.</param>
public readonly record struct ChainedCheckProcessResult(
    ChainedCheckState State,
    SkillCheckResult StepResult,
    bool IsChainComplete,
    string Message);
