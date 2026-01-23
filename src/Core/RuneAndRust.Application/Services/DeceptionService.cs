// ------------------------------------------------------------------------------
// <copyright file="DeceptionService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of the deception service with opposed roll mechanics.
// Part of v0.15.3c Deception System implementation.
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
/// Implementation of the deception service with opposed roll mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Handles the complete deception flow including context building,
/// opposed roll resolution, Liar's Burden stress application, and
/// [Lie Exposed] fumble consequence management.
/// </para>
/// <para>
/// Deception uses opposed rolls: Player (WILL + Rhetoric) vs. NPC (WITS).
/// </para>
/// </remarks>
public class DeceptionService : IDeceptionService
{
    private readonly ILogger<DeceptionService> _logger;
    private readonly Random _random;

    // In-memory tracking (would be persisted in full implementation)
    private readonly Dictionary<string, HashSet<string>> _untrustworthyFlags = new();
    private readonly Dictionary<string, HashSet<string>> _fooledNpcs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeceptionService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DeceptionService(ILogger<DeceptionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <inheritdoc/>
    public async Task<DeceptionResult> AttemptDeceptionAsync(
        string characterId,
        string targetNpcId,
        LieComplexity lieComplexity,
        CoverStoryQuality coverStoryQuality = CoverStoryQuality.None,
        bool hasForgedDocuments = false,
        int forgedDocumentQuality = 0,
        bool lieContainsTruth = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNpcId, nameof(targetNpcId));

        _logger.LogInformation(
            "Character {CharacterId} attempting {LieComplexity} deception against NPC {NpcId}",
            characterId, lieComplexity, targetNpcId);

        // Build complete context
        var context = await BuildContextAsync(
            characterId,
            targetNpcId,
            lieComplexity,
            coverStoryQuality,
            hasForgedDocuments,
            forgedDocumentQuality,
            lieContainsTruth);

        _logger.LogDebug(
            "Deception context: DC {EffectiveDc}, Player modifiers {PlayerDiceModifier}, NPC pool {NpcDicePool}",
            context.EffectiveDc, context.PlayerDiceModifier, context.NpcDicePool);

        // Simulate player roll (WILL + Rhetoric + modifiers)
        var playerPool = 5 + context.PlayerDiceModifier; // Base 5 = WILL 3 + Rhetoric 2
        var (playerSuccesses, playerBotches) = SimulateRoll(playerPool);

        _logger.LogDebug(
            "Player rolled {Pool}d10: {Successes} successes, {Botches} botches",
            playerPool, playerSuccesses, playerBotches);

        // Check for fumble (0 successes + ≥1 botch)
        var isFumble = playerSuccesses == 0 && playerBotches >= 1;
        if (isFumble)
        {
            _logger.LogWarning(
                "Deception fumbled! [Lie Exposed] consequence triggered for {CharacterId}",
                characterId);
            return await HandleFumbleAsync(characterId, targetNpcId, context, lieComplexity);
        }

        // Simulate NPC opposed roll
        var (npcSuccesses, _) = SimulateRoll(context.NpcDicePool);

        _logger.LogDebug(
            "NPC rolled {Pool}d10: {Successes} successes",
            context.NpcDicePool, npcSuccesses);

        // Compare successes
        var margin = playerSuccesses - npcSuccesses;
        var outcome = DetermineOutcome(margin, playerSuccesses);

        // Calculate and apply Liar's Burden
        var liarsBurden = CalculateLiarsBurden(outcome, false);

        _logger.LogDebug(
            "Applied {Amount} Psychic Stress to {CharacterId} from Liar's Burden",
            liarsBurden.TotalStressCost, characterId);

        if (outcome >= SkillOutcome.MarginalSuccess)
        {
            return await HandleSuccessAsync(
                characterId, targetNpcId, context, outcome,
                playerSuccesses, npcSuccesses, liarsBurden.TotalStressCost);
        }
        else
        {
            return HandleFailure(playerSuccesses, npcSuccesses, liarsBurden.TotalStressCost);
        }
    }

    /// <inheritdoc/>
    public Task<DeceptionContext> BuildContextAsync(
        string characterId,
        string targetNpcId,
        LieComplexity lieComplexity,
        CoverStoryQuality coverStoryQuality = CoverStoryQuality.None,
        bool hasForgedDocuments = false,
        int forgedDocumentQuality = 0,
        bool lieContainsTruth = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNpcId, nameof(targetNpcId));

        _logger.LogDebug(
            "Building deception context for {TargetId}, Lie: {LieComplexity}",
            targetNpcId, lieComplexity);

        // Get NPC states (simulated for now)
        var npcWits = 3; // Default WITS
        var disposition = DispositionLevel.Create(0); // Neutral
        var npcSuspicious = false;
        var npcTrusting = false;
        var npcTrainedObserver = false;
        var npcPreviouslyFooled = WasNpcPreviouslyFooledSync(characterId, targetNpcId);
        var npcHasAlert = false;
        var npcHasFatigued = false;
        var npcKnowsPlayer = false;
        var evidenceContradicts = false;
        var npcIsDistracted = false;
        var npcIsIntoxicated = false;
        var hasUntrustworthy = HasUntrustworthyFlagSync(characterId, targetNpcId);
        var factionStanding = 0;

        var context = new DeceptionContext(
            LieComplexity: lieComplexity,
            TargetId: targetNpcId,
            TargetWits: npcWits,
            TargetDisposition: disposition,
            NpcSuspicious: npcSuspicious,
            NpcTrusting: npcTrusting,
            NpcTrainedObserver: npcTrainedObserver,
            NpcPreviouslyFooled: npcPreviouslyFooled,
            NpcHasAlert: npcHasAlert,
            NpcHasFatigued: npcHasFatigued,
            NpcKnowsPlayer: npcKnowsPlayer,
            EvidenceContradicts: evidenceContradicts,
            ContradictingEvidenceDescription: null,
            CoverStoryQuality: coverStoryQuality,
            HasForgedDocuments: hasForgedDocuments,
            ForgedDocumentQuality: forgedDocumentQuality,
            LieContainsTruth: lieContainsTruth,
            NpcIsDistracted: npcIsDistracted,
            NpcIsIntoxicated: npcIsIntoxicated,
            PlayerHasUntrustworthy: hasUntrustworthy,
            PlayerFactionStanding: factionStanding);

        _logger.LogDebug(
            "Context built: Base DC {BaseDc}, Effective DC {EffectiveDc}, NPC Pool {NpcDicePool}",
            context.BaseDc, context.EffectiveDc, context.NpcDicePool);

        return Task.FromResult(context);
    }

    /// <inheritdoc/>
    public Task<bool> HasUntrustworthyFlagAsync(string characterId, string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        var hasFlag = HasUntrustworthyFlagSync(characterId, npcId);
        return Task.FromResult(hasFlag);
    }

    /// <inheritdoc/>
    public Task<bool> TryRemoveUntrustworthyFlagAsync(
        string characterId,
        string npcId,
        UntrustworthyRemovalReason removalReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        if (!HasUntrustworthyFlagSync(characterId, npcId))
        {
            _logger.LogDebug("No [Untrustworthy] flag found for {NpcId}", npcId);
            return Task.FromResult(true); // No flag to remove
        }

        // In full implementation, would validate conditions
        // For now, accept all removal reasons
        if (_untrustworthyFlags.TryGetValue(characterId, out var npcs))
        {
            npcs.Remove(npcId);
            _logger.LogInformation(
                "Removed [Untrustworthy] flag between {CharacterId} and {NpcId} due to {Reason}",
                characterId, npcId, removalReason);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public LiarsBurden CalculateLiarsBurden(SkillOutcome outcome, bool isFumble)
    {
        return new LiarsBurden(outcome, isFumble);
    }

    /// <inheritdoc/>
    public Task MarkNpcAsFooledAsync(string characterId, string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        if (!_fooledNpcs.ContainsKey(characterId))
        {
            _fooledNpcs[characterId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        _fooledNpcs[characterId].Add(npcId);

        _logger.LogDebug("Marked NPC {NpcId} as fooled by {CharacterId}", npcId, characterId);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> WasNpcPreviouslyFooledAsync(string characterId, string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        var wasFooled = WasNpcPreviouslyFooledSync(characterId, npcId);
        return Task.FromResult(wasFooled);
    }

    #region Private Methods

    private bool HasUntrustworthyFlagSync(string characterId, string npcId)
    {
        return _untrustworthyFlags.TryGetValue(characterId, out var npcs)
            && npcs.Contains(npcId);
    }

    private bool WasNpcPreviouslyFooledSync(string characterId, string npcId)
    {
        return _fooledNpcs.TryGetValue(characterId, out var npcs)
            && npcs.Contains(npcId);
    }

    private void ApplyUntrustworthyFlag(string characterId, string npcId)
    {
        if (!_untrustworthyFlags.ContainsKey(characterId))
        {
            _untrustworthyFlags[characterId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        _untrustworthyFlags[characterId].Add(npcId);

        _logger.LogInformation(
            "[Untrustworthy] flag applied between {CharacterId} and {NpcId}",
            characterId, npcId);
    }

    private (int Successes, int Botches) SimulateRoll(int pool)
    {
        var successes = 0;
        var botches = 0;

        for (var i = 0; i < pool; i++)
        {
            var roll = _random.Next(1, 11); // 1-10
            if (roll >= 8) successes++;
            if (roll == 1) botches++;
        }

        return (successes, botches);
    }

    private SkillOutcome DetermineOutcome(int margin, int playerSuccesses)
    {
        if (playerSuccesses >= 5 && margin >= 5)
            return SkillOutcome.CriticalSuccess;
        if (margin >= 3)
            return SkillOutcome.ExceptionalSuccess;
        if (margin >= 1)
            return SkillOutcome.FullSuccess;
        if (margin == 0)
            return SkillOutcome.MarginalSuccess;
        return SkillOutcome.Failure;
    }

    private async Task<DeceptionResult> HandleSuccessAsync(
        string characterId,
        string targetNpcId,
        DeceptionContext context,
        SkillOutcome outcome,
        int playerSuccesses,
        int npcSuccesses,
        int stressCost)
    {
        _logger.LogInformation("Deception succeeded with outcome {Outcome}", outcome);

        // Mark NPC as fooled
        await MarkNpcAsFooledAsync(characterId, targetNpcId);

        // Determine unlocked options based on success level
        var unlockedOptions = outcome switch
        {
            SkillOutcome.CriticalSuccess => new[] { "full_access", "additional_info", "npc_vouches" },
            SkillOutcome.ExceptionalSuccess => new[] { "full_access", "additional_info" },
            SkillOutcome.FullSuccess => new[] { "access_granted" },
            _ => Array.Empty<string>()
        };

        return DeceptionResult.Success(
            outcome: outcome,
            playerSuccesses: playerSuccesses,
            npcSuccesses: npcSuccesses,
            stressCost: stressCost,
            unlockedOptions: unlockedOptions);
    }

    private DeceptionResult HandleFailure(
        int playerSuccesses,
        int npcSuccesses,
        int stressCost)
    {
        _logger.LogInformation("Deception failed, NPC becomes suspicious");

        return DeceptionResult.Failure(
            playerSuccesses: playerSuccesses,
            npcSuccesses: npcSuccesses,
            stressCost: stressCost,
            dispositionChange: -10,
            lockedOptions: new[] { "current_approach" });
    }

    private Task<DeceptionResult> HandleFumbleAsync(
        string characterId,
        string targetNpcId,
        DeceptionContext context,
        LieComplexity lieComplexity)
    {
        // Calculate fumble stress
        var liarsBurden = CalculateLiarsBurden(SkillOutcome.CriticalFailure, true);

        _logger.LogDebug(
            "Applied {Amount} Psychic Stress to {CharacterId} from Liar's Burden (fumble)",
            liarsBurden.TotalStressCost, characterId);

        // Apply [Untrustworthy] flag
        ApplyUntrustworthyFlag(characterId, targetNpcId);

        // Create fumble consequence
        var fumbleConsequence = new FumbleConsequence(
            consequenceId: Guid.NewGuid().ToString(),
            characterId: characterId,
            skillId: "deception",
            consequenceType: FumbleType.LieExposed,
            targetId: targetNpcId,
            appliedAt: DateTime.UtcNow,
            expiresAt: null,
            description: $"Your lie to {targetNpcId} was completely exposed. They will never trust your word again.",
            recoveryCondition: $"benefit_{targetNpcId}");

        // Determine additional consequences
        var combatInitiated = DetermineCombatInitiation(lieComplexity);
        var (settlementBan, banDuration) = DetermineSettlementBan(lieComplexity);

        if (combatInitiated)
        {
            _logger.LogInformation(
                "Combat initiated with NPC {NpcId} due to [Lie Exposed]",
                targetNpcId);
        }

        if (settlementBan)
        {
            var durationText = banDuration > 0 ? $"{banDuration} days" : "permanently";
            _logger.LogWarning(
                "Character {CharacterId} banned from settlement for {Duration}",
                characterId, durationText);
        }

        return Task.FromResult(DeceptionResult.Fumble(
            playerSuccesses: 0,
            npcSuccesses: 0,
            stressCost: liarsBurden.TotalStressCost,
            fumbleConsequence: fumbleConsequence,
            combatInitiated: combatInitiated,
            settlementBan: settlementBan,
            banDuration: banDuration,
            questFailed: false,
            failedQuestId: null));
    }

    private bool DetermineCombatInitiation(LieComplexity lieComplexity)
    {
        // Base 50% chance, modified by lie complexity
        var combatChance = 50 + lieComplexity.GetCombatChanceModifier();
        var roll = _random.Next(1, 101);
        return roll <= combatChance;
    }

    private (bool Ban, int Duration) DetermineSettlementBan(LieComplexity lieComplexity)
    {
        // 30% base chance for settlement ban
        var banChance = lieComplexity >= LieComplexity.Unlikely ? 50 : 30;
        var roll = _random.Next(1, 101);

        if (roll > banChance)
            return (false, 0);

        // Duration: 1-6 days, or permanent for very low rolls on Outrageous lies
        var duration = lieComplexity == LieComplexity.Outrageous && roll <= 5
            ? 0 // Permanent
            : _random.Next(1, 7);

        return (true, duration);
    }

    #endregion
}
