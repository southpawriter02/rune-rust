using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Spell entity operations.
/// Provides specialized queries for spell lookup and filtering.
/// </summary>
/// <remarks>
/// See: v0.4.3b (The Grimoire) for implementation details.
/// </remarks>
public class SpellRepository : GenericRepository<Spell>, ISpellRepository
{
    private readonly ILogger<SpellRepository> _spellLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The generic repository logger.</param>
    /// <param name="spellLogger">The spell-specific logger.</param>
    public SpellRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<Spell>> logger,
        ILogger<SpellRepository> spellLogger)
        : base(context, logger)
    {
        _spellLogger = spellLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetBySchoolAsync(SpellSchool school)
    {
        _spellLogger.LogDebug("[Spell] Fetching spells in school {School}", school);

        var spells = await _dbSet
            .Where(s => s.School == school)
            .OrderBy(s => s.Tier)
            .ThenBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} spells in school {School}", spells.Count, school);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetByTargetTypeAsync(SpellTargetType targetType)
    {
        _spellLogger.LogDebug("[Spell] Fetching spells with target type {TargetType}", targetType);

        var spells = await _dbSet
            .Where(s => s.TargetType == targetType)
            .OrderBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} spells with target type {TargetType}", spells.Count, targetType);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetByMaxTierAsync(int maxTier)
    {
        _spellLogger.LogDebug("[Spell] Fetching spells up to tier {MaxTier}", maxTier);

        var spells = await _dbSet
            .Where(s => s.Tier <= maxTier)
            .OrderBy(s => s.Tier)
            .ThenBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} spells up to tier {MaxTier}", spells.Count, maxTier);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetByArchetypeAsync(ArchetypeType archetype)
    {
        _spellLogger.LogDebug("[Spell] Fetching spells available to archetype {Archetype}", archetype);

        var spells = await _dbSet
            .Where(s => s.Archetype == null || s.Archetype == archetype)
            .OrderBy(s => s.Tier)
            .ThenBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} spells available to archetype {Archetype}", spells.Count, archetype);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<Spell?> GetByNameAsync(string name)
    {
        _spellLogger.LogDebug("[Spell] Searching for spell by name '{SpellName}'", name);

        var normalizedName = name.Trim().ToLowerInvariant();

        var spell = await _dbSet
            .FirstOrDefaultAsync(s => s.Name.ToLower() == normalizedName);

        if (spell == null)
        {
            _spellLogger.LogDebug("[Spell] Spell '{SpellName}' not found", name);
        }
        else
        {
            _spellLogger.LogDebug("[Spell] Found spell '{SpellName}' ({SpellId})", spell.Name, spell.Id);
        }

        return spell;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetChargedSpellsAsync()
    {
        _spellLogger.LogDebug("[Spell] Fetching charged spells (ChargeTurns > 0)");

        var spells = await _dbSet
            .Where(s => s.ChargeTurns > 0)
            .OrderBy(s => s.ChargeTurns)
            .ThenBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} charged spells", spells.Count);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetInstantSpellsAsync()
    {
        _spellLogger.LogDebug("[Spell] Fetching instant spells (ChargeTurns == 0)");

        var spells = await _dbSet
            .Where(s => s.ChargeTurns == 0)
            .OrderBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} instant spells", spells.Count);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetConcentrationSpellsAsync()
    {
        _spellLogger.LogDebug("[Spell] Fetching concentration spells");

        var spells = await _dbSet
            .Where(s => s.RequiresConcentration)
            .OrderBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} concentration spells", spells.Count);

        return spells;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Spell>> GetByRangeAsync(SpellRange range)
    {
        _spellLogger.LogDebug("[Spell] Fetching spells with range {Range}", range);

        var spells = await _dbSet
            .Where(s => s.Range == range)
            .OrderBy(s => s.Name)
            .ToListAsync();

        _spellLogger.LogDebug("[Spell] Retrieved {Count} spells with range {Range}", spells.Count, range);

        return spells;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<Spell> spells)
    {
        var spellList = spells.ToList();
        _spellLogger.LogDebug("[Spell] Adding {Count} spells to database", spellList.Count);

        await _dbSet.AddRangeAsync(spellList);

        _spellLogger.LogDebug("[Spell] Successfully added {Count} spells to context", spellList.Count);
    }
}
