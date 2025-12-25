using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the Quake-style debug console overlay (v0.3.17a).
/// Uses a modal input loop similar to OptionsController pattern.
/// </summary>
public class DebugConsoleRenderer : IDebugConsoleRenderer
{
    private const int VisibleLogLines = 15;

    private readonly IDebugConsoleService _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugConsoleRenderer"/> class.
    /// </summary>
    /// <param name="console">The debug console service.</param>
    public DebugConsoleRenderer(IDebugConsoleService console)
    {
        _console = console;
    }

    /// <summary>
    /// Runs the debug console in modal mode.
    /// Blocks until user exits with ~ or Escape.
    /// </summary>
    public void Run()
    {
        _console.Toggle();
        _console.WriteLog("Debug Console activated. Type 'help' for commands, '~' to exit.");

        var inputBuffer = "";
        var historyIndex = -1;

        while (_console.IsVisible)
        {
            RenderConsoleScreen(inputBuffer);

            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Oem3: // Tilde key
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
                    if (!char.IsControl(key.KeyChar))
                    {
                        inputBuffer += key.KeyChar;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Processes a submitted command.
    /// v0.3.17a: Only handles built-in console commands.
    /// v0.3.17b will add cheat commands.
    /// </summary>
    /// <param name="command">The command to process.</param>
    private void ProcessCommand(string command)
    {
        _console.SubmitCommand(command);

        var cmd = command.Trim().ToLowerInvariant();

        switch (cmd)
        {
            case "help":
                _console.WriteLog("Available commands:");
                _console.WriteLog("  help  - Show this help message");
                _console.WriteLog("  clear - Clear the console log");
                _console.WriteLog("  exit  - Close the debug console");
                _console.WriteLog("  ~     - Close the debug console");
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
