namespace RuneAndRust.Application.Enums;

/// <summary>
/// Music themes that change based on game context.
/// </summary>
/// <remarks>
/// <para>
/// Each theme represents a different game state:
/// <list type="bullet">
///   <item><description><see cref="None"/> — No music playing</description></item>
///   <item><description><see cref="MainMenu"/> — Main menu ambient</description></item>
///   <item><description><see cref="Exploration"/> — Dungeon exploration</description></item>
///   <item><description><see cref="Combat"/> — Standard combat encounters</description></item>
///   <item><description><see cref="BossCombat"/> — Boss fight intensity</description></item>
///   <item><description><see cref="SafeArea"/> — Town/peaceful areas</description></item>
///   <item><description><see cref="Victory"/> — Victory stinger (one-shot)</description></item>
///   <item><description><see cref="Defeat"/> — Defeat stinger (one-shot)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum MusicTheme
{
    /// <summary>
    /// No music playing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Main menu music.
    /// </summary>
    MainMenu = 1,

    /// <summary>
    /// Dungeon exploration ambient music.
    /// </summary>
    Exploration = 2,

    /// <summary>
    /// Standard combat music.
    /// </summary>
    Combat = 3,

    /// <summary>
    /// Boss encounter music.
    /// </summary>
    BossCombat = 4,

    /// <summary>
    /// Town/safe area peaceful music.
    /// </summary>
    SafeArea = 5,

    /// <summary>
    /// Victory celebration stinger (one-shot, no loop).
    /// </summary>
    Victory = 6,

    /// <summary>
    /// Defeat/death stinger (one-shot, no loop).
    /// </summary>
    Defeat = 7
}
