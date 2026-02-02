// ═══════════════════════════════════════════════════════════════════════════════
// MomentumService.cs
// Service implementation for managing Storm Blade Momentum resource, including
// momentum gain from combat, decay on missed attacks/inaction, chain tracking,
// and threshold-based bonuses (attack, defense, movement, bonus attacks).
// Version: 0.18.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service implementation for managing Storm Blade Momentum resource.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MomentumService"/> implements the core Momentum mechanics for the Storm Blade
/// specialization (v0.18.4). It manages the full lifecycle of momentum: gain from successful
/// attacks, decay from misses/inaction, chain tracking, and threshold-based bonus calculations.
/// </para>
/// <para>
/// <strong>Core Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Momentum Generation:</strong> Storm Blades gain momentum from successful attacks
///       (+10), killing blows (+20), chain attacks (+5 × consecutive hits), and movement.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Momentum Decay:</strong> Missed attacks cause -25 decay and break chain.
///       No action causes -15 decay. Critical damage taken causes -20 decay.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Threshold Bonuses:</strong> Attack/defense bonus (0-4), movement bonus
///       (floor(momentum/20)), bonus attacks (0-2 at high momentum).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="IPlayerRepository"/> — Player entity persistence and retrieval</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> — Structured logging for all operations</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IMomentumService"/>
/// <seealso cref="MomentumState"/>
/// <seealso cref="MomentumGainResult"/>
/// <seealso cref="MomentumDecayResult"/>
public class MomentumService : IMomentumService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Momentum decay from missed attack.</summary>
    private const int MissedAttackDecay = 25;

    /// <summary>Momentum decay from no action (idle turn).</summary>
    private const int NoActionDecay = 15;

    /// <summary>Chain bonus per consecutive hit (capped at 10).</summary>
    private const int ChainBonusPerHit = 2;

    /// <summary>Maximum chain bonus cap.</summary>
    private const int MaxChainBonus = 10;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<MomentumService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="MomentumService"/> class.
    /// </summary>
    /// <param name="playerRepository">Repository for player data access and persistence.</param>
    /// <param name="logger">Logger for structured logging of all momentum operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="playerRepository"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public MomentumService(
        IPlayerRepository playerRepository,
        ILogger<MomentumService> logger)
    {
        _playerRepository = playerRepository ??
            throw new ArgumentNullException(nameof(playerRepository));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("MomentumService initialized with IPlayerRepository");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public MomentumState? GetMomentumState(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            _logger.LogDebug(
                "Momentum state queried for non-Storm Blade {CharacterId}: returned null",
                characterId);
            return null;
        }

        var state = player.MomentumState!;

        _logger.LogDebug(
            "Momentum state queried for {CharacterId}: {CurrentMomentum}/{MaxMomentum} " +
            "[{Threshold}] Chain: {ConsecutiveHits}",
            characterId,
            state.CurrentMomentum,
            MomentumState.MaxMomentum,
            state.Threshold,
            state.ConsecutiveHits);

        return state;
    }

    /// <inheritdoc/>
    public int GetBonusAttacks(Guid characterId)
    {
        var state = GetMomentumState(characterId);
        var bonus = state?.BonusAttacks ?? 0;

        _logger.LogDebug(
            "Bonus attacks queried for {CharacterId}: {BonusAttacks}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetMovementBonus(Guid characterId)
    {
        var state = GetMomentumState(characterId);
        var bonus = state?.MovementBonus ?? 0;

        _logger.LogDebug(
            "Movement bonus queried for {CharacterId}: +{MovementBonus}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetAttackBonus(Guid characterId)
    {
        var state = GetMomentumState(characterId);
        var bonus = state?.AttackBonus ?? 0;

        _logger.LogDebug(
            "Attack bonus queried for {CharacterId}: +{AttackBonus}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetDefenseBonus(Guid characterId)
    {
        var state = GetMomentumState(characterId);
        var bonus = state?.DefenseBonus ?? 0;

        _logger.LogDebug(
            "Defense bonus queried for {CharacterId}: +{DefenseBonus}",
            characterId,
            bonus);

        return bonus;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMAND METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public MomentumGainResult GainMomentum(Guid characterId, int amount, MomentumSource source)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            throw new InvalidOperationException(
                $"Character {characterId} does not have a momentum state (not a Storm Blade).");
        }

        var previousState = player.MomentumState!;
        var previousMomentum = previousState.CurrentMomentum;
        var previousThreshold = previousState.Threshold;
        var consecutiveHits = previousState.ConsecutiveHits;

        // Calculate chain bonus
        var chainBonus = Math.Min(consecutiveHits * ChainBonusPerHit, MaxChainBonus);
        var totalGain = amount + chainBonus;

        // Calculate new momentum (capped at 100)
        var actualGain = Math.Min(totalGain, MomentumState.MaxMomentum - previousMomentum);
        var newMomentum = previousMomentum + actualGain;

        // Update player's momentum state (preserve chain count)
        player.SetMomentumState(MomentumState.Create(characterId, newMomentum, consecutiveHits));
        UpdatePlayer(player);

        var newState = player.MomentumState!;
        var newThreshold = newState.Threshold;
        var thresholdCrossed = newThreshold != previousThreshold;

        // Create result
        var result = new MomentumGainResult(
            PreviousMomentum: previousMomentum,
            NewMomentum: newMomentum,
            AmountGained: actualGain,
            Source: source,
            ChainBonus: chainBonus > 0 ? chainBonus : null,
            ThresholdChanged: thresholdCrossed,
            NewThreshold: thresholdCrossed ? newThreshold : null);

        // Log momentum gain
        _logger.LogInformation(
            "Momentum gained for {CharacterId}: +{MomentumGained} (chain bonus: +{ChainBonus}) [{Source}] " +
            "({PreviousMomentum} → {NewMomentum})",
            characterId,
            actualGain,
            chainBonus,
            source,
            previousMomentum,
            newMomentum);

        if (thresholdCrossed)
        {
            _logger.LogInformation(
                "Momentum threshold crossed for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                previousThreshold,
                newThreshold);
        }

        return result;
    }

    /// <inheritdoc/>
    public MomentumDecayResult ApplyDecay(Guid characterId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            throw new InvalidOperationException(
                $"Character {characterId} does not have a momentum state (not a Storm Blade).");
        }

        var previousState = player.MomentumState!;
        var previousMomentum = previousState.CurrentMomentum;
        var previousThreshold = previousState.Threshold;
        var previousChain = previousState.ConsecutiveHits;

        // Determine decay amount and chain break based on reason
        var decayAmount = reason.Contains("Miss", StringComparison.OrdinalIgnoreCase)
            ? MissedAttackDecay
            : NoActionDecay;
        var chainBroken = reason.Contains("Miss", StringComparison.OrdinalIgnoreCase) && previousChain > 0;

        // Calculate new values
        var actualDecay = Math.Min(decayAmount, previousMomentum);
        var newMomentum = previousMomentum - actualDecay;
        var newChain = chainBroken ? 0 : previousChain;

        // Update player's momentum state
        player.SetMomentumState(MomentumState.Create(characterId, newMomentum, newChain));
        UpdatePlayer(player);

        var newState = player.MomentumState!;
        var newThreshold = newState.Threshold;
        var thresholdDropped = newThreshold != previousThreshold;

        // Create result
        var result = new MomentumDecayResult(
            PreviousMomentum: previousMomentum,
            NewMomentum: newMomentum,
            AmountDecayed: actualDecay,
            DecayReason: reason,
            ChainBroken: chainBroken,
            ThresholdChanged: thresholdDropped,
            NewThreshold: thresholdDropped ? newThreshold : null);

        // Log decay
        _logger.LogInformation(
            "Momentum decayed for {CharacterId}: -{MomentumDecayed} [{Reason}] " +
            "({PreviousMomentum} → {NewMomentum})",
            characterId,
            actualDecay,
            reason,
            previousMomentum,
            newMomentum);

        if (chainBroken)
        {
            _logger.LogInformation(
                "Chain broken for {CharacterId}: {PreviousChain} consecutive hits lost",
                characterId,
                previousChain);
        }

        return result;
    }

    /// <inheritdoc/>
    public MomentumDecayResult ResetMomentum(Guid characterId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            throw new InvalidOperationException(
                $"Character {characterId} does not have a momentum state (not a Storm Blade).");
        }

        var previousState = player.MomentumState!;
        var previousMomentum = previousState.CurrentMomentum;
        var previousThreshold = previousState.Threshold;
        var previousChain = previousState.ConsecutiveHits;

        // Full reset to 0
        player.SetMomentumState(MomentumState.Create(characterId, 0, 0));
        UpdatePlayer(player);

        var newThreshold = MomentumThreshold.Stationary;
        var thresholdDropped = previousThreshold != newThreshold;

        // Create result
        var result = new MomentumDecayResult(
            PreviousMomentum: previousMomentum,
            NewMomentum: 0,
            AmountDecayed: previousMomentum,
            DecayReason: reason,
            ChainBroken: previousChain > 0,
            ThresholdChanged: thresholdDropped,
            NewThreshold: thresholdDropped ? newThreshold : null);

        // Log reset
        _logger.LogInformation(
            "Momentum reset for {CharacterId}: {PreviousMomentum} → 0 [{Reason}]",
            characterId,
            previousMomentum,
            reason);

        return result;
    }

    /// <inheritdoc/>
    public void RecordHit(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            return;
        }

        var previousState = player.MomentumState!;
        var newChain = previousState.ConsecutiveHits + 1;

        player.SetMomentumState(MomentumState.Create(
            characterId,
            previousState.CurrentMomentum,
            newChain));
        UpdatePlayer(player);

        _logger.LogDebug(
            "Hit recorded for {CharacterId}: consecutive hits now {ConsecutiveHits}",
            characterId,
            newChain);
    }

    /// <inheritdoc/>
    public void RecordMiss(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasMomentumState)
        {
            return;
        }

        // RecordMiss is a convenience method that breaks chain and applies decay
        ApplyDecay(characterId, "Missed Attack");
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
