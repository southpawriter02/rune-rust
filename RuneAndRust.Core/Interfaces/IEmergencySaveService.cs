using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for emergency game state preservation during crashes (v0.3.16b).
/// Uses file-based synchronous I/O to maximize reliability when the
/// application is in a potentially unstable state.
/// Part of "The Black Box" crash recovery system.
/// </summary>
/// <remarks>See: SPEC-CRASH-001 for Crash Handling System design.</remarks>
public interface IEmergencySaveService
{
    /// <summary>
    /// Attempts to save the current game state to the emergency backup file.
    /// Uses synchronous I/O to ensure completion before process death.
    /// </summary>
    /// <param name="state">The game state to save.</param>
    /// <returns>True if save succeeded; false if save failed or state was invalid.</returns>
    bool TryEmergencySave(GameState state);

    /// <summary>
    /// Checks if an emergency save file exists.
    /// </summary>
    /// <returns>True if emergency.json exists; otherwise, false.</returns>
    bool EmergencySaveExists();

    /// <summary>
    /// Loads the emergency save file if it exists.
    /// </summary>
    /// <returns>The loaded GameState or null if not found or corrupt.</returns>
    GameState? LoadEmergencySave();

    /// <summary>
    /// Deletes the emergency save file after successful recovery.
    /// </summary>
    void ClearEmergencySave();
}
