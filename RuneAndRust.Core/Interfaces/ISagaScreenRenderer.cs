using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the Saga progression screen (v0.4.0c).
/// Implementations display Legend progress, PP balance, and attribute upgrade options.
/// </summary>
/// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
public interface ISagaScreenRenderer
{
    /// <summary>
    /// Renders the Saga progression screen ("The Shrine of Echoes").
    /// Displays Legend progress bar, PP balance, and attribute upgrade table.
    /// Clears the screen before rendering for a clean display.
    /// </summary>
    /// <param name="viewModel">The saga view model containing all display data.</param>
    void Render(SagaViewModel viewModel);
}
