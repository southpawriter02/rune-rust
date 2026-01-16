namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for game interactions.
/// </summary>
/// <remarks>
/// <para>
/// Provides interaction sound triggering:
/// <list type="bullet">
///   <item><description>Item pickup, equip, and use</description></item>
///   <item><description>Puzzle feedback and completion</description></item>
///   <item><description>Terrain-based footsteps</description></item>
///   <item><description>Door and chest interactions</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IInteractionSoundService
{
    // Item sounds

    /// <summary>
    /// Plays an item pickup sound.
    /// </summary>
    void PlayItemPickup();

    /// <summary>
    /// Plays an item drop sound.
    /// </summary>
    void PlayItemDrop();

    /// <summary>
    /// Plays an item equip sound.
    /// </summary>
    void PlayItemEquip();

    /// <summary>
    /// Plays an item use sound.
    /// </summary>
    void PlayItemUse();

    /// <summary>
    /// Plays a gold collection sound.
    /// </summary>
    void PlayGoldCollected();

    // Puzzle sounds

    /// <summary>
    /// Plays a puzzle correct sound.
    /// </summary>
    void PlayPuzzleCorrect();

    /// <summary>
    /// Plays a puzzle incorrect sound.
    /// </summary>
    void PlayPuzzleIncorrect();

    /// <summary>
    /// Plays a puzzle complete fanfare.
    /// </summary>
    void PlayPuzzleComplete();

    /// <summary>
    /// Plays a lever pull sound.
    /// </summary>
    void PlayLeverPull();

    // Movement sounds

    /// <summary>
    /// Plays a footstep sound based on terrain type.
    /// </summary>
    /// <param name="terrain">The terrain type (stone, wood, grass, water, metal, sand).</param>
    void PlayFootstep(string? terrain);

    /// <summary>
    /// Plays a door open sound.
    /// </summary>
    void PlayDoorOpen();

    /// <summary>
    /// Plays a door close sound.
    /// </summary>
    void PlayDoorClose();

    /// <summary>
    /// Plays a chest open sound.
    /// </summary>
    void PlayChestOpen();
}
