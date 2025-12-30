using RuneAndRust.Core.Entities;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Builds SpecializationGridViewModel from character and specialization data.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public interface ISpecializationGridViewModelBuilder
{
    /// <summary>
    /// Builds the view model for a character's specialization tree.
    /// </summary>
    /// <param name="character">The character viewing the tree.</param>
    /// <param name="specializationId">The specialization to display.</param>
    /// <returns>The populated view model.</returns>
    Task<SpecializationGridViewModel> BuildAsync(Character character, Guid specializationId);

    /// <summary>
    /// Refreshes the view model after a node unlock.
    /// </summary>
    /// <param name="existing">The existing view model to refresh.</param>
    /// <param name="character">The character (with updated state).</param>
    /// <returns>The updated view model.</returns>
    Task<SpecializationGridViewModel> RefreshAsync(
        SpecializationGridViewModel existing,
        Character character);
}
