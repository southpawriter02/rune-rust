// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedTurnHandler.cs
// Orchestrates per-turn effects across all trauma economy systems.
// Handles resource decay, Apotheosis costs, panic table checks, CPS effects,
// environmental stress, and trauma triggers.
// Version: 0.18.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates per-turn effects across all trauma economy systems.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The UnifiedTurnHandler coordinates turn-based processing across multiple
/// trauma economy subsystems: Stress, CPS, Rage, Momentum, and Coherence.
/// It ensures consistent processing order and aggregates all effects into
/// a single result object for UI and game logic consumption.
/// </para>
/// <para>
/// <strong>Turn Start Processing:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Resource decay (Rage 10/turn, Momentum 15/turn) when out of combat</description></item>
///   <item><description>Apotheosis stress cost (10/turn) for Arcanists</description></item>
///   <item><description>Apotheosis auto-exit when stress reaches 100</description></item>
/// </list>
/// <para>
/// <strong>Turn End Processing:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Panic table check (RuinMadness stage)</description></item>
///   <item><description>CPS stage effect application</description></item>
///   <item><description>Environmental stress application</description></item>
///   <item><description>Trauma check triggers</description></item>
/// </list>
/// <para>
/// <strong>Dependency Injection:</strong>
/// Required services must be non-null. Optional services (IRageService,
/// IMomentumService, ICoherenceService) determine specialization handling.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // At start of character's turn
/// var startResult = turnHandler.ProcessTurnStart(characterId, isInCombat: false);
/// if (startResult.AutoExitedApotheosis)
///     ShowApotheosisExitMessage(startResult.ApotheosisExitReason);
///
/// // At end of character's turn
/// var endResult = turnHandler.ProcessTurnEnd(characterId, environmentalStress: 3);
/// if (endResult.PanicEffectApplied.HasValue)
///     ApplyPanicBehavior(endResult.PanicEffectApplied.Value);
/// </code>
/// </example>
/// <seealso cref="TurnIntegrationResult"/>
/// <seealso cref="IStressService"/>
/// <seealso cref="ICpsService"/>
public class UnifiedTurnHandler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Stress cost per turn for maintaining Apotheosis state.
    /// </summary>
    private const int ApotheosisStressCostPerTurn = 10;

    /// <summary>
    /// Stress threshold that triggers Apotheosis auto-exit.
    /// </summary>
    private const int ApotheosisAutoExitStressThreshold = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IStressService _stressService;
    private readonly ITraumaService _traumaService;
    private readonly IDiceService _diceService;
    private readonly ICpsService _cpsService;
    private readonly IRageService? _rageService;
    private readonly IMomentumService? _momentumService;
    private readonly ICoherenceService? _coherenceService;
    private readonly ILogger<UnifiedTurnHandler> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedTurnHandler"/> class.
    /// </summary>
    /// <param name="stressService">Service for stress management (required).</param>
    /// <param name="traumaService">Service for trauma checks (required).</param>
    /// <param name="diceService">Service for dice rolls (required).</param>
    /// <param name="cpsService">Service for CPS and panic effects (required).</param>
    /// <param name="logger">Logger for structured logging (required).</param>
    /// <param name="rageService">Optional service for Berserker rage decay.</param>
    /// <param name="momentumService">Optional service for Storm Blade momentum decay.</param>
    /// <param name="coherenceService">Optional service for Arcanist Apotheosis.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required service is null.
    /// </exception>
    public UnifiedTurnHandler(
        IStressService stressService,
        ITraumaService traumaService,
        IDiceService diceService,
        ICpsService cpsService,
        ILogger<UnifiedTurnHandler> logger,
        IRageService? rageService = null,
        IMomentumService? momentumService = null,
        ICoherenceService? coherenceService = null)
    {
        ArgumentNullException.ThrowIfNull(stressService);
        ArgumentNullException.ThrowIfNull(traumaService);
        ArgumentNullException.ThrowIfNull(diceService);
        ArgumentNullException.ThrowIfNull(cpsService);
        ArgumentNullException.ThrowIfNull(logger);

        _stressService = stressService;
        _traumaService = traumaService;
        _diceService = diceService;
        _cpsService = cpsService;
        _logger = logger;
        _rageService = rageService;
        _momentumService = momentumService;
        _coherenceService = coherenceService;

        _logger.LogDebug(
            "UnifiedTurnHandler initialized. RageService={RageAvailable}, " +
            "MomentumService={MomentumAvailable}, CoherenceService={CoherenceAvailable}",
            _rageService is not null,
            _momentumService is not null,
            _coherenceService is not null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes start-of-turn effects for a character.
    /// </summary>
    /// <param name="characterId">The character to process.</param>
    /// <param name="isInCombat">Whether the character is currently in combat.</param>
    /// <returns>A <see cref="TurnIntegrationResult"/> capturing all effects applied.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Processing Order:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Apply resource decay (Rage, Momentum) if out of combat</description></item>
    ///   <item><description>Apply Apotheosis stress cost (10/turn) if in Apotheosis</description></item>
    ///   <item><description>Check Apotheosis auto-exit (stress >= 100)</description></item>
    /// </list>
    /// </remarks>
    public TurnIntegrationResult ProcessTurnStart(Guid characterId, bool isInCombat)
    {
        _logger.LogDebug(
            "Processing turn start for {CharacterId}. InCombat={InCombat}",
            characterId,
            isInCombat);

        // Step 1: Apply resource decay (out of combat only)
        RageDecayResult? rageDecay = null;
        MomentumDecayResult? momentumDecay = null;

        if (!isInCombat)
        {
            rageDecay = ApplyRageDecay(characterId);
            momentumDecay = ApplyMomentumDecay(characterId);
        }

        // Step 2: Process Apotheosis
        var (apotheosisActive, apotheosisStressCost, autoExited, exitReason) = ProcessApotheosis(characterId);

        var result = TurnIntegrationResult.CreateTurnStart(
            characterId,
            rageDecay: rageDecay,
            momentumDecay: momentumDecay,
            apotheosisActive: apotheosisActive,
            apotheosisStressCost: apotheosisStressCost,
            autoExitedApotheosis: autoExited,
            apotheosisExitReason: exitReason);

        _logger.LogInformation(
            "Turn start completed for {CharacterId}. {Result}",
            characterId,
            result);

        return result;
    }

    /// <summary>
    /// Processes end-of-turn effects for a character.
    /// </summary>
    /// <param name="characterId">The character to process.</param>
    /// <param name="environmentalStress">Environmental stress to apply (0-5).</param>
    /// <returns>A <see cref="TurnIntegrationResult"/> capturing all effects applied.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Processing Order:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Check stress thresholds for Panic Table (RuinMadness)</description></item>
    ///   <item><description>Apply CPS stage effects</description></item>
    ///   <item><description>Apply environmental stress</description></item>
    ///   <item><description>Check for trauma triggers</description></item>
    /// </list>
    /// </remarks>
    public TurnIntegrationResult ProcessTurnEnd(Guid characterId, int environmentalStress = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(environmentalStress);

        _logger.LogDebug(
            "Processing turn end for {CharacterId}. EnvironmentalStress={EnvStress}",
            characterId,
            environmentalStress);

        // Step 1: Get current CPS state and check for panic
        var cpsStage = _cpsService.GetCurrentStage(characterId);
        var (panicCheckPerformed, panicEffect) = ProcessPanicCheck(characterId, cpsStage);

        // Step 2: Get CPS effects
        var cpsEffects = GetCpsEffects(cpsStage);

        // Step 3: Apply environmental stress
        var actualEnvStress = Math.Min(environmentalStress, 5); // Cap at 5
        if (actualEnvStress > 0)
        {
            _stressService.ApplyStress(
                characterId,
                actualEnvStress,
                StressSource.Environmental,
                resistDc: 0); // Environmental stress is not resistable in turn processing

            _logger.LogDebug(
                "Applied {EnvStress} environmental stress to {CharacterId}",
                actualEnvStress,
                characterId);
        }

        // Step 4: Check for trauma trigger
        var traumaCheckTriggered = _stressService.RequiresTraumaCheck(characterId);

        var result = TurnIntegrationResult.CreateTurnEnd(
            characterId,
            panicCheckPerformed: panicCheckPerformed,
            panicEffectApplied: panicEffect,
            cpsStage: cpsStage,
            cpsEffectsApplied: cpsEffects,
            environmentalStressApplied: actualEnvStress,
            traumaCheckTriggered: traumaCheckTriggered);

        _logger.LogInformation(
            "Turn end completed for {CharacterId}. {Result}",
            characterId,
            result);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies rage decay for Berserker characters.
    /// </summary>
    private RageDecayResult? ApplyRageDecay(Guid characterId)
    {
        if (_rageService is null)
            return null;

        var rageState = _rageService.GetRageState(characterId);
        if (rageState is null || rageState.CurrentRage == 0)
            return null;

        var result = _rageService.ApplyDecay(characterId);

        _logger.LogDebug(
            "Applied rage decay to {CharacterId}. {PreviousRage}->{NewRage}",
            characterId,
            result.PreviousRage,
            result.NewRage);

        return result;
    }

    /// <summary>
    /// Applies momentum decay for Storm Blade characters.
    /// </summary>
    private MomentumDecayResult? ApplyMomentumDecay(Guid characterId)
    {
        if (_momentumService is null)
            return null;

        var momentumState = _momentumService.GetMomentumState(characterId);
        if (momentumState is null || momentumState.CurrentMomentum == 0)
            return null;

        var result = _momentumService.ApplyDecay(characterId, "No combat action");

        _logger.LogDebug(
            "Applied momentum decay to {CharacterId}. {PreviousMomentum}->{NewMomentum}",
            characterId,
            result.PreviousMomentum,
            result.NewMomentum);

        return result;
    }

    /// <summary>
    /// Processes Apotheosis state and stress costs for Arcanist characters.
    /// </summary>
    /// <returns>
    /// Tuple of (apotheosisActive, stressCost, autoExited, exitReason).
    /// </returns>
    private (bool Active, int StressCost, bool AutoExited, string? ExitReason) ProcessApotheosis(Guid characterId)
    {
        if (_coherenceService is null)
            return (false, 0, false, null);

        var coherenceState = _coherenceService.GetCoherenceState(characterId);
        if (coherenceState is null || !coherenceState.InApotheosis)
            return (false, 0, false, null);

        // Apply Apotheosis stress cost
        var stressResult = _stressService.ApplyStress(
            characterId,
            ApotheosisStressCostPerTurn,
            StressSource.ApotheosisStrain,
            resistDc: 0); // Apotheosis strain is not resistable

        _logger.LogDebug(
            "Applied Apotheosis stress cost ({Cost}) to {CharacterId}. NewStress={NewStress}",
            ApotheosisStressCostPerTurn,
            characterId,
            stressResult.NewStress);

        // Check for auto-exit
        if (stressResult.NewStress >= ApotheosisAutoExitStressThreshold)
        {
            _logger.LogWarning(
                "Apotheosis auto-exit triggered for {CharacterId}. Stress={Stress}",
                characterId,
                stressResult.NewStress);

            // Note: The actual exit is handled by the CoherenceService when it detects
            // the stress threshold via UpdateApotheosis or similar mechanisms.
            // We just track and report that auto-exit conditions were met.
            return (true, ApotheosisStressCostPerTurn, true, "Stress reached critical level (100)");
        }

        return (true, ApotheosisStressCostPerTurn, false, null);
    }

    /// <summary>
    /// Processes panic table check for characters in RuinMadness stage.
    /// </summary>
    private (bool Performed, PanicEffect? Effect) ProcessPanicCheck(Guid characterId, CpsStage stage)
    {
        // Panic table only applies at RuinMadness (60-79 stress)
        if (stage != CpsStage.RuinMadness)
            return (false, null);

        _logger.LogDebug(
            "Performing panic check for {CharacterId} in RuinMadness stage",
            characterId);

        var panicResult = _cpsService.RollPanicTable(characterId);

        if (panicResult.Effect == PanicEffect.None)
        {
            _logger.LogDebug(
                "Panic check for {CharacterId}: Lucky break (rolled 10)",
                characterId);
            return (true, null); // Check performed, but no effect
        }

        _logger.LogInformation(
            "Panic effect triggered for {CharacterId}: {PanicEffect}",
            characterId,
            panicResult.Effect);

        // Apply the panic effect
        _cpsService.ApplyPanicEffect(characterId, panicResult);

        return (true, panicResult.Effect);
    }

    /// <summary>
    /// Gets the CPS effects applicable to a given stage.
    /// </summary>
    private static IReadOnlyList<string> GetCpsEffects(CpsStage stage)
    {
        return stage switch
        {
            CpsStage.None => Array.Empty<string>(),
            CpsStage.WeightOfKnowing => new[] { "Subtle peripheral distortions", "Reality feels 'off'" },
            CpsStage.GlimmerMadness => new[] { "Text glitching active", "Moderate visual distortion" },
            CpsStage.RuinMadness => new[] { "Heavy distortion", "Maximum leetspeak", "UI instability" },
            CpsStage.HollowShell => new[] { "Screen blackout", "Terminal state" },
            _ => Array.Empty<string>()
        };
    }
}
