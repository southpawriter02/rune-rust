namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Actions that can be triggered by dialogue choices.
/// </summary>
public enum DialogueAction
{
    /// <summary>No special action.</summary>
    None,
    /// <summary>Accept a quest.</summary>
    AcceptQuest,
    /// <summary>Decline a quest.</summary>
    DeclineQuest,
    /// <summary>Open the shop window.</summary>
    OpenShop,
    /// <summary>Leave/end the conversation.</summary>
    Leave,
    /// <summary>Give an item to the player.</summary>
    GiveItem,
    /// <summary>Take an item from the player.</summary>
    TakeItem
}

/// <summary>
/// A node in a dialogue tree.
/// </summary>
/// <param name="Id">Node identifier.</param>
/// <param name="Text">The NPC's dialogue text.</param>
/// <param name="Choices">Available response choices.</param>
/// <param name="AutoAdvanceNodeId">Node to auto-advance to if no choices.</param>
public record DialogueNode(
    string Id,
    string Text,
    IReadOnlyList<DialogueChoiceData> Choices,
    string? AutoAdvanceNodeId = null);

/// <summary>
/// A dialogue response choice.
/// </summary>
/// <param name="Text">The choice text.</param>
/// <param name="NextNodeId">The next node to navigate to.</param>
/// <param name="Action">Action to trigger.</param>
/// <param name="ActionData">Data for the action (quest ID, etc.).</param>
public record DialogueChoiceData(
    string Text,
    string? NextNodeId = null,
    DialogueAction Action = DialogueAction.None,
    string? ActionData = null);
