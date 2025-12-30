namespace RuneAndRust.Core.Models;

/// <summary>
/// View model representing the current state of a dialogue for UI rendering.
/// Contains everything needed to display the dialogue UI.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public class DialogueViewModel
{
    /// <summary>
    /// The session ID for tracking.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// The NPC's display name.
    /// </summary>
    public required string NpcName { get; set; }

    /// <summary>
    /// Optional NPC title (e.g., "Iron-Bane Elder").
    /// </summary>
    public string? NpcTitle { get; set; }

    /// <summary>
    /// The current speaker's name (may differ from NPC if player is speaking).
    /// </summary>
    public required string SpeakerName { get; set; }

    /// <summary>
    /// The current dialogue text to display.
    /// </summary>
    public required string DialogueText { get; set; }

    /// <summary>
    /// The ID of the current node.
    /// </summary>
    public required string CurrentNodeId { get; set; }

    /// <summary>
    /// Available options for the player to select.
    /// Only includes visible options.
    /// </summary>
    public List<DialogueOptionViewModel> Options { get; set; } = new();

    /// <summary>
    /// Whether this is a terminal node (dialogue will end after any selection).
    /// </summary>
    public bool IsTerminalNode { get; set; }

    /// <summary>
    /// Whether the dialogue can be cancelled by the player.
    /// </summary>
    public bool CanCancel { get; set; } = true;

    /// <summary>
    /// Optional portrait identifier for the current speaker.
    /// </summary>
    public string? PortraitId { get; set; }

    /// <summary>
    /// Optional mood/emotion for the current speaker.
    /// </summary>
    public string? SpeakerMood { get; set; }
}
