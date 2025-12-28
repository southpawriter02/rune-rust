using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Controllers;

/// <summary>
/// Controller for main menu display and localized string retrieval (v0.3.15a - The Lexicon).
/// Extracted from TitleScreenService to support localization and separation of concerns.
/// </summary>
/// <remarks>See: SPEC-LOC-001 for Localization System design.</remarks>
public class MainMenuController
{
    private readonly ILocalizationService _loc;
    private readonly ILogger<MainMenuController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenuController"/> class.
    /// </summary>
    /// <param name="localizationService">The localization service for string lookup.</param>
    /// <param name="logger">The logger for traceability.</param>
    public MainMenuController(
        ILocalizationService localizationService,
        ILogger<MainMenuController> logger)
    {
        _loc = localizationService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the main menu prompt and returns the user's selection.
    /// </summary>
    /// <returns>The selected <see cref="MainMenuOption"/>.</returns>
    public MainMenuOption ShowMainMenu()
    {
        _logger.LogDebug("[MainMenu] Displaying main menu with locale {Locale}", _loc.CurrentLocale);

        return AnsiConsole.Prompt(
            new SelectionPrompt<MainMenuOption>()
                .Title($"[bold white]{_loc.Get(LocKeys.UI_MainMenu_SelectOption)}[/]")
                .AddChoices(
                    MainMenuOption.NewGame,
                    MainMenuOption.LoadGame,
                    MainMenuOption.Options,
                    MainMenuOption.Quit)
                .UseConverter(option => option switch
                {
                    MainMenuOption.NewGame => _loc.Get(LocKeys.UI_MainMenu_NewGame),
                    MainMenuOption.LoadGame => _loc.Get(LocKeys.UI_MainMenu_LoadGame),
                    MainMenuOption.Options => _loc.Get(LocKeys.UI_MainMenu_Options),
                    MainMenuOption.Quit => _loc.Get(LocKeys.UI_MainMenu_Quit),
                    _ => option.ToString()
                }));
    }

    /// <summary>
    /// Gets the version string for display on the title screen.
    /// v0.3.24b: Uses BuildInfo for version data instead of localization.
    /// </summary>
    /// <returns>The version string with build configuration.</returns>
    public string GetVersionString()
    {
        var config = BuildInfo.IsDebugBuild ? " [DEBUG]" : "";
        return $"{BuildInfo.FullVersionString}{config}";
    }

    /// <summary>
    /// Gets the localized "no saves found" message.
    /// </summary>
    /// <returns>The localized message.</returns>
    public string GetNoSavesMessage() => _loc.Get(LocKeys.UI_MainMenu_NoSaves);

    /// <summary>
    /// Gets the localized "options not implemented" message.
    /// </summary>
    /// <returns>The localized message.</returns>
    public string GetOptionsNotImplementedMessage() => _loc.Get(LocKeys.UI_MainMenu_OptionsNotImplemented);

    /// <summary>
    /// Gets the localized ASCII art logo for the title screen.
    /// </summary>
    /// <returns>The ASCII art logo string.</returns>
    public string GetTitleLogo() => _loc.Get(LocKeys.Art_TitleScreen_Logo);
}
