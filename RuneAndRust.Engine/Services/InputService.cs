namespace RuneAndRust.Engine.Services;

using Microsoft.Extensions.Logging;
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

        _logger.LogDebug("[Input] InputService initialized (v0.3.23a)");
    }

    /// <inheritdoc/>
    public InputEvent ReadNext()
    {
        var keyInfo = Console.ReadKey(intercept: true);
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
}
