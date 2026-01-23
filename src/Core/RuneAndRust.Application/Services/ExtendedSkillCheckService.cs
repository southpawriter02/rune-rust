using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Handles cooperative and chained skill checks.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates complex skill check scenarios:
/// </para>
/// <list type="bullet">
///   <item><description>Cooperative: Multiple participants with various combination methods</description></item>
///   <item><description>Chained: Sequential multi-step procedures with retry logic</description></item>
/// </list>
/// <para>
/// v0.15.1d: Initial implementation supporting 4 cooperation types and multi-step chains.
/// </para>
/// </remarks>
public sealed class ExtendedSkillCheckService : IExtendedSkillCheckService
{
    private readonly SkillCheckService _skillCheckService;
    private readonly IChainedCheckRepository _chainRepository;
    private readonly ILogger<ExtendedSkillCheckService> _logger;

    /// <summary>
    /// Creates a new extended skill check service.
    /// </summary>
    /// <param name="skillCheckService">The core skill check service.</param>
    /// <param name="chainRepository">Repository for chained check persistence.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public ExtendedSkillCheckService(
        SkillCheckService skillCheckService,
        IChainedCheckRepository chainRepository,
        ILogger<ExtendedSkillCheckService> logger)
    {
        _skillCheckService = skillCheckService;
        _chainRepository = chainRepository;
        _logger = logger;

        _logger.LogDebug("ExtendedSkillCheckService initialized");
    }

    #region Cooperative Checks

    /// <inheritdoc />
    public CooperativeCheckResult ResolveCooperativeCheck(
        IReadOnlyList<Player> participants,
        string skillId,
        int difficultyClass,
        CooperationType cooperationType,
        string? subType = null,
        SkillContext? context = null)
    {
        var participantsWithContext = participants
            .Select(p => (Participant: p, Context: context))
            .ToList();

        return ResolveCooperativeCheckWithContexts(
            participantsWithContext, skillId, difficultyClass, cooperationType, subType);
    }

    /// <inheritdoc />
    public CooperativeCheckResult ResolveCooperativeCheckWithContexts(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        CooperationType cooperationType,
        string? subType = null)
    {
        if (participants.Count == 0)
            throw new ArgumentException("At least one participant is required.", nameof(participants));

        _logger.LogDebug(
            "Resolving {CooperationType} check for {ParticipantCount} participants, skill {SkillId} DC {DC}",
            cooperationType, participants.Count, skillId, difficultyClass);

        return cooperationType switch
        {
            CooperationType.WeakestLink => ResolveWeakestLink(participants, skillId, difficultyClass, subType),
            CooperationType.BestAttempt => ResolveBestAttempt(participants, skillId, difficultyClass, subType),
            CooperationType.Combined => ResolveCombined(participants, skillId, difficultyClass, subType),
            CooperationType.Assisted => ResolveAssisted(participants, skillId, difficultyClass, subType),
            _ => throw new ArgumentOutOfRangeException(nameof(cooperationType))
        };
    }

    /// <summary>
    /// Resolves a WeakestLink cooperative check - lowest pool makes the roll.
    /// </summary>
    private CooperativeCheckResult ResolveWeakestLink(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        string? subType)
    {
        // Find participant with lowest dice pool
        var poolsWithPlayers = new List<(Player Player, int Pool, SkillContext? Context)>();

        foreach (var (player, context) in participants)
        {
            var pool = CalculateDicePool(player, skillId, context);
            poolsWithPlayers.Add((player, pool, context));
        }

        var weakest = poolsWithPlayers.MinBy(p => p.Pool)!;

        _logger.LogDebug(
            "WeakestLink: {PlayerId} has lowest pool ({Pool}d10)",
            weakest.Player.Id, weakest.Pool);

        // Perform check for weakest participant
        var result = PerformCheck(weakest.Player, skillId, difficultyClass, weakest.Context);

        _logger.LogInformation(
            "WeakestLink cooperative check: {Outcome} ({NetSuccesses} vs DC {DC})",
            result.Outcome, result.NetSuccesses, difficultyClass);

        return new CooperativeCheckResult(
            CooperationType: CooperationType.WeakestLink,
            ParticipantIds: participants.Select(p => p.Participant.Id.ToString()).ToList(),
            SkillId: skillId,
            SubType: subType,
            DifficultyClass: difficultyClass,
            FinalOutcome: result.Outcome,
            FinalNetSuccesses: result.NetSuccesses,
            ActiveRollerId: weakest.Player.Id.ToString(),
            IndividualResults: new[] { result },
            ContributingParticipants: new[] { weakest.Player.Id.ToString() });
    }

    /// <summary>
    /// Resolves a BestAttempt cooperative check - everyone rolls, best wins.
    /// </summary>
    private CooperativeCheckResult ResolveBestAttempt(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        string? subType)
    {
        var results = new List<(Player Player, SkillCheckResult Result)>();

        foreach (var (player, context) in participants)
        {
            var result = PerformCheck(player, skillId, difficultyClass, context);
            results.Add((player, result));

            _logger.LogDebug(
                "BestAttempt: {PlayerId} rolled {NetSuccesses} net",
                player.Id, result.NetSuccesses);
        }

        // Find best result by net successes
        var best = results.MaxBy(r => r.Result.NetSuccesses)!;

        _logger.LogInformation(
            "BestAttempt cooperative check: {BestPlayerId} won with {NetSuccesses} net ({Outcome})",
            best.Player.Id, best.Result.NetSuccesses, best.Result.Outcome);

        return new CooperativeCheckResult(
            CooperationType: CooperationType.BestAttempt,
            ParticipantIds: participants.Select(p => p.Participant.Id.ToString()).ToList(),
            SkillId: skillId,
            SubType: subType,
            DifficultyClass: difficultyClass,
            FinalOutcome: best.Result.Outcome,
            FinalNetSuccesses: best.Result.NetSuccesses,
            ActiveRollerId: best.Player.Id.ToString(),
            IndividualResults: results.Select(r => r.Result).ToList(),
            ContributingParticipants: new[] { best.Player.Id.ToString() });
    }

    /// <summary>
    /// Resolves a Combined cooperative check - sum all net successes.
    /// </summary>
    private CooperativeCheckResult ResolveCombined(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        string? subType)
    {
        var results = new List<(Player Player, SkillCheckResult Result)>();
        var totalNetSuccesses = 0;
        var contributors = new List<string>();

        foreach (var (player, context) in participants)
        {
            var result = PerformCheck(player, skillId, difficultyClass, context);
            results.Add((player, result));

            var net = result.NetSuccesses;
            totalNetSuccesses += net;

            if (net > 0)
                contributors.Add(player.Id.ToString());

            _logger.LogDebug(
                "Combined: {PlayerId} contributed {NetSuccesses} net",
                player.Id, net);
        }

        // Classify combined result
        var finalOutcome = ClassifyCombinedOutcome(totalNetSuccesses, difficultyClass);

        _logger.LogInformation(
            "Combined cooperative check: {TotalNet} total net from {Contributors} contributors ({Outcome})",
            totalNetSuccesses, contributors.Count, finalOutcome);

        return new CooperativeCheckResult(
            CooperationType: CooperationType.Combined,
            ParticipantIds: participants.Select(p => p.Participant.Id.ToString()).ToList(),
            SkillId: skillId,
            SubType: subType,
            DifficultyClass: difficultyClass,
            FinalOutcome: finalOutcome,
            FinalNetSuccesses: totalNetSuccesses,
            IndividualResults: results.Select(r => r.Result).ToList(),
            ContributingParticipants: contributors);
    }

    /// <summary>
    /// Resolves an Assisted cooperative check - primary gets bonus from helpers.
    /// </summary>
    private CooperativeCheckResult ResolveAssisted(
        IReadOnlyList<(Player Participant, SkillContext? Context)> participants,
        string skillId,
        int difficultyClass,
        string? subType)
    {
        if (participants.Count < 1)
            throw new ArgumentException("Assisted check requires at least one participant.", nameof(participants));

        var (primaryPlayer, primaryContext) = participants[0];

        var helpers = participants.Skip(1).ToList();
        var helperContributions = new List<HelperContribution>();
        var bonusDice = 0;

        // Roll for each helper to determine if they grant bonus
        foreach (var (helper, helperContext) in helpers)
        {
            // Helper makes a check - looking for 2+ net to grant bonus
            var helperResult = PerformCheck(helper, skillId, 0, helperContext);

            var helperNet = helperResult.NetSuccesses;
            var grantsBonus = helperNet >= 2;

            helperContributions.Add(new HelperContribution(helper.Id.ToString(), helperNet, grantsBonus));

            if (grantsBonus)
            {
                bonusDice++;
                _logger.LogDebug(
                    "Assisted: Helper {HelperId} grants +1d10 ({NetSuccesses} net)",
                    helper.Id, helperNet);
            }
            else
            {
                _logger.LogDebug(
                    "Assisted: Helper {HelperId} does not grant bonus ({NetSuccesses} net < 2)",
                    helper.Id, helperNet);
            }
        }

        _logger.LogDebug(
            "Assisted: {BonusDice} bonus dice from {HelperCount} helpers",
            bonusDice, helpers.Count);

        // Build context with helper bonus dice using SituationalModifier.Assisted factory
        var assistedContext = primaryContext ?? SkillContext.Empty;
        if (bonusDice > 0)
        {
            var situationalMods = assistedContext.SituationalModifiers.ToList();
            situationalMods.Add(SituationalModifier.Assisted(bonusDice, "party members"));

            assistedContext = new SkillContext(
                assistedContext.EquipmentModifiers,
                situationalMods,
                assistedContext.EnvironmentModifiers,
                assistedContext.TargetModifiers,
                assistedContext.AppliedStatuses);
        }

        // Primary makes the check with helper bonus
        var primaryResult = PerformCheck(primaryPlayer, skillId, difficultyClass, assistedContext);

        _logger.LogInformation(
            "Assisted cooperative check: {PrimaryId} rolled {NetSuccesses} net with +{BonusDice} helper dice ({Outcome})",
            primaryPlayer.Id, primaryResult.NetSuccesses, bonusDice, primaryResult.Outcome);

        return new CooperativeCheckResult(
            CooperationType: CooperationType.Assisted,
            ParticipantIds: participants.Select(p => p.Participant.Id.ToString()).ToList(),
            SkillId: skillId,
            SubType: subType,
            DifficultyClass: difficultyClass,
            FinalOutcome: primaryResult.Outcome,
            FinalNetSuccesses: primaryResult.NetSuccesses,
            ActiveRollerId: primaryPlayer.Id.ToString(),
            IndividualResults: new[] { primaryResult },
            HelperBonuses: helperContributions,
            ContributingParticipants: new[] { primaryPlayer.Id.ToString() }
                .Concat(helperContributions.Where(h => h.GrantedBonus).Select(h => h.HelperId))
                .ToList());
    }

    /// <summary>
    /// Calculates the dice pool for a player and skill.
    /// </summary>
    private static int CalculateDicePool(Player player, string skillId, SkillContext? context)
    {
        // Get skill proficiency level (0-5 for Untrained-Master)
        var proficiency = player.GetSkillProficiency(skillId);
        var proficiencyDice = (int)proficiency;

        // Get base attribute modifier from Wits (general competence indicator)
        var stats = player.Stats;
        var attributeDice = stats.Wits / 2;  // Wits/2 as attribute contribution

        var basePool = proficiencyDice + attributeDice;

        // Add context modifiers
        var contextBonus = context?.TotalDiceModifier ?? 0;

        return Math.Max(1, basePool + contextBonus);
    }

    /// <summary>
    /// Classifies a combined outcome based on total net successes vs DC.
    /// </summary>
    private static SkillOutcome ClassifyCombinedOutcome(int totalNet, int dc)
    {
        var margin = totalNet - dc;

        return margin switch
        {
            < 0 when totalNet == 0 => SkillOutcome.CriticalFailure,
            < 0 => SkillOutcome.Failure,
            0 => SkillOutcome.MarginalSuccess,
            1 or 2 => SkillOutcome.FullSuccess,
            3 or 4 => SkillOutcome.ExceptionalSuccess,
            >= 5 => SkillOutcome.CriticalSuccess
        };
    }

    #endregion

    #region Chained Checks

    /// <inheritdoc />
    public ChainedCheckState StartChainedCheck(
        Player player,
        string chainName,
        IReadOnlyList<ChainedCheckStep> steps,
        string? targetId = null)
    {
        var checkId = $"chain-{Guid.NewGuid():N}";

        var state = ChainedCheckState.Create(
            checkId, player.Id.ToString(), chainName, steps, targetId);

        _chainRepository.Add(state);

        _logger.LogInformation(
            "Started chained check {CheckId} '{ChainName}' for player {PlayerId} with {StepCount} steps",
            checkId, chainName, player.Id, steps.Count);

        foreach (var step in steps)
        {
            _logger.LogDebug(
                "  Step {StepId}: {StepName} - {SkillId} DC {DC} ({Retries} retries)",
                step.StepId, step.Name, step.SkillId, step.DifficultyClass, step.MaxRetries);
        }

        return state;
    }

    /// <inheritdoc />
    public ChainedCheckProcessResult ProcessChainStep(
        Player player,
        string checkId,
        SkillContext? stepContext = null)
    {
        var state = _chainRepository.GetById(checkId)
            ?? throw new InvalidOperationException($"Chain {checkId} not found.");

        if (state.Status == ChainedCheckStatus.Succeeded ||
            state.Status == ChainedCheckStatus.Failed)
        {
            throw new InvalidOperationException($"Chain {checkId} is already complete.");
        }

        if (state.Status == ChainedCheckStatus.AwaitingRetry)
        {
            throw new InvalidOperationException(
                $"Chain {checkId} is awaiting retry decision. Use RetryChainStep or AbandonChain.");
        }

        var step = state.GetCurrentStep();
        if (step == null)
            throw new InvalidOperationException($"Chain {checkId} has no remaining steps.");

        _logger.LogDebug(
            "Processing chain {CheckId} step {StepIndex}: {StepName} ({SkillId} DC {DC})",
            checkId, state.CurrentStepIndex, step.Value.Name, step.Value.SkillId, step.Value.DifficultyClass);

        // Determine context (step context, override, or empty)
        var context = stepContext ?? step.Value.Context ?? SkillContext.Empty;

        // Perform the step check
        var result = PerformCheck(player, step.Value.SkillId, step.Value.DifficultyClass, context);

        // Record result and update state
        state.RecordStepResult(result, wasRetry: false);
        _chainRepository.Update(state);

        // Build message
        var message = BuildStepMessage(state, step.Value, result);

        _logger.LogDebug(
            "Chain {CheckId} step {StepIndex} ({StepName}): {Outcome} - {Message}",
            checkId, state.CurrentStepIndex, step.Value.Name, result.Outcome, message);

        if (state.IsComplete)
        {
            _logger.LogInformation(
                "Chain {CheckId} completed with status {Status}",
                checkId, state.Status);
        }

        return new ChainedCheckProcessResult(
            State: state,
            StepResult: result,
            IsChainComplete: state.IsComplete,
            Message: message);
    }

    /// <inheritdoc />
    public ChainedCheckProcessResult RetryChainStep(
        Player player,
        string checkId,
        SkillContext? stepContext = null)
    {
        var state = _chainRepository.GetById(checkId)
            ?? throw new InvalidOperationException($"Chain {checkId} not found.");

        if (!state.CanRetry())
        {
            throw new InvalidOperationException($"Chain {checkId} cannot be retried.");
        }

        var step = state.GetCurrentStep();
        if (step == null)
            throw new InvalidOperationException($"Chain {checkId} has no current step.");

        _logger.LogDebug(
            "Retrying chain {CheckId} step {StepIndex}: {StepName} (retries remaining: {Retries})",
            checkId, state.CurrentStepIndex, step.Value.Name, state.RetriesRemaining[state.CurrentStepIndex]);

        var context = stepContext ?? step.Value.Context ?? SkillContext.Empty;

        var result = PerformCheck(player, step.Value.SkillId, step.Value.DifficultyClass, context);

        state.RecordStepResult(result, wasRetry: true);
        _chainRepository.Update(state);

        var message = BuildStepMessage(state, step.Value, result);

        _logger.LogDebug(
            "Chain {CheckId} step {StepIndex} ({StepName}) RETRY: {Outcome} - {Message}",
            checkId, state.CurrentStepIndex, step.Value.Name, result.Outcome, message);

        return new ChainedCheckProcessResult(
            State: state,
            StepResult: result,
            IsChainComplete: state.IsComplete,
            Message: message);
    }

    /// <inheritdoc />
    public ChainedCheckState AbandonChain(string checkId)
    {
        var state = _chainRepository.GetById(checkId)
            ?? throw new InvalidOperationException($"Chain {checkId} not found.");

        state.Abandon();
        _chainRepository.Update(state);

        _logger.LogInformation("Chain {CheckId} '{ChainName}' abandoned by player", checkId, state.ChainName);

        return state;
    }

    /// <inheritdoc />
    public ChainedCheckState? GetChainState(string checkId)
    {
        return _chainRepository.GetById(checkId);
    }

    /// <inheritdoc />
    public IReadOnlyList<ChainedCheckState> GetActiveChainsForCharacter(string characterId)
    {
        return _chainRepository.GetActiveByCharacterId(characterId);
    }

    /// <summary>
    /// Builds a display message for a step result.
    /// </summary>
    private static string BuildStepMessage(
        ChainedCheckState state,
        ChainedCheckStep step,
        SkillCheckResult result)
    {
        if (result.Outcome >= SkillOutcome.MarginalSuccess)
        {
            if (state.Status == ChainedCheckStatus.Succeeded)
            {
                return step.SuccessMessage ?? $"{step.Name} succeeded. {state.ChainName} complete!";
            }
            return step.SuccessMessage ?? $"{step.Name} succeeded. Proceed to next step.";
        }

        if (state.CanRetry())
        {
            return step.FailureMessage ??
                $"{step.Name} failed. {state.RetriesRemaining[state.CurrentStepIndex]} retry(ies) remaining.";
        }

        return step.FailureMessage ?? $"{step.Name} failed. {state.ChainName} cannot continue.";
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Performs a skill check using the underlying service.
    /// </summary>
    private SkillCheckResult PerformCheck(
        Player player,
        string skillId,
        int difficultyClass,
        SkillContext? context)
    {
        if (context != null && context != SkillContext.Empty)
        {
            return _skillCheckService.PerformCheckWithContext(
                player, skillId, difficultyClass, "Custom", context);
        }
        else
        {
            return _skillCheckService.PerformCheckWithDC(
                player, skillId, difficultyClass);
        }
    }

    #endregion
}
