using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for executing multi-phase tracking pursuits.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Extended Tracking System mechanics for the Wasteland Survival skill.
/// Manages the complete tracking lifecycle through phase transitions:
/// <code>
/// Acquisition → Pursuit → ClosingIn → TargetFound
///      ↓            ↓           ↓
///      └──────── Lost ←────────┘
///                  ↓
///                Cold
/// </code>
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Trail age determines base DC (8-28)</description></item>
///   <item><description>Condition modifiers adjust effective DC</description></item>
///   <item><description>Terrain determines check frequency</description></item>
///   <item><description>3 failures in acquisition or recovery → trail cold</description></item>
///   <item><description>Fumble during acquisition → immediate cold</description></item>
/// </list>
/// </para>
/// <para>
/// Counter-Tracking Integration (v0.15.5b):
/// <list type="bullet">
///   <item><description>When a target uses counter-tracking, their concealment DC replaces the base DC</description></item>
///   <item><description>Use <see cref="TrackingState.ActualBaseDc"/> which returns ContestedDc when set</description></item>
///   <item><description>Time multipliers from concealment affect pursuit duration</description></item>
///   <item><description>Counter-tracking can be applied via <see cref="ICounterTrackingService"/></description></item>
/// </list>
/// </para>
/// </remarks>
public class TrackingService : ITrackingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in tracking checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    /// <summary>
    /// DC for estimating target count (optional acquisition check).
    /// </summary>
    private const int EstimateTargetCountDc = 10;

    /// <summary>
    /// DC for estimating trail age (optional acquisition check).
    /// </summary>
    private const int EstimateTrailAgeDc = 12;

    /// <summary>
    /// DC modifier for acquisition retry after failure.
    /// </summary>
    private const int AcquisitionRetryDcModifier = 2;

    /// <summary>
    /// DC modifier for spiral search recovery.
    /// </summary>
    private const int SpiralSearchDcModifier = 4;

    /// <summary>
    /// DC modifier for return to last known recovery.
    /// </summary>
    private const int ReturnToLastKnownDcModifier = 8;

    /// <summary>
    /// Number of failed attempts that marks trail as cold.
    /// </summary>
    private const int MaxFailedAttempts = 3;

    /// <summary>
    /// Distance threshold for transitioning to ClosingIn phase (in feet).
    /// </summary>
    private const int ClosingInThresholdFeet = 500;

    /// <summary>
    /// Distance for automatic success in ClosingIn phase (in feet).
    /// </summary>
    private const int AutoSuccessDistanceFeet = 50;

    /// <summary>
    /// DC modifier when within 500 feet of target.
    /// </summary>
    private const int Within500FtDcModifier = -4;

    /// <summary>
    /// DC modifier when within 100 feet of target.
    /// </summary>
    private const int Within100FtDcModifier = -6;

    /// <summary>
    /// Required skill rank for Cold trails.
    /// </summary>
    private const int MasterRankRequirement = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly ITrackingStateRepository _repository;
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<TrackingService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the TrackingService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="repository">Repository for persisting tracking state.</param>
    /// <param name="configProvider">Configuration provider for skill definitions.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public TrackingService(
        SkillCheckService skillCheckService,
        ITrackingStateRepository repository,
        IGameConfigurationProvider configProvider,
        ILogger<TrackingService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("TrackingService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY TRACKING OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<TrackingResult> InitiateTrackingAsync(
        Player player,
        string targetDescription,
        TrailAge trailAge,
        TrackingModifiers modifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDescription, nameof(targetDescription));
        ArgumentNullException.ThrowIfNull(modifiers, nameof(modifiers));

        var playerId = player.Id.ToString();

        _logger.LogInformation(
            "Initiating tracking for Player={PlayerId}, Target='{Target}', TrailAge={TrailAge}",
            playerId, targetDescription, trailAge);

        // Check if player already has active tracking
        if (await _repository.HasActiveTrackingAsync(playerId, cancellationToken))
        {
            _logger.LogWarning(
                "Player {PlayerId} attempted to start tracking while already having active pursuit",
                playerId);
            throw new InvalidOperationException(
                "Cannot initiate new tracking - player already has an active pursuit. " +
                "Abandon the current tracking first.");
        }

        // Validate master rank requirement for Cold trails
        if (trailAge == TrailAge.Cold && !HasMasterRank(player))
        {
            _logger.LogWarning(
                "Player {PlayerId} attempted Cold trail without Master rank",
                playerId);
            throw new InvalidOperationException(
                "Cold trails (DC 28) require Master rank (Rank 5) in Wasteland Survival.");
        }

        // Create new tracking state
        var state = TrackingState.Create(playerId, targetDescription, trailAge);

        _logger.LogDebug(
            "Created TrackingState: Id={TrackingId}, BaseDc={BaseDc}",
            state.TrackingId, state.BaseDc);

        // Calculate effective DC (uses ActualBaseDc which respects ContestedDc from counter-tracking)
        var baseDc = state.ActualBaseDc;
        var effectiveDc = Math.Max(0, baseDc + modifiers.TotalDcModifier);

        _logger.LogDebug(
            "Acquisition check: BaseDc={BaseDc}, HasCounterTracking={HasCT}, ModifierDc={ModifierDc}, EffectiveDc={EffectiveDc}",
            baseDc, state.HasCounterTracking, modifiers.TotalDcModifier, effectiveDc);

        // Perform the acquisition check
        var context = modifiers.ToSkillContext();
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            effectiveDc,
            $"Tracking Acquisition ({trailAge})",
            context);

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}, IsFumble={IsFumble}",
            checkResult.NetSuccesses, checkResult.Outcome, checkResult.IsFumble);

        // Create tracking check record
        var check = TrackingCheck.FromSkillCheck(
            TrackingPhase.Acquisition,
            baseDc,
            effectiveDc,
            checkResult,
            modifiers.Terrain,
            0f,
            modifiers.ToString());

        // Handle fumble - immediate cold
        if (checkResult.IsFumble)
        {
            _logger.LogInformation(
                "Fumble during acquisition for TrackingId={TrackingId} - trail immediately cold",
                state.TrackingId);

            state.RecordFailure(check);
            state.TransitionToCold();
            await _repository.SaveAsync(state, cancellationToken);

            return TrackingResult.TrailGoneCold(
                TrackingPhase.Acquisition,
                check,
                state,
                "A critical error in your tracking technique has obliterated all signs of the trail. " +
                "The trail is completely cold - you must find a new lead.");
        }

        // Handle success
        if (checkResult.IsSuccess)
        {
            _logger.LogInformation(
                "Successful acquisition for TrackingId={TrackingId} - transitioning to Pursuit",
                state.TrackingId);

            state.RecordSuccess(check, 0f, modifiers.Terrain);
            state.TransitionToPursuit();
            await _repository.SaveAsync(state, cancellationToken);

            var direction = DetermineDirection();
            var discovery = TrackingDiscovery.DirectionOnly(direction);

            var nextActions = new List<string>
            {
                "Continue pursuit (track continue)",
                "Estimate target count (optional, DC 10)",
                "Estimate trail age (optional, DC 12)",
                "Abandon tracking"
            };

            var narrative = checkResult.Outcome switch
            {
                SkillOutcome.CriticalSuccess =>
                    $"You expertly locate and identify the trail heading {direction}. " +
                    $"The signs are crystal clear - this is the work of a master tracker.",
                SkillOutcome.ExceptionalSuccess =>
                    $"You quickly find the trail heading {direction}. " +
                    $"The target's passage is evident in the disturbed terrain.",
                SkillOutcome.FullSuccess =>
                    $"You locate the trail heading {direction}. " +
                    $"The signs of passage are clear enough to follow.",
                _ => // MarginalSuccess
                    $"After careful searching, you find faint signs of the trail heading {direction}. " +
                    $"It's subtle, but followable."
            };

            return TrackingResult.Success(
                TrackingPhase.Pursuit,
                TrackingPhase.Acquisition,
                check,
                state,
                narrative,
                nextActions,
                discovery);
        }

        // Handle failure
        _logger.LogInformation(
            "Failed acquisition attempt for TrackingId={TrackingId}, FailedAttempts={Failed}/{Max}",
            state.TrackingId, state.FailedAttemptsInPhase + 1, MaxFailedAttempts);

        state.RecordFailure(check);

        // Check for cold trail
        if (state.FailedAttemptsInPhase >= MaxFailedAttempts)
        {
            _logger.LogInformation(
                "Max failed attempts reached for TrackingId={TrackingId} - trail cold",
                state.TrackingId);

            state.TransitionToCold();
            await _repository.SaveAsync(state, cancellationToken);

            return TrackingResult.TrailGoneCold(
                TrackingPhase.Acquisition,
                check,
                state,
                "After three failed attempts, you've exhausted your options. " +
                "The trail has gone completely cold. You must find a new lead to resume tracking.");
        }

        // Can retry
        state.IncreaseCumulativeModifier(AcquisitionRetryDcModifier);
        await _repository.SaveAsync(state, cancellationToken);

        var retryNextActions = new List<string>
        {
            $"Retry acquisition (DC +{AcquisitionRetryDcModifier}, 10 minutes)",
            "Abandon tracking"
        };

        return TrackingResult.Failure(
            TrackingPhase.Acquisition,
            TrackingPhase.Acquisition,
            check,
            state,
            $"You fail to locate the trail. You can retry after 10 minutes at DC +{AcquisitionRetryDcModifier}. " +
            $"Failed attempts: {state.FailedAttemptsInPhase}/{MaxFailedAttempts}.",
            retryNextActions);
    }

    /// <inheritdoc/>
    public async Task<TrackingResult> ContinuePursuitAsync(
        Player player,
        string trackingId,
        TrackingModifiers modifiers,
        float distanceAdvanced,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));
        ArgumentNullException.ThrowIfNull(modifiers, nameof(modifiers));

        _logger.LogInformation(
            "Continuing pursuit for TrackingId={TrackingId}, Distance={Distance}mi",
            trackingId, distanceAdvanced);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot continue pursuit - tracking is {state.Status}.");
        }

        if (state.CurrentPhase != TrackingPhase.Pursuit)
        {
            throw new InvalidOperationException(
                $"Cannot continue pursuit from {state.CurrentPhase} phase. " +
                "Must be in Pursuit phase.");
        }

        // Calculate effective DC (uses ActualBaseDc which respects ContestedDc from counter-tracking)
        var baseDc = state.ActualBaseDc;
        var cumulativeModifier = state.CumulativeDcModifier;
        var conditionModifier = modifiers.TotalDcModifier;
        var effectiveDc = Math.Max(0, baseDc + cumulativeModifier + conditionModifier);

        _logger.LogDebug(
            "Pursuit check: BaseDc={BaseDc}, HasCounterTracking={HasCT}, Cumulative={Cumulative}, Condition={Condition}, EffectiveDc={EffectiveDc}",
            baseDc, state.HasCounterTracking, cumulativeModifier, conditionModifier, effectiveDc);

        // Perform the pursuit check
        var context = modifiers.ToSkillContext();
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            effectiveDc,
            $"Tracking Pursuit ({state.TrailAge})",
            context);

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}",
            checkResult.NetSuccesses, checkResult.Outcome);

        // Create tracking check record
        var totalDistance = state.DistanceCovered + distanceAdvanced;
        var check = TrackingCheck.FromSkillCheck(
            TrackingPhase.Pursuit,
            baseDc,
            effectiveDc,
            checkResult,
            modifiers.Terrain,
            totalDistance,
            modifiers.ToString());

        // Handle success
        if (checkResult.IsSuccess)
        {
            state.RecordSuccess(check, distanceAdvanced, modifiers.Terrain);

            _logger.LogInformation(
                "Successful pursuit for TrackingId={TrackingId}, TotalDistance={Distance}mi",
                trackingId, state.DistanceCovered);

            await _repository.SaveAsync(state, cancellationToken);

            var checkInterval = modifiers.GetCheckIntervalMiles();
            var nextActions = new List<string>
            {
                $"Continue pursuit (next check in {checkInterval:F1} miles)",
                "Abandon tracking"
            };

            var narrative = checkResult.Outcome switch
            {
                SkillOutcome.CriticalSuccess =>
                    $"You follow the trail with exceptional skill, covering {distanceAdvanced:F1} miles. " +
                    $"Total distance: {state.DistanceCovered:F1} miles.",
                SkillOutcome.ExceptionalSuccess =>
                    $"You maintain the trail easily across {distanceAdvanced:F1} miles. " +
                    $"Total distance: {state.DistanceCovered:F1} miles.",
                SkillOutcome.FullSuccess =>
                    $"You follow the trail for {distanceAdvanced:F1} miles. " +
                    $"Total distance: {state.DistanceCovered:F1} miles.",
                _ =>
                    $"You manage to maintain the trail for {distanceAdvanced:F1} miles, though it was difficult. " +
                    $"Total distance: {state.DistanceCovered:F1} miles."
            };

            return TrackingResult.Success(
                TrackingPhase.Pursuit,
                TrackingPhase.Pursuit,
                check,
                state,
                narrative,
                nextActions);
        }

        // Handle failure - transition to Lost
        _logger.LogInformation(
            "Failed pursuit for TrackingId={TrackingId} - transitioning to Lost",
            trackingId);

        state.RecordFailure(check);
        state.TransitionToLost();
        await _repository.SaveAsync(state, cancellationToken);

        var recoveryActions = new List<string>
        {
            "Backtrack (same DC, 10 minutes)",
            $"Spiral search (+{SpiralSearchDcModifier} DC, 30 minutes)",
            $"Return to last known (+{ReturnToLastKnownDcModifier} DC, 1 hour)",
            "Abandon tracking"
        };

        var fumbleNarrative = checkResult.IsFumble
            ? "You've completely lost your bearings. "
            : "";

        return TrackingResult.Failure(
            TrackingPhase.Lost,
            TrackingPhase.Pursuit,
            check,
            state,
            $"{fumbleNarrative}You've lost the trail after {state.DistanceCovered:F1} miles. " +
            "Choose a recovery method to try to relocate it.",
            recoveryActions);
    }

    /// <inheritdoc/>
    public async Task<TrackingResult> AttemptRecoveryAsync(
        Player player,
        string trackingId,
        RecoveryType recoveryType,
        TrackingModifiers modifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));
        ArgumentNullException.ThrowIfNull(modifiers, nameof(modifiers));

        _logger.LogInformation(
            "Attempting recovery for TrackingId={TrackingId}, Type={RecoveryType}",
            trackingId, recoveryType);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot attempt recovery - tracking is {state.Status}.");
        }

        if (state.CurrentPhase != TrackingPhase.Lost)
        {
            throw new InvalidOperationException(
                $"Cannot attempt recovery from {state.CurrentPhase} phase. " +
                "Must be in Lost phase.");
        }

        // Calculate DC modifier based on recovery type
        var recoveryDcModifier = recoveryType switch
        {
            RecoveryType.Backtrack => 0,
            RecoveryType.SpiralSearch => SpiralSearchDcModifier,
            RecoveryType.ReturnToLastKnown => ReturnToLastKnownDcModifier,
            _ => 0
        };

        var recoveryName = recoveryType switch
        {
            RecoveryType.Backtrack => "Backtrack",
            RecoveryType.SpiralSearch => "Spiral Search",
            RecoveryType.ReturnToLastKnown => "Return to Last Known",
            _ => "Recovery"
        };

        // Calculate effective DC (uses ActualBaseDc which respects ContestedDc from counter-tracking)
        var baseDc = state.ActualBaseDc;
        var cumulativeModifier = state.CumulativeDcModifier;
        var conditionModifier = modifiers.TotalDcModifier;
        var effectiveDc = Math.Max(0, baseDc + cumulativeModifier + conditionModifier + recoveryDcModifier);

        _logger.LogDebug(
            "Recovery check: BaseDc={BaseDc}, HasCounterTracking={HasCT}, Cumulative={Cumulative}, Condition={Condition}, Recovery={Recovery}, EffectiveDc={EffectiveDc}",
            baseDc, state.HasCounterTracking, cumulativeModifier, conditionModifier, recoveryDcModifier, effectiveDc);

        // Perform the recovery check
        var context = modifiers.ToSkillContext();
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            effectiveDc,
            $"Tracking Recovery ({recoveryName})",
            context);

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}",
            checkResult.NetSuccesses, checkResult.Outcome);

        // Create tracking check record
        var check = TrackingCheck.FromSkillCheck(
            TrackingPhase.Lost,
            baseDc,
            effectiveDc,
            checkResult,
            modifiers.Terrain,
            state.DistanceCovered,
            $"{recoveryName}: {modifiers}");

        // Handle success - recover to Pursuit
        if (checkResult.IsSuccess)
        {
            _logger.LogInformation(
                "Successful recovery for TrackingId={TrackingId} - returning to Pursuit",
                trackingId);

            state.RecordSuccess(check, 0f, modifiers.Terrain);
            state.RecoverToPursuit();
            await _repository.SaveAsync(state, cancellationToken);

            var nextActions = new List<string>
            {
                "Continue pursuit",
                "Abandon tracking"
            };

            var narrative = $"Using the {recoveryName.ToLower()} technique, you successfully " +
                           "relocate the trail. You can continue the pursuit.";

            return TrackingResult.Success(
                TrackingPhase.Pursuit,
                TrackingPhase.Lost,
                check,
                state,
                narrative,
                nextActions);
        }

        // Handle failure
        _logger.LogInformation(
            "Failed recovery attempt for TrackingId={TrackingId}, FailedAttempts={Failed}/{Max}",
            trackingId, state.FailedAttemptsInPhase + 1, MaxFailedAttempts);

        state.RecordFailure(check);

        // Check for cold trail
        if (state.FailedAttemptsInPhase >= MaxFailedAttempts)
        {
            _logger.LogInformation(
                "Max failed recovery attempts reached for TrackingId={TrackingId} - trail cold",
                trackingId);

            state.TransitionToCold();
            await _repository.SaveAsync(state, cancellationToken);

            return TrackingResult.TrailGoneCold(
                TrackingPhase.Lost,
                check,
                state,
                "After three failed recovery attempts, the trail has gone completely cold. " +
                "You must find a new lead to resume tracking.");
        }

        await _repository.SaveAsync(state, cancellationToken);

        var recoveryActions = new List<string>
        {
            "Backtrack (same DC, 10 minutes)",
            $"Spiral search (+{SpiralSearchDcModifier} DC, 30 minutes)",
            $"Return to last known (+{ReturnToLastKnownDcModifier} DC, 1 hour)",
            "Abandon tracking"
        };

        return TrackingResult.Failure(
            TrackingPhase.Lost,
            TrackingPhase.Lost,
            check,
            state,
            $"The {recoveryName.ToLower()} attempt fails. " +
            $"Failed recovery attempts: {state.FailedAttemptsInPhase}/{MaxFailedAttempts}.",
            recoveryActions);
    }

    /// <inheritdoc/>
    public async Task<TrackingResult> CloseInAsync(
        Player player,
        string trackingId,
        TrackingModifiers modifiers,
        int currentDistanceFeet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));
        ArgumentNullException.ThrowIfNull(modifiers, nameof(modifiers));

        _logger.LogInformation(
            "Closing in for TrackingId={TrackingId}, Distance={Distance}ft",
            trackingId, currentDistanceFeet);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot close in - tracking is {state.Status}.");
        }

        if (state.CurrentPhase != TrackingPhase.ClosingIn && state.CurrentPhase != TrackingPhase.Pursuit)
        {
            throw new InvalidOperationException(
                $"Cannot close in from {state.CurrentPhase} phase. " +
                "Must be in Pursuit or ClosingIn phase.");
        }

        // Transition to ClosingIn if not already there
        if (state.CurrentPhase == TrackingPhase.Pursuit)
        {
            state.TransitionToClosingIn(currentDistanceFeet);
            _logger.LogDebug("Transitioned to ClosingIn phase");
        }

        state.UpdateDistanceToTarget(currentDistanceFeet);

        // Auto-success within 50 feet
        if (currentDistanceFeet <= AutoSuccessDistanceFeet)
        {
            _logger.LogInformation(
                "Auto-success for TrackingId={TrackingId} - within {Distance}ft",
                trackingId, AutoSuccessDistanceFeet);

            // Create auto-success check
            var autoCheck = TrackingCheck.Create(
                Guid.NewGuid().ToString(),
                TrackingPhase.ClosingIn,
                state.BaseDc,
                0, // Auto-success, no effective DC
                99, // High net successes for auto-success
                SkillOutcome.CriticalSuccess,
                modifiers.Terrain,
                state.DistanceCovered,
                DateTime.UtcNow,
                "Auto-success: Within 50ft");

            state.RecordSuccess(autoCheck, 0f, modifiers.Terrain);
            state.MarkTargetFound();
            await _repository.SaveAsync(state, cancellationToken);

            return TrackingResult.TargetLocated(
                TrackingPhase.ClosingIn,
                autoCheck,
                state,
                "You're so close that you can practically see your target. " +
                "The encounter begins!");
        }

        // Calculate distance-based DC modifier
        var distanceDcModifier = currentDistanceFeet <= 100
            ? Within100FtDcModifier
            : Within500FtDcModifier;

        // Calculate effective DC (uses ActualBaseDc which respects ContestedDc from counter-tracking)
        var baseDc = state.ActualBaseDc;
        var cumulativeModifier = state.CumulativeDcModifier;
        var conditionModifier = modifiers.TotalDcModifier;
        var effectiveDc = Math.Max(0, baseDc + cumulativeModifier + conditionModifier + distanceDcModifier);

        _logger.LogDebug(
            "Closing check: BaseDc={BaseDc}, HasCounterTracking={HasCT}, Distance modifier={DistMod}, EffectiveDc={EffectiveDc}",
            baseDc, state.HasCounterTracking, distanceDcModifier, effectiveDc);

        // Perform the closing check
        var context = modifiers.ToSkillContext();
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            effectiveDc,
            $"Tracking Close In ({currentDistanceFeet}ft)",
            context);

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}",
            checkResult.NetSuccesses, checkResult.Outcome);

        // Create tracking check record
        var check = TrackingCheck.FromSkillCheck(
            TrackingPhase.ClosingIn,
            baseDc,
            effectiveDc,
            checkResult,
            modifiers.Terrain,
            state.DistanceCovered,
            $"Closing ({currentDistanceFeet}ft): {modifiers}");

        // Handle success - target found
        if (checkResult.IsSuccess)
        {
            _logger.LogInformation(
                "Successfully closed in for TrackingId={TrackingId} - target found",
                trackingId);

            state.RecordSuccess(check, 0f, modifiers.Terrain);
            state.MarkTargetFound();
            await _repository.SaveAsync(state, cancellationToken);

            var narrative = checkResult.Outcome switch
            {
                SkillOutcome.CriticalSuccess =>
                    "You close in with perfect stealth and positioning. " +
                    "You have the advantage - the target is unaware of your presence!",
                SkillOutcome.ExceptionalSuccess =>
                    "You successfully close in on your target without being detected. " +
                    "The encounter begins on your terms.",
                SkillOutcome.FullSuccess =>
                    "You locate your target. The encounter begins.",
                _ =>
                    "You find your target, but they may have noticed your approach. " +
                    "The encounter begins - be ready!"
            };

            return TrackingResult.TargetLocated(
                TrackingPhase.ClosingIn,
                check,
                state,
                narrative);
        }

        // Handle failure - target may be alerted, transition to Lost
        _logger.LogInformation(
            "Failed closing in for TrackingId={TrackingId} - target may be alerted",
            trackingId);

        state.RecordFailure(check);
        state.TransitionToLost();
        await _repository.SaveAsync(state, cancellationToken);

        var recoveryActions = new List<string>
        {
            "Backtrack (same DC, 10 minutes)",
            $"Spiral search (+{SpiralSearchDcModifier} DC, 30 minutes)",
            $"Return to last known (+{ReturnToLastKnownDcModifier} DC, 1 hour)",
            "Abandon tracking"
        };

        var alertNarrative = checkResult.IsFumble
            ? "You've made a terrible mistake - the target has definitely spotted you and is now fleeing! "
            : "You lose sight of the target. They may have been alerted to your presence. ";

        return TrackingResult.Failure(
            TrackingPhase.Lost,
            TrackingPhase.ClosingIn,
            check,
            state,
            alertNarrative + "Choose a recovery method to try to relocate them.",
            recoveryActions);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE QUERY OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<TrackingState?> GetTrackingStateAsync(
        string trackingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));

        _logger.LogDebug("Getting tracking state for TrackingId={TrackingId}", trackingId);

        return await _repository.GetByIdAsync(trackingId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TrackingState?> GetActiveTrackingAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        _logger.LogDebug("Getting active tracking for PlayerId={PlayerId}", playerId);

        return await _repository.GetActiveByTrackerAsync(playerId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> HasActiveTrackingAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        return await _repository.HasActiveTrackingAsync(playerId, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING MANAGEMENT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task AbandonTrackingAsync(
        string trackingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));

        _logger.LogInformation("Abandoning tracking TrackingId={TrackingId}", trackingId);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot abandon tracking - already {state.Status}.");
        }

        state.Abandon();
        await _repository.SaveAsync(state, cancellationToken);

        _logger.LogInformation("Tracking abandoned: TrackingId={TrackingId}", trackingId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTIONAL ESTIMATION CHECKS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<int?> EstimateTargetCountAsync(
        Player player,
        string trackingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));

        _logger.LogDebug("Estimating target count for TrackingId={TrackingId}", trackingId);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot estimate target count - tracking is {state.Status}.");
        }

        // Perform DC 10 check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WastelandSurvivalSkillId,
            EstimateTargetCountDc,
            "Estimate Target Count");

        if (!checkResult.IsSuccess)
        {
            _logger.LogDebug("Failed to estimate target count");
            return null;
        }

        // Estimate based on margin
        var baseCount = 2 + Random.Shared.Next(1, 4); // 2-5 base estimate
        var variance = Math.Max(0, 3 - checkResult.Margin); // Less variance with better roll

        var estimate = baseCount + Random.Shared.Next(-variance, variance + 1);
        estimate = Math.Max(1, estimate);

        state.SetEstimatedTargetCount(estimate);
        await _repository.SaveAsync(state, cancellationToken);

        _logger.LogInformation(
            "Estimated target count for TrackingId={TrackingId}: {Count}",
            trackingId, estimate);

        return estimate;
    }

    /// <inheritdoc/>
    public async Task<TrailAge?> EstimateTrailAgeAsync(
        Player player,
        string trackingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingId, nameof(trackingId));

        _logger.LogDebug("Estimating trail age for TrackingId={TrackingId}", trackingId);

        var state = await _repository.GetByIdAsync(trackingId, cancellationToken)
            ?? throw new InvalidOperationException($"Tracking not found: {trackingId}");

        if (!state.IsActive)
        {
            throw new InvalidOperationException(
                $"Cannot estimate trail age - tracking is {state.Status}.");
        }

        // Perform DC 12 check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WastelandSurvivalSkillId,
            EstimateTrailAgeDc,
            "Estimate Trail Age");

        if (!checkResult.IsSuccess)
        {
            _logger.LogDebug("Failed to estimate trail age");
            return null;
        }

        // Return the actual trail age (with possible margin of error for lower successes)
        var actualAge = state.TrailAge;

        // For marginal success, might be off by one category
        if (checkResult.Outcome == SkillOutcome.MarginalSuccess && Random.Shared.NextDouble() < 0.3)
        {
            // 30% chance to be off by one category
            var offset = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
            var estimatedValue = (int)actualAge + offset;
            estimatedValue = Math.Clamp(estimatedValue, (int)TrailAge.Obvious, (int)TrailAge.Cold);
            actualAge = (TrailAge)estimatedValue;
        }

        _logger.LogInformation(
            "Estimated trail age for TrackingId={TrackingId}: {Age}",
            trackingId, actualAge);

        return actualAge;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the direction of travel (placeholder implementation).
    /// </summary>
    /// <returns>A cardinal or intercardinal direction string.</returns>
    /// <remarks>
    /// In a full implementation, this would be determined by the game's
    /// spatial system and the target's actual movement direction.
    /// </remarks>
    private static string DetermineDirection()
    {
        var directions = new[]
        {
            "North", "Northeast", "East", "Southeast",
            "South", "Southwest", "West", "Northwest"
        };
        return directions[Random.Shared.Next(directions.Length)];
    }

    /// <summary>
    /// Checks if the player has Master rank (Rank 5) in Wasteland Survival.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has Master rank, false otherwise.</returns>
    /// <remarks>
    /// This is a placeholder implementation. In a full implementation,
    /// this would check the player's actual skill rank through the
    /// skill system.
    /// </remarks>
    private bool HasMasterRank(Player player)
    {
        // Placeholder: Check player's skill rank
        // In full implementation: player.GetSkillRank(WastelandSurvivalSkillId) >= MasterRankRequirement
        _logger.LogDebug(
            "Checking master rank for PlayerId={PlayerId} (placeholder: always true for non-Cold trails)",
            player.Id);

        return true; // Placeholder - would check actual skill rank
    }
}
