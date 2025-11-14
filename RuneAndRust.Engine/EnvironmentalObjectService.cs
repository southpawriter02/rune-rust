using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.22.1: Environmental Object Service
/// Handles destructible terrain, interactive objects, cover, and environmental obstacles.
/// This service manages the lifecycle of environmental objects in combat.
///
/// Responsibilities:
/// - Object creation and destruction
/// - Durability tracking and damage application
/// - Cover management (integrates with v0.20.2 CoverService)
/// - Interactive object triggering
/// - Chain reaction detection
/// </summary>
public class EnvironmentalObjectService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalObjectService>();
    private readonly DiceService _diceService;

    // In-memory storage for environmental objects (could be moved to repository later)
    private readonly Dictionary<int, EnvironmentalObject> _objects = new();
    private int _nextObjectId = 1;

    public EnvironmentalObjectService(DiceService diceService)
    {
        _diceService = diceService;
    }

    #region Object Retrieval

    /// <summary>
    /// Gets an environmental object by ID
    /// </summary>
    public EnvironmentalObject? GetObject(int objectId)
    {
        _objects.TryGetValue(objectId, out var obj);
        return obj;
    }

    /// <summary>
    /// Gets all environmental objects in a room
    /// </summary>
    public List<EnvironmentalObject> GetObjectsInRoom(int roomId)
    {
        return _objects.Values
            .Where(o => o.RoomId == roomId && o.State != EnvironmentalObjectState.Destroyed)
            .ToList();
    }

    /// <summary>
    /// Gets all environmental objects at a specific grid position
    /// </summary>
    public List<EnvironmentalObject> GetObjectsAtPosition(int roomId, string gridPosition)
    {
        return _objects.Values
            .Where(o => o.RoomId == roomId &&
                       o.GridPosition == gridPosition &&
                       o.State != EnvironmentalObjectState.Destroyed)
            .ToList();
    }

    /// <summary>
    /// Gets all active hazards at a position
    /// </summary>
    public List<EnvironmentalObject> GetHazardsAtPosition(string gridPosition)
    {
        return _objects.Values
            .Where(o => o.GridPosition == gridPosition &&
                       o.IsHazard &&
                       o.State == EnvironmentalObjectState.Active)
            .ToList();
    }

    #endregion

    #region Object Creation

    /// <summary>
    /// Creates a new environmental object
    /// </summary>
    public EnvironmentalObject CreateObject(EnvironmentalObject obj)
    {
        obj.ObjectId = _nextObjectId++;
        _objects[obj.ObjectId] = obj;

        _log.Information("Environmental object created: {ObjectId} - {Name} at {Position}",
            obj.ObjectId, obj.Name, obj.GridPosition);

        return obj;
    }

    /// <summary>
    /// Creates destructible cover at a position
    /// </summary>
    public EnvironmentalObject CreateCover(int roomId, string gridPosition, string name,
        CoverQuality quality, int durability)
    {
        var cover = new EnvironmentalObject
        {
            RoomId = roomId,
            GridPosition = gridPosition,
            ObjectType = EnvironmentalObjectType.Cover,
            Name = name,
            Description = $"Destructible {quality} cover",
            Icon = "🛡️",
            IsDestructible = true,
            CurrentDurability = durability,
            MaxDurability = durability,
            ProvidesCover = true,
            CoverQuality = quality,
            State = EnvironmentalObjectState.Active
        };

        return CreateObject(cover);
    }

    /// <summary>
    /// Creates a hazard object (fire, toxic pool, energy field)
    /// </summary>
    public EnvironmentalObject CreateHazard(int roomId, string gridPosition, string name,
        string damageFormula, string damageType, HazardTrigger trigger = HazardTrigger.Automatic)
    {
        var hazard = new EnvironmentalObject
        {
            RoomId = roomId,
            GridPosition = gridPosition,
            ObjectType = EnvironmentalObjectType.Hazard,
            Name = name,
            Description = $"Environmental hazard: {name}",
            Icon = "💀",
            IsHazard = true,
            HazardTrigger = trigger,
            DamageFormula = damageFormula,
            DamageType = damageType,
            State = EnvironmentalObjectState.Active
        };

        return CreateObject(hazard);
    }

    #endregion

    #region Destruction and Damage

    /// <summary>
    /// Applies damage to an environmental object
    /// </summary>
    public DestructionResult DamageObject(int objectId, int damage)
    {
        var obj = GetObject(objectId);
        if (obj == null)
        {
            return new DestructionResult
            {
                WasDestroyed = false,
                LogMessage = "Object not found"
            };
        }

        if (!obj.IsDestructible)
        {
            return new DestructionResult
            {
                WasDestroyed = false,
                RemainingDurability = 0,
                LogMessage = $"{obj.Name} is indestructible"
            };
        }

        bool destroyed = obj.ApplyDamage(damage);

        _log.Information("Environmental object damaged: {ObjectId} - {Name} took {Damage} damage, " +
                        "Remaining: {Remaining}/{Max}, Destroyed: {Destroyed}",
            objectId, obj.Name, damage, obj.CurrentDurability, obj.MaxDurability, destroyed);

        var result = new DestructionResult
        {
            WasDestroyed = destroyed,
            RemainingDurability = obj.CurrentDurability ?? 0,
            LogMessage = destroyed
                ? $"[DESTROYED] {obj.Name} has been obliterated! ({damage} damage)"
                : $"{obj.Name} damaged: {obj.CurrentDurability}/{obj.MaxDurability} HP remaining"
        };

        // Check for chain reactions (e.g., explosive barrel destroying nearby objects)
        if (destroyed && obj.ObjectType == EnvironmentalObjectType.Interactive)
        {
            result.ChainReactions = DetectChainReactions(obj);
        }

        return result;
    }

    /// <summary>
    /// Destroys an environmental object immediately
    /// </summary>
    public bool DestroyObject(int objectId, string destructionCause)
    {
        var obj = GetObject(objectId);
        if (obj == null)
        {
            return false;
        }

        obj.State = EnvironmentalObjectState.Destroyed;
        obj.CurrentDurability = 0;

        _log.Warning("Environmental object destroyed: {ObjectId} - {Name} - Cause: {Cause}",
            objectId, obj.Name, destructionCause);

        return true;
    }

    /// <summary>
    /// Detects chain reactions from object destruction
    /// </summary>
    private List<EnvironmentalEvent> DetectChainReactions(EnvironmentalObject source)
    {
        var reactions = new List<EnvironmentalEvent>();

        // Check for nearby explosive objects or hazards
        var nearbyObjects = GetObjectsInRoom(source.RoomId)
            .Where(o => o.ObjectId != source.ObjectId && IsNearby(source.GridPosition, o.GridPosition))
            .ToList();

        foreach (var nearby in nearbyObjects)
        {
            if (nearby.ObjectType == EnvironmentalObjectType.Interactive &&
                nearby.Name.Contains("Barrel", StringComparison.OrdinalIgnoreCase))
            {
                reactions.Add(new EnvironmentalEvent
                {
                    EventType = EnvironmentalEventType.ChainReaction,
                    ObjectId = nearby.ObjectId,
                    Description = $"Chain reaction: {source.Name} destruction triggered {nearby.Name}!"
                });
            }
        }

        return reactions;
    }

    /// <summary>
    /// Checks if two positions are nearby (simplified distance check)
    /// </summary>
    private bool IsNearby(string? position1, string? position2)
    {
        // TODO: Implement proper grid distance calculation
        // For now, simple string comparison
        return position1 == position2;
    }

    #endregion

    #region Cover Management

    /// <summary>
    /// Gets cover data at a specific position (integrates with v0.20.2 CoverService)
    /// </summary>
    public CoverData? GetCoverAtPosition(string gridPosition, string attackDirection)
    {
        var coverObjects = GetHazardsAtPosition(gridPosition)
            .Where(o => o.ProvidesCover && o.State == EnvironmentalObjectState.Active)
            .ToList();

        if (coverObjects.Count == 0)
        {
            return null;
        }

        // Get the best cover available at this position
        var bestCover = coverObjects
            .OrderByDescending(o => o.CoverQuality)
            .First();

        int defenseBonus = bestCover.CoverQuality switch
        {
            CoverQuality.Light => 2,
            CoverQuality.Heavy => 4,
            CoverQuality.Total => 6,
            _ => 0
        };

        return new CoverData
        {
            Quality = bestCover.CoverQuality,
            DefenseBonus = defenseBonus,
            RemainingDurability = bestCover.CurrentDurability,
            Description = bestCover.Description
        };
    }

    /// <summary>
    /// Validates if cover blocks a path between attacker and target
    /// </summary>
    public bool ValidateCoverPath(string attackerPosition, string targetPosition)
    {
        // TODO: Implement proper line of sight and cover arc calculation
        // For parent spec, just check if target has cover
        var coverAtTarget = GetCoverAtPosition(targetPosition, "any");
        return coverAtTarget != null;
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Attempts to interact with an environmental object
    /// </summary>
    public InteractionResult InteractWithObject(int objectId, int actorId)
    {
        var obj = GetObject(objectId);
        if (obj == null)
        {
            return new InteractionResult
            {
                Success = false,
                LogMessage = "Object not found"
            };
        }

        if (!obj.IsInteractive || !obj.CanTrigger())
        {
            return new InteractionResult
            {
                Success = false,
                LogMessage = $"{obj.Name} cannot be interacted with"
            };
        }

        // Trigger the object
        obj.Trigger();

        _log.Information("Environmental object interacted: {ObjectId} - {Name} by Actor {ActorId}",
            objectId, obj.Name, actorId);

        // Calculate effects based on interaction type
        var result = new InteractionResult
        {
            Success = true,
            StaminaCost = obj.InteractionCost,
            EffectDescription = obj.Description,
            LogMessage = $"🔧 {obj.Name} activated!"
        };

        return result;
    }

    #endregion

    #region Utility

    /// <summary>
    /// Clears all objects in a room (cleanup)
    /// </summary>
    public void ClearRoom(int roomId)
    {
        var objectsToRemove = _objects.Values
            .Where(o => o.RoomId == roomId)
            .Select(o => o.ObjectId)
            .ToList();

        foreach (var id in objectsToRemove)
        {
            _objects.Remove(id);
        }

        _log.Information("Cleared all environmental objects from room {RoomId}", roomId);
    }

    #endregion
}
