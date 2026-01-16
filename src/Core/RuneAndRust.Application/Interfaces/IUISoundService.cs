namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Triggers sound effects for UI interactions.
/// </summary>
/// <remarks>
/// <para>
/// Provides UI sound triggering:
/// <list type="bullet">
///   <item><description>Button clicks and hovers</description></item>
///   <item><description>Menu and dialog open/close</description></item>
///   <item><description>Notifications and errors</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IUISoundService
{
    /// <summary>
    /// Plays a button click sound.
    /// </summary>
    void PlayButtonClick();

    /// <summary>
    /// Plays a button hover sound.
    /// </summary>
    /// <param name="volume">Optional volume (default 0.5).</param>
    void PlayButtonHover(float volume = 0.5f);

    /// <summary>
    /// Plays a menu open sound.
    /// </summary>
    void PlayMenuOpen();

    /// <summary>
    /// Plays a menu close sound.
    /// </summary>
    void PlayMenuClose();

    /// <summary>
    /// Plays a notification sound.
    /// </summary>
    void PlayNotification();

    /// <summary>
    /// Plays an error sound.
    /// </summary>
    void PlayError();

    /// <summary>
    /// Plays a tab changed sound.
    /// </summary>
    void PlayTabChanged();
}
