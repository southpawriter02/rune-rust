using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for dialogue TUI rendering using Spectre.Console.
/// </summary>
/// <remarks>See: v0.4.2d (The Parley) for Dialogue TUI implementation.</remarks>
public interface IDialogueScreenRenderer
{
    /// <summary>
    /// Renders the dialogue screen with speaker, text, and selectable options.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The dialogue state to render.</param>
    void Render(DialogueTuiViewModel viewModel);

    /// <summary>
    /// Plays a brief feedback effect for locked option selection attempt.
    /// </summary>
    /// <param name="lockReason">The reason the option is locked.</param>
    void PlayLockedFeedback(string lockReason);
}
