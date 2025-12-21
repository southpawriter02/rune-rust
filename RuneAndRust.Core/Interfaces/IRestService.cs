using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rest and recovery operations.
/// Handles resource consumption, HP/Stamina/Stress recovery, and status effect management.
/// </summary>
public interface IRestService
{
    /// <summary>
    /// Performs a rest action for the character.
    /// Consumes supplies (Wilderness only), applies recovery formulas, and manages Exhausted status.
    /// Does not perform ambush checks - use the overload with Room parameter for wilderness rest with ambush.
    /// </summary>
    /// <param name="character">The character performing the rest.</param>
    /// <param name="type">The type of rest (Sanctuary or Wilderness).</param>
    /// <returns>A result containing recovery deltas and status information.</returns>
    Task<RestResult> PerformRestAsync(CharacterEntity character, RestType type);

    /// <summary>
    /// Performs a rest action with ambush check for wilderness rest.
    /// For Sanctuary rest, the room parameter is used only for logging.
    /// For Wilderness rest, an ambush check is performed before recovery.
    /// </summary>
    /// <param name="character">The character performing the rest.</param>
    /// <param name="type">The type of rest (Sanctuary or Wilderness).</param>
    /// <param name="room">The room where rest is attempted. Required for ambush calculation.</param>
    /// <returns>A result containing recovery deltas, status, and ambush details.</returns>
    Task<RestResult> PerformRestAsync(CharacterEntity character, RestType type, Room room);

    /// <summary>
    /// Checks if the character has the required supplies for a wilderness rest.
    /// Requires both a "Ration" tagged item and a "Water" tagged item.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>True if supplies are available; false otherwise.</returns>
    Task<bool> HasRequiredSuppliesAsync(CharacterEntity character);
}
