using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for InteractableObject entity operations.
/// Provides specialized queries for room-based object management.
/// </summary>
public class InteractableObjectRepository : GenericRepository<InteractableObject>, IInteractableObjectRepository
{
    private readonly ILogger<InteractableObjectRepository> _objectLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractableObjectRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The generic repository logger.</param>
    /// <param name="objectLogger">The object-specific logger.</param>
    public InteractableObjectRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<InteractableObject>> logger,
        ILogger<InteractableObjectRepository> objectLogger)
        : base(context, logger)
    {
        _objectLogger = objectLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InteractableObject>> GetByRoomIdAsync(Guid roomId)
    {
        _objectLogger.LogDebug("Fetching interactable objects for room {RoomId}", roomId);

        var objects = await _dbSet
            .Where(o => o.RoomId == roomId)
            .OrderBy(o => o.Name)
            .ToListAsync();

        _objectLogger.LogDebug("Retrieved {Count} interactable objects for room {RoomId}", objects.Count, roomId);

        return objects;
    }

    /// <inheritdoc/>
    public async Task<InteractableObject?> GetByNameInRoomAsync(Guid roomId, string name)
    {
        _objectLogger.LogDebug("Searching for object '{ObjectName}' in room {RoomId}", name, roomId);

        var normalizedName = name.Trim().ToLowerInvariant();

        var interactableObject = await _dbSet
            .FirstOrDefaultAsync(o => o.RoomId == roomId
                                   && o.Name.ToLower() == normalizedName);

        if (interactableObject == null)
        {
            _objectLogger.LogDebug("Object '{ObjectName}' not found in room {RoomId}", name, roomId);
        }
        else
        {
            _objectLogger.LogDebug("Found object '{ObjectName}' ({ObjectId}) in room {RoomId}",
                interactableObject.Name, interactableObject.Id, roomId);
        }

        return interactableObject;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InteractableObject>> GetContainersInRoomAsync(Guid roomId)
    {
        _objectLogger.LogDebug("Fetching container objects for room {RoomId}", roomId);

        var containers = await _dbSet
            .Where(o => o.RoomId == roomId && o.IsContainer)
            .OrderBy(o => o.Name)
            .ToListAsync();

        _objectLogger.LogDebug("Retrieved {Count} container objects for room {RoomId}", containers.Count, roomId);

        return containers;
    }

    /// <inheritdoc/>
    public async Task<bool> ObjectExistsInRoomAsync(Guid roomId, string name)
    {
        _objectLogger.LogDebug("Checking if object '{ObjectName}' exists in room {RoomId}", name, roomId);

        var normalizedName = name.Trim().ToLowerInvariant();

        var exists = await _dbSet
            .AnyAsync(o => o.RoomId == roomId
                        && o.Name.ToLower() == normalizedName);

        _objectLogger.LogDebug("Object '{ObjectName}' exists in room {RoomId}: {Exists}", name, roomId, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<InteractableObject> objects)
    {
        var objectList = objects.ToList();
        _objectLogger.LogDebug("Adding {Count} interactable objects to database", objectList.Count);

        await _dbSet.AddRangeAsync(objectList);

        _objectLogger.LogDebug("Successfully added {Count} interactable objects to context", objectList.Count);
    }

    /// <inheritdoc/>
    public async Task ClearRoomObjectsAsync(Guid roomId)
    {
        _objectLogger.LogInformation("Clearing all interactable objects from room {RoomId}", roomId);

        var roomObjects = await _dbSet
            .Where(o => o.RoomId == roomId)
            .ToListAsync();

        _dbSet.RemoveRange(roomObjects);
        await _context.SaveChangesAsync();

        _objectLogger.LogInformation("Cleared {Count} interactable objects from room {RoomId}", roomObjects.Count, roomId);
    }
}
