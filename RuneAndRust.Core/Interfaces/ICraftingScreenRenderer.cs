using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the crafting screen (v0.3.7b).
/// Implementations use Spectre.Console Layout for a full-screen interface.
/// </summary>
public interface ICraftingScreenRenderer
{
    /// <summary>
    /// Renders the complete crafting screen with recipe list, details panel, and trade filter.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The crafting view model containing all display data.</param>
    void Render(CraftingViewModel viewModel);
}
