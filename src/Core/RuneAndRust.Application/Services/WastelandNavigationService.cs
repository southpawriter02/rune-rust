using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for wasteland navigation operations.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Navigation System mechanics for the Wasteland Survival skill.
/// Allows characters to navigate through the post-collapse landscape with success
/// determined by terrain difficulty, equipment, familiarity, and environmental conditions.
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Terrain type determines base DC (8 to 24)</description></item>
///   <item><description>Compass provides +1d10 (ineffective in glitched terrain)</description></item>
///   <item><description>Familiar territory provides +2d10</description></item>
///   <item><description>Weather and night conditions apply penalties</description></item>
///   <item><description>Fumbles result in entering dangerous areas</description></item>
/// </list>
/// </para>
/// <para>
/// Outcome determination:
/// <list type="bullet">
///   <item><description>Net successes ≥ DC: Success (×1.0 time)</description></item>
///   <item><description>Net successes ≥ DC - 2: Partial success (×1.25 time)</description></item>
///   <item><description>Net successes &lt; DC - 2: Failure (×1.5 time)</description></item>
///   <item><description>0 successes + ≥1 botch: Fumble (dangerous area)</description></item>
/// </list>
/// </para>
/// </remarks>
public class WastelandNavigationService : IWastelandNavigationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - SKILL IDENTIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in navigation checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - TERRAIN DCS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base DC for Open Wasteland terrain.
    /// </summary>
    private const int OpenWastelandDc = 8;

    /// <summary>
    /// Base DC for Moderate Ruins terrain.
    /// </summary>
    private const int ModerateRuinsDc = 12;

    /// <summary>
    /// Base DC for Dense Ruins terrain.
    /// </summary>
    private const int DenseRuinsDc = 16;

    /// <summary>
    /// Base DC for Labyrinthine terrain.
    /// </summary>
    private const int LabyrintheDc = 20;

    /// <summary>
    /// Base DC for Glitched Labyrinth terrain.
    /// </summary>
    private const int GlitchedLabyrinthDc = 24;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - TERRAIN SPEED MULTIPLIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Travel speed multiplier for Open Wasteland (fast travel).
    /// </summary>
    private const float OpenWastelandSpeedMultiplier = 1.0f;

    /// <summary>
    /// Travel speed multiplier for Moderate Ruins.
    /// </summary>
    private const float ModerateRuinsSpeedMultiplier = 0.8f;

    /// <summary>
    /// Travel speed multiplier for Dense Ruins.
    /// </summary>
    private const float DenseRuinsSpeedMultiplier = 0.6f;

    /// <summary>
    /// Travel speed multiplier for Labyrinthine terrain.
    /// </summary>
    private const float LabyrinthineSpeedMultiplier = 0.4f;

    /// <summary>
    /// Travel speed multiplier for Glitched Labyrinth (slowest).
    /// </summary>
    private const float GlitchedLabyrinthSpeedMultiplier = 0.3f;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - OUTCOME TIME MULTIPLIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Time multiplier for successful navigation.
    /// </summary>
    private const float SuccessTimeMultiplier = 1.0f;

    /// <summary>
    /// Time multiplier for partial success (minor delays).
    /// </summary>
    private const float PartialSuccessTimeMultiplier = 1.25f;

    /// <summary>
    /// Time multiplier for failure (got lost, must backtrack).
    /// </summary>
    private const float FailureTimeMultiplier = 1.5f;

    /// <summary>
    /// Time multiplier for fumble (dangerous area has other consequences).
    /// </summary>
    private const float FumbleTimeMultiplier = 1.0f;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - DICE MODIFIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Bonus dice from having a working compass.
    /// </summary>
    private const int CompassBonus = 1;

    /// <summary>
    /// Bonus dice from familiar territory.
    /// </summary>
    private const int FamiliarTerritoryBonus = 2;

    /// <summary>
    /// Penalty dice for traveling at night without light.
    /// </summary>
    private const int NightWithoutLightPenalty = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - WEATHER MODIFIERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Dice modifier for Clear weather (good visibility).
    /// </summary>
    private const int ClearWeatherModifier = 1;

    /// <summary>
    /// Dice modifier for Cloudy weather (normal visibility).
    /// </summary>
    private const int CloudyWeatherModifier = 0;

    /// <summary>
    /// Dice modifier for Light Rain (reduced visibility).
    /// </summary>
    private const int LightRainWeatherModifier = -1;

    /// <summary>
    /// Dice modifier for Heavy Rain (poor visibility).
    /// </summary>
    private const int HeavyRainWeatherModifier = -2;

    /// <summary>
    /// Dice modifier for Fog (very poor visibility).
    /// </summary>
    private const int FogWeatherModifier = -3;

    /// <summary>
    /// Dice modifier for Storm (severe conditions).
    /// </summary>
    private const int StormWeatherModifier = -4;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS - OUTCOME THRESHOLDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Margin below DC for partial success.
    /// Net successes >= DC - 2 is still a partial success.
    /// </summary>
    private const int PartialSuccessMargin = 2;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly DiceService _diceService;
    private readonly IFumbleConsequenceService _fumbleConsequenceService;
    private readonly ILogger<WastelandNavigationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the WastelandNavigationService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="fumbleConsequenceService">Service for creating fumble consequences.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public WastelandNavigationService(
        SkillCheckService skillCheckService,
        DiceService diceService,
        IFumbleConsequenceService fumbleConsequenceService,
        ILogger<WastelandNavigationService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _fumbleConsequenceService = fumbleConsequenceService ?? throw new ArgumentNullException(nameof(fumbleConsequenceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("WastelandNavigationService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY NAVIGATION OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public NavigationResult AttemptNavigation(Player player, NavigationContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogInformation(
            "Attempting navigation for PlayerId={PlayerId}, Destination={Destination}, Terrain={Terrain}",
            player.Id, context.Destination, context.TerrainType);

        _logger.LogDebug(
            "Navigation context details: HasCompass={HasCompass}, CompassEffective={CompassEffective}, " +
            "FamiliarTerritory={FamiliarTerritory}, Weather={Weather}, Night={Night}, TotalModifier={TotalModifier}",
            context.HasCompass, context.CompassEffective, context.FamiliarTerritory,
            context.WeatherConditions, context.IsNightWithoutLight, context.TotalDiceModifier);

        // Check if navigation can be attempted
        if (!CanNavigate(player, context))
        {
            var reason = GetNavigationBlockedReason(player, context);
            _logger.LogWarning(
                "Navigation blocked for PlayerId={PlayerId}: {Reason}",
                player.Id, reason);

            return NavigationResult.Empty() with
            {
                RollDetails = $"Navigation blocked: {reason}"
            };
        }

        // Build skill context and perform check
        var skillContext = context.ToSkillContext();
        var baseDc = context.BaseDc;

        _logger.LogDebug(
            "Performing Wasteland Survival check: BaseDC={BaseDC}, DiceModifier={DiceModifier}",
            baseDc, context.TotalDiceModifier);

        // Perform the navigation check using skill check service
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            baseDc,
            $"Navigation to {context.Destination} ({context.TerrainType.GetDisplayName()})",
            skillContext);

        var netSuccesses = checkResult.NetSuccesses;
        var totalSuccesses = checkResult.DiceResult.TotalSuccesses;
        var totalBotches = checkResult.DiceResult.TotalBotches;

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, TotalSuccesses={TotalSuccesses}, " +
            "TotalBotches={TotalBotches}, Outcome={Outcome}",
            netSuccesses, totalSuccesses, totalBotches, checkResult.Outcome);

        // Build roll details string
        var rollDetails = BuildRollDetails(checkResult, context);

        // Determine navigation outcome
        var navigationResult = DetermineOutcome(
            netSuccesses,
            totalSuccesses,
            totalBotches,
            baseDc,
            player,
            context,
            rollDetails);

        // Log the final result
        LogNavigationResult(player, context, navigationResult);

        return navigationResult;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool CanNavigate(Player player, NavigationContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        // Check for empty destination
        if (string.IsNullOrWhiteSpace(context.Destination))
        {
            _logger.LogDebug("Navigation blocked: No destination specified");
            return false;
        }

        // Check for active Disoriented fumble consequence
        // Note: This requires access to the player's active consequences
        // For now, we assume the player can navigate if they have a valid destination
        // The consequence system would block this at a higher level

        _logger.LogDebug(
            "Navigation allowed for PlayerId={PlayerId} to Destination={Destination}",
            player.Id, context.Destination);

        return true;
    }

    /// <inheritdoc/>
    public string? GetNavigationBlockedReason(Player player, NavigationContext context)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (string.IsNullOrWhiteSpace(context.Destination))
        {
            return "No destination specified";
        }

        // Check for active Disoriented fumble consequence would go here
        // Return null if navigation is allowed
        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DIFFICULTY CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int CalculateBaseDc(NavigationTerrainType terrainType)
    {
        var dc = terrainType switch
        {
            NavigationTerrainType.OpenWasteland => OpenWastelandDc,
            NavigationTerrainType.ModerateRuins => ModerateRuinsDc,
            NavigationTerrainType.DenseRuins => DenseRuinsDc,
            NavigationTerrainType.Labyrinthine => LabyrintheDc,
            NavigationTerrainType.GlitchedLabyrinth => GlitchedLabyrinthDc,
            _ => ModerateRuinsDc
        };

        _logger.LogDebug("Base DC for terrain {Terrain}: {DC}", terrainType, dc);

        return dc;
    }

    /// <inheritdoc/>
    public int CalculateDiceModifiers(NavigationContext context)
    {
        var total = context.TotalDiceModifier;

        _logger.LogDebug(
            "Dice modifiers calculated: Compass={Compass}, FamiliarTerritory={Familiar}, " +
            "Weather={Weather}, Night={Night}, Total={Total}",
            context.CompassModifier, context.FamiliarTerritoryModifier,
            context.WeatherModifier, context.NightPenalty, total);

        return total;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TERRAIN INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public (string DisplayName, string Description) GetTerrainDescription(NavigationTerrainType terrainType)
    {
        return terrainType switch
        {
            NavigationTerrainType.OpenWasteland => (
                "Open Wasteland",
                "Flat, exposed terrain with clear sightlines. Easy navigation but no cover. DC 8."),

            NavigationTerrainType.ModerateRuins => (
                "Moderate Ruins",
                "Partially collapsed urban areas with recognizable streets. Moderate navigation difficulty. DC 12."),

            NavigationTerrainType.DenseRuins => (
                "Dense Ruins",
                "Heavily damaged urban zones with blocked paths and unstable structures. Difficult navigation. DC 16."),

            NavigationTerrainType.Labyrinthine => (
                "Labyrinthine",
                "Complex maze of collapsed buildings and debris. Very difficult navigation. DC 20."),

            NavigationTerrainType.GlitchedLabyrinth => (
                "Glitched Labyrinth",
                "Reality-warped maze affected by Glitch corruption. Compasses fail. Extreme navigation. DC 24."),

            _ => ("Unknown Terrain", "Unknown terrain type.")
        };
    }

    /// <inheritdoc/>
    public float GetTerrainSpeedMultiplier(NavigationTerrainType terrainType)
    {
        return terrainType switch
        {
            NavigationTerrainType.OpenWasteland => OpenWastelandSpeedMultiplier,
            NavigationTerrainType.ModerateRuins => ModerateRuinsSpeedMultiplier,
            NavigationTerrainType.DenseRuins => DenseRuinsSpeedMultiplier,
            NavigationTerrainType.Labyrinthine => LabyrinthineSpeedMultiplier,
            NavigationTerrainType.GlitchedLabyrinth => GlitchedLabyrinthSpeedMultiplier,
            _ => ModerateRuinsSpeedMultiplier
        };
    }

    /// <inheritdoc/>
    public bool IsCompassEffective(NavigationTerrainType terrainType)
    {
        // Compasses do not work in glitch-corrupted terrain
        var effective = terrainType != NavigationTerrainType.GlitchedLabyrinth;

        _logger.LogDebug(
            "Compass effectiveness in {Terrain}: {Effective}",
            terrainType, effective);

        return effective;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WEATHER INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int GetWeatherModifier(WeatherType weather)
    {
        var modifier = weather switch
        {
            WeatherType.Clear => ClearWeatherModifier,
            WeatherType.Cloudy => CloudyWeatherModifier,
            WeatherType.LightRain => LightRainWeatherModifier,
            WeatherType.HeavyRain => HeavyRainWeatherModifier,
            WeatherType.Fog => FogWeatherModifier,
            WeatherType.Storm => StormWeatherModifier,
            _ => CloudyWeatherModifier
        };

        _logger.LogDebug("Weather modifier for {Weather}: {Modifier}", weather, modifier);

        return modifier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DANGEROUS AREA INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public (string DisplayName, string Description) GetDangerousAreaDescription(DangerousAreaType areaType)
    {
        return areaType switch
        {
            DangerousAreaType.HazardZone => (
                "Hazard Zone",
                "Area with environmental hazards: radiation, toxic chemicals, unstable structures, " +
                "or other dangers. Risk of injury and equipment damage."),

            DangerousAreaType.HostileTerritory => (
                "Hostile Territory",
                "Territory controlled by hostile forces: raiders, mutants, or territorial fauna. " +
                "Combat encounter likely."),

            DangerousAreaType.GlitchPocket => (
                "Glitch Pocket",
                "Area of concentrated reality distortion. Strange phenomena, navigation impossible, " +
                "and risk of Glitch corruption."),

            _ => ("Unknown Area", "Unknown dangerous area type.")
        };
    }

    /// <inheritdoc/>
    public DangerousAreaType RollDangerousAreaType()
    {
        // Roll 1d6: 1-2 = HazardZone, 3-4 = HostileTerritory, 5-6 = GlitchPocket
        var roll = _diceService.RollTotal("1d6");
        var areaType = DangerousAreaTypeExtensions.FromRoll(roll);

        _logger.LogDebug(
            "Dangerous area type roll: d6={Roll}, Result={AreaType}",
            roll, areaType);

        return areaType;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIME CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int CalculateActualTravelTime(int baseTimeMinutes, NavigationResult result)
    {
        var actualTime = (int)(baseTimeMinutes * result.TimeModifier);

        _logger.LogDebug(
            "Travel time calculation: Base={Base}min × {Modifier:F2} = {Actual}min",
            baseTimeMinutes, result.TimeModifier, actualTime);

        return actualTime;
    }

    /// <inheritdoc/>
    public float GetOutcomeTimeMultiplier(NavigationOutcome outcome)
    {
        return outcome switch
        {
            NavigationOutcome.Success => SuccessTimeMultiplier,
            NavigationOutcome.PartialSuccess => PartialSuccessTimeMultiplier,
            NavigationOutcome.Failure => FailureTimeMultiplier,
            NavigationOutcome.Fumble => FumbleTimeMultiplier,
            _ => SuccessTimeMultiplier
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the navigation outcome based on skill check results.
    /// </summary>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="totalSuccesses">Total raw successes rolled.</param>
    /// <param name="totalBotches">Total botches rolled.</param>
    /// <param name="targetDc">The target DC for navigation.</param>
    /// <param name="player">The player attempting navigation.</param>
    /// <param name="context">The navigation context.</param>
    /// <param name="rollDetails">Roll details string for logging.</param>
    /// <returns>A NavigationResult with the determined outcome.</returns>
    /// <remarks>
    /// <para>
    /// Outcome determination logic:
    /// <list type="bullet">
    ///   <item><description>Fumble: 0 successes AND at least 1 botch → Dangerous area</description></item>
    ///   <item><description>Success: Net successes ≥ DC → ×1.0 time</description></item>
    ///   <item><description>Partial Success: Net successes ≥ DC - 2 → ×1.25 time</description></item>
    ///   <item><description>Failure: Net successes &lt; DC - 2 → ×1.5 time</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private NavigationResult DetermineOutcome(
        int netSuccesses,
        int totalSuccesses,
        int totalBotches,
        int targetDc,
        Player player,
        NavigationContext context,
        string rollDetails)
    {
        // Check for fumble: 0 successes AND at least 1 botch
        if (totalSuccesses == 0 && totalBotches >= 1)
        {
            _logger.LogInformation(
                "Navigation fumble for PlayerId={PlayerId}: 0 successes with {Botches} botch(es)",
                player.Id, totalBotches);

            // Roll for dangerous area type
            var dangerousAreaType = RollDangerousAreaType();

            // Create the Disoriented fumble consequence
            var consequence = _fumbleConsequenceService.CreateConsequence(
                characterId: player.Id.ToString(),
                skillId: WastelandSurvivalSkillId,
                fumbleType: FumbleType.Disoriented,
                targetId: null,
                description: $"Got critically lost navigating to {context.Destination} and " +
                           $"stumbled into a {dangerousAreaType.GetDisplayName()}");

            _logger.LogInformation(
                "Created Disoriented consequence for PlayerId={PlayerId}, DangerousArea={AreaType}",
                player.Id, dangerousAreaType);

            return NavigationResult.Fumble(
                netSuccesses,
                targetDc,
                dangerousAreaType,
                rollDetails);
        }

        // Check for success: net successes >= DC
        if (netSuccesses >= targetDc)
        {
            _logger.LogDebug(
                "Navigation success: {NetSuccesses} >= DC {TargetDc}",
                netSuccesses, targetDc);

            return NavigationResult.Success(netSuccesses, targetDc, rollDetails);
        }

        // Check for partial success: net successes >= DC - 2
        if (netSuccesses >= targetDc - PartialSuccessMargin)
        {
            _logger.LogDebug(
                "Navigation partial success: {NetSuccesses} >= DC {TargetDc} - {Margin}",
                netSuccesses, targetDc, PartialSuccessMargin);

            return NavigationResult.PartialSuccess(netSuccesses, targetDc, rollDetails);
        }

        // Otherwise, failure
        _logger.LogDebug(
            "Navigation failure: {NetSuccesses} < DC {TargetDc} - {Margin}",
            netSuccesses, targetDc, PartialSuccessMargin);

        return NavigationResult.Failure(netSuccesses, targetDc, rollDetails);
    }

    /// <summary>
    /// Builds a human-readable roll details string for the navigation result.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="context">The navigation context.</param>
    /// <returns>A formatted string describing the roll details.</returns>
    private static string BuildRollDetails(SkillCheckResult checkResult, NavigationContext context)
    {
        var compassStr = context.HasCompass
            ? (context.CompassEffective ? $"+{NavigationContext.CompassBonusDice}" : "Ineffective")
            : "None";

        var familiarStr = context.FamiliarTerritory
            ? $"+{NavigationContext.FamiliarTerritoryBonusDice}"
            : "0";

        var weatherStr = context.WeatherModifier >= 0
            ? $"+{context.WeatherModifier}"
            : context.WeatherModifier.ToString();

        var nightStr = context.IsNightWithoutLight
            ? $"-{NavigationContext.NightWithoutLightPenalty}"
            : "0";

        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"Compass: {compassStr}, Familiar: {familiarStr}, " +
               $"Weather: {weatherStr}, Night: {nightStr} | " +
               $"DC: {context.BaseDc} ({context.TerrainType.GetDisplayName()})";
    }

    /// <summary>
    /// Logs the final navigation result at the appropriate level.
    /// </summary>
    /// <param name="player">The player who attempted navigation.</param>
    /// <param name="context">The navigation context.</param>
    /// <param name="result">The navigation result.</param>
    private void LogNavigationResult(Player player, NavigationContext context, NavigationResult result)
    {
        switch (result.Outcome)
        {
            case NavigationOutcome.Success:
                _logger.LogInformation(
                    "Navigation successful for PlayerId={PlayerId} to {Destination}: " +
                    "{NetSuccesses} successes vs DC {TargetDc}",
                    player.Id, context.Destination, result.NetSuccesses, result.TargetDc);
                break;

            case NavigationOutcome.PartialSuccess:
                _logger.LogInformation(
                    "Navigation partial success for PlayerId={PlayerId} to {Destination}: " +
                    "{NetSuccesses} successes vs DC {TargetDc}, +25% travel time",
                    player.Id, context.Destination, result.NetSuccesses, result.TargetDc);
                break;

            case NavigationOutcome.Failure:
                _logger.LogInformation(
                    "Navigation failed for PlayerId={PlayerId} to {Destination}: " +
                    "{NetSuccesses} successes vs DC {TargetDc}, +50% travel time (got lost)",
                    player.Id, context.Destination, result.NetSuccesses, result.TargetDc);
                break;

            case NavigationOutcome.Fumble:
                _logger.LogWarning(
                    "Navigation fumble for PlayerId={PlayerId} to {Destination}: " +
                    "Entered {DangerousArea}! Player is now Disoriented.",
                    player.Id, context.Destination, result.DangerousAreaType?.GetDisplayName() ?? "unknown area");
                break;

            default:
                _logger.LogDebug(
                    "Navigation result for PlayerId={PlayerId}: {Outcome}",
                    player.Id, result.Outcome);
                break;
        }
    }
}
