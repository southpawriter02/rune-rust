using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ViewModels;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Terminal.Controllers;

/// <summary>
/// Controller for dialogue TUI input handling (v0.4.2d).
/// Manages option selection and navigation via keyboard.
/// </summary>
/// <remarks>See: v0.4.2d (The Parley) for Dialogue TUI implementation.</remarks>
public class DialogueController : IDialogueController
{
    private readonly IDialogueService _dialogueService;
    private readonly IDialogueScreenRenderer _renderer;
    private readonly ILogger<DialogueController> _logger;

    /// <summary>
    /// Gets the currently selected option index (0-based).
    /// </summary>
    public int SelectedIndex { get; private set; }

    /// <summary>
    /// Gets the last selected option ID (for result retrieval).
    /// </summary>
    public string? LastSelectedOptionId { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogueController"/> class.
    /// </summary>
    /// <param name="dialogueService">The dialogue service for conversation logic.</param>
    /// <param name="renderer">The renderer for locked feedback.</param>
    /// <param name="logger">The logger for traceability.</param>
    public DialogueController(
        IDialogueService dialogueService,
        IDialogueScreenRenderer renderer,
        ILogger<DialogueController> logger)
    {
        _dialogueService = dialogueService ?? throw new ArgumentNullException(nameof(dialogueService));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a keypress and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The game phase to transition to (Dialogue to stay, Exploration to exit).</returns>
    public async Task<GamePhase> HandleInputAsync(ConsoleKey key, Character character, GameState gameState)
    {
        _logger.LogTrace("[Dialogue TUI] Key pressed: {Key}", key);

        // Get current dialogue state
        var viewModel = await _dialogueService.GetCurrentDialogueAsync(character, gameState);
        if (viewModel == null)
        {
            _logger.LogWarning("[Dialogue TUI] HandleInput called with no active dialogue");
            return GamePhase.Exploration;
        }

        var optionCount = viewModel.Options.Count;

        switch (key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                    _logger.LogTrace("[Dialogue TUI] Selection moved up to index {Index}", SelectedIndex);
                }
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                if (SelectedIndex < optionCount - 1)
                {
                    SelectedIndex++;
                    _logger.LogTrace("[Dialogue TUI] Selection moved down to index {Index}", SelectedIndex);
                }
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                return await HandleSelectionAsync(viewModel, character, gameState);

            case ConsoleKey.Escape:
            case ConsoleKey.Q:
                _logger.LogInformation("[Dialogue TUI] Player cancelled dialogue via Escape");
                await _dialogueService.EndDialogueAsync(DialogueEndReason.PlayerCancel, gameState);
                ResetSelection();
                return GamePhase.Exploration;

            // Number keys for quick selection (1-9)
            case ConsoleKey.D1:
            case ConsoleKey.D2:
            case ConsoleKey.D3:
            case ConsoleKey.D4:
            case ConsoleKey.D5:
            case ConsoleKey.D6:
            case ConsoleKey.D7:
            case ConsoleKey.D8:
            case ConsoleKey.D9:
                var numIndex = (int)key - (int)ConsoleKey.D1;
                if (numIndex < optionCount)
                {
                    SelectedIndex = numIndex;
                    return await HandleSelectionAsync(viewModel, character, gameState);
                }
                break;
        }

        return GamePhase.Dialogue; // Stay in dialogue
    }

    /// <summary>
    /// Handles the selection of the currently highlighted option.
    /// </summary>
    private async Task<GamePhase> HandleSelectionAsync(
        DialogueViewModel viewModel,
        Character character,
        GameState gameState)
    {
        if (SelectedIndex < 0 || SelectedIndex >= viewModel.Options.Count)
        {
            _logger.LogWarning("[Dialogue TUI] Invalid selection index: {Index}", SelectedIndex);
            return GamePhase.Dialogue;
        }

        var selectedOption = viewModel.Options[SelectedIndex];

        // Check if option is available
        if (!selectedOption.IsAvailable)
        {
            _logger.LogDebug("[Dialogue TUI] Attempted to select locked option: {OptionId}", selectedOption.OptionId);
            _renderer.PlayLockedFeedback(selectedOption.LockedReason ?? "Requirements not met");
            return GamePhase.Dialogue;
        }

        // Select the option
        _logger.LogInformation("[Dialogue TUI] Selecting option: '{Text}'",
            TruncateText(selectedOption.Text, 30));

        LastSelectedOptionId = selectedOption.OptionId;
        var result = await _dialogueService.SelectOptionAsync(character, selectedOption.OptionId, gameState);

        if (!result.Success)
        {
            _logger.LogWarning("[Dialogue TUI] Option selection failed: {Error}", result.ErrorMessage);
            return GamePhase.Dialogue;
        }

        // Reset selection for next node
        ResetSelection();

        // Check if dialogue ended
        if (result.DialogueEnded)
        {
            _logger.LogInformation("[Dialogue TUI] Dialogue completed via terminal option");
            return GamePhase.Exploration;
        }

        return GamePhase.Dialogue;
    }

    /// <summary>
    /// Builds a TUI-specific view model with current selection state.
    /// </summary>
    /// <param name="character">The character in dialogue.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The view model for rendering, or null if not in dialogue.</returns>
    public async Task<DialogueTuiViewModel?> BuildTuiViewModelAsync(Character character, GameState gameState)
    {
        var viewModel = await _dialogueService.GetCurrentDialogueAsync(character, gameState);
        if (viewModel == null)
        {
            return null;
        }

        // Clamp selection to valid range
        if (viewModel.Options.Count > 0)
        {
            SelectedIndex = Math.Clamp(SelectedIndex, 0, viewModel.Options.Count - 1);
        }

        return DialogueTuiViewModel.FromDialogueViewModel(viewModel, SelectedIndex);
    }

    /// <summary>
    /// Resets the selection to the first option.
    /// Called when entering dialogue or transitioning nodes.
    /// </summary>
    public void ResetSelection()
    {
        SelectedIndex = 0;
        LastSelectedOptionId = null;
    }

    /// <summary>
    /// Gets whether dialogue is currently active for a character.
    /// </summary>
    /// <param name="characterId">The character ID to check.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>True if in dialogue.</returns>
    public bool IsInDialogue(Guid characterId, GameState gameState)
    {
        return _dialogueService.IsInDialogue(characterId, gameState);
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text[..(maxLength - 3)] + "...";
    }
}
