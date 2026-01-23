// ------------------------------------------------------------------------------
// <copyright file="RhetoricSpecializationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of rhetoric specialization ability logic for archetype bonuses.
// Part of v0.15.3i Specialization Integration implementation.
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
/// Implementation of rhetoric specialization ability logic.
/// </summary>
/// <remarks>
/// <para>
/// This service implements the Specialization Bonus Hook Pattern for Rhetoric skills,
/// enabling archetype-specific abilities to modify social interaction mechanics.
/// </para>
/// <para>
/// Supported archetypes and abilities:
/// <list type="bullet">
///   <item><description>Thul (Scholar): Voice of Reason, Scholarly Authority</description></item>
///   <item><description>Skald (Bard): Inspiring Words, Saga of Heroes</description></item>
///   <item><description>Kupmaðr (Merchant): Silver Tongue, Sniff Out Lies</description></item>
///   <item><description>Myrk-gengr (Infiltrator): Maintain Cover, Forge Documents</description></item>
/// </list>
/// </para>
/// </remarks>
public class RhetoricSpecializationService : IRhetoricSpecializationService
{
    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    /// <summary>
    /// The logger instance for comprehensive logging of ability operations.
    /// </summary>
    private readonly ILogger<RhetoricSpecializationService> _logger;

    /// <summary>
    /// The skill service for performing skill checks.
    /// </summary>
    private readonly ISkillService _skillService;

    /// <summary>
    /// The dice service for random number generation.
    /// </summary>
    private readonly IDiceService _diceService;

    /// <summary>
    /// Player lookup function to retrieve player entities by ID.
    /// </summary>
    private readonly Func<Guid, Player?> _playerLookup;

    // -------------------------------------------------------------------------
    // Constants - Archetype IDs
    // -------------------------------------------------------------------------

    /// <summary>
    /// Archetype ID for Thul (Scholar) specialization.
    /// </summary>
    private const string ThulArchetypeId = "thul";

    /// <summary>
    /// Archetype ID for Skald (Bard) specialization.
    /// </summary>
    private const string SkaldArchetypeId = "skald";

    /// <summary>
    /// Archetype ID for Kupmaðr (Merchant) specialization.
    /// </summary>
    private const string KupmadrArchetypeId = "kupmadr";

    /// <summary>
    /// Archetype ID for Myrk-gengr (Infiltrator) specialization.
    /// </summary>
    private const string MyrkGengrArchetypeId = "myrk-gengr";

    // -------------------------------------------------------------------------
    // Constants - Ability Parameters
    // -------------------------------------------------------------------------

    /// <summary>
    /// DC threshold for [Silver Tongue] auto-success.
    /// </summary>
    private const int SilverTongueDcThreshold = 12;

    /// <summary>
    /// Dice bonus for [Scholarly Authority] ability.
    /// </summary>
    private const int ScholarlyAuthorityBonus = 2;

    /// <summary>
    /// Dice bonus for [Sniff Out Lies] ability.
    /// </summary>
    private const int SniffOutLiesBonus = 2;

    /// <summary>
    /// Dice bonus for [Maintain Cover] ability.
    /// </summary>
    private const int MaintainCoverBonus = 2;

    /// <summary>
    /// DC for [Inspiring Words] rhetoric check.
    /// </summary>
    private const int InspiringWordsDc = 12;

    /// <summary>
    /// Dice bonus granted to inspired allies.
    /// </summary>
    private const int InspiringWordsAllyBonus = 1;

    /// <summary>
    /// DC for [Saga of Heroes] storytelling check.
    /// </summary>
    private const int SagaOfHeroesDc = 10;

    /// <summary>
    /// Net successes required for exceptional [Inspiring Words] (party-wide).
    /// </summary>
    private const int ExceptionalSuccessThreshold = 3;

    /// <summary>
    /// Topic tags that trigger [Scholarly Authority].
    /// </summary>
    private static readonly HashSet<string> LoreTopicTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "lore", "history", "academic", "scholarly", "ancient",
        "archives", "records", "traditions", "cultural", "mythology"
    };

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RhetoricSpecializationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for comprehensive logging.</param>
    /// <param name="skillService">The skill service for skill checks.</param>
    /// <param name="diceService">The dice service for random number generation.</param>
    /// <param name="playerLookup">Function to look up players by ID.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public RhetoricSpecializationService(
        ILogger<RhetoricSpecializationService> logger,
        ISkillService skillService,
        IDiceService diceService,
        Func<Guid, Player?> playerLookup)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _skillService = skillService ?? throw new ArgumentNullException(nameof(skillService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _playerLookup = playerLookup ?? throw new ArgumentNullException(nameof(playerLookup));

        _logger.LogInformation(
            "RhetoricSpecializationService initialized with 4 archetypes and 8 abilities");
    }

    // -------------------------------------------------------------------------
    // Pre-Check Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public SpecializationAbilityEffect GetPreCheckBonus(
        Guid playerId,
        SocialInteractionType interactionType,
        SocialContext context)
    {
        var archetypeId = GetPlayerArchetype(playerId);

        if (string.IsNullOrEmpty(archetypeId))
        {
            _logger.LogDebug(
                "No archetype found for player {PlayerId}, no pre-check bonus applied",
                playerId);
            return SpecializationAbilityEffect.None;
        }

        _logger.LogDebug(
            "Checking pre-check bonus for {Archetype} player {PlayerId} on {InteractionType}",
            archetypeId, playerId, interactionType);

        return archetypeId.ToLowerInvariant() switch
        {
            ThulArchetypeId => GetThulPreCheckBonus(playerId, context),
            SkaldArchetypeId => SpecializationAbilityEffect.None, // Skald bonuses are party-focused
            KupmadrArchetypeId => GetKupmadrPreCheckBonus(playerId, context),
            MyrkGengrArchetypeId => GetMyrkGengrPreCheckBonus(playerId, context),
            _ => SpecializationAbilityEffect.None
        };
    }

    /// <summary>
    /// Gets pre-check bonus for Thul archetype.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <param name="context">The social context.</param>
    /// <returns>Effect with dice bonus if applicable.</returns>
    private SpecializationAbilityEffect GetThulPreCheckBonus(
        Guid playerId,
        SocialContext context)
    {
        // [Scholarly Authority]: +2d10 when discussing lore/history
        if (IsLoreOrHistoryTopic(context))
        {
            _logger.LogDebug(
                "[Scholarly Authority] triggered for player {PlayerId} - topic matches lore/history",
                playerId);

            _logger.LogInformation(
                "[Scholarly Authority] applied: +{Bonus}d10 for lore/history topic",
                ScholarlyAuthorityBonus);

            return SpecializationAbilityEffect.DiceBonusEffect(
                RhetoricSpecializationAbility.ScholarlyAuthority,
                ScholarlyAuthorityBonus,
                "Your scholarly expertise lends weight to your words.");
        }

        return SpecializationAbilityEffect.None;
    }

    /// <summary>
    /// Gets pre-check bonus for Kupmaðr archetype.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <param name="context">The social context.</param>
    /// <returns>Effect with dice bonus if applicable.</returns>
    private SpecializationAbilityEffect GetKupmadrPreCheckBonus(
        Guid playerId,
        SocialContext context)
    {
        // [Sniff Out Lies]: +2d10 when detecting deception
        // Check if any situational modifier indicates deception detection
        var isDeceptionDetection = context.SituationalModifiers
            .Any(m => (m.Source?.Contains("deception", StringComparison.OrdinalIgnoreCase) ?? false) ||
                      (m.Description?.Contains("detect", StringComparison.OrdinalIgnoreCase) ?? false));

        if (isDeceptionDetection)
        {
            _logger.LogDebug(
                "[Sniff Out Lies] triggered for player {PlayerId} - deception detection context",
                playerId);

            _logger.LogInformation(
                "[Sniff Out Lies] applied: +{Bonus}d10 for deception detection",
                SniffOutLiesBonus);

            return SpecializationAbilityEffect.DiceBonusEffect(
                RhetoricSpecializationAbility.SniffOutLies,
                SniffOutLiesBonus,
                "Your merchant's instincts detect something false.");
        }

        return SpecializationAbilityEffect.None;
    }

    /// <summary>
    /// Gets pre-check bonus for Myrk-gengr archetype.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <param name="context">The social context.</param>
    /// <returns>Effect with dice bonus if applicable.</returns>
    private SpecializationAbilityEffect GetMyrkGengrPreCheckBonus(
        Guid playerId,
        SocialContext context)
    {
        // [Maintain Cover]: +2d10 when cover is challenged
        // Check if any situational modifier indicates cover challenge
        var isCoverChallenged = context.SituationalModifiers
            .Any(m => (m.Source?.Contains("cover", StringComparison.OrdinalIgnoreCase) ?? false) ||
                      (m.Description?.Contains("challenge", StringComparison.OrdinalIgnoreCase) ?? false) ||
                      (m.Description?.Contains("identity", StringComparison.OrdinalIgnoreCase) ?? false));

        if (isCoverChallenged)
        {
            _logger.LogDebug(
                "[Maintain Cover] triggered for player {PlayerId} - cover challenged",
                playerId);

            _logger.LogInformation(
                "[Maintain Cover] applied: +{Bonus}d10, -1 Stress, fumble downgrade",
                MaintainCoverBonus);

            return SpecializationAbilityEffect.MaintainCoverEffect(
                "Your training allows you to stay calm and in character.");
        }

        return SpecializationAbilityEffect.None;
    }

    /// <inheritdoc/>
    public bool CanAutoSucceed(
        Guid playerId,
        SocialInteractionType interactionType,
        int finalDc)
    {
        var archetypeId = GetPlayerArchetype(playerId);

        // [Silver Tongue]: Auto-succeed negotiation DC ≤ 12 (Kupmaðr only)
        if (archetypeId?.Equals(KupmadrArchetypeId, StringComparison.OrdinalIgnoreCase) == true &&
            interactionType == SocialInteractionType.Negotiation &&
            finalDc <= SilverTongueDcThreshold)
        {
            _logger.LogDebug(
                "[Silver Tongue] checking auto-success for player {PlayerId} - DC {Dc} vs threshold {Threshold}",
                playerId, finalDc, SilverTongueDcThreshold);

            _logger.LogInformation(
                "[Silver Tongue] auto-succeeding negotiation for player {PlayerId} - DC {Dc} ≤ {Threshold}",
                playerId, finalDc, SilverTongueDcThreshold);

            return true;
        }

        return false;
    }

    // -------------------------------------------------------------------------
    // Post-Check Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public SpecializationAbilityEffect ApplyPostCheckModification(
        Guid playerId,
        SocialResult result,
        SocialContext context)
    {
        var archetypeId = GetPlayerArchetype(playerId);

        if (string.IsNullOrEmpty(archetypeId))
        {
            return SpecializationAbilityEffect.None;
        }

        _logger.LogDebug(
            "Checking post-check modification for {Archetype} player {PlayerId} with outcome {Outcome}",
            archetypeId, playerId, result.Outcome);

        // [Voice of Reason]: Failed persuasion doesn't lock options (Thul only)
        if (archetypeId.Equals(ThulArchetypeId, StringComparison.OrdinalIgnoreCase) &&
            context.InteractionType == SocialInteractionType.Persuasion &&
            result.Outcome == SkillOutcome.Failure) // Regular failure only, not fumble
        {
            _logger.LogDebug(
                "[Voice of Reason] triggered for player {PlayerId} - persuasion failure",
                playerId);

            _logger.LogInformation(
                "[Voice of Reason] preventing option lock on persuasion failure for player {PlayerId}",
                playerId);

            return SpecializationAbilityEffect.PreventLockEffect(
                RhetoricSpecializationAbility.VoiceOfReason,
                "Your reasoned approach leaves the conversation open for future attempts.");
        }

        return SpecializationAbilityEffect.None;
    }

    // -------------------------------------------------------------------------
    // Skald Party Actions
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<SpecializationAbilityEffect> ApplyInspirationAsync(
        Guid skaldId,
        IEnumerable<Guid> targetIds)
    {
        var archetypeId = GetPlayerArchetype(skaldId);
        if (!archetypeId?.Equals(SkaldArchetypeId, StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(
                "ApplyInspiration called for non-Skald player {PlayerId} (archetype: {Archetype})",
                skaldId, archetypeId ?? "none");
            return SpecializationAbilityEffect.None;
        }

        var player = _playerLookup(skaldId);
        if (player == null)
        {
            _logger.LogWarning(
                "ApplyInspiration failed - player {PlayerId} not found",
                skaldId);
            return SpecializationAbilityEffect.None;
        }

        _logger.LogDebug(
            "[Inspiring Words] Skald {PlayerId} attempting to inspire allies",
            skaldId);

        // Skald makes DC 12 Rhetoric check
        var checkResult = _skillService.PerformSkillCheck(player, "rhetoric", InspiringWordsDc);

        if (!checkResult.Success)
        {
            _logger.LogDebug(
                "[Inspiring Words] failed - Skald did not succeed DC {Dc} check (rolled {Result})",
                InspiringWordsDc, checkResult.Total);
            return SpecializationAbilityEffect.None;
        }

        // Calculate net successes
        int netSuccesses = checkResult.Total - InspiringWordsDc;

        // Exceptional success (net ≥ 3) affects all party members
        bool isPartyWide = netSuccesses >= ExceptionalSuccessThreshold;

        var targets = targetIds.ToList();

        _logger.LogDebug(
            "[Inspiring Words] succeeded with {NetSuccesses} net successes (party-wide: {PartyWide})",
            netSuccesses, isPartyWide);

        _logger.LogInformation(
            "[Inspiring Words] Skald {PlayerId} inspired {TargetCount} allies with +{Bonus}d10 (party-wide: {PartyWide})",
            skaldId, isPartyWide ? "all" : targets.Count.ToString(), InspiringWordsAllyBonus, isPartyWide);

        // Return the effect - the calling code will apply the bonus to targets
        return await Task.FromResult(SpecializationAbilityEffect.InspirationEffect(
            InspiringWordsAllyBonus,
            isPartyWide,
            isPartyWide
                ? "Your rousing words fill all your allies with confidence."
                : "Your words of encouragement bolster your ally's confidence."));
    }

    /// <inheritdoc/>
    public async Task<SpecializationAbilityEffect> PerformStorytellingAsync(
        Guid skaldId,
        IEnumerable<Guid> listenerIds)
    {
        var archetypeId = GetPlayerArchetype(skaldId);
        if (!archetypeId?.Equals(SkaldArchetypeId, StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(
                "PerformStorytelling called for non-Skald player {PlayerId} (archetype: {Archetype})",
                skaldId, archetypeId ?? "none");
            return SpecializationAbilityEffect.None;
        }

        var player = _playerLookup(skaldId);
        if (player == null)
        {
            _logger.LogWarning(
                "PerformStorytelling failed - player {PlayerId} not found",
                skaldId);
            return SpecializationAbilityEffect.None;
        }

        _logger.LogDebug(
            "[Saga of Heroes] Skald {PlayerId} beginning storytelling session",
            skaldId);

        // Skald makes DC 10 Rhetoric check
        var checkResult = _skillService.PerformSkillCheck(player, "rhetoric", SagaOfHeroesDc);

        if (!checkResult.Success)
        {
            _logger.LogDebug(
                "[Saga of Heroes] failed - story didn't resonate (rolled {Result} vs DC {Dc})",
                checkResult.Total, SagaOfHeroesDc);
            return SpecializationAbilityEffect.None;
        }

        // Calculate net successes for stress reduction tier
        int netSuccesses = checkResult.Total - SagaOfHeroesDc;

        // Stress reduction based on success tier
        int stressReduction = netSuccesses switch
        {
            >= 5 => 4, // Critical
            >= 3 => 3, // Exceptional
            >= 1 => 2, // Full success
            _ => 1     // Marginal
        };

        var listeners = listenerIds.ToList();

        _logger.LogDebug(
            "[Saga of Heroes] succeeded with {NetSuccesses} net successes - {Reduction} stress reduction",
            netSuccesses, stressReduction);

        _logger.LogInformation(
            "[Saga of Heroes] reduced stress by {Amount} for {Count} listeners",
            stressReduction, listeners.Count);

        // Return the effect - the calling code will apply stress reduction to listeners
        return await Task.FromResult(SpecializationAbilityEffect.StressReliefEffect(
            RhetoricSpecializationAbility.SagaOfHeroes,
            stressReduction,
            $"Your tale of heroism soothes troubled minds, reducing stress by {stressReduction}."));
    }

    // -------------------------------------------------------------------------
    // Myrk-gengr Actions
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<SpecializationAbilityEffect> CreateForgedDocumentAsync(
        Guid forgerId,
        string documentType,
        bool hasReferenceSample)
    {
        var archetypeId = GetPlayerArchetype(forgerId);
        if (!archetypeId?.Equals(MyrkGengrArchetypeId, StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(
                "CreateForgedDocument called for non-Myrk-gengr player {PlayerId} (archetype: {Archetype})",
                forgerId, archetypeId ?? "none");
            return SpecializationAbilityEffect.None;
        }

        var player = _playerLookup(forgerId);
        if (player == null)
        {
            _logger.LogWarning(
                "CreateForgedDocument failed - player {PlayerId} not found",
                forgerId);
            return SpecializationAbilityEffect.None;
        }

        // Determine DC based on document type
        int baseDc = GetForgeryDc(documentType);

        _logger.LogDebug(
            "[Forge Documents] Myrk-gengr {PlayerId} attempting to forge {DocumentType} (DC {Dc})",
            forgerId, documentType, baseDc);

        // Reference sample grants +2d10 (reduced DC for simplicity)
        int effectiveDc = hasReferenceSample ? baseDc - 2 : baseDc;

        if (hasReferenceSample)
        {
            _logger.LogDebug(
                "[Forge Documents] Reference sample available - effective DC reduced to {EffectiveDc}",
                effectiveDc);
        }

        // Make the forgery check (using Rhetoric skill)
        var checkResult = _skillService.PerformSkillCheck(player, "rhetoric", effectiveDc);

        if (!checkResult.Success)
        {
            _logger.LogDebug(
                "[Forge Documents] failed - obvious fake produced (rolled {Result} vs DC {Dc})",
                checkResult.Total, effectiveDc);
            return SpecializationAbilityEffect.None;
        }

        // Calculate net successes for quality tier
        int netSuccesses = checkResult.Total - effectiveDc;

        // Quality based on success tier
        int quality = netSuccesses switch
        {
            >= 5 => 5, // Masterwork
            >= 3 => 4, // Excellent
            >= 1 => 3, // Good
            _ => 2     // Passable
        };

        _logger.LogDebug(
            "[Forge Documents] succeeded with {NetSuccesses} net successes - quality {Quality}/5",
            netSuccesses, quality);

        _logger.LogInformation(
            "[Forge Documents] player {PlayerId} created {DocumentType} with quality {Quality}/5",
            forgerId, documentType, quality);

        // Return the effect - the calling code will create the actual asset
        return await Task.FromResult(SpecializationAbilityEffect.ForgeryEffect(
            quality,
            $"You craft a convincing {documentType} (quality: {quality}/5)."));
    }

    // -------------------------------------------------------------------------
    // Query Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public IReadOnlyList<RhetoricSpecializationAbility> GetPlayerAbilities(Guid playerId)
    {
        var archetypeId = GetPlayerArchetype(playerId);

        if (string.IsNullOrEmpty(archetypeId))
        {
            return Array.Empty<RhetoricSpecializationAbility>();
        }

        return RhetoricSpecializationAbilityExtensions.GetAbilitiesForArchetype(archetypeId);
    }

    /// <inheritdoc/>
    public string? GetPlayerArchetype(Guid playerId)
    {
        var player = _playerLookup(playerId);
        return player?.ArchetypeId;
    }

    /// <inheritdoc/>
    public bool HasAbility(Guid playerId, RhetoricSpecializationAbility ability)
    {
        var abilities = GetPlayerAbilities(playerId);
        return abilities.Contains(ability);
    }

    /// <inheritdoc/>
    public (string Name, string Description, string Archetype) GetAbilityDetails(
        RhetoricSpecializationAbility ability)
    {
        return (
            ability.GetDisplayName(),
            ability.GetDescription(),
            ability.GetArchetypeName()
        );
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Checks if the social context involves lore or history topics.
    /// </summary>
    /// <param name="context">The social context to check.</param>
    /// <returns>True if any topic tag matches lore/history tags.</returns>
    private static bool IsLoreOrHistoryTopic(SocialContext context)
    {
        // Check social modifiers for topic-related sources
        return context.SocialModifiers.Any(m =>
            LoreTopicTags.Any(tag =>
                m.Source.Contains(tag, StringComparison.OrdinalIgnoreCase) ||
                m.Description.Contains(tag, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets the DC for forging a specific document type.
    /// </summary>
    /// <param name="documentType">The type of document to forge.</param>
    /// <returns>The DC for the forgery check.</returns>
    private static int GetForgeryDc(string documentType)
    {
        return documentType.ToLowerInvariant() switch
        {
            "note" or "simple" => 10,
            "travel" or "papers" => 12,
            "guild" or "credentials" => 14,
            "orders" or "official" => 16,
            "faction" or "authorization" => 18,
            _ => 20 // Complex/rare documents
        };
    }
}
