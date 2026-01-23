// ------------------------------------------------------------------------------
// <copyright file="LockpickingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for executing lockpicking attempts using the System Bypass skill.
// Handles prerequisite validation, dice pool modification, outcome processing,
// and fumble consequence management.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for executing lockpicking attempts using the System Bypass skill.
/// </summary>
/// <remarks>
/// <para>
/// Implements the lockpicking subsystem of the System Bypass skill.
/// Lockpicking in Aethelgard follows cargo cult mechanics—characters manipulate
/// incomprehensible mechanisms through pattern recognition rather than true understanding.
/// </para>
/// <para>
/// The service handles:
/// <list type="bullet">
///   <item><description>Prerequisite validation (tool requirements, skill availability)</description></item>
///   <item><description>DC calculation (lock type + corruption + jammed status)</description></item>
///   <item><description>Dice pool modification (tool quality bonuses/penalties)</description></item>
///   <item><description>Outcome processing (success, failure, fumble, critical)</description></item>
///   <item><description>Consequence management (mechanism jammed on fumble)</description></item>
///   <item><description>Salvage determination (components on critical success)</description></item>
/// </list>
/// </para>
/// <para>
/// Lockpicking outcomes:
/// <list type="bullet">
///   <item><description>Success: Lock opens normally</description></item>
///   <item><description>Critical Success: Lock opens + salvage component</description></item>
///   <item><description>Failure: Lock remains closed, can retry</description></item>
///   <item><description>Fumble: [Mechanism Jammed] - DC +2 permanent, key useless</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class LockpickingService : ILockpickingService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The skill identifier for lockpicking checks.
    /// </summary>
    private const string LockpickingSkillId = "lockpicking";

    /// <summary>
    /// The display name for the lockpicking skill.
    /// </summary>
    private const string LockpickingSkillName = "Lockpicking";

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly SkillCheckService _skillCheckService;
    private readonly IFumbleConsequenceService _fumbleService;
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<LockpickingService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LockpickingService"/> class.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="fumbleService">Service for managing fumble consequences.</param>
    /// <param name="configProvider">Provider for game configuration data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public LockpickingService(
        SkillCheckService skillCheckService,
        IFumbleConsequenceService fumbleService,
        IGameConfigurationProvider configProvider,
        ILogger<LockpickingService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _fumbleService = fumbleService ?? throw new ArgumentNullException(nameof(fumbleService));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("LockpickingService initialized");
    }

    // -------------------------------------------------------------------------
    // Core Lockpicking Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public LockpickingResult AttemptLockpick(Player player, LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(lockContext);

        _logger.LogInformation(
            "Player {PlayerId} ({PlayerName}) attempting to pick {LockType} lock {LockId} " +
            "(BaseDC {BaseDc}, EffectiveDC {EffectiveDc}, Tools: {ToolQuality})",
            player.Id,
            player.Name,
            lockContext.LockType,
            lockContext.LockId,
            lockContext.BaseDc,
            lockContext.EffectiveDc,
            lockContext.ToolQuality);

        // Validate prerequisites
        var blockedReason = GetAttemptBlockedReason(player, lockContext);
        if (blockedReason != null)
        {
            _logger.LogWarning(
                "Lockpicking attempt blocked for player {PlayerId} on lock {LockId}: {Reason}",
                player.Id,
                lockContext.LockId,
                blockedReason);

            return LockpickingResult.Blocked(lockContext, blockedReason);
        }

        // Build skill context from lock context (includes tool modifiers, corruption, jammed status)
        var skillContext = lockContext.ToSkillContext();

        _logger.LogDebug(
            "Skill context for lock {LockId}: DiceModifier={DiceMod}, DcModifier={DcMod}",
            lockContext.LockId,
            skillContext.TotalDiceModifier,
            skillContext.TotalDcModifier);

        // Perform the skill check using the context-aware method
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            LockpickingSkillId,
            lockContext.BaseDc,
            lockContext.LockType.GetDisplayName(),
            skillContext);

        _logger.LogDebug(
            "Lockpicking check result for lock {LockId}: NetSuccesses={NetSuccesses}, " +
            "DC={DC}, Margin={Margin}, Outcome={Outcome}, IsFumble={IsFumble}",
            lockContext.LockId,
            checkResult.NetSuccesses,
            checkResult.DifficultyClass,
            checkResult.Margin,
            checkResult.Outcome,
            checkResult.IsFumble);

        // Process the result based on outcome tier
        return ProcessCheckResult(player, lockContext, checkResult);
    }

    /// <inheritdoc />
    public bool CanAttempt(Player player, LockContext lockContext)
    {
        return GetAttemptBlockedReason(player, lockContext) == null;
    }

    /// <inheritdoc />
    public string? GetAttemptBlockedReason(Player player, LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(lockContext);

        // Check tool requirements from the lock context
        var contextBlockedReason = lockContext.GetBlockedReason();
        if (contextBlockedReason != null)
        {
            _logger.LogDebug(
                "Lock {LockId} blocked for player {PlayerId}: {Reason}",
                lockContext.LockId,
                player.Id,
                contextBlockedReason);
            return contextBlockedReason;
        }

        // Check for existing consequences that might block the attempt
        // Note: Jammed locks can still be attempted at higher DC; we only block
        // if there's a specific blocking consequence type
        var existingConsequences = _fumbleService.GetConsequencesAffectingCheck(
            player.Id.ToString(),
            LockpickingSkillId,
            lockContext.LockId);

        // Check if any consequence explicitly blocks lockpicking on this target
        var blockingConsequence = existingConsequences
            .FirstOrDefault(c => c.BlocksCheck(LockpickingSkillId, lockContext.LockId));

        if (blockingConsequence != null)
        {
            _logger.LogDebug(
                "Lock {LockId} blocked by consequence {ConsequenceId} ({ConsequenceType}) for player {PlayerId}",
                lockContext.LockId,
                blockingConsequence.ConsequenceId,
                blockingConsequence.ConsequenceType,
                player.Id);

            return "The mechanism is too damaged to attempt again without specialized tools.";
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Salvage Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<SalvageableComponent> GetPossibleSalvage(LockType lockType)
    {
        // Return components based on lock type with full property values
        return lockType switch
        {
            LockType.ImprovisedLatch => new[]
            {
                SalvageableComponent.WireBundle(),
                SalvageableComponent.SmallSpring()
            },
            LockType.SimpleLock or LockType.StandardLock => new[]
            {
                SalvageableComponent.HighTensionSpring(),
                SalvageableComponent.PinSet()
            },
            LockType.ComplexLock or LockType.MasterLock => new[]
            {
                SalvageableComponent.CircuitFragment(),
                SalvageableComponent.PowerCellFragment()
            },
            LockType.JotunForged => new[]
            {
                SalvageableComponent.EncryptionChip(),
                SalvageableComponent.BiometricSensor()
            },
            _ => Array.Empty<SalvageableComponent>()
        };
    }

    /// <inheritdoc />
    public SalvageableComponent? SelectRandomSalvage(LockType lockType)
    {
        var possibleSalvage = GetPossibleSalvage(lockType);
        if (possibleSalvage.Count == 0)
        {
            _logger.LogDebug("No salvage available for lock type {LockType}", lockType);
            return null;
        }

        var selected = possibleSalvage[Random.Shared.Next(possibleSalvage.Count)];
        _logger.LogDebug(
            "Selected salvage {ComponentId} ({ComponentName}) for lock type {LockType}",
            selected.ComponentId,
            selected.Name,
            lockType);

        return selected;
    }

    // -------------------------------------------------------------------------
    // Information Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public int GetEffectiveDc(LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(lockContext);
        return lockContext.EffectiveDc;
    }

    /// <inheritdoc />
    public int GetDiceModifier(LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(lockContext);
        return lockContext.ToolDiceModifier;
    }

    /// <inheritdoc />
    public int? EstimateSuccessRate(Player player, LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(lockContext);

        // Cannot estimate if attempt would be blocked
        if (!CanAttempt(player, lockContext))
        {
            return null;
        }

        // Get player's effective dice pool for lockpicking
        // This is a rough estimate based on skill level and tool quality
        var skill = _configProvider.GetSkillById(LockpickingSkillId);
        if (skill == null)
        {
            _logger.LogWarning("Skill {SkillId} not found in configuration", LockpickingSkillId);
            return null;
        }

        // Parse base dice pool and apply tool modifier
        var baseDice = DicePool.Parse(skill.BaseDicePool).Count;
        var effectiveDice = Math.Max(1, baseDice + lockContext.ToolDiceModifier);

        // Rough probability calculation:
        // Each d10 has 30% chance of success (8, 9, 10) and 10% botch (1)
        // Expected net successes = dice * 0.30 - dice * 0.10 = dice * 0.20
        var expectedNetSuccesses = effectiveDice * 0.2;

        // Compare to effective DC
        if (expectedNetSuccesses >= lockContext.EffectiveDc)
        {
            // Good chance of success
            var ratio = expectedNetSuccesses / lockContext.EffectiveDc;
            return Math.Min(90, (int)(50 + ratio * 20));
        }
        else
        {
            // Lower chance
            var ratio = expectedNetSuccesses / lockContext.EffectiveDc;
            return Math.Max(5, (int)(ratio * 50));
        }
    }

    /// <inheritdoc />
    public string GetDifficultyDescription(LockContext lockContext)
    {
        ArgumentNullException.ThrowIfNull(lockContext);

        var description = $"{lockContext.LockType.GetDisplayName()} - {lockContext.DifficultyRating}";

        if (lockContext.IsJammed)
        {
            description += " [JAMMED]";
        }

        if (lockContext.CorruptionLevel != CorruptionTier.Normal)
        {
            description += $" [{lockContext.CorruptionLevel}]";
        }

        return description;
    }

    // -------------------------------------------------------------------------
    // Private Processing Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes the skill check result and creates the appropriate lockpicking result.
    /// </summary>
    /// <param name="player">The player who made the attempt.</param>
    /// <param name="lockContext">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <returns>The processed lockpicking result.</returns>
    private LockpickingResult ProcessCheckResult(
        Player player,
        LockContext lockContext,
        SkillCheckResult checkResult)
    {
        _logger.LogDebug(
            "Processing check result for lock {LockId}: Outcome={Outcome}",
            lockContext.LockId,
            checkResult.Outcome);

        return checkResult.Outcome switch
        {
            SkillOutcome.CriticalFailure => ProcessFumble(player, lockContext, checkResult),
            SkillOutcome.Failure => ProcessFailure(lockContext, checkResult),
            SkillOutcome.CriticalSuccess => ProcessCriticalSuccess(lockContext, checkResult),
            _ => ProcessSuccess(lockContext, checkResult)
        };
    }

    /// <summary>
    /// Processes a successful lockpicking attempt (marginal, full, or exceptional).
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <returns>A success result with appropriate narrative.</returns>
    private LockpickingResult ProcessSuccess(LockContext lockContext, SkillCheckResult checkResult)
    {
        var narrative = LockpickingResult.GetNarrativeTemplate(checkResult.Outcome, lockContext.DisplayName);

        _logger.LogInformation(
            "Lock {LockId} successfully picked with outcome {Outcome} (NetSuccesses={NetSuccesses}, DC={DC})",
            lockContext.LockId,
            checkResult.Outcome,
            checkResult.NetSuccesses,
            lockContext.EffectiveDc);

        return LockpickingResult.Success(
            checkResult.Outcome,
            lockContext,
            checkResult,
            narrative);
    }

    /// <summary>
    /// Processes a critical success with component salvage.
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <returns>A success result with salvaged component.</returns>
    private LockpickingResult ProcessCriticalSuccess(LockContext lockContext, SkillCheckResult checkResult)
    {
        // Select a random salvageable component
        var salvage = SelectRandomSalvage(lockContext.LockType);

        var narrative = salvage.HasValue
            ? $"The lock yields effortlessly to your masterful touch. As {lockContext.DisplayName} opens, " +
              $"you notice a valuable [{salvage.Value.Name}] that you carefully extract."
            : $"The lock yields effortlessly to your masterful touch. {lockContext.DisplayName} opens with barely a sound.";

        _logger.LogInformation(
            "Lock {LockId} critically picked, salvaged {Component} (NetSuccesses={NetSuccesses}, Margin={Margin})",
            lockContext.LockId,
            salvage?.ComponentId ?? "nothing",
            checkResult.NetSuccesses,
            checkResult.Margin);

        return LockpickingResult.Success(
            SkillOutcome.CriticalSuccess,
            lockContext,
            checkResult,
            narrative,
            salvage);
    }

    /// <summary>
    /// Processes a failed lockpicking attempt.
    /// </summary>
    /// <param name="lockContext">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <returns>A failure result with incremented attempt count.</returns>
    private LockpickingResult ProcessFailure(LockContext lockContext, SkillCheckResult checkResult)
    {
        var narrative = LockpickingResult.GetNarrativeTemplate(SkillOutcome.Failure, lockContext.DisplayName);

        // Track the failed attempt
        var updatedContext = lockContext.WithFailedAttempt();

        _logger.LogDebug(
            "Lock {LockId} pick failed (NetSuccesses={NetSuccesses}, DC={DC}), attempt count now: {Attempts}",
            lockContext.LockId,
            checkResult.NetSuccesses,
            lockContext.EffectiveDc,
            updatedContext.PreviousAttempts);

        return LockpickingResult.Failure(
            SkillOutcome.Failure,
            lockContext,
            checkResult,
            narrative,
            updatedContext);
    }

    /// <summary>
    /// Processes a fumble with [Mechanism Jammed] consequence.
    /// </summary>
    /// <param name="player">The player who fumbled.</param>
    /// <param name="lockContext">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <returns>A fumble result with consequence and jammed lock.</returns>
    private LockpickingResult ProcessFumble(
        Player player,
        LockContext lockContext,
        SkillCheckResult checkResult)
    {
        var lockDisplayName = lockContext.LockName ?? lockContext.LockType.GetDisplayName();

        // Create the [Mechanism Jammed] consequence with custom description
        var consequenceDescription = $"The lock mechanism ({lockDisplayName}) is jammed. " +
                                     "DC permanently +2, associated key no longer works.";

        var consequence = _fumbleService.CreateConsequence(
            player.Id.ToString(),
            LockpickingSkillId,
            FumbleType.MechanismJammed,
            lockContext.LockId,
            consequenceDescription);

        // Update the lock context to reflect the jam
        var updatedContext = lockContext.WithJammed();

        var narrative = LockpickingResult.GetNarrativeTemplate(SkillOutcome.CriticalFailure, lockContext.DisplayName);

        // Check if improvised tools broke on fumble
        var toolBroken = lockContext.ToolQuality == ToolQuality.Improvised;

        if (toolBroken)
        {
            narrative += " Your improvised tools snap under the strain.";
        }

        _logger.LogWarning(
            "Lock {LockId} JAMMED by fumble from player {PlayerId} ({PlayerName}). " +
            "Consequence {ConsequenceId} created. Tools broken: {ToolBroken}",
            lockContext.LockId,
            player.Id,
            player.Name,
            consequence.ConsequenceId,
            toolBroken);

        return LockpickingResult.Fumble(
            lockContext,
            checkResult,
            consequence,
            updatedContext,
            narrative,
            toolBroken);
    }
}
