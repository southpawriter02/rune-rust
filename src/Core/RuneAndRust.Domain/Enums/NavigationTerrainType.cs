namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Terrain type classifications for navigation and tracking purposes.
/// </summary>
/// <remarks>
/// <para>
/// Defines terrain categories that affect tracking check frequency,
/// navigation difficulty, and exploration mechanics. These classifications
/// represent the overall character of an area rather than individual grid cells.
/// </para>
/// <para>
/// Distinct from <see cref="TerrainType"/> which is used for combat grid cells
/// and movement costs. NavigationTerrainType is used for overland travel,
/// tracking pursuits, and exploration at a higher level of abstraction.
/// </para>
/// <para>
/// Tracking check frequency by terrain:
/// <list type="bullet">
///   <item><description>OpenWasteland: Every 2 miles</description></item>
///   <item><description>ModerateRuins: Every 1 mile</description></item>
///   <item><description>DenseRuins: Every 0.5 miles</description></item>
///   <item><description>Labyrinthine: Every room/intersection (~0.1 miles)</description></item>
///   <item><description>GlitchedLabyrinth: Every room/intersection (~0.1 miles)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum NavigationTerrainType
{
    /// <summary>
    /// Open wasteland with clear lines of sight.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Flat or gently rolling terrain with minimal obstructions.
    /// Examples: ash plains, salt flats, scrubland, desert.
    /// </para>
    /// <para>
    /// Navigation DC: 8. Tracking check interval: 2 miles.
    /// Tracks are visible at distance but degrade faster from wind.
    /// </para>
    /// </remarks>
    OpenWasteland = 0,

    /// <summary>
    /// Moderate ruins with intermittent cover.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Partially collapsed structures with navigable paths.
    /// Examples: suburban ruins, collapsed factories, broken highways.
    /// </para>
    /// <para>
    /// Navigation DC: 12. Tracking check interval: 1 mile.
    /// Tracks are protected from elements but multiple paths exist.
    /// </para>
    /// </remarks>
    ModerateRuins = 1,

    /// <summary>
    /// Dense ruins with limited visibility.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Heavily damaged urban environment with many obstacles.
    /// Examples: city centers, industrial districts, collapsed tunnels.
    /// </para>
    /// <para>
    /// Navigation DC: 16. Tracking check interval: 0.5 miles.
    /// Many possible routes require frequent reconfirmation of trail.
    /// </para>
    /// </remarks>
    DenseRuins = 2,

    /// <summary>
    /// Labyrinthine terrain with complex pathways.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maze-like environment requiring constant navigation.
    /// Examples: underground complexes, intact buildings, cave systems.
    /// </para>
    /// <para>
    /// Navigation DC: 20. Tracking check interval: Per room/intersection.
    /// Each junction point requires a new tracking check.
    /// </para>
    /// </remarks>
    Labyrinthine = 3,

    /// <summary>
    /// Glitched labyrinthine terrain with reality distortions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unstable environment affected by Glitch corruption.
    /// Examples: Glitch-corrupted sectors, unstable reality zones.
    /// </para>
    /// <para>
    /// Navigation DC: 24. Tracking check interval: Per room/intersection.
    /// Glitch effects may alter paths, making tracking extremely difficult.
    /// This terrain type may impose additional penalties from Glitch mechanics.
    /// </para>
    /// </remarks>
    GlitchedLabyrinth = 4
}
