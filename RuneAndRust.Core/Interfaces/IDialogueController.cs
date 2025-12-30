using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ViewModels;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for the Dialogue UI controller.
/// Handles option navigation and selection operations.
/// </summary>
/// <remarks>See: v0.4.2d (The Parley) for Dialogue UI implementation.</remarks>
public interface IDialogueController
{
    /// <summary>
    /// Gets the currently selected option index (0-based).
    /// </summary>
    int SelectedIndex { get; }

    /// <summary>
    /// Gets the last selected option ID.
    /// </summary>
    string? LastSelectedOptionId { get; }

    /// <summary>
    /// Processes a key input and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The game phase to transition to.</returns>
    Task<GamePhase> HandleInputAsync(ConsoleKey key, Character character, GameState gameState);

    /// <summary>
    /// Builds the TUI-specific view model with current selection state.
    /// </summary>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The view model for rendering, or null if not in dialogue.</returns>
    Task<DialogueTuiViewModel?> BuildTuiViewModelAsync(Character character, GameState gameState);

    /// <summary>
    /// Resets the selection to the first option.
    /// </summary>
    void ResetSelection();

    /// <summary>
    /// Gets whether dialogue is currently active for a character.
    /// </summary>
    /// <param name="characterId">The character ID to check.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>True if in dialogue.</returns>
    bool IsInDialogue(Guid characterId, GameState gameState);
}
