using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository for dialogue tree persistence.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public interface IDialogueRepository
{
    /// <summary>
    /// Gets a dialogue tree by its string identifier.
    /// </summary>
    Task<DialogueTree?> GetTreeByIdAsync(string treeId);

    /// <summary>
    /// Gets a dialogue tree by database ID.
    /// </summary>
    Task<DialogueTree?> GetTreeByGuidAsync(Guid id);

    /// <summary>
    /// Gets a specific node from a tree.
    /// </summary>
    Task<DialogueNode?> GetNodeAsync(Guid treeId, string nodeId);

    /// <summary>
    /// Gets all dialogue trees.
    /// </summary>
    Task<IEnumerable<DialogueTree>> GetAllTreesAsync();

    /// <summary>
    /// Adds a new dialogue tree.
    /// </summary>
    Task AddTreeAsync(DialogueTree tree);

    /// <summary>
    /// Persists changes.
    /// </summary>
    Task SaveChangesAsync();
}
