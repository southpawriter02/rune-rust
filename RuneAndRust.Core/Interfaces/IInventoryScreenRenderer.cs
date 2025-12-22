using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the inventory screen (v0.3.7a).
/// Implementations use Spectre.Console Layout for a full-screen interface.
/// </summary>
public interface IInventoryScreenRenderer
{
    /// <summary>
    /// Renders the complete inventory screen with equipment panel, backpack list, and burden bar.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The inventory view model containing all display data.</param>
    void Render(InventoryViewModel viewModel);
}
