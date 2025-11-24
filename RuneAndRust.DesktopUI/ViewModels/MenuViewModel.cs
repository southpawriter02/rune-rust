using System;
using System.Windows.Input;
using ReactiveUI;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the main menu screen.
/// Allows starting a new game, loading saved games, and accessing settings.
/// </summary>
public class MenuViewModel : ViewModelBase
{
    /// <summary>
    /// Command to start a new game.
    /// </summary>
    public ICommand NewGameCommand { get; }

    /// <summary>
    /// Command to continue the most recent saved game.
    /// </summary>
    public ICommand ContinueGameCommand { get; }

    /// <summary>
    /// Command to load a saved game from the browser.
    /// </summary>
    public ICommand LoadGameCommand { get; }

    /// <summary>
    /// Command to open settings.
    /// </summary>
    public ICommand SettingsCommand { get; }

    /// <summary>
    /// Command to view achievements.
    /// </summary>
    public ICommand AchievementsCommand { get; }

    /// <summary>
    /// Command to exit the application.
    /// </summary>
    public ICommand ExitCommand { get; }

    /// <summary>
    /// Gets the application title.
    /// </summary>
    public string Title => "Rune & Rust";

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public string Version => "v0.43.1 - Desktop UI Foundation";

    public MenuViewModel()
    {
        // Placeholder commands for v0.43.1
        // Full implementation will be added in later specs
        NewGameCommand = ReactiveCommand.Create(OnNewGame);
        ContinueGameCommand = ReactiveCommand.Create(OnContinueGame);
        LoadGameCommand = ReactiveCommand.Create(OnLoadGame);
        SettingsCommand = ReactiveCommand.Create(OnSettings);
        AchievementsCommand = ReactiveCommand.Create(OnAchievements);
        ExitCommand = ReactiveCommand.Create(OnExit);
    }

    private void OnNewGame()
    {
        // Placeholder - will navigate to character creation in later specs
        Console.WriteLine("[MENU] New Game selected");
    }

    private void OnContinueGame()
    {
        // Placeholder - will load most recent save in later specs
        Console.WriteLine("[MENU] Continue Game selected");
    }

    private void OnLoadGame()
    {
        // Placeholder - will show save browser in v0.43.19
        Console.WriteLine("[MENU] Load Game selected");
    }

    private void OnSettings()
    {
        // Placeholder - will show settings in v0.43.18
        Console.WriteLine("[MENU] Settings selected");
    }

    private void OnAchievements()
    {
        // Placeholder - will show achievements in v0.43.15
        Console.WriteLine("[MENU] Achievements selected");
    }

    private void OnExit()
    {
        // Placeholder - will exit application
        Console.WriteLine("[MENU] Exit selected");
        Environment.Exit(0);
    }
}
