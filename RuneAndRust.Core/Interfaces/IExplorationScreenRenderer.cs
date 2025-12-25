using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the exploration HUD (v0.3.5a).
/// Implementations use Spectre.Console Layout for a persistent, three-pane interface.
/// </summary>
/// <remarks>See: SPEC-RENDER-001 for Rendering Pipeline System design.</remarks>
public interface IExplorationScreenRenderer
{
    /// <summary>
    /// Renders the complete exploration screen with status bar, room view, and minimap.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The exploration view model containing all display data.</param>
    void Render(ExplorationViewModel viewModel);
}
