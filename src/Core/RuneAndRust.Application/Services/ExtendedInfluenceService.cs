// ------------------------------------------------------------------------------
// <copyright file="ExtendedInfluenceService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of the Extended Influence Service for gradually changing
// NPC beliefs over multiple interactions with pool accumulation and resistance.
// Part of v0.15.3h Extended Influence System implementation.
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
/// Implementation of the Extended Influence Service for gradually changing
/// NPC beliefs over multiple interactions with pool accumulation and resistance.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates the complete extended influence workflow including:
/// </para>
/// <list type="bullet">
///   <item><description>Creating and managing influence tracking per character/NPC/belief</description></item>
///   <item><description>Processing rhetoric checks against conviction-based DCs</description></item>
///   <item><description>Accumulating influence pool from net successes</description></item>
///   <item><description>Managing resistance increases on failed attempts</description></item>
///   <item><description>Detecting conviction threshold reached and belief changes</description></item>
///   <item><description>Handling stall conditions and resumption</description></item>
/// </list>
/// <para>
/// The service uses success-counting dice mechanics where attribute + skill
/// determines dice pool, and each die rolling 6+ is a success.
/// </para>
/// </remarks>
public class ExtendedInfluenceService : IExtendedInfluenceService
{
    /// <summary>
    /// Maximum resistance modifier before potential failure.
    /// </summary>
    private const int MaxResistance = 6;

    /// <summary>
    /// Resistance reduction when resuming from stalled state.
    /// </summary>
    private const int DefaultResumeResistanceReduction = 2;

    /// <summary>
    /// Resistance threshold that triggers stall conditions.
    /// </summary>
    private const int StallResistanceThreshold = 4;

    /// <summary>
    /// The logger instance for comprehensive logging.
    /// </summary>
    private readonly ILogger<ExtendedInfluenceService> _logger;

    /// <summary>
    /// Repository for persisting influence tracking.
    /// </summary>
    private readonly IExtendedInfluenceRepository _repository;

    /// <summary>
    /// Random number generator for dice rolls.
    /// </summary>
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedInfluenceService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for comprehensive logging.</param>
    /// <param name="repository">The repository for influence persistence.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when logger or repository is null.
    /// </exception>
    public ExtendedInfluenceService(
        ILogger<ExtendedInfluenceService> logger,
        IExtendedInfluenceRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _random = new Random();
    }

    #region Influence Attempt Methods

    /// <inheritdoc/>
    public InfluenceAttemptResult AttemptInfluence(
        string characterId,
        string targetId,
        string beliefId,
        int characterRhetoric,
        int characterAttribute,
        int bonusDice = 0,
        int dcModifier = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(beliefId, nameof(beliefId));

        _logger.LogInformation(
            "Influence attempt: Character {CharacterId} attempting to influence " +
            "{TargetId} on belief {BeliefId}",
            characterId, targetId, beliefId);

        // Get the influence tracking (must already exist)
        var influence = _repository.GetByCharacterTargetAndBelief(characterId, targetId, beliefId);
        if (influence == null)
        {
            _logger.LogWarning(
                "No influence tracking found for Character {CharacterId}, " +
                "Target {TargetId}, Belief {BeliefId}. Use InitializeInfluence first.",
                characterId, targetId, beliefId);

            throw new InvalidOperationException(
                $"No influence tracking found for character {characterId} on belief {beliefId}. " +
                "Initialize the influence first using InitializeInfluence or GetOrCreateInfluence.");
        }

        // Validate state allows attempts
        if (!influence.Status.CanContinue())
        {
            _logger.LogWarning(
                "Cannot attempt influence when status is {Status}. " +
                "Influence {InfluenceId}",
                influence.Status, influence.Id);

            throw new InvalidOperationException(
                $"Cannot attempt influence when status is {influence.Status}. " +
                (influence.Status == InfluenceStatus.Stalled
                    ? $"Resume the influence first. Condition: {influence.ResumeCondition}"
                    : "This influence has reached a terminal state."));
        }

        // Calculate dice pool and DC
        var dicePool = characterAttribute + characterRhetoric + bonusDice;
        var baseDc = influence.GetBaseDc();
        var effectiveDc = influence.GetEffectiveDc() + dcModifier;

        _logger.LogDebug(
            "Influence attempt details: DicePool {DicePool} " +
            "(Attr {Attr} + Rhetoric {Rhetoric} + Bonus {Bonus}), " +
            "DC {EffectiveDc} (Base {BaseDc} + Resistance {Resistance} + Modifier {Modifier})",
            dicePool, characterAttribute, characterRhetoric, bonusDice,
            effectiveDc, baseDc, influence.ResistanceModifier, dcModifier);

        // Roll the dice (success-counting: 6+ on d10)
        var successes = RollSuccesses(dicePool);
        var netSuccesses = successes - effectiveDc;
        var isSuccess = netSuccesses >= 0;

        _logger.LogDebug(
            "Rhetoric check: {Successes} successes vs DC {DC}, " +
            "Net: {NetSuccesses}, Result: {Result}",
            successes, effectiveDc, netSuccesses, isSuccess ? "SUCCESS" : "FAILURE");

        InfluenceAttemptResult result;

        if (isSuccess)
        {
            result = ProcessSuccessfulAttempt(
                influence, netSuccesses, dicePool, successes);
        }
        else
        {
            result = ProcessFailedAttempt(
                influence, netSuccesses, dicePool, successes);
        }

        // Record the attempt in history
        influence.RecordAttempt(result);

        // Save the updated influence
        _repository.Save(influence);

        _logger.LogInformation(
            "Influence attempt complete: {Result}. Pool: {Pool}/{Threshold} ({Progress}%), " +
            "Resistance: {Resistance}, Status: {Status}",
            isSuccess ? "SUCCESS" : "FAILURE",
            influence.InfluencePool, influence.GetThreshold(), influence.ProgressPercentage,
            influence.ResistanceModifier, influence.Status);

        return result;
    }

    /// <inheritdoc/>
    public Task<InfluenceAttemptResult> AttemptInfluenceAsync(
        string characterId,
        string targetId,
        string beliefId,
        int characterRhetoric,
        int characterAttribute,
        int bonusDice = 0,
        int dcModifier = 0)
    {
        _logger.LogDebug(
            "AttemptInfluenceAsync called for Character {CharacterId}, " +
            "Target {TargetId}, Belief {BeliefId}",
            characterId, targetId, beliefId);

        var result = AttemptInfluence(
            characterId, targetId, beliefId,
            characterRhetoric, characterAttribute,
            bonusDice, dcModifier);

        return Task.FromResult(result);
    }

    /// <summary>
    /// Processes a successful influence attempt.
    /// </summary>
    private InfluenceAttemptResult ProcessSuccessfulAttempt(
        ExtendedInfluence influence,
        int netSuccesses,
        int diceRolled,
        int successesRolled)
    {
        var previousPool = influence.InfluencePool;
        var threshold = influence.GetThreshold();
        var interactionNumber = influence.InteractionCount + 1;

        // Add to pool (net successes, minimum 1 on success)
        var poolGain = Math.Max(1, netSuccesses);
        influence.AddToPool(poolGain);

        var isBreakthrough = poolGain >= 3;
        var narrative = GenerateSuccessNarrative(
            influence.TargetConviction, poolGain, isBreakthrough,
            influence.IsThresholdReached());

        _logger.LogInformation(
            "Successful influence: +{PoolGain} to pool. " +
            "New total: {NewPool}/{Threshold}{Breakthrough}",
            poolGain, influence.InfluencePool, threshold,
            isBreakthrough ? " (BREAKTHROUGH!)" : string.Empty);

        if (influence.Status == InfluenceStatus.Successful)
        {
            _logger.LogInformation(
                "*** CONVICTION THRESHOLD REACHED *** " +
                "Belief '{BeliefDescription}' has been changed! " +
                "Influence {InfluenceId}",
                influence.BeliefDescription, influence.Id);
        }

        return InfluenceAttemptResult.Success(
            netSuccesses: netSuccesses,
            previousPool: previousPool,
            threshold: threshold,
            resistance: influence.ResistanceModifier,
            conviction: influence.TargetConviction,
            interactionNumber: interactionNumber,
            diceRolled: diceRolled,
            successesRolled: successesRolled,
            narrative: narrative);
    }

    /// <summary>
    /// Processes a failed influence attempt.
    /// </summary>
    private InfluenceAttemptResult ProcessFailedAttempt(
        ExtendedInfluence influence,
        int netSuccesses,
        int diceRolled,
        int successesRolled)
    {
        var previousResistance = influence.ResistanceModifier;
        var interactionNumber = influence.InteractionCount + 1;

        // Increment resistance (returns actual increase applied)
        var resistanceIncrease = influence.IncrementResistance();

        // Check for stall condition (high resistance)
        bool shouldStall = influence.ResistanceModifier >= StallResistanceThreshold &&
                          influence.Status == InfluenceStatus.Active;

        if (shouldStall)
        {
            influence.Stall(
                "I need time to think about this.",
                "Wait 24+ hours game time, or find compelling evidence.");

            _logger.LogInformation(
                "Influence stalled due to high resistance ({Resistance}). " +
                "Influence {InfluenceId}",
                influence.ResistanceModifier, influence.Id);
        }

        var narrative = GenerateFailureNarrative(
            influence.TargetConviction, resistanceIncrease,
            influence.Status == InfluenceStatus.Failed,
            shouldStall);

        _logger.LogDebug(
            "Failed influence: Resistance +{Increase} " +
            "(was {Previous}, now {Current}). Status: {Status}",
            resistanceIncrease, previousResistance,
            influence.ResistanceModifier, influence.Status);

        if (influence.Status == InfluenceStatus.Failed)
        {
            _logger.LogWarning(
                "Influence FAILED - Maximum resistance reached with insufficient progress. " +
                "Pool: {Pool}/{Threshold} ({Progress}%). Influence {InfluenceId}",
                influence.InfluencePool, influence.GetThreshold(),
                influence.ProgressPercentage, influence.Id);
        }

        var result = InfluenceAttemptResult.Failure(
            netSuccesses: netSuccesses,
            currentPool: influence.InfluencePool,
            threshold: influence.GetThreshold(),
            previousResistance: previousResistance,
            resistanceIncrease: resistanceIncrease,
            conviction: influence.TargetConviction,
            interactionNumber: interactionNumber,
            diceRolled: diceRolled,
            successesRolled: successesRolled,
            narrative: narrative,
            maxResistance: MaxResistance);

        // Override status if stalled
        if (shouldStall && result.Status == InfluenceStatus.Active)
        {
            return InfluenceAttemptResult.Stalled(
                currentPool: influence.InfluencePool,
                threshold: influence.GetThreshold(),
                resistance: influence.ResistanceModifier,
                conviction: influence.TargetConviction,
                interactionNumber: interactionNumber,
                stallReason: influence.StallReason!,
                resumeCondition: influence.ResumeCondition!,
                narrative: narrative);
        }

        return result;
    }

    #endregion

    #region Progress Retrieval Methods

    /// <inheritdoc/>
    public ExtendedInfluence? GetInfluenceProgress(Guid influenceId)
    {
        _logger.LogDebug("Getting influence progress for ID {InfluenceId}", influenceId);
        return _repository.GetById(influenceId);
    }

    /// <inheritdoc/>
    public ExtendedInfluence? GetInfluenceProgress(
        string characterId,
        string targetId,
        string beliefId)
    {
        _logger.LogDebug(
            "Getting influence progress for Character {CharacterId}, " +
            "Target {TargetId}, Belief {BeliefId}",
            characterId, targetId, beliefId);

        return _repository.GetByCharacterTargetAndBelief(characterId, targetId, beliefId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetInfluenceProgressByTarget(
        string characterId,
        string targetId)
    {
        _logger.LogDebug(
            "Getting all influences between Character {CharacterId} and Target {TargetId}",
            characterId, targetId);

        return _repository.GetByCharacterAndTarget(characterId, targetId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetActiveInfluences(string characterId)
    {
        _logger.LogDebug(
            "Getting all active influences for Character {CharacterId}",
            characterId);

        return _repository.GetActiveByCharacter(characterId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetInfluencesOnTarget(string targetId)
    {
        _logger.LogDebug(
            "Getting all influences targeting NPC {TargetId}",
            targetId);

        return _repository.GetByTarget(targetId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetSuccessfulInfluences(string characterId)
    {
        _logger.LogDebug(
            "Getting successful influences for Character {CharacterId}",
            characterId);

        return _repository.GetSuccessfulByCharacter(characterId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedInfluence> GetStalledInfluences(string characterId)
    {
        _logger.LogDebug(
            "Getting stalled influences for Character {CharacterId}",
            characterId);

        return _repository.GetStalledByCharacter(characterId);
    }

    #endregion

    #region State Management Methods

    /// <inheritdoc/>
    public bool ResumeInfluence(Guid influenceId, int resistanceReduction = DefaultResumeResistanceReduction)
    {
        var influence = _repository.GetById(influenceId);
        if (influence == null)
        {
            _logger.LogWarning(
                "Cannot resume influence - not found: {InfluenceId}",
                influenceId);
            return false;
        }

        if (influence.Status != InfluenceStatus.Stalled)
        {
            _logger.LogWarning(
                "Cannot resume influence - not stalled (Status: {Status}): {InfluenceId}",
                influence.Status, influenceId);
            return false;
        }

        _logger.LogInformation(
            "Resuming influence {InfluenceId}. Resistance reduction: {Reduction}",
            influenceId, resistanceReduction);

        influence.Resume(resistanceReduction);
        _repository.Save(influence);

        _logger.LogInformation(
            "Influence resumed: {InfluenceId}. New resistance: {Resistance}",
            influenceId, influence.ResistanceModifier);

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> ResumeInfluenceAsync(Guid influenceId, int resistanceReduction = DefaultResumeResistanceReduction)
    {
        return Task.FromResult(ResumeInfluence(influenceId, resistanceReduction));
    }

    /// <inheritdoc/>
    public bool StallInfluence(Guid influenceId, string stallReason, string resumeCondition)
    {
        var influence = _repository.GetById(influenceId);
        if (influence == null)
        {
            _logger.LogWarning(
                "Cannot stall influence - not found: {InfluenceId}",
                influenceId);
            return false;
        }

        if (influence.Status.IsTerminal())
        {
            _logger.LogWarning(
                "Cannot stall influence - terminal status ({Status}): {InfluenceId}",
                influence.Status, influenceId);
            return false;
        }

        _logger.LogInformation(
            "Stalling influence {InfluenceId}. Reason: {Reason}, Resume: {Condition}",
            influenceId, stallReason, resumeCondition);

        influence.Stall(stallReason, resumeCondition);
        _repository.Save(influence);

        return true;
    }

    /// <inheritdoc/>
    public bool FailInfluence(Guid influenceId, string? reason = null)
    {
        var influence = _repository.GetById(influenceId);
        if (influence == null)
        {
            _logger.LogWarning(
                "Cannot fail influence - not found: {InfluenceId}",
                influenceId);
            return false;
        }

        if (influence.Status.IsTerminal())
        {
            _logger.LogWarning(
                "Cannot fail influence - already terminal ({Status}): {InfluenceId}",
                influence.Status, influenceId);
            return false;
        }

        _logger.LogInformation(
            "Failing influence {InfluenceId}. Reason: {Reason}",
            influenceId, reason ?? "No reason specified");

        influence.Fail(reason);
        _repository.Save(influence);

        return true;
    }

    /// <inheritdoc/>
    public bool CheckConvictionThreshold(Guid influenceId)
    {
        var influence = _repository.GetById(influenceId);
        if (influence == null)
        {
            _logger.LogWarning(
                "Cannot check threshold - influence not found: {InfluenceId}",
                influenceId);
            return false;
        }

        return influence.IsThresholdReached();
    }

    #endregion

    #region Initialization Methods

    /// <inheritdoc/>
    public ExtendedInfluence InitializeInfluence(
        string characterId,
        string targetId,
        string targetName,
        string beliefId,
        string beliefDescription,
        ConvictionLevel conviction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(beliefId, nameof(beliefId));

        _logger.LogInformation(
            "Initializing influence: Character {CharacterId} targeting " +
            "{TargetName} ({TargetId}) on belief '{BeliefDescription}' " +
            "(Conviction: {Conviction})",
            characterId, targetName, targetId, beliefDescription, conviction);

        // Check if tracking already exists
        var existing = _repository.GetByCharacterTargetAndBelief(characterId, targetId, beliefId);
        if (existing != null)
        {
            _logger.LogDebug(
                "Influence tracking already exists: {InfluenceId}. Returning existing.",
                existing.Id);
            return existing;
        }

        // Create new influence tracking
        var influence = ExtendedInfluence.Create(
            characterId: characterId,
            targetId: targetId,
            targetName: targetName,
            beliefId: beliefId,
            beliefDescription: beliefDescription,
            conviction: conviction,
            maxResistance: MaxResistance);

        _repository.Add(influence);

        _logger.LogInformation(
            "Influence initialized: {InfluenceId}. " +
            "Target conviction: {Conviction} (DC {BaseDc}, Threshold {Threshold})",
            influence.Id, conviction.GetDisplayName(),
            conviction.GetBaseDc(), conviction.GetPoolThreshold());

        return influence;
    }

    /// <inheritdoc/>
    public ExtendedInfluence GetOrCreateInfluence(
        string characterId,
        string targetId,
        string targetName,
        string beliefId,
        string beliefDescription,
        ConvictionLevel conviction)
    {
        var existing = _repository.GetByCharacterTargetAndBelief(characterId, targetId, beliefId);
        if (existing != null)
        {
            _logger.LogDebug(
                "Found existing influence tracking: {InfluenceId}",
                existing.Id);
            return existing;
        }

        return InitializeInfluence(
            characterId, targetId, targetName,
            beliefId, beliefDescription, conviction);
    }

    #endregion

    #region Utility Methods

    /// <inheritdoc/>
    public IReadOnlyList<string> GetTacticalAdvice(Guid influenceId)
    {
        var influence = _repository.GetById(influenceId);
        if (influence == null)
        {
            return Array.Empty<string>();
        }

        var advice = new List<string>();
        var conviction = influence.TargetConviction;
        var progress = influence.ProgressPercentage;
        var resistance = influence.ResistanceModifier;
        var effectiveDc = influence.GetEffectiveDc();

        // Progress-based advice
        if (progress >= 80)
        {
            advice.Add("You're very close to changing their belief. One or two more successful conversations should do it.");
        }
        else if (progress >= 50)
        {
            advice.Add("You've made significant progress. Their conviction is weakening.");
        }
        else if (progress < 25 && influence.InteractionCount > 3)
        {
            advice.Add("Progress is slow. Consider finding evidence or waiting for a better moment.");
        }

        // Resistance-based advice
        if (resistance >= 4)
        {
            advice.Add($"High resistance ({resistance}). The effective DC is now {effectiveDc}. " +
                "Consider waiting or finding external support.");
        }
        else if (resistance >= 2)
        {
            advice.Add($"Building resistance ({resistance}). Be careful with further failures.");
        }

        // Conviction-level advice
        if (conviction == ConvictionLevel.Fanatical)
        {
            advice.Add("This is a fanatical belief. A life-changing event may be required before they can truly change.");
        }
        else if (conviction >= ConvictionLevel.CoreBelief)
        {
            advice.Add($"This is a {conviction.GetDisplayName().ToLowerInvariant()}. " +
                "Prepare for a long campaign and consider supporting evidence.");
        }

        // Estimated attempts
        var estimatedRemaining = influence.GetEstimatedAttemptsRemaining();
        if (estimatedRemaining > 0)
        {
            advice.Add($"Estimated {estimatedRemaining} more successful attempts needed " +
                "(assuming average performance).");
        }

        return advice;
    }

    /// <inheritdoc/>
    public string? GetInfluenceSummary(Guid influenceId)
    {
        var influence = _repository.GetById(influenceId);
        return influence?.GetStateSummary();
    }

    /// <inheritdoc/>
    public int CalculateEffectiveDc(ConvictionLevel conviction, int currentResistance)
    {
        return conviction.GetBaseDc() + currentResistance;
    }

    /// <inheritdoc/>
    public (int integerIncrease, decimal newFractional) CalculateResistanceIncrease(
        ConvictionLevel conviction,
        decimal accumulatedFractional)
    {
        var rate = conviction.GetResistancePerFailure();
        if (rate == 0)
        {
            return (0, 0m);
        }

        var newFractional = accumulatedFractional + rate;
        var integerIncrease = (int)newFractional;
        newFractional -= integerIncrease;

        return (integerIncrease, newFractional);
    }

    /// <inheritdoc/>
    public bool RemoveInfluence(Guid influenceId)
    {
        _logger.LogInformation("Removing influence tracking: {InfluenceId}", influenceId);
        return _repository.Delete(influenceId);
    }

    /// <inheritdoc/>
    public InfluenceStatistics GetStatistics(string characterId)
    {
        var all = _repository.GetByCharacter(characterId);

        var totalStarted = all.Count;
        var successful = all.Where(i => i.Status == InfluenceStatus.Successful).ToList();
        var totalSuccessful = successful.Count;
        var totalFailed = all.Count(i => i.Status == InfluenceStatus.Failed);
        var currentlyActive = all.Count(i => i.Status == InfluenceStatus.Active);
        var currentlyStalled = all.Count(i => i.Status == InfluenceStatus.Stalled);
        var totalInteractions = all.Sum(i => i.InteractionCount);

        var avgInteractions = totalSuccessful > 0
            ? (decimal)successful.Sum(i => i.InteractionCount) / totalSuccessful
            : 0m;

        _logger.LogDebug(
            "Statistics for Character {CharacterId}: " +
            "Started {Started}, Successful {Successful}, Failed {Failed}, " +
            "Active {Active}, Stalled {Stalled}",
            characterId, totalStarted, totalSuccessful, totalFailed,
            currentlyActive, currentlyStalled);

        return new InfluenceStatistics(
            TotalStarted: totalStarted,
            TotalSuccessful: totalSuccessful,
            TotalFailed: totalFailed,
            CurrentlyActive: currentlyActive,
            CurrentlyStalled: currentlyStalled,
            AverageInteractionsToSuccess: avgInteractions,
            TotalInteractions: totalInteractions);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Rolls dice and counts successes (6+ on d10).
    /// </summary>
    /// <param name="diceCount">Number of dice to roll.</param>
    /// <returns>The number of successes.</returns>
    private int RollSuccesses(int diceCount)
    {
        if (diceCount <= 0)
        {
            return 0;
        }

        var successes = 0;
        for (var i = 0; i < diceCount; i++)
        {
            var roll = _random.Next(1, 11); // 1-10
            if (roll >= 6)
            {
                successes++;
            }
        }

        return successes;
    }

    /// <summary>
    /// Generates narrative text for a successful influence attempt.
    /// </summary>
    private static string GenerateSuccessNarrative(
        ConvictionLevel conviction,
        int poolGain,
        bool isBreakthrough,
        bool thresholdReached)
    {
        if (thresholdReached)
        {
            return conviction switch
            {
                ConvictionLevel.WeakOpinion =>
                    "They nod thoughtfully. \"You know, you might be right about that.\"",
                ConvictionLevel.ModerateBelief =>
                    "Their expression shifts. \"I... hadn't considered it that way before.\"",
                ConvictionLevel.StrongConviction =>
                    "After a long pause, they let out a breath. \"Perhaps I've been wrong about this.\"",
                ConvictionLevel.CoreBelief =>
                    "Something fundamental shifts behind their eyes. \"Everything I believed... I need to rethink everything.\"",
                ConvictionLevel.Fanatical =>
                    "They stare at you, shaken to their core. \"Who am I, if not this?\"",
                _ => "Their belief has changed."
            };
        }

        if (isBreakthrough)
        {
            return "Your words strike deep. They pause, visibly reconsidering their position.";
        }

        return poolGain switch
        {
            >= 2 => "They listen intently, nodding at several points. Progress.",
            1 => "A flicker of doubt crosses their face. A small victory.",
            _ => "They hear you out without dismissing your words."
        };
    }

    /// <summary>
    /// Generates narrative text for a failed influence attempt.
    /// </summary>
    private static string GenerateFailureNarrative(
        ConvictionLevel conviction,
        int resistanceIncrease,
        bool permanentFailure,
        bool stalled)
    {
        if (permanentFailure)
        {
            return "Their jaw sets with finality. \"I will not discuss this further. Ever.\"";
        }

        if (stalled)
        {
            return "They raise a hand. \"I need time to think. Leave me be for now.\"";
        }

        if (resistanceIncrease > 0)
        {
            return conviction switch
            {
                >= ConvictionLevel.CoreBelief =>
                    "Their expression hardens. You've only strengthened their resolve.",
                _ => "They shake their head. \"I'm not convinced.\" They seem more guarded now."
            };
        }

        return "They listen but remain unconvinced. \"That's not how I see it.\"";
    }

    #endregion
}
