namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of starting a dialogue session.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueStartResult
{
    /// <summary>
    /// Whether the dialogue started successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The dialogue view model if successful.
    /// </summary>
    public DialogueViewModel? ViewModel { get; init; }

    /// <summary>
    /// The active session if successful.
    /// </summary>
    public DialogueSession? Session { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static DialogueStartResult Successful(DialogueViewModel viewModel, DialogueSession session) => new()
    {
        Success = true,
        ViewModel = viewModel,
        Session = session
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static DialogueStartResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
