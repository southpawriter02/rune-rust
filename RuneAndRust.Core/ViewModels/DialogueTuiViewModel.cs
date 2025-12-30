using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// View model for dialogue TUI rendering, extending DialogueViewModel with selection state.
/// </summary>
/// <remarks>See: v0.4.2d (The Parley) for Dialogue TUI implementation.</remarks>
public record DialogueTuiViewModel(
    Guid SessionId,
    string NpcName,
    string? NpcTitle,
    string SpeakerName,
    string Text,
    bool IsTerminalNode,
    IReadOnlyList<DialogueOptionTuiViewModel> Options,
    int SelectedIndex)
{
    /// <summary>
    /// Combined speaker display (e.g., "Old Scavenger, Iron-Bane Elder").
    /// </summary>
    public string SpeakerDisplay => string.IsNullOrEmpty(NpcTitle)
        ? SpeakerName
        : $"{SpeakerName}, {NpcTitle}";

    /// <summary>
    /// Number of selectable options.
    /// </summary>
    public int OptionCount => Options.Count;

    /// <summary>
    /// Gets the currently selected option, or null if no options.
    /// </summary>
    public DialogueOptionTuiViewModel? SelectedOption =>
        Options.Count > 0 && SelectedIndex >= 0 && SelectedIndex < Options.Count
            ? Options[SelectedIndex]
            : null;

    /// <summary>
    /// Whether there are any available (non-locked) options.
    /// </summary>
    public bool HasAvailableOptions => Options.Any(o => o.IsAvailable);

    /// <summary>
    /// Creates from DialogueViewModel with selection state.
    /// </summary>
    public static DialogueTuiViewModel FromDialogueViewModel(
        DialogueViewModel vm,
        int selectedIndex)
    {
        var options = vm.Options
            .Where(o => o.IsVisible)
            .OrderBy(o => o.DisplayOrder)
            .Select(o => new DialogueOptionTuiViewModel(
                o.OptionId,
                o.Text,
                o.DisplayOrder,
                o.IsVisible,
                o.IsAvailable,
                o.LockedReason,
                o.Tooltip))
            .ToList();

        return new DialogueTuiViewModel(
            vm.SessionId,
            vm.NpcName,
            vm.NpcTitle,
            vm.SpeakerName,
            vm.DialogueText,
            vm.IsTerminalNode,
            options,
            selectedIndex);
    }
}

/// <summary>
/// Option view model for TUI with index tracking.
/// </summary>
public record DialogueOptionTuiViewModel(
    string OptionId,
    string Text,
    int DisplayOrder,
    bool IsVisible,
    bool IsAvailable,
    string? LockReason,
    string? Tooltip)
{
    /// <summary>
    /// Display text with lock indicator if locked.
    /// </summary>
    public string DisplayText => IsAvailable
        ? Text
        : $"[{LockReason ?? "LOCKED"}] {Text}";
}
