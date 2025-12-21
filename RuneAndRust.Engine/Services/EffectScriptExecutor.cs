using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Shared utility for parsing and executing effect scripts (v0.3.3a).
/// Effect scripts are semicolon-delimited commands: "DAMAGE:Fire:2d6;STATUS:Burning:2".
/// Used by both AbilityService and HazardService.
/// </summary>
public partial class EffectScriptExecutor
{
    private readonly IDiceService _diceService;
    private readonly IStatusEffectService _statusEffectService;
    private readonly ILogger<EffectScriptExecutor> _logger;

    /// <summary>
    /// Regex pattern for parsing dice notation (e.g., "2d6", "1d8").
    /// </summary>
    [GeneratedRegex(@"^(\d+)d(\d+)$", RegexOptions.Compiled)]
    private static partial Regex DiceNotationRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectScriptExecutor"/> class.
    /// </summary>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="statusEffectService">Service for applying status effects.</param>
    /// <param name="logger">Logger for traceability.</param>
    public EffectScriptExecutor(
        IDiceService diceService,
        IStatusEffectService statusEffectService,
        ILogger<EffectScriptExecutor> logger)
    {
        _diceService = diceService;
        _statusEffectService = statusEffectService;
        _logger = logger;

        _logger.LogTrace("EffectScriptExecutor initialized");
    }

    /// <summary>
    /// Executes an effect script against a target combatant.
    /// </summary>
    /// <param name="effectScript">The effect script to execute (e.g., "DAMAGE:Fire:2d6;STATUS:Burning:2").</param>
    /// <param name="target">The combatant receiving the effects.</param>
    /// <param name="sourceName">The name of the effect source for narrative purposes.</param>
    /// <param name="sourceId">The ID of the source for status effect tracking.</param>
    /// <returns>A result containing damage, healing, statuses applied, and narrative messages.</returns>
    public EffectScriptResult Execute(
        string effectScript,
        Combatant target,
        string sourceName,
        Guid? sourceId = null)
    {
        if (string.IsNullOrWhiteSpace(effectScript))
        {
            _logger.LogDebug("[EffectScript] Empty script provided for {Source}", sourceName);
            return EffectScriptResult.Empty;
        }

        _logger.LogDebug(
            "[EffectScript] Executing script: {Script} on {Target} from {Source}",
            effectScript, target.Name, sourceName);

        var commands = effectScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var totalDamage = 0;
        var totalHealing = 0;
        var statusesApplied = new List<string>();
        var narratives = new List<string>();
        var effectiveSourceId = sourceId ?? Guid.Empty;

        foreach (var command in commands)
        {
            var parts = command.Trim().Split(':');
            if (parts.Length == 0) continue;

            var commandType = parts[0].ToUpperInvariant();
            _logger.LogDebug("[EffectScript] Executing command: {Command}", command);

            switch (commandType)
            {
                case "DAMAGE":
                    var damageResult = ExecuteDamageCommand(parts, target, sourceName);
                    totalDamage += damageResult.Damage;
                    if (!string.IsNullOrEmpty(damageResult.Narrative))
                    {
                        narratives.Add(damageResult.Narrative);
                    }
                    break;

                case "HEAL":
                    var healResult = ExecuteHealCommand(parts, target);
                    totalHealing += healResult.Healing;
                    if (!string.IsNullOrEmpty(healResult.Narrative))
                    {
                        narratives.Add(healResult.Narrative);
                    }
                    break;

                case "STATUS":
                    var statusResult = ExecuteStatusCommand(parts, target, effectiveSourceId);
                    if (statusResult.Applied)
                    {
                        statusesApplied.Add(statusResult.StatusName);
                    }
                    if (!string.IsNullOrEmpty(statusResult.Narrative))
                    {
                        narratives.Add(statusResult.Narrative);
                    }
                    break;

                default:
                    _logger.LogWarning("[EffectScript] Unknown command type: {CommandType}", commandType);
                    break;
            }
        }

        var combinedNarrative = string.Join(" ", narratives);

        _logger.LogInformation(
            "[EffectScript] Execution complete. Damage: {Damage}, Healing: {Healing}, Statuses: [{Statuses}]",
            totalDamage, totalHealing, string.Join(", ", statusesApplied));

        return new EffectScriptResult(totalDamage, totalHealing, statusesApplied, combinedNarrative);
    }

    #region Command Handlers

    /// <summary>
    /// Executes a DAMAGE command: DAMAGE:Type:Dice (e.g., "DAMAGE:Fire:2d6")
    /// </summary>
    private (int Damage, string Narrative) ExecuteDamageCommand(
        string[] parts,
        Combatant target,
        string sourceName)
    {
        if (parts.Length < 3)
        {
            _logger.LogWarning(
                "[EffectScript] DAMAGE command missing parameters: {Parts}",
                string.Join(":", parts));
            return (0, string.Empty);
        }

        var damageTypeName = parts[1];
        var diceNotation = parts[2];

        // Parse dice notation (e.g., "2d6" -> count=2, sides=6)
        var match = DiceNotationRegex().Match(diceNotation);
        if (!match.Success)
        {
            _logger.LogWarning("[EffectScript] Invalid dice notation: {Notation}", diceNotation);
            return (0, string.Empty);
        }

        var diceCount = int.Parse(match.Groups[1].Value);
        var diceSides = int.Parse(match.Groups[2].Value);

        // Roll damage
        var totalRoll = 0;
        for (var i = 0; i < diceCount; i++)
        {
            totalRoll += _diceService.RollSingle(diceSides, $"Effect damage ({diceNotation})");
        }

        // Apply vulnerability multiplier
        var multiplier = _statusEffectService.GetDamageMultiplier(target);
        var finalDamage = (int)(totalRoll * multiplier);

        // Apply armor soak (Physical damage is soaked, elemental bypasses)
        if (damageTypeName.Equals("Physical", StringComparison.OrdinalIgnoreCase))
        {
            var soak = target.ArmorSoak + _statusEffectService.GetSoakModifier(target);
            finalDamage = Math.Max(0, finalDamage - soak);
        }

        // Apply damage to target
        target.CurrentHp -= finalDamage;

        _logger.LogInformation(
            "[EffectScript] DAMAGE: {Source} deals {Damage} {Type} damage to {Target} (rolled {Roll})",
            sourceName, finalDamage, damageTypeName, target.Name, totalRoll);

        var narrative = $"Deals {finalDamage} {damageTypeName.ToLower()} damage.";
        return (finalDamage, narrative);
    }

    /// <summary>
    /// Executes a HEAL command: HEAL:Amount or HEAL:Dice (e.g., "HEAL:15" or "HEAL:2d6")
    /// </summary>
    private (int Healing, string Narrative) ExecuteHealCommand(string[] parts, Combatant target)
    {
        if (parts.Length < 2)
        {
            _logger.LogWarning("[EffectScript] HEAL command missing amount parameter");
            return (0, string.Empty);
        }

        var amountStr = parts[1];
        int amount;

        // Check if it's dice notation or a flat value
        var match = DiceNotationRegex().Match(amountStr);
        if (match.Success)
        {
            var diceCount = int.Parse(match.Groups[1].Value);
            var diceSides = int.Parse(match.Groups[2].Value);
            amount = 0;
            for (var i = 0; i < diceCount; i++)
            {
                amount += _diceService.RollSingle(diceSides, $"Effect healing ({amountStr})");
            }
        }
        else if (!int.TryParse(amountStr, out amount))
        {
            _logger.LogWarning("[EffectScript] HEAL command has invalid amount: {Amount}", amountStr);
            return (0, string.Empty);
        }

        // Apply healing, clamped to max HP
        var actualHealing = Math.Min(amount, target.MaxHp - target.CurrentHp);
        target.CurrentHp += actualHealing;

        _logger.LogInformation(
            "[EffectScript] HEAL: {Target} healed for {Amount} HP (actual: {Actual})",
            target.Name, amount, actualHealing);

        var narrative = actualHealing > 0
            ? $"Restores {actualHealing} HP."
            : "HP is already full.";
        return (actualHealing, narrative);
    }

    /// <summary>
    /// Executes a STATUS command: STATUS:Type:Duration:Stacks (e.g., "STATUS:Bleeding:3:2")
    /// </summary>
    private (bool Applied, string StatusName, string Narrative) ExecuteStatusCommand(
        string[] parts,
        Combatant target,
        Guid sourceId)
    {
        if (parts.Length < 3)
        {
            _logger.LogWarning(
                "[EffectScript] STATUS command missing parameters: {Parts}",
                string.Join(":", parts));
            return (false, string.Empty, string.Empty);
        }

        var statusTypeName = parts[1];
        if (!Enum.TryParse<StatusEffectType>(statusTypeName, true, out var statusType))
        {
            _logger.LogWarning("[EffectScript] Unknown status effect type: {Type}", statusTypeName);
            return (false, statusTypeName, string.Empty);
        }

        if (!int.TryParse(parts[2], out var duration))
        {
            _logger.LogWarning("[EffectScript] STATUS command has invalid duration: {Duration}", parts[2]);
            return (false, statusTypeName, string.Empty);
        }

        // Optional stacks parameter (default to 1)
        var stacks = 1;
        if (parts.Length >= 4 && int.TryParse(parts[3], out var parsedStacks))
        {
            stacks = parsedStacks;
        }

        // Apply the effect (stacks times if stackable)
        for (var i = 0; i < stacks; i++)
        {
            _statusEffectService.ApplyEffect(target, statusType, duration, sourceId);
        }

        _logger.LogInformation(
            "[EffectScript] STATUS: Applied {Type} to {Target} (Duration: {Duration}, Stacks: {Stacks})",
            statusType, target.Name, duration, stacks);

        var narrative = $"Applies {statusTypeName}.";
        return (true, statusTypeName, narrative);
    }

    #endregion
}

/// <summary>
/// Result of executing an effect script.
/// </summary>
/// <param name="TotalDamage">Total damage dealt across all DAMAGE commands.</param>
/// <param name="TotalHealing">Total healing applied across all HEAL commands.</param>
/// <param name="StatusesApplied">List of status effect names that were applied.</param>
/// <param name="Narrative">Combined narrative message from all commands.</param>
public record EffectScriptResult(
    int TotalDamage,
    int TotalHealing,
    List<string> StatusesApplied,
    string Narrative)
{
    /// <summary>
    /// Empty result for when no effects are applied.
    /// </summary>
    public static EffectScriptResult Empty => new(0, 0, new List<string>(), string.Empty);
}
