using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing light sources and vision mechanics.
/// </summary>
public interface ILightService
{
    /// <summary>
    /// Activates a light source from an item.
    /// </summary>
    /// <param name="player">The player activating the light.</param>
    /// <param name="lightItem">The item to light.</param>
    /// <returns>Result of the activation attempt.</returns>
    LightResult ActivateLight(Player player, Item lightItem);

    /// <summary>
    /// Deactivates the player's active light source.
    /// </summary>
    /// <param name="player">The player extinguishing their light.</param>
    /// <returns>Result of the deactivation attempt.</returns>
    LightResult DeactivateLight(Player player);

    /// <summary>
    /// Refuels a fuel-based light source.
    /// </summary>
    /// <param name="player">The player refueling.</param>
    /// <param name="lantern">The lantern item to refuel.</param>
    /// <param name="fuelItem">The fuel item to consume.</param>
    /// <returns>Result of the refuel attempt.</returns>
    RefuelResult RefuelLantern(Player player, Item lantern, Item fuelItem);

    /// <summary>
    /// Gets the effective light level for a player considering vision type.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="room">The room.</param>
    /// <returns>The effective light level.</returns>
    LightLevel GetEffectiveLightLevel(Player player, Room room);

    /// <summary>
    /// Gets the effective light level for a monster considering vision type.
    /// </summary>
    /// <param name="monster">The monster.</param>
    /// <param name="room">The room.</param>
    /// <returns>The effective light level.</returns>
    LightLevel GetEffectiveLightLevel(Monster monster, Room room);

    /// <summary>
    /// Processes all light sources at end of turn, consuming duration/fuel.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="room">The current room.</param>
    /// <returns>Any light sources that expired this turn.</returns>
    IEnumerable<LightSourceExpiredResult> ProcessLightSources(Player player, Room room);

    /// <summary>
    /// Gets the light sensitivity penalty for a monster in the current light.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <param name="room">The room.</param>
    /// <returns>The accuracy penalty (0 or negative) for light-sensitive creatures.</returns>
    int GetLightSensitivityPenalty(Monster monster, Room room);
}

/// <summary>
/// Result of activating or deactivating a light source.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="LightSource">The light source involved, if any.</param>
/// <param name="Message">A message describing the result.</param>
public readonly record struct LightResult(bool Success, LightSource? LightSource, string Message);

/// <summary>
/// Result of refueling a light source.
/// </summary>
/// <param name="Success">Whether the refuel succeeded.</param>
/// <param name="FuelAdded">Amount of fuel added.</param>
/// <param name="CurrentFuel">Current fuel level after refuel.</param>
/// <param name="MaxFuel">Maximum fuel capacity.</param>
/// <param name="Message">A message describing the result.</param>
public readonly record struct RefuelResult(bool Success, int FuelAdded, int CurrentFuel, int MaxFuel, string Message);

/// <summary>
/// Result indicating a light source has expired.
/// </summary>
/// <param name="LightSource">The light source that expired.</param>
/// <param name="Message">A message describing the expiration.</param>
public readonly record struct LightSourceExpiredResult(LightSource LightSource, string Message);
