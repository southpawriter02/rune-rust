namespace RuneAndRust.Core.Enums;

/// <summary>
/// Reasons why a dialogue session ended.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public enum DialogueEndReason
{
    /// <summary>
    /// Player selected an exit option (terminal node).
    /// </summary>
    PlayerExit = 0,

    /// <summary>
    /// NPC ended the conversation.
    /// </summary>
    NpcExit = 1,

    /// <summary>
    /// Player cancelled the dialogue (e.g., pressed Escape).
    /// </summary>
    PlayerCancel = 2,

    /// <summary>
    /// Dialogue was interrupted by external event.
    /// </summary>
    Interrupted = 3,

    /// <summary>
    /// An error occurred during dialogue processing.
    /// </summary>
    Error = 4
}
