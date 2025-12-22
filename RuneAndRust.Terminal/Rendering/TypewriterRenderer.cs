using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Terminal.Helpers;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Renders text with typewriter animation effect (v0.3.4c).
/// Uses dynamic pacing with longer pauses at punctuation marks.
/// Supports skip functionality via any key press.
/// </summary>
public class TypewriterRenderer : ITypewriterRenderer
{
    private readonly ILogger<TypewriterRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypewriterRenderer"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public TypewriterRenderer(ILogger<TypewriterRenderer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PlaySequenceAsync(string text, int delayMs = 30)
    {
        _logger.LogInformation("[Narrative] Starting typewriter sequence, {Length} characters", text.Length);

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[olive]PROLOGUE[/]").LeftJustified());
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        var skipped = false;
        var charIndex = 0;

        foreach (char c in text)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                _logger.LogDebug("[Narrative] User skipped prologue sequence at character {Index}", charIndex);

                // Print remaining text instantly
                var remaining = text.Substring(charIndex);
                AnsiConsole.Markup($"[grey]{Markup.Escape(remaining)}[/]");
                skipped = true;
                break;
            }

            AnsiConsole.Markup($"[grey]{Markup.Escape(c.ToString())}[/]");

            // Dynamic pacing: longer pause at punctuation
            var currentDelay = c is '.' or '!' or '?' ? delayMs * 8 :
                               c is ',' or ';' or ':' ? delayMs * 3 :
                               c is '—' ? delayMs * 4 : delayMs;
            await Task.Delay(currentDelay);
            charIndex++;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        if (!skipped)
        {
            _logger.LogInformation("[Narrative] Prologue sequence completed normally");
        }

        AnsiConsole.MarkupLine("[grey italic]Press any key to enter the ruins...[/]");
        ConsoleInputHelper.WaitForKeyPress();
        _logger.LogInformation("[Narrative] Prologue complete. Transitioning to Exploration.");
        AnsiConsole.Clear();
    }
}
