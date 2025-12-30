using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Renders the Specialization UI ("Tree of Runes") to the terminal.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public interface ISpecializationScreenRenderer
{
    /// <summary>
    /// Renders the specialization view to the console.
    /// </summary>
    /// <param name="viewModel">The current view state.</param>
    void Render(SpecializationViewModel viewModel);
}
