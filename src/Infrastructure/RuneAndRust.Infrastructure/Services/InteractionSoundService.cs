namespace RuneAndRust.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for game interactions.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item><description>Item interaction sounds</description></item>
///   <item><description>Puzzle feedback sounds</description></item>
///   <item><description>Terrain-based footstep sounds</description></item>
///   <item><description>Environment interaction sounds</description></item>
/// </list>
/// </para>
/// </remarks>
public class InteractionSoundService : IInteractionSoundService
{
    private readonly ISoundEffectService _sfx;
    private readonly ILogger<InteractionSoundService> _logger;

    /// <summary>
    /// Creates a new interaction sound service.
    /// </summary>
    /// <param name="sfx">Sound effect service for playback.</param>
    /// <param name="logger">Logger for interaction sounds.</param>
    public InteractionSoundService(ISoundEffectService sfx, ILogger<InteractionSoundService> logger)
    {
        _sfx = sfx;
        _logger = logger;
        _logger.LogDebug("InteractionSoundService initialized");
    }

    // Item sounds

    /// <inheritdoc />
    public void PlayItemPickup()
    {
        _sfx.Play("item-pickup");
        _logger.LogDebug("Interaction sound: item pickup");
    }

    /// <inheritdoc />
    public void PlayItemDrop()
    {
        _sfx.Play("item-drop");
        _logger.LogDebug("Interaction sound: item drop");
    }

    /// <inheritdoc />
    public void PlayItemEquip()
    {
        _sfx.Play("item-equip");
        _logger.LogDebug("Interaction sound: item equip");
    }

    /// <inheritdoc />
    public void PlayItemUse()
    {
        _sfx.Play("item-use");
        _logger.LogDebug("Interaction sound: item use");
    }

    /// <inheritdoc />
    public void PlayGoldCollected()
    {
        _sfx.Play("gold-coins");
        _logger.LogDebug("Interaction sound: gold collected");
    }

    // Puzzle sounds

    /// <inheritdoc />
    public void PlayPuzzleCorrect()
    {
        _sfx.Play("puzzle-correct");
        _logger.LogDebug("Interaction sound: puzzle correct");
    }

    /// <inheritdoc />
    public void PlayPuzzleIncorrect()
    {
        _sfx.Play("puzzle-incorrect");
        _logger.LogDebug("Interaction sound: puzzle incorrect");
    }

    /// <inheritdoc />
    public void PlayPuzzleComplete()
    {
        _sfx.Play("puzzle-complete");
        _logger.LogDebug("Interaction sound: puzzle complete");
    }

    /// <inheritdoc />
    public void PlayLeverPull()
    {
        _sfx.Play("lever-pull");
        _logger.LogDebug("Interaction sound: lever pull");
    }

    // Movement sounds

    /// <inheritdoc />
    public void PlayFootstep(string? terrain)
    {
        var effectId = MapTerrainToFootstep(terrain);
        _sfx.Play(effectId, volume: 0.5f);
        _logger.LogDebug("Interaction sound: footstep ({Terrain}) → {EffectId}", terrain ?? "default", effectId);
    }

    /// <inheritdoc />
    public void PlayDoorOpen()
    {
        _sfx.Play("door-open");
        _logger.LogDebug("Interaction sound: door open");
    }

    /// <inheritdoc />
    public void PlayDoorClose()
    {
        _sfx.Play("door-close");
        _logger.LogDebug("Interaction sound: door close");
    }

    /// <inheritdoc />
    public void PlayChestOpen()
    {
        _sfx.Play("chest-open");
        _logger.LogDebug("Interaction sound: chest open");
    }

    /// <summary>
    /// Maps terrain type to a footstep effect ID.
    /// </summary>
    private static string MapTerrainToFootstep(string? terrain) => terrain?.ToLowerInvariant() switch
    {
        "stone" or "dungeon" or "cave" => "footstep-stone",
        "wood" or "floor" or "tavern" => "footstep-wood",
        "grass" or "forest" or "nature" => "footstep-grass",
        "water" or "shallow" or "marsh" => "footstep-water",
        "metal" or "grate" => "footstep-metal",
        "sand" or "desert" => "footstep-sand",
        _ => "footstep-stone" // Default fallback
    };
}
