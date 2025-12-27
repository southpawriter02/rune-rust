namespace RuneAndRust.Engine.Services;

using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Input;
using RuneAndRust.Engine.Helpers;

/// <summary>
/// Centralized input service that reads console input and converts it to semantic events.
/// Integrates with IInputConfigurationService for key binding lookup.
/// </summary>
/// <remarks>
/// See: SPEC-INPUT-001 for Input System design.
/// v0.3.23a: Blocking implementation.
/// v0.3.23c: Added SGR mouse sequence parsing.
/// </remarks>
public class InputService : IInputService
{
    private readonly ILogger<InputService> _logger;
    private readonly IInputConfigurationService _config;

    /// <summary>
    /// The debug console toggle key (tilde).
    /// </summary>
    private const ConsoleKey DebugConsoleKey = ConsoleKey.Oem3;

    /// <summary>
    /// Buffer for collecting escape sequences.
    /// </summary>
    private readonly StringBuilder _escapeBuffer = new();

    /// <summary>
    /// Timeout for escape sequence completion (ms).
    /// </summary>
    private const int EscapeSequenceTimeoutMs = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="config">The input configuration service for key binding lookup.</param>
    public InputService(
        ILogger<InputService> logger,
        IInputConfigurationService config)
    {
        _logger = logger;
        _config = config;

        _logger.LogDebug("[Input] InputService initialized (v0.3.23c)");
    }

    /// <inheritdoc/>
    public InputEvent ReadNext()
    {
        var keyInfo = Console.ReadKey(intercept: true);

        // Check for escape sequence start (mouse events, special keys)
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            return ParseEscapeSequence();
        }

        return ResolveKeyToEvent(keyInfo);
    }

    /// <inheritdoc/>
    public InputEvent ReadNextFiltered(bool filterMouseEvents = true)
    {
        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            if (filterMouseEvents)
            {
                // Filter out escape sequences from mouse events
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    DrainEscapeSequence();
                    continue;
                }

                // Filter out null/empty key events from mouse tracking
                if (keyInfo.KeyChar == '\0' && keyInfo.Key == 0)
                {
                    continue;
                }
            }

            return ResolveKeyToEvent(keyInfo);
        }
    }

    /// <inheritdoc/>
    public bool IsInputAvailable()
    {
        return Console.KeyAvailable;
    }

    /// <inheritdoc/>
    public bool TryReadNext(out InputEvent? inputEvent)
    {
        if (!Console.KeyAvailable)
        {
            inputEvent = null;
            return false;
        }

        inputEvent = ReadNext();
        return true;
    }

    /// <inheritdoc/>
    public void ClearInputBuffer()
    {
        var clearedCount = 0;
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
            clearedCount++;
        }

        if (clearedCount > 0)
        {
            _logger.LogTrace("[Input] Input buffer cleared ({Count} keys)", clearedCount);
        }
    }

    /// <summary>
    /// Resolves a raw key press to an InputEvent.
    /// </summary>
    /// <param name="keyInfo">The raw console key information.</param>
    /// <returns>The resolved InputEvent.</returns>
    private InputEvent ResolveKeyToEvent(ConsoleKeyInfo keyInfo)
    {
        // 1. Check for system-level hotkeys
        if (keyInfo.Key == DebugConsoleKey)
        {
            _logger.LogTrace("[Input] System event: ToggleDebugConsole");
            return new SystemEvent(SystemEventType.ToggleDebugConsole);
        }

        // 2. Check for mapped actions via IInputConfigurationService
        var command = _config.GetCommandForKey(keyInfo.Key);

        if (command != null && CommandToActionMapper.TryMapCommand(command, out var action))
        {
            _logger.LogTrace("[Input] Mapped {Key} -> {Command} -> {Action}",
                keyInfo.Key, command, action);
            return new ActionEvent(action) { SourceKey = keyInfo.Key };
        }

        // 3. Fallback to raw key event
        _logger.LogTrace("[Input] Raw key: {Key} ('{Char}')",
            keyInfo.Key,
            char.IsControl(keyInfo.KeyChar) ? "\\0" : keyInfo.KeyChar.ToString());
        return new RawKeyEvent(keyInfo);
    }

    /// <summary>
    /// Drains any remaining characters from an escape sequence.
    /// Mouse events typically send multi-byte escape sequences that need to be consumed.
    /// </summary>
    private void DrainEscapeSequence()
    {
        // Give a brief moment for escape sequence to arrive
        Thread.Sleep(10);

        // Drain any buffered characters from the escape sequence
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
        }
    }

    /// <summary>
    /// Parses VT escape sequences for mouse events or special keys.
    /// </summary>
    /// <returns>The parsed InputEvent (MouseEvent, ActionEvent, or RawKeyEvent).</returns>
    private InputEvent ParseEscapeSequence()
    {
        _escapeBuffer.Clear();

        // Wait briefly to see if more characters follow
        var deadline = DateTime.UtcNow.AddMilliseconds(EscapeSequenceTimeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            if (Console.KeyAvailable)
            {
                var nextKey = Console.ReadKey(intercept: true);
                _escapeBuffer.Append(nextKey.KeyChar);

                // Check for complete SGR mouse sequence
                if (TryParseSgrMouseSequence(_escapeBuffer.ToString(), out var mouseEvent))
                {
                    _logger.LogTrace("[Input] Parsed mouse event: {Type} at ({X}, {Y})",
                        mouseEvent.EventType, mouseEvent.ScreenX, mouseEvent.ScreenY);
                    return mouseEvent;
                }

                // Check for sequence terminators that indicate completion
                var lastChar = _escapeBuffer[^1];
                if (lastChar == 'M' || lastChar == 'm' || lastChar == '~' ||
                    (lastChar >= 'A' && lastChar <= 'Z'))
                {
                    // Sequence complete but not a mouse event
                    _logger.LogTrace("[Input] Non-mouse escape sequence: ESC{Seq}", _escapeBuffer.ToString());
                    break;
                }

                // Reset deadline on each character
                deadline = DateTime.UtcNow.AddMilliseconds(EscapeSequenceTimeoutMs);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        // No complete mouse sequence found - treat as escape key
        if (_escapeBuffer.Length == 0)
        {
            _logger.LogTrace("[Input] Bare escape key pressed");
            return new ActionEvent(GameAction.Cancel);
        }

        // Unknown escape sequence - drain and ignore
        _logger.LogDebug("[Input] Unknown escape sequence: ESC{Seq}", _escapeBuffer.ToString());
        DrainEscapeSequence();
        return new RawKeyEvent(new ConsoleKeyInfo('\x1b', ConsoleKey.Escape, false, false, false));
    }

    /// <summary>
    /// Attempts to parse an SGR extended mouse sequence.
    /// Format: [&lt;Cb;Cx;Cy{M|m}
    /// Example: [&lt;0;10;5M = Left click at column 10, row 5.
    /// </summary>
    /// <param name="sequence">The escape sequence (without the leading ESC).</param>
    /// <param name="mouseEvent">The parsed mouse event, if successful.</param>
    /// <returns>True if the sequence was a valid SGR mouse sequence.</returns>
    internal static bool TryParseSgrMouseSequence(string sequence, out MouseEvent mouseEvent)
    {
        mouseEvent = null!;

        // SGR format: [<Cb;Cx;Cy{M|m}
        // Must start with '[<' and end with 'M' or 'm'
        if (sequence.Length < 6 || !sequence.StartsWith("[<"))
            return false;

        var lastChar = sequence[^1];
        if (lastChar != 'M' && lastChar != 'm')
            return false;

        // Parse: [<button;x;y{M|m}
        var content = sequence[2..^1]; // Remove "[<" prefix and "M/m" suffix
        var parts = content.Split(';');

        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var buttonCode) ||
            !int.TryParse(parts[1], out var x) ||
            !int.TryParse(parts[2], out var y))
            return false;

        // Decode button (lower 2 bits)
        var button = (buttonCode & 0x03) switch
        {
            0 => MouseButton.Left,
            1 => MouseButton.Middle,
            2 => MouseButton.Right,
            _ => MouseButton.None
        };

        // Check for scroll (bit 6)
        if ((buttonCode & 64) != 0)
        {
            button = (buttonCode & 1) == 0 ? MouseButton.ScrollUp : MouseButton.ScrollDown;
        }

        // Decode modifiers (bits 2-4)
        var modifiers = ConsoleModifiers.None;
        if ((buttonCode & 4) != 0) modifiers |= ConsoleModifiers.Shift;
        if ((buttonCode & 8) != 0) modifiers |= ConsoleModifiers.Alt;
        if ((buttonCode & 16) != 0) modifiers |= ConsoleModifiers.Control;

        // Decode event type
        MouseEventType eventType;
        if ((buttonCode & 64) != 0)
        {
            eventType = MouseEventType.Scroll;
        }
        else if ((buttonCode & 32) != 0)
        {
            eventType = MouseEventType.Move;
        }
        else
        {
            eventType = lastChar == 'M' ? MouseEventType.ButtonDown : MouseEventType.ButtonUp;
        }

        mouseEvent = new MouseEvent(eventType, button, x, y, modifiers);
        return true;
    }
}
