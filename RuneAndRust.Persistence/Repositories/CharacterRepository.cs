using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Character entity operations.
/// Provides specialized queries for character management.
/// </summary>
public class CharacterRepository : GenericRepository<Character>, ICharacterRepository
{
    private readonly ILogger<CharacterRepository> _characterLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The generic repository logger.</param>
    /// <param name="characterLogger">The character-specific logger.</param>
    public CharacterRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<Character>> logger,
        ILogger<CharacterRepository> characterLogger)
        : base(context, logger)
    {
        _characterLogger = characterLogger;
    }

    /// <inheritdoc/>
    public async Task<Character?> GetByNameAsync(string name)
    {
        _characterLogger.LogDebug("Fetching character by name: {Name}", name);

        var character = await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name);

        if (character == null)
        {
            _characterLogger.LogDebug("No character found with name: {Name}", name);
        }
        else
        {
            _characterLogger.LogDebug("Found character '{CharacterName}' ({CharacterId})", character.Name, character.Id);
        }

        return character;
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name)
    {
        _characterLogger.LogDebug("Checking if character name exists: {Name}", name);

        var exists = await _dbSet.AnyAsync(c => c.Name == name);

        _characterLogger.LogDebug("Character name '{Name}' exists: {Exists}", name, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Character>> GetAllOrderedByCreationAsync()
    {
        _characterLogger.LogDebug("Fetching all characters ordered by creation date");

        var characters = await _dbSet
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        _characterLogger.LogDebug("Retrieved {Count} characters", characters.Count);

        return characters;
    }

    /// <inheritdoc/>
    public async Task<Character?> GetMostRecentAsync()
    {
        _characterLogger.LogDebug("Fetching most recently modified character");

        var character = await _dbSet
            .OrderByDescending(c => c.LastModified)
            .FirstOrDefaultAsync();

        if (character == null)
        {
            _characterLogger.LogDebug("No characters found in database");
        }
        else
        {
            _characterLogger.LogDebug("Most recent character: '{CharacterName}' (last modified: {LastModified})",
                character.Name, character.LastModified);
        }

        return character;
    }
}
