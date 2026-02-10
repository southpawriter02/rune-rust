// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderTier1AbilityService.cs
// Implements Tier 1 ability operations for the Jötun-Reader specialization:
// Deep Scan, Pattern Recognition, and Ancient Tongues.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 1 Jötun-Reader abilities: Deep Scan, Pattern Recognition,
/// and Ancient Tongues.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all Tier 1 ability operations for the Jötun-Reader
/// specialization. It operates on immutable value objects and returns new
/// instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 1 abilities are free (0 PP cost) and unlocked immediately.</b>
/// </para>
/// <para>
/// Logging follows the established specialization service pattern:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Information:</b> Successful ability activations, script
///     comprehension, examination completions
///   </description></item>
///   <item><description>
///     <b>Warning:</b> Rejected operations (invalid target, unreadable script)
///   </description></item>
///   <item><description>
///     <b>Debug:</b> Target validation, prerequisite checks, roll details
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="IJotunReaderAbilityService"/>
/// <seealso cref="LoreInsightResource"/>
/// <seealso cref="DeepScanResult"/>
public class JotunReaderTier1AbilityService : IJotunReaderAbilityService
{
    private readonly ILogger<JotunReaderTier1AbilityService> _logger;

    // Random instance for Deep Scan bonus dice (2d10)
    private readonly Random _random;

    /// <summary>
    /// Deep Scan bonus dice description.
    /// </summary>
    public const string DeepScanBonusDice = "2d10";

    /// <summary>
    /// Number of bonus dice for Deep Scan.
    /// </summary>
    public const int DeepScanDiceCount = 2;

    /// <summary>
    /// Sides per bonus die for Deep Scan.
    /// </summary>
    public const int DeepScanDiceSides = 10;

    /// <summary>
    /// AP cost to activate Deep Scan.
    /// </summary>
    public const int DeepScanApCost = 2;

    /// <summary>
    /// PP threshold required to unlock Tier 2 Jötun-Reader abilities.
    /// </summary>
    public const int Tier2Threshold = 8;

    /// <summary>
    /// Valid target types for Deep Scan.
    /// </summary>
    public static readonly IReadOnlyList<string> ValidDeepScanTargets = new[]
    {
        "machinery",
        "terminal",
        "jotun-tech",
        "jotun-machinery",
        "jotun-terminal",
        "jotun-technology"
    };

    /// <summary>
    /// Target types that qualify as Jötun technology for Pattern Recognition.
    /// </summary>
    public static readonly IReadOnlyList<string> JotunTechnologyTypes = new[]
    {
        "jotun-tech",
        "jotun-machinery",
        "jotun-terminal",
        "jotun-technology",
        "jotun-artifact",
        "machinery",
        "terminal"
    };

    /// <summary>
    /// Script types unlocked by Ancient Tongues.
    /// </summary>
    public static readonly IReadOnlyList<ScriptType> UnlockedScriptTypes = new[]
    {
        ScriptType.JotunFormal,
        ScriptType.JotunTechnical,
        ScriptType.DvergrStandard,
        ScriptType.DvergrRunic
    };

    /// <summary>
    /// Initializes a new instance of <see cref="JotunReaderTier1AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public JotunReaderTier1AbilityService(
        ILogger<JotunReaderTier1AbilityService> logger,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;
    }

    // ═══════ Deep Scan (Active) ═══════

    /// <inheritdoc />
    public DeepScanResult? ExecuteDeepScan(
        Guid targetId,
        string targetType,
        int baseRoll,
        int perceptionModifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);

        if (!IsValidDeepScanTarget(targetType))
        {
            _logger.LogWarning(
                "Deep Scan REJECTED: invalid target type '{TargetType}'. " +
                "Valid targets: {ValidTargets}",
                targetType,
                string.Join(", ", ValidDeepScanTargets));

            return null;
        }

        // Roll 2d10 bonus
        var bonusRoll = 0;
        for (var i = 0; i < DeepScanDiceCount; i++)
        {
            bonusRoll += _random.Next(1, DeepScanDiceSides + 1);
        }

        var totalModifiers = perceptionModifier + bonusRoll;
        var totalResult = baseRoll + totalModifiers;

        // Determine success level based on total result
        var successLevel = totalResult switch
        {
            < 10 => ExaminationSuccessLevel.Failure,
            < 15 => ExaminationSuccessLevel.Partial,
            < 20 => ExaminationSuccessLevel.Success,
            < 25 => ExaminationSuccessLevel.Expert,
            _ => ExaminationSuccessLevel.Master
        };

        // Generate insight: +1 on success, +2 on Master
        var insightGenerated = successLevel switch
        {
            ExaminationSuccessLevel.Failure => 0,
            ExaminationSuccessLevel.Master => 2,
            _ => 1
        };

        var layerInfo = GenerateLayerInfo(successLevel, targetType);

        var result = DeepScanResult.CreateSuccess(
            targetId: targetId,
            targetType: targetType,
            baseRoll: baseRoll,
            modifiers: totalModifiers,
            successLevel: successLevel,
            information: layerInfo,
            insightGenerated: insightGenerated,
            isFirstExamination: true);

        _logger.LogInformation(
            "Deep Scan executed on {TargetType} ({TargetId}): " +
            "d20={BaseRoll} + {PerceptionMod} (perception) + {BonusRoll} ({BonusDice}) = {Total}. " +
            "Success: {SuccessLevel}, Layers: {Layers}, Insight: +{Insight}",
            targetType,
            targetId,
            baseRoll,
            perceptionModifier,
            bonusRoll,
            DeepScanBonusDice,
            totalResult,
            successLevel,
            result.LayersRevealed,
            insightGenerated);

        return result;
    }

    /// <inheritdoc />
    public bool IsValidDeepScanTarget(string targetType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);

        var normalized = targetType.ToLowerInvariant().Trim();
        var isValid = ValidDeepScanTargets.Any(
            t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase));

        _logger.LogDebug(
            "Deep Scan target validation: '{TargetType}' → {IsValid}",
            targetType, isValid);

        return isValid;
    }

    // ═══════ Pattern Recognition (Passive) ═══════

    /// <inheritdoc />
    public bool ApplyPatternRecognition(string targetType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);

        var isJotun = IsJotunTechnology(targetType);

        if (isJotun)
        {
            _logger.LogInformation(
                "Pattern Recognition: auto-success on Layer 2 examination " +
                "for Jötun technology '{TargetType}'",
                targetType);
        }
        else
        {
            _logger.LogDebug(
                "Pattern Recognition: target '{TargetType}' is not Jötun technology, " +
                "no auto-success applied",
                targetType);
        }

        return isJotun;
    }

    /// <inheritdoc />
    public bool IsJotunTechnology(string targetType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);

        var normalized = targetType.ToLowerInvariant().Trim();
        return JotunTechnologyTypes.Any(
            t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    // ═══════ Ancient Tongues (Passive) ═══════

    /// <inheritdoc />
    public bool CanReadScript(ScriptType scriptType)
    {
        var canRead = UnlockedScriptTypes.Contains(scriptType);

        if (canRead)
        {
            _logger.LogDebug(
                "Ancient Tongues: script '{ScriptType}' is comprehensible",
                scriptType);
        }
        else
        {
            _logger.LogWarning(
                "Ancient Tongues: script '{ScriptType}' is NOT among unlocked scripts. " +
                "Unlocked: {UnlockedScripts}",
                scriptType,
                string.Join(", ", UnlockedScriptTypes));
        }

        return canRead;
    }

    /// <inheritdoc />
    public IReadOnlyList<ScriptType> GetUnlockedScripts() => UnlockedScriptTypes;

    // ═══════ Prerequisite Helpers ═══════

    /// <inheritdoc />
    public bool CanUnlockTier2(int ppInvested)
    {
        var canUnlock = ppInvested >= Tier2Threshold;

        _logger.LogDebug(
            "Jötun-Reader Tier 2 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, Tier2Threshold, canUnlock);

        return canUnlock;
    }

    /// <inheritdoc />
    public int CalculatePPInvested(IReadOnlyList<JotunReaderAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += GetAbilityPPCost(ability);
        }

        _logger.LogDebug(
            "Jötun-Reader PP invested calculation: {AbilityCount} abilities unlocked, " +
            "total PP invested: {TotalPP}",
            unlockedAbilities.Count, total);

        return total;
    }

    /// <inheritdoc />
    public int GetAbilityPPCost(JotunReaderAbilityId abilityId) => abilityId switch
    {
        // Tier 1: Free
        JotunReaderAbilityId.DeepScan => 0,
        JotunReaderAbilityId.PatternRecognition => 0,
        JotunReaderAbilityId.AncientTongues => 0,

        // Tier 2: 4 PP each (future — v0.20.3b)
        JotunReaderAbilityId.TechnicalMemory => 4,
        JotunReaderAbilityId.ExploitWeakness => 4,
        JotunReaderAbilityId.DataRecovery => 4,

        // Tier 3: 5 PP each (future — v0.20.3c)
        JotunReaderAbilityId.LoreKeeper => 5,
        JotunReaderAbilityId.AncientKnowledge => 5,

        // Capstone: 6 PP (future — v0.20.3c)
        JotunReaderAbilityId.VoiceOfTheGiants => 6,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown Jötun-Reader ability: {abilityId}")
    };

    // ═══════ Private Helpers ═══════

    /// <summary>
    /// Generates placeholder layer information strings based on success level.
    /// </summary>
    private static List<string> GenerateLayerInfo(
        ExaminationSuccessLevel successLevel,
        string targetType)
    {
        var layers = new List<string>();

        if (successLevel >= ExaminationSuccessLevel.Partial)
            layers.Add($"Layer 1: Basic observation of {targetType}");

        if (successLevel >= ExaminationSuccessLevel.Success)
            layers.Add($"Layer 2: Functional understanding of {targetType}");

        if (successLevel >= ExaminationSuccessLevel.Expert)
            layers.Add($"Layer 3: Detailed technical analysis of {targetType}");

        if (successLevel >= ExaminationSuccessLevel.Master)
            layers.Add($"Layer 4: Complete comprehension of {targetType} — bonus lore revealed");

        return layers;
    }
}
