using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the Options screen (v0.3.10b).
/// Implementations use Spectre.Console Layout for a full-screen tabbed interface.
/// </summary>
public interface IOptionsScreenRenderer
{
    /// <summary>
    /// Renders the complete Options screen with tabs, settings list, and controls.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The options view model containing tab state and setting values.</param>
    void Render(OptionsViewModel viewModel);
}
