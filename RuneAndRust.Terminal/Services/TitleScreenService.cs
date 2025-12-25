using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Terminal.Controllers;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Terminal-based implementation of ITitleScreenService using Spectre.Console (v0.3.15a - The Lexicon).
/// Displays an animated ASCII title with Elder Futhark rune glitch effects and main menu.
/// Uses MainMenuController for localized menu display.
/// </summary>
/// <remarks>See: SPEC-LOC-001 for Localization System design.</remarks>
public class TitleScreenService : ITitleScreenService
{
    private readonly ISaveGameRepository _saveRepository;
    private readonly MainMenuController _menuController;
    private readonly ILogger<TitleScreenService> _logger;
    private static readonly Random _random = new();

    // Elder Futhark runes for glitch effect
    private static readonly char[] GlitchChars = { 'ᚠ', 'ᚢ', 'ᚦ', 'ᚨ', 'ᚱ', 'ᚾ', 'ᛖ', '▓', '░', '▒' };

    private const int GlitchIterations = 15;
    private const int GlitchDelayMs = 80;
    private const float GlitchProbability = 0.03f;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleScreenService"/> class.
    /// </summary>
    /// <param name="saveRepository">The save game repository.</param>
    /// <param name="menuController">The main menu controller for localized display.</param>
    /// <param name="logger">The logger for traceability.</param>
    public TitleScreenService(
        ISaveGameRepository saveRepository,
        MainMenuController menuController,
        ILogger<TitleScreenService> logger)
    {
        _saveRepository = saveRepository;
        _menuController = menuController;
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
            var selection = _menuController.ShowMainMenu();

            switch (selection)
            {
                case MainMenuOption.NewGame:
                    _logger.LogInformation("[TitleScreen] User selected New Game");
                    return TitleScreenResult.CreateNewGame();

                case MainMenuOption.LoadGame:
                    var saves = await _saveRepository.GetAllAsync();
                    if (!saves.Any())
                    {
                        AnsiConsole.MarkupLine($"[yellow]{_menuController.GetNoSavesMessage()}[/]");
                        AnsiConsole.WriteLine();
                        continue; // Return to menu
                    }
                    // For now, load most recent save
                    var mostRecent = saves.OrderByDescending(s => s.LastPlayed).First();
                    _logger.LogInformation("[TitleScreen] User selected Load Game: Slot {SlotNumber}", mostRecent.SlotNumber);
                    return TitleScreenResult.LoadGame(mostRecent.SlotNumber);

                case MainMenuOption.Options:
                    AnsiConsole.MarkupLine($"[grey]{_menuController.GetOptionsNotImplementedMessage()}[/]");
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
        var titleArt = _menuController.GetTitleLogo();
        var lines = titleArt.Split('\n');

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
        var titleArt = _menuController.GetTitleLogo();
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(titleArt)}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{_menuController.GetVersionString()}[/]");
        AnsiConsole.WriteLine();
    }
}
