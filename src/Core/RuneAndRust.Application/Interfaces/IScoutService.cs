using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for the scout action in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to perform reconnaissance on adjacent rooms
/// using the Wasteland Survival skill. Scouting reveals information about enemies,
/// hazards, and points of interest before the player commits to entering a room.
/// </para>
/// <para>
/// Room Revelation Formula:
/// <code>
/// if (netSuccesses >= 0):
///     roomsRevealed = 1 + (netSuccesses / 2)
/// else:
///     roomsRevealed = 0
///
/// roomsRevealed = min(roomsRevealed, adjacentRoomCount)
/// </code>
/// </para>
/// <para>
/// DC Calculation:
/// <list type="bullet">
///   <item><description>Base DC: From terrain type (8, 12, 16, 20, or 24)</description></item>
///   <item><description>Visibility modifier: Added to DC (positive = harder)</description></item>
/// </list>
/// </para>
/// <para>
/// Base DCs by terrain:
/// <list type="bullet">
///   <item><description>OpenWasteland: DC 8 (clear sightlines)</description></item>
///   <item><description>ModerateRuins: DC 12 (partial cover)</description></item>
///   <item><description>DenseRuins: DC 16 (many hiding spots)</description></item>
///   <item><description>Labyrinthine: DC 20 (twisting passages)</description></item>
///   <item><description>GlitchedLabyrinth: DC 24 (reality distortions)</description></item>
/// </list>
/// </para>
/// <para>
/// Room revelation scaling:
/// <list type="bullet">
///   <item><description>Net 0-1: 1 room (base success)</description></item>
///   <item><description>Net 2-3: 2 rooms</description></item>
///   <item><description>Net 4-5: 3 rooms</description></item>
///   <item><description>Net 6+: 4+ rooms (capped at adjacent count)</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IScoutService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTING OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs a scout action to reveal adjacent rooms.
    /// </summary>
    /// <param name="player">The player performing the scout.</param>
    /// <param name="context">Context containing terrain and adjacent room information.</param>
    /// <returns>
    /// A <see cref="ScoutResult"/> containing revealed rooms and their contents.
    /// On success, includes enemies, hazards, and points of interest for each revealed room.
    /// On failure, no rooms are revealed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a Wasteland Survival skill check against the calculated DC.
    /// DC = Base DC (from terrain type) + Visibility modifier.
    /// </para>
    /// <para>
    /// Rooms revealed = 1 + (netSuccesses / 2), capped at adjacent room count.
    /// On failure (net &lt; 0), no rooms are revealed.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var context = ScoutContext.Create(player.Id, currentRoom.Id,
    ///     NavigationTerrainType.ModerateRuins, adjacentRoomIds);
    /// var result = scoutService.PerformScout(player, context);
    /// if (result.ScoutSucceeded)
    /// {
    ///     foreach (var room in result.RoomsRevealed)
    ///     {
    ///         Console.WriteLine(room.ToDisplayString());
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    ScoutResult PerformScout(Player player, ScoutContext context);

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base DC for scouting based on terrain type.
    /// </summary>
    /// <param name="terrainType">The navigation terrain type.</param>
    /// <returns>The base difficulty class for scouting in this terrain.</returns>
    /// <remarks>
    /// Base DCs by terrain:
    /// <list type="bullet">
    ///   <item><description>OpenWasteland: DC 8</description></item>
    ///   <item><description>ModerateRuins: DC 12</description></item>
    ///   <item><description>DenseRuins: DC 16</description></item>
    ///   <item><description>Labyrinthine: DC 20</description></item>
    ///   <item><description>GlitchedLabyrinth: DC 24</description></item>
    /// </list>
    /// </remarks>
    int GetBaseDc(NavigationTerrainType terrainType);

    /// <summary>
    /// Gets the full scout DC with visibility modifier applied.
    /// </summary>
    /// <param name="terrainType">The navigation terrain type.</param>
    /// <param name="visibilityModifier">Modifier from visibility conditions.</param>
    /// <returns>The calculated difficulty class (minimum 1).</returns>
    /// <remarks>
    /// <para>
    /// DC calculation:
    /// <code>Final DC = Base DC + Visibility Modifier (minimum 1)</code>
    /// </para>
    /// <para>
    /// Visibility modifiers:
    /// <list type="bullet">
    ///   <item><description>Excellent: -2</description></item>
    ///   <item><description>Good: -1</description></item>
    ///   <item><description>Normal: 0</description></item>
    ///   <item><description>Poor: +2</description></item>
    ///   <item><description>Terrible: +4</description></item>
    ///   <item><description>Static storm: +6</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    int GetScoutDc(NavigationTerrainType terrainType, int visibilityModifier);

    /// <summary>
    /// Calculates how many rooms are revealed based on net successes.
    /// </summary>
    /// <param name="netSuccesses">Number of successes above DC.</param>
    /// <param name="adjacentRoomCount">Maximum rooms available for scouting.</param>
    /// <returns>Number of rooms to reveal.</returns>
    /// <remarks>
    /// <para>
    /// Formula: 1 + (netSuccesses / 2), capped at adjacent room count.
    /// </para>
    /// <para>
    /// Failure (net &lt; 0) reveals 0 rooms.
    /// </para>
    /// </remarks>
    int CalculateRoomsRevealed(int netSuccesses, int adjacentRoomCount);

    // ═══════════════════════════════════════════════════════════════════════════
    // ROOM INFORMATION GATHERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Detects enemies in a specific room.
    /// </summary>
    /// <param name="room">The room to check.</param>
    /// <returns>List of detected enemies with type, count, threat level, and position.</returns>
    IReadOnlyList<DetectedEnemy> DetectEnemies(Room room);

    /// <summary>
    /// Detects hazards in a specific room.
    /// </summary>
    /// <param name="room">The room to check.</param>
    /// <returns>List of detected hazards with type, severity, and avoidance hints.</returns>
    IReadOnlyList<DetectedHazard> DetectHazards(Room room);

    /// <summary>
    /// Detects points of interest in a specific room.
    /// </summary>
    /// <param name="room">The room to check.</param>
    /// <returns>List of points of interest with type, description, and interaction hints.</returns>
    IReadOnlyList<PointOfInterest> DetectPointsOfInterest(Room room);

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name for a terrain type.
    /// </summary>
    /// <param name="terrainType">The terrain type.</param>
    /// <returns>A human-readable display name.</returns>
    string GetTerrainDisplayName(NavigationTerrainType terrainType);

    /// <summary>
    /// Gets the description of scouting difficulty for a terrain type.
    /// </summary>
    /// <param name="terrainType">The terrain type.</param>
    /// <returns>A description of how terrain affects scouting.</returns>
    string GetTerrainScoutingDescription(NavigationTerrainType terrainType);

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTING PREREQUISITES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether the player can attempt scouting.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The scouting context.</param>
    /// <returns>True if the player can attempt scouting; otherwise, false.</returns>
    /// <remarks>
    /// Scouting may be blocked by:
    /// <list type="bullet">
    ///   <item><description>No adjacent rooms to scout</description></item>
    ///   <item><description>Active Blinded status effect</description></item>
    ///   <item><description>Insufficient light level</description></item>
    ///   <item><description>Other incapacitating conditions</description></item>
    /// </list>
    /// </remarks>
    bool CanScout(Player player, ScoutContext context);

    /// <summary>
    /// Gets the reason why scouting is blocked, if any.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="context">The scouting context.</param>
    /// <returns>A human-readable reason why scouting is blocked, or null if allowed.</returns>
    string? GetScoutBlockedReason(Player player, ScoutContext context);
}
