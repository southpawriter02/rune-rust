namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Controls;

/// <summary>
/// Provides overlay functionality for combat grid popup placement.
/// </summary>
/// <remarks>
/// This interface is implemented by the combat grid panel to enable
/// the animation service to add/remove floating popups and status indicators.
/// </remarks>
public interface ICombatGridOverlay
{
    /// <summary>
    /// Converts a grid position to screen coordinates.
    /// </summary>
    /// <param name="position">The grid position.</param>
    /// <returns>Screen coordinates (Point) for popup placement.</returns>
    Point GridToScreen(GridPosition position);

    /// <summary>
    /// Adds a popup control at the specified screen position.
    /// </summary>
    /// <param name="popup">The popup control to add.</param>
    /// <param name="screenPosition">Screen coordinates for placement.</param>
    void AddPopup(DamagePopupControl popup, Point screenPosition);

    /// <summary>
    /// Removes a popup control from the overlay.
    /// </summary>
    /// <param name="popup">The popup to remove.</param>
    void RemovePopup(DamagePopupControl popup);

    /// <summary>
    /// Gets the entity token control for a combatant.
    /// </summary>
    /// <param name="combatantId">The combatant's unique ID.</param>
    /// <returns>The token control, or null if not found.</returns>
    EntityTokenControl? GetToken(Guid combatantId);

    /// <summary>
    /// Shows a status effect indicator at the specified position.
    /// </summary>
    /// <param name="position">Grid position for the indicator.</param>
    /// <param name="icon">Icon to display.</param>
    /// <param name="text">Text to display alongside icon.</param>
    /// <param name="fadeOut">True if indicator should fade out.</param>
    void ShowStatusIndicator(GridPosition position, string icon, string text, bool fadeOut = false);

    /// <summary>
    /// Hides the status indicator at the specified position.
    /// </summary>
    /// <param name="position">Grid position of the indicator to hide.</param>
    void HideStatusIndicator(GridPosition position);
}
