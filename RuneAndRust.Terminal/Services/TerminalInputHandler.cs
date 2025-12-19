using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Terminal-based implementation of IInputHandler using Spectre.Console.
/// Provides rich, formatted console output with colored prompts and messages.
/// </summary>
public class TerminalInputHandler : IInputHandler
{
    /// <inheritdoc/>
    public string GetInput(string prompt)
    {
        // Use Spectre.Console's Prompt for rich input handling
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]{prompt}[/] [grey]>[/]")
                .AllowEmpty());
    }

    /// <inheritdoc/>
    public void DisplayMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine($"[white]{EscapeMarkup(message)}[/]");
        }
    }

    /// <inheritdoc/>
    public void DisplayError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{EscapeMarkup(message)}[/]");
    }

    /// <summary>
    /// Escapes special Spectre.Console markup characters in the message.
    /// </summary>
    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }
}
