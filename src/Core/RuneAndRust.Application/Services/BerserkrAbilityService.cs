using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Berserkr specialization ability execution.
/// Implements Tier 1 (Foundation) and Tier 2 ability logic.
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (v0.20.5a):</para>
/// <list type="bullet">
/// <item>Fury Strike (Active): weapon + 3d6 damage, costs 2 AP + 20 Rage, nat 20 = +1d6 critical</item>
/// <item>Blood Scent (Passive): +10 Rage on enemy bloodied, +1 Attack vs bloodied targets</item>
/// <item>Pain is Fuel (Passive): +5 Rage per damage instance received</item>
/// </list>
/// <para>Tier 2 abilities (v0.20.5b):</para>
/// <list type="bullet">
/// <item>Reckless Assault (Stance): +4 Attack (+1/20 Rage), -2 Defense, +1 Corruption/turn at 80+ Rage</item>
/// <item>Unstoppable (Active): Ignore movement penalties for 2 turns, 1 AP + 15 Rage</item>
/// <item>Intimidating Presence (Active): Fear-based AoE, save DC 12+Rage/20, 2 AP + 10 Rage</item>
/// </list>
/// <para>Critical design principle: Corruption risk is evaluated BEFORE resources are spent.
/// This ensures the player sees the risk assessment before committing to an action.</para>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.</para>
/// </remarks>
public class BerserkrAbilityService : IBerserkrAbilityService
{
    /// <summary>
    /// AP cost for the Fury Strike ability.
    /// </summary>
    private const int FuryStrikeApCost = 2;

    /// <summary>
    /// Rage cost for the Fury Strike ability.
    /// </summary>
    private const int FuryStrikeRageCost = 20;

    /// <summary>
    /// PP threshold for Tier 2 ability unlock.
    /// </summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>
    /// PP threshold for Tier 3 ability unlock.
    /// </summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>
    /// PP threshold for Capstone ability unlock.
    /// </summary>
    private const int CapstonePpRequirement = 24;

    // ===== Tier 2 Constants (v0.20.5b) =====

    /// <summary>
    /// AP cost to enter the Reckless Assault stance.
    /// Exiting the stance is a free action.
    /// </summary>
    private const int RecklessAssaultApCost = 1;

    /// <summary>
    /// AP cost for the Unstoppable ability.
    /// </summary>
    private const int UnstoppableApCost = 1;

    /// <summary>
    /// Rage cost for the Unstoppable ability.
    /// </summary>
    private const int UnstoppableRageCost = 15;

    /// <summary>
    /// AP cost for the Intimidating Presence ability.
    /// </summary>
    private const int IntimidatingPresenceApCost = 2;

    /// <summary>
    /// Rage cost for the Intimidating Presence ability.
    /// </summary>
    private const int IntimidatingPresenceRageCost = 10;

    /// <summary>
    /// The specialization ID string for Berserkr.
    /// </summary>
    private const string BerserkrSpecId = "berserkr";

    private readonly IBerserkrRageService _rageService;
    private readonly IBerserkrCorruptionService _corruptionService;
    private readonly ILogger<BerserkrAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BerserkrAbilityService"/> class.
    /// </summary>
    /// <param name="rageService">Service for Rage resource management.</param>
    /// <param name="corruptionService">Service for Corruption risk evaluation.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public BerserkrAbilityService(
        IBerserkrRageService rageService,
        IBerserkrCorruptionService corruptionService,
        ILogger<BerserkrAbilityService> logger)
    {
        _rageService = rageService ?? throw new ArgumentNullException(nameof(rageService));
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public FuryStrikeResult? UseFuryStrike(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBerserkr(player))
        {
            _logger.LogWarning(
                "Fury Strike failed: {Player} ({PlayerId}) is not a Berserkr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.FuryStrike))
        {
            _logger.LogWarning(
                "Fury Strike failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < FuryStrikeApCost)
        {
            _logger.LogWarning(
                "Fury Strike failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, FuryStrikeApCost, player.CurrentAP);
            return null;
        }

        // Validate Rage cost
        var rage = _rageService.GetRage(player);
        if (rage == null || rage.CurrentRage < FuryStrikeRageCost)
        {
            _logger.LogWarning(
                "Fury Strike failed: {Player} ({PlayerId}) has insufficient Rage " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, FuryStrikeRageCost, rage?.CurrentRage ?? 0);
            return null;
        }

        // === Evaluate Corruption risk BEFORE spending resources ===
        var corruptionResult = _corruptionService.EvaluateRisk(
            BerserkrAbilityId.FuryStrike,
            rage.CurrentRage);

        // Deduct AP
        player.CurrentAP -= FuryStrikeApCost;

        // Spend Rage (atomic)
        _rageService.SpendRage(player, FuryStrikeRageCost);

        // Roll dice
        var attackRoll = RollD20();
        var isCritical = attackRoll == 20;
        var baseDamage = RollWeaponDamage();
        var furyDamage = Roll3D6();
        var criticalDamage = isCritical ? RollD6() : 0;
        var totalDamage = baseDamage + furyDamage + criticalDamage;

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Build result
        var result = new FuryStrikeResult
        {
            AttackRoll = attackRoll,
            BaseDamage = baseDamage,
            FuryDamage = furyDamage,
            CriticalBonusDamage = criticalDamage,
            FinalDamage = totalDamage,
            RageSpent = FuryStrikeRageCost,
            WasCritical = isCritical,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason
        };

        _logger.LogInformation(
            "Fury Strike executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "Attack: {AttackRoll}{CritTag}. {DamageBreakdown}. " +
            "Rage spent: {RageSpent}. AP remaining: {RemainingAP}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            attackRoll, isCritical ? " [CRITICAL]" : "",
            result.GetDamageBreakdown(),
            FuryStrikeRageCost, player.CurrentAP,
            corruptionResult.IsTriggered ? $"+{corruptionResult.CorruptionAmount}" : "none");

        return result;
    }

    /// <inheritdoc />
    public BloodiedState? CheckBloodScent(
        Player player,
        Guid targetId,
        string targetName,
        int previousHp,
        int currentHp,
        int maxHp)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBerserkr(player))
            return null;

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.BloodScent))
            return null;

        // Create bloodied state snapshot
        var bloodiedState = BloodiedState.Create(targetId, targetName, currentHp, maxHp);

        // Check if the target just became bloodied (transition detection)
        if (!bloodiedState.WasJustBloodied(previousHp))
        {
            _logger.LogInformation(
                "Blood Scent check: {Target} ({TargetId}) is {State} " +
                "(HP {CurrentHp}/{MaxHp}, previous HP {PreviousHp})",
                targetName, targetId,
                bloodiedState.IsBloodied ? "already bloodied" : "not bloodied",
                currentHp, maxHp, previousHp);
            return null;
        }

        // Target just became bloodied — grant Rage
        var rageGained = _rageService.AddRage(player, RageResource.BloodScentGain, "Blood Scent");

        _logger.LogInformation(
            "Blood Scent triggered: {Player} ({PlayerId}) detected {Target} ({TargetId}) " +
            "becoming bloodied (HP {CurrentHp}/{MaxHp}). +{RageGained} Rage gained",
            player.Name, player.Id, targetName, targetId,
            currentHp, maxHp, rageGained);

        return bloodiedState;
    }

    /// <inheritdoc />
    public int CheckPainIsFuel(Player player, int damageTaken)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBerserkr(player))
            return 0;

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.PainIsFuel))
            return 0;

        // No Rage gain from zero or negative damage
        if (damageTaken <= 0)
            return 0;

        // Apply Rage gain
        var rageGained = _rageService.AddRage(player, RageResource.PainIsFuelGain, "Pain is Fuel");

        _logger.LogInformation(
            "Pain is Fuel triggered: {Player} ({PlayerId}) took {Damage} damage. " +
            "+{RageGained} Rage gained",
            player.Name, player.Id, damageTaken, rageGained);

        return rageGained;
    }

    /// <inheritdoc />
    public Dictionary<BerserkrAbilityId, bool> GetAbilityReadiness(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var readiness = new Dictionary<BerserkrAbilityId, bool>();

        if (!IsBerserkr(player))
            return readiness;

        var rage = _rageService.GetRage(player);
        var currentRage = rage?.CurrentRage ?? 0;

        // Check each unlocked ability's readiness
        foreach (var abilityId in player.UnlockedBerserkrAbilities)
        {
            var isReady = abilityId switch
            {
                BerserkrAbilityId.FuryStrike =>
                    player.CurrentAP >= FuryStrikeApCost && currentRage >= FuryStrikeRageCost,

                // Passive abilities are always "ready" (they trigger automatically)
                BerserkrAbilityId.BloodScent => true,
                BerserkrAbilityId.PainIsFuel => true,

                // Tier 2 abilities (v0.20.5b)
                BerserkrAbilityId.RecklessAssault =>
                    player.IsInRecklessAssault || player.CurrentAP >= RecklessAssaultApCost,

                BerserkrAbilityId.Unstoppable =>
                    !player.HasUnstoppableActive
                    && player.CurrentAP >= UnstoppableApCost
                    && currentRage >= UnstoppableRageCost,

                BerserkrAbilityId.IntimidatingPresence =>
                    player.CurrentAP >= IntimidatingPresenceApCost
                    && currentRage >= IntimidatingPresenceRageCost,

                _ => false
            };

            readiness[abilityId] = isReady;
        }

        return readiness;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        if (player == null)
            return false;

        if (!IsBerserkr(player))
            return false;

        return GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        if (player == null)
            return false;

        if (!IsBerserkr(player))
            return false;

        return GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsBerserkr(player))
            return false;

        return GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.GetBerserkrPPInvested();
    }

    // ===== Tier 2 Ability Methods (v0.20.5b) =====

    /// <inheritdoc />
    public RecklessAssaultState? ExecuteRecklessAssault(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBerserkr(player))
        {
            _logger.LogWarning(
                "Reckless Assault failed: {Player} ({PlayerId}) is not a Berserkr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.RecklessAssault))
        {
            _logger.LogWarning(
                "Reckless Assault failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate Tier 2 PP requirement
        if (!CanUnlockTier2(player))
        {
            _logger.LogWarning(
                "Reckless Assault failed: {Player} ({PlayerId}) has insufficient PP " +
                "for Tier 2 (need {Required}, have {Available})",
                player.Name, player.Id, Tier2PpRequirement, GetPPInvested(player));
            return null;
        }

        // Toggle: if already in stance, exit as free action
        if (player.IsInRecklessAssault)
        {
            player.RecklessAssault!.End();

            _logger.LogInformation(
                "Reckless Assault exited: {Player} ({PlayerId}) left the stance after {Turns} turns. " +
                "Total Corruption accrued: {Corruption}",
                player.Name, player.Id,
                player.RecklessAssault.TurnsActive,
                player.RecklessAssault.CorruptionAccrued);

            player.RecklessAssault = null;
            return null; // null indicates stance was exited
        }

        // Entering stance: validate AP
        if (player.CurrentAP < RecklessAssaultApCost)
        {
            _logger.LogWarning(
                "Reckless Assault failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, RecklessAssaultApCost, player.CurrentAP);
            return null;
        }

        // Evaluate Corruption risk BEFORE spending resources
        var rage = _rageService.GetRage(player);
        var currentRage = rage?.CurrentRage ?? 0;
        var corruptionResult = _corruptionService.EvaluateRisk(
            BerserkrAbilityId.RecklessAssault,
            currentRage);

        // Spend AP
        player.CurrentAP -= RecklessAssaultApCost;

        // Create and apply stance
        var state = RecklessAssaultState.Create(player.Id);
        player.RecklessAssault = state;

        // Apply initial Corruption if triggered (per-turn; first check on entry)
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "Reckless Assault entered: {Player} ({PlayerId}). " +
            "Attack bonus: +{AttackBonus} (base {Base} + rage scaling {RageBonus}). " +
            "Defense penalty: {DefensePenalty}. AP remaining: {RemainingAP}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id,
            state.GetCurrentAttackBonus(currentRage),
            state.BaseAttackBonus,
            currentRage / 20,
            state.DefensePenalty,
            player.CurrentAP,
            corruptionResult.IsTriggered ? $"+{corruptionResult.CorruptionAmount}" : "none");

        return state;
    }

    /// <inheritdoc />
    public UnstoppableEffect? ExecuteUnstoppable(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBerserkr(player))
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) is not a Berserkr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.Unstoppable))
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate Tier 2 PP requirement
        if (!CanUnlockTier2(player))
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) has insufficient PP " +
                "for Tier 2 (need {Required}, have {Available})",
                player.Name, player.Id, Tier2PpRequirement, GetPPInvested(player));
            return null;
        }

        // Check if already active
        if (player.HasUnstoppableActive)
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) already has Unstoppable active " +
                "({TurnsRemaining} turns remaining)",
                player.Name, player.Id, player.UnstoppableEffect!.TurnsRemaining);
            return null;
        }

        // Validate AP
        if (player.CurrentAP < UnstoppableApCost)
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, UnstoppableApCost, player.CurrentAP);
            return null;
        }

        // Validate Rage
        var rage = _rageService.GetRage(player);
        if (rage == null || rage.CurrentRage < UnstoppableRageCost)
        {
            _logger.LogWarning(
                "Unstoppable failed: {Player} ({PlayerId}) has insufficient Rage " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, UnstoppableRageCost, rage?.CurrentRage ?? 0);
            return null;
        }

        // Evaluate Corruption risk BEFORE spending resources
        var corruptionResult = _corruptionService.EvaluateRisk(
            BerserkrAbilityId.Unstoppable,
            rage.CurrentRage);

        // Spend AP
        player.CurrentAP -= UnstoppableApCost;

        // Spend Rage
        _rageService.SpendRage(player, UnstoppableRageCost);

        // Create and apply effect
        var effect = UnstoppableEffect.Create(player.Id);
        player.UnstoppableEffect = effect;

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "Unstoppable activated: {Player} ({PlayerId}). " +
            "Duration: {Duration} turns. Rage spent: {RageSpent}. AP remaining: {RemainingAP}. " +
            "Movement penalties ignored: {Penalties}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id,
            effect.TurnsRemaining,
            UnstoppableRageCost, player.CurrentAP,
            string.Join(", ", effect.MovementPenaltiesIgnored),
            corruptionResult.IsTriggered ? $"+{corruptionResult.CorruptionAmount}" : "none");

        return effect;
    }

    /// <inheritdoc />
    public List<IntimidationEffect> ExecuteIntimidatingPresence(
        Player player,
        List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)> targets)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(targets);

        var results = new List<IntimidationEffect>();

        // Validate specialization
        if (!IsBerserkr(player))
        {
            _logger.LogWarning(
                "Intimidating Presence failed: {Player} ({PlayerId}) is not a Berserkr",
                player.Name, player.Id);
            return results;
        }

        // Validate ability unlock
        if (!player.HasBerserkrAbilityUnlocked(BerserkrAbilityId.IntimidatingPresence))
        {
            _logger.LogWarning(
                "Intimidating Presence failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return results;
        }

        // Validate Tier 2 PP requirement
        if (!CanUnlockTier2(player))
        {
            _logger.LogWarning(
                "Intimidating Presence failed: {Player} ({PlayerId}) has insufficient PP " +
                "for Tier 2 (need {Required}, have {Available})",
                player.Name, player.Id, Tier2PpRequirement, GetPPInvested(player));
            return results;
        }

        // Validate AP
        if (player.CurrentAP < IntimidatingPresenceApCost)
        {
            _logger.LogWarning(
                "Intimidating Presence failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, IntimidatingPresenceApCost, player.CurrentAP);
            return results;
        }

        // Validate Rage
        var rage = _rageService.GetRage(player);
        if (rage == null || rage.CurrentRage < IntimidatingPresenceRageCost)
        {
            _logger.LogWarning(
                "Intimidating Presence failed: {Player} ({PlayerId}) has insufficient Rage " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, IntimidatingPresenceRageCost, rage?.CurrentRage ?? 0);
            return results;
        }

        var currentRage = rage.CurrentRage;

        // Spend AP
        player.CurrentAP -= IntimidatingPresenceApCost;

        // Spend Rage
        _rageService.SpendRage(player, IntimidatingPresenceRageCost);

        _logger.LogInformation(
            "Intimidating Presence cast: {Player} ({PlayerId}) against {TargetCount} targets. " +
            "Save DC: {SaveDc}. Rage spent: {RageSpent}. AP remaining: {RemainingAP}",
            player.Name, player.Id, targets.Count,
            IntimidationEffect.CalculateSaveDc(currentRage),
            IntimidatingPresenceRageCost, player.CurrentAP);

        // Process each target
        foreach (var (targetId, targetName, willSaveRoll, isCoherent, isMindless, hasFearImmunity) in targets)
        {
            // Skip mindless creatures
            if (isMindless)
            {
                _logger.LogInformation(
                    "Intimidating Presence: {Target} ({TargetId}) is mindless — immune",
                    targetName, targetId);
                continue;
            }

            // Skip fear-immune targets
            if (hasFearImmunity)
            {
                _logger.LogInformation(
                    "Intimidating Presence: {Target} ({TargetId}) has fear immunity — immune",
                    targetName, targetId);
                continue;
            }

            // Create effect with DC scaled by Rage
            var effect = IntimidationEffect.Create(player.Id, targetId, targetName, currentRage);

            // Apply the save result
            effect.ApplySaveResult(willSaveRoll);

            if (effect.DidSave)
            {
                _logger.LogInformation(
                    "Intimidating Presence: {Target} ({TargetId}) SAVED ({SaveRoll} vs DC {SaveDc}). " +
                    "Granted 24h immunity",
                    targetName, targetId, willSaveRoll, effect.SaveDc);
            }
            else
            {
                _logger.LogWarning(
                    "Intimidating Presence: {Target} ({TargetId}) FAILED ({SaveRoll} vs DC {SaveDc}). " +
                    "-2 Attack for {Duration} turns",
                    targetName, targetId, willSaveRoll, effect.SaveDc, effect.TurnsRemaining);
            }

            // Evaluate Corruption for Coherent targets
            if (isCoherent)
            {
                var corruptionResult = _corruptionService.EvaluateRisk(
                    BerserkrAbilityId.IntimidatingPresence,
                    currentRage,
                    targetIsCoherent: true);

                if (corruptionResult.IsTriggered)
                {
                    _corruptionService.ApplyCorruption(player.Id, corruptionResult);
                }
            }

            results.Add(effect);
        }

        _logger.LogInformation(
            "Intimidating Presence complete: {Affected} targets affected, " +
            "{Failed} failed saves, {Saved} saved",
            results.Count,
            results.Count(e => !e.DidSave),
            results.Count(e => e.DidSave));

        return results;
    }

    /// <inheritdoc />
    public RecklessAssaultState? GetRecklessAssaultState(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.RecklessAssault;
    }

    /// <inheritdoc />
    public UnstoppableEffect? GetUnstoppableEffect(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.UnstoppableEffect;
    }

    // ===== Dice Roll Methods (internal virtual for testing) =====

    /// <summary>
    /// Rolls a d20 for attack rolls.
    /// </summary>
    /// <returns>A random value between 1 and 20.</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// </remarks>
    internal virtual int RollD20() => Random.Shared.Next(1, 21);

    /// <summary>
    /// Rolls a d6 for critical bonus damage.
    /// </summary>
    /// <returns>A random value between 1 and 6.</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// </remarks>
    internal virtual int RollD6() => Random.Shared.Next(1, 7);

    /// <summary>
    /// Rolls 3d6 for Fury Strike bonus damage.
    /// </summary>
    /// <returns>Sum of three d6 rolls (range: 3–18).</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// </remarks>
    internal virtual int Roll3D6()
    {
        return Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Rolls weapon damage (simulated as 1d8 for standard weapon).
    /// </summary>
    /// <returns>A random value between 1 and 8.</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// In a full implementation, this would reference the equipped weapon's damage dice.
    /// </remarks>
    internal virtual int RollWeaponDamage() => Random.Shared.Next(1, 9);

    /// <summary>
    /// Checks if a player is a Berserkr.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "berserkr".</returns>
    private static bool IsBerserkr(Player player)
    {
        return string.Equals(player.SpecializationId, BerserkrSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
