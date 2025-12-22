using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Displays the title screen with animated logo and main menu (v0.3.4a).
/// </summary>
public interface ITitleScreenService
{
    /// <summary>
    /// Shows the title screen with animation and menu.
    /// Returns the user's menu selection.
    /// </summary>
    /// <returns>A result containing the selected menu option and any associated data.</returns>
    Task<TitleScreenResult> ShowAsync();
}
