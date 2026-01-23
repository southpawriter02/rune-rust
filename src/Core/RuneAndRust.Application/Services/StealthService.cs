namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implementation of <see cref="IStealthService"/> for managing stealth mechanics.
/// </summary>
/// <remarks>
/// <para>
/// The stealth service provides:
/// <list type="bullet">
///   <item><description>Surface-based DC calculation (Silent DC 2, Normal DC 3, Noisy DC 4, VeryNoisy DC 5)</description></item>
///   <item><description>Environmental modifiers (lighting, alert status, zone effects)</description></item>
///   <item><description>Party stealth using weakest-link rule</description></item>
///   <item><description>[Hidden] status tracking with break conditions</description></item>
///   <item><description>[System-Wide Alert] fumble consequences</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class StealthService : IStealthService
{
    private readonly IDiceService _diceService;
    private readonly ILogger<StealthService> _logger;

    // In-memory storage for [Hidden] statuses
    private readonly Dictionary<string, HiddenStatus> _hiddenStatuses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StealthService"/> class.
    /// </summary>
    /// <param name="diceService">The dice rolling service.</param>
    /// <param name="logger">The logger instance.</param>
    public StealthService(
        IDiceService diceService,
        ILogger<StealthService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("StealthService initialized");
    }

    /// <inheritdoc/>
    public StealthCheckResult AttemptStealth(
        string characterId,
        StealthContext context,
        int dicePool)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dicePool);

        _logger.LogInformation(
            "Character {CharacterId} attempting stealth on {Surface} surface (DC {Dc}) with {Pool}d10",
            characterId, context.SurfaceType, context.EffectiveDc, dicePool);

        // Perform the dice roll using d10 pool for Acrobatics
        var pool = DicePool.D10(dicePool);
        var diceResult = _diceService.Roll(pool, context: RollContexts.Skill("Stealth"));

        _logger.LogDebug(
            "Stealth roll: {Successes} successes, {Botches} botches, IsFumble={IsFumble}",
            diceResult.TotalSuccesses, diceResult.TotalBotches, diceResult.IsFumble);

        // Determine outcome
        StealthCheckResult result;

        if (diceResult.IsFumble)
        {
            _logger.LogWarning(
                "Character {CharacterId} fumbled stealth check - triggering [System-Wide Alert]",
                characterId);

            result = StealthCheckResult.Fumble(characterId, context);

            // Clear any existing hidden status
            _hiddenStatuses.Remove(characterId);
        }
        else if (diceResult.NetSuccesses >= context.EffectiveDc)
        {
            // Determine skill outcome based on margin
            var margin = diceResult.NetSuccesses - context.EffectiveDc;
            var outcome = ClassifyStealthOutcome(margin, diceResult.NetSuccesses);

            result = StealthCheckResult.Success(
                characterId,
                context,
                diceResult.NetSuccesses,
                outcome);

            // Store [Hidden] status
            if (result.HiddenStatus != null)
            {
                _hiddenStatuses[characterId] = result.HiddenStatus;

                _logger.LogInformation(
                    "Character {CharacterId} is now [Hidden] (margin {Margin}, detection modifier {DetMod})",
                    characterId, margin, result.DetectionModifier);
            }
        }
        else
        {
            result = StealthCheckResult.Failure(
                characterId,
                context,
                diceResult.NetSuccesses);

            // Clear any existing hidden status
            _hiddenStatuses.Remove(characterId);

            _logger.LogInformation(
                "Character {CharacterId} failed stealth check ({NetSuccesses} vs DC {Dc}) - detected",
                characterId, diceResult.NetSuccesses, context.EffectiveDc);
        }

        return result;
    }

    /// <inheritdoc/>
    public PartyStealthResult AttemptPartyStealth(
        IReadOnlyDictionary<string, int> partyMemberPools,
        StealthContext context)
    {
        if (partyMemberPools.Count == 0)
        {
            throw new ArgumentException("Party must have at least one member.", nameof(partyMemberPools));
        }

        var participantIds = partyMemberPools.Keys.ToList();

        _logger.LogInformation(
            "Party of {Count} attempting stealth on {Surface} surface (DC {Dc})",
            partyMemberPools.Count, context.SurfaceType, context.EffectiveDc);

        // Find weakest member
        var (weakestId, weakestPool) = FindWeakestMember(partyMemberPools);

        _logger.LogDebug(
            "Weakest link: {MemberId} with {Pool}d10 Acrobatics pool",
            weakestId, weakestPool);

        // Perform the dice roll for weakest member using d10 pool
        var pool = DicePool.D10(weakestPool);
        var diceResult = _diceService.Roll(pool, context: RollContexts.Skill("Stealth"));

        _logger.LogDebug(
            "Party stealth roll by {MemberId}: {Successes} successes, {Botches} botches",
            weakestId, diceResult.TotalSuccesses, diceResult.TotalBotches);

        PartyStealthResult result;

        if (diceResult.IsFumble)
        {
            _logger.LogWarning(
                "Party fumbled stealth via {MemberId} - triggering [System-Wide Alert]",
                weakestId);

            result = PartyStealthResult.Fumble(
                participantIds,
                weakestId,
                weakestPool,
                context);

            // Clear any existing hidden statuses for party
            foreach (var memberId in participantIds)
            {
                _hiddenStatuses.Remove(memberId);
            }
        }
        else if (diceResult.NetSuccesses >= context.EffectiveDc)
        {
            var margin = diceResult.NetSuccesses - context.EffectiveDc;
            var outcome = ClassifyStealthOutcome(margin, diceResult.NetSuccesses);

            result = PartyStealthResult.Success(
                participantIds,
                weakestId,
                weakestPool,
                context,
                diceResult.NetSuccesses,
                outcome);

            // Apply [Hidden] to all party members
            foreach (var memberId in participantIds)
            {
                var hidden = HiddenStatus.FromStealthCheck(memberId);
                _hiddenStatuses[memberId] = hidden;
            }

            _logger.LogInformation(
                "Party is now [Hidden] (margin {Margin})",
                margin);
        }
        else
        {
            result = PartyStealthResult.Failure(
                participantIds,
                weakestId,
                weakestPool,
                context,
                diceResult.NetSuccesses);

            // Clear any existing hidden statuses for party
            foreach (var memberId in participantIds)
            {
                _hiddenStatuses.Remove(memberId);
            }

            _logger.LogInformation(
                "Party failed stealth check ({NetSuccesses} vs DC {Dc}) via {MemberId} - detected",
                diceResult.NetSuccesses, context.EffectiveDc, weakestId);
        }

        return result;
    }

    /// <inheritdoc/>
    public HiddenStatus? GetHiddenStatus(string characterId)
    {
        return _hiddenStatuses.TryGetValue(characterId, out var status) && status.IsHidden
            ? status
            : null;
    }

    /// <inheritdoc/>
    public bool IsHidden(string characterId)
    {
        return _hiddenStatuses.TryGetValue(characterId, out var status) && status.IsHidden;
    }

    /// <inheritdoc/>
    public bool BreakHidden(string characterId, HiddenBreakCondition condition)
    {
        if (!_hiddenStatuses.TryGetValue(characterId, out var status) || !status.IsHidden)
        {
            _logger.LogDebug(
                "Cannot break [Hidden] for {CharacterId} - not hidden",
                characterId);
            return false;
        }

        if (!status.WouldBreak(condition))
        {
            _logger.LogDebug(
                "Condition {Condition} does not break [Hidden] for {CharacterId}",
                condition, characterId);
            return false;
        }

        status.Break(condition);

        _logger.LogInformation(
            "Character {CharacterId} [Hidden] broken by {Condition}",
            characterId, condition);

        return true;
    }

    /// <inheritdoc/>
    public HiddenStatus ApplyHiddenStatus(
        string characterId,
        string source,
        int detectionModifier = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var hidden = source switch
        {
            "SlipIntoShadow" => HiddenStatus.FromSlipIntoShadow(characterId),
            "OneWithTheStatic" => HiddenStatus.FromOneWithTheStatic(characterId),
            _ => HiddenStatus.FromStealthCheck(characterId, detectionModifier)
        };

        _hiddenStatuses[characterId] = hidden;

        _logger.LogInformation(
            "Applied [Hidden] to {CharacterId} via {Source} (detection modifier {DetMod})",
            characterId, source, hidden.DetectionModifier);

        return hidden;
    }

    /// <inheritdoc/>
    public int GetStealthDc(
        StealthSurface surface,
        bool isDimLight = false,
        bool isIlluminated = false,
        bool enemiesAlerted = false)
    {
        var context = StealthContext.CreateWithModifiers(
            surface,
            isDimLight,
            isIlluminated,
            enemiesAlerted);

        _logger.LogDebug(
            "Stealth DC for {Surface}: base {Base}, effective {Effective}",
            surface, context.BaseDc, context.EffectiveDc);

        return context.EffectiveDc;
    }

    /// <inheritdoc/>
    public (string MemberId, int DicePool) FindWeakestMember(IReadOnlyDictionary<string, int> partyMemberPools)
    {
        if (partyMemberPools.Count == 0)
        {
            throw new ArgumentException("Party must have at least one member.", nameof(partyMemberPools));
        }

        string? weakestId = null;
        var weakestPool = int.MaxValue;

        foreach (var (memberId, pool) in partyMemberPools)
        {
            if (pool < weakestPool)
            {
                weakestPool = pool;
                weakestId = memberId;
            }
        }

        return (weakestId ?? partyMemberPools.Keys.First(), weakestPool);
    }

    /// <inheritdoc/>
    public void ClearAllHiddenStatuses()
    {
        var count = _hiddenStatuses.Count;
        _hiddenStatuses.Clear();

        _logger.LogInformation("Cleared {Count} [Hidden] statuses (encounter end)", count);
    }

    /// <summary>
    /// Classifies the stealth outcome based on margin.
    /// </summary>
    /// <param name="margin">The margin of success.</param>
    /// <param name="netSuccesses">The net successes rolled.</param>
    /// <returns>The skill outcome classification.</returns>
    private static SkillOutcome ClassifyStealthOutcome(int margin, int netSuccesses)
    {
        // Fumble case handled before calling this method
        return margin switch
        {
            >= 5 => SkillOutcome.CriticalSuccess,
            >= 3 => SkillOutcome.ExceptionalSuccess,
            >= 1 => SkillOutcome.FullSuccess,
            0 => SkillOutcome.MarginalSuccess,
            _ => SkillOutcome.Failure
        };
    }
}
