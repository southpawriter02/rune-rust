// ------------------------------------------------------------------------------
// <copyright file="InterrogationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of the interrogation service with multi-round resistance
// depletion, method-based reliability, and comprehensive consequence tracking.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implementation of the interrogation service with multi-round resistance
/// depletion, method-based reliability, and comprehensive consequence tracking.
/// </summary>
/// <remarks>
/// <para>
/// This service orchestrates the complete interrogation workflow including:
/// </para>
/// <list type="bullet">
///   <item><description>Session initialization with WILL-based resistance calculation</description></item>
///   <item><description>Round-by-round method selection and skill check resolution</description></item>
///   <item><description>Resistance depletion on successful checks</description></item>
///   <item><description>Information extraction with reliability-based accuracy</description></item>
///   <item><description>Side effect tracking (disposition, reputation, resources)</description></item>
///   <item><description>Fumble consequence handling including [Subject Broken]</description></item>
/// </list>
/// <para>
/// The service supports five interrogation methods, each with distinct
/// characteristics affecting DC, reliability, duration, and side effects.
/// Using Torture at any point caps information reliability at 60%.
/// </para>
/// </remarks>
public class InterrogationService : IInterrogationService
{
    /// <summary>
    /// The logger instance for comprehensive logging of interrogation operations.
    /// </summary>
    private readonly ILogger<InterrogationService> _logger;

    /// <summary>
    /// Random number generator for dice rolls and reliability checks.
    /// </summary>
    private readonly Random _random;

    /// <summary>
    /// In-memory tracking of active interrogation sessions.
    /// In full implementation, this would be persisted.
    /// </summary>
    private readonly Dictionary<string, InterrogationState> _activeInterrogations = new();

    /// <summary>
    /// Maximum rounds allowed per resistance level.
    /// </summary>
    private static readonly Dictionary<SubjectResistance, int> MaxRoundsByResistance = new()
    {
        [SubjectResistance.Minimal] = 3,
        [SubjectResistance.Low] = 6,
        [SubjectResistance.Moderate] = 10,
        [SubjectResistance.High] = 15,
        [SubjectResistance.Extreme] = 20
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InterrogationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for comprehensive logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public InterrogationService(ILogger<InterrogationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    #region Initialization Methods

    /// <inheritdoc/>
    public InterrogationState InitializeInterrogation(
        string interrogatorId,
        string subjectId,
        int subjectWill,
        int resistanceModifiers = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(interrogatorId, nameof(interrogatorId));
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId, nameof(subjectId));

        _logger.LogInformation(
            "Initializing interrogation: Interrogator {InterrogatorId} vs Subject {SubjectId} " +
            "(WILL {SubjectWill}, modifiers {Modifiers})",
            interrogatorId, subjectId, subjectWill, resistanceModifiers);

        // Assess resistance level based on WILL and modifiers
        var resistanceLevel = AssessResistance(subjectWill, resistanceModifiers);

        // Calculate initial resistance (checks required to break)
        var initialResistance = CalculateInitialResistance(resistanceLevel, subjectWill, resistanceModifiers);

        _logger.LogDebug(
            "Resistance assessment: Level {Level} ({DisplayName}), Initial checks to break: {Resistance}",
            resistanceLevel, resistanceLevel.GetDisplayName(), initialResistance);

        // Create the interrogation state
        var state = new InterrogationState
        {
            InterrogationId = Guid.NewGuid().ToString("N"),
            InterrogatorId = interrogatorId,
            SubjectId = subjectId,
            SubjectWill = subjectWill,
            InitialResistance = initialResistance,
            ResistanceLevel = resistanceLevel
        };

        // Initialize the state (sets ResistanceRemaining, clears history)
        state.Initialize();

        // Track the active interrogation
        _activeInterrogations[state.InterrogationId] = state;

        _logger.LogInformation(
            "Interrogation {InterrogationId} initialized: Subject has {Resistance} resistance " +
            "({Level}). Max rounds: {MaxRounds}",
            state.InterrogationId, state.ResistanceRemaining,
            resistanceLevel.GetDisplayName(), GetMaxRounds(resistanceLevel));

        return state;
    }

    /// <inheritdoc/>
    public Task<InterrogationState> InitializeInterrogationAsync(
        string interrogatorId,
        string subjectId,
        int subjectWill,
        int resistanceModifiers = 0)
    {
        _logger.LogDebug(
            "InitializeInterrogationAsync called for Subject {SubjectId}",
            subjectId);

        var state = InitializeInterrogation(interrogatorId, subjectId, subjectWill, resistanceModifiers);
        return Task.FromResult(state);
    }

    /// <inheritdoc/>
    public SubjectResistance AssessResistance(int subjectWill, int resistanceModifiers = 0)
    {
        // Modifiers contribute at half value to effective WILL
        var effectiveWill = subjectWill + (resistanceModifiers / 2);

        var resistance = SubjectResistanceExtensions.FromWillAttribute(effectiveWill);

        _logger.LogDebug(
            "Resistance assessment: WILL {SubjectWill} + modifiers {Modifiers}/2 = effective {EffectiveWill} -> {Level}",
            subjectWill, resistanceModifiers, effectiveWill, resistance.GetDisplayName());

        return resistance;
    }

    #endregion

    #region Round Execution Methods

    /// <inheritdoc/>
    public InterrogationRound ConductRound(InterrogationContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var state = context.InterrogationState;
        var method = context.SelectedMethod;

        _logger.LogInformation(
            "Conducting round {RoundNumber} of interrogation {InterrogationId}: " +
            "Method {Method}, Subject resistance {Resistance}/{InitialResistance}",
            state.RoundNumber + 1, state.InterrogationId,
            method.GetDisplayName(), state.ResistanceRemaining, state.InitialResistance);

        // Validate interrogation state
        if (state.Status.IsTerminal())
        {
            _logger.LogWarning(
                "Cannot conduct round on terminal interrogation {InterrogationId} (Status: {Status})",
                state.InterrogationId, state.Status.GetDisplayName());
            throw new InvalidOperationException(
                $"Cannot conduct rounds on a terminal interrogation. Current status: {state.Status.GetDisplayName()}");
        }

        // Check for max rounds exceeded
        var maxRounds = GetMaxRounds(state.ResistanceLevel);
        if (state.RoundNumber >= maxRounds)
        {
            _logger.LogInformation(
                "Max rounds ({MaxRounds}) reached for interrogation {InterrogationId}. Subject resisting.",
                maxRounds, state.InterrogationId);
            state.MarkSubjectResisting();
            throw new InvalidOperationException(
                $"Maximum rounds ({maxRounds}) reached. Subject has successfully resisted.");
        }

        // Calculate DC and dice pool
        var effectiveDc = context.CalculateEffectiveDc();
        var dicePool = context.CalculateDicePool();

        _logger.LogDebug(
            "Round parameters: DC {Dc}, Dice pool {Pool}d10, Method {Method}",
            effectiveDc, dicePool, method.GetDisplayName());

        // Perform the dice roll
        var (successes, botches) = SimulateRoll(dicePool);
        var successesRequired = CalculateSuccessesRequired(effectiveDc);

        _logger.LogDebug(
            "Roll result: {Successes} successes, {Botches} botches vs {Required} required (DC {Dc})",
            successes, botches, successesRequired, effectiveDc);

        // Check for fumble (0 successes + 1+ botches)
        var isFumble = successes == 0 && botches >= 1;
        var isSuccess = !isFumble && successes >= successesRequired;

        // Determine skill outcome
        var outcome = DetermineOutcome(successes, successesRequired, isFumble);

        _logger.LogDebug(
            "Outcome determination: Success={IsSuccess}, Fumble={IsFumble}, Outcome={Outcome}",
            isSuccess, isFumble, outcome);

        // Calculate resistance change (success reduces by 1)
        var resistanceChange = isSuccess ? -1 : 0;

        // Calculate side effects
        var dispositionChange = method.GetDispositionChangePerRound();
        var resourceCost = method.RequiresResources()
            ? CalculateBriberyCost(state.ResistanceLevel)
            : 0;

        // Handle fumble type
        FumbleType? fumbleType = null;
        if (isFumble)
        {
            fumbleType = method.GetFumbleType();
            _logger.LogWarning(
                "Fumble occurred during {Method}! Fumble type: {FumbleType}",
                method.GetDisplayName(), fumbleType.Value.GetDisplayName());
        }

        // Generate narrative description
        var narrative = GenerateRoundNarrative(method, isSuccess, isFumble);

        // Create the round record
        var round = new InterrogationRound
        {
            RoundNumber = state.RoundNumber + 1,
            MethodUsed = method,
            CheckResult = outcome,
            DiceRolled = dicePool,
            SuccessesAchieved = successes,
            SuccessesRequired = successesRequired,
            ResistanceChange = resistanceChange,
            ResistanceAfter = Math.Max(0, state.ResistanceRemaining + resistanceChange),
            DispositionChange = dispositionChange,
            ResourceCost = resourceCost,
            TimeElapsedMinutes = method.GetRoundDurationMinutes(),
            IsFumble = isFumble,
            FumbleType = fumbleType,
            NarrativeDescription = narrative
        };

        // Record the round in state
        state.RecordRound(round);

        _logger.LogInformation(
            "Round {RoundNumber} complete: {Outcome}, Resistance now {Resistance}/{InitialResistance}, " +
            "Disposition change: {DispositionChange}, Resource cost: {ResourceCost}",
            round.RoundNumber, outcome, state.ResistanceRemaining, state.InitialResistance,
            dispositionChange, resourceCost);

        // Handle Subject Broken fumble for Torture
        if (isFumble && fumbleType == FumbleType.SubjectBroken)
        {
            _logger.LogError(
                "SUBJECT BROKEN! Torture fumble in interrogation {InterrogationId}. " +
                "Subject is permanently incapacitated. No information can be extracted.",
                state.InterrogationId);
            state.MarkSubjectBrokenBeyondRecovery();
        }

        // Log torture warning
        if (method == InterrogationMethod.Torture)
        {
            _logger.LogWarning(
                "Torture used in interrogation {InterrogationId}. " +
                "Reliability is now capped at 60% regardless of other methods used.",
                state.InterrogationId);
        }

        return round;
    }

    /// <inheritdoc/>
    public Task<InterrogationRound> ConductRoundAsync(InterrogationContext context)
    {
        _logger.LogDebug(
            "ConductRoundAsync called for interrogation {InterrogationId}",
            context.InterrogationState.InterrogationId);

        var round = ConductRound(context);
        return Task.FromResult(round);
    }

    #endregion

    #region Cost Calculation Methods

    /// <inheritdoc/>
    public int CalculateBriberyCost(SubjectResistance resistanceLevel)
    {
        var baseCost = resistanceLevel.GetBaseBriberyCost();

        // Add ±20% variance for game variety
        var variance = (int)(baseCost * 0.2);
        var actualCost = baseCost + _random.Next(-variance, variance + 1);

        _logger.LogDebug(
            "Bribery cost calculated: Base {BaseCost} ± {Variance} = {ActualCost} gold for {Level}",
            baseCost, variance, actualCost, resistanceLevel.GetDisplayName());

        return actualCost;
    }

    /// <inheritdoc/>
    public int CalculateTortureDc(int subjectWill, int modifiers = 0)
    {
        var dc = (subjectWill * 2) + modifiers;

        _logger.LogDebug(
            "Torture DC calculated: (WILL {SubjectWill} × 2) + modifiers {Modifiers} = DC {Dc}",
            subjectWill, modifiers, dc);

        return dc;
    }

    #endregion

    #region Information Extraction Methods

    /// <inheritdoc/>
    public IReadOnlyList<InformationGained> ExtractInformation(
        InterrogationState state,
        IReadOnlyList<string> topics)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(topics, nameof(topics));

        if (state.Status != InterrogationStatus.SubjectBroken)
        {
            _logger.LogWarning(
                "Cannot extract information from interrogation {InterrogationId}: Status is {Status}, not SubjectBroken",
                state.InterrogationId, state.Status.GetDisplayName());
            throw new InvalidOperationException(
                $"Cannot extract information from unbroken subject. Status: {state.Status.GetDisplayName()}");
        }

        var reliability = state.CalculateReliability();
        var primaryMethod = state.GetPrimaryMethod();

        _logger.LogInformation(
            "Extracting information from interrogation {InterrogationId}: " +
            "{TopicCount} topics, Reliability {Reliability}%, Primary method {Method}",
            state.InterrogationId, topics.Count, reliability, primaryMethod.GetDisplayName());

        if (state.TortureUsed)
        {
            _logger.LogWarning(
                "Torture was used during interrogation {InterrogationId}. " +
                "Information reliability capped at {Reliability}% (maximum 60% with torture).",
                state.InterrogationId, reliability);
        }

        var result = new List<InformationGained>();

        foreach (var topic in topics)
        {
            // Roll d100 against reliability to determine truth
            var reliabilityRoll = _random.Next(1, 101);
            var isTrue = reliabilityRoll <= reliability;

            _logger.LogDebug(
                "Information extraction for topic '{Topic}': Roll {Roll} vs {Reliability}% reliability -> {IsTrue}",
                topic, reliabilityRoll, reliability, isTrue ? "TRUE" : "FALSE/MISLEADING");

            // Generate the information content
            var content = GenerateInformationContent(topic, isTrue);

            var info = new InformationGained
            {
                Topic = topic,
                Content = content,
                ReliabilityPercent = reliability,
                SourceMethod = primaryMethod,
                IsVerified = null, // Not yet verified by player investigation
                IsTrue = isTrue   // Hidden from player until verified
            };

            state.AddInformation(info);
            result.Add(info);

            _logger.LogInformation(
                "Information gained on '{Topic}': Reliability {Reliability}%, Source {Method}",
                topic, reliability, primaryMethod.GetDisplayName());
        }

        return result.AsReadOnly();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<InformationGained>> ExtractInformationAsync(
        InterrogationState state,
        IReadOnlyList<string> topics)
    {
        _logger.LogDebug(
            "ExtractInformationAsync called for interrogation {InterrogationId}",
            state.InterrogationId);

        var information = ExtractInformation(state, topics);
        return Task.FromResult(information);
    }

    #endregion

    #region Completion Methods

    /// <inheritdoc/>
    public InterrogationResult CompleteInterrogation(InterrogationState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        _logger.LogInformation(
            "Completing interrogation {InterrogationId}: Status {Status}, " +
            "Rounds {Rounds}, Torture used: {TortureUsed}",
            state.InterrogationId, state.Status.GetDisplayName(),
            state.RoundNumber, state.TortureUsed);

        // Generate narrative summary
        var narrativeSummary = GenerateInterrogationSummary(state);

        InterrogationResult result;
        if (state.Status.IsSuccess())
        {
            result = InterrogationResult.Success(state, state.InformationGained, narrativeSummary);

            _logger.LogInformation(
                "Interrogation {InterrogationId} SUCCESSFUL: {InfoCount} pieces of information, " +
                "Reliability {Reliability}%, Total time {TimeMinutes} minutes",
                state.InterrogationId, state.InformationGained.Count,
                result.InformationReliability, result.TotalTimeMinutes);
        }
        else
        {
            result = InterrogationResult.Failure(state, state.Status, narrativeSummary);

            _logger.LogInformation(
                "Interrogation {InterrogationId} FAILED: Status {Status}, " +
                "Rounds used {Rounds}, Total time {TimeMinutes} minutes",
                state.InterrogationId, state.Status.GetDisplayName(),
                state.RoundNumber, result.TotalTimeMinutes);
        }

        // Log final costs
        _logger.LogInformation(
            "Interrogation {InterrogationId} costs: Disposition {Disposition:+0;-0}, " +
            "Reputation {Reputation:+0;-0}, Resources {Resources} gold",
            state.InterrogationId, result.TotalDispositionChange,
            result.TotalReputationCost, result.TotalResourceCost);

        // Remove from active interrogations
        _activeInterrogations.Remove(state.InterrogationId);

        return result;
    }

    /// <inheritdoc/>
    public Task<InterrogationResult> CompleteInterrogationAsync(InterrogationState state)
    {
        _logger.LogDebug(
            "CompleteInterrogationAsync called for interrogation {InterrogationId}",
            state.InterrogationId);

        var result = CompleteInterrogation(state);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public void AbandonInterrogation(InterrogationState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        _logger.LogInformation(
            "Abandoning interrogation {InterrogationId} after {Rounds} rounds. " +
            "Costs incurred: Disposition {Disposition:+0;-0}, Reputation {Reputation:+0;-0}, Resources {Resources} gold",
            state.InterrogationId, state.RoundNumber,
            state.TotalDispositionChange, state.TotalReputationCost, state.TotalResourceCost);

        state.Abandon();

        if (state.TortureUsed)
        {
            _logger.LogWarning(
                "Interrogation {InterrogationId} abandoned after Torture was used. " +
                "Subject still gains [Traumatized] condition.",
                state.InterrogationId);
        }

        // Remove from active interrogations
        _activeInterrogations.Remove(state.InterrogationId);
    }

    /// <inheritdoc/>
    public Task AbandonInterrogationAsync(InterrogationState state)
    {
        _logger.LogDebug(
            "AbandonInterrogationAsync called for interrogation {InterrogationId}",
            state.InterrogationId);

        AbandonInterrogation(state);
        return Task.CompletedTask;
    }

    #endregion

    #region Method Availability and Context Building

    /// <inheritdoc/>
    public IReadOnlyDictionary<InterrogationMethod, bool> GetAvailableMethods(
        InterrogationState state,
        int availableGold)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        var briberyCost = CalculateBriberyCost(state.ResistanceLevel);
        var canAffordBribery = availableGold >= briberyCost;

        var methods = new Dictionary<InterrogationMethod, bool>
        {
            [InterrogationMethod.GoodCop] = !state.Status.IsTerminal(),
            [InterrogationMethod.BadCop] = !state.Status.IsTerminal(),
            [InterrogationMethod.Deception] = !state.Status.IsTerminal(),
            [InterrogationMethod.Bribery] = !state.Status.IsTerminal() && canAffordBribery,
            [InterrogationMethod.Torture] = !state.Status.IsTerminal()
        };

        _logger.LogDebug(
            "Available methods for interrogation {InterrogationId}: " +
            "GoodCop={GoodCop}, BadCop={BadCop}, Deception={Deception}, " +
            "Bribery={Bribery} (need {Cost} gold, have {Available}), Torture={Torture}",
            state.InterrogationId,
            methods[InterrogationMethod.GoodCop],
            methods[InterrogationMethod.BadCop],
            methods[InterrogationMethod.Deception],
            methods[InterrogationMethod.Bribery], briberyCost, availableGold,
            methods[InterrogationMethod.Torture]);

        return methods;
    }

    /// <inheritdoc/>
    public InterrogationContext BuildContext(
        InterrogationState state,
        InterrogationMethod selectedMethod,
        int interrogatorAttribute,
        int interrogatorRhetoric,
        int subjectWits,
        int availableGold = 0,
        int bonusDice = 0,
        int dcModifier = 0,
        bool useMight = false)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        _logger.LogDebug(
            "Building context for interrogation {InterrogationId}: Method {Method}, " +
            "Attribute {Attribute}, Rhetoric {Rhetoric}, Subject WITS {Wits}",
            state.InterrogationId, selectedMethod.GetDisplayName(),
            interrogatorAttribute, interrogatorRhetoric, subjectWits);

        // Validate method availability
        var availableMethods = GetAvailableMethods(state, availableGold);
        if (!availableMethods[selectedMethod])
        {
            if (selectedMethod == InterrogationMethod.Bribery)
            {
                throw new InvalidOperationException(
                    $"Insufficient gold for Bribery. Need {CalculateBriberyCost(state.ResistanceLevel)}, have {availableGold}.");
            }
            throw new InvalidOperationException(
                $"Method {selectedMethod.GetDisplayName()} is not available in current state.");
        }

        // Create base social context
        var baseContext = SocialContext.CreateMinimal(state.SubjectId, SocialInteractionType.Interrogation);

        var context = new InterrogationContext
        {
            BaseContext = baseContext,
            InterrogationState = state,
            SelectedMethod = selectedMethod,
            InterrogatorAttribute = interrogatorAttribute,
            InterrogatorRhetoric = interrogatorRhetoric,
            SubjectWits = subjectWits,
            AvailableGold = availableGold,
            BonusDice = bonusDice,
            DcModifier = dcModifier,
            UseMight = useMight
        };

        _logger.LogDebug(
            "Context built: Effective DC {Dc}, Dice pool {Pool}d10",
            context.CalculateEffectiveDc(), context.CalculateDicePool());

        return context;
    }

    /// <inheritdoc/>
    public Task<InterrogationContext> BuildContextAsync(
        InterrogationState state,
        InterrogationMethod selectedMethod,
        string interrogatorId,
        int bonusDice = 0,
        int dcModifier = 0,
        bool useMight = false)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentException.ThrowIfNullOrWhiteSpace(interrogatorId, nameof(interrogatorId));

        _logger.LogDebug(
            "BuildContextAsync called for interrogation {InterrogationId}, " +
            "Interrogator {InterrogatorId}",
            state.InterrogationId, interrogatorId);

        // In full implementation, would fetch character attributes from a service
        // For now, use reasonable defaults
        var interrogatorAttribute = 5;
        var interrogatorRhetoric = 3;
        var subjectWits = 4;
        var availableGold = 500;

        var context = BuildContext(
            state, selectedMethod,
            interrogatorAttribute, interrogatorRhetoric, subjectWits,
            availableGold, bonusDice, dcModifier, useMight);

        return Task.FromResult(context);
    }

    #endregion

    #region Display and Advice Methods

    /// <inheritdoc/>
    public string GetStateSummary(InterrogationState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        return state.GetStateSummary();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetTacticalAdvice(InterrogationState state, int availableGold)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        var advice = new List<string>();

        // Basic status advice
        if (state.Status.IsTerminal())
        {
            advice.Add("Interrogation has ended. No further rounds possible.");
            return advice.AsReadOnly();
        }

        var maxRounds = GetMaxRounds(state.ResistanceLevel);
        var roundsRemaining = maxRounds - state.RoundNumber;

        advice.Add($"Rounds remaining: {roundsRemaining} of {maxRounds}");
        advice.Add($"Resistance remaining: {state.ResistanceRemaining}");

        // Method recommendations
        if (state.ResistanceRemaining <= 2)
        {
            advice.Add("Subject is close to breaking. Consider reliable methods like GoodCop or Bribery.");
        }

        if (roundsRemaining <= state.ResistanceRemaining)
        {
            advice.Add("WARNING: May not have enough rounds to break subject. Consider aggressive methods.");
        }

        // Bribery advice
        var briberyCost = CalculateBriberyCost(state.ResistanceLevel);
        if (availableGold >= briberyCost)
        {
            advice.Add($"Bribery available ({briberyCost} gold). High reliability (90%).");
        }
        else
        {
            advice.Add($"Bribery unavailable - need {briberyCost} gold, have {availableGold}.");
        }

        // Torture warnings
        if (!state.TortureUsed)
        {
            advice.Add("WARNING: Using Torture caps reliability at 60% and causes severe reputation loss.");
        }
        else
        {
            advice.Add("Torture was used. Maximum reliability is now 60%.");
        }

        // Primary method tracking
        if (state.MethodsUsed.Count > 0)
        {
            advice.Add($"Current primary method: {state.GetPrimaryMethod().GetDisplayName()} ({state.GetPrimaryMethod().GetReliabilityPercent()}% base reliability)");
        }

        _logger.LogDebug(
            "Tactical advice generated for interrogation {InterrogationId}: {AdviceCount} suggestions",
            state.InterrogationId, advice.Count);

        return advice.AsReadOnly();
    }

    /// <inheritdoc/>
    public Task<InterrogationState?> GetActiveInterrogationAsync(string interrogationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(interrogationId, nameof(interrogationId));

        _activeInterrogations.TryGetValue(interrogationId, out var state);

        _logger.LogDebug(
            "GetActiveInterrogationAsync for {InterrogationId}: {Found}",
            interrogationId, state != null ? "Found" : "Not found");

        return Task.FromResult(state);
    }

    /// <inheritdoc/>
    public int GetMaxRounds(SubjectResistance resistanceLevel)
    {
        return MaxRoundsByResistance.TryGetValue(resistanceLevel, out var maxRounds)
            ? maxRounds
            : 10; // Default fallback
    }

    /// <inheritdoc/>
    public Task HandleSubjectBrokenFumbleAsync(InterrogationState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        _logger.LogError(
            "Handling [Subject Broken] fumble for interrogation {InterrogationId}. " +
            "Subject is permanently incapacitated (dead, insane, or catatonic). " +
            "Additional reputation penalty: -20",
            state.InterrogationId);

        state.MarkSubjectBrokenBeyondRecovery(-20);

        _logger.LogWarning(
            "Interrogation {InterrogationId} ended in catastrophic failure. " +
            "Total reputation cost: {ReputationCost}",
            state.InterrogationId, state.TotalReputationCost);

        return Task.CompletedTask;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Calculates the initial resistance based on level, WILL, and modifiers.
    /// </summary>
    /// <param name="level">The resistance level.</param>
    /// <param name="will">The subject's WILL attribute.</param>
    /// <param name="modifiers">Additional resistance modifiers.</param>
    /// <returns>The initial resistance value (checks to break).</returns>
    private int CalculateInitialResistance(SubjectResistance level, int will, int modifiers)
    {
        // Base resistance calculation
        var baseResistance = level switch
        {
            SubjectResistance.Minimal => Math.Max(1, will - 1),
            SubjectResistance.Low => will,
            SubjectResistance.Moderate => will + 1,
            SubjectResistance.High => will + 2,
            SubjectResistance.Extreme => will + 4,
            _ => will
        };

        // Add modifiers
        baseResistance += modifiers;

        // Clamp to level range
        var minChecks = level.GetMinChecksToBreak();
        var maxChecks = level.GetMaxChecksToBreak();

        return Math.Clamp(baseResistance, minChecks, maxChecks);
    }

    /// <summary>
    /// Simulates a dice pool roll using the d10 system.
    /// </summary>
    /// <param name="pool">Number of d10s to roll.</param>
    /// <returns>A tuple of (successes, botches).</returns>
    private (int Successes, int Botches) SimulateRoll(int pool)
    {
        var successes = 0;
        var botches = 0;

        for (var i = 0; i < pool; i++)
        {
            var roll = _random.Next(1, 11); // 1-10
            if (roll >= 8) successes++;     // 8, 9, 10 are successes
            if (roll == 1) botches++;       // 1 is a botch
        }

        return (successes, botches);
    }

    /// <summary>
    /// Calculates successes required to meet a DC.
    /// </summary>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>Number of successes required.</returns>
    private static int CalculateSuccessesRequired(int dc)
    {
        // Roughly DC/4, rounded up
        return (int)Math.Ceiling(dc / 4.0);
    }

    /// <summary>
    /// Determines the skill outcome based on successes achieved.
    /// </summary>
    /// <param name="successes">Successes achieved.</param>
    /// <param name="required">Successes required.</param>
    /// <param name="isFumble">Whether a fumble occurred.</param>
    /// <returns>The skill outcome.</returns>
    private static SkillOutcome DetermineOutcome(int successes, int required, bool isFumble)
    {
        if (isFumble)
        {
            return SkillOutcome.CriticalFailure;
        }

        var excess = successes - required;

        return excess switch
        {
            >= 3 => SkillOutcome.CriticalSuccess,
            >= 0 => SkillOutcome.FullSuccess,
            _ => SkillOutcome.Failure
        };
    }

    /// <summary>
    /// Generates narrative text for a round.
    /// </summary>
    /// <param name="method">The method used.</param>
    /// <param name="isSuccess">Whether the round succeeded.</param>
    /// <param name="isFumble">Whether a fumble occurred.</param>
    /// <returns>Narrative description of the round.</returns>
    private static string GenerateRoundNarrative(InterrogationMethod method, bool isSuccess, bool isFumble)
    {
        if (isFumble)
        {
            return method switch
            {
                InterrogationMethod.GoodCop =>
                    "Your attempt at empathy backfires completely. The subject sees through your facade and clams up.",
                InterrogationMethod.BadCop =>
                    "Your threats are met with defiance. The subject challenges you, emboldened rather than afraid.",
                InterrogationMethod.Deception =>
                    "Your lie is immediately spotted. The subject now knows you cannot be trusted.",
                InterrogationMethod.Bribery =>
                    "Your offer is taken as an insult. The subject's resolve hardens.",
                InterrogationMethod.Torture =>
                    "The torture goes too far. The subject is broken in ways that cannot be undone.",
                _ => "Something goes terribly wrong."
            };
        }

        if (isSuccess)
        {
            return method switch
            {
                InterrogationMethod.GoodCop =>
                    "Your sympathetic approach finds purchase. The subject seems slightly more willing to talk.",
                InterrogationMethod.BadCop =>
                    "Your aggressive stance has the desired effect. Fear flickers in the subject's eyes.",
                InterrogationMethod.Deception =>
                    "Your deception works. The subject unknowingly reveals something useful.",
                InterrogationMethod.Bribery =>
                    "The offer is tempting. The subject's resistance wavers.",
                InterrogationMethod.Torture =>
                    "The subject screams. Their will erodes under the pain.",
                _ => "The method has some effect."
            };
        }

        return method switch
        {
            InterrogationMethod.GoodCop =>
                "The subject remains unmoved by your words.",
            InterrogationMethod.BadCop =>
                "The subject endures your threats in sullen silence.",
            InterrogationMethod.Deception =>
                "The subject doesn't take the bait.",
            InterrogationMethod.Bribery =>
                "The offer isn't enough to sway them.",
            InterrogationMethod.Torture =>
                "The subject holds on, refusing to break.",
            _ => "No progress is made."
        };
    }

    /// <summary>
    /// Generates placeholder information content.
    /// </summary>
    /// <param name="topic">The topic asked about.</param>
    /// <param name="isTrue">Whether the information is true.</param>
    /// <returns>Information content string.</returns>
    private static string GenerateInformationContent(string topic, bool isTrue)
    {
        // In actual implementation, this would pull from game data/NPC knowledge
        // For now, return a placeholder indicating the topic
        // Note: The "[FALSE]" prefix would NOT be shown to the player
        var truthPrefix = isTrue ? "" : "[FALSE] ";
        return $"{truthPrefix}Information about: {topic}";
    }

    /// <summary>
    /// Generates a narrative summary of the complete interrogation.
    /// </summary>
    /// <param name="state">The interrogation state.</param>
    /// <returns>Narrative summary string.</returns>
    private static string GenerateInterrogationSummary(InterrogationState state)
    {
        if (state.Status == InterrogationStatus.SubjectBroken)
        {
            var methodSummary = state.TortureUsed
                ? "through brutal methods that will not be forgotten"
                : $"primarily using {state.GetPrimaryMethod().GetDisplayName().ToLower()} techniques";

            return $"After {state.RoundNumber} rounds of interrogation {methodSummary}, " +
                   $"the subject's resistance finally crumbled. " +
                   $"Information reliability: {state.CalculateReliability()}%.";
        }

        if (state.Status == InterrogationStatus.Abandoned)
        {
            var tortureSuffix = state.TortureUsed
                ? " The subject bears the marks of torture."
                : "";
            return $"The interrogation was abandoned after {state.RoundNumber} rounds. " +
                   $"No reliable information was obtained.{tortureSuffix}";
        }

        if (state.Status == InterrogationStatus.SubjectResisting)
        {
            return $"The subject successfully resisted all attempts over {state.RoundNumber} rounds. " +
                   "Their will proved unbreakable.";
        }

        return $"The interrogation concluded after {state.RoundNumber} rounds.";
    }

    #endregion
}
