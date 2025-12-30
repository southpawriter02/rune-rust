using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing dialogue sessions and conversation flow.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public interface IDialogueService
{
    /// <summary>
    /// Starts a new dialogue session with an NPC.
    /// </summary>
    /// <param name="character">The character initiating dialogue.</param>
    /// <param name="treeId">The dialogue tree identifier.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The result of starting the dialogue.</returns>
    Task<DialogueStartResult> StartDialogueAsync(
        Character character,
        string treeId,
        GameState gameState);

    /// <summary>
    /// Selects a dialogue option and advances the conversation.
    /// </summary>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="optionId">The option ID to select.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The result of selecting the option.</returns>
    Task<DialogueStepResult> SelectOptionAsync(
        Character character,
        string optionId,
        GameState gameState);

    /// <summary>
    /// Ends the current dialogue session.
    /// </summary>
    /// <param name="reason">The reason for ending the dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The result of ending the dialogue.</returns>
    Task<DialogueEndResult> EndDialogueAsync(
        DialogueEndReason reason,
        GameState gameState);

    /// <summary>
    /// Gets the current dialogue view model without advancing.
    /// </summary>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The current dialogue view model, or null if no active dialogue.</returns>
    Task<DialogueViewModel?> GetCurrentDialogueAsync(
        Character character,
        GameState gameState);

    /// <summary>
    /// Checks if a character is currently in a dialogue session.
    /// </summary>
    /// <param name="characterId">The character ID to check.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>True if the character is in dialogue.</returns>
    bool IsInDialogue(Guid characterId, GameState gameState);
}
