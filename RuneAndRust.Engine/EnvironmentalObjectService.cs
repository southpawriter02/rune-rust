using RuneAndRust.Core;
using Serilog;
using DestructionResult = RuneAndRust.Core.DestructionResult;

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
/// - Chain reaction detection and execution
/// - Hazard triggers on destruction
/// - Terrain creation aftermath
/// </summary>
public class EnvironmentalObjectService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalObjectService>();
    private readonly DiceService _diceService;

    // In-memory storage for environmental objects (could be moved to repository later)
    private readonly Dictionary<int, EnvironmentalObject> _objects = new();
    private int _nextObjectId = 1;

    // Grid dependency for radius calculations and terrain updates
    // Optional to maintain backward compatibility
    private BattlefieldGrid? _currentGrid;

    public EnvironmentalObjectService(DiceService diceService, BattlefieldGrid? grid = null)
    {
        _diceService = diceService;
        _currentGrid = grid;
    }

    /// <summary>
    /// Sets the current battlefield grid for environmental object placement and queries
    /// Call this after grid initialization in combat
    /// </summary>
    public void SetGrid(BattlefieldGrid grid)
    {
        _currentGrid = grid;
        _log.Information("BattlefieldGrid set for environmental object service");
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
    /// Gets all destructible objects in a room
    /// </summary>
    public List<EnvironmentalObject> GetDestructibleObjectsInRoom(int roomId)
    {
        return _objects.Values
            .Where(o => o.RoomId == roomId &&
                       o.IsDestructible &&
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

    /// <summary>
    /// v0.29.5: Gets environmental objects by their IDs
    /// Used for resolving tile EnvironmentalObjectIds list
    /// </summary>
    public List<EnvironmentalObject> GetObjectsByIds(List<int> objectIds)
    {
        return objectIds
            .Select(id => _objects.TryGetValue(id, out var obj) ? obj : null)
            .Where(obj => obj != null)
            .Cast<EnvironmentalObject>()
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
    /// Creates destructible cover at a position (v0.22.1 enhanced)
    /// </summary>
    public EnvironmentalObject CreateCover(
        int roomId,
        string gridPosition,
        string name,
        CoverQuality quality,
        int durability,
        int soakValue = 0,
        string? createsTerrainOnDestroy = "Difficult")
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
            SoakValue = soakValue,
            ProvidesCover = true,
            CoverQuality = quality,
            State = EnvironmentalObjectState.Active,
            CreatesTerrainOnDestroy = createsTerrainOnDestroy,
            TerrainDuration = null // Permanent rubble
        };

        return CreateObject(cover);
    }

    /// <summary>
    /// Creates an explosive object (barrel, spore pod, etc.) - v0.22.1
    /// </summary>
    public EnvironmentalObject CreateExplosiveObject(
        int roomId,
        string gridPosition,
        string name,
        string damageFormula, // "20 Fire", "5 Poison"
        int explosionRadius,
        int durability = 10,
        string? statusEffect = null,
        bool canTriggerAdjacents = true)
    {
        var explosive = new EnvironmentalObject
        {
            RoomId = roomId,
            GridPosition = gridPosition,
            ObjectType = EnvironmentalObjectType.Interactive,
            Name = name,
            Description = $"Explosive hazard: {name}",
            Icon = "🛢️",
            IsDestructible = true,
            CurrentDurability = durability,
            MaxDurability = durability,
            SoakValue = 0,
            IsHazard = true,
            HazardTrigger = Core.HazardTrigger.Manual,
            DamageFormula = damageFormula,
            DamageType = ParseDamageType(damageFormula),
            StatusEffect = statusEffect,
            IgnoresSoak = false,
            ExplosionRadius = explosionRadius,
            CanTriggerAdjacents = canTriggerAdjacents,
            State = EnvironmentalObjectState.Active
        };

        return CreateObject(explosive);
    }

    /// <summary>
    /// Creates an unstable ceiling hazard - v0.22.1
    /// </summary>
    public EnvironmentalObject CreateUnstableCeiling(
        int roomId,
        string gridPosition,
        int explosionRadius = 3)
    {
        var ceiling = new EnvironmentalObject
        {
            RoomId = roomId,
            GridPosition = gridPosition,
            ObjectType = EnvironmentalObjectType.Terrain,
            Name = "Unstable Ceiling",
            Description = "Structural integrity compromised. Explosive impact will trigger collapse.",
            Icon = "⚠️",
            IsDestructible = false, // Triggered by abilities, not direct damage
            IsHazard = true,
            HazardTrigger = Core.HazardTrigger.OnLoudAction, // Triggered by explosive/loud actions
            DamageFormula = "25 Physical",
            DamageType = "Physical",
            StatusEffect = "[Stunned]",
            IgnoresSoak = true,
            ExplosionRadius = explosionRadius,
            CanTriggerAdjacents = false,
            State = EnvironmentalObjectState.Active,
            TriggersRemaining = 1, // One-time collapse
            CreatesTerrainOnDestroy = "Difficult",
            TerrainDuration = null // Permanent rubble
        };

        return CreateObject(ceiling);
    }

    /// <summary>
    /// Creates a steam vent hazard (reusable with cooldown) - v0.22.1
    /// </summary>
    public EnvironmentalObject CreateSteamVent(
        int roomId,
        string gridPosition)
    {
        var steamVent = new EnvironmentalObject
        {
            RoomId = roomId,
            GridPosition = gridPosition,
            ObjectType = EnvironmentalObjectType.Interactive,
            Name = "High-Pressure Steam Vent",
            Description = "Ruptured geothermal conduit. Erupts periodically.",
            Icon = "💨",
            IsDestructible = true,
            CurrentDurability = 20,
            MaxDurability = 20,
            SoakValue = 5,
            IsHazard = true,
            HazardTrigger = Core.HazardTrigger.Manual,
            DamageFormula = "15 Fire",
            DamageType = "Fire",
            StatusEffect = "[Obscuring Terrain]",
            IgnoresSoak = false,
            ExplosionRadius = 1,
            CanTriggerAdjacents = false,
            State = EnvironmentalObjectState.Active,
            CooldownDuration = 2,
            CooldownRemaining = 0,
            CreatesTerrainOnDestroy = "Hazardous",
            TerrainDuration = 1 // Steam clears after 1 turn
        };

        return CreateObject(steamVent);
    }

    /// <summary>
    /// Creates a hazard object (fire, toxic pool, energy field)
    /// </summary>
    public EnvironmentalObject CreateHazard(
        int roomId,
        string gridPosition,
        string name,
        string damageFormula,
        string damageType,
        Core.HazardTrigger trigger = Core.HazardTrigger.Automatic)
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

    #region Damage and Destruction (v0.22.1 Enhanced)

    /// <summary>
    /// Applies damage to an environmental object with soak calculation
    /// v0.22.1: Enhanced with proper soak, destruction triggers, and combat instance tracking
    /// </summary>
    public DestructionResult ApplyDamageToObject(
        int objectId,
        int damage,
        string damageType,
        int attackerId,
        int combatInstanceId)
    {
        var obj = GetObject(objectId);
        if (obj == null || !obj.IsDestructible || obj.State == EnvironmentalObjectState.Destroyed)
        {
            return new DestructionResult
            {
                Success = false,
                ObjectDestroyed = false,
                Message = "Object cannot be damaged"
            };
        }

        // Apply soak (damage reduction)
        int damageAfterSoak = Math.Max(0, damage - obj.SoakValue);

        _log.Information(
            "Environmental object {ObjectId} ({Name}) taking {Damage} {DamageType} damage " +
            "(soak: {Soak}, after soak: {DamageAfterSoak}). Current durability: {Current}/{Max}",
            objectId, obj.Name, damage, damageType, obj.SoakValue, damageAfterSoak,
            obj.CurrentDurability, obj.MaxDurability);

        // Update durability
        obj.CurrentDurability = Math.Max(0, obj.CurrentDurability!.Value - damageAfterSoak);

        // Check for destruction
        if (obj.CurrentDurability <= 0)
        {
            return DestroyObject(objectId, attackerId, combatInstanceId, "Direct Damage");
        }

        // Update state to Damaged if below 50% durability
        if (obj.CurrentDurability <= obj.MaxDurability / 2 && obj.State == EnvironmentalObjectState.Active)
        {
            obj.State = EnvironmentalObjectState.Damaged;
            _log.Information("Object {ObjectId} state changed to Damaged", objectId);
        }

        return new DestructionResult
        {
            Success = true,
            ObjectDestroyed = false,
            ObjectId = objectId,
            ObjectName = obj.Name,
            DamageDealt = damageAfterSoak,
            RemainingDurability = obj.CurrentDurability.Value,
            Message = $"{obj.Name} damaged for {damageAfterSoak} (durability: {obj.CurrentDurability}/{obj.MaxDurability})"
        };
    }

    /// <summary>
    /// Applies damage to an environmental object (simplified version for backward compatibility)
    /// </summary>
    public DestructionResult DamageObject(int objectId, int damage)
    {
        return ApplyDamageToObject(objectId, damage, "Physical", attackerId: 0, combatInstanceId: 0);
    }

    /// <summary>
    /// Destroys an environmental object (v0.22.1 enhanced with full effects)
    /// </summary>
    public DestructionResult DestroyObject(
        int objectId,
        int? destroyerId,
        int combatInstanceId,
        string destructionMethod)
    {
        var obj = GetObject(objectId);
        if (obj == null || obj.State == EnvironmentalObjectState.Destroyed)
        {
            return new DestructionResult
            {
                Success = false,
                ObjectDestroyed = false,
                Message = "Object not found or already destroyed"
            };
        }

        _log.Information(
            "Destroying environmental object {ObjectId} ({Name}) via {Method}",
            objectId, obj.Name, destructionMethod);

        var result = new DestructionResult
        {
            Success = true,
            ObjectDestroyed = true,
            ObjectId = objectId,
            ObjectName = obj.Name,
            DestructionMethod = destructionMethod
        };

        // Handle destruction effects
        if (obj.IsHazard && obj.HazardTrigger == Core.HazardTrigger.Manual)
        {
            // Trigger explosion/hazard effect
            var hazardResult = TriggerDestructionHazard(obj, combatInstanceId);
            result.SecondaryTargets = hazardResult.AffectedCharacters;
            result.DamageDealt = hazardResult.TotalDamage;
            result.Message = hazardResult.Description;
        }
        else
        {
            result.Message = $"[DESTROYED] {obj.Name} has been obliterated!";
        }

        // Create terrain aftermath
        if (!string.IsNullOrEmpty(obj.CreatesTerrainOnDestroy))
        {
            CreateDestructionTerrain(obj);
            result.TerrainCreated = obj.CreatesTerrainOnDestroy;
        }

        // Chain reactions
        if (obj.CanTriggerAdjacents && obj.ExplosionRadius > 0)
        {
            var chainResults = TriggerChainReaction(obj, combatInstanceId);
            result.ChainReactions = chainResults;

            if (chainResults.Count > 0)
            {
                result.Message += $"\n⚡ CHAIN REACTION! {chainResults.Count} additional objects destroyed!";
            }
        }

        // Update object state
        obj.State = EnvironmentalObjectState.Destroyed;
        obj.CurrentDurability = 0;

        _log.Information("Object {ObjectId} destroyed successfully. Secondary targets: {Count}",
            objectId, result.SecondaryTargets.Count);

        return result;
    }

    /// <summary>
    /// Destroys an environmental object immediately (legacy compatibility)
    /// </summary>
    public bool DestroyObject(int objectId, string destructionCause)
    {
        var result = DestroyObject(objectId, null, 0, destructionCause);
        return result.ObjectDestroyed;
    }

    #endregion

    #region Hazard Triggers & Chain Reactions (v0.22.1)

    /// <summary>
    /// Triggers the destruction hazard of an object (explosion, eruption, etc.)
    /// v0.22.1: Core hazard trigger system
    /// </summary>
    private HazardResult TriggerDestructionHazard(
        EnvironmentalObject obj,
        int combatInstanceId)
    {
        _log.Information(
            "Triggering destruction hazard for {ObjectId}: {Damage} {DamageType} in radius {Radius}",
            obj.ObjectId, obj.DamageFormula, obj.DamageType, obj.ExplosionRadius);

        // Parse damage formula (e.g., "15 Fire" → 15, "Fire")
        var (damage, damageType) = ParseDamageFormula(obj.DamageFormula ?? "0 Physical");

        // Get affected characters in radius
        var affectedCharacters = GetCharactersInRadius(obj.GridPosition, obj.ExplosionRadius);

        var result = new HazardResult
        {
            WasTriggered = true,
            AffectedCharacters = affectedCharacters,
            TotalDamage = 0,
            StatusEffectApplied = obj.StatusEffect,
            Description = $"💥 {obj.Name} explodes!"
        };

        // Calculate total damage dealt (would integrate with actual damage service in production)
        result.TotalDamage = damage * affectedCharacters.Count;

        if (affectedCharacters.Count > 0)
        {
            result.Description += $"\n   {affectedCharacters.Count} targets hit for {damage} {damageType} damage each!";

            if (!string.IsNullOrEmpty(obj.StatusEffect))
            {
                result.Description += $"\n   Status effect applied: {obj.StatusEffect}";
            }
        }
        else
        {
            result.Description += " No targets in range.";
        }

        return result;
    }

    /// <summary>
    /// Creates terrain aftermath from object destruction
    /// v0.22.1: Full grid integration
    /// </summary>
    private void CreateDestructionTerrain(EnvironmentalObject obj)
    {
        if (_currentGrid == null || string.IsNullOrEmpty(obj.GridPosition))
        {
            _log.Warning("Cannot create terrain: No grid set or invalid position");
            return;
        }

        _log.Information(
            "Creating {TerrainType} terrain at {Position} from destroyed {ObjectName}",
            obj.CreatesTerrainOnDestroy, obj.GridPosition, obj.Name);

        var gridPos = ParseGridPosition(obj.GridPosition);
        if (gridPos == null)
        {
            _log.Warning("Could not parse grid position for terrain creation: {Position}", obj.GridPosition);
            return;
        }

        var tile = _currentGrid.GetTile(gridPos.Value);
        if (tile == null)
        {
            _log.Warning("Tile not found at position: {Position}", obj.GridPosition);
            return;
        }

        // Update tile type based on terrain created
        switch (obj.CreatesTerrainOnDestroy)
        {
            case "Difficult":
                // Difficult terrain (rubble, debris)
                // For now, mark in tile metadata - future: add TileType.Difficult
                _log.Information("Difficult terrain created at {Position} (rubble from {ObjectName})",
                    obj.GridPosition, obj.Name);
                // Could add: tile.TerrainModifier = "Difficult"
                break;

            case "Hazardous":
                // Hazardous terrain (poison cloud, steam, fire)
                // For now, log - future: integrate with DynamicHazard system
                _log.Information("Hazardous terrain created at {Position} ({ObjectName} aftermath), Duration={Duration}",
                    obj.GridPosition, obj.Name,
                    obj.TerrainDuration.HasValue ? $"{obj.TerrainDuration} turns" : "Permanent");
                // Could add: tile.HazardType = specific hazard
                break;

            default:
                _log.Debug("Unknown terrain type: {TerrainType}", obj.CreatesTerrainOnDestroy);
                break;
        }

        // Log duration for future timed terrain cleanup
        if (obj.TerrainDuration.HasValue)
        {
            _log.Debug("Terrain has {Duration} turn duration (implement cleanup timer)",
                obj.TerrainDuration);
        }
    }

    /// <summary>
    /// Triggers chain reaction from object destruction
    /// v0.22.1: Chain reaction system
    /// </summary>
    private List<DestructionResult> TriggerChainReaction(
        EnvironmentalObject sourceObj,
        int combatInstanceId)
    {
        _log.Information(
            "Checking for chain reactions from {ObjectId} explosion (radius: {Radius})",
            sourceObj.ObjectId, sourceObj.ExplosionRadius);

        var chainResults = new List<DestructionResult>();

        // Get all positions in explosion radius
        var nearbyPositions = GetPositionsInRadius(sourceObj.GridPosition, sourceObj.ExplosionRadius);

        foreach (var position in nearbyPositions)
        {
            var objects = GetObjectsAtPosition(sourceObj.RoomId, position);

            foreach (var obj in objects)
            {
                // Don't trigger self or already destroyed objects
                if (obj.ObjectId == sourceObj.ObjectId || obj.State == EnvironmentalObjectState.Destroyed)
                    continue;

                // Apply explosion damage to destructible objects
                if (obj.IsDestructible)
                {
                    var (explosionDamage, _) = ParseDamageFormula(sourceObj.DamageFormula ?? "0 Physical");

                    var result = ApplyDamageToObject(
                        obj.ObjectId,
                        explosionDamage,
                        "Explosive",
                        attackerId: 0, // Chain reaction, not player
                        combatInstanceId);

                    if (result.ObjectDestroyed)
                    {
                        chainResults.Add(result);
                        _log.Information("Chain reaction destroyed {ObjectId} ({Name})",
                            obj.ObjectId, obj.Name);
                    }
                }
            }
        }

        if (chainResults.Count > 0)
        {
            _log.Information("Chain reaction triggered {Count} additional destructions", chainResults.Count);
        }

        return chainResults;
    }

    #endregion

    #region Cover Management

    /// <summary>
    /// Gets cover data at a specific position (integrates with v0.20.2 CoverService)
    /// </summary>
    public CoverData? GetCoverAtPosition(int roomId, string gridPosition, string attackDirection)
    {
        var coverObjects = GetObjectsAtPosition(roomId, gridPosition)
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

        // Check if cover blocks this attack direction
        if (!CoverBlocksDirection(bestCover, attackDirection))
        {
            return null;
        }

        int defenseBonus = GetCoverDefenseBonus(bestCover.CoverQuality);

        _log.Debug(
            "Cover found at {Position}: {Name} ({Quality}, {Durability}/{MaxDurability})",
            gridPosition, bestCover.Name, bestCover.CoverQuality,
            bestCover.CurrentDurability, bestCover.MaxDurability);

        return new CoverData
        {
            Quality = bestCover.CoverQuality,
            DefenseBonus = defenseBonus,
            RemainingDurability = bestCover.CurrentDurability,
            Description = bestCover.Description
        };
    }

    /// <summary>
    /// Checks if cover blocks attack from a given direction
    /// </summary>
    private bool CoverBlocksDirection(EnvironmentalObject cover, string attackDirection)
    {
        if (string.IsNullOrEmpty(cover.CoverArc))
            return true; // Blocks all directions

        // In production, would parse JSON array and check direction
        // For now, simplified logic
        return true;
    }

    /// <summary>
    /// Gets defense bonus for cover quality
    /// </summary>
    private int GetCoverDefenseBonus(CoverQuality quality)
    {
        return quality switch
        {
            CoverQuality.Light => 2,
            CoverQuality.Heavy => 4,
            CoverQuality.Total => 6,
            _ => 0
        };
    }

    /// <summary>
    /// Validates if cover blocks a path between attacker and target
    /// </summary>
    public bool ValidateCoverPath(string attackerPosition, string targetPosition)
    {
        // TODO: Implement proper line of sight and cover arc calculation
        // For parent spec, just check if target has cover
        // This would integrate with BattlefieldGrid in production
        return false;
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

    #region Utility & Helper Methods

    /// <summary>
    /// Parses damage formula "15 Fire" into (damage, type)
    /// </summary>
    private (int damage, string type) ParseDamageFormula(string formula)
    {
        var parts = formula.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            if (int.TryParse(parts[0], out int damage))
            {
                return (damage, parts[1]);
            }
        }

        // Fallback
        _log.Warning("Could not parse damage formula: {Formula}", formula);
        return (0, "Physical");
    }

    /// <summary>
    /// Parses damage type from formula "15 Fire" → "Fire"
    /// </summary>
    private string ParseDamageType(string formula)
    {
        var parts = formula.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? parts[1] : "Physical";
    }

    /// <summary>
    /// Gets all character IDs in radius from a position
    /// v0.22.1: Full grid integration
    /// </summary>
    private List<int> GetCharactersInRadius(string? centerPosition, int radius)
    {
        if (_currentGrid == null || string.IsNullOrEmpty(centerPosition))
        {
            _log.Warning("GetCharactersInRadius called without grid or position");
            return new List<int>();
        }

        var centerGridPos = ParseGridPosition(centerPosition);
        if (centerGridPos == null)
        {
            _log.Warning("Could not parse grid position: {Position}", centerPosition);
            return new List<int>();
        }

        var affectedCharacters = new List<int>();

        // Check all tiles within Manhattan distance of radius
        foreach (var kvp in _currentGrid.Tiles)
        {
            var tile = kvp.Value;
            var distance = CalculateGridDistance(centerGridPos.Value, tile.Position);

            if (distance <= radius && tile.IsOccupied && !string.IsNullOrEmpty(tile.OccupantId))
            {
                // Convert occupant ID to integer (hash code for consistent IDs)
                int characterId = tile.OccupantId.GetHashCode();
                affectedCharacters.Add(characterId);

                _log.Debug("Character in radius: OccupantId={OccupantId}, CharacterId={CharacterId}, Distance={Distance}",
                    tile.OccupantId, characterId, distance);
            }
        }

        _log.Information("GetCharactersInRadius: Position={Position}, Radius={Radius}, Found={Count} characters",
            centerPosition, radius, affectedCharacters.Count);

        return affectedCharacters;
    }

    /// <summary>
    /// Gets all grid positions in radius from a center position
    /// v0.22.1: Full grid integration
    /// </summary>
    private List<string> GetPositionsInRadius(string? centerPosition, int radius)
    {
        if (_currentGrid == null || string.IsNullOrEmpty(centerPosition))
        {
            _log.Warning("GetPositionsInRadius called without grid or position");
            return centerPosition != null ? new List<string> { centerPosition } : new List<string>();
        }

        var centerGridPos = ParseGridPosition(centerPosition);
        if (centerGridPos == null)
        {
            _log.Warning("Could not parse grid position: {Position}", centerPosition);
            return new List<string> { centerPosition };
        }

        var positions = new List<string>();

        // Check all tiles within Manhattan distance of radius
        foreach (var kvp in _currentGrid.Tiles)
        {
            var tile = kvp.Value;
            var distance = CalculateGridDistance(centerGridPos.Value, tile.Position);

            if (distance <= radius)
            {
                positions.Add(FormatGridPosition(tile.Position));
            }
        }

        _log.Debug("GetPositionsInRadius: Position={Position}, Radius={Radius}, Found={Count} positions",
            centerPosition, radius, positions.Count);

        return positions;
    }

    /// <summary>
    /// Parses a string grid position to GridPosition struct
    /// Format: "Front_Left_Column_2" or "Player/Front/Col0"
    /// </summary>
    private GridPosition? ParseGridPosition(string position)
    {
        if (string.IsNullOrEmpty(position))
            return null;

        try
        {
            // Handle format: "Front_Left_Column_2" or "Player/Front/Col0" or "Enemy/Back/Col1"
            if (position.Contains("/"))
            {
                // Format: "Player/Front/Col0"
                var parts = position.Split('/');
                if (parts.Length >= 3)
                {
                    var zone = Enum.Parse<Zone>(parts[0], ignoreCase: true);
                    var row = Enum.Parse<Row>(parts[1], ignoreCase: true);
                    var colStr = parts[2].Replace("Col", "").Replace("Column", "");
                    if (int.TryParse(colStr, out int column))
                    {
                        return new GridPosition(zone, row, column);
                    }
                }
            }
            else
            {
                // Format: "Front_Left_Column_2" - assume Player zone for legacy compatibility
                var parts = position.Split('_');

                // Extract row
                Row row = Row.Front;
                if (position.Contains("Back", StringComparison.OrdinalIgnoreCase))
                    row = Row.Back;

                // Extract zone (assume Player if not specified, Enemy if specified)
                Zone zone = Zone.Player;
                if (position.Contains("Enemy", StringComparison.OrdinalIgnoreCase))
                    zone = Zone.Enemy;
                else if (position.Contains("Right", StringComparison.OrdinalIgnoreCase))
                    zone = Zone.Enemy; // Legacy: Right = Enemy zone

                // Extract column number
                int column = 0;
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int col))
                    {
                        column = col;
                        break;
                    }
                }

                return new GridPosition(zone, row, column);
            }
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to parse grid position: {Position}", position);
        }

        return null;
    }

    /// <summary>
    /// Formats a GridPosition struct back to string format
    /// </summary>
    private string FormatGridPosition(GridPosition position)
    {
        return $"{position.Zone}/{position.Row}/Col{position.Column}";
    }

    /// <summary>
    /// Calculates Manhattan distance between two grid positions
    /// Considers zone, row, and column differences
    /// </summary>
    private int CalculateGridDistance(GridPosition from, GridPosition to)
    {
        // Different zones = at least distance of 2 (across the battlefield)
        if (from.Zone != to.Zone)
            return 2 + Math.Abs(from.Column - to.Column);

        // Same zone but different rows = distance 1 + column difference
        if (from.Row != to.Row)
            return 1 + Math.Abs(from.Column - to.Column);

        // Same zone and row = just column difference
        return Math.Abs(from.Column - to.Column);
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

    /// <summary>
    /// Updates cooldowns for all reusable hazards (called each turn)
    /// v0.22.1: Cooldown management
    /// </summary>
    public void UpdateCooldowns(int roomId)
    {
        var reusableHazards = _objects.Values
            .Where(o => o.RoomId == roomId &&
                       o.CooldownDuration > 0 &&
                       o.CooldownRemaining > 0)
            .ToList();

        foreach (var hazard in reusableHazards)
        {
            hazard.CooldownRemaining--;

            if (hazard.CooldownRemaining <= 0)
            {
                hazard.State = EnvironmentalObjectState.Active;
                hazard.TriggersRemaining = 1; // Re-arm

                _log.Information("Hazard {ObjectId} ({Name}) cooldown complete - re-armed",
                    hazard.ObjectId, hazard.Name);
            }
        }
    }

    #endregion
}
