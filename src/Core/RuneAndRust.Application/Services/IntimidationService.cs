// ------------------------------------------------------------------------------
// <copyright file="IntimidationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of the intimidation service with Cost of Fear mechanics.
// Part of v0.15.3d Intimidation System implementation.
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
/// Implementation of the intimidation service with Cost of Fear mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Handles the complete intimidation flow including context building,
/// skill check resolution, mandatory reputation cost application, and
/// [Challenge Accepted] fumble consequence management.
/// </para>
/// <para>
/// Key features:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>Dual attribute choice: MIGHT (Physical) or WILL (Mental)</description>
///   </item>
///   <item>
///     <description>Mandatory reputation cost regardless of outcome</description>
///   </item>
///   <item>
///     <description>[Challenge Accepted] fumble with combat and [Furious] buff</description>
///   </item>
/// </list>
/// </remarks>
public class IntimidationService : IIntimidationService
{
    private readonly ILogger<IntimidationService> _logger;
    private readonly Random _random;

    // In-memory tracking (would be persisted in full implementation)
    private readonly Dictionary<string, HashSet<string>> _intimidatedNpcs = new();
    private readonly Dictionary<string, HashSet<string>> _furiousNpcs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IntimidationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public IntimidationService(ILogger<IntimidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <inheritdoc/>
    public async Task<IntimidationResult> AttemptIntimidationAsync(
        string characterId,
        string targetNpcId,
        IntimidationApproach approach)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNpcId, nameof(targetNpcId));

        _logger.LogInformation(
            "Character {CharacterId} attempting {Approach} intimidation against NPC {NpcId}",
            characterId, approach.GetDisplayName(), targetNpcId);

        // Build complete context
        var context = await BuildContextAsync(characterId, targetNpcId, approach);

        _logger.LogDebug(
            "Intimidation context: DC {EffectiveDc}, Player bonus +{PlayerBonus}d10, NPC bonus +{NpcBonus}d10",
            context.EffectiveDc, context.PlayerBonusDice, context.NpcBonusDice);

        // Perform the intimidation check
        return await IntimidateAsync(context);
    }

    /// <inheritdoc/>
    public async Task<IntimidationResult> IntimidateAsync(IntimidationContext context)
    {
        _logger.LogInformation(
            "Performing intimidation check against {TargetId} using {Approach}, DC {Dc}",
            context.TargetId, context.Approach.GetDisplayName(), context.EffectiveDc);

        // Simulate player roll: [Attribute] + Rhetoric + modifiers
        // Base pool: Attribute (e.g., MIGHT 3) + Rhetoric (2) = 5
        var basePool = 5;
        var playerPool = basePool + context.PlayerBonusDice;

        _logger.LogDebug(
            "Player rolling {BasePool}d10 base + {Bonus}d10 bonus = {Total}d10 ({Attribute})",
            basePool, context.PlayerBonusDice, playerPool, context.AttributeName);

        var (playerSuccesses, playerBotches) = SimulateRoll(playerPool);

        _logger.LogDebug(
            "Player rolled {Pool}d10: {Successes} successes, {Botches} botches",
            playerPool, playerSuccesses, playerBotches);

        // Check for fumble (0 successes + ≥1 botch)
        var isFumble = playerSuccesses == 0 && playerBotches >= 1;
        if (isFumble)
        {
            _logger.LogWarning(
                "Intimidation fumbled! [Challenge Accepted] consequence triggered against {TargetId}",
                context.TargetId);
            return await HandleFumbleAsync(context);
        }

        // Compare against DC
        var margin = playerSuccesses - context.EffectiveDc;
        var outcome = DetermineOutcome(margin, playerSuccesses);

        _logger.LogDebug(
            "Roll result: {Successes} successes vs DC {Dc}, margin {Margin}, outcome {Outcome}",
            playerSuccesses, context.EffectiveDc, margin, outcome);

        if (outcome >= SkillOutcome.MarginalSuccess)
        {
            return await HandleSuccessAsync(context, outcome, playerSuccesses);
        }
        else
        {
            return HandleFailure(context, playerSuccesses);
        }
    }

    /// <inheritdoc/>
    public Task<IntimidationContext> BuildContextAsync(
        string characterId,
        string targetNpcId,
        IntimidationApproach approach)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNpcId, nameof(targetNpcId));

        _logger.LogDebug(
            "Building intimidation context for {TargetId}, Approach: {Approach}",
            targetNpcId, approach.GetDisplayName());

        // Get NPC and player data (simulated for now)
        var playerLevel = 5;
        var npcLevel = 4;
        var targetType = IntimidationTarget.Common; // Default classification
        var factionId = "city_guard"; // Default faction

        // Calculate relative strength
        var relativeStrength = RelativeStrengthExtensions.FromLevels(playerLevel, npcLevel);

        // Check modifiers (simulated for now)
        var hasReputation = false;
        var wieldingArtifact = false;
        var intimidatingAlly = false;
        var targetHasBackup = false;

        var context = new IntimidationContext(
            TargetId: targetNpcId,
            TargetType: targetType,
            TargetFactionId: factionId,
            Approach: approach,
            RelativeStrength: relativeStrength,
            HasReputation: hasReputation,
            WieldingArtifact: wieldingArtifact,
            IntimidatingAlly: intimidatingAlly,
            TargetHasBackup: targetHasBackup,
            PlayerLevel: playerLevel,
            NpcLevel: npcLevel);

        _logger.LogDebug(
            "Context built: Target {TargetType} (DC {Dc}), Relative Strength {RelativeStrength}",
            context.TargetType.GetDisplayName(), context.EffectiveDc, context.RelativeStrength.GetDisplayName());

        return Task.FromResult(context);
    }

    /// <inheritdoc/>
    public Task<IntimidationTarget> ClassifyTargetAsync(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogDebug("Classifying intimidation target: {NpcId}", npcId);

        // In full implementation, would analyze NPC stats, role, combat experience
        // For now, return Common as default
        var targetType = IntimidationTarget.Common;

        _logger.LogDebug(
            "NPC {NpcId} classified as {TargetType} (DC {Dc})",
            npcId, targetType.GetDisplayName(), targetType.GetBaseDc());

        return Task.FromResult(targetType);
    }

    /// <inheritdoc/>
    public Task<RelativeStrength> CalculateRelativeStrengthAsync(string characterId, string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        // In full implementation, would get actual levels
        var playerLevel = 5;
        var npcLevel = 4;

        var relativeStrength = RelativeStrengthExtensions.FromLevels(playerLevel, npcLevel);

        _logger.LogDebug(
            "Relative strength {CharacterId} (level {PlayerLevel}) vs {NpcId} (level {NpcLevel}): {Result}",
            characterId, playerLevel, npcId, npcLevel, relativeStrength.GetDisplayName());

        return Task.FromResult(relativeStrength);
    }

    /// <inheritdoc/>
    public Task<bool> HasReputationBonusAsync(string characterId, string factionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(factionId, nameof(factionId));

        // In full implementation, would check faction standing for [Honored] or [Feared]
        // [Honored]: Standing >= 75, [Feared]: Standing <= -75
        var hasReputation = false;

        _logger.LogDebug(
            "Reputation bonus check for {CharacterId} with {FactionId}: {Result}",
            characterId, factionId, hasReputation);

        return Task.FromResult(hasReputation);
    }

    /// <inheritdoc/>
    public Task<bool> IsWieldingArtifactAsync(string characterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));

        // In full implementation, would check equipped weapon for [Artifact] flag
        var wieldingArtifact = false;

        _logger.LogDebug(
            "Artifact check for {CharacterId}: {Result}",
            characterId, wieldingArtifact);

        return Task.FromResult(wieldingArtifact);
    }

    /// <inheritdoc/>
    public Task<bool> HasIntimidatingAllyAsync(string characterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));

        // In full implementation, would check party for Kriger, summoned creatures, etc.
        var hasIntimidatingAlly = false;

        _logger.LogDebug(
            "Intimidating ally check for {CharacterId}: {Result}",
            characterId, hasIntimidatingAlly);

        return Task.FromResult(hasIntimidatingAlly);
    }

    /// <inheritdoc/>
    public Task<bool> TargetHasBackupAsync(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        // In full implementation, would check for visible NPC allies nearby
        var hasBackup = false;

        _logger.LogDebug(
            "Backup check for NPC {NpcId}: {Result}",
            npcId, hasBackup);

        return Task.FromResult(hasBackup);
    }

    /// <inheritdoc/>
    public Task<string?> GetTargetFactionAsync(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        // In full implementation, would look up NPC's faction
        var factionId = "city_guard";

        _logger.LogDebug(
            "Faction lookup for NPC {NpcId}: {FactionId}",
            npcId, factionId);

        return Task.FromResult<string?>(factionId);
    }

    /// <inheritdoc/>
    public CostOfFear CalculateCostOfFear(SkillOutcome outcome, string factionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(factionId, nameof(factionId));

        var costOfFear = new CostOfFear(outcome, factionId);

        _logger.LogDebug(
            "Cost of Fear calculated: {Outcome} = {Cost} reputation with {FactionId}",
            outcome, costOfFear.ReputationCost, factionId);

        return costOfFear;
    }

    /// <inheritdoc/>
    public Task ApplyFuriousEffectAsync(string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogInformation(
            "Applying [Furious] status effect to NPC {NpcId} (+2d10 damage, entire combat)",
            npcId);

        // Track furious status (in full implementation, would use StatusEffectService)
        if (!_furiousNpcs.ContainsKey("current_combat"))
        {
            _furiousNpcs["current_combat"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        _furiousNpcs["current_combat"].Add(npcId);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task InitiateChallengeAcceptedCombatAsync(
        string characterId,
        string npcId,
        int npcInitiativeBonus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        _logger.LogWarning(
            "[Challenge Accepted] Combat initiated between {CharacterId} and {NpcId}. " +
            "NPC gains +{InitiativeBonus} initiative and will NOT accept surrender.",
            characterId, npcId, npcInitiativeBonus);

        // In full implementation, would:
        // 1. Initiate combat via CombatService
        // 2. Apply NPC initiative bonus
        // 3. Set flag that NPC will not accept surrender

        return Task.CompletedTask;
    }

    #region Private Methods

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

    private SkillOutcome DetermineOutcome(int margin, int successes)
    {
        // Margin is successes - DC
        if (successes >= 5 && margin >= 5)
            return SkillOutcome.CriticalSuccess;
        if (margin >= 3)
            return SkillOutcome.ExceptionalSuccess;
        if (margin >= 1)
            return SkillOutcome.FullSuccess;
        if (margin == 0)
            return SkillOutcome.MarginalSuccess;
        return SkillOutcome.Failure;
    }

    private Task<IntimidationResult> HandleSuccessAsync(
        IntimidationContext context,
        SkillOutcome outcome,
        int playerSuccesses)
    {
        _logger.LogInformation(
            "Intimidation succeeded against {TargetId} with outcome {Outcome}",
            context.TargetId, outcome);

        // Track intimidated NPC
        MarkNpcAsIntimidated(context.TargetId);

        // Apply mandatory reputation cost
        var costOfFear = CalculateCostOfFear(outcome, context.TargetFactionId);

        _logger.LogInformation(
            "Cost of Fear applied: {Cost} reputation with {FactionId}",
            costOfFear.ReputationCost, context.TargetFactionId);

        // Determine unlocked options based on success level
        var unlockedOptions = outcome switch
        {
            SkillOutcome.CriticalSuccess => new[]
            {
                "full_compliance",
                "additional_information",
                "target_offers_help",
                "spread_fear"
            },
            SkillOutcome.ExceptionalSuccess => new[]
            {
                "full_compliance",
                "additional_information"
            },
            SkillOutcome.FullSuccess => new[]
            {
                "compliance"
            },
            _ => Array.Empty<string>()
        };

        return Task.FromResult(IntimidationResult.Success(
            outcome: outcome,
            factionId: context.TargetFactionId,
            playerSuccesses: playerSuccesses,
            dc: context.EffectiveDc,
            unlockedOptions: unlockedOptions));
    }

    private IntimidationResult HandleFailure(
        IntimidationContext context,
        int playerSuccesses)
    {
        _logger.LogInformation(
            "Intimidation failed against {TargetId}. Target refused to be cowed.",
            context.TargetId);

        // Apply mandatory reputation cost (even on failure)
        var costOfFear = CalculateCostOfFear(SkillOutcome.Failure, context.TargetFactionId);

        _logger.LogInformation(
            "Cost of Fear applied (failure): {Cost} reputation with {FactionId}",
            costOfFear.ReputationCost, context.TargetFactionId);

        return IntimidationResult.Failure(
            factionId: context.TargetFactionId,
            playerSuccesses: playerSuccesses,
            dc: context.EffectiveDc);
    }

    private async Task<IntimidationResult> HandleFumbleAsync(IntimidationContext context)
    {
        _logger.LogWarning(
            "[Challenge Accepted] Fumble handling for intimidation against {TargetId}",
            context.TargetId);

        // Create fumble consequence
        var fumbleConsequence = new FumbleConsequence(
            consequenceId: $"challenge_accepted_{Guid.NewGuid():N}",
            characterId: "player", // In full implementation, would be actual character ID
            skillId: "rhetoric_intimidation",
            consequenceType: FumbleType.ChallengeAccepted,
            targetId: context.TargetId,
            appliedAt: DateTime.UtcNow,
            expiresAt: null, // Lasts entire combat, no automatic expiration
            description: "[Challenge Accepted] - Target refuses to be cowed and initiates combat with [Furious] buff.",
            recoveryCondition: "Combat ends");

        // Apply [Furious] status effect
        await ApplyFuriousEffectAsync(context.TargetId);

        // Initiate combat
        await InitiateChallengeAcceptedCombatAsync(
            "player",
            context.TargetId,
            IntimidationResult.FumbleInitiativeBonus);

        // Apply reputation cost
        var costOfFear = CalculateCostOfFear(SkillOutcome.CriticalFailure, context.TargetFactionId);

        _logger.LogWarning(
            "Cost of Fear applied (fumble): {Cost} reputation with {FactionId}. " +
            "Combat initiated. NPC has [Furious] and will not accept surrender.",
            costOfFear.ReputationCost, context.TargetFactionId);

        return IntimidationResult.Fumble(
            factionId: context.TargetFactionId,
            playerSuccesses: 0,
            dc: context.EffectiveDc,
            fumbleConsequence: fumbleConsequence);
    }

    private void MarkNpcAsIntimidated(string npcId)
    {
        if (!_intimidatedNpcs.ContainsKey("player"))
        {
            _intimidatedNpcs["player"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        _intimidatedNpcs["player"].Add(npcId);

        _logger.LogDebug("Marked NPC {NpcId} as intimidated", npcId);
    }

    #endregion
}
