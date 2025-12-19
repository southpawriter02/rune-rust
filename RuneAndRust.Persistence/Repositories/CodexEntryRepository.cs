using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Codex Entry operations.
/// Provides CRUD and specialized queries for Scavenger's Journal entries.
/// </summary>
public class CodexEntryRepository : GenericRepository<CodexEntry>, ICodexEntryRepository
{
    private readonly ILogger<CodexEntryRepository> _codexLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodexEntryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="genericLogger">The generic repository logger.</param>
    /// <param name="codexLogger">The codex-specific logger.</param>
    public CodexEntryRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<CodexEntry>> genericLogger,
        ILogger<CodexEntryRepository> codexLogger)
        : base(context, genericLogger)
    {
        _codexLogger = codexLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CodexEntry>> GetByCategoryAsync(EntryCategory category)
    {
        _codexLogger.LogDebug("Fetching CodexEntries with category {Category}", category);

        var entries = await _dbSet
            .Where(e => e.Category == category)
            .OrderBy(e => e.Title)
            .ToListAsync();

        _codexLogger.LogDebug("Retrieved {Count} CodexEntries with category {Category}", entries.Count, category);

        return entries;
    }

    /// <inheritdoc/>
    public async Task<CodexEntry?> GetByTitleAsync(string title)
    {
        _codexLogger.LogDebug("Fetching CodexEntry with title {Title}", title);

        var entry = await _dbSet
            .FirstOrDefaultAsync(e => e.Title.ToLower() == title.ToLower());

        if (entry == null)
        {
            _codexLogger.LogDebug("CodexEntry with title {Title} not found", title);
        }
        else
        {
            _codexLogger.LogDebug("Retrieved CodexEntry: {Title}", entry.Title);
        }

        return entry;
    }

    /// <inheritdoc/>
    public async Task<CodexEntry?> GetWithFragmentsAsync(Guid id)
    {
        _codexLogger.LogDebug("Fetching CodexEntry {Id} with Fragments", id);

        var entry = await _dbSet
            .Include(e => e.Fragments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry == null)
        {
            _codexLogger.LogDebug("CodexEntry with ID {Id} not found", id);
        }
        else
        {
            _codexLogger.LogDebug("Retrieved CodexEntry {Title} with {FragmentCount} fragments",
                entry.Title, entry.Fragments.Count);
        }

        return entry;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<CodexEntry> entries)
    {
        var entryList = entries.ToList();
        _codexLogger.LogDebug("Adding {Count} CodexEntries", entryList.Count);

        await _dbSet.AddRangeAsync(entryList);

        _codexLogger.LogDebug("Successfully added {Count} CodexEntries to context", entryList.Count);
    }
}
