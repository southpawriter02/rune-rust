using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.22: Environmental Combat Service (Parent)
/// Orchestrates all environmental combat mechanics - the central hub for environmental systems.
/// Coordinates between sub-services to provide a unified environmental combat experience.
///
/// Responsibilities:
/// - Turn-based environmental processing (hazards, weather, ambient conditions)
/// - Room initialization with environmental elements
/// - Integration point for all environmental sub-systems
/// - Combat log generation for environmental events
///
/// Sub-Services:
/// - EnvironmentalObjectService (v0.22.1): Destructible terrain, interactive objects
/// - AmbientConditionService (v0.22.2): Room-wide persistent effects
/// - WeatherEffectService (v0.22.2): Dynamic weather conditions
/// - EnvironmentalManipulationService (v0.22.3): Push/pull, collapses, environmental kills
///
/// Integration with existing systems:
/// - v0.20 Tactical Grid System (positioning, movement)
/// - v0.21 Advanced Combat (stances, status effects, counter-attacks)
/// - v0.20.2 CoverService (cover bonuses and destruction)
/// - v0.6 HazardService (legacy hazard processing)
/// - v0.15 Trauma Economy (stress from/relief from environmental events)
/// </summary>
public class EnvironmentalCombatService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalCombatService>();

    private readonly EnvironmentalObjectService _objectService;
    private readonly AmbientConditionService _conditionService;
    private readonly WeatherEffectService _weatherService;
    private readonly EnvironmentalManipulationService _manipulationService;
    private readonly CoverService? _coverService;
    private readonly HazardService? _legacyHazardService;

    // Event tracking for combat logs
    private readonly List<EnvironmentalEvent> _combatEvents = new();

    public EnvironmentalCombatService(
        EnvironmentalObjectService objectService,
        AmbientConditionService conditionService,
        WeatherEffectService weatherService,
        EnvironmentalManipulationService manipulationService,
        CoverService? coverService = null,
        HazardService? legacyHazardService = null)
    {
        _objectService = objectService;
        _conditionService = conditionService;
        _weatherService = weatherService;
        _manipulationService = manipulationService;
        _coverService = coverService;
        _legacyHazardService = legacyHazardService;
    }

    #region Turn Management

    /// <summary>
    /// Processes all environmental effects at the start of a character's turn
    /// </summary>
    public List<string> ProcessStartOfTurn(int combatInstanceId, PlayerCharacter character, int roomId)
    {
        var logMessages = new List<string>();

        _log.Debug("Processing start of turn environmental effects for character {CharacterId} " +
                  "in room {RoomId}",
            character.CharacterId, roomId);

        // 1. Apply ambient conditions
        var ambientResult = _conditionService.ApplyAmbientEffects(character, roomId);
        if (ambientResult.WasTriggered && !string.IsNullOrEmpty(ambientResult.LogMessage))
        {
            logMessages.Add(ambientResult.LogMessage);
            RecordEvent(new EnvironmentalEvent
            {
                CombatInstanceId = combatInstanceId,
                EventType = EnvironmentalEventType.AmbientDamage,
                ActorId = null,
                Targets = new List<int> { character.CharacterId },
                DamageDealt = ambientResult.DamageDealt,
                StatusEffectApplied = ambientResult.StatusEffectApplied,
                Description = ambientResult.LogMessage
            });
        }

        // 2. Apply weather effects
        var weather = _weatherService.GetCurrentWeather(roomId);
        if (weather != null)
        {
            var weatherResult = _weatherService.ApplyWeatherEffects(character, weather);
            if (weatherResult.WasTriggered && !string.IsNullOrEmpty(weatherResult.LogMessage))
            {
                logMessages.Add(weatherResult.LogMessage);
                RecordEvent(new EnvironmentalEvent
                {
                    CombatInstanceId = combatInstanceId,
                    EventType = EnvironmentalEventType.WeatherEffectApplied,
                    ActorId = null,
                    Targets = new List<int> { character.CharacterId },
                    DamageDealt = weatherResult.DamageDealt,
                    StatusEffectApplied = weatherResult.StatusEffectApplied,
                    Description = weatherResult.LogMessage
                });
            }
        }

        // 3. Check for automatic hazards at character's position
        // (This would integrate with grid system to get character position)
        // For parent spec, this is a placeholder

        return logMessages;
    }

    /// <summary>
    /// Processes all environmental effects at the end of a character's turn
    /// </summary>
    public List<string> ProcessEndOfTurn(int combatInstanceId, PlayerCharacter character, int roomId)
    {
        var logMessages = new List<string>();

        // Process any end-of-turn environmental effects
        // (Most environmental effects trigger at start of turn, but some might trigger at end)

        return logMessages;
    }

    /// <summary>
    /// Advances all environmental states by one turn (call at round end)
    /// </summary>
    public void AdvanceTurn()
    {
        _conditionService.AdvanceTurn();
        _weatherService.AdvanceAllWeather();

        _log.Debug("Advanced all environmental states by one turn");
    }

    #endregion

    #region Room Initialization

    /// <summary>
    /// Initializes environmental elements for a room at combat start
    /// </summary>
    public void InitializeRoomEnvironment(int roomId, int combatInstanceId)
    {
        _log.Information("Initializing environmental elements for room {RoomId}, " +
                        "Combat Instance {CombatInstanceId}",
            roomId, combatInstanceId);

        // TODO: Load environmental objects from room data
        // TODO: Load ambient conditions from room data
        // TODO: Load weather effects from sector/room data

        // For parent spec, this is a placeholder
        // Child specs (v0.22.1, v0.22.2, v0.22.3) will implement specific initialization
    }

    /// <summary>
    /// Clears environmental data for a room (combat end cleanup)
    /// </summary>
    public void ClearRoomEnvironment(int roomId, int combatInstanceId)
    {
        _objectService.ClearRoom(roomId);
        _manipulationService.ClearCombatData(combatInstanceId);
        _combatEvents.Clear();

        _log.Information("Cleared environmental data for room {RoomId}, Combat Instance {CombatInstanceId}",
            roomId, combatInstanceId);
    }

    #endregion

    #region Object Interaction

    /// <summary>
    /// Handles interaction with an environmental object
    /// </summary>
    public InteractionResult InteractWithObject(int objectId, int actorId)
    {
        var result = _objectService.InteractWithObject(objectId, actorId);

        if (result.Success)
        {
            RecordEvent(new EnvironmentalEvent
            {
                EventType = EnvironmentalEventType.InteractionTriggered,
                ObjectId = objectId,
                ActorId = actorId,
                Description = result.LogMessage
            });
        }

        return result;
    }

    /// <summary>
    /// Applies damage to an environmental object
    /// </summary>
    public DestructionResult ApplyDamageToObject(int objectId, int damage, string damageType)
    {
        var result = _objectService.DamageObject(objectId, damage);

        if (result.WasDestroyed)
        {
            RecordEvent(new EnvironmentalEvent
            {
                EventType = EnvironmentalEventType.ObjectDestroyed,
                ObjectId = objectId,
                Description = result.LogMessage
            });

            // Process chain reactions
            foreach (var chainReaction in result.ChainReactions)
            {
                RecordEvent(chainReaction);
            }
        }

        return result;
    }

    #endregion

    #region Environmental Manipulation

    /// <summary>
    /// Pushes a character into a hazard
    /// </summary>
    public PushResult PushIntoHazard(int pusherId, int targetId, string startPosition,
        string endPosition, int roomId)
    {
        var result = _manipulationService.PushIntoHazard(pusherId, targetId, startPosition,
            endPosition, roomId);

        if (result.Success && result.HazardsEncountered.Count > 0)
        {
            RecordEvent(new EnvironmentalEvent
            {
                EventType = EnvironmentalEventType.PushIntoHazard,
                ActorId = pusherId,
                Targets = new List<int> { targetId },
                DamageDealt = result.TotalDamage,
                Description = result.LogMessage
            });
        }

        return result;
    }

    /// <summary>
    /// Triggers a controlled collapse
    /// </summary>
    public CollapseResult TriggerCollapse(int objectId, int triggeringAbilityId, string areaEffect)
    {
        var result = _manipulationService.TriggerControlledCollapse(objectId, triggeringAbilityId,
            areaEffect);

        if (result.Success)
        {
            RecordEvent(new EnvironmentalEvent
            {
                EventType = EnvironmentalEventType.CeilingCollapse,
                ObjectId = objectId,
                Targets = result.AffectedCharacters,
                DamageDealt = result.DamageDealt,
                Description = result.LogMessage
            });
        }

        return result;
    }

    /// <summary>
    /// Records an environmental kill
    /// </summary>
    public void RecordEnvironmentalKill(int combatInstanceId, int killerId, int victimId,
        string method, PlayerCharacter killer)
    {
        _manipulationService.RecordEnvironmentalKill(combatInstanceId, killerId, victimId,
            method, killer);

        RecordEvent(new EnvironmentalEvent
        {
            CombatInstanceId = combatInstanceId,
            EventType = EnvironmentalEventType.EnvironmentalKill,
            ActorId = killerId,
            Targets = new List<int> { victimId },
            Kills = 1,
            Description = $"☠️ Environmental kill via {method}"
        });
    }

    #endregion

    #region Integration with Existing Systems

    /// <summary>
    /// Gets cover bonus at position (integrates with v0.20.2 CoverService)
    /// </summary>
    public CoverBonus GetCoverBonus(GridPosition? targetPosition, GridPosition? attackerPosition,
        AttackType attackType, BattlefieldGrid grid)
    {
        if (_coverService != null)
        {
            return _coverService.CalculateCoverBonus(targetPosition, attackerPosition, attackType, grid);
        }

        return CoverBonus.None();
    }

    /// <summary>
    /// Processes legacy hazard (v0.6 compatibility)
    /// </summary>
    public (int damage, int stress, string logMessage) ProcessLegacyHazard(Room room,
        PlayerCharacter character)
    {
        if (_legacyHazardService != null)
        {
            return _legacyHazardService.ProcessAutomaticHazard(room, character);
        }

        return (0, 0, string.Empty);
    }

    #endregion

    #region Event Tracking

    /// <summary>
    /// Records an environmental event for combat log
    /// </summary>
    private void RecordEvent(EnvironmentalEvent evt)
    {
        _combatEvents.Add(evt);
        _log.Debug("Environmental event recorded: {EventType} - {Description}",
            evt.EventType, evt.Description);
    }

    /// <summary>
    /// Gets all environmental events for current combat
    /// </summary>
    public List<EnvironmentalEvent> GetCombatEvents()
    {
        return _combatEvents.ToList();
    }

    /// <summary>
    /// Gets combat log entries for all environmental events
    /// </summary>
    public List<string> GetCombatLog()
    {
        return _combatEvents
            .Select(e => e.GenerateLogEntry())
            .Where(log => !string.IsNullOrEmpty(log))
            .ToList();
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets environmental kill count for current combat
    /// </summary>
    public int GetEnvironmentalKillCount(int combatInstanceId)
    {
        return _manipulationService.GetEnvironmentalKillCount(combatInstanceId);
    }

    /// <summary>
    /// Gets total environmental damage dealt
    /// </summary>
    public int GetTotalEnvironmentalDamage()
    {
        return _combatEvents.Sum(e => e.DamageDealt);
    }

    /// <summary>
    /// Gets objects destroyed count
    /// </summary>
    public int GetObjectsDestroyedCount()
    {
        return _combatEvents.Count(e => e.EventType == EnvironmentalEventType.ObjectDestroyed);
    }

    #endregion
}
