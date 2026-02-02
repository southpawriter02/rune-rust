// ═══════════════════════════════════════════════════════════════════════════════
// CoherenceService.cs
// Service implementation for managing Arcanist Coherence resource, including
// coherence gain/loss, cascade checks, apotheosis management, and meditation.
// Version: 0.18.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service implementation for managing Arcanist Coherence resource.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CoherenceService"/> implements the core Coherence mechanics for the Arcanist
/// specialization (v0.18.4). It manages the full lifecycle of coherence: gain/loss from
/// spellcasting, cascade checks at low thresholds, apotheosis state management, and
/// meditation recovery.
/// </para>
/// <para>
/// <strong>Core Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Coherence Range:</strong> Coherence operates on a 0-100 scale with both
///       extremes being dangerous: low coherence risks cascades, high coherence enters
///       the powerful but costly Apotheosis state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Cascade Risk:</strong> At Destabilized (0-20), 25% cascade risk per cast.
///       At Unstable (21-40), 10% cascade risk. Balanced and above have 0% risk.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Apotheosis:</strong> At 81+ coherence, Arcanist enters Apotheosis with
///       +5 spell power and 20% crit, but pays 10 stress per turn.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="IPlayerRepository"/> — Player entity persistence</description></item>
///   <item><description><see cref="IDiceService"/> — Cascade probability rolls</description></item>
///   <item><description><see cref="IStressService"/> — Apotheosis stress cost application</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> — Structured logging</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ICoherenceService"/>
/// <seealso cref="CoherenceState"/>
/// <seealso cref="CascadeResult"/>
/// <seealso cref="ApotheosisResult"/>
public class CoherenceService : ICoherenceService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Coherence recovered from meditation.</summary>
    private const int MeditationRecovery = 20;

    /// <summary>Stress cost per turn while in Apotheosis.</summary>
    private const int ApotheosisStressCost = 10;

    /// <summary>Cascade risk at Destabilized threshold (25%).</summary>
    private const int DestabilizedCascadeRisk = 25;

    /// <summary>Cascade risk at Unstable threshold (10%).</summary>
    private const int UnstableCascadeRisk = 10;

    /// <summary>Coherence loss from cascade at Unstable.</summary>
    private const int UnstableCascadeCoherenceLoss = 10;

    /// <summary>Self damage from cascade at Unstable.</summary>
    private const int UnstableCascadeSelfDamage = 5;

    /// <summary>Stress gain from cascade at Unstable.</summary>
    private const int UnstableCascadeStressGain = 10;

    /// <summary>Coherence loss from cascade at Destabilized.</summary>
    private const int DestabilizedCascadeCoherenceLoss = 20;

    /// <summary>Self damage from cascade at Destabilized.</summary>
    private const int DestabilizedCascadeSelfDamage = 15;

    /// <summary>Stress gain from cascade at Destabilized.</summary>
    private const int DestabilizedCascadeStressGain = 15;

    /// <summary>Corruption gain from cascade at Destabilized.</summary>
    private const int DestabilizedCascadeCorruptionGain = 5;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IPlayerRepository _playerRepository;
    private readonly IDiceService _diceService;
    private readonly IStressService _stressService;
    private readonly ILogger<CoherenceService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="CoherenceService"/> class.
    /// </summary>
    public CoherenceService(
        IPlayerRepository playerRepository,
        IDiceService diceService,
        IStressService stressService,
        ILogger<CoherenceService> logger)
    {
        _playerRepository = playerRepository ??
            throw new ArgumentNullException(nameof(playerRepository));
        _diceService = diceService ??
            throw new ArgumentNullException(nameof(diceService));
        _stressService = stressService ??
            throw new ArgumentNullException(nameof(stressService));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("CoherenceService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public CoherenceState? GetCoherenceState(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            _logger.LogDebug(
                "Coherence state queried for non-Arcanist {CharacterId}: returned null",
                characterId);
            return null;
        }

        var state = player.CoherenceState!;

        _logger.LogDebug(
            "Coherence state queried for {CharacterId}: {CurrentCoherence}/{MaxCoherence} [{Threshold}]",
            characterId,
            state.CurrentCoherence,
            CoherenceState.MaxCoherence,
            state.Threshold);

        return state;
    }

    /// <inheritdoc/>
    public int GetSpellPowerBonus(Guid characterId)
    {
        var state = GetCoherenceState(characterId);
        var bonus = state?.SpellPowerBonus ?? 0;

        _logger.LogDebug(
            "Spell power bonus queried for {CharacterId}: {SpellPowerBonus:+#;-#;0}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetCriticalCastChance(Guid characterId)
    {
        var state = GetCoherenceState(characterId);
        var chance = state?.CriticalCastChance ?? 0;

        _logger.LogDebug(
            "Critical cast chance queried for {CharacterId}: {CritChance}%",
            characterId,
            chance);

        return chance;
    }

    /// <inheritdoc/>
    public bool CanMeditate(Guid characterId)
    {
        var state = GetCoherenceState(characterId);
        var canMeditate = state?.CanMeditate ?? false;

        _logger.LogDebug(
            "Meditation availability queried for {CharacterId}: {CanMeditate}",
            characterId,
            canMeditate);

        return canMeditate;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMAND METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool GainCoherence(Guid characterId, int amount, CoherenceSource source)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            _logger.LogDebug(
                "Cannot gain coherence for non-Arcanist {CharacterId}",
                characterId);
            return false;
        }

        var previousState = player.CoherenceState!;
        var previousCoherence = previousState.CurrentCoherence;

        // Calculate new coherence (capped at 100)
        var newCoherence = Math.Min(
            previousCoherence + amount,
            CoherenceState.MaxCoherence);
        var actualGain = newCoherence - previousCoherence;

        // Update player's coherence state
        player.SetCoherenceState(CoherenceState.Create(characterId, newCoherence, previousState.IsCombat));
        UpdatePlayer(player);

        _logger.LogInformation(
            "Coherence gained for {CharacterId}: +{CoherenceGained} [{Source}] " +
            "({PreviousCoherence} → {NewCoherence})",
            characterId,
            actualGain,
            source,
            previousCoherence,
            newCoherence);

        return true;
    }

    /// <inheritdoc/>
    public bool LoseCoherence(Guid characterId, int amount, string reason)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            _logger.LogDebug(
                "Cannot lose coherence for non-Arcanist {CharacterId}",
                characterId);
            return false;
        }

        var previousState = player.CoherenceState!;
        var previousCoherence = previousState.CurrentCoherence;

        // Calculate new coherence (minimum 0)
        var newCoherence = Math.Max(
            previousCoherence - amount,
            CoherenceState.MinCoherence);
        var actualLoss = previousCoherence - newCoherence;

        // Update player's coherence state
        player.SetCoherenceState(CoherenceState.Create(characterId, newCoherence, previousState.IsCombat));
        UpdatePlayer(player);

        _logger.LogInformation(
            "Coherence lost for {CharacterId}: -{CoherenceLost} [{Reason}] " +
            "({PreviousCoherence} → {NewCoherence})",
            characterId,
            actualLoss,
            reason,
            previousCoherence,
            newCoherence);

        return true;
    }

    /// <inheritdoc/>
    public CascadeResult CheckCascade(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            return CascadeResult.NoCascade;
        }

        var state = player.CoherenceState!;
        var threshold = state.Threshold;

        // Determine cascade risk based on threshold
        var cascadeRisk = threshold switch
        {
            CoherenceThreshold.Destabilized => DestabilizedCascadeRisk,
            CoherenceThreshold.Unstable => UnstableCascadeRisk,
            _ => 0
        };

        // No cascade risk at Balanced and above
        if (cascadeRisk == 0)
        {
            _logger.LogDebug(
                "Cascade check for {CharacterId}: 0% risk at {Threshold}",
                characterId,
                threshold);
            return CascadeResult.NoCascade;
        }

        // Roll for cascade
        var roll = _diceService.RollTotal("1d100");
        var cascadeTriggered = roll <= cascadeRisk;

        _logger.LogDebug(
            "Cascade check for {CharacterId}: rolled {Roll} vs {Risk}% risk = {Triggered}",
            characterId,
            roll,
            cascadeRisk,
            cascadeTriggered ? "CASCADE" : "safe");

        if (!cascadeTriggered)
        {
            return CascadeResult.NoCascade;
        }

        // Determine cascade effects based on threshold
        return threshold == CoherenceThreshold.Destabilized
            ? new CascadeResult(
                CascadeTriggered: true,
                CoherenceLost: DestabilizedCascadeCoherenceLoss,
                SelfDamage: DestabilizedCascadeSelfDamage,
                StressGained: DestabilizedCascadeStressGain,
                CorruptionGained: DestabilizedCascadeCorruptionGain,
                SpellDisrupted: true,
                CascadeEffectId: null)
            : new CascadeResult(
                CascadeTriggered: true,
                CoherenceLost: UnstableCascadeCoherenceLoss,
                SelfDamage: UnstableCascadeSelfDamage,
                StressGained: UnstableCascadeStressGain,
                CorruptionGained: null,
                SpellDisrupted: true,
                CascadeEffectId: null);
    }

    /// <inheritdoc/>
    public ApotheosisResult UpdateApotheosis(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            return ApotheosisResult.NoChange(0);
        }

        var state = player.CoherenceState!;

        // Check if in Apotheosis threshold
        if (state.Threshold != CoherenceThreshold.Apotheosis)
        {
            _logger.LogDebug(
                "Apotheosis update for {CharacterId}: not in Apotheosis (at {Threshold})",
                characterId,
                state.Threshold);
            return ApotheosisResult.NoChange(state.CurrentCoherence);
        }

        // Apply stress cost for being in Apotheosis
        _stressService.ApplyStress(characterId, ApotheosisStressCost, StressSource.Heretical);

        _logger.LogInformation(
            "Apotheosis maintained for {CharacterId}: applied {StressCost} stress",
            characterId,
            ApotheosisStressCost);

        // Calculate estimated turns remaining based on stress budget
        // Note: Would need GetCurrentStress on IStressService for stress budget calc
        var currentStress = 50; // Placeholder - stress tracking handled separately
        var stressBudget = 100 - currentStress;
        var turnsRemaining = stressBudget / ApotheosisStressCost;

        return new ApotheosisResult(
            EnteredApotheosis: false,
            TurnsRemaining: turnsRemaining,
            AbilitiesUnlocked: null,
            StressCostPerTurn: ApotheosisStressCost,
            ExitedApotheosis: false,
            ExitReason: null,
            FinalCoherence: state.CurrentCoherence);
    }

    /// <inheritdoc/>
    public bool Meditate(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasCoherenceState)
        {
            _logger.LogDebug(
                "Cannot meditate for non-Arcanist {CharacterId}",
                characterId);
            return false;
        }

        var state = player.CoherenceState!;

        // Cannot meditate in combat
        if (state.IsCombat)
        {
            _logger.LogDebug(
                "Cannot meditate for {CharacterId}: in combat",
                characterId);
            return false;
        }

        // Apply meditation recovery
        var previousCoherence = state.CurrentCoherence;
        var newCoherence = Math.Min(
            previousCoherence + MeditationRecovery,
            CoherenceState.MaxCoherence);

        player.SetCoherenceState(CoherenceState.Create(characterId, newCoherence, false));
        UpdatePlayer(player);

        _logger.LogInformation(
            "Meditation performed for {CharacterId}: +{Recovered} coherence " +
            "({PreviousCoherence} → {NewCoherence})",
            characterId,
            newCoherence - previousCoherence,
            previousCoherence,
            newCoherence);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private Player GetPlayer(Guid characterId)
    {
        var player = _playerRepository.GetByIdAsync(characterId).GetAwaiter().GetResult();

        if (player is null)
        {
            _logger.LogWarning("Character not found: {CharacterId}", characterId);
            throw new CharacterNotFoundException(characterId);
        }

        return player;
    }

    private void UpdatePlayer(Player player)
    {
        _playerRepository.UpdateAsync(player).GetAwaiter().GetResult();
    }
}
