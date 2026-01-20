using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

/// <summary>
/// Options available in the main menu.
/// </summary>
public enum MainMenuOption
{
    /// <summary>
    /// Start a new game session.
    /// </summary>
    NewGame,

    /// <summary>
    /// Load an existing saved game.
    /// </summary>
    LoadGame,

    /// <summary>
    /// Exit the application.
    /// </summary>
    Quit
}

/// <summary>
/// The main menu view for the TUI application.
/// </summary>
/// <remarks>
/// MainMenuView handles rendering the title screen, presenting menu options,
/// collecting player name input, and displaying welcome messages.
/// </remarks>
public class MainMenuView
{
    /// <summary>
    /// Logger for view operations and diagnostics.
    /// </summary>
    private readonly ILogger<MainMenuView> _logger;

    /// <summary>
    /// Creates a new main menu view instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics. If null, a no-op logger is used.</param>
    public MainMenuView(ILogger<MainMenuView>? logger = null)
    {
        _logger = logger ?? NullLogger<MainMenuView>.Instance;
        _logger.LogDebug("MainMenuView initialized");
    }

    /// <summary>
    /// Renders the title screen with game logo and subtitle.
    /// </summary>
    public void RenderTitle()
    {
        _logger.LogDebug("Rendering title screen");
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

    /// <summary>
    /// Displays menu options and gets the user's selection.
    /// </summary>
    /// <returns>The selected menu option.</returns>
    public MainMenuOption GetMenuSelection()
    {
        _logger.LogDebug("Prompting for menu selection");

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

        _logger.LogInformation("Menu selection: {MenuOption}", selection);
        return selection;
    }

    /// <summary>
    /// Prompts the user to enter their player name with validation.
    /// </summary>
    /// <returns>The trimmed player name (1-50 characters).</returns>
    public string GetPlayerName()
    {
        _logger.LogDebug("Prompting for player name");
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

        var trimmedName = name.Trim();
        _logger.LogInformation("Player name entered: {PlayerName}", trimmedName);
        return trimmedName;
    }

    /// <summary>
    /// Renders a welcome message for the player with instructions.
    /// </summary>
    /// <param name="playerName">The player's name to include in the welcome message.</param>
    public void RenderWelcome(string playerName)
    {
        _logger.LogDebug("Rendering welcome message for player: {PlayerName}", playerName);

        Console.WriteLine();
        AnsiConsole.MarkupLine($"[green]Welcome, [bold]{Markup.Escape(playerName)}[/]![/]");
        AnsiConsole.MarkupLine("[grey]Your adventure begins in the Forgotten Depths...[/]");
        Console.WriteLine();
        AnsiConsole.MarkupLine("[grey]Type 'help' for a list of commands.[/]");
        Console.WriteLine();
    }
}
