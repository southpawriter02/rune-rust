using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for the scout action in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Scout Action mechanics for the Wasteland Survival skill.
/// Allows characters to perform reconnaissance on adjacent rooms, revealing enemies,
/// hazards, and points of interest before committing to entry.
/// </para>
/// <para>
/// DC Calculation:
/// <list type="bullet">
///   <item><description>Base DC: From terrain type (8, 12, 16, 20, or 24)</description></item>
///   <item><description>Visibility modifier: Added to DC (positive = harder)</description></item>
/// </list>
/// </para>
/// <para>
/// Room Revelation Formula:
/// <code>
/// if (netSuccesses >= 0):
///     roomsRevealed = 1 + (netSuccesses / 2)
/// else:
///     roomsRevealed = 0
///
/// roomsRevealed = min(roomsRevealed, adjacentRoomCount)
/// </code>
/// </para>
/// <para>
/// Base DCs by terrain:
/// <list type="bullet">
///   <item><description>OpenWasteland: DC 8 (clear sightlines)</description></item>
///   <item><description>ModerateRuins: DC 12 (partial cover)</description></item>
///   <item><description>DenseRuins: DC 16 (many hiding spots)</description></item>
///   <item><description>Labyrinthine: DC 20 (twisting passages)</description></item>
///   <item><description>GlitchedLabyrinth: DC 24 (reality distortions)</description></item>
/// </list>
/// </para>
/// </remarks>
public class ScoutService : IScoutService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - SKILL IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in scouting checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - TERRAIN BASE DCS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base DC for scouting in Open Wasteland terrain.
    /// </summary>
    private const int OpenWastelandBaseDc = 8;

    /// <summary>
    /// Base DC for scouting in Moderate Ruins terrain.
    /// </summary>
    private const int ModerateRuinsBaseDc = 12;

    /// <summary>
    /// Base DC for scouting in Dense Ruins terrain.
    /// </summary>
    private const int DenseRuinsBaseDc = 16;

    /// <summary>
    /// Base DC for scouting in Labyrinthine terrain.
    /// </summary>
    private const int LabyrinthineBaseDc = 20;

    /// <summary>
    /// Base DC for scouting in Glitched Labyrinth terrain.
    /// </summary>
    private const int GlitchedLabyrinthBaseDc = 24;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - VISIBILITY MODIFIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DC modifier for excellent visibility conditions (-2 to DC).
    /// </summary>
    public const int ExcellentVisibilityModifier = -2;

    /// <summary>
    /// DC modifier for good visibility conditions (-1 to DC).
    /// </summary>
    public const int GoodVisibilityModifier = -1;

    /// <summary>
    /// DC modifier for normal visibility conditions (no change).
    /// </summary>
    public const int NormalVisibilityModifier = 0;

    /// <summary>
    /// DC modifier for poor visibility conditions (+2 to DC).
    /// </summary>
    public const int PoorVisibilityModifier = 2;

    /// <summary>
    /// DC modifier for terrible visibility conditions (+4 to DC).
    /// </summary>
    public const int TerribleVisibilityModifier = 4;

    /// <summary>
    /// DC modifier for static storm conditions (+6 to DC).
    /// </summary>
    public const int StaticStormVisibilityModifier = 6;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - EQUIPMENT BONUSES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Equipment bonus for survival kit (-1 to effective DC).
    /// </summary>
    public const int SurvivalKitBonus = 1;

    /// <summary>
    /// Equipment bonus for binoculars (-2 to effective DC).
    /// </summary>
    public const int BinocularsBonus = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - ROOM REVELATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum number of rooms revealed on a successful scout (net successes >= 0).
    /// </summary>
    private const int BaseRoomsRevealed = 1;

    /// <summary>
    /// Divisor for calculating additional rooms revealed from net successes.
    /// Formula: roomsRevealed = BaseRoomsRevealed + (netSuccesses / SuccessesPerRoom)
    /// </summary>
    private const int SuccessesPerRoom = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - THREAT ASSESSMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Experience value threshold for Low threat monsters (0-50 XP).
    /// </summary>
    private const int LowThreatMaxXp = 50;

    /// <summary>
    /// Experience value threshold for Moderate threat monsters (51-150 XP).
    /// </summary>
    private const int ModerateThreatMaxXp = 150;

    /// <summary>
    /// Experience value threshold for High threat monsters (151-400 XP).
    /// </summary>
    private const int HighThreatMaxXp = 400;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly ILogger<ScoutService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the ScoutService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public ScoutService(
        SkillCheckService skillCheckService,
        ILogger<ScoutService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("ScoutService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTING OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public ScoutResult PerformScout(Player player, ScoutContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogInformation(
            "PlayerId={PlayerId} initiating scout action from room {CurrentRoomId}",
            player.Id, context.CurrentRoomId);

        // Check if scouting can proceed
        if (!CanScout(player, context))
        {
            var reason = GetScoutBlockedReason(player, context);
            _logger.LogWarning(
                "Scouting blocked for PlayerId={PlayerId}: {Reason}",
                player.Id, reason);

            return ScoutResult.Empty(reason);
        }

        // Calculate the target DC
        var dc = GetScoutDc(context.TerrainType, context.VisibilityModifier);

        _logger.LogInformation(
            "PlayerId={PlayerId} scouting in {TerrainType} terrain (BaseDC={BaseDc}, " +
            "VisibilityMod={VisibilityMod}, FinalDC={FinalDc}, EquipmentBonus={EquipmentBonus})",
            player.Id, context.TerrainType, context.BaseDc,
            context.VisibilityModifier, dc, context.EquipmentBonus);

        // Perform the skill check
        // Note: Equipment bonus is passed as dice bonus to the check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WastelandSurvivalSkillId,
            dc,
            $"Scout {context.TerrainType.GetDisplayName()} terrain",
            AdvantageType.Normal,
            context.EquipmentBonus);

        var netSuccesses = checkResult.NetSuccesses;
        var rollDetails = BuildRollDetails(checkResult, context, dc);

        _logger.LogDebug(
            "Skill check result for PlayerId={PlayerId}: " +
            "NetSuccesses={NetSuccesses}, TotalSuccesses={TotalSuccesses}, " +
            "TotalBotches={TotalBotches}, Success={Success}",
            player.Id, netSuccesses,
            checkResult.DiceResult.TotalSuccesses,
            checkResult.DiceResult.TotalBotches,
            checkResult.IsSuccess);

        // Check for success (net successes >= 0 reveals at least 1 room)
        if (netSuccesses >= 0)
        {
            var roomsToReveal = CalculateRoomsRevealed(netSuccesses, context.AdjacentRoomCount);

            _logger.LogInformation(
                "PlayerId={PlayerId} successfully scouted: NetSuccesses={NetSuccesses}, " +
                "RoomsRevealed={RoomsRevealed} of {AdjacentCount} adjacent",
                player.Id, netSuccesses, roomsToReveal, context.AdjacentRoomCount);

            // Note: In a real implementation, we would retrieve room data from a repository
            // For now, we return an empty list as room resolution requires external data
            var revealedRooms = new List<ScoutedRoom>();

            return ScoutResult.Success(
                rooms: revealedRooms,
                netSuccesses: netSuccesses,
                targetDc: dc,
                rollDetails: rollDetails);
        }

        // Failure - no rooms revealed
        _logger.LogDebug(
            "PlayerId={PlayerId} failed scout: NetSuccesses={NetSuccesses} vs DC {Dc}",
            player.Id, netSuccesses, dc);

        return ScoutResult.Failure(
            netSuccesses: netSuccesses,
            targetDc: dc,
            rollDetails: rollDetails);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetBaseDc(NavigationTerrainType terrainType)
    {
        var dc = terrainType switch
        {
            NavigationTerrainType.OpenWasteland => OpenWastelandBaseDc,
            NavigationTerrainType.ModerateRuins => ModerateRuinsBaseDc,
            NavigationTerrainType.DenseRuins => DenseRuinsBaseDc,
            NavigationTerrainType.Labyrinthine => LabyrinthineBaseDc,
            NavigationTerrainType.GlitchedLabyrinth => GlitchedLabyrinthBaseDc,
            _ => ModerateRuinsBaseDc // Default to moderate DC for unknown terrain
        };

        _logger.LogDebug(
            "Base DC for terrain {TerrainType}: {Dc}",
            terrainType, dc);

        return dc;
    }

    /// <inheritdoc/>
    public int GetScoutDc(NavigationTerrainType terrainType, int visibilityModifier)
    {
        var baseDc = GetBaseDc(terrainType);
        var finalDc = Math.Max(1, baseDc + visibilityModifier);

        _logger.LogDebug(
            "Scout DC calculation: Base={BaseDc} + Visibility={VisibilityMod} = {FinalDc} (min 1)",
            baseDc, visibilityModifier, finalDc);

        return finalDc;
    }

    /// <inheritdoc/>
    public int CalculateRoomsRevealed(int netSuccesses, int adjacentRoomCount)
    {
        // Failure reveals nothing
        if (netSuccesses < 0)
        {
            _logger.LogDebug(
                "Scout failed with {NetSuccesses} net successes - no rooms revealed",
                netSuccesses);
            return 0;
        }

        // Success: base room + 1 per 2 additional successes
        var roomsFromSuccesses = BaseRoomsRevealed + (netSuccesses / SuccessesPerRoom);

        // Cannot reveal more rooms than are adjacent
        var actualRooms = Math.Min(roomsFromSuccesses, adjacentRoomCount);

        _logger.LogDebug(
            "Rooms revealed calculation: {BaseRooms} + ({NetSuccesses} / {Divisor}) = {Calculated}, " +
            "capped at {Adjacent} = {Actual}",
            BaseRoomsRevealed, netSuccesses, SuccessesPerRoom, roomsFromSuccesses,
            adjacentRoomCount, actualRooms);

        return actualRooms;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ROOM INFORMATION GATHERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<DetectedEnemy> DetectEnemies(Room room)
    {
        ArgumentNullException.ThrowIfNull(room, nameof(room));

        var detectedEnemies = new List<DetectedEnemy>();

        // Get alive monsters from the room
        var aliveMonsters = room.GetAliveMonsters().ToList();

        if (aliveMonsters.Count == 0)
        {
            _logger.LogDebug(
                "No enemies detected in room {RoomId}",
                room.Id);
            return detectedEnemies;
        }

        // Group monsters by type (using MonsterDefinitionId or Name)
        var monsterGroups = aliveMonsters
            .GroupBy(m => m.MonsterDefinitionId ?? m.Name)
            .ToList();

        foreach (var group in monsterGroups)
        {
            var representative = group.First();
            var count = group.Count();

            // Determine threat level from experience value
            var threatLevel = DetermineThreatLevel(representative.ExperienceValue);

            // Determine position based on room context
            var position = DetermineMonsterPosition(representative, room);

            var detectedEnemy = DetectedEnemy.CreateWithThreatLevel(
                enemyType: representative.Name,
                count: count,
                threatLevel: threatLevel,
                position: position);

            detectedEnemies.Add(detectedEnemy);

            _logger.LogDebug(
                "Detected {Count} {EnemyType} ({ThreatLevel}) at {Position} in room {RoomId}",
                count, representative.Name, threatLevel, position, room.Id);
        }

        _logger.LogInformation(
            "Detected {TotalEnemies} enemies ({GroupCount} groups) in room {RoomId}",
            aliveMonsters.Count, detectedEnemies.Count, room.Id);

        return detectedEnemies;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DetectedHazard> DetectHazards(Room room)
    {
        ArgumentNullException.ThrowIfNull(room, nameof(room));

        var detectedHazards = new List<DetectedHazard>();

        // Get active hazard zones from the room
        var activeHazards = room.GetActiveHazards().ToList();

        if (activeHazards.Count == 0)
        {
            _logger.LogDebug(
                "No hazards detected in room {RoomId}",
                room.Id);
            return detectedHazards;
        }

        foreach (var hazardZone in activeHazards)
        {
            // Map HazardType to DetectableHazardType
            var detectableType = MapToDetectableHazardType(hazardZone.HazardType);

            // Determine severity based on hazard characteristics
            var severity = DetermineHazardSeverity(hazardZone);

            var detectedHazard = DetectedHazard.Create(
                hazardType: detectableType,
                severity: severity,
                customDescription: hazardZone.Description);

            detectedHazards.Add(detectedHazard);

            _logger.LogDebug(
                "Detected hazard {HazardName} ({DetectableType}, {Severity}) in room {RoomId}",
                hazardZone.Name, detectableType, severity, room.Id);
        }

        _logger.LogInformation(
            "Detected {HazardCount} hazards in room {RoomId}",
            detectedHazards.Count, room.Id);

        return detectedHazards;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PointOfInterest> DetectPointsOfInterest(Room room)
    {
        ArgumentNullException.ThrowIfNull(room, nameof(room));

        var pointsOfInterest = new List<PointOfInterest>();

        // Detect containers from items and interactables
        DetectContainers(room, pointsOfInterest);

        // Detect mechanisms from interactive objects
        DetectMechanisms(room, pointsOfInterest);

        // Detect resource nodes from harvestable features
        DetectResourceNodes(room, pointsOfInterest);

        // Detect crafting stations as mechanisms
        DetectCraftingStations(room, pointsOfInterest);

        // Detect landmarks from room features
        DetectLandmarks(room, pointsOfInterest);

        _logger.LogInformation(
            "Detected {PoiCount} points of interest in room {RoomId}",
            pointsOfInterest.Count, room.Id);

        return pointsOfInterest;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public string GetTerrainDisplayName(NavigationTerrainType terrainType)
    {
        return terrainType.GetDisplayName();
    }

    /// <inheritdoc/>
    public string GetTerrainScoutingDescription(NavigationTerrainType terrainType)
    {
        return terrainType switch
        {
            NavigationTerrainType.OpenWasteland =>
                "Clear sightlines make scouting easy. Enemies and features are readily visible.",
            NavigationTerrainType.ModerateRuins =>
                "Partial cover requires careful observation. Some threats may be obscured.",
            NavigationTerrainType.DenseRuins =>
                "Many hiding spots make thorough scouting difficult. Enemies could be anywhere.",
            NavigationTerrainType.Labyrinthine =>
                "Twisting passages severely limit visibility. Scouting is challenging.",
            NavigationTerrainType.GlitchedLabyrinth =>
                "Reality distortions make accurate observation nearly impossible. " +
                "Even successful scouting may be unreliable.",
            _ => "Unknown terrain conditions."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTING PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CanScout(Player player, ScoutContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for no adjacent rooms
        if (context.AdjacentRoomCount == 0)
        {
            _logger.LogDebug(
                "Scouting blocked for PlayerId={PlayerId}: No adjacent rooms",
                player.Id);
            return false;
        }

        // Note: Additional checks could include:
        // - Active Blinded status effect
        // - Insufficient light level
        // - Other incapacitating conditions
        // These would require integration with a status effect service

        _logger.LogDebug(
            "Scouting allowed for PlayerId={PlayerId}",
            player.Id);

        return true;
    }

    /// <inheritdoc/>
    public string? GetScoutBlockedReason(Player player, ScoutContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for no adjacent rooms
        if (context.AdjacentRoomCount == 0)
        {
            return "There are no adjacent rooms to scout.";
        }

        // Note: Additional checks could include:
        // - Active Blinded status effect: "You cannot see well enough to scout."
        // - Insufficient light: "It is too dark to scout effectively."
        // - Incapacitated: "You are unable to scout in your current condition."

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS - THREAT ASSESSMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines threat level from a monster's experience value.
    /// </summary>
    /// <param name="experienceValue">The monster's XP reward.</param>
    /// <returns>The assessed threat level.</returns>
    /// <remarks>
    /// Threat thresholds:
    /// <list type="bullet">
    ///   <item><description>Low: 0-50 XP</description></item>
    ///   <item><description>Moderate: 51-150 XP</description></item>
    ///   <item><description>High: 151-400 XP</description></item>
    ///   <item><description>Extreme: 401+ XP</description></item>
    /// </list>
    /// </remarks>
    private ThreatLevel DetermineThreatLevel(int experienceValue)
    {
        return experienceValue switch
        {
            <= LowThreatMaxXp => ThreatLevel.Low,
            <= ModerateThreatMaxXp => ThreatLevel.Moderate,
            <= HighThreatMaxXp => ThreatLevel.High,
            _ => ThreatLevel.Extreme
        };
    }

    /// <summary>
    /// Determines the position description for a monster in a room.
    /// </summary>
    /// <param name="monster">The monster to describe.</param>
    /// <param name="room">The room containing the monster.</param>
    /// <returns>A position description string.</returns>
    private static string DetermineMonsterPosition(Monster monster, Room room)
    {
        // Use monster behavior to infer position
        return monster.Behavior switch
        {
            AIBehavior.Defensive => "guarding the area",
            AIBehavior.Aggressive => "patrolling aggressively",
            AIBehavior.Cowardly => "lurking in the shadows",
            AIBehavior.Chaotic => "moving unpredictably",
            AIBehavior.Support => "positioned to support allies",
            _ => "somewhere in the room"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS - HAZARD MAPPING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps a HazardType (combat zones) to DetectableHazardType (exploration).
    /// </summary>
    /// <param name="hazardType">The combat hazard type.</param>
    /// <returns>The corresponding detectable hazard type.</returns>
    private static DetectableHazardType MapToDetectableHazardType(HazardType hazardType)
    {
        return hazardType switch
        {
            // Toxic/environmental hazards
            HazardType.PoisonGas => DetectableHazardType.ToxicZone,
            HazardType.AcidPool => DetectableHazardType.ToxicZone,

            // Obvious physical hazards
            HazardType.Fire => DetectableHazardType.ObviousDanger,
            HazardType.Ice => DetectableHazardType.ObviousDanger,
            HazardType.Spikes => DetectableHazardType.ObviousDanger,
            HazardType.Electricity => DetectableHazardType.ObviousDanger,

            // Magical/supernatural hazards
            HazardType.Darkness => DetectableHazardType.GlitchPocket,
            HazardType.Radiant => DetectableHazardType.GlitchPocket,
            HazardType.Necrotic => DetectableHazardType.GlitchPocket,

            // Default to obvious danger
            _ => DetectableHazardType.ObviousDanger
        };
    }

    /// <summary>
    /// Determines hazard severity from a HazardZone's characteristics.
    /// </summary>
    /// <param name="hazardZone">The hazard zone to assess.</param>
    /// <returns>The determined severity level.</returns>
    private HazardSeverity DetermineHazardSeverity(HazardZone hazardZone)
    {
        // Check for lethal indicators
        if (hazardZone.HazardType == HazardType.Necrotic ||
            (hazardZone.AppliesStatus && hazardZone.DamagePerTurn))
        {
            return HazardSeverity.Lethal;
        }

        // Check for severe hazards (high damage or status effects)
        if (hazardZone.DamagePerTurn || hazardZone.AppliesStatus)
        {
            return HazardSeverity.Severe;
        }

        // Check for moderate hazards (entry damage only)
        if (hazardZone.DamageOnEntry)
        {
            return HazardSeverity.Moderate;
        }

        // Default to minor
        return HazardSeverity.Minor;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS - POINT OF INTEREST DETECTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Detects container-type points of interest from room items.
    /// </summary>
    /// <param name="room">The room to scan.</param>
    /// <param name="pointsOfInterest">List to add detected POIs to.</param>
    private void DetectContainers(Room room, List<PointOfInterest> pointsOfInterest)
    {
        // Check for dropped items (loot)
        if (room.HasDroppedLoot)
        {
            pointsOfInterest.Add(PointOfInterest.Container(
                "Dropped items lie scattered on the ground."));

            _logger.LogDebug(
                "Detected dropped loot container in room {RoomId}",
                room.Id);
        }

        // Check for regular items that suggest containers
        if (room.HasItems && room.Items.Count > 0)
        {
            pointsOfInterest.Add(PointOfInterest.Container(
                "Items can be seen in the room."));

            _logger.LogDebug(
                "Detected item container ({Count} items) in room {RoomId}",
                room.Items.Count, room.Id);
        }

        // Check for hidden items (suggests searchable area)
        var hiddenItems = room.GetUndiscoveredHiddenItems();
        if (hiddenItems.Count > 0)
        {
            pointsOfInterest.Add(PointOfInterest.Container(
                "Something about this area suggests hidden valuables."));

            _logger.LogDebug(
                "Detected hidden container ({Count} hidden items) in room {RoomId}",
                hiddenItems.Count, room.Id);
        }
    }

    /// <summary>
    /// Detects mechanism-type points of interest from interactive objects.
    /// </summary>
    /// <param name="room">The room to scan.</param>
    /// <param name="pointsOfInterest">List to add detected POIs to.</param>
    private void DetectMechanisms(Room room, List<PointOfInterest> pointsOfInterest)
    {
        if (!room.HasInteractables)
        {
            return;
        }

        foreach (var interactable in room.GetVisibleInteractables())
        {
            // Skip items that are better categorized elsewhere
            var poi = PointOfInterest.Mechanism(
                $"A {interactable.Name.ToLower()} can be interacted with.");

            pointsOfInterest.Add(poi);

            _logger.LogDebug(
                "Detected mechanism '{Name}' in room {RoomId}",
                interactable.Name, room.Id);
        }
    }

    /// <summary>
    /// Detects resource node points of interest from harvestable features.
    /// </summary>
    /// <param name="room">The room to scan.</param>
    /// <param name="pointsOfInterest">List to add detected POIs to.</param>
    private void DetectResourceNodes(Room room, List<PointOfInterest> pointsOfInterest)
    {
        if (!room.HasAvailableHarvestableFeatures)
        {
            return;
        }

        foreach (var feature in room.GetAvailableHarvestableFeatures())
        {
            var poi = PointOfInterest.ResourceNode(
                $"A {feature.Name.ToLower()} can be harvested for materials.");

            pointsOfInterest.Add(poi);

            _logger.LogDebug(
                "Detected resource node '{Name}' in room {RoomId}",
                feature.Name, room.Id);
        }
    }

    /// <summary>
    /// Detects crafting stations as mechanism points of interest.
    /// </summary>
    /// <param name="room">The room to scan.</param>
    /// <param name="pointsOfInterest">List to add detected POIs to.</param>
    private void DetectCraftingStations(Room room, List<PointOfInterest> pointsOfInterest)
    {
        if (!room.HasCraftingStations)
        {
            return;
        }

        foreach (var station in room.GetCraftingStations())
        {
            var poi = PointOfInterest.Mechanism(
                $"A {station.Name.ToLower()} is available for crafting.");

            pointsOfInterest.Add(poi);

            _logger.LogDebug(
                "Detected crafting station '{Name}' in room {RoomId}",
                station.Name, room.Id);
        }
    }

    /// <summary>
    /// Detects landmark points of interest from room features.
    /// </summary>
    /// <param name="room">The room to scan.</param>
    /// <param name="pointsOfInterest">List to add detected POIs to.</param>
    private void DetectLandmarks(Room room, List<PointOfInterest> pointsOfInterest)
    {
        if (!room.HasFeatures)
        {
            return;
        }

        foreach (var feature in room.Features)
        {
            var poi = PointOfInterest.Landmark(
                $"A notable {feature.DisplayName?.ToLower() ?? "feature"} stands out.");

            pointsOfInterest.Add(poi);

            _logger.LogDebug(
                "Detected landmark '{DisplayName}' in room {RoomId}",
                feature.DisplayName, room.Id);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS - ROLL DETAILS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a human-readable roll details string for the scout result.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="context">The scouting context.</param>
    /// <param name="dc">The scout DC.</param>
    /// <returns>A formatted string describing the roll details.</returns>
    private static string BuildRollDetails(
        SkillCheckResult checkResult,
        ScoutContext context,
        int dc)
    {
        var visibilityStr = context.VisibilityModifier switch
        {
            <= -2 => "excellent",
            -1 => "good",
            0 => "normal",
            <= 2 => "poor",
            <= 4 => "terrible",
            _ => "obscured"
        };

        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"DC: {dc} ({context.TerrainType.GetDisplayName()}, {visibilityStr} visibility)";
    }
}
