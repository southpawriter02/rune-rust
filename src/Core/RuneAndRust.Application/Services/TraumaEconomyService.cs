// ═══════════════════════════════════════════════════════════════════════════════
// TraumaEconomyService.cs
// Unified orchestration service implementing ITraumaEconomyService. Aggregates
// state from all trauma economy subsystems and delegates processing to
// specialized handlers (UnifiedDamageHandler, UnifiedRestHandler, UnifiedTurnHandler).
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates all trauma economy operations across subsystems.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// TraumaEconomyService is the unified entry point for trauma economy operations.
/// It aggregates state from multiple subsystems (Stress, Corruption, CPS, Trauma,
/// Specialization Resources) and delegates complex processing to specialized handlers.
/// </para>
/// <para>
/// <strong>Architecture:</strong>
/// </para>
/// <code>
///              ┌──────────────────────────────────────┐
///              │      TraumaEconomyService            │
///              │   (Orchestration + State Access)     │
///              └──────────────────┬───────────────────┘
///                                 │
///          ┌──────────────────────┼──────────────────────┐
///          ▼                      ▼                      ▼
/// ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
/// │ UnifiedDamage   │  │ UnifiedRest     │  │ UnifiedTurn     │
/// │    Handler      │  │    Handler      │  │    Handler      │
/// └────────┬────────┘  └────────┬────────┘  └────────┬────────┘
///          │                    │                    │
///          ▼                    ▼                    ▼
///    ┌──────────────────────────────────────────────────────┐
///    │  Sub-Services: IStressService, ICorruptionService,   │
///    │  ICpsService, ITraumaService, IRageService, etc.     │
///    └──────────────────────────────────────────────────────┘
/// </code>
/// <para>
/// <strong>Logging:</strong>
/// All operations are logged at appropriate levels:
/// <list type="bullet">
///   <item><description>Debug: Detailed operation parameters and intermediate results</description></item>
///   <item><description>Information: Major state changes and transitions</description></item>
///   <item><description>Warning: Threshold crossings and dangerous states</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This service should be registered as scoped in DI to ensure thread-safe
/// character state access within a single request/operation.
/// </para>
/// </remarks>
/// <seealso cref="ITraumaEconomyService"/>
/// <seealso cref="TraumaEconomyState"/>
/// <seealso cref="UnifiedDamageHandler"/>
/// <seealso cref="UnifiedRestHandler"/>
/// <seealso cref="UnifiedTurnHandler"/>
public class TraumaEconomyService : ITraumaEconomyService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IStressService _stressService;
    private readonly ICorruptionService _corruptionService;
    private readonly ICpsService _cpsService;
    private readonly ITraumaService _traumaService;
    private readonly ISpecializationProvider _specProvider;
    private readonly UnifiedDamageHandler _damageHandler;
    private readonly UnifiedRestHandler _restHandler;
    private readonly UnifiedTurnHandler _turnHandler;
    private readonly ITraumaEconomyConfiguration _configuration;
    private readonly ILogger<TraumaEconomyService> _logger;

    // Optional specialization resource services
    private readonly IRageService? _rageService;
    private readonly IMomentumService? _momentumService;
    private readonly ICoherenceService? _coherenceService;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaEconomyService"/> class.
    /// </summary>
    /// <param name="stressService">Service for stress management (required).</param>
    /// <param name="corruptionService">Service for corruption management (required).</param>
    /// <param name="cpsService">Service for CPS management (required).</param>
    /// <param name="traumaService">Service for trauma management (required).</param>
    /// <param name="specProvider">Provider for specialization data (required).</param>
    /// <param name="damageHandler">Handler for unified damage processing (required).</param>
    /// <param name="restHandler">Handler for unified rest processing (required).</param>
    /// <param name="turnHandler">Handler for unified turn processing (required).</param>
    /// <param name="configuration">Trauma economy configuration (required).</param>
    /// <param name="logger">Logger instance (optional, uses NullLogger if not provided).</param>
    /// <param name="rageService">Optional service for Berserker rage.</param>
    /// <param name="momentumService">Optional service for Storm Blade momentum.</param>
    /// <param name="coherenceService">Optional service for Arcanist coherence.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public TraumaEconomyService(
        IStressService stressService,
        ICorruptionService corruptionService,
        ICpsService cpsService,
        ITraumaService traumaService,
        ISpecializationProvider specProvider,
        UnifiedDamageHandler damageHandler,
        UnifiedRestHandler restHandler,
        UnifiedTurnHandler turnHandler,
        ITraumaEconomyConfiguration configuration,
        ILogger<TraumaEconomyService>? logger = null,
        IRageService? rageService = null,
        IMomentumService? momentumService = null,
        ICoherenceService? coherenceService = null)
    {
        ArgumentNullException.ThrowIfNull(stressService);
        ArgumentNullException.ThrowIfNull(corruptionService);
        ArgumentNullException.ThrowIfNull(cpsService);
        ArgumentNullException.ThrowIfNull(traumaService);
        ArgumentNullException.ThrowIfNull(specProvider);
        ArgumentNullException.ThrowIfNull(damageHandler);
        ArgumentNullException.ThrowIfNull(restHandler);
        ArgumentNullException.ThrowIfNull(turnHandler);
        ArgumentNullException.ThrowIfNull(configuration);

        _stressService = stressService;
        _corruptionService = corruptionService;
        _cpsService = cpsService;
        _traumaService = traumaService;
        _specProvider = specProvider;
        _damageHandler = damageHandler;
        _restHandler = restHandler;
        _turnHandler = turnHandler;
        _configuration = configuration;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TraumaEconomyService>.Instance;

        _rageService = rageService;
        _momentumService = momentumService;
        _coherenceService = coherenceService;

        _logger.LogDebug("TraumaEconomyService initialized with all required dependencies");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE ACCESS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public TraumaEconomyState GetState(Guid characterId)
    {
        _logger.LogDebug("Getting trauma economy state for character {CharacterId}", characterId);

        // Get individual subsystem states
        var stressState = _stressService.GetStressState(characterId);
        var corruptionState = _corruptionService.GetCorruptionState(characterId);
        var cpsState = CpsState.Create(stressState.CurrentStress);
        var traumas = _traumaService.GetTraumasAsync(characterId).GetAwaiter().GetResult();

        // Get specialization resource if available
        object? specializationResource = null;
        string? specializationType = null;

        if (_rageService != null)
        {
            var rageState = _rageService.GetRageState(characterId);
            if (rageState != null)
            {
                specializationResource = rageState;
                specializationType = "rage";
            }
        }

        if (specializationResource == null && _momentumService != null)
        {
            var momentumState = _momentumService.GetMomentumState(characterId);
            if (momentumState != null)
            {
                specializationResource = momentumState;
                specializationType = "momentum";
            }
        }

        if (specializationResource == null && _coherenceService != null)
        {
            var coherenceState = _coherenceService.GetCoherenceState(characterId);
            if (coherenceState != null)
            {
                specializationResource = coherenceState;
                specializationType = "coherence";
            }
        }

        // Compose unified state
        var state = TraumaEconomyState.Create(
            characterId,
            stressState,
            corruptionState,
            cpsState,
            traumas,
            specializationResource,
            specializationType);

        _logger.LogDebug(
            "Trauma economy state retrieved: Stress={Stress}, Corruption={Corruption}, Traumas={TraumaCount}",
            stressState.CurrentStress,
            corruptionState.CurrentCorruption,
            traumas?.Count ?? 0);

        return state;
    }

    /// <inheritdoc/>
    public TraumaEconomySnapshot CreateSnapshot(Guid characterId)
    {
        _logger.LogDebug("Creating trauma economy snapshot for character {CharacterId}", characterId);

        var state = GetState(characterId);
        var snapshot = TraumaEconomySnapshot.Create(state);

        _logger.LogDebug(
            "Snapshot created: {Snapshot}",
            snapshot.ToString());

        return snapshot;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UNIFIED PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public DamageIntegrationResult ProcessDamage(Guid characterId, int damage, DamageContext context)
    {
        _logger.LogInformation(
            "Processing damage for character {CharacterId}: {Damage} damage, Critical={IsCritical}, Interrupt={IsInterrupt}, AllyDied={AllyDied}",
            characterId,
            damage,
            context.IsCriticalHit,
            context.IsInterrupt,
            context.IsAllyDeathEvent);

        var result = _damageHandler.ProcessDamage(characterId, damage, context);

        _logger.LogInformation(
            "Damage processed: Soak={Soak}, StressGained={StressGained}, TraumaTriggered={TraumaTriggered}",
            result.SoakApplied,
            result.StressGained,
            result.TraumaCheckTriggered);

        if (result.TraumaCheckTriggered)
        {
            _logger.LogWarning(
                "Trauma check triggered for character {CharacterId} after damage",
                characterId);
        }

        return result;
    }

    /// <inheritdoc/>
    public RestIntegrationResult ProcessRest(Guid characterId, RestType restType, PartyContext? partyContext = null)
    {
        _logger.LogInformation(
            "Processing rest for character {CharacterId}: Type={RestType}, HasPartyContext={HasParty}",
            characterId,
            restType,
            partyContext != null);

        var result = _restHandler.ProcessRest(characterId, restType, partyContext);

        _logger.LogInformation(
            "Rest processed: StressRecovered={StressRecovered}, CpsChanged={CpsChanged}",
            result.StressRecovered,
            result.CpsStageChanged);

        return result;
    }

    /// <inheritdoc/>
    public TurnIntegrationResult ProcessTurnStart(Guid characterId, bool isInCombat)
    {
        _logger.LogDebug(
            "Processing turn start for character {CharacterId}: InCombat={IsInCombat}",
            characterId,
            isInCombat);

        var result = _turnHandler.ProcessTurnStart(characterId, isInCombat);

        if (result.RageDecay != null)
        {
            _logger.LogDebug(
                "Rage decayed: {PreviousRage} -> {NewRage}",
                result.RageDecay.PreviousRage,
                result.RageDecay.NewRage);
        }

        if (result.MomentumDecay != null)
        {
            _logger.LogDebug(
                "Momentum decayed: {PreviousMomentum} -> {NewMomentum}",
                result.MomentumDecay.PreviousMomentum,
                result.MomentumDecay.NewMomentum);
        }

        if (result.ApotheosisActive)
        {
            _logger.LogInformation(
                "Apotheosis active: StressCost={StressCost}, AutoExited={AutoExited}",
                result.ApotheosisStressCost,
                result.AutoExitedApotheosis);
        }

        return result;
    }

    /// <inheritdoc/>
    public TurnIntegrationResult ProcessTurnEnd(Guid characterId, int environmentalStress = 0)
    {
        _logger.LogDebug(
            "Processing turn end for character {CharacterId}: EnvironmentalStress={EnvStress}",
            characterId,
            environmentalStress);

        var result = _turnHandler.ProcessTurnEnd(characterId, environmentalStress);

        if (result.EnvironmentalStressApplied > 0)
        {
            _logger.LogDebug(
                "Environmental stress applied: {Amount}",
                result.EnvironmentalStressApplied);
        }

        if (result.PanicCheckPerformed)
        {
            _logger.LogWarning(
                "Panic check performed for character {CharacterId}: Effect={Effect}",
                characterId,
                result.PanicEffectApplied);
        }

        if (result.TraumaCheckTriggered)
        {
            _logger.LogWarning(
                "Trauma check triggered for character {CharacterId} at turn end",
                characterId);
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetEffectiveMaxHp(Guid characterId)
    {
        var state = GetState(characterId);
        return state.EffectiveMaxHp;
    }

    /// <inheritdoc/>
    public int GetTotalDefensePenalty(Guid characterId)
    {
        var state = GetState(characterId);

        // Stress-based defense penalty
        int stressPenalty = state.StressState.DefensePenalty;

        // Corruption-based penalty (1 per 25 corruption)
        int corruptionPenalty = state.CorruptionState.CurrentCorruption / 25;

        int total = stressPenalty + corruptionPenalty;

        _logger.LogDebug(
            "Total defense penalty for {CharacterId}: {Total} (Stress={StressPenalty}, Corruption={CorruptionPenalty})",
            characterId,
            total,
            stressPenalty,
            corruptionPenalty);

        return total;
    }

    /// <inheritdoc/>
    public int GetTotalSkillPenalty(Guid characterId)
    {
        var state = GetState(characterId);

        // Stress-based skill penalty (disadvantage = 2)
        int stressPenalty = state.StressState.HasSkillDisadvantage ? 2 : 0;

        // CPS logic disadvantage - check if method exists, otherwise calculate directly
        int cpsPenalty = CalculateCpsLogicDisadvantage(state.CpsState);

        int total = stressPenalty + cpsPenalty;

        _logger.LogDebug(
            "Total skill penalty for {CharacterId}: {Total} (Stress={StressPenalty}, CPS={CpsPenalty})",
            characterId,
            total,
            stressPenalty,
            cpsPenalty);

        return total;
    }

    /// <inheritdoc/>
    public WarningLevel GetWarningLevel(Guid characterId)
    {
        var state = GetState(characterId);
        return state.GetWarningLevel();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetActiveWarnings(Guid characterId)
    {
        var state = GetState(characterId);
        var warnings = new List<string>();

        // Check stress warning
        if (state.StressState.CurrentStress >= _configuration.CriticalWarningThreshold)
        {
            warnings.Add(_configuration.StressHighMessage);
        }

        // Check corruption warning
        if (state.CorruptionState.CurrentCorruption >= _configuration.CriticalWarningThreshold)
        {
            warnings.Add(_configuration.CorruptionRisingMessage);
        }

        // Check specialization-specific warnings
        if (_rageService != null)
        {
            var rageState = _rageService.GetRageState(characterId);
            if (rageState != null && rageState.CurrentRage >= _configuration.CriticalWarningThreshold)
            {
                warnings.Add(_configuration.RageOverflowWarning);
            }
        }

        if (_momentumService != null)
        {
            var momentumState = _momentumService.GetMomentumState(characterId);
            if (momentumState != null && momentumState.CurrentMomentum >= _configuration.CriticalWarningThreshold)
            {
                warnings.Add(_configuration.MomentumCriticalMessage);
            }
        }

        if (_coherenceService != null)
        {
            var coherenceState = _coherenceService.GetCoherenceState(characterId);
            if (coherenceState != null && coherenceState.CurrentCoherence >= _configuration.CriticalWarningThreshold)
            {
                warnings.Add(_configuration.CoherenceCriticalMessage);
            }
        }

        _logger.LogDebug(
            "Active warnings for {CharacterId}: {Count} warnings",
            characterId,
            warnings.Count);

        return warnings.AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the logic disadvantage penalty from CPS stage.
    /// </summary>
    /// <param name="cpsState">The CPS state to evaluate.</param>
    /// <returns>Logic disadvantage penalty (0-3).</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>None/WeightOfKnowing: 0 disadvantage</description></item>
    ///   <item><description>GlimmerMadness: 1 disadvantage</description></item>
    ///   <item><description>RuinMadness: 2 disadvantage</description></item>
    ///   <item><description>HollowShell: 3 disadvantage</description></item>
    /// </list>
    /// </remarks>
    private static int CalculateCpsLogicDisadvantage(CpsState cpsState)
    {
        return cpsState.Stage switch
        {
            CpsStage.None => 0,
            CpsStage.WeightOfKnowing => 0,
            CpsStage.GlimmerMadness => 1,
            CpsStage.RuinMadness => 2,
            CpsStage.HollowShell => 3,
            _ => 0
        };
    }
}
