using Spectre.Console;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        DisplayWelcomeScreen();
        MainGameLoop();
    }

    static void DisplayWelcomeScreen()
    {
        AnsiConsole.Clear();

        // Create a fancy title
        var rule = new Rule("[bold yellow]RUNE & RUST[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Create a panel with the game description
        var panel = new Panel(
            "[dim]A text-based dungeon crawler set in the twilight of a broken world.\n" +
            "Corrupted machines guard ancient ruins. Only the bold survive.\n\n" +
            "[yellow]Vertical Slice v0.1[/] - A 30-minute adventure[/]"
        )
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Press [yellow]ENTER[/] to begin your journey...[/]");
        Console.ReadLine();
    }

    static void MainGameLoop()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold green]Welcome to Rune & Rust![/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Game systems are initializing...[/]");
        AnsiConsole.WriteLine();

        // Test DiceService
        var diceService = new DiceService();
        AnsiConsole.MarkupLine("[yellow]Testing dice system:[/]");
        var testRoll = diceService.Roll(5);
        AnsiConsole.MarkupLine($"[dim]> {testRoll}[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[green]✓[/] [dim]Core systems loaded successfully[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Type 'quit' to exit[/]");
        AnsiConsole.WriteLine();

        // Simple input loop
        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[grey]>[/]")
                    .AllowEmpty()
            );

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[yellow]Thanks for playing![/]");
                break;
            }

            // Echo input back (Week 1 milestone requirement)
            AnsiConsole.MarkupLine($"[dim]You entered: {input.EscapeMarkup()}[/]");
        }
    }
}
