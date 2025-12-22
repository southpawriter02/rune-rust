using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Terminal-based implementation of ITitleScreenService using Spectre.Console (v0.3.4a).
/// Displays an animated ASCII title with Elder Futhark rune glitch effects and main menu.
/// </summary>
public class TitleScreenService : ITitleScreenService
{
    private readonly ISaveGameRepository _saveRepository;
    private readonly ILogger<TitleScreenService> _logger;
    private static readonly Random _random = new();

    // Elder Futhark runes for glitch effect
    private static readonly char[] GlitchChars = { 'ᚠ', 'ᚢ', 'ᚦ', 'ᚨ', 'ᚱ', 'ᚾ', 'ᛖ', '▓', '░', '▒' };

    private const string TitleArt = @"
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║   ██████╗ ██╗   ██╗███╗   ██╗███████╗                             ║
║   ██╔══██╗██║   ██║████╗  ██║██╔════╝                             ║
║   ██████╔╝██║   ██║██╔██╗ ██║█████╗                               ║
║   ██╔══██╗██║   ██║██║╚██╗██║██╔══╝                               ║
║   ██║  ██║╚██████╔╝██║ ╚████║███████╗                             ║
║   ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝                             ║
║                           ▲                                        ║
║                          ╱ ╲                                       ║
║   ██████╗ ██╗   ██╗███████╗████████╗                              ║
║   ██╔══██╗██║   ██║██╔════╝╚══██╔══╝                              ║
║   ██████╔╝██║   ██║███████╗   ██║                                 ║
║   ██╔══██╗██║   ██║╚════██║   ██║                                 ║
║   ██║  ██║╚██████╔╝███████║   ██║                                 ║
║   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝   ╚═╝                                 ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝";

    private const int GlitchIterations = 15;
    private const int GlitchDelayMs = 80;
    private const float GlitchProbability = 0.03f;

    public TitleScreenService(
        ISaveGameRepository saveRepository,
        ILogger<TitleScreenService> logger)
    {
        _saveRepository = saveRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TitleScreenResult> ShowAsync()
    {
        _logger.LogInformation("[TitleScreen] Displaying title screen");
        Console.Clear();

        await PlayGlitchAnimationAsync();
        RenderStableTitle();

        while (true)
        {
            var selection = ShowMainMenu();

            switch (selection)
            {
                case MainMenuOption.NewGame:
                    _logger.LogInformation("[TitleScreen] User selected New Game");
                    return TitleScreenResult.CreateNewGame();

                case MainMenuOption.LoadGame:
                    var saves = await _saveRepository.GetAllAsync();
                    if (!saves.Any())
                    {
                        AnsiConsole.MarkupLine("[yellow]No saved games found.[/]");
                        AnsiConsole.WriteLine();
                        continue; // Return to menu
                    }
                    // For now, load most recent save
                    var mostRecent = saves.OrderByDescending(s => s.LastPlayed).First();
                    _logger.LogInformation("[TitleScreen] User selected Load Game: Slot {SlotNumber}", mostRecent.SlotNumber);
                    return TitleScreenResult.LoadGame(mostRecent.SlotNumber);

                case MainMenuOption.Options:
                    AnsiConsole.MarkupLine("[grey]Options not yet implemented.[/]");
                    AnsiConsole.WriteLine();
                    continue; // Return to menu

                case MainMenuOption.Quit:
                    _logger.LogInformation("[TitleScreen] User selected Quit");
                    return TitleScreenResult.Quit();
            }
        }
    }

    /// <summary>
    /// Plays the glitch animation using AnsiConsole.Live() for smooth updates.
    /// </summary>
    private async Task PlayGlitchAnimationAsync()
    {
        var lines = TitleArt.Split('\n');

        await AnsiConsole.Live(new Markup(""))
            .StartAsync(async ctx =>
            {
                for (int i = 0; i < GlitchIterations; i++)
                {
                    var glitched = ApplyGlitchEffect(lines);
                    var panel = new Panel(new Markup($"[red]{Markup.Escape(glitched)}[/]"))
                        .Border(BoxBorder.None);
                    ctx.UpdateTarget(panel);
                    await Task.Delay(GlitchDelayMs);
                }
            });
    }

    /// <summary>
    /// Applies random glitch effects to the title art, replacing characters with Elder Futhark runes.
    /// </summary>
    private string ApplyGlitchEffect(string[] lines)
    {
        var result = new StringBuilder();
        foreach (var line in lines)
        {
            foreach (var c in line)
            {
                if (_random.NextDouble() < GlitchProbability && !char.IsWhiteSpace(c))
                {
                    result.Append(GlitchChars[_random.Next(GlitchChars.Length)]);
                }
                else
                {
                    result.Append(c);
                }
            }
            result.AppendLine();
        }
        return result.ToString();
    }

    /// <summary>
    /// Renders the stable (non-glitched) title and version info.
    /// </summary>
    private void RenderStableTitle()
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(TitleArt)}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]v0.3.4a - The Gateway[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Displays the main menu and returns the user's selection.
    /// </summary>
    private MainMenuOption ShowMainMenu()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<MainMenuOption>()
                .Title("[bold white]Select an option:[/]")
                .AddChoices(
                    MainMenuOption.NewGame,
                    MainMenuOption.LoadGame,
                    MainMenuOption.Options,
                    MainMenuOption.Quit)
                .UseConverter(option => option switch
                {
                    MainMenuOption.NewGame => "New Game",
                    MainMenuOption.LoadGame => "Load Game",
                    MainMenuOption.Options => "Options",
                    MainMenuOption.Quit => "Quit",
                    _ => option.ToString()
                }));
    }
}
