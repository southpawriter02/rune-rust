using Spectre.Console;
using Spectre.Console.Rendering;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders the "Red Screen of Death" when a critical error occurs (v0.3.16a/b).
/// Uses Spectre.Console for styled terminal output.
/// Part of "The Safety Net" and "The Black Box" crash recovery system.
/// </summary>
/// <remarks>
/// See: SPEC-CRASH-001 for Crash Handling System design.
/// This is a static class because it must function when the DI container
/// may not be available (e.g., if the host failed to build).
/// </remarks>
public static class CrashScreenRenderer
{
    /// <summary>
    /// Renders the crash screen with exception details.
    /// Clears the terminal and displays a red-bordered error panel.
    /// </summary>
    /// <param name="ex">The exception that caused the crash.</param>
    /// <param name="logPath">Path to the crash log file, if available.</param>
    /// <param name="backupSaved">Whether the emergency save was successful (v0.3.16b). Null if not attempted.</param>
    public static void Render(Exception ex, string? logPath = null, bool? backupSaved = null)
    {
        AnsiConsole.Clear();

        // Build the panel content
        var rows = new List<IRenderable>
        {
            new Markup("[bold white]A critical error has occurred.[/]"),
            new Text(""),
            new Markup($"[bold red]{Markup.Escape(ex.GetType().Name)}[/]"),
            new Markup($"[italic grey]{Markup.Escape(TruncateMessage(ex.Message))}[/]"),
            new Text("")
        };

        // Add crash log path or fallback message
        if (!string.IsNullOrEmpty(logPath))
        {
            rows.Add(new Markup($"[grey]Crash report saved to:[/]"));
            rows.Add(new Markup($"[dim]{Markup.Escape(logPath)}[/]"));
        }
        else
        {
            rows.Add(new Markup("[yellow]Unable to save crash report.[/]"));
        }

        // v0.3.16b: Add backup status section
        if (backupSaved.HasValue)
        {
            rows.Add(new Text(""));
            if (backupSaved.Value)
            {
                rows.Add(new Markup("[green]Game progress backed up successfully[/]"));
            }
            else
            {
                rows.Add(new Markup("[yellow]Unable to backup game progress[/]"));
            }
        }

        rows.Add(new Text(""));
        rows.Add(new Markup("[dim]Please report this issue at:[/]"));
        rows.Add(new Markup("[blue link]https://github.com/southpawriter02/rune-rust/issues[/]"));

        // Create the error panel
        var panel = new Panel(new Rows(rows))
        {
            Header = new PanelHeader(" [bold white on red] SYSTEM FAILURE [/] ", Justify.Center),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Red),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press [bold]ENTER[/] to exit...[/]");

        // Wait for user acknowledgement
        Console.ReadLine();
    }

    /// <summary>
    /// Truncates long exception messages to prevent display issues.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="maxLength">Maximum length before truncation.</param>
    /// <returns>Truncated message with ellipsis if needed.</returns>
    private static string TruncateMessage(string message, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(message))
            return "(No message provided)";

        if (message.Length <= maxLength)
            return message;

        return message[..(maxLength - 3)] + "...";
    }
}
