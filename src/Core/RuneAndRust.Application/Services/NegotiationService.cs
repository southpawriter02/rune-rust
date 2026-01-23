// ------------------------------------------------------------------------------
// <copyright file="NegotiationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of the negotiation service for multi-round negotiations.
// Orchestrates position tracking, tactic execution, and deal resolution.
// Part of v0.15.3e Negotiation System implementation.
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
/// Service for managing multi-round negotiations.
/// </summary>
/// <remarks>
/// <para>
/// Orchestrates the complete negotiation workflow including:
/// </para>
/// <list type="bullet">
///   <item><description>Position track initialization and management</description></item>
///   <item><description>Tactic-based round execution (Persuade, Deceive, Pressure, Concede)</description></item>
///   <item><description>Concession handling and bonus application</description></item>
///   <item><description>Crisis detection and fumble consequences</description></item>
///   <item><description>Deal finalization and result generation</description></item>
/// </list>
/// <para>
/// The service delegates to underlying Rhetoric skill services based on
/// the selected tactic each round:
/// </para>
/// <list type="bullet">
///   <item><description>Persuade → IPersuasionService</description></item>
///   <item><description>Deceive → IDeceptionService</description></item>
///   <item><description>Pressure → IIntimidationService</description></item>
/// </list>
/// </remarks>
public class NegotiationService : INegotiationService
{
    private readonly ILogger<NegotiationService> _logger;
    private readonly Random _random;

    // In-memory tracking of active negotiations (would be persisted in full implementation)
    private readonly Dictionary<string, NegotiationPositionTrack> _activeNegotiations = new();

    // Default configuration values
    private const int MinimumDc = 4;
    private const int BaseStressCostSuccess = 3;
    private const int BaseStressCostFailure = 6;
    private const int BaseStressCostFumble = 8;
    private const int ReputationCostCriticalSuccess = -3;
    private const int ReputationCostSuccess = -5;
    private const int ReputationCostFailure = -10;

    /// <summary>
    /// Initializes a new instance of the <see cref="NegotiationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public NegotiationService(ILogger<NegotiationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <inheritdoc/>
    public async Task<NegotiationPositionTrack> InitiateNegotiationAsync(
        string npcId,
        RequestComplexity requestComplexity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogInformation(
            "Initiating negotiation with NPC {NpcId}. Request complexity: {Complexity} (DC {BaseDc}, Gap {Gap})",
            npcId,
            requestComplexity.GetDisplayName(),
            requestComplexity.GetBaseDc(),
            requestComplexity.GetInitialGap());

        // Calculate starting positions
        var pcStartPosition = requestComplexity.GetDefaultPcStartPosition();
        var npcStartPosition = await CalculateNpcStartPositionAsync(npcId, requestComplexity);
        var totalRounds = requestComplexity.GetDefaultRounds();

        _logger.LogDebug(
            "Calculated starting positions: PC at {PcPos} ({PcName}), NPC at {NpcPos} ({NpcName})",
            pcStartPosition,
            NegotiationPositionTrack.GetPositionName(pcStartPosition),
            npcStartPosition,
            NegotiationPositionTrack.GetPositionName(npcStartPosition));

        // Create and initialize the position track
        var track = new NegotiationPositionTrack
        {
            NegotiationId = Guid.NewGuid().ToString(),
            NpcId = npcId,
            RequestComplexity = requestComplexity
        };

        track.Initialize(pcStartPosition, npcStartPosition, totalRounds);

        // Store in active negotiations
        _activeNegotiations[track.NegotiationId] = track;

        _logger.LogInformation(
            "Negotiation {NegotiationId} initialized. " +
            "PC at {PcPos}, NPC at {NpcPos}, Gap: {Gap}, Rounds: {Rounds}",
            track.NegotiationId,
            track.PcPosition,
            track.NpcPosition,
            track.Gap,
            totalRounds);

        return await Task.FromResult(track);
    }

    /// <inheritdoc/>
    public async Task<NegotiationRound> ExecuteRoundAsync(NegotiationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Validate context
        var validationError = context.GetValidationError();
        if (validationError != null)
        {
            _logger.LogError("Invalid negotiation context: {Error}", validationError);
            throw new InvalidOperationException(validationError);
        }

        _logger.LogInformation(
            "Executing negotiation round {Round}. Tactic: {Tactic}, Status: {Status}, Gap: {Gap}",
            context.CurrentRound,
            context.SelectedTactic.GetDisplayName(),
            context.CurrentStatus.GetDisplayName(),
            context.CurrentGap);

        // Log any warnings about risky tactics
        var warning = context.GetTacticWarning();
        if (warning != null)
        {
            _logger.LogWarning("Tactic warning: {Warning}", warning);
        }

        // Handle concession tactic separately (no skill check)
        if (context.SelectedTactic == NegotiationTactic.Concede)
        {
            return await ExecuteConcessionRoundAsync(context);
        }

        // Execute the skill check for the selected tactic
        var outcome = await ExecuteTacticCheckAsync(context);

        _logger.LogDebug(
            "Tactic check result: {Outcome} for {Tactic}",
            outcome,
            context.SelectedTactic.GetDisplayName());

        // Calculate position movement
        var (movementSteps, movementDescription) = CalculateMovementDetails(outcome);

        // Store positions before movement
        var pcPositionBefore = context.PositionTrack.PcPosition;
        var npcPositionBefore = context.PositionTrack.NpcPosition;

        // Apply position changes based on outcome
        ApplyPositionMovement(context.PositionTrack, outcome);

        _logger.LogDebug(
            "Position movement: {Description}. " +
            "PC: {PcBefore} → {PcAfter}, NPC: {NpcBefore} → {NpcAfter}",
            movementDescription,
            pcPositionBefore,
            context.PositionTrack.PcPosition,
            npcPositionBefore,
            context.PositionTrack.NpcPosition);

        // Calculate tactic-specific costs
        var (stressCost, reputationCost, dispositionChange) =
            CalculateTacticCosts(context.SelectedTactic, outcome);

        _logger.LogDebug(
            "Round costs: Stress {Stress}, Reputation {Rep}, Disposition {Disp}",
            stressCost,
            reputationCost,
            dispositionChange);

        // Handle fumble consequences for dangerous tactics
        if (outcome == SkillOutcome.CriticalFailure)
        {
            HandleFumbleConsequences(context, stressCost, reputationCost);
        }

        // Clear used concession bonus
        if (context.ActiveConcession != null)
        {
            _logger.LogDebug(
                "Consuming active concession bonus: +{Dice}d10, -{Dc} DC",
                context.ActiveConcession.BonusDice,
                context.ActiveConcession.DcReduction);
            context.PositionTrack.ClearConcessionBonus();
        }

        // Create the round record
        var round = new NegotiationRound
        {
            RoundNumber = context.CurrentRound,
            TacticUsed = context.SelectedTactic,
            CheckResult = outcome,
            PositionMovement = movementDescription,
            PcPositionAfter = context.PositionTrack.PcPosition,
            NpcPositionAfter = context.PositionTrack.NpcPosition,
            GapAfter = context.PositionTrack.Gap,
            StressCost = stressCost,
            ReputationCost = reputationCost,
            DispositionChange = dispositionChange,
            EffectiveDcUsed = context.GetEffectiveDc(),
            DicePoolUsed = CalculateDicePool(context),
            NarrativeDescription = GenerateRoundNarrative(context, outcome)
        };

        // Record the round in the position track
        context.PositionTrack.RecordRound(round);

        _logger.LogInformation(
            "Round {Round} complete. Outcome: {Outcome}, " +
            "New positions: PC {Pc} ({PcName}), NPC {Npc} ({NpcName}), Gap: {Gap}, Status: {Status}",
            round.RoundNumber,
            outcome,
            round.PcPositionAfter,
            NegotiationPositionTrack.GetPositionName(round.PcPositionAfter),
            round.NpcPositionAfter,
            NegotiationPositionTrack.GetPositionName(round.NpcPositionAfter),
            round.GapAfter,
            context.PositionTrack.Status.GetDisplayName());

        return await Task.FromResult(round);
    }

    /// <inheritdoc/>
    public async Task<NegotiationPositionTrack> ApplyConcessionAsync(
        NegotiationPositionTrack positionTrack,
        Concession concession)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);
        ArgumentNullException.ThrowIfNull(concession);

        if (positionTrack.Status.IsTerminal())
        {
            throw new InvalidOperationException(
                $"Cannot apply concession to a completed negotiation. Status: {positionTrack.Status}");
        }

        _logger.LogInformation(
            "Applying concession: {Type} ({DisplayName}), DC reduction: -{Dc}, Bonus: +{Dice}d10",
            concession.Type,
            concession.Type.GetDisplayName(),
            concession.DcReduction,
            concession.BonusDice);

        // Move PC position 1 step toward NPC (voluntary concession)
        var positionBefore = positionTrack.PcPosition;
        positionTrack.MovePcPosition(1);

        _logger.LogDebug(
            "PC position moved from {Before} to {After} (toward NPC)",
            positionBefore,
            positionTrack.PcPosition);

        // Set concession bonus for next round
        positionTrack.SetConcessionBonus(concession);

        _logger.LogInformation(
            "Concession applied. Next check will have +{Dice}d10 and -{Dc} DC bonus",
            concession.BonusDice,
            concession.DcReduction);

        return await Task.FromResult(positionTrack);
    }

    /// <inheritdoc/>
    public bool IsNegotiationComplete(NegotiationPositionTrack positionTrack)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);
        return positionTrack.Status.IsTerminal();
    }

    /// <inheritdoc/>
    public async Task<NegotiationResult> FinalizeNegotiationAsync(NegotiationPositionTrack positionTrack)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);

        if (!positionTrack.Status.IsTerminal())
        {
            throw new InvalidOperationException(
                $"Cannot finalize an incomplete negotiation. Status: {positionTrack.Status}");
        }

        _logger.LogInformation(
            "Finalizing negotiation {NegotiationId}. Status: {Status}, Final gap: {Gap}",
            positionTrack.NegotiationId,
            positionTrack.Status.GetDisplayName(),
            positionTrack.Gap);

        // Calculate totals from round history
        var totalStress = positionTrack.GetTotalStressCost();
        var totalReputation = positionTrack.GetTotalReputationCost();
        var totalDisposition = positionTrack.GetTotalDispositionChange();

        _logger.LogDebug(
            "Accumulated costs: Stress {Stress}, Reputation {Rep}, Disposition {Disp}",
            totalStress,
            totalReputation,
            totalDisposition);

        // Calculate final disposition change based on outcome
        var outcomeDisposition = CalculateFinalDispositionChange(positionTrack);
        totalDisposition += outcomeDisposition;

        // Determine unlocked options for successful negotiations
        var unlockedOptions = await DetermineUnlockedOptionsAsync(positionTrack);

        // Generate deal terms if successful
        string? dealTerms = null;
        if (positionTrack.Status == NegotiationStatus.DealReached)
        {
            dealTerms = GenerateDealTerms(positionTrack);
            _logger.LogInformation("Deal reached: {Terms}", dealTerms);
        }

        // Create fumble consequence if collapsed from fumble
        FumbleConsequence? fumbleConsequence = null;
        if (positionTrack.Status == NegotiationStatus.Collapsed &&
            positionTrack.History.Any(r => r.IsFumble))
        {
            fumbleConsequence = CreateNegotiationCollapseConsequence(positionTrack);
            _logger.LogWarning(
                "Negotiation collapsed with fumble consequence: {Description}",
                fumbleConsequence.Description);
        }

        // Create base social result
        var isSuccess = positionTrack.Status == NegotiationStatus.DealReached;
        var baseResult = CreateBaseSocialResult(positionTrack, totalDisposition, totalReputation);

        // Determine overall outcome based on final position
        var overallOutcome = DetermineOverallOutcome(positionTrack);

        // Create the final result
        NegotiationResult result;
        if (isSuccess)
        {
            result = NegotiationResult.Success(
                baseResult,
                positionTrack.PcPosition,
                positionTrack.NpcPosition,
                positionTrack.RoundNumber,
                positionTrack.History,
                dealTerms ?? "Terms agreed",
                unlockedOptions,
                totalDisposition,
                totalReputation,
                totalStress);
        }
        else if (fumbleConsequence != null)
        {
            result = NegotiationResult.Fumble(
                baseResult,
                positionTrack.PcPosition,
                positionTrack.NpcPosition,
                positionTrack.RoundNumber,
                positionTrack.History,
                fumbleConsequence,
                totalDisposition,
                totalReputation,
                totalStress);
        }
        else
        {
            result = NegotiationResult.Failure(
                baseResult,
                positionTrack.PcPosition,
                positionTrack.NpcPosition,
                positionTrack.RoundNumber,
                positionTrack.History,
                totalDisposition,
                totalReputation,
                totalStress);
        }

        // Mark the position track as complete and remove from active negotiations
        positionTrack.MarkComplete();
        _activeNegotiations.Remove(positionTrack.NegotiationId);

        _logger.LogInformation(
            "Negotiation finalized. {Summary}",
            result.GetNarrativeSummary());

        return await Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Concession>> GetAvailableConcessionsAsync()
    {
        _logger.LogDebug("Fetching available concessions");

        var concessions = new List<Concession>
        {
            // Always available options
            Concession.PromiseFavor("A future favor to be named later"),
            Concession.TradeInformation("Valuable information you possess"),
            Concession.TakeRisk("Accept a personal risk in the arrangement"),
            Concession.StakeReputation("player_faction", "Your faction's reputation backs this deal")
        };

        _logger.LogDebug("Returning {Count} available concessions", concessions.Count);
        return await Task.FromResult<IReadOnlyList<Concession>>(concessions);
    }

    /// <inheritdoc/>
    public async Task<int> CalculateNpcStartPositionAsync(string npcId, RequestComplexity requestComplexity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogDebug(
            "Calculating NPC start position for {NpcId} with complexity {Complexity}",
            npcId, requestComplexity.GetDisplayName());

        // Base position from complexity
        var basePosition = requestComplexity.GetDefaultNpcStartPosition();

        // In a full implementation, this would query NPC disposition and traits
        // For now, return the base position
        var finalPosition = Math.Clamp(basePosition, 3, 7);

        _logger.LogDebug(
            "NPC start position calculated: {Position} ({Name})",
            finalPosition,
            NegotiationPositionTrack.GetPositionName(finalPosition));

        return await Task.FromResult(finalPosition);
    }

    /// <inheritdoc/>
    public async Task<int> GetNpcFlexibilityAsync(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogDebug("Getting flexibility for NPC {NpcId}", npcId);

        // Default to average flexibility
        // In a full implementation, this would query NPC traits
        const int defaultFlexibility = 2;

        _logger.LogDebug("NPC flexibility: {Flexibility}", defaultFlexibility);
        return await Task.FromResult(defaultFlexibility);
    }

    /// <inheritdoc/>
    public async Task<NegotiationContext> BuildContextAsync(
        NegotiationPositionTrack positionTrack,
        NegotiationTactic selectedTactic,
        Concession? concessionOffer = null)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);

        if (selectedTactic == NegotiationTactic.Concede && concessionOffer == null)
        {
            throw new ArgumentException(
                "Concession offer is required when using the Concede tactic.",
                nameof(concessionOffer));
        }

        _logger.LogDebug(
            "Building context for round {Round}, tactic: {Tactic}",
            positionTrack.RoundNumber + 1,
            selectedTactic.GetDisplayName());

        var npcFlexibility = await GetNpcFlexibilityAsync(positionTrack.NpcId);

        var context = new NegotiationContext
        {
            BaseContext = SocialContext.CreateMinimal(
                positionTrack.NpcId,
                SocialInteractionType.Negotiation),
            RequestComplexity = positionTrack.RequestComplexity,
            PositionTrack = positionTrack,
            SelectedTactic = selectedTactic,
            ActiveConcession = positionTrack.ActiveConcession,
            NpcFlexibility = npcFlexibility,
            ConcessionOffer = concessionOffer
        };

        _logger.LogDebug(
            "Context built: DC {EffectiveDc}, Dice modifier {DiceMod}, " +
            "Concession bonus: {HasConcession}",
            context.GetEffectiveDc(),
            context.GetTotalDiceModifier(),
            context.ActiveConcession != null);

        return context;
    }

    /// <inheritdoc/>
    public string GetStateSummary(NegotiationPositionTrack positionTrack)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);
        return positionTrack.GetStatusSummary();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetTacticalAdviceAsync(NegotiationPositionTrack positionTrack)
    {
        ArgumentNullException.ThrowIfNull(positionTrack);

        var advice = new List<string>();

        // Status-based advice
        switch (positionTrack.Status)
        {
            case NegotiationStatus.Opening:
                advice.Add("Opening phase: Establish your position with a strong opening move.");
                break;

            case NegotiationStatus.Bargaining:
                advice.Add($"Bargaining phase: Gap is {positionTrack.Gap}. Continue pressing your case.");
                if (positionTrack.RoundsRemaining <= 2)
                {
                    advice.Add("Warning: Limited rounds remaining. Consider a concession to improve odds.");
                }
                break;

            case NegotiationStatus.CrisisManagement:
                advice.Add("CRISIS: Negotiation at risk of collapse!");
                advice.Add("Avoid Pressure and Deceive tactics - fumbles will collapse the negotiation.");
                advice.Add("Consider making a concession to stabilize the situation.");
                break;

            case NegotiationStatus.Finalization:
                advice.Add("Finalization phase: Close to a deal! One more success should seal it.");
                break;
        }

        // Position-based advice
        if (positionTrack.Gap >= 5)
        {
            advice.Add("Large gap: Consider using a powerful concession like StakeReputation.");
        }

        if (positionTrack.ConsecutiveFailures >= 1)
        {
            advice.Add($"Warning: {positionTrack.ConsecutiveFailures} consecutive failure(s). " +
                      "Another failure may trigger crisis.");
        }

        // Concession reminder
        if (positionTrack.ActiveConcession != null)
        {
            advice.Add($"Active bonus: +{positionTrack.ActiveConcession.BonusDice}d10, " +
                      $"-{positionTrack.ActiveConcession.DcReduction} DC on next check.");
        }

        return await Task.FromResult<IReadOnlyList<string>>(advice);
    }

    /// <inheritdoc/>
    public int CalculatePositionMovement(SkillOutcome outcome, int npcFlexibility)
    {
        // Base movement
        var baseMovement = outcome switch
        {
            SkillOutcome.CriticalSuccess => 2,
            SkillOutcome.ExceptionalSuccess => 2,
            SkillOutcome.FullSuccess => 1,
            SkillOutcome.MarginalSuccess => 1,
            SkillOutcome.Failure => 1,
            SkillOutcome.CriticalFailure => 2,
            _ => 1
        };

        // Flexibility bonus on success (NPC moves more if flexible)
        if (outcome >= SkillOutcome.MarginalSuccess && npcFlexibility == 3)
        {
            baseMovement = Math.Min(baseMovement + 1, 3);
        }

        return baseMovement;
    }

    /// <inheritdoc/>
    public async Task<NegotiationPositionTrack?> GetActiveNegotiationAsync(string negotiationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(negotiationId, nameof(negotiationId));

        _activeNegotiations.TryGetValue(negotiationId, out var track);
        return await Task.FromResult(track);
    }

    /// <inheritdoc/>
    public async Task AbandonNegotiationAsync(string negotiationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(negotiationId, nameof(negotiationId));

        if (_activeNegotiations.TryGetValue(negotiationId, out var track))
        {
            _logger.LogInformation(
                "Abandoning negotiation {NegotiationId} with NPC {NpcId}",
                negotiationId,
                track.NpcId);

            track.ForceCollapse();
            _activeNegotiations.Remove(negotiationId);
        }
        else
        {
            _logger.LogWarning(
                "Attempted to abandon unknown negotiation {NegotiationId}",
                negotiationId);
        }

        await Task.CompletedTask;
    }

    #region Private Methods

    /// <summary>
    /// Executes a concession round (no skill check).
    /// </summary>
    private async Task<NegotiationRound> ExecuteConcessionRoundAsync(NegotiationContext context)
    {
        if (context.ConcessionOffer == null)
        {
            throw new InvalidOperationException("Concede tactic requires a concession offer");
        }

        _logger.LogInformation(
            "Executing concession round. Concession: {Type} ({DisplayName})",
            context.ConcessionOffer.Type,
            context.ConcessionOffer.Type.GetDisplayName());

        // Apply the concession
        await ApplyConcessionAsync(context.PositionTrack, context.ConcessionOffer);

        // Create the round record
        var round = NegotiationRound.ForConcession(
            context.CurrentRound,
            context.ConcessionOffer,
            context.PositionTrack.PcPosition,
            context.PositionTrack.NpcPosition,
            $"You offer a concession: {context.ConcessionOffer.Description}. " +
            $"This grants +{context.ConcessionOffer.BonusDice}d10 and " +
            $"-{context.ConcessionOffer.DcReduction} DC on your next check.");

        // Record the round
        context.PositionTrack.RecordRound(round);

        _logger.LogInformation(
            "Concession round complete. New gap: {Gap}, Next check bonus: +{Dice}d10, -{Dc} DC",
            round.GapAfter,
            context.ConcessionOffer.BonusDice,
            context.ConcessionOffer.DcReduction);

        return round;
    }

    /// <summary>
    /// Executes the skill check for the selected tactic.
    /// </summary>
    private async Task<SkillOutcome> ExecuteTacticCheckAsync(NegotiationContext context)
    {
        var effectiveDc = context.GetEffectiveDc();
        var dicePool = CalculateDicePool(context);

        _logger.LogDebug(
            "Executing {Tactic} check: {DicePool}d10 vs DC {Dc}",
            context.SelectedTactic.GetDisplayName(),
            dicePool,
            effectiveDc);

        // Simulate the dice roll
        var (successes, botches) = SimulateRoll(dicePool);

        _logger.LogDebug(
            "Roll result: {Successes} successes, {Botches} botches",
            successes,
            botches);

        // Determine outcome
        var outcome = DetermineOutcome(successes, botches, effectiveDc);

        _logger.LogDebug(
            "Outcome determined: {Outcome} (successes {Successes} vs DC {Dc})",
            outcome,
            successes,
            effectiveDc);

        return await Task.FromResult(outcome);
    }

    /// <summary>
    /// Simulates a dice pool roll using success-counting mechanics.
    /// </summary>
    private (int successes, int botches) SimulateRoll(int dicePool)
    {
        var successes = 0;
        var botches = 0;

        for (var i = 0; i < dicePool; i++)
        {
            var roll = _random.Next(1, 11); // 1-10
            if (roll >= 7) successes++;
            else if (roll == 1) botches++;
        }

        return (successes, botches);
    }

    /// <summary>
    /// Determines the skill outcome based on successes, botches, and DC.
    /// </summary>
    private static SkillOutcome DetermineOutcome(int successes, int botches, int dc)
    {
        // Check for fumble first (0 successes and at least 1 botch)
        if (successes == 0 && botches > 0)
        {
            return SkillOutcome.CriticalFailure;
        }

        // Calculate margin
        var margin = successes - dc;

        return margin switch
        {
            >= 5 => SkillOutcome.CriticalSuccess,
            >= 3 => SkillOutcome.ExceptionalSuccess,
            >= 1 => SkillOutcome.FullSuccess,
            0 => SkillOutcome.MarginalSuccess,
            _ => SkillOutcome.Failure
        };
    }

    /// <summary>
    /// Calculates the dice pool size for the check.
    /// </summary>
    private static int CalculateDicePool(NegotiationContext context)
    {
        // Base pool (attribute + skill, typically 5-7)
        const int basePool = 5;

        // Add social context modifiers
        var modifiers = context.BaseContext.TotalDiceModifier;

        // Add concession bonus
        var concessionBonus = context.GetConcessionBonusDice();

        return Math.Max(1, basePool + modifiers + concessionBonus);
    }

    /// <summary>
    /// Calculates movement details based on outcome.
    /// </summary>
    private static (int steps, string description) CalculateMovementDetails(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                (2, "NPC moved 2 steps toward compromise (critical success)"),
            SkillOutcome.ExceptionalSuccess =>
                (2, "NPC moved 2 steps toward compromise (exceptional success)"),
            SkillOutcome.FullSuccess =>
                (1, "NPC moved 1 step toward compromise (success)"),
            SkillOutcome.MarginalSuccess =>
                (1, "NPC moved 1 step toward compromise (marginal success)"),
            SkillOutcome.Failure =>
                (1, "PC moved 1 step toward NPC (failure)"),
            SkillOutcome.CriticalFailure =>
                (2, "PC moved 2 steps toward NPC (fumble)"),
            _ => (0, "No movement")
        };
    }

    /// <summary>
    /// Applies position movement based on outcome.
    /// </summary>
    private static void ApplyPositionMovement(NegotiationPositionTrack track, SkillOutcome outcome)
    {
        switch (outcome)
        {
            case SkillOutcome.CriticalSuccess:
            case SkillOutcome.ExceptionalSuccess:
                track.MoveNpcPosition(2);
                break;

            case SkillOutcome.FullSuccess:
            case SkillOutcome.MarginalSuccess:
                track.MoveNpcPosition(1);
                break;

            case SkillOutcome.Failure:
                track.MovePcPosition(1);
                break;

            case SkillOutcome.CriticalFailure:
                track.MovePcPosition(2);
                break;
        }
    }

    /// <summary>
    /// Calculates tactic-specific costs (stress, reputation, disposition).
    /// </summary>
    private static (int stress, int reputation, int disposition) CalculateTacticCosts(
        NegotiationTactic tactic,
        SkillOutcome outcome)
    {
        var stress = 0;
        var reputation = 0;
        var disposition = 0;

        switch (tactic)
        {
            case NegotiationTactic.Deceive:
                // Liar's Burden: always incurs stress
                stress = outcome switch
                {
                    SkillOutcome.CriticalSuccess => BaseStressCostSuccess,
                    SkillOutcome.ExceptionalSuccess => BaseStressCostSuccess,
                    SkillOutcome.FullSuccess => BaseStressCostSuccess,
                    SkillOutcome.MarginalSuccess => BaseStressCostSuccess,
                    SkillOutcome.Failure => BaseStressCostFailure,
                    SkillOutcome.CriticalFailure => BaseStressCostFumble,
                    _ => 0
                };
                break;

            case NegotiationTactic.Pressure:
                // Cost of Fear: always costs reputation
                reputation = outcome switch
                {
                    SkillOutcome.CriticalSuccess => ReputationCostCriticalSuccess,
                    SkillOutcome.ExceptionalSuccess => ReputationCostSuccess,
                    SkillOutcome.FullSuccess => ReputationCostSuccess,
                    SkillOutcome.MarginalSuccess => ReputationCostSuccess,
                    _ => ReputationCostFailure
                };
                // Intimidation also affects disposition
                disposition = -5;
                break;
        }

        return (stress, reputation, disposition);
    }

    /// <summary>
    /// Handles fumble consequences for dangerous tactics.
    /// </summary>
    private void HandleFumbleConsequences(NegotiationContext context, int stressCost, int reputationCost)
    {
        var tactic = context.SelectedTactic;
        var status = context.PositionTrack.Status;

        // Fumble during Crisis Management always collapses the negotiation
        if (status == NegotiationStatus.CrisisManagement)
        {
            _logger.LogWarning(
                "Fumble during Crisis Management! Negotiation collapsing.");
            context.PositionTrack.ForceCollapse();
            return;
        }

        // Fumble on Deceive or Pressure can collapse negotiation
        if (tactic == NegotiationTactic.Deceive || tactic == NegotiationTactic.Pressure)
        {
            _logger.LogWarning(
                "Fumble on {Tactic} tactic. Negotiation at high risk.",
                tactic.GetDisplayName());

            // These tactics' fumbles are more likely to cause collapse
            if (context.CurrentGap >= 4)
            {
                _logger.LogWarning(
                    "Large gap ({Gap}) combined with fumble triggers collapse.",
                    context.CurrentGap);
                context.PositionTrack.ForceCollapse();
            }
        }
    }

    /// <summary>
    /// Generates narrative text for a round.
    /// </summary>
    private static string GenerateRoundNarrative(NegotiationContext context, SkillOutcome outcome)
    {
        var tacticName = context.SelectedTactic.GetDisplayName().ToLower();
        var successText = outcome >= SkillOutcome.MarginalSuccess ? "succeed" : "fail";

        return outcome switch
        {
            SkillOutcome.CriticalSuccess =>
                $"Your {tacticName} is masterfully executed. The NPC is visibly moved toward your position.",

            SkillOutcome.ExceptionalSuccess =>
                $"Your {tacticName} lands with exceptional effectiveness. You've made significant progress.",

            SkillOutcome.FullSuccess =>
                $"Your {tacticName} succeeds clearly. The NPC shifts their stance in your favor.",

            SkillOutcome.MarginalSuccess =>
                $"Your {tacticName} barely succeeds. The NPC makes a small concession.",

            SkillOutcome.Failure =>
                $"Your {tacticName} fails to convince. You feel pressure to give ground.",

            SkillOutcome.CriticalFailure =>
                $"Your {tacticName} backfires catastrophically. The negotiation takes a severe turn.",

            _ => $"The round concludes with your {tacticName} attempt."
        };
    }

    /// <summary>
    /// Calculates final disposition change based on outcome.
    /// </summary>
    private static int CalculateFinalDispositionChange(NegotiationPositionTrack track)
    {
        if (track.Status == NegotiationStatus.DealReached)
        {
            // Fair deals improve disposition
            return track.PcPosition >= 4 ? 5 : 2;
        }
        else if (track.Status == NegotiationStatus.Collapsed)
        {
            // Collapsed negotiations damage relationship
            return -10;
        }

        return 0;
    }

    /// <summary>
    /// Determines unlocked options for successful negotiations.
    /// </summary>
    private async Task<IReadOnlyList<string>> DetermineUnlockedOptionsAsync(NegotiationPositionTrack track)
    {
        if (track.Status != NegotiationStatus.DealReached)
        {
            return Array.Empty<string>();
        }

        var options = new List<string> { "Deal terms in effect" };

        // Add options based on how favorable the deal was
        if (track.PcPosition <= 2)
        {
            options.Add("Excellent terms secured - additional cooperation available");
        }

        if (track.PcPosition <= 4)
        {
            options.Add("Favorable relationship - future negotiations easier");
        }

        return await Task.FromResult<IReadOnlyList<string>>(options);
    }

    /// <summary>
    /// Generates deal terms description based on final positions.
    /// </summary>
    private static string GenerateDealTerms(NegotiationPositionTrack track)
    {
        var pcPosition = track.PcPosition;
        var complexityName = track.RequestComplexity.GetDisplayName();

        return pcPosition switch
        {
            <= 2 => $"Excellent terms on your {complexityName.ToLower()} request. " +
                   "The NPC has agreed to highly favorable conditions.",
            <= 4 => $"Good terms on your {complexityName.ToLower()} request. " +
                   "You've secured a favorable arrangement.",
            5 => $"Fair compromise reached on your {complexityName.ToLower()} request. " +
                "Both parties have made concessions.",
            _ => $"Deal reached on your {complexityName.ToLower()} request, " +
                "though with some unfavorable terms."
        };
    }

    /// <summary>
    /// Creates a fumble consequence for negotiation collapse.
    /// </summary>
    private static FumbleConsequence CreateNegotiationCollapseConsequence(NegotiationPositionTrack track)
    {
        return new FumbleConsequence(
            consequenceId: Guid.NewGuid().ToString(),
            characterId: "player",
            skillId: "negotiation",
            consequenceType: FumbleType.TrustShattered,
            targetId: track.NpcId,
            appliedAt: DateTime.UtcNow,
            expiresAt: null,
            description: $"Catastrophic negotiation failure with {track.NpcId}. " +
                        "Future negotiations will be significantly more difficult.",
            recoveryCondition: "Complete a favor for the NPC");
    }

    /// <summary>
    /// Creates the base social result for finalization.
    /// </summary>
    private static SocialResult CreateBaseSocialResult(
        NegotiationPositionTrack track,
        int dispositionChange,
        int reputationChange)
    {
        var outcome = track.Status == NegotiationStatus.DealReached
            ? SkillOutcome.FullSuccess
            : SkillOutcome.Failure;

        return new SocialResult(
            InteractionType: SocialInteractionType.Negotiation,
            TargetId: track.NpcId,
            Outcome: outcome,
            OutcomeDetails: new OutcomeDetails(
                outcomeType: outcome,
                margin: 0,
                isFumble: track.History.Any(r => r.IsFumble),
                isCritical: track.Status == NegotiationStatus.DealReached),
            DispositionChange: dispositionChange,
            NewDisposition: DispositionLevel.CreateNeutral(),
            ReputationChange: reputationChange,
            AffectedFactionId: null,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: Array.Empty<string>(),
            StressCost: 0,
            FumbleConsequence: null,
            NarrativeText: null);
    }

    /// <summary>
    /// Determines the overall outcome based on final position.
    /// </summary>
    private static SkillOutcome DetermineOverallOutcome(NegotiationPositionTrack track)
    {
        if (track.Status != NegotiationStatus.DealReached)
        {
            return track.History.Any(r => r.IsFumble)
                ? SkillOutcome.CriticalFailure
                : SkillOutcome.Failure;
        }

        return track.PcPosition switch
        {
            <= 2 => SkillOutcome.CriticalSuccess,
            <= 4 => SkillOutcome.FullSuccess,
            _ => SkillOutcome.MarginalSuccess
        };
    }

    #endregion
}
