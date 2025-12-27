namespace RuneAndRust.Core.Models.Input;

using RuneAndRust.Core.Enums;

/// <summary>
/// Base record for all input events in the system.
/// Represents a single discrete input from the user.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-001 for Input System design.
/// v0.3.23a: Initial implementation.
/// </remarks>
public abstract record InputEvent
{
    /// <summary>
    /// Timestamp when the input was captured.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a semantic game action (e.g., MoveNorth, Attack, Confirm).
/// These are the primary events consumed by game logic.
/// </summary>
/// <param name="Action">The resolved game action.</param>
public record ActionEvent(GameAction Action) : InputEvent
{
    /// <summary>
    /// The original key that triggered this action (for debugging/rebind UI).
    /// </summary>
    public ConsoleKey? SourceKey { get; init; }
}

/// <summary>
/// Represents a raw key press that wasn't mapped to an action.
/// Used for text input scenarios (character names, debug console commands).
/// </summary>
/// <param name="KeyInfo">The raw console key information.</param>
public record RawKeyEvent(ConsoleKeyInfo KeyInfo) : InputEvent
{
    /// <summary>
    /// The printable character for this key press (if any).
    /// </summary>
    public char Character => KeyInfo.KeyChar;

    /// <summary>
    /// Whether this key produces a printable character.
    /// </summary>
    public bool IsPrintable => !char.IsControl(KeyInfo.KeyChar);
}

/// <summary>
/// Represents a system-level event (debug console toggle, screenshot, etc.).
/// These bypass normal action handling.
/// </summary>
/// <param name="EventType">The type of system event.</param>
public record SystemEvent(SystemEventType EventType) : InputEvent;

/// <summary>
/// Types of system-level input events.
/// </summary>
public enum SystemEventType
{
    /// <summary>Toggle the debug console (~ key).</summary>
    ToggleDebugConsole,

    /// <summary>Take a screenshot (future).</summary>
    Screenshot
}

/// <summary>
/// Represents a mouse interaction (v0.3.23c preparation).
/// </summary>
/// <param name="X">X coordinate in console cells.</param>
/// <param name="Y">Y coordinate in console cells.</param>
/// <param name="IsLeftClick">True for left click, false for right click.</param>
public record MouseEvent(int X, int Y, bool IsLeftClick) : InputEvent;
