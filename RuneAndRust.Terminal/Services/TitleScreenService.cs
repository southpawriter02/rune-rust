using System.Text;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
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
/// <remarks>
/// See: SPEC-LOC-001 for Localization System design.
/// v0.3.21: Added rich metadata display for save slots with HP bars and location.
/// </remarks>
public class TitleScreenService : ITitleScreenService
{
    private readonly ISaveGameRepository _saveRepository;
    private readonly MainMenuController _menuController;
    private readonly ILogger<TitleScreenService> _logger;
    private static readonly Random _random = new();

    /// <summary>
    /// Back option for save selection menu.
    /// </summary>
    private const int BackOption = int.MinValue;

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
                    var loadResult = await ShowLoadMenuAsync();
                    if (loadResult.HasValue)
                    {
                        _logger.LogInformation("[TitleScreen] User selected Load Game: Slot {SlotNumber}", loadResult.Value);
                        return TitleScreenResult.LoadGame(loadResult.Value);
                    }
                    // User selected Back - return to main menu
                    continue;

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

    /// <summary>
    /// Displays the load game menu with rich metadata display (v0.3.21).
    /// Shows all saves including autosave backups (-1, -2) with HP bars and location.
    /// </summary>
    /// <returns>The selected slot number, or null if user selected Back.</returns>
    private async Task<int?> ShowLoadMenuAsync()
    {
        var saves = (await _saveRepository.GetAllAsync()).ToList();

        if (!saves.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]{_menuController.GetNoSavesMessage()}[/]");
            AnsiConsole.WriteLine();
            return null;
        }

        _logger.LogDebug("[LoadMenu] Displaying {Count} save slots", saves.Count);

        // Build selection choices with metadata
        var choices = new List<(int slot, string display)>();

        // Sort: Manual saves (1, 2, 3) first, then autosaves (0, -1, -2)
        var manualSaves = saves.Where(s => s.SlotNumber > 0).OrderBy(s => s.SlotNumber);
        var autoSaves = saves.Where(s => s.SlotNumber <= 0).OrderByDescending(s => s.SlotNumber);

        foreach (var save in manualSaves)
        {
            choices.Add((save.SlotNumber, FormatSaveSlotDisplay(save)));
        }

        if (autoSaves.Any())
        {
            foreach (var save in autoSaves)
            {
                choices.Add((save.SlotNumber, FormatSaveSlotDisplay(save)));
            }
        }

        // Add Back option
        choices.Add((BackOption, "[grey]← Back to Main Menu[/]"));

        var selectedSlot = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("[bold white]Select Save Slot[/]")
                .PageSize(10)
                .AddChoices(choices.Select(c => c.slot))
                .UseConverter(slot =>
                {
                    var match = choices.FirstOrDefault(c => c.slot == slot);
                    return match.display;
                }));

        if (selectedSlot == BackOption)
        {
            return null;
        }

        return selectedSlot;
    }

    /// <summary>
    /// Formats a save slot for display with metadata (v0.3.21).
    /// </summary>
    /// <param name="save">The save game entity.</param>
    /// <returns>A formatted string for the selection prompt.</returns>
    private static string FormatSaveSlotDisplay(SaveGame save)
    {
        var slotLabel = GetSlotLabel(save.SlotNumber);
        var metadata = save.Metadata;

        if (metadata == null)
        {
            // Legacy save without metadata
            var timeAgo = FormatTimeAgo(save.LastPlayed);
            return $"{slotLabel} {Markup.Escape(save.CharacterName)} [grey]({timeAgo})[/]";
        }

        // Rich metadata display
        var hpBar = RenderHpBar(metadata.CurrentHp, metadata.MaxHp);
        var timeAgo2 = FormatTimeAgo(metadata.SaveTimestamp);

        var line1 = $"{slotLabel} [white]{Markup.Escape(metadata.CharacterName)}[/] [grey]Lvl {metadata.Level} {metadata.Archetype}[/]";
        var line2 = $"   [dim]Loc:[/] {Markup.Escape(metadata.LocationName)} [dim]|[/] HP: {hpBar} [grey]({timeAgo2})[/]";

        return $"{line1}\n{line2}";
    }

    /// <summary>
    /// Gets the display label for a slot number (v0.3.21b).
    /// </summary>
    /// <param name="slotNumber">The slot number.</param>
    /// <returns>A styled label string.</returns>
    private static string GetSlotLabel(int slotNumber)
    {
        return slotNumber switch
        {
            > 0 => $"[bold][[Slot {slotNumber}]][/]",
            0 => "[cyan][[Autosave]][/]",
            -1 => "[grey][[Backup 1]][/]",
            -2 => "[grey][[Backup 2]][/]",
            _ => "[red][[Corrupted]][/]"
        };
    }

    /// <summary>
    /// Renders an HP bar with color coding (v0.3.21).
    /// </summary>
    /// <param name="current">Current HP.</param>
    /// <param name="max">Maximum HP.</param>
    /// <returns>A colored HP bar string.</returns>
    private static string RenderHpBar(int current, int max)
    {
        if (max <= 0) return "[grey]---[/]";

        var ratio = (float)current / max;
        var filledBlocks = (int)Math.Round(ratio * 5);
        var emptyBlocks = 5 - filledBlocks;

        var color = ratio switch
        {
            >= 0.7f => "green",
            >= 0.4f => "yellow",
            _ => "red"
        };

        var filled = new string('█', filledBlocks);
        var empty = new string('░', emptyBlocks);

        return $"[{color}]{filled}[/][grey]{empty}[/] ({current}/{max})";
    }

    /// <summary>
    /// Formats a timestamp as a relative time string.
    /// </summary>
    /// <param name="timestamp">The timestamp to format.</param>
    /// <returns>A human-readable relative time string.</returns>
    private static string FormatTimeAgo(DateTime timestamp)
    {
        if (timestamp == DateTime.MinValue) return "Unknown";

        var elapsed = DateTime.UtcNow - timestamp;

        if (elapsed.TotalMinutes < 1) return "Just now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
        if (elapsed.TotalDays < 7) return $"{(int)elapsed.TotalDays}d ago";

        return timestamp.ToString("MMM d");
    }
}
