namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Manages terminal state such as mouse mode and alternate screen buffer.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-002 for Mouse Support System design.
/// v0.3.23c: Initial implementation.
/// </remarks>
public interface ITerminalService
{
    /// <summary>
    /// Whether mouse mode is currently enabled.
    /// </summary>
    bool IsMouseEnabled { get; }

    /// <summary>
    /// Enables SGR Extended mouse mode for VT-compatible terminals.
    /// Must be paired with DisableMouseMode() on exit.
    /// </summary>
    void EnableMouseMode();

    /// <summary>
    /// Disables mouse mode and restores terminal state.
    /// Safe to call even if mouse mode was never enabled.
    /// </summary>
    void DisableMouseMode();

    /// <summary>
    /// Checks if the current terminal likely supports mouse input.
    /// </summary>
    /// <returns>True if mouse support is likely available.</returns>
    bool IsMouseSupported();
}
