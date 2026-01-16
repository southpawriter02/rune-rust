namespace RuneAndRust.Application.Enums;

/// <summary>
/// Audio channels for separate volume control.
/// </summary>
/// <remarks>
/// <para>
/// Each channel can have independent volume and mute settings:
/// <list type="bullet">
///   <item><description><see cref="Master"/> — Affects all other channels</description></item>
///   <item><description><see cref="Music"/> — Background music tracks</description></item>
///   <item><description><see cref="Effects"/> — Combat and game SFX</description></item>
///   <item><description><see cref="UI"/> — Interface sounds (clicks, hovers)</description></item>
///   <item><description><see cref="Voice"/> — Dialogue and narration</description></item>
/// </list>
/// </para>
/// </remarks>
public enum AudioChannel
{
    /// <summary>
    /// Master volume affecting all channels.
    /// </summary>
    /// <remarks>
    /// When muted, all audio stops. Master volume is multiplied with
    /// individual channel volumes to calculate effective volume.
    /// </remarks>
    Master = 0,

    /// <summary>
    /// Background music.
    /// </summary>
    Music = 1,

    /// <summary>
    /// Combat and game sound effects.
    /// </summary>
    Effects = 2,

    /// <summary>
    /// User interface interaction sounds.
    /// </summary>
    UI = 3,

    /// <summary>
    /// Voice and dialogue audio.
    /// </summary>
    /// <remarks>Reserved for future dialogue system integration.</remarks>
    Voice = 4
}
