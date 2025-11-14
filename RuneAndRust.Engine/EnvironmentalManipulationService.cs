using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.22.3: Environmental Manipulation Service
/// Handles push/pull mechanics, controlled collapses, and environmental kills.
/// Enables weaponizing the environment against enemies.
///
/// Responsibilities:
/// - Push/pull characters into hazards
/// - Controlled structural collapses
/// - Environmental kill tracking and rewards
/// - Environmental combo detection
/// - Chain reaction management
/// </summary>
public class EnvironmentalManipulationService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalManipulationService>();
    private readonly EnvironmentalObjectService _objectService;
    private readonly TraumaEconomyService _traumaService;
    private readonly DiceService _diceService;

    // Environmental kill tracking
    private readonly Dictionary<int, List<EnvironmentalEvent>> _environmentalKills = new();

    public EnvironmentalManipulationService(EnvironmentalObjectService objectService,
        TraumaEconomyService traumaService, DiceService diceService)
    {
        _objectService = objectService;
        _traumaService = traumaService;
        _diceService = diceService;
    }

    #region Push/Pull Mechanics

    /// <summary>
    /// Pushes a character into a hazard
    /// </summary>
    public PushResult PushIntoHazard(int pusherId, int targetId, string startPosition,
        string endPosition, int roomId)
    {
        var result = new PushResult
        {
            Success = false,
            NewPosition = endPosition
        };

        // Get hazards at destination
        var hazards = GetHazardsInPath(startPosition, endPosition);

        if (hazards.Count == 0)
        {
            result.Success = true;
            result.LogMessage = "Target pushed, but no hazards encountered";
            return result;
        }

        result.Success = true;
        result.HazardsEncountered = hazards;

        var logMessages = new List<string>();
        int totalDamage = 0;

        // Trigger each hazard
        foreach (var hazardId in hazards)
        {
            var hazard = _objectService.GetObject(hazardId);
            if (hazard == null || !hazard.IsHazard)
            {
                continue;
            }

            // Roll damage for hazard
            int damage = ParseAndRollDamage(hazard.DamageFormula ?? "2d6");
            totalDamage += damage;

            logMessages.Add($"⚡ Target pushed into {hazard.Name}! {damage} {hazard.DamageType} damage");

            _log.Information("Character {TargetId} pushed into hazard {HazardId} by {PusherId}, " +
                           "Damage: {Damage}",
                targetId, hazardId, pusherId, damage);
        }

        result.TotalDamage = totalDamage;
        result.LogMessage = string.Join("\n", logMessages);

        // Trigger environmental kill tracking if damage is lethal
        // (actual HP check would be done by caller)

        return result;
    }

    /// <summary>
    /// Gets hazards along a path from start to end position
    /// </summary>
    public List<int> GetHazardsInPath(string startPosition, string endPosition)
    {
        // TODO: Implement proper path calculation based on grid system
        // For now, just check end position
        var hazards = _objectService.GetHazardsAtPosition(endPosition);
        return hazards.Select(h => h.ObjectId).ToList();
    }

    /// <summary>
    /// Pulls a character (reverse of push)
    /// </summary>
    public PushResult PullCharacter(int pullerId, int targetId, string startPosition,
        string endPosition, int roomId)
    {
        // Pull is mechanically identical to push, just different direction
        var result = PushIntoHazard(pullerId, targetId, startPosition, endPosition, roomId);
        result.LogMessage = result.LogMessage.Replace("pushed", "pulled");
        return result;
    }

    #endregion

    #region Controlled Collapses

    /// <summary>
    /// Triggers a controlled collapse (ceiling drop, floor break)
    /// </summary>
    public CollapseResult TriggerControlledCollapse(int objectId, int triggeringAbilityId,
        string areaEffect)
    {
        var obj = _objectService.GetObject(objectId);
        if (obj == null)
        {
            return new CollapseResult
            {
                Success = false,
                LogMessage = "Collapse target not found"
            };
        }

        // Determine affected characters (simplified - would use grid calculation)
        var affectedCharacters = GetAffectedCharacters(objectId, areaEffect);

        // Calculate collapse damage
        int baseDamage = obj.ObjectType == EnvironmentalObjectType.Terrain ? 20 : 10;
        int collapseDamage = _diceService.RollDamage(baseDamage / 6); // Convert to d6s

        // Destroy the object
        _objectService.DestroyObject(objectId, "Controlled collapse triggered");

        var result = new CollapseResult
        {
            Success = true,
            AffectedCharacters = affectedCharacters,
            DamageDealt = collapseDamage,
            TerrainCreated = "Rubble (Difficult Terrain)",
            LogMessage = $"⚠️ STRUCTURAL COLLAPSE! {obj.Name} destroyed! " +
                        $"{collapseDamage} damage to {affectedCharacters.Count} targets. " +
                        $"Rubble created at {obj.GridPosition}."
        };

        _log.Warning("Controlled collapse triggered: Object {ObjectId} - {Name}, " +
                    "Affected: {Affected}, Damage: {Damage}",
            objectId, obj.Name, affectedCharacters.Count, collapseDamage);

        return result;
    }

    /// <summary>
    /// Gets characters affected by area effect centered on object
    /// </summary>
    public List<int> GetAffectedCharacters(int objectId, string areaEffect)
    {
        // TODO: Implement proper area effect calculation based on grid system
        // For now, return empty list (would be populated by caller with grid data)
        return new List<int>();
    }

    #endregion

    #region Environmental Kills

    /// <summary>
    /// Records an environmental kill and applies rewards
    /// </summary>
    public void RecordEnvironmentalKill(int combatInstanceId, int killerId, int victimId,
        string method, PlayerCharacter killer)
    {
        var killEvent = new EnvironmentalEvent
        {
            EventType = EnvironmentalEventType.EnvironmentalKill,
            CombatInstanceId = combatInstanceId,
            ActorId = killerId,
            Targets = new List<int> { victimId },
            Kills = 1,
            Description = $"{method} environmental kill"
        };

        if (!_environmentalKills.ContainsKey(combatInstanceId))
        {
            _environmentalKills[combatInstanceId] = new List<EnvironmentalEvent>();
        }
        _environmentalKills[combatInstanceId].Add(killEvent);

        // Apply rewards (stress relief per v0.15 Trauma Economy)
        int stressRelief = method.Contains("collapse", StringComparison.OrdinalIgnoreCase) ? 10 : 5;
        killer.PsychicStress = Math.Max(0, killer.PsychicStress - stressRelief);

        _log.Information("☠️ ENVIRONMENTAL KILL! Killer: {KillerId}, Victim: {VictimId}, " +
                        "Method: {Method}, Stress Relief: {Relief}",
            killerId, victimId, method, stressRelief);
    }

    /// <summary>
    /// Gets environmental kill count for a combat instance
    /// </summary>
    public int GetEnvironmentalKillCount(int combatInstanceId)
    {
        if (_environmentalKills.TryGetValue(combatInstanceId, out var kills))
        {
            return kills.Sum(k => k.Kills);
        }
        return 0;
    }

    /// <summary>
    /// Gets all environmental kills for a combat instance
    /// </summary>
    public List<EnvironmentalEvent> GetEnvironmentalKills(int combatInstanceId)
    {
        return _environmentalKills.TryGetValue(combatInstanceId, out var kills)
            ? kills
            : new List<EnvironmentalEvent>();
    }

    #endregion

    #region Environmental Combos

    /// <summary>
    /// Checks if an ability triggers an environmental combo
    /// </summary>
    public ComboResult CheckEnvironmentalCombo(int abilityId, List<int> affectedObjectIds)
    {
        // TODO: Implement combo detection based on ability + environmental element
        // Examples:
        // - Fire ability + explosive barrel = "Inferno Chain"
        // - Lightning ability + water hazard = "Conductivity Surge"
        // - Push ability + cliff/chasm = "Gravity Assist"

        var result = new ComboResult
        {
            ComboDetected = false
        };

        // Check each affected object for combo potential
        foreach (var objectId in affectedObjectIds)
        {
            var obj = _objectService.GetObject(objectId);
            if (obj == null)
            {
                continue;
            }

            // Simple combo detection (would be expanded with ability data)
            if (obj.IsHazard && obj.Name.Contains("Barrel", StringComparison.OrdinalIgnoreCase))
            {
                result.ComboDetected = true;
                result.ComboName = "Explosive Chain";
                result.BonusDamage = 10;
                result.LogMessage = "🔥 COMBO! Explosive Chain reaction! +10 bonus damage";
                break;
            }
        }

        if (result.ComboDetected)
        {
            _log.Information("Environmental combo detected: {ComboName}, Bonus Damage: {Bonus}",
                result.ComboName, result.BonusDamage);
        }

        return result;
    }

    #endregion

    #region Utility

    /// <summary>
    /// Parses damage formula and rolls dice (e.g., "2d6" -> rolls 2d6)
    /// </summary>
    private int ParseAndRollDamage(string? formula)
    {
        if (string.IsNullOrEmpty(formula))
        {
            return 0;
        }

        // Simple parser for "XdY" format
        var parts = formula.Split('d');
        if (parts.Length == 2 && int.TryParse(parts[0], out int numDice))
        {
            return _diceService.RollDamage(numDice);
        }
        return 0;
    }

    /// <summary>
    /// Clears environmental kill data for a combat instance
    /// </summary>
    public void ClearCombatData(int combatInstanceId)
    {
        _environmentalKills.Remove(combatInstanceId);
        _log.Information("Cleared environmental kill data for combat instance {CombatInstanceId}",
            combatInstanceId);
    }

    #endregion
}
