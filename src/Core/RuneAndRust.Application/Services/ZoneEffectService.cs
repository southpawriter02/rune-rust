namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for managing persistent zone effects during combat.
/// </summary>
/// <remarks>
/// <para>ZoneEffectService is responsible for the complete lifecycle of zone effects:</para>
/// <list type="bullet">
///   <item><description>Creating zones from definitions with calculated affected cells</description></item>
///   <item><description>Tracking active zones during combat</description></item>
///   <item><description>Ticking zones each round to apply effects and manage duration</description></item>
///   <item><description>Querying zones by position, caster, or other criteria</description></item>
///   <item><description>Handling entity movement into/out of zones</description></item>
/// </list>
/// <para>Zone effects are processed during the round tick phase, after initiative but before
/// individual turns are processed.</para>
/// <para>Key mechanics:</para>
/// <list type="bullet">
///   <item><description>Damage zones deal damage each tick to enemies (or friendlies if configured)</description></item>
///   <item><description>Healing zones restore health each tick to allies</description></item>
///   <item><description>Buff/Debuff zones apply status effects when entities enter</description></item>
///   <item><description>Terrain zones modify movement through affected cells</description></item>
///   <item><description>Mixed zones can combine multiple effect types</description></item>
/// </list>
/// </remarks>
public class ZoneEffectService : IZoneEffectService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Maximum number of zones a single caster can have active at once.
    /// </summary>
    /// <remarks>
    /// <para>When this limit is reached, the oldest zone is automatically removed.</para>
    /// </remarks>
    private const int MaxZonesPerCaster = 10;

    /// <summary>
    /// Default damage type for zones that don't specify one.
    /// </summary>
    private const string DefaultDamageType = "magic";

    /// <summary>
    /// Reason string for duration-based expiration.
    /// </summary>
    private const string ExpirationReasonDuration = "Duration";

    /// <summary>
    /// Reason string for manual removal.
    /// </summary>
    private const string ExpirationReasonRemoved = "Removed";

    /// <summary>
    /// Reason string for combat ending.
    /// </summary>
    private const string ExpirationReasonCombatEnded = "Combat Ended";

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IZoneProvider _zoneProvider;
    private readonly ICombatGridService _gridService;
    private readonly IDiceService _diceService;
    private readonly IBuffDebuffService _buffDebuffService;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<ZoneEffectService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Collection of all currently active zones.
    /// </summary>
    private readonly List<ZoneEffect> _activeZones = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the ZoneEffectService.
    /// </summary>
    /// <param name="zoneProvider">Provider for zone definitions.</param>
    /// <param name="gridService">Service for combat grid operations.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="buffDebuffService">Service for status effect management.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public ZoneEffectService(
        IZoneProvider zoneProvider,
        ICombatGridService gridService,
        IDiceService diceService,
        IBuffDebuffService buffDebuffService,
        IGameEventLogger eventLogger,
        ILogger<ZoneEffectService> logger)
    {
        _zoneProvider = zoneProvider ?? throw new ArgumentNullException(nameof(zoneProvider));
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _buffDebuffService = buffDebuffService ?? throw new ArgumentNullException(nameof(buffDebuffService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("ZoneEffectService initialized with MaxZonesPerCaster={Max}", MaxZonesPerCaster);
    }

    // ═══════════════════════════════════════════════════════════════
    // ZONE CREATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ZoneEffect CreateZone(string zoneId, GridPosition center, Combatant caster)
    {
        // Default to North direction for non-directional zones
        return CreateZone(zoneId, center, Direction.North, caster);
    }

    /// <inheritdoc />
    public ZoneEffect CreateZone(string zoneId, GridPosition center, Direction direction, Combatant caster)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);
        ArgumentNullException.ThrowIfNull(caster);

        _logger.LogDebug("CreateZone called: ZoneId={ZoneId}, Center={Center}, Direction={Direction}, Caster={Caster}",
            zoneId, center, direction, caster.DisplayName);

        // Get zone definition
        var definition = _zoneProvider.GetZone(zoneId);
        if (definition is null)
        {
            _logger.LogWarning("Zone definition not found: {ZoneId}", zoneId);
            throw new ArgumentException($"Zone definition not found: {zoneId}", nameof(zoneId));
        }

        // Check caster zone limit
        var casterZones = GetZonesByCaster(caster.Id);
        if (casterZones.Count >= MaxZonesPerCaster)
        {
            var oldestZone = casterZones.OrderBy(z => z.CreatedAt).First();
            _logger.LogWarning("{Caster} at max zone limit ({Max}), removing oldest zone: {Zone}",
                caster.DisplayName, MaxZonesPerCaster, oldestZone.Name);
            RemoveZone(oldestZone.Id);
        }

        // Calculate affected cells based on shape
        var affectedCells = CalculateAffectedCells(center, definition.Shape, definition.Radius, direction);

        _logger.LogDebug("Calculated {CellCount} affected cells for zone {Zone} with shape {Shape} radius {Radius}",
            affectedCells.Count, definition.Name, definition.Shape, definition.Radius);

        // Create the zone effect
        var zone = ZoneEffect.Create(definition, center, caster.Id, affectedCells);

        // Add to active zones
        _activeZones.Add(zone);

        _logger.LogInformation("{Caster} created zone '{Zone}' at {Position} affecting {CellCount} cells, duration {Duration} turns",
            caster.DisplayName, zone.Name, center, zone.CellCount, zone.RemainingDuration);

        // Publish creation event
        _eventLogger.LogCombat(
            "ZoneCreated",
            $"{caster.DisplayName} created zone '{zone.Name}' at ({center.X}, {center.Y}) affecting {zone.CellCount} cells",
            data: new Dictionary<string, object>
            {
                ["zoneEffectId"] = zone.Id,
                ["zoneId"] = zone.ZoneId,
                ["zoneName"] = zone.Name,
                ["centerX"] = center.X,
                ["centerY"] = center.Y,
                ["casterId"] = caster.Id,
                ["cellCount"] = zone.CellCount,
                ["duration"] = zone.RemainingDuration
            });

        return zone;
    }

    // ═══════════════════════════════════════════════════════════════
    // ZONE REMOVAL
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool RemoveZone(Guid zoneEffectId)
    {
        _logger.LogDebug("RemoveZone called for ZoneEffectId={ZoneEffectId}", zoneEffectId);

        var zone = _activeZones.FirstOrDefault(z => z.Id == zoneEffectId);
        if (zone is null)
        {
            _logger.LogDebug("Zone not found for removal: {ZoneEffectId}", zoneEffectId);
            return false;
        }

        _activeZones.Remove(zone);

        _logger.LogInformation("Zone '{Zone}' removed (manual)", zone.Name);

        // Publish expiration event with "Removed" reason
        _eventLogger.LogCombat(
            "ZoneExpired",
            $"Zone '{zone.Name}' was removed",
            data: new Dictionary<string, object>
            {
                ["zoneEffectId"] = zone.Id,
                ["zoneId"] = zone.ZoneId,
                ["zoneName"] = zone.Name,
                ["reason"] = ExpirationReasonRemoved
            });

        return true;
    }

    /// <inheritdoc />
    public int RemoveZonesByCaster(Guid casterId)
    {
        _logger.LogDebug("RemoveZonesByCaster called for CasterId={CasterId}", casterId);

        var casterZones = _activeZones.Where(z => z.CasterId == casterId).ToList();
        var removedCount = 0;

        foreach (var zone in casterZones)
        {
            _activeZones.Remove(zone);
            removedCount++;

            _logger.LogDebug("Removed zone '{Zone}' belonging to caster {CasterId}", zone.Name, casterId);

            _eventLogger.LogCombat(
                "ZoneExpired",
                $"Zone '{zone.Name}' was removed (caster cleanup)",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["zoneName"] = zone.Name,
                    ["reason"] = ExpirationReasonRemoved
                });
        }

        _logger.LogInformation("Removed {Count} zones for caster {CasterId}", removedCount, casterId);
        return removedCount;
    }

    /// <inheritdoc />
    public void ClearAllZones()
    {
        _logger.LogDebug("ClearAllZones called, clearing {Count} zones", _activeZones.Count);

        // Publish expiration events for all zones
        foreach (var zone in _activeZones)
        {
            _eventLogger.LogCombat(
                "ZoneExpired",
                $"Zone '{zone.Name}' expired (combat ended)",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["zoneName"] = zone.Name,
                    ["reason"] = ExpirationReasonCombatEnded
                });
        }

        var clearedCount = _activeZones.Count;
        _activeZones.Clear();

        _logger.LogInformation("All zones cleared ({Count} total)", clearedCount);
    }

    // ═══════════════════════════════════════════════════════════════
    // ZONE QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<ZoneEffect> GetZonesAt(GridPosition position)
    {
        var zones = _activeZones.Where(z => z.ContainsPosition(position)).ToList();

        _logger.LogDebug("GetZonesAt({Position}) found {Count} zones", position, zones.Count);

        return zones;
    }

    /// <inheritdoc />
    public IReadOnlyList<ZoneEffect> GetAllActiveZones()
    {
        _logger.LogDebug("GetAllActiveZones returning {Count} zones", _activeZones.Count);
        return _activeZones.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ZoneEffect> GetZonesByCaster(Guid casterId)
    {
        var zones = _activeZones.Where(z => z.CasterId == casterId).ToList();

        _logger.LogDebug("GetZonesByCaster({CasterId}) found {Count} zones", casterId, zones.Count);

        return zones;
    }

    /// <inheritdoc />
    public IReadOnlyList<ZoneEffect> GetZonesByType(ZoneEffectType effectType)
    {
        var zones = _activeZones.Where(z => z.EffectType == effectType).ToList();

        _logger.LogDebug("GetZonesByType({EffectType}) found {Count} zones", effectType, zones.Count);

        return zones;
    }

    /// <inheritdoc />
    public int GetActiveZoneCount()
    {
        return _activeZones.Count;
    }

    /// <inheritdoc />
    public int GetZoneCountByCaster(Guid casterId)
    {
        return _activeZones.Count(z => z.CasterId == casterId);
    }

    // ═══════════════════════════════════════════════════════════════
    // ZONE PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ZoneTickResult TickZones(IEnumerable<Combatant> combatants)
    {
        ArgumentNullException.ThrowIfNull(combatants);

        _logger.LogDebug("TickZones called with {ZoneCount} active zones", _activeZones.Count);

        var result = new ZoneTickResult();
        var combatantList = combatants.ToList();
        var zonesToRemove = new List<ZoneEffect>();

        // Process each zone
        foreach (var zone in _activeZones)
        {
            _logger.LogDebug("Processing zone '{Zone}' (Type={Type}, Duration={Duration})",
                zone.Name, zone.EffectType, zone.RemainingDuration);

            // Find combatants within this zone
            foreach (var combatant in combatantList)
            {
                // Get combatant position from grid
                var position = GetCombatantPosition(combatant);
                if (position is null)
                {
                    _logger.LogDebug("Combatant {Name} has no grid position, skipping", combatant.DisplayName);
                    continue;
                }

                // Check if combatant is within zone
                if (!zone.ContainsPosition(position.Value))
                {
                    continue;
                }

                // Check if this zone affects this combatant based on friendly/enemy rules
                var isFriendly = IsFriendlyToCaster(combatant, zone.CasterId, combatantList);

                if (isFriendly && !zone.AffectsFriendly)
                {
                    _logger.LogDebug("{Combatant} is friendly but zone does not affect friendlies", combatant.DisplayName);
                    continue;
                }
                if (!isFriendly && !zone.AffectsEnemy)
                {
                    _logger.LogDebug("{Combatant} is enemy but zone does not affect enemies", combatant.DisplayName);
                    continue;
                }

                // Apply zone effect to this combatant
                ApplyZoneEffect(zone, combatant, result);
            }

            // Tick duration
            if (zone.Tick())
            {
                zonesToRemove.Add(zone);
                result.RecordExpiration(zone.ZoneId);

                _logger.LogDebug("Zone '{Zone}' expired (duration reached 0)", zone.Name);
            }
        }

        // Remove expired zones
        foreach (var zone in zonesToRemove)
        {
            _activeZones.Remove(zone);

            _logger.LogInformation("Zone '{Zone}' expired and removed", zone.Name);

            _eventLogger.LogCombat(
                "ZoneExpired",
                $"Zone '{zone.Name}' expired (duration ended)",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["zoneName"] = zone.Name,
                    ["reason"] = ExpirationReasonDuration
                });
        }

        _logger.LogDebug("TickZones complete: Damage={Damage}, Healing={Healing}, Expired={Expired}",
            result.TotalDamageDealt, result.TotalHealingDone, result.ExpiredZones.Count);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // ENTITY MOVEMENT TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void OnEntityEntered(Combatant combatant, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var zones = GetZonesAt(position);

        foreach (var zone in zones)
        {
            _logger.LogDebug("{Combatant} entered zone '{Zone}' at {Position}",
                combatant.DisplayName, zone.Name, position);

            _eventLogger.LogCombat(
                "ZoneEntered",
                $"{combatant.DisplayName} entered zone '{zone.Name}' at ({position.X}, {position.Y})",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["combatantId"] = combatant.Id,
                    ["combatantName"] = combatant.DisplayName,
                    ["positionX"] = position.X,
                    ["positionY"] = position.Y
                });
        }
    }

    /// <inheritdoc />
    public void OnEntityExited(Combatant combatant, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var zones = GetZonesAt(position);

        foreach (var zone in zones)
        {
            _logger.LogDebug("{Combatant} exited zone '{Zone}' from {Position}",
                combatant.DisplayName, zone.Name, position);

            _eventLogger.LogCombat(
                "ZoneExited",
                $"{combatant.DisplayName} exited zone '{zone.Name}' from ({position.X}, {position.Y})",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["combatantId"] = combatant.Id,
                    ["combatantName"] = combatant.DisplayName,
                    ["positionX"] = position.X,
                    ["positionY"] = position.Y
                });

            // Note: Zone-based buffs/debuffs could be removed here if needed
            // For now, status effects persist until their own duration expires
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsPositionInZone(GridPosition position)
    {
        return _activeZones.Any(z => z.ContainsPosition(position));
    }

    /// <inheritdoc />
    public bool IsPositionInDamageZone(GridPosition position)
    {
        return _activeZones.Any(z =>
            z.ContainsPosition(position) &&
            (z.EffectType == ZoneEffectType.Damage || z.DealsDamage));
    }

    /// <inheritdoc />
    public bool IsPositionInHealingZone(GridPosition position)
    {
        return _activeZones.Any(z =>
            z.ContainsPosition(position) &&
            (z.EffectType == ZoneEffectType.Healing || z.ProvidesHealing));
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a combatant's grid position from the grid service or entity.
    /// </summary>
    /// <param name="combatant">The combatant to get position for.</param>
    /// <returns>The grid position, or null if not on grid.</returns>
    private GridPosition? GetCombatantPosition(Combatant combatant)
    {
        // Try to get from grid service first
        var gridPosition = _gridService.GetEntityPosition(combatant.Id);
        if (gridPosition.HasValue)
        {
            return gridPosition.Value;
        }

        // Fall back to entity's stored position
        if (combatant.IsPlayer && combatant.Player?.CombatGridPosition.HasValue == true)
        {
            return combatant.Player.CombatGridPosition;
        }

        if (combatant.IsMonster && combatant.Monster?.CombatGridPosition.HasValue == true)
        {
            return combatant.Monster.CombatGridPosition;
        }

        return null;
    }

    /// <summary>
    /// Determines if a combatant is friendly to the zone caster.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <param name="casterId">The caster's entity ID.</param>
    /// <param name="allCombatants">All combatants in the encounter.</param>
    /// <returns>True if friendly (same team), false if enemy.</returns>
    /// <remarks>
    /// <para>Friendliness is determined by comparing entity types:</para>
    /// <list type="bullet">
    ///   <item><description>If combatant is the caster, always friendly</description></item>
    ///   <item><description>Players are friendly to players</description></item>
    ///   <item><description>Monsters are friendly to monsters</description></item>
    ///   <item><description>Players and monsters are enemies</description></item>
    /// </list>
    /// </remarks>
    private bool IsFriendlyToCaster(Combatant combatant, Guid casterId, List<Combatant> allCombatants)
    {
        // Same entity is always friendly
        if (combatant.Id == casterId)
        {
            return true;
        }

        // Find the caster
        var caster = allCombatants.FirstOrDefault(c => c.Id == casterId);
        if (caster is null)
        {
            _logger.LogDebug("Caster {CasterId} not found in combatant list", casterId);
            return false;
        }

        // Same type (player-player or monster-monster) is friendly
        // Different types (player-monster) are enemies
        return combatant.IsPlayer == caster.IsPlayer;
    }

    /// <summary>
    /// Applies a zone's effect to a target combatant.
    /// </summary>
    /// <param name="zone">The zone applying its effect.</param>
    /// <param name="target">The target combatant.</param>
    /// <param name="result">The tick result to update.</param>
    private void ApplyZoneEffect(ZoneEffect zone, Combatant target, ZoneTickResult result)
    {
        _logger.LogDebug("Applying zone '{Zone}' effect ({Type}) to {Target}",
            zone.Name, zone.EffectType, target.DisplayName);

        switch (zone.EffectType)
        {
            case ZoneEffectType.Damage:
                ApplyDamageEffect(zone, target, result);
                break;

            case ZoneEffectType.Healing:
                ApplyHealingEffect(zone, target, result);
                break;

            case ZoneEffectType.Buff:
            case ZoneEffectType.Debuff:
                ApplyStatusEffect(zone, target);
                break;

            case ZoneEffectType.Terrain:
                ApplyTerrainEffect(zone, target);
                break;

            case ZoneEffectType.Mixed:
                // Apply all configured effects
                if (zone.DealsDamage)
                {
                    ApplyDamageEffect(zone, target, result);
                }
                if (zone.ProvidesHealing)
                {
                    ApplyHealingEffect(zone, target, result);
                }
                if (zone.AppliesStatusEffect)
                {
                    ApplyStatusEffect(zone, target);
                }
                break;
        }
    }

    /// <summary>
    /// Applies damage from a zone to a target.
    /// </summary>
    /// <param name="zone">The zone dealing damage.</param>
    /// <param name="target">The target combatant.</param>
    /// <param name="result">The tick result to update.</param>
    private void ApplyDamageEffect(ZoneEffect zone, Combatant target, ZoneTickResult result)
    {
        if (string.IsNullOrEmpty(zone.DamageValue))
        {
            _logger.LogDebug("Zone '{Zone}' has no damage value configured", zone.Name);
            return;
        }

        // Roll damage
        var damage = _diceService.RollTotal(zone.DamageValue);
        var damageType = zone.DamageType ?? DefaultDamageType;

        // Apply damage directly to the combatant's underlying entity
        var actualDamage = ApplyDamageToCombatant(target, damage);

        // Update result
        result.RecordDamage(zone.ZoneId, target.Id, actualDamage);

        _logger.LogInformation("Zone '{Zone}' dealt {Damage} {Type} damage to {Target} (rolled {Rolled})",
            zone.Name, actualDamage, damageType, target.DisplayName, damage);

        // Publish damage event
        _eventLogger.LogCombat(
            "ZoneDamage",
            $"Zone '{zone.Name}' dealt {actualDamage} {damageType} damage to {target.DisplayName}",
            data: new Dictionary<string, object>
            {
                ["zoneEffectId"] = zone.Id,
                ["zoneId"] = zone.ZoneId,
                ["targetId"] = target.Id,
                ["targetName"] = target.DisplayName,
                ["damage"] = actualDamage,
                ["damageType"] = damageType
            });
    }

    /// <summary>
    /// Applies healing from a zone to a target.
    /// </summary>
    /// <param name="zone">The zone providing healing.</param>
    /// <param name="target">The target combatant.</param>
    /// <param name="result">The tick result to update.</param>
    private void ApplyHealingEffect(ZoneEffect zone, Combatant target, ZoneTickResult result)
    {
        if (string.IsNullOrEmpty(zone.HealValue))
        {
            _logger.LogDebug("Zone '{Zone}' has no heal value configured", zone.Name);
            return;
        }

        // Roll healing
        var healRoll = _diceService.RollTotal(zone.HealValue);

        // Apply healing directly to the combatant's underlying entity
        var actualHealing = ApplyHealingToCombatant(target, healRoll);

        // Update result
        result.RecordHealing(zone.ZoneId, target.Id, actualHealing);

        _logger.LogInformation("Zone '{Zone}' healed {Target} for {Amount} (rolled {Rolled})",
            zone.Name, target.DisplayName, actualHealing, healRoll);

        // Publish healing event
        _eventLogger.LogCombat(
            "ZoneHeal",
            $"Zone '{zone.Name}' healed {target.DisplayName} for {actualHealing}",
            data: new Dictionary<string, object>
            {
                ["zoneEffectId"] = zone.Id,
                ["zoneId"] = zone.ZoneId,
                ["targetId"] = target.Id,
                ["targetName"] = target.DisplayName,
                ["healAmount"] = actualHealing
            });
    }

    /// <summary>
    /// Applies a status effect from a zone to a target.
    /// </summary>
    /// <param name="zone">The zone applying the status.</param>
    /// <param name="target">The target combatant.</param>
    private void ApplyStatusEffect(ZoneEffect zone, Combatant target)
    {
        if (string.IsNullOrEmpty(zone.StatusEffectId))
        {
            _logger.LogDebug("Zone '{Zone}' has no status effect configured", zone.Name);
            return;
        }

        // Get effect target
        var effectTarget = GetEffectTarget(target);
        if (effectTarget is null)
        {
            _logger.LogWarning("Cannot get effect target for {Combatant}", target.DisplayName);
            return;
        }

        // Apply effect via buff/debuff service
        var applyResult = _buffDebuffService.ApplyEffect(effectTarget, zone.StatusEffectId, zone.CasterId);

        if (applyResult.WasApplied)
        {
            _logger.LogInformation("Zone '{Zone}' applied status '{Status}' to {Target}",
                zone.Name, zone.StatusEffectId, target.DisplayName);

            // Publish status applied event
            _eventLogger.LogCombat(
                "ZoneStatusApplied",
                $"Zone '{zone.Name}' applied status '{zone.StatusEffectId}' to {target.DisplayName}",
                data: new Dictionary<string, object>
                {
                    ["zoneEffectId"] = zone.Id,
                    ["zoneId"] = zone.ZoneId,
                    ["targetId"] = target.Id,
                    ["targetName"] = target.DisplayName,
                    ["statusEffectId"] = zone.StatusEffectId
                });
        }
        else
        {
            _logger.LogDebug("Zone '{Zone}' failed to apply status to {Target}: {Reason}",
                zone.Name, target.DisplayName, applyResult.Message);
        }
    }

    /// <summary>
    /// Applies terrain effects from a zone to a target.
    /// </summary>
    /// <param name="zone">The zone with terrain modifiers.</param>
    /// <param name="target">The target combatant.</param>
    /// <remarks>
    /// <para>Terrain zones typically apply status effects like slowed or immobilized.</para>
    /// </remarks>
    private void ApplyTerrainEffect(ZoneEffect zone, Combatant target)
    {
        // Terrain zones apply their status effect if configured
        if (!string.IsNullOrEmpty(zone.StatusEffectId))
        {
            ApplyStatusEffect(zone, target);
        }

        _logger.LogDebug("Zone '{Zone}' applied terrain effect '{Terrain}' affecting {Target}",
            zone.Name, zone.TerrainModifier ?? "none", target.DisplayName);
    }

    /// <summary>
    /// Gets the IEffectTarget interface from a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to get the effect target from.</param>
    /// <returns>The effect target interface, or null if not available.</returns>
    /// <remarks>
    /// <para>Returns null if the underlying entity (Player/Monster) does not implement IEffectTarget.</para>
    /// <para>Currently, neither Player nor Monster implements IEffectTarget, so this will always return null
    /// until those entities are updated. Status effects will not be applied through this path.</para>
    /// </remarks>
    private static IEffectTarget? GetEffectTarget(Combatant combatant)
    {
        if (combatant.IsPlayer && combatant.Player is IEffectTarget playerTarget)
        {
            return playerTarget;
        }

        if (combatant.IsMonster && combatant.Monster is IEffectTarget monsterTarget)
        {
            return monsterTarget;
        }

        return null;
    }

    /// <summary>
    /// Applies damage directly to a combatant's underlying entity.
    /// </summary>
    /// <param name="combatant">The combatant to damage.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>The actual damage dealt after any reductions.</returns>
    /// <remarks>
    /// <para>This method directly calls TakeDamage on the underlying Player or Monster entity,
    /// bypassing the IEffectTarget interface requirement.</para>
    /// </remarks>
    private static int ApplyDamageToCombatant(Combatant combatant, int damage)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.TakeDamage(damage);
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            return combatant.Monster.TakeDamage(damage);
        }

        // Fallback: return the raw damage if we can't apply it
        return damage;
    }

    /// <summary>
    /// Applies healing directly to a combatant's underlying entity.
    /// </summary>
    /// <param name="combatant">The combatant to heal.</param>
    /// <param name="healing">The amount of healing to apply.</param>
    /// <returns>The actual healing done after any caps.</returns>
    /// <remarks>
    /// <para>This method directly calls Heal on the underlying Player or Monster entity,
    /// bypassing the IEffectTarget interface requirement.</para>
    /// </remarks>
    private static int ApplyHealingToCombatant(Combatant combatant, int healing)
    {
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.Heal(healing);
        }

        if (combatant.IsMonster && combatant.Monster != null)
        {
            return combatant.Monster.Heal(healing);
        }

        // Fallback: return the raw healing if we can't apply it
        return healing;
    }

    /// <summary>
    /// Calculates the affected cells for a zone based on its shape.
    /// </summary>
    /// <param name="center">The center position of the zone.</param>
    /// <param name="shape">The shape of the zone.</param>
    /// <param name="radius">The radius of the zone.</param>
    /// <param name="direction">The direction for directional shapes (Line, Cone).</param>
    /// <returns>A list of affected grid positions.</returns>
    /// <remarks>
    /// <para>Shape calculations:</para>
    /// <list type="bullet">
    ///   <item><description>Circle: All cells within Euclidean distance of radius</description></item>
    ///   <item><description>Square: All cells within Manhattan distance of radius</description></item>
    ///   <item><description>Line: Cells extending from center in the given direction</description></item>
    ///   <item><description>Cone: Expanding cells in the given direction</description></item>
    ///   <item><description>Ring: Cells at exactly the radius distance (hollow circle)</description></item>
    /// </list>
    /// </remarks>
    private IReadOnlyList<GridPosition> CalculateAffectedCells(
        GridPosition center,
        ZoneShape shape,
        int radius,
        Direction direction)
    {
        var cells = new List<GridPosition>();

        switch (shape)
        {
            case ZoneShape.Circle:
                cells.AddRange(CalculateCircleCells(center, radius));
                break;

            case ZoneShape.Square:
                cells.AddRange(CalculateSquareCells(center, radius));
                break;

            case ZoneShape.Line:
                cells.AddRange(CalculateLineCells(center, radius, direction));
                break;

            case ZoneShape.Cone:
                cells.AddRange(CalculateConeCells(center, radius, direction));
                break;

            case ZoneShape.Ring:
                cells.AddRange(CalculateRingCells(center, radius));
                break;

            default:
                _logger.LogWarning("Unknown zone shape: {Shape}", shape);
                break;
        }

        // Filter to valid grid positions
        var grid = _gridService.GetActiveGrid();
        if (grid is not null)
        {
            cells = cells.Where(c => grid.IsInBounds(c)).ToList();
        }

        _logger.LogDebug("CalculateAffectedCells: Shape={Shape}, Radius={Radius}, Direction={Direction}, Cells={Count}",
            shape, radius, direction, cells.Count);

        return cells;
    }

    /// <summary>
    /// Calculates cells for a circle shape.
    /// </summary>
    private static IEnumerable<GridPosition> CalculateCircleCells(GridPosition center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Use squared distance for circle check (avoids sqrt)
                if (x * x + y * y <= radius * radius)
                {
                    yield return new GridPosition(center.X + x, center.Y + y);
                }
            }
        }
    }

    /// <summary>
    /// Calculates cells for a square shape.
    /// </summary>
    private static IEnumerable<GridPosition> CalculateSquareCells(GridPosition center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                yield return new GridPosition(center.X + x, center.Y + y);
            }
        }
    }

    /// <summary>
    /// Calculates cells for a line shape extending in a direction.
    /// </summary>
    private static IEnumerable<GridPosition> CalculateLineCells(GridPosition center, int radius, Direction direction)
    {
        var (dx, dy) = GetDirectionOffset(direction);

        // Include center cell
        yield return center;

        // Extend in direction
        for (int i = 1; i <= radius; i++)
        {
            yield return new GridPosition(center.X + dx * i, center.Y + dy * i);
        }
    }

    /// <summary>
    /// Calculates cells for a cone shape expanding in a direction.
    /// </summary>
    /// <remarks>
    /// <para>Cone expands as it extends from the origin. Width increases with distance.</para>
    /// </remarks>
    private static IEnumerable<GridPosition> CalculateConeCells(GridPosition center, int radius, Direction direction)
    {
        var (dx, dy) = GetDirectionOffset(direction);

        // For each row extending in the direction
        for (int i = 1; i <= radius; i++)
        {
            var width = i; // Cone expands with distance

            // Calculate perpendicular offsets
            for (int w = -width; w <= width; w++)
            {
                // Direction determines primary axis
                if (dx != 0)
                {
                    // Moving horizontally (East/West), spread vertically
                    yield return new GridPosition(center.X + dx * i, center.Y + w);
                }
                else if (dy != 0)
                {
                    // Moving vertically (North/South), spread horizontally
                    yield return new GridPosition(center.X + w, center.Y + dy * i);
                }
            }
        }
    }

    /// <summary>
    /// Calculates cells for a ring shape (hollow circle).
    /// </summary>
    private static IEnumerable<GridPosition> CalculateRingCells(GridPosition center, int radius)
    {
        var innerRadiusSq = (radius - 1) * (radius - 1);
        var outerRadiusSq = radius * radius;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                var distSq = x * x + y * y;

                // Include cells on the ring (between inner and outer radius)
                if (distSq <= outerRadiusSq && distSq >= innerRadiusSq)
                {
                    yield return new GridPosition(center.X + x, center.Y + y);
                }
            }
        }
    }

    /// <summary>
    /// Gets the X/Y offset for a cardinal direction.
    /// </summary>
    /// <param name="direction">The direction to get offset for.</param>
    /// <returns>Tuple of (dx, dy) offset values.</returns>
    /// <remarks>
    /// <para>Only cardinal directions (N/S/E/W) are supported for zone shapes.
    /// Up/Down return (0,0) as they are not applicable to 2D grid zones.</para>
    /// </remarks>
    private static (int dx, int dy) GetDirectionOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => (0, -1),
            Direction.South => (0, 1),
            Direction.East => (1, 0),
            Direction.West => (-1, 0),
            // Up/Down are vertical directions, not applicable to 2D grid zones
            Direction.Up => (0, 0),
            Direction.Down => (0, 0),
            _ => (0, 0)
        };
    }
}
