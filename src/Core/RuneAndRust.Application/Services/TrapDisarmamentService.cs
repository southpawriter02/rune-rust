// ------------------------------------------------------------------------------
// <copyright file="TrapDisarmamentService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling the three-step trap disarmament procedure.
// Manages Detection, Analysis, and Disarmament stages with proper
// DC escalation, tool requirements, and fumble consequences.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling the three-step trap disarmament procedure.
/// </summary>
/// <remarks>
/// <para>
/// The trap disarmament procedure follows three steps:
/// <list type="number">
///   <item><description>Detection: Find the trap before triggering it (Perception check)</description></item>
///   <item><description>Analysis: Study the trap to reveal information (optional, WITS check)</description></item>
///   <item><description>Disarmament: Neutralize the trap (higher of WITS or FINESSE)</description></item>
/// </list>
/// </para>
/// <para>
/// This service manages:
/// <list type="bullet">
///   <item><description>Detection checks against trap-specific DCs</description></item>
///   <item><description>Analysis with tiered information reveal (DC-2, DC, DC+2)</description></item>
///   <item><description>Disarmament with tool modifiers and hint bonuses</description></item>
///   <item><description>DC escalation on failure (+1 per failed attempt)</description></item>
///   <item><description>[Forced Execution] fumble consequence</description></item>
///   <item><description>Critical success salvage system</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class TrapDisarmamentService : ITrapDisarmamentService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The skill identifier for Perception checks during detection.
    /// </summary>
    private const string PerceptionSkillId = "perception";

    /// <summary>
    /// The skill identifier for WITS checks during analysis.
    /// </summary>
    private const string WitsCheckId = "wits-check";

    /// <summary>
    /// The skill identifier for FINESSE checks during disarmament.
    /// </summary>
    private const string FinesseCheckId = "finesse-check";

    /// <summary>
    /// Minimum DC threshold requiring tools for disarmament.
    /// </summary>
    private const int MinimumDcRequiringTools = 4;

    /// <summary>
    /// Critical success threshold (net successes >= this value yields salvage).
    /// </summary>
    private const int CriticalSuccessThreshold = 5;

    // -------------------------------------------------------------------------
    // Detection DC by Trap Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Static mapping of trap types to their detection DCs.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, int> DetectionDcs =
        new Dictionary<TrapType, int>
        {
            { TrapType.Tripwire, 8 },
            { TrapType.PressurePlate, 10 },
            { TrapType.Electrified, 14 },
            { TrapType.LaserGrid, 18 },
            { TrapType.JotunDefense, 22 }
        };

    // -------------------------------------------------------------------------
    // Disarm DC by Trap Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Static mapping of trap types to their base disarm DCs.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, int> DisarmDcs =
        new Dictionary<TrapType, int>
        {
            { TrapType.Tripwire, 8 },
            { TrapType.PressurePlate, 12 },
            { TrapType.Electrified, 16 },
            { TrapType.LaserGrid, 20 },
            { TrapType.JotunDefense, 24 }
        };

    // -------------------------------------------------------------------------
    // Trap Effects by Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Static mapping of trap types to their trigger effects.
    /// </summary>
    /// <remarks>
    /// Tuple format: (DamageDice, DamageType, TriggersAlert, TriggersLockdown)
    /// </remarks>
    private static readonly IReadOnlyDictionary<TrapType, (string DamageDice, string DamageType, bool Alert, bool Lockdown)> TrapEffects =
        new Dictionary<TrapType, (string, string, bool, bool)>
        {
            { TrapType.Tripwire, ("0", "none", true, false) },        // Alarm only
            { TrapType.PressurePlate, ("2d10", "physical", false, false) },
            { TrapType.Electrified, ("3d10", "lightning", false, false) },
            { TrapType.LaserGrid, ("0", "none", true, true) },        // Alarm + lockdown
            { TrapType.JotunDefense, ("5d10", "physical", true, false) }
        };

    // -------------------------------------------------------------------------
    // Salvageable Components by Trap Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Static mapping of trap types to their salvageable components.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, IReadOnlyList<string>> SalvageComponents =
        new Dictionary<TrapType, IReadOnlyList<string>>
        {
            { TrapType.Tripwire, new[] { "trigger-mechanism", "wire-bundle" } },
            { TrapType.PressurePlate, new[] { "high-tension-spring", "pressure-sensor" } },
            { TrapType.Electrified, new[] { "capacitor", "blighted-power-cell" } },
            { TrapType.LaserGrid, new[] { "sensor-module", "focusing-crystal" } },
            { TrapType.JotunDefense, new[] { "jotun-mechanism-fragment", "ancient-power-core" } }
        };

    // -------------------------------------------------------------------------
    // Narrative Descriptions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Narrative text for successful detection by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> DetectionSuccessNarratives =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "Your trained eye catches the glint of wire stretched across the passage." },
            { TrapType.PressurePlate, "The floor tile ahead sits slightly higher than its neighbors. A pressure plate." },
            { TrapType.Electrified, "The faint hum of electricity warns you of the charged grid ahead." },
            { TrapType.LaserGrid, "Faint red lines crisscross the corridor—Old World security lasers." },
            { TrapType.JotunDefense, "Something ancient stirs at the edge of your perception. Jotun defenses." }
        };

    /// <summary>
    /// Narrative text for failed detection (trap triggers) by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> DetectionFailureNarratives =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "Your foot catches the wire too late. A shrill alarm pierces the darkness." },
            { TrapType.PressurePlate, "The floor gives way beneath your foot. Something clicks—then strikes." },
            { TrapType.Electrified, "The grid crackles to life as you step onto it. Lightning arcs through your body." },
            { TrapType.LaserGrid, "The beam flares red as you break it. Doors slam shut. Alarms wail." },
            { TrapType.JotunDefense, "Ancient mechanisms awaken. Pain and noise erupt in equal measure." }
        };

    /// <summary>
    /// Narrative text for successful disarmament by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> DisarmSuccessNarratives =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "The wire goes slack in your hands. The trap is neutralized." },
            { TrapType.PressurePlate, "With careful precision, you disable the pressure mechanism. Safe." },
            { TrapType.Electrified, "The crackling fades. The grid is now just harmless metal." },
            { TrapType.LaserGrid, "The beams flicker and die. You've blinded the ancient eye." },
            { TrapType.JotunDefense, "The Jotun mechanism groans and falls silent. You've outsmarted the ancients." }
        };

    /// <summary>
    /// Narrative text for critical success disarmament by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> CriticalSuccessNarratives =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "Not only do you disable the wire, but you recover usable components from the mechanism." },
            { TrapType.PressurePlate, "With expert precision, you dismantle the trap entirely, salvaging valuable components." },
            { TrapType.Electrified, "You not only disable the grid but carefully extract its power components. Excellent work." },
            { TrapType.LaserGrid, "The system yields to your expertise. You salvage the focusing crystal and sensor module intact." },
            { TrapType.JotunDefense, "The ancient mechanism surrenders its secrets. You recover fragments of Jotun technology." }
        };

    /// <summary>
    /// Narrative text for fumble ([Forced Execution]) by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> FumbleNarratives =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "Your tools slip at the worst moment. The alarm screams into the darkness." },
            { TrapType.PressurePlate, "A wrong move—the mechanism triggers with you on top of it." },
            { TrapType.Electrified, "Your probe touches the wrong contact. Lightning answers your mistake." },
            { TrapType.LaserGrid, "You stumble into the beam. Doors seal. Alarms howl. You're trapped." },
            { TrapType.JotunDefense, "The ancient defense senses your fumbling. It punishes such incompetence severely." }
        };

    /// <summary>
    /// Consequence descriptions revealed through analysis by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> ConsequenceDescriptions =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "Triggering this wire would sound an alarm, alerting nearby threats." },
            { TrapType.PressurePlate, "Failure would release a crushing mechanism dealing 2d10 damage." },
            { TrapType.Electrified, "The grid would unleash 3d10 lightning damage through the conductive surface." },
            { TrapType.LaserGrid, "Breaking the beam would trigger alarms and seal all nearby doors." },
            { TrapType.JotunDefense, "The ancient defense would unleash 5d10 damage and summon security constructs." }
        };

    /// <summary>
    /// Disarmament hints revealed through analysis by trap type.
    /// </summary>
    private static readonly IReadOnlyDictionary<TrapType, string> DisarmHints =
        new Dictionary<TrapType, string>
        {
            { TrapType.Tripwire, "The wire is attached to a simple bell mechanism. Cutting it near the anchor would silence it." },
            { TrapType.PressurePlate, "The pressure sensor has a calibration screw. Loosening it would increase the trigger threshold." },
            { TrapType.Electrified, "The power coupling on the left side appears to be the main feed. Disconnecting it first would be wise." },
            { TrapType.LaserGrid, "The emitters cycle every few seconds. Timing your movements to the cycle is the key." },
            { TrapType.JotunDefense, "The mechanism hums in a pattern—ancient but recognizable. Matching the frequency with your tools might confuse it." }
        };

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly SkillCheckService _skillCheckService;
    private readonly IDiceService _diceService;
    private readonly ILogger<TrapDisarmamentService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TrapDisarmamentService"/> class.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public TrapDisarmamentService(
        SkillCheckService skillCheckService,
        IDiceService diceService,
        ILogger<TrapDisarmamentService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("TrapDisarmamentService initialized");
    }

    // -------------------------------------------------------------------------
    // Detection Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public TrapDisarmResult AttemptDetection(Player player, TrapType trapType)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Create new disarmament state
        var state = TrapDisarmState.Create(player.Id.ToString(), trapType);
        var detectionDc = GetDetectionDc(trapType);

        _logger.LogInformation(
            "Detection attempt initiated: PlayerId={PlayerId}, PlayerName={PlayerName}, " +
            "TrapType={TrapType}, DetectionDC={DetectionDc}",
            player.Id,
            player.Name,
            trapType,
            detectionDc);

        // Perform Perception check for detection
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            PerceptionSkillId,
            detectionDc,
            $"Detect {trapType}");

        _logger.LogDebug(
            "Detection check result: TrapType={TrapType}, NetSuccesses={NetSuccesses}, " +
            "DC={DetectionDc}, IsSuccess={IsSuccess}",
            trapType,
            checkResult.NetSuccesses,
            detectionDc,
            checkResult.IsSuccess);

        if (checkResult.IsSuccess)
        {
            // Successful detection
            state.MarkDetected();

            _logger.LogInformation(
                "Trap detected successfully: PlayerId={PlayerId}, TrapType={TrapType}, " +
                "NetSuccesses={NetSuccesses}",
                player.Id,
                trapType,
                checkResult.NetSuccesses);

            return TrapDisarmResult.DetectionSuccess(state, checkResult.NetSuccesses, detectionDc);
        }
        else
        {
            // Failed detection - walk into trap, trigger it
            state.MarkTriggered();

            var (damageDice, damageType, alert, lockdown) = GetTrapEffect(trapType);
            var actualDamage = RollTrapDamage(damageDice);

            _logger.LogWarning(
                "Detection FAILED - trap triggered: PlayerId={PlayerId}, TrapType={TrapType}, " +
                "Damage={Damage}, DamageType={DamageType}, Alert={Alert}, Lockdown={Lockdown}",
                player.Id,
                trapType,
                actualDamage,
                damageType,
                alert,
                lockdown);

            var narrative = GetDetectionFailureNarrative(trapType);

            return TrapDisarmResult.DetectionFailure(
                state,
                checkResult.NetSuccesses,
                detectionDc,
                actualDamage,
                damageType,
                alert,
                lockdown,
                narrative);
        }
    }

    // -------------------------------------------------------------------------
    // Analysis Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public TrapAnalysisInfo AnalyzeTrap(TrapDisarmState state, Player player)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(player);

        if (state.Status != DisarmStatus.Detected)
        {
            throw new InvalidOperationException(
                $"Cannot analyze trap in status {state.Status}. Must be Detected first.");
        }

        var baseDisarmDc = GetDisarmDc(state.TrapType);

        _logger.LogInformation(
            "Analysis attempt initiated: PlayerId={PlayerId}, PlayerName={PlayerName}, " +
            "TrapType={TrapType}, BaseDisarmDC={BaseDisarmDc}",
            player.Id,
            player.Name,
            state.TrapType,
            baseDisarmDc);

        // Perform WITS check for analysis
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WitsCheckId,
            baseDisarmDc,
            $"Analyze {state.TrapType}");

        _logger.LogDebug(
            "Analysis check result: TrapType={TrapType}, NetSuccesses={NetSuccesses}, " +
            "BaseDC={BaseDC}",
            state.TrapType,
            checkResult.NetSuccesses,
            baseDisarmDc);

        // Create analysis info based on check result
        var analysisInfo = TrapAnalysisInfo.FromCheckResult(
            checkResult.NetSuccesses,
            baseDisarmDc,
            state.CurrentDisarmDc,
            GetConsequenceDescription(state.TrapType),
            GetDisarmHint(state.TrapType));

        // Record analysis in state
        state.RecordAnalysis(analysisInfo);

        _logger.LogInformation(
            "Analysis complete: PlayerId={PlayerId}, TrapType={TrapType}, " +
            "DcRevealed={DcRevealed}, ConsequencesRevealed={ConsequencesRevealed}, " +
            "HintRevealed={HintRevealed}, GrantsHintBonus={GrantsHintBonus}",
            player.Id,
            state.TrapType,
            analysisInfo.DisarmDcRevealed,
            analysisInfo.ConsequencesRevealed,
            analysisInfo.HintRevealed,
            analysisInfo.GrantsHintBonus);

        return analysisInfo;
    }

    // -------------------------------------------------------------------------
    // Disarmament Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public TrapDisarmResult AttemptDisarmament(TrapDisarmState state, Player player, ToolQuality toolQuality)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(player);

        // Validate state
        if (state.Status != DisarmStatus.Detected &&
            state.Status != DisarmStatus.Analyzed &&
            state.Status != DisarmStatus.DisarmInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot attempt disarmament in status {state.Status}. " +
                "Must be Detected, Analyzed, or DisarmInProgress.");
        }

        // Validate tool requirements
        var baseDisarmDc = GetDisarmDc(state.TrapType);
        if (baseDisarmDc >= MinimumDcRequiringTools && toolQuality == ToolQuality.BareHands)
        {
            _logger.LogWarning(
                "Disarmament rejected - tools required: PlayerId={PlayerId}, TrapType={TrapType}, " +
                "BaseDC={BaseDC}, ToolQuality={ToolQuality}",
                player.Id,
                state.TrapType,
                baseDisarmDc,
                toolQuality);

            throw new InvalidOperationException(
                $"DC {baseDisarmDc}+ traps require [Tinker's Toolkit]. Cannot attempt bare-handed.");
        }

        // If coming from Detected state, skip analysis
        if (state.Status == DisarmStatus.Detected)
        {
            state.SkipAnalysis();
            _logger.LogDebug(
                "Analysis skipped - proceeding directly to disarmament: PlayerId={PlayerId}, TrapType={TrapType}",
                player.Id,
                state.TrapType);
        }

        // Create trap context for modifiers
        var context = new TrapContext(
            state.TrapType,
            DisarmStep.Disarmament,
            toolQuality,
            state.HasHintBonus,
            state.FailedAttempts);

        var effectiveDc = context.GetEffectiveDc();
        var totalModifier = context.GetTotalModifier();

        _logger.LogInformation(
            "Disarmament attempt initiated: PlayerId={PlayerId}, PlayerName={PlayerName}, " +
            "TrapType={TrapType}, EffectiveDC={EffectiveDc}, ToolQuality={ToolQuality}, " +
            "HasHintBonus={HasHintBonus}, TotalModifier={TotalModifier}, FailedAttempts={FailedAttempts}",
            player.Id,
            player.Name,
            state.TrapType,
            effectiveDc,
            toolQuality,
            state.HasHintBonus,
            totalModifier,
            state.FailedAttempts);

        // Determine which skill to use (higher of WITS or FINESSE)
        var skillId = GetBestDisarmSkill(player);

        _logger.LogDebug(
            "Using skill for disarmament: PlayerId={PlayerId}, SkillId={SkillId}, " +
            "PlayerWits={Wits}, PlayerFinesse={Finesse}",
            player.Id,
            skillId,
            player.Attributes.Wits,
            player.Attributes.Finesse);

        // Perform the skill check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            skillId,
            effectiveDc,
            $"Disarm {state.TrapType}",
            AdvantageType.Normal,
            totalModifier);

        _logger.LogDebug(
            "Disarmament check result: TrapType={TrapType}, NetSuccesses={NetSuccesses}, " +
            "EffectiveDC={EffectiveDc}, IsSuccess={IsSuccess}, IsFumble={IsFumble}",
            state.TrapType,
            checkResult.NetSuccesses,
            effectiveDc,
            checkResult.IsSuccess,
            checkResult.IsFumble);

        // Check for fumble first (0 successes + botch = [Forced Execution])
        if (checkResult.IsFumble)
        {
            return HandleFumble(state, player, checkResult.NetSuccesses, effectiveDc);
        }

        // Check for success
        if (checkResult.IsSuccess)
        {
            var isCritical = checkResult.NetSuccesses >= CriticalSuccessThreshold;
            return HandleSuccess(state, player, checkResult.NetSuccesses, effectiveDc, isCritical);
        }

        // Failure - DC escalation
        return HandleFailure(state, player, checkResult.NetSuccesses, effectiveDc);
    }

    // -------------------------------------------------------------------------
    // Trap Information Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public (string DamageDice, string DamageType, bool Alert, bool Lockdown) GetTrapEffect(TrapType trapType)
    {
        if (TrapEffects.TryGetValue(trapType, out var effect))
        {
            _logger.LogDebug(
                "Retrieved trap effect: TrapType={TrapType}, DamageDice={DamageDice}, " +
                "DamageType={DamageType}, Alert={Alert}, Lockdown={Lockdown}",
                trapType,
                effect.DamageDice,
                effect.DamageType,
                effect.Alert,
                effect.Lockdown);
            return effect;
        }

        _logger.LogDebug(
            "Trap type {TrapType} has no effect configuration (defaulting to no effect)",
            trapType);
        return ("0", "none", false, false);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetSalvageableComponents(TrapType trapType)
    {
        if (SalvageComponents.TryGetValue(trapType, out var components))
        {
            _logger.LogDebug(
                "Retrieved salvageable components: TrapType={TrapType}, Components=[{Components}]",
                trapType,
                string.Join(", ", components));
            return components;
        }

        _logger.LogDebug(
            "Trap type {TrapType} has no salvageable components (defaulting to empty)",
            trapType);
        return Array.Empty<string>();
    }

    /// <inheritdoc />
    public int GetDetectionDc(TrapType trapType)
    {
        if (DetectionDcs.TryGetValue(trapType, out var dc))
        {
            _logger.LogDebug(
                "Retrieved detection DC: TrapType={TrapType}, DetectionDC={DetectionDc}",
                trapType,
                dc);
            return dc;
        }

        _logger.LogDebug(
            "Trap type {TrapType} has no detection DC configured (defaulting to 12)",
            trapType);
        return 12; // Default fallback
    }

    /// <inheritdoc />
    public int GetDisarmDc(TrapType trapType)
    {
        if (DisarmDcs.TryGetValue(trapType, out var dc))
        {
            _logger.LogDebug(
                "Retrieved disarm DC: TrapType={TrapType}, DisarmDC={DisarmDc}",
                trapType,
                dc);
            return dc;
        }

        _logger.LogDebug(
            "Trap type {TrapType} has no disarm DC configured (defaulting to 12)",
            trapType);
        return 12; // Default fallback
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Outcome Handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles a fumble outcome ([Forced Execution]).
    /// </summary>
    /// <param name="state">The current disarmament state.</param>
    /// <param name="player">The player who fumbled.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="effectiveDc">The effective DC for the check.</param>
    /// <returns>The fumble TrapDisarmResult.</returns>
    /// <remarks>
    /// On fumble ([Forced Execution]):
    /// <list type="bullet">
    ///   <item><description>Trap triggers on the disarmer</description></item>
    ///   <item><description>Full trap effects applied (damage, alerts, lockdown)</description></item>
    ///   <item><description>Trap is destroyed (no salvage possible)</description></item>
    /// </list>
    /// </remarks>
    private TrapDisarmResult HandleFumble(
        TrapDisarmState state,
        Player player,
        int netSuccesses,
        int effectiveDc)
    {
        state.MarkDestroyed();

        var (damageDice, damageType, alert, lockdown) = GetTrapEffect(state.TrapType);
        var actualDamage = RollTrapDamage(damageDice);
        var narrative = GetFumbleNarrative(state.TrapType);

        _logger.LogWarning(
            "FUMBLE! [Forced Execution]: PlayerId={PlayerId}, TrapType={TrapType}, " +
            "Damage={Damage}, DamageType={DamageType}, Alert={Alert}, Lockdown={Lockdown}",
            player.Id,
            state.TrapType,
            actualDamage,
            damageType,
            alert,
            lockdown);

        return TrapDisarmResult.Fumble(
            state,
            netSuccesses,
            effectiveDc,
            actualDamage,
            damageType,
            alert,
            lockdown,
            narrative);
    }

    /// <summary>
    /// Handles a success outcome (including critical success with salvage).
    /// </summary>
    /// <param name="state">The current disarmament state.</param>
    /// <param name="player">The player who succeeded.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="effectiveDc">The effective DC for the check.</param>
    /// <param name="isCritical">Whether this was a critical success.</param>
    /// <returns>The success TrapDisarmResult.</returns>
    /// <remarks>
    /// On success:
    /// <list type="bullet">
    ///   <item><description>Trap is disabled safely</description></item>
    ///   <item><description>Critical success (net >= 5) yields salvage components</description></item>
    /// </list>
    /// </remarks>
    private TrapDisarmResult HandleSuccess(
        TrapDisarmState state,
        Player player,
        int netSuccesses,
        int effectiveDc,
        bool isCritical)
    {
        IReadOnlyList<string> salvage = isCritical
            ? GetSalvageableComponents(state.TrapType)
            : Array.Empty<string>();

        state.MarkDisarmed(salvage);

        var narrative = isCritical
            ? GetCriticalSuccessNarrative(state.TrapType)
            : GetSuccessNarrative(state.TrapType);

        if (isCritical)
        {
            _logger.LogInformation(
                "CRITICAL SUCCESS - trap disarmed with salvage: PlayerId={PlayerId}, " +
                "TrapType={TrapType}, NetSuccesses={NetSuccesses}, SalvageCount={SalvageCount}, " +
                "Components=[{Components}]",
                player.Id,
                state.TrapType,
                netSuccesses,
                salvage.Count,
                string.Join(", ", salvage));
        }
        else
        {
            _logger.LogInformation(
                "Disarmament SUCCESS: PlayerId={PlayerId}, TrapType={TrapType}, " +
                "NetSuccesses={NetSuccesses}",
                player.Id,
                state.TrapType,
                netSuccesses);
        }

        return TrapDisarmResult.DisarmSuccess(
            state,
            netSuccesses,
            effectiveDc,
            isCritical,
            salvage,
            narrative);
    }

    /// <summary>
    /// Handles a failure outcome (DC escalation).
    /// </summary>
    /// <param name="state">The current disarmament state.</param>
    /// <param name="player">The player who failed.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="effectiveDc">The effective DC for the check.</param>
    /// <returns>The failure TrapDisarmResult.</returns>
    /// <remarks>
    /// On failure:
    /// <list type="bullet">
    ///   <item><description>Trap remains active</description></item>
    ///   <item><description>DC increases by +1 for next attempt</description></item>
    ///   <item><description>Character may retry</description></item>
    /// </list>
    /// </remarks>
    private TrapDisarmResult HandleFailure(
        TrapDisarmState state,
        Player player,
        int netSuccesses,
        int effectiveDc)
    {
        state.RecordFailedAttempt();

        _logger.LogInformation(
            "Disarmament FAILED - DC escalation: PlayerId={PlayerId}, TrapType={TrapType}, " +
            "NetSuccesses={NetSuccesses}, FailedAttempts={FailedAttempts}, NewEffectiveDC={NewDc}",
            player.Id,
            state.TrapType,
            netSuccesses,
            state.FailedAttempts,
            state.CurrentDisarmDc);

        return TrapDisarmResult.DisarmFailure(state, netSuccesses, effectiveDc);
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Skill and Attribute
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines the best skill to use for disarmament based on player attributes.
    /// </summary>
    /// <param name="player">The player attempting disarmament.</param>
    /// <returns>The skill ID for the higher of WITS or FINESSE.</returns>
    /// <remarks>
    /// Disarmament uses the higher of WITS (intelligence/perception) or
    /// FINESSE (agility/precision) to represent that some characters
    /// outthink traps while others have steadier hands.
    /// </remarks>
    private string GetBestDisarmSkill(Player player)
    {
        var wits = player.Attributes.Wits;
        var finesse = player.Attributes.Finesse;

        return wits >= finesse ? WitsCheckId : FinesseCheckId;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Dice Rolling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rolls damage dice for a triggered trap.
    /// </summary>
    /// <param name="damageDice">Dice expression (e.g., "2d10", "3d10").</param>
    /// <returns>Total damage rolled, or 0 if no damage.</returns>
    private int RollTrapDamage(string damageDice)
    {
        if (string.IsNullOrEmpty(damageDice) || damageDice == "0")
        {
            _logger.LogDebug("No damage dice to roll (damage expression: {DamageDice})", damageDice);
            return 0;
        }

        var pool = DicePool.Parse(damageDice);
        var result = _diceService.Roll(pool);

        _logger.LogDebug(
            "Trap damage roll: {DamageDice} = [{Rolls}] = {Total}",
            damageDice,
            string.Join(", ", result.Rolls),
            result.Total);

        return result.Total;
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods - Narrative Text
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the narrative text for failed detection (trap triggers).
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Narrative text for the detection failure.</returns>
    private string GetDetectionFailureNarrative(TrapType trapType)
    {
        return DetectionFailureNarratives.TryGetValue(trapType, out var narrative)
            ? narrative
            : "The trap activates.";
    }

    /// <summary>
    /// Gets the narrative text for successful disarmament.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Narrative text for the disarmament success.</returns>
    private string GetSuccessNarrative(TrapType trapType)
    {
        return DisarmSuccessNarratives.TryGetValue(trapType, out var narrative)
            ? narrative
            : "The trap is disabled.";
    }

    /// <summary>
    /// Gets the narrative text for critical success disarmament.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Narrative text for the critical success.</returns>
    private string GetCriticalSuccessNarrative(TrapType trapType)
    {
        return CriticalSuccessNarratives.TryGetValue(trapType, out var narrative)
            ? narrative
            : "The trap is disabled and yields components.";
    }

    /// <summary>
    /// Gets the narrative text for fumble ([Forced Execution]).
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Narrative text for the fumble.</returns>
    private string GetFumbleNarrative(TrapType trapType)
    {
        return FumbleNarratives.TryGetValue(trapType, out var narrative)
            ? narrative
            : "The trap triggers on you.";
    }

    /// <summary>
    /// Gets the consequence description for a trap type.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Description of failure consequences.</returns>
    private string GetConsequenceDescription(TrapType trapType)
    {
        return ConsequenceDescriptions.TryGetValue(trapType, out var description)
            ? description
            : "Unknown consequences.";
    }

    /// <summary>
    /// Gets the disarm hint for a trap type.
    /// </summary>
    /// <param name="trapType">The type of trap.</param>
    /// <returns>Hint for disarmament.</returns>
    private string GetDisarmHint(TrapType trapType)
    {
        return DisarmHints.TryGetValue(trapType, out var hint)
            ? hint
            : "No hints available.";
    }
}
