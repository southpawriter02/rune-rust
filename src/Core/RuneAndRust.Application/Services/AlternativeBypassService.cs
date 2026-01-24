// ------------------------------------------------------------------------------
// <copyright file="AlternativeBypassService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling alternative bypass methods when primary skill-based
// approaches are unavailable. Includes MIGHT-based brute force for physical
// obstacles and alternative approaches for doors, terminals, and traps.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling alternative bypass methods when primary skill-based
/// approaches are unavailable.
/// </summary>
/// <remarks>
/// <para>
/// This service ensures that obstacles are never absolute barriers. Every
/// locked door, secured terminal, and active trap has multiple solutions,
/// though each comes with its own trade-offs:
/// <list type="bullet">
///   <item><description>Brute Force: MIGHT-based, fast but loud and may damage contents</description></item>
///   <item><description>Find Key/Codes: Investigation-based, takes time but clean</description></item>
///   <item><description>Runic Bypass: Runecraft-based, requires abilities but quick</description></item>
///   <item><description>Alternate Routes: Perception-based, avoids obstacle entirely</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Design Principle:</b> No character should be permanently stuck because
/// they lack a specific skill. Alternative methods provide different paths
/// forward, each suited to different character builds.
/// </para>
/// </remarks>
public sealed class AlternativeBypassService : IAlternativeBypassService
{
    // =========================================================================
    // CONSTANTS
    // =========================================================================

    /// <summary>
    /// Net successes threshold for critical success (≥5).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    /// <summary>
    /// Fumble damage dice (1d6).
    /// </summary>
    private const int FumbleDamageDice = 1;
    private const int FumbleDamageDieSize = 6;

    /// <summary>
    /// Content damage dice (1d10).
    /// </summary>
    private const int ContentDamageDice = 1;
    private const int ContentDamageDieSize = 10;

    /// <summary>
    /// Minimum dice in any pool.
    /// </summary>
    private const int MinimumDicePool = 1;

    /// <summary>
    /// Maximum assistance bonus dice.
    /// </summary>
    private const int MaxAssistanceBonusDice = 2;

    // =========================================================================
    // TOOL BONUS DICE MAPPING
    // =========================================================================

    /// <summary>
    /// Bonus dice provided by tools for brute force attempts.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, int> ToolBonusDiceMap =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "crowbar", 1 },
            { "sledgehammer", 2 },
            { "breaching charge", 2 },
            { "industrial cutter", 2 },
            { "knife", 0 } // DC modifier only, no bonus dice
        };

    // =========================================================================
    // DEPENDENCIES
    // =========================================================================

    private readonly IDiceService _diceService;
    private readonly ILogger<AlternativeBypassService> _logger;

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="AlternativeBypassService"/> class.
    /// </summary>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AlternativeBypassService(
        IDiceService diceService,
        ILogger<AlternativeBypassService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("AlternativeBypassService initialized");
    }

    // =========================================================================
    // BRUTE FORCE METHODS
    // =========================================================================

    /// <inheritdoc />
    public BruteForceOption GetBruteForceOption(BruteForceTargetType targetType, int targetStrength = 3)
    {
        _logger.LogDebug(
            "Getting brute force option for {TargetType} with strength {Strength}",
            targetType,
            targetStrength);

        return targetType switch
        {
            BruteForceTargetType.SimpleDoor => BruteForceOption.SimpleDoor,
            BruteForceTargetType.ReinforcedDoor => BruteForceOption.ReinforcedDoor,
            BruteForceTargetType.Vault => BruteForceOption.Vault,
            BruteForceTargetType.Container => BruteForceOption.Container(targetStrength),
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, "Unknown target type")
        };
    }

    /// <inheritdoc />
    public BruteForceResult AttemptBruteForce(int mightAttribute, BruteForceOption target, BruteForceContext context)
    {
        _logger.LogInformation(
            "Brute force attempt on {TargetType}, attempt #{AttemptNumber}, MIGHT {Might}",
            target.TargetType,
            context.AttemptNumber,
            mightAttribute);

        // Calculate effective DC
        var effectiveDc = target.GetEffectiveDc(context.AttemptNumber - 1, context.HasFumbled);

        // Apply tool DC modifier if available
        if (!string.IsNullOrEmpty(context.ToolUsed))
        {
            var toolDcMod = target.GetToolDcModifier(context.ToolUsed);
            effectiveDc += toolDcMod;

            _logger.LogDebug(
                "Tool {Tool} modifies DC by {Modifier}, effective DC now {EffectiveDc}",
                context.ToolUsed,
                toolDcMod,
                effectiveDc);
        }

        _logger.LogDebug(
            "Effective DC: {BaseDc} + ({Attempts} × {RetryPenalty}) + (fumbled: {Fumbled} ? 2 : 0) = {EffectiveDc}",
            target.BaseDc,
            context.AttemptNumber - 1,
            target.RetryPenaltyPerAttempt,
            context.HasFumbled,
            effectiveDc);

        // Build dice pool from MIGHT
        var dicePool = mightAttribute;

        // Apply tool bonus dice
        if (!string.IsNullOrEmpty(context.ToolUsed))
        {
            var toolBonusDice = GetToolBonusDice(context.ToolUsed);
            dicePool += toolBonusDice;

            _logger.LogDebug("Tool {Tool} adds {BonusDice} bonus dice", context.ToolUsed, toolBonusDice);
        }

        // Apply assistance bonus (max +2d10)
        var assistBonus = Math.Min(context.AssistingCharacters, MaxAssistanceBonusDice);
        if (assistBonus > 0)
        {
            dicePool += assistBonus;
            _logger.LogDebug("{Assistants} assistants add {BonusDice} bonus dice", context.AssistingCharacters, assistBonus);
        }

        // Apply exhaustion penalty
        var exhaustionPenalty = context.ExhaustionLevel / 2;
        if (exhaustionPenalty > 0)
        {
            dicePool -= exhaustionPenalty;
            _logger.LogDebug("Exhaustion level {Level} applies -{Penalty} penalty dice", context.ExhaustionLevel, exhaustionPenalty);
        }

        // Ensure minimum pool
        dicePool = Math.Max(MinimumDicePool, dicePool);

        _logger.LogDebug(
            "Dice pool: {Might} + {ToolBonus} + {AssistBonus} - {ExhaustionPenalty} = {Total}d10",
            mightAttribute,
            !string.IsNullOrEmpty(context.ToolUsed) ? GetToolBonusDice(context.ToolUsed) : 0,
            assistBonus,
            exhaustionPenalty,
            dicePool);

        // Roll the dice
        var rollResult = _diceService.Roll(DicePool.D10(dicePool));

        _logger.LogDebug(
            "Brute force roll: {Dice}d10 → {Successes} successes, {Botches} botches, net {Net}",
            dicePool,
            rollResult.TotalSuccesses,
            rollResult.TotalBotches,
            rollResult.NetSuccesses);

        // Determine outcome
        if (rollResult.IsFumble)
        {
            _logger.LogWarning(
                "Brute force FUMBLE on {TargetType}: rolling damage, tool broken: {ToolUsed}",
                target.TargetType,
                !string.IsNullOrEmpty(context.ToolUsed));

            return CreateFumbleResult(target, context);
        }

        if (rollResult.NetSuccesses > 0)
        {
            var isCritical = rollResult.NetSuccesses >= CriticalSuccessThreshold;

            _logger.LogInformation(
                "Brute force {Result} on {TargetType}: NetSuccesses={Net}, IsCritical={IsCritical}",
                isCritical ? "CRITICAL SUCCESS" : "SUCCESS",
                target.TargetType,
                rollResult.NetSuccesses,
                isCritical);

            return CreateSuccessResult(target, isCritical);
        }

        _logger.LogInformation(
            "Brute force FAILED on {TargetType}: NetSuccesses={Net}, CanRetry={CanRetry}",
            target.TargetType,
            rollResult.NetSuccesses,
            target.CanRetry(context.AttemptNumber));

        return CreateFailureResult(target, context);
    }

    /// <inheritdoc />
    public bool CanRetry(BruteForceOption target, int attemptsMade)
    {
        var canRetry = target.CanRetry(attemptsMade);

        _logger.LogDebug(
            "CanRetry check for {TargetType}: attempts={Attempts}, max={Max}, result={CanRetry}",
            target.TargetType,
            attemptsMade,
            target.MaxAttempts,
            canRetry);

        return canRetry;
    }

    /// <inheritdoc />
    public int GetRetryDc(BruteForceOption target, int attemptsMade, bool hasFumbled)
    {
        var retryDc = target.GetEffectiveDc(attemptsMade, hasFumbled);

        _logger.LogDebug(
            "GetRetryDc for {TargetType}: baseDc={BaseDc}, attempts={Attempts}, fumbled={Fumbled}, retryDc={RetryDc}",
            target.TargetType,
            target.BaseDc,
            attemptsMade,
            hasFumbled,
            retryDc);

        return retryDc;
    }

    // =========================================================================
    // ALTERNATIVE METHOD QUERIES
    // =========================================================================

    /// <inheritdoc />
    public IEnumerable<AlternativeMethod> GetAlternatives(BypassObstacleType obstacleType)
    {
        _logger.LogDebug("Getting alternatives for {ObstacleType}", obstacleType);

        var alternatives = AlternativeMethod.GetAlternativesFor(obstacleType).ToList();

        _logger.LogDebug("Found {Count} alternatives for {ObstacleType}", alternatives.Count, obstacleType);

        return alternatives;
    }

    /// <inheritdoc />
    public AlternativeMethod? GetAlternativeById(string methodId)
    {
        _logger.LogDebug("Looking up alternative method by ID: {MethodId}", methodId);

        var method = AlternativeMethod.GetById(methodId);

        if (method.HasValue)
        {
            _logger.LogDebug("Found method: {MethodName}", method.Value.MethodName);
        }
        else
        {
            _logger.LogDebug("Method not found: {MethodId}", methodId);
        }

        return method;
    }

    // =========================================================================
    // PREREQUISITE EVALUATION
    // =========================================================================

    /// <inheritdoc />
    public AlternativeEvaluationResult EvaluateAlternative(
        AlternativeMethod method,
        IReadOnlyList<string> characterAbilities,
        IReadOnlyList<string> characterItems)
    {
        _logger.LogDebug(
            "Evaluating alternative {MethodName} for character with {AbilityCount} abilities and {ItemCount} items",
            method.MethodName,
            characterAbilities.Count,
            characterItems.Count);

        var missingPrerequisites = new List<string>();

        foreach (var prereq in method.Prerequisites)
        {
            if (!MeetsPrerequisite(prereq, characterAbilities, characterItems))
            {
                missingPrerequisites.Add(prereq);
                _logger.LogDebug("Missing prerequisite: {Prerequisite}", prereq);
            }
        }

        var difficulty = GetDifficultyDescription(method.CheckDc);
        var canAttempt = missingPrerequisites.Count == 0;

        _logger.LogInformation(
            "Evaluation result for {MethodName}: CanAttempt={CanAttempt}, MissingCount={MissingCount}, Difficulty={Difficulty}",
            method.MethodName,
            canAttempt,
            missingPrerequisites.Count,
            difficulty);

        if (canAttempt)
        {
            return AlternativeEvaluationResult.Ready(difficulty);
        }

        return AlternativeEvaluationResult.NotReady(missingPrerequisites.AsReadOnly(), difficulty);
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// <inheritdoc />
    public string GetDifficultyDescription(int dc)
    {
        return dc switch
        {
            0 => "Automatic",
            <= 8 => "Very Easy",
            <= 12 => "Easy",
            <= 16 => "Moderate",
            <= 20 => "Hard",
            <= 24 => "Very Hard",
            _ => "Nearly Impossible"
        };
    }

    /// <inheritdoc />
    public int GetToolBonusDice(string toolName)
    {
        if (string.IsNullOrEmpty(toolName))
        {
            return 0;
        }

        return ToolBonusDiceMap.TryGetValue(toolName, out var bonusDice) ? bonusDice : 0;
    }

    // =========================================================================
    // PRIVATE HELPERS - RESULT CREATION
    // =========================================================================

    /// <summary>
    /// Creates a successful brute force result.
    /// </summary>
    private BruteForceResult CreateSuccessResult(BruteForceOption target, bool isCritical)
    {
        var consequences = ApplyConsequences(target.Consequences, isCritical);
        var noise = DetermineNoiseLevel(target.Consequences, isCritical);
        var contentDamage = DetermineContentDamage(target.Consequences, isCritical);
        var exhaustion = DetermineExhaustion(target.Consequences, isCritical);

        var narrative = isCritical
            ? GenerateCriticalSuccessNarrative(target.TargetType)
            : GenerateSuccessNarrative(target.TargetType);

        _logger.LogDebug(
            "Success result: Critical={IsCritical}, Noise={Noise}, ContentDamage={ContentDamage}, Exhaustion={Exhaustion}",
            isCritical,
            noise,
            contentDamage,
            exhaustion);

        return BruteForceResult.CreateSuccess(
            isCritical,
            consequences,
            noise,
            narrative,
            contentDamage,
            exhaustion);
    }

    /// <summary>
    /// Creates a failed brute force result.
    /// </summary>
    private BruteForceResult CreateFailureResult(BruteForceOption target, BruteForceContext context)
    {
        var narrative = GenerateFailureNarrative(target.TargetType, context.AttemptNumber);

        return BruteForceResult.CreateFailure(
            context.AttemptNumber,
            target.MaxAttempts,
            target.BaseDc,
            target.RetryPenaltyPerAttempt,
            narrative);
    }

    /// <summary>
    /// Creates a fumble brute force result.
    /// </summary>
    private BruteForceResult CreateFumbleResult(BruteForceOption target, BruteForceContext context)
    {
        // Roll fumble damage (1d6)
        var damageRoll = _diceService.Roll(DicePool.D6(FumbleDamageDice));
        var fumbleDamage = damageRoll.Total;

        _logger.LogDebug("Fumble damage roll: {Damage}", fumbleDamage);

        var narrative = GenerateFumbleNarrative(target.TargetType);

        return BruteForceResult.CreateFumble(
            context.AttemptNumber,
            target.MaxAttempts,
            target.BaseDc,
            target.RetryPenaltyPerAttempt,
            !string.IsNullOrEmpty(context.ToolUsed),
            fumbleDamage,
            narrative);
    }

    // =========================================================================
    // PRIVATE HELPERS - CONSEQUENCE PROCESSING
    // =========================================================================

    /// <summary>
    /// Applies consequences based on probability and critical success avoidance.
    /// </summary>
    private IReadOnlyList<AppliedConsequence> ApplyConsequences(
        IReadOnlyList<BruteForceConsequence> consequences,
        bool isCritical)
    {
        var applied = new List<AppliedConsequence>();

        foreach (var consequence in consequences)
        {
            // Critical success can avoid some consequences
            if (isCritical && consequence.CanBeAvoidedOnCritical)
            {
                _logger.LogDebug(
                    "Consequence {Type} avoided due to critical success",
                    consequence.Type);
                continue;
            }

            // Check probability
            if (!consequence.ShouldApply(Random.Shared))
            {
                _logger.LogDebug(
                    "Consequence {Type} not applied (probability check failed, prob={Probability})",
                    consequence.Type,
                    consequence.Probability);
                continue;
            }

            applied.Add(new AppliedConsequence(
                consequence.Type,
                consequence.GetNarrativeDescription()));

            _logger.LogDebug("Applied consequence: {Type}", consequence.Type);
        }

        return applied.AsReadOnly();
    }

    /// <summary>
    /// Determines the noise level based on consequences.
    /// </summary>
    private NoiseLevel DetermineNoiseLevel(IReadOnlyList<BruteForceConsequence> consequences, bool isCritical)
    {
        var noiseConsequence = consequences
            .Where(c => c.Type == ConsequenceType.Noise || c.Type == ConsequenceType.MaxNoise)
            .OrderByDescending(c => c.NoiseLevel)
            .FirstOrDefault();

        if (noiseConsequence.Type == ConsequenceType.Noise ||
            noiseConsequence.Type == ConsequenceType.MaxNoise)
        {
            var baseNoise = noiseConsequence.NoiseLevel;

            // Critical success reduces noise by one level if consequence is avoidable
            if (isCritical && noiseConsequence.CanBeAvoidedOnCritical)
            {
                return ReduceNoiseLevel(baseNoise);
            }

            return baseNoise;
        }

        // Default noise for brute force attempts
        return isCritical ? NoiseLevel.Moderate : NoiseLevel.Loud;
    }

    /// <summary>
    /// Determines content damage based on consequences.
    /// </summary>
    private int DetermineContentDamage(IReadOnlyList<BruteForceConsequence> consequences, bool isCritical)
    {
        // Critical success avoids content damage
        if (isCritical)
        {
            return 0;
        }

        var contentConsequence = consequences
            .FirstOrDefault(c => c.Type == ConsequenceType.ContentDamage);

        if (contentConsequence.Type == ConsequenceType.ContentDamage &&
            contentConsequence.ShouldApply(Random.Shared))
        {
            // Roll content damage (1d10)
            var damageRoll = _diceService.Roll(DicePool.D10(ContentDamageDice));
            return damageRoll.Total;
        }

        return 0;
    }

    /// <summary>
    /// Determines exhaustion gained based on consequences.
    /// </summary>
    private int DetermineExhaustion(IReadOnlyList<BruteForceConsequence> consequences, bool isCritical)
    {
        // Critical success avoids exhaustion
        if (isCritical)
        {
            return 0;
        }

        var exhaustionConsequence = consequences
            .FirstOrDefault(c => c.Type == ConsequenceType.Exhaustion);

        if (exhaustionConsequence.Type == ConsequenceType.Exhaustion)
        {
            return 1; // Standard exhaustion from brute force
        }

        return 0;
    }

    /// <summary>
    /// Reduces noise level by one step.
    /// </summary>
    private static NoiseLevel ReduceNoiseLevel(NoiseLevel level) => level switch
    {
        NoiseLevel.Extreme => NoiseLevel.VeryLoud,
        NoiseLevel.VeryLoud => NoiseLevel.Loud,
        NoiseLevel.Loud => NoiseLevel.Moderate,
        NoiseLevel.Moderate => NoiseLevel.Quiet,
        _ => NoiseLevel.Silent
    };

    // =========================================================================
    // PRIVATE HELPERS - PREREQUISITE CHECKING
    // =========================================================================

    /// <summary>
    /// Checks if a specific prerequisite is met.
    /// </summary>
    private bool MeetsPrerequisite(
        string prerequisite,
        IReadOnlyList<string> abilities,
        IReadOnlyList<string> items)
    {
        // Normalize for comparison
        var normalizedPrereq = prerequisite.ToLowerInvariant();

        // Check for ability prerequisites (indicated by brackets)
        if (prerequisite.Contains('[') && prerequisite.Contains(']'))
        {
            // Extract ability name from brackets
            var start = prerequisite.IndexOf('[') + 1;
            var end = prerequisite.IndexOf(']');
            var abilityName = prerequisite.Substring(start, end - start);

            return abilities.Any(a =>
                a.Contains(abilityName, StringComparison.OrdinalIgnoreCase));
        }

        // Check for skill requirements
        if (normalizedPrereq.Contains("skill trained") ||
            normalizedPrereq.Contains("skill"))
        {
            // Extract skill name (e.g., "Runecraft skill trained" -> "Runecraft")
            var skillName = prerequisite
                .Replace("skill trained", "", StringComparison.OrdinalIgnoreCase)
                .Replace("skill", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            return abilities.Any(a =>
                a.Contains(skillName, StringComparison.OrdinalIgnoreCase));
        }

        // Check for item prerequisites
        if (normalizedPrereq.Contains("available") ||
            normalizedPrereq.Contains("weapon") ||
            normalizedPrereq.Contains("item") ||
            normalizedPrereq.Contains("object") ||
            normalizedPrereq.Contains("shield") ||
            normalizedPrereq.Contains("wire") ||
            normalizedPrereq.Contains("cable"))
        {
            // Check if any item might satisfy this requirement
            // This is a simplified check - real implementation would be more specific
            return items.Any(i =>
                normalizedPrereq.Contains(i.ToLowerInvariant()) ||
                i.ToLowerInvariant().Split(' ')
                    .Any(word => normalizedPrereq.Contains(word)));
        }

        // Check for context requirements (e.g., "Must have achieved User Level access")
        if (normalizedPrereq.Contains("must have") ||
            normalizedPrereq.Contains("achieved") ||
            normalizedPrereq.Contains("access"))
        {
            // These are context-dependent and would need to be checked against game state
            // For now, assume not met if we can't verify
            _logger.LogDebug(
                "Context-dependent prerequisite not automatically verifiable: {Prerequisite}",
                prerequisite);
            return false;
        }

        // Check for environmental requirements
        if (normalizedPrereq.Contains("terminal") ||
            normalizedPrereq.Contains("line of sight") ||
            normalizedPrereq.Contains("exist in area"))
        {
            // These require game state verification
            _logger.LogDebug(
                "Environmental prerequisite not automatically verifiable: {Prerequisite}",
                prerequisite);
            return false;
        }

        // If we can't categorize it, assume it's not met
        _logger.LogDebug("Unknown prerequisite type, assuming not met: {Prerequisite}", prerequisite);
        return false;
    }

    // =========================================================================
    // PRIVATE HELPERS - NARRATIVE GENERATION
    // =========================================================================

    /// <summary>
    /// Generates narrative text for critical success.
    /// </summary>
    private static string GenerateCriticalSuccessNarrative(BruteForceTargetType target)
    {
        return target switch
        {
            BruteForceTargetType.SimpleDoor =>
                "With a precise strike, the door gives way cleanly. The hinges surrender without protest, and the opening is clear.",
            BruteForceTargetType.ReinforcedDoor =>
                "You find the weak point in the reinforcement. One powerful blow, and the door buckles inward with minimal noise.",
            BruteForceTargetType.Vault =>
                "The vault door groans under your assault, and ancient seals finally give way. The interior appears undisturbed.",
            BruteForceTargetType.Container =>
                "The lock mechanism snaps cleanly, and the container opens without damaging the contents within.",
            _ => "The obstacle yields to your strength with surprising ease."
        };
    }

    /// <summary>
    /// Generates narrative text for normal success.
    /// </summary>
    private static string GenerateSuccessNarrative(BruteForceTargetType target)
    {
        return target switch
        {
            BruteForceTargetType.SimpleDoor =>
                "The door crashes inward with a resounding bang. Splinters scatter across the floor.",
            BruteForceTargetType.ReinforcedDoor =>
                "Metal screeches against metal as the door finally gives way. The noise echoes through the corridors.",
            BruteForceTargetType.Vault =>
                "After tremendous effort, the vault door surrenders. Your muscles ache, but the way is open.",
            BruteForceTargetType.Container =>
                "The container bursts open. Some contents may have shifted roughly.",
            _ => "The obstacle is destroyed."
        };
    }

    /// <summary>
    /// Generates narrative text for failure.
    /// </summary>
    private static string GenerateFailureNarrative(BruteForceTargetType target, int attempt)
    {
        var attemptText = attempt > 1 ? $"Despite your {attempt} attempts, " : "";

        return target switch
        {
            BruteForceTargetType.SimpleDoor =>
                $"{attemptText}the door holds firm. You'll need to try harder.",
            BruteForceTargetType.ReinforcedDoor =>
                $"{attemptText}the reinforced door refuses to yield. Perhaps a different approach?",
            BruteForceTargetType.Vault =>
                $"{attemptText}the vault door remains sealed. This will require significant effort.",
            BruteForceTargetType.Container =>
                $"{attemptText}the container's lock resists your force.",
            _ => "The obstacle remains intact."
        };
    }

    /// <summary>
    /// Generates narrative text for fumble.
    /// </summary>
    private static string GenerateFumbleNarrative(BruteForceTargetType target)
    {
        return target switch
        {
            BruteForceTargetType.SimpleDoor =>
                "Your strike goes wide! You stumble into the doorframe, taking damage from the impact.",
            BruteForceTargetType.ReinforcedDoor =>
                "The door doesn't budge, but something in your shoulder does. Pain shoots through your arm.",
            BruteForceTargetType.Vault =>
                "A hidden mechanism triggers as you strike wrong—sparks fly and you jerk back, burned.",
            BruteForceTargetType.Container =>
                "Your tool slips, gashing your hand. Blood drips onto the unyielding container.",
            _ => "Something goes wrong. You take damage from your own effort."
        };
    }
}
