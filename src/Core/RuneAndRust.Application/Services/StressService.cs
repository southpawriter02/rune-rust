// ═══════════════════════════════════════════════════════════════════════════════
// StressService.cs
// Service implementation for managing character Psychic Stress, including
// stress application with WILL-based resistance, rest-based recovery with
// formula-driven calculations, and post-Trauma Check reset operations.
// Version: 0.18.0d
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
/// Service implementation for managing character Psychic Stress.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StressService"/> implements the core stress mechanics of the Psychic Stress
/// system (v0.18.0). It manages the full lifecycle of stress: application (with optional
/// WILL-based resistance), recovery (via rest or named sources), and post-Trauma Check reset.
/// </para>
/// <para>
/// <strong>Core Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Resistance Checks:</strong> When stress is applied with a resist DC, a
///       WILL-based dice roll determines how much stress is actually absorbed. The number
///       of successes maps to a reduction table: 0→0%, 1→50%, 2-3→75%, 4+→100%.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Recovery Formulas:</strong> Short Rest (WILL × 2), Long Rest (WILL × 5),
///       Sanctuary (full reset to 0), Milestone (fixed 25).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Trauma Reset:</strong> After a Trauma Check resolves, stress resets to 75
///       (passed) or 50 (failed).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="IPlayerRepository"/> — Player entity persistence and retrieval</description></item>
///   <item><description><see cref="IDiceService"/> — WILL-based resistance dice rolls</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> — Structured logging for all operations</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> CombatService (defense penalty, stress application),
/// SkillCheckService (skill disadvantage), RestService (recovery),
/// TraumaService (trauma check, reset), UI Layer (stress bar display).
/// </para>
/// </remarks>
/// <seealso cref="IStressService"/>
/// <seealso cref="StressState"/>
/// <seealso cref="StressApplicationResult"/>
/// <seealso cref="StressRecoveryResult"/>
/// <seealso cref="StressCheckResult"/>
public class StressService : IStressService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Stress value after passing a Trauma Check.</summary>
    /// <remarks>
    /// The character barely maintained composure. They remain at Breaking threshold
    /// (75 stress = Defense -3) but avoided permanent psychological damage.
    /// </remarks>
    private const int TraumaPassResetValue = 75;

    /// <summary>Stress value after failing a Trauma Check.</summary>
    /// <remarks>
    /// The character broke under pressure. Stress drops further because the psyche
    /// "released" through the trauma — but the character now carries a permanent Trauma.
    /// Set to 50 (Anxious threshold, Defense -2).
    /// </remarks>
    private const int TraumaFailResetValue = 50;

    /// <summary>Short Rest recovery multiplier applied to the character's WILL attribute.</summary>
    /// <remarks>Formula: recovery = WILL × 2. Quick tactical recovery.</remarks>
    private const int ShortRestMultiplier = 2;

    /// <summary>Long Rest recovery multiplier applied to the character's WILL attribute.</summary>
    /// <remarks>Formula: recovery = WILL × 5. Significant but not full recovery.</remarks>
    private const int LongRestMultiplier = 5;

    /// <summary>Milestone recovery fixed amount (ignores WILL attribute).</summary>
    /// <remarks>Achievement reward. Always recovers exactly 25 stress.</remarks>
    private const int MilestoneRecovery = 25;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IPlayerRepository _playerRepository;
    private readonly IDiceService _diceService;
    private readonly ILogger<StressService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StressService"/> class.
    /// </summary>
    /// <param name="playerRepository">Repository for player data access and persistence.</param>
    /// <param name="diceService">Service for dice rolling mechanics (WILL-based resistance checks).</param>
    /// <param name="logger">Logger for structured logging of all stress operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="playerRepository"/>, <paramref name="diceService"/>,
    /// or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// All three dependencies are required. The service logs its initialization at Debug level
    /// to assist with dependency injection troubleshooting.
    /// </remarks>
    public StressService(
        IPlayerRepository playerRepository,
        IDiceService diceService,
        ILogger<StressService> logger)
    {
        _playerRepository = playerRepository ??
            throw new ArgumentNullException(nameof(playerRepository));
        _diceService = diceService ??
            throw new ArgumentNullException(nameof(diceService));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("StressService initialized with IPlayerRepository, IDiceService");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public StressState GetStressState(Guid characterId)
    {
        var player = GetPlayer(characterId);
        var state = StressState.Create(player.PsychicStress);

        _logger.LogDebug(
            "Stress state queried for {CharacterId}: {CurrentStress}/{MaxStress} [{Threshold}]",
            characterId,
            state.CurrentStress,
            StressState.MaxStress,
            state.Threshold);

        return state;
    }

    /// <inheritdoc/>
    public int GetDefensePenalty(Guid characterId)
    {
        var player = GetPlayer(characterId);
        var penalty = player.PsychicStress / 20;

        _logger.LogDebug(
            "Defense penalty queried for {CharacterId}: -{DefensePenalty} (stress: {CurrentStress})",
            characterId,
            penalty,
            player.PsychicStress);

        return penalty;
    }

    /// <inheritdoc/>
    public bool HasSkillDisadvantage(Guid characterId)
    {
        var player = GetPlayer(characterId);
        var hasDisadvantage = player.PsychicStress >= 80;

        _logger.LogDebug(
            "Skill disadvantage queried for {CharacterId}: {HasDisadvantage} (stress: {CurrentStress})",
            characterId,
            hasDisadvantage,
            player.PsychicStress);

        return hasDisadvantage;
    }

    /// <inheritdoc/>
    public bool RequiresTraumaCheck(Guid characterId)
    {
        var player = GetPlayer(characterId);
        var requiresCheck = player.PsychicStress >= StressState.MaxStress;

        _logger.LogDebug(
            "Trauma check requirement queried for {CharacterId}: {RequiresCheck} (stress: {CurrentStress})",
            characterId,
            requiresCheck,
            player.PsychicStress);

        return requiresCheck;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMAND METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public StressApplicationResult ApplyStress(
        Guid characterId,
        int amount,
        StressSource source,
        int resistDc = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var player = GetPlayer(characterId);
        var previousStress = player.PsychicStress;

        // Perform resistance check if DC provided
        StressCheckResult? resistanceResult = null;
        var finalStress = amount;

        if (resistDc > 0)
        {
            resistanceResult = PerformResistanceCheck(characterId, amount, resistDc);
            finalStress = resistanceResult.Value.FinalStress;

            _logger.LogDebug(
                "Resistance check for {CharacterId}: {Successes} successes, " +
                "{ReductionPercent:P0} reduction ({BaseStress} → {FinalStress})",
                characterId,
                resistanceResult.Value.Successes,
                resistanceResult.Value.ReductionPercent,
                amount,
                finalStress);
        }

        // Calculate and clamp new stress
        var newStress = Math.Clamp(
            previousStress + finalStress,
            StressState.MinStress,
            StressState.MaxStress);

        // Update player stress and persist
        UpdatePlayerStress(player, newStress);

        // Create result
        var result = StressApplicationResult.Create(
            previousStress, newStress, source, resistanceResult);

        // Log stress application
        _logger.LogInformation(
            "Stress applied to {CharacterId}: +{StressGained} [{Source}] " +
            "({PreviousStress} → {NewStress})",
            characterId,
            result.StressGained,
            source,
            previousStress,
            newStress);

        // Log threshold crossing
        if (result.ThresholdCrossed)
        {
            _logger.LogInformation(
                "Stress threshold crossed for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                result.PreviousThreshold,
                result.NewThreshold);
        }

        // Log trauma trigger
        if (result.TraumaCheckTriggered)
        {
            _logger.LogWarning(
                "TRAUMA CHECK TRIGGERED for {CharacterId}: Stress at {NewStress}",
                characterId,
                newStress);
        }

        return result;
    }

    /// <inheritdoc/>
    public StressRecoveryResult RecoverStress(Guid characterId, RestType restType)
    {
        var player = GetPlayer(characterId);
        var previousStress = player.PsychicStress;
        var will = player.Attributes.Will;

        // Calculate recovery amount using rest type formula
        var recoveryAmount = CalculateRecoveryAmount(restType, will);

        // Calculate new stress (minimum 0); Sanctuary always resets to 0
        var newStress = restType == RestType.Sanctuary
            ? StressState.MinStress
            : Math.Max(StressState.MinStress, previousStress - recoveryAmount);

        // Update player stress and persist
        UpdatePlayerStress(player, newStress);

        // Create result
        var result = StressRecoveryResult.Create(previousStress, newStress, restType);

        // Log recovery
        _logger.LogInformation(
            "Stress recovered for {CharacterId}: -{AmountRecovered} [{RecoverySource}] " +
            "({PreviousStress} → {NewStress})",
            characterId,
            result.AmountRecovered,
            restType,
            previousStress,
            newStress);

        // Log threshold improvement
        if (result.ThresholdDropped)
        {
            _logger.LogInformation(
                "Stress threshold improved for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                result.PreviousThreshold,
                result.NewThreshold);
        }

        return result;
    }

    /// <inheritdoc/>
    public StressRecoveryResult RecoverStress(Guid characterId, int amount, string source)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var player = GetPlayer(characterId);
        var previousStress = player.PsychicStress;

        // Calculate new stress (minimum 0)
        var newStress = Math.Max(StressState.MinStress, previousStress - amount);

        // Update player stress and persist
        UpdatePlayerStress(player, newStress);

        // Create result using Milestone as placeholder for custom recovery sources
        var result = StressRecoveryResult.Create(previousStress, newStress, RestType.Milestone);

        // Log recovery with custom source name
        _logger.LogInformation(
            "Stress recovered for {CharacterId}: -{AmountRecovered} [{Source}] " +
            "({PreviousStress} → {NewStress})",
            characterId,
            result.AmountRecovered,
            source,
            previousStress,
            newStress);

        // Log threshold improvement
        if (result.ThresholdDropped)
        {
            _logger.LogInformation(
                "Stress threshold improved for {CharacterId}: {PreviousThreshold} → {NewThreshold}",
                characterId,
                result.PreviousThreshold,
                result.NewThreshold);
        }

        return result;
    }

    /// <inheritdoc/>
    public void ResetAfterTraumaCheck(Guid characterId, bool passed)
    {
        var player = GetPlayer(characterId);
        var previousStress = player.PsychicStress;
        var newStress = passed ? TraumaPassResetValue : TraumaFailResetValue;

        // Update player stress and persist
        UpdatePlayerStress(player, newStress);

        // Log reset with result context
        _logger.LogInformation(
            "Stress reset after Trauma Check for {CharacterId}: " +
            "{Result} → {NewStress} (was {PreviousStress})",
            characterId,
            passed ? "PASSED" : "FAILED",
            newStress,
            previousStress);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public StressCheckResult PerformResistanceCheck(
        Guid characterId,
        int baseStress,
        int dc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseStress);
        ArgumentOutOfRangeException.ThrowIfNegative(dc);

        var player = GetPlayer(characterId);
        var will = player.Attributes.Will;

        // Roll WILL dice pool vs DC using success-counting mechanics.
        // Each d10 showing 8+ is a success, showing 1 is a botch.
        // NetSuccesses = max(0, successes - botches).
        var dicePool = DicePool.D10(will);
        var rollResult = _diceService.Roll(dicePool);
        var successes = rollResult.NetSuccesses;

        _logger.LogDebug(
            "WILL resistance roll for {CharacterId}: {Will}d10 → " +
            "{Successes} net successes ({TotalSuccesses} successes, {TotalBotches} botches)",
            characterId,
            will,
            successes,
            rollResult.TotalSuccesses,
            rollResult.TotalBotches);

        // Create resistance result (maps successes to reduction via 0%/50%/75%/100% table)
        return StressCheckResult.Create(successes, baseStress);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a player by ID, throwing <see cref="CharacterNotFoundException"/> if not found.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The <see cref="Player"/> entity.</returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no player exists with the specified <paramref name="characterId"/>.
    /// </exception>
    /// <remarks>
    /// Uses sync-over-async wrapping on <see cref="IPlayerRepository.GetByIdAsync"/> because
    /// <see cref="IStressService"/> defines synchronous method signatures. This is safe with
    /// the current <c>InMemoryPlayerRepository</c> which completes synchronously.
    /// </remarks>
    // TODO: v0.19.x — Refactor IStressService to async when combat/rest systems are integrated.
    private Player GetPlayer(Guid characterId)
    {
        var player = _playerRepository.GetByIdAsync(characterId).GetAwaiter().GetResult();

        if (player is null)
        {
            _logger.LogWarning(
                "Character not found: {CharacterId}",
                characterId);
            throw new CharacterNotFoundException(characterId);
        }

        return player;
    }

    /// <summary>
    /// Calculates the stress recovery amount based on rest type and the character's WILL attribute.
    /// </summary>
    /// <param name="restType">The type of rest determining the recovery formula.</param>
    /// <param name="will">The character's WILL attribute value.</param>
    /// <returns>
    /// The recovery amount: Short = WILL × 2, Long = WILL × 5,
    /// Sanctuary = <see cref="int.MaxValue"/> (full reset handled by caller),
    /// Milestone = 25 (fixed).
    /// </returns>
    private static int CalculateRecoveryAmount(RestType restType, int will) =>
        restType switch
        {
            RestType.Short => will * ShortRestMultiplier,
            RestType.Long => will * LongRestMultiplier,
            RestType.Sanctuary => int.MaxValue, // Full reset handled by caller
            RestType.Milestone => MilestoneRecovery,
            _ => 0
        };

    /// <summary>
    /// Updates a player's psychic stress value and persists the change to the repository.
    /// </summary>
    /// <param name="player">The player entity to update.</param>
    /// <param name="newStress">The new stress value (will be clamped to [0, 100] by the entity).</param>
    /// <remarks>
    /// Uses sync-over-async wrapping on <see cref="IPlayerRepository.UpdateAsync"/> because
    /// <see cref="IStressService"/> defines synchronous method signatures.
    /// </remarks>
    // TODO: v0.19.x — Refactor to async when IStressService is made async.
    private void UpdatePlayerStress(Player player, int newStress)
    {
        player.SetPsychicStress(newStress);
        _playerRepository.UpdateAsync(player).GetAwaiter().GetResult();
    }
}
