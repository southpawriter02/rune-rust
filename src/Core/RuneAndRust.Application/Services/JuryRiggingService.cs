// ------------------------------------------------------------------------------
// <copyright file="JuryRiggingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling the five-step jury-rigging procedure.
// Manages Observe, Probe, Pattern, Method Selection, Experiment, and Iterate stages
// with proper DC modifiers, familiarity tracking, and complication handling.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling the five-step jury-rigging procedure.
/// </summary>
/// <remarks>
/// <para>
/// The jury-rigging procedure follows a five-step trial-and-error flow:
/// <list type="number">
///   <item><description>Observe (optional): Study the mechanism visually (WITS DC 10)</description></item>
///   <item><description>Probe (automatic): Try obvious buttons and levers</description></item>
///   <item><description>Pattern (optional): Recognize mechanism type (WITS DC 12)</description></item>
///   <item><description>Method Selection: Choose a bypass approach</description></item>
///   <item><description>Experiment: Attempt the bypass (System Bypass check)</description></item>
///   <item><description>Iterate: Learn from failure (DC -1 next attempt)</description></item>
/// </list>
/// </para>
/// <para>
/// This service manages:
/// <list type="bullet">
///   <item><description>Observation and probing for mechanism hints</description></item>
///   <item><description>Pattern recognition for familiarity bonuses</description></item>
///   <item><description>Method validation based on context (familiarity, glitched state)</description></item>
///   <item><description>Experiment rolls with proper DC and dice modifiers</description></item>
///   <item><description>Complication table processing for failures</description></item>
///   <item><description>Iteration learning (DC reduction on retry)</description></item>
///   <item><description>Critical success salvage system</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class JuryRiggingService : IJuryRiggingService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// DC for observation (WITS check).
    /// </summary>
    private const int ObservationDc = 10;

    /// <summary>
    /// DC for pattern recognition (WITS check).
    /// </summary>
    private const int PatternRecognitionDc = 12;

    /// <summary>
    /// Net successes threshold for critical success (yields salvage).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    /// <summary>
    /// Bonus dice granted for mechanism familiarity.
    /// </summary>
    private const int FamiliarityBonusDice = 2;

    /// <summary>
    /// Minimum DC for any jury-rigging attempt.
    /// </summary>
    private const int MinimumDc = 4;

    /// <summary>
    /// Damage dealt by Sparks Fly complication (1d6).
    /// </summary>
    private const int SparksFlyDamageDice = 1;
    private const int SparksFlyDamageDieSize = 6;

    // -------------------------------------------------------------------------
    // Default Salvage Components by Mechanism Category
    // -------------------------------------------------------------------------

    /// <summary>
    /// Default salvageable components by mechanism type category.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultSalvageComponents =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "terminal", new[] { "circuit-board", "display-unit", "processing-chip" } },
            { "door-lock", new[] { "lock-mechanism", "servo-motor", "keycard-reader" } },
            { "security-panel", new[] { "sensor-array", "alarm-module", "blighted-power-cell" } },
            { "elevator", new[] { "control-board", "cable-spool", "safety-relay" } },
            { "power-junction", new[] { "capacitor", "transformer-coil", "power-regulator" } },
            { "vending-machine", new[] { "coin-mechanism", "dispensing-motor", "selection-panel" } },
            { "communication", new[] { "antenna-array", "signal-processor", "transmitter-module" } },
            { "climate-control", new[] { "thermostat", "fan-motor", "filter-housing" } },
            { "jotun-device", new[] { "jotun-mechanism-fragment", "ancient-power-core", "rune-circuitry" } }
        };

    // -------------------------------------------------------------------------
    // Mechanism Hints by Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hints revealed during observation by mechanism type.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ObservationHints =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "terminal", new[] { "The terminal has a standard Old World interface", "A maintenance port is visible on the side", "The screen flickers occasionally" } },
            { "door-lock", new[] { "This is an electromagnetic lock mechanism", "The keycard reader looks corroded", "Power cables run behind the panel" } },
            { "security-panel", new[] { "Multiple sensor types are visible", "The panel has redundant security circuits", "An override switch may be accessible" } },
            { "elevator", new[] { "The control system uses standard protocols", "Emergency brake is mechanical", "Call buttons are pressure-sensitive" } },
            { "power-junction", new[] { "High voltage warning signs are visible", "Multiple feed lines converge here", "A manual cutoff switch exists" } }
        };

    // -------------------------------------------------------------------------
    // Probe Reactions by Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reactions observed during probing by mechanism type.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ProbeReactions =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "terminal", new[] { "The screen illuminates briefly", "A cursor blinks on the display", "Error codes scroll past" } },
            { "door-lock", new[] { "A red light pulses", "The mechanism clicks internally", "A faint hum emanates from within" } },
            { "security-panel", new[] { "Multiple lights flash in sequence", "A soft alarm chirps once", "The display shows 'ACCESS DENIED'" } },
            { "elevator", new[] { "Floor indicators light up", "A motor whirs somewhere above", "Emergency lights flicker" } },
            { "power-junction", new[] { "Sparks dance between contacts", "A loud hum fills the room", "Indicator lights glow steady" } }
        };

    // -------------------------------------------------------------------------
    // Glitched Reactions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Additional reactions observed when mechanism is glitched.
    /// </summary>
    private static readonly IReadOnlyList<string> GlitchedReactions = new[]
    {
        "The display flickers wildly between random characters",
        "Lights flash in erratic, unpredictable patterns",
        "Strange sounds emit at irregular intervals",
        "The mechanism stutters and jerks unexpectedly",
        "Error messages appear and disappear rapidly"
    };

    // -------------------------------------------------------------------------
    // Complication Narratives
    // -------------------------------------------------------------------------

    /// <summary>
    /// Narrative text for each complication effect.
    /// </summary>
    private static readonly IReadOnlyDictionary<ComplicationEffect, string> ComplicationNarratives =
        new Dictionary<ComplicationEffect, string>
        {
            { ComplicationEffect.PermanentLock, "A deep clunk echoes from within as internal bolts slide into place. The mechanism has locked permanently—no amount of tinkering will open it now." },
            { ComplicationEffect.AlarmTriggered, "Your fumbling activates a security protocol! A klaxon begins wailing, and warning lights flash throughout the area." },
            { ComplicationEffect.SparksFly, "A shower of sparks erupts from the mechanism, burning your hands and face!" },
            { ComplicationEffect.Nothing, "The mechanism buzzes indifferently. Your attempt achieves nothing, but nothing goes wrong either." },
            { ComplicationEffect.PartialSuccess, "The mechanism partially responds! While not fully bypassed, one of its secondary functions activates." },
            { ComplicationEffect.GlitchInFavor, "The machine's own instability works in your favor! A fortuitous glitch causes it to bypass itself." }
        };

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly IDiceService _diceService;
    private readonly ILogger<JuryRiggingService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="JuryRiggingService"/> class.
    /// </summary>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public JuryRiggingService(
        IDiceService diceService,
        ILogger<JuryRiggingService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("JuryRiggingService initialized");
    }

    // -------------------------------------------------------------------------
    // Session Initialization
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public JuryRigState InitiateJuryRig(
        string characterId,
        string mechanismType,
        string mechanismName,
        int baseDc,
        bool isGlitched,
        IEnumerable<string>? knownMechanismTypes = null)
    {
        _logger.LogInformation(
            "Initiating jury-rigging session: CharacterId={CharacterId}, " +
            "MechanismType={MechanismType}, MechanismName={MechanismName}, " +
            "BaseDC={BaseDc}, IsGlitched={IsGlitched}",
            characterId,
            mechanismType,
            mechanismName,
            baseDc,
            isGlitched);

        var state = JuryRigState.Create(
            characterId,
            mechanismType,
            mechanismName,
            baseDc,
            isGlitched,
            knownMechanismTypes);

        _logger.LogDebug(
            "Jury-rigging session created: SessionId={SessionId}, IsFamiliar={IsFamiliar}",
            state.JuryRigId,
            state.IsFamiliarMechanism);

        return state;
    }

    // -------------------------------------------------------------------------
    // Observe Step
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ObservationResult PerformObservation(JuryRigState state, int witsScore)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Observe, "perform observation");

        _logger.LogInformation(
            "Performing observation: SessionId={SessionId}, WitsScore={WitsScore}, DC={DC}",
            state.JuryRigId,
            witsScore,
            ObservationDc);

        // Roll WITS check against DC 10
        var netSuccesses = RollNetSuccesses(witsScore, ObservationDc);

        _logger.LogDebug(
            "Observation roll: NetSuccesses={NetSuccesses}, DC={DC}, Success={Success}",
            netSuccesses,
            ObservationDc,
            netSuccesses > 0);

        if (netSuccesses > 0)
        {
            // Success - reveal hints based on mechanism type
            var hints = GetMechanismHints(state.MechanismType, netSuccesses);
            state.CompleteObservation(true, hints);

            _logger.LogInformation(
                "Observation SUCCESS: SessionId={SessionId}, HintsRevealed={HintCount}",
                state.JuryRigId,
                hints.Count);

            return ObservationResult.Succeeded(
                netSuccesses,
                ObservationDc,
                state.MechanismType,
                hints);
        }
        else
        {
            // Failure - no hints
            state.CompleteObservation(false);

            _logger.LogInformation(
                "Observation FAILED: SessionId={SessionId}, NetSuccesses={NetSuccesses}",
                state.JuryRigId,
                netSuccesses);

            return ObservationResult.Failure(netSuccesses, ObservationDc);
        }
    }

    /// <inheritdoc />
    public ObservationResult SkipObservation(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Observe, "skip observation");

        _logger.LogInformation(
            "Skipping observation: SessionId={SessionId}",
            state.JuryRigId);

        state.SkipObservation();

        return ObservationResult.Skipped();
    }

    // -------------------------------------------------------------------------
    // Probe Step
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ProbeResult PerformProbe(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Probe, "perform probe");

        _logger.LogInformation(
            "Performing probe: SessionId={SessionId}, MechanismType={MechanismType}",
            state.JuryRigId,
            state.MechanismType);

        // Get probe reactions based on mechanism type and state
        var reactions = GetProbeReactions(state.MechanismType, state.IsGlitched, true);

        // Complete the probe step
        state.CompleteProbe(reactions);

        _logger.LogInformation(
            "Probe complete: SessionId={SessionId}, ReactionsObserved={ReactionCount}, IsGlitched={IsGlitched}",
            state.JuryRigId,
            reactions.Count,
            state.IsGlitched);

        if (state.IsGlitched)
        {
            return ProbeResult.Glitched(reactions);
        }

        return ProbeResult.Responsive(
            reactions,
            isGlitched: false,
            isLocked: false,
            isPowered: true);
    }

    // -------------------------------------------------------------------------
    // Pattern Recognition Step
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public PatternResult AttemptPatternRecognition(JuryRigState state, int witsScore)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Pattern, "attempt pattern recognition");

        _logger.LogInformation(
            "Attempting pattern recognition: SessionId={SessionId}, WitsScore={WitsScore}, " +
            "DC={DC}, IsFamiliar={IsFamiliar}",
            state.JuryRigId,
            witsScore,
            PatternRecognitionDc,
            state.IsFamiliarMechanism);

        // Roll WITS check against DC 12
        var netSuccesses = RollNetSuccesses(witsScore, PatternRecognitionDc);

        _logger.LogDebug(
            "Pattern recognition roll: NetSuccesses={NetSuccesses}, DC={DC}, Success={Success}",
            netSuccesses,
            PatternRecognitionDc,
            netSuccesses > 0);

        if (netSuccesses > 0)
        {
            state.CompletePatternRecognition(true);

            if (state.IsFamiliarMechanism)
            {
                _logger.LogInformation(
                    "Pattern recognition SUCCESS with familiarity: SessionId={SessionId}, " +
                    "MechanismType={MechanismType}, BonusDice={BonusDice}",
                    state.JuryRigId,
                    state.MechanismType,
                    FamiliarityBonusDice);

                return PatternResult.SuccessWithFamiliarity(
                    netSuccesses,
                    state.MechanismType);
            }
            else
            {
                _logger.LogInformation(
                    "Pattern recognition SUCCESS (new type): SessionId={SessionId}, " +
                    "MechanismType={MechanismType}",
                    state.JuryRigId,
                    state.MechanismType);

                return PatternResult.SuccessNewType(
                    netSuccesses,
                    state.MechanismType);
            }
        }
        else
        {
            state.CompletePatternRecognition(false);

            _logger.LogInformation(
                "Pattern recognition FAILED: SessionId={SessionId}, NetSuccesses={NetSuccesses}",
                state.JuryRigId,
                netSuccesses);

            return PatternResult.Failure(netSuccesses);
        }
    }

    /// <inheritdoc />
    public PatternResult SkipPatternRecognition(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Pattern, "skip pattern recognition");

        _logger.LogInformation(
            "Skipping pattern recognition: SessionId={SessionId}",
            state.JuryRigId);

        state.SkipPatternRecognition();

        return PatternResult.Skipped();
    }

    // -------------------------------------------------------------------------
    // Method Selection
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<MethodOption> GetAvailableMethods(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _logger.LogDebug(
            "Getting available methods: SessionId={SessionId}, IsFamiliar={IsFamiliar}, " +
            "IsGlitched={IsGlitched}",
            state.JuryRigId,
            state.IsFamiliarMechanism,
            state.IsGlitched);

        var methods = MethodOption.GetAllForContext(
            state.IsFamiliarMechanism,
            state.IsGlitched);

        var availableCount = methods.Count(m => m.IsAvailable);

        _logger.LogDebug(
            "Available methods: SessionId={SessionId}, TotalMethods={Total}, Available={Available}",
            state.JuryRigId,
            methods.Count,
            availableCount);

        return methods;
    }

    /// <inheritdoc />
    public void SelectMethod(JuryRigState state, BypassMethod method)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.MethodSelection, "select method");

        _logger.LogInformation(
            "Selecting bypass method: SessionId={SessionId}, Method={Method}, " +
            "DCModifier={DCModifier}",
            state.JuryRigId,
            method,
            method.GetDcModifier());

        state.SelectMethod(method);

        _logger.LogDebug(
            "Method selected: SessionId={SessionId}, EffectiveDC={EffectiveDC}",
            state.JuryRigId,
            state.GetModifiedDc());
    }

    // -------------------------------------------------------------------------
    // Experiment
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public JuryRigResult PerformExperiment(
        JuryRigState state,
        JuryRigContext context,
        int systemBypassScore)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Experiment, "perform experiment");

        var effectiveDc = state.GetModifiedDc();
        var totalDiceModifier = context.TotalDiceModifier;

        _logger.LogInformation(
            "Performing experiment: SessionId={SessionId}, Method={Method}, " +
            "SystemBypassScore={Score}, EffectiveDC={DC}, DiceModifier={Modifier}",
            state.JuryRigId,
            context.MethodUsed,
            systemBypassScore,
            effectiveDc,
            totalDiceModifier);

        // Roll System Bypass check with modifiers
        var modifiedScore = Math.Max(1, systemBypassScore + totalDiceModifier);
        var dicePool = DicePool.D10(modifiedScore);
        var rollResult = _diceService.Roll(dicePool);

        var netSuccesses = rollResult.NetSuccesses;
        var isFumble = rollResult.IsFumble;

        _logger.LogDebug(
            "Experiment roll: Successes={Successes}, Botches={Botches}, " +
            "NetSuccesses={NetSuccesses}, IsFumble={IsFumble}",
            rollResult.TotalSuccesses,
            rollResult.TotalBotches,
            netSuccesses,
            isFumble);

        JuryRigResult result;

        // Check for fumble first
        if (isFumble)
        {
            result = HandleFumble(state, context, netSuccesses, effectiveDc);
        }
        // Check for success
        else if (netSuccesses > 0)
        {
            var isCritical = netSuccesses >= CriticalSuccessThreshold;
            result = HandleSuccess(state, context, netSuccesses, effectiveDc, isCritical);
        }
        // Handle failure - roll on complication table
        else
        {
            result = HandleFailure(state, context, netSuccesses, effectiveDc);
        }

        // Record the attempt
        state.RecordAttempt(result);

        return result;
    }

    // -------------------------------------------------------------------------
    // Complication Processing
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ComplicationEffect ProcessComplication(int roll)
    {
        if (roll < 1 || roll > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roll),
                roll,
                "Complication roll must be between 1 and 10.");
        }

        var effect = roll switch
        {
            1 => ComplicationEffect.PermanentLock,
            2 or 3 => ComplicationEffect.AlarmTriggered,
            4 or 5 => ComplicationEffect.SparksFly,
            6 or 7 => ComplicationEffect.Nothing,
            8 or 9 => ComplicationEffect.PartialSuccess,
            10 => ComplicationEffect.GlitchInFavor,
            _ => ComplicationEffect.Nothing
        };

        _logger.LogDebug(
            "Complication processed: Roll={Roll}, Effect={Effect}",
            roll,
            effect);

        return effect;
    }

    // -------------------------------------------------------------------------
    // Iteration
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public void ApplyIteration(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateStep(state, JuryRigStep.Iterate, "apply iteration");

        var previousDc = state.GetModifiedDc();

        state.ApplyIteration();

        var newDc = state.GetModifiedDc();

        _logger.LogInformation(
            "Iteration applied: SessionId={SessionId}, IterationCount={Count}, " +
            "PreviousDC={PreviousDC}, NewDC={NewDC}",
            state.JuryRigId,
            state.IterationCount,
            previousDc,
            newDc);
    }

    // -------------------------------------------------------------------------
    // Session Management
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public void AbandonSession(JuryRigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.IsTerminal)
        {
            throw new InvalidOperationException(
                $"Cannot abandon jury-rigging: session is already in terminal state {state.Status}.");
        }

        _logger.LogInformation(
            "Abandoning jury-rigging session: SessionId={SessionId}, " +
            "AttemptsMade={Attempts}, TotalDamage={Damage}",
            state.JuryRigId,
            state.PreviousAttempts.Count,
            state.TotalDamageTaken);

        state.Abandon();
    }

    // -------------------------------------------------------------------------
    // Information Queries
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<string> GetSalvageableComponents(string mechanismType)
    {
        if (string.IsNullOrEmpty(mechanismType))
        {
            return Array.Empty<string>();
        }

        // Check for exact match first
        if (DefaultSalvageComponents.TryGetValue(mechanismType, out var components))
        {
            return components;
        }

        // Check for category match (e.g., "door-lock-1" matches "door-lock")
        foreach (var kvp in DefaultSalvageComponents)
        {
            if (mechanismType.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Default fallback components
        return new[] { "unknown-component", "salvaged-part" };
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetMechanismHints(string mechanismType, int netSuccesses)
    {
        if (string.IsNullOrEmpty(mechanismType) || netSuccesses <= 0)
        {
            return Array.Empty<string>();
        }

        // Get hints for mechanism type
        if (ObservationHints.TryGetValue(mechanismType, out var allHints))
        {
            // More successes = more hints revealed
            var hintCount = Math.Min(netSuccesses, allHints.Count);
            return allHints.Take(hintCount).ToList();
        }

        // Check for category match
        foreach (var kvp in ObservationHints)
        {
            if (mechanismType.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                var hintCount = Math.Min(netSuccesses, kvp.Value.Count);
                return kvp.Value.Take(hintCount).ToList();
            }
        }

        return new[] { "The mechanism appears to be Old World technology" };
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetProbeReactions(string mechanismType, bool isGlitched, bool isPowered)
    {
        if (string.IsNullOrEmpty(mechanismType))
        {
            return Array.Empty<string>();
        }

        if (!isPowered)
        {
            return new[] { "The mechanism is dark and silent", "No response to any input" };
        }

        var reactions = new List<string>();

        // Get base reactions for mechanism type
        if (ProbeReactions.TryGetValue(mechanismType, out var baseReactions))
        {
            reactions.AddRange(baseReactions.Take(2));
        }
        else
        {
            // Check for category match
            foreach (var kvp in ProbeReactions)
            {
                if (mechanismType.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    reactions.AddRange(kvp.Value.Take(2));
                    break;
                }
            }
        }

        // Add glitched reactions if applicable
        if (isGlitched)
        {
            var random = new Random();
            var glitchIndex = random.Next(GlitchedReactions.Count);
            reactions.Add(GlitchedReactions[glitchIndex]);
        }

        if (reactions.Count == 0)
        {
            reactions.Add("The mechanism hums softly");
        }

        return reactions;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Outcome Handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles a fumble outcome (mechanism destroyed).
    /// </summary>
    private JuryRigResult HandleFumble(
        JuryRigState state,
        JuryRigContext context,
        int netSuccesses,
        int effectiveDc)
    {
        _logger.LogWarning(
            "FUMBLE! Mechanism destroyed: SessionId={SessionId}, Method={Method}",
            state.JuryRigId,
            context.MethodUsed);

        return JuryRigResult.Fumble(
            netSuccesses,
            effectiveDc,
            context.MethodUsed);
    }

    /// <summary>
    /// Handles a success outcome (including critical success with salvage).
    /// </summary>
    private JuryRigResult HandleSuccess(
        JuryRigState state,
        JuryRigContext context,
        int netSuccesses,
        int effectiveDc,
        bool isCritical)
    {
        // Check if Brute Disassembly (always gets salvage)
        if (context.MethodUsed == BypassMethod.BruteDisassembly)
        {
            var salvage = GetSalvageableComponents(state.MechanismType);

            _logger.LogInformation(
                "Brute Disassembly SUCCESS: SessionId={SessionId}, " +
                "NetSuccesses={NetSuccesses}, SalvageCount={SalvageCount}",
                state.JuryRigId,
                netSuccesses,
                salvage.Count);

            return JuryRigResult.BruteDisassemblySuccess(
                netSuccesses,
                effectiveDc,
                salvage);
        }

        if (isCritical)
        {
            var salvage = GetSalvageableComponents(state.MechanismType);

            _logger.LogInformation(
                "CRITICAL SUCCESS: SessionId={SessionId}, Method={Method}, " +
                "NetSuccesses={NetSuccesses}, SalvageCount={SalvageCount}",
                state.JuryRigId,
                context.MethodUsed,
                netSuccesses,
                salvage.Count);

            return JuryRigResult.CriticalSuccess(
                netSuccesses,
                effectiveDc,
                context.MethodUsed,
                salvage);
        }

        _logger.LogInformation(
            "Experiment SUCCESS: SessionId={SessionId}, Method={Method}, " +
            "NetSuccesses={NetSuccesses}",
            state.JuryRigId,
            context.MethodUsed,
            netSuccesses);

        return JuryRigResult.Success(
            netSuccesses,
            effectiveDc,
            context.MethodUsed);
    }

    /// <summary>
    /// Handles a failure outcome (roll on complication table).
    /// </summary>
    private JuryRigResult HandleFailure(
        JuryRigState state,
        JuryRigContext context,
        int netSuccesses,
        int effectiveDc)
    {
        // Roll d10 on complication table
        var complicationRoll = _diceService.Roll(DiceType.D10).Total;
        var effect = ProcessComplication(complicationRoll);

        _logger.LogInformation(
            "Experiment FAILED: SessionId={SessionId}, Method={Method}, " +
            "NetSuccesses={NetSuccesses}, ComplicationRoll={Roll}, Effect={Effect}",
            state.JuryRigId,
            context.MethodUsed,
            netSuccesses,
            complicationRoll,
            effect);

        // Handle special complication effects
        var damage = 0;
        var alert = false;
        var narrative = ComplicationNarratives.GetValueOrDefault(effect, "Something unexpected happens.");

        switch (effect)
        {
            case ComplicationEffect.AlarmTriggered:
                alert = true;
                break;

            case ComplicationEffect.SparksFly:
                damage = _diceService.Roll(DiceType.D6, SparksFlyDamageDice).Total;
                narrative = $"{narrative} Take {damage} electrical damage.";
                break;

            case ComplicationEffect.GlitchInFavor:
                // Auto-success!
                return JuryRigResult.GlitchInFavor(
                    netSuccesses,
                    effectiveDc,
                    context.MethodUsed);

            case ComplicationEffect.PermanentLock:
                return JuryRigResult.PermanentLock(
                    netSuccesses,
                    effectiveDc,
                    context.MethodUsed);

            case ComplicationEffect.PartialSuccess:
                return JuryRigResult.PartialSuccess(
                    netSuccesses,
                    effectiveDc,
                    context.MethodUsed,
                    "One secondary function activates.");
        }

        return JuryRigResult.Failure(
            netSuccesses,
            effectiveDc,
            context.MethodUsed,
            complicationRoll,
            effect,
            damage,
            alert,
            narrative);
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Dice Rolling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rolls an attribute check and returns net successes against DC.
    /// </summary>
    /// <param name="score">The attribute score (determines dice pool size).</param>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>Net successes (can be negative).</returns>
    private int RollNetSuccesses(int score, int dc)
    {
        var dicePool = DicePool.D10(Math.Max(1, score));
        var result = _diceService.Roll(dicePool);

        _logger.LogDebug(
            "Dice roll: Score={Score}, Successes={Successes}, Botches={Botches}, " +
            "NetSuccesses={Net}, DC={DC}",
            score,
            result.TotalSuccesses,
            result.TotalBotches,
            result.NetSuccesses,
            dc);

        // Net successes relative to DC
        return result.NetSuccesses;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates that the state is at the expected step and not terminal.
    /// </summary>
    private void ValidateStep(JuryRigState state, JuryRigStep expectedStep, string operation)
    {
        if (state.IsTerminal)
        {
            throw new InvalidOperationException(
                $"Cannot {operation}: jury-rigging is in terminal state {state.Status}.");
        }

        if (state.CurrentStep != expectedStep)
        {
            throw new InvalidOperationException(
                $"Cannot {operation} from step {state.CurrentStep}. Must be at {expectedStep} step.");
        }
    }
}
