using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Seiðkona specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic.
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (v0.20.8a):</para>
/// <list type="bullet">
/// <item>Seiðr Bolt (Active): 2d6 Aetheric damage, +1 Resonance, +1 Accumulated Damage, costs 1 AP.
///   Subject to probability-based Corruption check at Resonance 5+ (Heretical path).</item>
/// <item>Wyrd Sight (Active): detect invisible/magic/Corruption within 10 spaces for 3 turns, costs 2 AP.
///   Does NOT build Resonance, does NOT trigger Corruption checks.</item>
/// <item>Aether Attunement (Passive): +10% AP regeneration rate. Always active, no Resonance or Corruption.</item>
/// </list>
/// <para>Tier 2 abilities (v0.20.8b):</para>
/// <list type="bullet">
/// <item>Fate's Thread (Active): Divination/precognition, +2 Resonance, costs 2 AP (1 with Cascade).
///   Subject to Corruption check at Resonance 5+. No damage, no Accumulated Damage.</item>
/// <item>Weave Disruption (Active): Dispel/counterspell, d20 + Resonance bonus, +1 Resonance,
///   costs 3 AP (2 with Cascade). Subject to Corruption check. No direct damage.</item>
/// <item>Resonance Cascade (Passive): Reduces all Seiðkona ability AP costs by 1 (min 1) at
///   Resonance 5+. No Resonance gain, no Corruption risk. Does NOT affect Unraveling capstone.</item>
/// </list>
/// <para>Tier 3 abilities (v0.20.8c):</para>
/// <list type="bullet">
/// <item>Völva's Vision (Active): Mass detection revealing all hidden/invisible/concealed enemies
///   within 15 spaces, +2 Resonance, costs 3 AP (2 with Cascade). No damage, no Accumulated Damage.</item>
/// <item>Aether Storm (Active): 4d6 Aetheric damage (cone AoE), +2 Resonance, costs 5 AP
///   (4 with Cascade). Adds damage to Accumulated Aetheric Damage tracker.</item>
/// </list>
/// <para>Capstone ability (v0.20.8c):</para>
/// <list type="bullet">
/// <item>The Unraveling (Active): Releases 100% of Accumulated Aetheric Damage, costs 5 AP
///   (immune to Cascade), requires Resonance 10 and Accumulated Damage &gt; 0. Guaranteed 20%
///   Corruption check (+2). Once per combat.</item>
/// </list>
/// <para>Critical design principle: Corruption risk is evaluated BEFORE resources are spent.
/// For all active abilities, the evaluation uses the current Resonance (before any gain from the cast).
/// This ensures the player sees the risk assessment before committing to an action.</para>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.</para>
/// </remarks>
public class SeidkonaAbilityService : ISeidkonaAbilityService
{
    // ===== Tier 1 Constants (v0.20.8a) =====

    /// <summary>
    /// AP cost for the Seiðr Bolt ability.
    /// </summary>
    private const int SeidrBoltApCost = 1;

    /// <summary>
    /// AP cost for the Wyrd Sight ability.
    /// </summary>
    private const int WyrdSightApCost = 2;

    /// <summary>
    /// Passive AP regeneration bonus from Aether Attunement (represents +10%).
    /// </summary>
    private const int AetherAttunementBonus = 10;

    /// <summary>
    /// Amount of Resonance gained per Seiðr Bolt cast.
    /// </summary>
    private const int SeidrBoltResonanceGain = 1;

    // ===== Tier 2 Constants (v0.20.8b) =====

    /// <summary>
    /// Base AP cost for the Fate's Thread ability (before Cascade reduction).
    /// </summary>
    private const int FatesThreadApCost = 2;

    /// <summary>
    /// Amount of Resonance gained per Fate's Thread cast.
    /// Higher than Tier 1's +1, representing the escalation risk of Tier 2 abilities.
    /// </summary>
    private const int FatesThreadResonanceGain = 2;

    /// <summary>
    /// Base AP cost for the Weave Disruption ability (before Cascade reduction).
    /// </summary>
    private const int WeaveDisruptionApCost = 3;

    /// <summary>
    /// Amount of Resonance gained per Weave Disruption cast.
    /// Lower than Fate's Thread (+1 vs +2), balancing risk with higher AP cost.
    /// </summary>
    private const int WeaveDisruptionResonanceGain = 1;

    /// <summary>
    /// Resonance level at which Resonance Cascade becomes active.
    /// </summary>
    private const int CascadeActivationThreshold = 5;

    /// <summary>
    /// AP cost reduction applied by Resonance Cascade when active.
    /// </summary>
    private const int CascadeReduction = 1;

    /// <summary>
    /// Minimum AP cost enforced after Cascade reduction.
    /// </summary>
    private const int MinimumApCost = 1;

    // ===== Tier 3 Constants (v0.20.8c) =====

    /// <summary>
    /// Base AP cost for the Völva's Vision ability (before Cascade reduction).
    /// </summary>
    private const int VolvasVisionApCost = 3;

    /// <summary>
    /// Amount of Resonance gained per Völva's Vision cast.
    /// Same as Aether Storm (+2), representing the high Aetheric channeling of Tier 3 abilities.
    /// </summary>
    private const int VolvasVisionResonanceGain = 2;

    /// <summary>
    /// Detection radius for the Völva's Vision reveal effect (15 spaces).
    /// The largest detection radius of any Seiðkona ability.
    /// </summary>
    private const int VolvasVisionRevealRadius = 15;

    /// <summary>
    /// Base AP cost for the Aether Storm ability (before Cascade reduction).
    /// Highest cost in the Seiðkona ability tree before the capstone.
    /// </summary>
    private const int AetherStormApCost = 5;

    /// <summary>
    /// Amount of Resonance gained per Aether Storm cast.
    /// Same as Völva's Vision (+2), representing the high Aetheric channeling of Tier 3 abilities.
    /// </summary>
    private const int AetherStormResonanceGain = 2;

    // ===== Capstone Constants (v0.20.8c) =====

    /// <summary>
    /// Fixed AP cost for the Unraveling capstone ability.
    /// Immune to Resonance Cascade reduction — always costs exactly 5 AP.
    /// </summary>
    private const int UnravelingApCost = 5;

    /// <summary>
    /// Required Resonance level for the Unraveling capstone.
    /// Must be exactly 10 (max) — the Unraveling can only be unleashed at full Resonance.
    /// </summary>
    private const int UnravelingRequiredResonance = 10;

    // ===== PP Requirements =====

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

    /// <summary>
    /// The specialization ID string for Seiðkona.
    /// </summary>
    private const string SeidkonaSpecId = "seidkona";

    private readonly ISeidkonaResonanceService _resonanceService;
    private readonly ISeidkonaCorruptionService _corruptionService;
    private readonly ILogger<SeidkonaAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeidkonaAbilityService"/> class.
    /// </summary>
    /// <param name="resonanceService">Service for Aether Resonance resource management.</param>
    /// <param name="corruptionService">Service for Corruption risk evaluation.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SeidkonaAbilityService(
        ISeidkonaResonanceService resonanceService,
        ISeidkonaCorruptionService corruptionService,
        ILogger<SeidkonaAbilityService> logger)
    {
        _resonanceService = resonanceService ?? throw new ArgumentNullException(nameof(resonanceService));
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1 Abilities (v0.20.8a) =====

    /// <inheritdoc />
    public SeidrBoltResult? ExecuteSeidrBolt(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.SeidrBolt))
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade may apply to SeidrBolt, though 1→1 min)
        var effectiveCost = GetEffectiveApCostInternal(player, SeidrBoltApCost);

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, effectiveCost, player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +1 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.SeidrBolt,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Roll damage (2d6 Aetheric)
        var damageRoll = Roll2D6();

        // Build Resonance (+1 from Seiðr Bolt)
        _resonanceService.BuildResonance(player, SeidrBoltResonanceGain, "Seiðr Bolt cast");

        // Add to Accumulated Aetheric Damage tracker
        _resonanceService.AddAccumulatedDamage(player, damageRoll);

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new SeidrBoltResult
        {
            DamageRoll = damageRoll,
            TotalDamage = damageRoll,
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent
        };

        _logger.LogInformation(
            "Seiðr Bolt executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "{DamageBreakdown}. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            result.GetDamageBreakdown(),
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    /// <inheritdoc />
    public WyrdSightResult? ExecuteWyrdSight(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WyrdSight))
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade may reduce 2→1)
        var effectiveCost = GetEffectiveApCostInternal(player, WyrdSightApCost);

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, effectiveCost, player.CurrentAP);
            return null;
        }

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Create Wyrd Sight effect — NO Resonance gain, NO Corruption check
        var wyrdSight = WyrdSightResult.Create(player.Id);
        player.SetWyrdSight(wyrdSight);

        _logger.LogInformation(
            "Wyrd Sight activated: {Player} ({PlayerId}). {Description}",
            player.Name, player.Id, wyrdSight.GetDescription());

        return wyrdSight;
    }

    /// <inheritdoc />
    public int GetAetherAttunementBonus(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSeidkona(player))
            return 0;

        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherAttunement))
            return 0;

        return AetherAttunementBonus;
    }

    // ===== Tier 2 Abilities (v0.20.8b) =====

    /// <inheritdoc />
    public FatesThreadResult? ExecuteFatesThread(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Fate's Thread failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.FatesThread))
        {
            _logger.LogWarning(
                "Fate's Thread failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade reduces 2→1 at Resonance 5+)
        var effectiveCost = GetEffectiveApCostInternal(player, FatesThreadApCost);
        var cascadeApplied = effectiveCost < FatesThreadApCost;

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Fate's Thread failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}{CascadeNote}, have {Available})",
                player.Name, player.Id, effectiveCost,
                cascadeApplied ? " [Cascade active]" : "",
                player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +2 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.FatesThread,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Build Resonance (+2 from Fate's Thread — higher than Tier 1)
        var actualGain = _resonanceService.BuildResonance(player, FatesThreadResonanceGain, "Fate's Thread cast");

        // NO accumulated damage — Fate's Thread is divination, not damage

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new FatesThreadResult
        {
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            ResonanceGained = actualGain,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent,
            ApCostPaid = effectiveCost,
            CascadeApplied = cascadeApplied
        };

        _logger.LogInformation(
            "Fate's Thread executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "AP: {ApCost}{CascadeNote}. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            effectiveCost, cascadeApplied ? " [Cascade]" : "",
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    /// <inheritdoc />
    public WeaveDisruptionResult? ExecuteWeaveDisruption(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Weave Disruption failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WeaveDisruption))
        {
            _logger.LogWarning(
                "Weave Disruption failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade reduces 3→2 at Resonance 5+)
        var effectiveCost = GetEffectiveApCostInternal(player, WeaveDisruptionApCost);
        var cascadeApplied = effectiveCost < WeaveDisruptionApCost;

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Weave Disruption failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}{CascadeNote}, have {Available})",
                player.Name, player.Id, effectiveCost,
                cascadeApplied ? " [Cascade active]" : "",
                player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +1 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.WeaveDisruption,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Roll d20 for dispel attempt (result stored for combat system resolution)
        var dispelRoll = RollD20();
        var totalRoll = dispelRoll + resonanceBefore;

        // Build Resonance (+1 from Weave Disruption)
        var actualGain = _resonanceService.BuildResonance(player, WeaveDisruptionResonanceGain, "Weave Disruption cast");

        // NO accumulated damage — Weave Disruption is dispel, not damage

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new WeaveDisruptionResult
        {
            DispelRoll = dispelRoll,
            ResonanceBonus = resonanceBefore,
            TotalRoll = totalRoll,
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            ResonanceGained = actualGain,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent,
            ApCostPaid = effectiveCost,
            CascadeApplied = cascadeApplied
        };

        _logger.LogInformation(
            "Weave Disruption executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "AP: {ApCost}{CascadeNote}. {DispelBreakdown}. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            effectiveCost, cascadeApplied ? " [Cascade]" : "",
            result.GetDispelBreakdown(),
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    // ===== Tier 3 Abilities (v0.20.8c) =====

    /// <inheritdoc />
    public VolvasVisionResult? ExecuteVolvasVision(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Völva's Vision failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.VolvasVision))
        {
            _logger.LogWarning(
                "Völva's Vision failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade reduces 3→2 at Resonance 5+)
        var effectiveCost = GetEffectiveApCostInternal(player, VolvasVisionApCost);
        var cascadeApplied = effectiveCost < VolvasVisionApCost;

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Völva's Vision failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}{CascadeNote}, have {Available})",
                player.Name, player.Id, effectiveCost,
                cascadeApplied ? " [Cascade active]" : "",
                player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +2 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.VolvasVision,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Build Resonance (+2 from Völva's Vision — high buildup, shared with Aether Storm)
        var actualGain = _resonanceService.BuildResonance(player, VolvasVisionResonanceGain, "Völva's Vision cast");

        // NO accumulated damage — Völva's Vision is detection, not damage

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new VolvasVisionResult
        {
            RevealRadius = VolvasVisionRevealRadius,
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            ResonanceGained = actualGain,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent,
            ApCostPaid = effectiveCost,
            CascadeApplied = cascadeApplied
        };

        _logger.LogInformation(
            "Völva's Vision executed: {Player} ({PlayerId}). " +
            "AP: {ApCost}{CascadeNote}. Reveal radius: {Radius} spaces. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id,
            effectiveCost, cascadeApplied ? " [Cascade]" : "",
            VolvasVisionRevealRadius,
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    /// <inheritdoc />
    public AetherStormResult? ExecuteAetherStorm(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Aether Storm failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherStorm))
        {
            _logger.LogWarning(
                "Aether Storm failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Calculate effective AP cost (Cascade reduces 5→4 at Resonance 5+)
        var effectiveCost = GetEffectiveApCostInternal(player, AetherStormApCost);
        var cascadeApplied = effectiveCost < AetherStormApCost;

        // Validate AP cost
        if (player.CurrentAP < effectiveCost)
        {
            _logger.LogWarning(
                "Aether Storm failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}{CascadeNote}, have {Available})",
                player.Name, player.Id, effectiveCost,
                cascadeApplied ? " [Cascade active]" : "",
                player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +2 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.AetherStorm,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= effectiveCost;

        // Roll damage (4d6 Aetheric)
        var damageRoll = Roll4D6();

        // Build Resonance (+2 from Aether Storm — high buildup, shared with Völva's Vision)
        var actualGain = _resonanceService.BuildResonance(player, AetherStormResonanceGain, "Aether Storm cast");

        // Add to Accumulated Aetheric Damage tracker — Aether Storm DOES accumulate damage
        _resonanceService.AddAccumulatedDamage(player, damageRoll);

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new AetherStormResult
        {
            DamageRoll = damageRoll,
            TotalDamage = damageRoll,
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            ResonanceGained = actualGain,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent,
            ApCostPaid = effectiveCost,
            CascadeApplied = cascadeApplied
        };

        _logger.LogInformation(
            "Aether Storm executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "AP: {ApCost}{CascadeNote}. {DamageBreakdown}. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            effectiveCost, cascadeApplied ? " [Cascade]" : "",
            result.GetDamageBreakdown(),
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    // ===== Capstone Ability (v0.20.8c) =====

    /// <inheritdoc />
    public UnravelingResult? ExecuteUnraveling(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.Unraveling))
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate per-combat cooldown
        if (player.HasUsedUnravelingThisCombat)
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) has already used the Unraveling this combat",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost (NO Cascade — Unraveling is immune to AP reduction)
        if (player.CurrentAP < UnravelingApCost)
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required} [Cascade immune], have {Available})",
                player.Name, player.Id, UnravelingApCost, player.CurrentAP);
            return null;
        }

        // Validate Resonance requirement (must be exactly 10)
        var resonance = _resonanceService.GetResonance(player);
        var currentResonance = resonance?.CurrentResonance ?? 0;

        if (currentResonance != UnravelingRequiredResonance)
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) does not have max Resonance " +
                "(need {Required}, have {Current})",
                player.Name, player.Id, UnravelingRequiredResonance, currentResonance);
            return null;
        }

        // Validate Accumulated Aetheric Damage (must be > 0)
        var accumulatedDamage = _resonanceService.GetAccumulatedDamage(player);
        var totalAccumulated = accumulatedDamage?.TotalDamage ?? 0;

        if (totalAccumulated <= 0)
        {
            _logger.LogWarning(
                "The Unraveling failed: {Player} ({PlayerId}) has no Accumulated Aetheric Damage to release",
                player.Name, player.Id);
            return null;
        }

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Guaranteed 20% capstone check — always performed regardless of Resonance
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.Unraveling,
            currentResonance);

        // Deduct AP (fixed 5 AP — Cascade immune)
        player.CurrentAP -= UnravelingApCost;

        // Capture accumulated damage for release before resetting
        var damageToRelease = totalAccumulated;

        // Reset Resonance from 10 to 0
        _resonanceService.ResetResonance(player, "Unraveling capstone");

        // Reset Accumulated Aetheric Damage to 0
        _resonanceService.ResetAccumulatedDamage(player);

        // Set per-combat cooldown flag
        player.HasUsedUnravelingThisCombat = true;

        // Apply Corruption if triggered (+2 at CapstoneActivation tier)
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Build result
        var result = new UnravelingResult
        {
            AccumulatedDamageConsumed = damageToRelease,
            TotalDamage = damageToRelease,
            ResonanceBefore = UnravelingRequiredResonance,
            ResonanceAfter = 0,
            CorruptionCheckPerformed = true, // Always true for Unraveling
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent,
            ApCostPaid = UnravelingApCost,
            CooldownActivated = true
        };

        _logger.LogInformation(
            "THE UNRAVELING EXECUTED: {Player} ({PlayerId}). " +
            "{DamageBreakdown}. AP: {ApCost} [Cascade immune]. " +
            "{ResonanceChange}. Cooldown: activated (once per combat). " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id,
            result.GetDamageBreakdown(),
            UnravelingApCost,
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)");

        return result;
    }

    /// <inheritdoc />
    public ResonanceCascadeState GetResonanceCascadeState(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSeidkona(player))
            return ResonanceCascadeState.Evaluate(0, false);

        var resonance = _resonanceService.GetResonance(player);
        var currentResonance = resonance?.CurrentResonance ?? 0;
        var cascadeUnlocked = player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.ResonanceCascade);

        return ResonanceCascadeState.Evaluate(currentResonance, cascadeUnlocked);
    }

    /// <inheritdoc />
    public int GetEffectiveApCost(Player player, SeidkonaAbilityId abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Unraveling is immune to Cascade reduction — always costs exactly 5 AP
        if (abilityId == SeidkonaAbilityId.Unraveling)
            return UnravelingApCost;

        var baseCost = GetBaseApCost(abilityId);
        if (baseCost == 0)
            return 0; // Passive abilities have no AP cost

        return GetEffectiveApCostInternal(player, baseCost);
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public Dictionary<SeidkonaAbilityId, bool> GetAbilityReadiness(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var readiness = new Dictionary<SeidkonaAbilityId, bool>();

        if (!IsSeidkona(player))
            return readiness;

        // Tier 1 active abilities
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.SeidrBolt))
        {
            var cost = GetEffectiveApCostInternal(player, SeidrBoltApCost);
            readiness[SeidkonaAbilityId.SeidrBolt] = player.CurrentAP >= cost;
        }

        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WyrdSight))
        {
            var cost = GetEffectiveApCostInternal(player, WyrdSightApCost);
            readiness[SeidkonaAbilityId.WyrdSight] = player.CurrentAP >= cost;
        }

        // Tier 1 passive — always "ready" if unlocked
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherAttunement))
        {
            readiness[SeidkonaAbilityId.AetherAttunement] = true;
        }

        // Tier 2 active abilities (v0.20.8b)
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.FatesThread))
        {
            var cost = GetEffectiveApCostInternal(player, FatesThreadApCost);
            readiness[SeidkonaAbilityId.FatesThread] = player.CurrentAP >= cost;
        }

        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WeaveDisruption))
        {
            var cost = GetEffectiveApCostInternal(player, WeaveDisruptionApCost);
            readiness[SeidkonaAbilityId.WeaveDisruption] = player.CurrentAP >= cost;
        }

        // Tier 2 passive — always "ready" if unlocked
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.ResonanceCascade))
        {
            readiness[SeidkonaAbilityId.ResonanceCascade] = true;
        }

        // Tier 3 active abilities (v0.20.8c)
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.VolvasVision))
        {
            var cost = GetEffectiveApCostInternal(player, VolvasVisionApCost);
            readiness[SeidkonaAbilityId.VolvasVision] = player.CurrentAP >= cost;
        }

        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherStorm))
        {
            var cost = GetEffectiveApCostInternal(player, AetherStormApCost);
            readiness[SeidkonaAbilityId.AetherStorm] = player.CurrentAP >= cost;
        }

        // Capstone ability (v0.20.8c) — multiple preconditions
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.Unraveling))
        {
            var resonance = _resonanceService.GetResonance(player);
            var currentResonance = resonance?.CurrentResonance ?? 0;
            var accumulatedDamage = _resonanceService.GetAccumulatedDamage(player);
            var totalAccumulated = accumulatedDamage?.TotalDamage ?? 0;

            readiness[SeidkonaAbilityId.Unraveling] =
                player.CurrentAP >= UnravelingApCost &&
                currentResonance == UnravelingRequiredResonance &&
                totalAccumulated > 0 &&
                !player.HasUsedUnravelingThisCombat;
        }

        return readiness;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSeidkona(player))
            return 0;

        return player.GetSeidkonaPPInvested();
    }

    // ===== Dice Roll Methods (internal virtual for test overriding) =====

    /// <summary>
    /// Rolls 2d6 for Seiðr Bolt damage.
    /// Marked <c>internal virtual</c> for unit test overriding.
    /// </summary>
    /// <returns>The sum of two d6 rolls (2–12).</returns>
    internal virtual int Roll2D6()
    {
        return Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Rolls 1d20 for Weave Disruption dispel checks.
    /// Marked <c>internal virtual</c> for unit test overriding.
    /// </summary>
    /// <returns>A random integer between 1 and 20 inclusive.</returns>
    internal virtual int RollD20()
    {
        return Random.Shared.Next(1, 21);
    }

    /// <summary>
    /// Rolls 4d6 for Aether Storm damage.
    /// Marked <c>internal virtual</c> for unit test overriding.
    /// </summary>
    /// <returns>The sum of four d6 rolls (4–24).</returns>
    internal virtual int Roll4D6()
    {
        return Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7) +
               Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);
    }

    // ===== Private Helpers =====

    /// <summary>
    /// Checks if a player is a Seiðkona.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "seidkona".</returns>
    private static bool IsSeidkona(Player player)
    {
        return string.Equals(player.SpecializationId, SeidkonaSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the base AP cost for a Seiðkona ability (before Cascade reduction).
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>The base AP cost, or 0 for passive abilities.</returns>
    private static int GetBaseApCost(SeidkonaAbilityId abilityId)
    {
        return abilityId switch
        {
            SeidkonaAbilityId.SeidrBolt => SeidrBoltApCost,
            SeidkonaAbilityId.WyrdSight => WyrdSightApCost,
            SeidkonaAbilityId.AetherAttunement => 0,     // Passive
            SeidkonaAbilityId.FatesThread => FatesThreadApCost,
            SeidkonaAbilityId.WeaveDisruption => WeaveDisruptionApCost,
            SeidkonaAbilityId.ResonanceCascade => 0,      // Passive
            SeidkonaAbilityId.VolvasVision => VolvasVisionApCost,
            SeidkonaAbilityId.AetherStorm => AetherStormApCost,
            SeidkonaAbilityId.Unraveling => UnravelingApCost,
            _ => 0
        };
    }

    /// <summary>
    /// Calculates the effective AP cost for a given base cost, applying Resonance Cascade
    /// reduction if the player has the ability unlocked and Resonance is 5+.
    /// </summary>
    /// <param name="player">The Seiðkona player to evaluate.</param>
    /// <param name="baseCost">The base AP cost of the ability.</param>
    /// <returns>The effective AP cost after any Cascade reduction (minimum 1).</returns>
    private int GetEffectiveApCostInternal(Player player, int baseCost)
    {
        // Check if Resonance Cascade is unlocked and active
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.ResonanceCascade))
            return baseCost;

        var resonance = _resonanceService.GetResonance(player);
        var currentResonance = resonance?.CurrentResonance ?? 0;

        if (currentResonance < CascadeActivationThreshold)
            return baseCost;

        // Cascade is active — reduce by 1, minimum 1
        return Math.Max(baseCost - CascadeReduction, MinimumApCost);
    }
}
