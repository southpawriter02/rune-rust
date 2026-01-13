using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing environmental hazard zones and their effects.
/// </summary>
/// <remarks>
/// This service handles the processing of hazard zone effects including:
/// <list type="bullet">
///   <item><description>Entry damage when players enter a hazardous room</description></item>
///   <item><description>Per-turn damage while players remain in hazardous areas</description></item>
///   <item><description>Saving throw rolls to reduce or negate effects</description></item>
///   <item><description>Status effect application on failed saves</description></item>
///   <item><description>Duration countdown for temporary hazards</description></item>
/// </list>
/// </remarks>
/// <seealso cref="HazardZone"/>
/// <seealso cref="SavingThrow"/>
public interface IHazardZoneService
{
    /// <summary>
    /// Gets a description of active hazards in the room for display.
    /// </summary>
    /// <param name="room">The room to describe hazards for.</param>
    /// <returns>A formatted description of active hazards, or empty string if none.</returns>
    string GetRoomHazardsDescription(Room room);

    /// <summary>
    /// Finds a hazard zone in a room by keyword.
    /// </summary>
    /// <param name="room">The room to search.</param>
    /// <param name="keyword">The keyword to match.</param>
    /// <returns>The matching hazard zone, or null if not found.</returns>
    HazardZone? FindHazard(Room room, string keyword);

    /// <summary>
    /// Gets detailed examination information for a hazard.
    /// </summary>
    /// <param name="hazard">The hazard to examine.</param>
    /// <returns>A detailed description of the hazard.</returns>
    string ExamineHazard(HazardZone hazard);

    /// <summary>
    /// Processes entry hazards when a player enters a room.
    /// </summary>
    /// <param name="room">The room being entered.</param>
    /// <param name="player">The player entering the room.</param>
    /// <returns>Results for each hazard that dealt entry damage.</returns>
    IEnumerable<HazardEffectResult> ProcessEntryHazards(Room room, Player player);

    /// <summary>
    /// Processes per-turn hazard effects.
    /// </summary>
    /// <param name="room">The room the player is in.</param>
    /// <param name="player">The player affected by hazards.</param>
    /// <returns>Results for each hazard that dealt turn damage.</returns>
    IEnumerable<HazardEffectResult> ProcessTurnHazards(Room room, Player player);

    /// <summary>
    /// Applies a hazard's effect to a player.
    /// </summary>
    /// <param name="hazard">The hazard zone.</param>
    /// <param name="player">The target player.</param>
    /// <param name="isEntry">True if this is entry damage, false for per-turn.</param>
    /// <returns>The result of applying the effect.</returns>
    HazardEffectResult ApplyHazardEffect(HazardZone hazard, Player player, bool isEntry);

    /// <summary>
    /// Processes turn ticks for all hazards in a room, expiring temporary hazards.
    /// </summary>
    /// <param name="room">The room to process.</param>
    /// <returns>Messages for any expired hazards.</returns>
    IEnumerable<string> ProcessHazardTurnTicks(Room room);

    /// <summary>
    /// Gets a summary of hazard zones in a room.
    /// </summary>
    /// <param name="room">The room to summarize.</param>
    /// <returns>A summary of hazard counts and states.</returns>
    HazardRoomSummary GetRoomHazardSummary(Room room);

    /// <summary>
    /// Performs a saving throw for a player against a save specification.
    /// </summary>
    /// <param name="player">The player making the save.</param>
    /// <param name="save">The saving throw configuration.</param>
    /// <returns>The result of the saving throw.</returns>
    SavingThrowResult PerformSavingThrow(Player player, SavingThrow save);
}

/// <summary>
/// Summary of hazard zones in a room.
/// </summary>
/// <param name="Total">Total number of hazard zones.</param>
/// <param name="Active">Number of currently active hazards.</param>
/// <param name="Permanent">Number of permanent hazards.</param>
/// <param name="Temporary">Number of temporary hazards.</param>
/// <param name="Expired">Number of expired hazards (pending cleanup).</param>
public readonly record struct HazardRoomSummary(
    int Total,
    int Active,
    int Permanent,
    int Temporary,
    int Expired);

/// <summary>
/// Result of a saving throw roll.
/// </summary>
/// <param name="Total">The total roll (d20 + modifier).</param>
/// <param name="Rolls">The individual dice rolls.</param>
/// <param name="Modifier">The attribute modifier applied.</param>
/// <param name="DC">The Difficulty Class that was rolled against.</param>
/// <param name="Success">Whether the save succeeded.</param>
/// <param name="Attribute">The attribute used for the save.</param>
public readonly record struct SavingThrowResult(
    int Total,
    IReadOnlyList<int> Rolls,
    int Modifier,
    int DC,
    bool Success,
    string Attribute);
