using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Active Ability operations.
/// Provides CRUD and specialized queries for character abilities.
/// </summary>
public class ActiveAbilityRepository : GenericRepository<ActiveAbility>, IActiveAbilityRepository
{
    private readonly ILogger<ActiveAbilityRepository> _abilityLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveAbilityRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="genericLogger">The generic repository logger.</param>
    /// <param name="abilityLogger">The ability-specific logger.</param>
    public ActiveAbilityRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<ActiveAbility>> genericLogger,
        ILogger<ActiveAbilityRepository> abilityLogger)
        : base(context, genericLogger)
    {
        _abilityLogger = abilityLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ActiveAbility>> GetByArchetypeAsync(ArchetypeType archetype, int maxTier = 1)
    {
        _abilityLogger.LogDebug(
            "Fetching ActiveAbilities for archetype {Archetype} up to Tier {MaxTier}",
            archetype, maxTier);

        var abilities = await _dbSet
            .Where(a => a.Archetype == archetype && a.Tier <= maxTier)
            .OrderBy(a => a.Tier)
            .ThenBy(a => a.Name)
            .ToListAsync();

        _abilityLogger.LogDebug(
            "Retrieved {Count} ActiveAbilities for archetype {Archetype}",
            abilities.Count, archetype);

        return abilities;
    }

    /// <inheritdoc/>
    public async Task<ActiveAbility?> GetByNameAsync(string name)
    {
        _abilityLogger.LogDebug("Fetching ActiveAbility with name {Name}", name);

        var ability = await _dbSet
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());

        if (ability == null)
        {
            _abilityLogger.LogDebug("ActiveAbility with name {Name} not found", name);
        }
        else
        {
            _abilityLogger.LogDebug("Retrieved ActiveAbility: {Name}", ability.Name);
        }

        return ability;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<ActiveAbility> abilities)
    {
        var abilityList = abilities.ToList();
        _abilityLogger.LogDebug("Adding {Count} ActiveAbilities", abilityList.Count);

        await _dbSet.AddRangeAsync(abilityList);

        _abilityLogger.LogDebug("Successfully added {Count} ActiveAbilities to context", abilityList.Count);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        _abilityLogger.LogDebug("Checking if ActiveAbility exists with name {Name}", name);

        var exists = await _dbSet
            .AnyAsync(a => a.Name.ToLower() == name.ToLower());

        _abilityLogger.LogDebug(
            "ActiveAbility with name {Name} exists: {Exists}",
            name, exists);

        return exists;
    }
}
