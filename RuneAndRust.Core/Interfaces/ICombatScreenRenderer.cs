using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering combat UI to the terminal.
/// Implementations should use Spectre.Console for rich text rendering.
/// </summary>
public interface ICombatScreenRenderer
{
    /// <summary>
    /// Renders the complete combat screen including header, turn order, and combat log.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The combat view model containing all display data.</param>
    void Render(CombatViewModel viewModel);
}
