using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of ending a dialogue session.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueEndResult
{
    /// <summary>
    /// Whether the dialogue ended successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The reason the dialogue ended.
    /// </summary>
    public DialogueEndReason Reason { get; init; }

    /// <summary>
    /// The session ID that ended.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// Total duration of the dialogue session.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of nodes visited during the session.
    /// </summary>
    public int NodesVisited { get; init; }

    /// <summary>
    /// Number of options selected during the session.
    /// </summary>
    public int OptionsSelected { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful end result.
    /// </summary>
    public static DialogueEndResult Successful(
        DialogueEndReason reason,
        Guid sessionId,
        TimeSpan duration,
        int nodesVisited,
        int optionsSelected) => new()
    {
        Success = true,
        Reason = reason,
        SessionId = sessionId,
        Duration = duration,
        NodesVisited = nodesVisited,
        OptionsSelected = optionsSelected
    };

    /// <summary>
    /// Creates a failed end result.
    /// </summary>
    public static DialogueEndResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
