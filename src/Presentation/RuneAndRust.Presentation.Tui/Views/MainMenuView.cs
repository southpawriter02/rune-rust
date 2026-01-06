using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

public enum MainMenuOption
{
    NewGame,
    LoadGame,
    Quit
}

public class MainMenuView
{
    public void RenderTitle()
    {
        AnsiConsole.Clear();

        var title = new FigletText("Rune & Rust")
            .Centered()
            .Color(Color.Yellow);

        AnsiConsole.Write(title);

        AnsiConsole.Write(new Rule("[grey]A Text-Based Dungeon Crawler[/]")
            .Centered()
            .RuleStyle("grey"));

        Console.WriteLine();
    }

    public MainMenuOption GetMenuSelection()
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<MainMenuOption>()
                .Title("[cyan]What would you like to do?[/]")
                .AddChoices(MainMenuOption.NewGame, MainMenuOption.LoadGame, MainMenuOption.Quit)
                .UseConverter(option => option switch
                {
                    MainMenuOption.NewGame => "New Game",
                    MainMenuOption.LoadGame => "Load Game",
                    MainMenuOption.Quit => "Quit",
                    _ => option.ToString()
                }));

        return selection;
    }

    public string GetPlayerName()
    {
        Console.WriteLine();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Enter your name, adventurer:[/]")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Name must be between 1 and 50 characters[/]")
                .Validate(name =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                        return ValidationResult.Error("[red]Name cannot be empty[/]");
                    if (name.Length > 50)
                        return ValidationResult.Error("[red]Name is too long[/]");
                    return ValidationResult.Success();
                }));

        return name.Trim();
    }

    public void RenderWelcome(string playerName)
    {
        Console.WriteLine();
        AnsiConsole.MarkupLine($"[green]Welcome, [bold]{Markup.Escape(playerName)}[/]![/]");
        AnsiConsole.MarkupLine("[grey]Your adventure begins in the Forgotten Depths...[/]");
        Console.WriteLine();
        AnsiConsole.MarkupLine("[grey]Type 'help' for a list of commands.[/]");
        Console.WriteLine();
    }
}
