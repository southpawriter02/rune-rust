using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Dialogue tree persistence.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class DialogueRepository : IDialogueRepository
{
    private readonly RuneAndRustDbContext _context;
    private readonly ILogger<DialogueRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogueRepository"/> class.
    /// </summary>
    public DialogueRepository(
        RuneAndRustDbContext context,
        ILogger<DialogueRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tree Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<DialogueTree?> GetTreeByIdAsync(string treeId)
    {
        _logger.LogDebug("[DialogueRepo] GetTreeByIdAsync: {TreeId}", treeId);

        return await _context.DialogueTrees
            .Include(t => t.Nodes)
                .ThenInclude(n => n.Options)
            .FirstOrDefaultAsync(t => t.TreeId == treeId);
    }

    /// <inheritdoc/>
    public async Task<DialogueTree?> GetTreeByGuidAsync(Guid id)
    {
        _logger.LogDebug("[DialogueRepo] GetTreeByGuidAsync: {Id}", id);

        return await _context.DialogueTrees
            .Include(t => t.Nodes)
                .ThenInclude(n => n.Options)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc/>
    public async Task<DialogueNode?> GetNodeAsync(Guid treeId, string nodeId)
    {
        _logger.LogDebug("[DialogueRepo] GetNodeAsync: Tree={TreeId}, Node={NodeId}", treeId, nodeId);

        return await _context.DialogueNodes
            .Include(n => n.Options)
            .FirstOrDefaultAsync(n => n.TreeId == treeId && n.NodeId == nodeId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DialogueTree>> GetAllTreesAsync()
    {
        _logger.LogDebug("[DialogueRepo] GetAllTreesAsync");

        return await _context.DialogueTrees
            .Include(t => t.Nodes)
                .ThenInclude(n => n.Options)
            .OrderBy(t => t.TreeId)
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Mutations
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task AddTreeAsync(DialogueTree tree)
    {
        _logger.LogDebug(
            "[DialogueRepo] AddTreeAsync: {TreeId} ({NpcName}, {NodeCount} nodes)",
            tree.TreeId, tree.NpcName, tree.Nodes.Count);

        await _context.DialogueTrees.AddAsync(tree);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        var changeCount = await _context.SaveChangesAsync();
        _logger.LogDebug("[DialogueRepo] SaveChangesAsync: {Count} changes", changeCount);
    }
}
