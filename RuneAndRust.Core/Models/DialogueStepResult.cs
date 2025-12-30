using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of selecting a dialogue option and advancing the conversation.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueStepResult
{
    /// <summary>
    /// Whether the step was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Whether the dialogue has ended.
    /// </summary>
    public bool DialogueEnded { get; init; }

    /// <summary>
    /// The reason the dialogue ended (if it ended).
    /// </summary>
    public DialogueEndReason? EndReason { get; init; }

    /// <summary>
    /// The updated view model if dialogue continues.
    /// </summary>
    public DialogueViewModel? ViewModel { get; init; }

    /// <summary>
    /// Results of any effects that were executed.
    /// </summary>
    public List<DialogueEffectResult> EffectResults { get; init; } = new();

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a result for continuing dialogue.
    /// </summary>
    public static DialogueStepResult Continue(DialogueViewModel viewModel, List<DialogueEffectResult>? effects = null) => new()
    {
        Success = true,
        DialogueEnded = false,
        ViewModel = viewModel,
        EffectResults = effects ?? new()
    };

    /// <summary>
    /// Creates a result for ended dialogue.
    /// </summary>
    public static DialogueStepResult End(DialogueEndReason reason, List<DialogueEffectResult>? effects = null) => new()
    {
        Success = true,
        DialogueEnded = true,
        EndReason = reason,
        EffectResults = effects ?? new()
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static DialogueStepResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
