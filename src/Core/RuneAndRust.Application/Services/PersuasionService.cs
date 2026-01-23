// ------------------------------------------------------------------------------
// <copyright file="PersuasionService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling persuasion attempts.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Service for handling persuasion attempts.
/// </summary>
/// <remarks>
/// <para>
/// Orchestrates the complete persuasion flow: building context, evaluating
/// argument alignment, performing skill checks, and applying outcomes.
/// </para>
/// </remarks>
public class PersuasionService : IPersuasionService
{
    private readonly ILogger<PersuasionService> _logger;

    // In-memory tracking for conversation state (would be persisted in full implementation)
    private readonly Dictionary<string, Dictionary<string, int>> _previousAttempts = new();
    private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _usedArguments = new();
    private readonly Dictionary<string, Dictionary<string, FumbleConsequence>> _trustShatteredConsequences = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PersuasionService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PersuasionService(ILogger<PersuasionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<PersuasionResult> AttemptPersuasionAsync(
        string characterId,
        string targetId,
        PersuasionRequest requestType,
        IReadOnlyList<string> argumentThemes,
        string? evidenceItemId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentNullException.ThrowIfNull(argumentThemes, nameof(argumentThemes));

        _logger.LogDebug(
            "Persuasion attempt initiated: {CharacterId} → {TargetId}, Request: {RequestType}, Themes: [{Themes}]",
            characterId, targetId, requestType, string.Join(", ", argumentThemes));

        // Check if persuasion is blocked
        if (await IsPersuasionBlockedAsync(characterId, targetId))
        {
            var consequence = await GetTrustShatteredConsequenceAsync(characterId, targetId);
            _logger.LogInformation(
                "Persuasion blocked by [Trust Shattered] for {CharacterId} → {TargetId}",
                characterId, targetId);

            return CreateBlockedResult(targetId, requestType, consequence);
        }

        // Build context
        var context = await BuildContextAsync(characterId, targetId, requestType, argumentThemes, evidenceItemId);

        _logger.LogDebug(
            "Persuasion context built: DC {EffectiveDc}, Dice modifier {DiceModifier}",
            context.EffectiveDc, context.TotalDiceModifier);

        // Simulate skill check (in full implementation, this would use IDiceRollerService)
        var (outcome, outcomeDetails) = SimulateSkillCheck(context);

        _logger.LogInformation(
            "Persuasion check result: {Outcome}, Margin: {Margin}",
            outcome, outcomeDetails.Margin);

        // Process result
        var result = ProcessOutcome(characterId, context, outcome, outcomeDetails);

        // Record attempt
        await RecordAttemptAsync(characterId, targetId, result);

        // Track used arguments
        TrackUsedArguments(characterId, targetId, argumentThemes);

        // If fumble, create [Trust Shattered] consequence
        if (result.IsFumble && result.FumbleConsequence != null)
        {
            StoreTrustShatteredConsequence(characterId, targetId, result.FumbleConsequence);
            _logger.LogWarning(
                "[Trust Shattered] consequence created: {CharacterId} → {TargetId}",
                characterId, targetId);
        }

        _logger.LogInformation(
            "Persuasion complete: {Outcome}, RequestGranted: {Granted}, Disposition: {DispositionChange}",
            result.Outcome, result.RequestGranted, result.DispositionChange);

        return result;
    }

    /// <inheritdoc/>
    public Task<PersuasionContext> BuildContextAsync(
        string characterId,
        string targetId,
        PersuasionRequest requestType,
        IReadOnlyList<string> argumentThemes,
        string? evidenceItemId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentNullException.ThrowIfNull(argumentThemes, nameof(argumentThemes));

        _logger.LogDebug(
            "Building persuasion context for {TargetId}, Request: {RequestType}",
            targetId, requestType);

        // Get disposition (simulated - would use INpcService in full implementation)
        var disposition = DispositionLevel.Create(25); // Default: NeutralPositive

        // Evaluate argument alignment (simulated)
        var alignment = EvaluateArgumentAlignmentSync(argumentThemes);

        // Check evidence (simulated)
        var hasEvidence = !string.IsNullOrWhiteSpace(evidenceItemId);
        var evidenceDescription = hasEvidence ? evidenceItemId : null;

        // Get previous attempts
        var previousAttempts = GetPreviousAttemptsSync(characterId, targetId);
        var sameArgumentUsed = CheckSameArgumentUsedSync(characterId, targetId, argumentThemes);

        var context = new PersuasionContext(
            RequestType: requestType,
            TargetId: targetId,
            TargetDisposition: disposition,
            TargetFactionId: null, // Would be populated from INpcService
            ArgumentAlignment: alignment,
            EvidenceProvided: hasEvidence,
            EvidenceDescription: evidenceDescription,
            NpcStressed: false,
            NpcFeared: false,
            NpcGrateful: false,
            PreviousAttempts: previousAttempts,
            SameArgumentUsed: sameArgumentUsed,
            PlayerFactionStanding: 0);

        _logger.LogDebug(
            "Context built: Base DC {BaseDc}, Effective DC {EffectiveDc}, Dice {DiceModifier}",
            context.BaseDc, context.EffectiveDc, context.TotalDiceModifier);

        return Task.FromResult(context);
    }

    /// <inheritdoc/>
    public Task<ArgumentAlignment> EvaluateArgumentAlignmentAsync(
        string targetId,
        IReadOnlyList<string> argumentThemes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentNullException.ThrowIfNull(argumentThemes, nameof(argumentThemes));

        var alignment = EvaluateArgumentAlignmentSync(argumentThemes);
        return Task.FromResult(alignment);
    }

    /// <inheritdoc/>
    public Task<bool> IsPersuasionBlockedAsync(string characterId, string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));

        var isBlocked = _trustShatteredConsequences.TryGetValue(characterId, out var targets)
            && targets.TryGetValue(targetId, out var consequence)
            && consequence.IsActive;

        return Task.FromResult(isBlocked);
    }

    /// <inheritdoc/>
    public Task<FumbleConsequence?> GetTrustShatteredConsequenceAsync(string characterId, string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));

        FumbleConsequence? consequence = null;
        if (_trustShatteredConsequences.TryGetValue(characterId, out var targets)
            && targets.TryGetValue(targetId, out var stored)
            && stored.IsActive)
        {
            consequence = stored;
        }

        return Task.FromResult(consequence);
    }

    /// <inheritdoc/>
    public Task<bool> AttemptTrustRecoveryAsync(
        string characterId,
        string targetId,
        string recoveryAction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(recoveryAction, nameof(recoveryAction));

        if (!_trustShatteredConsequences.TryGetValue(characterId, out var targets)
            || !targets.TryGetValue(targetId, out var consequence))
        {
            _logger.LogDebug("No [Trust Shattered] consequence found for {TargetId}", targetId);
            return Task.FromResult(true); // No consequence to recover from
        }

        // Check if recovery action matches condition
        if (consequence.RecoveryCondition != null
            && recoveryAction.Contains(consequence.RecoveryCondition, StringComparison.OrdinalIgnoreCase))
        {
            consequence.Deactivate("Recovery condition met", DateTime.UtcNow);
            _logger.LogInformation(
                "Trust recovered with {TargetId} via {Action}",
                targetId, recoveryAction);
            return Task.FromResult(true);
        }

        _logger.LogDebug(
            "Recovery action '{Action}' did not match condition '{Condition}'",
            recoveryAction, consequence.RecoveryCondition);
        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<int> GetPreviousAttemptsAsync(string characterId, string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));

        var count = GetPreviousAttemptsSync(characterId, targetId);
        return Task.FromResult(count);
    }

    /// <inheritdoc/>
    public Task RecordAttemptAsync(string characterId, string targetId, PersuasionResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));

        if (!result.RequestGranted)
        {
            // Increment failed attempt counter
            if (!_previousAttempts.ContainsKey(characterId))
            {
                _previousAttempts[characterId] = new Dictionary<string, int>();
            }
            if (!_previousAttempts[characterId].ContainsKey(targetId))
            {
                _previousAttempts[characterId][targetId] = 0;
            }
            _previousAttempts[characterId][targetId]++;

            _logger.LogDebug(
                "Failed attempt recorded: {CharacterId} → {TargetId}, Count: {Count}",
                characterId, targetId, _previousAttempts[characterId][targetId]);
        }
        else
        {
            // Reset on success
            if (_previousAttempts.TryGetValue(characterId, out var targetAttempts))
            {
                targetAttempts.Remove(targetId);
            }
            _logger.LogDebug("Attempt counter reset on success for {TargetId}", targetId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResetConversationStateAsync(string characterId, string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId, nameof(targetId));

        if (_previousAttempts.TryGetValue(characterId, out var attempts))
        {
            attempts.Remove(targetId);
        }
        if (_usedArguments.TryGetValue(characterId, out var arguments))
        {
            arguments.Remove(targetId);
        }

        _logger.LogDebug(
            "Conversation state reset: {CharacterId} → {TargetId}",
            characterId, targetId);

        return Task.CompletedTask;
    }

    #region Private Methods

    private int GetPreviousAttemptsSync(string characterId, string targetId)
    {
        if (_previousAttempts.TryGetValue(characterId, out var targetAttempts)
            && targetAttempts.TryGetValue(targetId, out var count))
        {
            return count;
        }
        return 0;
    }

    private bool CheckSameArgumentUsedSync(
        string characterId,
        string targetId,
        IReadOnlyList<string> argumentThemes)
    {
        if (!_usedArguments.TryGetValue(characterId, out var targetArgs)
            || !targetArgs.TryGetValue(targetId, out var usedThemes))
        {
            return false;
        }

        return argumentThemes.Any(t => usedThemes.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    private void TrackUsedArguments(
        string characterId,
        string targetId,
        IReadOnlyList<string> argumentThemes)
    {
        if (!_usedArguments.ContainsKey(characterId))
        {
            _usedArguments[characterId] = new Dictionary<string, HashSet<string>>();
        }
        if (!_usedArguments[characterId].ContainsKey(targetId))
        {
            _usedArguments[characterId][targetId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var theme in argumentThemes)
        {
            _usedArguments[characterId][targetId].Add(theme);
        }
    }

    private ArgumentAlignment EvaluateArgumentAlignmentSync(IReadOnlyList<string> argumentThemes)
    {
        // Simulated NPC values (would come from INpcService)
        var npcValues = new[] { "loyalty", "profit", "honor", "duty" };
        var npcDislikes = new[] { "betrayal", "cowardice", "dishonesty" };

        return ArgumentAlignment.Evaluate(argumentThemes, npcValues, npcDislikes);
    }

    private void StoreTrustShatteredConsequence(
        string characterId,
        string targetId,
        FumbleConsequence consequence)
    {
        if (!_trustShatteredConsequences.ContainsKey(characterId))
        {
            _trustShatteredConsequences[characterId] = new Dictionary<string, FumbleConsequence>();
        }
        _trustShatteredConsequences[characterId][targetId] = consequence;
    }

    private static (SkillOutcome, OutcomeDetails) SimulateSkillCheck(PersuasionContext context)
    {
        // Simulate a basic success for testing
        // In full implementation, this would use IDiceRollerService
        var margin = 2; // Simulated margin
        var outcome = SkillOutcome.FullSuccess;
        var details = new OutcomeDetails(outcome, margin, false, false);
        return (outcome, details);
    }

    private PersuasionResult ProcessOutcome(
        string characterId,
        PersuasionContext context,
        SkillOutcome outcome,
        OutcomeDetails details)
    {
        return outcome switch
        {
            SkillOutcome.CriticalSuccess => PersuasionResult.CreateSuccess(
                context.TargetId,
                context.RequestType,
                outcome,
                details,
                context.TargetDisposition,
                context.TargetFactionId,
                GetUnlockedOptions(ConvictionDepth.Deep),
                "Your words resonate deeply. The NPC is fully convinced."),

            SkillOutcome.ExceptionalSuccess or SkillOutcome.FullSuccess => PersuasionResult.CreateSuccess(
                context.TargetId,
                context.RequestType,
                outcome,
                details,
                context.TargetDisposition,
                context.TargetFactionId,
                GetUnlockedOptions(ConvictionDepth.Moderate),
                "Your argument finds purchase. The NPC agrees."),

            SkillOutcome.MarginalSuccess => PersuasionResult.CreateSuccess(
                context.TargetId,
                context.RequestType,
                outcome,
                details,
                context.TargetDisposition,
                context.TargetFactionId,
                narrativeText: "Barely convinced, the NPC agrees with reservations."),

            SkillOutcome.Failure => PersuasionResult.CreateFailure(
                context.TargetId,
                context.RequestType,
                outcome,
                details,
                context.TargetDisposition,
                context.TargetFactionId,
                new[] { "current_argument" },
                "Your words fall flat. The NPC shakes their head."),

            SkillOutcome.CriticalFailure => CreateTrustShatteredResult(characterId, context, details),

            _ => PersuasionResult.CreateFailure(
                context.TargetId,
                context.RequestType,
                outcome,
                details,
                context.TargetDisposition,
                context.TargetFactionId)
        };
    }

    private PersuasionResult CreateTrustShatteredResult(
        string characterId,
        PersuasionContext context,
        OutcomeDetails details)
    {
        var consequence = new FumbleConsequence(
            consequenceId: Guid.NewGuid().ToString(),
            characterId: characterId,
            skillId: "persuasion",
            consequenceType: FumbleType.TrustShattered,
            targetId: context.TargetId,
            appliedAt: DateTime.UtcNow,
            expiresAt: null, // No automatic expiry
            description: $"Your attempt at persuasion backfired catastrophically. {context.TargetId} will no longer listen to your arguments.",
            recoveryCondition: $"complete_quest_for_{context.TargetId}");

        return PersuasionResult.CreateTrustShattered(
            context.TargetId,
            context.RequestType,
            details,
            context.TargetDisposition,
            consequence,
            context.TargetFactionId,
            narrativeText: $"Your words come out completely wrong. {context.TargetId}'s expression hardens. 'We're done here.'");
    }

    private PersuasionResult CreateBlockedResult(
        string targetId,
        PersuasionRequest requestType,
        FumbleConsequence? consequence)
    {
        var disposition = DispositionLevel.Create(0);
        var details = new OutcomeDetails(SkillOutcome.Failure, -99, false, false);

        return new PersuasionResult(
            TargetId: targetId,
            RequestType: requestType,
            Outcome: SkillOutcome.Failure,
            OutcomeDetails: details,
            RequestGranted: false,
            ConvictionDepth: ConvictionDepth.None,
            DispositionChange: 0,
            NewDisposition: disposition,
            ReputationChange: 0,
            AffectedFactionId: null,
            UnlockedOptions: Array.Empty<string>(),
            LockedOptions: new[] { "*" },
            FumbleConsequence: consequence,
            NarrativeText: $"{targetId} refuses to speak with you. The trust between you has been shattered.");
    }

    private static IReadOnlyList<string> GetUnlockedOptions(ConvictionDepth depth)
    {
        return depth switch
        {
            ConvictionDepth.Deep => new[] { "request_favor", "request_information", "request_alliance" },
            ConvictionDepth.Strong => new[] { "request_favor", "request_information" },
            ConvictionDepth.Moderate => new[] { "request_favor" },
            _ => Array.Empty<string>()
        };
    }

    #endregion
}
