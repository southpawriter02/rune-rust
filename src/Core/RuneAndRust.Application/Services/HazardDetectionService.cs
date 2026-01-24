using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for hazard detection operations in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Hazard Detection System mechanics for the Wasteland Survival skill.
/// Allows characters to detect and identify environmental hazards through passive
/// perception (automatic hints) or active investigation (formal skill checks).
/// </para>
/// <para>
/// Detection methods:
/// <list type="bullet">
///   <item><description>Passive: WITS ÷ 2 vs hazard DC (hint only, no type revealed)</description></item>
///   <item><description>Active: Full Wasteland Survival check (full identification)</description></item>
///   <item><description>Critical: Net successes ≥ 5 (additional context revealed)</description></item>
///   <item><description>Area Sweep: Multiple hazard detection in single location</description></item>
/// </list>
/// </para>
/// <para>
/// Consequence application:
/// <list type="bullet">
///   <item><description>ObviousDanger: 1d6 physical damage</description></item>
///   <item><description>HiddenPit: 2d10 fall damage, may require assistance</description></item>
///   <item><description>ToxicZone: [Poisoned] status for 3 rounds</description></item>
///   <item><description>GlitchPocket: Random glitch effect + [Disoriented] for 2 rounds</description></item>
///   <item><description>AmbushSite: Enemies gain surprise round</description></item>
/// </list>
/// </para>
/// </remarks>
public class HazardDetectionService : IHazardDetectionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - SKILL IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in hazard detection checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - HAZARD DETECTION DCS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Detection DC for Obvious Danger hazards (easy to spot).
    /// </summary>
    private const int ObviousDangerDc = 8;

    /// <summary>
    /// Detection DC for Hidden Pit hazards.
    /// </summary>
    private const int HiddenPitDc = 12;

    /// <summary>
    /// Detection DC for Toxic Zone hazards.
    /// </summary>
    private const int ToxicZoneDc = 16;

    /// <summary>
    /// Detection DC for Ambush Site hazards.
    /// </summary>
    private const int AmbushSiteDc = 16;

    /// <summary>
    /// Detection DC for Glitch Pocket hazards (hardest to detect).
    /// </summary>
    private const int GlitchPocketDc = 20;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - DETECTION THRESHOLDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Net successes required for critical detection (additional context revealed).
    /// </summary>
    private const int CriticalDetectionThreshold = 5;

    /// <summary>
    /// Divisor for calculating passive perception from WITS.
    /// </summary>
    private const int PassivePerceptionDivisor = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - DAMAGE DICE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Damage dice for Obvious Danger hazard.
    /// </summary>
    private const string ObviousDangerDamage = "1d6";

    /// <summary>
    /// Damage dice for Hidden Pit hazard.
    /// </summary>
    private const string HiddenPitDamage = "2d10";

    /// <summary>
    /// Damage dice for Glitch Pocket psychic damage (on roll of 2).
    /// </summary>
    private const string GlitchPocketPsychicDamage = "2d10";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - STATUS EFFECT DURATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Duration in rounds for Poisoned status from Toxic Zone.
    /// </summary>
    private const int PoisonedDuration = 3;

    /// <summary>
    /// Duration in rounds for Disoriented status from Glitch Pocket.
    /// </summary>
    private const int DisorientedDuration = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly DiceService _diceService;
    private readonly ILogger<HazardDetectionService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the HazardDetectionService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public HazardDetectionService(
        SkillCheckService skillCheckService,
        DiceService diceService,
        ILogger<HazardDetectionService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("HazardDetectionService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PASSIVE DETECTION (Automatic on Area Entry)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<HazardDetectionResult> CheckPassiveDetection(
        Player player,
        IReadOnlyList<DetectableHazardType> hazards)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(hazards, nameof(hazards));

        var passivePerception = GetPassivePerception(player);
        var results = new List<HazardDetectionResult>();

        _logger.LogDebug(
            "Checking passive detection for PlayerId={PlayerId} with PassivePerception={Passive}, " +
            "HazardCount={Count}",
            player.Id, passivePerception, hazards.Count);

        foreach (var hazard in hazards)
        {
            var dc = GetDetectionDc(hazard);

            if (passivePerception >= dc)
            {
                _logger.LogInformation(
                    "PlayerId={PlayerId} passively sensed {HazardType}: " +
                    "Passive={Passive} >= DC={Dc}",
                    player.Id, hazard, passivePerception, dc);

                results.Add(HazardDetectionResult.PassiveHint(hazard, dc));
            }
            else
            {
                _logger.LogDebug(
                    "PlayerId={PlayerId} did not sense {HazardType}: " +
                    "Passive={Passive} < DC={Dc}",
                    player.Id, hazard, passivePerception, dc);
            }
        }

        _logger.LogDebug(
            "Passive detection complete for PlayerId={PlayerId}: {Detected}/{Total} hazards sensed",
            player.Id, results.Count, hazards.Count);

        return results;
    }

    /// <inheritdoc/>
    public int GetPassivePerception(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var wits = player.Attributes.Wits;
        var passive = wits / PassivePerceptionDivisor;

        _logger.LogDebug(
            "Calculated passive perception for PlayerId={PlayerId}: " +
            "WITS={Wits} ÷ {Divisor} = {Passive}",
            player.Id, wits, PassivePerceptionDivisor, passive);

        return passive;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ACTIVE DETECTION (Player-Initiated Investigation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public IReadOnlyList<HazardDetectionResult> InvestigateArea(
        Player player,
        IReadOnlyList<DetectableHazardType> hazards)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(hazards, nameof(hazards));

        _logger.LogInformation(
            "PlayerId={PlayerId} investigating area with {Count} potential hazards",
            player.Id, hazards.Count);

        // Check if investigation can proceed
        if (!CanInvestigate(player))
        {
            var reason = GetInvestigationBlockedReason(player);
            _logger.LogWarning(
                "Investigation blocked for PlayerId={PlayerId}: {Reason}",
                player.Id, reason);

            return Array.Empty<HazardDetectionResult>();
        }

        var results = new List<HazardDetectionResult>();

        foreach (var hazard in hazards)
        {
            var result = PerformActiveDetection(player, hazard, DetectionMethod.AreaSweep);
            results.Add(result);
        }

        var detectedCount = results.Count(r => r.HazardDetected);
        _logger.LogInformation(
            "Area sweep complete for PlayerId={PlayerId}: {Detected}/{Total} hazards detected",
            player.Id, detectedCount, hazards.Count);

        return results;
    }

    /// <inheritdoc/>
    public HazardDetectionResult InvestigateSpecific(Player player, DetectableHazardType hazardType)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogInformation(
            "PlayerId={PlayerId} actively investigating for {HazardType}",
            player.Id, hazardType);

        // Check if investigation can proceed
        if (!CanInvestigate(player))
        {
            var reason = GetInvestigationBlockedReason(player);
            _logger.LogWarning(
                "Investigation blocked for PlayerId={PlayerId}: {Reason}",
                player.Id, reason);

            return HazardDetectionResult.NotDetected(0, GetDetectionDc(hazardType), $"Investigation blocked: {reason}");
        }

        return PerformActiveDetection(player, hazardType, DetectionMethod.ActiveInvestigation);
    }

    /// <summary>
    /// Performs an active detection skill check against a specific hazard.
    /// </summary>
    /// <param name="player">The player investigating.</param>
    /// <param name="hazardType">The hazard type to detect.</param>
    /// <param name="method">The detection method being used.</param>
    /// <returns>A HazardDetectionResult indicating the outcome.</returns>
    private HazardDetectionResult PerformActiveDetection(
        Player player,
        DetectableHazardType hazardType,
        DetectionMethod method)
    {
        var dc = GetDetectionDc(hazardType);

        _logger.LogDebug(
            "Performing {Method} for PlayerId={PlayerId} against {HazardType} (DC={Dc})",
            method, player.Id, hazardType, dc);

        // Perform the skill check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            WastelandSurvivalSkillId,
            dc,
            $"Investigate for {hazardType.GetDisplayName()}",
            AdvantageType.Normal,
            0);

        var netSuccesses = checkResult.NetSuccesses;
        var rollDetails = BuildRollDetails(checkResult, hazardType, dc);

        _logger.LogDebug(
            "Skill check result for PlayerId={PlayerId}: " +
            "NetSuccesses={NetSuccesses}, TotalSuccesses={TotalSuccesses}, " +
            "TotalBotches={TotalBotches}, Outcome={Outcome}",
            player.Id, netSuccesses,
            checkResult.DiceResult.TotalSuccesses,
            checkResult.DiceResult.TotalBotches,
            checkResult.Outcome);

        // Determine if detection succeeded
        if (netSuccesses >= dc)
        {
            var isCritical = netSuccesses >= CriticalDetectionThreshold;

            _logger.LogInformation(
                "PlayerId={PlayerId} detected {HazardType}: " +
                "{NetSuccesses} successes vs DC {Dc}{Critical}",
                player.Id, hazardType, netSuccesses, dc,
                isCritical ? " [CRITICAL]" : "");

            // Create success result based on detection method
            return method == DetectionMethod.AreaSweep
                ? HazardDetectionResult.AreaSweepSuccess(
                    hazardType,
                    GetAvoidanceOptions(hazardType),
                    GetHazardConsequence(hazardType),
                    netSuccesses,
                    dc,
                    rollDetails)
                : HazardDetectionResult.ActiveSuccess(
                    hazardType,
                    GetAvoidanceOptions(hazardType),
                    GetHazardConsequence(hazardType),
                    netSuccesses,
                    dc,
                    rollDetails);
        }

        _logger.LogDebug(
            "PlayerId={PlayerId} failed to detect {HazardType}: " +
            "{NetSuccesses} successes vs DC {Dc}",
            player.Id, hazardType, netSuccesses, dc);

        return HazardDetectionResult.NotDetected(netSuccesses, dc, rollDetails);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HAZARD INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetDetectionDc(DetectableHazardType hazardType)
    {
        var dc = hazardType switch
        {
            DetectableHazardType.ObviousDanger => ObviousDangerDc,
            DetectableHazardType.HiddenPit => HiddenPitDc,
            DetectableHazardType.ToxicZone => ToxicZoneDc,
            DetectableHazardType.AmbushSite => AmbushSiteDc,
            DetectableHazardType.GlitchPocket => GlitchPocketDc,
            _ => HiddenPitDc // Default to moderate DC
        };

        _logger.LogDebug("Detection DC for {HazardType}: {Dc}", hazardType, dc);

        return dc;
    }

    /// <inheritdoc/>
    public (string DisplayName, string Description) GetHazardDescription(DetectableHazardType hazardType)
    {
        return (hazardType.GetDisplayName(), hazardType.GetDescription());
    }

    /// <inheritdoc/>
    public string GetHazardConsequence(DetectableHazardType hazardType)
    {
        return hazardType.GetConsequenceDescription();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetAvoidanceOptions(DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger => new[]
            {
                "Carefully navigate around the debris",
                "Clear a path through (may take time)",
                "Find an alternate route"
            },

            DetectableHazardType.HiddenPit => new[]
            {
                "Walk around the concealed area",
                "Jump over the pit (DC 12 Acrobatics)",
                "Use debris or planks to bridge the gap"
            },

            DetectableHazardType.ToxicZone => new[]
            {
                "Find a path that avoids the contaminated area",
                "Use protective equipment (gas mask, hazmat suit)",
                "Wait for conditions to clear (if temporary)"
            },

            DetectableHazardType.GlitchPocket => new[]
            {
                "Give the distortion a wide berth",
                "Wait for the glitch cycle to stabilize",
                "Attempt to time passage through (extremely risky)"
            },

            DetectableHazardType.AmbushSite => new[]
            {
                "Approach from a different direction",
                "Create a distraction to draw enemies out",
                "Prepare for combat before entering",
                "Find a way to avoid the area entirely"
            },

            _ => new[] { "Proceed with caution" }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSEQUENCE APPLICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public HazardTriggerResult ApplyHazardConsequence(Player player, DetectableHazardType hazardType)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogWarning(
            "PlayerId={PlayerId} triggered {HazardType}!",
            player.Id, hazardType);

        var result = hazardType switch
        {
            DetectableHazardType.ObviousDanger => ApplyObviousDangerConsequence(player),
            DetectableHazardType.HiddenPit => ApplyHiddenPitConsequence(player),
            DetectableHazardType.ToxicZone => ApplyToxicZoneConsequence(player),
            DetectableHazardType.GlitchPocket => ApplyGlitchPocketConsequence(player),
            DetectableHazardType.AmbushSite => ApplyAmbushSiteConsequence(player),
            _ => HazardTriggerResult.Empty()
        };

        LogTriggerResult(player, result);

        return result;
    }

    /// <summary>
    /// Applies the consequence of triggering an Obvious Danger hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <returns>A HazardTriggerResult with damage information.</returns>
    private HazardTriggerResult ApplyObviousDangerConsequence(Player player)
    {
        var damage = _diceService.RollTotal(ObviousDangerDamage);

        _logger.LogDebug(
            "Obvious Danger consequence for PlayerId={PlayerId}: {Damage} damage ({Dice})",
            player.Id, damage, ObviousDangerDamage);

        return HazardTriggerResult.ObviousDanger(damage, $"Rolled {ObviousDangerDamage} = {damage} damage");
    }

    /// <summary>
    /// Applies the consequence of triggering a Hidden Pit hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <returns>A HazardTriggerResult with fall damage information.</returns>
    private HazardTriggerResult ApplyHiddenPitConsequence(Player player)
    {
        var damage = _diceService.RollTotal(HiddenPitDamage);

        _logger.LogDebug(
            "Hidden Pit consequence for PlayerId={PlayerId}: {Damage} fall damage ({Dice})",
            player.Id, damage, HiddenPitDamage);

        return HazardTriggerResult.HiddenPit(damage, $"Rolled {HiddenPitDamage} = {damage} fall damage");
    }

    /// <summary>
    /// Applies the consequence of triggering a Toxic Zone hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <returns>A HazardTriggerResult with poison status information.</returns>
    private HazardTriggerResult ApplyToxicZoneConsequence(Player player)
    {
        _logger.LogDebug(
            "Toxic Zone consequence for PlayerId={PlayerId}: [Poisoned] for {Duration} rounds",
            player.Id, PoisonedDuration);

        return HazardTriggerResult.ToxicZone($"Applied [Poisoned] status for {PoisonedDuration} rounds");
    }

    /// <summary>
    /// Applies the consequence of triggering a Glitch Pocket hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <returns>A HazardTriggerResult with glitch effect information.</returns>
    private HazardTriggerResult ApplyGlitchPocketConsequence(Player player)
    {
        // Roll on the Glitch Effect Table
        var (roll, effectName, effectDescription) = RollGlitchEffect();

        _logger.LogDebug(
            "Glitch Pocket consequence for PlayerId={PlayerId}: " +
            "Roll={Roll}, Effect={EffectName}",
            player.Id, roll, effectName);

        // Roll 2 causes psychic damage
        var damage = 0;
        if (HazardTriggerResult.GlitchEffectCausesDamage(roll))
        {
            damage = _diceService.RollTotal(GlitchPocketPsychicDamage);
            _logger.LogDebug(
                "Glitch Pocket psychic damage for PlayerId={PlayerId}: {Damage} ({Dice})",
                player.Id, damage, GlitchPocketPsychicDamage);
        }

        return HazardTriggerResult.GlitchPocket(
            roll,
            effectDescription,
            damage,
            $"Glitch Effect Table roll: {roll} ({effectName})" +
            (damage > 0 ? $", {GlitchPocketPsychicDamage} = {damage} psychic damage" : ""));
    }

    /// <summary>
    /// Applies the consequence of triggering an Ambush Site hazard.
    /// </summary>
    /// <param name="player">The player triggering the hazard.</param>
    /// <returns>A HazardTriggerResult with surprise round information.</returns>
    private HazardTriggerResult ApplyAmbushSiteConsequence(Player player)
    {
        _logger.LogDebug(
            "Ambush Site consequence for PlayerId={PlayerId}: Enemies gain surprise round",
            player.Id);

        return HazardTriggerResult.AmbushSite("Enemies gain surprise round");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GLITCH POCKET SPECIFICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public (int Roll, string EffectName, string EffectDescription) RollGlitchEffect()
    {
        var roll = _diceService.RollTotal("1d6");

        _logger.LogDebug("Glitch Effect Table roll: {Roll}", roll);

        var (name, description) = GetGlitchEffectDescription(roll);

        _logger.LogDebug(
            "Glitch Effect result: {Name} - {Description}",
            name, description);

        return (roll, name, description);
    }

    /// <inheritdoc/>
    public (string EffectName, string EffectDescription) GetGlitchEffectDescription(int roll)
    {
        return roll switch
        {
            1 => ("Teleport", "Reality tears—you are teleported 2d6 rooms in a random direction!"),
            2 => ("Psychic Feedback", "Psychic feedback—you take 2d10 psychic damage!"),
            3 => ("Equipment Malfunction", "Equipment malfunction—a random item is disabled until repaired!"),
            4 => ("Time Skip", "Time skip—you lose 1d4 hours, disoriented and confused!"),
            5 => ("Memory Echo", "Memory echo—you experience a vivid flashback of past combat!"),
            6 => ("Reality Anchor", "Reality anchor—you cannot leave this room for 1d6 rounds!"),
            _ => ("Unknown Effect", "Strange glitch effect—reality shimmers around you.")
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DETECTION CHECK PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CanInvestigate(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for blocking status effects
        // Note: This would need integration with a status effect service
        // For now, we assume investigation is allowed unless explicitly blocked

        _logger.LogDebug(
            "Investigation allowed for PlayerId={PlayerId}",
            player.Id);

        return true;
    }

    /// <inheritdoc/>
    public string? GetInvestigationBlockedReason(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for blocking conditions
        // Would check for Disoriented, Blinded, or other incapacitating statuses

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a human-readable roll details string for the detection result.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="hazardType">The hazard type being detected.</param>
    /// <param name="dc">The detection DC.</param>
    /// <returns>A formatted string describing the roll details.</returns>
    private static string BuildRollDetails(SkillCheckResult checkResult, DetectableHazardType hazardType, int dc)
    {
        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"DC: {dc} ({hazardType.GetDisplayName()})";
    }

    /// <summary>
    /// Logs the trigger result at the appropriate level with full details.
    /// </summary>
    /// <param name="player">The player who triggered the hazard.</param>
    /// <param name="result">The trigger result.</param>
    private void LogTriggerResult(Player player, HazardTriggerResult result)
    {
        var logLevel = result.Severity >= 3 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(
            logLevel,
            "Hazard triggered for PlayerId={PlayerId}: " +
            "Type={HazardType}, Damage={Damage}, Status={Status}, " +
            "Duration={Duration}, RequiresAssistance={Assistance}, " +
            "SurpriseRound={Surprise}",
            player.Id,
            result.HazardType.GetDisplayName(),
            result.DamageDealt,
            result.StatusApplied ?? "None",
            result.StatusDuration,
            result.RequiresAssistance,
            result.TriggersSurpriseRound);

        if (result.IsGlitchEffect)
        {
            _logger.LogInformation(
                "Glitch effect for PlayerId={PlayerId}: Roll={Roll}, Effect={Effect}",
                player.Id,
                result.GlitchEffectRoll,
                HazardTriggerResult.GetGlitchEffectName(result.GlitchEffectRoll!.Value));
        }

        if (result.HasSpecialEffect)
        {
            _logger.LogDebug(
                "Special effect for PlayerId={PlayerId}: {Effect}",
                player.Id, result.SpecialEffect);
        }
    }
}
