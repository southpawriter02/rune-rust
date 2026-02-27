using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Loads dialogue trees from JSON configuration (config/dialogues.json).
/// Registered as singleton in DI — dialogue data is immutable after initial load.
/// </summary>
public interface IDialogueProvider
{
    /// <summary>Returns a complete dialogue tree by its root ID.</summary>
    IReadOnlyList<DialogueNodeDto>? GetDialogueTree(string rootDialogueId);

    /// <summary>Returns a specific dialogue node by ID within a tree.</summary>
    DialogueNodeDto? GetNode(string rootDialogueId, string nodeId);

    /// <summary>Returns true if a dialogue tree exists for the given root ID.</summary>
    bool HasDialogue(string rootDialogueId);

    /// <summary>Returns all available dialogue tree root IDs.</summary>
    IReadOnlyList<string> GetAllDialogueIds();

    /// <summary>Reloads definitions from disk.</summary>
    void Reload();
}
