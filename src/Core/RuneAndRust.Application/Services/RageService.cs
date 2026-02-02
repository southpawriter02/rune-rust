// ═══════════════════════════════════════════════════════════════════════════════
// RageService.cs
// Service implementation for managing Berserker Rage resource, including
// rage gain from combat sources, non-combat decay, and threshold-based
// bonuses (damage, soak, fear immunity, forced targeting).
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
/// Service implementation for managing Berserker Rage resource.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RageService"/> implements the core Rage mechanics for the Berserker
/// specialization (v0.18.4). It manages the full lifecycle of rage: gain from combat
/// actions, decay during non-combat, and threshold-based bonus calculations.
/// </para>
/// <para>
/// <strong>Core Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Rage Generation:</strong> Berserkers gain rage from dealing damage
///       (floor(damage/10)), taking damage (+5), killing enemies (+15), critical hits (+10),
///       and seeing allies downed (+20).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Rage Decay:</strong> Outside combat, rage decays at 10 points per turn.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Threshold Bonuses:</strong> Damage bonus (floor(rage/10)), soak bonus
///       (0-4 by threshold), fear immunity at FrenzyBeyondReason.
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
/// <seealso cref="IRageService"/>
/// <seealso cref="RageState"/>
/// <seealso cref="RageGainResult"/>
/// <seealso cref="RageDecayResult"/>
public class RageService : IRageService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Amount of rage decay per non-combat turn.</summary>
    private const int DecayAmount = 10;

    /// <summary>Rage gain from taking damage (flat amount).</summary>
    private const int TakingDamageGain = 5;

    /// <summary>Rage gain from killing an enemy.</summary>
    private const int KillGain = 15;

    /// <summary>Rage gain from landing a critical hit.</summary>
    private const int CriticalHitGain = 10;

    /// <summary>Rage gain from ally being downed.</summary>
    private const int AllyDownedGain = 20;

    /// <summary>Party stress reduction at FrenzyBeyondReason.</summary>
    private const int FrenzyStressReduction = 10;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<RageService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="RageService"/> class.
    /// </summary>
    /// <param name="playerRepository">Repository for player data access and persistence.</param>
    /// <param name="logger">Logger for structured logging of all rage operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="playerRepository"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public RageService(
        IPlayerRepository playerRepository,
        ILogger<RageService> logger)
    {
        _playerRepository = playerRepository ??
            throw new ArgumentNullException(nameof(playerRepository));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("RageService initialized with IPlayerRepository");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public RageState? GetRageState(Guid characterId)
    {
        var player = GetPlayer(characterId);

        // Check if player has rage state (Berserker specialization)
        if (!player.HasRageState)
        {
            _logger.LogDebug(
                "Rage state queried for non-Berserker {CharacterId}: returned null",
                characterId);
            return null;
        }

        var state = player.RageState!;

        _logger.LogDebug(
            "Rage state queried for {CharacterId}: {CurrentRage}/{MaxRage} [{Threshold}]",
            characterId,
            state.CurrentRage,
            RageState.MaxRage,
            state.Threshold);

        return state;
    }

    /// <inheritdoc/>
    public int GetDamageBonus(Guid characterId)
    {
        var state = GetRageState(characterId);
        var bonus = state?.DamageBonus ?? 0;

        _logger.LogDebug(
            "Damage bonus queried for {CharacterId}: +{DamageBonus}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public int GetSoakBonus(Guid characterId)
    {
        var state = GetRageState(characterId);
        var bonus = state?.SoakBonus ?? 0;

        _logger.LogDebug(
            "Soak bonus queried for {CharacterId}: +{SoakBonus}",
            characterId,
            bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public bool IsFearImmune(Guid characterId)
    {
        var state = GetRageState(characterId);
        var immune = state?.FearImmune ?? false;

        _logger.LogDebug(
            "Fear immunity queried for {CharacterId}: {IsImmune}",
            characterId,
            immune);

        return immune;
    }

    /// <inheritdoc/>
    public bool MustAttackNearest(Guid characterId)
    {
        var state = GetRageState(characterId);
        var mustAttack = state?.MustAttackNearest ?? false;

        _logger.LogDebug(
            "Must-attack-nearest queried for {CharacterId}: {MustAttack}",
            characterId,
            mustAttack);

        return mustAttack;
    }

    /// <inheritdoc/>
    public int? GetPartyStressReduction(Guid characterId)
    {
        var state = GetRageState(characterId);

        if (state?.Threshold != RageThreshold.FrenzyBeyondReason)
        {
            return null;
        }

        _logger.LogDebug(
            "Party stress reduction queried for {CharacterId}: {Reduction}",
            characterId,
            FrenzyStressReduction);

        return FrenzyStressReduction;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMAND METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public RageGainResult GainRage(Guid characterId, int amount, RageSource source)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var player = GetPlayer(characterId);

        if (!player.HasRageState)
        {
            throw new InvalidOperationException(
                $"Character {characterId} does not have a rage state (not a Berserker).");
        }

        var previousState = player.RageState!;
        var previousRage = previousState.CurrentRage;
        var previousThreshold = previousState.Threshold;

        // Calculate rage gain based on source
        var rageGain = CalculateRageGain(amount, source);

        // Calculate new rage (capped at 100)
        var actualGain = Math.Min(rageGain, RageState.MaxRage - previousRage);
        var newRage = previousRage + actualGain;

        // Update player's rage state
        player.SetRageState(RageState.Create(characterId, newRage));
        UpdatePlayer(player);

        var newState = player.RageState!;
        var newThreshold = newState.Threshold;
        var thresholdCrossed = newThreshold != previousThreshold;

        // Create result
        var result = new RageGainResult(
            PreviousRage: previousRage,
            NewRage: newRage,
            AmountGained: actualGain,
            Source: source,
            ThresholdChanged: thresholdCrossed,
            NewThreshold: thresholdCrossed ? newThreshold : null);

        // Log rage gain
        _logger.LogInformation(
            "Rage gained for {CharacterId}: +{RageGained} [{Source}] " +
            "({PreviousRage} → {NewRage})",
            characterId,
            actualGain,
            source,
            previousRage,
            newRage);

        // Log threshold crossing
        if (thresholdCrossed)
        {
            _logger.LogInformation(
                "Rage threshold crossed for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                previousThreshold,
                newThreshold);
        }

        return result;
    }

    /// <inheritdoc/>
    public RageDecayResult ApplyDecay(Guid characterId)
    {
        var player = GetPlayer(characterId);

        if (!player.HasRageState)
        {
            throw new InvalidOperationException(
                $"Character {characterId} does not have a rage state (not a Berserker).");
        }

        var previousState = player.RageState!;
        var previousRage = previousState.CurrentRage;
        var previousThreshold = previousState.Threshold;

        // Calculate decay (minimum 0)
        var actualDecay = Math.Min(DecayAmount, previousRage);
        var newRage = previousRage - actualDecay;

        // Update player's rage state
        player.SetRageState(RageState.Create(characterId, newRage));
        UpdatePlayer(player);

        var newState = player.RageState!;
        var newThreshold = newState.Threshold;
        var thresholdDropped = newThreshold != previousThreshold;
        var zeroedOut = newRage == 0;

        // Create result
        var result = new RageDecayResult(
            PreviousRage: previousRage,
            NewRage: newRage,
            AmountDecayed: actualDecay,
            ThresholdChanged: thresholdDropped,
            NewThreshold: thresholdDropped ? newThreshold : null);

        // Log decay
        _logger.LogInformation(
            "Rage decayed for {CharacterId}: -{RageDecayed} ({PreviousRage} → {NewRage})",
            characterId,
            actualDecay,
            previousRage,
            newRage);

        // Log threshold drop
        if (thresholdDropped)
        {
            _logger.LogInformation(
                "Rage threshold dropped for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                previousThreshold,
                newThreshold);
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates rage gain based on source type and amount.
    /// </summary>
    private static int CalculateRageGain(int amount, RageSource source) =>
        source switch
        {
            RageSource.DealingDamage => amount / 10, // floor(damage / 10)
            RageSource.TakingDamage => TakingDamageGain,
            RageSource.EnemyKill => KillGain,
            RageSource.AllyDamaged => AllyDownedGain,
            RageSource.RageMaintenance => TakingDamageGain, // Same as taking damage
            _ => 0
        };

    /// <summary>
    /// Gets a player by ID, throwing <see cref="CharacterNotFoundException"/> if not found.
    /// </summary>
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

    /// <summary>
    /// Updates a player entity in the repository.
    /// </summary>
    private void UpdatePlayer(Player player)
    {
        _playerRepository.UpdateAsync(player).GetAwaiter().GetResult();
    }
}
