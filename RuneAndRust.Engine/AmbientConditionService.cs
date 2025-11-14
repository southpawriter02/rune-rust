using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.22.2: Ambient Condition Service
/// Handles room-wide persistent environmental effects.
/// Extends v0.11 AmbientCondition system with v0.22 capabilities.
///
/// Responsibilities:
/// - Managing ambient conditions in rooms
/// - Applying per-turn effects (damage, stress, modifiers)
/// - Resolve checks for resisting effects
/// - Condition suppression mechanics
/// - Integration with weather effects and hazards
/// </summary>
public class AmbientConditionService
{
    private static readonly ILogger _log = Log.ForContext<AmbientConditionService>();
    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaService;
    private readonly ResolveCheckService? _resolveCheckService;

    // In-memory storage for active ambient conditions
    private readonly Dictionary<int, AmbientConditionExtended> _activeConditions = new();
    private int _nextConditionId = 1;

    public AmbientConditionService(DiceService diceService, TraumaEconomyService traumaService,
        ResolveCheckService? resolveCheckService = null)
    {
        _diceService = diceService;
        _traumaService = traumaService;
        _resolveCheckService = resolveCheckService;
    }

    #region Condition Management

    /// <summary>
    /// Applies an ambient condition to a room
    /// </summary>
    public int ApplyCondition(int roomId, AmbientConditionType conditionType, string name,
        string description, int? durationTurns = null)
    {
        var condition = new AmbientConditionExtended
        {
            BaseCondition = new AmbientCondition
            {
                ConditionId = _nextConditionId.ToString(),
                Name = name,
                Description = description,
                Type = conditionType
            },
            DurationTurns = durationTurns,
            TurnsRemaining = durationTurns ?? 0
        };

        // Apply default effects based on condition type
        ApplyDefaultEffects(condition, conditionType);

        int conditionId = _nextConditionId++;
        _activeConditions[conditionId] = condition;

        _log.Information("Ambient condition applied: {ConditionId} - {Name} to Room {RoomId}, Duration: {Duration}",
            conditionId, name, roomId, durationTurns?.ToString() ?? "Permanent");

        return conditionId;
    }

    /// <summary>
    /// Applies default effects based on condition type (v0.11 baseline)
    /// </summary>
    private void ApplyDefaultEffects(AmbientConditionExtended condition, AmbientConditionType type)
    {
        switch (type)
        {
            case AmbientConditionType.PsychicResonance:
                condition.BaseCondition.PsychicStressPerTurn = 2;
                condition.BaseCondition.WillResolveModifier = -1;
                condition.ResolveCheckDC = 12;
                condition.ResolveCheckStat = "WILL";
                condition.Intensity = "Moderate";
                break;

            case AmbientConditionType.ToxicAir:
                condition.BaseCondition.CanApplyStatusEffect = "Poisoned";
                condition.BaseCondition.StatusEffectChance = 0.2f;
                condition.ResolveCheckDC = 14;
                condition.ResolveCheckStat = "STURDINESS";
                condition.StatusOnFumble = "[Poisoned]";
                condition.IsSuppressible = true;
                condition.SuppressionAbility = "Purify Air";
                break;

            case AmbientConditionType.RunicInstability:
                condition.BaseCondition.CausesWildMagic = true;
                condition.BaseCondition.WildMagicChance = 0.2f;
                condition.Intensity = "High";
                break;

            case AmbientConditionType.Flooded:
                condition.BaseCondition.MovementCostModifier = 1;
                condition.BaseCondition.EnhancesHazardTypes.Add("LivePowerConduit");
                condition.BaseCondition.HazardEnhancementMultiplier = 2.0f;
                break;

            case AmbientConditionType.CorrodedAtmosphere:
                condition.BaseCondition.CausesEquipmentDegradation = true;
                condition.BaseCondition.DegradationAmount = 1;
                condition.BaseCondition.CanApplyStatusEffect = "Corroded";
                condition.BaseCondition.StatusEffectChance = 0.15f;
                break;

            case AmbientConditionType.DimLighting:
                condition.BaseCondition.AccuracyModifier = -1;
                condition.Intensity = "Low";
                break;

            case AmbientConditionType.ExtremeHeat:
                condition.BaseCondition.EnhancesHazardTypes.Add("Fire");
                condition.BaseCondition.HazardEnhancementMultiplier = 1.5f;
                break;

            case AmbientConditionType.HighRadiation:
                condition.CorruptionPerTurn = 2;
                condition.ResolveCheckDC = 16;
                condition.ResolveCheckStat = "STURDINESS";
                condition.Intensity = "Extreme";
                break;
        }
    }

    /// <summary>
    /// Gets all active conditions for a room
    /// </summary>
    public List<AmbientConditionExtended> GetActiveConditions(int roomId)
    {
        // TODO: Add room tracking to conditions
        // For now, return all non-suppressed conditions
        return _activeConditions.Values
            .Where(c => !c.IsCurrentlySuppressed)
            .ToList();
    }

    /// <summary>
    /// Suppresses an ambient condition temporarily
    /// </summary>
    public bool SuppressCondition(int conditionId, int duration)
    {
        if (!_activeConditions.TryGetValue(conditionId, out var condition))
        {
            return false;
        }

        if (!condition.IsSuppressible)
        {
            _log.Warning("Attempted to suppress non-suppressible condition: {ConditionId}", conditionId);
            return false;
        }

        condition.IsCurrentlySuppressed = true;
        condition.SuppressionDuration = duration;

        _log.Information("Ambient condition suppressed: {ConditionId} - {Name} for {Duration} turns",
            conditionId, condition.BaseCondition.Name, duration);

        return true;
    }

    /// <summary>
    /// Removes an ambient condition
    /// </summary>
    public bool RemoveCondition(int conditionId)
    {
        if (_activeConditions.Remove(conditionId))
        {
            _log.Information("Ambient condition removed: {ConditionId}", conditionId);
            return true;
        }
        return false;
    }

    #endregion

    #region Effect Application

    /// <summary>
    /// Applies ambient effects to a character at turn start
    /// </summary>
    public HazardResult ApplyAmbientEffects(PlayerCharacter character, int roomId)
    {
        var conditions = GetActiveConditions(roomId);
        if (conditions.Count == 0)
        {
            return new HazardResult { WasTriggered = false };
        }

        var result = new HazardResult
        {
            WasTriggered = true,
            AffectedCharacters = new List<int> { character.CharacterId }
        };

        var logMessages = new List<string>();
        int totalDamage = 0;
        int totalStress = 0;

        foreach (var condition in conditions)
        {
            var baseCondition = condition.BaseCondition;

            // Apply psychic stress
            if (baseCondition.PsychicStressPerTurn > 0)
            {
                totalStress += baseCondition.PsychicStressPerTurn;
                _traumaService.AddStress(character, baseCondition.PsychicStressPerTurn);
                logMessages.Add($"💀 {baseCondition.Name}: +{baseCondition.PsychicStressPerTurn} Psychic Stress");
            }

            // Apply corruption
            if (condition.CorruptionPerTurn > 0)
            {
                // TODO: Implement corruption tracking
                logMessages.Add($"☠️ {baseCondition.Name}: +{condition.CorruptionPerTurn} Corruption");
            }

            // Resolve check for status effects
            if (condition.ResolveCheckDC.HasValue && _resolveCheckService != null)
            {
                // TODO: Implement resolve check and apply status on fumble
                logMessages.Add($"⚠️ {baseCondition.Name}: Resolve check required (DC {condition.ResolveCheckDC})");
            }

            // Apply status effects (chance-based)
            if (!string.IsNullOrEmpty(baseCondition.CanApplyStatusEffect) &&
                baseCondition.StatusEffectChance > 0)
            {
                var random = new Random();
                if (random.NextDouble() < baseCondition.StatusEffectChance)
                {
                    result.StatusEffectApplied = baseCondition.CanApplyStatusEffect;
                    logMessages.Add($"🧪 {baseCondition.Name}: Applied {baseCondition.CanApplyStatusEffect}");
                }
            }
        }

        result.DamageDealt = totalDamage;
        result.LogMessage = string.Join("\n", logMessages);

        if (logMessages.Count > 0)
        {
            _log.Information("Ambient effects applied to character {CharacterId}: {Effects}",
                character.CharacterId, string.Join(", ", logMessages));
        }

        return result;
    }

    /// <summary>
    /// Gets combined modifiers from all active conditions
    /// </summary>
    public (int accuracy, int movement, int willResolve) GetCombinedModifiers(int roomId)
    {
        var conditions = GetActiveConditions(roomId);

        int accuracyMod = conditions.Sum(c => c.BaseCondition.AccuracyModifier);
        int movementMod = conditions.Sum(c => c.BaseCondition.MovementCostModifier);
        int willMod = conditions.Sum(c => c.BaseCondition.WillResolveModifier);

        return (accuracyMod, movementMod, willMod);
    }

    #endregion

    #region Turn Management

    /// <summary>
    /// Advances all condition states by one turn
    /// </summary>
    public void AdvanceTurn()
    {
        var expiredConditions = new List<int>();

        foreach (var kvp in _activeConditions)
        {
            var condition = kvp.Value;

            // Advance suppression
            if (condition.IsCurrentlySuppressed && condition.SuppressionDuration.HasValue)
            {
                condition.SuppressionDuration--;
                if (condition.SuppressionDuration <= 0)
                {
                    condition.IsCurrentlySuppressed = false;
                    _log.Information("Condition suppression ended: {ConditionId}", kvp.Key);
                }
            }

            // Advance duration
            if (!condition.AdvanceTurn())
            {
                expiredConditions.Add(kvp.Key);
            }
        }

        // Remove expired conditions
        foreach (var conditionId in expiredConditions)
        {
            RemoveCondition(conditionId);
        }
    }

    #endregion
}
