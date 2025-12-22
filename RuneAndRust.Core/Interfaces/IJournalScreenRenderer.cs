using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the full-screen Journal UI (v0.3.7c).
/// Implementations handle platform-specific rendering (Terminal, GUI, etc.).
/// </summary>
public interface IJournalScreenRenderer
{
    /// <summary>
    /// Renders the Journal screen with the provided ViewModel data.
    /// </summary>
    /// <param name="viewModel">The JournalViewModel containing all display-ready data.</param>
    void Render(JournalViewModel viewModel);
}
