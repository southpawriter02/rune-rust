using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing light sources and vision mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Handles light source lifecycle:
/// <list type="bullet">
///   <item><description>Activation/deactivation of torches and lanterns</description></item>
///   <item><description>Fuel consumption and refueling</description></item>
///   <item><description>Vision type mitigation of light penalties</description></item>
///   <item><description>Light sensitivity for creatures in bright light</description></item>
/// </list>
/// </para>
/// </remarks>
public class LightService : ILightService
{
    private readonly ILogger<LightService> _logger;

    /// <summary>
    /// Default duration for a torch in turns.
    /// </summary>
    public const int DefaultTorchDuration = 10;

    /// <summary>
    /// Default max fuel for a lantern.
    /// </summary>
    public const int DefaultLanternMaxFuel = 100;

    /// <summary>
    /// Default fuel amount added per refuel action.
    /// </summary>
    public const int DefaultRefuelAmount = 50;

    /// <summary>
    /// Creates a new light service.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public LightService(ILogger<LightService>? logger = null)
    {
        _logger = logger ?? NullLogger<LightService>.Instance;
        _logger.LogDebug("LightService initialized");
    }

    /// <inheritdoc />
    public LightResult ActivateLight(Player player, Item lightItem)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(lightItem);

        // Check if player already has an active light
        if (player.ActiveLightSource != null)
        {
            return new LightResult(false, null, "You already have an active light source.");
        }

        // Determine light source type based on item
        LightSource lightSource;
        if (lightItem.Name.Contains("torch", StringComparison.OrdinalIgnoreCase))
        {
            lightSource = LightSource.CreateTorch(DefaultTorchDuration, lightItem.Id, player.Id);
        }
        else if (lightItem.Name.Contains("lantern", StringComparison.OrdinalIgnoreCase))
        {
            lightSource = LightSource.CreateLantern(DefaultLanternMaxFuel, lightItem.Id, player.Id);
        }
        else
        {
            // Generic light source
            lightSource = LightSource.Create(
                "light", lightItem.Name, LightLevel.Bright,
                isPortable: true, duration: DefaultTorchDuration,
                associatedItemId: lightItem.Id, ownerId: player.Id);
        }

        if (!lightSource.Activate())
        {
            _logger.LogWarning("Failed to activate light source {Light} for player {Player}",
                lightSource.Name, player.Name);
            return new LightResult(false, null, "Cannot light this item.");
        }

        player.SetActiveLightSource(lightSource);

        _logger.LogInformation("Player {Player} lit {Light}",
            player.Name, lightSource.Name);

        var durationMsg = lightSource.HasDuration
            ? $" ({lightSource.RemainingDuration} turns remaining)"
            : lightSource.UsesFuel
                ? $" ({lightSource.GetFuelPercentage()}% fuel)"
                : "";

        return new LightResult(true, lightSource, $"You light the {lightSource.Name}.{durationMsg}");
    }

    /// <inheritdoc />
    public LightResult DeactivateLight(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.ActiveLightSource == null)
        {
            return new LightResult(false, null, "You have no active light source.");
        }

        var source = player.ActiveLightSource;
        source.Deactivate();
        player.SetActiveLightSource(null);

        _logger.LogInformation("Player {Player} extinguished {Light}",
            player.Name, source.Name);

        return new LightResult(true, source, $"You extinguish the {source.Name}.");
    }

    /// <inheritdoc />
    public RefuelResult RefuelLantern(Player player, Item lantern, Item fuelItem)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(lantern);
        ArgumentNullException.ThrowIfNull(fuelItem);

        if (player.ActiveLightSource == null)
        {
            return new RefuelResult(false, 0, 0, 0, "You have no active light source.");
        }

        if (player.ActiveLightSource.AssociatedItemId != lantern.Id)
        {
            return new RefuelResult(false, 0, 0, 0, "That is not your active light source.");
        }

        var source = player.ActiveLightSource;

        if (!source.UsesFuel)
        {
            return new RefuelResult(false, 0, 0, 0, $"The {source.Name} doesn't use fuel.");
        }

        var fuelAdded = source.Refuel(DefaultRefuelAmount);

        _logger.LogInformation("Refueled {Light} with {Fuel} fuel for player {Player}",
            source.Name, fuelAdded, player.Name);

        return new RefuelResult(
            true, fuelAdded, source.CurrentFuel, source.MaxFuel,
            $"Added {fuelAdded} fuel to {source.Name}. ({source.GetFuelPercentage()}% full)");
    }

    /// <inheritdoc />
    public LightLevel GetEffectiveLightLevel(Player player, Room room)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);

        return player.GetEffectiveLightLevel(room);
    }

    /// <inheritdoc />
    public LightLevel GetEffectiveLightLevel(Monster monster, Room room)
    {
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(room);

        return monster.GetEffectiveLightLevel(room);
    }

    /// <inheritdoc />
    public IEnumerable<LightSourceExpiredResult> ProcessLightSources(Player player, Room room)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(room);

        var expired = new List<LightSourceExpiredResult>();

        // Process player's active light source
        if (player.ActiveLightSource != null && player.ActiveLightSource.Tick())
        {
            var source = player.ActiveLightSource;
            var msg = source.UsesFuel
                ? $"Your {source.Name} runs out of fuel!"
                : $"Your {source.Name} burns out!";

            expired.Add(new LightSourceExpiredResult(source, msg));
            player.SetActiveLightSource(null);

            _logger.LogInformation("{Light} expired for player {Player}",
                source.Name, player.Name);
        }

        // Process room's light sources
        foreach (var source in room.LightSources.Where(ls => ls.IsActive).ToList())
        {
            if (source.Tick())
            {
                expired.Add(new LightSourceExpiredResult(source, $"The {source.Name} goes dark."));

                _logger.LogDebug("Room light source {Light} expired in {Room}",
                    source.Name, room.Name);
            }
        }

        return expired;
    }

    /// <inheritdoc />
    public int GetLightSensitivityPenalty(Monster monster, Room room)
    {
        ArgumentNullException.ThrowIfNull(monster);
        ArgumentNullException.ThrowIfNull(room);

        if (!monster.LightSensitivity)
        {
            return 0;
        }

        var lightLevel = room.CurrentLightLevel;

        if (lightLevel == LightLevel.Bright)
        {
            _logger.LogDebug(
                "Monster {Monster} suffers light sensitivity penalty {Penalty} in bright light",
                monster.Name, monster.LightSensitivityPenalty);

            return monster.LightSensitivityPenalty;
        }

        return 0;
    }
}
