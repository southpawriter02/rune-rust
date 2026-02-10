// ═══════════════════════════════════════════════════════════════════════════════
// SkjaldmaerTier2AbilityService.cs
// Implements Tier 2 ability operations for the Skjaldmær specialization:
// Hold the Line, Counter-Shield, and Rally.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 2 Skjaldmær abilities: Hold the Line, Counter-Shield, and Rally.
/// </summary>
/// <remarks>
/// <para>
/// This service handles the Tier 2 ability operations as an extension of
/// <see cref="ISkjaldmaerAbilityService"/>. It operates on immutable value objects
/// and returns new instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 2 requires 8 PP invested in the Skjaldmær ability tree.</b>
/// Each Tier 2 ability costs 4 PP to unlock.
/// </para>
/// </remarks>
public class SkjaldmaerTier2AbilityService
{
    private readonly ILogger<SkjaldmaerTier2AbilityService> _logger;

    // Random instance for Counter-Shield damage rolls (1d6)
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of <see cref="SkjaldmaerTier2AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    public SkjaldmaerTier2AbilityService(
        ILogger<SkjaldmaerTier2AbilityService> logger,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;
    }

    // ═══════ Hold the Line ═══════

    /// <summary>
    /// Activates Hold the Line at the specified position.
    /// </summary>
    /// <param name="position">Grid position that enemies cannot move through.</param>
    /// <returns>An active HoldTheLineState with 2 turns remaining.</returns>
    public HoldTheLineState ActivateHoldTheLine((int X, int Y) position)
    {
        var state = HoldTheLineState.Activate(position);

        _logger.LogInformation(
            "Hold the Line activated at position ({X}, {Y}). " +
            "Duration: {Duration} turns. Enemies cannot pass through this space",
            position.X, position.Y, HoldTheLineState.DefaultDuration);

        return state;
    }

    /// <summary>
    /// Advances Hold the Line by one turn. Deactivates at 0 turns remaining.
    /// </summary>
    /// <param name="state">Current Hold the Line state.</param>
    /// <returns>Updated state with decremented turns.</returns>
    public HoldTheLineState TickHoldTheLine(HoldTheLineState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var updated = state.Tick();

        if (updated.IsExpired())
        {
            _logger.LogInformation(
                "Hold the Line expired at position ({X}, {Y}). " +
                "Movement blocking removed",
                state.BlockedPosition.X, state.BlockedPosition.Y);
        }
        else
        {
            _logger.LogDebug(
                "Hold the Line ticked at position ({X}, {Y}). " +
                "Turns remaining: {TurnsRemaining}",
                state.BlockedPosition.X, state.BlockedPosition.Y,
                updated.TurnsRemaining);
        }

        return updated;
    }

    /// <summary>
    /// Force-deactivates Hold the Line (e.g., when the Skjaldmær moves).
    /// </summary>
    /// <returns>An inactive HoldTheLineState.</returns>
    public HoldTheLineState DeactivateHoldTheLine()
    {
        _logger.LogInformation(
            "Hold the Line deactivated manually. Movement blocking removed");

        return HoldTheLineState.Deactivate();
    }

    // ═══════ Counter-Shield ═══════

    /// <summary>
    /// Executes a Counter-Shield reaction, rolling 1d6 damage against a melee attacker.
    /// </summary>
    /// <param name="skjaldmaerId">ID of the Skjaldmær triggering the reaction.</param>
    /// <param name="attackerId">ID of the melee attacker being countered.</param>
    /// <param name="attackerDefense">Attacker's defense value (logged for audit).</param>
    /// <returns>A CounterShieldResult containing the damage roll.</returns>
    public CounterShieldResult ExecuteCounterShield(
        Guid skjaldmaerId,
        Guid attackerId,
        int attackerDefense)
    {
        // Roll 1d6 for counter-shield damage
        var damageRoll = _random.Next(
            CounterShieldResult.MinDamageRoll,
            CounterShieldResult.MaxDamageRoll + 1);

        var result = CounterShieldResult.Create(skjaldmaerId, attackerId, damageRoll);

        _logger.LogInformation(
            "Counter-Shield triggered: Skjaldmær {SkjaldmaerId} dealt " +
            "{DamageRoll} damage to attacker {AttackerId} (defender defense: {Defense})",
            skjaldmaerId, damageRoll, attackerId, attackerDefense);

        return result;
    }

    // ═══════ Rally ═══════

    /// <summary>
    /// Creates Rally buffs for all specified allies.
    /// </summary>
    /// <param name="casterId">ID of the Skjaldmær casting Rally.</param>
    /// <param name="allyIds">IDs of allies within Rally's 6-space radius.</param>
    /// <returns>List of active RallyBuff instances.</returns>
    public IReadOnlyList<RallyBuff> ActivateRally(
        Guid casterId,
        IReadOnlyList<Guid> allyIds)
    {
        ArgumentNullException.ThrowIfNull(allyIds);

        var buffs = new List<RallyBuff>(allyIds.Count);

        foreach (var allyId in allyIds)
        {
            var buff = RallyBuff.Create(allyId, casterId);
            buffs.Add(buff);

            _logger.LogDebug(
                "Rally buff applied: Ally {AllyId} receives +{SaveBonus} " +
                "save bonus from Skjaldmær {CasterId}",
                allyId, RallyBuff.DefaultSaveBonus, casterId);
        }

        _logger.LogInformation(
            "Rally activated by Skjaldmær {CasterId}: {AllyCount} allies " +
            "received +{SaveBonus} to their next saving throw",
            casterId, buffs.Count, RallyBuff.DefaultSaveBonus);

        return buffs.AsReadOnly();
    }

    /// <summary>
    /// Consumes a Rally buff after a saving throw has been made.
    /// </summary>
    /// <param name="buff">The buff to consume.</param>
    /// <returns>A consumed (inactive) RallyBuff.</returns>
    public RallyBuff ConsumeRallyBuff(RallyBuff buff)
    {
        ArgumentNullException.ThrowIfNull(buff);

        var consumed = buff.Consume();

        _logger.LogInformation(
            "Rally buff consumed: Ally {AllyId} used +{SaveBonus} save bonus " +
            "(cast by {CasterId})",
            buff.AffectedCharacterId, buff.SaveBonus, buff.CasterCharacterId);

        return consumed;
    }

    /// <summary>
    /// Calculates total save bonus from active Rally buffs for a character.
    /// </summary>
    /// <param name="buffs">All Rally buffs currently in play.</param>
    /// <param name="allyId">Character to calculate bonus for.</param>
    /// <returns>Total save bonus from all applicable active buffs.</returns>
    public int GetAllySaveBonus(IReadOnlyList<RallyBuff> buffs, Guid allyId)
    {
        ArgumentNullException.ThrowIfNull(buffs);

        var totalBonus = 0;
        foreach (var buff in buffs)
        {
            totalBonus += buff.GetBonusForCharacter(allyId);
        }

        return totalBonus;
    }

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// Requires 8 PP invested in the Skjaldmær tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockTier2(int ppInvested)
    {
        var canUnlock = ppInvested >= PrerequisiteService.Tier2Threshold;

        _logger.LogDebug(
            "Tier 2 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, PrerequisiteService.Tier2Threshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Calculates total PP invested from a list of unlocked abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    public int CalculatePPInvested(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += PrerequisiteService.GetAbilityPPCost(ability);
        }

        _logger.LogDebug(
            "PP invested calculation: {AbilityCount} abilities unlocked, " +
            "total PP invested: {TotalPP}",
            unlockedAbilities.Count, total);

        return total;
    }
}
