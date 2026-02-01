// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionService.cs
// Service implementation for managing character Runic Blight Corruption, including
// corruption accumulation from heretical sources, Blot-Priest corruption transfer,
// rare corruption removal, stage-based skill modifier queries, resource penalty
// calculations, and WILL-based Terminal Error survival checks.
// Version: 0.18.1d
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
/// Service implementation for managing character Runic Blight Corruption.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CorruptionService"/> implements the core corruption mechanics of the Runic
/// Blight Corruption system (v0.18.1). It manages the full lifecycle of corruption:
/// accumulation from heretical sources, Blot-Priest transfer between characters, extremely
/// rare removal through narrative events, and WILL-based Terminal Error survival checks
/// when corruption reaches the maximum value of 100.
/// </para>
/// <para>
/// <strong>Core Mechanics:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Accumulation:</strong> Corruption increases from various sources
///       (MysticMagic, HereticalAbility, Artifact, Environmental, Consumable, Ritual,
///       ForlornContact, BlightTransfer). Unlike Psychic Stress, corruption does NOT
///       naturally recover — it is near-permanent.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Transfer:</strong> The Blot-Priest specialization can absorb corruption
///       from allies via <see cref="TransferCorruption"/>, reducing the ally's corruption
///       at the cost of increasing their own.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Removal:</strong> Extremely rare corruption removal through divine
///       purification rituals, Pure Essence artifacts, or story-specific events. There
///       is NO natural corruption recovery. All removals are logged at Warning level.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Terminal Error:</strong> When corruption reaches 100, a WILL-based
///       survival check is performed. The WILL dice pool is reduced by the Resolve
///       penalty (<c>floor(corruption/20) = 5</c> at 100). Success sets corruption
///       to 99; failure transforms the character into a Forlorn NPC.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Resource Penalties:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Max HP Penalty: <c>floor(corruption / 10) × 5</c>% (0-50%)</description></item>
///   <item><description>Max AP Penalty: <c>floor(corruption / 10) × 5</c>% (0-50%)</description></item>
///   <item><description>Resolve Dice Penalty: <c>floor(corruption / 20)</c> dice (0-5)</description></item>
/// </list>
/// <para>
/// <strong>Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ICorruptionRepository"/> — CorruptionTracker entity persistence and retrieval</description></item>
///   <item><description><see cref="IPlayerRepository"/> — Player entity access for WILL attribute in Terminal Error checks</description></item>
///   <item><description><see cref="IDiceService"/> — WILL-based dice rolls for Terminal Error survival</description></item>
///   <item><description><see cref="ILogger{TCategoryName}"/> — Structured logging for all corruption operations</description></item>
/// </list>
/// <para>
/// <strong>Consumers:</strong> CombatService (HP/AP penalties, corruption from abilities),
/// SkillCheckService (tech bonus, social penalty), AbilityService (heretical corruption costs),
/// Blot-Priest Mechanics (corruption transfer), UI Layer (corruption bar display).
/// </para>
/// </remarks>
/// <seealso cref="ICorruptionService"/>
/// <seealso cref="CorruptionTracker"/>
/// <seealso cref="CorruptionState"/>
/// <seealso cref="CorruptionAddResult"/>
/// <seealso cref="CorruptionTransferResult"/>
/// <seealso cref="CorruptionSkillModifiers"/>
/// <seealso cref="TerminalErrorResult"/>
public class CorruptionService : ICorruptionService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Default difficulty class for the Terminal Error survival check.</summary>
    /// <remarks>
    /// The character must roll at least 3 successes on their (penalty-reduced) WILL dice pool.
    /// At corruption 100, the Resolve penalty is 5, making survival extremely difficult for
    /// characters with low WILL. A character with WILL 6 rolls only 1 die (6 - 5 = 1).
    /// </remarks>
    private const int TerminalErrorDc = 3;

    /// <summary>Corruption value set after surviving a Terminal Error check.</summary>
    /// <remarks>
    /// Set to 99 — one point below Terminal Error. The character survived but remains
    /// deeply marked by the Blight. Any further corruption will trigger another Terminal
    /// Error check immediately.
    /// </remarks>
    private const int SurvivalCorruptionValue = 99;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly ICorruptionRepository _corruptionRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IDiceService _diceService;
    private readonly ILogger<CorruptionService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="CorruptionService"/> class.
    /// </summary>
    /// <param name="corruptionRepository">Repository for corruption tracker persistence and retrieval.</param>
    /// <param name="playerRepository">Repository for player data access (WILL attribute for Terminal Error checks).</param>
    /// <param name="diceService">Service for dice rolling mechanics (WILL-based Terminal Error survival).</param>
    /// <param name="logger">Logger for structured logging of all corruption operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="corruptionRepository"/>, <paramref name="playerRepository"/>,
    /// <paramref name="diceService"/>, or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// All four dependencies are required. The service logs its initialization at Debug level
    /// to assist with dependency injection troubleshooting.
    /// </remarks>
    public CorruptionService(
        ICorruptionRepository corruptionRepository,
        IPlayerRepository playerRepository,
        IDiceService diceService,
        ILogger<CorruptionService> logger)
    {
        _corruptionRepository = corruptionRepository ??
            throw new ArgumentNullException(nameof(corruptionRepository));
        _playerRepository = playerRepository ??
            throw new ArgumentNullException(nameof(playerRepository));
        _diceService = diceService ??
            throw new ArgumentNullException(nameof(diceService));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "CorruptionService initialized with ICorruptionRepository, IPlayerRepository, IDiceService");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public CorruptionState GetCorruptionState(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);
        var state = CorruptionState.Create(tracker.CurrentCorruption);

        _logger.LogDebug(
            "Corruption state queried for {CharacterId}: {CurrentCorruption}/{MaxCorruption} [{Stage}]",
            characterId,
            state.CurrentCorruption,
            CorruptionState.MaxCorruption,
            state.Stage);

        return state;
    }

    /// <inheritdoc/>
    public int GetMaxHpPenaltyPercent(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);
        var penalty = tracker.MaxHpPenaltyPercent;

        _logger.LogDebug(
            "Max HP penalty queried for {CharacterId}: -{HpPenalty}% (corruption: {CurrentCorruption})",
            characterId,
            penalty,
            tracker.CurrentCorruption);

        return penalty;
    }

    /// <inheritdoc/>
    public int GetMaxApPenaltyPercent(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);
        var penalty = tracker.MaxApPenaltyPercent;

        _logger.LogDebug(
            "Max AP penalty queried for {CharacterId}: -{ApPenalty}% (corruption: {CurrentCorruption})",
            characterId,
            penalty,
            tracker.CurrentCorruption);

        return penalty;
    }

    /// <inheritdoc/>
    public int GetResolveDicePenalty(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);
        var penalty = tracker.ResolveDicePenalty;

        _logger.LogDebug(
            "Resolve dice penalty queried for {CharacterId}: -{ResolvePenalty} dice (corruption: {CurrentCorruption})",
            characterId,
            penalty,
            tracker.CurrentCorruption);

        return penalty;
    }

    /// <inheritdoc/>
    public CorruptionSkillModifiers GetSkillModifiers(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);
        var modifiers = CorruptionSkillModifiers.Create(
            tracker.TechBonus, tracker.SocialPenalty, tracker.IsFactionLocked);

        _logger.LogDebug(
            "Skill modifiers queried for {CharacterId}: Tech {TechBonus}, Social {SocialPenalty}, FactionLocked={FactionLocked}",
            characterId,
            modifiers.TechBonus,
            modifiers.SocialPenalty,
            modifiers.FactionLocked);

        return modifiers;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMAND METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public CorruptionAddResult AddCorruption(
        Guid characterId,
        int amount,
        CorruptionSource source)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var tracker = GetOrCreateTracker(characterId);
        var result = tracker.AddCorruption(amount, source);

        // Log corruption change
        _logger.LogInformation(
            "Corruption added to {CharacterId}: +{AmountGained} [{Source}] ({PreviousCorruption} → {NewCorruption})",
            characterId,
            result.AmountGained,
            source,
            result.PreviousCorruption,
            result.NewCorruption);

        // Log threshold crossing
        if (result.ThresholdCrossed.HasValue)
        {
            _logger.LogWarning(
                "Corruption threshold {Threshold}% crossed for {CharacterId} (corruption: {NewCorruption})",
                result.ThresholdCrossed.Value,
                characterId,
                result.NewCorruption);
        }

        // Log stage change
        if (result.StageCrossed)
        {
            _logger.LogInformation(
                "Corruption stage changed for {CharacterId}: {PreviousStage} → {NewStage}",
                characterId,
                result.PreviousStage,
                result.NewStage);
        }

        // Log faction lock
        if (result.NowFactionLocked)
        {
            _logger.LogWarning(
                "Faction reputation LOCKED for {CharacterId}: Corruption at {NewCorruption}",
                characterId,
                result.NewCorruption);
        }

        // Log Terminal Error
        if (result.IsTerminalError)
        {
            _logger.LogError(
                "TERMINAL ERROR: {CharacterId} corruption reached {NewCorruption}",
                characterId,
                result.NewCorruption);
        }

        SaveTracker(tracker);
        return result;
    }

    /// <inheritdoc/>
    public CorruptionTransferResult TransferCorruption(
        Guid fromCharacterId,
        Guid toCharacterId,
        int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (fromCharacterId == toCharacterId)
        {
            throw new ArgumentException(
                "Cannot transfer corruption to the same character.",
                nameof(toCharacterId));
        }

        var sourceTracker = GetOrCreateTracker(fromCharacterId);
        var targetTracker = GetOrCreateTracker(toCharacterId);

        // Validate source has enough corruption to transfer
        if (sourceTracker.CurrentCorruption < amount)
        {
            _logger.LogWarning(
                "Transfer failed: {FromCharacterId} has {CurrentCorruption} corruption, requested {Amount}",
                fromCharacterId,
                sourceTracker.CurrentCorruption,
                amount);

            return CorruptionTransferResult.Create(
                success: false,
                amountTransferred: 0,
                sourceNewCorruption: sourceTracker.CurrentCorruption,
                targetNewCorruption: targetTracker.CurrentCorruption,
                targetTerminalError: false);
        }

        // Perform the transfer: reduce source, increase target
        var sourceOldCorruption = sourceTracker.CurrentCorruption;
        sourceTracker.SetCorruption(sourceTracker.CurrentCorruption - amount);

        var targetResult = targetTracker.AddCorruption(amount, CorruptionSource.BlightTransfer);

        // Persist both trackers
        SaveTracker(sourceTracker);
        SaveTracker(targetTracker);

        var result = CorruptionTransferResult.Create(
            success: true,
            amountTransferred: amount,
            sourceNewCorruption: sourceTracker.CurrentCorruption,
            targetNewCorruption: targetTracker.CurrentCorruption,
            targetTerminalError: targetResult.IsTerminalError);

        // Log successful transfer
        _logger.LogInformation(
            "Corruption transferred: {Amount} from {FromCharacterId} to {ToCharacterId} " +
            "(Source: {SourceOldCorruption} → {SourceNewCorruption}, Target: {TargetOldCorruption} → {TargetNewCorruption})",
            amount,
            fromCharacterId,
            toCharacterId,
            sourceOldCorruption,
            sourceTracker.CurrentCorruption,
            targetResult.PreviousCorruption,
            targetTracker.CurrentCorruption);

        // Log Terminal Error on target if triggered
        if (result.TargetTerminalError)
        {
            _logger.LogError(
                "Transfer caused TERMINAL ERROR for target {CharacterId}: Corruption at {NewCorruption}",
                toCharacterId,
                targetTracker.CurrentCorruption);
        }

        return result;
    }

    /// <inheritdoc/>
    public bool RemoveCorruption(Guid characterId, int amount, string reason)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var tracker = GetOrCreateTracker(characterId);

        // Validate the character has enough corruption to remove
        if (tracker.CurrentCorruption == 0 || amount > tracker.CurrentCorruption)
        {
            _logger.LogWarning(
                "Corruption removal failed: {CharacterId} has {CurrentCorruption} corruption, requested {Amount}",
                characterId,
                tracker.CurrentCorruption,
                amount);
            return false;
        }

        var previousCorruption = tracker.CurrentCorruption;
        tracker.SetCorruption(tracker.CurrentCorruption - amount);

        SaveTracker(tracker);

        _logger.LogWarning(
            "Corruption REMOVED from {CharacterId}: -{Amount} ({PreviousCorruption} → {NewCorruption}). Reason: {Reason}",
            characterId,
            amount,
            previousCorruption,
            tracker.CurrentCorruption,
            reason);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // TERMINAL ERROR METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public TerminalErrorResult PerformTerminalErrorCheck(Guid characterId)
    {
        var tracker = GetOrCreateTracker(characterId);

        if (!tracker.IsTerminalError)
        {
            throw new InvalidOperationException(
                $"Character {characterId} corruption is {tracker.CurrentCorruption}, " +
                $"not at Terminal Error ({CorruptionTracker.MaxCorruption}).");
        }

        // Get character's WILL attribute for the survival check
        var player = GetPlayer(characterId);
        var baseWill = player.Attributes.Will;

        // Apply Resolve penalty — corruption reduces WILL for this check
        // At corruption 100, penalty is floor(100/20) = 5 dice
        var resolvePenalty = tracker.ResolveDicePenalty;
        var effectiveWill = Math.Max(1, baseWill - resolvePenalty);

        _logger.LogWarning(
            "Terminal Error check for {CharacterId}: WILL {BaseWill} - {ResolvePenalty} penalty = {EffectiveWill} dice vs DC {Dc}",
            characterId,
            baseWill,
            resolvePenalty,
            effectiveWill,
            TerminalErrorDc);

        // Roll WILL dice pool vs DC using success-counting mechanics.
        // Each d10 showing 8+ is a success, showing 1 is a botch.
        // NetSuccesses = max(0, successes - botches).
        var dicePool = DicePool.D10(effectiveWill);
        var rollResult = _diceService.Roll(dicePool);
        var successes = rollResult.NetSuccesses;

        _logger.LogDebug(
            "Terminal Error roll for {CharacterId}: {EffectiveWill}d10 → " +
            "{Successes} net successes ({TotalSuccesses} successes, {TotalBotches} botches)",
            characterId,
            effectiveWill,
            successes,
            rollResult.TotalSuccesses,
            rollResult.TotalBotches);

        var survived = successes >= TerminalErrorDc;

        if (survived)
        {
            // Survived — set corruption to 99 (one point below Terminal Error)
            tracker.SetCorruption(SurvivalCorruptionValue);
            SaveTracker(tracker);

            var result = TerminalErrorResult.Success(successes, TerminalErrorDc);

            _logger.LogWarning(
                "Terminal Error SURVIVED for {CharacterId}: {Successes}/{Dc} successes, " +
                "corruption set to {FinalCorruption}{CriticalSuffix}",
                characterId,
                successes,
                TerminalErrorDc,
                SurvivalCorruptionValue,
                result.WasCriticalSuccess ? " [CRITICAL SUCCESS]" : "");

            return result;
        }
        else
        {
            // Failed — character becomes Forlorn (unplayable NPC)
            var result = TerminalErrorResult.Failure(successes, TerminalErrorDc);

            _logger.LogError(
                "Terminal Error FAILED for {CharacterId}: {Successes}/{Dc} successes — CHARACTER BECAME FORLORN",
                characterId,
                successes,
                TerminalErrorDc);

            // Note: Actual Forlorn state management is handled by the Character entity (future version).
            // The CorruptionService only reports the outcome; the caller is responsible for
            // initiating character retirement.

            return result;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an existing corruption tracker for a character, or creates a new one if none exists.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The <see cref="CorruptionTracker"/> for the character.</returns>
    /// <exception cref="CharacterNotFoundException">
    /// Thrown when no player exists with the specified <paramref name="characterId"/>
    /// and no existing tracker is found. A tracker can only be created for a valid character.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Implements a get-or-create pattern: queries the <see cref="ICorruptionRepository"/>
    /// for an existing tracker, and if none is found, validates the character exists via
    /// <see cref="IPlayerRepository"/> before creating and persisting a new tracker.
    /// </para>
    /// <para>
    /// Uses sync-over-async wrapping on repository calls because
    /// <see cref="ICorruptionService"/> defines synchronous method signatures. This is safe
    /// with the current in-memory implementations which complete synchronously.
    /// </para>
    /// </remarks>
    // TODO: v0.19.x — Refactor ICorruptionService to async when combat/rest systems are integrated.
    private CorruptionTracker GetOrCreateTracker(Guid characterId)
    {
        var tracker = _corruptionRepository.GetByCharacterIdAsync(characterId)
            .GetAwaiter().GetResult();

        if (tracker is not null)
            return tracker;

        // Validate the character exists before creating a tracker —
        // prevents orphaned trackers for invalid character IDs.
        var player = _playerRepository.GetByIdAsync(characterId)
            .GetAwaiter().GetResult();

        if (player is null)
        {
            _logger.LogWarning(
                "Character not found: {CharacterId}",
                characterId);
            throw new CharacterNotFoundException(characterId);
        }

        tracker = CorruptionTracker.Create(characterId);
        _corruptionRepository.AddAsync(tracker).GetAwaiter().GetResult();

        _logger.LogDebug(
            "Created new corruption tracker for {CharacterId}",
            characterId);

        return tracker;
    }

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
    /// <see cref="ICorruptionService"/> defines synchronous method signatures. This is safe with
    /// the current <c>InMemoryPlayerRepository</c> which completes synchronously.
    /// </remarks>
    // TODO: v0.19.x — Refactor ICorruptionService to async when combat/rest systems are integrated.
    private Player GetPlayer(Guid characterId)
    {
        var player = _playerRepository.GetByIdAsync(characterId)
            .GetAwaiter().GetResult();

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
    /// Persists a corruption tracker to the repository.
    /// </summary>
    /// <param name="tracker">The tracker to persist.</param>
    /// <remarks>
    /// Uses sync-over-async wrapping on <see cref="ICorruptionRepository.UpdateAsync"/>.
    /// </remarks>
    // TODO: v0.19.x — Refactor to async when ICorruptionService is made async.
    private void SaveTracker(CorruptionTracker tracker)
    {
        _corruptionRepository.UpdateAsync(tracker).GetAwaiter().GetResult();
    }
}
