using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Provides standard CRUD operations for any entity type.
/// </summary>
/// <typeparam name="T">The entity type this repository handles.</typeparam>
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly RuneAndRustDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<GenericRepository<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public GenericRepository(RuneAndRustDbContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Fetching {EntityType} with ID {Id}", typeof(T).Name, id);

        var entity = await _dbSet.FindAsync(id);

        if (entity == null)
        {
            _logger.LogDebug("{EntityType} with ID {Id} not found", typeof(T).Name, id);
        }
        else
        {
            _logger.LogDebug("Successfully retrieved {EntityType} with ID {Id}", typeof(T).Name, id);
        }

        return entity;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all {EntityType} entities", typeof(T).Name);

        var entities = await _dbSet.ToListAsync();

        _logger.LogDebug("Retrieved {Count} {EntityType} entities", entities.Count, typeof(T).Name);

        return entities;
    }

    /// <inheritdoc/>
    public async Task AddAsync(T entity)
    {
        _logger.LogDebug("Adding new {EntityType} entity", typeof(T).Name);

        await _dbSet.AddAsync(entity);

        _logger.LogDebug("Successfully added {EntityType} entity to context", typeof(T).Name);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(T entity)
    {
        _logger.LogDebug("Updating {EntityType} entity", typeof(T).Name);

        _dbSet.Update(entity);

        _logger.LogDebug("Successfully updated {EntityType} entity in context", typeof(T).Name);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogDebug("Deleting {EntityType} with ID {Id}", typeof(T).Name, id);

        var entity = await _dbSet.FindAsync(id);

        if (entity != null)
        {
            _dbSet.Remove(entity);
            _logger.LogDebug("Successfully marked {EntityType} with ID {Id} for deletion", typeof(T).Name, id);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent {EntityType} with ID {Id}", typeof(T).Name, id);
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        _logger.LogDebug("Saving changes to database");

        var changeCount = await _context.SaveChangesAsync();

        _logger.LogDebug("Saved {ChangeCount} changes to database", changeCount);
    }
}
