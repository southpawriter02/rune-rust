namespace RuneAndRust.Core.Interfaces;

using RuneAndRust.Core.Models.Input;

/// <summary>
/// Abstracted input service that converts raw keyboard input into semantic events.
/// Integrates with IInputConfigurationService for key binding resolution.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-001 for Input System design.
/// v0.3.23a: Initial blocking implementation.
/// v0.3.23b: Will add async/non-blocking support.
/// </remarks>
public interface IInputService
{
    /// <summary>
    /// Reads the next input event, blocking until input is available.
    /// </summary>
    /// <returns>The resolved input event (ActionEvent, RawKeyEvent, or SystemEvent).</returns>
    InputEvent ReadNext();

    /// <summary>
    /// Reads the next input event, blocking until input is available.
    /// Filters out mouse escape sequences and invalid keys.
    /// </summary>
    /// <param name="filterMouseEvents">If true, filters out mouse tracking escape sequences.</param>
    /// <returns>The resolved input event.</returns>
    InputEvent ReadNextFiltered(bool filterMouseEvents = true);

    /// <summary>
    /// Checks if input is available without blocking.
    /// </summary>
    /// <returns>True if a key press is waiting in the buffer.</returns>
    bool IsInputAvailable();

    /// <summary>
    /// Attempts to read input without blocking.
    /// </summary>
    /// <param name="inputEvent">The input event if available.</param>
    /// <returns>True if input was available and read.</returns>
    bool TryReadNext(out InputEvent? inputEvent);

    /// <summary>
    /// Clears any pending input from the buffer.
    /// Useful after screen transitions to prevent accidental actions.
    /// </summary>
    void ClearInputBuffer();
}
