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
/// Mouse button identifier for mouse events.
/// </summary>
/// <remarks>
/// Values match SGR mouse protocol button codes.
/// v0.3.23c: Initial implementation.
/// </remarks>
public enum MouseButton
{
    /// <summary>Left mouse button.</summary>
    Left = 0,

    /// <summary>Middle mouse button (scroll wheel click).</summary>
    Middle = 1,

    /// <summary>Right mouse button.</summary>
    Right = 2,

    /// <summary>No button (used for movement events).</summary>
    None = 3,

    /// <summary>Scroll wheel up.</summary>
    ScrollUp = 64,

    /// <summary>Scroll wheel down.</summary>
    ScrollDown = 65
}

/// <summary>
/// Type of mouse event.
/// </summary>
/// <remarks>
/// v0.3.23c: Initial implementation.
/// </remarks>
public enum MouseEventType
{
    /// <summary>Mouse button pressed down.</summary>
    ButtonDown,

    /// <summary>Mouse button released.</summary>
    ButtonUp,

    /// <summary>Mouse moved (if tracking enabled).</summary>
    Move,

    /// <summary>Mouse wheel scrolled.</summary>
    Scroll
}

/// <summary>
/// Represents a mouse event from the terminal.
/// Uses 1-based screen coordinates (column, row).
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-002 for Mouse Support System design.
/// v0.3.23c: Full implementation with button/modifier support.
/// </remarks>
/// <param name="EventType">The type of mouse event.</param>
/// <param name="Button">The mouse button involved.</param>
/// <param name="ScreenX">1-based column (X coordinate).</param>
/// <param name="ScreenY">1-based row (Y coordinate).</param>
/// <param name="Modifiers">Keyboard modifiers held during event.</param>
public record MouseEvent(
    MouseEventType EventType,
    MouseButton Button,
    int ScreenX,
    int ScreenY,
    ConsoleModifiers Modifiers = ConsoleModifiers.None
) : InputEvent
{
    /// <summary>
    /// Creates a click event (button down).
    /// </summary>
    /// <param name="x">1-based column.</param>
    /// <param name="y">1-based row.</param>
    /// <param name="button">The button clicked.</param>
    /// <returns>A new MouseEvent for a button down.</returns>
    public static MouseEvent Click(int x, int y, MouseButton button = MouseButton.Left)
        => new(MouseEventType.ButtonDown, button, x, y);

    /// <summary>
    /// Creates a scroll event.
    /// </summary>
    /// <param name="x">1-based column.</param>
    /// <param name="y">1-based row.</param>
    /// <param name="up">True for scroll up, false for scroll down.</param>
    /// <returns>A new MouseEvent for a scroll.</returns>
    public static MouseEvent Scroll(int x, int y, bool up)
        => new(MouseEventType.Scroll, up ? MouseButton.ScrollUp : MouseButton.ScrollDown, x, y);

    /// <summary>
    /// Checks if this event is a left click (button down).
    /// </summary>
    public bool IsLeftClick => EventType == MouseEventType.ButtonDown && Button == MouseButton.Left;

    /// <summary>
    /// Checks if this event is a right click (button down).
    /// </summary>
    public bool IsRightClick => EventType == MouseEventType.ButtonDown && Button == MouseButton.Right;

    /// <summary>
    /// Alias for ScreenX (backward compatibility).
    /// </summary>
    public int X => ScreenX;

    /// <summary>
    /// Alias for ScreenY (backward compatibility).
    /// </summary>
    public int Y => ScreenY;
}
