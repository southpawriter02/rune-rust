using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Orchestrates dialogue interactions between player and NPCs.
/// Bridges IDialogueProvider data to the presentation layer's DialogueWindowViewModel.
/// Registered as scoped in DI — one instance per game session.
/// </summary>
public interface IDialogueService
{
    /// <summary>
    /// Starts a dialogue with an NPC. Returns the opening dialogue node,
    /// or null if the NPC has no dialogue tree.
    /// </summary>
    DialogueNodeDto? StartDialogue(Npc npc);

    /// <summary>
    /// Processes a player's choice selection and returns the next dialogue node.
    /// Returns null if the conversation ends.
    /// </summary>
    DialogueNodeDto? ProcessChoice(string rootDialogueId, string choiceNodeId, DialogueOutcomeDto? outcome);

    /// <summary>
    /// Handles a dialogue outcome (quest acceptance, reputation change, shop opening, etc.).
    /// </summary>
    void ProcessOutcome(DialogueOutcomeDto outcome, Npc npc);

    /// <summary>Gets the current active dialogue state, or null if no dialogue is active.</summary>
    ActiveDialogueState? GetCurrentDialogue();

    /// <summary>Ends the current dialogue session.</summary>
    void EndDialogue();

    /// <summary>Returns true if a dialogue is currently active.</summary>
    bool IsDialogueActive { get; }
}
