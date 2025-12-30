using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for the Specialization UI controller.
/// Handles grid navigation and node unlock operations.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public interface ISpecializationController
{
    /// <summary>
    /// Gets the current ViewModel for rendering.
    /// </summary>
    SpecializationGridViewModel? CurrentViewModel { get; }

    /// <summary>
    /// Gets whether the controller has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the controller with a character and specialization.
    /// </summary>
    /// <param name="character">The character viewing the specialization.</param>
    /// <param name="specializationId">The specialization to display.</param>
    Task InitializeAsync(Character character, Guid specializationId);

    /// <summary>
    /// Processes a key input and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The game phase to transition to.</returns>
    Task<GamePhase> HandleInputAsync(ConsoleKey key);

    /// <summary>
    /// Resets controller state to initial values.
    /// </summary>
    void Reset();
}
