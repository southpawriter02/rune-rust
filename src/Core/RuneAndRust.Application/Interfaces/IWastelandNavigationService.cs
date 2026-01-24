using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for wasteland navigation operations in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to navigate through the post-collapse landscape
/// using the Wasteland Survival skill. Navigation difficulty is based on terrain type,
/// equipment, familiarity, and environmental conditions.
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
public interface IWastelandNavigationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY NAVIGATION OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to navigate to a destination through the specified terrain.
    /// </summary>
    /// <param name="player">The character attempting navigation.</param>
    /// <param name="context">The navigation context with terrain and conditions.</param>
    /// <returns>
    /// A <see cref="NavigationResult"/> containing the outcome, time modifier,
    /// and any hazard information if a fumble occurred.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a Wasteland Survival skill check against the terrain's base DC,
    /// modified by equipment, familiarity, and environmental conditions.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var context = NavigationContext.CreateWithBonuses(
    ///     destination: "Sector 7",
    ///     terrainType: NavigationTerrainType.DenseRuins,
    ///     hasCompass: true,
    ///     familiarTerritory: false);
    /// var result = navigationService.AttemptNavigation(player, context);
    /// if (result.ReachedDestination)
    /// {
    ///     Console.WriteLine($"Arrived with {result.TimeModifier:P0} time modifier");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    NavigationResult AttemptNavigation(Player player, NavigationContext context);

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether the player can attempt navigation.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The navigation context.</param>
    /// <returns>True if navigation can be attempted; otherwise, false.</returns>
    /// <remarks>
    /// Navigation may be blocked by:
    /// <list type="bullet">
    ///   <item><description>Active Disoriented fumble consequence</description></item>
    ///   <item><description>Missing or invalid destination</description></item>
    ///   <item><description>Other blocking conditions</description></item>
    /// </list>
    /// </remarks>
    bool CanNavigate(Player player, NavigationContext context);

    /// <summary>
    /// Gets the reason why navigation is blocked, if any.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The navigation context.</param>
    /// <returns>A human-readable reason why navigation is blocked, or null if allowed.</returns>
    string? GetNavigationBlockedReason(Player player, NavigationContext context);

    // ═══════════════════════════════════════════════════════════════════════════
    // DIFFICULTY CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the base DC for a given terrain type.
    /// </summary>
    /// <param name="terrainType">The terrain type being navigated.</param>
    /// <returns>The base difficulty class for navigation.</returns>
    /// <remarks>
    /// Terrain DCs:
    /// <list type="bullet">
    ///   <item><description>OpenWasteland: DC 8</description></item>
    ///   <item><description>ModerateRuins: DC 12</description></item>
    ///   <item><description>DenseRuins: DC 16</description></item>
    ///   <item><description>Labyrinthine: DC 20</description></item>
    ///   <item><description>GlitchedLabyrinth: DC 24</description></item>
    /// </list>
    /// </remarks>
    int CalculateBaseDc(NavigationTerrainType terrainType);

    /// <summary>
    /// Calculates the total dice modifier for a navigation context.
    /// </summary>
    /// <param name="context">The navigation context with all conditions.</param>
    /// <returns>The total dice modifier (can be negative).</returns>
    /// <remarks>
    /// <para>
    /// Modifiers stack additively:
    /// <list type="bullet">
    ///   <item><description>Working compass: +1d10 (not in glitched terrain)</description></item>
    ///   <item><description>Familiar territory: +2d10</description></item>
    ///   <item><description>Weather: +1 (clear) to -4 (storm)</description></item>
    ///   <item><description>Night (no light): -2d10</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int CalculateDiceModifiers(NavigationContext context);

    // ═══════════════════════════════════════════════════════════════════════════
    // TERRAIN INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a human-readable description of a terrain type.
    /// </summary>
    /// <param name="terrainType">The terrain type.</param>
    /// <returns>A tuple of (DisplayName, Description) for the terrain.</returns>
    (string DisplayName, string Description) GetTerrainDescription(NavigationTerrainType terrainType);

    /// <summary>
    /// Gets the travel speed multiplier for a terrain type.
    /// </summary>
    /// <param name="terrainType">The terrain type.</param>
    /// <returns>A multiplier applied to base travel speed (1.0 = normal).</returns>
    float GetTerrainSpeedMultiplier(NavigationTerrainType terrainType);

    /// <summary>
    /// Checks if compasses are effective in the specified terrain.
    /// </summary>
    /// <param name="terrainType">The terrain type to check.</param>
    /// <returns>True if compasses function normally; false in glitched terrain.</returns>
    bool IsCompassEffective(NavigationTerrainType terrainType);

    // ═══════════════════════════════════════════════════════════════════════════
    // WEATHER INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the dice modifier for a weather condition.
    /// </summary>
    /// <param name="weather">The weather type.</param>
    /// <returns>The dice modifier (positive for clear, negative for bad weather).</returns>
    /// <remarks>
    /// Weather modifiers:
    /// <list type="bullet">
    ///   <item><description>Clear: +1d10</description></item>
    ///   <item><description>Cloudy: +0</description></item>
    ///   <item><description>LightRain: -1d10</description></item>
    ///   <item><description>HeavyRain: -2d10</description></item>
    ///   <item><description>Fog: -3d10</description></item>
    ///   <item><description>Storm: -4d10</description></item>
    /// </list>
    /// </remarks>
    int GetWeatherModifier(WeatherType weather);

    // ═══════════════════════════════════════════════════════════════════════════
    // DANGEROUS AREA INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a description of a dangerous area type.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>A tuple of (DisplayName, Description) for the dangerous area.</returns>
    (string DisplayName, string Description) GetDangerousAreaDescription(DangerousAreaType areaType);

    /// <summary>
    /// Rolls to determine what type of dangerous area is entered on a fumble.
    /// </summary>
    /// <returns>The type of dangerous area entered.</returns>
    /// <remarks>
    /// Rolls d6:
    /// <list type="bullet">
    ///   <item><description>1-2: HazardZone</description></item>
    ///   <item><description>3-4: HostileTerritory</description></item>
    ///   <item><description>5-6: GlitchPocket</description></item>
    /// </list>
    /// </remarks>
    DangerousAreaType RollDangerousAreaType();

    // ═══════════════════════════════════════════════════════════════════════════
    // TIME CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the actual travel time based on base time and result.
    /// </summary>
    /// <param name="baseTimeMinutes">The base travel time in minutes.</param>
    /// <param name="result">The navigation result with time modifier.</param>
    /// <returns>The actual travel time in minutes after applying modifiers.</returns>
    int CalculateActualTravelTime(int baseTimeMinutes, NavigationResult result);

    /// <summary>
    /// Gets the time multiplier for a navigation outcome.
    /// </summary>
    /// <param name="outcome">The navigation outcome.</param>
    /// <returns>The time multiplier (1.0, 1.25, or 1.5).</returns>
    float GetOutcomeTimeMultiplier(NavigationOutcome outcome);
}
