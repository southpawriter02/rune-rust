using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Input;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the Quake-style debug console overlay (v0.3.17a).
/// Uses a modal input loop similar to OptionsController pattern.
/// v0.3.17b: Added cheat command routing.
/// v0.3.23a: Refactored to use IInputService.
/// </summary>
/// <remarks>
/// See: SPEC-DEBUG-001 for Debug Console System design.
/// See: SPEC-CHEAT-001 for Cheat Command System design.
/// </remarks>
public class DebugConsoleRenderer : IDebugConsoleRenderer
{
    private const int VisibleLogLines = 15;

    private readonly IDebugConsoleService _console;
    private readonly ICheatService _cheats;
    private readonly IInputService _inputService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugConsoleRenderer"/> class.
    /// </summary>
    /// <param name="console">The debug console service.</param>
    /// <param name="cheats">The cheat service.</param>
    /// <param name="inputService">The input service for standardized input handling.</param>
    public DebugConsoleRenderer(
        IDebugConsoleService console,
        ICheatService cheats,
        IInputService inputService)
    {
        _console = console;
        _cheats = cheats;
        _inputService = inputService;
    }

    /// <summary>
    /// Runs the debug console in modal mode.
    /// Blocks until user exits with ~ or Escape.
    /// </summary>
    /// <remarks>v0.3.23a: Refactored to use IInputService.</remarks>
    public void Run()
    {
        _console.Toggle();
        _console.WriteLog("Debug Console activated. Type 'help' for commands, '~' to exit.");

        var inputBuffer = "";
        var historyIndex = -1;

        while (_console.IsVisible)
        {
            RenderConsoleScreen(inputBuffer);

            var inputEvent = _inputService.ReadNext();

            switch (inputEvent)
            {
                // System event: Toggle debug console (tilde key)
                case SystemEvent { EventType: SystemEventType.ToggleDebugConsole }:
                    _console.WriteLog("Debug Console closed.");
                    _console.Toggle();
                    break;

                // Raw key handling for text input
                case RawKeyEvent rawKey:
                    switch (rawKey.KeyInfo.Key)
                    {
                        case ConsoleKey.Escape:
                            _console.WriteLog("Debug Console closed.");
                            _console.Toggle();
                            break;

                        case ConsoleKey.Enter:
                            if (!string.IsNullOrEmpty(inputBuffer))
                            {
                                ProcessCommand(inputBuffer);
                                inputBuffer = "";
                                historyIndex = -1;
                            }
                            break;

                        case ConsoleKey.Backspace:
                            if (inputBuffer.Length > 0)
                            {
                                inputBuffer = inputBuffer[..^1];
                            }
                            break;

                        case ConsoleKey.UpArrow:
                            if (_console.CommandHistory.Count > 0)
                            {
                                historyIndex = Math.Min(historyIndex + 1, _console.CommandHistory.Count - 1);
                                inputBuffer = _console.CommandHistory[^(historyIndex + 1)];
                            }
                            break;

                        case ConsoleKey.DownArrow:
                            if (historyIndex > 0)
                            {
                                historyIndex--;
                                inputBuffer = _console.CommandHistory[^(historyIndex + 1)];
                            }
                            else
                            {
                                historyIndex = -1;
                                inputBuffer = "";
                            }
                            break;

                        default:
                            if (rawKey.IsPrintable)
                            {
                                inputBuffer += rawKey.Character;
                            }
                            break;
                    }
                    break;

                // Action events - map to behavior
                case ActionEvent { Action: Core.Enums.GameAction.Cancel }:
                    _console.WriteLog("Debug Console closed.");
                    _console.Toggle();
                    break;

                case ActionEvent { Action: Core.Enums.GameAction.Confirm }:
                    if (!string.IsNullOrEmpty(inputBuffer))
                    {
                        ProcessCommand(inputBuffer);
                        inputBuffer = "";
                        historyIndex = -1;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Processes a submitted command.
    /// v0.3.17a: Built-in console commands.
    /// v0.3.17b: Added cheat command routing.
    /// </summary>
    /// <param name="command">The command to process.</param>
    private void ProcessCommand(string command)
    {
        _console.SubmitCommand(command);

        var cmd = command.Trim().ToLowerInvariant();

        // v0.3.17b: Check for cheat commands (/ prefix)
        if (cmd.StartsWith('/'))
        {
            ProcessCheatCommand(cmd);
            return;
        }

        switch (cmd)
        {
            case "help":
                _console.WriteLog("Console commands:");
                _console.WriteLog("  help   - Show this help");
                _console.WriteLog("  clear  - Clear console log");
                _console.WriteLog("  exit/~ - Close console");
                _console.WriteLog("");
                _console.WriteLog("Cheat commands (/ prefix):");
                _console.WriteLog("  /god    - Toggle invincibility");
                _console.WriteLog("  /heal   - Restore HP/Stamina/AP");
                _console.WriteLog("  /tp X   - Teleport to room (GUID or name)");
                _console.WriteLog("  /reveal - Reveal all map rooms");
                break;

            case "clear":
                _console.ClearLog();
                _console.WriteLog("Console cleared.");
                break;

            case "exit":
            case "~":
                _console.WriteLog("Debug Console closed.");
                _console.Toggle();
                break;

            default:
                _console.WriteLog($"Unknown command: {command}", "Error");
                _console.WriteLog("Type 'help' for available commands.");
                break;
        }
    }

    /// <summary>
    /// Processes cheat commands prefixed with /.
    /// v0.3.17b: The Toolbox implementation.
    /// </summary>
    /// <param name="cmd">The command (already lowercased, with / prefix).</param>
    private void ProcessCheatCommand(string cmd)
    {
        var parts = cmd.TrimStart('/').Split(' ', 2);
        var verb = parts[0];
        var args = parts.Length > 1 ? parts[1] : "";

        switch (verb)
        {
            case "god":
            case "godmode":
                var state = _cheats.ToggleGodMode();
                _console.WriteLog($"God Mode: {(state ? "ON" : "OFF")}", "Cheat");
                break;

            case "heal":
                if (_cheats.FullHeal())
                {
                    _console.WriteLog("Character fully restored.", "Cheat");
                }
                else
                {
                    _console.WriteLog("No active character.", "Error");
                }
                break;

            case "tp":
            case "teleport":
                if (string.IsNullOrEmpty(args))
                {
                    _console.WriteLog("Usage: /tp <room-id or name>", "Error");
                    break;
                }
                var roomName = _cheats.TeleportAsync(args).GetAwaiter().GetResult();
                if (roomName != null)
                {
                    _console.WriteLog($"Teleported to: {roomName}", "Cheat");
                }
                else
                {
                    _console.WriteLog($"Room not found: {args}", "Error");
                }
                break;

            case "reveal":
                var count = _cheats.RevealMapAsync().GetAwaiter().GetResult();
                _console.WriteLog($"Revealed {count} rooms.", "Cheat");
                break;

            case "spawn":
                _console.WriteLog("Spawn not yet implemented.", "Error");
                break;

            default:
                _console.WriteLog($"Unknown cheat: /{verb}", "Error");
                break;
        }
    }

    /// <summary>
    /// Renders the full console screen.
    /// </summary>
    /// <param name="inputBuffer">The current input buffer.</param>
    private void RenderConsoleScreen(string inputBuffer)
    {
        AnsiConsole.Clear();

        // Header
        var header = new Rule("[bold yellow]DEBUG CONSOLE[/]")
        {
            Style = Style.Parse("purple"),
            Justification = Justify.Center
        };
        AnsiConsole.Write(header);

        AnsiConsole.WriteLine();

        // Log view (last N lines)
        var visibleLogs = _console.LogHistory.TakeLast(VisibleLogLines).ToList();

        foreach (var log in visibleLogs)
        {
            RenderLogLine(log);
        }

        // Padding to push input to bottom
        var padding = VisibleLogLines - visibleLogs.Count;
        for (var i = 0; i < padding; i++)
        {
            AnsiConsole.WriteLine();
        }

        AnsiConsole.WriteLine();

        // Input separator
        var separator = new Rule()
        {
            Style = Style.Parse("purple")
        };
        AnsiConsole.Write(separator);

        // Input line
        AnsiConsole.Markup($"[yellow]>[/] {Markup.Escape(inputBuffer)}[blink]_[/]");
    }

    /// <summary>
    /// Renders a single log line with appropriate coloring.
    /// </summary>
    /// <param name="log">The log entry to render.</param>
    private static void RenderLogLine(string log)
    {
        // Color based on source
        var color = "grey";

        if (log.Contains("[User]"))
        {
            color = "cyan";
        }
        else if (log.Contains("[Error]"))
        {
            color = "red";
        }
        else if (log.Contains("[System]"))
        {
            color = "grey";
        }

        AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(log)}[/]");
    }
}
