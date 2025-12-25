using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for processing ambient conditions in rooms (v0.3.3b).
/// Handles passive stat modifiers and turn-based tick effects (damage, stress, corruption).
/// </summary>
/// <remarks>See: SPEC-COND-001 for Ambient Condition System design.</remarks>
public partial class ConditionService : IConditionService
{
    private readonly IRepository<AmbientCondition> _conditionRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly EffectScriptExecutor _effectScriptExecutor;
    private readonly IDiceService _diceService;
    private readonly ILogger<ConditionService> _logger;

    /// <summary>
    /// Regex pattern for parsing flat numeric values in tick scripts.
    /// </summary>
    [GeneratedRegex(@"^(\d+)$", RegexOptions.Compiled)]
    private static partial Regex FlatValueRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionService"/> class.
    /// </summary>
    /// <param name="conditionRepository">Repository for ambient conditions.</param>
    /// <param name="roomRepository">Repository for rooms.</param>
    /// <param name="effectScriptExecutor">Executor for DAMAGE commands.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="logger">Logger for traceability.</param>
    public ConditionService(
        IRepository<AmbientCondition> conditionRepository,
        IRoomRepository roomRepository,
        EffectScriptExecutor effectScriptExecutor,
        IDiceService diceService,
        ILogger<ConditionService> logger)
    {
        _conditionRepository = conditionRepository;
        _roomRepository = roomRepository;
        _effectScriptExecutor = effectScriptExecutor;
        _diceService = diceService;
        _logger = logger;

        _logger.LogTrace("[Condition] ConditionService initialized");
    }

    /// <inheritdoc/>
    public async Task<AmbientCondition?> GetRoomConditionAsync(Guid roomId)
    {
        _logger.LogTrace("[Condition] Getting condition for room {RoomId}", roomId);

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room?.ConditionId == null)
        {
            _logger.LogTrace("[Condition] Room {RoomId} has no active condition", roomId);
            return null;
        }

        var condition = await _conditionRepository.GetByIdAsync(room.ConditionId.Value);
        if (condition != null)
        {
            _logger.LogDebug(
                "[Condition] Room {RoomId} has condition [{ConditionName}] ({ConditionType})",
                roomId, condition.Name, condition.Type);
        }

        return condition;
    }

    /// <inheritdoc/>
    public Dictionary<CharacterAttribute, int> GetStatModifiers(ConditionType type)
    {
        _logger.LogTrace("[Condition] Getting stat modifiers for {ConditionType}", type);

        var penalties = type.GetPassivePenalties();

        _logger.LogDebug(
            "[Condition] {ConditionType} has {Count} passive penalties",
            type, penalties.Count);

        return penalties;
    }

    /// <inheritdoc/>
    public void ApplyPassiveModifiers(Combatant combatant, ConditionType? conditionType)
    {
        if (conditionType == null)
        {
            _logger.LogTrace("[Condition] No condition to apply to {Combatant}", combatant.Name);
            return;
        }

        var penalties = GetStatModifiers(conditionType.Value);

        foreach (var (attribute, penalty) in penalties)
        {
            switch (attribute)
            {
                case CharacterAttribute.Sturdiness:
                    combatant.ConditionSturdinessModifier = penalty;
                    _logger.LogDebug(
                        "[Condition] {Combatant} affected by STURDINESS {Penalty} from {Condition}",
                        combatant.Name, penalty, conditionType);
                    break;

                case CharacterAttribute.Finesse:
                    combatant.ConditionFinesseModifier = penalty;
                    _logger.LogDebug(
                        "[Condition] {Combatant} affected by FINESSE {Penalty} from {Condition}",
                        combatant.Name, penalty, conditionType);
                    break;

                case CharacterAttribute.Wits:
                    combatant.ConditionWitsModifier = penalty;
                    _logger.LogDebug(
                        "[Condition] {Combatant} affected by WITS {Penalty} from {Condition}",
                        combatant.Name, penalty, conditionType);
                    break;

                case CharacterAttribute.Will:
                    combatant.ConditionWillModifier = penalty;
                    _logger.LogDebug(
                        "[Condition] {Combatant} affected by WILL {Penalty} from {Condition}",
                        combatant.Name, penalty, conditionType);
                    break;

                case CharacterAttribute.Might:
                    // Might is not typically affected by conditions, but log if it happens
                    _logger.LogWarning(
                        "[Condition] MIGHT modifier from {Condition} not implemented",
                        conditionType);
                    break;
            }
        }

        combatant.ActiveCondition = conditionType;

        _logger.LogInformation(
            "[Condition] {Combatant} entered [{Condition}] zone",
            combatant.Name, conditionType);
    }

    /// <inheritdoc/>
    public async Task<ConditionTickResult> ProcessTurnTickAsync(Combatant combatant, AmbientCondition condition)
    {
        _logger.LogTrace(
            "[Condition] Processing tick for {Combatant} in [{ConditionName}]",
            combatant.Name, condition.Name);

        if (string.IsNullOrEmpty(condition.TickScript))
        {
            _logger.LogTrace("[Condition] {Condition} has no tick effect", condition.Name);
            return ConditionTickResult.None;
        }

        // Check tick chance (e.g., StaticField has 25% chance)
        if (condition.TickChance < 1.0f)
        {
            var roll = _diceService.RollSingle(100, "Condition tick chance");
            var threshold = (int)(condition.TickChance * 100);

            if (roll > threshold)
            {
                _logger.LogTrace(
                    "[Condition] {Condition} tick skipped (rolled {Roll} vs threshold {Threshold})",
                    condition.Name, roll, threshold);
                return ConditionTickResult.None;
            }

            _logger.LogDebug(
                "[Condition] {Condition} tick triggered (rolled {Roll} vs threshold {Threshold})",
                condition.Name, roll, threshold);
        }

        var totalDamage = 0;
        var stressApplied = 0;
        var corruptionApplied = 0;
        var narratives = new List<string>();

        // Parse and execute tick script commands
        var commands = condition.TickScript.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var command in commands)
        {
            var parts = command.Trim().Split(':');
            if (parts.Length == 0) continue;

            var commandType = parts[0].ToUpperInvariant();

            switch (commandType)
            {
                case "DAMAGE":
                    // Use EffectScriptExecutor for damage commands
                    var damageResult = _effectScriptExecutor.Execute(command, combatant, condition.Name);
                    totalDamage += damageResult.TotalDamage;
                    if (!string.IsNullOrEmpty(damageResult.Narrative))
                    {
                        narratives.Add(damageResult.Narrative);
                    }
                    break;

                case "STRESS":
                    stressApplied = ParseStressCommand(parts, combatant);
                    if (stressApplied > 0)
                    {
                        narratives.Add($"+{stressApplied} Stress.");
                    }
                    break;

                case "CORRUPTION":
                    corruptionApplied = ParseCorruptionCommand(parts, combatant);
                    if (corruptionApplied > 0)
                    {
                        narratives.Add($"+{corruptionApplied} Corruption.");
                    }
                    break;

                default:
                    _logger.LogWarning(
                        "[Condition] Unknown tick command type: {CommandType}",
                        commandType);
                    break;
            }
        }

        var message = BuildTickMessage(condition, narratives);

        _logger.LogInformation(
            "[Condition] {Condition} affects {Combatant}: Damage={Damage}, Stress={Stress}, Corruption={Corruption}",
            condition.Name, combatant.Name, totalDamage, stressApplied, corruptionApplied);

        return new ConditionTickResult(
            WasApplied: true,
            ConditionName: condition.Name,
            Message: message,
            DamageDealt: totalDamage,
            StressApplied: stressApplied,
            CorruptionApplied: corruptionApplied
        );
    }

    #region Private Helpers

    /// <summary>
    /// Parses and applies a STRESS command: STRESS:amount (e.g., "STRESS:3")
    /// </summary>
    private int ParseStressCommand(string[] parts, Combatant combatant)
    {
        if (parts.Length < 2)
        {
            _logger.LogWarning("[Condition] STRESS command missing amount parameter");
            return 0;
        }

        if (!int.TryParse(parts[1], out var stress))
        {
            _logger.LogWarning("[Condition] STRESS command has invalid amount: {Amount}", parts[1]);
            return 0;
        }

        var previousStress = combatant.CurrentStress;
        combatant.CurrentStress = Math.Min(combatant.MaxStress, combatant.CurrentStress + stress);
        var actualStress = combatant.CurrentStress - previousStress;

        _logger.LogDebug(
            "[Condition] STRESS: {Combatant} gained {Stress} stress ({Previous} -> {Current})",
            combatant.Name, actualStress, previousStress, combatant.CurrentStress);

        return actualStress;
    }

    /// <summary>
    /// Parses and applies a CORRUPTION command: CORRUPTION:amount (e.g., "CORRUPTION:1")
    /// </summary>
    private int ParseCorruptionCommand(string[] parts, Combatant combatant)
    {
        if (parts.Length < 2)
        {
            _logger.LogWarning("[Condition] CORRUPTION command missing amount parameter");
            return 0;
        }

        if (!int.TryParse(parts[1], out var corruption))
        {
            _logger.LogWarning("[Condition] CORRUPTION command has invalid amount: {Amount}", parts[1]);
            return 0;
        }

        var previousCorruption = combatant.CurrentCorruption;
        combatant.CurrentCorruption = Math.Min(combatant.MaxCorruption, combatant.CurrentCorruption + corruption);
        var actualCorruption = combatant.CurrentCorruption - previousCorruption;

        _logger.LogDebug(
            "[Condition] CORRUPTION: {Combatant} gained {Corruption} corruption ({Previous} -> {Current})",
            combatant.Name, actualCorruption, previousCorruption, combatant.CurrentCorruption);

        return actualCorruption;
    }

    /// <summary>
    /// Builds a display message for a condition tick.
    /// </summary>
    private static string BuildTickMessage(AmbientCondition condition, List<string> narratives)
    {
        if (narratives.Count == 0)
        {
            return $"{condition.Name} pulses.";
        }

        return $"{condition.Name}: {string.Join(" ", narratives)}";
    }

    #endregion
}
