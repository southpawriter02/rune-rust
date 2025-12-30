using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Renderer interface for the specialization tree grid UI.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public interface ISpecializationGridRenderer
{
    /// <summary>
    /// Renders the specialization grid to the terminal.
    /// </summary>
    /// <param name="vm">The view model containing tree and selection state.</param>
    void Render(SpecializationGridViewModel vm);
}
